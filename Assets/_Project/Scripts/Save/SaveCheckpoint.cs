using StarterAssets;
using UnityEngine;

[DisallowMultipleComponent]
public class SaveCheckpoint : MonoBehaviour, IInteractable
{
    [SerializeField] private SaveService saveService;
    [SerializeField] private string checkpointId = "cp_trai";
    [SerializeField] private string displayName = "Trai Gieng Troi";
    [SerializeField] private Transform respawnPoint;
    [Tooltip("Bắt buộc với checkpoint trước Đàn Kim Quy.")]
    [SerializeField] private KimQuyGate requiredGate;
    [SerializeField] private WorldStateStore worldState;

    public string CheckpointId => checkpointId;
    public Transform RespawnPoint => respawnPoint;
    public string Prompt => $"Luu tai {displayName}";
    public bool IsConfigured => saveService != null && respawnPoint != null &&
                                (checkpointId == "cp_trai" || checkpointId == "cp_truoc_dan" ||
                                 checkpointId == "cp_kim_quy") &&
                                (checkpointId == "cp_trai" || requiredGate != null) &&
                                (checkpointId != "cp_kim_quy" || worldState != null);

    private void Awake()
    {
        if (IsConfigured) return;
        Debug.LogError($"{name}: Kiểm tra ID, SaveService, RespawnPoint và Gate.", this);
        enabled = false;
    }

    public bool CanInteract(GameObject interactor)
    {
        if (!isActiveAndEnabled || !IsConfigured || !saveService.isActiveAndEnabled ||
            interactor == null || Time.timeScale == 0f)
            return false;

        if (checkpointId != "cp_trai" && !requiredGate.CanPass)
            return false;
        if (checkpointId == "cp_kim_quy" && !worldState.ChapterPrerequisitesMet)
            return false;

        PlayerVitals vitals = interactor.GetComponent<PlayerVitals>();
        ThirdPersonController motor = interactor.GetComponent<ThirdPersonController>();
        PlayerCombatController combat = interactor.GetComponent<PlayerCombatController>();
        return vitals != null && !vitals.IsDead &&
               (motor == null || !motor.IsDodging) &&
               (combat == null || !combat.IsAttacking);
    }

    public void Interact(GameObject interactor)
    {
        if (CanInteract(interactor))
            saveService.SaveAtCheckpoint(checkpointId);
    }
}
