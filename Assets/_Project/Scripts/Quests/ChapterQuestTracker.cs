using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Nhiệm vụ của một chương: HUD hiện bước đầu tiên còn thiếu yêu cầu; suy từ world state, không đếm event.
public class ChapterQuestTracker : MonoBehaviour
{
    [Serializable]
    public class Step
    {
        [TextArea(1, 3)] public string text;
        public List<StateRequirement> requirements = new List<StateRequirement>();
    }

    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private string doneText = "Chương đã hoàn thành.";
    [SerializeField] private List<Step> steps = new List<Step>();

    private string lastText;

    private void Awake()
    {
        if (worldState == null || objectiveText == null)
        {
            Debug.LogError($"{name}: ChapterQuestTracker thiếu tham chiếu.", this);
            enabled = false;
        }
    }

    public string CurrentObjective
    {
        get
        {
            foreach (Step step in steps)
                foreach (StateRequirement r in step.requirements)
                    if (!r.IsMet(worldState)) return step.text;
            return doneText;
        }
    }

    private void LateUpdate()
    {
        string text = $"MỤC TIÊU\n{CurrentObjective}";
        if (text == lastText) return;
        objectiveText.text = text;
        lastText = text;
    }
}
