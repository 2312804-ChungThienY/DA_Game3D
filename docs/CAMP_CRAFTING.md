# Chế thuốc tại Trại Giếng Trời

Thực hiện ngày 02/11/2026 theo `Son_Doong_Camp_Crafting.zip` và yêu cầu hiện hành. Công thức **1 lá cẩm + 1 linh chi → 1 thuốc máu**, dùng E trực tiếp tại bàn. Mốc trước sửa `a6fb50e`; mã/scene/ZIP gốc sao lưu trong `Temp/CampCraftingBackup_20261102`.

## Cấu hình

- `02_HangLinhMach/Gameplay/Camp_CraftingStation`: vị trí `(-5.8,0,0)`, cách checkpoint `Campfire_Placeholder` 1.8 m. Root độc lập, không có SaveCheckpoint; rotation 0, scale 1.
- Cube con `TableVisual`: local position `(0,0.35,0)`, scale `(1.2,0.7,0.8)`, BoxCollider thường. Cả root/con ở layer Interactable. Vật liệu khối thử màu nâu riêng, không thay asset nguồn.
- `CampCraftingStation`: Inventory của An, World State của GameSession, Interaction Distance=3, Craft Interval=0.35. Dùng scanner/E hiện có, không thêm input action.
- `Canvas_Gameplay/TXT_CraftFeedback`: giữa dưới, tọa độ `(0,200)` so với đáy, 850×70, font24, wrap, không Raycast Target; đặt trên lời nhắc E và dưới các panel modal. Dùng font static Noto Sans tiếng Việt đã tạo ở bước inventory.

## Giao dịch và lưu

Chỉ ghép `TryCraftHealthPotion()` vào PlayerInventory cũ, giữ `.meta` và các API khác. Hai script mới là HealthPotionCrafting (static, không gắn object) và CampCraftingStation. Bản nháp trừ một nguyên liệu từ chồng nhỏ nhất mỗi loại rồi thử thêm một thuốc; chỉ commit cả túi khi thành công, phát Changed một lần. Thất bại giữ nguyên toàn bộ túi. HUD inventory tự theo Changed.

Chặn tương tác khi ngoài 3m, pause, chết, đánh, né, chương đã hoàn thành, sai An/túi hoặc trong khoảng 0.35 giây. Thông báo thành công/thiếu nguyên liệu/hết chỗ bằng tiếng Việt, tự xóa sau 4 giây unscaled time. Chế không hồi máu và không ảnh hưởng cooldown uống Q.

**Lưu thủ công theo yêu cầu lần này:** sau chế phải tới checkpoint trại và E để ghi. Chế thành công chỉ đổi runtime inventory; không tự gọi SaveAtCamp, không hồi đầy chỉ số. Chế chưa lưu rồi tải sẽ rollback về snapshot trước. SaveData/SaveService giữ nguyên v5; không thay ID pickup, âm lượng hay tiến trình cốt truyện.

## Kiểm chứng

- Chạy đúng phương thức menu `Son Doong → Tests → Check potion crafting`: **7 contract đạt**, nguồn không đổi khi thành công/thất bại, draft không chia sẻ slot với nguồn. [Báo cáo](reports/20261102_CraftingContracts.json).
- Play Mode từ Boot đạt **57 kiểm tra**, không lỗi runtime: giao dịch tại bàn, Changed một lần, không hồi máu/không tự ghi checkpoint, thiếu nguyên liệu/hết chỗ, giải phóng ô/chồng thuốc chưa đầy, interval, pause, xa bàn, đánh và né thật (gọi lệnh nội bộ để kiểm tra trạng thái), chết và hoàn thành chương. Ba nhánh craft chưa lưu→rollback, craft+lưu→Continue, dùng+lưu→reload đều đạt. [Báo cáo](reports/20261102_CraftingPlayMode.json).
- Đã xem [ảnh HUD Play Mode](images/CraftingHUD_PlayMode.png), xác nhận chữ Việt và bố cục không đè lời nhắc E/túi. E/Q được kiểm tra qua đường lệnh tương tác/sử dụng, chưa xác nhận bằng bấm phím tay có focus; cảm giác ngắm/đi lại và độ phân giải khác còn cần kiểm tra trực tiếp.
- Bản Windows Development đạt cùng **57 kiểm tra**, không lỗi runtime ([báo cáo](reports/20261102_CraftingWindows.json)). Build ở `Builds/CampCraftingValidation/SonDoong.exe`, không đưa binary vào Git. [Build cuối](reports/20261102_CraftingBuild.json) Succeeded, 0 lỗi và 0 cảnh báo mới trong lần build tăng dần; không coi đây là xác nhận mọi cảnh báo shader cũ đã được sửa.
- Save chính/backup/tmp gốc được sao lưu và khôi phục byte-for-byte sau probe. SaveData, SaveService, GameSaveStorage và PotionUseController giữ nguyên nội dung.

## Công cụ tiếp nối

`CampCraftingSetup.cs` đọc request `Library/SonDoongCraftingSetup.request.json`, action `install`, `validate`, `contracts`, `play-test`, `build`, `open-main`; xuất `Logs/CampCraftingSetup.json`. Không gọi install chỉ để kiểm tra. Menu contract checks vẫn có sẵn cho thao tác trực tiếp trong Editor.

`CampCraftingSmokeProbe` chỉ chạy khi được bật từ công cụ Editor hoặc Development build có `-crafting-self-test`; hỗ trợ `-crafting-report <path>` và `-crafting-backup <thư mục mới>`. Probe sao lưu ba file chính/backup/tmp và khôi phục byte-for-byte khi kết thúc hoặc assertion; nếu ứng dụng bị buộc tắt, khôi phục theo manifest trong backup trước khi chơi tiếp. Không chạy probe cùng lúc với các probe inventory/audio dùng chung file save.
