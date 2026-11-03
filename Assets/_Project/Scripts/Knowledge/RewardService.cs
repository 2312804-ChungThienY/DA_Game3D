using System;
using System.Collections.Generic;
using UnityEngine;

public enum RewardType { Fragment, BossTool, Item, Ability, Flag }

[Serializable]
public class RewardEntry
{
    public RewardType type;
    public string id;
    public int quantity = 1;
}

// Trao phần thưởng theo giao dịch: kiểm tra chỗ trong túi cho mọi Item trước; thiếu chỗ thì không trao gì.
public class RewardService : MonoBehaviour
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private InventoryService inventory;

    private void Awake()
    {
        if (worldState == null || inventory == null)
        {
            Debug.LogError($"{name}: RewardService thiếu WorldState hoặc Inventory.", this);
            enabled = false;
        }
    }

    public bool TryGrant(List<RewardEntry> rewards, out string failReason)
    {
        failReason = null;
        if (!enabled) { failReason = "Lỗi cấu hình"; return false; }

        // Gộp các Item theo ID để kiểm tra chỗ chính xác.
        Dictionary<string, int> items = new Dictionary<string, int>();
        foreach (RewardEntry r in rewards)
            if (r != null && r.type == RewardType.Item)
                items[r.id] = (items.TryGetValue(r.id, out int q) ? q : 0) + Mathf.Max(1, r.quantity);
        foreach (KeyValuePair<string, int> item in items)
            if (!inventory.CanAdd(item.Key, item.Value)) { failReason = "Túi đầy"; return false; }

        foreach (RewardEntry r in rewards)
        {
            if (r == null) continue;
            switch (r.type)
            {
                case RewardType.Fragment: worldState.TryAdd(StateSet.Fragments, r.id); break;
                case RewardType.BossTool: worldState.TryAdd(StateSet.BossTools, r.id); break;
                case RewardType.Ability: worldState.TryAdd(StateSet.Abilities, r.id); break;
                case RewardType.Flag: worldState.TryAdd(StateSet.Flags, r.id); break;
            }
        }
        foreach (KeyValuePair<string, int> item in items) inventory.TryAdd(item.Key, item.Value);
        return true;
    }

    public static string Describe(List<RewardEntry> rewards, InventoryService inventory)
    {
        List<string> parts = new List<string>();
        foreach (RewardEntry r in rewards)
        {
            if (r == null) continue;
            if (r.type == RewardType.Item)
            {
                ItemDefinition def = inventory != null ? inventory.GetDefinition(r.id) : null;
                parts.Add($"{(def != null ? def.displayName : r.id)} x{Mathf.Max(1, r.quantity)}");
            }
            else parts.Add(RewardNames.Of(r.id));
        }
        return string.Join(", ", parts);
    }
}

// Tên hiển thị của các ID phần thưởng không phải vật phẩm túi.
public static class RewardNames
{
    public static string Of(string id)
    {
        switch (id)
        {
            case "c2_frag_1": return "Mảnh Mặt Trời Lông Chim 1";
            case "c2_frag_2": return "Mảnh Mặt Trời Lông Chim 2";
            case "c2_frag_3": return "Mảnh Mặt Trời Lông Chim 3";
            case "c2_tool_long_gio_lon": return "Lông Gió Lớn";
            case WorldIds.AbilityWind: return "Lông Chim Lạc";
            default: return id;
        }
    }
}
