using UnityEngine;

// Hắc Ảnh Ném (C2 mục 3): đứng yên trên gờ, luôn quay về An, báo trước 0,6 s bằng vệt đỏ rồi ném cầu sương
// chậm (7 m/s, 12 sát thương). An lại gần trong 2 m thì đánh cận chiến (10 sát thương, hồi 1,6 s).
[RequireComponent(typeof(EnemyHealth))]
public class ThrowerBrain : MonoBehaviour
{
    public enum ThrowerState { Idle, Aim, Cooldown, Melee, Dead }

    [SerializeField] private Transform hand;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private Transform aimTelegraph;
    [SerializeField] private GameObject meleeTelegraph;
    [SerializeField] private float detectRange = 10f; // spec 14 m; giảm sau thử nghiệm: hai gờ cách 11 m không bắn chéo
    [SerializeField] private float aimTime = 0.6f;
    [SerializeField] private float throwCooldown = 3f; // spec 2,5 s; giãn sau thử nghiệm
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private float projectileDamage = 12f;
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeTelegraphTime = 0.4f;
    [SerializeField] private float meleeDamage = 10f;
    [SerializeField] private float meleeCooldown = 1.6f;
    [SerializeField] private LayerMask sightBlockers = 1 << 3;

    private EnemyHealth health;
    private PlayerVitals player;
    private float stateTime;
    private float nextMelee;
    private Vector3 lockedTarget;
    private int throws;

    public ThrowerState State { get; private set; } = ThrowerState.Idle;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (hand == null || projectilePrefab == null || aimTelegraph == null)
        {
            Debug.LogError($"{name}: ThrowerBrain thiếu Hand, Projectile hoặc Aim Telegraph.", this);
            enabled = false;
            return;
        }
        aimTelegraph.gameObject.SetActive(false);
        if (meleeTelegraph != null) meleeTelegraph.SetActive(false);
    }

    private void OnEnable() { if (health != null) health.Died += OnDied; }
    private void OnDisable() { if (health != null) health.Died -= OnDied; }

    private void Start() => player = FindAnyObjectByType<PlayerVitals>();

    private void Update()
    {
        if (State == ThrowerState.Dead || Time.deltaTime <= 0f) return;

        GameSession session = AppRoot.Instance != null ? AppRoot.Instance.Session : null;
        if (player == null || player.IsDead || (session != null && session.State != GameSession.SessionState.Playing))
        {
            Enter(ThrowerState.Idle);
            return;
        }

        stateTime += Time.deltaTime;
        Vector3 toPlayer = player.transform.position - transform.position;
        Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
        if (flat.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(flat);
        bool sees = toPlayer.magnitude <= detectRange && HasLineOfSight();

        if (flat.magnitude <= meleeRange && State != ThrowerState.Melee && Time.time >= nextMelee)
        {
            Enter(ThrowerState.Melee);
            return;
        }

        switch (State)
        {
            case ThrowerState.Idle:
                if (sees) Enter(ThrowerState.Aim);
                break;
            case ThrowerState.Aim:
                if (!sees) { Enter(ThrowerState.Idle); break; }
                lockedTarget = player.transform.position + Vector3.up * 1f;
                UpdateAimLine();
                if (stateTime >= aimTime) Throw();
                break;
            case ThrowerState.Cooldown:
                if (stateTime >= throwCooldown) Enter(sees ? ThrowerState.Aim : ThrowerState.Idle);
                break;
            case ThrowerState.Melee:
                if (stateTime >= meleeTelegraphTime && meleeTelegraph != null && meleeTelegraph.activeSelf)
                {
                    meleeTelegraph.SetActive(false);
                    if (flat.magnitude <= meleeRange + 0.3f && HasLineOfSight())
                        player.TryTakeDamage(new DamageInfo(gameObject, meleeDamage, DamageTeam.Enemy) { AttackId = $"{name}_melee" });
                    nextMelee = Time.time + meleeCooldown;
                }
                if (stateTime >= meleeTelegraphTime + 0.3f) Enter(ThrowerState.Idle);
                break;
        }
    }

    private void Enter(ThrowerState next)
    {
        if (State == next && next == ThrowerState.Idle) return;
        State = next;
        stateTime = 0f;
        aimTelegraph.gameObject.SetActive(next == ThrowerState.Aim);
        if (meleeTelegraph != null) meleeTelegraph.SetActive(next == ThrowerState.Melee);
    }

    private void UpdateAimLine()
    {
        Vector3 from = hand.position;
        Vector3 d = lockedTarget - from;
        aimTelegraph.position = from + d * 0.5f;
        aimTelegraph.rotation = Quaternion.LookRotation(d);
        aimTelegraph.localScale = new Vector3(0.15f, 0.02f, d.magnitude);
    }

    // Hướng chốt lúc ném; đạn bay chậm nên An né được bằng bước ngang.
    private void Throw()
    {
        throws++;
        EnemyProjectile p = Instantiate(projectilePrefab, hand.position, Quaternion.identity);
        p.Launch(lockedTarget - hand.position, projectileSpeed, projectileDamage, gameObject, $"{name}_throw_{throws}");
        Enter(ThrowerState.Cooldown);
    }

    private bool HasLineOfSight() =>
        !Physics.Linecast(transform.position + Vector3.up * 1.5f, player.transform.position + Vector3.up * 1.2f, sightBlockers, QueryTriggerInteraction.Ignore);

    private void OnDied(EnemyHealth _)
    {
        State = ThrowerState.Dead;
        aimTelegraph.gameObject.SetActive(false);
        if (meleeTelegraph != null) meleeTelegraph.SetActive(false);
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
        Destroy(gameObject, 1f);
    }
}
