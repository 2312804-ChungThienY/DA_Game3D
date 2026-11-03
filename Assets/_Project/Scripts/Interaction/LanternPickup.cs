using UnityEngine;

public class LanternPickup : MonoBehaviour, IInteractable
{
    private bool collected;

    public string Prompt => "Nhận Lồng Đèn Sen";

    public bool CanInteract(GameObject interactor)
    {
        if (collected || !isActiveAndEnabled)
        {
            return false;
        }

        LanternController lantern =
            interactor.GetComponent<LanternController>();

        return lantern != null &&
               lantern.isActiveAndEnabled &&
               !lantern.HasLantern;
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        LanternController lantern =
            interactor.GetComponent<LanternController>();

        // Chỉ loại vật ngoài cảnh sau khi cấp đèn thành công.
        if (!lantern.TryReceiveLantern())
        {
            return;
        }

        collected = true;
        gameObject.SetActive(false);
    }

    public void ApplySavedState(bool owned)
    {
        collected = owned;
        gameObject.SetActive(!owned);
    }
}
