using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class TrainingDummy : MonoBehaviour, IDamageable
{
    [SerializeField, Min(1f)] private float maxHealth = 60f;
    [SerializeField] private TMP_Text statusText;

    private float health;

    private void Awake()
    {
        maxHealth = Mathf.Max(1f, maxHealth);
        health = maxHealth;
        RefreshUI();
    }

    public bool TryTakeDamage(DamageInfo damage)
    {
        if (!isActiveAndEnabled || health <= 0f ||
            damage.SourceTeam == DamageTeam.Enemy ||
            float.IsNaN(damage.Amount) || float.IsInfinity(damage.Amount) ||
            damage.Amount <= 0f) return false;

        health = Mathf.Max(0f, health - damage.Amount);
        RefreshUI();
        Debug.Log($"Hình nộm nhận {damage.Amount} sát thương. Còn {health} máu.", this);
        if (health <= 0f) gameObject.SetActive(false);
        return true;
    }

    private void RefreshUI()
    {
        if (statusText != null)
            statusText.text = $"Hình nộm: {health:0}/{maxHealth:0}";
    }
}
