using System.Collections.Generic;
using UnityEngine;

// The table reuses the existing inventory transaction. Saving remains an
// explicit interaction with the existing cp_trai checkpoint.
public sealed class CampCraftingTable : MonoBehaviour, IInteractable
{
    [SerializeField] private InventoryService inventory;
    private float nextInteraction;
    private static readonly List<KeyValuePair<string, int>> Recipe = new List<KeyValuePair<string, int>>
    {
        new KeyValuePair<string, int>(InventoryService.LaCam, 1),
        new KeyValuePair<string, int>(InventoryService.LinhChi, 1)
    };

    public string Prompt => "Chế thuốc: 1 Lá cẩm + 1 Linh chi";

    public bool CanInteract(GameObject interactor)
    {
        if (!isActiveAndEnabled || inventory == null || interactor == null ||
            AppRoot.Instance == null || AppRoot.Instance.Session.State != GameSession.SessionState.Playing ||
            Time.timeScale <= 0 || Time.time < nextInteraction) return false;
        var actions = interactor.GetComponent<PlayerActionState>();
        if (actions != null && !actions.CanInteract) return false;
        var vitals = interactor.GetComponent<PlayerVitals>();
        return vitals != null && !vitals.IsDead &&
            Vector3.Distance(interactor.transform.position, transform.position) <= 3f;
    }

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;
        nextInteraction = Time.time + .35f;
        bool crafted = inventory.TryCraft(Recipe, InventoryService.ThuocMau, 1, out string reason);
        SoundscapeRuntime.Play(crafted?SoundCue.Pickup:SoundCue.Hit,transform.position,.5f);
        AppRoot.Instance.UI?.ShowToast(crafted
            ? "Đã chế 1 Thuốc máu. Lưu tiến trình tại lửa trại."
            : reason, 3f);
    }
}
