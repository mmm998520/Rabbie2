// ReSharper disable ClassWithVirtualMembersNeverInherited.Global

using System;
using System.Collections.Generic;
using UnityEngine;

namespace TarodevController {
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IPlayerController {
        [SerializeField] public static LayerMask playerLayer;

        [SerializeField] private ScriptableStats _stats;

        private FrameInput _frameInput;
        private Rigidbody2D _rb;
        private CapsuleCollider2D[] _cols; // Standing and crouching colliders
        private CapsuleCollider2D _col; // Current collider
        private PlayerInput _input;

        private Vector2 _speed;
        private bool _jumpToConsume;
        private bool _endedJumpEarly;
        private int _fixedFrame;
        private bool _coyoteUsed;
        private bool _coyoteUsable;
        private bool _doubleJumpUsable;
        private bool _bufferedJumpUsable;
        private bool _crouching;
        private bool _grounded;
        private Vector2 _groundNormal;
        private int _frameLeftGrounded = int.MinValue;
        private int _lastJumpPressed = int.MinValue;
        private int _frameLastAttacked = int.MinValue;
        private bool _attackToConsume;
        private readonly RaycastHit2D[] _allGroundColliderHits = new RaycastHit2D[2]; // This includes triggers
        private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[2];
        private readonly RaycastHit2D[] _ceilingHits = new RaycastHit2D[2];
        private readonly Collider2D[] _crouchHits = new Collider2D[5];
        private readonly Collider2D[] _wallHits = new Collider2D[5];
        private readonly Collider2D[] _ladderHits = new Collider2D[1];
        private int _groundHitCount;
        private Vector2 _currentExternalVelocity;
        private float dashInputFrame;
        private bool _dashToConsume;
        private bool _canDash;
        private Vector2 _dashVel;
        enum DashState
        {
            none,
            dashing,
            sliding
        }
        private DashState dashState;
        private int _startedDashing;
        private Bounds _standingColliderBounds;
        private int _frameStartedCrouching;
        private bool _isOnWall;
        private int _wallHitCount;
        private float _currentWallJumpMoveMultiplier;
        private int _wallDir;
        private int _ladderHitCount;
        public static int canFlyFrames;
        public bool isFlying;

        #region External

        public Vector2 Speed => _speed;
        public bool Crouching => _crouching;
        public bool ClimbingLadder => _onLadder;
        public Vector2 GroundNormal => _groundNormal;
        public ScriptableStats PlayerStats => _stats;
        public int WallDirection => _wallDir;
        public Vector2 Input => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action<bool> WallGrabChanged;
        public event Action<bool, Vector2> DashingChanged;
        public event Action<bool> Jumped;
        public event Action DoubleJumped;
        public event Action Attacked;

        public virtual void ApplyVelocity(Vector2 vel, PlayerForce forceType) {
            if (forceType == PlayerForce.Burst) _speed += vel;
            else _currentExternalVelocity += vel;
        }

        #endregion

        protected virtual void Awake() {
            playerLayer = 1 << gameObject.layer;

            Physics2D.queriesStartInColliders = false;

            _rb = GetComponent<Rigidbody2D>();
            _cols = GetComponents<CapsuleCollider2D>();
            _input = GetComponent<PlayerInput>();

            // Colliders cannot be check whilst disabled. Let's cache it instead
            _standingColliderBounds = _cols[0].bounds;
            _standingColliderBounds.center = _cols[0].offset;

            SetCrouching(false);
        }

        protected virtual void Update() {
            GatherInput();
        }

        protected virtual void GatherInput() {
            _frameInput = _input.FrameInput;

            if (_frameInput.JumpDown) {
                _jumpToConsume = true;
                _lastJumpPressed = _fixedFrame;
            }

            if (_frameInput.DashDown) _dashToConsume = true;
            if (_frameInput.AttackDown) _attackToConsume = true;
        }

        protected virtual void FixedUpdate() {
            _fixedFrame++;
            _currentExternalVelocity = Vector2.MoveTowards(_currentExternalVelocity, Vector2.zero, _stats.ExternalVelocityDecay * Time.fixedDeltaTime);

            CheckCollisions();

            if(canFlyFrames > 3 && (_frameInput.FlyDown || _frameInput.FlyHeld))
            {
                HandleAttacking();
                HandleFlyHorizontal();
                HandleFlyVertical();
                HandleDash();
            }
            else
            {
                HandleLadders();
                HandleAttacking();
                HandleCrouching();
                HandleWalkHorizontal();
                HandleWalls();
                HandleJump();
                HandleDash();
                HandleFall();
            }

            ApplyVelocity();
        }

        #region Collisions

        protected virtual void CheckCollisions() {
            // Ground & Ceiling
            var offset = (Vector2)transform.position + _col.offset;

            var groundHitCount =
                Physics2D.CapsuleCastNonAlloc(offset, _col.size, _col.direction, 0, Vector2.down, _allGroundColliderHits, _stats.GrounderDistance, ~_stats.PlayerLayer);

            // Remove triggers from detection. Easily handled by lambda, but trying to keep update GC free. 
            // Wish we had QueryTriggerInteraction in 2D...
            _groundHitCount = 0;
            for (int i = 0; i < groundHitCount; i++) {
                if (!_allGroundColliderHits[i].collider.isTrigger) {
                    _groundHits[i] = _allGroundColliderHits[i];
                    _groundHitCount++;
                }
            }

            var ceilingHits = Physics2D.CapsuleCastNonAlloc(offset, _col.size, _col.direction, 0, Vector2.up, _ceilingHits, _stats.GrounderDistance, ~_stats.PlayerLayer);
            if (_speed.y > 0) {
                for (int i = 0; i < ceilingHits; i++) {
                    if (!_ceilingHits[i].collider.isTrigger) _speed.y = 0;
                }
            }

            if (!_grounded && _groundHitCount > 0) {
                _grounded = true;
                _coyoteUsable = true;
                _doubleJumpUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                //_canDash = true;
                GroundedChanged?.Invoke(true, Mathf.Abs(_speed.y));
            }
            else if (_grounded && _groundHitCount == 0) {
                _grounded = false;
                _frameLeftGrounded = _fixedFrame;
                GroundedChanged?.Invoke(false, 0);
            }

            // Walls
            var bounds = GetWallDetectionBounds();
            _wallHitCount = Physics2D.OverlapBoxNonAlloc(bounds.center, bounds.size, 0, _wallHits, _stats.ClimbableLayer);

            _ladderHitCount = Physics2D.OverlapBoxNonAlloc(bounds.center, bounds.size, 0, _ladderHits, _stats.LadderLayer);
        }

        #endregion

        #region Ladders

        private bool _onLadder;
        private Vector2 _ladderSnapVel;
        private int _frameLeftLadder = int.MinValue;

        protected virtual void HandleLadders() {
            if (!_onLadder && _ladderHitCount > 0 && _frameInput.Move.y != 0 && _fixedFrame > _frameLeftLadder + _stats.LadderCooldownFrames) ToggleClimbingLadders(true);
            else if (_onLadder && _ladderHitCount == 0) ToggleClimbingLadders(false);

            // Snap to center of ladder
            if (_onLadder && _frameInput.Move.x == 0 && _stats.SnapToLadders) {
                var pos = _rb.position;
                _rb.position = Vector2.SmoothDamp(pos, new Vector2(_ladderHits[0].transform.position.x, pos.y), ref _ladderSnapVel, _stats.LadderSnapSpeed);
            }
        }

        private void ToggleClimbingLadders(bool on) {
            if (on) {
                _onLadder = true;
                _speed = Vector2.zero;
            }
            else {
                if (!_onLadder) return;
                _frameLeftLadder = _fixedFrame;
                _onLadder = false;
            }
        }

        #endregion


        #region Attacking

        protected virtual void HandleAttacking() {
            if (!_attackToConsume) return;

            if (_frameLastAttacked + _stats.AttackFrameCooldown < _fixedFrame) {
                _frameLastAttacked = _fixedFrame;
                Attacked?.Invoke();
            }

            _attackToConsume = false;
        }

        #endregion

        #region Crouching

        protected virtual void HandleCrouching() {
            if (!_stats.AllowCrouch) return;
            var crouchCheck = _frameInput.Move.y <= _stats.CrouchInputThreshold;
            if (crouchCheck != _crouching) SetCrouching(crouchCheck);
        }

        protected virtual void SetCrouching(bool active) {
            // Prevent standing into colliders
            if (_crouching) {
                var pos = _standingColliderBounds.center + transform.position;
                pos.y += _standingColliderBounds.extents.y;
                var size = new Vector3(_standingColliderBounds.size.x, _stats.CrouchBufferCheck);
                var hits = Physics2D.OverlapBoxNonAlloc(pos, size, 0, _crouchHits);

                if (hits > 0) return;
            }

            _crouching = active;
            _col = _cols[active ? 1 : 0];
            _cols[0].enabled = !active;
            _cols[1].enabled = active;

            if (_crouching) _frameStartedCrouching = _fixedFrame;
        }

        #endregion

        #region WalkHorizontal

        protected virtual void HandleWalkHorizontal() {
            if (_frameInput.Move.x != 0) {
                if (_crouching && _grounded) {
                    var crouchPoint = Mathf.InverseLerp(0, _stats.CrouchSlowdownFrames, _fixedFrame - _frameStartedCrouching);
                    var penaltySpeed = _stats.MaxSpeed * Mathf.Lerp(1, _stats.CrouchSpeedPenalty, crouchPoint);

                    _speed.x = Mathf.MoveTowards(_speed.x, penaltySpeed * _frameInput.Move.x, _stats.Deceleration * Time.fixedDeltaTime);
                }
                else {
                    // Prevent useless horizontal wall buildup
                    if (_rb.velocity.x == 0) {
                        if (_frameInput.Move.x < 0 && _speed.x > 0) _speed.x = 0;
                        else if (_frameInput.Move.x > 0 && _speed.x < 0) _speed.x = 0;
                    }

                    var inputX = _frameInput.Move.x * _currentWallJumpMoveMultiplier * (_onLadder ? _stats.LadderShimmySpeedMultiplier : 1);
                    if (_stats.AllowCreeping) _speed.x = Mathf.MoveTowards(_speed.x, _stats.MaxSpeed * inputX, _stats.Acceleration * Time.fixedDeltaTime);
                    else _speed.x += inputX * _stats.Acceleration * Time.fixedDeltaTime;
                }
            }
            else {
                _speed.x = Mathf.MoveTowards(_speed.x, 0, _stats.Deceleration * (_grounded ? 1 : _stats.AirDecelerationPenalty) * Time.fixedDeltaTime);
            }

            _speed.x = Mathf.Clamp(_speed.x, -_stats.MaxSpeed, _stats.MaxSpeed);
        }

        #endregion

        #region FlyHorizontal

        protected virtual void HandleFlyHorizontal()
        {
            if (_frameInput.Move.x != 0)
            {
                // Prevent useless horizontal wall buildup
                if (_rb.velocity.x == 0)
                {
                    if (_frameInput.Move.x < 0 && _speed.x > 0) _speed.x = 0;
                    else if (_frameInput.Move.x > 0 && _speed.x < 0) _speed.x = 0;
                }

                var inputX = _frameInput.Move.x * _currentWallJumpMoveMultiplier * (_onLadder ? _stats.LadderShimmySpeedMultiplier : 1);
                if (_stats.AllowCreeping) _speed.x = Mathf.MoveTowards(_speed.x, _stats.FlyMaxSpeed * inputX, _stats.FlyAcceleration * Time.fixedDeltaTime);
                else _speed.x += inputX * _stats.FlyAcceleration * Time.fixedDeltaTime;
            }
            else
            {
                _speed.x = Mathf.MoveTowards(_speed.x, 0, _stats.FlyDeceleration * Time.fixedDeltaTime);
            }

            _speed.x = Mathf.Clamp(_speed.x, -_stats.FlyMaxSpeed, _stats.FlyMaxSpeed);
        }
        #endregion

        #region FlyVertical

        protected virtual void HandleFlyVertical()
        {
            if (_frameInput.Move.y != 0)
            {
                // Prevent useless horizontal wall buildup
                if (_rb.velocity.y == 0)
                {
                    if (_frameInput.Move.y < 0 && _speed.y > 0) _speed.y = 0;
                    else if (_frameInput.Move.y > 0 && _speed.y < 0) _speed.y = 0;
                }

                var inputY = _frameInput.Move.y * _currentWallJumpMoveMultiplier * (_onLadder ? _stats.LadderShimmySpeedMultiplier : 1);
                if (_stats.AllowCreeping) _speed.y = Mathf.MoveTowards(_speed.y, _stats.FlyMaxSpeed * inputY, _stats.FlyAcceleration * Time.fixedDeltaTime);
                else _speed.y += inputY * _stats.FlyAcceleration * Time.fixedDeltaTime;
            }
            else
            {
                _speed.y = Mathf.MoveTowards(_speed.y, 0, _stats.FlyDeceleration * Time.fixedDeltaTime);
            }

            _speed.y = Mathf.Clamp(_speed.y, -_stats.FlyMaxSpeed, _stats.FlyMaxSpeed);
        }
        #endregion

        #region Walls

        protected virtual void HandleWalls() {
            _currentWallJumpMoveMultiplier = Mathf.MoveTowards(_currentWallJumpMoveMultiplier, 1, 1f / _stats.WallJumpInputLossFrames);

            if (!_stats.AllowWalls) return;

            // May need to prioritize the newest wall here... But who is going to make a climbable wall that tight?
            _wallDir = _wallHitCount > 0 ? (int)Mathf.Sign(_wallHits[0].transform.position.x - transform.position.x) : 0;

            if (_isOnWall && !IsPushing()) SetOnWall(false);
            else if (!_isOnWall && IsPushing()) SetOnWall(true);

            bool IsPushing() {
                if (_wallDir == 0) return false;
                if (_stats.RequireInputPush) return Mathf.Approximately(_frameInput.Move.x, _wallDir);
                return true;
            }
        }

        private void SetOnWall(bool on) {
            _isOnWall = on;
            if (!on) _endedJumpEarly = false; // Not flicking this affects future wall jumps
            WallGrabChanged?.Invoke(on);
        }

        private Bounds GetWallDetectionBounds() {
            var wallHitOffset = transform.position + _standingColliderBounds.center;
            return new Bounds(wallHitOffset, _stats.WallDetectorSize);
        }

        #endregion

        #region Jump

        private bool CanUseCoyote => _coyoteUsable && !_grounded && _frameLeftGrounded + _stats.CoyoteFrames > _fixedFrame;
        private bool HasBufferedJump => (_grounded || _isOnWall) && _bufferedJumpUsable && _lastJumpPressed + _stats.JumpBufferFrames > _fixedFrame;
        private bool CanDoubleJump => _stats.AllowDoubleJump && _doubleJumpUsable && !_coyoteUsable;

        protected virtual void HandleJump() {
            // Wall jump
            if (!_grounded && ((_isOnWall && _jumpToConsume) || HasBufferedJump)) {
                var power = _stats.WallJumpPower;
                power.x *= -_wallDir;
                _speed = power;
                _jumpToConsume = false;
                _currentWallJumpMoveMultiplier = 0;
                _bufferedJumpUsable = false;
                SetOnWall(false);
                Jumped?.Invoke(true);
            }

            // Double jump
            if (_jumpToConsume && CanDoubleJump) {
                _speed.y = _stats.JumpPower;
                _doubleJumpUsable = false;
                _endedJumpEarly = false;
                _jumpToConsume = false;
                DoubleJumped?.Invoke();
            }

            // Standard jump
            if ((_jumpToConsume && (CanUseCoyote || _onLadder)) || HasBufferedJump) {
                if (dashState == DashState.none)
                {
                    _coyoteUsable = false;
                    _bufferedJumpUsable = false;
                    _speed.y = _stats.JumpPower;
                    ToggleClimbingLadders(false);
                    Jumped?.Invoke(false);
                }
            }

            // Early end detection
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && _rb.velocity.y > 0) _endedJumpEarly = true;
            if (dashState == DashState.sliding)
            {
                _endedJumpEarly = true;
            }
        }

