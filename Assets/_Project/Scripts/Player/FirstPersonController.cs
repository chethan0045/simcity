using UnityEngine;

namespace Simcity.Player
{
    /// <summary>
    /// First-person movement + mouse look driven by a CharacterController.
    ///
    /// Phase 0 deliberately uses Unity's legacy Input (Input.GetAxis / GetKey) so it
    /// runs with zero package setup. We'll migrate to the new Input System in a later
    /// phase for touch controls + rebinding (see GAME_DESIGN.md §11).
    ///
    /// NOTE: requires Project Settings > Player > Active Input Handling = "Both" or
    /// "Input Manager (Old)". If it's set to "Input System Package (New)" only, the
    /// legacy Input calls throw at runtime.
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

        /// <summary>The camera transform this controller pitches up/down.</summary>
        public Transform CameraPivot { get; private set; }

        private CharacterController _cc;
        private float _pitch;
        private float _verticalVelocity;

        private void Awake()
        {
            Instance = this;
            _cc = GetComponent<CharacterController>();
        }

        public void SetCamera(Transform cam) => CameraPivot = cam;

        private void Start() => LockCursor(true);

        private void Update()
        {
            HandleLook();
            HandleMove();

            // Esc releases the cursor so you can click out of Play mode; click locks again.
            if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
            else if (Cursor.lockState != CursorLockMode.Locked && Input.GetMouseButtonDown(0)) LockCursor(true);
        }

        private void HandleLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked || CameraPivot == null) return;

            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up, mouseX);                              // yaw the body
            _pitch = Mathf.Clamp(_pitch - mouseY, minPitch, maxPitch);
            CameraPivot.localEulerAngles = new Vector3(_pitch, 0f, 0f);        // pitch the camera
        }

        private void HandleMove()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 move = Vector3.ClampMagnitude(transform.right * h + transform.forward * v, 1f);
            float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

            if (_cc.isGrounded)
            {
                _verticalVelocity = -1f; // small downward force keeps us snapped to the ground
                if (Input.GetButtonDown("Jump"))
                    _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            _verticalVelocity += gravity * Time.deltaTime;

            Vector3 velocity = move * speed + Vector3.up * _verticalVelocity;
            _cc.Move(velocity * Time.deltaTime);
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
