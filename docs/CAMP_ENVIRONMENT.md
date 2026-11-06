# Trại Giếng Trời — khu mẫu môi trường, 04/11/2026

**Bản hiện hành 05/11:** [CHAPTER01_EXPANDED](CHAPTER01_EXPANDED.md) mở rộng địa hình toàn C1 và bốn đường nối. Ràng buộc giữ kích thước/collider bản khối và tọa độ trong tài liệu trại này thuộc mốc trước mở rộng.

**Cập nhật sau khu mẫu:** yêu cầu tiếp theo mở rộng art ra toàn C1; `92_EnvironmentPreview` nay chứa toàn chương. Những bố trí/trạng thái thử riêng trại bên dưới là mốc trước lượt này; xem [CHAPTER01_ENVIRONMENT](CHAPTER01_ENVIRONMENT.md) để biết sửa tiếp xúc, NavMesh và ảnh mới.

Phong cách được người dùng chọn: 3D hoạt hình low poly. Bố cục hư cấu lấy cảm hứng từ vách đá vôi cao, hố sụt đón nắng và cây mọc sâu trong hang. Ảnh người dùng gửi là mẫu bố cục/ánh sáng, không phải asset hoặc ảnh hiện trường cần sao chép từng chi tiết.

## Scene và bố cục

- Scene thử: `Assets/_Project/Scenes/92_EnvironmentPreview.unity`. Camera `Cameras/Camera_An_View_NoActor` xem từ sau điểm xuất phát; không có nhân vật hoặc quái mới.
- Nền trại giữ 20 × 20 m; nền hành lang giữ 4 × 10 m, bề rộng đi thực tế giữa vách blockout là 3 m. Nền/collider và điểm hồi sinh lấy từ scene hiện hành sau merge, không dựa vào ghi chú cũ.
- Spawn `(0,0.2,-5)`, checkpoint/lửa `(-4,0,0)`, bàn chế `(-3,0,3)`, cửa sang Vườn Rêu `(0,0,10)`. Khoảng cách bàn–checkpoint khoảng 3.16 m, hướng ngắm tách nhau.
- `Layout_Markers_EditorOnly` có Cube mốc checkpoint, bàn, cửa và spawn; mặc định tắt để xem hình. Bật nhóm trong Hierarchy khi chỉnh bố cục; nhóm mang tag EditorOnly.
- Vách hai bên cao khoảng 17–19 m, vách xa khoảng 13 m để đọc được khoảng hở trời; vòm thấp hơn sau camera. Lều, lửa và đồ tiếp tế ở trái; cây/bụi tập trung chân vách và miệng đường. Cây có capsule thân, bụi/cỏ không collider; đá/vách/lều có hộp va chạm đơn giản. Các collider mesh nhập gốc được tắt trên instance của dự án để tránh mắc chân.
- Đá dùng màu URP/Lit riêng, hai mặt; cây dùng texture A11 với alpha clip; đạo cụ A16 dùng bảng màu riêng. Giữ hướng xoay nhập FBX trước khi thêm yaw và chuẩn hóa bounds, không sửa model/material nguồn.

## Tài nguyên thực tế

Cả ba bộ đã có trên đĩa. A16 và A14 được người dùng nhập trong lúc tiếp nối; không tải trùng. Chỉ chọn FBX/model cần thiết, không đưa súng hoặc đạo cụ hiện đại ngoài bố cục vào trại.

| Bộ | Nguồn trong dự án | Mẫu dùng |
| --- | --- | --- |
| A09 — Loafbrr, Mines and Cave Set | `Assets/A09 - Mines and Cave Set/MInesAndCaveSet/prefabs/Wall` | Cave_Wall_Rocks A/B/C, Cave_Rocks A/B/C |
| A11 — Quaternius, Ultimate Stylized Nature Pack | `Assets/A11 - Ultimate Stylized Nature Pack/FBX` | NormalTree 1/2, Bush_Small, Grass_Large/Small, Rock 1/2/3 |
| A16 — Quaternius, Survival Pack | `Assets/A16 - Survival Pack/FBX` | Tent, Bonfire_Fire, WoodLog, Pot_Small, WaterBottle_1 |

