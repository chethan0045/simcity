using UnityEngine;

namespace Simcity.Player
{
    /// <summary>
    /// Touch input for phones/tablets: a left-thumb virtual joystick to move (push it far to
    /// sprint), drag anywhere on the right to look, and on-screen Jump / E buttons. Exposes the
    /// same intents the desktop path produces, so <see cref="FirstPersonController"/> and
    /// <see cref="Simcity.Interaction.PlayerInteractor"/> read it without caring about platform.
    /// Enabled automatically on touch platforms (or set <see cref="forceOn"/> to test in editor).
    /// A functional greybox control — visual/ergonomic polish comes with the UGUI art pass.
    /// </summary>
    public class MobileControls : MonoBehaviour
    {
        public static MobileControls Instance { get; private set; }
        public static bool Active => Instance != null && Instance.isActiveAndEnabled;

        public static Vector2 Move => Instance != null ? Instance._move : Vector2.zero;
        public static Vector2 Look => Instance != null ? Instance._look : Vector2.zero;
        public static bool Sprint => Instance != null && Instance._sprint;

        public static bool ConsumeJump()
        {
            if (Instance != null && Instance._jumpQueued) { Instance._jumpQueued = false; return true; }
            return false;
        }

        public static bool ConsumeInteract()
        {
            if (Instance != null && Instance._interactQueued) { Instance._interactQueued = false; return true; }
            return false;
        }

        [Tooltip("Force the touch UI on in the editor for testing.")]
        public bool forceOn;

        private Vector2 _move, _look;
        private bool _sprint, _jumpQueued, _interactQueued;

        private int _moveFinger = -1, _lookFinger = -1;
        private Vector2 _moveOrigin;
        private const float JoyRadius = 110f;

        private GUIStyle _btn;
        private static Texture2D _tex;

        private void Awake()
        {
            Instance = this;
            enabled = forceOn || Application.isMobilePlatform;
        }

        private void OnDestroy() { if (Instance == this) Instance = null; }

        private void Update()
        {
            _look = Vector2.zero;
            if (Input.touchCount == 0) { _move = Vector2.zero; _sprint = false; _moveFinger = _lookFinger = -1; return; }

            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                bool leftHalf = t.position.x < Screen.width * 0.5f;

                if (t.phase == TouchPhase.Began)
                {
                    if (leftHalf && _moveFinger == -1) { _moveFinger = t.fingerId; _moveOrigin = t.position; }
                    else if (!leftHalf && _lookFinger == -1) { _lookFinger = t.fingerId; }
                }

                if (t.fingerId == _moveFinger)
                {
                    _move = Vector2.ClampMagnitude((t.position - _moveOrigin) / JoyRadius, 1f);
                    _sprint = _move.magnitude > 0.85f;
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    { _moveFinger = -1; _move = Vector2.zero; _sprint = false; }
                }
                else if (t.fingerId == _lookFinger)
                {
                    if (t.phase == TouchPhase.Moved) _look += t.deltaPosition;
                    if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) _lookFinger = -1;
                }
            }
        }

        private void OnGUI()
        {
            if (_btn == null) _btn = new GUIStyle(GUI.skin.button) { fontSize = 18 };

            if (_moveFinger != -1)
            {
                DrawKnob(_moveOrigin, JoyRadius, new Color(1, 1, 1, 0.10f));
                DrawKnob(_moveOrigin + _move * JoyRadius, 34f, new Color(1, 1, 1, 0.28f));
            }

            float s = 84f, pad = 24f;
            if (GUI.RepeatButton(new Rect(Screen.width - s - pad, Screen.height - s * 2 - pad * 2, s, s), "Jump", _btn))
                _jumpQueued = true;
            if (GUI.Button(new Rect(Screen.width - s - pad, Screen.height - s - pad, s, s), "E", _btn))
                _interactQueued = true;
        }

        // Touch coords are bottom-left origin; OnGUI is top-left — flip Y.
        private static void DrawKnob(Vector2 centerScreen, float radius, Color color)
        {
            if (_tex == null) { _tex = new Texture2D(1, 1); _tex.SetPixel(0, 0, Color.white); _tex.Apply(); }
            var prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(centerScreen.x - radius, (Screen.height - centerScreen.y) - radius, radius * 2, radius * 2), _tex);
            GUI.color = prev;
        }
    }
}
