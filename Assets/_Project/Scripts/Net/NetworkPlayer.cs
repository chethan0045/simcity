#if SIMCITY_NETCODE
using Unity.Netcode;
using UnityEngine;
using Simcity.Stats;
using Simcity.Social;
using Simcity.Economy;
using Simcity.World;

namespace Simcity.Net
{
    /// <summary>
    /// The per-player networked behaviour. Every connected friend gets one of these
    /// (auto-spawned by NGO as the PlayerPrefab). It decides, on spawn, whether THIS
    /// machine owns the player:
    ///   • Owner  → full first-person rig (camera, input, needs, wallet, HUD) + publishes
    ///              its chosen look so others can see it.
    ///   • Remote → just an avatar with a name you can walk up to and gift.
    /// Movement is owner-authoritative (OwnerNetworkTransform); the look replicates via a
    /// NetworkVariable. Coins move between players over RPCs (TryGiveCoins).
    /// </summary>
    public class NetworkPlayer : NetworkBehaviour
    {
        // Owner-writable so each player publishes its own appearance to everyone else.
        private readonly NetworkVariable<NetAppearance> _appearance =
            new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private CharacterAppearance _avatar;

        public override void OnNetworkSpawn()
        {
            gameObject.name = IsOwner ? "NetPlayer (You)" : "NetPlayer (Remote)";

            _avatar = gameObject.AddComponent<CharacterAppearance>();
            _avatar.hideHeadForFirstPerson = IsOwner; // don't render our own head into our camera

            if (IsOwner)
            {
                var cfg = CoopWorld.LocalAppearance ?? AppearanceConfig.CreateDefault();
                _appearance.Value = NetAppearance.From(cfg);     // tell everyone what we look like
                PlayerFactory.OutfitNetworkedOwner(gameObject, cfg);
                _avatar.Apply(cfg);
            }
            else
            {
                var cfg = _appearance.Value.ToConfig();
                PlayerFactory.OutfitNetworkedRemote(gameObject, cfg);
                _avatar.Apply(cfg);
                gameObject.AddComponent<PlayerGiftInteractable>();
                _appearance.OnValueChanged += OnAppearanceChanged; // catch a late-arriving look
            }
        }

        public override void OnNetworkDespawn()
        {
            _appearance.OnValueChanged -= OnAppearanceChanged;
        }

        private void OnAppearanceChanged(NetAppearance _, NetAppearance v)
        {
            var cfg = v.ToConfig();
            _avatar.Apply(cfg);
            var ch = GetComponent<Character>();
            if (ch != null) ch.displayName = cfg.characterName;
        }

        // ---- Player-to-player coin gifting (the economy becomes co-op, GAME_DESIGN §6/§9) ----

        /// <summary>Called on the GIVER's local player when they gift the player owned by
        /// <paramref name="receiverClientId"/>. Deducts locally (optimistic), then asks the
        /// server to credit the receiver on their own machine.</summary>
        public void TryGiveCoins(ulong receiverClientId, int amount)
        {
            if (!IsOwner || amount <= 0) return;
            var wallet = GetComponent<Wallet>();
            if (wallet == null || !wallet.TrySpend(amount))
            {
                Debug.Log("[Gift] Not enough Coins to give.");
                return;
            }
            Debug.Log($"[Gift] You gave {amount} Coins to a friend.");
            GiveCoinsServerRpc(receiverClientId, amount);
        }

        [ServerRpc]
        private void GiveCoinsServerRpc(ulong receiverClientId, int amount)
        {
            if (!NetworkManager.ConnectedClients.TryGetValue(receiverClientId, out var client)) return;
            var receiver = client.PlayerObject != null ? client.PlayerObject.GetComponent<NetworkPlayer>() : null;
            if (receiver == null) return;

            var target = new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { receiverClientId } }
            };
            receiver.CreditCoinsClientRpc(amount, target);
        }

        [ClientRpc]
        private void CreditCoinsClientRpc(int amount, ClientRpcParams _ = default)
        {
            if (!IsOwner) return; // only the receiver's own machine holds (and credits) their wallet
            var wallet = GetComponent<Wallet>();
            if (wallet == null) return;
            wallet.Add(amount);
            Debug.Log($"[Gift] A friend gave you {amount} Coins.");
        }
    }
}
#endif
