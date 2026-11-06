# Hướng dẫn tiếp nối dự án Sơn Đoòng

## Đọc trước khi làm việc

1. Đọc `docs/PROJECT_MEMORY.md` để hiểu game, phạm vi và các hợp đồng hệ thống.
2. Đọc `docs/PROJECT_STATUS.md` để biết trạng thái thực tế, việc chưa kiểm chứng và yêu cầu gần nhất.
3. Đọc `docs/REQUIREMENTS_MATRIX.md` để phân biệt yêu cầu phải giữ với khoảng thiếu; `docs/CURRENT_ARCHITECTURE.md` để dùng đúng API/ID sau merge. Chỉ mục tài liệu ở `docs/README.md`.
4. Dùng `docs/SPEC_INDEX.md` tìm nguồn; đọc phần tương ứng trong `spec/` trước triển khai. Khi mâu thuẫn, yêu cầu người dùng mới nhất → đặc tả chương/§55 → §40–54 → §1–39 ở điểm chưa thay thế. Tài liệu trong `docs/history/` chỉ là lịch sử.

Đây là ghi nhớ lưu trên đĩa cho các phiên làm việc cùng repository. Không giả định lịch sử chat cũ luôn có trong ngữ cảnh.

## Nguồn thiết kế và phạm vi

- Yêu cầu hiện hành của người dùng là ưu tiên. Kế hoạch `spec/Ke_hoach_game_Son_Doong_12_tuan.md` có bản gốc v1.0 ngày 29/10/2026 và phần điều chỉnh 40–55 ngày 01/11; không chỉ đọc 39 phần cũ.
- `Tên_dự_án.md` là tầm nhìn toàn game; `Các_kỹ_thuật_làm_game.md` là danh mục kỹ thuật tham khảo. Phần 34 của kế hoạch xác định kỹ thuật nào thực sự áp dụng.
- Game là phiêu lưu hành động 3D góc nhìn thứ ba, An khám phá Sơn Đoòng, dùng Lồng Đèn Sen, giải đố và phục hồi linh mạch. Người dùng xác nhận lại ngày 04/11/2026: **5 chương**, tổng mục tiêu thiết kế 65–95 phút, Windows x64 chơi đơn offline; trước mắt **ưu tiên môi trường chương 1**, nhân vật/quái mới chờ yêu cầu sau. Có thiết kế C3–C5 không có nghĩa được tự triển khai ngay.
- Phong cách hiện hành **3D hoạt hình low poly**. Lịch 7 ngày/12 tuần là dự kiến cũ, chưa có hạn mới. Không nỏ/móc dây/đu dây/cưỡi chim/bơi hoặc tự thêm sinh tồn. Giữ ba thanh Master/Music/SFX và HUD C1 mở đầu Vườn Rêu, tiến độ 3 mảnh/2 câu đố; code sau merge chưa đủ không phải người dùng đã bỏ yêu cầu.
- Bảng asset là danh mục ứng viên, không phải bằng chứng đã tải, đã được kiểm thử hoặc đã triển khai gameplay.

## Làm việc và ghi nhớ

- Phản hồi tiếp 05/11: bỏ bàn chế C1, chế Bình máu trong Inventory bằng giao dịch nguyên tử; E lửa chỉ lưu. Dùng model thực tế A20 trống, A22 cổng FBX, A23 gương GLB (bản FBX tối ưu riêng), A24 bệ OBJ; số A23/A24 này khác danh mục gốc. Không hiện A/B/C trên trống, dùng Mặt Trời/Cánh Chim/Sóng Nước; giữ enum/ID nội bộ. Hai giấy gợi ý được nhặt và lưu vào Nhật ký, vẫn cho tự thử puzzle. Người dùng chốt Hắc Ảnh choáng 4s, Hộ vệ 2s khi Focus, có chống choáng lặp; ngừng tấn công và cúi đầu, dấu sao trên đầu. Thêm sương URP mỏng/particle, giữ thấy đường/tia/prompt; sửa chiều quái/mảnh trống và khe cửa, không thay model An.

