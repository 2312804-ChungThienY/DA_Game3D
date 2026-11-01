using UnityEngine;
using UnityEngine.AI;

public enum EnemyMovementState
{
    Patrol,
    Chase,
    Search,
    Return
}

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyPatrolChase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerVitals player;
    [SerializeField] private Transform eyePoint;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Vision")]
    [SerializeField, Min(0f)] private float sightRange = 10f;
    [SerializeField, Range(0f, 360f)] private float fullViewAngle = 120f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Behaviour")]
    [SerializeField, Min(0f)] private float lostSightDelay = 3f;
    [SerializeField, Min(0f)] private float searchDuration = 2f;
    [SerializeField, Min(0f)] private float maxChaseDistance = 18f;
    [SerializeField, Min(0f)] private float chaseStopDistance = 1.7f;

    [Header("Movement")]
    [SerializeField, Min(0f)] private float patrolSpeed = 2f;
    [SerializeField, Min(0f)] private float chaseSpeed = 3.5f;
    [SerializeField, Min(0.05f)] private float repathInterval = 0.25f;

    [SerializeField] private EnemyMovementState state = EnemyMovementState.Patrol;

    private NavMeshAgent agent;
    private NavMeshPath path;
    private EnemyMeleeAttack meleeAttack;
    private Vector3 homePosition;
    private Vector3 lastSeenPosition;
    private int patrolIndex;
    private float lastSeenTime;
    private float searchStartedAt;
    private float nextRepathAt;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        meleeAttack = GetComponent<EnemyMeleeAttack>();
        path = new NavMeshPath();

        bool valid = player != null && eyePoint != null &&
                     patrolPoints != null && patrolPoints.Length >= 2;
        if (valid)
        {
            foreach (Transform point in patrolPoints)
            {
                if (point != null) continue;
                valid = false;
                break;
            }
        }

        if (!valid)
        {
            Debug.LogError($"{name}: Gán Player, EyePoint và ít nhất hai điểm tuần tra.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        if (!enabled) return;
        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{name}: Agent chưa nằm trên NavMesh. Kiểm tra Bake và Agent Type.", this);
            enabled = false;
            return;
        }

        homePosition = transform.position;
        state = EnemyMovementState.Patrol;
    }

    private void Update()
    {
        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
        if (!Application.isFocused || Time.timeScale == 0f || player == null ||
            !player.gameObject.activeInHierarchy || player.IsDead)
        {
            agent.isStopped = true;
            return;
        }
        if (meleeAttack != null && meleeAttack.IsAttacking)
        {
            agent.isStopped = true;
            return;
        }
        agent.isStopped = false;

        bool insideChaseArea =
            Vector3.Distance(player.transform.position, homePosition) <= maxChaseDistance &&
            Vector3.Distance(transform.position, homePosition) <= maxChaseDistance;

        if ((state == EnemyMovementState.Chase || state == EnemyMovementState.Search) &&
            !insideChaseArea)
            ChangeState(EnemyMovementState.Return);

        // Finish returning home before considering a new chase.
        if (state != EnemyMovementState.Return && insideChaseArea && CanSeePlayer())
        {
            lastSeenPosition = player.transform.position;
            lastSeenTime = Time.time;
            ChangeState(EnemyMovementState.Chase);
        }

        switch (state)
        {
            case EnemyMovementState.Patrol:
                agent.speed = patrolSpeed;
                agent.stoppingDistance = 0.2f;
                Vector3 patrolTarget = patrolPoints[patrolIndex].position;
                MoveTo(patrolTarget);
                if (Reached(patrolTarget))
                {
                    patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                    nextRepathAt = 0f;
                }
                break;

            case EnemyMovementState.Chase:
                agent.speed = chaseSpeed;
                agent.stoppingDistance = chaseStopDistance;
                MoveTo(lastSeenPosition);
                if (meleeAttack != null && CanSeePlayer())
                    meleeAttack.TryStartAttack();
                if (Time.time - lastSeenTime >= lostSightDelay)
                    ChangeState(EnemyMovementState.Search);
                break;

            case EnemyMovementState.Search:
                agent.speed = patrolSpeed;
                agent.stoppingDistance = 0.2f;
                MoveTo(lastSeenPosition);
                if (Time.time - searchStartedAt >= searchDuration)
                    ChangeState(EnemyMovementState.Return);
                break;

            case EnemyMovementState.Return:
                agent.speed = patrolSpeed;
                agent.stoppingDistance = 0.2f;
                MoveTo(homePosition);
                if (Reached(homePosition))
                {
                    patrolIndex = 0;
                    ChangeState(EnemyMovementState.Patrol);
                }
                break;
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 targetPoint = player.transform.position + Vector3.up;
        Vector3 offset = targetPoint - eyePoint.position;
        if (offset.magnitude > sightRange) return false;

        Vector3 horizontalDirection = Vector3.ProjectOnPlane(offset, Vector3.up);
        if (Vector3.Angle(transform.forward, horizontalDirection) > fullViewAngle * 0.5f)
            return false;

        return !Physics.Linecast(eyePoint.position, targetPoint, obstacleMask,
            QueryTriggerInteraction.Ignore);
    }

    private void ChangeState(EnemyMovementState next)
    {
        if (state == next) return;
        state = next;
        nextRepathAt = 0f;
        if (next == EnemyMovementState.Search) searchStartedAt = Time.time;
    }

    private void MoveTo(Vector3 destination)
    {
        if (Time.time < nextRepathAt) return;
        nextRepathAt = Time.time + Mathf.Max(0.05f, repathInterval);

        NavMeshQueryFilter filter = new NavMeshQueryFilter
        {
            agentTypeID = agent.agentTypeID,
            areaMask = agent.areaMask
        };

        if (!NavMesh.SamplePosition(destination, out NavMeshHit hit, 1f, filter) ||
            !agent.CalculatePath(hit.position, path) ||
            path.status != NavMeshPathStatus.PathComplete)
        {
            agent.ResetPath();
            return;
        }
        agent.SetPath(path);
    }

    private bool Reached(Vector3 destination)
    {
        return !agent.pathPending &&
               Vector3.Distance(transform.position, destination) <=
               agent.stoppingDistance + 0.2f;
    }

    private void OnDisable()
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }
}
