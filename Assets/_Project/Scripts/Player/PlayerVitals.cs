using System;
using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public class PlayerVitals : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField] private DamageTeam team = DamageTeam.Player;

    private float currentHealth;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;
    public bool IsInvulnerable { get; set; }

    public event Action Changed;
    public event Action Died;
    // Đòn hợp lệ bị bất tử chặn (dùng cho né đúng lúc).
    public event Action<DamageInfo> DamageAvoided;

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
        IsInvulnerable = false;
    }

    public bool TryTakeDamage(DamageInfo damage)
    {
        if (!isActiveAndEnabled || Time.timeScale == 0f || IsDead ||
            float.IsNaN(damage.Amount) || float.IsInfinity(damage.Amount) ||
            damage.Amount <= 0f ||
            (damage.SourceTeam != DamageTeam.Neutral && damage.SourceTeam == team))
            return false;

        if (IsInvulnerable)
        {
            DamageAvoided?.Invoke(damage);
            return false;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage.Amount);
        Changed?.Invoke();

        if (IsDead)
        {
            IsInvulnerable = false;
            Died?.Invoke();
        }

        return true;
    }

    // Rơi vào vùng chết: chết ngay, bỏ qua bất tử.
    public void Kill()
    {
        if (!isActiveAndEnabled || IsDead) return;

        currentHealth = 0f;
        IsInvulnerable = false;
        Changed?.Invoke();
        Died?.Invoke();
    }

    // Hồi máu từ thuốc; không vượt máu trần, không hồi khi đã chết.
    public bool Heal(float amount)
    {
        if (!isActiveAndEnabled || IsDead || float.IsNaN(amount) || float.IsInfinity(amount) ||
            amount <= 0f || currentHealth >= maxHealth)
            return false;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Changed?.Invoke();
        return true;
    }

    // Hồi đầy tại mốc lưu.
    public void RestoreFull()
    {
        currentHealth = maxHealth;
        IsInvulnerable = false;
        Changed?.Invoke();
    }
}
