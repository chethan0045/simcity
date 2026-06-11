using UnityEngine;
using Simcity.Player;
using Simcity.Interaction;
using Simcity.Stats;
using Simcity.Economy;
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
    }
}
