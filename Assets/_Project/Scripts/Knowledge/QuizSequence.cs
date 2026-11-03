using UnityEngine;
using UnityEngine.Events;

// Linh Thần Vấn Đáp (spec phần 44): hỏi lần lượt các câu; tiến độ giữa chừng không lưu.
// Xong: ghi sequenceId vào Puzzles, hiện thẻ thức tỉnh, gọi onCompleted.
public class QuizSequence : MonoBehaviour, IInteractable
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private QuizPanel quizPanel;
    [SerializeField] private StoryCardPanel storyPanel;
    [SerializeField] private string sequenceId;
    [SerializeField] private string requiredPuzzleId;
    [SerializeField] private string speakerName = "Linh Thần";
    [SerializeField] private QuizDefinition[] quizzes;
    [SerializeField] private StoryCard completionCard;
    [SerializeField] private GameObject visual;
    [SerializeField] private UnityEvent onCompleted;
    [SerializeField] private SaveService saveService;
    [SerializeField] private string saveCheckpointId;

    private int index;

    public bool IsDone => worldState != null && worldState.Has(StateSet.Puzzles, sequenceId);
    private bool IsAvailable => string.IsNullOrEmpty(requiredPuzzleId) || worldState.Has(StateSet.Puzzles, requiredPuzzleId);
    public string Prompt => $"Trò chuyện với {speakerName} ({quizzes.Length} câu hỏi)";

    private void Awake()
    {
        if (worldState == null || quizPanel == null || storyPanel == null || quizzes == null || quizzes.Length == 0 ||
            !WorldIds.IsKnown(StateSet.Puzzles, sequenceId))
        {
            Debug.LogError($"{name}: QuizSequence thiếu tham chiếu hoặc ID sai.", this);
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

    private void Apply()
    {
        if (visual != null) visual.SetActive(IsAvailable);
    }

    public bool CanInteract(GameObject interactor) => isActiveAndEnabled && IsAvailable && !IsDone;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        index = 0;
        AskCurrent();
    }

    private void AskCurrent()
    {
        if (index >= quizzes.Length)
        {
            worldState.TryAdd(StateSet.Puzzles, sequenceId);
            if (saveService != null && !string.IsNullOrEmpty(saveCheckpointId)) saveService.SaveAt(saveCheckpointId);
            if (completionCard != null) storyPanel.Show(completionCard, () => onCompleted?.Invoke());
            else onCompleted?.Invoke();
            return;
        }
        quizPanel.Ask(quizzes[index], () =>
        {
            index++;
            AskCurrent();
        });
    }
}
