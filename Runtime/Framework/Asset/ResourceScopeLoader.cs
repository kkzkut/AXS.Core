using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace AXS.Core.Asset
{
    /// <summary>
    /// アセットの安全なロード機構。
    /// 後から Addressables パッケージを導入した際にもコードの書き換えを最小限にするための抽象ローダーです。
    /// 現在は Resources API を内部で使用し、GameObject破棄時またはスコープ破棄時に自動解放を行います。
    /// </summary>
    public class ResourceScopeLoader : IDisposable
    {
        private readonly List<GameObject> _instantiatedObjects = new List<GameObject>();
        private bool _isDisposed;

        /// <summary>
        /// Resources からアセットを非同期ロードします。
        /// </summary>
        public async UniTask<T> LoadAssetAsync<T>(string resourcePath, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ResourceScopeLoader));
            }

            var req = Resources.LoadAsync<T>(resourcePath);
            await req.ToUniTask(cancellationToken: cancellationToken);

            return req.asset as T;
        }

        /// <summary>
        /// Resources からプレハブをロードしてインスタンス化します。スコープ破棄時に追跡・破棄されます。
        /// </summary>
        public async UniTask<GameObject> InstantiateAsync(string resourcePath, Transform parent = null, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(ResourceScopeLoader));
            }

            var prefab = await LoadAssetAsync<GameObject>(resourcePath, cancellationToken);
            if (prefab == null) return null;

            var instance = UnityEngine.Object.Instantiate(prefab, parent);
            _instantiatedObjects.Add(instance);
            return instance;
        }

        /// <summary>
        /// スコープ内で生成されたインスタンスを解放します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            for (int i = _instantiatedObjects.Count - 1; i >= 0; i--)
            {
                var instance = _instantiatedObjects[i];
                if (instance != null)
                {
                    UnityEngine.Object.Destroy(instance);
                }
            }
            _instantiatedObjects.Clear();
        }
    }
}
