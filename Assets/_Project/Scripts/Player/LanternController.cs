using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
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

    private InputAction toggleAction;
    private InputAction focusAction;

    private bool hasLantern;
    private bool lanternOn;
    private bool exhaustedUntilRelease;

    private float energy;
    private float timeSinceFocus;

    public bool HasLantern => hasLantern;
    public bool IsLanternOn => hasLantern && lanternOn;
    public bool IsFocusing { get; private set; }

    private void Awake()
    {
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
        PlayerInput input = GetComponent<PlayerInput>();

        toggleAction = input.actions.FindAction(
            "ToggleLantern", false);

        focusAction = input.actions.FindAction(
            "Focus", false);

        if (toggleAction == null || focusAction == null)
        {
            Debug.LogError(
                $"{name}: Thiếu action ToggleLantern hoặc Focus.",
                this);

            enabled = false;
        }
    }

    private void Update()
    {
        IsFocusing = false;

        // Không tiêu hao hoặc hồi năng lượng khi gameplay bị khóa.
        if (!Application.isFocused ||
            Time.timeScale == 0f ||
            toggleAction == null ||
            focusAction == null ||
            !toggleAction.enabled ||
            !focusAction.enabled)
        {
            RefreshVisuals();
            return;
        }

        if (hasLantern && toggleAction.WasPressedThisFrame())
        {
            lanternOn = !lanternOn;
        }

        bool focusHeld = focusAction.IsPressed();

        // Hết năng lượng phải thả chuột trước khi Focus lại.
        if (!focusHeld || !IsLanternOn)
        {
            exhaustedUntilRelease = false;
        }

        IsFocusing =
            IsLanternOn &&
            focusHeld &&
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

    public void ApplySavedState(bool owned)
    {
        hasLantern = owned;
        lanternOn = owned;
        energy = maxEnergy;
        timeSinceFocus = recoveryDelay;
        exhaustedUntilRelease = false;
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
                string state = exhaustedUntilRelease
                    ? "Het nang luong - tha chuot phai"
                    : IsFocusing ? "Dang Focus" : "Giu chuot phai de Focus";

                energyText.text =
                    $"Den: {Mathf.CeilToInt(energy)}/" +
                    $"{Mathf.CeilToInt(maxEnergy)}\n{state}";
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
