using UnityEngine;
using Simcity.Player;
using Simcity.Interaction;
using Simcity.Stats;
using Simcity.Economy;
using Simcity.Economy.RealMoney;
using Simcity.Social;
using Simcity.UI;

namespace Simcity.World
{
    /// <summary>Builds the first-person player rig (controller, camera, interactor,
    /// needs, wallet, housing, customized body) + HUD. Shared by all world builders.</summary>
    public static class PlayerFactory
    {
        public static GameObject BuildPlayer(AppearanceConfig appearance, Vector3 spawn)
        {
            // Only the player rig should have an active camera / listener.
            foreach (var cam in Object.FindObjectsOfType<Camera>()) cam.gameObject.SetActive(false);
            foreach (var listener in Object.FindObjectsOfType<AudioListener>()) listener.enabled = false;

            var player = new GameObject("Player");
            player.transform.position = spawn;

            var cc = player.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.3f;
            cc.center = new Vector3(0f, 0.9f, 0f);

            var fpc = player.AddComponent<FirstPersonController>();

            var camGo = new GameObject("PlayerCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(player.transform);
            camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            fpc.SetCamera(camGo.transform);

            var interactor = player.AddComponent<PlayerInteractor>();
            interactor.rayOrigin = camGo.transform;

            player.AddComponent<CharacterNeeds>();
            player.AddComponent<Wallet>();   // Housing requires Wallet — add first.
            player.AddComponent<Housing>();
            player.AddComponent<DevTools>();

            // Social identity (Relationships before Character — Character requires it).
            player.AddComponent<Relationships>();
            var character = player.AddComponent<Character>();
            character.isPlayer = true;
            character.sociability = 0.6f;
            character.displayName = string.IsNullOrWhiteSpace(appearance?.characterName) ? "You" : appearance.characterName;

            // Economy: hold crafted goods and a seller reputation.
            player.AddComponent<Inventory>();
            player.AddComponent<SellerProfile>();
            player.AddComponent<RealMoneyService>(); // Phase 7 buy/cash-out (sandbox); requires Wallet

            var appear = player.AddComponent<CharacterAppearance>();
            appear.hideHeadForFirstPerson = true;
            appear.Apply(appearance ?? AppearanceConfig.CreateDefault());

            return player;
        }

        public static void BuildHud(GameObject player)
        {
            var hud = new GameObject("PlayerHud").AddComponent<PlayerHud>();
            hud.needs = player.GetComponent<CharacterNeeds>();
            hud.wallet = player.GetComponent<Wallet>();
            hud.housing = player.GetComponent<Housing>();
            hud.relationships = player.GetComponent<Relationships>();
            hud.inventory = player.GetComponent<Inventory>();
            hud.seller = player.GetComponent<SellerProfile>();
        }

        /// <summary>Phase 6: turn an already-spawned networked player object that THIS
        /// machine owns into the full first-person rig (camera, input, needs, wallet, HUD).
        /// The GameObject + CharacterController already exist (from the network prefab); we
        /// add the local-only gameplay layer that must never run on a remote copy.
        /// Appearance is applied separately by NetworkPlayer (it owns the synced look).</summary>
        public static void OutfitNetworkedOwner(GameObject player, AppearanceConfig appearance)
        {
            foreach (var cam in Object.FindObjectsOfType<Camera>()) cam.gameObject.SetActive(false);
            foreach (var listener in Object.FindObjectsOfType<AudioListener>()) listener.enabled = false;

            var fpc = player.AddComponent<FirstPersonController>();

            var camGo = new GameObject("PlayerCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(player.transform);
            camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();
            fpc.SetCamera(camGo.transform);

            var interactor = player.AddComponent<PlayerInteractor>();
            interactor.rayOrigin = camGo.transform;

            player.AddComponent<CharacterNeeds>();
            player.AddComponent<Wallet>();   // Housing requires Wallet — add first.
            player.AddComponent<Housing>();
            player.AddComponent<DevTools>();

            player.AddComponent<Relationships>();
            var character = player.AddComponent<Character>();
            character.isPlayer = true;
            character.sociability = 0.6f;
            character.displayName = string.IsNullOrWhiteSpace(appearance?.characterName) ? "You" : appearance.characterName;

            player.AddComponent<Inventory>();
            player.AddComponent<SellerProfile>();
            player.AddComponent<RealMoneyService>(); // Phase 7 buy/cash-out (sandbox); requires Wallet

            BuildHud(player);
        }

        /// <summary>Phase 6: a player object owned by SOMEONE ELSE — just an avatar with a
        /// social identity so we can target them (Talk, gift). No needs/wallet/HUD: those
        /// are simulated on the owning machine and replicated, not run here.</summary>
        public static void OutfitNetworkedRemote(GameObject player, AppearanceConfig appearance)
        {
            player.AddComponent<Relationships>();
            var character = player.AddComponent<Character>();
            character.isPlayer = false; // not THIS machine's player
            character.displayName = string.IsNullOrWhiteSpace(appearance?.characterName) ? "Friend" : appearance.characterName;
        }
    }
}
