using UnityEngine;
using UnityEngine.Audio;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    public interface IDodgeVitals
    {
        bool IsDead { get; }
        void SetDodgeInvulnerability(float startsAt, float endsAt);
        void ClearDodgeInvulnerability();
    }

    public interface IDodgeStamina
    {
        bool TrySpend(float amount);
    }

    public interface IPlayerAttackState
    {
        bool IsAttacking { get; }
    }

    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        public AudioMixerGroup SfxOutput;

        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        [Header("Dodge")]
        [Min(0.01f)] public float DodgeDuration = 0.45f;
        [Min(0f)] public float DodgeStaminaCost = 25f;
        [Min(0f)] public float DodgeCooldown = 0.65f;
        [Min(0f)] public float DodgeSpeed = 6f;
        [Min(0f)] public float DodgeInvulnerableStart = 0.10f;
        [Min(0f)] public float DodgeInvulnerableEnd = 0.30f;

        private IDodgeVitals _vitals;
        private IDodgeStamina _stamina;
        private IPlayerAttackState _combatController;
        private bool _isDodging;
        private bool _resetHorizontalVelocity;
        private Vector3 _dodgeDirection;
        private float _dodgeStartedAt;
        private float _dodgeEndsAt;
        private float _nextDodgeAllowedAt = float.NegativeInfinity;
#if ENABLE_INPUT_SYSTEM
        private InputAction _dodgeAction;
