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
    [SerializeField, Range(1f, 180f)]
    private float fullConeAngle = 60f;

    [Header("Layers")]
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask visibilityMask;

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

        Collider[] colliders = Physics.OverlapSphere(
            focusSource.position,
            range,
            targetMask,
            QueryTriggerInteraction.Ignore);

        foreach (Collider candidate in colliders)
        {
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

            if (distance < 0.001f || distance > range)
            {
                continue;
            }

            float angle = Vector3.Angle(
                focusSource.forward, offset);

            // Góc toàn phần 60° nghĩa là mỗi bên 30°.
            if (angle > fullConeAngle * 0.5f)
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
            focusSource.forward * range);
    }
}