using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-75)]
public class PotionUseController : MonoBehaviour
{
    [SerializeField] private float useCooldown = 2f;
    private PlayerInput input;
    private PlayerInventory inventory;
    private PlayerVitals vitals;
    private InputAction useAction;
    private float nextUseAt = float.NegativeInfinity;

    private void Start()
    {
        input = GetComponent<PlayerInput>();
        inventory = GetComponent<PlayerInventory>();
        vitals = GetComponent<PlayerVitals>();
        if (input != null) useAction = input.actions.FindAction("UsePotion", false);
        if (input == null || inventory == null || vitals == null || useAction == null)
        {
            Debug.LogError($"{name}: Check inventory/vitals/PlayerInput and UsePotion action.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (Application.isFocused && useAction != null && useAction.WasPressedThisFrame())
            TryUsePotion();
    }

    public bool TryUsePotion()
    {
        if (!isActiveAndEnabled || input == null || vitals == null || inventory == null ||
            useAction == null || Time.timeScale == 0f || vitals.IsDead ||
            !input.inputIsActive || !useAction.enabled || Time.time < nextUseAt) return false;
        if (!inventory.TryUseHealthPotion(vitals)) return false;
        nextUseAt = Time.time + Mathf.Max(0f, useCooldown);
        return true;
    }
}
