using System.Text;
using TMPro;
using UnityEngine;

public class InventoryHUD : MonoBehaviour
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private TMP_Text inventoryText;
    [SerializeField] private TMP_Text potionText;
    [SerializeField] private ItemDefinition[] definitions;

    private void Awake()
    {
        if (inventory == null || inventoryText == null)
        {
            Debug.LogError($"{name}: Assign inventory and text.", this);
            enabled = false;
        }
    }
    private void OnEnable() { if (inventory != null) inventory.Changed += Refresh; }
    private void Start() => Refresh();
    private void OnDisable() { if (inventory != null) inventory.Changed -= Refresh; }

    private string DisplayName(string id)
    {
        if (definitions != null)
            foreach (ItemDefinition definition in definitions)
                if (definition != null && definition.ItemId == id) return definition.DisplayName;
        return InventoryRules.DisplayName(id);
    }

    private void Refresh()
    {
        if (inventory == null || inventoryText == null) return;
        StringBuilder text = new StringBuilder($"TÚI ĐỒ ({inventory.UsedSlots}/12)\n");
        var slots = inventory.ExportSlots();
        for (int i = 0; i < slots.Count; i++)
            text.AppendLine(slots[i].quantity == 0 ? $"{i + 1:00}. --" :
                $"{i + 1:00}. {DisplayName(slots[i].itemId)} x{slots[i].quantity}");
        inventoryText.text = text.ToString();
        if (potionText != null) potionText.text = $"[Q] Thuốc: {inventory.Count(InventoryRules.HealthPotion)}";
    }
}
