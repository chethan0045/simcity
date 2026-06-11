#if SIMCITY_NETCODE
using UnityEngine;
using Simcity.Stats;
using Simcity.Social;
using Simcity.Net;

namespace Simcity.World
{
    /// <summary>
    /// Phase 6 town. The static, deterministic parts (ground, lighting, diner, market,
    /// beds…) are built locally on every machine — identical inputs, identical world, so
    /// there's no need to stream geometry. The dynamic parts are networked: players spawn
    /// per connection, NPCs + the clock spawn on the host (see NetworkBootstrap), and they
    /// replicate to everyone. This is the same town as Phase 3, made co-op.
    /// </summary>
    public static class CoopWorld
    {
        /// <summary>The local player's chosen look — read by NetworkPlayer when this
        /// machine's player object spawns, then published to the rest of the group.</summary>
        public static AppearanceConfig LocalAppearance;

        public static void Build(AppearanceConfig appearance)
        {
            LocalAppearance = appearance ?? AppearanceConfig.CreateDefault();

            NetworkBootstrap.Ensure();        // NetworkManager + transport + prefab registry
            TownWorld.BuildSharedEnvironment(); // lighting / clock / ground / facilities

            if (Object.FindObjectOfType<SocialSystem>() == null)
                new GameObject("SocialSystem").AddComponent<SocialSystem>(); // local proximity bonding

            Debug.Log("[CoopWorld] Shared town built. Players, villagers, and the clock " +
                      "sync once the host/join handshake completes.");
        }
    }
}
#endif
