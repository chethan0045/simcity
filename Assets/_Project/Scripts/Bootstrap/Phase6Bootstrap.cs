using UnityEngine;
using Simcity.Core;
using Simcity.Player;
using Simcity.Stats;
using Simcity.UI;
using Simcity.World;

namespace Simcity.Bootstrap
{
    /// <summary>
    /// Phase 6 entry point — the live bootstrapper (supersedes Phase 0-3). First run shows
    /// the character creator; then:
    ///   • With co-op ON  (SIMCITY_NETCODE define + the netcode packages) → the Co-op menu
    ///     (Host / Join by code / Solo).
    ///   • With co-op OFF (default repo state) → straight into the single-player town,
    ///     exactly like Phase 3. So the game still builds and plays with zero netcode setup.
    /// </summary>
    public static class Phase6Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Build()
        {
            if (Object.FindObjectOfType<FirstPersonController>() != null) return;
            if (Object.FindObjectOfType<CharacterCreationScreen>() != null) return;
#if SIMCITY_NETCODE
            if (Object.FindObjectOfType<CoopMenuScreen>() != null) return;
#endif

            if (SaveSystem.HasProfile())
            {
                Enter(SaveSystem.Load().appearance);
            }
            else
            {
                CharacterCreatorLauncher.Launch(cfg =>
                {
                    SaveSystem.Save(new CharacterProfile { appearance = cfg });
                    Enter(cfg);
                });
            }
        }

        private static void Enter(AppearanceConfig appearance)
        {
#if SIMCITY_NETCODE
            CoopMenuScreen.Show(appearance);
#else
            TownWorld.Build(appearance);
#endif
        }
    }
}
