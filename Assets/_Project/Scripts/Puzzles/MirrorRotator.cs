using UnityEngine;

public class MirrorRotator : MonoBehaviour, IInteractable
{
    [SerializeField] private LightPuzzleReceiver receiver;
    [SerializeField] private float baseYaw;
    [SerializeField, Range(0, 3)] private int initialStep;

    private int currentStep;

    public Vector3 FrontNormal => transform.forward;
    public string Prompt => "Xoay gương 90 độ";

    private void Awake()
    {
        if (receiver == null)
        {
            Debug.LogError($"{name}: Chưa gán Receiver.", this);
            enabled = false;
            return;
        }

        currentStep = initialStep;
        ApplyRotation();
    }

    public bool CanInteract(GameObject interactor)
    {
        return isActiveAndEnabled && receiver != null &&
               receiver.isActiveAndEnabled && !receiver.IsSolved;
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        currentStep = (currentStep + 1) % 4;
        ApplyRotation();
    }

    private void ApplyRotation()
    {
        transform.localRotation = Quaternion.Euler(0f, baseYaw + currentStep * 90f, 0f);
    }

    public void ApplySolvedState(bool solved)
    {
        currentStep = solved ? 0 : initialStep;
        ApplyRotation();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
