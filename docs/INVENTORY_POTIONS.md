# Túi đồ, pickup và thuốc hồi máu — cập nhật 05/11/2026

**Mới nhất C1:** túi I có nút Chế bình máu, icon render từ model đã nhập, công thức 1 Lá cẩm + 1 Linh chi → 1 Bình máu. `InventoryPanel.TryCraftPotion` gọi giao dịch `InventoryService.TryCraft`, chỉ khi UI mở/InMenu/An sống, debounce 0,25 giây theo unscaled time. `CanCraft` thử cả sức chứa sau khi trừ nguyên liệu để bật nút đúng. Không autosave khi chế. Bàn chế đã gỡ khỏi Main/Preview; E lửa vẫn lưu. Xem [CAMP_CRAFTING](CAMP_CRAFTING.md) và [revision](C1_REVISION_WORKLOG.md); các phần kiểm kê 04/11 bên dưới là nền lịch sử khi khác biệt.

Yêu cầu giữ: túi 12 ô, nguyên liệu chồng 10/thuốc chồng 5, pickup qua E không nhân đôi, Q dùng một thuốc hồi 35 máu/chờ 2 giây, rollback theo checkpoint. Sau merge dùng InventoryService, ItemPickup, PotionUser; PlayerInventory, WorldItemPickup, PotionUseController, InventoryRules trong báo cáo 02/11 là kiến trúc cũ.

## Cấu hình hiện hành

| Thành phần | Nguồn/cấu hình hiện có |
| --- | --- |
| Inventory | GameSession/InventoryService trong C1 và C2, không nằm trên An. SlotCount=12; TryAdd trả lượng nhận; TryRemove/TryCraft trả thành công/thất bại, phát Changed khi commit |
| Definition | Assets/_Project/Data/Items/ID_*.asset: 9 định nghĩa hiện dùng; ItemDefinition.kind xác định chiếm ô, maxStack trong từng asset |
| Nguyên liệu/thuốc | ID_item_la_cam và ID_item_linh_chi: maxStack 10; ID_item_thuoc_mau: maxStack 5; Equipment/Fragment/Relic không chiếm slot qua UsesSlot |
| Input | InputReader đọc InputSystem.actions, map Gameplay/UI; UsePotion/Inventory thay map Player trong tài liệu cũ |
| Q | PotionUser trên actor, inventory trỏ GameSession, healAmount=35/cooldown=2; dùng simulation time |
| UI | Canvas_Gameplay/UI_Inventory/InventoryPanel: mở/đóng I, 12 nút chọn ô xem chi tiết, nhóm trang bị/mảnh, pause qua PushUiPause; Q dùng ngoài menu. HudBars hiện số thuốc |
| Pickup C1 | 7 ItemPickup: pickup_la_cam_01/02/03, pickup_linh_chi_01/02/03, pickup_thuoc_mau_01; không còn bộ hậu tố _001 cũ trong scene |

Điểm đọc nguồn: [InventoryService.cs](../Assets/_Project/Scripts/Inventory/InventoryService.cs):8–19, :62–105, :119–136; [ItemDefinition.cs](../Assets/_Project/Scripts/Inventory/ItemDefinition.cs):7–15; [PotionUser.cs](../Assets/_Project/Scripts/Player/PotionUser.cs):37–54; [InventoryPanel.cs](../Assets/_Project/Scripts/UI/InventoryPanel.cs):67–93; [InputReader.cs](../Assets/_Project/Scripts/Core/InputReader.cs):36–66. C1 đặt InventoryService tại [02_HangLinhMach.unity](../Assets/_Project/Scenes/02_HangLinhMach.unity):31224, UI inventory tại :11904.

Ba asset cũ IT_LaCam.asset, IT_LinhChi.asset, IT_ThuocMau.asset còn trong Data/Items, nhưng trỏ GUID script cũ dd4ebb065764821459b1bcabd28a6903, không phải ItemDefinition hiện hành. Chúng không nằm trong catalog 9 asset ID_* của GameSession. Đây là tài nguyên sót cần xử lý trong lượt sửa được giao; chưa xóa hoặc đổi GUID trong lượt tài liệu.

## Luật runtime đọc từ code

