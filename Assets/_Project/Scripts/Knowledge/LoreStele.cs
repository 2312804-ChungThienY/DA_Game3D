using TMPro;
using UnityEngine;

// Bia văn tự: chữ chỉ hiện khi rọi đèn trong 5 m (LanternRevealSurface). Lần đầu hiện thì ghi Lore vào
// WorldStateStore (Nhật ký mục Tri thức đọc từ tập này). E để đọc toàn văn khi chữ đã hiện.
public class LoreStele : MonoBehaviour, IInteractable
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private LoreDefinition lore;
    [SerializeField] private LanternRevealSurface reveal;
    [SerializeField] private TMP_Text text3D;
    [SerializeField] private StoryCardPanel readPanel;

    public bool IsRead => worldState != null && lore != null && worldState.Has(StateSet.Lore, lore.loreId);
    public string Prompt => reveal.IsRevealed ? $"Đọc bia: {lore.title}" : "Bia đá phủ rêu – bật đèn và lại gần để đọc";

    private void Awake()
    {
        if (worldState == null || lore == null || reveal == null || text3D == null ||
            !WorldIds.IsKnown(StateSet.Lore, lore.loreId))
        {
            Debug.LogError($"{name}: LoreStele thiếu tham chiếu hoặc ID bia sai.", this);
            enabled = false;
            return;
        }
        text3D.text = $"[{ContentTagText.Label(lore.tag)}]\n{lore.title}\n\n{lore.body}";
    }

    private void Update()
    {
        if (reveal.IsRevealed && !IsRead) worldState.TryAdd(StateSet.Lore, lore.loreId);
    }

    public bool CanInteract(GameObject interactor) => isActiveAndEnabled;

    public void Interact(GameObject interactor)
    {
        if (!reveal.IsRevealed || readPanel == null) return;
        readPanel.Show($"{lore.title}  ({ContentTagText.Label(lore.tag)})", lore.body);
    }
}
