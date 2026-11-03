using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Bảng ảnh tĩnh và lời dẫn. Hai chế độ: thẻ thường (tạm dừng gameplay, Tiếp tục để chơi tiếp)
// và kết chương (dừng hẳn, nút chuyển sang chương sau hoặc về menu).
public class StoryCardPanel : MonoBehaviour, IUiPauseOwner
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button continueButton;

    private Action onClosed;
    private bool chapterEnd;
    private string nextScene;
    private string nextCheckpoint;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (panel == null || titleText == null || bodyText == null || continueButton == null)
        {
            Debug.LogError($"{name}: StoryCardPanel thiếu tham chiếu.", this);
            enabled = false;
            return;
        }

        continueButton.onClick.AddListener(Continue);
        panel.SetActive(false);
    }

    public void Show(StoryCard card, Action closed = null)
    {
        if (card != null) Show(card.title, card.body, closed);
    }

    public void Show(string title, string body, Action closed = null)
    {
        GameSession session = AppRoot.Instance != null ? AppRoot.Instance.Session : null;
        if (!enabled || IsOpen || session == null || !session.PushUiPause(this)) return;

        chapterEnd = false;
        onClosed = closed;
        Open(title, body, "Tiếp tục");
    }

    // Kết chương: dừng gameplay rồi hiện thẻ; nút chuyển sang scene chương sau nếu đã có.
    public void ShowChapterEnd(string title, string body, string sceneNext = null, string checkpointNext = null)
    {
        if (!enabled) return;
        if (AppRoot.Instance != null) AppRoot.Instance.Session.CompleteChapter();

        chapterEnd = true;
        nextScene = sceneNext;
        nextCheckpoint = checkpointNext;
        bool hasNext = GameSession.CanLoadScene(nextScene);
        Open(title, body, hasNext ? "Sang chương tiếp theo" : "Về menu chính");
    }

    private void Open(string title, string body, string buttonLabel)
    {
        titleText.text = title;
        bodyText.text = body;
        TMP_Text label = continueButton.GetComponentInChildren<TMP_Text>();
        if (label != null) label.text = buttonLabel;
        panel.SetActive(true);
    }

    public void CloseFromBack()
    {
        if (!chapterEnd) Continue();
    }

    private void Continue()
    {
        if (!IsOpen) return;
        panel.SetActive(false);
        GameSession session = AppRoot.Instance != null ? AppRoot.Instance.Session : null;
        if (session == null) return;

        if (chapterEnd)
        {
            if (GameSession.CanLoadScene(nextScene)) session.LoadChapter(nextScene, nextCheckpoint);
            else session.ReturnToMainMenu();
            return;
        }

        session.PopUiPause(this);
        Action callback = onClosed;
        onClosed = null;
        callback?.Invoke();
    }
}
