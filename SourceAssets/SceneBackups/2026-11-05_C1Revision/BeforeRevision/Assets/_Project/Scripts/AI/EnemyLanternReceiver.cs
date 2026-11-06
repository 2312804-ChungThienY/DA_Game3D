using UnityEngine;

// Chiếu Focus liên tục đủ thời gian thì địch bị làm yếu (nhận thêm sát thương). Hiệu ứng tạm, không lưu.
public class EnemyLanternReceiver : MonoBehaviour, IEnemyLanternTarget
{
    [SerializeField] private EnemyDefinition definition;
    [SerializeField] private Transform aimPoint;
    [SerializeField] private Renderer tintRenderer;
    [SerializeField] private Color weakenedColor = new Color(1f, 0.85f, 0.3f);
    [SerializeField] private float graceTime = 0.25f;

    private EnemyHealth health;
    private float focusProgress;
    private float lastFocusTime = float.NegativeInfinity;
    private float weakenedUntil = float.NegativeInfinity;
    private MaterialPropertyBlock block;
    private bool tinted;

    public Transform AimPoint => aimPoint;
    public bool CanReceiveFocus => isActiveAndEnabled && aimPoint != null && (health == null || !health.IsDead);
    public bool IsWeakened => Time.time < weakenedUntil;
    public float DamageMultiplier => IsWeakened && definition != null ? definition.weakenMultiplier : 1f;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        block = new MaterialPropertyBlock();
        if (definition == null || aimPoint == null)
        {
            Debug.LogError($"{name}: EnemyLanternReceiver thiếu Definition hoặc Aim Point.", this);
            enabled = false;
        }
    }

    public void ReceiveFocus(float deltaTime)
    {
        if (!CanReceiveFocus || deltaTime <= 0f) return;

        if (Time.time - lastFocusTime > graceTime) focusProgress = 0f;
        lastFocusTime = Time.time;
        focusProgress += deltaTime;

        // Chiếu tiếp chỉ làm mới thời hạn, không cộng dồn.
        if (focusProgress >= definition.weakenFocusTime)
            weakenedUntil = Time.time + definition.weakenDuration;
    }

    private void Update()
    {
        bool weakened = IsWeakened;
        if (tintRenderer == null || weakened == tinted) return;

        tinted = weakened;
        tintRenderer.GetPropertyBlock(block);
        if (weakened) block.SetColor("_BaseColor", weakenedColor);
        else block.Clear();
        tintRenderer.SetPropertyBlock(block);
    }
}
