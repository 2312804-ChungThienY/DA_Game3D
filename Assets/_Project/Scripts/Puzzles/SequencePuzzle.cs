using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

// Câu đố chuỗi dùng chung (chuông gió chương 2, trống bốn vành chương 5). Spec chapters/00 mục 4.
public class SequencePuzzleController : MonoBehaviour
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private SequencePuzzleDefinition definition;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private UnityEvent onSolved;

    private int currentIndex;
    private bool busy;
    private uint restoreVersion;

    public bool IsSolved => worldState != null && definition != null && worldState.Has(StateSet.Puzzles, definition.puzzleId);
    public bool CanAcceptInput => isActiveAndEnabled && !busy && !IsSolved;
    public int Progress => currentIndex;

    private void Awake()
    {
        if (worldState == null || definition == null || definition.sequence == null || definition.sequence.Length == 0 ||
            !WorldIds.IsKnown(StateSet.Puzzles, definition.puzzleId))
        {
            Debug.LogError($"{name}: SequencePuzzleController thiếu dữ liệu hoặc ID sai.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        restoreVersion = worldState != null ? worldState.RestoreVersion : 0;
        if (worldState != null) worldState.Changed += ShowStatus;
    }

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= ShowStatus;
        StopAllCoroutines();
        busy = false;
        currentIndex = 0;
    }

    private void Start() => ShowStatus();

    public bool TryInput(string nodeId)
    {
        if (!CanAcceptInput) return false;
        busy = true;
        StartCoroutine(Evaluate(nodeId));
        return true;
    }

    private IEnumerator Evaluate(string nodeId)
    {
        yield return new WaitForSeconds(definition.inputDelay);

        if (nodeId != definition.sequence[currentIndex])
        {
            if (statusText != null) statusText.text = "Sai thứ tự. Thử lại!";
            yield return new WaitForSeconds(definition.failDelay);
            currentIndex = 0;
            busy = false;
            ShowStatus();
            yield break;
        }

        currentIndex++;
        busy = false;
        if (currentIndex >= definition.sequence.Length)
        {
            currentIndex = 0;
            worldState.TryAdd(StateSet.Puzzles, definition.puzzleId);
            onSolved?.Invoke();
        }
        ShowStatus();
    }

    // Khi world state đổi chỉ cập nhật hiển thị solved, không reset lượt đang thử.
    private void ShowStatus()
    {
        if (worldState != null && worldState.RestoreVersion != restoreVersion)
        {
            restoreVersion = worldState.RestoreVersion;
            StopAllCoroutines();
            busy = false;
            currentIndex = 0;
        }
        // Snapshot restore or an external solve invalidates a delayed input.
        if (IsSolved && busy) { StopAllCoroutines(); busy = false; currentIndex = 0; }
        if (statusText == null || definition == null) return;
        statusText.text = IsSolved ? "Đã giải" : $"{currentIndex}/{definition.sequence.Length}";
    }
}
