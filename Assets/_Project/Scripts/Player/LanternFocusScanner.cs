using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class LanternFocusScanner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LanternController lantern;
    [SerializeField] private Transform focusSource;

    [Header("Rules")]
    [SerializeField] private float range = 8f;
    [SerializeField, Min(0.5f)] private float enemyRange = 10f;
    [SerializeField, Range(1f, 180f)]
    private float fullConeAngle = 90f;

    // Camera ở sau và cao hơn An nên thường chúc xuống; nới góc dọc để chiếu dễ hơn.
    [SerializeField, Range(1f, 90f)]
    private float verticalTolerance = 45f;

    [Header("Layers")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask visibilityMask;

    // Hướng chiếu theo camera (có góc ngẩng), gốc chiếu ở đèn.
    private Transform aimTransform;
    private Collider[] overlap = new Collider[64];

    private readonly HashSet<ILanternTarget> checkedTargets =
        new HashSet<ILanternTarget>();

    private void Awake()
    {
        if (lantern == null || focusSource == null)
        {
            Debug.LogError(
                $"{name}: Thiếu Lantern hoặc Focus Source.",
                this);

            enabled = false;
        }
    }

    private void Start()
    {
        Camera main = Camera.main;
        aimTransform = main != null ? main.transform : null;
    }

    private Vector3 AimForward =>
        aimTransform != null ? aimTransform.forward : focusSource.forward;

    private void LateUpdate()
    {
        if (!Application.isFocused ||
            Time.timeScale == 0f ||
            !lantern.isActiveAndEnabled ||
            !lantern.IsFocusing)
        {
            return;
        }

        checkedTargets.Clear();

        int count;
        // Grow only for unusually crowded scenes; never silently lose targets.
        while ((count = Physics.OverlapSphereNonAlloc(focusSource.position, Mathf.Max(range, enemyRange), overlap,
                   targetMask, QueryTriggerInteraction.Ignore)) == overlap.Length)
            System.Array.Resize(ref overlap, overlap.Length * 2);

        for (int i = 0; i < count; i++)
        {
            Collider candidate = overlap[i];
            ILanternTarget target =
                candidate.GetComponentInParent<ILanternTarget>();

            if (target == null ||
                !checkedTargets.Add(target) ||
                !target.CanReceiveFocus ||
                target.AimPoint == null)
            {
                continue;
            }

            Vector3 offset =
                target.AimPoint.position - focusSource.position;

            float distance = offset.magnitude;

            float targetRange = target is IEnemyLanternTarget ? enemyRange : range;
            if (distance < 0.001f || distance > targetRange)
            {
                continue;
            }

            // Góc ngang theo nón chiếu (mỗi bên một nửa góc toàn phần), góc dọc cho phép lệch verticalTolerance.
            if (!IsInsideCone(AimForward, offset))
            {
                continue;
            }

            bool blocked = false;

            if (Physics.Raycast(
                    focusSource.position,
                    offset.normalized,
                    out RaycastHit hit,
                    distance,
                    visibilityMask,
                    QueryTriggerInteraction.Ignore))
            {
                ILanternTarget hitTarget =
                    hit.collider.GetComponentInParent<ILanternTarget>();

                blocked = hitTarget != target;
            }

            if (blocked)
            {
                continue;
            }

            target.ReceiveFocus(Time.deltaTime);
        }
    }

    private bool IsInsideCone(Vector3 aim, Vector3 offset)
    {
        Vector3 flatAim = new Vector3(aim.x, 0f, aim.z);
        Vector3 flatOffset = new Vector3(offset.x, 0f, offset.z);

        float yawAngle = flatAim.sqrMagnitude < 0.0001f || flatOffset.sqrMagnitude < 0.0001f
            ? 0f
            : Vector3.Angle(flatAim, flatOffset);
        if (yawAngle > fullConeAngle * 0.5f) return false;

        float aimPitch = Mathf.Atan2(aim.y, flatAim.magnitude) * Mathf.Rad2Deg;
        float targetPitch = Mathf.Atan2(offset.y, flatOffset.magnitude) * Mathf.Rad2Deg;
        return Mathf.Abs(targetPitch - aimPitch) <= verticalTolerance;
    }

    private void OnDrawGizmosSelected()
    {
        if (focusSource == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            focusSource.position, range);

        Gizmos.DrawRay(
            focusSource.position,
            AimForward * Mathf.Max(range, enemyRange));
    }
}
