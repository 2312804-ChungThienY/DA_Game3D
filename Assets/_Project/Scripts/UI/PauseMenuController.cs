using System.Collections;
using StarterAssets;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

[DefaultExecutionOrder(-200)]
public class PauseMenuController : MonoBehaviour
{
    [Header("Gameplay")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private StarterAssetsInputs starterInputs;
    [SerializeField] private PlayerVitals vitals;
    [SerializeField] private WorldStateStore worldState;

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject returnConfirmPanel;
    [SerializeField] private CanvasGroup pauseCanvasGroup;
    [SerializeField] private TMP_Text statusText;

    private InputAction pauseAction;
    private InputActionMap pausedGameplayMap;
    private Selectable[] pauseSelectables;
    private bool ready;
    private bool isPaused;
    private bool resuming;
    private bool leaving;
    private bool audioOwned;
    private bool previousAudioPause;
    private bool previousInputActive;
    private float previousTimeScale;
    private CursorLockMode previousCursorLock;
    private bool previousCursorVisible;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        // Independent of the map that is disabled while paused.
        pauseAction = new InputAction("PauseMenu", InputActionType.Button, "<Keyboard>/escape");

        if (starterInputs == null && playerInput != null)
            starterInputs = playerInput.GetComponent<StarterAssetsInputs>();

        ready = playerInput != null && starterInputs != null && vitals != null && worldState != null &&
            pausePanel != null && returnConfirmPanel != null && pauseCanvasGroup != null &&
            pausePanel != gameObject && !transform.IsChildOf(pausePanel.transform);

        if (!ready)
        {
            Debug.LogError("PauseMenuController: assign gameplay/UI references and keep controller outside the panel.", this);
            enabled = false;
            return;
        }
        pauseSelectables = pausePanel.GetComponentsInChildren<Selectable>(true);
        HidePanels();
    }

    private void OnEnable()
    {
        if (ready) pauseAction.Enable();
    }

    private void Update()
    {
        if (!ready || leaving) return;

        if (isPaused && (vitals.IsDead || worldState.IsRelicRestored || CheckpointRespawnFlow.IsTransitioning))
        {
            StopAllCoroutines();
            isPaused = false;
            resuming = false;
            HidePanels();
            RestoreAudio();
            // Death/completion/loading now owns the gameplay lock.
            return;
        }

        if (!Application.isFocused || CheckpointRespawnFlow.IsTransitioning ||
            !pauseAction.WasPressedThisFrame()) return;

        if (isPaused)
        {
            if (resuming) return;
            if (returnConfirmPanel.activeSelf) CancelReturnToMenu();
            else ResumeGameplay();
        }
        else if (CanPause()) PauseGameplay();
    }

    private bool CanPause()
    {
        return !isPaused && !leaving && !resuming && Time.timeScale > 0f &&
            !vitals.IsDead && !worldState.IsRelicRestored &&
            !CheckpointRespawnFlow.IsTransitioning && playerInput.isActiveAndEnabled &&
            playerInput.inputIsActive && playerInput.currentActionMap != null;
    }

    public void PauseGameplay()
    {
        if (!ready || !CanPause()) return;

        previousTimeScale = Time.timeScale;
        previousAudioPause = AudioListener.pause;
        previousInputActive = playerInput.inputIsActive;
        previousCursorLock = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        pausedGameplayMap = playerInput.currentActionMap;

        isPaused = true;
        // Set this FIRST; combat must check timeScale before disabled input.
        Time.timeScale = 0f;
        playerInput.DeactivateInput();
        ClearCachedInputs();
        AudioListener.pause = true;
        audioOwned = true;

        SetReturnConfirmation(false);
        pausePanel.SetActive(true);
        pauseCanvasGroup.interactable = true;
        pauseCanvasGroup.blocksRaycasts = true;
        SetStatus(string.Empty);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SelectFirst(false);
    }

    public void ResumeGameplay()
    {
        if (!ready || !isPaused || resuming || leaving || vitals.IsDead ||
            worldState.IsRelicRestored || CheckpointRespawnFlow.IsTransitioning) return;
        SetReturnConfirmation(false);
        StartCoroutine(ResumeAfterRelease());
    }

