#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CraftingContractChecks
{
    [MenuItem("Son Doong/Tests/Check potion crafting")]
    public static void Run()
    {
        var source = InventoryRules.EmptySlots();
        InventoryRules.AddToDraft(source, InventoryRules.Herb, 1);
        InventoryRules.AddToDraft(source, InventoryRules.Mushroom, 1);
        Check(source, PotionCraftResult.Success, 0, 0, 1);
        Check(InventoryRules.EmptySlots(), PotionCraftResult.MissingIngredients, 0, 0, 0);
        var herbOnly = InventoryRules.EmptySlots();
        InventoryRules.AddToDraft(herbOnly, InventoryRules.Herb, 2);
        Check(herbOnly, PotionCraftResult.MissingIngredients, 0, 0, 0);
        Check(Full(2, 2, 5), PotionCraftResult.NoSpace, 0, 0, 0);
        Check(Full(1, 2, 5), PotionCraftResult.Success, 0, 1, 51);
        Check(Full(2, 2, 4), PotionCraftResult.Success, 1, 1, 41);
        var split = Full(10, 10, 5);
        split[2] = new InventorySlotData(InventoryRules.Herb, 1);
        Check(split, PotionCraftResult.Success, 10, 9, 46);
        Debug.Log("PASS: 7 crafting contracts; source unchanged on success/failure.");
    }

    private static List<InventorySlotData> Full(int herb, int mushroom, int potion)
    {
        var slots = InventoryRules.EmptySlots();
        slots[0] = new InventorySlotData(InventoryRules.Herb, herb);
        slots[1] = new InventorySlotData(InventoryRules.Mushroom, mushroom);
        for (int i = 2; i < slots.Count; i++)
            slots[i] = new InventorySlotData(InventoryRules.HealthPotion, potion);
        return slots;
    }

    private static int Count(List<InventorySlotData> slots, string id)
    {
        int total = 0;
        foreach (var slot in slots) if (slot.itemId == id) total += slot.quantity;
        return total;
    }

    private static void Check(List<InventorySlotData> source, PotionCraftResult expected,
        int herbs, int mushrooms, int potions)
    {
        string before = JsonUtility.ToJson(new Wrapper { slots = source });
        var result = HealthPotionCrafting.BuildDraft(source, out var draft);
        Require(result == expected, "Unexpected result");
        Require(before == JsonUtility.ToJson(new Wrapper { slots = source }), "Source mutated");
        if (result != PotionCraftResult.Success)
        {
            Require(draft == null, "Failure exposed draft");
            return;
        }
        Require(InventoryRules.IsValidSlots(draft), "Invalid output slots");
        Require(Count(draft, InventoryRules.Herb) == herbs, "Wrong herb count");
        Require(Count(draft, InventoryRules.Mushroom) == mushrooms, "Wrong mushroom count");
        Require(Count(draft, InventoryRules.HealthPotion) == potions, "Wrong potion count");
        draft[0].quantity = 999;
        Require(before == JsonUtility.ToJson(new Wrapper { slots = source }), "Shared slot references");
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException("Crafting contract: " + message);
    }
    [Serializable] private class Wrapper { public List<InventorySlotData> slots; }
}
#endif
