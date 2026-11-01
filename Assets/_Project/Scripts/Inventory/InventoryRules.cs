using System.Collections.Generic;

public static class InventoryRules
{
    public const int SlotCount = 12;
    public const string Herb = "item_la_cam";
    public const string Mushroom = "item_linh_chi";
    public const string HealthPotion = "item_thuoc_mau";
    public const float PotionHealing = 35f;

    public static int MaxStack(string id)
    {
        if (id == Herb || id == Mushroom) return 10;
        return id == HealthPotion ? 5 : 0;
    }

    public static string DisplayName(string id)
    {
        if (id == Herb) return "Lá cẩm";
        if (id == Mushroom) return "Linh chi";
        return id == HealthPotion ? "Thuốc máu" : "Trống";
    }

    public static bool IsValidPickupId(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || id.Length > 96) return false;
        foreach (char c in id)
            if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                  (c >= '0' && c <= '9') || c == '_' || c == '-')) return false;
        return true;
    }

    public static List<InventorySlotData> EmptySlots()
    {
        List<InventorySlotData> result = new List<InventorySlotData>(SlotCount);
        for (int i = 0; i < SlotCount; i++) result.Add(new InventorySlotData());
        return result;
    }

    public static bool IsValidSlots(List<InventorySlotData> slots)
    {
        if (slots == null || slots.Count != SlotCount) return false;
        foreach (InventorySlotData slot in slots)
        {
            if (slot == null) return false;
            if (string.IsNullOrEmpty(slot.itemId))
            {
                if (slot.quantity != 0) return false;
            }
            else if (MaxStack(slot.itemId) == 0 || slot.quantity < 1 || slot.quantity > MaxStack(slot.itemId))
                return false;
        }
        return true;
    }

    public static List<InventorySlotData> Clone(List<InventorySlotData> slots)
    {
        List<InventorySlotData> result = new List<InventorySlotData>(slots.Count);
        foreach (InventorySlotData slot in slots)
            result.Add(new InventorySlotData(slot.itemId ?? string.Empty, slot.quantity));
        return result;
    }

    // Mutates only the supplied draft. Returns accepted quantity.
    public static int AddToDraft(List<InventorySlotData> slots, string id, int quantity)
    {
        int limit = MaxStack(id);
        if (limit == 0 || quantity <= 0) return 0;
        int remaining = quantity;
        foreach (InventorySlotData slot in slots)
        {
            if (slot.itemId != id || slot.quantity >= limit) continue;
            int accepted = System.Math.Min(limit - slot.quantity, remaining);
            slot.quantity += accepted;
            remaining -= accepted;
            if (remaining == 0) return quantity;
        }
        foreach (InventorySlotData slot in slots)
        {
            if (!string.IsNullOrEmpty(slot.itemId)) continue;
            int accepted = System.Math.Min(limit, remaining);
            slot.itemId = id;
            slot.quantity = accepted;
            remaining -= accepted;
            if (remaining == 0) break;
        }
        return quantity - remaining;
    }
}
