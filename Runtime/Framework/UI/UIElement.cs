using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AXS.Core.UI
{
    /// <summary>
    /// 画面・モーダル・ダイアログの基底クラス。ライフサイクルと非同期アニメーションを管理します。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIElement : MonoBehaviour, IBackHandler
    {
        [SerializeField] private bool closeOnBackPressed = true;

        private CanvasGroup _canvasGroup;
        public CanvasGroup CanvasGroup => _canvasGroup != null ? _canvasGroup : (_canvasGroup = GetComponent<CanvasGroup>());

        public bool IsOpen { get; private set; }

        protected virtual void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// 画面を開きます。
        /// </summary>
        public async UniTask OpenAsync(CancellationToken cancellationToken = default)
        {
            if (IsOpen) return;
            IsOpen = true;
            gameObject.SetActive(true);

            SetInteractable(false);
            await OnOpenAnimationAsync(cancellationToken);
            SetInteractable(true);
        }

        /// <summary>
        /// 画面を閉じます。
        /// </summary>
        public async UniTask CloseAsync(CancellationToken cancellationToken = default)
        {
            if (!IsOpen) return;
            SetInteractable(false);
            await OnCloseAnimationAsync(cancellationToken);

            IsOpen = false;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// オープン演出。派生クラスでオーバーライドして独自アニメーションを実装できます。
        /// </summary>
        protected virtual UniTask OnOpenAnimationAsync(CancellationToken cancellationToken)
        {
            CanvasGroup.alpha = 1f;
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// クローズ演出。派生クラスでオーバーライドして独自アニメーションを実装できます。
        /// </summary>
        protected virtual UniTask OnCloseAnimationAsync(CancellationToken cancellationToken)
        {
            CanvasGroup.alpha = 0f;
            return UniTask.CompletedTask;
        }

        protected virtual void SetInteractable(bool state)
        {
            CanvasGroup.interactable = state;
            CanvasGroup.blocksRaycasts = state;
        }

        public virtual async UniTask<bool> OnBackPressedAsync()
        {
            if (!closeOnBackPressed || !IsOpen)
            {
                return false;
            }

            await CloseAsync();
            return true;
        }
    }
}
