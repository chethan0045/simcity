using UnityEngine;

namespace Simcity.Common
{
    /// <summary>
    /// One place for product identity + version, surfaced in-game (settings footer) and used
    /// by the build pipeline. The version comes from Player Settings (or the CI's BUILD_VERSION
    /// env var via BuildScript), so a single source of truth flows from build to UI.
    /// </summary>
    public static class AppInfo
    {
        public const string ProductName = "Untitled Life Sim";

        /// <summary>Set in Project Settings > Player > Version (CI overrides via BUILD_VERSION).</summary>
        public static string Version => Application.version;

        public static string Label => $"{ProductName}  v{Version}";
    }
}
