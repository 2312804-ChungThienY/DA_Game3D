using UnityEngine;

// Chiếu Focus liên tục đủ thời gian thì địch bị làm yếu (nhận thêm sát thương). Hiệu ứng tạm, không lưu.
public class EnemyLanternReceiver : MonoBehaviour, IEnemyLanternTarget
{
    [SerializeField] private EnemyDefinition definition;
    [SerializeField] private Transform aimPoint;
    // Legacy serialized authoring references; weakening no longer changes materials.
    [SerializeField,HideInInspector] private Renderer tintRenderer;
    [SerializeField,HideInInspector] private Color weakenedColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private float graceTime = 0.25f;
    [SerializeField] private bool enableStun;
    [SerializeField,Min(.1f)] private float stunDuration=4f;
    [SerializeField,Min(.1f)] private float stunRecoveryImmunity=3f;

    private EnemyHealth health;
    private float focusProgress;
    private float lastFocusTime = float.NegativeInfinity;
    private float weakenedUntil = float.NegativeInfinity;
    private float stunnedUntil=float.NegativeInfinity,nextStunAllowed=float.NegativeInfinity,stunCharge;
    private bool canStunOnThisHold=true;

    public Transform AimPoint => aimPoint;
    public bool CanReceiveFocus => isActiveAndEnabled && aimPoint != null && (health == null || !health.IsDead);
    public bool IsWeakened => Time.time < weakenedUntil;
    public bool IsStunned=>enableStun&&(health==null||!health.IsDead)&&Time.time<stunnedUntil;
    public float StunRemaining=>IsStunned?stunnedUntil-Time.time:0;
    public float StunImmunityRemaining=>Mathf.Max(0,nextStunAllowed-Time.time);
    public float DamageMultiplier => IsWeakened && definition != null ? definition.weakenMultiplier : 1f;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (definition == null || aimPoint == null)
        {
            Debug.LogError($"{name}: EnemyLanternReceiver thiếu Definition hoặc Aim Point.", this);
            enabled = false;
        }
    }

    public void ReceiveFocus(float deltaTime)
    {
        if (!CanReceiveFocus || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime <= 0f || Time.timeScale<=0) return;

        if (Time.time - lastFocusTime > graceTime){focusProgress=0;stunCharge=0;canStunOnThisHold=true;}
        lastFocusTime = Time.time;
        focusProgress += deltaTime;

        // Chiếu tiếp chỉ làm mới thời hạn, không cộng dồn.
        if (focusProgress >= definition.weakenFocusTime)
            weakenedUntil = Time.time + definition.weakenDuration;
        if(enableStun)
        {
            if(IsStunned||Time.time<nextStunAllowed)stunCharge=0;
            else stunCharge+=deltaTime;
            if(canStunOnThisHold&&stunCharge>=definition.weakenFocusTime&&Time.time>=nextStunAllowed)
            {stunnedUntil=Time.time+stunDuration;nextStunAllowed=stunnedUntil+stunRecoveryImmunity;canStunOnThisHold=false;stunCharge=0;}
        }
    }

}
