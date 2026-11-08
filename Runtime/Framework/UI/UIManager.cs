using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AXS.Core.UI
{
    /// <summary>
    /// UI画面・モーダルのスタック管理およびナビゲーション制御マネージャー。
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<UIManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[UIManager]");
                        _instance = go.AddComponent<UIManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private readonly List<UIElement> _activeStack = new List<UIElement>();

        public int StackCount => _activeStack.Count;

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
        /// モーダル/画面をスタックにプッシュして開きます。
        /// </summary>
        public async UniTask PushAsync(UIElement element, CancellationToken cancellationToken = default)
        {
            if (element == null) return;

            if (_activeStack.Contains(element))
            {
                _activeStack.Remove(element);
            }
            _activeStack.Add(element);

            await element.OpenAsync(cancellationToken);
        }

        /// <summary>
        /// スタックの最前面にある画面をポップして閉じます。
        /// </summary>
        public async UniTask<UIElement> PopAsync(CancellationToken cancellationToken = default)
        {
            if (_activeStack.Count == 0) return null;

            int lastIndex = _activeStack.Count - 1;
            var top = _activeStack[lastIndex];
            _activeStack.RemoveAt(lastIndex);

            await top.CloseAsync(cancellationToken);
            return top;
        }

        /// <summary>
        /// 特定の要素をスタックから閉じて除外します。
        /// </summary>
        public async UniTask CloseAsync(UIElement element, CancellationToken cancellationToken = default)
        {
            if (element == null) return;
            _activeStack.Remove(element);
            await element.CloseAsync(cancellationToken);
        }

        /// <summary>
        /// AndroidバックキーまたはESCキー入力時に呼び出されます。最前面のUIから戻る処理を試行します。
        /// </summary>
        public async UniTask<bool> DispatchBackActionAsync()
        {
            if (_activeStack.Count == 0) return false;

            for (int i = _activeStack.Count - 1; i >= 0; i--)
            {
                var current = _activeStack[i];
                if (current != null && current.IsOpen)
                {
                    bool handled = await current.OnBackPressedAsync();
                    if (handled)
                    {
                        _activeStack.Remove(current);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 全てのスタックを閉じます。
        /// </summary>
        public async UniTask CloseAllAsync()
        {
            var elements = new List<UIElement>(_activeStack);
            _activeStack.Clear();

            foreach (var elem in elements)
            {
                if (elem != null && elem.IsOpen)
                {
                    await elem.CloseAsync();
                }
            }
        }
    }
}
