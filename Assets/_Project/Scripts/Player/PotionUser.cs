using UnityEngine;

// Phím Q: dùng một thuốc máu (spec phần 17). Trừ thuốc và cộng máu trong cùng lệnh.
[RequireComponent(typeof(PlayerVitals))]
public class PotionUser : MonoBehaviour
{
    [SerializeField] private InventoryService inventory;
    [SerializeField] private float healAmount = 35f;
    [SerializeField] private float cooldown = 2f;

    private PlayerVitals vitals;
    private InputReader input;
    private float readyAt;

    public float CooldownRemaining => Mathf.Max(0f, readyAt - Time.time);

    private void Awake()
    {
        vitals = GetComponent<PlayerVitals>();
        if (inventory == null)
        {
            Debug.LogError($"{name}: PotionUser thiếu Inventory.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        input = AppRoot.Instance != null ? AppRoot.Instance.Input : null;
        if (input == null || !input.IsReady)
        {
            Debug.LogError($"{name}: PotionUser không tìm thấy InputReader.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f || !input.WasPressed(input.UsePotion)) return;
        TryUsePotion();
    }

    public bool TryUsePotion()
    {
        var actions = GetComponent<PlayerActionState>();
        if ((actions != null && !actions.CanInteract) || !isActiveAndEnabled || vitals.IsDead || Time.timeScale == 0f) return false;

        if (inventory.Count(InventoryService.ThuocMau) <= 0) { Toast("Không còn thuốc máu"); return false; }
        if (vitals.CurrentHealth >= vitals.MaxHealth) { Toast("Máu đang đầy"); return false; }
        if (Time.time < readyAt) return false;

        if (!inventory.TryRemove(InventoryService.ThuocMau, 1)) return false;
        vitals.Heal(healAmount);
        readyAt = Time.time + cooldown;
        Toast($"Hồi {Mathf.RoundToInt(healAmount)} máu");
        return true;
    }

    private static void Toast(string message)
    {
        if (AppRoot.Instance != null && AppRoot.Instance.UI != null) AppRoot.Instance.UI.ShowToast(message, 1.2f);
    }
}
