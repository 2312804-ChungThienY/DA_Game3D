using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-30)]
public class LanternController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject lanternVisual;
    [SerializeField] private Light focusLight;
    [SerializeField] private Camera viewCamera;
    [SerializeField] private TMP_Text energyText;

    [Header("Energy")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float focusCost = 12f;
    [SerializeField] private float recoveryRate = 20f;
    [SerializeField] private float recoveryDelay = 1f;

    private InputReader input;
    private PlayerActionState actions;
    private InputAction toggleAction;
    private InputAction focusAction;

    private bool hasLantern;
    private bool lanternOn;
    private bool exhaustedUntilRelease;
    private bool interruptedUntilRelease;

    private float energy;
    private float timeSinceFocus;

    public bool HasLantern => hasLantern;
    public bool IsLanternOn => hasLantern && lanternOn;
    public bool IsFocusing { get; private set; }

    private void Awake()
    {
        actions = GetComponent<PlayerActionState>();
        energy = maxEnergy;
        timeSinceFocus = recoveryDelay;

        if (lanternVisual == null ||
            focusLight == null ||
            viewCamera == null ||
            energyText == null)
        {
            Debug.LogError(
                $"{name}: LanternController thiếu tham chiếu.",
                this);

            enabled = false;
        }

        RefreshVisuals();
    }

    private void Start()
    {
        input = AppRoot.Instance != null ? AppRoot.Instance.Input : null;
        toggleAction = input != null && input.IsReady ? input.ToggleLantern : null;
        focusAction = input != null && input.IsReady ? input.Focus : null;

        if (toggleAction == null || focusAction == null)
        {
            Debug.LogError(
                $"{name}: Không tìm thấy action ToggleLantern hoặc Focus trong InputReader.",
                this);

            enabled = false;
        }
    }

    private void Update()
    {
        IsFocusing = false;

        // Không tiêu hao hoặc hồi năng lượng khi gameplay bị khóa.
        if (input == null || !input.GameplayActive || !Application.isFocused ||
            Time.timeScale == 0f ||
            toggleAction == null ||
            focusAction == null ||
            !toggleAction.enabled ||
            !focusAction.enabled)
        {
            RefreshVisuals();
            return;
        }

        if (hasLantern && input.WasPressed(toggleAction))
        {
            lanternOn = !lanternOn;
        }

        bool focusHeld = input.IsHeld(focusAction);
        bool canFocus = actions == null || actions.CanFocus;
        if (focusHeld && (input.WasPressed(input.Attack) || input.IsHeld(input.Attack))) interruptedUntilRelease = true;
        if (!focusHeld) interruptedUntilRelease = false;

        // Hết năng lượng phải thả chuột trước khi Focus lại.
        if (!focusHeld || !IsLanternOn)
        {
            exhaustedUntilRelease = false;
        }

        IsFocusing =
            IsLanternOn &&
            focusHeld &&
            canFocus &&
            !interruptedUntilRelease &&
            !exhaustedUntilRelease &&
            energy > 0f;

        if (IsFocusing)
        {
            timeSinceFocus = 0f;
            energy = Mathf.Max(
                0f, energy - focusCost * Time.deltaTime);

            if (energy <= 0f)
            {
                IsFocusing = false;
                exhaustedUntilRelease = true;

                Debug.Log(
                    "Hết năng lượng. Thả chuột phải để chờ hồi.",
                    this);
            }
        }
        else if (hasLantern)
        {
            timeSinceFocus += Time.deltaTime;

            if (timeSinceFocus >= recoveryDelay)
            {
                energy = Mathf.Min(
                    maxEnergy,
                    energy + recoveryRate * Time.deltaTime);
            }
        }

        RefreshVisuals();
    }

    private void LateUpdate()
    {
        if (IsFocusing && focusLight != null && viewCamera != null)
        {
            // Nguồn sáng ở đèn, hướng chiếu theo camera.
            focusLight.transform.rotation = viewCamera.transform.rotation;
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
        energy = maxEnergy;

        RefreshVisuals();
        return true;
    }

    public float Energy => energy;
    public float MaxEnergy => maxEnergy;

    // Cộng năng lượng từ nguồn ngoài (ví dụ vòm sáng Kim Quy ở chương 5).
    public void AddEnergy(float amount)
    {
        if (!hasLantern || amount <= 0f || float.IsNaN(amount) || float.IsInfinity(amount)) return;
        energy = Mathf.Min(maxEnergy, energy + amount);
        RefreshVisuals();
    }

    public void RefillEnergy()
    {
        energy = maxEnergy;
        exhaustedUntilRelease = false;
        RefreshVisuals();
    }

    public void ApplySavedState(bool owned)
    {
        hasLantern = owned;
        lanternOn = owned;
        energy = maxEnergy;
        timeSinceFocus = recoveryDelay;
        exhaustedUntilRelease = false;
        interruptedUntilRelease = false;
        IsFocusing = false;
        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        if (lanternVisual != null)
        {
            lanternVisual.SetActive(IsLanternOn);
        }

        if (focusLight != null)
        {
            focusLight.enabled = IsLanternOn && IsFocusing;
        }

        if (energyText != null)
        {
            if (!hasLantern)
            {
                energyText.text = string.Empty;
            }
            else
            {
                string state = interruptedUntilRelease
                    ? "Thả rồi giữ lại chuột phải để chiếu"
                    : exhaustedUntilRelease
                    ? "Hết năng lượng – thả chuột phải để hồi"
                    : IsFocusing ? "Đang chiếu tập trung" : "Giữ chuột phải để chiếu tập trung";

                // Số năng lượng hiện ở thanh HUD; dòng này chỉ là gợi ý thao tác.
                energyText.text = state;
            }
        }
    }

    private void OnDisable()
    {
        IsFocusing = false;

        if (focusLight != null)
        {
            focusLight.enabled = false;
        }

        if (lanternVisual != null)
        {
            lanternVisual.SetActive(false);
        }
    }
}
