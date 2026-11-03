using UnityEngine;

// Mốc lưu tương tác bằng E (lửa trại, bàn thờ). Hồi sinh tại spawnPoint.
public class CheckpointAltar : MonoBehaviour, IInteractable, ICheckpoint
{
    [SerializeField] private SaveService saveService;
    [SerializeField] private string checkpointId = SaveService.CampCheckpointId;
    [SerializeField] private string displayName = "lửa trại";
    [SerializeField] private Transform spawnPoint;

    [Header("Tùy chọn")]
    [SerializeField] private WorldStateStore worldState;
    [Tooltip("Chỉ dùng được khi trùm này đã bị đánh bại (để trống nếu không cần).")]
    [SerializeField] private string requiredBossId;
    [SerializeField] private CraftingPanel craftingPanel;

    public string CheckpointId => checkpointId;
    public Transform SpawnPoint => spawnPoint;
    public string Prompt => craftingPanel != null
        ? $"Lưu tiến trình và chế thuốc tại {displayName}"
        : $"Lưu tiến trình tại {displayName}";

    private void Awake()
    {
        if (saveService == null || spawnPoint == null || !SaveService.IsKnownCheckpoint(checkpointId) ||
            (!string.IsNullOrEmpty(requiredBossId) && worldState == null))
        {
            Debug.LogError($"{name}: CheckpointAltar thiếu SaveService, Spawn Point, World State hoặc ID mốc sai.", this);
            enabled = false;
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return isActiveAndEnabled && saveService != null && saveService.isActiveAndEnabled &&
               (string.IsNullOrEmpty(requiredBossId) || worldState.Has(StateSet.Bosses, requiredBossId));
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;

        bool saved = saveService.SaveAt(checkpointId);
        if (saved && craftingPanel != null) craftingPanel.Open(this);
    }

    // Mỗi lần chế thành công tại trại thì lưu lại trạng thái an toàn.
    public bool SaveAfterCraft() => saveService.SaveAt(checkpointId);
}