A09 có `MInesAndCaveSet/README.txt` ghi **Creative Commons 0/CC0**, khác mô tả Asset Store EULA trong danh mục gốc; [trang nguồn được danh mục dẫn](https://assetstore.unity.com/packages/3d/environments/mines-and-cave-set-305701). A11 `License.txt` ghi CC0 nhưng tiêu đề lại là Ultimate Platformer Pack; [trang Quaternius](https://quaternius.com/packs/ultimatestylizednature.html). Ghi nhận đúng các tệp đang có và đối chiếu bản phân phối trước release, không tự kết luận mọi bản phân phối có cùng giấy phép. A16 có `Assets/A16 - Survival Pack/License.txt`, CC0 1.0, tác giả Quaternius; [trang nguồn](https://quaternius.com/packs/survival.html) đã đối chiếu ở lượt dựng trại ngày 04/11/2026. Lượt đồng bộ tài liệu chỉ đọc tệp nguồn, không xác minh lại web. Không dùng A10/A12/A14 trong nhóm trại này.

Tham khảo địa hình: [Oxalis — Sơn Đoòng](https://oxalisadventure.com/cave/son-doong-cave/), phần hố sụt và rừng trong hang. Đây là cảm hứng thiết kế, không dùng kích thước hang thật cho gameplay.

## Nhóm cảnh và công cụ

`Assets/_Project/Prefabs/Environment/Camp/PF_ART_Camp.prefab` gom vách, vòm, đá, cây, đạo cụ và đèn cục bộ. Các material riêng nằm trong `Assets/_Project/Materials/Camp`.

Khi ghép màn chính: đổi tên nhóm môi trường `Area_Camp` thành `ENV_01_Camp`, giữ object/ref cũ. Tắt hình khối cũ, giữ collider nền/vách. Các mesh thử đá/cây cũ được tắt thay vì xóa. Hai model tương tác được đặt dưới object gameplay: lửa dưới checkpoint `cp_trai`, bàn dưới `Gameplay/Camp_CraftingStation`. `ART_Camp` giữ các bản trang trí tương ứng ở trạng thái tắt. Cụm nhìn xa ở cuối hành lang chỉ bật trong preview; màn chính dùng Vườn Rêu sẵn có.

Bản sao scene và `.meta` trước sửa ở `SourceAssets/SceneBackups/2026-11-04_CampEnvironment`, ngoài Assets để không nhập trùng GUID. Có thêm bản ngay trước tích hợp trong thư mục con `BeforeIntegration_*`.

Công cụ Editor: `Assets/_Project/Editor/CampEnvironmentSetup.cs`. Menu `Tools → Son Doong → Camp Environment` mở preview, kiểm tra preview và chạy phép thử Play Mode. Công cụ chỉ xử lý yêu cầu tường minh; không tự dựng lại hoặc ghi scene khi import. Scene thử không nằm trong danh sách build.

## Tương tác và kiểm tra

Sau merge, checkpoint cũ đồng thời mở CraftingPanel. Theo yêu cầu tách hai đối tượng E, scene dùng `CampCraftingTable` để gọi giao dịch `InventoryService.TryCraft` hiện có. E tại bàn đổi 1 Lá cẩm + 1 Linh chi thành 1 Thuốc máu, hiện toast và nhắc lưu tại lửa; không tự ghi save. Checkpoint giữ nguyên ID `cp_trai`, SaveService và SpawnPoint, bỏ tham chiếu mở panel chế thuốc. Không thay code của save, inventory, nhân vật hoặc quái.

Ảnh `docs/images/camp-{spawn,crafting,exit,overview}-2026-11-04.png` là render kiểm tra Editor từ scene thử, không phải bằng chứng bấm phím hoặc build.

Đã tích hợp vào màn chính và chạy Play Mode từ Boot. Tuyến đi vòng mép bàn tới cửa đạt; ray ngắm chọn riêng bàn và checkpoint; chế thuốc, lưu, về menu rồi Continue giữ thuốc đã lưu và hồi sinh trên nền an toàn. Camera qua 24 góc tại spawn, sát vách đông và miệng cửa. Không có lỗi runtime; save chính/backup/tmp gốc được khôi phục byte-for-byte. Đây là lệnh API/controller tự động, chưa phải bấm E/WASD thật. Lượt đầu tuyến probe đi xuyên bàn bị chặn đúng bởi collider; đã đổi tuyến để đi vòng cạnh bàn và chạy lại đạt.

Báo cáo: [Editor](reports/CampEnvironment_Editor_2026-11-04.json), [Play Mode](reports/CampEnvironment_PlayMode_2026-11-04.json). Ảnh [camera gameplay tại spawn](images/camp-play-spawn-2026-11-04.png) dùng camera và actor prototype sẵn có; không thêm hoặc thay nhân vật.

Chưa bake ánh sáng hoặc đo FPS/build Windows cho khu cảnh này. Đèn và probe hiện là cấu hình thử; vị trí probe chưa phải bằng chứng đã có dữ liệu GI baked. Phong ấn và pickup khối hiện có vẫn giữ trong màn chính để bảo toàn gameplay; chúng chưa phải model cuối.

## Thử trực tiếp còn cần

Mở `00_Boot`, Chơi mới: đi từ spawn đến bàn rồi vào hành lang, xoay camera sát vách và sau lưng. Ngắm bàn rồi E, quay về lửa rồi E; xác nhận lời nhắc đúng đối tượng. Về menu và Continue, kiểm tra điểm hồi sinh. Xem thêm ở Low/Medium, đo hiệu năng trong Windows build trước khi gọi khu mẫu đã nghiệm thu hoàn toàn.
