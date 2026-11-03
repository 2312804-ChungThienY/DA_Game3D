using UnityEngine;

// Cột Gió chương 2 (C2 mục 2): An có Lông Chim Lạc bước vào thì được nâng tới hoverTopY và lơ lửng.
// Cột mở bằng câu đố được lưu qua Puzzles; cột đấu trường cần công cụ Lông Gió Lớn (BossTools).
[RequireComponent(typeof(Collider))]
public class WindColumn : MonoBehaviour
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private string columnId;
    [SerializeField] private bool startsActive = true;
    [SerializeField] private string requiredAbility = WorldIds.AbilityWind;
    [SerializeField] private string requiredBossTool = "";
    [SerializeField] private float liftSpeed = 6f;
    [SerializeField] private float hoverTopY = 9f;
    [SerializeField] private GameObject visual;

    private PlayerAbilities cachedAbilities;

    // Thời điểm cuối An được cột đang bật nâng (trùm quạt gió bỏ qua An khi ở trong cột).
    public static float PlayerLiftedTime { get; private set; } = float.NegativeInfinity;
    public static bool PlayerInActiveColumn => Time.time - PlayerLiftedTime < 0.15f;

    public bool IsActive { get; private set; }
    public float HoverTopY => hoverTopY;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => PlayerLiftedTime = float.NegativeInfinity;

    private void Awake()
    {
        if (worldState == null || visual == null || !GetComponent<Collider>().isTrigger ||
            (!startsActive && !WorldIds.IsKnown(StateSet.Puzzles, columnId)))
        {
            Debug.LogError($"{name}: WindColumn thiếu WorldState, Visual, ID hoặc collider không phải trigger.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (worldState != null) worldState.Changed += Refresh;
    }

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= Refresh;
    }

    private void Start() => Refresh();

    public void Refresh()
    {
        if (worldState == null) return;
        IsActive = (startsActive || worldState.Has(StateSet.Puzzles, columnId)) &&
                   (string.IsNullOrEmpty(requiredBossTool) || worldState.Has(StateSet.BossTools, requiredBossTool));
        if (visual.activeSelf != IsActive) visual.SetActive(IsActive);
    }

    // Gọi bởi câu đố (UnityEvent): cột mở bằng câu đố được lưu.
    public void Activate()
    {
        if (!string.IsNullOrEmpty(columnId)) worldState.TryAdd(StateSet.Puzzles, columnId);
        Refresh();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isActiveAndEnabled || !IsActive || Time.timeScale == 0f) return;
        PlayerMotor motor = other.GetComponentInParent<PlayerMotor>();
        if (motor == null) return;

        if (cachedAbilities == null) cachedAbilities = motor.GetComponent<PlayerAbilities>();
        if (cachedAbilities == null || !cachedAbilities.Has(requiredAbility)) return;

        motor.RequestLift(liftSpeed, hoverTopY);
        PlayerLiftedTime = Time.time;
    }
}
