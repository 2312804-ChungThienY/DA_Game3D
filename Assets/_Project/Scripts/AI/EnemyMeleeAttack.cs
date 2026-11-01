using UnityEngine;
using UnityEngine.AI;

public enum EnemyAttackPhase
{
    None = 0,
    Windup = 1,
    Recovery = 2,
    Active = 3
}

[DefaultExecutionOrder(-50)]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerVitals player;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private GameObject warningIndicator;

    [Header("Optional attack definitions")]
    [Tooltip("Selected first when the player is within this attack's start range.")]
    [SerializeField] private EnemyAttackDefinition closeAttack;
    [Tooltip("Selected when the player is outside close range but within this attack's range.")]
    [SerializeField] private EnemyAttackDefinition mediumAttack;
    [Tooltip("A unit-radius-0.5 cylinder, directly under the unscaled enemy root. Remove its collider.")]
    [SerializeField] private GameObject radialWarning;

    // Preserve the field names used by the previous Hac Anh component.
    // Leaving both definitions empty retains its original single-impact attack.
    [Header("Fallback attack - existing Hac Anh")]
    [SerializeField] private float damage = 15f;
    [SerializeField] private float startRange = 1.7f;
    [SerializeField] private float hitRange = 1.8f;
    [SerializeField] private float fullHitAngle = 90f;
    [SerializeField] private float maxHeightDifference = 1.5f;
    [SerializeField] private float windupTime = 0.45f;
    [SerializeField] private float recoveryTime = 0.55f;
    [SerializeField] private float cooldown = 1.6f;

    [Header("Visibility")]
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private EnemyAttackPhase phase = EnemyAttackPhase.None;

    private NavMeshAgent agent;
    private EnemyPatrolChase brain;
    private Vector3 attackDirection;
    private float attackStartedAt;
    private float previousElapsed;
    private float nextAttackAt = float.NegativeInfinity;
    private bool contactResolved;
    private bool rotationLocked;
    private bool previousUpdateRotation;
    private RuntimeAttack currentAttack;

    public bool IsAttacking => phase != EnemyAttackPhase.None;

    private struct RuntimeAttack
    {
        public EnemyAttackShape Shape;
        public float Damage, StartRange, HitRange, FullHitAngle;
        public float Windup, Active, Recovery, StartInterval;
        public float ActiveEnd => Windup + Active;
        public float TotalDuration => Windup + Active + Recovery;
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        brain = GetComponent<EnemyPatrolChase>();

        bool definitionsValid =
            (closeAttack == null || closeAttack.IsValid) &&
            (mediumAttack == null || mediumAttack.IsValid);

        if (player == null || attackOrigin == null || brain == null ||
            !definitionsValid || damage <= 0f || windupTime <= 0f || recoveryTime < 0f)
        {
            Debug.LogError($"{name}: Check attack references and definitions.", this);
            enabled = false;
        }

        SetWarnings(false);
    }

    private bool SelectAttack(float distance, out RuntimeAttack attack)
    {
        EnemyAttackDefinition definition = null;

        if (closeAttack != null && distance <= closeAttack.StartRange + 0.1f)
            definition = closeAttack;
        else if (mediumAttack != null && distance <= mediumAttack.StartRange + 0.1f)
            definition = mediumAttack;

        if (definition != null)
        {
            attack = new RuntimeAttack
            {
                Shape = definition.Shape,
                Damage = definition.Damage,
                StartRange = definition.StartRange,
                HitRange = definition.HitRange,
                FullHitAngle = definition.FullHitAngle,
                Windup = definition.WindupTime,
                Active = definition.ActiveDuration,
                Recovery = definition.RecoveryTime,
                StartInterval = definition.MinimumStartInterval
            };
            return true;
        }

        attack = default(RuntimeAttack);
        if (closeAttack != null || mediumAttack != null || distance > startRange + 0.1f)
            return false;

        attack = new RuntimeAttack
        {
            Shape = EnemyAttackShape.FrontCone,
            Damage = damage,
            StartRange = startRange,
            HitRange = hitRange,
            FullHitAngle = fullHitAngle,
            Windup = windupTime,
            Active = 0f,
            Recovery = recoveryTime,
            StartInterval = cooldown
        };
        return true;
    }

    public bool TryStartAttack()
    {
        if (!isActiveAndEnabled || IsAttacking || Time.timeScale == 0f ||
            !Application.isFocused || !brain.isActiveAndEnabled || player.IsDead ||
            !player.gameObject.activeInHierarchy || !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh || Time.time < nextAttackAt)
            return false;

        Vector3 offset = player.transform.position - transform.position;
        if (Mathf.Abs(offset.y) > maxHeightDifference) return false;

        Vector3 horizontal = Vector3.ProjectOnPlane(offset, Vector3.up);
        if (!SelectAttack(horizontal.magnitude, out RuntimeAttack selected) || !HasClearLine())
            return false;

        currentAttack = selected;
        attackDirection = horizontal.sqrMagnitude > 0.0001f
            ? horizontal.normalized : transform.forward;

        previousUpdateRotation = agent.updateRotation;
        agent.updateRotation = false;
        rotationLocked = true;
        transform.rotation = Quaternion.LookRotation(attackDirection, Vector3.up);
        agent.isStopped = true;

        attackStartedAt = Time.time;
        previousElapsed = 0f;
        nextAttackAt = Time.time + Mathf.Max(currentAttack.StartInterval, currentAttack.TotalDuration);
        contactResolved = false;
        phase = EnemyAttackPhase.Windup;

        if (radialWarning != null && currentAttack.Shape == EnemyAttackShape.Radial)
        {
            Vector3 scale = radialWarning.transform.localScale;
            radialWarning.transform.localScale = new Vector3(
                currentAttack.HitRange * 2f, scale.y, currentAttack.HitRange * 2f);
        }

        SetWarnings(true);
        return true;
    }

    private void Update()
    {
        if (!IsAttacking) return;

        if (!Application.isFocused || !brain.isActiveAndEnabled || player.IsDead ||
            !player.gameObject.activeInHierarchy || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            CancelAttack();
            return;
        }
        if (Time.timeScale == 0f) return;

        agent.isStopped = true;
        float elapsed = Time.time - attackStartedAt;

        bool crossesActive = currentAttack.Active > 0f
            ? previousElapsed < currentAttack.ActiveEnd && elapsed >= currentAttack.Windup
            : previousElapsed < currentAttack.Windup && elapsed >= currentAttack.Windup;

        // A missed contact can be checked again during the active window.
        // Once contact occurs (including a successful dodge), this strike is consumed.
        if (!contactResolved && crossesActive) CheckContact();

        if (elapsed < currentAttack.Windup)
        {
            phase = EnemyAttackPhase.Windup;
            SetWarnings(true);
        }
        else if (currentAttack.Active > 0f && elapsed < currentAttack.ActiveEnd)
        {
            phase = EnemyAttackPhase.Active;
            SetWarnings(true);
        }
        else
        {
            phase = EnemyAttackPhase.Recovery;
            SetWarnings(false);
        }

        previousElapsed = elapsed;
        if (elapsed >= currentAttack.TotalDuration) CancelAttack();
    }

    private void CheckContact()
    {
        if (player.IsDead) return;

        Vector3 offset = player.transform.position - transform.position;
        if (Mathf.Abs(offset.y) > maxHeightDifference) return;

        Vector3 horizontal = Vector3.ProjectOnPlane(offset, Vector3.up);
        if (horizontal.magnitude > currentAttack.HitRange) return;

        if (currentAttack.Shape == EnemyAttackShape.FrontCone &&
            horizontal.sqrMagnitude > 0.0001f &&
            Vector3.Angle(attackDirection, horizontal) > currentAttack.FullHitAngle * 0.5f)
            return;

        if (!HasClearLine()) return;

        contactResolved = true;
        player.TryTakeDamage(new DamageInfo(gameObject, currentAttack.Damage, DamageTeam.Enemy));
    }

    private bool HasClearLine()
    {
        return !Physics.Linecast(attackOrigin.position,
            player.transform.position + Vector3.up,
            obstacleMask, QueryTriggerInteraction.Ignore);
    }

    private void SetWarnings(bool visible)
    {
        if (warningIndicator != null && warningIndicator != gameObject)
            warningIndicator.SetActive(visible);

        if (radialWarning != null && radialWarning != gameObject)
            radialWarning.SetActive(visible && currentAttack.Shape == EnemyAttackShape.Radial);
    }

    private void CancelAttack()
    {
        phase = EnemyAttackPhase.None;
        SetWarnings(false);
        if (rotationLocked && agent != null) agent.updateRotation = previousUpdateRotation;
        rotationLocked = false;
    }

    private void OnDisable()
    {
        CancelAttack();
    }
}
