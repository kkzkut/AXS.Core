using System.Threading;
using Cysharp.Threading.Tasks;

namespace AXS.Core.Scene
{
    /// <summary>
    /// シーン遷移演出（黒フェード、ワイプ等）の抽象インターフェース。Layer 3のPrefab演出とLayer 1を疎結合にします。
    /// </summary>
    public interface ISceneTransitionEffect
    {
        UniTask ShowAsync(CancellationToken cancellationToken = default);
        UniTask HideAsync(CancellationToken cancellationToken = default);
        void SetProgress(float progress);
    }
}
