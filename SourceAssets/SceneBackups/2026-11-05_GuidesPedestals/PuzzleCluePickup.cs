using UnityEngine;

public sealed class PuzzleCluePickup:MonoBehaviour,IInteractable
{
    [SerializeField] WorldStateStore worldState;
    [SerializeField] StoryCardPanel storyPanel;
    [SerializeField] string clueId;
    [SerializeField] GameObject paperVisual;
    [SerializeField] Collider interactionCollider;
    public bool IsFound=>worldState&&worldState.Has(StateSet.Flags,clueId);
    public string ClueId=>clueId;
    public string Prompt=>clueId==WorldIds.ClueDrum?"Nhặt bản chép về ba trống":"Nhặt ghi chép về gương";
    public static string Title(string id)=>id==WorldIds.ClueDrum?"Bản chép: Nhịp thức tỉnh":"Ghi chép: Đường đi của ánh sáng";
    public static string Body(string id)=>id==WorldIds.ClueDrum
        ?"Trên ba trống có ba dấu: Mặt Trời, Cánh Chim và Sóng Nước.\n\n“Ánh ngày gọi cánh chim thức giấc; chim lướt qua mặt nước, linh mạch mới ngân vang.”\n\nGõ theo thứ tự <b>Mặt Trời → Cánh Chim → Sóng Nước</b>. Nếu nhịp sai, chờ tiếng trống lắng rồi bắt đầu lại.\n\nĐây là quy ước giải đố trong thế giới hư cấu của game. Bản chép được giữ trong Nhật ký (J)."
        :"Nguồn sáng đứng yên, hai gương dẫn tia tới viên ngọc. Mặt bạc có đường viền ngọc là mặt nhận sáng; mặt đồng phía sau không phản xạ.\n\nDùng E xoay từng gương theo một phần tư vòng. Theo dõi tia: chỉnh gương nhận tia đầu tiên để dẫn tới gương còn lại, rồi chỉnh gương thứ hai hướng về ngọc. Tia phải đi qua <b>cả hai gương</b>.\n\nGiữ đường sáng nối tới ngọc trong một giây để mở bệ thưởng. Có thể đứng ở bên cạnh bệ để nhìn rõ đường tia. Ghi chép được giữ trong Nhật ký (J).";
    void Awake(){if(!worldState||!storyPanel||!paperVisual||!interactionCollider||!WorldIds.IsKnown(StateSet.Flags,clueId)){Debug.LogError(name+": thiếu tham chiếu giấy gợi ý.",this);enabled=false;}}
    void OnEnable(){if(worldState)worldState.Changed+=Apply;}
    void OnDisable(){if(worldState)worldState.Changed-=Apply;}
    void Start()=>Apply();
    public bool CanInteract(GameObject interactor)=>isActiveAndEnabled&&!IsFound&&interactor&&AppRoot.Instance&&AppRoot.Instance.Session.State==GameSession.SessionState.Playing&&interactor.GetComponent<PlayerVitals>()&&!interactor.GetComponent<PlayerVitals>().IsDead;
    public void Interact(GameObject interactor)
    {
        if(!CanInteract(interactor)||!worldState.TryAdd(StateSet.Flags,clueId))return;
        SoundscapeRuntime.Play(SoundCue.Pickup);storyPanel.Show(Title(clueId),Body(clueId));Apply();
    }
    void Apply(){if(!paperVisual||!interactionCollider||!worldState)return;paperVisual.SetActive(!IsFound);interactionCollider.enabled=!IsFound;}
}
