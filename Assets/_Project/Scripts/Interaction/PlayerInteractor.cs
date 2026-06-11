using UnityEngine;

namespace Simcity.Interaction
{
    /// <summary>
    /// Raycasts forward from the camera each frame, tracks the focused Interactable,
    /// shows a prompt + crosshair (via OnGUI so Phase 0 needs no Canvas setup), and
    /// triggers the interaction on the interact key.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        public float range = 3f;
        public KeyCode interactKey = KeyCode.E;

        [Tooltip("Where the interaction ray starts — usually the player camera.")]
        public Transform rayOrigin;

        private Interactable _focus;
        private GUIStyle _hudStyle;

        private void Update()
        {
            _focus = null;
            Transform origin = rayOrigin != null ? rayOrigin : transform;

            if (Physics.Raycast(origin.position, origin.forward, out RaycastHit hit, range))
                _focus = hit.collider.GetComponentInParent<Interactable>();

            if (_focus != null && Input.GetKeyDown(interactKey))
                _focus.Interact(gameObject);
        }

        private void OnGUI()
        {
            EnsureStyle();

            float cx = Screen.width * 0.5f;
            float cy = Screen.height * 0.5f;

            // Crosshair
            GUI.Label(new Rect(cx - 6, cy - 12, 24, 24), "+", _hudStyle);

            // Controls hint
            GUI.Label(new Rect(12, 10, 800, 30),
                "WASD move · Mouse look · Shift sprint · Space jump · E interact · Esc free cursor (click to relock)",
                _hudStyle);

            // Focused-object prompt
            if (_focus != null)
            {
                string text = $"[{interactKey}] {_focus.Prompt}  —  {_focus.name}";
                Vector2 size = _hudStyle.CalcSize(new GUIContent(text));
                GUI.Label(new Rect(cx - size.x * 0.5f, cy + 22, size.x + 12, size.y + 8), text, _hudStyle);
            }
        }

        private void EnsureStyle()
        {
            if (_hudStyle != null) return;
            _hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                normal = { textColor = Color.white }
            };
        }
    }
}
