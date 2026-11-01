using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public class PlayerCombatController : MonoBehaviour, IPlayerAttackState
{
    [Header("References")]
    [SerializeField] private Transform attackOrigin;

    [Header("Attack")]
    [SerializeField, Min(0.01f)] private float damage = 20f;
    [SerializeField, Min(0.01f)] private float staminaCost = 10f;
    [SerializeField, Min(0.01f)] private float attackDuration = 0.65f;
    [SerializeField, Min(0f)] private float activeStart = 0.15f;
    [SerializeField, Min(0f)] private float activeEnd = 0.30f;

    [Header("Detection")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask visibilityMask;

    private PlayerVitals vitals;
    private PlayerStamina stamina;
    private ThirdPersonController motor;
    private CharacterController character;
    private PlayerInput playerInput;
    private InputAction attackAction;
    private InputAction dodgeAction;
    private float attackStartedAt;
    private float previousElapsed;
    private Vector3 attackDirection;
    private bool ready;
    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();

    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        vitals = GetComponent<PlayerVitals>();
        stamina = GetComponent<PlayerStamina>();
        motor = GetComponent<ThirdPersonController>();
        character = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        if (attackOrigin == null || vitals == null || stamina == null ||
            motor == null || character == null || playerInput == null)
        {
            Debug.LogError($"{name}: Thiếu tham chiếu cho combat.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (vitals != null) vitals.Died += CancelAttack;
    }

    private void Start()
    {
        if (!enabled) return;
        attackAction = playerInput.actions.FindAction("Attack", false);
        dodgeAction = playerInput.actions.FindAction("Dodge", false);
        ready = attackAction != null && staminaCost > 0f && damage > 0f &&
                activeStart >= 0f && activeEnd > activeStart && attackDuration >= activeEnd;
        if (!ready)
        {
            Debug.LogError($"{name}: Kiểm tra action Attack và thời gian đòn.", this);
            enabled = false;
        }
    }

    private void OnDisable()
    {
        if (vitals != null) vitals.Died -= CancelAttack;
        CancelAttack();
    }

    private void Update()
    {
        if (!ready) return;
        if (Time.timeScale == 0f) return;

        if (!Application.isFocused || vitals.IsDead || !motor.isActiveAndEnabled ||
            !character.enabled || !playerInput.inputIsActive ||
            playerInput.currentActionMap == null || !attackAction.enabled)
        {
            CancelAttack();
            return;
        }

        if (motor.IsDodging)
        {
            CancelAttack();
            return;
        }

        if (!IsAttacking)
        {
            // Dodge wins when both buttons are pressed in one frame.
            if (dodgeAction != null && dodgeAction.enabled && dodgeAction.WasPressedThisFrame())
                return;
            if (attackAction.WasPressedThisFrame()) TryStartAttack();
            return;
        }

        float elapsed = Time.time - attackStartedAt;
        if (previousElapsed < activeEnd && elapsed >= activeStart)
            CheckHits();
        previousElapsed = elapsed;

        if (elapsed >= attackDuration) CancelAttack();
    }

    private void TryStartAttack()
    {
        if (!motor.Grounded || !character.isGrounded || motor.IsDodging ||
            !stamina.TrySpend(staminaCost)) return;

        IsAttacking = true;
        attackStartedAt = Time.time;
        previousElapsed = 0f;
        attackDirection = transform.forward;
        attackDirection.y = 0f;
        attackDirection.Normalize();
        hitTargets.Clear();
    }

    private void CheckHits()
    {
        Vector3 origin = attackOrigin.position;
        Vector3 pointA = origin + attackDirection * 0.35f;
        Vector3 pointB = origin + attackDirection * 1.4f;
        Collider[] colliders = Physics.OverlapCapsule(pointA, pointB, 0.4f,
            targetMask, QueryTriggerInteraction.Collide);

        foreach (Collider candidate in colliders)
        {
            IDamageable target = candidate.GetComponentInParent<IDamageable>();
            if (target == null || hitTargets.Contains(target) ||
                !(target is Component targetComponent)) continue;
            if (targetComponent.transform.IsChildOf(transform)) continue;

            Vector3 offset = candidate.ClosestPoint(origin) - origin;
            float distance = offset.magnitude;
            if (distance > 0.001f && Physics.Raycast(origin, offset.normalized,
                out RaycastHit blocker, distance, visibilityMask,
                QueryTriggerInteraction.Ignore))
            {
                IDamageable visibleTarget = blocker.collider.GetComponentInParent<IDamageable>();
                if (visibleTarget != target) continue;
            }

            hitTargets.Add(target);
            target.TryTakeDamage(new DamageInfo(gameObject, damage, DamageTeam.Player));
        }
    }

    private void CancelAttack()
    {
        IsAttacking = false;
        previousElapsed = 0f;
        hitTargets.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null) return;
        Vector3 direction = IsAttacking ? attackDirection : transform.forward;
        Vector3 a = attackOrigin.position + direction * 0.35f;
        Vector3 b = attackOrigin.position + direction * 1.4f;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(a, 0.4f);
        Gizmos.DrawWireSphere(b, 0.4f);
        Gizmos.DrawLine(a, b);
    }
}
