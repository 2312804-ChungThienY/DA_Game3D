using TMPro;
using UnityEngine;

// HUD dạng thanh: máu, thể lực, năng lượng đèn, số thuốc (spec phần 20, C1-R07).
public class HudBars : MonoBehaviour
{
    [SerializeField] private PlayerVitals vitals;
    [SerializeField] private PlayerStamina stamina;
    [SerializeField] private LanternController lantern;
    [SerializeField] private InventoryService inventory;

    [Header("Thanh (anchorMax.x = tỉ lệ)")]
    [SerializeField] private RectTransform healthFill;
    [SerializeField] private RectTransform staminaFill;
    [SerializeField] private RectTransform lanternFill;
    [SerializeField] private GameObject lanternGroup;

    [Header("Nhãn")]
    [SerializeField] private TMP_Text healthLabel;
    [SerializeField] private TMP_Text staminaLabel;
    [SerializeField] private TMP_Text lanternLabel;
    [SerializeField] private TMP_Text potionText;

    private void Awake()
    {
        if (vitals == null || stamina == null || lantern == null || inventory == null ||
            healthFill == null || staminaFill == null || lanternFill == null || lanternGroup == null ||
            healthLabel == null || staminaLabel == null || lanternLabel == null || potionText == null)
        {
            Debug.LogError($"{name}: HudBars thiếu tham chiếu.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        SetBar(healthFill, healthLabel, "Máu", vitals.CurrentHealth, vitals.MaxHealth);
        SetBar(staminaFill, staminaLabel, "Thể lực", stamina.CurrentStamina, stamina.MaxStamina);

        bool hasLantern = lantern.HasLantern;
        if (lanternGroup.activeSelf != hasLantern) lanternGroup.SetActive(hasLantern);
        if (hasLantern) SetBar(lanternFill, lanternLabel, "Đèn", lantern.Energy, lantern.MaxEnergy);

        string potions = $"[Q] Thuốc máu: {inventory.Count(InventoryService.ThuocMau)}";
        if (potionText.text != potions) potionText.text = potions;
    }

    private static void SetBar(RectTransform fill, TMP_Text label, string name, float value, float max)
    {
        float ratio = max > 0f ? Mathf.Clamp01(value / max) : 0f;
        Vector2 anchor = fill.anchorMax;
        if (!Mathf.Approximately(anchor.x, ratio))
        {
            anchor.x = ratio;
            fill.anchorMax = anchor;
        }

        string text = $"{name} {Mathf.CeilToInt(value)}/{Mathf.CeilToInt(max)}";
        if (label.text != text) label.text = text;
    }
}
