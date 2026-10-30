using System;
using UnityEngine;

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

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
        IsInvulnerable = false;
    }

    public bool TryTakeDamage(DamageInfo damage)
    {
        if (!isActiveAndEnabled || Time.timeScale == 0f || IsDead || IsInvulnerable ||
            float.IsNaN(damage.Amount) || float.IsInfinity(damage.Amount) ||
            damage.Amount <= 0f ||
            (damage.SourceTeam != DamageTeam.Neutral && damage.SourceTeam == team))
            return false;

        currentHealth = Mathf.Max(0f, currentHealth - damage.Amount);
        Changed?.Invoke();

        if (IsDead)
        {
            IsInvulnerable = false;
            Died?.Invoke();
        }

        return true;
    }
}
