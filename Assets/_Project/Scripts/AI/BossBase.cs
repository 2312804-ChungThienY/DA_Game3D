using System.Collections;
using TMPro;
using UnityEngine;

// Khung chung cho trùm chương 2–5 (spec chapters/00): kích hoạt bởi ArenaGate, AI chạy bằng coroutine,
// cửa sổ choáng có trần sát thương, chết thì ghi Bosses. Trùm di chuyển bằng transform của chính nó.
[RequireComponent(typeof(EnemyHealth))]
public abstract class BossBase : MonoBehaviour, IArenaBoss
{
    [SerializeField] protected WorldStateStore worldState;
    [SerializeField] protected string bossId;
    [SerializeField] protected string displayName = "Trùm";
    [Tooltip("Lưu ngay khi trùm bị đánh bại để tải lại không dựng lại trùm.")]
    [SerializeField] private SaveService saveOnDefeat;
    [SerializeField] private string defeatCheckpointId;

    [Header("Thanh máu trùm (tùy chọn)")]
    [SerializeField] private GameObject bossBar;
    [SerializeField] private RectTransform bossBarFill;
    [SerializeField] private TMP_Text bossBarLabel;

    protected EnemyHealth health;
    protected PlayerVitals player;
    protected PlayerMotor playerMotor;
    private Coroutine ai;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private float stunUntil = float.NegativeInfinity;
    private float stunCapRemaining;
    private bool dead;

    public bool IsActive { get; private set; }
    public bool DefeatSavePending { get; private set; }
    protected bool Invulnerable { get; set; }
    protected bool InStunWindow => Time.time < stunUntil;
    protected float StunDamageMultiplier { get; set; } = 1f;

    protected virtual void Awake()
    {
        health = GetComponent<EnemyHealth>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        health.DamageFilter = FilterDamage;
        if (worldState == null || !WorldIds.IsKnown(StateSet.Bosses, bossId))
        {
            Debug.LogError($"{name}: BossBase thiếu WorldState hoặc ID trùm sai.", this);
            enabled = false;
        }
        if (bossBar != null) bossBar.SetActive(false);
    }

    protected virtual void OnEnable()
    {
        health.Died += OnDied;
        health.Changed += RefreshBar;
        if (worldState != null) worldState.Changed += ApplyWorldState;
    }

    protected virtual void OnDisable()
    {
        if (ai != null) StopCoroutine(ai);
        ai = null;
        IsActive = false;
        health.Died -= OnDied;
        health.Changed -= RefreshBar;
        if (worldState != null) worldState.Changed -= ApplyWorldState;
        if (bossBar != null) bossBar.SetActive(false);
    }

    protected virtual void Start()
    {
        player = FindAnyObjectByType<PlayerVitals>();
        playerMotor = player != null ? player.GetComponent<PlayerMotor>() : null;
        ApplyWorldState();
    }

    private void ApplyWorldState()
    {
        if (!dead && worldState.Has(StateSet.Bosses, bossId)) gameObject.SetActive(false);
    }

    public void Activate()
    {
        if (!isActiveAndEnabled || IsActive || dead) return;
        IsActive = true;
        if (bossBar != null) bossBar.SetActive(true);
        RefreshBar();
        ai = StartCoroutine(RunAI());
    }

    public void ResetBoss()
    {
        // A defeated boss stays defeated; reload restores this from world state.
        if (dead || (worldState != null && worldState.Has(StateSet.Bosses, bossId))) return;
        if (ai != null) StopCoroutine(ai);
        ai = null;
        IsActive = false;
        Invulnerable = false;
        stunUntil = float.NegativeInfinity;
        stunCapRemaining = 0f;
        OnReset();
        transform.SetPositionAndRotation(startPosition, startRotation);
        health.ResetToFull();
        if (bossBar != null) bossBar.SetActive(false);
    }

    protected abstract IEnumerator RunAI();
    protected virtual void OnReset() { }

    // Trong cửa sổ choáng, tổng sát thương nhận không vượt damageCap.
    protected void BeginStunWindow(float duration, float damageCap)
    {
        stunUntil = Time.time + duration;
        stunCapRemaining = damageCap;
    }

    private float FilterDamage(float amount)
    {
        if (!IsActive || dead || Invulnerable) return 0f;
        if (InStunWindow)
        {
            amount = Mathf.Min(amount * StunDamageMultiplier, stunCapRemaining);
            stunCapRemaining -= amount;
        }
        return amount;
    }

    // An còn sống và phiên đang chơi: trùm mới hành động.
    protected bool CanAct
    {
        get
        {
            GameSession session = AppRoot.Instance != null ? AppRoot.Instance.Session : null;
            return Time.timeScale>0f && player != null && !player.IsDead && (session == null || session.State == GameSession.SessionState.Playing);
        }
    }

    // Chờ theo thời gian game; đứng yên khi An chết hoặc tạm dừng.
    protected IEnumerator Wait(float seconds)
    {
        float t = 0f;
        while (t < seconds || !CanAct)
        {
            if (CanAct) t += Time.deltaTime;
            yield return null;
        }
    }

    protected Vector3 PlayerPosition => player.transform.position;

    private void OnDied(EnemyHealth _)
    {
        dead = true;
        IsActive = false;
        if (ai != null) StopCoroutine(ai);
        ai = null;
        OnDefeated();
        foreach (Collider c in GetComponentsInChildren<Collider>()) c.enabled = false;
        if (bossBar != null) bossBar.SetActive(false);
        worldState.TryAdd(StateSet.Bosses, bossId);
        TrySaveDefeat();
        if (AppRoot.Instance != null && AppRoot.Instance.UI != null)
            AppRoot.Instance.UI.ShowToast(DefeatSavePending?$"{displayName} đã được giải thoát, nhưng chưa lưu được chiến thắng":$"{displayName} đã được giải thoát");
        Invoke(nameof(Hide), 1f);
    }

    protected virtual void OnDefeated() { }

    public bool TrySaveDefeat()
    {
        if(!dead)return false;
        DefeatSavePending=saveOnDefeat!=null&&!string.IsNullOrEmpty(defeatCheckpointId)&&!saveOnDefeat.SaveAt(defeatCheckpointId);
        return !DefeatSavePending;
    }

    private void Hide() => gameObject.SetActive(false);

    private void RefreshBar()
    {
        if (bossBarFill == null) return;
        Vector2 anchor = bossBarFill.anchorMax;
        anchor.x = Mathf.Clamp01(health.Current / health.Max);
        bossBarFill.anchorMax = anchor;
        if (bossBarLabel != null) bossBarLabel.text = $"{displayName}  {Mathf.CeilToInt(health.Current)}/{Mathf.CeilToInt(health.Max)}";
    }
}

// Trùm có thể được đấu trường đánh thức.
public interface IArenaBoss
{
    void Activate();
}
