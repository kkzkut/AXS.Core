using Cysharp.Threading.Tasks;

namespace AXS.Core.UI
{
    /// <summary>
    /// OSバックキーやESCキーが押された際のハンドリング対象インターフェース。
    /// </summary>
    public interface IBackHandler
    {
        /// <summary>
        /// 戻る操作を処理します。処理を消費した場合は true を返します。
        /// </summary>
        UniTask<bool> OnBackPressedAsync();
    }
}
