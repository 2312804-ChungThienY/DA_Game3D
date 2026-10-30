# Hướng dẫn tiếp nối dự án Sơn Đoòng

## Đọc trước khi làm việc

1. Đọc `docs/PROJECT_MEMORY.md` để hiểu game, phạm vi và các hợp đồng hệ thống.
2. Đọc `docs/PROJECT_STATUS.md` để biết trạng thái thực tế, việc chưa kiểm chứng và yêu cầu gần nhất.
3. Dùng `docs/SPEC_INDEX.md` để tìm mục nguồn; đọc lại phần tương ứng trong `spec/` trước khi triển khai một hệ thống. Nếu spec thay đổi, cập nhật ghi nhớ theo bản mới.

Đây là ghi nhớ lưu trên đĩa cho các phiên làm việc cùng repository. Không giả định lịch sử chat cũ luôn có trong ngữ cảnh.

## Nguồn thiết kế và phạm vi

- Yêu cầu hiện hành của người dùng là ưu tiên. Kế hoạch chi tiết nằm trong `spec/Ke_hoach_game_Son_Doong_12_tuan.md` (v1.0, 29/10/2026).
- `Tên_dự_án.md` là tầm nhìn toàn game; `Các_kỹ_thuật_làm_game.md` là danh mục kỹ thuật tham khảo. Phần 34 của kế hoạch xác định kỹ thuật nào thực sự áp dụng.
- Game là phiêu lưu hành động 3D góc nhìn thứ ba, An khám phá Sơn Đoòng, dùng Lồng Đèn Sen, giải đố và phục hồi linh mạch. Bản bắt buộc: chương Kim Quy, 5 khu liên kết, 25–35 phút, Windows x64 chơi đơn ngoại tuyến.
- Không tự đưa toàn bộ 4 chương, hệ sinh tồn, nỏ hoặc móc dây vào phạm vi bắt buộc. Mở rộng chỉ theo điều kiện trong spec và yêu cầu của người dùng.
- Bảng asset là danh mục ứng viên, không phải bằng chứng đã tải, đã được kiểm thử hoặc đã triển khai gameplay.

## Làm việc và ghi nhớ

- Trao đổi bằng tiếng Việt; hướng dẫn Unity bằng tên object/component thực tế trong dự án.
- Phân biệt thiết kế, placeholder, cấu hình đã kiểm tra, Play Mode và kiểm thử build. Không đánh dấu hoàn thành chỉ vì có scene/prefab.
- Đọc trạng thái hiện tại trước khi sửa: người dùng cũng thao tác trong Unity giữa các lượt chat. Giữ các thay đổi không thuộc công việc đang làm.
- Dự án hiện ghi Editor `6000.4.8f1`, khác Unity 6.3 LTS trong kế hoạch. Không tự đổi phiên bản để khớp tài liệu; ghi nhận khác biệt và xử lý khi có công việc liên quan.
- Giữ asset và `.meta` đi cùng nhau, giữ GUID khi di chuyển/đổi tên; ưu tiên Unity quản lý metadata. Không sửa asset nguồn nhập vào khi có thể cấu hình bằng prefab variant của dự án.
- Một bộ điều khiển vị trí cho mỗi actor; một hệ input nhất quán; Cinemachine 3, không trộn API 2.x. Chi tiết runtime state, giao dịch và save xem PROJECT_MEMORY.
- Sau công việc có thay đổi đáng kể, cập nhật PROJECT_STATUS với ngày, file liên quan, kiểm chứng thực tế và việc còn lại. Cập nhật PROJECT_MEMORY nếu có quyết định thiết kế mới được người dùng chấp thuận; không tự biến nhận xét thành yêu cầu mới.
- Yêu cầu ngày 30/10/2026 là đọc hiểu spec và lưu ghi nhớ, **không chỉnh code/game thêm trong lượt đó**. Backlog trong tài liệu không phải lệnh tự triển khai. Các lượt tiếp theo làm theo yêu cầu mới của người dùng.
