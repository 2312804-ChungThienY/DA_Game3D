using UnityEngine;

// Vật trao năng lực vĩnh viễn (spec chapters/00): E → thêm vào Abilities, hiện thẻ hướng dẫn, ẩn vật.
public class AbilityPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private StoryCardPanel storyPanel;
    [SerializeField] private string abilityId = WorldIds.AbilityWind;
    [SerializeField] private string displayName = "Lông Chim Lạc";
    [SerializeField] private StoryCard card;
    private Renderer[] renderers;
    private Collider[] colliders;
    private bool[] rendererDefaults, colliderDefaults;

    public string Prompt => $"Nhận {displayName}";

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);
        rendererDefaults = System.Array.ConvertAll(renderers, r => r.enabled);
        colliderDefaults = System.Array.ConvertAll(colliders, c => c.enabled);
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

    public bool CanInteract(GameObject interactor) => isActiveAndEnabled && interactor != null &&
        interactor.TryGetComponent<PlayerVitals>(out var player) && !player.IsDead &&
        AppRoot.Instance != null && AppRoot.Instance.Session.State == GameSession.SessionState.Playing &&
        !worldState.Has(StateSet.Abilities, abilityId);

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        if (!worldState.TryAdd(StateSet.Abilities, abilityId)) return;
        if (card != null) storyPanel.Show(card);
    }

    private void Apply()
    {
        if (worldState == null || renderers == null) return;
        bool visible = !worldState.Has(StateSet.Abilities, abilityId);
        // Keep the state listener alive so restoring an earlier snapshot can show it again.
        for (int i = 0; i < renderers.Length; i++) if (renderers[i]) renderers[i].enabled = visible && rendererDefaults[i];
        for (int i = 0; i < colliders.Length; i++) if (colliders[i]) colliders[i].enabled = visible && colliderDefaults[i];
    }
}
