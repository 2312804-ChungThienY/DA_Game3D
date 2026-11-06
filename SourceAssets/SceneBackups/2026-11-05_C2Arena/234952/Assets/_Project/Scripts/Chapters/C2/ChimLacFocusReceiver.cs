using UnityEngine;

// Nhận chiếu đèn cho trùm Chim Lạc: chỉ khi đang đậu; đủ 1,5 s (mất chiếu quá 0,25 s thì về 0) thì báo choáng.
public class ChimLacFocusReceiver : MonoBehaviour, IEnemyLanternTarget
{
    [SerializeField] private BossChimLac boss;
    [SerializeField] private Transform aimPoint;
    [SerializeField] private float requiredTime = 1.5f;
    [SerializeField] private float graceTime = 0.25f;

    private float progress;
    private float lastFocusTime = float.NegativeInfinity;
    private PlayerMotor playerMotor;

    public Transform AimPoint => aimPoint;
    public bool CanReceiveFocus => isActiveAndEnabled && aimPoint != null && boss != null && boss.IsActive &&
        boss.State == BossChimLac.ChimState.Perched && playerMotor != null && playerMotor.IsHovering &&
        WindColumn.PlayerInActiveColumn;
    public bool StunTriggered { get; private set; }
    public float Progress => progress;

    private void Awake()
    {
        if (boss != null && aimPoint != null && requiredTime > 0f) return;
        Debug.LogError($"{name}: ChimLacFocusReceiver thiếu Boss, AimPoint hoặc thời gian hợp lệ.", this);
        enabled = false;
    }

    private void OnDisable() => ResetProgress();
    private void Start() => playerMotor = FindAnyObjectByType<PlayerMotor>();

    public void ResetProgress()
    {
        progress = 0f;
        StunTriggered = false;
        lastFocusTime = float.NegativeInfinity;
    }

    public void ReceiveFocus(float deltaTime)
    {
        if (!CanReceiveFocus || Time.timeScale <= 0f || deltaTime <= 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;
        if (Time.time - lastFocusTime > graceTime) progress = 0f;
        lastFocusTime = Time.time;
        progress += deltaTime;
        if (progress >= requiredTime) StunTriggered = true;
    }
}
