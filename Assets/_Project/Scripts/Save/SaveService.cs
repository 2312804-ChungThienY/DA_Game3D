using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

// Nơi duy nhất đọc/ghi file lưu, dùng chung cho mọi chương (mỗi scene chương có một bản).
// Object trong scene chỉ đọc WorldStateStore/InventoryService, không tự đọc file.
public class SaveService : MonoBehaviour
{
    public const string CampCheckpointId = "cp_trai";
    public const string KimQuyCheckpointId = "cp_kim_quy";

    [Header("State")]
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private InventoryService inventory;
    [SerializeField] private LanternController lantern;

    [Header("Chỉ chương 1 (để trống ở chương khác)")]
    [SerializeField] private SealTarget campSeal;
    [SerializeField] private GameObject lanternPickup;
    [SerializeField] private GameObject drumAPickup;
    [SerializeField] private GameObject drumBPickup;
    [SerializeField] private GameObject drumCPickup;
    [SerializeField] private LightPuzzleReceiver mirrorReceiver;

    [Header("Checkpoints")]
    [Tooltip("Điểm xuất hiện mặc định khi không tìm thấy mốc.")]
    [SerializeField] private Transform campSpawn;
    [SerializeField] private CheckpointAltar[] checkpoints;

    private readonly List<ICheckpoint> sceneCheckpoints = new List<ICheckpoint>();
    private PlayerMotor motor;
    private PlayerVitals vitals;
    private PlayerStamina stamina;
    private PlayerCameraController cameraController;
    private InputReader input;
    private LanternPickup lanternPickupComponent;
    private float loadedElapsed;
    private float elapsedStartTime;
    private bool ready;

    private static string SavePath => Path.Combine(Application.persistentDataPath, "save_v1.json");
    private static string BackupPath => SavePath + ".bak";
    private static string TemporaryPath => SavePath + ".tmp";

    public bool IsReady => ready;

    public static bool IsKnownCheckpoint(string id) => WorldIds.IsKnownCheckpoint(id);

    public static bool HasValidSave() => TryReadSaved(out _);

    public static bool TryReadSaved(out SaveData data) => TryRead(SavePath, out data) || TryRead(BackupPath, out data);

    // Chơi mới: xóa ô lưu chính, bản sao lưu và file tạm.
    public static void DeleteSaveFiles()
    {
        foreach (string path in new[] { SavePath, BackupPath, TemporaryPath })
        {
            try
            {
                if (File.Exists(path)) File.Delete(path);
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Không xóa được {path}: {exception.Message}");
            }
        }
    }

    private void Awake()
    {
        // Thu mọi mốc trong scene một lần (CheckpointAltar và CheckpointTrigger).
        if (checkpoints != null)
            foreach (CheckpointAltar altar in checkpoints)
                if (altar != null) sceneCheckpoints.Add(altar);
        foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            if (behaviour is ICheckpoint cp && behaviour.gameObject.scene == gameObject.scene && !sceneCheckpoints.Contains(cp))
                sceneCheckpoints.Add(cp);
    }

