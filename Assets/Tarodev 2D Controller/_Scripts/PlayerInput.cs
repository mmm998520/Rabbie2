using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TarodevController {
    public class PlayerInput : MonoBehaviour {
        public FrameInput FrameInput { get; private set; }

        private void Update() {
            FrameInput = Gather();
        }

#if (ENABLE_INPUT_SYSTEM)
        private PlayerInputActions _actions;
        private InputAction _move, _jump, _dash, _attack, _fly;

        private void Awake() {
            _actions = new PlayerInputActions();
            _move = _actions.Player.Move;
            _jump = _actions.Player.Jump;
            _dash = _actions.Player.Dash;
            _attack = _actions.Player.Attack;
            _fly = _actions.Player.Fly;
        }

        private void OnEnable() {
            _actions.Enable();
        }

        private void OnDisable() {
            _actions.Disable();
        }

        private FrameInput Gather() {
            return new FrameInput {
                JumpDown = _jump.triggered && _jump.ReadValue<float>() > 0,
                JumpHeld = !_jump.triggered && _jump.ReadValue<float>() > 0f,
                DashDown = _dash.triggered && _dash.ReadValue<float>() > 0,
                AttackDown = _attack.triggered && _attack.ReadValue<float>() > 0,
                FlyDown = _fly.triggered && _fly.ReadValue<float>() > 0,
                FlyHeld = !_fly.triggered && _fly.ReadValue<float>() > 0,
                FlyUp = _fly.triggered && _fly.ReadValue<float>() == 0,
                Move = _move.ReadValue<Vector2>()
            };
        }

#elif (ENABLE_LEGACY_INPUT_MANAGER)
        private FrameInput Gather() {
            return new FrameInput {
                JumpDown = Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.C),
                JumpHeld = Input.GetButton("Jump") || Input.GetKey(KeyCode.C),
                DashDown = Input.GetKeyDown(KeyCode.X),
                AttackDown = Input.GetKeyDown(KeyCode.Z),
                Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
            };
        }
#endif
    }

    public struct FrameInput {
        public Vector2 Move;
        public bool JumpDown;
        public bool JumpHeld;
        public bool DashDown;
        public bool AttackDown;
        public bool FlyDown;
        public bool FlyHeld;
        public bool FlyUp;
    }
}