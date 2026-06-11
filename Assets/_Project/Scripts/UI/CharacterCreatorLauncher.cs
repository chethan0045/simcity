using System;
using UnityEngine;
using Simcity.Common;
using Simcity.Stats;
using Simcity.World;

namespace Simcity.UI
{
    /// <summary>
    /// Spins up the character creator scene (camera, pedestal, turntable preview,
    /// UI) and calls back with the chosen AppearanceConfig once the player hits
    /// "Start Life". Shared by the phase bootstrappers so each can decide which
    /// world to build afterward. Tears its own objects down on completion.
    /// </summary>
    public static class CharacterCreatorLauncher
    {
        public static void Launch(Action<AppearanceConfig> onComplete)
        {
            WorldCommon.EnsureLighting();

            var root = new GameObject("CharacterCreator");

            foreach (var cam in UnityEngine.Object.FindObjectsOfType<Camera>()) cam.gameObject.SetActive(false);
            foreach (var listener in UnityEngine.Object.FindObjectsOfType<AudioListener>()) listener.enabled = false;

            var camGo = new GameObject("CreatorCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(root.transform);
            camGo.transform.position = new Vector3(0.6f, 1.4f, 3.2f);
            camGo.transform.LookAt(new Vector3(0f, 1.0f, 0f));
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();

            var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Pedestal";
            pedestal.transform.SetParent(root.transform);
            pedestal.transform.position = new Vector3(0f, 0.05f, 0f);
            pedestal.transform.localScale = new Vector3(1.4f, 0.1f, 1.4f);
            MaterialUtils.SetColor(pedestal.GetComponent<Renderer>(), new Color(0.3f, 0.3f, 0.34f));

            var pivot = new GameObject("PreviewPivot").transform;
            pivot.SetParent(root.transform);
            pivot.position = new Vector3(0f, 0.1f, 0f);

            var preview = new GameObject("PreviewAvatar").AddComponent<CharacterAppearance>();
            preview.transform.SetParent(pivot, false);

            var screen = root.AddComponent<CharacterCreationScreen>();
            screen.preview = preview;
            screen.previewPivot = pivot;
            screen.config = AppearanceConfig.CreateDefault();
            screen.OnComplete = cfg =>
            {
                UnityEngine.Object.Destroy(root); // tears down creator camera + preview
                onComplete?.Invoke(cfg);
            };

            Debug.Log("[Creator] Customize your character, then press 'Start Life ▶'.");
        }
    }
}
