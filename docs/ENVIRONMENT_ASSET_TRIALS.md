# Nhập và thử môi trường theo đợt — 30/10/2026

Khu thử: `Assets/_Project/Scenes/93_TestEnvironment.unity`. Đây là scene kiểm tra riêng, chưa đưa vào danh sách build chính hoặc thay khối ở Trại Giếng Trời.

## Lựa chọn gói

| Mã nguồn | Trạng thái và vai trò |
| --- | --- |
| A09 — Mines and Cave Set | Đã có trong dự án; thử 3 prefab nền/đá/vách trong đợt 1 |
| A11 — Ultimate Stylized Nature Pack | Bộ thiên nhiên chủ đạo đang thử; 1 cây NormalTree, 1 bụi, 1 cỏ trong đợt 2 |
| A10 — Ultimate Modular Ruins Pack | Đã nhập và đã xử lý polygon ở lượt trước; dành cho Điện Trống/di tích, chưa đặt vào khu thử trại |
| A12 — Nature Kit | Đã có trên đĩa, giữ làm phương án thay A11; chưa trộn vào phong cách hiện tại |

Không nhập lại hoặc tải thêm vì cả bốn gói đã tồn tại. Các mã, tác giả, URL và giấy phép tham chiếu theo `spec/Danh_muc_asset_free_Son_Doong.md`: A09 từ Loafbrr/Unity Asset Store EULA; A10/A11 từ Quaternius/CC0; A12 từ Kenney/CC0. Giữ giấy phép nguồn trong từng gói.

Đường dẫn hiện tại khác các ghi chú cũ: A09 ở `Assets/A09 - Mines and Cave Set/MInesAndCaveSet/`; Starter Assets đang ở `Assets/A01 - First Person + Third Person  Character Controllers/`. Camera được tìm bằng GUID để không hỏng khi thư mục nguồn đổi tên.

## Mẫu đã cấu hình

Prefab thử nằm trong `_Project/Prefabs/Environment/A09_Trials` và `A11_Trials`. Chúng chứa prefab/model nguồn dưới dạng nested prefab, có cấu hình riêng; không sửa model hay material gốc.

| Mẫu | Tỷ lệ sau cấu hình | Collider |
| --- | --- | --- |
| A09 Ground_Mines_A | Rộng 6 m, sâu khoảng 5,97 m, dày khoảng 0,114 m | MeshCollider nguồn; mặt trên đặt cao hơn nền an toàn khoảng 0,08 m |
| A09 Cave_Rocks_A | Rộng 2,2 m, cao khoảng 3,24 m | MeshCollider nguồn |
| A09 Cave_Wall_Rocks_A | Cao 3,8 m, rộng khoảng 1,73 m | MeshCollider nguồn |
| A11 NormalTree_1 | Cao 4,8 m | Capsule thân cây, cao 2,2 m/radius 0,18 m; tán không cản |
| A11 Bush_Small | Cao 0,8 m | Không collider, cho phép đi xuyên |
| A11 Grass_Small | Cao 0,35 m | Không collider, cho phép đi xuyên |

Tất cả phần môi trường dùng layer Environment. Có nền thử 32 × 28 m, đường giữa rộng 3 m, vách an toàn, mốc capsule cao 1,8 m và nhãn A09/A11 để so tỷ lệ. An xuất hiện ở `(0, 0.2, -10)`. Camera đi theo PlayerCameraRoot, bật tránh vật cản Environment; GroundLayers nhận Environment.

## Vật liệu và phong cách

- Vật liệu riêng đặt trong `_Project/Materials/EnvironmentTrials`.
- A09: shader nguồn được Unity báo hỗ trợ, nhưng ảnh kiểm tra thực tế chưa hiển thị các mẫu như mong đợi. Đã tạo hai material URP/Lit riêng, lấy texture từ `_ColorTexture` nguồn, opaque và hai mặt; không đổi ORM Shader Graph hoặc material nguồn.
- Không gán ORM đóng gói của nguồn trực tiếp vào metallic/smoothness của URP vì cách đóng kênh khác nhau. Bản thử dùng smoothness thấp; chưa coi đây là vật liệu cuối.
- A11: bốn material bark/leaves/bush/grass dùng URP/Lit, gán texture đúng tên trong thư mục Textures. Lá/cỏ dùng alpha clip và hai mặt. Sắc xanh được giảm độ chói, giữ bảng xanh lá–nâu đất–xám đá.
- Giữ một loại cây chủ đạo, một loại bụi và một loại cỏ để đánh giá trước khi nhân rộng. A12 không tham gia bộ vật liệu hiện tại.

## Kiểm tra đã thực hiện

Unity Editor **6000.4.8f1** đã tạo và lưu scene/prefab/material bằng API của Editor. Công cụ ở `Assets/_Project/Editor/EnvironmentAssetTrials.cs`.

1. Đọc bounds/collider/material của 6 nguồn trước khi cấu hình.
2. Dùng đúng CharacterController của prefab An, tắt component nhận input trong lúc thử và dịch khu thử tạm ra xa các scene khác để tránh ảnh hưởng va chạm.
3. Cho controller đi hết một vòng quanh bounds của từng mẫu với khoảng hở tính theo bán kính nhân vật: **6/6 đạt**.
4. Kiểm tra chặn trực tiếp bởi đá/vách/thân cây, raycast mặt nền, đi xuyên bụi/cỏ theo chính sách: **6/6 đạt**.
5. Kiểm tra vật liệu được gán; cả 6 mẫu dùng URP/Lit sau tinh chỉnh. Chụp và xem lại ảnh tổng quan, thấy đủ mẫu A09/A11 và không có vật liệu hồng.

Báo cáo giữ trong [EnvironmentAssetTrials_2026-10-30.json](reports/EnvironmentAssetTrials_2026-10-30.json); ảnh tại [environment-trials-2026-10-30.png](images/environment-trials-2026-10-30.png). Kiểm tra này chạy trong Editor, **chưa phải Play Mode với thao tác chuột/phím của người chơi**, chưa đo hiệu năng hoặc build.

## Thử bằng tay trước khi đưa vào màn chính

Mở `93_TestEnvironment`, nhấn Play. Đi bằng WASD, xoay chuột, chạy Shift, nhảy Space:

- Đi vòng và áp sát đá/vách; không xuyên mặt hoặc bị mắc ở chân.
- Đi qua nền A09, kiểm tra chân, nhảy và nhận mặt đất.
- Đi quanh thân cây, đưa camera sát tán và vách; thân chặn, tán không gây khối va chạm vô hình.
- Đi xuyên bụi/cỏ; kiểm tra alpha lá từ cả hai phía, bóng, màu và tỷ lệ so với An.
- Quan sát ở chất lượng đang dùng, đặc biệt camera tránh vật cản và độ dễ đọc đường đi.

Chỉ sau khi lượt Play Mode này ổn mới chọn vài mẫu đưa vào `02_HangLinhMach`; giữ collider/đường đi của blockout đã kiểm tra. Chưa mở rộng Điện Trống hoặc thay hàng loạt môi trường trong công việc này.
