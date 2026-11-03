using UnityEngine;

// Vật nhặt một đơn vị (spec phần 17). Chỉ đánh dấu đã nhặt khi túi đã nhận đủ.
public class ItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private InventoryService inventory;
    [SerializeField] private string pickupId = "pickup_la_cam_01";
    [SerializeField] private string itemId = InventoryService.LaCam;

    private string displayName;

    public string Prompt => $"Nhặt {displayName}";

    private void Awake()
    {
        if (worldState == null || inventory == null || !WorldIds.IsKnown(StateSet.Pickups, pickupId))
        {
            Debug.LogError($"{name}: ItemPickup thiếu WorldState/Inventory hoặc ID vật nhặt sai.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        ItemDefinition def = inventory != null ? inventory.GetDefinition(itemId) : null;
        if (def == null)
        {
            Debug.LogError($"{name}: Không có vật phẩm {itemId} trong catalog.", this);
            enabled = false;
            return;
        }
        displayName = def.displayName;
        RefreshFromState();
    }

    private void OnEnable()
    {
        if (worldState != null) worldState.Changed += RefreshFromState;
    }

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= RefreshFromState;
    }

    public bool CanInteract(GameObject interactor) =>
        isActiveAndEnabled && displayName != null && !worldState.Has(StateSet.Pickups, pickupId);

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;

        if (!inventory.CanAdd(itemId, 1))
        {
            Toast("Túi đầy");
            return;
        }

        if (inventory.TryAdd(itemId, 1) == 1 && worldState.TryAdd(StateSet.Pickups, pickupId))
            Toast($"Nhận {displayName}");
    }

    private void RefreshFromState()
    {
        if (worldState.Has(StateSet.Pickups, pickupId)) gameObject.SetActive(false);
    }

    private static void Toast(string message)
    {
        if (AppRoot.Instance != null && AppRoot.Instance.UI != null) AppRoot.Instance.UI.ShowToast(message);
    }
}
