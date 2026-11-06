using TMPro;
using UnityEngine;

// Hộ vệ Đá (spec phần 16, C1-R05): quét tay ở gần, nện đất ở tầm trung, xa thì đuổi.
// Trùm di chuyển bằng transform của chính nó (đấu trường phẳng), không dùng NavMeshAgent.
[RequireComponent(typeof(EnemyHealth))]
public class StoneGuardian : MonoBehaviour, IArenaBoss
{
    public enum GuardianState { Dormant, Chase, Sweep, Slam, Dead }

    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private string bossId = WorldIds.GuardianBoss;
    [SerializeField] private Vector3 arenaMin = new Vector3(-8f, 0f, 91f);
    [SerializeField] private Vector3 arenaMax = new Vector3(8f, 0f, 107f);

    [Header("Báo trước")]
    [SerializeField] private GameObject sweepTelegraph;
    [SerializeField] private GameObject slamTelegraph;

    [Header("Thanh máu trùm (tùy chọn)")]
    [SerializeField] private GameObject bossBar;
    [SerializeField] private RectTransform bossBarFill;
    [SerializeField] private TMP_Text bossBarLabel;

    [Header("Di chuyển")]
    [SerializeField] private float moveSpeed = 2.4f;
    [SerializeField] private float turnSpeed = 240f;

    [Header("Quét tay")]
    [SerializeField] private float sweepRange = 2.6f;
    [SerializeField] private float sweepTelegraphTime = 0.8f;
    [SerializeField] private float sweepActiveTime = 0.2f;
    [SerializeField] private float sweepRecovery = 1.2f;
    [SerializeField] private float sweepDamage = 20f;
    [SerializeField] private Vector3 sweepBoxSize = new Vector3(3f, 2f, 2.4f);

    [Header("Nện đất")]
    [SerializeField] private float slamMaxRange = 7f;
    [SerializeField] private float slamTelegraphTime = 1.2f;
    [SerializeField] private float slamActiveTime = 0.25f;
    [SerializeField] private float slamRecovery = 1.5f;
    [SerializeField] private float slamDamage = 25f;
    [SerializeField] private float slamRadius = 3f;
    [SerializeField] private float slamCooldown = 3f;

    [SerializeField] private LayerMask playerMask = 1 << 6;
    [SerializeField] private LayerMask sightBlockers = 1 << 3;

    private EnemyHealth health;
    private PlayerVitals player;
    private Vector3 slamCenter;
    private float stateTime;
    private float nextSlamTime;
    private bool hitDone;
    private int attackCount;
    private readonly Collider[] hits = new Collider[4];

