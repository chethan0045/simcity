using UnityEngine;

namespace Simcity.Common
{
    public enum QualityTier { Low = 0, Medium = 1, High = 2, Ultra = 3 }

    /// <summary>
    /// Phase 8 "scalable realism" dial. Maps four quality tiers onto Unity's QualitySettings
    /// (shadows, AA, lights, LOD bias) plus a target frame rate, persists the choice, and
    /// applies it at boot — so the same build runs on a phone and a desktop. Render-pipeline
    /// agnostic (works in Built-In and URP); a URP-specific render-scale pass can hang off this
    /// later. Also the home for the saved mouse sensitivity.
    /// </summary>
    public static class QualityManager
    {
        private const string TierKey = "simcity.quality.tier";
        private const string SensKey = "simcity.quality.sensitivity";

        public static QualityTier Tier { get; private set; } = QualityTier.High;
        public static float MouseSensitivity { get; private set; } = 2f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            Tier = (QualityTier)PlayerPrefs.GetInt(TierKey, DefaultTierForPlatform());
            MouseSensitivity = PlayerPrefs.GetFloat(SensKey, 2f);
            Apply(Tier);
        }

        private static int DefaultTierForPlatform() =>
            Application.isMobilePlatform ? (int)QualityTier.Medium : (int)QualityTier.High;

        public static void Apply(QualityTier tier)
        {
            Tier = tier;
            switch (tier)
            {
                case QualityTier.Low:
                    Set(ShadowQuality.Disable, ShadowResolution.Low, 20f, 0, 1, 0, 0.7f);
                    break;
                case QualityTier.Medium:
                    Set(ShadowQuality.HardOnly, ShadowResolution.Low, 45f, 0, 2, 2, 1.0f);
                    break;
                case QualityTier.High:
                    Set(ShadowQuality.All, ShadowResolution.Medium, 90f, 2, 4, 2, 1.5f);
                    break;
                case QualityTier.Ultra:
                    Set(ShadowQuality.All, ShadowResolution.VeryHigh, 150f, 4, 8, 4, 2.0f);
                    break;
            }

            // Cap frame rate to keep mobile thermals/battery sane; uncapped on Ultra desktop.
            bool vsync = tier >= QualityTier.High && !Application.isMobilePlatform;
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            Application.targetFrameRate = Application.isMobilePlatform
                ? (tier <= QualityTier.Low ? 30 : 60)
                : (tier == QualityTier.Ultra ? -1 : 60);

            PlayerPrefs.SetInt(TierKey, (int)tier);
            PlayerPrefs.Save();
        }

        private static void Set(ShadowQuality shadows, ShadowResolution shadowRes, float shadowDist,
            int antiAliasing, int pixelLights, int cascades, float lodBias)
        {
            QualitySettings.shadows = shadows;
            QualitySettings.shadowResolution = shadowRes;
            QualitySettings.shadowDistance = shadowDist;
            QualitySettings.shadowCascades = cascades;
            QualitySettings.antiAliasing = antiAliasing;
            QualitySettings.pixelLightCount = pixelLights;
            QualitySettings.lodBias = lodBias;
        }

        public static void SetSensitivity(float sensitivity)
        {
            MouseSensitivity = Mathf.Clamp(sensitivity, 0.5f, 8f);
            PlayerPrefs.SetFloat(SensKey, MouseSensitivity);
            PlayerPrefs.Save();
        }
    }
}
