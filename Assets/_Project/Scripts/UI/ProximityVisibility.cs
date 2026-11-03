using UnityEngine;

// Chỉ hiện một phần HUD (ví dụ trạng thái câu đố) khi An ở gần khu vực liên quan.
[RequireComponent(typeof(CanvasGroup))]
public class ProximityVisibility : MonoBehaviour
{
    [SerializeField] private Transform anchor;
    [SerializeField, Min(1f)] private float radius = 12f;
    [SerializeField, Min(0.05f)] private float checkInterval = 0.2f;

    private CanvasGroup group;
    private Transform player;
    private float nextCheck;

    private void Awake()
    {
        group = GetComponent<CanvasGroup>();
        if (anchor == null)
        {
            Debug.LogError($"{name}: ProximityVisibility thiếu Anchor.", this);
            enabled = false;
            return;
        }
        SetVisible(false);
    }

    private void Start()
    {
        PlayerMotor motor = FindAnyObjectByType<PlayerMotor>();
        player = motor != null ? motor.transform : null;
    }

    private void Update()
    {
        if (player == null || Time.unscaledTime < nextCheck) return;
        nextCheck = Time.unscaledTime + checkInterval;
        SetVisible((player.position - anchor.position).sqrMagnitude <= radius * radius);
    }

    private void SetVisible(bool visible)
    {
        group.alpha = visible ? 1f : 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}
