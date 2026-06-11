#if SIMCITY_NETCODE
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Simcity.Net
{
    /// <summary>
    /// Phase 6 connection spine. Wraps Unity Gaming Services so friends can play
    /// together over the internet with no port-forwarding:
    ///
    ///   HOST  → init UGS + anon sign-in → Relay allocation → create a Lobby that
    ///           carries the Relay join code → StartHost → heartbeat the lobby.
    ///   JOIN  → init UGS + anon sign-in → join Lobby by its short code → read the
    ///           Relay join code from lobby data → join the Relay → StartClient.
    ///
    /// The short code friends share is the LOBBY code (6 chars). Relay does the actual
    /// NAT-punching transport; the Lobby is the discovery/handshake layer (GAME_DESIGN §9/§12).
    ///
    /// Everything here is compiled only when the SIMCITY_NETCODE define + the
    /// com.unity.netcode.gameobjects / com.unity.services.* packages are present. Without
    /// them the game runs single-player exactly as in Phases 0-5.
    /// </summary>
    public class CoopService : MonoBehaviour
    {
        public static CoopService Instance { get; private set; }

        public const int MaxPlayers = 4;           // host + 3 friends — a co-op group, not a server
        private const string ConnectionType = "dtls";
        private const string RelayCodeKey = "relayJoinCode";

        /// <summary>Status string for the menu UI to surface ("Connecting…", errors, etc.).</summary>
        public string Status { get; private set; } = "";
        /// <summary>The lobby code to share with friends once hosting (empty until hosted).</summary>
        public string LobbyCode { get; private set; } = "";
        public bool IsBusy { get; private set; }

        private Lobby _lobby;
        private Coroutine _heartbeat;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Host a session. Returns the lobby code on success, null on failure.</summary>
        public async Task<string> HostAsync()
        {
            if (IsBusy) return null;
            IsBusy = true;
            try
            {
                await EnsureSignedIn();

                Status = "Allocating Relay…";
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(MaxPlayers - 1);
                string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                GetTransport().SetRelayServerData(new RelayServerData(allocation, ConnectionType));

                Status = "Creating lobby…";
                var options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new System.Collections.Generic.Dictionary<string, DataObject>
                    {
                        { RelayCodeKey, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) },
                    },
                };
                _lobby = await LobbyService.Instance.CreateLobbyAsync("Co-op Town", MaxPlayers, options);
                LobbyCode = _lobby.LobbyCode;
                _heartbeat = StartCoroutine(HeartbeatLoop(_lobby.Id, 15f));

                if (!NetworkManager.Singleton.StartHost())
                    throw new Exception("NetworkManager.StartHost() returned false.");

                Status = $"Hosting — share code {LobbyCode}";
                Debug.Log($"[Coop] Hosting. Lobby code: {LobbyCode}  (relay: {relayJoinCode})");
                return LobbyCode;
            }
            catch (Exception e)
            {
                Status = $"Host failed: {e.Message}";
                Debug.LogError($"[Coop] Host failed: {e}");
                await CleanupLobby();
                return null;
            }
            finally { IsBusy = false; }
        }

        /// <summary>Join a session by the host's lobby code. Returns true on success.</summary>
        public async Task<bool> JoinAsync(string lobbyCode)
        {
            if (IsBusy || string.IsNullOrWhiteSpace(lobbyCode)) return false;
            IsBusy = true;
            try
            {
                await EnsureSignedIn();

                Status = "Joining lobby…";
                _lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode.Trim().ToUpperInvariant());
                LobbyCode = _lobby.LobbyCode;

                if (!_lobby.Data.TryGetValue(RelayCodeKey, out var relayData) || string.IsNullOrEmpty(relayData.Value))
                    throw new Exception("Lobby has no Relay join code.");

                Status = "Joining Relay…";
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(relayData.Value);
                GetTransport().SetRelayServerData(new RelayServerData(allocation, ConnectionType));

                if (!NetworkManager.Singleton.StartClient())
                    throw new Exception("NetworkManager.StartClient() returned false.");

                Status = "Connected.";
                Debug.Log($"[Coop] Joined lobby {LobbyCode}.");
                return true;
            }
            catch (Exception e)
            {
                Status = $"Join failed: {e.Message}";
                Debug.LogError($"[Coop] Join failed: {e}");
                await CleanupLobby();
                return false;
            }
            finally { IsBusy = false; }
        }

        public async void Shutdown()
        {
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                NetworkManager.Singleton.Shutdown();
            await CleanupLobby();
            Status = "";
            LobbyCode = "";
        }

        private static async Task EnsureSignedIn()
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
                await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        private static UnityTransport GetTransport()
        {
            var t = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (t == null) throw new Exception("NetworkManager is missing a UnityTransport.");
            return t;
        }

        // Lobbies expire after ~30s without a heartbeat; ping while we host.
        private IEnumerator HeartbeatLoop(string lobbyId, float interval)
        {
            var wait = new WaitForSecondsRealtime(interval);
            while (true)
            {
                yield return wait;
                _ = SafeHeartbeat(lobbyId);
            }
        }

        private static async Task SafeHeartbeat(string lobbyId)
        {
            try { await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId); }
            catch (Exception e) { Debug.LogWarning($"[Coop] Heartbeat failed: {e.Message}"); }
        }

        private async Task CleanupLobby()
        {
            if (_heartbeat != null) { StopCoroutine(_heartbeat); _heartbeat = null; }
            if (_lobby == null) return;
            try
            {
                bool isHost = _lobby.HostId == AuthenticationService.Instance.PlayerId;
                if (isHost) await LobbyService.Instance.DeleteLobbyAsync(_lobby.Id);
                else await LobbyService.Instance.RemovePlayerAsync(_lobby.Id, AuthenticationService.Instance.PlayerId);
            }
            catch (Exception e) { Debug.LogWarning($"[Coop] Lobby cleanup: {e.Message}"); }
            finally { _lobby = null; }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
#endif
