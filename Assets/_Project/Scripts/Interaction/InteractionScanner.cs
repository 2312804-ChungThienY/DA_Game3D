using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionScanner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private TMP_Text promptText;

    private InputAction interactAction;

    public event System.Action Interacted;

    [Header("Detection")]
    [SerializeField, Min(0.1f)] private float interactDistance = 3f;
    [SerializeField, Min(1f)] private float cameraRayDistance = 25f;

    // Bao gồm vật tương tác và vật cản; loại Player.
    [SerializeField] private LayerMask detectionMask;

    private void Awake()
    {
        if (viewCamera == null ||
            interactionOrigin == null ||
            promptText == null)
        {
            Debug.LogError(
                $"{name}: InteractionScanner thiếu tham chiếu.",
                this);

            enabled = false;
        }

        ClearPrompt();
    }

    private void Start()
    {
        InputReader input = AppRoot.Instance != null ? AppRoot.Instance.Input : null;
        interactAction = input != null && input.IsReady ? input.Interact : null;

        if (interactAction == null)
        {
            Debug.LogError($"{name}: Không tìm thấy action Interact trong InputReader.", this);
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        ClearPrompt();

        if (!Application.isFocused ||
            Time.timeScale == 0f ||
            interactAction == null ||
            !interactAction.enabled)
        {
            return;
        }

        // Tìm collider đầu tiên ở giữa màn hình.
        Ray cameraRay = viewCamera.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f));

        if (!Physics.Raycast(
                cameraRay,
                out RaycastHit hit,
                cameraRayDistance,
                detectionMask,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        IInteractable target =
            hit.collider.GetComponentInParent<IInteractable>();

        if (target == null || !target.CanInteract(gameObject))
        {
            return;
        }

        // Đo từ An, không đo từ camera phía sau.
        Vector3 toTarget = hit.point - interactionOrigin.position;
        float distance = toTarget.magnitude;

        // Vật tương tác có thể quy định tầm riêng (ví dụ ô đất điều khiển từ xa).
        float maxDistance = target is IInteractRange range
            ? range.MaxInteractDistance
            : interactDistance;

        if (distance > maxDistance)
        {
            return;
        }

        // Kiểm tra vật cản từ An đến điểm đang ngắm.
        if (distance > 0.01f &&
            Physics.Raycast(
                interactionOrigin.position,
                toTarget.normalized,
                out RaycastHit blocker,
                distance,
                detectionMask,
                QueryTriggerInteraction.Ignore))
        {
            IInteractable visibleTarget =
                blocker.collider.GetComponentInParent<IInteractable>();

            if (visibleTarget != target)
            {
                return;
            }
        }

        promptText.text = $"[E] {target.Prompt}";

        if (interactAction.WasPressedThisFrame())
        {
            target.Interact(gameObject);
            Interacted?.Invoke();
            ClearPrompt();
        }
    }

    private void OnDisable()
    {
        ClearPrompt();
    }

    private void ClearPrompt()
    {
        if (promptText != null)
        {
            promptText.text = string.Empty;
        }
    }
}