    public GuardianState State { get; private set; } = GuardianState.Dormant;
    public float StateElapsed=>stateTime;
    public float AttackWindup=>State==GuardianState.Sweep?sweepTelegraphTime:slamTelegraphTime;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (worldState == null || sweepTelegraph == null || slamTelegraph == null || !WorldIds.IsKnown(StateSet.Bosses, bossId))
        {
            Debug.LogError($"{name}: StoneGuardian thiếu WorldState, telegraph hoặc ID trùm sai.", this);
            enabled = false;
            return;
        }
        HideTelegraphs();
        if (bossBar != null) bossBar.SetActive(false);
    }

    private void OnEnable()
    {
        if (health == null) return;
        health.Died += OnDied;
        health.Changed += OnDamaged;
        if (worldState != null) worldState.Changed += ApplyWorldState;
    }

    private void OnDisable()
    {
        if (health == null) return;
        health.Died -= OnDied;
        health.Changed -= OnDamaged;
        if (worldState != null) worldState.Changed -= ApplyWorldState;
        if (bossBar != null) bossBar.SetActive(false);
    }

    private void Start()
    {
        player = FindAnyObjectByType<PlayerVitals>();
        ApplyWorldState();
    }

    // Trùm độc nhất: đã bị đánh bại thì không xuất hiện lại sau khi tải.
    private void ApplyWorldState()
    {
        if (State != GuardianState.Dead && worldState.Has(StateSet.Bosses, bossId)) gameObject.SetActive(false);
    }

    public void Activate()
    {
        if (!isActiveAndEnabled || State != GuardianState.Dormant) return;
        Enter(GuardianState.Chase);
        if (bossBar != null) bossBar.SetActive(true);
        RefreshBar();
    }

    private void OnDamaged()
    {
        RefreshBar();
        if (State == GuardianState.Dormant && !health.IsDead) Activate();
    }

    private void Update()
    {
        if (State == GuardianState.Dormant || State == GuardianState.Dead) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        // An chết hoặc phiên không ở trạng thái chơi: đứng yên, hủy đòn đang báo.
        GameSession session = AppRoot.Instance != null ? AppRoot.Instance.Session : null;
        if (player == null || player.IsDead ||
            (session != null && session.State != GameSession.SessionState.Playing))
        {
            if (State != GuardianState.Chase) Enter(GuardianState.Chase);
            return;
        }

        float previous = stateTime;
        stateTime += dt;

        switch (State)
        {
            case GuardianState.Chase: TickChase(dt); break;
            case GuardianState.Sweep: TickAttack(previous, sweepTelegraphTime, sweepActiveTime, sweepRecovery, true); break;
            case GuardianState.Slam: TickAttack(previous, slamTelegraphTime, slamActiveTime, slamRecovery, false); break;
        }
    }

    private void TickChase(float dt)
    {
        float distance = FlatDistanceToPlayer();
        FacePlayer(turnSpeed * dt);

        if (distance <= sweepRange)
        {
            Enter(GuardianState.Sweep);
            return;
        }

        if (distance <= slamMaxRange && Time.time >= nextSlamTime)
        {
            Enter(GuardianState.Slam);
            return;
        }

        Vector3 dir = player.transform.position - transform.position;
        dir.y = 0f;
        Vector3 next = transform.position + dir.normalized * moveSpeed * dt;
        next.x = Mathf.Clamp(next.x, arenaMin.x, arenaMax.x);
        next.z = Mathf.Clamp(next.z, arenaMin.z, arenaMax.z);
        Vector3 move=next-transform.position;
        // Environment collision for the existing transform motor; no competing agent/root motion.
        if(move.sqrMagnitude>.000001f&&Physics.CapsuleCast(transform.position+Vector3.up*.65f,transform.position+Vector3.up*2.4f,.58f,move.normalized,out var obstruction,move.magnitude+.05f,sightBlockers,QueryTriggerInteraction.Ignore))
        {
            move=Vector3.ProjectOnPlane(move,obstruction.normal);move.y=0;
            if(move.sqrMagnitude>.000001f&&Physics.CapsuleCast(transform.position+Vector3.up*.65f,transform.position+Vector3.up*2.4f,.58f,move.normalized,move.magnitude+.05f,sightBlockers,QueryTriggerInteraction.Ignore))move=Vector3.zero;
        }
        transform.position += move;
    }

    // Báo trước → cửa sổ sát thương (xét giao khoảng thời gian frame) → hồi thế.
    private void TickAttack(float previous, float telegraphTime, float activeTime, float recovery, bool sweep)
    {
        float activeEnd = telegraphTime + activeTime;

        if (stateTime >= telegraphTime) HideTelegraphs();

        if (!hitDone && previous < activeEnd && stateTime >= telegraphTime)
        {
            hitDone = true;
            if (sweep) TryHitSweep();
            else TryHitSlam();
        }

        if (stateTime >= activeEnd + recovery) Enter(GuardianState.Chase);
    }

    private void Enter(GuardianState next)
    {
        State = next;
        stateTime = 0f;
        hitDone = false;
        HideTelegraphs();

        switch (next)
        {
            case GuardianState.Sweep:
                attackCount++;
                FacePlayer(360f);
                sweepTelegraph.SetActive(true);
                break;
            case GuardianState.Slam:
                attackCount++;
                FacePlayer(360f);
                nextSlamTime = Time.time + slamCooldown;
                slamCenter = player.transform.position;
                slamCenter.y = transform.position.y;
                slamTelegraph.transform.position = slamCenter + Vector3.up * 0.03f;
                slamTelegraph.SetActive(true);
                break;
        }
    }

    private void TryHitSweep()
    {
        Vector3 center = transform.position + transform.forward * (sweepBoxSize.z * 0.5f + 0.4f) + Vector3.up * 1f;
        int count = Physics.OverlapBoxNonAlloc(center, sweepBoxSize * 0.5f, hits, transform.rotation, playerMask, QueryTriggerInteraction.Ignore);
        TryDamage(count, transform.position + Vector3.up * 1.2f, sweepDamage, "sweep");
    }

    private void TryHitSlam()
    {
        int count = Physics.OverlapSphereNonAlloc(slamCenter + Vector3.up * 0.9f, slamRadius, hits, playerMask, QueryTriggerInteraction.Ignore);
        TryDamage(count, slamCenter + Vector3.up * 0.5f, slamDamage, "slam");
    }

    // Một đòn trúng mỗi mục tiêu một lần; vật cản Environment chặn sát thương.
    private void TryDamage(int count, Vector3 origin, float damage, string attackName)
    {
        for (int i = 0; i < count; i++)
        {
            PlayerVitals target = hits[i].GetComponentInParent<PlayerVitals>();
            if (target == null || target != player || target.IsDead) continue;
            if (Physics.Linecast(origin, target.transform.position + Vector3.up * 1f, sightBlockers, QueryTriggerInteraction.Ignore)) return;

            target.TryTakeDamage(new DamageInfo(gameObject, damage, DamageTeam.Enemy) { AttackId = $"{bossId}_{attackName}_{attackCount}" });
            return;
        }
    }

    private void OnDied(EnemyHealth _)
    {
        State = GuardianState.Dead;
        HideTelegraphs();
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
        if (bossBar != null) bossBar.SetActive(false);

        // Ghi trạng thái trước: cổng mở và bệ trống sáng qua Changed.
        worldState.TryAdd(StateSet.Bosses, bossId);
        if (AppRoot.Instance != null && AppRoot.Instance.UI != null)
            AppRoot.Instance.UI.ShowToast("Hộ vệ Đá đã bị đánh bại");
        Invoke(nameof(Hide), 1f);
    }

    private void Hide() => gameObject.SetActive(false);

    private void HideTelegraphs()
    {
        if (sweepTelegraph != null) sweepTelegraph.SetActive(false);
        if (slamTelegraph != null) slamTelegraph.SetActive(false);
    }

    private void RefreshBar()
    {
        if (bossBarFill == null || health == null) return;
        Vector2 anchor = bossBarFill.anchorMax;
        anchor.x = Mathf.Clamp01(health.Current / health.Max);
        bossBarFill.anchorMax = anchor;
        if (bossBarLabel != null) bossBarLabel.text = $"Hộ vệ Đá  {Mathf.CeilToInt(health.Current)}/{Mathf.CeilToInt(health.Max)}";
    }

    private float FlatDistanceToPlayer()
    {
        Vector3 d = player.transform.position - transform.position;
        d.y = 0f;
        return d.magnitude;
    }

    private void FacePlayer(float maxDegrees)
    {
        Vector3 d = player.transform.position - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(d), maxDegrees);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sweepRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamMaxRange);
    }
}