#endif

        public bool IsDodging => _isDodging;

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = -53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }


        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        private void Start()
        {
            if (CinemachineCameraTarget == null)
            {
                Debug.LogError($"{name}: Missing CinemachineCameraTarget.", this);
                enabled = false;
                return;
            }
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            _vitals = GetComponent<IDodgeVitals>();
            _stamina = GetComponent<IDodgeStamina>();
            _combatController = GetComponent<IPlayerAttackState>();

            if (_input == null || _mainCamera == null || CinemachineCameraTarget == null)
            {
                Debug.LogError(
                    $"{name}: Check StarterAssetsInputs, MainCamera and camera target.", this);
                enabled = false;
                return;
            }

            // Other scenes still use this Starter Assets motor without combat components.
            // They retain normal locomotion; dodge is available only where both are present.

#if ENABLE_INPUT_SYSTEM
            _dodgeAction = _playerInput.actions.FindAction("Dodge", false);
            if (_dodgeAction == null)
            {
                Debug.LogWarning($"{name}: Missing Dodge action; normal movement still works.", this);
            }
#endif

            _isDodging = false;
            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            if (!Application.isFocused || (_vitals != null && _vitals.IsDead) || !_controller.enabled)
            {
                CancelDodge();
                return;
            }

            // Time.time and dodge movement freeze while paused.
            if (Time.timeScale <= 0f) return;

#if ENABLE_INPUT_SYSTEM
            if (!_playerInput.isActiveAndEnabled || !_playerInput.inputIsActive ||
                _playerInput.currentActionMap == null ||
                !_playerInput.currentActionMap.enabled)
            {
                CancelDodge();
                return;
            }

            if (_dodgeAction != null && !_dodgeAction.enabled)
            {
                CancelDodge();
                return;
            }
#endif

            GroundedCheck();

#if ENABLE_INPUT_SYSTEM
            if (_dodgeAction != null && _dodgeAction.enabled &&
                _dodgeAction.WasPressedThisFrame())
            {
                TryStartDodge();
            }
#endif

            if (_isDodging || (_combatController != null && _combatController.IsAttacking))
                _input.jump = false;
            JumpAndGravity();

            // Exactly one movement branch runs per frame.
            if (_isDodging)
                MoveDodge();
            else
                Move();
        }

        private void LateUpdate()
        {
            if (_input != null && (_vitals == null || !_vitals.IsDead) &&
                Application.isFocused && Time.timeScale > 0f)
            {
                CameraRotation();
            }
        }

        private bool TryStartDodge()
        {
            if (_vitals == null || _stamina == null || _isDodging ||
                (_combatController != null && _combatController.IsAttacking) ||
                !Grounded || _verticalVelocity > 0f ||
                Time.time < _nextDodgeAllowedAt || _vitals.IsDead)
            {
                return false;
            }

            if (!_stamina.TrySpend(DodgeStaminaCost)) return false;

            Vector3 inputDirection = new Vector3(_input.move.x, 0f, _input.move.y);
            _dodgeDirection = inputDirection.sqrMagnitude > 0.0001f
                ? Quaternion.Euler(0f, _mainCamera.transform.eulerAngles.y, 0f) * inputDirection.normalized
                : transform.forward;

            _dodgeDirection.y = 0f;
            _dodgeDirection.Normalize();
            transform.rotation = Quaternion.LookRotation(_dodgeDirection, Vector3.up);
            _targetRotation = transform.eulerAngles.y;
            _rotationVelocity = 0f;

            _isDodging = true;
            _dodgeStartedAt = Time.time;
            _dodgeEndsAt = _dodgeStartedAt + Mathf.Max(0.01f, DodgeDuration);
            _nextDodgeAllowedAt = _dodgeStartedAt + Mathf.Max(DodgeCooldown, DodgeDuration);

            float invulnerableStart = Mathf.Clamp(DodgeInvulnerableStart, 0f, DodgeDuration);
            float invulnerableEnd = Mathf.Clamp(DodgeInvulnerableEnd, invulnerableStart, DodgeDuration);

            _vitals.SetDodgeInvulnerability(
                _dodgeStartedAt + invulnerableStart,
                _dodgeStartedAt + invulnerableEnd);

            _input.jump = false;
            _speed = 0f;
            return true;
        }

        private void MoveDodge()
        {
            // Integrate only the portion of this frame inside the dodge interval.
            // A long frame cannot extend dodge travel beyond DodgeDuration.
            float dodgeDelta = Mathf.Max(0f,
                Mathf.Min(Time.time, _dodgeEndsAt) -
                Mathf.Max(Time.time - Time.deltaTime, _dodgeStartedAt));

            Vector3 displacement = _dodgeDirection * DodgeSpeed * dodgeDelta;
            displacement.y = _verticalVelocity * Time.deltaTime;
            _controller.Move(displacement);

            if (_hasAnimator)
            {
                // No new animation parameters or roll clips are required yet.
                _animator.SetFloat(_animIDSpeed, 0f);
                _animator.SetFloat(_animIDMotionSpeed, 1f);
            }

            if (Time.time >= _dodgeEndsAt) CancelDodge();
        }

        private void CancelDodge()
        {
            if (_vitals != null) _vitals.ClearDodgeInvulnerability();

            if (!_isDodging) return;

            _isDodging = false;
            _resetHorizontalVelocity = true;
            _speed = 0f;
            _animationBlend = 0f;
            _rotationVelocity = 0f;
            _jumpTimeoutDelta = JumpTimeout;
            if (_input != null) _input.jump = false;
        }

        private void OnDisable()
        {
            CancelDodge();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = _input.lookFromTouch ? Time.deltaTime : 1.0f;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            if (_combatController != null && _combatController.IsAttacking)
            {
                _speed = 0f;
                _animationBlend = 0f;
                _rotationVelocity = 0f;
                _controller.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
                if (_hasAnimator)
                {
                    _animator.SetFloat(_animIDSpeed, 0f);
                    _animator.SetFloat(_animIDMotionSpeed, 1f);
                }
                return;
            }

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = _resetHorizontalVelocity
                ? 0f
                : new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
            _resetHorizontalVelocity = false;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                    Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (!_isDodging && _input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if above terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity > _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight <= 0.5f ||
                FootstepAudioClips == null || FootstepAudioClips.Length == 0) return;
            int index = Random.Range(0, FootstepAudioClips.Length);
            RoutedAudio.PlayClipAtPoint(FootstepAudioClips[index],
                transform.TransformPoint(_controller.center), FootstepAudioVolume, SfxOutput);
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
                RoutedAudio.PlayClipAtPoint(LandingAudioClip,
                    transform.TransformPoint(_controller.center), FootstepAudioVolume, SfxOutput);
        }
    }
}
