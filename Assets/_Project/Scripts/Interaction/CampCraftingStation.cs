using StarterAssets;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class CampCraftingStation : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerInventory inventory;
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField, Min(0.5f)] private float interactionDistance = 3f;
    [SerializeField, Min(0.1f)] private float craftInterval = 0.35f;
    private float nextCraftAt;
    private float clearFeedbackAt;

    public string Prompt => "Chế 1 thuốc: 1 lá cẩm + 1 linh chi";

    private void Awake()
    {
        if (inventory == null || worldState == null)
        {
            Debug.LogError($"{name}: Assign An inventory and GameSession world state.", this);
            enabled = false;
        }
        if (feedbackText != null) feedbackText.text = string.Empty;
    }

    public bool CanInteract(GameObject interactor)
    {
        if (!isActiveAndEnabled || inventory == null || worldState == null ||
            interactor == null || Time.timeScale <= 0f || worldState.IsRelicRestored ||
            interactor.GetComponent<PlayerInventory>() != inventory ||
            !inventory.isActiveAndEnabled || Time.time < nextCraftAt ||
            (interactor.transform.position - transform.position).sqrMagnitude >
                interactionDistance * interactionDistance) return false;
        PlayerVitals vitals = interactor.GetComponent<PlayerVitals>();
        ThirdPersonController motor = interactor.GetComponent<ThirdPersonController>();
        PlayerCombatController combat = interactor.GetComponent<PlayerCombatController>();
        return vitals != null && vitals.isActiveAndEnabled && !vitals.IsDead &&
            (motor == null || !motor.IsDodging) && (combat == null || !combat.IsAttacking);
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        nextCraftAt = Time.time + craftInterval;
        PotionCraftResult result = inventory.TryCraftHealthPotion();
        string message = result == PotionCraftResult.Success
            ? "Đã chế 1 thuốc máu. Lưu tại checkpoint để giữ kết quả."
            : result == PotionCraftResult.NoSpace
                ? "Không đủ chỗ chứa thuốc. Nguyên liệu được giữ nguyên."
                : "Cần 1 lá cẩm và 1 linh chi.";
        if (feedbackText != null)
        {
            feedbackText.text = message;
            clearFeedbackAt = Time.unscaledTime + 4f;
        }
        else Debug.Log(message, this);
    }

    private void Update()
    {
        if (feedbackText != null && clearFeedbackAt > 0f && Time.unscaledTime >= clearFeedbackAt)
        {
            feedbackText.text = string.Empty;
            clearFeedbackAt = 0f;
        }
    }

    private void OnDisable()
    {
        if (feedbackText != null) feedbackText.text = string.Empty;
    }
}