        #endregion

        #region Dash

        protected virtual void HandleDash() {
            if (!_stats.AllowDash) return;

            if (_dashToConsume)
            {
                dashInputFrame = _fixedFrame;
            }

            if ((_fixedFrame - dashInputFrame <= _stats.DashBufferFrames) && _canDash && !_crouching)
            {
                var dir = new Vector2(_frameInput.Move.x, _grounded && _frameInput.Move.y < 0 ? 0 : _frameInput.Move.y).normalized;
                if (dir == Vector2.zero)
                {
                    _dashToConsume = false;
                    return;
                }

                _dashVel = dir * _stats.DashVelocity;
                dashState = DashState.dashing;
                DashingChanged?.Invoke(true, dir);
                _canDash = false;
                _startedDashing = _fixedFrame;
            }

            if (dashState == DashState.dashing)
            {
                _speed = _dashVel;
                // Cancel when the time is out or we've reached our max safety distance
                if (_startedDashing + _stats.DashDurationFrames < _fixedFrame)
                {
                    dashState = DashState.sliding;
                    //DashingChanged?.Invoke(false, Vector2.zero);
                }
            }
            else if (dashState == DashState.sliding && _stats.DashSlidingFrames != 0)
            {
                _speed.x = _dashVel.x * _stats.DashSlidingRate + _speed.x * (1 - _stats.DashSlidingRate);
                _speed.y = _dashVel.y * _stats.DashSlidingRate + _speed.y * (1 - _stats.DashSlidingRate);
                if (_startedDashing + _stats.DashDurationFrames +_stats.DashSlidingFrames < _fixedFrame)
                {
                    dashState = DashState.none;
                    DashingChanged?.Invoke(false, Vector2.zero);
                }
            }

            if (_fixedFrame - _startedDashing > _stats.DashCDFrames)
            {
                _canDash = true;
            }
            _dashToConsume = false;

            #region 原作者的Dash
            if (false)
            {
                if (_dashToConsume && _canDash && !_crouching)
                {
                    var dir = new Vector2(_frameInput.Move.x, _grounded && _frameInput.Move.y < 0 ? 0 : _frameInput.Move.y).normalized;
                    if (dir == Vector2.zero)
                    {
                        _dashToConsume = false;
                        return;
                    }

                    _dashVel = dir * _stats.DashVelocity;
                    dashState = DashState.dashing;
                    DashingChanged?.Invoke(true, dir);
                    _canDash = false;
                    _startedDashing = _fixedFrame;

                    // Strip external buildup
                    _currentExternalVelocity = Vector2.zero;
                }

                if (dashState == DashState.dashing)
                {
                    _speed = _dashVel;
                    // Cancel when the time is out or we've reached our max safety distance
                    if (_startedDashing + _stats.DashDurationFrames < _fixedFrame)
                    {
                        dashState = DashState.none;
                        DashingChanged?.Invoke(false, Vector2.zero);
                        if (_speed.y > 0) _speed.y = 0;
                        _speed.x *= _stats.DashEndHorizontalMultiplier;
                        if (_grounded) _canDash = true;
                    }
                }

                _dashToConsume = false;
            }
            #endregion
        }

