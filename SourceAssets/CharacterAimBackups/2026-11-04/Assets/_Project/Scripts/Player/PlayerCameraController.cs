using UnityEngine;

// Xoay điểm bám camera (PlayerCameraRoot) độc lập với hướng mô hình.
[DefaultExecutionOrder(50)]
public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float topClamp = 65f;
    [SerializeField] private float bottomClamp = -35f;

    private InputReader input;
    private SettingsService settings;
    private float yaw;
    private float pitch;
    private LockOnController lockOn;

    public float Yaw => yaw;
    public float Pitch => pitch;

    private void Awake()
    {
        if (cameraTarget == null)
        {
            Debug.LogError($"{name}: PlayerCameraController thiếu Camera Target.", this);
            enabled = false;
            return;
        }
        yaw = cameraTarget.eulerAngles.y;
        pitch = Mathf.DeltaAngle(0f, cameraTarget.eulerAngles.x);
        lockOn = GetComponent<LockOnController>();
    }

    private void Start()
    {
        input = AppRoot.Instance != null ? AppRoot.Instance.Input : null;
        settings = AppRoot.Instance != null ? AppRoot.Instance.Settings : null;
        if (input == null)
        {
            Debug.LogError($"{name}: PlayerCameraController không tìm thấy InputReader.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (!input.GameplayActive || Time.timeScale <= 0f) return;
        Vector2 look = input.LookValue;
        if (look.sqrMagnitude >= 0.0001f && (lockOn == null || !lockOn.HasTarget))
        {
            float sensitivity = settings != null ? settings.MouseSensitivity : SettingsService.DefaultSensitivity;
            yaw += look.x * sensitivity;
            pitch += look.y * sensitivity;
        }

        if (lockOn != null && lockOn.HasTarget && lockOn.Aim != null)
        {
            Vector3 d = lockOn.AimPosition - (transform.position + Vector3.up * 1.6f);
            float desiredYaw = Mathf.Atan2(d.x, d.z) * Mathf.Rad2Deg;
            float desiredPitch = Mathf.Clamp(Mathf.Atan2(-d.y, new Vector2(d.x, d.z).magnitude) * Mathf.Rad2Deg + 10f, -10f, 35f);
            float blend = 1f - Mathf.Exp(-lockOn.CameraTurnSpeed * Time.deltaTime);
            yaw = Mathf.LerpAngle(yaw, desiredYaw, blend);
            pitch = Mathf.Lerp(pitch, desiredPitch, blend);
        }
        yaw = Mathf.Repeat(yaw + 180f, 360f) - 180f;
        pitch = Mathf.Clamp(pitch, bottomClamp, topClamp);
        cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    // Đặt góc nhìn (dùng khi hồi sinh và khi kiểm thử tự động).
    public void SetAngles(float newYaw, float newPitch)
    {
        yaw = newYaw;
        pitch = Mathf.Clamp(newPitch, bottomClamp, topClamp);
        if (cameraTarget != null) cameraTarget.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
