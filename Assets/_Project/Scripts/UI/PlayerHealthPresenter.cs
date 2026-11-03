using TMPro;
using UnityEngine;

// Hiển thị máu trên HUD. Màn chết và khóa điều khiển do GameSession xử lý.
public class PlayerHealthPresenter : MonoBehaviour
{
    [SerializeField] private PlayerVitals vitals;
    [SerializeField] private TMP_Text healthText;

    private void Awake()
    {
        if (vitals == null || healthText == null)
        {
            Debug.LogError($"{name}: Thiếu tham chiếu cho HUD máu.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (vitals != null) vitals.Changed += Refresh;
    }

    private void Start() => Refresh();

    private void OnDisable()
    {
        if (vitals != null) vitals.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (!enabled) return;
        healthText.text = $"Máu: {Mathf.CeilToInt(vitals.CurrentHealth)}/{Mathf.CeilToInt(vitals.MaxHealth)}";
    }
}
