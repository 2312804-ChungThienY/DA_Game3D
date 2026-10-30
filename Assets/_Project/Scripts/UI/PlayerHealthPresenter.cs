using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class PlayerHealthPresenter : MonoBehaviour
{
    [SerializeField] private PlayerVitals vitals;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private MonoBehaviour[] disableOnDeath;

    private void Awake()
    {
        if (playerInput == null && vitals != null)
            playerInput = vitals.GetComponent<PlayerInput>();

        if (vitals == null || healthText == null || deathPanel == null || playerInput == null)
        {
            Debug.LogError($"{name}: Thiếu tham chiếu cho HUD máu.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (vitals == null) return;
        vitals.Changed += Refresh;
        vitals.Died += HandleDeath;
    }

    private void Start() => Refresh();

    private void OnDisable()
    {
        if (vitals == null) return;
        vitals.Changed -= Refresh;
        vitals.Died -= HandleDeath;
    }

    private void Refresh()
    {
        healthText.text = $"Máu: {Mathf.CeilToInt(vitals.CurrentHealth)}/{Mathf.CeilToInt(vitals.MaxHealth)}";
        deathPanel.SetActive(vitals.IsDead);
    }

    private void HandleDeath()
    {
        playerInput.DeactivateInput();

        // Disable the actual Starter Assets motor even if the scene's array was left empty.
        ThirdPersonController motor = playerInput.GetComponent<ThirdPersonController>();
        if (motor != null) motor.enabled = false;

        InteractionScanner interaction = playerInput.GetComponent<InteractionScanner>();
        if (interaction != null) interaction.enabled = false;
        LanternController lantern = playerInput.GetComponent<LanternController>();
        if (lantern != null) lantern.enabled = false;
        LanternFocusScanner focus = playerInput.GetComponent<LanternFocusScanner>();
        if (focus != null) focus.enabled = false;

        if (disableOnDeath != null)
        {
            foreach (MonoBehaviour behaviour in disableOnDeath)
            {
                if (behaviour == null || behaviour == this || behaviour == vitals ||
                    behaviour == playerInput) continue;
                behaviour.enabled = false;
            }
        }

        Refresh();
        Debug.Log("An đã chết. Gameplay đã bị khóa.", this);
    }
}
