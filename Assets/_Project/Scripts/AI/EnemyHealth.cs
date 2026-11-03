using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyDefinition definition;

    private EnemyLanternReceiver lanternReceiver;

    // Địch đang bật trong scene (khóa mục tiêu dùng, tránh Find trong Update).
    private static readonly List<EnemyHealth> active = new List<EnemyHealth>();
    public static IReadOnlyList<EnemyHealth> Active => active;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => active.Clear();

    private void OnEnable() { if (!active.Contains(this)) active.Add(this); }

    private void OnDisable() => active.Remove(this);

    public float Current { get; private set; }
    public float Max => definition != null ? definition.maxHealth : 1f;
    public bool IsDead => Current <= 0f;

    public event Action Changed;
    // Trùm lọc sát thương (bất tử khi bay, trần sát thương khi choáng). Trả về 0 thì không nhận.
    public Func<float, float> DamageFilter;
    public event Action<EnemyHealth> Died;

    private void Awake()
    {
        lanternReceiver = GetComponent<EnemyLanternReceiver>();
        if (definition == null)
        {
            Debug.LogError($"{name}: EnemyHealth thiếu Definition.", this);
            enabled = false;
            return;
        }
        Current = definition.maxHealth;
    }

    public bool TryTakeDamage(DamageInfo damage)
    {
        if (!isActiveAndEnabled || IsDead || Time.timeScale == 0f ||
            damage.SourceTeam != DamageTeam.Player ||
            float.IsNaN(damage.Amount) || float.IsInfinity(damage.Amount) || damage.Amount <= 0f)
            return false;

        float multiplier = lanternReceiver != null ? lanternReceiver.DamageMultiplier : 1f;
        float amount = damage.Amount * multiplier;
        if (DamageFilter != null) amount = DamageFilter(amount);
        if (amount <= 0f) return false;
        Current = Mathf.Max(0f, Current - amount);
        Changed?.Invoke();
        if (IsDead) Died?.Invoke(this);
        return true;
    }

    public void ResetToFull()
    {
        Current = Max;
        Changed?.Invoke();
    }
}
