using UnityEngine;
using Simcity.Common;

namespace Simcity.Player
{
    /// <summary>
    /// First-person movement + mouse look driven by a CharacterController. Input comes from
    /// the keyboard/mouse on desktop, or from <see cref="MobileControls"/> on touch devices —
    /// the controller doesn't care which. Phase 8 adds game feel: subtle head-bob while walking
    /// and a small FOV kick when sprinting.
    ///
    /// Still uses Unity's legacy Input (so it runs with zero package setup) — requires Project
    /// Settings > Player > Active Input Handling = "Both" or "Input Manager (Old)".
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        public static FirstPersonController Instance { get; private set; }

        [Header("Movement")]
        public float walkSpeed = 4f;
        public float sprintSpeed = 7f;
        public float jumpHeight = 1.1f;
        public float gravity = -20f;

        [Header("Look")]
        public float mouseSensitivity = 2f;
        public float minPitch = -85f;
        public float maxPitch = 85f;

        [Header("Game feel")]
        public float headBobAmplitude = 0.045f;
        public float headBobFrequency = 9f;
        public float sprintFovAdd = 8f;
        public float fovLerpSpeed = 8f;

        /// <summary>The camera transform this controller pitches up/down.</summary>
        public Transform CameraPivot { get; private set; }

        private CharacterController _cc;
        private float _pitch;
        private float _verticalVelocity;

        private Camera _cam;
        private float _baseFov;
        private Vector3 _camBaseLocalPos;
        private float _bobTimer;
        private float _planarSpeed;
        private bool _isSprinting;

        private void Awake()
        {
            Instance = this;
            _cc = GetComponent<CharacterController>();
        }

        public void SetCamera(Transform cam)
        {
            CameraPivot = cam;
            _cam = cam != null ? cam.GetComponent<Camera>() : null;
            if (_cam != null) _baseFov = _cam.fieldOfView;
            if (cam != null) _camBaseLocalPos = cam.localPosition;
        }

        private void Start()
        {
            mouseSensitivity = QualityManager.MouseSensitivity;
            if (!MobileControls.Active) LockCursor(true);
        }

        private void Update()
        {
            HandleLook();
            HandleMove();
            HandleGameFeel();

            // Desktop only: Esc releases the cursor; click locks it again.
            if (!MobileControls.Active)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
                else if (Cursor.lockState != CursorLockMode.Locked && Input.GetMouseButtonDown(0)) LockCursor(true);
            }
        }

        private void HandleLook()
        {
            if (CameraPivot == null) return;

            Vector2 look;
            if (MobileControls.Active)
            {
                look = MobileControls.Look * 0.1f; // touch delta is in pixels — scale down
            }
            else
            {
                if (Cursor.lockState != CursorLockMode.Locked) return;
                look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            }

            transform.Rotate(Vector3.up, look.x * mouseSensitivity);                  // yaw the body
            _pitch = Mathf.Clamp(_pitch - look.y * mouseSensitivity, minPitch, maxPitch);
            CameraPivot.localEulerAngles = new Vector3(_pitch, 0f, 0f);               // pitch the camera
        }

        private void HandleMove()
        {
            float h, v;
            bool jump;
            if (MobileControls.Active)
            {
                var m = MobileControls.Move;
                h = m.x; v = m.y;
                _isSprinting = MobileControls.Sprint;
                jump = MobileControls.ConsumeJump();
            }
            else
            {
                h = Input.GetAxisRaw("Horizontal");
                v = Input.GetAxisRaw("Vertical");
                _isSprinting = Input.GetKey(KeyCode.LeftShift);
                jump = Input.GetButtonDown("Jump");
            }

            Vector3 move = Vector3.ClampMagnitude(transform.right * h + transform.forward * v, 1f);
            float speed = _isSprinting ? sprintSpeed : walkSpeed;
            _planarSpeed = move.magnitude * speed;

            if (_cc.isGrounded)
            {
                _verticalVelocity = -1f; // small downward force keeps us snapped to the ground
                if (jump) _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = move * speed + Vector3.up * _verticalVelocity;
            _cc.Move(velocity * Time.deltaTime);
        }

        // Head-bob while walking + a small FOV push when sprinting. Pure camera polish.
        private void HandleGameFeel()
        {
            if (CameraPivot == null) return;

            bool moving = _cc.isGrounded && _planarSpeed > 0.1f;
            bool sprinting = _isSprinting && moving;

            if (_cam != null)
            {
                float targetFov = _baseFov + (sprinting ? sprintFovAdd : 0f);
                _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFov, Time.deltaTime * fovLerpSpeed);
            }

            if (moving)
            {
                _bobTimer += Time.deltaTime * headBobFrequency * (sprinting ? 1.4f : 1f);
                float bob = Mathf.Sin(_bobTimer) * headBobAmplitude * (sprinting ? 1.3f : 1f);
                CameraPivot.localPosition = _camBaseLocalPos + new Vector3(0f, bob, 0f);
            }
            else
            {
                _bobTimer = 0f;
                CameraPivot.localPosition = Vector3.Lerp(CameraPivot.localPosition, _camBaseLocalPos, Time.deltaTime * 10f);
            }
        }

        private void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