    private void Start()
    {
        // Các thành phần của An nằm cùng object với LanternController.
        if (lantern != null)
        {
            motor = lantern.GetComponent<PlayerMotor>();
            vitals = lantern.GetComponent<PlayerVitals>();
            stamina = lantern.GetComponent<PlayerStamina>();
            cameraController = lantern.GetComponent<PlayerCameraController>();
        }

        if (lanternPickup != null) lanternPickupComponent = lanternPickup.GetComponent<LanternPickup>();

        input = AppRoot.Instance != null ? AppRoot.Instance.Input : null;
        elapsedStartTime = Time.time;

        ready = worldState != null && inventory != null && lantern != null && lantern.isActiveAndEnabled &&
                motor != null && campSpawn != null && (lanternPickup == null || lanternPickupComponent != null);

        if (!ready)
        {
            Debug.LogError("SaveService: Thiếu tham chiếu trong Inspector hoặc An thiếu PlayerMotor.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        // F9 chỉ dùng để thử trong Editor và bản Development: nạp lại scene rồi áp bản lưu như hồi sinh.
        if (!ready || !Debug.isDebugBuild || input == null || !input.GameplayEnabled) return;

        if (input.DebugLoad.WasPressedThisFrame() && AppRoot.Instance != null && HasValidSave())
            AppRoot.Instance.Session.Respawn();
    }

    public bool SaveAtCamp() => SaveAt(CampCheckpointId);

    public bool SaveAt(string checkpointId)
    {
        if (!ready || (vitals != null && vitals.IsDead)) return false;

        SaveData data = Capture(checkpointId, gameObject.scene.name);
        if (!IsValid(data, out string reason))
        {
            Debug.LogError($"Trạng thái hiện tại không hợp lệ để lưu: {reason}", this);
            ShowToast("Chưa lưu được tiến trình");
            return false;
        }

        if (!WriteFile(data)) return false;

        // Tại mốc lưu hồi đầy máu, thể lực và năng lượng đèn.
        RestorePlayerResources();
        Debug.Log($"Đã lưu tại {checkpointId}: {SavePath}", this);
        ShowToast("Đã lưu tiến trình");
        return true;
    }

    // Ảnh chụp trạng thái hiện tại; dùng để lưu và để mang sang chương sau.
    public SaveData Capture(string checkpointId, string sceneName)
    {
        // Trạng thái đèn và phong ấn nằm trên component; đồng bộ vào world state trước khi chụp.
        if (lantern.HasLantern) worldState.TryAdd(StateSet.Flags, WorldIds.FlagLantern);
        if (campSeal != null && campSeal.IsSolved) worldState.TryAdd(StateSet.Seals, WorldIds.SealGarden);

        return new SaveData
        {
            buildVersion = Application.version,
            timestampUtcTicks = DateTime.UtcNow.Ticks,
            chapterId = ChapterOf(sceneName),
            sceneName = sceneName,
            checkpointId = checkpointId,
            inventory = inventory.Export(),
            world = worldState.Export(),
            elapsedPlaySeconds = loadedElapsed + (Time.time - elapsedStartTime)
        };
    }

    // Gọi bởi GameSession sau khi scene vừa nạp xong (Continue, hồi sinh, F9).
    public void LoadGame()
    {
        if (!ready) return;

        if (!TryReadSaved(out SaveData data))
        {
            Debug.LogWarning("Không có bản lưu hợp lệ để tải.", this);
            return;
        }
        ApplySnapshot(data);
        Debug.Log($"Đã tải game tại {data.checkpointId}.", this);
    }

    // Áp toàn bộ ảnh chụp: world state (phát Changed một lần), túi, đèn, vật chương 1, vị trí mốc.
    public void ApplySnapshot(SaveData data)
    {
        if (!ready || data == null) return;

        worldState.Restore(data.world);
        inventory.Restore(data.inventory);

        bool hasLantern = worldState.Has(StateSet.Flags, WorldIds.FlagLantern);
        lantern.ApplySavedState(hasLantern);
        if (campSeal != null) campSeal.ApplySavedState(worldState.Has(StateSet.Seals, WorldIds.SealGarden));
        if (lanternPickupComponent != null) lanternPickupComponent.ApplySavedState(hasLantern);
        if (drumAPickup != null) drumAPickup.SetActive(!worldState.HasRelic(WorldStateStore.DrumA));
        if (drumBPickup != null) drumBPickup.SetActive(!worldState.HasRelic(WorldStateStore.DrumB));
        if (mirrorReceiver != null) mirrorReceiver.ApplySolvedState(worldState.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId));
        if (drumCPickup != null) drumCPickup.SetActive(!worldState.HasRelic(WorldStateStore.DrumC));

        Transform spawn = FindSpawn(data.checkpointId);
        motor.Teleport(spawn.position, spawn.rotation);
        if (cameraController != null) cameraController.SetAngles(spawn.eulerAngles.y, 0f);
        RestorePlayerResources();

        loadedElapsed = data.elapsedPlaySeconds;
        elapsedStartTime = Time.time;
    }

    private Transform FindSpawn(string checkpointId)
    {
        foreach (ICheckpoint checkpoint in sceneCheckpoints)
            if (checkpoint != null && checkpoint.CheckpointId == checkpointId && checkpoint.SpawnPoint != null)
                return checkpoint.SpawnPoint;

        return campSpawn;
    }

    private void RestorePlayerResources()
    {
        if (lantern.HasLantern) lantern.RefillEnergy();
        if (vitals != null) vitals.RestoreFull();
        if (stamina != null) stamina.RestoreFull();
    }

    private bool WriteFile(SaveData data)
    {
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);

            using (FileStream stream = new FileStream(TemporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(JsonUtility.ToJson(data, true));
                writer.Flush();
                stream.Flush(true);
            }

            if (!TryRead(TemporaryPath, out _))
            {
                Debug.LogError("Bản lưu tạm không hợp lệ; giữ file cũ.", this);
                ShowToast("Chưa lưu được tiến trình");
                return false;
            }

            if (File.Exists(SavePath))
            {
                // Giữ bản sao lưu hợp lệ nếu file chính đã hỏng.
                string backupForReplace = !TryRead(SavePath, out _) && TryRead(BackupPath, out _) ? null : BackupPath;
                File.Replace(TemporaryPath, SavePath, backupForReplace);
            }
            else
            {
                File.Move(TemporaryPath, SavePath);
            }
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Không lưu được game: {exception.Message}", this);
            ShowToast("Chưa lưu được tiến trình");
            return false;
        }
    }

