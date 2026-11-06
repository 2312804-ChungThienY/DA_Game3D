using System.Collections.Generic;
using UnityEngine;

// Cửa mở bằng E khi đủ mọi yêu cầu; lời nhắc liệt kê thứ còn thiếu. Trạng thái mở lưu trong Doors.
public class RequirementDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private WorldStateStore worldState;
    [SerializeField] private string doorId = WorldIds.DoorKimQuy;
    [SerializeField] private string doorName = "cổng Đàn Kim Quy";
    [SerializeField] private GameObject door;
    [SerializeField] private List<StateRequirement> requirements = new List<StateRequirement>();
    [Tooltip("Lưu tại mốc trước đấu trường ngay khi mở, để thua trận không phải lấy lại đồ.")]
    [SerializeField] private SaveService saveService;
    [SerializeField] private string saveCheckpointId;
    private bool opening;

    public bool IsOpen => worldState != null && worldState.Has(StateSet.Doors, doorId);

    public string Prompt
    {
        get
        {
            string missing = MissingText();
            return missing == null ? $"Mở {doorName}" : $"{Capitalize(doorName)} – còn thiếu: {missing}";
        }
    }

    private void Awake()
    {
        if (worldState == null || door == null || door == gameObject || !WorldIds.IsKnown(StateSet.Doors, doorId))
        {
            Debug.LogError($"{name}: RequirementDoor thiếu WorldState, Door hoặc ID cửa sai.", this);
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

    public bool CanInteract(GameObject interactor) => isActiveAndEnabled && !IsOpen && !opening;

    public void Interact(GameObject interactor)
    {
        if (!CanInteract(interactor)) return;

        string missing = MissingText();
        if (missing != null)
        {
            if (AppRoot.Instance != null && AppRoot.Instance.UI != null)
                AppRoot.Instance.UI.ShowToast($"Cổng chưa mở: thiếu {missing}");
            return;
        }

        opening=true;
        var before=worldState.Export();
        try
        {
            if(!worldState.TryAddBatch(new[]{new StateRequirement{set=StateSet.Doors,id=doorId}},out bool changed))return;
            if(saveService!=null&&!string.IsNullOrEmpty(saveCheckpointId)&&!saveService.SaveAt(saveCheckpointId))
            {
                worldState.Restore(before);
                if(AppRoot.Instance?.UI!=null)AppRoot.Instance.UI.ShowToast("Chưa lưu được. Cổng vẫn đóng; hãy thử mở lại.");
                return;
            }
            if(changed)worldState.PublishChange();
        }
        finally{opening=false;}
    }

    private void Apply()
    {
        if (door != null) door.SetActive(!IsOpen);
    }

    private string MissingText()
    {
        List<string> missing = new List<string>();
        foreach (StateRequirement requirement in requirements)
            if (requirement != null && !requirement.IsMet(worldState)) missing.Add(requirement.displayName);
        return missing.Count == 0 ? null : string.Join(", ", missing);
    }

    private static string Capitalize(string text) =>
        string.IsNullOrEmpty(text) ? text : char.ToUpper(text[0]) + text.Substring(1);
}
