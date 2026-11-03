using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Bảng chế thuốc tại lửa trại: 1 lá cẩm + 1 linh chi → 1 thuốc máu (spec phần 17).
public class CraftingPanel : MonoBehaviour, IUiPauseOwner
{
    [SerializeField] private InventoryService inventory;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text ingredientsText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text reasonText;
    [SerializeField] private Button craftButton;
    [SerializeField] private Button closeButton;

    private static readonly List<KeyValuePair<string, int>> Recipe = new List<KeyValuePair<string, int>>
    {
        new KeyValuePair<string, int>(InventoryService.LaCam, 1),
        new KeyValuePair<string, int>(InventoryService.LinhChi, 1)
    };

    private CheckpointAltar altar;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (inventory == null || panel == null || ingredientsText == null || resultText == null ||
            reasonText == null || craftButton == null || closeButton == null)
        {
            Debug.LogError($"{name}: CraftingPanel thiếu tham chiếu.", this);
            enabled = false;
            return;
        }

        craftButton.onClick.AddListener(Craft);
        closeButton.onClick.AddListener(Close);
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        if (inventory != null) inventory.Changed += Refresh;
    }

    private void OnDisable()
    {
        if (inventory != null) inventory.Changed -= Refresh;
    }

    public void Open(CheckpointAltar source)
    {
        GameSession session = AppRoot.Instance != null ? AppRoot.Instance.Session : null;
        if (!enabled || IsOpen || session == null || !session.PushUiPause(this)) return;

        altar = source;
        reasonText.text = string.Empty;
        panel.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (!IsOpen) return;
        panel.SetActive(false);
        altar = null;
        if (AppRoot.Instance != null) AppRoot.Instance.Session.PopUiPause(this);
    }

    public void CloseFromBack() => Close();

    private void Craft()
    {
        if (!IsOpen) return;

        if (inventory.TryCraft(Recipe, InventoryService.ThuocMau, 1, out string reason))
        {
            bool saved = altar == null || altar.SaveAfterCraft();
            reasonText.text = saved ? "Đã chế 1 Thuốc máu" : "Đã chế 1 Thuốc máu (chưa lưu được)";
        }
        else
        {
            reasonText.text = reason;
        }
        Refresh();
    }

    private void Refresh()
    {
        if (!IsOpen) return;

        ingredientsText.text =
            $"Lá cẩm: {inventory.Count(InventoryService.LaCam)}/1\n" +
            $"Linh chi: {inventory.Count(InventoryService.LinhChi)}/1";
        resultText.text = $"→ Thuốc máu (hồi 35 máu)\nĐang có: {inventory.Count(InventoryService.ThuocMau)}";
    }
}
