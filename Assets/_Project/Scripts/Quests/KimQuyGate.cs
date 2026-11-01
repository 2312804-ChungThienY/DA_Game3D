using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class KimQuyGate : MonoBehaviour
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private GameObject barrier;
    [SerializeField] private TMP_Text statusText;

    private bool ready;
    private bool encounterLocked;

    public bool HasProgressRequirements => worldState != null &&
                           worldState.HasRelic(WorldStateStore.DrumA) &&
                           worldState.HasRelic(WorldStateStore.DrumB) &&
                           worldState.HasRelic(WorldStateStore.DrumC) &&
                           worldState.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId) &&
                           worldState.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId);

    public bool CanPass => HasProgressRequirements && !encounterLocked;

    public void SetEncounterLocked(bool locked)
    {
        encounterLocked = locked;
        Refresh();
    }

    private void Awake()
    {
        ready = worldState != null && barrier != null &&
                statusText != null && barrier != gameObject;
        if (!ready)
        {
            Debug.LogError($"{name}: Gán WorldState, Barrier con và Status Text.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (!ready) return;
        worldState.Changed += Refresh;
        Refresh();
    }

    private void Start() => Refresh();

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (!ready) return;

        bool hasA = worldState.HasRelic(WorldStateStore.DrumA);
        bool hasB = worldState.HasRelic(WorldStateStore.DrumB);
        bool hasC = worldState.HasRelic(WorldStateStore.DrumC);
        bool drumSolved = worldState.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId);
        bool mirrorSolved = worldState.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId);
        int fragmentCount = (hasA ? 1 : 0) + (hasB ? 1 : 0) + (hasC ? 1 : 0);
        int puzzleCount = (drumSolved ? 1 : 0) + (mirrorSolved ? 1 : 0);
        bool allowed = CanPass;

        barrier.SetActive(!allowed);
        if (encounterLocked)
        {
            statusText.text = "Cổng đã khóa\nĐánh bại Hộ vệ để mở lối";
            return;
        }
        statusText.text = allowed
            ? "Cổng đã mở\nTiến vào Đàn Kim Quy"
            : $"Cổng đang phong kín\nMảnh trống: {fragmentCount}/3\nCâu đố: {puzzleCount}/2";
    }
}