    private static string ChapterOf(string sceneName)
    {
        switch (sceneName)
        {
            case GameSession.SecondChapterScene: return "c2";
            default: return "c1";
        }
    }

    private static void ShowToast(string message)
    {
        if (AppRoot.Instance != null && AppRoot.Instance.UI != null)
            AppRoot.Instance.UI.ShowToast(message);
    }

    private static bool TryRead(string path, out SaveData data)
    {
        data = null;

        try
        {
            if (!File.Exists(path)) return false;

            string json = File.ReadAllText(path);
            LegacySaveData header = JsonUtility.FromJson<LegacySaveData>(json);
            if (header == null) return false;

            SaveData candidate;
            if (header.schemaVersion == SaveData.CurrentSchema)
                candidate = Normalize(JsonUtility.FromJson<SaveData>(json));
            else if (header.schemaVersion == 0 && header.version >= 1 && header.version <= 3)
                candidate = MigrateLegacy(header);
            else
                return false; // Phiên bản cao hơn hoặc lạ: không đọc, giữ nguyên file.

            if (!IsValid(candidate, out _)) return false;

            data = candidate;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Không đọc được bản lưu: {exception.Message}");
            return false;
        }
    }

    // Bản v4 đầu tiên (02/11) ghi phục dựng chương 1 là Flags "relic_restored"; chuyển sang Puzzles c1_relic_restored.
    private static SaveData Normalize(SaveData data)
    {
        if (data?.world?.sets == null) return data;
        StringSetDto flags = data.world.sets.Find(s => s != null && s.set == StateSet.Flags.ToString());
        if (flags != null && flags.ids != null && flags.ids.Remove(WorldIds.LegacyRelicRestoredFlag))
            AddTo(data.world, StateSet.Puzzles, WorldIds.RelicRestoredC1);
        if (string.IsNullOrEmpty(data.sceneName)) data.sceneName = WorldIds.SceneOfCheckpoint(data.checkpointId);
        return data;
    }

    // v1–v3 (chương 1 cũ) → v4 trong bộ nhớ.
    private static SaveData MigrateLegacy(LegacySaveData old)
    {
        SaveData data = new SaveData { checkpointId = old.checkpointId };
        if (old.hasLantern) AddTo(data.world, StateSet.Flags, WorldIds.FlagLantern);
        if (old.campSealSolved) AddTo(data.world, StateSet.Seals, WorldIds.SealGarden);
        if (old.collectedRelics != null)
            foreach (string id in old.collectedRelics) AddTo(data.world, StateSet.Fragments, id);
        if (old.version >= 2 && old.solvedPuzzles != null)
            foreach (string id in old.solvedPuzzles) AddTo(data.world, StateSet.Puzzles, id);
        return data;
    }

    private static void AddTo(WorldSnapshot world, StateSet set, string id)
    {
        string key = set.ToString();
        StringSetDto dto = world.sets.Find(s => s != null && s.set == key);
        if (dto == null)
        {
            dto = new StringSetDto { set = key };
            world.sets.Add(dto);
        }
        if (!dto.ids.Contains(id)) dto.ids.Add(id);
    }

