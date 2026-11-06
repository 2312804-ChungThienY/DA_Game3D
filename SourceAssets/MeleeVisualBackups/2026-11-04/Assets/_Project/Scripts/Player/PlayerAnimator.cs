using System.Collections.Generic;
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
    private Vector3 skeletonPosition;
    private Quaternion skeletonRotation;
    private readonly Dictionary<int, AnimatorControllerParameterType> parameters = new Dictionary<int, AnimatorControllerParameterType>();

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
        foreach (var parameter in animator.parameters) parameters[parameter.nameHash] = parameter.type;
        if (skeleton != null) { skeletonPosition = skeleton.localPosition; skeletonRotation = skeleton.localRotation; }
    }

    // Phần thân xoay bằng code (chỉ hình ảnh, không ảnh hưởng collider/motor).
    private void LateUpdate()
    {
        bool dead = vitals != null && vitals.IsDead;
        animator.updateMode = dead ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
        if (lantern != null && lanternLayer >= 0)
            animator.SetLayerWeight(lanternLayer, Mathf.MoveTowards(animator.GetLayerWeight(lanternLayer),
                lantern.IsLanternOn && !dead && (dodge == null || !dodge.IsDodging) ? 1f : 0f, (dead ? Time.unscaledDeltaTime : Time.deltaTime) * 6f));

        if (skeleton == null) return;
        deathTilt = Mathf.MoveTowards(deathTilt, dead ? 1f : 0f, (dead ? Time.unscaledDeltaTime : Time.deltaTime) * (dead ? 1.5f : 10f));
        float pitch = dodge != null && dodge.IsDodging ? dodge.RollProgress * 360f : deathTilt * deathTiltAngle;
        Quaternion r = Quaternion.Euler(pitch, 0f, 0f);
        skeleton.localRotation = skeletonRotation * r;
        skeleton.localPosition = skeletonPosition + bodyPivot - r * bodyPivot;
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

    public void ResetPose()
    {
        deathTilt = 0f; lastAttackTime = float.NegativeInfinity;
        animator.updateMode = AnimatorUpdateMode.Normal;
        foreach (var parameter in parameters)
            if (parameter.Value == AnimatorControllerParameterType.Trigger) animator.ResetTrigger(parameter.Key);
        if (skeleton != null) { skeleton.localPosition = skeletonPosition; skeleton.localRotation = skeletonRotation; }
        if (animator.runtimeAnimatorController != null) { animator.Rebind(); animator.Update(0f); }
        SetBool(DeadHash, vitals != null && vitals.IsDead);
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
        return parameters.TryGetValue(hash, out var actual) && actual == type;
    }

    private void SetTrigger(int hash) { if (Has(hash, AnimatorControllerParameterType.Trigger)) animator.SetTrigger(hash); }
    private void SetBool(int hash, bool v) { if (Has(hash, AnimatorControllerParameterType.Bool)) animator.SetBool(hash, v); }
    private void SetInt(int hash, int v) { if (Has(hash, AnimatorControllerParameterType.Int)) animator.SetInteger(hash, v); }
}
