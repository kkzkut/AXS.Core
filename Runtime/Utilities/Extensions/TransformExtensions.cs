using UnityEngine;

namespace AXS.Core.Utilities
{
    /// <summary>
    /// TransformおよびRectTransformに関する汎用拡張メソッド。
    /// </summary>
    public static class TransformExtensions
    {
        /// <summary>
        /// ローカル位置・回転・スケールをデフォルト値にリセットします。
        /// </summary>
        public static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        /// <summary>
        /// 親要素全体に広がるようにアンカー・ピボット・オフセットを全画面ストレッチ設定にします。
        /// </summary>
        public static void FillParent(this RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
    }
}
