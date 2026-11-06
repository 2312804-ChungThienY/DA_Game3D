# Thử asset môi trường — đồng bộ 04/11/2026

Khu mẫu đang dùng là **92_EnvironmentPreview**, đã ghép ART_Camp vào 02_HangLinhMach theo yêu cầu 04/11. Chi tiết và cách thử hiện hành ở [CAMP_ENVIRONMENT.md](CAMP_ENVIRONMENT.md). Scene **93_TestEnvironment** và dữ liệu 30/10 dưới đây là khu thử sáu mẫu trước merge, không phải bản khu trại mới hoặc bằng chứng mọi asset đã nghiệm thu.

## Lựa chọn và nguồn hiện hành

| Bộ | Trạng thái và vai trò |
| --- | --- |
| A09 Mines and Cave Set | Đã có; ghép vách/vòm/đá/cửa trại. Source tại Assets/A09 - Mines and Cave Set/MInesAndCaveSet |
| A11 Ultimate Stylized Nature Pack | Đã có; bộ cây/bụi/cỏ chủ đạo của khu mẫu low poly hiện tại |
| A16 Survival Pack | Đã có sau lượt nhập của người dùng 04/11; dùng lều/lửa/ghế gỗ/nồi/bình trong trại |
| A10 Ultimate Modular Ruins Pack | Đã có và sửa import 30/10; ứng viên di tích/phong ấn sau bố cục, chưa dùng trong ART_Camp hiện tại |
| A12 Nature Kit | Đã có; phương án thay A11, không trộn vào ART_Camp |
| A14 Mossy Rock | Đã có sau lượt nhập 04/11; chưa dùng trong ART_Camp, chưa là bằng chứng đã thử vật liệu/collider |

Không tải lại bộ đã có chỉ vì danh mục spec còn ghi Chưa tải. Cột ứng viên trong spec không tự là trạng thái import/nghiệm thu. Giữ source và .meta; chỉnh bằng material/prefab riêng của dự án.

Nguồn giấy phép cần ghi đúng sự khác nhau:

- A09: README.txt trong bộ nhập ghi trực tiếp **Creative Commons 0/CC0** tại dòng 1. Danh mục spec và ghi chú 30/10 từng ghi Asset Store EULA theo kênh tải. Đây là khác biệt nguồn cần đối chiếu đúng release/kênh phân phối; không tự kết luận pháp lý rằng hai ghi chú tương đương.
- A11: License.txt ghi CC0 1.0 nhưng tiêu đề là **Ultimate Platformer Pack**, không khớp tên bộ thiên nhiên. Giữ file gốc và ghi nhận provenance cần đối chiếu release; không sửa tên giấy phép để che khác biệt.
- A16: License.txt ghi Quaternius, CC0 1.0. A10/A12 và các nguồn còn lại tra file giấy phép trong từng bộ cùng danh mục nguồn; không suy giấy phép từ tên gói.

## Scene, prefab và kiểm tra hiện hành

92_EnvironmentPreview giữ nền/collider blockout và camera tham chiếu sau spawn, không tạo nhân vật/quái mới. Các Cube mốc ở Layout_Markers_EditorOnly mặc định tắt. Main dùng World_Geometry/ENV_01_Camp/ART_Camp, giữ cp_trai, pickup và đường đi gameplay. Prefab mới ở Prefabs/Environment/Camp, material riêng ở Materials/Camp; không dùng chính sách MeshCollider của thử nghiệm cũ cho tất cả đá mới.

Khu trại hiện dùng hộp collider đơn giản cho đá/vách/lều, capsule thân cây; bụi/cỏ không collider. Các collider source mesh được tắt bằng override trên mẫu dự án. Tách model lửa/bàn xuống dưới object gameplay; bước này có CampCraftingTable riêng và checkpoint cũ. Bảng màu đã được người dùng chọn là đá xám lạnh/cây xanh rêu/nắng vàng ấm theo phong cách 3D hoạt hình low poly.

