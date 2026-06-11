using UnityEngine;
using Simcity.Common;
using Simcity.Player;

namespace Simcity.UI
{
    /// <summary>
    /// Press <b>F10</b> for settings: pick a quality tier and mouse sensitivity, applied live
    /// and persisted (QualityManager). A modal OnGUI overlay that frees the cursor and freezes
    /// the player while open — placeholder for the real UGUI options menu in the art pass.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
        private bool _open;
        private float _sens;
        private CursorLockMode _prevLock;
        private bool _prevVisible;
        private GUIStyle _label, _title, _hint;

        private void Awake() => _sens = QualityManager.MouseSensitivity;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10)) Toggle();
            else if (_open && Input.GetKeyDown(KeyCode.Escape)) Toggle();
        }

        private void Toggle()
        {
            _open = !_open;
            if (_open)
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

        private void OnGUI()
        {
            EnsureStyles();

            GUI.Label(new Rect(12, Screen.height - 26, 260, 22), "F10 settings · F1 tutorial", _hint);
            if (!_open) return;

            float w = 360f, h = 250f;
            var panel = new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h);
            GUI.Box(panel, GUIContent.none);
            GUILayout.BeginArea(new Rect(panel.x + 18, panel.y + 14, w - 36, h - 28));

            GUILayout.Label("Settings", _title);
            GUILayout.Space(8);

            GUILayout.Label($"Quality: {QualityManager.Tier}", _label);
            GUILayout.BeginHorizontal();
            foreach (QualityTier tier in System.Enum.GetValues(typeof(QualityTier)))
            {
                bool cur = QualityManager.Tier == tier;
                if (GUILayout.Button(cur ? $"[{tier}]" : tier.ToString(), GUILayout.Height(30)))
                    QualityManager.Apply(tier);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(12);
            GUILayout.Label($"Mouse sensitivity: {_sens:0.0}", _label);
            float v = GUILayout.HorizontalSlider(_sens, 0.5f, 8f);
            if (!Mathf.Approximately(v, _sens))
            {
                _sens = v;
                QualityManager.SetSensitivity(_sens);
                if (FirstPersonController.Instance != null) FirstPersonController.Instance.mouseSensitivity = _sens;
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Close (F10)", GUILayout.Height(28))) Toggle();
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (_label != null) return;
            _label = new GUIStyle(GUI.skin.label) { fontSize = 14, normal = { textColor = Color.white } };
            _title = new GUIStyle(GUI.skin.label)
            { fontSize = 20, fontStyle = FontStyle.Bold, normal = { textColor = Color.white } };
            _hint = new GUIStyle(GUI.skin.label) { fontSize = 12, normal = { textColor = new Color(1, 1, 1, 0.5f) } };
        }
    }
}
