using System;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyVitals : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1f)] private float maxHealth = 60f;
    [SerializeField] private TMP_Text statusText;

    private float health;
    private EnemyLanternWeakness lanternWeakness;

    public float CurrentHealth => health;
    public bool IsDead => health <= 0f;
    public event Action Died;

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        health = maxHealth;
        lanternWeakness = GetComponent<EnemyLanternWeakness>();
        RefreshUI();
    }

    public bool TryTakeDamage(DamageInfo damage)
    {
        if (!isActiveAndEnabled || Time.timeScale == 0f || IsDead ||
            damage.SourceTeam == DamageTeam.Enemy ||
            float.IsNaN(damage.Amount) || float.IsInfinity(damage.Amount) ||
            damage.Amount <= 0f)
            return false;

        float multiplier = lanternWeakness != null
            ? lanternWeakness.DamageMultiplier : 1f;
        float finalDamage = damage.Amount * multiplier;
        health = Mathf.Max(0f, health - finalDamage);
        RefreshUI();
        Debug.Log($"{name} nhận {finalDamage:0.##} sát thương. Còn {health:0.##} máu.", this);

        if (IsDead)
        {
            Died?.Invoke();
            gameObject.SetActive(false);
        }
        return true;
    }

    private void RefreshUI()
    {
        if (statusText != null)
            statusText.text = $"{name}: {health:0}/{maxHealth:0}";
    }
}
