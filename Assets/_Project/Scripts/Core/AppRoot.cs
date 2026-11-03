using UnityEngine;

// Dịch vụ dùng chung tồn tại suốt phiên chạy; được tạo đúng một lần trước scene đầu tiên.
[DefaultExecutionOrder(-1000)]
public class AppRoot : MonoBehaviour
{
    private const string UiPrefabPath = "PF_AppRootUI";

    public static AppRoot Instance { get; private set; }

    public SettingsService Settings { get; private set; }
    public InputReader Input { get; private set; }
    public GameSession Session { get; private set; }
    public AppRootUI UI { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Instance = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;

        GameObject root = new GameObject("AppRoot");
        DontDestroyOnLoad(root);
        root.AddComponent<AppRoot>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Settings = gameObject.AddComponent<SettingsService>();
        Input = gameObject.AddComponent<InputReader>();

        // Prefab UI duy nhất được tải từ Resources; chứa EventSystem và các bảng dùng chung.
        AppRootUI uiPrefab = Resources.Load<AppRootUI>(UiPrefabPath);
        if (uiPrefab == null)
        {
            Debug.LogError($"AppRoot: thiếu Resources/{UiPrefabPath}.", this);
        }
        else
        {
            UI = Instantiate(uiPrefab, transform);
            UI.name = "AppRootUI";
        }

        Session = gameObject.AddComponent<GameSession>();
    }
}
