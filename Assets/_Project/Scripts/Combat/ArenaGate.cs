using UnityEngine;

// Đấu trường: An đi qua vùng bắt đầu thì khóa cổng sau lưng và đánh thức trùm; trùm chết thì mở.
// Thua trận: GameSession nạp lại scene nên đấu trường và trùm về trạng thái ban đầu.
[RequireComponent(typeof(Collider))]
public class ArenaGate : MonoBehaviour
{
    [SerializeField] private WorldStateStore worldState;
    [Tooltip("Component trùm có IArenaBoss (StoneGuardian, BossBase).")]
    [SerializeField] private MonoBehaviour boss;
    [SerializeField] private GameObject gateBehind;
    [SerializeField] private string bossId = WorldIds.GuardianBoss;

    public bool IsLocked => gateBehind != null && gateBehind.activeSelf;

    private void Awake()
    {
        if (worldState == null || !(boss is IArenaBoss) || gateBehind == null || !GetComponent<Collider>().isTrigger)
        {
            Debug.LogError($"{name}: ArenaGate thiếu WorldState, Boss, Gate hoặc collider không phải trigger.", this);
            enabled = false;
            return;
        }
        gateBehind.SetActive(false);
    }

    private void OnEnable()
    {
        if (worldState != null) worldState.Changed += Apply;
    }

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= Apply;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled || IsLocked || worldState.Has(StateSet.Bosses, bossId)) return;

        PlayerVitals player = other.GetComponentInParent<PlayerVitals>();
        if (player == null || player.IsDead) return;

        gateBehind.SetActive(true);
        ((IArenaBoss)boss).Activate();
    }

    public void Unlock()
    {
        if (gateBehind != null) gateBehind.SetActive(false);
    }

    private void Apply()
    {
        if (worldState.Has(StateSet.Bosses, bossId)) Unlock();
    }
}
