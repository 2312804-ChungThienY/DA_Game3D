using UnityEngine;

public enum ItemKind { Material, Consumable, Equipment, Fragment, Relic }

// Định nghĩa vật phẩm (spec phần 17). Chỉ Material và Consumable chiếm ô túi.
[CreateAssetMenu(menuName = "Son Doong/Item Definition", fileName = "ID_NewItem")]
public class ItemDefinition : ScriptableObject
{
    public string itemId;
    public string displayName;
    [TextArea] public string description;
    public ItemKind kind;
    [Min(1)] public int maxStack = 1;

    public bool UsesSlot => kind == ItemKind.Material || kind == ItemKind.Consumable;
}
