using UnityEngine;
using Simcity.Core;
using Simcity.Player;
using Simcity.UI;
using Simcity.World;

namespace Simcity.Bootstrap
{
    /// <summary>
    /// Phase 2 entry: character creator → save → spawn into the single APARTMENT
    /// (no NPCs). Superseded by Phase3Bootstrap (town + villagers); auto-run disabled
    /// so only one bootstrapper runs. Call Build() to use the apartment-only flow.
    /// </summary>
    public static class Phase2Bootstrap
    {
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Build()
        {
            if (Object.FindObjectOfType<FirstPersonController>() != null) return;
            if (Object.FindObjectOfType<CharacterCreationScreen>() != null) return;

            if (SaveSystem.HasProfile())
            {
                ApartmentWorld.Build(SaveSystem.Load().appearance);
            }
            else
            {
                CharacterCreatorLauncher.Launch(cfg =>
                {
                    SaveSystem.Save(new CharacterProfile { appearance = cfg });
                    ApartmentWorld.Build(cfg);
                });
            }
        }
    }
}
