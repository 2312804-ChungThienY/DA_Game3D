using System.Collections;
using TMPro;
using UnityEngine;

public enum DrumPuzzleState { Unsolved, Listening, Failed, Solved }

public class DrumPuzzleController : MonoBehaviour
{
    [SerializeField] private DrumPuzzleDefinition definition;
    [SerializeField] private GameObject door;
    [SerializeField] private GameObject rewardDisplay;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private WorldStateStore worldState;

    private int currentIndex;
    private bool busy;

    public DrumPuzzleState State { get; private set; }
    public bool CanAcceptInput => isActiveAndEnabled && !busy && State != DrumPuzzleState.Solved;

    private void Awake()
    {
        if (definition == null || definition.Sequence == null || definition.Sequence.Length != 3 ||
            definition.Sequence[0] != DrumNote.A || definition.Sequence[1] != DrumNote.C ||
            definition.Sequence[2] != DrumNote.B || door == null || rewardDisplay == null ||
            statusText == null || door == gameObject || rewardDisplay == gameObject ||
            door == rewardDisplay)
        {
            Debug.LogError($"{name}: Thiếu dữ liệu A-C-B, cửa, bệ thưởng hoặc UI.", this);
            enabled = false;
            return;
        }

        ApplySolvedState(false);
    }

    private void OnEnable()
    {
        if (worldState != null) worldState.Changed += ApplyWorldState;
    }

    private void Start() => ApplyWorldState();

    public void ApplyWorldState()
    {
        if (worldState == null || !isActiveAndEnabled || definition == null ||
            door == null || rewardDisplay == null || statusText == null) return;

        // Chỉ áp lại khi trạng thái solved đổi; thay đổi world state khác không reset lượt đang gõ.
        bool solved = worldState.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId);
        if (solved == (State == DrumPuzzleState.Solved)) return;

        ApplySolvedState(solved);
    }

    public bool TryHit(DrumNote note)
    {
        if (!CanAcceptInput) return false;
        SoundscapeRuntime.Play(SoundCue.Drum,transform.position,.8f,note==DrumNote.A?.85f:note==DrumNote.B?1f:1.18f);

        busy = true;
        State = DrumPuzzleState.Listening;
        StartCoroutine(EvaluateHit(note));
        return true;
    }

    private IEnumerator EvaluateHit(DrumNote note)
    {
        statusText.text = $"Vừa gõ: {note}";
        yield return new WaitForSeconds(definition.InputDelay);

        if (note != definition.Sequence[currentIndex])
        {
            State = DrumPuzzleState.Failed;
            statusText.text = "Sai thứ tự. Hãy thử lại!";
            yield return new WaitForSeconds(definition.FailureDelay);
            currentIndex = 0;
            busy = false;
            State = DrumPuzzleState.Unsolved;
            ShowProgress();
            yield break;
        }

        currentIndex++;
        if (currentIndex == definition.Sequence.Length)
        {
            State = DrumPuzzleState.Solved;
            busy = false;
            if (worldState != null)
                worldState.TrySolvePuzzle(WorldStateStore.DrumPuzzleId);
            ApplySolvedState(true);
            Debug.Log("Câu đố pz_trong_01 đã hoàn thành.", this);
            yield break;
        }

        busy = false;
        ShowProgress();
    }

    public void ResetAttempt()
    {
        if (!isActiveAndEnabled || State == DrumPuzzleState.Solved) return;
        StopAllCoroutines();
        currentIndex = 0;
        busy = false;
        State = DrumPuzzleState.Unsolved;
        ShowProgress();
    }

    public void ApplySolvedState(bool solved)
    {
        StopAllCoroutines();
        currentIndex = 0;
        busy = false;
        State = solved ? DrumPuzzleState.Solved : DrumPuzzleState.Unsolved;
        door.SetActive(!solved);
        rewardDisplay.SetActive(solved);
        if (solved) statusText.text = "Đã giải câu đố! Bệ thưởng đã mở.";
        else ShowProgress();
    }

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= ApplyWorldState;
        StopAllCoroutines();
        busy = false;
        if (State != DrumPuzzleState.Solved)
        {
            currentIndex = 0;
            State = DrumPuzzleState.Unsolved;
            if (definition != null && definition.Sequence != null && statusText != null)
                ShowProgress();
        }
    }

    private void ShowProgress()
    {
        statusText.text = $"Thứ tự trống: {currentIndex}/{definition.Sequence.Length}";
    }
}
