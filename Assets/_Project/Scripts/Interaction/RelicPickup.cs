using UnityEngine;

public class RelicPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private string relicId = WorldStateStore.DrumA;
    [SerializeField] private string displayName = "Mảnh Trống A";

    public string Prompt => $"Nhận {displayName}";

    private void Awake()
    {
        if (worldState == null || !WorldStateStore.IsValidRelicId(relicId))
        {
            Debug.LogError($"{name}: Thiếu WorldState hoặc ID mảnh trống sai.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (worldState == null)
        {
            return;
        }

        worldState.Changed += RefreshFromState;
        RefreshFromState();
    }

    private void OnDisable()
    {
        if (worldState != null)
        {
            worldState.Changed -= RefreshFromState;
        }
    }

    public bool CanInteract(GameObject interactor)
    {
        return isActiveAndEnabled && worldState != null &&
               !worldState.HasRelic(relicId) &&
               (relicId != WorldStateStore.DrumB ||
                worldState.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId)) &&
               (relicId != WorldStateStore.DrumC ||
                worldState.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId));
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        if (worldState.TryCollectRelic(relicId))
        {
            Debug.Log($"Đã nhận: {displayName}", this);
            gameObject.SetActive(false);
        }
    }

    private void RefreshFromState()
    {
        if (worldState.HasRelic(relicId))
        {
            gameObject.SetActive(false);
        }
    }
}
