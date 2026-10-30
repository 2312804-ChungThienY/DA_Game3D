using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
    [SerializeField] private Transform campSpawn;

    private InputAction loadAction;
    private LanternPickup lanternPickupComponent;
    private bool ready;

    private string SavePath => Path.Combine(Application.persistentDataPath, "save_v1.json");
    private string BackupPath => SavePath + ".bak";

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
                playerController != null && playerInput != null && campSpawn != null;

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

    public bool SaveAtCamp()
    {
        if (!ready)
        {
            return false;
        }

        SaveData data = new SaveData
        {
            hasLantern = lantern.HasLantern,
            campSealSolved = campSeal.IsSolved,
            collectedRelics = worldState.ExportRelics(),
            solvedPuzzles = worldState.ExportSolvedPuzzles()
        };

        if (!IsValid(data))
        {
            Debug.LogError("Trạng thái hiện tại không hợp lệ để lưu.", this);
            return false;
        }

        string temporaryPath = SavePath + ".tmp";

        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);

            using (FileStream stream = new FileStream(
                       temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(JsonUtility.ToJson(data, true));
                writer.Flush();
                stream.Flush(true);
            }

            if (!TryRead(temporaryPath, out _))
            {
                Debug.LogError("Bản lưu tạm không hợp lệ; giữ file cũ.", this);
                return false;
            }

            if (File.Exists(SavePath))
            {
                // Keep a valid backup if the primary file was already damaged.
                string backupForReplace =
                    !TryRead(SavePath, out _) && TryRead(BackupPath, out _)
                        ? null
                        : BackupPath;

                File.Replace(temporaryPath, SavePath, backupForReplace);
            }
            else
            {
                File.Move(temporaryPath, SavePath);
            }

            lantern.ApplySavedState(data.hasLantern);
            Debug.Log($"Đã lưu tại trại: {SavePath}", this);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Không lưu được game: {exception.Message}", this);
            return false;
        }
    }

    public void LoadGame()
    {
        if (!ready)
        {
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

        worldState.RestoreSnapshot(data.collectedRelics, data.solvedPuzzles);
        lantern.ApplySavedState(data.hasLantern);
        campSeal.ApplySavedState(data.campSealSolved);
        lanternPickupComponent.ApplySavedState(data.hasLantern);
        drumAPickup.SetActive(!worldState.HasRelic(WorldStateStore.DrumA));
        drumBPickup.SetActive(!worldState.HasRelic(WorldStateStore.DrumB));
        mirrorReceiver.ApplySolvedState(
            worldState.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId));
        drumCPickup.SetActive(!worldState.HasRelic(WorldStateStore.DrumC));

        bool wasEnabled = playerController.enabled;
        playerController.enabled = false;
        playerController.transform.SetPositionAndRotation(campSpawn.position, campSpawn.rotation);
        Physics.SyncTransforms();
        playerController.enabled = wasEnabled;

        Debug.Log("Đã tải game tại cp_trai.", this);
    }

    private bool TryRead(string path, out SaveData data)
    {
        data = null;

        try
        {
            if (!File.Exists(path))
            {
                return false;
            }

            SaveData candidate = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            if (candidate != null && candidate.version == 1)
            {
                candidate.version = 2;
                candidate.solvedPuzzles = new List<string>();
            }
            if (candidate != null && candidate.version == 2)
                candidate.version = 3;
            if (!IsValid(candidate))
            {
                return false;
            }

            data = candidate;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Không đọc được bản lưu: {exception.Message}", this);
            return false;
        }
    }

    private static bool IsValid(SaveData data)
    {
        if (data == null || data.version != 3 || data.checkpointId != "cp_trai" ||
            data.collectedRelics == null || data.solvedPuzzles == null)
        {
            return false;
        }

        HashSet<string> uniqueIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (string id in data.collectedRelics)
        {
            if ((id != WorldStateStore.DrumA && id != WorldStateStore.DrumB &&
                 id != WorldStateStore.DrumC) ||
                !uniqueIds.Add(id))
            {
                return false;
            }
        }

        HashSet<string> puzzles = new HashSet<string>(StringComparer.Ordinal);
        foreach (string id in data.solvedPuzzles)
        {
            if ((id != WorldStateStore.DrumPuzzleId &&
                 id != WorldStateStore.MirrorPuzzleId) || !puzzles.Add(id))
                return false;
        }

        bool drumSolved = puzzles.Contains(WorldStateStore.DrumPuzzleId);
        bool mirrorSolved = puzzles.Contains(WorldStateStore.MirrorPuzzleId);

        if (data.campSealSolved && !data.hasLantern)
        {
            return false;
        }

        if ((uniqueIds.Count > 0 || drumSolved || mirrorSolved) &&
            (!data.hasLantern || !data.campSealSolved))
        {
            return false;
        }

        if (uniqueIds.Contains(WorldStateStore.DrumB) && !drumSolved)
            return false;

        if (uniqueIds.Contains(WorldStateStore.DrumC) && !mirrorSolved)
            return false;

        return true;
    }
}
