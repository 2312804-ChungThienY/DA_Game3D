using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Nhật ký (phím J): mục tiêu hiện tại, các bia đã đọc (mục Tri thức) và Điểm Tri Thức.
// Điểm suy ra từ tập QuizFirstTry, không lưu bộ đếm riêng (spec phần 44).
public class JournalPanel : MonoBehaviour, IUiPauseOwner
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text objectiveSource;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private TMP_Text knowledgeText;
    [SerializeField] private TMP_Text pointsText;
    [SerializeField] private Button closeButton;
    [SerializeField] private LoreDefinition[] allLore;

    private InputReader input;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (worldState == null || panel == null || objectiveText == null || knowledgeText == null ||
            pointsText == null || closeButton == null || allLore == null)
        {
            Debug.LogError($"{name}: JournalPanel thiếu tham chiếu.", this);
            enabled = false;
            return;
        }
        closeButton.onClick.AddListener(Close);
        panel.SetActive(false);
    }

    private void Start() => input = AppRoot.Instance != null ? AppRoot.Instance.Input : null;

    private void Update()
    {
        if (input == null || !input.IsReady || !input.Journal.WasPressedThisFrame()) return;
        if (IsOpen) Close();
        else if (input.GameplayEnabled) Open();
    }

    public void Open()
    {
        GameSession session = AppRoot.Instance != null ? AppRoot.Instance.Session : null;
        if (!enabled || IsOpen || session == null || !session.PushUiPause(this)) return;

        // Gameplay đã tắt; bật riêng phím J để đóng bằng chính phím mở.
        input.Journal.Enable();
        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (!IsOpen) return;
        panel.SetActive(false);
        if (AppRoot.Instance != null) AppRoot.Instance.Session.PopUiPause(this);
        if (input != null && !input.GameplayEnabled) input.Journal.Disable();
    }

    public void CloseFromBack() => Close();

    private void Refresh()
    {
        string objective = objectiveSource != null ? objectiveSource.text.Replace("MỤC TIÊU\n", string.Empty) : string.Empty;
        objectiveText.text = $"<b>Mục tiêu</b>\n{objective}";

        StringBuilder text = new StringBuilder("<b>Tri thức</b>\n");
        int read = 0;
        foreach (LoreDefinition lore in allLore)
        {
            if (lore == null || !worldState.Has(StateSet.Lore, lore.loreId)) continue;
            read++;
            text.Append($"\n<b>{lore.title}</b> <i>({ContentTagText.Label(lore.tag)})</i>\n{lore.body}\n");
        }
        if (read == 0) text.Append("\nChưa đọc bia nào. Bật đèn (F) và lại gần bia đá để đọc.");
        knowledgeText.text = text.ToString();
        pointsText.text = $"Bia đã đọc: {read}/{allLore.Length}    Điểm Tri Thức: {worldState.Count(StateSet.QuizFirstTry)}";
    }
}
