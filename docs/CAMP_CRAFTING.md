# Chế bình máu trong túi đồ C1 — cập nhật 05/11/2026

Yêu cầu mới thay thế bàn chế: **I → Chế bình máu**, tiêu 1 Lá cẩm + 1 Linh chi, tạo 1 Bình máu; Q dùng khi đang chơi để hồi 35 máu. Bàn `CampCraftingTable` đã được gỡ khỏi Main và Preview. Checkpoint `cp_trai` giữ root, ref và spawn cũ; E tại lửa chỉ lưu, không mở chế.

`InventoryPanel.TryCraftPotion` chỉ nhận khi túi đang mở trong `InMenu`, An còn sống; debounce 0,25 giây theo unscaled time vì túi tạm dừng game. `InventoryService.CanCraft/TryCraft` dùng chung giao dịch: thử trừ nguyên liệu trên bản sao, thêm thành phẩm rồi commit. Túi đầy không mất nguyên liệu; nếu trừ nguyên liệu làm trống ô thì có thể chế. Nút hiển thị số nguyên liệu và lý do không thể chế. Thành công không tự ghi save; cần E checkpoint để giữ thuốc qua Continue.

`UI_Crafting/CraftingPanel` là đường dùng riêng còn được C2 tham chiếu; không xóa script/prefab dùng chung để gỡ bàn C1. Probe mới và kết quả ghi tại [C1_REVISION_WORKLOG](C1_REVISION_WORKLOG.md). Không chạy bot/probe cũ đòi tìm bàn trong scene mới.

## Lịch sử trước thay đổi 05/11 — không dùng làm cấu hình hiện hành

Yêu cầu tại mốc 04/11: E bàn chế, E riêng tại lửa để lưu. Được thay thế bởi chế trong túi theo phần đầu tài liệu.

## Cấu hình scene hiện hành

| Object trong 02_HangLinhMach | Cấu hình đọc từ scene/nguồn |
| --- | --- |
| Gameplay/Camp_CraftingStation | CampCraftingTable, vị trí (-3,0,3), rotation 0/scale 1, layer Interactable; inventory trỏ GameSession/InventoryService |
| Model con | VIS_CraftingTable, từ PF_VIS_CraftingTable.prefab; BoxCollider tương tác trên root bàn, center (0,0.5,0), size (1.65,1,0.9) |
| Gameplay/Campfire_Placeholder | Giữ CheckpointAltar, checkpointId=cp_trai, SaveService và SpawnPoint cũ; craftingPanel hiện None để E tại lửa chỉ lưu |
| Spawn | Gameplay/Checkpoint_CampSpawn tại (0,0.2,-5) |
| Phản hồi | AppRootUI.ShowToast, 3 giây; không dùng TXT_CraftFeedback của gói cũ |

Điểm đọc nguồn: [CampCraftingTable.cs](../Assets/_Project/Scripts/Interaction/CampCraftingTable.cs):6–35; [CheckpointAltar.cs](../Assets/_Project/Scripts/Save/CheckpointAltar.cs):17–50; scene [02_HangLinhMach.unity](../Assets/_Project/Scenes/02_HangLinhMach.unity):505 và :13665. Khoảng cách tâm bàn–checkpoint khoảng 3.16 m. Bản sao trước sửa scene và .meta ở SourceAssets/SceneBackups/2026-11-04_CampEnvironment.

## Giao dịch, chặn tương tác và lưu

CampCraftingTable.Interact gọi [InventoryService.TryCraft](../Assets/_Project/Scripts/Inventory/InventoryService.cs):81 trên bản sao slot: trừ nguyên liệu trước, thử thêm kết quả, commit và phát Changed một lần khi hợp lệ. Thất bại không commit bản nháp. ItemDefinition hiện dùng trường maxStack; không còn PlayerInventory.TryCraftHealthPotion hoặc HealthPotionCrafting của gói cũ.

