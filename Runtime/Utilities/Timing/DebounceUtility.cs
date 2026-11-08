using System.Collections.Generic;
using UnityEngine;

namespace AXS.Core.Utilities
{
    /// <summary>
    /// UIボタン等の連続タップ（連打）防止ユーティリティ。
    /// </summary>
    public static class DebounceUtility
    {
        private static readonly Dictionary<int, float> LastExecutionTimes = new Dictionary<int, float>();

        /// <summary>
        /// 指定キーに対して、前回の実行から coolDownSeconds 以上経過しているかを判定し、更新します。
        /// </summary>
        /// <param name="key">判定対象の識別子（例: ボタンのGetInstanceID()）</param>
        /// <param name="coolDownSeconds">最小実行間隔（秒）。デフォルト0.3秒</param>
        /// <returns>実行可能な場合true</returns>
        public static bool CanExecute(int key, float coolDownSeconds = 0.3f)
        {
            float currentTime = Time.unscaledTime;
            if (LastExecutionTimes.TryGetValue(key, out float lastTime))
            {
                if (currentTime - lastTime < coolDownSeconds)
                {
                    return false;
                }
            }

            LastExecutionTimes[key] = currentTime;
            return true;
        }

        public static bool CanExecute(EntityId key, float coolDownSeconds = 0.3f)
        {
            return CanExecute(key.GetHashCode(), coolDownSeconds);
        }

        public static void ResetKey(int key)
        {
            LastExecutionTimes.Remove(key);
        }

        public static void ClearAll()
        {
            LastExecutionTimes.Clear();
        }
    }
}
