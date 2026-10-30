using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class MirrorBeamEmitter : MonoBehaviour
{
    [SerializeField] private Transform source;
    [SerializeField] private LightPuzzleReceiver receiver;
    [SerializeField] private LayerMask beamMask;
    [SerializeField, Min(0.01f)] private float totalLength = 40f;
    [SerializeField, Range(0, 3)] private int maxReflections = 3;
    [SerializeField, Range(0, 2)] private int requiredMirrors = 2;

    private const float SurfaceOffset = 0.01f;
    private LineRenderer line;
    private readonly List<Vector3> points = new List<Vector3>();
    private readonly HashSet<MirrorRotator> visitedMirrors = new HashSet<MirrorRotator>();

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;

        if (source == null || receiver == null)
        {
            Debug.LogError($"{name}: Thiếu Source hoặc Receiver.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        if (!Application.isFocused || Time.timeScale == 0f) return;
        bool reachesReceiver = TraceBeam();
        receiver.TickLight(reachesReceiver, Time.deltaTime);
    }

    private bool TraceBeam()
    {
        points.Clear();
        visitedMirrors.Clear();

        Vector3 origin = source.position;
        Vector3 direction = source.forward.normalized;
        float remaining = totalLength;
        int reflections = 0;
        bool validReceiverHit = false;
        points.Add(origin);

        while (remaining > SurfaceOffset)
        {
            if (!Physics.Raycast(origin, direction, out RaycastHit hit, remaining,
                    beamMask, QueryTriggerInteraction.Ignore))
            {
                points.Add(origin + direction * remaining);
                break;
            }

            points.Add(hit.point);
            remaining -= hit.distance;

            LightPuzzleReceiver hitReceiver = hit.collider.GetComponentInParent<LightPuzzleReceiver>();
            if (hitReceiver == receiver)
            {
                validReceiverHit = visitedMirrors.Count >= requiredMirrors;
                break;
            }

            MirrorRotator mirror = hit.collider.GetComponentInParent<MirrorRotator>();
            if (mirror == null || !mirror.isActiveAndEnabled) break;

            bool entersFront = Vector3.Dot(direction, mirror.FrontNormal) < -0.001f;
            bool hitsFrontSurface = Vector3.Dot(hit.normal, mirror.FrontNormal) > 0.9f;
            if (!entersFront || !hitsFrontSurface || reflections >= maxReflections) break;

            visitedMirrors.Add(mirror);
            direction = Vector3.Reflect(direction, hit.normal).normalized;
            reflections++;
            origin = hit.point + direction * SurfaceOffset;
            remaining -= SurfaceOffset;
        }

        line.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) line.SetPosition(i, points[i]);
        return validReceiverHit;
    }
}
