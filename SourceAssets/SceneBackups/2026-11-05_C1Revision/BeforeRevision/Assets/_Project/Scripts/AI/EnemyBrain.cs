using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Idle, Patrol, Chase, Attack, Search, Return, Dead }

// FSM Hắc Ảnh. NavMeshAgent là nguồn dịch chuyển duy nhất của địch.
[RequireComponent(typeof(NavMeshAgent), typeof(EnemyHealth))]
public class EnemyBrain : MonoBehaviour
{
    [SerializeField] private EnemyDefinition definition;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private GameObject telegraph;
    [SerializeField] private Vector3 attackBoxSize = new Vector3(1.2f, 1.6f, 1.6f);
    [SerializeField] private LayerMask sightBlockers = 1 << 3;
    [SerializeField] private LayerMask playerMask = 1 << 6;

    private NavMeshAgent agent;
    private EnemyHealth health;
    private PlayerVitals player;
    private Vector3 spawnPosition;
    private Vector3 lastSeenPosition;
    private int patrolIndex;
    private float stateTime;
    private float lastSeenTime;
    private float nextAttackTime;
    private bool attackHitDone;
    private readonly Collider[] hits = new Collider[4];

    public EnemyState State { get; private set; } = EnemyState.Idle;
    public EnemyDefinition Definition=>definition;
    public float StateElapsed=>stateTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
        spawnPosition = transform.position;
        if (definition == null)
        {
            Debug.LogError($"{name}: EnemyBrain thiếu Definition.", this);
            enabled = false;
            return;
        }
        if (telegraph != null) telegraph.SetActive(false);
    }

    private void OnEnable()
    {
        if (health == null) return;
        health.Died += OnDied;
        health.Changed += OnDamaged;
    }

    private void OnDisable()
    {
        if (health == null) return;
        health.Died -= OnDied;
        health.Changed -= OnDamaged;
    }

    // Bị đánh từ phía sau vẫn quay lại đuổi An.
    private void OnDamaged()
    {
        if (State == EnemyState.Dead || health.IsDead || player == null || player.IsDead) return;
        if (State == EnemyState.Chase || State == EnemyState.Attack) return;

        lastSeenPosition = player.transform.position;
        lastSeenTime = Time.time;
        FacePlayer();
        Enter(EnemyState.Chase);
    }

    private void Start() => player = FindAnyObjectByType<PlayerVitals>();

    private void Update()
    {
        if (State == EnemyState.Dead || !agent.isOnNavMesh) return;
        stateTime += Time.deltaTime;

        // Ngoài lúc đang chơi hoặc An đã chết: đứng yên, không tấn công.
        GameSession session = AppRoot.Instance != null ? AppRoot.Instance.Session : null;
        bool canAct = player != null && !player.IsDead &&
                      (session == null || session.State == GameSession.SessionState.Playing);
        if (!canAct)
        {
            agent.isStopped = true;
            if (telegraph != null) telegraph.SetActive(false);
            return;
        }
        agent.isStopped = false;

        bool sees = CanSeePlayer();
        if (sees)
        {
            lastSeenTime = Time.time;
            lastSeenPosition = player.transform.position;
        }

        switch (State)
        {
            case EnemyState.Idle:
                if (sees) Enter(EnemyState.Chase);
                else if (patrolPoints != null && patrolPoints.Length >= 2 && stateTime > 1f) Enter(EnemyState.Patrol);
                break;

            case EnemyState.Patrol:
                if (sees) { Enter(EnemyState.Chase); break; }
                if (!agent.pathPending && agent.remainingDistance < 0.4f)
                {
                    patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
                    agent.SetDestination(patrolPoints[patrolIndex].position);
                }
                break;

            case EnemyState.Chase:
                if (Vector3.Distance(transform.position, spawnPosition) > definition.leashDistance) { Enter(EnemyState.Return); break; }
                if (Time.time - lastSeenTime > definition.loseSightTime) { Enter(EnemyState.Search); break; }
                agent.SetDestination(lastSeenPosition);
                if (sees && DistanceToPlayer() <= definition.attackRange && Time.time >= nextAttackTime) Enter(EnemyState.Attack);
                break;

            case EnemyState.Attack:
                TickAttack();
                break;

            case EnemyState.Search:
                if (sees) { Enter(EnemyState.Chase); break; }
                if (stateTime > definition.searchTime) Enter(EnemyState.Return);
                break;

            case EnemyState.Return:
                if (sees && Vector3.Distance(transform.position, spawnPosition) < definition.leashDistance * 0.8f) { Enter(EnemyState.Chase); break; }
                if (!agent.pathPending && agent.remainingDistance < 0.5f) Enter(EnemyState.Idle);
                break;
        }
    }

    private void Enter(EnemyState next)
    {
        State = next;
        stateTime = 0f;
        if (telegraph != null) telegraph.SetActive(false);

        switch (next)
        {
            case EnemyState.Idle:
                agent.ResetPath();
                break;
            case EnemyState.Patrol:
                agent.speed = definition.walkSpeed;
                agent.SetDestination(patrolPoints[patrolIndex].position);
                break;
            case EnemyState.Chase:
                agent.speed = definition.chaseSpeed;
                break;
            case EnemyState.Attack:
                agent.ResetPath();
                attackHitDone = false;
                FacePlayer();
                if (telegraph != null) telegraph.SetActive(true);
                break;
            case EnemyState.Search:
                agent.speed = definition.walkSpeed;
                agent.SetDestination(lastSeenPosition);
                break;
            case EnemyState.Return:
                agent.speed = definition.walkSpeed;
                agent.SetDestination(spawnPosition);
                break;
        }
    }

    // Báo trước → cửa sổ sát thương → hồi thế. Animator không quyết định sát thương.
    private void TickAttack()
    {
        float telegraphEnd = definition.attackTelegraph;
        float activeEnd = telegraphEnd + definition.attackActive;

        if (stateTime < telegraphEnd) return;

        if (telegraph != null) telegraph.SetActive(false);
        if (!attackHitDone && stateTime <= activeEnd + Time.deltaTime)
        {
            attackHitDone = true;
            TryHitPlayer();
        }

        if (stateTime >= activeEnd + 0.3f)
        {
            nextAttackTime = Time.time + definition.attackCooldown;
            Enter(EnemyState.Chase);
        }
    }

    private void TryHitPlayer()
    {
        if (player == null || player.IsDead) return;

        Vector3 center = transform.position + transform.forward * (attackBoxSize.z * 0.5f + 0.3f) + Vector3.up * 0.9f;
        int count = Physics.OverlapBoxNonAlloc(center, attackBoxSize * 0.5f, hits, transform.rotation, playerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            PlayerVitals target = hits[i].GetComponentInParent<PlayerVitals>();
            if (target == null || target != player || !HasLineOfSight()) continue;
            target.TryTakeDamage(new DamageInfo(gameObject, definition.damage, DamageTeam.Enemy) { AttackId = $"{name}_atk" });
            return;
        }
    }

    private bool CanSeePlayer()
    {
        if (player == null || player.IsDead) return false;
        Vector3 toPlayer = player.transform.position - transform.position;
        if (toPlayer.magnitude > definition.detectionRadius) return false;

        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        // Đang đuổi thì không cần nằm trong góc nhìn (đã phát hiện rồi).
        if (State != EnemyState.Chase && State != EnemyState.Attack &&
            Vector3.Angle(transform.forward, flat) > definition.viewAngle * 0.5f)
            return false;

        return HasLineOfSight();
    }

    private bool HasLineOfSight()
    {
        Vector3 eye = transform.position + Vector3.up * 1.5f;
        Vector3 target = player.transform.position + Vector3.up * 1.2f;
        return !Physics.Linecast(eye, target, sightBlockers, QueryTriggerInteraction.Ignore);
    }

    private float DistanceToPlayer()
    {
        Vector3 d = player.transform.position - transform.position;
        d.y = 0f;
        return d.magnitude;
    }

    private void FacePlayer()
    {
        Vector3 d = player.transform.position - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude > 0.001f) transform.rotation = Quaternion.LookRotation(d);
    }

    private void OnDied(EnemyHealth _)
    {
        State = EnemyState.Dead;
        if (telegraph != null) telegraph.SetActive(false);
        if (agent.isOnNavMesh) agent.isStopped = true;
        agent.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
        Destroy(gameObject, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        if (definition == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, definition.detectionRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? spawnPosition : transform.position, definition.leashDistance);
    }
}
