using System;
using System.Collections.Generic;
using UnityEngine;

// Đòn dao của An: thời gian logic do script giữ, không phụ thuộc Animation Event.
[DefaultExecutionOrder(-40)]
[DisallowMultipleComponent]
public class CombatController : MonoBehaviour
{
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private PlayerVitals vitals;
    [SerializeField] private GameObject slashVisual;

    [Header("Đòn dao")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float attackDuration = 0.65f;
    [SerializeField] private float activeStart = 0.15f;
    [SerializeField] private float activeEnd = 0.30f;
    [SerializeField] private float range = 1.8f;
    [SerializeField] private float radius = 0.6f;
    [SerializeField] private float staminaCost = 10f;
    [SerializeField] private LayerMask targetMask = (1 << 7) | (1 << 9);

    [Header("Lộn + chém (Quà Kim Quy)")]
    [SerializeField] private float lungeWindow = 0.25f;
    [SerializeField] private float lungeMultiplier = 1.25f;
    [SerializeField] private float lungeSpeed = 6f;
    [SerializeField] private float lungeDuration = 0.15f;

    private readonly HashSet<IDamageable> hitThisAttack = new HashSet<IDamageable>();
    private readonly Collider[] overlap = new Collider[16];
    private InputReader input;
    private float attackTime = -1f;
    private int attackCount;
    private float currentDamage;
    private float bufferedAttackTime = float.NegativeInfinity;
    private DodgeController dodge;
    private PlayerMotor motor;
    private PlayerActionState actions;
    private Vector3 attackForward;
    private int inputRevision;

    public bool IsAttacking => attackTime >= 0f;
    // Bắt đầu một đòn (true nếu là đòn lộn + chém); Animator chỉ hiển thị.
    public event Action<bool> AttackStarted;
    public bool IsInDamageWindow => IsAttacking && attackTime >= activeStart && attackTime < activeEnd;
    public bool LastAttackWasLunge { get; private set; }
    public int LungeCount { get; private set; }

    private void Awake()
    {
        if (stamina == null) stamina = GetComponent<PlayerStamina>();
        if (vitals == null) vitals = GetComponent<PlayerVitals>();
        dodge = GetComponent<DodgeController>();
        motor = GetComponent<PlayerMotor>();
        actions = GetComponent<PlayerActionState>();
        if (slashVisual != null) slashVisual.SetActive(false);
    }

    private void Start()
    {
        input = AppRoot.Instance != null ? AppRoot.Instance.Input : null;
        if (input == null || !input.IsReady)
        {
            Debug.LogError($"{name}: CombatController không tìm thấy InputReader.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (inputRevision != input.Revision || !input.GameplayActive)
        {
            bufferedAttackTime = float.NegativeInfinity;
            inputRevision = input.Revision;
        }
        if (Time.deltaTime <= 0f || !input.GameplayActive) return;

        if (vitals != null && vitals.IsDead)
        {
            CancelAttack();
            return;
        }

        bool pressed = input.WasPressed(input.Attack);
        if (pressed && IsAttacking && attackDuration - attackTime <= 0.16f) bufferedAttackTime = Time.time;

        // Đang lộn: ghi nhớ lần bấm để đâm tới ngay khi lộn xong.
        if (dodge != null && dodge.IsDodging)
        {
            if (pressed) bufferedAttackTime = Time.time;
            return;
        }

        if (!IsAttacking)
        {
            bool afterRoll = dodge != null && Time.time - dodge.LastRollEndTime <= lungeWindow;
            bool buffered = Time.time - bufferedAttackTime <= (afterRoll ? lungeWindow : 0.16f);
            if ((pressed || buffered) && actions.CanAttack && (stamina == null || stamina.TrySpend(staminaCost)))
            {
                bufferedAttackTime = float.NegativeInfinity;
                motor.FaceDirection(motor.ReadAttackDirection());
                attackForward = transform.forward;
                LastAttackWasLunge = afterRoll;
                currentDamage = afterRoll ? damage * lungeMultiplier : damage;
                if (afterRoll)
                {
                    LungeCount++;
                    if (motor != null) motor.BeginDash(transform.forward * lungeSpeed, lungeDuration);
                }
                attackTime = 0f;
                attackCount++;
                hitThisAttack.Clear();
                AttackStarted?.Invoke(afterRoll);
            }
            return;
        }

        // Xét giao giữa khoảng thời gian của frame và cửa sổ sát thương: FPS thấp vẫn trúng.
        float previous = attackTime;
        attackTime += Time.deltaTime;
        bool overlapsWindow = previous < activeEnd && attackTime > activeStart;
        if (slashVisual != null) slashVisual.SetActive(attackTime > activeStart && previous < activeEnd);
        if (overlapsWindow) ApplyHits();

        if (attackTime >= attackDuration)
        {
            float queued = bufferedAttackTime;
            CancelAttack();
            bufferedAttackTime = queued;
        }
    }

    private void ApplyHits()
    {
        Vector3 start = transform.position + Vector3.up * 1f + attackForward * 0.3f;
        Vector3 end = start + attackForward * (range - 0.3f);
        int count = Physics.OverlapCapsuleNonAlloc(start, end, radius, overlap, targetMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            IDamageable target = overlap[i].GetComponentInParent<IDamageable>();
            if (target == null || ReferenceEquals(target, vitals) || hitThisAttack.Contains(target)) continue;
            Vector3 origin = transform.position + Vector3.up;
            Vector3 point = overlap[i].ClosestPoint(origin);
            if (Physics.Linecast(origin, point, (1 << 0) | (1 << 3), QueryTriggerInteraction.Ignore)) continue;
            hitThisAttack.Add(target);

            // Một đòn chỉ trúng mỗi mục tiêu một lần dù có nhiều collider.
            target.TryTakeDamage(new DamageInfo(gameObject, currentDamage, DamageTeam.Player)
            {
                AttackId = $"dao_{attackCount}"
            });
        }
    }

    public void CancelAttack()
    {
        attackTime = -1f;
        bufferedAttackTime = float.NegativeInfinity;
        hitThisAttack.Clear();
        if (slashVisual != null) slashVisual.SetActive(false);
    }

    private void OnDisable() => CancelAttack();

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.5f);
        Vector3 forward = IsAttacking ? attackForward : transform.forward;
        Vector3 start = transform.position + Vector3.up * 1f + forward * 0.3f;
        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(start + forward * (range - 0.3f), radius);
    }
}
