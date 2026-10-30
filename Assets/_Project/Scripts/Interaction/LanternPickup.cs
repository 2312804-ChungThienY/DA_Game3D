using UnityEngine;

public class LanternPickup : MonoBehaviour, IInteractable
{
    private bool collected;

    // Dùng không dấu để thử với font TMP mặc định.
    public string Prompt => "Nhan Long Den Sen";

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
}