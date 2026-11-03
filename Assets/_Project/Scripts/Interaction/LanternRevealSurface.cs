using UnityEngine;

// Nội dung (văn tự, gợi ý) chỉ hiện khi đèn đang bật, trong tầm và không bị che.
public class LanternRevealSurface : MonoBehaviour
{
    [SerializeField] private GameObject revealContent;
    [SerializeField] private Transform revealPoint;
    [SerializeField, Min(0.5f)] private float revealRange = 5f;
    [SerializeField, Min(0.05f)] private float checkInterval = 0.2f;
    [SerializeField] private LayerMask blockingMask = 1 << 3;

    private LanternController lantern;
    private float nextCheck;

    public bool IsRevealed { get; private set; }

    private void Awake()
    {
        if (revealContent == null || revealPoint == null)
        {
            Debug.LogError($"{name}: LanternRevealSurface thiếu Reveal Content hoặc Reveal Point.", this);
            enabled = false;
            return;
        }
        SetRevealed(false);
    }

    private void Start() => lantern = FindAnyObjectByType<LanternController>();

    private void Update()
    {
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + checkInterval;
        SetRevealed(CanSee());
    }

    private bool CanSee()
    {
        if (lantern == null || !lantern.IsLanternOn) return false;

        Vector3 origin = lantern.transform.position + Vector3.up * 1.3f;
        Vector3 offset = revealPoint.position - origin;
        float distance = offset.magnitude;
        if (distance > revealRange) return false;

        // Tường chắn giữa An và bề mặt thì không đọc được.
        return distance < 0.01f ||
               !Physics.Raycast(origin, offset / distance, distance, blockingMask, QueryTriggerInteraction.Ignore);
    }

    private void SetRevealed(bool revealed)
    {
        IsRevealed = revealed;
        if (revealContent.activeSelf != revealed) revealContent.SetActive(revealed);
    }
}
