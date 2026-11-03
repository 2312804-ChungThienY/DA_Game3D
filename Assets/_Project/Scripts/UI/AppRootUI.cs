using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Các bảng dùng chung cho mọi scene: tạm dừng, cài đặt, màn chết, hộp xác nhận, thông báo ngắn.
public class AppRootUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject confirmPanel;

    [Header("Pause")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button pauseSettingsButton;
    [SerializeField] private Button pauseMenuButton;

    [Header("Death")]
    [SerializeField] private Button respawnButton;
    [SerializeField] private Button deathMenuButton;

    [Header("Settings")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button settingsBackButton;

    [Header("Confirm")]
    [SerializeField] private TMP_Text confirmText;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;

    [Header("Toast")]
    [SerializeField] private TMP_Text toastText;

    private Action pendingConfirm;
    private Coroutine toastRoutine;

    private static GameSession Session => AppRoot.Instance != null ? AppRoot.Instance.Session : null;
    private static SettingsService Settings => AppRoot.Instance != null ? AppRoot.Instance.Settings : null;

    private void Awake()
    {
        if (pausePanel == null || settingsPanel == null || deathPanel == null || confirmPanel == null ||
            resumeButton == null || pauseSettingsButton == null || pauseMenuButton == null ||
            respawnButton == null || deathMenuButton == null || volumeSlider == null ||
            sensitivitySlider == null || fullscreenToggle == null || settingsBackButton == null ||
            confirmText == null || confirmYesButton == null || confirmNoButton == null || toastText == null)
        {
            Debug.LogError($"{name}: AppRootUI thiếu tham chiếu.", this);
            enabled = false;
            return;
        }

        resumeButton.onClick.AddListener(() => Session?.Resume());
        pauseSettingsButton.onClick.AddListener(OpenSettings);
        pauseMenuButton.onClick.AddListener(() => Session?.ReturnToMainMenu());
        respawnButton.onClick.AddListener(() => Session?.Respawn());
        deathMenuButton.onClick.AddListener(() => Session?.ReturnToMainMenu());
        settingsBackButton.onClick.AddListener(CloseSettings);
        confirmYesButton.onClick.AddListener(ConfirmYes);
        confirmNoButton.onClick.AddListener(CloseConfirm);

        volumeSlider.minValue = 0f;
        volumeSlider.maxValue = 1f;
        sensitivitySlider.minValue = SettingsService.MinSensitivity;
        sensitivitySlider.maxValue = SettingsService.MaxSensitivity;
        volumeSlider.onValueChanged.AddListener(v => Settings?.SetMasterVolume(v));
        sensitivitySlider.onValueChanged.AddListener(v => Settings?.SetMouseSensitivity(v));
        fullscreenToggle.onValueChanged.AddListener(v => Settings?.SetFullscreen(v));

        HideAll();
    }

    public void HideAll()
    {
        if (!enabled) return;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        deathPanel.SetActive(false);
        confirmPanel.SetActive(false);
        pendingConfirm = null;
    }

    public void ShowPause(bool show)
    {
        if (!enabled) return;
        settingsPanel.SetActive(false);
        pausePanel.SetActive(show);
    }

    public void ShowDeath(bool show)
    {
        if (!enabled) return;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        deathPanel.SetActive(show);
    }

    public void OpenSettings()
    {
        if (!enabled) return;
        SettingsService settings = Settings;
        if (settings != null)
        {
            volumeSlider.SetValueWithoutNotify(settings.MasterVolume);
            sensitivitySlider.SetValueWithoutNotify(settings.MouseSensitivity);
            fullscreenToggle.SetIsOnWithoutNotify(settings.Fullscreen);
        }
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (!enabled) return;
        settingsPanel.SetActive(false);
    }

    public void ShowConfirm(string message, Action onYes)
    {
        if (!enabled) return;
        confirmText.text = message;
        pendingConfirm = onYes;
        confirmPanel.SetActive(true);
    }

    // Đóng lớp trên cùng. Trả về false nếu không có lớp phụ nào để đóng.
    public bool HandleBack()
    {
        if (!enabled) return false;
        if (confirmPanel.activeSelf)
        {
            CloseConfirm();
            return true;
        }
        if (settingsPanel.activeSelf)
        {
            CloseSettings();
            return true;
        }
        return false;
    }

    public void ShowToast(string message, float seconds = 2f)
    {
        if (!enabled) return;
        if (toastRoutine != null) StopCoroutine(toastRoutine);
        toastRoutine = StartCoroutine(ToastRoutine(message, seconds));
    }

    private IEnumerator ToastRoutine(string message, float seconds)
    {
        toastText.text = message;
        toastText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(seconds);
        toastText.gameObject.SetActive(false);
        toastRoutine = null;
    }

    private void ConfirmYes()
    {
        Action action = pendingConfirm;
        CloseConfirm();
        action?.Invoke();
    }

    private void CloseConfirm()
    {
        pendingConfirm = null;
        confirmPanel.SetActive(false);
    }
}
