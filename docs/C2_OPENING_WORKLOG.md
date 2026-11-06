# C2 lượt1 — nối Kim Quy và mở đầu Chim Lạc

Ngày05/11/2026, theo yêu cầu “thực hiện tiếp”, làm lượt1 trong [lộ trình](CHAPTER02_MECHANICS_PLAN.md). Phạm vi là đường vào C2, lời dẫn, vật hướng dẫn/năng lực và giữ dữ liệu qua chuyển chương/Continue. P2–P5, đồ họa toàn chương và C3 chưa thuộc lượt này; model/rig An giữ nguyên.

## Nội dung và cấu hình

- `RelicAltar` ở Main/Preview vẫn trao Phúc Kim Quy và lưu tại `cp_kim_quy` trước kết chương. Lời dẫn giữ gợi ý “vành của ta vang cuối cùng”, thêm luồng gió/chiếc lông dẫn tới Giếng Trời Chim Lạc. Không kể Chim Lạc đã được giải thoát trước khi chơi C2.
- `ChapterExit` C1 giữ root/ref/điều kiện `c1_relic_restored`, đích `03_GiengTroiChimLac/c2_cp_start`; lời nhắc gọi rõ tên Giếng Trời Chim Lạc. Nút trên màn kết C1 vẫn tới cùng đích. Bài thử phát hiện vị trí cũ(-7,0,307) bị `Continuous_Limestone_Cliffs_4` và `Cliff_Buttress_29/30` bọc kín sau lần mở rộng; đưa nguyên root ra sân(-6,0,300), kiểm khoảng đứng/ray/NavMesh từ checkpoint. [Bằng chứng trước sửa](reports/C2_Opening_ExitBeforeFix_2026-11-05.json), [lượt thử đầu dừng đúng ở cổng](reports/C2_Opening_FirstPortalFailure_2026-11-05.json); save cá nhân đã trả sau lượt dừng này.
- Scene03 thêm `World_Geometry/ART_C2_Opening`: giấy `Guide_C2_Wind` trên bệ A24 ở(-1.8,0,-3.4), vài dấu gió sát mặt đường và vùng sáng ấm quanh lông. Giấy tùy chọn E→Story, lưu `Flags/c2_guide_wind`, đọc lại J; không mở khóa nhiệm vụ hay trao năng lực.
- Pickup năng lực cũ giữ `AbilityPickup`, ID `ability_wind`, collider/ref; đặt tại(1.6,1.1,-2.4) để lông nằm cạnh dáng An trong camera sau lưng, thay vì bị An che ở giữa màn hình. Thêm con `VIS_WindFeather` với mesh lông low poly của dự án; ẩn toàn bộ renderer khối lông thử cũ, căn BoxCollider tương tác theo model mới. Thẻ `SC_C2_LongChimLac` nói rõ bước vào cột xanh bên phải→đợi hover→WASD+Space sang gờ; F đọc bia/bệ để nhận mảnh đầu, Phúc Kim Quy và E lửa lưu. Đây chưa phải phục dựng hay thưởng thắng Chim Lạc.
- `ChapterQuestTracker` chỉ chỉnh hai câu mở đầu; giữ mọi requirements/thứ tự bước sau. Spawn `c2_cp_start` căn theo spawn mặc định đầu C2 và nhìn+Z. Giữ checkpoint ID/ref; NavMesh bake lại vào **asset C2 cũ**, giữ GUID, không dùng baker viết cố định vào NavMesh C1.

Khung Story/Journal C2 được dùng lại mẫu đọc có cuộn của C1 qua `Chapter01UISetup.ReadingPanels`, tách khỏi skin HUD/inventory. Có viewport clip, thanh kéo và nút Tiếp tục nằm ngoài vùng chữ; chữ mục tiêu mở đầu chia hai dòng. Đây là sửa khả năng đọc nội dung mới, chưa phải toàn bộ UI C2 của lượt7. Ảnh lượt đầu phát hiện chữ Story cũ đè lên nút và lông bị An che; đã bổ sung kiểm hình học UI và vị trí lông trong camera thật vào probe.

Tài nguyên mới ở `Assets/_Project/Art/Chapter02Opening`: một mesh lông, ba material; bệ/giấy dùng lại asset/material sẵn có. Không cần tải thêm asset.

## Thay đổi runtime và phạm vi hồi quy