CanInteract hiện chặn khi component tắt, thiếu inventory/interactor, phiên khác Playing, pause, trong debounce 0.35 giây, actor chết hoặc cách bàn quá 3 m. Scanner hiện có tiếp tục kiểm ray ngắm/LOS/tầm; bàn không tạo input action mới. **Code bàn hiện không kiểm riêng Attack/Dodge hoặc xác nhận interactor là đúng actor sở hữu inventory**; yêu cầu chặn các trường hợp này của bước cũ chưa được tái đáp ứng đầy đủ chỉ bằng các guard hiện có. Không dùng 57 ca cũ làm bằng chứng các guard mới đã đạt.

Chế thành công chỉ thay runtime inventory, không hồi máu và không gọi SaveService. Toast nhắc tới lửa để lưu. CheckpointAltar.Interact gọi SaveService.SaveAt("cp_trai"), nơi ghi snapshot và hồi đầy tài nguyên theo luật mốc lưu. Save hiện ghi **schemaVersion 4**, không phải v5 trước merge.

UI_Crafting/CraftingPanel vẫn tồn tại trong scene và code. [CraftingPanel.cs](../Assets/_Project/Scripts/UI/CraftingPanel.cs):77–79 có nhánh SaveAfterCraft nếu mở với altar; đó là đường UI khác với E trực tiếp tại bàn. Trại hiện bỏ liên kết mở panel trên checkpoint. Khi tái dùng panel phải đối chiếu chính sách lưu, không mặc định hai đường chế có cùng hành vi.

## Kiểm chứng có hiệu lực

[CampEnvironment_PlayMode_2026-11-04.json](reports/CampEnvironment_PlayMode_2026-11-04.json) của kiến trúc hiện hành đạt **53 kiểm tra tổng hợp khu trại**, có ray chọn riêng bàn/checkpoint, craft đúng một thuốc, lưu checkpoint, về menu→Continue giữ thuốc đã lưu và spawn an toàn. Save gốc được khôi phục byte-for-byte. Con số 53 là tổng ca khu môi trường, không phải 53 contract riêng của crafting.

Báo cáo dùng API/controller/ray ngắm; chưa phải bấm E tay. Chưa có Windows build của tổ hợp mới. Lượt đồng bộ tài liệu không chạy lại thử nghiệm. Cần tái thử thiếu nguyên liệu, túi đầy/ô vừa trống/chồng chưa đầy, Changed đúng một lần, debounce, Attack/Dodge, actor sai, craft chưa lưu→rollback và E thực tế trước nghiệm thu đầy đủ hợp đồng cũ.

Công cụ hiện còn: Tools → Son Doong → Camp Environment → Run Play Mode Regression trong [CampEnvironmentSetup.cs](../Assets/_Project/Editor/CampEnvironmentSetup.cs):83–88; [CampEnvironmentSmokeProbe.cs](../Assets/_Project/Tests/CampEnvironmentSmokeProbe.cs). Probe opt-in có backup/restore save, không chạy đồng thời với công cụ khác dùng cùng save.

## Bằng chứng lịch sử — 02/11, trước merge

Tại commit nền a6fb50e, bàn cũ ở (-5.8,0,0) dùng CampCraftingStation, PlayerInventory, HealthPotionCrafting, TXT_CraftFeedback và save v5. Backup cũ được ghi ở Temp/CampCraftingBackup_20261102; Temp là dữ liệu cục bộ, không đảm bảo đi theo clone.

- [7 contract cũ](reports/20261102_CraftingContracts.json).
- [57 ca Play Mode cũ](reports/20261102_CraftingPlayMode.json), [ảnh HUD cũ](images/CraftingHUD_PlayMode.png).
- [57 ca Windows cũ](reports/20261102_CraftingWindows.json), [build cũ](reports/20261102_CraftingBuild.json). Không có cảnh báo mới ở build tăng dần không chứng minh mọi cảnh báo shader trước đó đã được sửa.

CampCraftingSetup.cs, CampCraftingSmokeProbe, menu contract và cờ -crafting-self-test cũ đã bị thay/xóa sau merge. Không gửi SonDoongCraftingSetup.request.json hoặc áp lại ZIP cũ để kiểm tra dự án hiện hành.
