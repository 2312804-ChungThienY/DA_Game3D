using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Bệ phục dựng cổ vật (spec phần 16, chapters/00 PF_RelicAltar): đủ 3 mảnh và trùm đã bị đánh bại thì
// ghi restoredId, tùy chọn trao năng lực, lưu tại mốc và hiện màn kết chương. Không trao lại sau khi tải.
public class RelicAltar : MonoBehaviour, IInteractable
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private SaveService saveService;
    [SerializeField] private StoryCardPanel storyPanel;
    [SerializeField] private GameObject restoredVisual;
    [SerializeField] private GameObject readyGlow;

    [Header("Điều kiện")]
    [SerializeField] private string[] fragmentIds = { WorldStateStore.DrumA, WorldStateStore.DrumB, WorldStateStore.DrumC };
    [SerializeField] private string bossId = WorldIds.GuardianBoss;
    [SerializeField] private string bossName = "Hộ vệ Đá";
    [SerializeField] private string restoredId = WorldIds.RelicRestoredC1;
    [SerializeField] private string relicName = "Trống Đồng";

    [Header("Kết quả")]
    [SerializeField] private string grantAbility = WorldIds.MovementKit;
    [SerializeField] private string checkpointId = SaveService.KimQuyCheckpointId;
    [SerializeField] private bool endsChapter = true;
    [SerializeField] private string nextScene = GameSession.SecondChapterScene;
    [SerializeField] private string nextCheckpoint = "c2_cp_start";
    [SerializeField] private string endTitle = "Chương 1 hoàn thành – Đàn Kim Quy";
    [SerializeField, TextArea(6, 14)] private string endBody =
        "Ba mảnh Trống Đồng khớp lại trên bệ đá. Tiếng trống ngân dài qua lòng hang, linh mạch dưới Đàn Kim Quy sáng trở lại và Hắc Sương lùi xa.\n\n" +
        "Từ mặt nước tối, bóng Rùa Vàng hiện lên và trao cho An Phúc Kim Quy: từ nay An lộn né được (Ctrl + hướng), lộn rồi chém, và né đúng lúc.\n" +
        "Kim Quy dặn: \"Khi gặp trống bốn vành, vành của ta vang cuối cùng.\"\n\n" +
        "Truyền thuyết: Kim Quy giúp An Dương Vương xây thành Cổ Loa.\n\n" +
        "Tiến trình đã được lưu tại bệ Kim Quy.";
    [SerializeField] private UnityEvent onRestored;

    public bool IsRestored => worldState != null && worldState.Has(StateSet.Puzzles, restoredId);

    public string Prompt
    {
        get
        {
            string missing = MissingText();
            return missing == null ? $"Phục dựng {relicName}" : $"Bệ {relicName} – cần: {missing}";
        }
    }

    private void Awake()
    {
        if (worldState == null || saveService == null || restoredVisual == null ||
            !WorldIds.IsKnown(StateSet.Puzzles, restoredId) || (endsChapter && storyPanel == null))
        {
            Debug.LogError($"{name}: RelicAltar thiếu tham chiếu hoặc ID sai.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (worldState != null) worldState.Changed += Apply;
    }

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= Apply;
    }

    private void Start() => Apply();

    public bool CanInteract(GameObject interactor) => isActiveAndEnabled && !IsRestored;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;

        string missing = MissingText();
        if (missing != null)
        {
            if (AppRoot.Instance != null && AppRoot.Instance.UI != null)
                AppRoot.Instance.UI.ShowToast($"Chưa thể phục dựng: cần {missing}");
            return;
        }

        // Ghi trạng thái trước, rồi lưu ảnh chụp an toàn tại bệ, cuối cùng mới hiện màn kết.
        WorldSnapshot before=worldState.Export();
        worldState.TryAdd(StateSet.Puzzles, restoredId);
        if (!string.IsNullOrEmpty(grantAbility)) worldState.TryAdd(StateSet.Abilities, grantAbility);
        if (!string.IsNullOrEmpty(checkpointId)&&!saveService.SaveAt(checkpointId))
        {
            worldState.Restore(before);
            AppRoot.Instance?.UI?.ShowToast("Chưa lưu được phục dựng. Tiến trình giữ nguyên; hãy thử lại.",4);
            return;
        }
        onRestored?.Invoke();
        SoundscapeRuntime.Play(SoundCue.Success);
        if (endsChapter) storyPanel.ShowChapterEnd(endTitle, endBody, nextScene, nextCheckpoint);
    }

    private void Apply()
    {
        bool restored = IsRestored;
        restoredVisual.SetActive(restored);
        if (readyGlow != null) readyGlow.SetActive(!restored && MissingText() == null);
    }

    private string MissingText()
    {
        List<string> missing = new List<string>();
        int fragments = 0;
        foreach (string id in fragmentIds)
            if (worldState.Has(StateSet.Fragments, id)) fragments++;
        if (fragments < fragmentIds.Length) missing.Add($"đủ {fragmentIds.Length} mảnh ({fragments}/{fragmentIds.Length})");
        if (!string.IsNullOrEmpty(bossId) && !worldState.Has(StateSet.Bosses, bossId)) missing.Add($"đánh bại {bossName}");
        return missing.Count == 0 ? null : string.Join(", ", missing);
    }
}