[Báo cáo Editor 04/11](reports/CampEnvironment_Editor_2026-11-04.json) ghi 115 mẫu kiểm tra khoảng trống tuyến đi; [báo cáo Play Mode](reports/CampEnvironment_PlayMode_2026-11-04.json) ghi 53 ca tổng hợp, có di chuyển bằng controller, ray chọn tương tác, camera và save/Continue. Đây là API/controller tự động, chưa phải chuột/phím thực tế, bake GI, đo FPS hoặc Windows build. Lượt đồng bộ tài liệu này không chạy lại Unity.

Công cụ còn phù hợp cho khu mẫu là [CampEnvironmentSetup.cs](../Assets/_Project/Editor/CampEnvironmentSetup.cs):83–88, menu Tools → Son Doong → Camp Environment. Không dựng lại scene chỉ để đọc trạng thái; đọc scene hiện hành và giữ thay đổi người dùng trước.

## Khu thử 93 — cấu hình và bằng chứng lịch sử 30/10

93_TestEnvironment.unity còn trên đĩa, không nằm trong danh sách build bật hiện tại. Prefab mẫu dưới Prefabs/Environment/A09_Trials và A11_Trials, material ở Materials/EnvironmentTrials. Sáu mẫu khi tạo ngày 30/10:

| Mẫu | Kích thước thử khi tạo | Collider khi tạo |
| --- | --- | --- |
| Ground_Mines_A | Rộng 6 m, sâu khoảng 5.97 m, dày khoảng 0.114 m | MeshCollider source, mặt trên cao hơn nền an toàn khoảng 0.08 m |
| Cave_Rocks_A | Rộng 2.2 m, cao khoảng 3.24 m | MeshCollider source |
| Cave_Wall_Rocks_A | Cao 3.8 m, rộng khoảng 1.73 m | MeshCollider source |
| NormalTree_1 | Cao 4.8 m | Capsule thân cao 2.2 m/radius 0.18 m, tán không chặn |
| Bush_Small | Cao 0.8 m | Không collider |
| Grass_Small | Cao 0.35 m | Không collider |

Lúc tạo có nền 32×28 m, đường giữa 3 m, vách bao, mốc 1.8 m; An xuất hiện (0,0.2,-10), camera theo PlayerCameraRoot/tránh Environment. A09 dùng URP/Lit riêng lấy texture _ColorTexture; A11 lấy texture đúng tên, lá/cỏ alpha clip hai mặt. Không nối ORM tùy tiện vào metallic/smoothness của URP.

Unity 6000.4.8f1 khi đó đã đọc bounds/material/collider, kiểm controller đi vòng **6/6** và chính sách va chạm **6/6**, xem ảnh không hồng. Bằng chứng: [báo cáo 30/10](reports/EnvironmentAssetTrials_2026-10-30.json), [ảnh 30/10](images/environment-trials-2026-10-30.png). Đây là phép thử Editor, không phải Play Mode bàn phím hoặc build.

Sau merge, PF_An_Prototype đã đổi sang PlayerMotor/InputReader chung. [EnvironmentAssetTrials.cs](../Assets/_Project/Editor/EnvironmentAssetTrials.cs):236–243 vẫn có nhánh tìm PlayerInput và tên GroundLayers cũ; **chưa xác nhận scene 93 chơi được nguyên trạng trên runtime mới**. Công cụ vẫn tồn tại với action inspect/refine/build; Build tại :100–101 từ chối nếu scene đã có. Không tự xóa scene để chạy lại builder, hoặc lấy các số 6/6 cũ làm chứng cứ tương thích sau merge.

## Kiểm tra trực tiếp còn cần

Ưu tiên mở 00_Boot → Chơi mới để thử bản khu trại đang dùng: đi vòng mép bàn/đá, áp sát vách, xoay camera sau spawn và miệng cửa; chọn E riêng cho bàn và checkpoint, về menu/Continue. Xem Low/Medium, alpha lá từ hai phía và đo build khi có cấu hình tham chiếu. Việc đưa ART_Camp vào main ngày 04/11 là trạng thái đã làm theo yêu cầu mới; không còn coi câu lịch sử “chưa đưa mẫu nào vào main” là lệnh hoặc trạng thái hiện hành.
