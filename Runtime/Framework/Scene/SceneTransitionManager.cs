using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AXS.Core.Scene
{
    /// <summary>
    /// UniTaskベースの非同期シーン遷移マネージャー。
    /// 演出（ISceneTransitionEffect）と連動し、安全なシーン切り替えと進行度表示を提供します。
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        private static SceneTransitionManager _instance;
        public static SceneTransitionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<SceneTransitionManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[SceneTransitionManager]");
                        _instance = go.AddComponent<SceneTransitionManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private ISceneTransitionEffect _defaultEffect;
        public bool IsTransitioning { get; private set; }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// デフォルトの遷移演出エフェクト（Layer 3等で実装）を登録します。
        /// </summary>
        public void RegisterDefaultEffect(ISceneTransitionEffect effect)
        {
            _defaultEffect = effect;
        }

        /// <summary>
        /// 指定シーンへ非同期遷移します。
        /// </summary>
        public async UniTask LoadSceneAsync(
            string sceneName,
            LoadSceneMode mode = LoadSceneMode.Single,
            ISceneTransitionEffect customEffect = null,
            CancellationToken cancellationToken = default)
        {
            if (IsTransitioning)
            {
                Debug.LogWarning($"[SceneTransitionManager] Scene transition is already in progress. Target: {sceneName}");
                return;
            }

            IsTransitioning = true;
            var effect = customEffect ?? _defaultEffect;

            try
            {
                // 1. フェードイン演出
                if (effect != null)
                {
                    effect.SetProgress(0f);
                    await effect.ShowAsync(cancellationToken);
                }

                // 2. シーン非同期ロード
                var op = SceneManager.LoadSceneAsync(sceneName, mode);
                if (op != null)
                {
                    op.allowSceneActivation = false;

                    while (op.progress < 0.9f)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        effect?.SetProgress(op.progress / 0.9f);
                        await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    }

                    effect?.SetProgress(1f);
                    op.allowSceneActivation = true;

                    await op.ToUniTask(cancellationToken: cancellationToken);
                }

                // 3. フェードアウト演出
                if (effect != null)
                {
                    await effect.HideAsync(cancellationToken);
                }
            }
            finally
            {
                IsTransitioning = false;
            }
        }
    }
}