- ItemPickup kiểm inventory.CanAdd, chỉ nhận một đơn vị rồi thêm ID vào StateSet.Pickups; Changed khiến vật đã nhặt tắt. ID phải thuộc WorldIds và item phải có trong catalog.
- PotionUser.Update chỉ đọc Q khi gameplay bật và không pause. TryUsePotion chặn component tắt, chết, pause, hết thuốc, đầy máu hoặc cooldown; trừ một thuốc rồi Heal, không save. API công khai không tự kiểm mọi điều kiện input mà Update kiểm.
- TryCraft thử trên bản sao, chỉ commit khi cả giao dịch hợp lệ. Cách trừ hiện tại từ slot cuối về đầu, không phải “chồng nhỏ nhất” của implementation cũ.
- Phím I/chọn ô và chế trong túi có trong C1 hiện hành; xem [CAMP_CRAFTING.md](CAMP_CRAFTING.md).

## Save và khoảng trống bảo toàn dữ liệu

[SaveData.cs](../Assets/_Project/Scripts/Save/SaveData.cs):8–17 ghi **schemaVersion 4**, List<InventorySlotDto> có chỉ số slot và WorldSnapshot. [SaveService.cs](../Assets/_Project/Scripts/Save/SaveService.cs):43–45 giữ save_v1.json/.bak/.tmp dưới persistentDataPath; WriteFile tại :216–245 flush, đọc kiểm tra rồi replace/backup. ApplySnapshot tại :175–195 áp world/inventory, đặt actor về checkpoint và hồi tài nguyên.

Nhánh đọc legacy tại SaveService:272–294 nhận version 1–5 cũ khi schemaVersion==0, chuyển trong bộ nhớ và chỉ ghi định dạng mới khi lưu thành công. Không gọi đây là ghi v5 hoặc “migration v1–v4 sang v5” như triển khai cũ.

**Gap cần sửa hoặc chốt:** SaveService.IsValid tại :403–410 chỉ kiểm slot/quantity dương/itemId không rỗng, chưa đối catalog và maxStack của inventory native schema4. InventoryService.Restore tại :127–134 bỏ ô sai item/UsesSlot/maxStack kèm warning. Vì vậy yêu cầu cũ “save sai item/stack bị từ chối, không tự mất đồ” có nhánh chưa đáp ứng; không khẳng định file luôn bị từ chối trước apply. Đây là phát hiện từ đọc mã, chưa chạy ca file hỏng sau merge.

## Kiểm chứng hiện tại

[53 ca trại ngày 04/11](reports/CampEnvironment_PlayMode_2026-11-04.json) có cấp nguyên liệu qua API, chế một thuốc, lưu checkpoint và Continue giữ thuốc đã lưu. Không bao phủ đầy đủ pickup/Q/12 slot/migration/backup hỏng. Chưa có bằng chứng Windows build của tổ hợp sau merge. Lượt tài liệu này không chạy game hoặc đổi file lưu.

Cần thử lại: E/Q tay, túi đầy/chồng chưa đầy, Q khi pause/đầy máu/chết, cooldown, nhặt rồi load lùi, craft chưa lưu→rollback, native sai item/stack, legacy v5 thật, primary hỏng→backup, snapshot có ID cũ. Không lưu đồ debug vào ô cá nhân; backup trước bài thử ghi save.

## Bằng chứng lịch sử — 02/11, trước merge

Mốc nền cũ 7af0cf2; backup được ghi tại Temp/InventoryPotionsBackup_20261102. Khi đó có PlayerInventory trên An, ba pickup gần trại hậu tố _001, InventoryHUD và save v5.

- [76 ca Play Mode cũ](reports/20261102_InventoryPlayMode.json), [ảnh HUD cũ](images/InventoryHUD_PlayMode.png).
- [76 ca Windows cũ](reports/20261102_InventoryWindows.json), [build cũ](reports/20261102_InventoryBuild.json). Build tăng dần trước merge không chứng minh đã dựng/chạy HEAD hiện hành.

InventoryPotionsSetup.cs, InventoryPotionsSmokeProbe, PlayerInventory/Test/Fill 12 Slots và cờ -inventory-self-test không còn là công cụ hiện hành. Không gửi SonDoongInventorySetup.request.json hoặc áp lại gói cũ để kiểm tra dự án mới. Chưa có bộ 76 ca tương đương chuyển sang API mới; probe trại chỉ bao phủ phần hẹp nêu trên.
