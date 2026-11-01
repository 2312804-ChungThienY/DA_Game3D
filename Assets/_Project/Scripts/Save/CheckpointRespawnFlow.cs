using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-500)]
public class CheckpointRespawnFlow : MonoBehaviour
{
    public const string MenuScene = "01_MainMenu";
    public const string GameplayScene = "02_HangLinhMach";

    private enum LaunchMode { None, NewGame, Continue }
    private static LaunchMode pendingMode;
    private static SaveData pendingSnapshot;
    private static bool transitioning;

    [SerializeField] private SaveService saveService;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject loadingErrorPanel;

    private LaunchMode activeMode;
    private SaveData activeSnapshot;
    private bool initializing;

    public static bool IsTransitioning => transitioning;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetPendingState()
    {
        pendingMode = LaunchMode.None;
        pendingSnapshot = null;
        transitioning = false;
    }

    public static bool StartNewGameFromMenu() =>
        LoadSceneRequest(GameplayScene, LaunchMode.NewGame, null);

    public static bool ContinueFromMenu(SaveData data)
    {
        return GameSaveStorage.IsValid(data) &&
            LoadSceneRequest(GameplayScene, LaunchMode.Continue, data);
    }

    public static bool OpenMainMenu() => LoadSceneRequest(MenuScene, LaunchMode.None, null);

    public static void NotifyMenuReady()
    {
        pendingMode = LaunchMode.None;
        pendingSnapshot = null;
        transitioning = false;
    }

    private static bool LoadSceneRequest(string scene, LaunchMode mode, SaveData data)
    {
        if (transitioning) return false;
        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError($"Scene is missing from the build Scene List: {scene}");
            return false;
        }

        float previousTimeScale = Time.timeScale;
        pendingMode = mode;
        pendingSnapshot = data;
        transitioning = true;
        Time.timeScale = 0f;

        try
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
            if (operation == null) throw new InvalidOperationException("Scene load could not start.");
            return true;
        }
        catch (Exception exception)
        {
            ResetPendingState();
            Time.timeScale = previousTimeScale;
            Debug.LogException(exception);
            return false;
        }
    }

    // Existing SaveService and death-screen button callers keep using this API.
    public void ReloadWithSnapshot(SaveData data)
    {
        ContinueFromMenu(data);
    }

    private IEnumerator Start()
    {
        if (loadingErrorPanel != null) loadingErrorPanel.SetActive(false);

        activeMode = pendingMode;
        activeSnapshot = pendingSnapshot;
        ResetPendingState();

        if (activeMode == LaunchMode.None)
        {
            // Direct scene testing in Editor: no save is modified.
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            yield break;
        }

        yield return InitializeCurrentRequest();
    }

    private IEnumerator InitializeCurrentRequest()
    {
        initializing = true;
        Time.timeScale = 0f;
        if (loadingErrorPanel != null) loadingErrorPanel.SetActive(false);
        if (playerInput != null) playerInput.DeactivateInput();
        yield return null; // Let SaveService and other scene components finish Start.

        bool restored = false;
        try
        {
            if (saveService != null && playerInput != null)
            {
                restored = activeMode == LaunchMode.NewGame
                    ? saveService.TryStartNewGame()
                    : saveService.ApplySnapshot(activeSnapshot);
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }

        initializing = false;
        if (!restored)
        {
            if (loadingErrorPanel != null) loadingErrorPanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.LogError("Gameplay initialization failed; play remains locked.", this);
            yield break;
        }

        activeMode = LaunchMode.None;
        activeSnapshot = null;
        Time.timeScale = 1f;
        playerInput.ActivateInput();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ChapterCompletionPresenter completion = GetComponent<ChapterCompletionPresenter>();
        if (completion != null) completion.RefreshFromWorldState();
    }

    public void RetryInitialization()
    {
        if (initializing || activeMode == LaunchMode.None) return;
        // Retry through a fresh scene so initialization and ready checks run again.
        if (activeMode == LaunchMode.NewGame) StartNewGameFromMenu();
        else ContinueFromMenu(activeSnapshot);
    }

    public void ReturnToMainMenu()
    {
        OpenMainMenu();
    }
}
