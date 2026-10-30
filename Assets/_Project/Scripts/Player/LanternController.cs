using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class LanternController : MonoBehaviour
{
    [SerializeField] private GameObject lanternVisual;

    private PlayerInput playerInput;
    private InputAction toggleAction;

    private bool hasLantern;
    private bool lanternOn;

    public bool HasLantern => hasLantern;
    public bool IsLanternOn => hasLantern && lanternOn;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (lanternVisual == null)
        {
            Debug.LogError(
                $"{name}: Chưa gán Lantern Visual.", this);

            enabled = false;
            return;
        }

        ApplyVisual();
    }

    private void Start()
    {
        // Đọc action từ bộ input thực tế của nhân vật.
        toggleAction = playerInput.actions.FindAction(
            "ToggleLantern", false);

        if (toggleAction == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy action ToggleLantern.",
                this);

            enabled = false;
        }
    }

    private void Update()
    {
        if (!hasLantern ||
            !Application.isFocused ||
            Time.timeScale == 0f ||
            toggleAction == null ||
            !toggleAction.enabled)
        {
            return;
        }

        if (toggleAction.WasPressedThisFrame())
        {
            lanternOn = !lanternOn;
            ApplyVisual();
        }
    }

    public bool TryReceiveLantern()
    {
        if (!isActiveAndEnabled || hasLantern)
        {
            return false;
        }

        hasLantern = true;
        lanternOn = true;
        ApplyVisual();

        Debug.Log(
            "Đã nhận Lồng Đèn Sen. Nhấn F để bật/tắt.",
            this);

        return true;
    }

    private void OnEnable()
    {
        ApplyVisual();
    }

    private void OnDisable()
    {
        if (lanternVisual != null)
        {
            lanternVisual.SetActive(false);
        }
    }

    private void ApplyVisual()
    {
        if (lanternVisual != null)
        {
            lanternVisual.SetActive(IsLanternOn);
        }
    }
}