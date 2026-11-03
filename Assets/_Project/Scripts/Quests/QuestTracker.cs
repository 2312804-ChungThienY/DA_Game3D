using TMPro;
using UnityEngine;

public class QuestTracker : MonoBehaviour
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private LanternController lantern;
    [SerializeField] private TMP_Text objectiveText;

    private string lastText;

    private void Awake()
    {
        if (worldState == null || lantern == null || objectiveText == null)
        {
            Debug.LogError($"{name}: QuestTracker thiếu tham chiếu.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        string objective;

        if (!lantern.HasLantern)
        {
            objective = "Nhận Lồng Đèn Sen tại trại.";
        }
        else if (!worldState.HasRelic(WorldStateStore.DrumA))
        {
            objective = "Giữ chuột phải chiếu đèn mở phong ấn.\nTìm Mảnh Trống A ở Vườn Rêu.";
        }
        else if (!worldState.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId))
        {
            objective = "Đến Điện Trống.\nRọi đèn đọc gợi ý, gõ đúng thứ tự.";
        }
        else if (!worldState.HasRelic(WorldStateStore.DrumB))
        {
            objective = "Câu đố đã giải.\nNhận Mảnh Trống B trên bệ thưởng.";
        }
        else if (!worldState.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId))
        {
            objective = "Đến Hành Lang Quang.\nXoay hai gương đưa tia sáng tới ngọc.";
        }
        else if (!worldState.HasRelic(WorldStateStore.DrumC))
        {
            objective = "Câu đố gương đã giải.\nNhận Mảnh Trống C trên bệ thưởng.";
        }
        else if (!worldState.Has(StateSet.Doors, WorldIds.DoorKimQuy))
        {
            objective = "Đã đủ 3 mảnh trống.\nĐi qua cổng gương, mở cổng Đàn Kim Quy.";
        }
        else if (!worldState.Has(StateSet.Bosses, WorldIds.GuardianBoss))
        {
            objective = "Đánh bại Hộ vệ Đá.\nTránh vùng đỏ, chiếu đèn để làm yếu.";
        }
        else if (!worldState.Has(StateSet.Puzzles, WorldIds.RelicRestoredC1))
        {
            objective = "Phục dựng Trống Đồng tại bệ Kim Quy.";
        }
        else
        {
            objective = "Chương 1 hoàn thành.\nĐã nhận Quà Kim Quy.";
        }

        string text = $"MỤC TIÊU\n{objective}\nMảnh trống: {worldState.RelicCount}/3";
        if (text != lastText)
        {
            objectiveText.text = text;
            lastText = text;
        }
    }
}
