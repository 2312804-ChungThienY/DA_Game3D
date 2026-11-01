using System;
using UnityEngine;
using StarterAssets;

[DisallowMultipleComponent]
public class PlayerVitals : MonoBehaviour, IDamageable, IDodgeVitals
{
    [SerializeField, Min(1f)] private float maxHealth = 100f;
    [SerializeField] private DamageTeam team = DamageTeam.Player;

    private float currentHealth;
    private bool manualInvulnerability;
    private float dodgeInvulnerableFrom = float.PositiveInfinity;
    private float dodgeInvulnerableUntil = float.NegativeInfinity;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0f;

    // Existing callers can still assign IsInvulnerable for other abilities.
    // Dodge uses a separate interval evaluated at the instant damage arrives.
    public bool IsInvulnerable
    {
        get => manualInvulnerability ||
               (Time.time >= dodgeInvulnerableFrom &&
                Time.time < dodgeInvulnerableUntil);
        set => manualInvulnerability = value;
    }

    public event Action Changed;
    public event Action Died;

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        currentHealth = maxHealth;
        manualInvulnerability = false;
        ClearDodgeInvulnerability();
    }

    public void SetDodgeInvulnerability(float startsAt, float endsAt)
    {
        if (IsDead || endsAt <= startsAt)
        {
            ClearDodgeInvulnerability();
            return;
        }

        dodgeInvulnerableFrom = startsAt;
        dodgeInvulnerableUntil = endsAt;
    }

    public void ClearDodgeInvulnerability()
    {
        dodgeInvulnerableFrom = float.PositiveInfinity;
        dodgeInvulnerableUntil = float.NegativeInfinity;
    }

    public void RestoreFull()
    {
        currentHealth = maxHealth;
        manualInvulnerability = false;
        ClearDodgeInvulnerability();
        Changed?.Invoke();
    }

    public bool TryTakeDamage(DamageInfo damage)
    {
        if (!isActiveAndEnabled || Time.timeScale == 0f ||
            IsDead || IsInvulnerable)
        {
            return false;
        }

        if (float.IsNaN(damage.Amount) || float.IsInfinity(damage.Amount) ||
            damage.Amount <= 0f)
        {
            return false;
        }

        if (damage.SourceTeam != DamageTeam.Neutral && damage.SourceTeam == team)
        {
            return false;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage.Amount);
        Changed?.Invoke();

        if (IsDead)
        {
            manualInvulnerability = false;
            ClearDodgeInvulnerability();
            Died?.Invoke();
        }

        return true;
    }
}
