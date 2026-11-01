using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private GameObject newGameConfirmPanel;
    [SerializeField] private TMP_Text statusText;
    private bool busy;
    private bool canContinue;
    private bool confirmationOpen;

    private void Awake()
    {
        CheckpointRespawnFlow.NotifyMenuReady();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (newGameButton == null || continueButton == null || quitButton == null ||
            newGameConfirmPanel == null || statusText == null)
        {
            Debug.LogError("MainMenuController: assign buttons, confirmation panel and text.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        newGameConfirmPanel.SetActive(false);
        RefreshContinue();
    }

    private void RefreshContinue()
    {
        canContinue = GameSaveStorage.TryReadBest(out SaveData data, out bool backup);
        statusText.text = canContinue
            ? (backup ? "Co the tiep tuc tu ban luu du phong." : "")
            : (GameSaveStorage.HasAnySaveFiles ? "Chua co ban luu su dung duoc." : "");
        SetBusy(false);
    }

    private void SetBusy(bool value)
    {
        busy = value;
        newGameButton.interactable = !busy && !confirmationOpen;
        continueButton.interactable = !busy && !confirmationOpen && canContinue;
        quitButton.interactable = !busy && !confirmationOpen;
    }

    public void NewGame()
    {
        if (busy || confirmationOpen) return;
        if (GameSaveStorage.HasAnySaveFiles)
        {
            confirmationOpen = true;
            newGameConfirmPanel.SetActive(true);
            SetBusy(false);
        }
        else ConfirmNewGame();
    }

    public void ConfirmNewGame()
    {
        if (busy) return;
        confirmationOpen = false;
        newGameConfirmPanel.SetActive(false);
        SetBusy(true);
        statusText.text = "Dang bat dau...";
        if (!CheckpointRespawnFlow.StartNewGameFromMenu())
        {
            SetBusy(false);
            statusText.text = "Chua the mo man choi.";
        }
    }

    public void CancelNewGame()
    {
        if (busy) return;
        confirmationOpen = false;
        newGameConfirmPanel.SetActive(false);
        SetBusy(false);
    }

    public void ContinueGame()
    {
        if (busy || confirmationOpen) return;
        if (!GameSaveStorage.TryReadBest(out SaveData data, out bool backup))
        {
            RefreshContinue();
            statusText.text = "Chua co ban luu su dung duoc.";
            return;
        }
        SetBusy(true);
        statusText.text = backup ? "Dang tai ban du phong..." : "Dang tai...";
        if (!CheckpointRespawnFlow.ContinueFromMenu(data))
        {
            SetBusy(false);
            statusText.text = "Chua the mo man choi.";
        }
    }

    public void QuitGame()
    {
        if (busy || confirmationOpen) return;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
