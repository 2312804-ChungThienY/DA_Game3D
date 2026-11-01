using System.Collections.Generic;

public enum PotionCraftResult { Success, MissingIngredients, NoSpace }

// Pure transaction: never mutates the source, including on failure.
public static class HealthPotionCrafting
{
    public static PotionCraftResult BuildDraft(List<InventorySlotData> source,
        out List<InventorySlotData> draft)
    {
        draft = null;
        if (!InventoryRules.IsValidSlots(source)) return PotionCraftResult.MissingIngredients;
        List<InventorySlotData> candidate = InventoryRules.Clone(source);
        if (!RemoveOne(candidate, InventoryRules.Herb) ||
            !RemoveOne(candidate, InventoryRules.Mushroom))
            return PotionCraftResult.MissingIngredients;
        if (InventoryRules.AddToDraft(candidate, InventoryRules.HealthPotion, 1) != 1)
            return PotionCraftResult.NoSpace;
        draft = candidate;
        return PotionCraftResult.Success;
    }

    private static bool RemoveOne(List<InventorySlotData> slots, string itemId)
    {
        // Prefer the smallest stack so a single remaining ingredient frees its slot.
        InventorySlotData selected = null;
        foreach (InventorySlotData slot in slots)
            if (slot.itemId == itemId && slot.quantity > 0 &&
                (selected == null || slot.quantity < selected.quantity)) selected = slot;
        if (selected == null) return false;
        selected.quantity--;
        if (selected.quantity == 0) selected.itemId = string.Empty;
        return true;
    }
}
