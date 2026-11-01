using UnityEngine;
using UnityEngine.InputSystem;

public class ChapterCompletionPresenter : MonoBehaviour
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject completionPanel;

    private void Awake()
    {
        if (worldState == null || playerInput == null || completionPanel == null ||
            completionPanel == gameObject || transform.IsChildOf(completionPanel.transform))
        {
            Debug.LogError($"{name}: Check completion references. Keep presenter outside the panel.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        // On load, CheckpointRespawnFlow calls RefreshFromWorldState AFTER
        // restoring the snapshot and activating input. Do not subscribe to Changed.
        if (completionPanel != null) completionPanel.SetActive(false);
    }

    public void RefreshFromWorldState()
    {
        if (!isActiveAndEnabled || worldState == null || playerInput == null || completionPanel == null)
            return;

        if (!worldState.IsRelicRestored)
        {
            completionPanel.SetActive(false);
            return;
        }

        playerInput.DeactivateInput();
        Time.timeScale = 0f;
        completionPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ReturnToMainMenu()
    {
        CheckpointRespawnFlow.OpenMainMenu();
    }
}
