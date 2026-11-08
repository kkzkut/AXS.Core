using UnityEngine;

namespace AXS.Core.Utilities
{
    /// <summary>
    /// 音量計算ユーティリティ。線形ボリューム(0.0 - 1.0)とデシベル(-80dB - 0dB)の相互変換を提供します。
    /// </summary>
    public static class AudioMath
    {
        private const float MinDb = -80f;
        private const float MaxDb = 0f;

        /// <summary>
        /// 線形音量(0.0 - 1.0)をAudioMixer用デシベル(-80dB - 0dB)に変換します。
        /// </summary>
        public static float LinearToDecibel(float linear)
        {
            if (linear <= 0.0001f)
            {
                return MinDb;
            }
            float db = 20f * Mathf.Log10(Mathf.Clamp01(linear));
            return Mathf.Clamp(db, MinDb, MaxDb);
        }

        /// <summary>
        /// デシベル(-80dB - 0dB)を線形音量(0.0 - 1.0)に変換します。
        /// </summary>
        public static float DecibelToLinear(float db)
        {
            if (db <= MinDb)
            {
                return 0f;
            }
            return Mathf.Clamp01(Mathf.Pow(10f, db / 20f));
        }
    }
}
