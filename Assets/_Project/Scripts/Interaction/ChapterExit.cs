using UnityEngine;

// Cổng sang chương sau (spec chapters/00): hiện khi requiredPuzzleId đã có; E → GameSession.LoadChapter.
public class ChapterExit : MonoBehaviour, IInteractable
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private string requiredPuzzleId = WorldIds.RelicRestoredC1;
    [SerializeField] private string nextScene = GameSession.SecondChapterScene;
    [SerializeField] private string nextCheckpointId = "c2_cp_start";
    [SerializeField] private string label = "Đi theo luồng gió sang chương 2";
    [SerializeField] private GameObject portalVisual;

    [Header("Khi chương sau chưa có trong bản build")]
    [SerializeField] private StoryCardPanel storyPanel;
    [SerializeField] private string endTitle = "Chương hoàn thành";
    [SerializeField, TextArea(3, 8)] private string endBody = "Chương tiếp theo đang được xây dựng. Tiến trình đã được lưu.";

    public string Prompt => label;

    private void Awake()
    {
        if (worldState == null || portalVisual == null || !WorldIds.IsKnown(StateSet.Puzzles, requiredPuzzleId))
        {
            Debug.LogError($"{name}: ChapterExit thiếu tham chiếu hoặc ID sai.", this);
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

    private bool Open => worldState.Has(StateSet.Puzzles, requiredPuzzleId);

    private void Apply() => portalVisual.SetActive(Open);

    public bool CanInteract(GameObject interactor) => isActiveAndEnabled && Open;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;

        if (!GameSession.CanLoadScene(nextScene))
        {
            if (storyPanel != null) storyPanel.ShowChapterEnd(endTitle, endBody);
            else if (AppRoot.Instance != null && AppRoot.Instance.UI != null)
                AppRoot.Instance.UI.ShowToast("Chương tiếp theo chưa có trong bản này");
            return;
        }
        AppRoot.Instance.Session.LoadChapter(nextScene, nextCheckpointId);
    }
}
