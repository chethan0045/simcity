using UnityEngine;
using Simcity.Player;

namespace Simcity.UI
{
    /// <summary>
    /// First-run onboarding. Shows the controls and the core loop the first time you play
    /// (tracked in PlayerPrefs), freezes the player while up, and dismisses with Enter or the
    /// button. Press <b>F1</b> any time to bring it back. Placeholder for the guided,
    /// step-by-step tutorial that lands with the art pass.
    /// </summary>
    public class TutorialScreen : MonoBehaviour
    {
        private const string DoneKey = "simcity.tutorial.done";

        private bool _open;
        private CursorLockMode _prevLock;
        private bool _prevVisible;
        private GUIStyle _label, _title;

        private static readonly string[] Lines =
        {
            "Move  —  WASD / left thumb-stick      Look  —  mouse / right-drag",
            "Sprint  —  Shift / push stick far      Jump  —  Space / button",
            "Interact  —  E  (look at a station, then press)",
            "",
            "The loop:  Craft at the Workbench  →  Sell at the Market  →  earn Coins",
            "Live:  Eat at the Diner · Sleep in a Bed · pay Rent · Work for quick Coins",
            "Bond:  walk up to villagers and Talk to build relationships",
            "Money:  visit the Bank to buy / cash out Coins (sandbox — no real money)",
            "",
            "F10  settings · quality + sensitivity        F1  reopen this",
        };

        private void Start()
        {
            if (PlayerPrefs.GetInt(DoneKey, 0) == 0) SetOpen(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) SetOpen(!_open);
            else if (_open && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                Dismiss();
        }

        private void SetOpen(bool open)
        {
            _open = open;
            if (open)
            {
                _prevLock = Cursor.lockState;
                _prevVisible = Cursor.visible;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                if (FirstPersonController.Instance != null) FirstPersonController.Instance.enabled = false;
            }
            else
            {
                Cursor.lockState = _prevLock;
                Cursor.visible = _prevVisible;
                if (FirstPersonController.Instance != null) FirstPersonController.Instance.enabled = true;
            }
        }

        private void Dismiss()
        {
            SetOpen(false);
            PlayerPrefs.SetInt(DoneKey, 1);
            PlayerPrefs.Save();
        }

        private void OnGUI()
        {
            if (!_open) return;
            EnsureStyles();

            float w = 620f, h = 320f;
            var panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 24, panel.y + 18, w - 48, h - 36));

            GUILayout.Label("Welcome — a life in the town", _title);
            GUILayout.Space(8);
            foreach (var line in Lines) GUILayout.Label(line, _label);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Got it  (Enter)", GUILayout.Height(30))) Dismiss();
            GUILayout.EndArea();
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