        #endregion

        #region Falling

        protected virtual void HandleFall() {
            if (dashState == DashState.dashing) return;

            if (_onLadder) {
                var multiplier = _frameInput.Move.y < 0 ? _stats.LadderSlideMultiplier : 1;
                _speed.y = _frameInput.Move.y * _stats.LadderClimbSpeed * multiplier;

                return;
            }

            if (_grounded && _speed.y <= 0) {
                // Slopes
                _speed.y = _stats.GroundingForce;
                _groundNormal = Vector2.zero;

                // We use a raycast here as the groundHits from capsule cast act a bit weird.
                var hit = Physics2D.Raycast(transform.position, Vector2.down, _stats.GrounderDistance * 2, ~_stats.PlayerLayer);
                if (hit) {
                    _groundNormal = hit.normal;

                    var slopePerp = Vector2.Perpendicular(_groundNormal).normalized;
                    var slopeAngle = Vector2.Angle(_groundNormal, Vector2.up);

                    if (slopeAngle != 0) {
                        if (_speed.x == 0) {
                            _speed.y = 0; // Prevent slipping
                        }
                        else {
                            _speed.y = _speed.x * -slopePerp.y;
                            _speed.y += _stats.GroundingForce;
                        }
                    }
                }

                return;
            }

            if (_isOnWall) {
                _speed.y += -_stats.WallFallSpeed * Time.fixedDeltaTime;
                if (_speed.y < -_stats.MaxWallFallSpeed) {
                    _speed.y = -_stats.MaxWallFallSpeed;
                }

                if (_stats.CanClimbWalls && _frameInput.Move.y > 0) {
                    _speed.y = _stats.WallClimbSpeed;
                }
            }


            if (_isOnWall && _rb.velocity.y < 0) {
                _speed.y += -_stats.WallFallSpeed * Time.fixedDeltaTime;
                if (_speed.y < -_stats.MaxWallFallSpeed) {
                    _speed.y = -_stats.MaxWallFallSpeed;
                }
            }
            else {
                var fallSpeed = _endedJumpEarly && _speed.y > 0
                    ? -_stats.FallSpeed * (_stats.JumpEndEarlyGravityModifier * _currentWallJumpMoveMultiplier)
                    : -_stats.FallSpeed;

                _speed.y += fallSpeed * Time.fixedDeltaTime;
                if (_speed.y < -_stats.MaxFallSpeed) {
                    _speed.y = -_stats.MaxFallSpeed;
                }
            }
        }

