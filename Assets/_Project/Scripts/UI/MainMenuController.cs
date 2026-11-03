using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        if (newGameButton == null || continueButton == null || settingsButton == null || quitButton == null)
        {
            Debug.LogError($"{name}: MainMenuController thiếu nút.", this);
            enabled = false;
            return;
        }

        newGameButton.onClick.AddListener(OnNewGame);
        continueButton.onClick.AddListener(() => AppRoot.Instance.Session.Continue());
        settingsButton.onClick.AddListener(() => AppRoot.Instance.UI.OpenSettings());
        quitButton.onClick.AddListener(() => AppRoot.Instance.Session.QuitGame());
    }

    private void Start()
    {
        // Tiếp tục chỉ bật khi có bản lưu hợp lệ.
        continueButton.interactable = SaveService.HasValidSave();
    }

    private void OnNewGame()
    {
        if (SaveService.HasValidSave())
        {
            AppRoot.Instance.UI.ShowConfirm(
                "Chơi mới sẽ xóa tiến trình đã lưu. Bạn có chắc không?",
                () => AppRoot.Instance.Session.NewGame());
        }
        else
        {
            AppRoot.Instance.Session.NewGame();
        }
    }
}
