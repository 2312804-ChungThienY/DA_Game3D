using UnityEngine;
using System;
using System.Collections.Generic;

public sealed class PuzzleCluePickup:MonoBehaviour,IInteractable
{
    [SerializeField] WorldStateStore worldState;
    [SerializeField] StoryCardPanel storyPanel;
    [SerializeField] string clueId;
    [SerializeField] GameObject paperVisual;
    [SerializeField] Collider interactionCollider;
    public bool IsFound=>worldState&&worldState.Has(StateSet.Flags,clueId);
    public string ClueId=>clueId;
    public static IReadOnlyList<string> Chapter1ClueIds{get;}=Array.AsReadOnly(new[]{WorldIds.GuideCampIntro,WorldIds.GuideCampControls,WorldIds.ClueDrum,WorldIds.ClueMirror,WorldIds.GuideKimQuy});
    public static IReadOnlyList<string> AllClueIds{get;}=Array.AsReadOnly(new[]{WorldIds.GuideCampIntro,WorldIds.GuideCampControls,WorldIds.ClueDrum,WorldIds.ClueMirror,WorldIds.GuideKimQuy,WorldIds.GuideWindC2});
    public string Prompt=>"Nhặt "+Title(clueId);
    public static string Title(string id)
    {
        switch(id)
        {
            case WorldIds.GuideCampIntro:return "Lời nhắn ở Trại Giếng Trời";
            case WorldIds.GuideCampControls:return "Sổ tay: Những bước đầu của An";
            case WorldIds.GuideKimQuy:return "Lời dặn trước cổng Đàn Kim Quy";
            case WorldIds.GuideWindC2:return "Theo luồng gió: Giếng Trời Chim Lạc";
            case WorldIds.ClueDrum:return "Bản chép: Nhịp thức tỉnh";
            case WorldIds.ClueMirror:return "Ghi chép: Đường đi của ánh sáng";
            default:return "Bản chép";
        }
    }
    public static string Body(string id)
    {
        switch(id)
        {
            case WorldIds.GuideWindC2:return "<b>Từ Đàn Kim Quy đến Giếng Trời</b>\n\nLinh mạch Kim Quy đã thức tỉnh. An theo luồng gió qua khe đá tới một giếng trời cao; trên đỉnh, Hắc Sương vẫn bao quanh Chim Lạc. Hành trình tiếp theo là đi lên, tìm <b>ba mảnh Mặt Trời Lông Chim</b> và giải thoát Linh Thần.\n\n<b>Bắt đầu tại đáy giếng</b>\nNhìn vào chiếc lông vàng trong khoảng sáng trước mặt rồi nhấn <b>E</b>. Sau khi nhận Lông Chim Lạc, bước vào <b>cột gió xanh bên phải</b> để được nâng lên. Đợi lơ lửng gần đỉnh, dùng WASD và Space nhảy sang gờ đá; cột gió không chở bạn ngang qua phòng.\n\nRọi đèn thường (F) vào bia trên gờ, đọc kiến thức rồi tìm Bệ Vấn Đáp để nhận mảnh đầu tiên. Phúc Kim Quy vẫn theo An: Ctrl trái + hướng để lộn né, tốn thể lực.\n\n<b>Lưu hành trình</b>\nLửa trại ở bên trái là mốc lưu đầu chương. Sau khi nhận lông hoặc nhặt giấy, ghé lại và nhấn E để lưu; chưa lưu thì Continue trở về trạng thái ở mốc trước. Giấy này được giữ trong <b>Nhật ký (J)</b>. Đây là bối cảnh hư cấu của game.";
            case WorldIds.GuideCampIntro:return "<b>Sơn Đoòng: Cõi Mộng Kỳ Thần</b>\n\nBạn vào vai <b>An</b>, một nhà khảo cổ trẻ lạc vào miền hang động nơi Hắc Sương đang che phủ linh mạch. Đây là cuộc phiêu lưu hành động góc nhìn thứ ba, kết hợp khám phá, giải đố và chiến đấu bằng Lồng Đèn Sen.\n\nTrong chương Kim Quy, hành trình đi qua <b>Trại Giếng Trời → Vườn Rêu → Điện Trống → Hành Lang Quang → Đàn Kim Quy</b>. Tìm ba mảnh Trống Đồng, giải hai câu đố và vượt qua Hộ vệ để phục hồi linh mạch.\n\nHãy quan sát các khoảng sáng, ký hiệu và bản chép dọc đường. Câu chuyện, công trình cổ và phong ấn là hư cấu của game, lấy cảm hứng từ vẻ đẹp hang động Sơn Đoòng.\n\nCác giấy đã nhặt có thể đọc lại trong <b>Nhật ký (J)</b>.";
            case WorldIds.GuideCampControls:return "<b>Di chuyển và quan sát</b>\nWASD: di chuyển · Chuột: xoay camera\nShift trái: chạy · Space: nhảy\nCtrl trái: lộn né theo hướng di chuyển\n\n<b>Nhìn vào vật rồi nhấn E</b> để nhặt đồ, đọc giấy hoặc tương tác. Nhận Lồng Đèn Sen trên bệ gần lửa trại. F bật/tắt đèn; giữ chuột phải để chiếu tập trung vào phong ấn hoặc quái có đường nhìn thông thoáng. Quái bị chiếu đủ lâu sẽ suy yếu và choáng; khi đèn cạn, thả chuột để hồi năng lượng.\n\n<b>Chiến đấu</b>\nChuột trái: đánh dao · Bấm con lăn: khóa/bỏ khóa mục tiêu\nTheo dõi thể lực và tránh các đòn đang báo trước.\n\n<b>Vật phẩm và lưu game</b>\nI: túi đồ, chế Bình máu từ 1 Lá cẩm + 1 Linh chi.\nQ: dùng Bình máu khi đang chơi. J: Nhật ký. Esc: tạm dừng/quay lại.\n\nNhấn <b>E tại lửa trại hoặc checkpoint</b> để lưu và hồi phục. Hãy lưu để giữ vật phẩm và giấy đã nhặt; chế thuốc không tự lưu.";
            case WorldIds.GuideKimQuy:return "<b>Chuẩn bị trước khi vào Đàn Kim Quy</b>\n\nCổng chỉ mở khi bạn đã <b>thu đủ ba mảnh Trống Đồng</b> và giải xong <b>câu đố ba trống cùng câu đố gương</b>. Giải câu đố chưa đồng nghĩa đã nhận mảnh thưởng: nhớ tới từng bệ để nhặt. Khi đủ điều kiện, nhìn vào cổng rồi nhấn E.\n\nGhé checkpoint ở bên phải để <b>lưu và hồi đầy tài nguyên</b>. Mở I để chế thêm Bình máu nếu có nguyên liệu; dùng Q khi cần hồi máu trong trận.\n\n<b>Hộ vệ Đá</b> báo trước các đòn quét gần và nện xuống một vùng. Lộn né hoặc rời khỏi vùng đánh, chờ lúc nó thu đòn rồi phản công. Chiếu tập trung bằng chuột phải làm Hộ vệ suy yếu và choáng ngắn; ngôi sao cùng động tác cúi đầu báo lúc có thể áp sát. Hộ vệ có khoảng chống choáng lặp, đừng đứng yên chiếu liên tục.\n\nSau khi hạ Hộ vệ, tìm đàn ở cuối hang và nhấn <b>E để phục dựng linh mạch</b>. Ghi chép này có thể đọc lại trong Nhật ký (J).";
            case WorldIds.ClueDrum:return "Trên ba trống có ba dấu: Mặt Trời, Cánh Chim và Sóng Nước.\n\n“Ánh ngày gọi cánh chim thức giấc; chim lướt qua mặt nước, linh mạch mới ngân vang.”\n\nGõ theo thứ tự <b>Mặt Trời → Cánh Chim → Sóng Nước</b>. Nếu nhịp sai, chờ tiếng trống lắng rồi bắt đầu lại.\n\nĐây là quy ước giải đố trong thế giới hư cấu của game. Bản chép được giữ trong Nhật ký (J).";
            case WorldIds.ClueMirror:return "Nguồn sáng đứng yên, hai gương dẫn tia tới viên ngọc. Mặt bạc có đường viền ngọc là mặt nhận sáng; mặt đồng phía sau không phản xạ.\n\nDùng E xoay từng gương theo một phần tư vòng. Theo dõi tia: chỉnh gương nhận tia đầu tiên để dẫn tới gương còn lại, rồi chỉnh gương thứ hai hướng về ngọc. Tia phải đi qua <b>cả hai gương</b>.\n\nGiữ đường sáng nối tới ngọc trong một giây để mở bệ thưởng. Có thể đứng ở bên cạnh bệ để nhìn rõ đường tia. Ghi chép được giữ trong Nhật ký (J).";
            default:return string.Empty;
        }
    }
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
