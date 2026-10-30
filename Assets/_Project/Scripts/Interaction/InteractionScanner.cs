using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionScanner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private Transform interactionOrigin;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private InputActionReference interactAction;

    [Header("Detection")]
    [SerializeField, Min(0.1f)] private float interactDistance = 3f;
    [SerializeField, Min(1f)] private float cameraRayDistance = 15f;

    // Bao gồm vật tương tác và vật cản; loại Player.
    [SerializeField] private LayerMask detectionMask;

    private void Awake()
    {
        if (viewCamera == null ||
            interactionOrigin == null ||
            promptText == null ||
            interactAction == null ||
            interactAction.action == null)
        {
            Debug.LogError(
                $"{name}: InteractionScanner thiếu tham chiếu.",
                this);

            enabled = false;
        }

        ClearPrompt();
    }

    private void LateUpdate()
    {
        ClearPrompt();

        InputAction action = interactAction.action;

        if (!Application.isFocused ||
            Time.timeScale == 0f ||
            !action.enabled)
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

        if (distance > interactDistance)
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

        if (action.WasPressedThisFrame())
        {
            target.Interact(gameObject);
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