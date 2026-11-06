using System.Collections;
using UnityEngine;

// Một nút của câu đố chuỗi (chuông, vành trống): E → controller.TryInput(nodeId).
public class SequenceInputNode : MonoBehaviour, IInteractable
{
    [SerializeField] private SequencePuzzleController controller;
    [SerializeField] private string nodeId;
    [SerializeField] private string prompt = "Gõ chuông";
    [SerializeField] private GameObject hitIndicator;

    public string Prompt => prompt;

    private void Awake()
    {
        if (controller == null || string.IsNullOrEmpty(nodeId))
        {
            Debug.LogError($"{name}: SequenceInputNode thiếu Controller hoặc Node Id.", this);
            enabled = false;
        }
        if (hitIndicator != null) hitIndicator.SetActive(false);
    }

    public bool CanInteract(GameObject interactor) => isActiveAndEnabled && controller.CanAcceptInput;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor) || !controller.TryInput(nodeId)) return;
        if (hitIndicator != null)
        {
            StopAllCoroutines();
            StartCoroutine(Flash());
        }
    }

    private IEnumerator Flash()
    {
        hitIndicator.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        hitIndicator.SetActive(false);
    }
}