- Yêu cầu tiếp theo 05/11 cho phép hoàn thiện quái Hắc Ảnh/Hộ vệ, đèn sen, cửa phong ấn, HUD/menu/UI và âm thanh C1 bằng asset đã nhập; **không thay model An**. Ưu tiên sửa lỗi Unity/meta và khe hở ảnh người chơi trước. Giới hạn cũ “quái mới chờ yêu cầu” được thay trong phạm vi này. Golem Kevin Iglesias và Ghost A08 đã có trên đĩa; kiểm tra animation/rig thực tế, không coi có FBX là đã hoàn thiện actor.

- Yêu cầu mới 05/11/2026 cho phép dựng lại/mở rộng địa hình toàn C1 và làm đường nối giữa năm khu bằng asset miễn phí đã nhập. Ràng buộc giữ nguyên footprint/collider bản khối của lượt trại cũ không còn áp cho công việc mở rộng này. Giữ thứ tự, root/ref/ID gameplay và save; spawn được căn lại theo checkpoint ID. World mới `World_Geometry/ENV_C1_ExpandedWorld`, cảnh cũ inactive. Xem `docs/CHAPTER01_EXPANDED.md` và PROJECT_STATUS cho mức kiểm chứng, không tự chạy công cụ dựng lại để đọc cảnh.

- Trao đổi bằng tiếng Việt; hướng dẫn Unity bằng tên object/component thực tế trong dự án.
- Phân biệt thiết kế, placeholder, cấu hình đã kiểm tra, Play Mode và kiểm thử build. Không đánh dấu hoàn thành chỉ vì có scene/prefab.
- Runtime hiện dùng PlayerMotor/InputReader, InventoryService, WorldStateStore theo StateSet/WorldIds, native save schema 4 và migration legacy version 1–5. Không áp API PlayerInventory/ChapterQuestHUD/save v5 prototype từ nhật ký cũ vào code hiện hành. ID runtime C1 còn relic_drum_*; thay ID phải đi cùng migration và ref, không đổi lẻ để khớp spec.
- Trại C1 hiện chuyển chế thuốc vào túi theo phản hồi 05/11; flow E bàn chế và báo cáo 53/569 trước revision là lịch sử. E lửa chỉ lưu. C2 hiện có altar mở panel chế riêng; không tự thay C2 khi sửa C1. Giữ ref/ID/spawn khi thay model; báo cáo trước merge giữ nhãn lịch sử.
- Đọc trạng thái hiện tại trước khi sửa: người dùng cũng thao tác trong Unity giữa các lượt chat. Giữ các thay đổi không thuộc công việc đang làm.
- Dự án hiện ghi Editor `6000.4.8f1`, khác Unity 6.3 LTS trong kế hoạch. Không tự đổi phiên bản để khớp tài liệu; ghi nhận khác biệt và xử lý khi có công việc liên quan.
- Giữ asset và `.meta` đi cùng nhau, giữ GUID khi di chuyển/đổi tên; ưu tiên Unity quản lý metadata. Không sửa asset nguồn nhập vào khi có thể cấu hình bằng prefab variant của dự án.
- Một bộ điều khiển vị trí cho mỗi actor; một hệ input nhất quán; Cinemachine 3, không trộn API 2.x. Chi tiết runtime state, giao dịch và save xem PROJECT_MEMORY.
- Sau công việc có thay đổi đáng kể, cập nhật PROJECT_STATUS với ngày, file liên quan, kiểm chứng thực tế và việc còn lại. Cập nhật PROJECT_MEMORY nếu có quyết định thiết kế mới được người dùng chấp thuận; không tự biến nhận xét thành yêu cầu mới.
- Yêu cầu ngày 30/10/2026 là đọc hiểu spec và lưu ghi nhớ, **không chỉnh code/game thêm trong lượt đó**. Backlog trong tài liệu không phải lệnh tự triển khai. Các lượt tiếp theo làm theo yêu cầu mới của người dùng.
