using UnityEngine;

// Bộ lộn "Quà Kim Quy" (spec phần 43). Chỉ dùng được khi có movement_kit; chương 1 bấm Ctrl không lộn.
// Mọi dịch chuyển đi qua PlayerMotor nên tường vẫn chặn.
[DefaultExecutionOrder(-60)]
[RequireComponent(typeof(PlayerMotor), typeof(PlayerVitals), typeof(PlayerStamina))]
public class DodgeController : MonoBehaviour
{
    [SerializeField] private float staminaCost = 25f;
    [SerializeField] private float duration = 0.45f;
    [SerializeField] private float distance = 3.6f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float minInterval = 0.65f;
    [SerializeField] private float invulnerableStart = 0.10f;
    [SerializeField] private float invulnerableEnd = 0.30f;

    [Header("Né đúng lúc")]
    [SerializeField] private float perfectWindow = 0.15f;
    [SerializeField] private float slowScale = 0.3f;
    [SerializeField] private float slowSeconds = 0.3f;
    [SerializeField] private float lanternReward = 15f;

    [Header("Chui khe thấp")]
    [SerializeField] private float crawlSpeed = 3f;
    [SerializeField] private float maxCrawlTime = 0.8f;

    private PlayerMotor motor;
    private PlayerVitals vitals;
    private PlayerStamina stamina;
    private PlayerAbilities abilities;
    private LanternController lantern;
    private CombatController combat;
    private Animator animator;
    private InputReader input;
    private PlayerActionState actions;

    private float rollTime = -1f;
    private float lastStart = float.NegativeInfinity;
    private float crawlTime;
    private Vector3 rollDirection;
    private bool perfectUsed;
    private int animRoll;

    public bool IsDodging => rollTime >= 0f;
    public float RollProgress => IsDodging ? Mathf.Clamp01(rollTime / duration) : 0f;
    public float LastRollEndTime { get; private set; } = float.NegativeInfinity;
    public int PerfectDodgeCount { get; private set; }

    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        actions = GetComponent<PlayerActionState>();
        vitals = GetComponent<PlayerVitals>();
        stamina = GetComponent<PlayerStamina>();
        abilities = GetComponent<PlayerAbilities>();
        lantern = GetComponent<LanternController>();
        combat = GetComponent<CombatController>();
        animator = GetComponent<Animator>();
        animRoll = Animator.StringToHash("Roll");
        if (abilities == null)
        {
            Debug.LogError($"{name}: DodgeController cần PlayerAbilities.", this);
            enabled = false;
        }
    }

    private void OnEnable() { if (vitals != null) vitals.DamageAvoided += OnDamageAvoided; }

    private void OnDisable()
    {
        if (vitals != null) vitals.DamageAvoided -= OnDamageAvoided;
        CancelRoll();
    }

    private void Start()
    {
        input = AppRoot.Instance != null ? AppRoot.Instance.Input : null;
        if (input == null || !input.IsReady)
        {
            Debug.LogError($"{name}: DodgeController không tìm thấy InputReader.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (Time.deltaTime <= 0f) return;

        if (vitals.IsDead)
        {
            CancelRoll();
            return;
        }

        if (IsDodging)
        {
            TickRoll();
            return;
        }

        // Đã ra khỏi khe thì đứng dậy.
        if (motor.IsLow) motor.SetLow(false);

        if (input.WasPressed(input.Dodge)) TryStartRoll();
    }

    public bool TryStartRoll()
    {
        if (!actions.Active || actions.IsHurt || !isActiveAndEnabled || IsDodging || vitals.IsDead || !abilities.Has(PlayerAbilities.MovementKit)) return false;
        if (Time.time - lastStart < minInterval || !motor.IsGrounded) return false;
        if (combat != null && combat.IsInDamageWindow) return false;
        if (!stamina.TrySpend(staminaCost)) return false;

        Vector3 direction = motor.ReadMoveDirection();
        bool hasMove = direction.sqrMagnitude >= 0.01f;
        if (direction.sqrMagnitude < 0.01f) direction = transform.forward;
        direction.y = 0f;
        rollDirection = direction.normalized;

        bool sprintRoll = input.IsHeld(input.Sprint) && hasMove &&
            (lantern == null || !lantern.IsFocusing);
        float speed = distance * (sprintRoll ? sprintMultiplier : 1f) / duration;
        // Một clip lộn: xoay thân theo hướng lộn (spec phần 43), sau lộn khóa mục tiêu quay mặt lại.
        motor.BeginDash(rollDirection * speed, duration);
        motor.SetLow(true);
        if (combat != null) combat.CancelAttack();

        lastStart = Time.time;
        rollTime = 0f;
        crawlTime = 0f;
        perfectUsed = false;
        if (animator != null && HasParameter(animRoll)) animator.SetTrigger(animRoll);
        return true;
    }

    private void TickRoll()
    {
        rollTime += Time.deltaTime;
        vitals.IsInvulnerable = rollTime >= invulnerableStart && rollTime < invulnerableEnd;

        if (rollTime < duration) return;

        // Còn kẹt dưới khe thấp: bò tiếp theo hướng lộn tới khi đứng dậy được.
        if (!motor.SetLow(false) && crawlTime < maxCrawlTime)
        {
            crawlTime += Time.deltaTime;
            motor.BeginDash(rollDirection * crawlSpeed, Time.deltaTime * 2f);
            return;
        }
        EndRoll();
    }

    public void CancelRoll()
    {
        EndRoll(false);
        LastRollEndTime = float.NegativeInfinity;
    }

    private void EndRoll(bool completed = true)
    {
        if (!IsDodging) return;
        rollTime = -1f;
        if (vitals != null) vitals.IsInvulnerable = false;
        if (motor != null) motor.CancelDash();
        if (completed) LastRollEndTime = Time.time;
    }

    // Đòn địch bị chặn trong 0,15 giây đầu của khoảng bất tử: né đúng lúc.
    private void OnDamageAvoided(DamageInfo damage)
    {
        if (!IsDodging || perfectUsed || damage.SourceTeam != DamageTeam.Enemy) return;
        if (rollTime - invulnerableStart > perfectWindow) return;

        perfectUsed = true;
        PerfectDodgeCount++;
        if (lantern != null) lantern.AddEnergy(lanternReward);
        if (AppRoot.Instance != null)
        {
            AppRoot.Instance.Session.RequestSlowMotion(slowScale, slowSeconds);
            if (AppRoot.Instance.UI != null) AppRoot.Instance.UI.ShowToast("Né đúng lúc!", 1f);
        }
    }

    private bool HasParameter(int hash)
    {
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.nameHash == hash) return true;
        return false;
    }
}
