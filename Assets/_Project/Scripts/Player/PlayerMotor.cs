using UnityEngine;

// Nơi duy nhất gọi CharacterController.Move cho An.
[RequireComponent(typeof(PlayerActionState))]
[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class PlayerMotor : MonoBehaviour
{
    private const float LiftRequestTimeout = 0.1f;
    private const float TerminalVelocity = -53f;

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private LanternController lantern;
    [SerializeField] private PlayerVitals vitals;

    [Header("Speed")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 5.5f;
    [SerializeField] private float focusSpeed = 2f;
    [SerializeField] private float sprintStaminaPerSecond = 8f;
    [SerializeField, Range(0f, 0.3f)] private float rotationSmoothTime = 0.12f;
    [SerializeField] private float speedChangeRate = 10f;

    [Header("Jump and gravity")]
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpTimeout = 0.2f;
    [SerializeField] private float fallTimeout = 0.15f;

    [Header("Ground check")]
    [SerializeField] private float groundedRadius = 0.28f;
    [SerializeField] private LayerMask groundLayers = 9;

    private CharacterController controller;
    private Transform cameraTransform;
    private PlayerCameraController cameraController;
    private InputReader input;

    private PlayerActionState actions;
    private CombatController combat;
    private float jumpQueuedUntil = float.NegativeInfinity;
    private float lastGrounded = float.NegativeInfinity;
    private float nextJump;
    private int inputRevision;
    private bool sprintExhausted;
    private float speed;
    private float animationBlend;
    private float targetRotation;
    private float rotationVelocity;
    private float verticalVelocity;
    // Vận tốc dọc trung bình trong frame: giữ đúng độ cao nhảy khi FPS thấp.
    private float appliedVerticalVelocity;
    [SerializeField, Min(0f)] private float jumpBuffer = 0.15f;
    [SerializeField, Min(0f)] private float coyoteTime = 0.12f;
    [SerializeField, Min(1f)] private float acceleration = 30f;
    [SerializeField, Min(1f)] private float deceleration = 45f;
    private float ignoreLiftUntil;
    private Vector3 measuredVelocity;
    private float fallTimeoutDelta;

    private float liftUpSpeed;
    private float liftTopY;
    private float lastLiftRequestTime = float.NegativeInfinity;
    private Vector3 externalVelocity;

    // Lộn (DodgeController) và thân thấp khi chui khe.
    private Vector3 dashVelocity;
    private float dashUntil = float.NegativeInfinity;
    private bool dashKeepsFacing;
    private float standingHeight;
    private Vector3 standingCenter;
    private bool isLow;

    [Header("Movement audio")]
    [SerializeField] private AudioSource movementAudio;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private AudioClip landingClip;
    private float lastFootstep = float.NegativeInfinity;
    private float lastLanding = float.NegativeInfinity;
    private int footstepIndex;
    private int animSpeed;
    private int animGrounded;
    private int animJump;
    private int animFreeFall;
    private int animMotionSpeed;

    public bool IsGrounded { get; private set; } = true;
    public bool IsHovering { get; private set; }
    public bool IsSprinting { get; private set; }
    public Vector3 Velocity => measuredVelocity;
    public bool IsDashing => Time.time < dashUntil;
    public bool IsLow => isLow;
    // Khóa mục tiêu: An luôn quay mặt về đây (hướng đi vẫn theo camera).
    public Transform FacingTarget { get; set; }
    // Hướng đi mong muốn theo input và camera (dùng để chọn hướng lộn).
    public Vector3 DesiredMoveDirection { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        actions = GetComponent<PlayerActionState>();
        combat = GetComponent<CombatController>();
        cameraController = GetComponent<PlayerCameraController>();
        if (animator == null) animator = GetComponent<Animator>();
        if (stamina == null) stamina = GetComponent<PlayerStamina>();
        if (lantern == null) lantern = GetComponent<LanternController>();
        if (vitals == null) vitals = GetComponent<PlayerVitals>();

        animSpeed = Animator.StringToHash("Speed");
        animGrounded = Animator.StringToHash("Grounded");
        animJump = Animator.StringToHash("Jump");
        animFreeFall = Animator.StringToHash("FreeFall");
        animMotionSpeed = Animator.StringToHash("MotionSpeed");

        standingHeight = controller.height;
        standingCenter = controller.center;

        fallTimeoutDelta = fallTimeout;
        targetRotation = transform.eulerAngles.y;
    }

    private void Start()
    {
        input = AppRoot.Instance != null ? AppRoot.Instance.Input : null;
        if (input == null || !input.IsReady)
        {
            Debug.LogError($"{name}: PlayerMotor không tìm thấy InputReader.", this);
            enabled = false;
            return;
        }

        Camera main = Camera.main;
        if (main == null)
        {
            Debug.LogError($"{name}: PlayerMotor cần một camera có tag MainCamera.", this);
            enabled = false;
            return;
        }
        cameraTransform = main.transform;
    }

    private void Update()
    {
        // Đã chết thì đứng yên chờ hồi sinh.
        if (Time.deltaTime <= 0f || (vitals != null && vitals.IsDead)) return;

        if (inputRevision != input.Revision || !input.GameplayActive)
        {
            jumpQueuedUntil = float.NegativeInfinity;
            inputRevision = input.Revision;
            speed = 0f;
        }
        GroundedCheck();
        JumpAndGravity();
        Move();

        externalVelocity = Vector3.zero;
    }

    // Gọi mỗi frame khi An đứng trong cột gió.
    public void RequestLift(float upSpeed, float hoverTopY)
    {
        if (Time.time < ignoreLiftUntil) return;
        liftUpSpeed = upSpeed;
        liftTopY = hoverTopY;
        lastLiftRequestTime = Time.time;
    }

    // Gọi mỗi frame để cộng vận tốc ngang từ môi trường (gió đẩy, xoáy nước).
    public void AddExternalVelocity(Vector3 horizontalVelocity)
    {
        horizontalVelocity.y = 0f;
        externalVelocity += horizontalVelocity;
    }

    public void Teleport(Vector3 position, Quaternion rotation)
    {
        actions.ResetTransient();
        var presentation = GetComponent<PlayerAnimator>();
        if (presentation != null) presentation.ResetPose();
        bool wasEnabled = controller.enabled;
        controller.height = standingHeight;
        controller.center = standingCenter;
        isLow = false;
        rotationVelocity = 0f;
        animationBlend = 0f;
        jumpQueuedUntil = lastGrounded = float.NegativeInfinity;
        nextJump = Time.time;
        IsSprinting = false;
        measuredVelocity = Vector3.zero;
        ignoreLiftUntil = 0f;
        Vector3 warpDelta=position-transform.position;
        controller.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        Unity.Cinemachine.CinemachineCore.OnTargetObjectWarped(transform,warpDelta);
        var followPivot=transform.Find("PlayerCameraRoot");
        if(followPivot)Unity.Cinemachine.CinemachineCore.OnTargetObjectWarped(followPivot,warpDelta);
        Physics.SyncTransforms();
        controller.enabled = wasEnabled;

        verticalVelocity = 0f;
        appliedVerticalVelocity = 0f;
        speed = 0f;
        dashUntil = float.NegativeInfinity;
        externalVelocity = Vector3.zero;
        targetRotation = rotation.eulerAngles.y;
        lastLiftRequestTime = float.NegativeInfinity;
        IsHovering = false;
        GroundedCheck();
    }

    // Lộn: vận tốc ngang cố định trong một khoảng; vẫn bị tường chặn vì đi qua CharacterController.Move.
    public void BeginDash(Vector3 horizontalVelocity, float duration, bool keepFacing = false)
    {
        horizontalVelocity.y = 0f;
        dashVelocity = horizontalVelocity;
        dashUntil = Time.time + Mathf.Max(0f, duration);
        dashKeepsFacing = keepFacing;
        if (!keepFacing && horizontalVelocity.sqrMagnitude > 0.0001f)
        {
            targetRotation = Quaternion.LookRotation(horizontalVelocity).eulerAngles.y;
            transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);
        }
    }

    public void CancelDash() => dashUntil = float.NegativeInfinity;

    // Hạ capsule còn nửa chiều cao (lộn chui khe thấp). Đứng dậy chỉ khi trên đầu trống.
    public bool SetLow(bool low)
    {
        if (low == isLow) return true;
        if (low)
        {
            controller.height = standingHeight * 0.5f;
            controller.center = new Vector3(standingCenter.x, standingCenter.y * 0.5f, standingCenter.z);
            isLow = true;
            return true;
        }

        Vector3 bottom = transform.position + Vector3.up * (controller.radius + 0.05f);
        Vector3 top = transform.position + Vector3.up * (standingHeight - controller.radius);
        if (Physics.CheckCapsule(bottom, top, controller.radius * 0.95f, groundLayers, QueryTriggerInteraction.Ignore))
            return false;

        controller.height = standingHeight;
        controller.center = standingCenter;
        isLow = false;
        return true;
    }

    private bool IsLifting => Time.time - lastLiftRequestTime <= LiftRequestTimeout;

    // Probe starts above the feet, then sweeps down; wall contact cannot ground An.
    private Vector3 GroundCheckPosition =>
        transform.position + Vector3.up * (groundedRadius + 0.08f);

    private void GroundedCheck()
    {
        IsGrounded = verticalVelocity <= 0f && Physics.SphereCast(
            GroundCheckPosition, groundedRadius * 0.9f, Vector3.down,
            out RaycastHit ground, 0.15f,
            groundLayers, QueryTriggerInteraction.Ignore) &&
            Vector3.Angle(ground.normal, Vector3.up) <= controller.slopeLimit;
        if (IsGrounded) lastGrounded = Time.time;
        if (animator != null) animator.SetBool(animGrounded, IsGrounded || IsHovering);
    }

    private void JumpAndGravity()
    {
        if (input.WasPressed(input.Jump) && actions.CanJump) jumpQueuedUntil = Time.time + jumpBuffer;
        bool jumpPressed = actions.CanJump && Time.time <= jumpQueuedUntil && Time.time >= nextJump;

        if (IsLifting)
        {
            // Cột gió: tiến dần tới vận tốc nâng, dừng lại ở đỉnh để lơ lửng.
            if (transform.position.y >= liftTopY - 0.3f)
            {
                verticalVelocity = 0f;
                IsHovering = true;
            }
            else
            {
                verticalVelocity = Mathf.MoveTowards(verticalVelocity, liftUpSpeed, 30f * Time.deltaTime);
                IsHovering = false;
            }

            if (IsHovering && jumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpQueuedUntil = float.NegativeInfinity;
                lastGrounded = float.NegativeInfinity;
                nextJump = Time.time + jumpTimeout;
                IsGrounded = false;
                lastLiftRequestTime = float.NegativeInfinity;
                ignoreLiftUntil = Time.time + 0.25f;
                IsHovering = false;
                if (animator != null) animator.SetBool(animJump, true);
            }
            appliedVerticalVelocity = verticalVelocity;
            return;
        }

        IsHovering = false;

        if (IsGrounded)
        {
            fallTimeoutDelta = fallTimeout;
            if (animator != null)
            {
                animator.SetBool(animJump, false);
                animator.SetBool(animFreeFall, false);
            }

            if (verticalVelocity < 0f) verticalVelocity = -2f;

            if (jumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpQueuedUntil = float.NegativeInfinity;
                lastGrounded = float.NegativeInfinity;
                nextJump = Time.time + jumpTimeout;
                IsGrounded = false;
                if (animator != null) animator.SetBool(animJump, true);
            }

        }
        else
        {
            if (jumpPressed && Time.time - lastGrounded <= coyoteTime)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpQueuedUntil = lastGrounded = float.NegativeInfinity;
                nextJump = Time.time + jumpTimeout;
                if (animator != null) animator.SetBool(animJump, true);
            }
            if (fallTimeoutDelta >= 0f) fallTimeoutDelta -= Time.deltaTime;
            else if (animator != null) animator.SetBool(animFreeFall, true);
        }

        float startVelocity = verticalVelocity;
        if (verticalVelocity > TerminalVelocity) verticalVelocity += gravity * Time.deltaTime;
        appliedVerticalVelocity = 0.5f * (startVelocity + verticalVelocity);
    }

    private void Move()
    {
        Vector2 move = actions.CanMove ? input.MoveValue : Vector2.zero;
        bool hasMove = move.sqrMagnitude > 0.0001f;
        DesiredMoveDirection = hasMove
            ? Quaternion.Euler(0f, ViewYaw, 0f) * new Vector3(move.x, 0f, move.y).normalized
            : Vector3.zero;

        if (IsDashing)
        {
            if (dashKeepsFacing) FaceTarget();
            MoveController((dashVelocity + Vector3.up * appliedVerticalVelocity) * Time.deltaTime);
            if (animator != null) animator.SetFloat(animSpeed, dashVelocity.magnitude);
            return;
        }
        // A committed strike must not keep sliding in the previous WASD direction.
        if (combat != null && combat.IsAttacking) speed = 0f;
        bool focusing = lantern != null && lantern.IsFocusing;

        // Chạy tốn thể lực; Focus đi chậm và không chạy.
        IsSprinting = false;
        float targetSpeed = focusing ? focusSpeed : walkSpeed;
        if (!input.IsHeld(input.Sprint)) sprintExhausted = false;
        if (hasMove && !focusing && !sprintExhausted && input.IsHeld(input.Sprint))
        {
            IsSprinting = stamina == null || stamina.TrySpend(sprintStaminaPerSecond * Time.deltaTime);
            if (IsSprinting) targetSpeed = sprintSpeed;
            else sprintExhausted = true;
        }
        if (!hasMove) targetSpeed = 0f;

        // Dùng tốc độ tự điều khiển, không lẫn vận tốc đẩy từ môi trường.
        float currentSpeed = speed;
        if (Mathf.Abs(currentSpeed - targetSpeed) > 0.1f)
        {
            speed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * (targetSpeed > currentSpeed ? acceleration : deceleration));
            speed = Mathf.Round(speed * 1000f) / 1000f;
        }
        else
        {
            speed = targetSpeed;
        }

        animationBlend = combat != null && combat.IsAttacking ? 0f :
            Mathf.Lerp(animationBlend, speed, 1f - Mathf.Exp(-speedChangeRate * Time.deltaTime));
        if (animationBlend < 0.01f) animationBlend = 0f;

        // Hướng đi theo camera, chuẩn hóa để đi chéo không nhanh hơn.
        if (hasMove)
        {
            Vector3 inputDirection = new Vector3(move.x, 0f, move.y).normalized;
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + ViewYaw;
            if (FacingTarget == null)
            {
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, Mathf.Min(rotationSmoothTime, 0.06f));
                transform.rotation = Quaternion.Euler(0f, rotation, 0f);
            }
        }
        if (combat == null || !combat.IsAttacking) FaceTarget();

        Vector3 direction = Quaternion.Euler(0f, targetRotation, 0f) * Vector3.forward;
        Vector3 motion = direction * speed + externalVelocity + Vector3.up * appliedVerticalVelocity;
        MoveController(motion * Time.deltaTime);

        if (animator != null)
        {
            animator.SetFloat(animSpeed, animationBlend);
            animator.SetFloat(animMotionSpeed, 1f);
        }
    }

    public Vector3 ReadMoveDirection()
    {
        Vector2 value = input != null ? input.MoveValue : Vector2.zero;
        return Quaternion.Euler(0f, ViewYaw, 0f) * new Vector3(value.x, 0f, value.y).normalized;
    }

    private float ViewYaw => cameraController != null ? cameraController.Yaw :
        (cameraTransform != null ? cameraTransform.eulerAngles.y : transform.eulerAngles.y);

    public Vector3 ReadAttackDirection()
    {
        Vector3 direction = FacingTarget != null ? FacingTarget.position - transform.position :
            Quaternion.Euler(0f, ViewYaw, 0f) * Vector3.forward;
        direction.y = 0f;
        return direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
    }

    public void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) return;
        targetRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        rotationVelocity = 0f;
        transform.rotation = Quaternion.Euler(0f, targetRotation, 0f);
    }

    private void MoveController(Vector3 displacement)
    {
        Vector3 before = transform.position;
        int steps = Mathf.Max(1, Mathf.CeilToInt(displacement.magnitude / 0.3f));
        CollisionFlags flags = CollisionFlags.None;
        for (int i = 0; i < steps; i++) flags |= controller.Move(displacement / steps);
        measuredVelocity = Time.deltaTime > 0f ? (transform.position - before) / Time.deltaTime : Vector3.zero;
        if ((flags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) verticalVelocity = 0f;
    }

    private void FaceTarget()
    {
        if (FacingTarget == null) return;
        Vector3 d = FacingTarget.position - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude < 0.01f) return;
        float yaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, yaw, ref rotationVelocity, Mathf.Min(rotationSmoothTime, 0.06f));
        transform.rotation = Quaternion.Euler(0f, rotation, 0f);
    }

    // Animation Events reuse the imported footsteps, routed through the SFX mixer.
    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (movementAudio == null || footstepClips == null || footstepClips.Length == 0 ||
            !actions.Active || !IsGrounded || IsDashing || speed < 0.2f ||
            Time.time - lastFootstep < 0.12f || animationEvent.animatorClipInfo.weight < 0.35f) return;
        lastFootstep = Time.time;
        movementAudio.PlayOneShot(footstepClips[footstepIndex++ % footstepClips.Length], 0.35f);
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (movementAudio == null || landingClip == null || !actions.Active ||
            Time.time - lastLanding < 0.2f || animationEvent.animatorClipInfo.weight < 0.35f) return;
        lastLanding = Time.time;
        movementAudio.PlayOneShot(landingClip, 0.4f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = IsGrounded ? new Color(0f, 1f, 0f, 0.35f) : new Color(1f, 0f, 0f, 0.35f);
        Gizmos.DrawSphere(GroundCheckPosition, groundedRadius);
    }
}
