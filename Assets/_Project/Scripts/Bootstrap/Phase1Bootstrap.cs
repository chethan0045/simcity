using UnityEngine;
using Simcity.Player;
using Simcity.Stats;
using Simcity.World;

namespace Simcity.Bootstrap
{
    /// <summary>
    /// Phase 1 world (apartment + ordinary-life day loop) WITHOUT the character
    /// creator. Superseded by Phase2Bootstrap, which adds creation + persistence.
    ///
    /// Auto-run is disabled so the bootstrappers don't race. Call Build() manually
    /// (or re-enable the attribute) to launch straight into the loop with a default
    /// character. The actual world-building now lives in World/ApartmentWorld.
    /// </summary>
    public static class Phase1Bootstrap
    {
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Build()
        {
            if (Object.FindObjectOfType<FirstPersonController>() != null) return;
            ApartmentWorld.Build(AppearanceConfig.CreateDefault());
        }
    }
}
