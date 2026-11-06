using UnityEngine;

// Đưa sự kiện gameplay vào Animator của An. Animator chỉ hiển thị: không quyết định sát thương hay dịch chuyển
// (Apply Root Motion tắt). Tham số thiếu trong controller thì bỏ qua.
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private float comboWindow = 0.9f;
    [Tooltip("Transform cha của Hips; xoay để lộn tròn và ngã khi chết.")]
    [SerializeField] private Transform skeleton;
    [SerializeField] private Vector3 bodyPivot = new Vector3(0f, 0.55f, 0f);
    [SerializeField] private float deathTiltAngle = -80f;

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int AttackIndexHash = Animator.StringToHash("AttackIndex");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeadHash = Animator.StringToHash("Dead");
    private static readonly int InteractHash = Animator.StringToHash("Interact");

    private Animator animator;
    private PlayerVitals vitals;
    private CombatController combat;
    private InteractionScanner scanner;
    private float lastHealth;
    private float lastAttackTime = float.NegativeInfinity;
    private int attackIndex;
    private DodgeController dodge;
    private LanternController lantern;
    private int lanternLayer = -1;
    private float deathTilt;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.applyRootMotion = false;
        vitals = GetComponent<PlayerVitals>();
        combat = GetComponent<CombatController>();
        scanner = GetComponent<InteractionScanner>();
        dodge = GetComponent<DodgeController>();
        lantern = GetComponent<LanternController>();
        if (skeleton == null) skeleton = transform.Find("Skeleton");
        lanternLayer = animator.GetLayerIndex("LanternArm");
    }

    // Phần thân xoay bằng code (chỉ hình ảnh, không ảnh hưởng collider/motor).
    private void LateUpdate()
    {
        if (lantern != null && lanternLayer >= 0)
            animator.SetLayerWeight(lanternLayer, Mathf.MoveTowards(animator.GetLayerWeight(lanternLayer),
                lantern.IsLanternOn ? 1f : 0f, Time.deltaTime * 6f));

        if (skeleton == null) return;
        bool dead = vitals != null && vitals.IsDead;
        deathTilt = Mathf.MoveTowards(deathTilt, dead ? 1f : 0f, Time.deltaTime * (dead ? 1.5f : 10f));
        float pitch = dodge != null && dodge.IsDodging ? dodge.RollProgress * 360f : deathTilt * deathTiltAngle;
        Quaternion r = Quaternion.Euler(pitch, 0f, 0f);
        skeleton.localRotation = r;
        skeleton.localPosition = bodyPivot - r * bodyPivot;
    }

    private void OnEnable()
    {
        if (vitals != null) { vitals.Changed += OnHealthChanged; vitals.Died += OnDied; lastHealth = vitals.CurrentHealth; }
        if (combat != null) combat.AttackStarted += OnAttack;
        if (scanner != null) scanner.Interacted += OnInteract;
    }

    private void OnDisable()
    {
        if (vitals != null) { vitals.Changed -= OnHealthChanged; vitals.Died -= OnDied; }
        if (combat != null) combat.AttackStarted -= OnAttack;
        if (scanner != null) scanner.Interacted -= OnInteract;
    }

    private void OnAttack(bool lunge)
    {
        // Chuỗi 2 nhát: bấm tiếp trong cửa sổ combo thì đổi clip.
        attackIndex = Time.time - lastAttackTime <= comboWindow ? 1 - attackIndex : 0;
        lastAttackTime = Time.time;
        SetInt(AttackIndexHash, lunge ? 1 : attackIndex);
        SetTrigger(AttackHash);
    }

    private void OnHealthChanged()
    {
        float health = vitals.CurrentHealth;
        if (health < lastHealth && health > 0f) SetTrigger(HitHash);
        if (health > 0f) SetBool(DeadHash, false);
        lastHealth = health;
    }

    private void OnDied() => SetBool(DeadHash, true);

    private void OnInteract() => SetTrigger(InteractHash);

    private bool Has(int hash, AnimatorControllerParameterType type)
    {
        if (animator.runtimeAnimatorController == null) return false;
        foreach (AnimatorControllerParameter p in animator.parameters)
            if (p.nameHash == hash && p.type == type) return true;
        return false;
    }

    private void SetTrigger(int hash) { if (Has(hash, AnimatorControllerParameterType.Trigger)) animator.SetTrigger(hash); }
    private void SetBool(int hash, bool v) { if (Has(hash, AnimatorControllerParameterType.Bool)) animator.SetBool(hash, v); }
    private void SetInt(int hash, int v) { if (Has(hash, AnimatorControllerParameterType.Int)) animator.SetInteger(hash, v); }
}
