# Thiết kế game — bản tóm tắt điều hướng

**Cập nhật 06/11/2026.** Quyết định chi tiết ở [PROJECT_MEMORY](PROJECT_MEMORY.md), yêu cầu và khoảng thiếu ở [REQUIREMENTS_MATRIX](REQUIREMENTS_MATRIX.md), nguồn chương ở [SPEC_INDEX](SPEC_INDEX.md). Code/scene là bằng chứng triển khai, không tự định nghĩa lại thiết kế.

## Đã được xác nhận

- Sơn Đoòng: Cõi Mộng Kỳ Thần là phiêu lưu hành động 3D góc nhìn thứ ba, chơi đơn offline trên Windows x64, tiếng Việt. An dùng Lồng Đèn Sen để khám phá hang, chiến đấu, giải đố, thu cổ vật và phục hồi linh mạch.
- Phong cách mỹ thuật hiện hành là **Mid Poly Stylized Việt Nam**; chuẩn texture, URP Lit và LOD ở [ART_BIBLE](ART_BIBLE.md). Quyết định này không tự thay luật chơi hoặc chứng nhận art C1/C2 đã được chuyển đổi.
- Mục tiêu toàn game: **5 chương**, 65–95 phút thiết kế, chưa có số đo playtest người mới hoặc hạn giao mới. C1 Kim Quy và C2 Chim Lạc có scene/runtime/báo cáo giới hạn; C3 Sơn Tinh, C4 Thủy Tinh, C5 Trục Linh Mạch mới ở đặc tả, chưa triển khai.
- C1 đi qua năm khu từ Trại Giếng Trời đến Đàn Kim Quy, gồm trống A–C–B, gương, ba mảnh/hai câu đố, Hắc Ảnh/Hộ vệ, đèn và checkpoint. C1 chế Bình máu trong túi; E lửa lưu. C2 có năm phòng, cột gió, chuông, bia/quiz, quái ném, Chim Lạc, trạm chế riêng và mạch chuyển chương; xem [kế hoạch C2](CHAPTER02_MECHANICS_PLAN.md).
- Một mức khó, một tuyến nhiệm vụ chính; không tự thêm sinh tồn, nỏ, móc dây, đu dây, cưỡi chim hoặc bơi. Nội dung tri thức phân biệt **Lịch sử / Truyền thuyết / Tự nhiên**, có nguồn; phần hư cấu không trình bày như sự thật.

## Luật khi mở rộng bản đồ, nhân vật và vật phẩm

- Bản đồ phải giữ đường đi/khóa tiến trình, tầm tương tác, camera, collider, NavMesh, mốc lưu và lối hồi sinh. Nhà sàn C2 hiện là art/bối cảnh; đừng suy từ mesh nhà rằng có hệ xây nhà hoặc NPC.
- An và quái đã có gameplay root/visual tách nhau. Thay hình không được tự đổi motor, input, ID, damage hoặc save. Model/rig An hiện không thuộc phạm vi cho phép thay.
- Vật phẩm có định nghĩa SO, ID bền, quy tắc túi và reward qua dịch vụ; nhặt/chế/thưởng phải kiểm đầy túi, chống nhận trùng và giữ trạng thái qua Continue. Không thêm item/cơ chế mới chỉ vì có model.
- Tìm asset/model/NPC trong project trước khi đề xuất mới. **Chưa tìm thấy hệ NPC gameplay** trong `Assets/_Project/Scripts`; hành vi, hội thoại, nhiệm vụ và vai trò của NPC là đề xuất cần người dùng giao/duyệt.

## Cần quyết định trước công việc tương ứng

- Phạm vi và vai trò NPC; có hội thoại/tương tác gì, xuất hiện ở chương nào.
- Khi nào bắt đầu C3–C5 và tiêu chí duyệt nội dung/hình ảnh cho từng chương. Backlog trong spec không tự là lệnh triển khai.