    private static bool IsValid(SaveData data, out string reason)
    {
        reason = null;
        if (data == null || data.schemaVersion != SaveData.CurrentSchema) { reason = "schema"; return false; }
        if (!IsKnownCheckpoint(data.checkpointId)) { reason = "checkpoint"; return false; }
        if (WorldIds.SceneOfCheckpoint(data.checkpointId) != data.sceneName) { reason = "mốc không thuộc scene"; return false; }
        if (data.world == null || data.world.sets == null || data.inventory == null) { reason = "thiếu dữ liệu"; return false; }

        // Mọi ID phải thuộc danh mục, không trùng, không lặp tập.
        HashSet<string> seenSets = new HashSet<string>(StringComparer.Ordinal);
        foreach (StringSetDto dto in data.world.sets)
        {
            if (dto == null || dto.ids == null || !Enum.TryParse(dto.set, out StateSet set) || !seenSets.Add(dto.set))
            { reason = "tập world state"; return false; }

            HashSet<string> unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (string id in dto.ids)
                if (!WorldIds.IsKnown(set, id) || !unique.Add(id)) { reason = $"ID {id}"; return false; }
        }

        HashSet<int> usedSlots = new HashSet<int>();
        foreach (InventorySlotDto slot in data.inventory)
        {
            if (slot == null || string.IsNullOrEmpty(slot.itemId) || slot.quantity <= 0 ||
                slot.slot < 0 || slot.slot >= InventoryService.SlotCount || !usedSlots.Add(slot.slot))
            { reason = "ô túi"; return false; }
        }

        WorldSnapshot w = data.world;
        bool lanternOwned = w.Get(StateSet.Flags).Contains(WorldIds.FlagLantern);
        bool sealCleared = w.Get(StateSet.Seals).Contains(WorldIds.SealGarden);
        List<string> fragments = w.Get(StateSet.Fragments);
        List<string> puzzles = w.Get(StateSet.Puzzles);
        bool hasA = fragments.Contains(WorldStateStore.DrumA), hasB = fragments.Contains(WorldStateStore.DrumB), hasC = fragments.Contains(WorldStateStore.DrumC);
        bool drumSolved = puzzles.Contains(WorldStateStore.DrumPuzzleId);
        bool mirrorSolved = puzzles.Contains(WorldStateStore.MirrorPuzzleId);
        bool doorOpen = w.Get(StateSet.Doors).Contains(WorldIds.DoorKimQuy);
        bool guardianDown = w.Get(StateSet.Bosses).Contains(WorldIds.GuardianBoss);
        bool restored = puzzles.Contains(WorldIds.RelicRestoredC1);
        bool kit = w.Get(StateSet.Abilities).Contains(WorldIds.MovementKit);
        bool inChapter2 = data.sceneName == GameSession.SecondChapterScene;

        // Ràng buộc tiến trình chương 1: không thể có bước sau khi thiếu bước trước.
        if (sealCleared && !lanternOwned) { reason = "phong ấn cần đèn"; return false; }
        if ((hasA || hasB || hasC || drumSolved || mirrorSolved) && (!lanternOwned || !sealCleared))
        { reason = "tiến trình cần đèn và phong ấn"; return false; }
        if (hasB && !drumSolved) { reason = "mảnh B"; return false; }
        if (hasC && !mirrorSolved) { reason = "mảnh C"; return false; }
        if (doorOpen && (!(hasA && hasB && hasC) || !drumSolved || !mirrorSolved)) { reason = "cổng Đàn"; return false; }
        if (guardianDown && !doorOpen) { reason = "Hộ vệ"; return false; }
        if (restored && !guardianDown) { reason = "phục dựng"; return false; }
        if (kit && !restored) { reason = "Quà Kim Quy"; return false; }
        if (data.checkpointId == KimQuyCheckpointId && !guardianDown) { reason = "mốc Kim Quy"; return false; }
        // Chương 2 chỉ mở sau khi hoàn thành chương 1.
        if (inChapter2 && !restored) { reason = "chương 2 cần hoàn thành chương 1"; return false; }

        // Điểm đúng lần đầu phải là câu đã trả lời.
        List<string> answered = w.Get(StateSet.QuizAnswered);
        foreach (string id in w.Get(StateSet.QuizFirstTry))
            if (!answered.Contains(id)) { reason = $"câu {id} chưa trả lời"; return false; }

        return true;
    }
}
