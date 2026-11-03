using System.Collections;
using UnityEngine;

public class DrumInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private DrumPuzzleController puzzle;
    [SerializeField] private DrumNote note;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private GameObject hitIndicator;

    public string Prompt => $"Gõ trống {note}";

    private void Awake()
    {
        if (puzzle == null || hitIndicator == gameObject)
        {
            Debug.LogError($"{name}: Gán Puzzle và HitIndicator riêng.", this);
            enabled = false;
            return;
        }
        if (hitIndicator != null) hitIndicator.SetActive(false);
    }

    public bool CanInteract(GameObject interactor) =>
        isActiveAndEnabled && puzzle != null && puzzle.CanAcceptInput;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor) || !puzzle.TryHit(note)) return;
        if (audioSource != null && hitSound != null) audioSource.PlayOneShot(hitSound);
        if (hitIndicator != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashIndicator());
        }
    }

    private IEnumerator FlashIndicator()
    {
        hitIndicator.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        hitIndicator.SetActive(false);
    }

    private void OnDisable()
    {
        if (hitIndicator != null) hitIndicator.SetActive(false);
    }
}
