using System.Text;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ChapterQuestHUD : MonoBehaviour
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text checklistText;

    private void OnEnable()
    {
        if (worldState != null) worldState.Changed += Refresh;
        Refresh();
    }
    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= Refresh;
    }
    private void Start()
    {
        if (worldState == null || objectiveText == null)
            Debug.LogError("ChapterQuestHUD: Assign World State and Objective Text.", this);
        Refresh();
    }
    public void Refresh()
    {
        if (worldState == null) return;
        bool a = worldState.HasRelic(WorldStateStore.DrumA);
        bool b = worldState.HasRelic(WorldStateStore.DrumB);
        bool c = worldState.HasRelic(WorldStateStore.DrumC);
        bool drums = worldState.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId);
        bool mirrors = worldState.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId);
        bool guardian = worldState.HasDefeatedEnemy(WorldStateStore.GuardianId);
        bool restored = worldState.IsRelicRestored;
        ChapterObjective current = ChapterObjectiveRules.Evaluate(a, drums, b, mirrors, c, guardian, restored);
        if (objectiveText != null)
            objectiveText.text = (restored ? "HOÀN THÀNH\n" : "MỤC TIÊU\n") +
                ChapterObjectiveRules.Description(current);
        if (progressText != null)
            progressText.text = $"Mảnh trống: {(a ? 1 : 0) + (b ? 1 : 0) + (c ? 1 : 0)}/3  |  Câu đố: {(drums ? 1 : 0) + (mirrors ? 1 : 0)}/2";
        if (checklistText == null) return;
        StringBuilder list = new StringBuilder();
        Append(list, a, "Mảnh trống A — Vườn Rêu");
        Append(list, drums, "Câu đố Điện Trống");
        Append(list, b, "Mảnh trống B");
        Append(list, mirrors, "Câu đố Hành Lang Quang");
        Append(list, c, "Mảnh trống C");
        Append(list, guardian, "Hộ vệ Đá");
        Append(list, restored, "Phục hồi cổ vật");
        checklistText.text = list.ToString();
    }
    private static void Append(StringBuilder list, bool done, string label)
    {
        list.Append(done ? "[x] " : "[ ] ").Append(label).Append('\n');
    }
}
