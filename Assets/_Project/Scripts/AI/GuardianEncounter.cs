using UnityEngine;

public class GuardianEncounter : MonoBehaviour
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private KimQuyGate gate;
    [SerializeField] private PlayerVitals player;
    [SerializeField] private GameObject guardianRoot;
    [SerializeField] private EnemyVitals guardianVitals;
    [SerializeField] private GameObject altarReadyIndicator;

    private bool ready;
    private bool fightActive;
    public bool IsFightActive => fightActive;

    private void Awake()
    {
        ready = worldState != null && gate != null && player != null &&
            guardianRoot != null && guardianVitals != null &&
            guardianVitals.gameObject == guardianRoot &&
            guardianRoot != gameObject && !transform.IsChildOf(guardianRoot.transform);
        if (!ready)
        {
            Debug.LogError($"{name}: Check encounter references. Keep this object outside guardianRoot.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (!ready) return;
        worldState.Changed += ApplyWorldState;
        guardianVitals.Died += HandleGuardianDied;
    }

    private void Start() => ApplyWorldState();

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= ApplyWorldState;
        if (guardianVitals != null) guardianVitals.Died -= HandleGuardianDied;
    }

    private void ApplyWorldState()
    {
        if (!ready) return;
        bool defeated = worldState.HasDefeatedEnemy(WorldStateStore.GuardianId);
        if (defeated)
        {
            fightActive = false;
            guardianRoot.SetActive(false);
            gate.SetEncounterLocked(false);
        }
        else if (!fightActive)
        {
            guardianRoot.SetActive(false);
            gate.SetEncounterLocked(false);
        }

        if (altarReadyIndicator != null) altarReadyIndicator.SetActive(defeated);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ready || fightActive || Time.timeScale == 0f || player.IsDead ||
            worldState.HasDefeatedEnemy(WorldStateStore.GuardianId) || !gate.CanPass)
            return;

        if (other.GetComponentInParent<PlayerVitals>() != player) return;

        fightActive = true;
        gate.SetEncounterLocked(true);
        guardianRoot.SetActive(true);
        Debug.Log("Guardian encounter started.", this);
    }

    private void HandleGuardianDied()
    {
        if (!ready || !fightActive) return;
        // WorldState event applies the unlocked gate and altar indicator.
        worldState.TryMarkEnemyDefeated(WorldStateStore.GuardianId);
        Debug.Log("guardian_01 recorded. Save at a checkpoint to persist victory.", this);
    }
}
