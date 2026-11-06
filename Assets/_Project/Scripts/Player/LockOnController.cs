using UnityEngine;

// Khóa mục tiêu bằng chuột giữa (spec phần 43). Camera giữ quái trong khung, An quay mặt về quái.
// Tự bỏ khóa khi mục tiêu chết, ra ngoài 15 m hoặc bị che quá 1 giây. Dùng được từ chương 1.
[DefaultExecutionOrder(60)]
[RequireComponent(typeof(PlayerMotor), typeof(PlayerCameraController))]
public class LockOnController : MonoBehaviour
{
    [SerializeField] private float maxDistance = 15f;
    [SerializeField] private float occlusionTimeout = 1f;
    [SerializeField] private float cameraTurnSpeed = 10f;
    [SerializeField] private LayerMask blockers = 1 << 3;

    private PlayerMotor motor;
    private PlayerVitals vitals;
    private InputReader input;
    private RectTransform marker;
    private EnemyHealth target;
    private Transform aim;
    private float occludedTime;

    public bool HasTarget => target != null;
    public EnemyHealth Target => target;
    public Transform Aim => aim;
    public Vector3 AimPosition => target != null ? AimPositionOf(target) : Vector3.zero;
    public float CameraTurnSpeed => cameraTurnSpeed;

    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        vitals = GetComponent<PlayerVitals>();
    }

    private void Start()
    {
        input = AppRoot.Instance != null ? AppRoot.Instance.Input : null;
        if (input == null || !input.IsReady)
        {
            Debug.LogError($"{name}: LockOnController không tìm thấy InputReader.", this);
            enabled = false;
            return;
        }
        // Dấu khóa nằm trên Canvas của scene (tùy chọn).
        GameObject markerGo = null;
        foreach (var canvas in gameObject.scene.GetRootGameObjects())
            foreach (var rect in canvas.GetComponentsInChildren<RectTransform>(true))
                if (rect.name == "LockOnMarker") { markerGo = rect.gameObject; break; }
        if (markerGo != null)
        {
            marker = markerGo.GetComponent<RectTransform>();
            markerGo.SetActive(false);
        }
    }

    private void OnEnable() => Canvas.willRenderCanvases += UpdateMarker;
    private void OnDisable() { Canvas.willRenderCanvases -= UpdateMarker; Unlock(); }

    private void Update()
    {
        if (vitals != null && vitals.IsDead) { Unlock(); return; }

        if (input.WasPressed(input.LockOn))
        {
            if (HasTarget) Unlock();
            else TryLock();
        }

        if (!HasTarget || Time.deltaTime <= 0f) return;

        if (target == null || !target.isActiveAndEnabled || target.IsDead ||
            FlatDistance(target.transform.position) > maxDistance)
        {
            Unlock();
            return;
        }

        occludedTime = IsOccluded(AimPosition) ? occludedTime + Time.deltaTime : 0f;
        if (occludedTime > occlusionTimeout) Unlock();
    }

    private void UpdateMarker()
    {
        if (!HasTarget || Time.timeScale == 0f) return;

        if (marker != null && Camera.main != null)
        {
            Vector3 screen = Camera.main.WorldToScreenPoint(AimPosition);
            marker.gameObject.SetActive(screen.z > 0f);
            marker.position = screen;
        }
    }

    public bool TryLock()
    {
        Camera cam = Camera.main;
        if (cam == null || input == null || !input.GameplayActive || Time.timeScale <= 0f || vitals.IsDead) return false;

        EnemyHealth best = null;
        float bestScore = float.MaxValue;
        foreach (EnemyHealth enemy in EnemyHealth.Active)
        {
            if (enemy == null || enemy.IsDead) continue;
            Vector3 point = AimPositionOf(enemy);
            float dist = FlatDistance(point);
            if (dist > maxDistance) continue;
            Vector3 vp = cam.WorldToViewportPoint(point);
            if (vp.z <= 0f || vp.x < 0f || vp.x > 1f || vp.y < 0f || vp.y > 1f) continue;
            if (IsOccluded(point)) continue;

            float score = Vector2.Distance(new Vector2(vp.x, vp.y), new Vector2(0.5f, 0.5f)) * 20f + dist;
            if (score < bestScore) { bestScore = score; best = enemy; }
        }

        if (best == null) return false;
        target = best;
        aim = AimOf(best);
        occludedTime = 0f;
        motor.FacingTarget = aim;
        return true;
    }

    public void Unlock()
    {
        target = null;
        aim = null;
        occludedTime = 0f;
        if (motor != null) motor.FacingTarget = null;
        if (marker != null) marker.gameObject.SetActive(false);
    }

    private static Transform AimOf(EnemyHealth enemy)
    {
        EnemyLanternReceiver receiver = enemy.GetComponent<EnemyLanternReceiver>();
        return receiver != null && receiver.AimPoint != null ? receiver.AimPoint : enemy.transform;
    }

    private static Vector3 AimPositionOf(EnemyHealth enemy)
    {
        Transform point = AimOf(enemy);
        return point != enemy.transform ? point.position : enemy.transform.position + Vector3.up * 1.2f;
    }

    private bool IsOccluded(Vector3 point) =>
        Physics.Linecast(transform.position + Vector3.up * 1.5f, point, blockers, QueryTriggerInteraction.Ignore);

    private float FlatDistance(Vector3 point)
    {
        Vector3 d = point - transform.position;
        d.y = 0f;
        return d.magnitude;
    }
}
