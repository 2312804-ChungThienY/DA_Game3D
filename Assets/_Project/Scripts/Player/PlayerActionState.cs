using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public sealed class PlayerActionState : MonoBehaviour
{
    private PlayerVitals vitals;
    private PlayerMotor motor;
    private CombatController combat;
    private DodgeController dodge;
    private float lastHealth;
    private float hurtUntil;
    public bool IsHurt => Time.time < hurtUntil;
    public bool Active => isActiveAndEnabled && Application.isFocused && Time.timeScale > 0f &&
        AppRoot.Instance != null && AppRoot.Instance.Input != null && AppRoot.Instance.Input.GameplayEnabled && !vitals.IsDead;
    public bool CanMove => Active && !IsHurt && (combat == null || !combat.IsAttacking) && (dodge == null || !dodge.IsDodging);
    public bool CanJump => CanMove && !motor.IsLow;
    public bool CanAttack => Active && !IsHurt && (dodge == null || !dodge.IsDodging) && !motor.IsLow;
    public bool CanInteract => CanMove && (motor.IsGrounded || motor.IsHovering) && !motor.IsLow;
    public bool CanFocus => Active && !IsHurt && (combat == null || !combat.IsAttacking) && (dodge == null || !dodge.IsDodging);
    private void Awake()
    {
        vitals = GetComponent<PlayerVitals>(); motor = GetComponent<PlayerMotor>();
        combat = GetComponent<CombatController>(); dodge = GetComponent<DodgeController>();
    }
    private void OnEnable() { lastHealth = vitals.CurrentHealth; vitals.Changed += OnHealth; }
    private void OnDisable() { vitals.Changed -= OnHealth; }
    private void OnHealth()
    {
        if (vitals.CurrentHealth < lastHealth)
        {
            hurtUntil = Time.time + 0.18f;
            combat?.CancelAttack(); dodge?.CancelRoll(); motor.CancelDash();
        }
        lastHealth = vitals.CurrentHealth;
    }
    public void ResetTransient() { hurtUntil = 0f; combat?.CancelAttack(); dodge?.CancelRoll(); }
}
