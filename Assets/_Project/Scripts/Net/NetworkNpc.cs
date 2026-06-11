#if SIMCITY_NETCODE
using Unity.Netcode;
using UnityEngine;
using Simcity.Stats;
using Simcity.Social;
using Simcity.AI;
using Simcity.Interaction;

namespace Simcity.Net
{
    /// <summary>
    /// A villager that lives on the HOST and is mirrored to every client. The host runs
    /// the brain (NpcBrain + SimpleMover + needs) and a server-authoritative NetworkTransform
    /// streams its movement out, so the whole group watches the same NPCs eat/sleep/work.
    ///
    /// The look isn't sent over the wire — only a small integer SEED is. Each machine
    /// rebuilds the identical randomized appearance + name from that seed, which is far
    /// cheaper than replicating colors and keeps NPCs looking the same for everyone.
    /// </summary>
    public class NetworkNpc : NetworkBehaviour
    {
        private readonly NetworkVariable<int> _seed = new();
        private CharacterAppearance _avatar;
        private Character _character;

        /// <summary>Server-only: set before NetworkObject.Spawn() so the seed is part of the
        /// initial spawn snapshot and clients build the right villager with no flicker.</summary>
        public void SetSeed(int seed) => _seed.Value = seed;

        public override void OnNetworkSpawn()
        {
            BuildVisual(_seed.Value);
            if (IsServer) BuildBrain(_seed.Value);
            else _seed.OnValueChanged += OnSeedChanged;
        }

        public override void OnNetworkDespawn()
        {
            _seed.OnValueChanged -= OnSeedChanged;
        }

        private AppearanceConfig ConfigFor(int seed)
        {
            var cfg = AppearanceConfig.CreateDefault();
            cfg.Randomize(new System.Random(seed));
            return cfg;
        }

        // Built on every machine — the body, social identity, and "Talk" target.
        private void BuildVisual(int seed)
        {
            var cfg = ConfigFor(seed);

            _avatar = gameObject.AddComponent<CharacterAppearance>();
            _avatar.hideHeadForFirstPerson = false;
            _avatar.Apply(cfg);

            gameObject.AddComponent<Relationships>();
            _character = gameObject.AddComponent<Character>();
            _character.displayName = cfg.characterName;
            _character.sociability = Mathf.Clamp01((float)new System.Random(seed + 1).NextDouble());

            var col = gameObject.AddComponent<CapsuleCollider>();
            col.height = 1.8f;
            col.radius = 0.35f;
            col.center = new Vector3(0f, 0.9f, 0f);

            gameObject.AddComponent<NpcInteractable>().prompt = "Talk";
            gameObject.name = $"Villager ({cfg.characterName})";
        }

        // Server-only — the utility AI that actually drives the villager's life.
        private void BuildBrain(int seed)
        {
            var needs = gameObject.AddComponent<CharacterNeeds>();
            gameObject.AddComponent<SimpleMover>();
            var brain = gameObject.AddComponent<NpcBrain>();
            brain.displayName = _character != null ? _character.displayName : "Villager";

            var rng = new System.Random(seed);
            needs.Replenish(NeedType.Hunger, -rng.Next(0, 40));
            needs.Replenish(NeedType.Energy, -rng.Next(0, 40));
        }

        private void OnSeedChanged(int _, int v)
        {
            var cfg = ConfigFor(v);
            if (_avatar != null) _avatar.Apply(cfg);
            if (_character != null) _character.displayName = cfg.characterName;
        }
    }
}
#endif
