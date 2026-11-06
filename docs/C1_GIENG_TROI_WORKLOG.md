# Trại Giếng Trời — lượt dựng theo phác thảo 06/11/2026

Người dùng đã duyệt [phác thảo](C1_GIENG_TROI_SKETCH.md), giao thực hiện khu Trại Giếng Trời trước. Scene đích là `Assets/_Project/Scenes/02_HangLinhMach.unity`; chỉ thêm art dưới `ENV_01_TraiGiengTroi` và một cụm thạch nhũ ở `LINK_1_01_TraiGiengTroi_to_02_VuonReu`. Scene thử và các khu C1 còn lại không được dựng lại.

## Đã làm

- Giữ mặt nền, vách/trần/collider, `Gameplay`, `cp_trai`, spawn, bệ đèn, phong ấn và NavMesh hiện có. Sao lưu scene cùng `.meta` vào `SourceAssets/SceneBackups/2026-11-06_C1GiengTroi/` trước sửa.
- Tắt 51 phiến lát lặp trong trại; đặt 30 phiến bất quy tắc không collider và đổi màu dải đường nền. Đá, lối chính và vách cũ vẫn ở nguyên vị trí.
- Thêm 10 thạch nhũ ở trần kín hai vai giếng trời, 4 ở đoạn nối đầu. Mesh dẫn xuất có hai mức LOD, material URP Lit từ `Limestone_Moss` 1K đã nhập. Các thạch nhũ không có collider; đáy thấp nhất khoảng 4,96 m trên nền, miệng giếng vẫn mở.
- Thêm 6 dương xỉ rìa bằng mesh/material đã có; đổi sắc của 8 vách phụ để giảm lặp. Thêm tia nắng dùng mesh và shader `CaveLightShaft` sẵn có, material riêng ở `Assets/_Project/Art/Chapter01GiengTroi/`.
- Công cụ Editor một lần `Assets/_Project/Editor/Chapter01GiengTroiRevision.cs` dựng lại nhóm art nếu mở scene C1 sạch và chưa có nhóm mới. Không tự chạy khi import và từ chối dựng đè. Scene đích đã lưu; không sửa prefab nguồn hoặc model/rig An.

## Ảnh để duyệt

- [Từ điểm xuất phát, sau khi thêm tia nắng](images/c1-gieng-troi-art-sunrays-2026-11-06.png).
- [Nhìn lên miệng giếng và cụm thạch nhũ](images/c1-gieng-troi-art-roof-detail-2026-11-06.png).

Các ảnh là camera tham chiếu trong Editor, không phải ảnh Play Mode hoặc bằng chứng 60 FPS. Model An trong ảnh là prototype hiện có.

## Kiểm chứng đã chạy

- Unity 6000.4.8f1, Edit Mode: biên dịch công cụ mới xong, Console không có lỗi mã. `manage_scene(validate)` báo **0 missing scripts, 0 broken prefabs, 0 issues**.
- Kiểm qua Unity: 51 phiến cũ inactive, 30 phiến mới, 14 LODGroup thạch nhũ, **0 collider mới**; `cp_trai`, bệ nhận đèn, phong ấn và spawn còn nguyên. Năm điểm NavMesh từ spawn đến sát phong ấn đều lấy mẫu được; bốn đoạn liên tiếp trả `PathComplete`. NavMesh asset C1 không đổi.
- So scene theo file ID trước/sau: **0 object cũ bị xóa**, 317 object/component được thêm. Ngoài các thay đổi art chủ ý, Unity tự thêm `UniversalAdditionalLightData` cho ba `ART_Drum_ReadingLight` khi lưu; đây là dữ liệu URP mặc định tại Điện Trống, không đổi transform/gameplay của đèn. `git diff --check` cảnh báo tám dòng trống giá trị do Unity serialize có khoảng trắng cuối dòng.
- Console sau lưu: 0 error; còn cảnh báo MCP WebSocket không khởi tạo. Lúc chụp ảnh từng có cảnh báo Unity giảm độ phân giải shadow atlas cho 5 đèn punctual; cảnh báo không xuất hiện ở lần đọc cuối.

## Còn cần kiểm

Chưa chạy Play Mode sau lượt art này, chưa chơi tay Windows build, chưa đo FPS hoặc duyệt mỹ thuật cuối. Cần chơi từ Boot để xác nhận người mới thấy đèn/phong ấn, bấm E/RMB và xoay camera dưới thạch nhũ; nếu phát hiện ray/prompt bị che, điều chỉnh art trước khi đổi cơ chế.

Nguồn art: dương xỉ và mesh/UV thạch nhũ, phiến lát là dẫn xuất nội bộ; chất liệu đá dùng bản sao A14 Poly Haven CC0 đã có. Tia nắng dùng asset nội bộ sẵn có. Không nhập gói trả phí hoặc asset mới; giữ credit nguồn gốc trong `CHAPTER01_ASSET_CREDITS.txt`.
