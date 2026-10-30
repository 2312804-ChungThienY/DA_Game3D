using TMPro;
using UnityEngine;

public class PlayerStaminaHUD : MonoBehaviour
{
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private TMP_Text staminaText;

    private void Awake()
    {
        if (stamina == null || staminaText == null)
        {
            Debug.LogError($"{name}: Thiếu Stamina hoặc Stamina Text.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (stamina != null) stamina.Changed += Refresh;
    }

    private void Start() => Refresh();

    private void OnDisable()
    {
        if (stamina != null) stamina.Changed -= Refresh;
    }

    private void Refresh()
    {
        staminaText.text =
            $"Thể lực: {Mathf.CeilToInt(stamina.CurrentStamina)}/" +
            $"{Mathf.CeilToInt(stamina.MaxStamina)}";
    }
}
