using UnityEngine;
using UnityEngine.Rendering;
using Simcity.Common;
using Simcity.Player;
using Simcity.Interaction;

namespace Simcity.Bootstrap
{
    /// <summary>
    /// Builds a playable greybox test world (lighting, ground, walls, obstacles,
    /// interactables, first-person player) at runtime.
    ///
    /// SUPERSEDED BY Phase1Bootstrap: auto-run is disabled so the two builders don't
    /// race. To use the old greybox instead of the apartment, comment out the
    /// [RuntimeInitializeOnLoadMethod] on Phase1Bootstrap and uncomment the one below.
    /// </summary>
    public static class Phase0Bootstrap
    {
        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Build()
        {
            // If a player is already in the scene, assume it's an authored scene and bail.
            if (Object.FindObjectOfType<FirstPersonController>() != null) return;

            EnsureLighting();
            BuildGround();
            BuildWalls();
            BuildInteractables();
            BuildPlayer();

            Debug.Log("[Phase0Bootstrap] Greybox world built — move with WASD, look with the mouse, " +
                      "and press E while looking at a block.");
        }

        private static void EnsureLighting()
        {
            if (Object.FindObjectOfType<Light>() == null)
            {
                var go = new GameObject("Directional Light");
                var light = go.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.1f;
                light.shadows = LightShadows.Soft;
                go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.45f, 0.47f, 0.5f);
        }

        private static void BuildGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(6f, 1f, 6f); // Plane is 10x10m -> 60x60m
            MaterialUtils.SetColor(ground.GetComponent<Renderer>(), new Color(0.34f, 0.4f, 0.32f));
        }

        private static void BuildWalls()
        {
            var wall = new Color(0.55f, 0.55f, 0.6f);
            const float s = 28f, h = 3f, t = 0.5f;

            CreateBox("Wall_N", new Vector3(0, h / 2f, s / 2f), new Vector3(s + t, h, t), wall);
            CreateBox("Wall_S", new Vector3(0, h / 2f, -s / 2f), new Vector3(s + t, h, t), wall);
            CreateBox("Wall_E", new Vector3(s / 2f, h / 2f, 0), new Vector3(t, h, s + t), wall);
            CreateBox("Wall_W", new Vector3(-s / 2f, h / 2f, 0), new Vector3(t, h, s + t), wall);

            // A couple of obstacles to give the space depth and test collision.
            CreateBox("Pillar_1", new Vector3(5f, 1.5f, 4f), new Vector3(1f, 3f, 1f), wall);
            CreateBox("Pillar_2", new Vector3(-6f, 1.5f, 7f), new Vector3(1f, 3f, 1f), wall);
            CreateBox("Ramp", new Vector3(8f, 0.5f, -4f), new Vector3(3f, 1f, 6f), wall);
        }

        private static void BuildInteractables()
        {
            CreateInteractable("Lever_A", new Vector3(2f, 1f, 2f), "Toggle");
            CreateInteractable("Lever_B", new Vector3(-3f, 1f, 3f), "Toggle");
            CreateInteractable("Crate", new Vector3(0f, 0.5f, 6f), "Use");
        }

        private static void BuildPlayer()
        {
            // Disable any pre-existing camera/listener (e.g. the template's Main Camera)
            // so only the first-person rig is active.
            foreach (var cam in Object.FindObjectsOfType<Camera>()) cam.gameObject.SetActive(false);
            foreach (var listener in Object.FindObjectsOfType<AudioListener>()) listener.enabled = false;

            var player = new GameObject("Player");
            player.transform.position = new Vector3(0f, 1.2f, -8f);

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
        }

        private static GameObject CreateBox(string name, Vector3 pos, Vector3 size, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = size;
            MaterialUtils.SetColor(go.GetComponent<Renderer>(), color);
            return go;
        }

        private static void CreateInteractable(string name, Vector3 pos, string prompt)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            var interactable = go.AddComponent<GreyboxInteractable>();
            interactable.prompt = prompt;
        }
    }
}
