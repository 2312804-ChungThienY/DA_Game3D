using UnityEngine;

// Vật trao năng lực vĩnh viễn (spec chapters/00): E → thêm vào Abilities, hiện thẻ hướng dẫn, ẩn vật.
public class AbilityPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private StoryCardPanel storyPanel;
    [SerializeField] private string abilityId = WorldIds.AbilityWind;
    [SerializeField] private string displayName = "Lông Chim Lạc";
    [SerializeField] private StoryCard card;

    public string Prompt => $"Nhận {displayName}";

    private void Awake()
    {
        if (worldState == null || storyPanel == null || !WorldIds.IsKnown(StateSet.Abilities, abilityId))
        {
            Debug.LogError($"{name}: AbilityPickup thiếu tham chiếu hoặc ID năng lực sai.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (worldState != null) worldState.Changed += Apply;
    }

    private void OnDisable()
    {
        if (worldState != null) worldState.Changed -= Apply;
    }

    private void Start() => Apply();

    public bool CanInteract(GameObject interactor) => isActiveAndEnabled && !worldState.Has(StateSet.Abilities, abilityId);

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        worldState.TryAdd(StateSet.Abilities, abilityId);
        if (card != null) storyPanel.Show(card);
    }

    private void Apply()
    {
        if (worldState.Has(StateSet.Abilities, abilityId)) gameObject.SetActive(false);
    }
}
