using UnityEngine;
using Simcity.Common;
using Simcity.Interaction;
using Simcity.Stats;
using Simcity.AI;
using Simcity.Social;

namespace Simcity.World
{
    /// <summary>
    /// Phase 3 world: a small greybox town populated by utility-AI villagers. Builds
    /// the ground, shared facilities (diner / workshop / beds / benches — each an
    /// Amenity for NPCs and an Interactable for the player), spawns NPCs with random
    /// looks, and drops in the first-person player.
    /// </summary>
    public static class TownWorld
    {
        public static Vector3 Center = Vector3.zero;
        public static float WanderRadius = 12f;

        public static GameObject Build(AppearanceConfig playerAppearance)
        {
            BuildSharedEnvironment();
            new GameObject("SocialSystem").AddComponent<SocialSystem>();
            SpawnNpcs(7);

            var player = PlayerFactory.BuildPlayer(playerAppearance, new Vector3(0f, 1.2f, 8f));
            PlayerFactory.BuildHud(player);

            Debug.Log("[TownWorld] Town built. Watch villagers eat/sleep/work/gather by time of day. " +
                      "You can use the Diner, Workshop, and Beds too.");
            return player;
        }

        /// <summary>Lighting + clock + ground + the (static, deterministic) facilities.
        /// Everything here is identical on every machine, so Phase 6's co-op world builds
        /// it locally on each client rather than syncing the geometry over the network.</summary>
        public static void BuildSharedEnvironment()
        {
            WorldCommon.EnsureLighting();
            WorldCommon.EnsureClock();
            BuildGround();
            BuildStations();
        }

        private static void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(4f, 1f, 4f); // ~40x40m
            MaterialUtils.SetColor(ground.GetComponent<Renderer>(), new Color(0.32f, 0.4f, 0.3f));

            var plaza = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            plaza.name = "Plaza";
            plaza.transform.position = new Vector3(0f, 0.02f, 0f);
            plaza.transform.localScale = new Vector3(6f, 0.02f, 6f);
            MaterialUtils.SetColor(plaza.GetComponent<Renderer>(), new Color(0.5f, 0.48f, 0.44f));
        }

        private static void BuildStations()
        {
            var diner = WorldCommon.CreateBox("Diner", new Vector3(-9, 1, 2), new Vector3(3, 2, 3),
                new Color(0.8f, 0.55f, 0.3f));
            diner.AddComponent<Amenity>().type = AmenityType.Food;
            diner.AddComponent<FoodInteractable>().prompt = "Eat";

            var shop = WorldCommon.CreateBox("Workshop", new Vector3(9, 1, 2), new Vector3(3, 2, 3),
                new Color(0.45f, 0.5f, 0.6f));
            shop.AddComponent<Amenity>().type = AmenityType.Work;
            shop.AddComponent<WorkInteractable>().prompt = "Work";

            // Workbench (craft goods) and Market (sell them) — the soft-currency economy.
            WorldCommon.CreateBox("Workbench", new Vector3(5.5f, 0.6f, -2f), new Vector3(2f, 1.2f, 1.2f),
                new Color(0.55f, 0.42f, 0.28f)).AddComponent<CraftInteractable>().prompt = "Craft";

            WorldCommon.CreateBox("Market", new Vector3(-5.5f, 1f, -2f), new Vector3(2.4f, 2f, 1.4f),
                new Color(0.3f, 0.6f, 0.5f)).AddComponent<MarketInteractable>().prompt = "Sell goods";

            // Bank: the (web) money portal — buy Coins / cash out earned Coins (Phase 7, sandbox).
            WorldCommon.CreateBox("Bank", new Vector3(-8.5f, 1f, -2f), new Vector3(2.2f, 2f, 1.4f),
                new Color(0.2f, 0.5f, 0.35f)).AddComponent<BankInteractable>();

            Vector3[] beds = { new Vector3(-7, 0.4f, -8), new Vector3(-2, 0.4f, -9), new Vector3(3, 0.4f, -9), new Vector3(8, 0.4f, -8) };
            for (int i = 0; i < beds.Length; i++)
            {
                var bed = WorldCommon.CreateBox($"Bed_{i + 1}", beds[i], new Vector3(1.6f, 0.6f, 2.4f),
                    new Color(0.35f, 0.45f, 0.8f));
                bed.AddComponent<Amenity>().type = AmenityType.Bed;
                bed.AddComponent<BedInteractable>().prompt = "Sleep";
            }

            Vector3[] benches = { new Vector3(-3, 0.3f, 3), new Vector3(3, 0.3f, 3), new Vector3(0, 0.3f, -3) };
            for (int j = 0; j < benches.Length; j++)
            {
                var bench = WorldCommon.CreateBox($"Bench_{j + 1}", benches[j], new Vector3(1.6f, 0.4f, 0.5f),
                    new Color(0.4f, 0.3f, 0.22f));
                bench.AddComponent<Amenity>().type = AmenityType.Social;
            }
        }

        private static void SpawnNpcs(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"Villager_{i + 1}");
                float ang = Mathf.Deg2Rad * (360f / count) * i;
                go.transform.position = new Vector3(Mathf.Cos(ang) * 5f, 0f, Mathf.Sin(ang) * 5f);

                var needs = go.AddComponent<CharacterNeeds>();
                go.AddComponent<SimpleMover>();
                var brain = go.AddComponent<NpcBrain>();
                brain.displayName = $"Villager {i + 1}";

                // Desync needs a little so they don't all act in lockstep.
                var rng = new System.Random(1000 + i);
                needs.Replenish(NeedType.Hunger, -rng.Next(0, 40));
                needs.Replenish(NeedType.Energy, -rng.Next(0, 40));

                var cfg = AppearanceConfig.CreateDefault();
                cfg.Randomize(rng);
                var appear = go.AddComponent<CharacterAppearance>();
                appear.hideHeadForFirstPerson = false; // we see NPCs fully
                appear.Apply(cfg);

                // Social identity + a collider so the player can target them to Talk.
                go.AddComponent<Relationships>();
                var character = go.AddComponent<Character>();
                character.displayName = string.IsNullOrEmpty(cfg.characterName) ? brain.displayName : cfg.characterName;
                character.sociability = (float)rng.NextDouble();
                brain.displayName = character.displayName;

                var col = go.AddComponent<CapsuleCollider>();
                col.height = 1.8f;
                col.radius = 0.35f;
                col.center = new Vector3(0f, 0.9f, 0f);

                go.AddComponent<NpcInteractable>().prompt = "Talk";
            }
        }
    }
}
