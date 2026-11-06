# HUD mục tiêu — cập nhật 05/11/2026

HUD C1 hiện mở đầu **Khám phá Vườn Rêu / Tìm mảnh Mặt Trống**, giữ hướng dẫn Vườn Rêu từ quyết định02/11–04/11 và bỏ tên mã A/B/C theo phản hồi05/11. `QuestTracker` dùng world state hiện hành, không áp lại ChapterQuestHUD/Rules của prototype. Tên người chơi thấy là Mặt Trống/Thân Trống/Chân Trống; ID `relic_drum_a/b/c` không đổi.

## Hành vi hiện hành

Tám mục tiêu đang chơi và nhánh hoàn thành: Vườn Rêu/Mặt Trống → giải trống → nhận Thân Trống → giải gương → nhận Chân Trống → mở cổng Kim Quy → Hộ vệ → phục dựng → hoàn thành. Các mục tiêu câu đố nhắc tìm giấy quanh khu, không bắt buộc nhặt giấy. Giải câu đố không tự coi đã nhận phần thưởng.

`objectiveText` và `progressText` tách riêng trong `C1_HUD_Theme/Quest_Frame`; panel 480×186, CanvasScaler reference 1920×1080. Tiến độ đếm ba relic_drum_a/b/c và **chỉ** pz_trong_01/pz_guong_01; không đếm restoration thành câu đố thứ ba. Không đổi ID để khớp tên trong thiết kế cũ.

HUD giữ HP/stamina/đèn, thuốc Q, E ở gần vật tương tác, tên khu và chú giải phím. Thanh đèn ẩn khi chưa sở hữu. Trạng thái phong ấn/trống/gương chuyển xuống gần prompt, giữ ProximityVisibility; không chồng chữ lên mép màn hình. Túi/nhật ký/story giữ controller và tham chiếu đang dùng; nội dung dài có ScrollRect.

Revision05/11: túi có icon, công thức/nút chế và phản hồi sức chứa; Nhật ký append hai manh mối đã tìm và mở ở đầu trang, vùng cuộn có scrollbar. Thẻ giao diện nền ngọc tối/viền đồng mảnh, không dùng ảnh khung đặc che nội dung. Kết quả mới tại [C1_REVISION_WORKLOG](C1_REVISION_WORKLOG.md); Completion dưới đây là mốc trước revision.

C2 vẫn dùng `ChapterQuestTracker` theo StateRequirement; lượt này không làm art/nhiệm vụ C2. Tracker chỉ đọc state và ghi text, không phát thưởng hoặc lưu currentQuestIndex. Native save schema4 giữ nguyên.

## Bằng chứng và giới hạn

[C1Completion Play Mode](reports/C1Completion_PlayMode_2026-11-05.json) kiểm mở đầu/0 trên 3 và 0 trên 2, UI pause/settings/túi/nhật ký, ending, Continue và death/respawn. [Traversal](reports/C1Completion_Traversal_2026-11-05.json) giải trống/gương bằng runtime, nhận riêng ba mảnh, mở cổng và khôi phục state. Bài traversal cô lập trùm bằng fixture; bài completion kiểm chết trùm qua combat thật với fixture vị trí và bất tử của người thử.

Ảnh `docs/images/c1-complete-*` có capture 1280×720, 1600×900 và 1600×1000; `c1-complete-native-*` là Game View 1920×1080. Đây là kiểm tra layout/automation, chưa phải người dùng nghiệm thu mọi nhánh bằng chơi tay hoặc kiểm accessibility. Báo cáo/ảnh HUD 02/11 và audit 04/11 giữ nhãn lịch sử trước bản này.
