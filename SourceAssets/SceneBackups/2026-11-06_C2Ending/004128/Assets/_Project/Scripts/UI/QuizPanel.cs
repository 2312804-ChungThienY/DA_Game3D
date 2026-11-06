using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Bảng câu hỏi 4 lựa chọn (spec phần 44). Gameplay tạm dừng, dùng map UI.
// Sai: hiện lời giải thích và bia liên quan, cho thử lại. Đúng: ghi QuizAnswered; đúng ngay lần chọn đầu
// (và chưa từng sai câu này trong snapshot) thì ghi QuizFirstTry. Esc đóng bảng không tính là trả lời.
public class QuizPanel : MonoBehaviour, IUiPauseOwner
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private Button[] optionButtons = new Button[4];
    [SerializeField] private TMP_Text explanationText;
    [SerializeField] private Button retryButton;

    private QuizDefinition current;
    private Action onSolved;
    private bool firstChoice;
    private bool awaitingRetry;

    public bool IsOpen => panel != null && panel.activeSelf;
    public QuizDefinition Current => current;

    private void Awake()
    {
        bool ok = worldState != null && panel != null && headerText != null && questionText != null &&
                  explanationText != null && retryButton != null && optionButtons != null && optionButtons.Length == 4;
        if (ok) foreach (Button b in optionButtons) ok &= b != null;
        if (!ok)
        {
            Debug.LogError($"{name}: QuizPanel thiếu tham chiếu.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            int index = i;
            optionButtons[i].onClick.AddListener(() => Choose(index));
        }
        retryButton.onClick.AddListener(ShowOptions);
        panel.SetActive(false);
    }

    public bool Ask(QuizDefinition quiz, Action solved)
    {
        GameSession session = AppRoot.Instance != null ? AppRoot.Instance.Session : null;
        if (!enabled || IsOpen || quiz == null || !quiz.IsWellFormed || !WorldIds.IsKnown(StateSet.QuizAnswered,quiz.quizId) || session == null || !session.PushUiPause(this)) return false;

        current = quiz;
        onSolved = solved;
        firstChoice = !worldState.Has(StateSet.QuizAnswered,quiz.quizId)&&!worldState.Has(StateSet.Flags,WorldIds.QuizMistakeFlag(quiz.quizId));
        headerText.text = $"Bệ Vấn Đáp – {ContentTagText.Label(quiz.tag)}";
        questionText.text = quiz.question;
        for (int i = 0; i < 4; i++)
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = $"{(char)('A' + i)}. {quiz.options[i]}";
        panel.SetActive(true);
        ShowOptions();
        return true;
    }

    // Chọn đáp án theo chỉ số (nút bấm và kiểm thử tự động dùng chung).
    public void Choose(int index)
    {
        if (!IsOpen || current == null || awaitingRetry || index<0 || index>=optionButtons.Length) return;

        if (index != current.correctIndex)
        {
            firstChoice = false;
            worldState.TryAdd(StateSet.Flags,WorldIds.QuizMistakeFlag(current.quizId));
            awaitingRetry=true;
            string lore = current.relatedLore != null ? $"\n\nXem lại bia: “{current.relatedLore.title}”." : string.Empty;
            explanationText.text = $"Chưa đúng. {current.explanation}{lore}";
            foreach (Button b in optionButtons) b.interactable = false;
            retryButton.gameObject.SetActive(true);
            return;
        }

        var result = new List<StateRequirement>{new StateRequirement{set=StateSet.QuizAnswered,id=current.quizId}};
        if (firstChoice && !worldState.Has(StateSet.Flags,WorldIds.QuizMistakeFlag(current.quizId)))
            result.Add(new StateRequirement{set=StateSet.QuizFirstTry,id=current.quizId});
        if(worldState.TryAddBatch(result,out bool changed)&&changed)worldState.PublishChange();

        Action callback = onSolved;
        Close();
        callback?.Invoke();
    }

    public void CloseFromBack() => Close();

    private void Close()
    {
        if (!IsOpen) return;
        panel.SetActive(false);
        current = null;
        onSolved = null;
        if (AppRoot.Instance != null) AppRoot.Instance.Session.PopUiPause(this);
    }

    private void ShowOptions()
    {
        if(!IsOpen||current==null)return;
        awaitingRetry=false;
        explanationText.text = "Chọn một đáp án.";
        foreach (Button b in optionButtons) b.interactable = true;
        retryButton.gameObject.SetActive(false);
    }
}
