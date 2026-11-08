using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace AXS.Core.Asset
{
    /// <summary>
    /// Addressablesの安全なロード機構。
    /// ロードしたアセットハンドルを追跡し、呼び出し元の破棄時またはスコープ破棄時に自動でAddressables.Releaseを実行します。
    /// </summary>
    public class AddressableScopeLoader : IDisposable
    {
        private readonly List<AsyncOperationHandle> _handles = new List<AsyncOperationHandle>();
        private bool _isDisposed;

        /// <summary>
        /// アセットを非同期ロードします。ロードされたハンドルはスコープ内に登録され、Dispose時に自動解放されます。
        /// </summary>
        public async UniTask<T> LoadAssetAsync<T>(object key, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AddressableScopeLoader));
            }

            var handle = Addressables.LoadAssetAsync<T>(key);
            _handles.Add(handle);

            try
            {
                var result = await handle.ToUniTask(cancellationToken: cancellationToken);
                return result;
            }
            catch
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                    _handles.Remove(handle);
                }
                throw;
            }
        }

        /// <summary>
        /// プレハブをインスタンス化して返します。破棄時は自動で Addressables.ReleaseInstance が呼ばれるよう追跡します。
        /// </summary>
        public async UniTask<GameObject> InstantiateAsync(object key, Transform parent = null, CancellationToken cancellationToken = default)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AddressableScopeLoader));
            }

            var handle = Addressables.InstantiateAsync(key, parent);
            _handles.Add(handle);

            try
            {
                var instance = await handle.ToUniTask(cancellationToken: cancellationToken);
                return instance;
            }
            catch
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                    _handles.Remove(handle);
                }
                throw;
            }
        }

        /// <summary>
        /// スコープ内でロードされた全てのアセットを安全に解放します。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            for (int i = _handles.Count - 1; i >= 0; i--)
            {
                var handle = _handles[i];
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _handles.Clear();
        }
    }
}
