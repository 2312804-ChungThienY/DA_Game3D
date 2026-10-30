using UnityEngine;

public interface IInteractable
{
    // Nội dung lời nhắc
    string Prompt { get; }

    // Hiện tại có được tương tác không
    bool CanInteract(GameObject interactor);

    // 	Thực hiện thao tác
    void Interact(GameObject interactor);
}