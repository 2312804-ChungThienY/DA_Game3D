using UnityEngine;
using UnityEngine.InputSystem;

public class SaveService : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private LanternController lantern;
    [SerializeField] private SealTarget campSeal;

    [Header("Scene objects")]
    [SerializeField] private GameObject lanternPickup;
    [SerializeField] private GameObject drumAPickup;
    [SerializeField] private GameObject drumBPickup;
    [SerializeField] private GameObject drumCPickup;
    [SerializeField] private LightPuzzleReceiver mirrorReceiver;

    [Header("Player and checkpoint")]
    [SerializeField] private CharacterController playerController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private SaveCheckpoint[] checkpoints;
    [SerializeField] private CheckpointRespawnFlow respawnFlow;

    private InputAction loadAction;
    private LanternPickup lanternPickupComponent;
    private bool ready;

    private string SavePath => GameSaveStorage.SavePath;
    private string BackupPath => GameSaveStorage.BackupPath;

    private void Start()
    {
        // Both components live on An beside LanternController. Resolve them
        // here because they are inherited from the Starter Assets prefab.
        if (lantern != null)
        {
            if (playerController == null)
            {
                playerController = lantern.GetComponent<CharacterController>();
            }

            if (playerInput == null)
            {
                playerInput = lantern.GetComponent<PlayerInput>();
            }
        }

        if (lanternPickup != null)
        {
            lanternPickupComponent = lanternPickup.GetComponent<LanternPickup>();
        }

        ready = worldState != null && lantern != null && lantern.isActiveAndEnabled &&
                campSeal != null && campSeal.isActiveAndEnabled &&
                lanternPickupComponent != null && drumAPickup != null && drumBPickup != null &&
                drumCPickup != null && mirrorReceiver != null &&
                playerController != null && playerInput != null && respawnFlow != null &&
                playerInput.GetComponent<PlayerVitals>() != null &&
                playerInput.GetComponent<PlayerStamina>() != null &&
                FindCheckpoint("cp_trai") != null &&
                FindCheckpoint("cp_truoc_dan") != null &&
                FindCheckpoint("cp_kim_quy") != null;

        if (!ready)
        {
            Debug.LogError("SaveService: Thiếu tham chiếu trong Inspector.", this);
            enabled = false;
            return;
        }

        loadAction = playerInput.actions.FindAction("LoadGame", false);
        if (loadAction == null)
        {
            Debug.LogWarning("Không có action LoadGame; chưa thể tải bằng F9.", this);
        }
    }

    private void Update()
    {
        if (!ready || !Application.isFocused || Time.timeScale == 0f ||
            loadAction == null || !loadAction.enabled)
        {
            return;
        }

        if (loadAction.WasPressedThisFrame())
        {
            LoadGame();
        }
    }

    private SaveCheckpoint FindCheckpoint(string id)
    {
        if (checkpoints == null ||
            (id != "cp_trai" && id != "cp_truoc_dan" && id != "cp_kim_quy"))
            return null;

        SaveCheckpoint found = null;
        foreach (SaveCheckpoint checkpoint in checkpoints)
        {
            if (checkpoint == null || checkpoint.CheckpointId != id)
                continue;
            if (!checkpoint.IsConfigured || found != null)
            {
                Debug.LogError($"Checkpoint thiếu cấu hình hoặc trùng ID: {id}", this);
                return null;
            }
            found = checkpoint;
        }
        return found;
    }

    public bool SaveAtCheckpoint(string checkpointId)
    {
        if (!ready || Time.timeScale == 0f ||
            (worldState.IsRelicRestored && checkpointId != "cp_kim_quy"))
        {
            return false;
        }

        SaveCheckpoint selectedCheckpoint = FindCheckpoint(checkpointId);
        if (selectedCheckpoint == null ||
            !selectedCheckpoint.CanInteract(playerInput.gameObject))
            return false;

        PlayerVitals vitals = playerInput.GetComponent<PlayerVitals>();
        PlayerStamina stamina = playerInput.GetComponent<PlayerStamina>();
        if (vitals == null || stamina == null || vitals.IsDead)
            return false;

        SaveData data = new SaveData
        {
            checkpointId = checkpointId,
            hasLantern = lantern.HasLantern,
            campSealSolved = campSeal.IsSolved,
            relicRestored = worldState.IsRelicRestored,
            collectedRelics = worldState.ExportRelics(),
            solvedPuzzles = worldState.ExportSolvedPuzzles(),
            defeatedEnemies = worldState.ExportDefeatedEnemies()
        };

        if (!IsValid(data))
        {
            Debug.LogError("Trạng thái hiện tại không hợp lệ để lưu.", this);
            return false;
        }

        if (!TryWriteSnapshot(data)) return false;
        RefillPlayer(vitals, stamina);
        Debug.Log($"Đã lưu tại {checkpointId}: {SavePath}", this);
        return true;
    }

    public bool TryCompleteChapter()
    {
        if (!ready || Time.timeScale == 0f || !worldState.CanRestoreRelic)
            return false;

        SaveCheckpoint checkpoint = FindCheckpoint("cp_kim_quy");
        PlayerVitals vitals = playerInput.GetComponent<PlayerVitals>();
        PlayerStamina stamina = playerInput.GetComponent<PlayerStamina>();
        if (checkpoint == null || !checkpoint.CanInteract(playerInput.gameObject) ||
            vitals == null || stamina == null || vitals.IsDead)
            return false;

        SaveData data = new SaveData
        {
            checkpointId = "cp_kim_quy",
            hasLantern = lantern.HasLantern,
            campSealSolved = campSeal.IsSolved,
            relicRestored = true,
            collectedRelics = worldState.ExportRelics(),
            solvedPuzzles = worldState.ExportSolvedPuzzles(),
            defeatedEnemies = worldState.ExportDefeatedEnemies()
        };
        if (!IsValid(data) || !TryWriteSnapshot(data)) return false;

        worldState.TryRestoreRelic();
        RefillPlayer(vitals, stamina);
        Debug.Log("Đã lưu hoàn thành chương tại cp_kim_quy.", this);
        return true;
    }

    private bool TryWriteSnapshot(SaveData data) => GameSaveStorage.TryWrite(data);

    public bool TryStartNewGame()
    {
        if (!ready) return false;
        SaveData initial = new SaveData();
        if (!IsValid(initial) || !ApplySnapshot(initial)) return false;
        return TryWriteSnapshot(initial);
    }

    private void RefillPlayer(PlayerVitals vitals, PlayerStamina stamina)
    {
        lantern.ApplySavedState(lantern.HasLantern);
        vitals.RestoreFull();
        stamina.RestoreFull();
    }

    public bool SaveAtCamp() => SaveAtCheckpoint("cp_trai");

    public void LoadGame()
    {
        if (!ready || respawnFlow == null || !respawnFlow.isActiveAndEnabled)
        {
            Debug.LogWarning("SaveService chưa sẵn sàng để tải.", this);
            return;
        }

        if (!TryRead(SavePath, out SaveData data))
        {
            if (!TryRead(BackupPath, out data))
            {
                Debug.LogWarning("Không có bản lưu hợp lệ để tải.", this);
                return;
            }

            Debug.LogWarning("Đang dùng bản lưu dự phòng.", this);
        }

        respawnFlow.ReloadWithSnapshot(data);
    }

    public bool ApplySnapshot(SaveData data)
    {
        if (!ready || !IsValid(data)) return false;

        SaveCheckpoint checkpoint = FindCheckpoint(data.checkpointId);
        if (checkpoint == null) return false;

        PlayerVitals vitals = playerInput.GetComponent<PlayerVitals>();
        PlayerStamina stamina = playerInput.GetComponent<PlayerStamina>();
        if (vitals == null || stamina == null)
        {
            Debug.LogError("Không thể khôi phục: An thiếu Vitals hoặc Stamina.", this);
            return false;
        }

        worldState.RestoreSnapshot(
            data.collectedRelics, data.solvedPuzzles, data.defeatedEnemies,
            data.relicRestored);
        lantern.ApplySavedState(data.hasLantern);
        campSeal.ApplySavedState(data.campSealSolved);
        lanternPickupComponent.ApplySavedState(data.hasLantern);
        drumAPickup.SetActive(!worldState.HasRelic(WorldStateStore.DrumA));
        drumBPickup.SetActive(!worldState.HasRelic(WorldStateStore.DrumB));
        mirrorReceiver.ApplySolvedState(
            worldState.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId));
        drumCPickup.SetActive(!worldState.HasRelic(WorldStateStore.DrumC));

        vitals.RestoreFull();
        stamina.RestoreFull();

        bool wasEnabled = playerController.enabled;
        playerController.enabled = false;
        playerController.transform.SetPositionAndRotation(
            checkpoint.RespawnPoint.position, checkpoint.RespawnPoint.rotation);
        Physics.SyncTransforms();
        playerController.enabled = wasEnabled;

        return true;
    }

    private bool TryRead(string path, out SaveData data)
    {
        if (!GameSaveStorage.TryReadOne(path, out data)) return false;
        if (IsValid(data)) return true;
        data = null;
        return false;
    }

    private bool IsValid(SaveData data) =>
        GameSaveStorage.IsValid(data) && FindCheckpoint(data.checkpointId) != null;
}
