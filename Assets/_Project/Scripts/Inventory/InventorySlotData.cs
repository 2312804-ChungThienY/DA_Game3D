using System;

[Serializable]
public class InventorySlotData
{
    public string itemId = string.Empty;
    public int quantity;

    public InventorySlotData() { }
    public InventorySlotData(string id, int count) { itemId = id; quantity = count; }
}
