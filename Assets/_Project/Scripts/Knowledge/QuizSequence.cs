using System.Collections.Generic;
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

    private int index, generation;
    private uint restoreVersion;
    private bool running, committing, awaitingSave;
    public bool IsAwaitingSave => awaitingSave;

    public bool IsDone => worldState != null && worldState.Has(StateSet.Puzzles, sequenceId);
    private bool IsAvailable => worldState != null && (string.IsNullOrEmpty(requiredPuzzleId) || worldState.Has(StateSet.Puzzles, requiredPuzzleId));
    public string Prompt => IsDone ? $"Đọc lại lời {speakerName}" : awaitingSave ? "Lưu hoàn tất trò chuyện" : $"Trò chuyện với {speakerName} ({quizzes.Length} câu hỏi)";

    private void Awake()
    {
        bool ok = worldState && quizPanel && storyPanel && saveService && quizzes != null && quizzes.Length > 0 &&
            WorldIds.IsKnown(StateSet.Puzzles, sequenceId) && WorldIds.SceneOfCheckpoint(saveCheckpointId) != null &&
            (string.IsNullOrEmpty(requiredPuzzleId) || WorldIds.IsKnown(StateSet.Puzzles, requiredPuzzleId));
        var ids = new HashSet<string>();
        if (ok) foreach (var q in quizzes) ok &= q && q.IsWellFormed && WorldIds.IsKnown(StateSet.QuizAnswered, q.quizId) && ids.Add(q.quizId);
        if (!ok)
        {
            Debug.LogError($"{name}: QuizSequence thiếu tham chiếu hoặc ID sai.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (worldState != null) { restoreVersion = worldState.RestoreVersion; worldState.Changed += Apply; }
    }

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= Apply;
        Cancel(true);
    }

    private void Start() => Apply();

    private void Apply()
    {
        if (!committing && (restoreVersion != worldState.RestoreVersion || !IsAvailable)) Cancel(true);
        restoreVersion = worldState.RestoreVersion;
        if (visual != null) visual.SetActive(IsAvailable);
    }

    private void Cancel(bool closePanel)
    {
        bool owns = running && quizzes != null && index < quizzes.Length && quizPanel && quizPanel.Current == quizzes[index];
        running = awaitingSave = false;
        index = 0;
        generation++;
        if (closePanel && owns) quizPanel.CloseFromBack();
    }

    public bool CanInteract(GameObject interactor) => isActiveAndEnabled && IsAvailable && !running && !committing &&
        AppRoot.Instance != null && AppRoot.Instance.Session.State == GameSession.SessionState.Playing;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        if (IsDone) { if (completionCard) storyPanel.Show(completionCard); return; }
        if (awaitingSave) { Commit(); return; }
        running = true;
        index = 0;
        generation++;
        AskCurrent();
    }

    private void AskCurrent()
    {
        if (index == quizzes.Length) { Commit(); return; }
        int ticket = generation, question = index;
        if (!quizPanel.Ask(quizzes[index], () =>
        {
            if (!running || ticket != generation || question != index || !IsAvailable) return;
            index++;
            AskCurrent();
        }, () => { if (ticket == generation) Cancel(false); }, $"{speakerName} · Câu {index + 1}/{quizzes.Length}")) Cancel(false);
    }

    private void Commit()
    {
        if (committing || IsDone || !IsAvailable) return;
        foreach (var q in quizzes) if (!worldState.Has(StateSet.QuizAnswered, q.quizId)) { Cancel(false); return; }
        committing = true;
        try
        {
            var before = worldState.Export();
            var result = new List<StateRequirement> { new StateRequirement { set = StateSet.Puzzles, id = sequenceId } };
            if (!worldState.TryAddBatch(result, out bool changed)) { Cancel(false); return; }
            if (!saveService.SaveAt(saveCheckpointId))
            {
                worldState.Restore(before);
                running = false;
                awaitingSave = true;
                restoreVersion = worldState.RestoreVersion;
                AppRoot.Instance?.UI?.ShowToast("Chưa lưu được lời Chim Lạc. Nhấn E để thử lưu lại; không cần trả lời lại trong phiên này.", 5);
                return;
            }
            running = awaitingSave = false;
            index = 0;
            if (changed) worldState.PublishChange();
            int ticket = ++generation;
            uint version = worldState.RestoreVersion;
            System.Action completed = () => { if (this && isActiveAndEnabled && ticket == generation && version == worldState.RestoreVersion && IsDone) onCompleted?.Invoke(); };
            if (completionCard) storyPanel.Show(completionCard, completed);
            else completed();
        }
        finally { committing = false; }
    }
}
