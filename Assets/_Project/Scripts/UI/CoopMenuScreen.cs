#if SIMCITY_NETCODE
using UnityEngine;
using Simcity.Stats;
using Simcity.World;
using Simcity.Net;

namespace Simcity.UI
{
    /// <summary>
    /// The co-op front door, shown after character creation when SIMCITY_NETCODE is on.
    /// Choose to HOST (get a share code), JOIN by a friend's code, or play SOLO. Uses
    /// OnGUI so it needs no Canvas setup, matching the rest of the project's greybox UI.
    /// </summary>
    public class CoopMenuScreen : MonoBehaviour
    {
        private AppearanceConfig _appearance;
        private GameObject _root;
        private string _code = "";
        private bool _busy;
        private GUIStyle _label, _title;

        public static void Show(AppearanceConfig appearance)
        {
            WorldCommon.EnsureLighting();
            foreach (var cam in FindObjectsOfType<Camera>()) cam.gameObject.SetActive(false);
            foreach (var l in FindObjectsOfType<AudioListener>()) l.enabled = false;

            var root = new GameObject("CoopMenu");
            var camGo = new GameObject("CoopMenuCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(root.transform);
            camGo.transform.position = new Vector3(0f, 2f, -6f);
            camGo.transform.LookAt(new Vector3(0f, 1f, 0f));
            camGo.AddComponent<Camera>();
            camGo.AddComponent<AudioListener>();

            var screen = root.AddComponent<CoopMenuScreen>();
            screen._appearance = appearance;
            screen._root = root;

            Debug.Log("[Coop] Choose Host, Join, or Solo.");
        }

        private CoopService EnsureService()
        {
            return CoopService.Instance != null
                ? CoopService.Instance
                : new GameObject("CoopService").AddComponent<CoopService>();
        }

        private async void OnHost()
        {
            if (_busy) return;
            _busy = true;
            CoopWorld.Build(_appearance);
            var code = await EnsureService().HostAsync();
            _busy = false;
            if (code != null) Dismiss(); // hosting; the player rig auto-spawns with its own camera
        }

        private async void OnJoin()
        {
            if (_busy || string.IsNullOrWhiteSpace(_code)) return;
            _busy = true;
            CoopWorld.Build(_appearance);
            bool ok = await EnsureService().JoinAsync(_code);
            _busy = false;
            if (ok) Dismiss();
        }

        private void OnSolo()
        {
            if (_busy) return;
            Destroy(_root);                 // drop the menu camera
            TownWorld.Build(_appearance);   // the offline single-player town (GAME_DESIGN §9 B)
        }

        private void Dismiss() => Destroy(_root);

        private void OnGUI()
        {
            EnsureStyles();

            float w = 420f, x = (Screen.width - w) * 0.5f, y = Screen.height * 0.28f;
            GUI.Label(new Rect(x, y - 54, w, 40), "Co-op", _title);

            GUI.enabled = !_busy;
            if (GUI.Button(new Rect(x, y, w, 44), "Host a town  (get a code to share)")) OnHost();

            y += 64;
            GUI.Label(new Rect(x, y, 120, 30), "Friend's code:", _label);
            _code = GUI.TextField(new Rect(x + 124, y, 180, 30), _code, 12);
            if (GUI.Button(new Rect(x + 312, y, w - 312, 30), "Join")) OnJoin();

            y += 56;
            if (GUI.Button(new Rect(x, y, w, 36), "Play solo (offline)")) OnSolo();
            GUI.enabled = true;

            var svc = CoopService.Instance;
            if (svc != null)
            {
                y += 52;
                if (!string.IsNullOrEmpty(svc.LobbyCode))
                    GUI.Label(new Rect(x, y, w, 30), $"Share this code: {svc.LobbyCode}", _title);
                if (!string.IsNullOrEmpty(svc.Status))
                    GUI.Label(new Rect(x, y + 30, w, 30), svc.Status, _label);
            }
        }

        private void EnsureStyles()
        {
            if (_label != null) return;
            _label = new GUIStyle(GUI.skin.label) { fontSize = 15, normal = { textColor = Color.white } };
            _title = new GUIStyle(GUI.skin.label)
            { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
        }
    }
}
#endif