`GameSession.LoadChapter` chỉ nhận khi Playing/ChapterComplete, giữ pending snapshot **trước** khi gọi LoadScene để handler sceneLoaded luôn thấy dữ liệu. Vẫn dùng SaveService.Capture/ApplySnapshot/SaveAt, native schema4, không tạo writer khác.

`AbilityPickup` giữ object/listener sống, chỉ ẩn renderer/collider khi đã nhận. Lưu trạng thái enabled ban đầu của từng phần hình/collider, nên restore snapshot cũ hiện lại đúng model mà không bật sphere nguồn đã ẩn. Nhận chỉ qua actor sống khi Playing, chỉ hiện thẻ khi TryAdd thực sự thành công.

`PuzzleCluePickup` có `Chapter1ClueIds` cho năm giấy C1; `AllClueIds` thêm giấy C2 để Journal dùng chung xuyên chương. Validator/probe C1 dùng danh sách C1 thay vì đòi mọi giấy cả game phải nằm trong C1. Không đổi ID cũ, không đổi save schema; `c2_guide_wind` là Flag bổ sung.

Editor authoring: `Chapter02OpeningSetup`, request `Library/SonDoongC2Opening.request.json`, action `apply/validate/test`. Chỉ chạy khi scene đã lưu và ngoài Play Mode. Backup trước sửa tại đường dẫn trong `Logs/C2Opening-backup.json` dưới `SourceAssets/SceneBackups/2026-11-05_C2Opening`, gồm scene/meta, runtime/helper, SC và NavMesh C2.

## Kiểm chứng

`Chapter02OpeningProbe` chỉ chạy opt-in trong Editor. Nó bảo vệ primary/bak/tmp save trước khi tạo fixture, dùng SaveService thật tạo bản C1 trước phục dựng, thao tác bệ/cổng/nút thật qua API, nạp scene/menu/Continue thật. C1 boss/mảnh/puzzle được đặt bằng fixture; đây không phải đánh trùm/đi cả C1 hoặc test E bằng bàn phím.

Kịch bản: cổng khóa trước phục dựng → bệ C1/màn kết → menu/Continue C1→cổng trong cảnh→C2 → kiểm mốc/nền/khoảng trống, túi/đèn/Phúc/tri thức → đọc giấy/J → nhận lông/thẻ → restore trước nhặt → nhận lại/lưu ở checkpoint → menu/Continue C2 → chạy lại tuyến từ nút cuối C1.

**Kết quả cuối:** [Editor21checks](reports/C2_Opening_Editor_2026-11-05.json), [PlayMode74checks](reports/C2_Opening_PlayMode_2026-11-05.json),0failures/0runtime errors. Phép thử sau sửa UI kiểm cả viewport clip/scrollbar, khoảng cách vùng chữ với nút và vị trí lông cạnh nhân vật trong camera thật. Giữ2thuốc/3lá/2chi qua cả hai đường chuyển và Continue; giấy/năng lực mới được lưu đúng; restore snapshot cũ hiện lại lông. Primary/bak/tmp của người dùng được trả byte-for-byte sau mỗi probe.

[Bàn giao SHA/meta/save](reports/C2_Opening_Delivery_2026-11-05.json):10metadata hiện hữu giữ SHA, gồm ba scene/SC/NavMesh; bốn ảnh mới1920×1080. [Editor cuối](reports/C2_Opening_EditorState_2026-11-05.json): dừng Play, không biên dịch, scene sạch. Chưa cập nhật executable Windows trong lượt này.

Ảnh Game View1920×1080: [mở đầu](images/c2-opening-arrival-2026-11-05.png), [giấy hướng dẫn](images/c2-opening-guide-2026-11-05.png), [cuộn xuống cuối giấy](images/c2-opening-guide-bottom-2026-11-05.png), [thẻ nhận lông](images/c2-opening-feather-tutorial-2026-11-05.png). Ảnh vẫn thấy greybox vì art toàn C2 là lượt7.

Lượt này chưa nghiệm thu full tuyến P1–P5, cảm giác bàn phím/chuột tay, FPS, âm thanh, pha2/kết Chim Lạc hoặc chuyển thật C3. Bản Windows C1 trước đó không tự chứa các sửa C2 này; Windows mới được ghi riêng nếu có build trong lượt này.
