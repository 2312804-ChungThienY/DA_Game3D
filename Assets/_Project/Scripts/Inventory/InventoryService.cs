using System;
using System.Collections.Generic;
using UnityEngine;

// Túi 12 ô. Mọi thay đổi đi qua đây; UI chỉ đọc và gọi lệnh, không tự Remove rồi Add.
public class InventoryService : MonoBehaviour
{
    public const int SlotCount = 12;
    public const string LaCam = "item_la_cam";
    public const string LinhChi = "item_linh_chi";
    public const string ThuocMau = "item_thuoc_mau";

    [SerializeField] private ItemDefinition[] catalog;

    private readonly Dictionary<string, ItemDefinition> byId = new Dictionary<string, ItemDefinition>(StringComparer.Ordinal);
    private Slot[] slots = new Slot[SlotCount];

    public event Action Changed;

    public struct Slot
    {
        public string itemId;
        public int quantity;
        public bool IsEmpty => string.IsNullOrEmpty(itemId) || quantity <= 0;
    }

    private void Awake()
    {
        foreach (ItemDefinition item in catalog ?? Array.Empty<ItemDefinition>())
        {
            if (item == null || string.IsNullOrEmpty(item.itemId) || byId.ContainsKey(item.itemId))
            {
                Debug.LogError($"{name}: catalog vật phẩm có mục rỗng hoặc trùng ID.", this);
                continue;
            }
            byId.Add(item.itemId, item);
        }
    }

    public ItemDefinition GetDefinition(string itemId) =>
        itemId != null && byId.TryGetValue(itemId, out ItemDefinition item) ? item : null;

    public IEnumerable<ItemDefinition> Catalog => byId.Values;

    public Slot GetSlot(int index) => index >= 0 && index < SlotCount ? slots[index] : default;

    public int Count(string itemId)
    {
        int total = 0;
        foreach (Slot slot in slots)
            if (!slot.IsEmpty && slot.itemId == itemId) total += slot.quantity;
        return total;
    }

    public bool CanAdd(string itemId, int quantity)
    {
        Slot[] copy = (Slot[])slots.Clone();
        return AddTo(copy, itemId, quantity) == quantity;
    }

    // Trả về số lượng đã nhận; phần còn lại không vừa túi.
    public int TryAdd(string itemId, int quantity)
    {
        if (quantity <= 0 || GetDefinition(itemId) == null || !GetDefinition(itemId).UsesSlot) return 0;

        int added = AddTo(slots, itemId, quantity);
        if (added > 0) Changed?.Invoke();
        return added;
    }

    public bool TryRemove(string itemId, int quantity)
    {
        if (quantity <= 0 || Count(itemId) < quantity) return false;

        RemoveFrom(slots, itemId, quantity);
        Changed?.Invoke();
        return true;
    }

    // Giao dịch: thử trên bản sao (trừ nguyên liệu trước rồi thêm kết quả), chỉ commit khi mọi bước thành công.
    public bool TryCraft(IList<KeyValuePair<string, int>> inputs, string outputId, int outputQuantity, out string failReason)
    {
        failReason = null;
        Slot[] working = (Slot[])slots.Clone();

        foreach (KeyValuePair<string, int> input in inputs)
        {
            int have = CountIn(working, input.Key);
            if (have < input.Value)
            {
                ItemDefinition def = GetDefinition(input.Key);
                failReason = $"Thiếu {(def != null ? def.displayName : input.Key)} ({have}/{input.Value})";
                return false;
            }
            RemoveFrom(working, input.Key, input.Value);
        }

        if (GetDefinition(outputId) == null || AddTo(working, outputId, outputQuantity) != outputQuantity)
        {
            failReason = "Túi đầy, không còn chỗ cho thành phẩm";
            return false;
        }

        slots = working;
        Changed?.Invoke();
        return true;
    }

    public List<InventorySlotDto> Export()
    {
        List<InventorySlotDto> result = new List<InventorySlotDto>();
        for (int i = 0; i < SlotCount; i++)
            if (!slots[i].IsEmpty)
                result.Add(new InventorySlotDto { slot = i, itemId = slots[i].itemId, quantity = slots[i].quantity });
        return result;
    }

    // Thay toàn bộ túi; mục không hợp lệ bị bỏ kèm cảnh báo.
    public void Restore(List<InventorySlotDto> saved)
    {
        slots = new Slot[SlotCount];
        if (saved != null)
        {
            foreach (InventorySlotDto dto in saved)
            {
                ItemDefinition def = dto != null ? GetDefinition(dto.itemId) : null;
                if (def == null || !def.UsesSlot || dto.slot < 0 || dto.slot >= SlotCount ||
                    !slots[dto.slot].IsEmpty || dto.quantity <= 0 || dto.quantity > def.maxStack)
                {
                    Debug.LogWarning($"{name}: bỏ ô túi không hợp lệ trong bản lưu.", this);
                    continue;
                }
                slots[dto.slot] = new Slot { itemId = dto.itemId, quantity = dto.quantity };
            }
        }
        Changed?.Invoke();
    }

    private int AddTo(Slot[] target, string itemId, int quantity)
    {
        ItemDefinition def = GetDefinition(itemId);
        if (def == null || !def.UsesSlot || quantity <= 0) return 0;

        int remaining = quantity;
        // Lấp chồng đang có trước, rồi tới ô trống.
        for (int i = 0; i < target.Length && remaining > 0; i++)
        {
            if (target[i].IsEmpty || target[i].itemId != itemId) continue;
            int space = def.maxStack - target[i].quantity;
            int put = Mathf.Min(space, remaining);
            target[i].quantity += put;
            remaining -= put;
        }
        for (int i = 0; i < target.Length && remaining > 0; i++)
        {
            if (!target[i].IsEmpty) continue;
            int put = Mathf.Min(def.maxStack, remaining);
            target[i] = new Slot { itemId = itemId, quantity = put };
            remaining -= put;
        }
        return quantity - remaining;
    }

    private static void RemoveFrom(Slot[] target, string itemId, int quantity)
    {
        // Lấy từ ô cuối để ô đầu ổn định khi xem túi.
        for (int i = target.Length - 1; i >= 0 && quantity > 0; i--)
        {
            if (target[i].IsEmpty || target[i].itemId != itemId) continue;
            int take = Mathf.Min(target[i].quantity, quantity);
            target[i].quantity -= take;
            quantity -= take;
            if (target[i].quantity <= 0) target[i] = default;
        }
    }

    private static int CountIn(Slot[] target, string itemId)
    {
        int total = 0;
        foreach (Slot slot in target)
            if (!slot.IsEmpty && slot.itemId == itemId) total += slot.quantity;
        return total;
    }
}
