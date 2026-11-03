using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Quản lý phiên chơi: chuyển scene, tạm dừng, chết, hồi sinh, tiếp tục.
public class GameSession : MonoBehaviour
{
    // Lồng trong lớp để không trùng UnityEditor.SessionState.
    public enum SessionState { Boot, MainMenu, Loading, Playing, Paused, Dead, InMenu, ChapterComplete }

    public const string BootScene = "00_Boot";
    public const string MainMenuScene = "01_MainMenu";
    public const string FirstChapterScene = "02_HangLinhMach";
    public const string SecondChapterScene = "03_GiengTroiChimLac";

    private AppRoot root;
    private PlayerVitals vitals;
    private bool restoreSaveOnLoad;
    private SaveData pendingTransfer;
    private Coroutine slowMotion;
    private readonly List<IUiPauseOwner> uiOwners = new List<IUiPauseOwner>();

    public SessionState State { get; private set; } = SessionState.Boot;
    public event Action<SessionState> StateChanged;

    private void Awake() => root = GetComponent<AppRoot>();

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnbindVitals();
    }

    private void Start()
    {
        // Scene đầu tiên có thể đã nạp xong trước khi đăng ký sự kiện.
        if (State == SessionState.Boot)
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void Update()
    {
        InputReader input = root.Input;
        if (input == null || !input.IsReady) return;

        if (State == SessionState.Playing && input.Pause.WasPressedThisFrame())
        {
            Pause();
        }
        else if (input.UICancel.WasPressedThisFrame())
        {
            bool handled = root.UI != null && root.UI.HandleBack();
            if (!handled && State == SessionState.Paused) Resume();
            else if (!handled && State == SessionState.InMenu && uiOwners.Count > 0)
                uiOwners[uiOwners.Count - 1].CloseFromBack();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && State == SessionState.Playing) SetCursorLocked(true);
    }

    public void NewGame()
    {
        SaveService.DeleteSaveFiles();
        LoadScene(FirstChapterScene, false);
    }

    // Mở đúng chương và mốc đã lưu.
    public void Continue()
    {
        if (!SaveService.TryReadSaved(out SaveData data) || !CanLoadScene(data.sceneName)) return;
        LoadScene(data.sceneName, true);
    }

    public void Respawn()
    {
        // Tải lại toàn bộ scene của bản lưu rồi áp bản lưu: không trộn trạng thái phiên cũ.
        if (SaveService.TryReadSaved(out SaveData data) && CanLoadScene(data.sceneName))
            LoadScene(data.sceneName, true);
        else
            LoadScene(SceneManager.GetActiveScene().name, false);
    }

    // Chuyển chương: mang toàn bộ trạng thái (năng lực, tri thức, túi) sang scene mới,
    // đặt An ở mốc đầu chương rồi lưu một ảnh chụp tại đó (spec phần 50).
    public void LoadChapter(string sceneName, string checkpointId)
    {
        if (!CanLoadScene(sceneName) || WorldIds.SceneOfCheckpoint(checkpointId) != sceneName) return;

        SaveService current = FindAnyObjectByType<SaveService>();
        if (current == null || !current.IsReady)
        {
            Debug.LogError("GameSession: không có SaveService để mang trạng thái sang chương mới.", this);
            return;
        }

        SaveData transfer = current.Capture(checkpointId, sceneName);
        LoadScene(sceneName, false);
        pendingTransfer = transfer;
    }

    public static bool CanLoadScene(string sceneName) =>
        !string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName);

    // Né đúng lúc: chậm thời gian ngắn theo thời gian thực; không đè lên Pause/menu.
    public void RequestSlowMotion(float scale, float realSeconds)
    {
        if (State != SessionState.Playing) return;
        if (slowMotion != null) StopCoroutine(slowMotion);
        slowMotion = StartCoroutine(SlowMotionRoutine(scale, realSeconds));
    }

    private IEnumerator SlowMotionRoutine(float scale, float realSeconds)
    {
        Time.timeScale = scale;
        yield return new WaitForSecondsRealtime(realSeconds);
        if (State == SessionState.Playing) Time.timeScale = 1f;
        slowMotion = null;
    }

    public void ReturnToMainMenu() => LoadScene(MainMenuScene, false);

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Pause()
    {
        if (State != SessionState.Playing) return;

        Time.timeScale = 0f;
        root.Input.SetGameplayEnabled(false);
        SetCursorLocked(false);
        if (root.UI != null) root.UI.ShowPause(true);
        SetState(SessionState.Paused);
    }

    public void Resume()
    {
        if (State != SessionState.Paused) return;

        if (root.UI != null) root.UI.HideAll();
        Time.timeScale = 1f;
        root.Input.SetGameplayEnabled(true);
        SetCursorLocked(true);
        SetState(SessionState.Playing);
    }

    // Bảng trong gameplay (túi đồ, chế thuốc): dừng thời gian, tắt Gameplay, mở chuột.
    public bool PushUiPause(IUiPauseOwner owner)
    {
        if (owner == null || (State != SessionState.Playing && State != SessionState.InMenu)) return false;
        if (uiOwners.Contains(owner)) return true;

        uiOwners.Add(owner);
        Time.timeScale = 0f;
        root.Input.SetGameplayEnabled(false);
        SetCursorLocked(false);
        SetState(SessionState.InMenu);
        return true;
    }

    public void PopUiPause(IUiPauseOwner owner)
    {
        if (!uiOwners.Remove(owner) || uiOwners.Count > 0 || State != SessionState.InMenu) return;

        Time.timeScale = 1f;
        root.Input.SetGameplayEnabled(true);
        SetCursorLocked(true);
        SetState(SessionState.Playing);
    }

    // Kết chương: gameplay dừng hẳn, chỉ còn bảng kết.
    public void CompleteChapter()
    {
        if (State != SessionState.Playing && State != SessionState.InMenu) return;

        uiOwners.Clear();
        Time.timeScale = 0f;
        root.Input.SetGameplayEnabled(false);
        SetCursorLocked(false);
        SetState(SessionState.ChapterComplete);
    }

    private void LoadScene(string sceneName, bool restoreSave)
    {
        pendingTransfer = null;
        StopSlowMotion();
        uiOwners.Clear();
        Time.timeScale = 1f;
        root.Input.SetGameplayEnabled(false);
        if (root.UI != null) root.UI.HideAll();
        UnbindVitals();
        restoreSaveOnLoad = restoreSave;
        SetState(SessionState.Loading);
        SceneManager.LoadScene(sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode != LoadSceneMode.Single) return;

        StopAllCoroutines();
        uiOwners.Clear();
        Time.timeScale = 1f;

        if (scene.name == BootScene)
        {
            SetState(SessionState.Boot);
            return;
        }

        if (scene.name == MainMenuScene)
        {
            root.Input.SetGameplayEnabled(false);
            SetCursorLocked(false);
            if (root.UI != null) root.UI.HideAll();
            SetState(SessionState.MainMenu);
            return;
        }

        StartCoroutine(EnterGameplay());
    }

    private IEnumerator EnterGameplay()
    {
        SetState(SessionState.Loading);

        // Đợi một frame để các object trong scene chạy xong Start.
        yield return null;

        UnbindVitals();
        vitals = FindAnyObjectByType<PlayerVitals>();
        if (vitals != null) vitals.Died += OnPlayerDied;

        SaveService save = FindAnyObjectByType<SaveService>();
        if (pendingTransfer != null)
        {
            SaveData transfer = pendingTransfer;
            pendingTransfer = null;
            restoreSaveOnLoad = false;
            if (save != null && save.IsReady)
            {
                save.ApplySnapshot(transfer);
                save.SaveAt(transfer.checkpointId);
            }
        }
        else if (restoreSaveOnLoad)
        {
            restoreSaveOnLoad = false;
            if (save != null) save.LoadGame();
        }

        if (root.UI != null) root.UI.HideAll();
        root.Input.SetGameplayEnabled(true);
        SetCursorLocked(true);
        SetState(SessionState.Playing);
    }

    private void OnPlayerDied()
    {
        if (State != SessionState.Playing && State != SessionState.Paused && State != SessionState.InMenu) return;

        uiOwners.Clear();
        Time.timeScale = 1f;
        root.Input.SetGameplayEnabled(false);
        SetCursorLocked(false);
        if (root.UI != null) root.UI.ShowDeath(true);
        SetState(SessionState.Dead);
    }

    private void UnbindVitals()
    {
        if (vitals != null) vitals.Died -= OnPlayerDied;
        vitals = null;
    }

    private void StopSlowMotion()
    {
        if (slowMotion != null) StopCoroutine(slowMotion);
        slowMotion = null;
    }

    private void SetState(SessionState next)
    {
        if (State == next) return;
        State = next;
        StateChanged?.Invoke(next);
    }

    private static void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}

// Bảng gameplay giữ quyền tạm dừng; Esc đóng bảng trên cùng.
public interface IUiPauseOwner
{
    void CloseFromBack();
}
