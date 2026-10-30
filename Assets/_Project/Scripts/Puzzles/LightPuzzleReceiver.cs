using TMPro;
using UnityEngine;

public class LightPuzzleReceiver : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float requiredTime = 1f;
    [SerializeField] private GameObject door;
    [SerializeField] private GameObject rewardDisplay;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private MirrorRotator mirrorOne;
    [SerializeField] private MirrorRotator mirrorTwo;

    private float progress;

    public bool IsSolved { get; private set; }

    private void Awake()
    {
        if (door == null || rewardDisplay == null || statusText == null ||
            door == gameObject || rewardDisplay == gameObject)
        {
            Debug.LogError($"{name}: Thiếu cửa, bệ thưởng hoặc UI của câu đố gương.", this);
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

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= ApplyWorldState;
    }

    public void ApplyWorldState()
    {
        if (worldState == null || !isActiveAndEnabled) return;
        bool solved = worldState.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId);
        if (IsSolved != solved) ApplySolvedState(solved);
    }

    public void ApplySolvedState(bool solved)
    {
        progress = 0f;
        IsSolved = solved;
        if (mirrorOne != null) mirrorOne.ApplySolvedState(solved);
        if (mirrorTwo != null) mirrorTwo.ApplySolvedState(solved);
        if (door != null) door.SetActive(!solved);
        if (rewardDisplay != null) rewardDisplay.SetActive(solved);
        if (statusText != null) RefreshUI();
    }

    public void TickLight(bool receivingLight, float deltaTime)
    {
        if (!isActiveAndEnabled || IsSolved) return;

        progress = receivingLight && deltaTime > 0f
            ? Mathf.Min(requiredTime, progress + deltaTime)
            : 0f;

        if (progress >= requiredTime)
        {
            if (worldState != null)
                worldState.TrySolvePuzzle(WorldStateStore.MirrorPuzzleId);
            ApplySolvedState(true);
            Debug.Log("Đã giải câu đố gương pz_guong_01.", this);
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        statusText.text = IsSolved
            ? "Đã giải câu đố gương!"
            : $"Ngọc nhận sáng: {progress:0.0}/{requiredTime:0.0}";
    }
}
