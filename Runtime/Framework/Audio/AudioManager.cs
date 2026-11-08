using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using AXS.Core.Utilities;

namespace AXS.Core.Audio
{
    /// <summary>
    /// BGM・SE・Voiceの統合オーディオマネージャー。
    /// BGMクロスフェード、SE同時再生制限、AudioSourceプール、音量設定の保存・反映を提供します。
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[AudioManager]");
                        _instance = go.AddComponent<AudioManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        [Header("Audio Mixer (Optional)")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string bgmVolumeParam = "BGMVolume";
        [SerializeField] private string seVolumeParam = "SEVolume";
        [SerializeField] private string voiceVolumeParam = "VoiceVolume";

        [Header("Settings")]
        [SerializeField] private int maxSimultaneousSameSe = 2;
        [SerializeField] private int initialSePoolSize = 10;

        private AudioSource _bgmSourceA;
        private AudioSource _bgmSourceB;
        private bool _isBgmSourceAActive;
        private CancellationTokenSource _bgmFadeCts;

        private readonly List<AudioSource> _seSourcePool = new List<AudioSource>();
        private readonly Dictionary<EntityId, (int frame, int count)> _sePlayHistory = new Dictionary<EntityId, (int, int)>();

        private const string PrefsPrefix = "AXS_Audio_Vol_";

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSources();
                LoadAndApplyVolumes();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeSources()
        {
            _bgmSourceA = gameObject.AddComponent<AudioSource>();
            _bgmSourceA.loop = true;
            _bgmSourceA.playOnAwake = false;

            _bgmSourceB = gameObject.AddComponent<AudioSource>();
            _bgmSourceB.loop = true;
            _bgmSourceB.playOnAwake = false;

            for (int i = 0; i < initialSePoolSize; i++)
            {
                CreateNewSeSource();
            }
        }

        private AudioSource CreateNewSeSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _seSourcePool.Add(source);
            return source;
        }

        #region BGM Control

        /// <summary>
        /// BGMをクロスフェード再生します。
        /// </summary>
        public async UniTask PlayBgmAsync(AudioClip clip, float fadeDuration = 0.5f, CancellationToken cancellationToken = default)
        {
            if (clip == null) return;

            _bgmFadeCts?.Cancel();
            _bgmFadeCts?.Dispose();
            _bgmFadeCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var token = _bgmFadeCts.Token;

            AudioSource currentSource = _isBgmSourceAActive ? _bgmSourceA : _bgmSourceB;
            AudioSource nextSource = _isBgmSourceAActive ? _bgmSourceB : _bgmSourceA;
            _isBgmSourceAActive = !_isBgmSourceAActive;

            nextSource.clip = clip;
            nextSource.volume = 0f;
            nextSource.Play();

            float elapsed = 0f;
            float startVol = currentSource.isPlaying ? currentSource.volume : 0f;

            while (elapsed < fadeDuration)
            {
                token.ThrowIfCancellationRequested();
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);

                nextSource.volume = t;
                if (currentSource.isPlaying)
                {
                    currentSource.volume = (1f - t) * startVol;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            nextSource.volume = 1f;
            if (currentSource.isPlaying)
            {
                currentSource.Stop();
                currentSource.clip = null;
            }
        }

        public void StopBgm()
        {
            _bgmFadeCts?.Cancel();
            _bgmSourceA.Stop();
            _bgmSourceB.Stop();
            _bgmSourceA.clip = null;
            _bgmSourceB.clip = null;
        }

        #endregion

        #region SE Control

        /// <summary>
        /// SEを再生します。同一クリップの同フレーム内多重再生を抑制します。
        /// </summary>
        public void PlaySe(AudioClip clip, float volumeScale = 1.0f)
        {
            if (clip == null) return;

            EntityId clipId = clip.GetEntityId();
            int currentFrame = Time.frameCount;

            if (_sePlayHistory.TryGetValue(clipId, out var history))
            {
                if (history.frame == currentFrame)
                {
                    if (history.count >= maxSimultaneousSameSe)
                    {
                        return; // 同一フレームでの最大数超過時は間引き
                    }
                    _sePlayHistory[clipId] = (currentFrame, history.count + 1);
                }
                else
                {
                    _sePlayHistory[clipId] = (currentFrame, 1);
                }
            }
            else
            {
                _sePlayHistory[clipId] = (currentFrame, 1);
            }

            AudioSource availableSource = null;
            for (int i = 0; i < _seSourcePool.Count; i++)
            {
                if (!_seSourcePool[i].isPlaying)
                {
                    availableSource = _seSourcePool[i];
                    break;
                }
            }

            if (availableSource == null)
            {
                availableSource = CreateNewSeSource();
            }

            availableSource.PlayOneShot(clip, volumeScale);
        }

        #endregion

        #region Volume Control & Persistence

        public void SetVolume(AudioChannelType channel, float volume)
        {
            volume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(PrefsPrefix + channel, volume);
            PlayerPrefs.Save();

            ApplyVolume(channel, volume);
        }

        public float GetVolume(AudioChannelType channel)
        {
            return PlayerPrefs.GetFloat(PrefsPrefix + channel, 1.0f);
        }

        private void LoadAndApplyVolumes()
        {
            foreach (AudioChannelType channel in Enum.GetValues(typeof(AudioChannelType)))
            {
                float vol = GetVolume(channel);
                ApplyVolume(channel, vol);
            }
        }

        private void ApplyVolume(AudioChannelType channel, float volume)
        {
            if (audioMixer != null)
            {
                string param = channel switch
                {
                    AudioChannelType.Master => masterVolumeParam,
                    AudioChannelType.BGM => bgmVolumeParam,
                    AudioChannelType.SE => seVolumeParam,
                    AudioChannelType.Voice => voiceVolumeParam,
                    _ => string.Empty
                };

                if (!string.IsNullOrEmpty(param))
                {
                    audioMixer.SetFloat(param, AudioMath.LinearToDecibel(volume));
                }
            }
        }

        #endregion
    }
}
