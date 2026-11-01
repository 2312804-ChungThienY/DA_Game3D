using StarterAssets;
using TMPro;
using UnityEngine;

public class RelicRestorationAltar : MonoBehaviour, IInteractable
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private SaveService saveService;
    [SerializeField] private ChapterCompletionPresenter completionPresenter;
    [SerializeField] private GameObject brokenDrumVisual;
    [SerializeField] private GameObject restoredDrumVisual;
    [SerializeField] private Light restoredLight;
    [SerializeField] private TMP_Text feedbackText;
    private bool busy;

    public string Prompt => "Phuc dung Trong Dong";

    private void Awake()
    {
        if (worldState == null || saveService == null || completionPresenter == null ||
            brokenDrumVisual == null || restoredDrumVisual == null ||
            brokenDrumVisual == gameObject || restoredDrumVisual == gameObject)
        {
            Debug.LogError($"{name}: Check altar references and child visuals.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (worldState != null) worldState.Changed += ApplyVisuals;
    }

    private void Start() => ApplyVisuals();

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= ApplyVisuals;
    }

    public bool CanInteract(GameObject interactor)
    {
        if (!isActiveAndEnabled || busy || interactor == null || worldState == null ||
            saveService == null || completionPresenter == null ||
            !completionPresenter.isActiveAndEnabled || !worldState.CanRestoreRelic)
            return false;

        PlayerVitals vitals = interactor.GetComponent<PlayerVitals>();
        ThirdPersonController motor = interactor.GetComponent<ThirdPersonController>();
        PlayerCombatController combat = interactor.GetComponent<PlayerCombatController>();
        return vitals != null && !vitals.IsDead &&
            (motor == null || !motor.IsDodging) &&
            (combat == null || !combat.IsAttacking);
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        busy = true;
        try
        {
            // SaveService commits the completed snapshot BEFORE changing WorldState.
            if (!saveService.TryCompleteChapter())
            {
                if (feedbackText != null) feedbackText.text = "Chua the hoan tat. Hay thu lai.";
                return;
            }
            if (feedbackText != null) feedbackText.text = string.Empty;
            completionPresenter.RefreshFromWorldState();
        }
        finally
        {
            busy = false;
        }
    }

    private void ApplyVisuals()
    {
        if (worldState == null || brokenDrumVisual == null || restoredDrumVisual == null) return;
        bool restored = worldState.IsRelicRestored;
        brokenDrumVisual.SetActive(!restored);
        restoredDrumVisual.SetActive(restored);
        if (restoredLight != null) restoredLight.enabled = restored;
    }
}
