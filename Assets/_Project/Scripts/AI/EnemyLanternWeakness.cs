using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(200)]
[RequireComponent(typeof(EnemyVitals))]
public class EnemyLanternWeakness : MonoBehaviour, ILanternTarget
{
    [Header("References")]
    [SerializeField] private Transform aimPoint;
    [SerializeField] private GameObject weakenedIndicator;

    [Header("Rules")]
    [SerializeField, Min(0.01f)] private float requiredFocusTime = 1f;
    [SerializeField, Min(0.01f)] private float weakenedDuration = 4f;
    [SerializeField, Min(1f)] private float incomingDamageMultiplier = 1.5f;

    [Header("Runtime debug")]
    [SerializeField] private float focusProgress;

    private EnemyVitals vitals;
    private int lastFocusFrame = -1;
    private float weakenedUntil = float.NegativeInfinity;

    public Transform AimPoint => aimPoint;
    public bool CanReceiveFocus => isActiveAndEnabled && vitals != null &&
                                   !vitals.IsDead && aimPoint != null;
    public bool IsWeakened => isActiveAndEnabled && vitals != null &&
                              !vitals.IsDead && Time.time < weakenedUntil;
    public float DamageMultiplier => IsWeakened ? incomingDamageMultiplier : 1f;

    private void Awake()
    {
        vitals = GetComponent<EnemyVitals>();
        if (aimPoint == null)
        {
            Debug.LogError($"{name}: Chưa gán AimPoint nhận ánh sáng.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        focusProgress = 0f;
        lastFocusFrame = -1;
        weakenedUntil = float.NegativeInfinity;
        RefreshIndicator();
    }

    public void ReceiveFocus(float deltaTime)
    {
        if (!CanReceiveFocus || Time.timeScale == 0f || deltaTime <= 0f ||
            lastFocusFrame == Time.frameCount)
            return;

        lastFocusFrame = Time.frameCount;
        focusProgress = Mathf.Min(requiredFocusTime, focusProgress + deltaTime);
        if (focusProgress >= requiredFocusTime)
            weakenedUntil = Time.time + weakenedDuration;
    }

    private void LateUpdate()
    {
        // LanternFocusScanner runs at order 100, before this component at 200.
        if (Time.timeScale > 0f && lastFocusFrame != Time.frameCount)
            focusProgress = 0f;
        RefreshIndicator();
    }

    private void RefreshIndicator()
    {
        if (weakenedIndicator != null && weakenedIndicator != gameObject)
            weakenedIndicator.SetActive(IsWeakened);
    }

    private void OnDisable()
    {
        focusProgress = 0f;
        lastFocusFrame = -1;
        weakenedUntil = float.NegativeInfinity;
        if (weakenedIndicator != null && weakenedIndicator != gameObject)
            weakenedIndicator.SetActive(false);
    }
}
