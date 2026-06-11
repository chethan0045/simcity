#if SIMCITY_NETCODE
using System.Reflection;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Transports.UTP;

namespace Simcity.Net
{
    /// <summary>
    /// Stands up the Netcode-for-GameObjects runtime entirely in code (this project has no
    /// scenes or prefab assets — everything is built procedurally, Phases 0-5). It creates a
    /// NetworkManager + UnityTransport and three runtime "prefab" templates — player, NPC,
    /// and a systems object (the networked clock) — registering each with a fixed
    /// GlobalObjectIdHash so host and clients agree on what's what.
    ///
    /// On the host, NPCs and the clock are spawned once the server starts. Players are
    /// auto-spawned by NGO (the PlayerPrefab) as each friend connects.
    /// </summary>
    public static class NetworkBootstrap
    {
        // Stable, hand-assigned prefab hashes (host & client must match). Arbitrary but fixed.
        private const uint PlayerHash = 0x5C170001;
        private const uint NpcHash = 0x5C170002;
        private const uint SystemsHash = 0x5C170003;

        private const int NpcCount = 5;
        private const float NpcRing = 5f;

        private static GameObject _playerTemplate;
        private static GameObject _npcTemplate;
        private static GameObject _systemsTemplate;
        private static bool _hooked;

        /// <summary>Idempotently create the NetworkManager + register prefabs. Call before
        /// CoopService.HostAsync()/JoinAsync().</summary>
        public static void Ensure()
        {
            if (NetworkManager.Singleton != null) return;

            var nmGo = new GameObject("NetworkManager");
            Object.DontDestroyOnLoad(nmGo);
            var nm = nmGo.AddComponent<NetworkManager>();
            var transport = nmGo.AddComponent<UnityTransport>();

            BuildTemplates();

            nm.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
                ConnectionApproval = false,
                EnableSceneManagement = false,   // we build the world in code on every machine
                PlayerPrefab = _playerTemplate,
            };
            nm.AddNetworkPrefab(_npcTemplate);
            nm.AddNetworkPrefab(_systemsTemplate);

            if (!_hooked)
            {
                nm.OnServerStarted += OnServerStarted;
                _hooked = true;
            }
        }

        private static void BuildTemplates()
        {
            if (_playerTemplate != null) return;

            // --- Player: owner-authoritative movement, gameplay layer added on spawn. ---
            _playerTemplate = NewTemplate("NetPlayerTemplate", PlayerHash);
            var cc = _playerTemplate.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.9f, 0f);
            _playerTemplate.AddComponent<OwnerNetworkTransform>();
            _playerTemplate.AddComponent<NetworkPlayer>();

            // --- NPC: server-authoritative transform, host-driven brain. ---
            _npcTemplate = NewTemplate("NetNpcTemplate", NpcHash);
            _npcTemplate.AddComponent<NetworkTransform>(); // default: server authoritative
            _npcTemplate.AddComponent<NetworkNpc>();

            // --- Systems: the replicated game clock. ---
            _systemsTemplate = NewTemplate("NetSystemsTemplate", SystemsHash);
            _systemsTemplate.AddComponent<NetworkClock>();
        }

        private static GameObject NewTemplate(string name, uint hash)
        {
            var go = new GameObject(name);
            go.SetActive(false);              // a template, not a live object
            Object.DontDestroyOnLoad(go);
            var no = go.AddComponent<NetworkObject>();
            ForceHash(no, hash);
            return go;
        }

        // NetworkObject's GlobalObjectIdHash is normally derived from an asset GUID at edit
        // time. With code-only prefabs we assign it directly so every machine hashes alike.
        private static void ForceHash(NetworkObject no, uint hash)
        {
            var field = typeof(NetworkObject).GetField("GlobalObjectIdHash",
                BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(no, hash);
        }

        private static void OnServerStarted()
        {
            // The host owns the town's NPCs and its clock.
            SpawnNpcs();
            SpawnNetworked(_systemsTemplate, Vector3.zero, 0, isNpc: false);
            Debug.Log($"[Coop] Server up — spawned {NpcCount} villagers + networked clock.");
        }

        private static void SpawnNpcs()
        {
            for (int i = 0; i < NpcCount; i++)
            {
                float ang = Mathf.Deg2Rad * (360f / NpcCount) * i;
                var pos = new Vector3(Mathf.Cos(ang) * NpcRing, 0f, Mathf.Sin(ang) * NpcRing);
                SpawnNetworked(_npcTemplate, pos, 2000 + i, isNpc: true);
            }
        }

        private static void SpawnNetworked(GameObject template, Vector3 pos, int seed, bool isNpc)
        {
            var go = Object.Instantiate(template, pos, Quaternion.identity);
            go.SetActive(true);
            if (isNpc) go.GetComponent<NetworkNpc>().SetSeed(seed); // before Spawn -> in initial snapshot
            go.GetComponent<NetworkObject>().Spawn();
        }
    }
}
#endif
