using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Son Doong/Inventory/Item")]
public class ItemDefinition : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    public string ItemId => itemId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? InventoryRules.DisplayName(itemId) : displayName;
    public Sprite Icon => icon;
    public int MaxStack => InventoryRules.MaxStack(itemId);
    public bool IsValid => MaxStack > 0;
}
