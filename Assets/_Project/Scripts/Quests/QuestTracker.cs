using TMPro;
using UnityEngine;

public class QuestTracker : MonoBehaviour
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private LanternController lantern;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text progressText;

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

        if (!worldState.HasRelic(WorldStateStore.DrumA))
        {
            objective = "Khám phá Vườn Rêu.\nTìm mảnh Mặt Trống.";
        }
        else if (!worldState.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId))
        {
            objective = "Khám phá Điện Trống.\nTìm bản chép và gõ đúng nhịp.";
        }
        else if (!worldState.HasRelic(WorldStateStore.DrumB))
        {
            objective = "Nhịp trống đã khớp.\nNhận mảnh Thân Trống trên bệ.";
        }
        else if (!worldState.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId))
        {
            objective = "Đến Hành Lang Quang.\nTìm ghi chép, dẫn sáng qua hai gương.";
        }
        else if (!worldState.HasRelic(WorldStateStore.DrumC))
        {
            objective = "Ngọc đã nhận đủ sáng.\nNhận mảnh Chân Trống trên bệ.";
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

        int puzzles=(worldState.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId)?1:0)+(worldState.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId)?1:0);
        string progress=$"Mảnh trống  {worldState.RelicCount}/3     •     Câu đố  {puzzles}/2";
        string text=progressText!=null?objective:$"MỤC TIÊU\n{objective}\n{progress}";
        if(progressText!=null&&progressText.text!=progress)progressText.text=progress;
        if (text != lastText)
        {
            objectiveText.text = text;
            lastText = text;
        }
    }
}
