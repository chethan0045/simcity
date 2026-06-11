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
    /// their own lives. Auto-runs — supersedes Phase 0/1/2 bootstrappers.
    /// </summary>
    public static class Phase3Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Build()
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
