using System.Text;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Túi 12 ô (I), chế bình máu qua giao dịch InventoryService; dùng thuốc bằng Q ngoài menu.
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
    [SerializeField] private Button craftPotionButton;
    [SerializeField] private TMP_Text recipeText;
    [SerializeField] private TMP_Text craftingStatusText;
    [SerializeField] private Image[] slotIcons;
    [SerializeField] private Color normalColor = new Color(0.15f, 0.15f, 0.18f, 0.95f);
    [SerializeField] private Color selectedColor = new Color(0.55f, 0.42f, 0.12f, 0.95f);

    private InputReader input;
    private int selected;
    private float nextCraft,nextCraftRefresh,noticeUntil;
    private static readonly List<KeyValuePair<string,int>> PotionRecipe=new List<KeyValuePair<string,int>>{new KeyValuePair<string,int>(InventoryService.LaCam,1),new KeyValuePair<string,int>(InventoryService.LinhChi,1)};

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
        if(craftPotionButton)craftPotionButton.onClick.AddListener(()=>TryCraftPotion());
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
        if(IsOpen&&craftPotionButton&&Time.unscaledTime>=nextCraftRefresh){RefreshCrafting();nextCraftRefresh=Time.unscaledTime+.12f;}
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

    public bool TryCraftPotion()
    {
        var session=AppRoot.Instance?AppRoot.Instance.Session:null;
        var vitals=lantern?lantern.GetComponent<PlayerVitals>():null;
        if(!IsOpen||session==null||session.State!=GameSession.SessionState.InMenu||!vitals||vitals.IsDead||Time.unscaledTime<nextCraft)return false;
        if(!inventory.TryCraft(PotionRecipe,InventoryService.ThuocMau,1,out string reason))
        {if(craftingStatusText)craftingStatusText.text=reason;RefreshCrafting();return false;}
        nextCraft=Time.unscaledTime+.25f;
        noticeUntil=Time.unscaledTime+2.5f;
        if(craftingStatusText)craftingStatusText.text="Đã chế 1 Bình máu. Lưu tại điểm lửa để giữ tiến trình.";
        SoundscapeRuntime.Play(SoundCue.Pickup);RefreshCrafting();return true;
    }
    private void RefreshCrafting()
    {
        if(!craftPotionButton)return;
        bool ready=inventory.CanCraft(PotionRecipe,InventoryService.ThuocMau,1,out string reason);
        craftPotionButton.interactable=IsOpen&&ready&&Time.unscaledTime>=nextCraft;
        if(recipeText)recipeText.text=$"<b>1 Lá cẩm</b>  ({inventory.Count(InventoryService.LaCam)}/1)\n+ <b>1 Linh chi</b>  ({inventory.Count(InventoryService.LinhChi)}/1)\n\nTạo 1 Bình máu · hồi 35 máu bằng Q";
        if(craftingStatusText&&!ready&&Time.unscaledTime>=noticeUntil)craftingStatusText.text=reason;
    }

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
            if(slotIcons!=null&&i<slotIcons.Length&&slotIcons[i]){slotIcons[i].sprite=def?def.icon:null;slotIcons[i].enabled=def&&def.icon;}
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
        AppendFragment(key, WorldStateStore.DrumA, "Mặt Trống");
        AppendFragment(key, WorldStateStore.DrumB, "Thân Trống");
        AppendFragment(key, WorldStateStore.DrumC, "Chân Trống");
        if (worldState.Has(StateSet.Puzzles, WorldIds.RelicRestoredC1)) key.Append("\n<b>Trống Đồng Kim Quy</b> (đã phục dựng)\n");
        keyItemsText.text = key.ToString();
        RefreshCrafting();
    }

    private void AppendFragment(StringBuilder text, string id, string label)
    {
        text.Append(worldState.HasRelic(id) ? $"{label}: đã có\n" : $"{label}: chưa có\n");
    }
}
