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
    private bool granting;

    private void Awake()
    {
        if (worldState == null || inventory == null)
        {
            Debug.LogError($"{name}: RewardService thiếu WorldState hoặc Inventory.", this);
            enabled = false;
        }
    }

    public bool TryGrant(List<RewardEntry> rewards, out string failReason, string pickupClaim = null)
    {
        failReason = null;
        if (!isActiveAndEnabled || granting || worldState==null || inventory==null) { failReason = "Chưa thể nhận thưởng"; return false; }
        if(rewards==null||rewards.Count==0){failReason="Phần thưởng không hợp lệ";return false;}
        if(pickupClaim!=null&&(!WorldIds.IsKnown(StateSet.Pickups,pickupClaim)||worldState.Has(StateSet.Pickups,pickupClaim)))
        {failReason="Bệ đã trao thưởng hoặc ID không hợp lệ";return false;}
        var items=new Dictionary<string,int>(StringComparer.Ordinal);
        var states=new List<StateRequirement>();
        foreach (RewardEntry r in rewards)
        {
            if(r==null||string.IsNullOrEmpty(r.id)||r.quantity<=0){failReason="Phần thưởng không hợp lệ";return false;}
            if(r.type==RewardType.Item)
            {
                long total=(items.TryGetValue(r.id,out int previous)?previous:0)+(long)r.quantity;
                if(total>int.MaxValue){failReason="Số lượng không hợp lệ";return false;}
                items[r.id]=(int)total;continue;
            }
            StateSet set;
            switch (r.type)
            {
                case RewardType.Fragment:set=StateSet.Fragments;break;
                case RewardType.BossTool:set=StateSet.BossTools;break;
                case RewardType.Ability:set=StateSet.Abilities;break;
                case RewardType.Flag:set=StateSet.Flags;break;
                default:failReason="Loại thưởng không hợp lệ";return false;
            }
            if(r.quantity!=1||!WorldIds.IsKnown(set,r.id)){failReason="ID phần thưởng không hợp lệ";return false;}
            states.Add(new StateRequirement{set=set,id=r.id});
        }
        var additions=new List<KeyValuePair<string,int>>(items);
        if(!inventory.PrepareAdditions(additions,out var draft,out failReason))return false;
        if(pickupClaim!=null)states.Add(new StateRequirement{set=StateSet.Pickups,id=pickupClaim});
        granting=true;
        try
        {
            if(!worldState.TryAddBatch(states,out bool changed)){failReason="ID phần thưởng không hợp lệ";return false;}
            inventory.CommitAdditions(draft);
            // Every observer now sees the complete reward, including its claim.
            if(items.Count>0)inventory.PublishChange();
            if(changed)worldState.PublishChange();
            return true;
        }
        finally{granting=false;}
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
