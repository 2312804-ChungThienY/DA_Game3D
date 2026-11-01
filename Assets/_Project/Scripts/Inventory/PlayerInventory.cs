using System;
using System.Collections.Generic;
using UnityEngine;

public struct InventoryAddResult
{
    public int Added;
    public int Remaining;
    public InventoryAddResult(int added, int remaining) { Added = added; Remaining = remaining; }
}

[DisallowMultipleComponent]
public class PlayerInventory : MonoBehaviour
{
    private List<InventorySlotData> slots = InventoryRules.EmptySlots();
    public event Action Changed;
    public int UsedSlots
    {
        get
        {
            int count = 0;
            foreach (InventorySlotData slot in slots) if (slot.quantity > 0) count++;
            return count;
        }
    }

    public int Count(string id)
    {
        int result = 0;
        foreach (InventorySlotData slot in slots) if (slot.itemId == id) result += slot.quantity;
        return result;
    }

    public bool CanAddOne(string id)
    {
        int limit = InventoryRules.MaxStack(id);
        if (limit == 0) return false;
        foreach (InventorySlotData slot in slots)
            if (string.IsNullOrEmpty(slot.itemId) || (slot.itemId == id && slot.quantity < limit))
                return true;
        return false;
    }

    public InventoryAddResult TryAdd(string id, int quantity)
    {
        if (quantity <= 0 || InventoryRules.MaxStack(id) == 0)
            return new InventoryAddResult(0, System.Math.Max(0, quantity));
        List<InventorySlotData> draft = InventoryRules.Clone(slots);
        int accepted = InventoryRules.AddToDraft(draft, id, quantity);
        if (accepted > 0)
        {
            slots = draft;
            Changed?.Invoke();
        }
        return new InventoryAddResult(accepted, quantity - accepted);
    }

    public bool TryCollectSinglePickup(string pickupId, string itemId, WorldStateStore world)
    {
        if (world == null || !InventoryRules.IsValidPickupId(pickupId) ||
            world.HasCollectedPickup(pickupId)) return false;
        List<InventorySlotData> draft = InventoryRules.Clone(slots);
        if (InventoryRules.AddToDraft(draft, itemId, 1) != 1) return false;

        List<InventorySlotData> previous = slots;
        slots = draft;
        // No yield or external call occurs between the capacity check and commit.
        if (!world.TryMarkPickupCollected(pickupId))
        {
            slots = previous;
            return false;
        }
        Changed?.Invoke();
        return true;
    }

    public bool TryUseHealthPotion(PlayerVitals vitals)
    {
        if (vitals == null || !vitals.isActiveAndEnabled || vitals.IsDead ||
            vitals.CurrentHealth >= vitals.MaxHealth || Time.timeScale == 0f) return false;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlotData slot = slots[i];
            if (slot.itemId != InventoryRules.HealthPotion || slot.quantity < 1) continue;
            InventorySlotData previous = new InventorySlotData(slot.itemId, slot.quantity);
            slot.quantity--;
            if (slot.quantity == 0) slot.itemId = string.Empty;

            if (!vitals.TryHeal(InventoryRules.PotionHealing))
            {
                slots[i] = previous;
                return false;
            }
            Changed?.Invoke();
            return true;
        }
        return false;
    }

    // Only the camp interaction calls this in the current gameplay flow.
    public PotionCraftResult TryCraftHealthPotion()
    {
        PotionCraftResult result = HealthPotionCrafting.BuildDraft(slots, out List<InventorySlotData> draft);
        if (result != PotionCraftResult.Success) return result;
        slots = draft;
        Changed?.Invoke();
        return PotionCraftResult.Success;
    }

    public List<InventorySlotData> ExportSlots() => InventoryRules.Clone(slots);

    public bool RestoreSlots(List<InventorySlotData> saved, bool notify = true)
    {
        if (!InventoryRules.IsValidSlots(saved)) return false;
        slots = InventoryRules.Clone(saved);
        if (notify) NotifyChanged();
        return true;
    }

    public void NotifyChanged() => Changed?.Invoke();

#if UNITY_EDITOR
    [ContextMenu("Test/Fill 12 Slots - Play Mode Only")]
    private void TestFillBag()
    {
        if (!Application.isPlaying) return;
        slots = InventoryRules.EmptySlots();
        InventoryRules.AddToDraft(slots, InventoryRules.Herb, 120);
        Changed?.Invoke();
    }
#endif
}
