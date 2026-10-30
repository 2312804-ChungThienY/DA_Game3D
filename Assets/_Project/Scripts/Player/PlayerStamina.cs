using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerVitals))]
public class PlayerStamina : MonoBehaviour
{
    [SerializeField, Min(1f)] private float maxStamina = 100f;
    [SerializeField, Min(0f)] private float recoveryRate = 20f;
    [SerializeField, Min(0f)] private float recoveryDelay = 1f;

    private PlayerVitals vitals;
    private float currentStamina;
    private float recoveryStartsAt;

    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;

    public event Action Changed;

    private void Awake()
    {
        vitals = GetComponent<PlayerVitals>();
        maxStamina = Mathf.Max(1f, maxStamina);
        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (!Application.isFocused || Time.timeScale == 0f || vitals.IsDead ||
            currentStamina >= maxStamina || Time.time <= recoveryStartsAt)
            return;

        float recoveryDelta = Mathf.Min(Time.deltaTime, Time.time - recoveryStartsAt);
        float previous = currentStamina;
        currentStamina = Mathf.Min(maxStamina, currentStamina + recoveryRate * recoveryDelta);
        if (currentStamina != previous) Changed?.Invoke();
    }

    public bool TrySpend(float amount)
    {
        if (!isActiveAndEnabled || Time.timeScale == 0f || vitals.IsDead ||
            float.IsNaN(amount) || float.IsInfinity(amount) || amount <= 0f ||
            currentStamina < amount)
            return false;

        currentStamina -= amount;
        recoveryStartsAt = Time.time + recoveryDelay;
        Changed?.Invoke();
        return true;
    }

    public void RestoreFull()
    {
        currentStamina = maxStamina;
        recoveryStartsAt = Time.time + recoveryDelay;
        Changed?.Invoke();
    }

    [ContextMenu("Test/Spend 25 Stamina")]
    private void TestSpend25()
    {
        if (Application.isPlaying)
            Debug.Log($"Tiêu hao 25: {TrySpend(25f)}", this);
    }

    [ContextMenu("Test/Spend 100 Stamina")]
    private void TestSpend100()
    {
        if (Application.isPlaying)
            Debug.Log($"Tiêu hao 100: {TrySpend(100f)}", this);
    }
}
