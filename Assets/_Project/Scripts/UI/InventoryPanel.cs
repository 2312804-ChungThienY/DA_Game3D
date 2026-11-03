using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Túi 12 ô chỉ để xem (phím I). Không thay đổi inventory; dùng thuốc bằng Q ngoài menu.
public class InventoryPanel : MonoBehaviour, IUiPauseOwner
{
    [SerializeField] private InventoryService inventory;
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private LanternController lantern;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button[] slotButtons = new Button[InventoryService.SlotCount];
    [SerializeField] private TMP_Text[] slotTexts = new TMP_Text[InventoryService.SlotCount];
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private TMP_Text keyItemsText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Color normalColor = new Color(0.15f, 0.15f, 0.18f, 0.95f);
    [SerializeField] private Color selectedColor = new Color(0.55f, 0.42f, 0.12f, 0.95f);

    private InputReader input;
    private int selected;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        bool slotsOk = slotButtons != null && slotTexts != null &&
                       slotButtons.Length == InventoryService.SlotCount && slotTexts.Length == InventoryService.SlotCount;
        if (slotsOk)
            for (int i = 0; i < InventoryService.SlotCount; i++)
                slotsOk &= slotButtons[i] != null && slotTexts[i] != null;

        if (inventory == null || worldState == null || lantern == null || panel == null || !slotsOk ||
            detailText == null || keyItemsText == null || closeButton == null)
        {
            Debug.LogError($"{name}: InventoryPanel thiếu tham chiếu.", this);
            enabled = false;
            return;
        }

        for (int i = 0; i < InventoryService.SlotCount; i++)
        {
            int index = i;
            slotButtons[i].onClick.AddListener(() => Select(index));
        }
        closeButton.onClick.AddListener(Close);
        panel.SetActive(false);
    }

    private void Start() => input = AppRoot.Instance != null ? AppRoot.Instance.Input : null;

    private void OnEnable()
    {
        if (inventory != null) inventory.Changed += Refresh;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.Changed -= Refresh;
    }

    private void Update()
    {
        if (input == null || !input.IsReady || !input.Inventory.WasPressedThisFrame()) return;

        if (IsOpen) Close();
        else if (input.GameplayEnabled) Open();
    }

    public void Open()
    {
        GameSession session = AppRoot.Instance != null ? AppRoot.Instance.Session : null;
        if (!enabled || IsOpen || session == null || !session.PushUiPause(this)) return;

        // Gameplay đã tắt; bật riêng phím I để đóng túi bằng chính phím mở.
        input.Inventory.Enable();
        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (!IsOpen) return;
        panel.SetActive(false);
        if (AppRoot.Instance != null) AppRoot.Instance.Session.PopUiPause(this);
        if (input != null && !input.GameplayEnabled) input.Inventory.Disable();
    }

    public void CloseFromBack() => Close();

    private void Select(int index)
    {
        selected = index;
        Refresh();
    }

    private void Refresh()
    {
        if (!IsOpen) return;

        for (int i = 0; i < InventoryService.SlotCount; i++)
        {
            InventoryService.Slot slot = inventory.GetSlot(i);
            ItemDefinition def = slot.IsEmpty ? null : inventory.GetDefinition(slot.itemId);
            slotTexts[i].text = def == null ? "—" : $"{def.displayName}\nx{slot.quantity}";
            slotButtons[i].image.color = i == selected ? selectedColor : normalColor;
        }

        InventoryService.Slot current = inventory.GetSlot(selected);
        ItemDefinition currentDef = current.IsEmpty ? null : inventory.GetDefinition(current.itemId);
        detailText.text = currentDef == null
            ? "Ô trống"
            : $"<b>{currentDef.displayName}</b> x{current.quantity} (tối đa {currentDef.maxStack})\n{currentDef.description}";

        // Dùng "\n" thay AppendLine để TMP không nhận "\r" trên Windows.
        StringBuilder key = new StringBuilder("<b>Trang bị</b>\n");
        key.Append("Dao găm\n");
        if (lantern.HasLantern) key.Append("Lồng Đèn Sen\n");
        key.Append($"\n<b>Mảnh Trống Đồng</b> {worldState.RelicCount}/3\n");
        AppendFragment(key, WorldStateStore.DrumA, "Mảnh Trống A");
        AppendFragment(key, WorldStateStore.DrumB, "Mảnh Trống B");
        AppendFragment(key, WorldStateStore.DrumC, "Mảnh Trống C");
        if (worldState.Has(StateSet.Puzzles, WorldIds.RelicRestoredC1)) key.Append("\n<b>Trống Đồng Kim Quy</b> (đã phục dựng)\n");
        keyItemsText.text = key.ToString();
    }

    private void AppendFragment(StringBuilder text, string id, string label)
    {
        text.Append(worldState.HasRelic(id) ? $"{label}: đã có\n" : $"{label}: chưa có\n");
    }
}
