using System;
using AXS.Core.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AXS.Platform.Mobile
{
    /// <summary>
    /// Android端末の物理・仮想バックボタン（またはPCのESCキー）の入力を監視し、
    /// UIManagerのモーダル閉却処理へディスパッチするハンドラー。
    /// </summary>
    public class AndroidBackButtonHandler : MonoBehaviour
    {
        private static AndroidBackButtonHandler _instance;
        public static AndroidBackButtonHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<AndroidBackButtonHandler>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[AndroidBackButtonHandler]");
                        _instance = go.AddComponent<AndroidBackButtonHandler>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// スタックが空の状態でバックキーが押された時のイベント（例: アプリ終了確認ダイアログの表示など）
        /// </summary>
        public event Action OnBackStackEmpty;

        private bool _isProcessing;

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

        private void Update()
        {
            if (_isProcessing) return;

            bool isBackPressed = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                isBackPressed = true;
            }
#else
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isBackPressed = true;
            }
#endif

            if (isBackPressed)
            {
                HandleBackPressedAsync().Forget();
            }
        }

        private async UniTaskVoid HandleBackPressedAsync()
        {
            _isProcessing = true;
            try
            {
                bool handled = await UIManager.Instance.DispatchBackActionAsync();
                if (!handled)
                {
                    OnBackStackEmpty?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                _isProcessing = false;
            }
        }
    }
}
