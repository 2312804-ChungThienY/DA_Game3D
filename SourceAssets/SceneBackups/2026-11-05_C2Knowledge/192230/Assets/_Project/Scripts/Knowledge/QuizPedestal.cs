using System.Collections.Generic;
using UnityEngine;

// Bệ Vấn Đáp (spec phần 44, chapters/00): hỏi một câu rồi trao vật phẩm của phòng.
// Trả lời sai vẫn được thử lại và vẫn nhận đủ đồ. Túi đầy: giữ bệ, lần sau E trao lại không hỏi lại.
public class QuizPedestal : MonoBehaviour, IInteractable
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private InventoryService inventory;
    [SerializeField] private RewardService rewardService;
    [SerializeField] private QuizPanel quizPanel;
    [SerializeField] private string pedestalId;
    [SerializeField] private QuizDefinition quiz;
    [SerializeField] private List<RewardEntry> rewards = new List<RewardEntry>();
    [SerializeField] private GameObject openedVisual;
    [SerializeField] private GameObject closedVisual;

    public bool IsOpened => worldState != null && worldState.Has(StateSet.Pickups, pedestalId);
    public string Prompt => worldState.Has(StateSet.QuizAnswered, quiz.quizId)
        ? $"Nhận {RewardService.Describe(rewards, inventory)}"
        : "Bệ Vấn Đáp – trả lời câu hỏi";

    private void Awake()
    {
        if (worldState == null || inventory == null || rewardService == null || quizPanel == null || quiz == null ||
            !WorldIds.IsKnown(StateSet.Pickups, pedestalId) || !WorldIds.IsKnown(StateSet.QuizAnswered, quiz.quizId))
        {
            Debug.LogError($"{name}: QuizPedestal thiếu tham chiếu hoặc ID sai.", this);
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

    public bool CanInteract(GameObject interactor) => isActiveAndEnabled && !IsOpened;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        if (worldState.Has(StateSet.QuizAnswered, quiz.quizId)) Grant();
        else quizPanel.Ask(quiz, Grant);
    }

    private void Grant()
    {
        if (IsOpened) return;
        if (!rewardService.TryGrant(rewards, out string reason))
        {
            Toast($"{reason}: bệ vẫn giữ phần thưởng");
            return;
        }
        worldState.TryAdd(StateSet.Pickups, pedestalId);
        Toast($"Nhận {RewardService.Describe(rewards, inventory)}");
    }

    private void Apply()
    {
        bool opened = IsOpened;
        if (openedVisual != null) openedVisual.SetActive(opened);
        if (closedVisual != null) closedVisual.SetActive(!opened);
    }

    private static void Toast(string message)
    {
        if (AppRoot.Instance != null && AppRoot.Instance.UI != null) AppRoot.Instance.UI.ShowToast(message, 2.5f);
    }
}