    private IEnumerator ResumeAfterRelease()
    {
        resuming = true;
        pauseCanvasGroup.interactable = false;
        SetStatus("Thả nút hành động để tiếp tục.");

        // Two complete input updates after release prevent the UI click from
        // becoming Attack/Jump/Focus when the gameplay map is enabled again.
        int releasedFrames = 0;
        while (releasedFrames < 2)
        {
            yield return null; // Not WaitForSeconds: timeScale is zero.
            releasedFrames = Application.isFocused && !AnyActionButtonHeld()
                ? releasedFrames + 1 : 0;
        }

        if (!isPaused || leaving || vitals.IsDead || worldState.IsRelicRestored ||
            CheckpointRespawnFlow.IsTransitioning)
        {
            resuming = false;
            yield break;
        }

        ClearCachedInputs();
        if (previousInputActive) playerInput.ActivateInput();
        RestoreAudio();
        Time.timeScale = previousTimeScale;
        Cursor.lockState = previousCursorLock;
        Cursor.visible = previousCursorVisible;
        isPaused = false;
        resuming = false;
        HidePanels();
    }

    private bool AnyActionButtonHeld()
    {
        if (pauseAction.IsPressed()) return true;
        if (pausedGameplayMap == null) return false;

        foreach (InputAction action in pausedGameplayMap.actions)
        {
            if (action.name == "Move" || action.name == "Look") continue;
            foreach (InputControl control in action.controls)
            {
                if (control is ButtonControl button && button.isPressed) return true;
            }
        }
        return false;
    }

    public void ShowReturnToMenuConfirmation()
    {
        if (!ready || !isPaused || resuming || leaving) return;
        SetReturnConfirmation(true);
        SelectFirst(true);
    }

    public void CancelReturnToMenu()
    {
        if (!ready || resuming || leaving) return;
        SetReturnConfirmation(false);
        SelectFirst(false);
    }

    public void ConfirmReturnToMenu()
    {
        if (!ready || !isPaused || resuming || leaving || !returnConfirmPanel.activeSelf) return;
        RestoreAudio();
        if (CheckpointRespawnFlow.OpenMainMenu())
        {
            leaving = true;
            isPaused = false;
            HidePanels();
            // Scene transition keeps timeScale=0 until the menu sets it to 1.
            return;
        }

        AudioListener.pause = true;
        audioOwned = true;
        SetStatus("Chưa thể về menu. Hãy thử lại.");
    }

    private void ClearCachedInputs()
    {
        if (starterInputs == null) return;
        starterInputs.move = Vector2.zero;
        starterInputs.look = Vector2.zero;
        starterInputs.jump = false;
        starterInputs.sprint = false;
    }

    private void RestoreAudio()
    {
        if (!audioOwned) return;
        AudioListener.pause = previousAudioPause;
        audioOwned = false;
    }

    private void SetReturnConfirmation(bool open)
    {
        if (returnConfirmPanel == null) return;
        returnConfirmPanel.SetActive(open);
        if (pauseSelectables == null) return;
        foreach (Selectable selectable in pauseSelectables)
        {
            if (selectable != null &&
                !selectable.transform.IsChildOf(returnConfirmPanel.transform))
                selectable.interactable = !open;
        }
    }

    private void SelectFirst(bool confirmation)
    {
        if (EventSystem.current == null || pauseSelectables == null) return;
        foreach (Selectable selectable in pauseSelectables)
        {
            if (selectable != null && selectable.isActiveAndEnabled && selectable.IsInteractable() &&
                selectable.transform.IsChildOf(returnConfirmPanel.transform) == confirmation)
            {
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
                return;
            }
        }
    }

    private void HidePanels()
    {
        SetReturnConfirmation(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (pauseCanvasGroup != null) pauseCanvasGroup.interactable = true;
        SetStatus(string.Empty);
    }

    private void SetStatus(string text)
    {
        if (statusText != null) statusText.text = text;
    }

    private void OnDisable()
    {
        if (pauseAction != null) pauseAction.Disable();
        StopAllCoroutines();
        bool ownedPause = isPaused;
        isPaused = false;
        resuming = false;
        HidePanels();
        RestoreAudio();

        // Manual component disable must not leave the game frozen, but never
        // unlock a scene while another flow is loading or showing completion.
        if (ownedPause && !CheckpointRespawnFlow.IsTransitioning &&
            vitals != null && !vitals.IsDead && worldState != null && !worldState.IsRelicRestored)
        {
            Time.timeScale = previousTimeScale;
            ClearCachedInputs();
            if (Application.isPlaying && previousInputActive && playerInput != null)
                playerInput.ActivateInput();
            Cursor.lockState = previousCursorLock;
            Cursor.visible = previousCursorVisible;
        }
    }

    private void OnDestroy()
    {
        if (pauseAction != null) pauseAction.Dispose();
    }
}