        #endregion

        protected virtual void ApplyVelocity() {
            _rb.velocity = _speed + _currentExternalVelocity;
            _jumpToConsume = false;
        }

        private void OnDrawGizmos() {
            if (_stats.ShowWallDetection) {
                Gizmos.color = Color.white;
                var bounds = GetWallDetectionBounds();
                Gizmos.DrawWireCube(bounds.center, bounds.size);
            }
        }
    }

    public interface IPlayerController {
        Vector2 Input { get; }
        Vector2 Speed { get; }
        bool Crouching { get; }
        bool ClimbingLadder { get; }
        Vector2 GroundNormal { get; }
        ScriptableStats PlayerStats { get; }
        int WallDirection { get; }

        event Action<bool, float> GroundedChanged; // Grounded - Impact force
        event Action<bool> WallGrabChanged;
        event Action<bool, Vector2> DashingChanged; // Dashing - Dir
        event Action<bool> Jumped; // Is wall jump
        event Action DoubleJumped;
        event Action Attacked;

        void ApplyVelocity(Vector2 vel, PlayerForce forceType);
    }

    public enum PlayerForce {
        /// <summary>
        /// Added directly to the players movement speed, to be controlled by the standard deceleration
        /// </summary>
        Burst,

        /// <summary>
        /// An additive force handled by the decay system
        /// </summary>
        Decay
    }
}