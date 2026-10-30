using UnityEngine;

public class CampCheckpoint : MonoBehaviour, IInteractable
{
    [SerializeField] private SaveService saveService;

    public string Prompt => "Luu tien trinh tai trai";

    public bool CanInteract(GameObject interactor)
    {
        return isActiveAndEnabled &&
               saveService != null &&
               saveService.isActiveAndEnabled;
    }

    public void Interact(GameObject interactor)
    {
        if (CanInteract(interactor))
        {
            saveService.SaveAtCamp();
        }
    }
}
