# Túi đồ, pickup và thuốc hồi máu

Bước tiếp nối đã có [bàn chế thuốc tại trại](CAMP_CRAFTING.md); các mô tả chưa chế thuốc dưới đây ghi lại phạm vi bước inventory.

Triển khai theo `Son_Doong_Inventory_Potions.zip` và yêu cầu ngày 02/11/2026 trong scene `02_HangLinhMach`. Mốc trước sửa: `7af0cf2`. Sao lưu mã/scene/input/gói gốc tại `Temp/InventoryPotionsBackup_20261102`; Git giữ snapshot toàn bộ phần dự án được theo dõi.

## Cấu hình hiện tại

- An (`PF_An_Prototype` trong scene) có `PlayerInventory`, `PotionUseController` với cooldown 2 giây. `GameSession/SaveService.inventory` trỏ đúng túi của An.
- `Assets/_Project/Input/PlayerControls.inputactions` sao chép các action hiện có, giữ ID, thêm `Player/UsePotion` kiểu Button với `<Keyboard>/q`, không Interactions. An dùng bản này; không sửa input nguồn Starter Assets.
- Ba asset trong `Assets/_Project/Data/Items`: `IT_LaCam` (`item_la_cam`, Lá cẩm, chồng 10), `IT_LinhChi` (`item_linh_chi`, Linh chi, chồng 10), `IT_ThuocMau` (`item_thuoc_mau`, Thuốc máu, chồng 5). MaxStack lấy từ InventoryRules thống nhất với bộ kiểm tra save, không phải trường tùy chỉnh riêng trong Inspector. Icon để trống.
- `Gameplay/InventoryPickups` chứa ba bệ và `Pickup_LaCam_01`, `Pickup_LinhChi_01`, `Pickup_ThuocMau_01` gần trại: X=-2.5/-1.4/-0.3, Y=1.05, Z=-1.5. Mỗi pickup một đơn vị, collider thường, layer Interactable, tham chiếu cùng túi và WorldState của GameSession. ID lần lượt `pickup_la_cam_001`, `pickup_linh_chi_001`, `pickup_thuoc_mau_001`; giữ ID ổn định khi đã có save.
- `Canvas_Gameplay`: `TXT_Inventory` dưới trái, `TXT_PotionCount` phía trên danh sách, `InventoryUI/InventoryHUD` tham chiếu ba definition. Font static `Inventory_Vietnamese.asset` từ Noto Sans đã có, không sửa font nguồn. HUD nằm dưới các panel modal.
- Nhắm pickup và E để nhặt qua InteractionScanner. Q tìm thuốc theo ID, hồi 35 tới tối đa 100; đầy máu, chết, input bị khóa hoặc pause không tiêu thuốc. Hồi dùng theo simulation time nên pause không làm thời gian chờ trôi.
- Menu component PlayerInventory có `Test/Fill 12 Slots - Play Mode Only`. Dừng Play sau bài thử, không ghi checkpoint bằng đồ debug. Chưa có phím I, chọn ô hoặc chế thuốc trong bước này.

## Ghép mã và lưu/tải

Giữ `.meta` của năm script cũ. Chỉ thêm TryHeal vào PlayerVitals, giữ IDodgeVitals và bất tử né; giữ lọc ID/null ở WorldStateStore; SaveService giữ fallback tìm CharacterController/PlayerInput, LanternPickup.ApplySavedState và LightPuzzleReceiver.ApplySolvedState. Không thay menu/pause/audio/combat.

SaveData v5 bổ sung đủ 12 slot và collectedPickups vào cùng snapshot. File chính vẫn `save_v1.json`, backup `.bak`. GameSaveStorage giữ ghi file tạm, flush, đọc kiểm tra rồi replace/backup; không ghi đè backup hợp lệ bằng file chính hỏng. v1–v4 chuyển sang v5 với túi/pickup rỗng và giữ tiến trình cũ, kể cả v1/v2 thiếu checkpointId. File v5 thiếu trường túi/pickup bị từ chối, không tự xóa túi.

SaveService đăng ký pickup đặt sẵn cùng scene một lần, kể cả inactive; từ chối cấu hình sai/ID trùng hoặc ID trong save không còn trong scene. Khi tải, thay toàn bộ inventory trước khi phát world event, áp lại mọi pickup rồi thông báo HUD. Dùng thuốc giữ nguyên ID pickup đã thu. Nhặt/dùng nhưng chưa lưu sẽ rollback theo snapshot; đó là luật checkpoint.

## Kiểm chứng và tiếp nối

Play Mode từ Boot đạt **76 kiểm tra**, không lỗi runtime ([báo cáo](reports/20261102_InventoryPlayMode.json)). Bao gồm migration v1–v4, file v5 thiếu trường/sai slot/stack/ID, chống nhân đồ, cooldown khi pause, death/respawn, Continue, hoàn thành chương và reset Chơi mới. Save gốc chính/backup/tmp được khôi phục byte-for-byte. Đã xem [ảnh HUD trong Play Mode](images/InventoryHUD_PlayMode.png).

Bản Windows Development cũng đạt **76 kiểm tra**, không lỗi runtime ([báo cáo](reports/20261102_InventoryWindows.json)); save gốc được khôi phục. Build trong `Builds/InventoryPotionsValidation/SonDoong.exe` (không commit binary). [Báo cáo build cuối](reports/20261102_InventoryBuild.json): Succeeded, 0 lỗi, 0 cảnh báo mới trên lần build tăng dần. Lần build đầu có cảnh báo compute shader ConvGeneric của gói AI Inference đã có từ trước; kết quả tăng dần không khẳng định chúng đã được sửa. Đã chuyển lời gọi FindObjectsByType của gói sang overload hiện hành cho Unity 6000.4 để bỏ hai cảnh báo API cũ.

Lệnh nhặt và dùng thuốc được gọi qua API mà E/Q sử dụng; chưa mô phỏng phím thật có focus. Cần thử tay E/Q, ngắm pickup, giữ Q, cảm giác đi lại/collider và bố cục ở các độ phân giải khác.

`InventoryPotionsSetup.cs` nhận request ở `Library/SonDoongInventorySetup.request.json`, các action `inspect`, `install`, `validate`, `play-test`, `build`, `open-main`; output `Logs/InventoryPotionsSetup.json`. `install` tạo/cập nhật các object riêng của bước này, không chạy chỉ để kiểm tra.

`InventoryPotionsSmokeProbe` chỉ chạy khi Editor SessionState được công cụ bật hoặc Development build có `-inventory-self-test`. `-inventory-report <path>` và `-inventory-backup <thư mục mới>` chỉ định báo cáo/sao lưu. Probe sao lưu chính/backup/tmp và khôi phục byte-for-byte khi xong hoặc gặp assertion. Không chạy hai probe đồng thời trên cùng thư mục lưu. Nếu ứng dụng bị buộc đóng giữa thử nghiệm, khôi phục theo manifest trong thư mục backup trước khi chơi tiếp.
