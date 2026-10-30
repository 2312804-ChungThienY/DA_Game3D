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
            objective = "Nhan Long Den Sen tai trai.";
        }
        else if (!worldState.HasRelic(WorldStateStore.DrumA))
        {
            objective = "Dung Focus mo phong an.\nTim Manh Trong A tai Vuon Reu.";
        }
        else if (!worldState.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId))
        {
            objective = "Den Dien Trong.\nDoc goi y va go dung thu tu.";
        }
        else if (!worldState.HasRelic(WorldStateStore.DrumB))
        {
            objective = "Cau do da giai.\nNhan Manh Trong B tren be thuong.";
        }
        else
        {
            objective = "Da nhan hai manh trong.\nMuc tieu tiep theo: Hanh Lang Quang.";
        }

        string text = $"MUC TIEU\n{objective}\nManh trong: {worldState.RelicCount}/3";
        if (text != lastText)
        {
            objectiveText.text = text;
            lastText = text;
        }
    }
}
