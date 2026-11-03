using UnityEngine;

public class TestPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private string itemName = "Vật phẩm thử";

    private bool collected;

    public string Prompt => $"Nhặt {itemName}";

    public bool CanInteract(GameObject interactor)
    {
        return !collected && isActiveAndEnabled;
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor))
        {
            return;
        }

        // Đánh dấu trước để chặn nhận lặp.
        collected = true;

        Debug.Log(
            $"{interactor.name} đã nhặt: {itemName}",
            this);

        gameObject.SetActive(false);
    }
}