using UnityEngine;

// Mốc lưu tự động (trước đấu trường, đầu pha trùm): An vào lần đầu trong phiên thì lưu.
[RequireComponent(typeof(Collider))]
public class CheckpointTrigger : MonoBehaviour, ICheckpoint
{
    [SerializeField] private SaveService saveService;
    [SerializeField] private string checkpointId;
    [SerializeField] private Transform spawnPoint;

    private bool savedThisSession;

    public string CheckpointId => checkpointId;
    public Transform SpawnPoint => spawnPoint;

    private void Awake()
    {
        if (saveService == null || spawnPoint == null || !SaveService.IsKnownCheckpoint(checkpointId) ||
            !GetComponent<Collider>().isTrigger)
        {
            Debug.LogError($"{name}: CheckpointTrigger thiếu SaveService, Spawn Point, ID sai hoặc collider không phải trigger.", this);
            enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActiveAndEnabled || savedThisSession) return;
        PlayerVitals player = other.GetComponentInParent<PlayerVitals>();
        if (player == null || player.IsDead) return;

        savedThisSession = saveService.SaveAt(checkpointId);
    }
}

// Mốc lưu có điểm hồi sinh (CheckpointAltar, CheckpointTrigger).
public interface ICheckpoint
{
    string CheckpointId { get; }
    Transform SpawnPoint { get; }
}
