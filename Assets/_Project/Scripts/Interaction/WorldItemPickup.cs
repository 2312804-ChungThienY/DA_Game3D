using UnityEngine;

public class WorldItemPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private string pickupId;
    [SerializeField] private ItemDefinition item;
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private WorldStateStore worldState;

    public string PickupId => pickupId;
    public PlayerInventory Inventory => inventory;
    public WorldStateStore WorldState => worldState;
    public bool IsConfigured => InventoryRules.IsValidPickupId(pickupId) && item != null && item.IsValid &&
        inventory != null && worldState != null;
    public string Prompt => inventory != null && item != null && inventory.CanAddOne(item.ItemId)
        ? $"Nhặt {item.DisplayName}" : "Túi đầy";

    private void Awake()
    {
        if (!IsConfigured)
        {
            Debug.LogError($"{name}: Check pickup ID, definition, inventory and world state.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (worldState != null) worldState.Changed += ApplyWorldState;
        ApplyWorldState();
    }

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= ApplyWorldState;
    }

    public bool CanInteract(GameObject interactor)
    {
        if (!isActiveAndEnabled || !IsConfigured || interactor == null || Time.timeScale == 0f ||
            worldState.HasCollectedPickup(pickupId) || interactor.GetComponent<PlayerInventory>() != inventory)
            return false;
        PlayerVitals vitals = interactor.GetComponent<PlayerVitals>();
        return vitals != null && !vitals.IsDead;
    }

    public void Interact(GameObject interactor)
    {
        if (CanInteract(interactor)) inventory.TryCollectSinglePickup(pickupId, item.ItemId, worldState);
    }

    public void ApplyWorldState()
    {
        if (worldState == null || !InventoryRules.IsValidPickupId(pickupId)) return;
        bool visible = !worldState.HasCollectedPickup(pickupId);
        if (gameObject.activeSelf != visible) gameObject.SetActive(visible);
    }
}
