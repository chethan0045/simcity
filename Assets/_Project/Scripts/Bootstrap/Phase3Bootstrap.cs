using UnityEngine;
using Simcity.Core;
using Simcity.Player;
using Simcity.UI;
using Simcity.World;

namespace Simcity.Bootstrap
{
    /// <summary>
    /// Phase 3 entry point. First run shows the character creator; then spawns into
    /// the populated TOWN (vs. Phase 2's single apartment) with AI villagers living
    /// their own lives.
    ///
    /// SUPERSEDED BY Phase6Bootstrap: auto-run is disabled so the two don't race.
    /// Phase6Bootstrap runs this exact single-player flow when co-op is off (no
    /// SIMCITY_NETCODE define), so the Phase 3 experience is unchanged by default.
    /// To force the plain Phase 3 town, comment out Phase6Bootstrap's attribute and
    /// uncomment the one below.
    /// </summary>
    public static class Phase3Bootstrap
    {
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Build()
        {
            if (Object.FindObjectOfType<FirstPersonController>() != null) return;
            if (Object.FindObjectOfType<CharacterCreationScreen>() != null) return;

            if (SaveSystem.HasProfile())
            {
                TownWorld.Build(SaveSystem.Load().appearance);
            }
            else
            {
                CharacterCreatorLauncher.Launch(cfg =>
                {
                    SaveSystem.Save(new CharacterProfile { appearance = cfg });
                    TownWorld.Build(cfg);
                });
            }
        }
    }
}
