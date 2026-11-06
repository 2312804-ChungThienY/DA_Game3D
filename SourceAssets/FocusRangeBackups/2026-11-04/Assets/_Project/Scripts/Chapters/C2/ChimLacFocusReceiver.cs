using UnityEngine;

// Nhận chiếu đèn cho trùm Chim Lạc: chỉ khi đang đậu; đủ 1,5 s (mất chiếu quá 0,25 s thì về 0) thì báo choáng.
public class ChimLacFocusReceiver : MonoBehaviour, ILanternTarget
{
    [SerializeField] private BossChimLac boss;
    [SerializeField] private Transform aimPoint;
    [SerializeField] private float requiredTime = 1.5f;
    [SerializeField] private float graceTime = 0.25f;

    private float progress;
    private float lastFocusTime = float.NegativeInfinity;

    public Transform AimPoint => aimPoint;
    public bool CanReceiveFocus => isActiveAndEnabled && boss != null && boss.State == BossChimLac.ChimState.Perched;
    public bool StunTriggered { get; private set; }
    public float Progress => progress;

    public void ResetProgress()
    {
        progress = 0f;
        StunTriggered = false;
        lastFocusTime = float.NegativeInfinity;
    }

    public void ReceiveFocus(float deltaTime)
    {
        if (!CanReceiveFocus || deltaTime <= 0f) return;
        if (Time.time - lastFocusTime > graceTime) progress = 0f;
        lastFocusTime = Time.time;
        progress += deltaTime;
        if (progress >= requiredTime) StunTriggered = true;
    }
}
