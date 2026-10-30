using UnityEngine;

public class DrumPuzzleReset : MonoBehaviour, IInteractable
{
    [SerializeField] private DrumPuzzleController puzzle;
    public string Prompt => "Lam lai cau do trong";

    public bool CanInteract(GameObject interactor) =>
        isActiveAndEnabled && puzzle != null && puzzle.isActiveAndEnabled &&
        puzzle.State != DrumPuzzleState.Solved;

    public void Interact(GameObject interactor)
    {
        if (CanInteract(interactor)) puzzle.ResetAttempt();
    }
}
