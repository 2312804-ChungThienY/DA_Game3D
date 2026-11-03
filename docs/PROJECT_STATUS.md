# Trạng thái thực tế và điểm tiếp nối

**Cập nhật: 04/11/2026, Asia/Bangkok.** Đây là ảnh chụp từ file dự án và các lượt làm việc trước. Đọc lại file hiện hành trước khi sửa vì người dùng cũng chỉnh trong Unity giữa các lượt chat. Thiết kế đích ở [PROJECT_MEMORY.md](PROJECT_MEMORY.md), nguồn ở [SPEC_INDEX.md](SPEC_INDEX.md).

## Merge nhánh 2312755_NguyenThangThieng (04/11/2026)

- Gộp vào `main` theo kiến trúc chung của nhánh mới: `AppRoot`/`GameSession`, `InputReader`, `WorldStateStore` theo `StateSet`, `InventoryService`, save schema 4, scene chương 1 và 2. Các file Unity YAML có xung đột được lấy thành bộ nhất quán từ nhánh mới; hai font Noto Sans TMP giữ bản đã có glyph ở `main`. GUID của các asset mới lấy theo nhánh mới. Giữ cả hai phần lịch sử trạng thái bên dưới để truy nguyên, nhưng các mô tả API/save v5 tại các mục cũ là trạng thái **trước merge**, không còn mô tả mã hiện tại.
- Bỏ script/prefab prototype và smoke probe cũ của `main` đã được kiến trúc mới thay thế, sau khi xác nhận không còn scene/prefab nào tham chiếu GUID của chúng. `SaveService` mới đọc thêm save v4/v5 cũ của chương 1, chuyển inventory, pickup, puzzle, Hộ vệ và phục dựng sang schema 4 trong bộ nhớ; file cũ chỉ bị thay khi lần lưu mới thành công.
- Đã kiểm tra: toàn bộ script runtime hiện tại biên dịch bằng Roslyn đi kèm Unity 6000.4.8f1, không còn dấu conflict trong code/scene/prefab, không còn tham chiếu scene/prefab đến GUID script prototype đã xóa. **Chưa kiểm tra Play Mode hoặc build Windows sau merge**; cần mở Unity để nhập lại asset, kiểm tra Console, chạy tuyến chương 1→2 và thử Continue với save v5 thật (kèm backup trước khi thử lưu).

## Yêu cầu gần nhất

### HUD mục tiêu chương Kim Quy (02/11/2026)

- Thêm nguyên gói Quest HUD: ChapterObjectiveRules ở `Scripts/Quests` sẵn có, ChapterQuestHUD và menu ChapterObjectiveChecks; không sửa script cũ. Trong `Canvas_Gameplay`, `PNL_Quest` 480×210 neo trên phải, nền đen alpha0.55, hai text đúng thông số có font Việt/raycast tắt; QuestUI luôn bật, checklist trống. Tắt component QuestTracker và TXT_QuestObjective cũ trong scene để tránh trùng hiển thị.
- Chỉ đọc WorldStateStore, tính lại tám mục tiêu và progress 3 mảnh/2 puzzle; không lưu chỉ số nhiệm vụ, không cấp thưởng. Mục tiêu đầu là Vườn Rêu theo yêu cầu; bước nhận đèn vẫn ở gameplay hiện có, không nằm trong HUD mới. Save v5, pickup/chế/uống và mã gameplay giữ nguyên.
- **128 tổ hợp + reset New Game và 64 kiểm tra Play Mode đạt**, không lỗi runtime. Đã thử tiến trình qua API nhặt/puzzle/Hộ vệ/bệ, rollback checkpoint trước B, Continue hoàn thành, reset Chơi mới, inventory không đổi mục tiêu và đăng ký event không trùng. Save chính/backup/tmp gốc khôi phục byte-for-byte.
- Đã xem ảnh HUD Play Mode và panel tại 1920×1080, 1280×720, 1600×1000: đủ dấu Việt, chữ không tràn. Ảnh đa tỉ lệ là render riêng panel; còn thử tuyến đầy đủ bằng tay và Windows cho bước này. Chi tiết/báo cáo: [QUEST_HUD.md](QUEST_HUD.md). Mốc trước sửa `24b34e4`; backup `Temp/QuestHUDBackup_20261102`.

### Bàn chế thuốc tại Trại Giếng Trời (02/11/2026)

- Theo ZIP Camp Crafting: ghép duy nhất `TryCraftHealthPotion()` vào PlayerInventory, giữ `.meta`; thêm HealthPotionCrafting, CampCraftingStation và menu CraftingContractChecks. E tại bàn đổi 1 Lá cẩm + 1 Linh chi → 1 Thuốc máu trên bản nháp, commit toàn bộ và phát Changed một lần; thất bại giữ nguyên túi. **Lưu thủ công tại checkpoint theo yêu cầu mới**, không tự gọi SaveAtCamp, không hồi máu khi chế; SaveData/SaveService vẫn nguyên v5.
- Scene `02_HangLinhMach`: `Gameplay/Camp_CraftingStation` ở `(-5.8,0,0)`, cách checkpoint lửa trại 1.8m, có TableVisual đúng kích thước và collider/layer Interactable; gán túi An, WorldState GameSession, range3/interval0.35. `TXT_CraftFeedback` giữa dưới, font Việt 24, 850×70, không raycast, không dùng chung prompt E.
- **7 contract checks và 57 kiểm tra Play Mode đạt**, không lỗi runtime. Đã thử ba nhánh lưu/tải, thiếu nguyên liệu/không chỗ/ô vừa trống/chồng chưa đầy, debounce, pause/xa/đánh/né/chết/hoàn thành; save gốc khôi phục byte-for-byte. Đã xem ảnh UI; bấm phím E/Q và cảm giác ngắm/đi lại trực tiếp vẫn cần thử thủ công.
- Bản Windows Development dựng thành công (bản cuối 0 lỗi, 0 cảnh báo mới), đạt **57 kiểm tra**, không lỗi runtime; save gốc được khôi phục.
- Chi tiết và báo cáo: [CAMP_CRAFTING.md](CAMP_CRAFTING.md). Mốc trước sửa `a6fb50e`; sao lưu `Temp/CampCraftingBackup_20261102`. Chưa có panel chế thuốc; dùng E trực tiếp.

### Túi đồ, nguyên liệu và Q dùng thuốc (02/11/2026)

- Theo ZIP Inventory Potions: scene `02_HangLinhMach` có túi 12 ô trên An, Q dùng thuốc hồi 35/2 giây, ba ItemDefinition (Lá cẩm/Linh chi chồng 10, Thuốc máu chồng 5), ba pickup ID riêng trên bệ gần trại và HUD danh sách dưới trái. Input của An dùng bản riêng `Assets/_Project/Input/PlayerControls.inputactions`, giữ ID các action cũ và thêm `Player/UsePotion`.
- Ghép năm script cũ thay vì ghi đè các sửa đổi riêng: giữ IDodgeVitals/bất tử né, fallback component của SaveService, ApplySavedState của đèn/gương, quy trình ghi file tạm/flush/kiểm tra/backup. SaveData hiện là **v5**, có inventory và collectedPickups; giữ đọc v1–v4, kể cả bản camp cũ thiếu checkpoint ID. Save v5 thiếu hai trường mới bị từ chối để tránh mất/nhân đồ.
- Play Mode từ Boot đạt **76 kiểm tra**, không lỗi runtime: nhặt một lần/chồng/túi đầy, 40→75 và giới hạn max, cooldown simulation/pause, rollback trước/sau nhặt hoặc dùng thuốc, migration, backup, chết/hồi sinh, Continue, hoàn thành và xác nhận Chơi mới. Save chính/backup/tmp của người dùng được sao lưu rồi khôi phục byte-for-byte. Ảnh HUD đã xem; E/Q trực tiếp bằng bàn phím và cảm giác ngắm/đi lại vẫn cần thử thủ công. Kiểm thử gọi cùng các lệnh tương tác/sử dụng mà input gọi, không giả là đã bấm phím thật.
- Bản Windows Development dựng thành công (bản cuối 0 lỗi, 0 cảnh báo mới), đạt cùng **76 kiểm tra**, không lỗi runtime; mã mới đã bỏ hai lời gọi API FindObjectsSortMode cũ của gói.
- Chi tiết cấu hình, kiểm chứng và công cụ tái kiểm tra ở [INVENTORY_POTIONS.md](INVENTORY_POTIONS.md). Mốc trước sửa `7af0cf2`; backup ở `Temp/InventoryPotionsBackup_20261102`. Chưa có cửa sổ phím I, chọn ô hoặc chế thuốc trong bước này.

### Cài đặt âm lượng menu/pause (02/11/2026)

- Đã tích hợp gói Audio Settings vào `01_MainMenu` và `02_HangLinhMach`: panel tiếng Việt, ba slider/phần trăm, Mặc định/Đóng, SceneAudio và mixer dùng chung; bước chân/tiếp đất của An đi qua SFX. Giữ GUID, fallback StarterAssetsInputs và SaveData v4; không sửa combat/né. Checkpoint trước sửa: `ae01a35`.
- Windows x64 Development build thành công, không lỗi. Hai lượt chạy đạt 35 + 5 kiểm tra lưu/cấu hình/pause, không lỗi runtime; hash tiến trình không đổi và prefs âm thanh ban đầu đã khôi phục. Xem [AUDIO_SETTINGS.md](AUDIO_SETTINGS.md), báo cáo và ảnh.
- Còn nghe âm thật, nhấn Esc và kéo chuột trực tiếp. Hai scene chưa đặt nhạc nền/nguồn trống/cửa/địch. Cảnh báo build chủ yếu từ compute shader AI Inference có sẵn, chưa xử lý trong công việc âm lượng.
- Workspace hiện tại: `E:\DoAnGame3D\Doan_Game3D`; đường dẫn cũ có dấu không còn tồn tại.

### Sửa lỗi dựng NavMesh lúc chạy (31/10/2026)

- Stack trace `TestCombatNavMeshBootstrap.Awake()` trên `02_HangLinhMach` dẫn đến hai instance `Rock_Cave_02` và `Rock_Cave_03`. Rotation ghi trong scene bị làm tròn khiến bình phương độ dài quaternion lần lượt lớn hơn 1; đã chuẩn hóa hai cặp Y/W trên instance, giữ nguyên prefab và hình học. `Cave_Rocks_A` dùng MeshCollider từ `MinesSet.fbx`; FBX đang `isReadable: 0` trong khi NavMesh được dựng lúc chạy. Đã bật Read/Write trên import settings của FBX (`MinesSet.fbx.meta`), giữ GUID và collider gốc để NavMesh runtime đọc được mesh trong Windows player.
- Đã sao lưu scene và `.meta` trước sửa vào `Temp`; kiểm tra tĩnh hai quaternion có độ dài gần 1 (sai số dưới 1e-8) và `isReadable: 1`. `git diff --check` trên toàn scene vẫn báo các dòng YAML có khoảng trắng cuối dòng đã tồn tại từ những thay đổi scene trước đó; bốn dòng quaternion mới và `.meta` không có lỗi này. **Chưa xác nhận Unity Editor reimport, Play Mode không còn ba cảnh báo, dữ liệu NavMesh đúng hoặc Windows build.** Cần kiểm tra sau khi Unity reimport FBX và chạy lại từ Boot.

### Menu tạm dừng Esc trong 02_HangLinhMach (31/10/2026)

- Đã sao lưu scene và `PlayerCombatController.cs` vào `Temp/PauseMenuBackup_20261031` trước khi sửa. Thêm `PauseMenuController.cs` từ gói người dùng, giữ `.meta` mới đi cùng script; gắn component trên `GameSession`, tham chiếu `PlayerInput`, `StarterAssetsInputs`, `PlayerVitals` của An và `WorldStateStore`. Controller dùng action Esc riêng, khóa gameplay input, tạm dừng thời gian/âm thanh, mở chuột và chờ thả nút hành động hai lần cập nhật input trước khi tiếp tục. Thêm fallback tìm `StarterAssetsInputs` trên An nếu tham chiếu scene bị mất. Không sửa `SaveData` hay file lưu.
- Trong Canvas_Gameplay đã tạo `Panel_Pause` phủ màn hình, có CanvasGroup và trạng thái, hai nút Tiếp tục/Về menu; `Panel_ReturnConfirm` có nội dung cảnh báo và hai nút Về menu/Hủy. Hai panel tắt trong Edit Mode, nằm trên HUD; mỗi nút có đúng một OnClick. `PlayerCombatController.Update()` kiểm tra `Time.timeScale == 0f` ngay sau `ready`, trước nhánh hủy đòn. Input System settings hiện đã là `ProcessEventsInDynamicUpdate` (`m_UpdateMode: 1`).
- Kiểm tra tĩnh: Roslyn Unity 6000.4.8f1 biên dịch `Assembly-CSharp` thành công; scene có 1041 fileID không trùng, 59 ID mới và các tham chiếu/nút pause đã được đối chiếu. **Chưa xác nhận Unity Editor import, Play Mode từ Boot, thao tác Esc/chuột/phím, các trường hợp đánh/né/địch/hiệu ứng/chết/hoàn thành, tiếp tục từ menu hoặc Windows build.** Chưa commit vì điều kiện kiểm tra Play/build trong yêu cầu chưa đạt.

### Menu Chơi mới – Tiếp tục – Thoát (31/10/2026)

- Đã sao lưu mã, ba scene và Build Settings trước khi sửa vào `Temp/MainMenuBackup_20261031`. Thêm `GameSaveStorage` đọc v1–v4 và xác thực snapshot cho menu; ưu tiên save chính rồi backup, ghi qua file tạm có flush/đọc kiểm tra lại và giữ backup hợp lệ khi file chính lỗi. `SaveService` vẫn tự lấy CharacterController/PlayerInput từ An, giữ tham chiếu pickup/receiver hiện có, dùng cùng bộ xác thực và thêm `TryStartNewGame`; Chơi mới chỉ ghi snapshot trống cp_trai sau khi scene đã khởi tạo thành công. `CheckpointRespawnFlow` chuyển scene bất đồng bộ, khóa input/timeScale khi tải, có thử lại/về menu lúc lỗi; `ChapterCompletionPresenter` có Về menu.
- Build Settings toàn cục bật ba scene theo thứ tự `00_Boot`, `01_MainMenu`, `02_HangLinhMach`; Windows Build Profile hiện dùng danh sách toàn cục (`m_OverrideGlobalSceneList: 0`). `00_Boot` có `BootMenuLoader`. `01_MainMenu` có một Canvas Overlay 1920×1080, EventSystem InputSystem UI actions, tiêu đề, Chơi mới/Tiếp tục/Thoát, text trạng thái và panel xác nhận thay save; mỗi nút gắn đúng một OnClick. `02_HangLinhMach` có `Panel_LoadError` với Thử lại/Về menu và thêm nút Về menu trên `Panel_ChapterComplete`; flow tham chiếu panel lỗi. Giữ F9 để debug màn chơi trực tiếp.
- Kiểm tra tĩnh: Roslyn với tham chiếu Unity biên dịch `Assembly-CSharp` thành công; Boot/Menu/Main có 20/90/982 fileID không trùng/thiếu tham chiếu local; kiểm tra phương thức OnClick và thứ tự scene. **Chưa xác nhận Unity Editor import, Play từ Boot, click UI, file save thật, Continue từ backup, hồi sinh, load bản hoàn thành hoặc Windows build.** Chưa commit theo điều kiện thử trong yêu cầu. Menu pause/cài đặt vẫn là bước sau.

### Phục dựng Trống Đồng và hoàn thành chương Kim Quy (31/10/2026)

- Đã sao lưu mã và scene sắp sửa vào `Temp/ChapterCompletionBackup_20261031`. Giữ các `.meta` cũ, mở rộng `WorldStateStore` với `relicRestored` và điều kiện đủ A/B/C, hai câu đố, `guardian_01`. `SaveData` lên version 4; `SaveService` giữ đường dẫn `save_v1.json`, ghi qua file tạm/kiểm tra lại/backup như trước, chuyển v1/v2/v3 khi đọc và chỉ ghi cờ hoàn thành vào WorldState sau khi file cp_kim_quy đã được commit thành công. `SaveCheckpoint` hỗ trợ record cp_kim_quy không collider; `CheckpointRespawnFlow` gọi presenter sau khi khôi phục và bật lại input để tải bản hoàn thành vẫn hiện panel.
- Trong `02_HangLinhMach` thêm `Checkpoint_KimQuySpawn` `(0,0.2,107)` và `Checkpoint_KimQuy_Record`; `SaveService.checkpoints` có ba mốc. `Altar_KimQuy` chuyển sang layer Interactable, giữ BoxCollider thường, có `RelicRestorationAltar`; `BrokenDrumVisual` dạng khối thử, `RestoredDrumVisual` dùng Cylinder dấu bố trí cũ và chỉ hiện sau khi phục dựng. `Panel_ChapterComplete` phủ màn hình có tiêu đề/phụ đề tiếng Việt và nút `BTN_QuitGame`; `ChapterCompletionPresenter` trên GameSession khóa input, dừng thời gian, mở chuột sau khi lưu hoặc tải bản hoàn thành. `QuestTracker` đổi mục tiêu sau thắng Hộ vệ thành phục dựng rồi hoàn thành.
- Kiểm tra tĩnh: Roslyn với tham chiếu Unity biên dịch `Assembly-CSharp` thành công; scene có 947 fileID không trùng/thiếu tham chiếu local; `git diff --check` không báo lỗi nội dung. **Chưa xác nhận Unity Editor import, Play Mode, tương tác bệ, ghi file thật, tải v1/v2/v3/v4, UI nút Thoát hoặc Windows build.** Chưa commit theo điều kiện kiểm tra trong yêu cầu. Camera/hoạt ảnh kết thúc và menu Tiếp tục chưa triển khai.

### Trận Hộ vệ trong 02_HangLinhMach và lưu guardian_01 (31/10/2026)

- Đã sao lưu các file code/scene sắp sửa vào `Temp/GuardianEncounterBackup_20261031`. Giữ GUID `.meta` hiện có, ghép `WorldStateStore` với tập `defeatedEnemies`, `EnemyVitals.Died`, `KimQuyGate.SetEncounterLocked` và `SaveData` version 3 có danh sách địch độc nhất. `SaveService` ghi/đọc/phục hồi `defeatedEnemies`; chuyển bản lưu v1→v2→v3, chấp nhận v3 cũ chưa có trường mới bằng tập rỗng, và kiểm tra `guardian_01` chỉ hợp lệ khi đủ A/B/C cùng hai câu đố. Giữ đường dẫn save và luồng reload/checkpoint cũ. `QuestTracker` hiển thị mục tiêu đánh Hộ vệ rồi lưu.
- Thêm `GuardianEncounter` trên `Arena_KimQuy_Controller` ở `(0,1,96)` với BoxCollider trigger 3×2×1 và Rigidbody kinematic; tham chiếu WorldState, cổng, PlayerVitals An, root và EnemyVitals Hộ vệ. Scene có instance `Guardian_Main` từ `PF_Guardian_Prototype` ở `(0,0,103)`, xoay 180°, tắt trong Edit Mode, có hai điểm tuần tra quanh Z=103; thêm `Navigation_Guardian` Agent Type 1 và dấu `AltarReadyIndicator` không collider trên bệ. Cổng tại Z=93 vẫn giữ quyền bật/tắt Barrier; trận chỉ gọi khóa/mở. Thắng ghi `guardian_01` vào phiên, mở cổng và bật dấu; cần tự lưu ở checkpoint trước Đàn để giữ qua F9.
- Kiểm tra tĩnh: Roslyn với tham chiếu Unity biên dịch `Assembly-CSharp` thành công; scene có 915 fileID không trùng/thiếu tham chiếu local, tham chiếu prefab/cổng/An và script mới đã được kiểm tra văn bản. **Chưa xác nhận Unity Editor import, Bake NavMesh Guardian ở màn chính, Play Mode, F9/hồi sinh, tương tác checkpoint hoặc Windows build.** Chưa commit theo điều kiện thử Play/build. Bệ trống chưa có tương tác phục dựng hoặc tự lưu kết thúc chương.

### Hộ vệ Đá hai đòn trong 91_TestCombat (31/10/2026)

- Đã sao lưu `EnemyMeleeAttack.cs`, `91_TestCombat.unity` và `NavMeshAreas.asset` vào `Temp/GuardianBackup_20261031` trước khi sửa. Thay nội dung `EnemyMeleeAttack.cs` theo ZIP nhưng giữ `.meta`/GUID cũ; thêm `EnemyAttackDefinition.cs` và hai dữ liệu `ATK_Guardian_Sweep` (20 máu, quét trước 120°, lấy đà 0,8 giây) và `ATK_Guardian_Slam` (25 máu, bán kính 3 m, lấy đà 1,2 giây). Hắc Ảnh để hai definition trống, tiếp tục dùng đòn cũ 15 máu.
- Tạo prefab độc lập `PF_Guardian_Prototype` từ cấu hình Hắc Ảnh: một `EnemyVitals` 180 máu, `EnemyLanternWeakness` Focus 1 giây/yếu 4 giây/hệ số 1,5, Capsule cao 3 m vật liệu đá, Agent radius 0,75/height 3, hai definition và `SlamWarning` Cylinder không collider, vật liệu URP Unlit cam đỏ. Nhãn tùy chọn của `EnemyVitals` dùng tên object thay vì cố định “Hắc Ảnh”. Tạo Agent Type `Guardian_Test` và `Navigation_Guardian` thu collider Environment; bootstrap runtime tiếp tục tạo NavMesh khi chưa có dữ liệu bake. Instance `Guardian_Test` ở `(0,0,5)`, dùng PlayerVitals và hai điểm tuần tra scene. Hắc Ảnh test được tắt bằng override để thử riêng; bật lại để kiểm tra fallback.
- Kiểm tra tĩnh: Roslyn với tham chiếu Unity biên dịch `Assembly-CSharp` thành công; scene 103 fileID và prefab Hộ vệ 30 fileID không trùng/thiếu tham chiếu local; `.meta` của `EnemyMeleeAttack` giữ GUID cũ. **Chưa xác nhận Unity Editor import, Bake NavMesh hai Agent Type, Play Mode, sát thương quét/nện, né, Focus, pause hoặc Windows build.** Cần kiểm tra bảng thử trong tệp yêu cầu; chỉ commit sau khi Play đạt. Hộ vệ chưa được nối vào scene chính, cổng đấu trường hoặc save `guardian_01`.

### Checkpoint trước Đàn Kim Quy và hồi sinh theo ID (31/10/2026)

- Tệp hướng dẫn mới lặp lại bước dựng Đàn Kim Quy đã có, rồi bổ sung bước checkpoint. Đổi marker Z=90 thành `Checkpoint_BeforeArenaSpawn` `(0,0.2,90)`; thêm bệ `Checkpoint_BeforeArena` `(1,0.5,90)` layer Interactable, BoxCollider thường. Giữ `Checkpoint_CampSpawn` ở trại. Bệ trại được đổi từ `CampCheckpoint` sang `SaveCheckpoint` trên chính component scene cũ; hai bệ có đúng một `IInteractable` mỗi object.
- `SaveCheckpoint` lưu theo ID `cp_trai`/`cp_truoc_dan`, chặn tương tác khi An chết, đang đánh/né hoặc pause; mốc trước Đàn còn yêu cầu `Gate_KimQuy.CanPass`. `SaveService` quản lý hai checkpoint, phát hiện thiếu/trùng ID, ghi ID đã chọn vào `SaveData`, kiểm tra snapshot `cp_truoc_dan` phải có đủ A/B/C và hai câu đố, và đặt An ở đúng RespawnPoint khi ApplySnapshot. F9 và nút hồi sinh tiếp tục dùng `CheckpointRespawnFlow` tải lại scene. `SaveData` hiện đã là version **3**, nên giữ version 3 và đường chuyển đổi bản cũ hiện có; không hạ về version 2 như câu trong tệp hướng dẫn.
- Kiểm tra tĩnh: Roslyn với tham chiếu Unity biên dịch `Assembly-CSharp` thành công; scene có 892 fileID không trùng/thiếu tham chiếu local, hai `SaveCheckpoint` và không còn `CampCheckpoint` trong scene; `git diff --check` không báo lỗi nội dung. **Chưa xác nhận Editor import, Play Mode, F9/hồi sinh tại cả hai mốc hoặc Windows build**; chưa commit theo điều kiện kiểm tra đạt.

### Đàn Kim Quy và cổng kiểm tra tiến trình trong 02_HangLinhMach (31/10/2026)

- Hành Lang Quang hiện kết thúc ở Z=82, nên đã mở vách bắc thành lối 3,5 m và thêm `Floor_LightToKimQuy_Bridge` Z=82–88. `Floor_ToKimQuy` Z=88–94, `Floor_KimQuyArena` Z=94–112, mặt trên đều Y=0; sàn đấu trường 18×18 m. Thêm vách và trần đoạn nối, vách quanh đấu trường, chừa lối vào qua cổng. Các khối là blockout cần kiểm tra đường đi, camera và NavMesh trong Editor.
- Tạo `PF_KimQuyGate` và instance `Gate_KimQuy` tại `(0,0,93)`. `Barrier` local `(0,1.5,0)`, scale `(3,3,0.4)`, layer Interactable, BoxCollider thường và NavMeshObstacle Box có Carving; TextMeshPro 3D `TXT_KimQuyGateStatus` dùng Noto Sans SDF. Instance gán WorldStateStore của GameSession, prefab giữ tham chiếu nội bộ Barrier/Text để dùng lại. `KimQuyGate` kiểm tra đúng các ID A/B/C và hai câu đố, cập nhật khi WorldStateStore phát Changed, nên F9 tải snapshot tự đóng/mở cổng mà không đổi SaveData.
- Thêm `GuardianSpawn` `(0,0,103)`, `ArenaEntryPoint` `(0,0,96)`, `BeforeArenaCheckpointMarker` `(0,0.2,90)` trong Gameplay; hai marker cuối chưa có chức năng. Thêm bệ `Altar_KimQuy` và trống Cylinder chỉ làm dấu bố trí, chưa có Hộ vệ hoặc kết chương.
- Kiểm tra tĩnh: Roslyn với tham chiếu Unity biên dịch `Assembly-CSharp` thành công; scene có 885 fileID và prefab cổng 13 fileID, không trùng/thiếu tham chiếu local; `git diff --check` không báo lỗi nội dung. **Chưa xác nhận Unity Editor import, Play Mode, đủ điều kiện mở cổng, tải lùi F9, va chạm, Bake NavMesh hoặc Windows build.** Chưa commit theo điều kiện Play đạt.

### Hắc Ảnh trong màn chính và hồi sinh từ checkpoint (31/10/2026)

- `02_HangLinhMach` giữ một An, gắn thêm `PlayerVitals`, `PlayerStamina`, `PlayerCombatController` và `AttackOrigin` trên instance prefab hiện có; Focus mask nhận Enemy. Canvas chính có `TXT_PlayerHealth`, `TXT_PlayerStamina`, `Panel_Death` và `BTN_Respawn`; `MainCombatUI` gán `PlayerHealthPresenter`/`PlayerStaminaHUD` và khóa motor, dao, tương tác, đèn khi chết. EventSystem của scene đã dùng InputSystemUIInputModule với actions UI riêng; PlayerInput gốc có UI Input Module để trống.
- Đặt `Enemy_HacAnh_Main` từ `PF_HacAnh_Prototype` ở `(0,0,65)`, hai điểm tuần tra `(-2,0,65)`/`(2,0,65)` và gán PlayerVitals của An cho AI/đòn đánh. Sàn `Floor_LightCorridor` đã phủ Z=62–82, rộng 20 m, nên không dựng sàn chồng. Thêm `Navigation_Main` với NavMeshSurface thu collider Environment và bootstrap runtime khi chưa có NavMeshData. Cửa Trống Đồng và cửa Gương có NavMeshObstacle Carving; cửa Trống đổi sang Default để không bị bake thành vách tĩnh. **NavMeshData vẫn cần Bake và kiểm tra trong Editor.**
- `SaveService.LoadGame` (F9 và nút hồi sinh) đọc file chính/backup hợp lệ rồi nhờ `CheckpointRespawnFlow` tải lại scene hiện hành; scene mới chờ các component hoàn tất Start, áp snapshot, hồi đầy máu/thể lực/năng lượng, đặt An ở `cp_trai`, sau đó mới mở input. `PlayerVitals.RestoreFull` xóa bất tử; `SaveAtCamp` từ chối khi An chết và hồi đầy chỉ số sau khi ghi file thành công. Hồi sinh tạo lại địch và xóa trạng thái chiến đấu/làm yếu nhờ tải lại scene; trạng thái puzzle/nhặt vật được áp theo snapshot.
- Kiểm tra tĩnh: biên dịch `Assembly-CSharp` bằng Roslyn và tham chiếu Unity thành công; scene có 776 fileID, hai prefab câu đố có 72/53 fileID, không trùng; `git diff --check` không báo lỗi nội dung. **Chưa xác nhận Editor import, Play Mode, click nút hồi sinh, Bake NavMesh hoặc Windows build.** Cần thử save→chết→hồi sinh với mảnh/puzzle/địch, thao tác An sau hồi sinh, UI khi PlayerInput bị khóa và tải F9 sau khi mở lại build. Chưa commit vì điều kiện Play/build chưa đạt; các thay đổi combat từ bước trước vẫn chưa commit.

### Lồng Đèn Sen làm yếu Hắc Ảnh trong 91_TestCombat (31/10/2026)

- Thêm `EnemyVitals` (60 máu, triển khai `IDamageable`) thay `TrainingDummy` trên gốc `PF_HacAnh_Prototype`. Đòn dao 20 sát thương thường, 30 khi chịu hệ số 1,5; chết thì tắt gốc để dừng AI, đòn đánh và hiệu ứng. `PF_TrainingDummy` riêng vẫn giữ cho phép thử dao.
- Thêm `EnemyLanternWeakness` triển khai `ILanternTarget`: Focus liên tục 1 giây để làm yếu 4 giây; chiếu tiếp làm mới thời hạn, không cộng dồn hay gây choáng. Ngắt chiếu đặt lại tiến độ 0, hiệu ứng dùng `Time.time` nên dừng khi pause và xóa khi địch bị tắt. Scanner thứ tự 100, weakness thứ tự 200.
- Prefab địch có `LanternAimPoint` local `(0,1.2,0)` và `WeakenedIndicator` Sphere xanh ngọc local `(0,2.6,0)`, scale 0,2, không collider; giữ `AttackWarning` riêng. Instance An trong `91_TestCombat` override Focus Target Mask thành LanternTarget+Enemy (1152), Visibility Mask thành Default+Environment+Interactable+LanternTarget+PuzzleBeam+Enemy (3465). Đặt `Pickup_Lantern_Combat` từ `PF_LanternPickup` gần An; prefab vật nhặt có collider thường, layer Interactable và `LanternPickup`.
- Kiểm tra tĩnh: Roslyn với tham chiếu Unity biên dịch `Assembly-CSharp` thành công; prefab địch 26 fileID và scene 97 fileID không trùng, `git diff --check` không báo lỗi nội dung. **Chưa xác nhận Unity Editor import, Play Mode, NavMesh Bake hoặc Windows build**. Cần thử nhặt đèn/Focus, ngắt chiếu và làm mới, 20/30 sát thương, vật cản, giao chiến, chết, pause; sau khi đạt mới commit. Các thay đổi của các bước combat trước vẫn còn trong working tree.

### Hắc Ảnh đánh trả trong 91_TestCombat (31/10/2026)

- Trên `PF_HacAnh_Prototype`, thêm `AttackOrigin` local `(0,1,0)` và `AttackWarning` là Sphere nhỏ màu vàng local `(0,2.3,0)`, scale `(0.25,0.25,0.25)`, không có collider, mặc định ẩn. Thêm `EnemyMeleeAttack` lên gốc; scene instance gán `PlayerVitals` của An cho component này. `DamageZone_Test` được tắt bằng override trên instance scene để phép thử máu không lẫn sát thương bẫy.
- Đòn bắt đầu trong tầm 1,7 m, gây 15 sát thương tại mốc hết báo trước 0,45 giây nếu An còn sống, trong tầm 1,8 m, trước mặt trong góc 90°, chênh cao tối đa 1,5 m và không bị vật cản Default/Environment/Interactable/PuzzleBeam che. Hồi thế 0,55 giây; giữa hai lần bắt đầu tối thiểu 1,6 giây. Hướng chốt khi bắt đầu, Agent dừng và không tự xoay lúc đánh. Impact giải quyết tối đa một lần kể cả frame dài; `PlayerVitals` tự kiểm tra bất tử né tại thời điểm nhận sát thương. Khi An chết hoặc component/địch bị tắt, đòn và cảnh báo được hủy.
- `EnemyPatrolChase` giữ quyền quyết định di chuyển/trạng thái, dừng lệnh Agent khi `EnemyMeleeAttack.IsAttacking` và chỉ yêu cầu đánh trong `Chase` khi còn thấy An. `NavMeshAgent` vẫn là nơi duy nhất dịch chuyển địch. Hắc Ảnh tiếp tục dùng một `TrainingDummy` 60 máu trên gốc để nhận dao.
- Kiểm tra tĩnh: `Assembly-CSharp` biên dịch thành công bằng Roslyn của Unity 6000.4.8f1 với `.rsp` dự án; prefab địch 19 fileID, scene 95 fileID, không trùng; `git diff --check` đạt. **Chưa xác nhận Unity Editor import, Play Mode, va chạm/đường nhìn, nhịp 0,45/0,55/1,6 giây, cửa sổ bất tử hay Windows build**. NavMeshData Editor vẫn chưa Bake/lưu từ bước trước. Chưa commit theo điều kiện Play đạt.

### Hắc Ảnh tuần tra, phát hiện, đuổi và quay về trong 91_TestCombat (31/10/2026)

- Package `com.unity.ai.navigation` 2.0.12 đã có trong `Packages/manifest.json`, nên không cài lại. Mở rộng `Floor_Combat` từ 20×20 lên 40×40, thêm `Wall_AI_Test` tại `(0,1.5,0)` scale `(4,3,0.5)` layer Environment với BoxCollider thường. Dời An tới `(0,0.2,-8)` và hình nộm cũ tới `(0,1,-5)` để vùng thử dao ở phía An của tường.
- Thêm `Navigation` với `NavMeshSurface`: agent type 0, Collect All, layer Environment (mask 8), Physics Colliders, Walkable. Chưa tạo/lưu NavMeshData trong Editor vì chưa điều khiển được Editor/Bake từ phiên này. `TestCombatNavMeshBootstrap` dựng NavMesh ở `Awake` khi Surface chưa có dữ liệu bake, để scene thử có thể chạy; cần Bake trong Editor và kiểm tra vùng đi được trước khi coi dữ liệu bake là hoàn thành.
- Thêm `EnemyPatrolPoints` với Point_A `(-4,0,6)` và Point_B `(4,0,6)`. Tạo prefab `PF_HacAnh_Prototype`: gốc layer Enemy có NavMeshAgent type 0, radius 0,4, height 2, speed 2, angular speed 360, acceleration 12; con `Visual` là capsule tối màu có collider thường, con `EyePoint` local `(0,1.6,0)`. Một `TrainingDummy` 60 máu trên gốc nhận dao và tắt cả địch khi hết máu. Không có Rigidbody động/CharacterController/đòn đánh địch.
- `EnemyPatrolChase` có Patrol → Chase → Search → Return, tầm nhìn 10 m/góc 120°/kiểm tra tường, mất dấu 3 giây, tìm 2 giây, leash 18 m, dừng gần An 1,7 m. Instance `Enemy_HacAnh_Test` ở `(4,0,6)` hướng 180°, đã gán `PlayerVitals` An, EyePoint và hai điểm tuần tra; mask tầm nhìn Default+Environment+Interactable+PuzzleBeam (2313). Script kiểm tra `isOnNavMesh` trước khi lập đường, dừng khi An chết/pause.
- Kiểm tra tĩnh: `Assembly-CSharp` biên dịch thành công bằng Roslyn của Unity 6000.4.8f1 với `.rsp` dự án và script mới; scene 95 fileID, prefab địch 12 fileID, không trùng; `git diff --check` đạt. **Chưa chạy Unity Editor import, Bake lưu NavMeshData, Play Mode hoặc Windows build**; cần xác nhận đường vòng qua tường, góc/LOS, các chuyển trạng thái, leash, dừng gần An và 3 đòn dao hạ địch. Chưa commit theo điều kiện Play đạt.

### Đòn dao và hình nộm trong 91_TestCombat (31/10/2026)

- Đã thêm action `Attack` kiểu Button, `<Mouse>/leftButton`, không interaction vào map `Player` của `StarterAssets.inputactions`; giữ binding Focus chuột phải. Trên An instance của `91_TestCombat` thêm `AttackOrigin` local `(0,1,0)` và `PlayerCombatController` với 20 damage, 10 stamina, đòn 0,65 giây, active `[0,15; 0,30)`, target mask Enemy+Hurtbox (640), visibility mask Default+Environment+Enemy+Hurtbox+Interactable+PuzzleBeam (2953).
- `PlayerCombatController` đọc Input System, kiểm tra khoảng active kể cả frame dài, dùng `OverlapCapsule` và `HashSet<IDamageable>` để một mục tiêu nhiều collider chỉ bị xử lý một lần mỗi đòn; lọc bản thân và ray kiểm tra vật cản. `ThirdPersonController` khóa chuyển động ngang/nhảy lúc đánh nhưng vẫn là motor duy nhất gọi `CharacterController.Move`; đánh và né không chồng nhau. Do ranh giới assembly `Unity.StarterAssets`/`Assembly-CSharp`, trạng thái đánh nối qua `IPlayerAttackState` khai báo trong file motor, thay vì tham chiếu trực tiếp `PlayerCombatController` trong assembly mẫu.
- Thêm `TrainingDummy` 60 máu, prefab capsule `PF_TrainingDummy` layer Enemy, collider thường, không Rigidbody; instance ở `(0,1,1.6)` trước An và trước vùng DamageZone. Đã thêm `PlayerCombatController` vào `PlayerHealthPresenter.disableOnDeath`; `TrainingDummy.statusText` để trống vì text UI là tùy chọn, console ghi máu và hình nộm ẩn khi hết máu.
- Kiểm tra tĩnh: Roslyn của Unity 6000.4.8f1 biên dịch `Unity.StarterAssets` và `Assembly-CSharp` thành công với `.rsp` dự án và hai script mới; JSON Input Actions hợp lệ; scene có 78 fileID không trùng, prefab có 6 fileID không trùng; `git diff --check` đạt. **Chưa chạy Play Mode, Unity Editor import hoặc Windows build**. Cần thử 3 đòn, giữ/bấm liên tục, thiếu stamina, đánh/né cùng lúc, hụt tầm, tường chắn, collider con và chết giữa đòn. Chưa tạo commit theo điều kiện kiểm tra Play đạt.

### Né trong 91_TestCombat (31/10/2026)

- Đã sao lưu snapshot Git trước khi sửa thành `Temp/Son_Doong_before_dodge_2026-10-31.zip` (659.986.443 byte). Không tạo được commit mốc vì môi trường chặn ghi `.git/index.lock`; bản sao ZIP nằm trong `Temp` và không được Git theo dõi.
- Thay nội dung `ThirdPersonController.cs` và `PlayerVitals.cs` từ `Son_Doong_Dodge_StarterAssets.zip`, giữ nguyên `.meta`/GUID. `ThirdPersonController` thêm né 0,45 giây, 25 thể lực, cooldown 0,65 giây, tốc độ 6 m/s; hướng chốt lúc bắt đầu, một nhánh `CharacterController.Move` mỗi frame, xóa cửa sổ bất tử khi hủy. `PlayerVitals` kiểm tra bất tử theo `[0,10; 0,30)` tại thời điểm nhận sát thương.
- Gói ZIP tham chiếu trực tiếp `PlayerVitals`/`PlayerStamina` từ assembly `Unity.StarterAssets`, trong khi hai class nằm ở `Assembly-CSharp`, nên đã nối bằng `IDodgeVitals`/`IDodgeStamina` khai báo trong chính file motor và triển khai trên hai component. Scene khác thiếu hai component vẫn đi lại bình thường, chỉ không né. Không thêm motor thứ hai.
- `91_TestCombat` có `PlayerVitals` và `PlayerStamina` trên cùng An; đã thêm tham chiếu `ThirdPersonController` vào `PlayerHealthPresenter.disableOnDeath`. Action `Dodge` trong map `Player` của `StarterAssets.inputactions` là Button, binding `<Keyboard>/leftCtrl`, không có interaction. Đã lưu sáu giá trị Dodge đúng bảng yêu cầu trên variant `PF_An_Prototype` để scene kế thừa, giữ GroundLayers và camera hiện có.
- Kiểm tra: biên dịch thành công `Unity.StarterAssets` và `Assembly-CSharp` bằng Roslyn của Unity 6000.4.8f1 với file `.rsp` hiện có, output tạm trong `Temp/DodgePackage`; scene có 73 fileID không trùng; `git diff --check` không báo lỗi; `.meta` không thay đổi. Đây chưa phải biên dịch/reimport trong Unity Editor. **Chưa chạy Play Mode hoặc Windows build**, nên chưa xác nhận va chạm, quãng đường, input, HUD thể lực, cửa sổ bất tử, chết hoặc pause. Theo điều kiện “commit khi kiểm tra đạt”, chưa tạo commit cuối.
### Tự test toàn bộ + Ngày 3 (02/11/2026)

Yêu cầu: tự test toàn bộ (không test được thì ghi lại), rồi làm xong toàn bộ ngày 3 của lịch phần 52: bộ animation, Quà Kim Quy, khóa mục tiêu, hệ Tri thức, khuôn phòng dùng chung, chương 2; kết quả "chương 1 chuyển sang chương 2, chương 2 chơi hết bằng khối".

**Cách test:** bot kiểm thử `Scripts/DevTools/PlaytestBot*.cs` (bọc `#if UNITY_EDITOR`, không vào build). Bot điều khiển An bằng **input giả lập của Input System** như người chơi: đi theo waypoint, tự bước ngang khi vướng, ngắm camera tới khi tia giữa màn hình trúng vật, nhấn E, giữ chuột phải, chém, chạy/lộn né vùng đỏ, lộn né đạn, uống thuốc. Chạy khi đang Play: `PlaytestBot.Run("c1")`, `"c1c2"` (Menu → hết chương 1 → nút "Sang chương tiếp theo" → hết chương 2), `"c2s"` (ghi bản lưu đã xong chương 1 → Tiếp tục → cổng chương ở Đàn Kim Quy → chương 2). Báo cáo ghi ra `Application.temporaryCachePath/playtest_report.txt`. Bot ghi đè `save_v1.json`; đã khôi phục bản lưu gốc của người dùng sau test.

**Kết quả cuối (Editor 6000.4.8f1, Console không lỗi):** `c1c2` ĐẠT toàn bộ chương 1 (115 s bot) và chương 2 tới bước cuối cầu; lần chạy đó dừng 144 s ở sân P4 dù không có vật cản (chạy tay ngay sau đó đi qua bình thường; không tái hiện ở lần chạy `c2s` kế tiếp; nghi Editor bị giảm tốc khi chạy nền). `c2s` ĐẠT toàn bộ chương 2 (212 s bot). Các ca đã đạt:
- Chương 1: Chơi mới; Ctrl không lộn (MV01); nhặt đèn; lửa trại lưu + bảng chế, Esc đóng; Focus xóa phong ấn; mảnh A, nguyên liệu; gõ sai A-B rồi đúng A-C-B (phải bước tới từng trống vì tầm 3 m); mảnh B; hạ 2 Hắc Ảnh bằng đòn thật; xoay gương; mảnh C; lưu `cp_truoc_dan` và không có quái trong 10 m; mở cổng; đấu trường khóa; hạ Hộ vệ bằng chém thật (bot né 11 lần, 0 lần trúng, 28 s); phục dựng, kết chương.
- Chuyển chương: snapshot mang sang (Quà Kim Quy, đèn, túi, tri thức), lưu `c2_cp_start`.
- Chương 2 (đủ 7 ca nghiệm thu của spec C2 mục 5): lộn Ctrl+D đi 3,7 m tốn 25 thể lực; lộn vào tường không xuyên; né đúng lúc (đòn Enemy **bơm trực tiếp** trong khoảng bất tử: không mất máu, chậm thời gian rồi về 1, đèn +15); lộn + chém thành đòn đâm; cột gió không nâng khi chưa có Lông, nâng sau khi nhận; thẻ hướng dẫn đóng bằng Esc; bia ghi Tri thức khi rọi đèn; chế thuốc ở lửa trại chương 2; bệ vấn đáp sai → giải thích, thử lại → nhận mảnh, không có điểm lần đầu, không trao lần hai; đúng ngay → có Điểm Tri Thức; lưu `CP_C2_Mid`; phù điêu hiện khi rọi đèn; chuông sai không bật cột, đúng 2-3-1 bật cột; F9 sau khi lấy mảnh 3 → mảnh 3 mất, chuông chưa giải, đứng ở mốc giữa; nhảy 2 khe cầu; lên 2 gờ bằng cột gió hạ Hắc Ảnh Ném; mốc `c2_cp_arena` an toàn; cổng báo thiếu Lông Gió Lớn rồi mở; rơi khe đỉnh tổ → chết → hồi sinh ở `c2_cp_arena`, cổng mở, đấu trường reset; hạ Chim Lạc (lên cột gió giữa rọi đèn khi chim đậu → choáng → chém; 4 lần choáng, 47 s); F9 sau khi thắng trùm không sống lại; phục dựng; Linh Thần 3 câu; J mở/đóng Nhật ký; cổng chương (chương 3 chưa có) → màn kết chương 2; Tiếp tục mở lại đúng chương 2 (CH01).
- Kiểm tay trong Play: khóa mục tiêu bằng chuột giữa (camera giữ quái, An quay mặt về quái, dấu khóa hiện), tự bỏ khóa khi > 15 m và khi quái chết; lộn chui khe 0,95 m (thanh chắn tạm, đi bộ bị chặn, lộn qua được rồi đứng dậy).

**Lỗi/thiết kế sai tìm được và đã sửa:**
- Mốc lưu không an toàn: `cp_truoc_dan` cách tuần tra `HacAnh_01` 2,5 m; `c2_cp_arena` cách `HacAnh_C2_2` 5 m → hồi sinh là bị đánh ngay (100 → 55). Đã dời `HacAnh_01` tuần tra (±3,5; 64,5), `HacAnh_C2_2` sang đoạn cầu 2.
- Mốc tự động trước đấu trường lưu trước khi nhận Lông Gió Lớn/mở cổng → thua trận phải làm lại bệ P4. `RequirementDoor` giờ lưu tại mốc trước đấu trường khi mở (chương 1 `cp_truoc_dan`, chương 2 `c2_cp_arena`).
- Trùm chết không lưu → F9 dựng lại trùm. `BossBase` lưu khi bị đánh bại; bệ phục dựng và Linh Thần Vấn Đáp chương 2 cũng lưu.
- Cân bằng (spec ghi số là điểm bắt đầu): hai Hắc Ảnh Ném 14 m bắn chéo hai gờ, bot chết 2 lần. Đổi tầm 10 m, nhịp ném 3 s. Cầu P4 vẫn tốn 2 thuốc với bot → **cần người thật thử**.
- Câu gợi ý trống bốn vành theo spec chương 5: Kim Quy "vang cuối cùng", Chim Lạc "đứng thứ nhất".

**Đã làm (ngày 3):**
- **Nền tảng:** `WorldIds` cho chương 1–2 (+ mốc theo scene); ID phục dựng chương 1 đổi thành Puzzles `c1_relic_restored` (spec mục 55.6; bản v4 cũ có Flags `relic_restored` được chuyển khi đọc). `SaveService` dùng chung mọi chương (tham chiếu chương 1 thành tùy chọn, tự tìm `CheckpointAltar`/`CheckpointTrigger` trong scene, `Capture/ApplySnapshot`, kiểm tra mốc thuộc đúng scene, chương 2 cần xong chương 1). `GameSession.LoadChapter` (mang snapshot, lưu ở mốc đầu chương mới), Continue/hồi sinh mở scene trong bản lưu, `RequestSlowMotion`. `CheckpointTrigger`, `ChapterExit` (chương sau chưa có thì hiện màn kết rồi về menu), `StoryCard` SO + `StoryCardPanel.Show`.
- **Quà Kim Quy (spec 43):** `PlayerAbilities`, `DodgeController` (Ctrl + hướng/không hướng, 25 thể lực, 0,45 s, bất tử 0,10–0,30 s, cách 0,65 s, Shift ×1,5, chỉ khi chạm đất và có `movement_kit`; né đúng lúc trong 0,15 s đầu bất tử: chậm 0,3 s, đèn +15; chui khe: capsule 0,9 m, bò tiếp tới khi đứng được). `PlayerMotor`: `BeginDash/CancelDash/SetLow/FacingTarget/DesiredMoveDirection`. `PlayerVitals.DamageAvoided`. `CombatController`: lộn + chém ×1,25 + đâm tới, nhận bấm đệm trong lúc lộn.
- **Khóa mục tiêu:** `LockOnController` (chuột giữa; chọn gần tâm màn hình trong 15 m có đường nhìn; camera ghi đè góc; bỏ khóa khi chết/xa/bị che > 1 s), `EnemyHealth.Active`, `LockOnMarker` trên Canvas.
- **Animation:** gói A10 `Character_Animated` có Roll/Dagger/Death nhưng rig không map Humanoid được (chân rời khỏi cây xương) → không retarget. Tự tạo clip Humanoid bằng đường cong muscle qua `Editor/AnAnimationBuilder.cs` (menu Tools/Son Doong/Build An Animations): `AN_Slash1/2`, `AN_Hit`, `AN_Interact`, `AN_Roll`, `AN_Death`, `AN_LanternHold`; `AC_An.controller` = bản sao Starter Assets (locomotion, nhảy) + layer Actions (mask nửa thân trên), FullBody, LanternArm (mask tay trái, trọng số theo đèn bật). `PlayerAnimator` điều khiển trigger; xoay transform `Skeleton` 360° khi lộn và ngả 80° khi chết (một clip lộn + xoay thân, spec cho phép). Root motion tắt. **Chất lượng là blockout**, chưa đạt "không trượt chân"; cần clip thật (Mixamo/A04) ở giai đoạn đồ họa.
- **Tri thức (spec 44):** `ContentTag`, `LoreDefinition`, `QuizDefinition`, `LoreStele` (hiện khi rọi đèn, E đọc toàn văn), `QuizPanel`, `RewardService`/`RewardEntry`, `QuizPedestal`, `QuizSequence`, `JournalPanel` (J: mục tiêu, bia đã đọc, Điểm Tri Thức = số QuizFirstTry), `ChapterQuestTracker`. Dữ liệu `Data/Knowledge`: 3 bia chương 1 (Sơn Đoòng – VQG Phong Nha–Kẻ Bàng; Đông Sơn – làng Đông Sơn, Thanh Hóa; Kim Quy – Cổ Loa, nhãn Truyền thuyết), 5 bia + 7 câu chương 2 theo bảng phần 46. `verified = true` kèm `sourceRef`; **đối chiếu theo kiến thức chung của AI, người dùng cần duyệt lại nguồn (KN01)**.
- **Khuôn phòng dùng chung:** `Editor/SharedPrefabBuilder.cs` tạo 15 prefab trong `Prefabs/Shared` (bia, bệ vấn đáp, cột gió, mốc lưu altar/trigger, KillZone, tường ranh giới, cửa yêu cầu, cổng chương, bệ phục dựng, vật nhặt, vật năng lực, chuông, đạn, Hắc Ảnh Ném). `Editor/ChapterBuilder.cs`: thêm UI Tri thức, tạo `Scenes/Template_Chapter.unity` từ 02 (rig, HUD, mọi bảng, GameSession với `ChapterQuestTracker`), dựng chương. `Editor/BuilderUtil.cs`.
- **Chương 2 `03_GiengTroiChimLac`** (thêm vào Build Settings): 5 phòng theo spec C2 (tọa độ trong `ChapterBuilder.BuildChapter2`), `WindColumn`, `AbilityPickup`, `SequencePuzzleController/Definition/InputNode` (SQ_C2_Bells 2-3-1), `ThrowerBrain` + `EnemyProjectile`, `BossBase` (+ `IArenaBoss`, `EnemyHealth.DamageFilter`), `BossChimLac` + `ChimLacFocusReceiver`, `ArenaGate` nhận mọi trùm. NavMesh `03_GiengTroiChimLac_NavMesh.asset`. Hắc Ảnh không qua được khe cầu nên mỗi con tuần tra một đoạn (spec ghi Z 42↔62).
- Chương 1: thêm 3 bia, Nhật ký, bảng câu hỏi, `ChapterExit_C1` (sau phục dựng), nút kết chương "Sang chương tiếp theo".

**Chưa kiểm chứng / còn lại:**
- Chưa chơi tay người thật; chưa build Windows. Trận Hộ vệ và Chim Lạc quá dễ với bot (phản xạ tức thì, 0 sát thương) nhưng cầu P4 lại khó → cần người thử để cân bằng.
- Chưa thử: bỏ khóa khi bị che > 1 s, lộn khi khóa mục tiêu, lộn ở FPS thấp (MV03), QZ03 (mở bảng khi đang giữ phím), lộn + chạy ×1,5 đo bằng số, né đúng lúc với đòn địch thật (chỉ bơm trực tiếp).
- Animation chưa nhìn kỹ từng clip; đòn chém chưa chụp được khung hình.
- Không có `WorldIdCatalog` SO và công cụ Validate Persistent IDs (dùng lớp tĩnh `WorldIds`); không có `ChapterContext`; World/Inventory vẫn nằm trong từng scene, chuyển chương bằng snapshot.
- Chương 3 chưa có: cổng chương 2 hiện màn kết và về menu.

### Ngày 2 – hoàn thành phần còn lại: Đàn Kim Quy, Hộ vệ Đá, phục dựng, HUD thanh, túi đồ, chế thuốc (02/11/2026)

Yêu cầu: kiểm tra phần việc ngày 2 còn thiếu, làm xong và test kỹ. Kết quả ngày 2 theo lịch phần 52: **chương 1 chơi hết bằng khối trong Editor** (chưa build Windows).

**Lõi (theo hợp đồng `spec/chapters/00_Nen_tang_dung_chung.md` mục 3, 5):**
- `Core/StateSet.cs`; `WorldStateStore` tổng quát `Has/TryAdd/Count/Export/Restore` (Restore phát `Changed` đúng 1 lần), giữ wrapper cũ `HasRelic/TryCollectRelic/HasSolvedPuzzle/TrySolvePuzzle`. Danh mục ID tạm là lớp tĩnh `Core/WorldIds.cs` (chưa phải SO `WorldIdCatalog`). `Core/StateRequirement.cs`.
- `GameSession`: thêm trạng thái `InMenu`, `ChapterComplete`; `PushUiPause/PopUiPause(IUiPauseOwner)` (timeScale 0, tắt Gameplay, mở chuột; Esc đóng bảng trên cùng); `CompleteChapter()`. `InputReader.GameplayEnabled` thành cờ riêng để bảng UI bật lẻ phím I.
- **Lưu schema 4** (`SaveData`: schemaVersion, buildVersion, timestamp, chapterId, sceneName, checkpointId, inventory, world {sets, ints}, elapsedPlaySeconds). Đọc bản v1–v3 cũ qua `LegacySaveData` và chuyển trong bộ nhớ (hasLantern → Flags `equipment_lantern`, phong ấn → Seals `seal_vuon_01`). Kiểm tra ràng buộc tiến trình (mảnh B/C cần câu đố; cổng cần 3 mảnh + 2 câu đố; Hộ vệ cần cổng; phục dựng cần Hộ vệ; `movement_kit` cần phục dựng; `cp_kim_quy` cần Hộ vệ). Vẫn ghi `save_v1.json`. ID mảnh vẫn `relic_drum_a/b/c` (C1-R11 đổi sang `fragment_*` chưa làm). **F9 giờ nạp lại scene rồi áp bản lưu** (giống Hồi sinh) thay vì áp tại chỗ.

**Túi đồ, thuốc, chế thuốc (phần 17, C1-R08):** `Inventory/ItemDefinition` + 9 SO `Data/Items/ID_*` (lá cẩm, linh chi chồng 10; thuốc máu chồng 5; dao, đèn, 3 mảnh, trống Kim Quy không chiếm ô). `InventoryService` (trên `GameSession` của scene, chưa ở AppRoot) 12 ô, `TryAdd` trả số đã nhận, `CanAdd`, `TryRemove`, `TryCraft` giao dịch trên bản sao. `ItemPickup` (7 vật: `pickup_la_cam_01..03`, `pickup_linh_chi_01..03`, `pickup_thuoc_mau_01`; túi đầy thì vật ở lại). `PotionUser` trên An (Q, +35, không dùng khi đầy máu/chết, hồi 2 s), `PlayerVitals.Heal`. Lửa trại (`CheckpointAltar.craftingPanel`) lưu rồi mở `CraftingPanel`; chế thành công thì lưu lại. `InventoryPanel` (I mở/đóng, Esc đóng, chỉ xem, có mục trang bị và mảnh).

**HUD (C1-R07):** `HudBars` trong `Canvas_Gameplay/HUD_Bars`: thanh máu, thể lực, đèn (ẩn khi chưa có đèn), số thuốc. Gỡ `PlayerHealthPresenter`/`PlayerStaminaHUD` khỏi Canvas 02 và tắt `TXT_PlayerHealth/Stamina`; `TXT_LanternEnergy` chỉ còn dòng gợi ý. Thanh máu Hộ vệ `BossBar`.

**Khu 5 Đàn Kim Quy (C1-R04..R06), scene 02:** mở tường bắc Hành Lang Quang (Z=82, khe 4 m); `World_Geometry/Area_KimQuy`: lối nối Z=82–90, đấu trường 18×18 m (Z=90–108) có trần; 3 đèn mới; KillZone kéo tới Z=145. `Gameplay/Zone_KimQuy`:
- `Gate_DanKimQuy` (`RequirementDoor`, E mở khi đủ 3 mảnh + 2 câu đố, lời nhắc liệt kê thứ thiếu, lưu `door_dan_kim_quy`).
- `ArenaTrigger` (`ArenaGate`, trigger Ignore Raycast tại Z 92–94) bật `Gate_ArenaLock` và đánh thức Hộ vệ; mở khi `guardian_01` có trong Bosses. Thua trận → nạp lại scene, về `cp_truoc_dan` (giữ nguyên chỗ cũ (−7, 0, 78)).
- `HoVeDa_Guardian` (`AI/StoneGuardian` + `EnemyHealth`/`EnemyLanternReceiver` dùng `Data/Enemies/ED_HoVeDa` 180 HP): di chuyển bằng transform trong biên đấu trường; quét tay ≤2,6 m (báo 0,8 s tấm đỏ phía trước, 20 dmg, hồi 1,2 s), nện đất ≤7 m (vòng đỏ bán kính 3 m đặt tại vị trí An lúc báo, 1,2 s, 25 dmg, hồi 1,5 s, hồi chiêu 3 s), kiểm tra vật cản Environment, mỗi đòn trúng 1 lần. Chết: ghi `guardian_01`, ẩn sau 1 s; tải lại không sống lại.
- `Altar_TrongDong` (`Puzzles/RelicAltar`): đủ 3 mảnh + Hộ vệ chết → cờ `relic_restored`, Abilities `movement_kit` (Quà Kim Quy, chưa có gameplay lộn – ngày 3), lưu `cp_kim_quy`, `StoryCardPanel` kết chương (nút "Về menu chính" vì chưa có scene chương 2).
- `CP_KimQuy` (`CheckpointAltar` với `requiredBossId = guardian_01`).
- Vật liệu mới `Materials/Blockout/M_BO_Floor/Wall/Interact/Boss/Relic/Herb/Mushroom/Potion`. `QuestTracker` thêm bước cổng → Hộ vệ → phục dựng → hoàn thành.

**Kiểm chứng Play Mode (Editor, input giả lập qua Input System; một số bước gọi hàm trực tiếp, ghi rõ):** bản v3 cũ của người dùng đọc hợp lệ; nhặt Linh chi bằng ngắm + E; lửa trại E → lưu v4 + mở bảng chế, thiếu lá cẩm báo "Thiếu Lá cẩm (0/1)", Esc đóng và trả quyền điều khiển không bật nhầm Pause; chế thành công (trừ 1+1, +1 thuốc, file lưu có thuốc); Q hồi 50→85 và trừ thuốc; I mở/I đóng túi, chồng 10 đúng; cổng báo thiếu "Mảnh Trống C" rồi E mở; đi bộ qua trigger thì khóa cổng, Hộ vệ đánh An (85→35 rồi chết); Hồi sinh về đúng mốc (trại / `cp_truoc_dan`), cổng Đàn giữ mở, đấu trường mở khóa, Hộ vệ đầy 180 máu; đèn chiếu làm yếu Hộ vệ; chém dao trúng 180→160→140 (phần máu còn lại gây bằng `TryTakeDamage` phe Player); đòn phe Enemy không trừ máu Hộ vệ; Hộ vệ chết → cổng mở, bệ sáng, `cp_kim_quy` mở khóa, nhiệm vụ đổi; E tại bệ → ChapterComplete, thẻ kết, bản lưu `cp_kim_quy` đủ cờ; Về menu → Tiếp tục: đứng tại `cp_kim_quy`, Hộ vệ không sống lại, bệ không trao lại. Ca biên túi (gọi trực tiếp): đầy túi thì chế thất bại và túi giữ nguyên; nguyên liệu dùng hết làm trống ô thì thuốc vào được; vật nhặt ở lại khi túi đầy; bản lưu quá chồng/không phải vật phẩm túi bị bỏ. Bản v3 có tiến trình (Điện Trống, A+B) + F9 → nạp đúng. Không lưu được `cp_kim_quy` khi Hộ vệ còn sống (lỗi Console duy nhất trong test là ca cố ý này). Hắc Ảnh vẫn tuần tra/gác. Đã khôi phục `save_v1.json` gốc của người dùng sau test.
- **Chưa kiểm chứng:** chơi tay toàn tuyến; né đòn nện bằng chạy (chỉ kiểm bằng logic: vòng cố định tại vị trí lúc báo); cân bằng Hộ vệ (trong test An đứng yên bị hạ nhanh, cần người chơi thử 1–3 phút/trận); NavMesh chưa bake lại (khu 5 không có Hắc Ảnh); build Windows.
- **Còn lại sau ngày 2:** C1-R11 đổi ID mảnh; `WorldIdCatalog` SO và công cụ Validate Persistent IDs; Inventory/World chuyển lên AppRoot khi có nhiều chương; khóa mục tiêu, animation, Quà Kim Quy có hiệu lực, Nhật ký (ngày 3); âm thanh.

### Ngày 2 – phần 2A, 2B: đánh dao và Hắc Ảnh (02/11/2026)

- **2A** `Combat/CombatController.cs` trên `PF_An_Prototype`: chuột trái, đòn 0,65 s, sát thương trong 0,15–0,30 s (xét giao khoảng frame nên FPS thấp vẫn trúng), OverlapCapsule tầm 1,8 m bán kính 0,6 trên layer Enemy/Hurtbox, mỗi mục tiêu trúng một lần mỗi đòn, 20 sát thương, tốn 10 thể lực, hủy khi chết. `SlashFX` (khối cam) hiện trong cửa sổ sát thương. `DamageInfo` có `AttackId`. Chưa có animation đánh (blockout).
- **2B** `AI/EnemyDefinition` (SO `Data/Enemies/ED_HacAnh`: 60 HP, 15 dmg, 2/3,5 m/s, 10 m, 120°, 1,7 m, hồi 1,6 s, leash 18 m), `EnemyHealth` (chỉ nhận đòn phe Player, nhân hệ số làm yếu), `EnemyLanternReceiver` (Focus 1 s → ×1,5 trong 4 s, đổi màu vàng), `EnemyBrain` (Idle/Patrol/Chase/Attack/Search/Return/Dead, NavMeshAgent, báo trước 0,4 s bằng tấm đỏ, kiểm tra LOS trước khi gây sát thương, đứng yên khi không Playing hoặc An chết, bị đánh thì quay lại đuổi). Prefab `Prefabs/Enemies/PF_HacAnh` (khối tím, layer Enemy, LanternHitbox layer LanternTarget). Vật liệu `Materials/Blockout/M_BO_Enemy/Telegraph/Slash`.
- Scene 02: NavMeshSurface trên `World_Geometry` (collider Environment), dữ liệu `Scenes/02_HangLinhMach_NavMesh.asset`; `Gameplay/Enemies`: `HacAnh_01` tuần tra (−6, 65)↔(−6, 74), `HacAnh_02` đứng gác (7, 0, 67).
- **Kiểm chứng Play Mode (Editor):** quái phát hiện và đánh An (100 → 70 sau 2 đòn); chém xa 4 m không trúng; chém sát trừ đúng 20 (60 → 40); Focus làm yếu đổi trạng thái; khi bị làm yếu 20 sát thương thành 30; về 0 thì chết và biến mất; đòn phe Enemy không làm quái mất máu; bị đánh từ sau lưng thì chuyển Chase. Console không lỗi. Chưa thử leash/Return dài, nhiều quái cùng lúc, bằng tay người chơi.
- Chưa làm: lưu trạng thái quái (theo thiết kế quái thường không lưu, tải lại thì về điểm sinh — đúng nhờ nạp lại scene), animation, âm thanh. Tiếp theo: 2C Đàn Kim Quy, 2D Hộ vệ Đá, 2E phục dựng.

### Ngày 1 lịch phần 52: nền tảng phiên chơi chương 1 (01/11/2026)

**Đã làm (code + cấu hình, đã chạy Play Mode trong Editor qua MCP, input giả lập bằng Input System):**

- **C1-F01 một bộ input:** `Assets/_Project/Input/PlayerControls.inputactions` (map Gameplay đủ phím phần 10 + DebugLoad F9; map UI từ template), đặt làm project-wide actions. `InputReader` (trên AppRoot) là nơi duy nhất đọc input; Gameplay chỉ bật khi đang chơi. Đã xóa `Assets/InputSystem_Actions.inputactions` (không còn tham chiếu). `StarterAssets.inputactions` trong gói A01 không còn được dùng.
- **C1-R01 PlayerMotor:** thay `ThirdPersonController`, `StarterAssetsInputs`, `PlayerInput` trên `PF_An_Prototype` bằng `PlayerMotor` + `PlayerCameraController`; thêm `PlayerVitals`, `PlayerStamina` vào prefab. Capsule 1,8 / 0,3 / tâm 0,9 / bậc 0,3. Đi 3,5, chạy 5,5 m/s tốn 8 thể lực/giây, Focus 2 m/s, nhảy 1,2 m, trọng lực −20. Dịch chuyển dọc theo vận tốc trung bình trong frame nên độ cao nhảy đúng cả khi frame dài 0,33 s (đã đo 1,22 m). Có sẵn `RequestLift`, `AddExternalVelocity`, `Teleport`, `IsHovering` cho chương 2–5. Pitch camera −35..65°.
- **Phiên chơi:** `AppRoot` tự tạo trước scene đầu (Settings, InputReader, GameSession, `Resources/PF_AppRootUI`). `GameSession`: Boot → Menu → Loading → Playing ↔ Paused, Dead → Respawn; Esc tạm dừng/đóng lớp trên; chết mở màn "Bạn đã gục ngã" (Hồi sinh tại mốc lưu / Về menu). Respawn nạp lại scene rồi áp bản lưu. `00_Boot` (BootLoader) và `01_MainMenu` (Chơi mới có hỏi xác nhận khi đã có bản lưu, Tiếp tục chỉ bật khi có bản lưu hợp lệ, Cài đặt, Thoát). Settings PlayerPrefs: âm lượng tổng, độ nhạy chuột, toàn màn hình. Một EventSystem duy nhất trong AppRootUI; đã bỏ EventSystem ở 02, 90, 91.
- **Mốc lưu:** `CheckpointAltar` thay `CampCheckpoint` (đã xóa). 02 có `cp_trai` (lửa trại), `cp_dien_trong` (−5, 0, 48), `cp_truoc_dan` (−7, 0, 78, tạm ở cuối Hành Lang Quang tới khi có Đàn Kim Quy). `SaveService` lưu/tải theo checkpointId, hồi đầy máu/thể lực/đèn, báo "Đã lưu tiến trình", F9 chỉ trong Editor/Development. `cp_kim_quy` chờ khu 5 (ngày 2).
- **C1-F03:** layer `Hazard` (12) chỉ va Player; `KillZone` dưới 02 và 91; tường ranh giới 4 phía ở 91. **C1-F02:** bật Avoid Obstacles (bán kính 0,2) ở 90. **C1-F07:** Build Settings = 00, 01, 02.
- **C1-F04:** chữ tiếng Việt có dấu trong mọi script hiển thị; HUD máu/thể lực ở 02; dòng trạng thái phong ấn/trống/gương chỉ hiện khi An ở gần (`ProximityVisibility`); đã tắt bảng chết cũ ở 91. **C1-F05:** nhãn A/B/C to 0,8 m ở mặt trên và mặt hướng lối vào; gợi ý "Thứ tự gõ A → C → B" chỉ hiện khi đèn bật trong 6 m (`LanternRevealSurface`). **C1-F08:** `DrumPuzzleController` không còn reset lượt gõ khi world state đổi vì lý do khác.
- Nền tảng cho chương sau: `IInteractRange`, `LanternController.AddEnergy/RefillEnergy`, Focus chiếu theo hướng camera, `PlayerVitals.Kill/RestoreFull`.

**Kiểm chứng Play Mode (Editor):** Boot → Menu đúng chữ; Chơi mới hỏi xác nhận rồi xóa save, vào 02 Playing; chạy 5,5 m/s và thể lực giảm ~8/s; nhảy cao 1,22 m; Esc tạm dừng (timeScale 0, UI, chuột tự do) và Esc lần nữa chơi tiếp; nhặt đèn, lưu ở lửa trại, rơi KillZone → Dead + màn chết; Hồi sinh → nạp lại, còn đèn, đứng ở mốc; lưu ở `cp_dien_trong` rồi F9 về đúng mốc; 91: tường chặn ở z ≈ 9,7, vùng đỏ trừ 20 máu; nhãn trống và gợi ý hiện đúng qua ảnh chụp. Console không lỗi. Chưa thử bằng chuột/phím người thật, chưa build Windows (C1-F06 để ngày 7).

**Ghi chú:** Unity tự ghi lại số làm tròn trong 4 vật liệu `EnvironmentTrials/M_Trial_*.mat` khi mở scene 93, và thêm ký tự vào atlas font Noto/LiberationSans Fallback khi dò dấu tiếng Việt; không ảnh hưởng gameplay, có thể hoàn tác bằng git nếu muốn. Chưa commit.

**Lượt test toàn tuyến 02/11/2026 (thay người dùng, Editor, input giả lập):** Boot → Menu → Chơi mới (xác nhận) → đi bộ W tới bệ đèn → E nhặt đèn → E lưu `cp_trai` → giữ chuột phải xóa phong ấn → E mảnh A → trống A-B báo sai về 0, A-C-B bằng E mở cửa → E mảnh B → E xoay gương 1 ba nấc, gương 2 hai nấc → giải → E mảnh C → E lưu `cp_truoc_dan` → Tạm dừng → Cài đặt độ nhạy 0,8 (lưu PlayerPrefs) → Về menu → Tiếp tục (đúng mốc, đủ tiến trình) → chết → Hồi sinh (đúng mốc, máu đầy). Console không lỗi. Đã sửa tên hiển thị trong scene "Manh Trong A/B/C" thành "Mảnh Trống A/B/C".

**Sửa theo phản hồi người dùng chơi thử (02/11/2026):**
- Chuột quá nhạy: độ nhạy mặc định 0,15 (khoảng 0,03–0,6), đổi khóa PlayerPrefs `settings.mouseSensitivity.v2` để bỏ giá trị 0,8 cũ.
- Phong ấn khó mở: người dùng yêu cầu dễ hơn. Nón Focus 90° (theo phương ngang) và cho lệch dọc ±45° so với hướng camera (camera thường chúc xuống); thời gian giữ phong ấn 1,0 s (spec 1,5 s). Đã thử: camera chúc 25°, đứng cách 4 m, mở sau khoảng 1 s.
- Kẹt cạnh gương: khối gương lơ lửng 0,75–2,25 m làm capsule kẹt. Thêm `Base` (Environment) từ sàn tới đáy gương cho Mirror_01/02 và tới đáy ngọc cho Receiver trong `PF_MirrorPuzzle_01`. Đã thử: áp sát rồi đi ngang, lùi được; tia lời giải vẫn tới ngọc.

**Tiếp theo (ngày 2):** đánh dao + thể lực đòn, Hắc Ảnh + NavMesh + đèn làm yếu, khu 5 Đàn Kim Quy + Hộ vệ Đá + phục dựng + `cp_kim_quy`, HUD dạng thanh, túi đồ và chế thuốc.

### Đặc tả triển khai chương 2–5 cho AI Agent (01/11/2026)

- Người dùng xác nhận cách hiểu "P1–P3 lấy mảnh, P4 lấy công cụ" theo phòng trong chương là đúng.
- Đã viết `spec/chapters/00_Nen_tang_dung_chung.md` và `C2_Chim_Lac.md`, `C3_Son_Tinh.md`, `C4_Thuy_Tinh.md`, `C5_Truc_Linh_Mach.md`; thêm phần 55 vào kế hoạch chính. Chỉ viết tài liệu, không sửa code, scene hay asset.
- Đặc tả dựa trên code thật (IInteractable, ILanternTarget, IDamageable, WorldStateStore, SealTarget, PlayerVitals, layer Water = 4). Phát hiện cần đổi khi làm chương 1: WorldStateStore tổng quát theo StateSet, LanternFocusScanner chiếu theo hướng camera, thêm layer Hazard (12), `IInteractRange`, PlayerMotor có `RequestLift`/`AddExternalVelocity`/`Teleport`.
- Điểm tiếp nối: ngày 1 của lịch phần 52 (sửa lỗi chương 1, PlayerMotor, InputReader, Boot/Menu/Pause, hồi sinh), xây đúng API ở mục 3–4 của file nền tảng để chương 2–5 dùng được.

### Chốt kế hoạch toàn game (01/11/2026)

- Người dùng chốt hướng mới và yêu cầu **chỉ viết kế hoạch, chưa triển khai gì**. Đã nối phần 40–54 vào cuối `spec/Ke_hoach_game_Son_Doong_12_tuan.md` (CRLF, bản trước sao lưu ở thư mục scratchpad của phiên). Cập nhật SPEC_INDEX (mục và hash mới) và PROJECT_MEMORY mục 13a.
- Không sửa code, scene, asset hay package trong lượt này. Điểm tiếp nối: chờ người dùng xác nhận các giả định ở phần 54 (đặc biệt cách hiểu "P1–P3 lấy mảnh, P4 lấy công cụ" theo phòng trong chương), rồi bắt đầu ngày 1 của lịch phần 52.

### Chạy thử Play Mode qua MCP for Unity (01/11/2026)

- Kết nối Claude Code với Unity qua MCP for Unity 10.3.0 (HTTP `127.0.0.1:8090`, đăng ký scope user). Package Unity đủ theo §8; Console không có lỗi compile. Không sửa file dự án trong lượt chạy thử.
- `02_HangLinhMach`, Play Mode trong Editor, input giả lập qua Input System (phím E/F9, chuột phải) và đặt An/camera bằng code: **đạt** nhặt đèn qua scanner, lưu tại lửa trại (`save_v1.json` ghi ở persistentDataPath), Focus xóa phong ấn (năng lượng 100→54 sau ~3,8 s), nhặt mảnh A, trống sai A-B reset về 0 có báo sai, đúng A-C-B mở cửa/bệ, nhặt B, xoay gương tới lời giải mở cửa/bệ, nhặt C, quest tới "Tien den Dan Kim Quy", F9 tải lùi khôi phục đúng toàn bộ (mảnh, puzzle, phong ấn, pickup, góc gương, vị trí). Gõ trống và xoay gương gọi `Interact` trực tiếp sau khi xác nhận lời nhắc scanner, nên không kiểm tra tầm 3 m cho từng lần gõ.
- `91_TestCombat`: **đạt** trừ 20 máu khi đi qua vùng đỏ, chết ở 0, khóa PlayerInput/motor, hiện panel chết. **Lỗi/thiếu thấy được:** sàn không có tường/vùng chết nên chạy ra mép rơi vô hạn; Sprint không tiêu thể lực; không có hồi sinh; chữ "BAN DA GUC NGA" lệch phải; robot không có animation chết.
- Quan sát khác: lần đầu mở scene vật thể hiện cyan do shader đang biên dịch bất đồng bộ (tự hết); HUD hiện cùng lúc mọi dòng trạng thái puzzle ở mọi khu, trộn chữ có/không dấu; ký hiệu A/B/C trên trống khó thấy; hết tuyến ở tường cụt sau bệ C vì chưa có Đàn Kim Quy; `00_Boot`, `01_MainMenu`, `92_TestSave` chỉ có camera/đèn/volume; danh sách build còn `SampleScene` không tồn tại. `DrumPuzzleController` nghe `WorldStateStore.Changed` và gọi `ApplySolvedState(false)` nên lượt gõ dở bị reset khi có thay đổi world state khác (rủi ro nhỏ, chưa sửa).
- Console trong lúc thử chỉ có "PlayerLoop called recursively" do lệnh MCP chạy code khi đang Play, không có stack trace từ script dự án. Chưa phải thao tác chuột/phím thật của người chơi, chưa build Windows.

### Thể lực và input Dodge chuẩn bị trong 91_TestCombat (31/10/2026)

- Thêm `PlayerStamina` trên cùng An có `PlayerVitals` trong instance scene thử: tối đa 100, `TrySpend` giao dịch khi đủ, chờ 1 giây từ lần tiêu hao cuối rồi hồi 20/giây, không hồi hoặc tiêu khi chết/pause; có Context Menu thử trừ 25/100. Thêm `PlayerStaminaHUD` vào `CombatTestUI`, `TXT_PlayerStamina` dưới chữ máu và dời chữ đèn xuống để không chồng UI.
- Thêm đúng một action `Dodge` kiểu Button với binding `<Keyboard>/leftCtrl` trong `StarterAssets.inputactions` đang dùng bởi PlayerInput của An. Chưa nối action này với di chuyển, bất tử hay tiêu thể lực; `ThirdPersonController` vẫn là motor duy nhất gọi CharacterController.Move. Không đổi prefab An hoặc scene chính vì stamina hiện chỉ ở instance `91_TestCombat`.
- Kiểm tra tĩnh: JSON Input Actions đọc được, một action/binding Dodge; scene có 72 fileID không trùng và tham chiếu local hợp lệ; `git diff --check` không báo lỗi. **Chưa xác nhận Unity biên dịch hoặc Play Mode**. Cần dùng Context Menu thử 100→75→100, trừ đủ 100, trừ thiếu, đổi mốc hồi theo lần trừ cuối, pause và chết. Chưa commit khi các kiểm tra này chưa đạt.

### Máu, vùng sát thương và chết trong 91_TestCombat (31/10/2026)

- Thêm `DamageInfo`/`DamageTeam`, `IDamageable`, `PlayerVitals` và `DamageZone`. An có 100 máu, sát thương hợp lệ mới trừ, chặn cùng phe, chặn số âm/NaN/vô cực, máu dừng ở 0 và sự kiện chết chỉ phát một lần. Vùng thử gây 20 sát thương mỗi 1 giây, ghi thời điểm theo `PlayerVitals` để nhiều collider không nhân đôi lần trúng; không gây sát thương khi pause.
- Dựng `91_TestCombat` từ rig An, Cinemachine camera, Main Camera, Canvas và EventSystem của scene thử đang dùng; giữ một An, một camera, sàn Environment 20×20, HUD máu, panel chết phủ màn hình. `PlayerVitals` chỉ gắn trên instance An của scene này; `PlayerHealthPresenter` khóa PlayerInput, motor ThirdPersonController, InteractionScanner, LanternController và LanternFocusScanner khi chết. Tạo `PF_DamageZone_Test` màu đỏ với BoxCollider trigger và Rigidbody kinematic, không gravity, tại `(0,0.5,4)`.
- Kiểm tra tĩnh: scene có 66 fileID không trùng, prefab vùng nguy hiểm có 7 fileID không trùng; các tham chiếu local hợp lệ, cấu hình trigger/rigidbody/20 damage/1 s/100 HP hiện diện, Player–Default được bật trong layer collision matrix; `git diff --check` không báo lỗi. **Chưa xác nhận Unity biên dịch hoặc Play Mode** vì Editor đang mở. Cần thử HUD 100→80→60→40→20→0, rời vùng ngừng trừ, chết một lần, khóa di chuyển/tương tác/đèn; chưa nối máu với save, F9 hoặc hồi sinh, chưa làm stamina/né.

### Hành Lang Quang, mảnh C và save v3 trong 02_HangLinhMach (31/10/2026)

- Mở lối bắc Điện Trống tại Z=62, thêm `Area_LightCorridor` với sàn Z=62–82, vách/trần và hai đèn điểm. Đặt instance `Puzzle_Mirror_01` tại `(0,0,72)`; cửa gương chặn khoảng mở 3 m tại Z=80, bệ thưởng phía sau tại Z=81. `Pickup_DrumC` là con scene của `RewardDisplay`, layer Interactable và dùng `RelicPickup` với `relic_drum_c`; marker đồng cũ được tắt trên instance. Thêm `TXT_MirrorStatus` và nối receiver với `WorldStateStore`.
- `WorldStateStore` nhận `pz_guong_01`; `LightPuzzleReceiver` ghi solved vào store, khôi phục cửa/bệ/tiến độ và đưa gương về nấc lời giải hoặc nấc ban đầu khi tải. `SaveData` lên version 3; `SaveService` chuyển v1/v2 hợp lệ trong bộ nhớ, lưu/tải mảnh C và trạng thái gương, bật lại pickup C khi tải lùi. Mảnh C chỉ tương tác được khi gương đã giải; HUD mục tiêu cập nhật đến 3/3.
- Kiểm tra tĩnh: scene 723 fileID không trùng, tham chiếu local mới hợp lệ, có đúng một khu, instance gương, pickup C và status UI; `git diff --check` không báo lỗi. **Chưa xác nhận Unity biên dịch, Play Mode, F9 tải lùi, khởi động Play lại, đường đi/camera hoặc build Windows**. Cần thử ba trạng thái chưa giải / đã giải chưa nhận C / đã nhận C, đặc biệt lưu trước khi giải rồi giải và nhận C không lưu rồi F9. Không commit khi các kiểm tra này chưa đạt.

### Prototype câu đố xoay gương trong 90_TestMechanics (31/10/2026)

- Đã thêm `LightPuzzleReceiver`, `MirrorRotator`, `MirrorBeamEmitter` và `PF_MirrorPuzzle_01`. Prefab có nguồn sáng, hai gương với mặt phản xạ và dấu mặt trước, ngọc nhận, cửa thử và bệ thưởng. Tia dùng Raycast qua Default/Environment/Interactable/PuzzleBeam, chặn mặt lưng và vật cản, giới hạn 3 phản xạ/40 m. Ngọc cần nhận sáng liên tục 1 giây; mất tia trước khi giải thì tiến độ về 0, tạm dừng thì giữ nguyên. Sau khi giải, gương khóa tương tác và cửa/bệ đổi trạng thái.
- Đã đặt instance `Puzzle_Mirror_01` ở gốc `90_TestMechanics`, thêm `TXT_MirrorStatus` trong Canvas, đổi Detection Mask trên `PF_An_Prototype` thành 2313 để thấy gương, tạo ba material URP/Unlit. Gương khởi đầu nấc 1/2; nấc lời giải là 0/0. Dời `Wall_FocusTest` từ X=4,21429 sang X=-7 để collider cũ không chặn đoạn tia cuối theo bố cục mới.
- Kiểm tra tĩnh: prefab 52 fileID và scene 93 fileID không trùng, tham chiếu local mới hợp lệ; tính pháp tuyến/đường phản xạ tâm cho 16 tổ hợp chỉ cho cặp 0/0. `git diff --check` không báo lỗi. **Chưa xác nhận Unity biên dịch hoặc Play Mode**; cần thử tia thực tế, 16 cặp nấc, tường chắn, mặt lưng, reset tiến độ, pause và hiển thị UI trong Editor. Chưa nối scene chính, mảnh C, WorldStateStore hoặc save.

### Điện Trống, mảnh B và save v2 trong 02_HangLinhMach (31/10/2026)

- Theo bước người dùng gửi, mở vách bắc Vườn Rêu thành cửa 4 m, thêm `Area_DrumHall` với sàn hành lang Z=40–46, sàn Điện Trống Z=46–62, vách bao, trần và hai đèn điểm. Vách ngăn ở Z=57 ghép sát `Door` rộng 3 m, có dầm trên cửa để không đi vòng/nhảy qua; kích thước vẫn là blockout cần kiểm tra bằng An và camera.
- Đặt instance `PF_DrumPuzzle_01` tại `(0,0,54)` trong Gameplay; override trống A/B/C ở X=-3/0/3, Z=-1, Door `(0,1.5,3)` scale `(3,3,0.5)`, RewardDisplay Z=6. Dời bảng gợi ý lên mặt trước vách ngăn ở Z=56,25 để không bị tường che. Thêm `TXT_DrumPuzzleStatus` trên Canvas và gán WorldStateStore của GameSession. Giữ scene `90_TestMechanics` nguyên trạng.
- Thêm `Pickup_DrumB` làm con scene của RewardDisplay, active trong Edit Mode, layer Interactable, BoxCollider thường và `RelicPickup` với `relic_drum_b`; tắt khối marker B cũ ở instance màn chính. `RelicPickup` chỉ cho nhặt B khi `pz_trong_01` đã solved.
- `WorldStateStore` giữ tập puzzle solved và khôi phục đồng thời relic/puzzle bằng một event; controller ghi solved trước khi mở cửa và áp lại cửa/bệ từ WorldState khi tải. `SaveData` lên version 2, `SaveService` lưu cả hai tập, khôi phục mảnh B và chuyển bản lưu v1 hợp lệ thành v2 trong bộ nhớ; bản v2 vẫn dùng `save_v1.json` để đọc lại slot cũ. Validation chỉ chấp nhận A/B và ràng buộc B cần puzzle solved. QuestTracker chuyển mục tiêu theo câu đố và mảnh B.
- Kiểm tra tĩnh: scene có 654 fileID không trùng, 4.283 tham chiếu local đều tồn tại; có đúng một Area_DrumHall, một instance puzzle, một Pickup_DrumB và một status text; `git diff --check` không báo lỗi. Thử `dotnet build` trên csproj do Unity tạo không chạy được vì thiếu `Temp/obj/Assembly-CSharp/project.assets.json`; `dotnet restore` cũng thất bại không có chẩn đoán chi tiết. **Chưa xác nhận Unity biên dịch, Play Mode A-C-B, đường đi/camera, nhặt B, lưu tại trại/F9 hay build**. Cần mở lại scene từ đĩa trong Editor, đợi compile xong rồi chạy các ca này.

### Prototype câu đố Trống Đồng trong 90_TestMechanics (31/10/2026)

- Theo tệp hướng dẫn người dùng gửi, thêm `DrumPuzzleDefinition`, `DrumPuzzleController`, `DrumInteractable`, `DrumPuzzleReset`, `DrumVisualLabel`; asset `DP_Drum_01` có chuỗi A → C → B, input delay 0,2 s và failure delay 0,7 s. Tiến độ chỉ ở runtime; `ApplySolvedState(bool)` đã có để nối save trong bước sau, chưa nối với `SaveService` hiện tại.
- Tạo `PF_DrumPuzzle_01` với ba trống Cylinder cách 2 m, layer Interactable và collider thường; ba màu vàng/xanh/đỏ, nhãn 3D TextMeshPro được tạo khi Play, hit indicator không collider, bảng gợi ý `A -> C -> B`, Door có collider, RewardDisplay ban đầu tắt và ResetPedestal tương tác E. Âm thanh để trống vì là tùy chọn; bệ thưởng chỉ là marker, chưa cấp mảnh B.
- Đặt instance tên `Puzzle_Drum_01` tại `(-4,0,2)` trong `90_TestMechanics`; thêm `TXT_DrumPuzzleStatus` vào Canvas, neo trên giữa, nối với controller instance. Scene vẫn dùng An/camera/InteractionScanner/sàn đã có. Prefab được để riêng để sau này đưa vào Điện Trống.
- Kiểm tra tĩnh YAML: prefab có 71 fileID không trùng, scene có 87 fileID không trùng; tham chiếu local, asset định nghĩa, status text và prefab instance có mặt; `git diff --check` không báo lỗi. **Chưa xác nhận Unity biên dịch hoặc Play Mode** vì Editor đang mở; cần mở lại scene từ đĩa và thử A-C-B, A-B, A-C-A, nhấn E nhanh/giữ E, ResetPedestal, tầm 3 m, cửa và bệ thưởng. Không đánh dấu gameplay hoàn thành trước kiểm chứng này.

### F9 tải bản lưu chưa có đèn làm vật nhặt không tương tác được (31/10/2026)

- Nguyên nhân xác nhận từ code: `LanternPickup.Interact` đặt `collected = true` rồi tắt GameObject. Khi F9 tải snapshot `hasLantern = false`, `SaveService` chỉ bật lại GameObject; cờ private vẫn `true`, nên `CanInteract` luôn trả về false.
- Thêm `LanternPickup.ApplySavedState(bool owned)` để đặt lại `collected` đồng bộ với snapshot và bật/tắt vật. `SaveService` lấy component từ object đã gán và gọi phương thức này khi tải; đồng thời kiểm tra component tồn tại khi khởi tạo.
- Đã rà soát thứ tự restore: `LanternController.ApplySavedState(false)` chạy trước khi bật vật nhặt, nên `CanInteract` có thể trả về true; tải bản lưu đã có đèn vẫn giữ vật tắt. Chưa xác nhận Play Mode trong Unity.

### Không ngắm chọn được vật nhặt đèn (31/10/2026)

- Người dùng làm rõ rằng lỗi là không chọn/nhặt được `PF_LanternPickup`, không phải Focus vào phong ấn. Tìm thấy `InteractionScanner` trên An tham chiếu action `Interact` của `Assets/InputSystem_Actions.inputactions`, trong khi `PlayerInput` của An dùng `StarterAssets.inputactions`. Scanner kiểm tra `action.enabled` trên asset khác, nên không đọc phím E của An và không hiện lời nhắc mục tiêu.
- Sửa `InteractionScanner` lấy `Interact` trực tiếp từ `PlayerInput.actions` của cùng An trong `Start`; bỏ tham chiếu `InputActionReference` cũ khỏi `PF_An_Prototype`. `StarterAssets.inputactions` có action `Interact` dạng Button và binding `<Keyboard>/e`.
- Mở rộng BoxCollider thường của `PF_LanternPickup` từ kích thước local 1×1×1 thành 2×1,5×2, tâm Y=0,25. Với scale scene 0,3×0,4×0,3, vùng ngắm khoảng 0,6×0,6×0,6 m, đáy Y≈0,85 cao hơn mặt bệ Y=0,8; vật vẫn phải ở trong tầm tương tác 3 m và không bị vật cản che.
- Kiểm tra tĩnh action, prefab, scene. Chưa xác nhận Play Mode vì Unity Editor đang mở; cần mở lại scene từ đĩa, tiến đến bệ, ngắm vật và nhấn E để xác nhận lời nhắc và trạng thái sở hữu đèn.

### Không Focus được phong ấn bằng đèn (31/10/2026)

- Kiểm tra tĩnh cho thấy `Focus` trong `StarterAssets.inputactions` là chuột phải, `LanternController` và `LanternFocusScanner` cùng trên An; `Seal_CampExit` có `Barrier` layer `LanternTarget` và `AimPoint` ở trước mặt phong ấn. `visibilityMask` trên `PF_An_Prototype` lại chứa cả `Default`, `Environment`, `Interactable`, `LanternTarget` (`1289`), khiến collider vật tương tác hoặc mặc định có thể chặn tia Focus.
- Đổi `visibilityMask` của prefab An thành chỉ `Environment` (`8`, theo `ProjectSettings/TagManager.asset`), đúng luật LOS của đèn trong spec. `targetMask` vẫn là `LanternTarget` (`1024`). Scene `02_HangLinhMach` thừa kế cấu hình prefab này; không có override mask trong scene.
- Chưa xác nhận Play Mode vì Unity Editor đang mở. Cần mở lại scene từ đĩa, nhặt đèn bằng E, đi tới cách phong ấn dưới 8 m, giữ chuột phải 1,5 giây và quan sát `TXT_SealProgress` tăng. Nếu HUD không hiện `Dang Focus`, kiểm tra trạng thái đèn/action trước khi xét LOS.

### Checkpoint trại và lưu/tải prototype (31/10/2026)

- Theo tệp bước tiếp theo do người dùng gửi, thêm `WorldStateStore.ExportRelics/RestoreRelics`, `LanternController.ApplySavedState`, `SealTarget.IsSolved/ApplySavedState`. Restore thay tập ID, reset Focus/progress dở và không phát lại phần thưởng phong ấn.
- Thêm `SaveData` version 1 cho `cp_trai`, `SaveService` ghi `save_v1.json` tại `Application.persistentDataPath` qua `.tmp`, đọc lại file tạm, thay file chính và tạo `.bak` khi ghi đè. Khi tải ưu tiên file chính, thử backup nếu không hợp lệ; hiện chỉ chấp nhận mảnh A và logic đoạn Trại → Vườn Rêu. Save thành công hồi năng lượng đèn; load đặt An lại checkpoint và khôi phục đèn, phong ấn, hai pickup.
- Thêm action `LoadGame` phím F9 vào `StarterAssets.inputactions` mà `PlayerInput` của An dùng. `SaveService` lấy `PlayerInput` và `CharacterController` từ cùng object với `LanternController` nếu hai ô Inspector chưa gán, vì chúng được thừa kế từ prefab Starter Assets.
- Trong `02_HangLinhMach` thêm `Checkpoint_CampSpawn` tại `(0,0.2,-5)` dưới Gameplay; gắn `SaveService` vào GameSession và `CampCheckpoint` lên `Campfire_Placeholder` để dùng collider `Fire_OrangeMarker` (layer Interactable) khi ngắm và nhấn E.
- Kiểm tra tĩnh: scene có 556 ID không trùng và không thiếu tham chiếu local; JSON Input Actions hợp lệ, `Player/LoadGame` là Button có binding `<Keyboard>/f9`. Chưa có xác nhận Unity biên dịch, Play Mode lưu/tải, backup lỗi, hay build Windows. Cần mở lại scene từ đĩa trong Editor đang chạy trước khi kiểm tra để tránh ghi đè bởi bản scene cũ.

### Mảnh Trống A, HUD mục tiêu và camera hành lang (31/10/2026)

- Theo tệp bước tiếp theo do người dùng gửi, thêm `GameSession` root identity trong `02_HangLinhMach`; gắn `WorldStateStore` giữ tập ID `relic_drum_a/b/c` trong lượt chơi hiện tại và `QuestTracker` suy mục tiêu từ đèn/mảnh trống. ID do yêu cầu mới quy định; chưa có save/Boot và chưa chuyển đổi ID từ thiết kế `fragment_a`.
- `FragmentA_LocationMarker` trở thành `Pickup_DrumA` tại `(0,1.27,28)`, layer Interactable, BoxCollider thường và `RelicPickup` tham chiếu WorldStateStore; `FragmentA_Pedestal` không đổi. Thêm `TXT_QuestObjective` vào `Canvas_Gameplay`, neo trên phải và nối với QuestTracker. QuestTracker trỏ tới LanternController của An trong `PrototypeRig`.
- Tìm thấy `PlayerFollowCamera` trong scene override `AvoidObstacles.Enabled = 0` dù mask có Environment. Đã bật lên, đặt radius 0,2 m và near clip MainCamera 0,1 m. Thêm `Ceiling_Passage` có collider Environment, đáy Y=4, che tầm nhìn ra ngoài khi nhìn lên và không cản lối đi.
- Kiểm tra tĩnh sau sửa: 550 ID scene không trùng, không thiếu tham chiếu local; script GUID khớp `.meta`, các tham chiếu WorldState/Quest/đèn/UI và camera override có mặt. Chưa kiểm tra biên dịch trong Unity, Play Mode nhặt E/focus, camera khi xoay ở mọi vị trí, hay build. Unity Editor đang mở; cần mở lại scene từ đĩa trước khi Play nếu scene đang giữ bản cũ.

### Dựng Trại → Hành lang → Vườn Rêu trong 02_HangLinhMach (31/10/2026)

- Theo yêu cầu mới đã đổi ba Empty `Floor_Camp`, `Floor_Passage`, `Floor_MossGarden` thành sàn Cube có BoxCollider thường, layer Environment, lần lượt 20×1×20 tại z0, 4×1×10 tại z15 và 20×1×20 tại z30; mặt trên Y=0. Tạo vách bao trại/vườn chừa cửa giữa, hai vách hành lang tại x±1,75, thêm trụ cửa và dầm cao chống đi vòng hoặc nhảy qua phong ấn.
- Giữ An có camera, UI, scanner và đèn đã cấu hình trong `PrototypeRig`, chuyển rig vào Gameplay, dời An tới `(0,0.2,-5)`; bỏ instance An thứ hai chưa cấu hình. `Spawn_Camp` là dấu bố trí, không checkpoint.
- Đổi placeholder đèn thành `Camp_Interactables`, đặt `LanternPedestal` và instance `PF_LanternPickup` trên bệ; tắt khối vàng giả. Đặt `Seal_CampExit` từ `PF_SealPrototype` tại z18 và nối `progressText` tới `TXT_SealProgress`. Bệ và khối đồng ở Vườn Rêu chỉ đánh dấu vị trí mảnh Trống A, chưa là pickup/nhiệm vụ.
- Trang trí lối đi bằng 10 cây, 14 đá, 18 bụi và 18 cụm cỏ từ prefab variant A09/A11 đã thử; thêm đá bên lối, đá viền đường, vật liệu nền hang có texture và nền Vườn Rêu nhuộm xanh. Asset gốc không bị chỉnh. Vách dùng vật liệu A09 đã thử. Có thêm 5 đèn tại lửa trại/bệ đèn/phong ấn/vườn/bệ mảnh trống và giảm Directional Light để có tương phản hang. Phần trang trí này là bản dựng cảnh, chưa được nghiệm thu mỹ thuật/performance.
- Đã kiểm tra tĩnh YAML: 534 object ID không trùng, tham chiếu local không thiếu, ba sàn có BoxCollider, một An, prefab pickup/phong ấn và tham chiếu UI. Unity Editor của dự án đang mở nên chưa chạy Editor batch mode; chưa có ảnh Game View, Play Mode để xác nhận raycast, đường đi, camera, mỹ thuật, FPS hoặc build. Cần mở lại scene từ đĩa trong Editor trước khi Play để tránh ghi đè bởi scene đang mở.

### Kiểm tra Pickup_Lantern trong 90_TestMechanics (30/10/2026)

- `Pickup_Lantern` đã có BoxCollider, layer Interactable và `LanternPickup`; `PF_An_Prototype` có `InteractionScanner` và scene có `LanternController` gắn cùng object An. `LanternPickup.CanInteract` chỉ cho nhặt khi controller đang bật và chưa sở hữu đèn.
- Vị trí cũ của pickup `(5.56, 1.18, 0.2)` cắm nhẹ vào `Block_High` (đáy pickup y≈0,98 trong khi mặt khối y=1), dễ bị raycast của scanner chạm khối trước. Đã chuyển pickup sang `(4.5, 0.2, 1.5)` ở chỗ thoáng; giữ nguyên script và các object khác.
- Đã kiểm tra tham chiếu script/layer/action và tọa độ scene dạng text. Chưa có kiểm chứng Play Mode cho thao tác ngắm và nhấn E; cần kiểm tra trực tiếp trong Editor.

Yêu cầu hiện tại: **nhập và thử môi trường theo từng đợt**, đúng mã A09/A11, hoãn A10 đến Điện Trống và giữ A12 làm phương án thay thế. Đã chuẩn bị khu thử, xem [ENVIRONMENT_ASSET_TRIALS.md](ENVIRONMENT_ASSET_TRIALS.md).

Yêu cầu trước đó về đọc spec và ghi nhớ đã hoàn thành: đã đọc đủ bốn file nguồn, tạo AGENTS và tài liệu trong docs, không sửa game trong lượt đọc. Giới hạn “không chỉnh code gì thêm” áp dụng cho lượt đọc tài liệu đó; yêu cầu mới đã cho phép xử lý cảnh báo. Backlog gameplay phía dưới vẫn chỉ là ghi nhận.

## Cập nhật thử môi trường theo đợt

- Bốn gói đã có, không nhập lại. Thư mục nguồn đã được người dùng tổ chức lại: A09 ở `Assets/A09 - Mines and Cave Set/MInesAndCaveSet/`, Starter Assets ở `Assets/A01 - First Person + Third Person  Character Controllers/`. Dùng GUID cho prefab camera để chịu được đổi đường dẫn.
- Có scene mới `Assets/_Project/Scenes/93_TestEnvironment.unity`, chứa An, camera theo PlayerCameraRoot/va chạm Environment, nền an toàn, vách bao, đường đi và mốc cao1,8m. Không thêm scene thử vào build chính.
- Đợt1 thử3 prefab A09 nền/đá/vách; đợt2 thử3 model A11 NormalTree_1/Bush_Small/Grass_Small. A11 là bộ thiên nhiên chủ đạo của khu thử, chưa trộn A12; A10 dành cho di tích sau.
- Có6 prefab cấu hình riêng trong `_Project/Prefabs/Environment/A09_Trials` và `A11_Trials`; material riêng URP/Lit trong `_Project/Materials/EnvironmentTrials`. Giữ nguồn. Cây cao4,8m có capsule thân; bụi0,8m/cỏ0,35m không collider. A09 giữ collider nguồn.
- Ảnh kiểm tra lần đầu phát hiện mẫu A09 không hiển thị như mong đợi dù shader nguồn được báo hỗ trợ. Đã gán texture màu sang URP/Lit riêng cho prefab thử; ảnh sau hiển thị đủ mẫu, không hồng. Không sửa ORM graph nguồn.
- CharacterController của An đã qua phép thử Editor: đi quanh6/6 mẫu và đúng chính sách va chạm6/6. Đây là kiểm tra tự động trong Editor, **chưa phải Play Mode dùng chuột/phím, kiểm tra camera động hoặc build**.
- Báo cáo và ảnh đã lưu trong `docs/reports/EnvironmentAssetTrials_2026-10-30.json` và `docs/images/environment-trials-2026-10-30.png`.
- **Điểm tiếp nối:** người dùng mở93_TestEnvironment và chạy Play Mode theo checklist trong tài liệu. Chưa đưa asset thử vào02_HangLinhMach trước khi lượt này ổn; không tự coi việc đặt model là nghiệm thu cảm giác chơi.

## Cập nhật xử lý cảnh báo Unity

- Đã gỡ `com.unity.ai.assistant` theo lựa chọn của người dùng; cập nhật manifest/lock và bỏ dependency `com.unity.2d.sprite` không còn được dùng trong đồ thị. Giữ AI Inference.
- Đã sửa polygon trong 28 file A10 (FBX/Blend/OBJ/Preview), giữ tọa độ, transform và GUID; đã sửa tên node/mesh trùng trong 5 file FBX/DAE A12.
- Vật liệu hang dùng ORM Shader Graph URP, vật liệu nhân vật dùng URP/Lit và được Unity xác nhận hỗ trợ. Log converter bỏ qua shader không đồng nghĩa shader bị hỏng; không đổi hàng loạt các shader hợp lệ.
- Force reimport trong **Editor dự án chính, Unity 6000.4.8f1**: kiểm tra41 file,467 tài nguyên con/330 Mesh, **0 thông báo warning/error khi import và0 lỗi kiểm tra**. Không còn mesh rỗng/tên mesh trùng trong33 file; bounds của mesh có cùng tên trước/sau không đổi trong ngưỡng0,0001. Chưa kiểm thử hình ảnh/Play Mode/build cho asset mới.
- Bản gốc33 model, metadata và package files nằm ngoài Assets tại `SourceAssets/ImportFixBackups/2026-10-30/`, có manifest SHA-256 trước/sau. Không ghi đè hoặc xóa backup này khi làm việc tiếp.
- Có công cụ Editor `Assets/_Project/Editor/AssetImportDiagnostics.cs`, menu Tools/Son Doong/Validate Selected Imported Assets. Đây là code kiểm tra Editor, không phải triển khai các hệ gameplay trong Scripts.
- Phát hiện bộ A10 Ultimate Modular Ruins, A11 Ultimate Stylized Nature và A12 Nature Kit đã có trong Assets; một số nguồn Blend/License còn nằm trực tiếp ở Assets. Chưa coi toàn bộ các gói này là đã nghiệm thu mỹ thuật/gameplay.
- Các scene, logic gameplay và các việc còn thiếu dưới đây không được bổ sung trong lượt sửa cảnh báo.

## Môi trường hiện tại

| Nội dung | Giá trị đọc từ dự án |
| --- | --- |
| Workspace | `E:\Đồ án Game\Doan_Game3D` |
| Unity Editor | `6000.4.8f1`, revision `f8b72d3d7343` |
| URP | `17.4.0` |
| Input System | `1.19.0` |
| Cinemachine | `3.1.7` |
| AI Navigation | `2.0.12` |
| ProBuilder | `6.0.9` |
| Animation Rigging | `1.4.1` |
| Unity UI / Test Framework | `2.0.0` / `1.6.0` |
| Serialization | Force Text (`m_SerializationMode: 2`) |
| Version Control mode | Visible Meta Files |
| Git | Đã khởi tạo nhánh `main` theo lựa chọn rõ ràng của người dùng |
| Commit đã xác nhận | `ad9f245` — Khởi tạo dự án URP và cấu trúc thư mục |

Nguồn: `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `EditorSettings.asset`, `VersionControlSettings.asset`, `git log`. Packages khác còn trong manifest; bảng trên chỉ liệt kê các gói liên quan.

Kế hoạch yêu cầu Unity **6.3 LTS**, khác Editor hiện dùng. Đã ghi nhận, chưa có quyết định đổi Editor. Không nâng/hạ phiên bản hoặc sửa package để “khớp spec” trong lượt đọc tài liệu.

## Những gì đã có trên đĩa

- Cấu trúc `Assets/_Project`: Art, Audio, Animations, Materials, Prefabs, Scenes, Scripts, Data, UI, Tests.
- Scripts có thư mục Core, Player, Interaction, Combat, AI, Puzzles, Inventory, Quests, Save, UI. **Chưa có file `.cs` trong `Assets/_Project/Scripts` ở thời điểm kiểm tra.** Không được kết luận các hệ gameplay tự viết đã hoàn thành.
- Prefabs có Player, Environment, Interactables, UI. `Player/PF_An_Prototype.prefab` là variant của Starter Assets PlayerArmature.
- Có `Assets/Starter Assets/`. Bộ controller mẫu đang được dùng để dựng thử; danh mục asset nguồn vẫn ghi trạng thái cũ “Chưa tải”. Không sửa danh mục gốc trong lượt này.
- Cuối lượt đọc thấy thêm `Assets/LoafbrrAssets/MInesAndCaveSet`, tương ứng tên bộ hang A09 trong danh mục. Mới xác nhận thư mục đã có; chưa kiểm tra đầy đủ nội dung, giấy phép, vật liệu, collider hoặc tương thích. Không coi trạng thái “đã nhập” là “đã nghiệm thu”.
- Có đủ scene `00_Boot`, `01_MainMenu`, `02_HangLinhMach`, `90_TestMechanics`, `91_TestCombat`, `92_TestSave` trong `_Project/Scenes`.
- Boot/MainMenu/TestCombat/TestSave hiện là scene nền mẫu; việc tồn tại file không có nghĩa đã có menu/service/combat/save.
- `.gitignore` bỏ Library/Temp/Obj/Logs/Build/Builds/UserSettings và file IDE; Assets/.meta, Packages/manifest+lock và ProjectSettings đã được theo dõi ở commit đầu. Có `.plastic` từ trước khi khởi tạo Git; `.plastic` được ignore trong Git.

## Scene thử điều khiển — 90_TestMechanics

- Có Floor_Test, Wall, Wall_Left, Wall_Right, Low_Step, Block_High; đã gán Environment trong lượt trước.
- Có prefab An, MainCamera và PlayerFollowCamera từ Starter Assets.
- Layer Environment đã sửa tên từ lỗi chính tả `Enviroment`.
- Camera từng không bám nhân vật vì `Target.TrackingTarget = None`. Đã nối tới `PF_An_Prototype/PlayerCameraRoot`, đặt CustomLookAtTarget = false.
- Tham chiếu camera target qua stripped Transform trong variant và scene đã được kiểm tra cấu trúc file. Không lấy trạng thái này thay cho kiểm thử Play Mode.
- **Chú ý hiện tại:** scene thử có `AvoidObstacles.Enabled = 0`; CollisionFilter có mask9 nhưng tránh vật cản đang tắt. Ghi nhận để xử lý khi người dùng yêu cầu công việc liên quan; không tự sửa trong lượt đọc spec.

## Layer và mask đang dùng

| Layer | Index hiện tại |
| --- | --- |
| Default | 0 |
| Environment | 3 |
| Player | 6 |
| Enemy | 7 |
| Interactable | 8 |
| Hurtbox | 9 |
| LanternTarget | 10 |
| PuzzleBeam | 11 |

- Prefab An override root layer6; ground mask hiện **9 = (1<<0) | (1<<3) = Default + Environment**. Số mask9 không có nghĩa layer Hurtbox9.
- Camera obstacle filter cũng mask9; Player6 không thuộc mask này.
- Ở lượt thiết lập layer đã kiểm tra Physics cho phép Player–Environment, sáu collider địa hình bật và không trigger. Khi có thay đổi Physics sau này phải kiểm tra lại, không dùng ghi nhớ thay file thật.
- Layer phân theo vai trò collider; các collider phụ tương lai có thể khác root. Không tự đổi index vì nhiều prefab/mask phụ thuộc.

## Trại Giếng Trời trong 02_HangLinhMach

**Mức hoàn thành: blockout bố cục, chưa phải khu gameplay hoàn chỉnh.** Người dùng yêu cầu dựng riêng trại trước, chỉ nối tiếp các khu khác sau khi kiểm tra thuận tiện đi lại.

- Đủ năm root `World_Geometry`, `Gameplay`, `Lighting`, `Cameras`, `UI`, transform position/rotation0, scale1.
- Nền khoảng20×20m, vách phía tây/đông/nam/bắc chừa lối ra, bốn khối đá đơn giản.
- An và `Spawn_An` ở `(0, 0.2, -6)`, nhìn theo +Z; camera theo PlayerCameraRoot.
- `Campfire_Placeholder` ở `(-4, 0, 0)` với bệ và khối cam.
- `LanternPickup_Placeholder` ở `(4, 0, 3)` với bệ và khối vàng.
- Lối chính xanh ngọc rộng2,8m, hướng bắc tới cổng và hành lang ra Vườn Rêu rộng4m, dài khoảng6m; `Exit_EndStop_Temporary` chặn cuối ở z16 để tránh rơi.
- `Connection_To_VuonReu_Future` là mốc nối dự kiến; chưa dựng Vườn Rêu/Điện Trống/Hành Lang Quang/Đàn Kim Quy.
- Đã tạo30 mesh khối và8 vật liệu đơn sắc URP trong `_Project/Materials`: Ground, Wall, Rock, Campfire, Lantern, Path, Spawn, Boundary.
- Địa hình/collider chính dùng Environment; khối mốc tương tác dùng Interactable; lớp sơn đường/mũi tên không có collider.
- Camera tránh vật cản ở scene trại **đã bật**, mask chứa Environment. Scene thử điều khiển có trạng thái khác như ghi ở trên.
- Các lượt trước chỉ kiểm tra file: ID/ref,5 root, collider bật, route trung tâm thoáng, shader/guid assets/packages. **Chưa chạy Play Mode/build/playtest đo bố cục.**
- Lửa trại/đèn hiện chỉ là hình khối đánh dấu. Chưa có nhận trang bị, đèn Reveal/Focus, tương tác E, checkpoint lưu, crafting hoặc mục tiêu nhiệm vụ.
- Kích thước20×20 và vị trí khối là bố cục thử, chưa là chốt thiết kế; script sinh thử nằm trong Temp và không phải phần runtime hay nguồn đặc tả chính.

## Khác biệt, việc chưa đạt và câu hỏi cần giữ lại

1. **Editor6.4.8 so với kế hoạch6.3 LTS**: chưa giải quyết, không tự đổi.
2. **Danh sách build chưa theo kế hoạch**: `ProjectSettings/EditorBuildSettings.asset` vẫn chỉ `Assets/Scenes/SampleScene.unity`; file này đang bị xóa trong working tree. Kế hoạch cần chỉ00_Boot/01_MainMenu/02_HangLinhMach. Chưa kiểm tra cấu hình override Build Profile riêng, chưa sửa danh sách.
3. **Camera scene thử đang tắt Avoid Obstacles**, dù filter đúng; cần phân biệt với scene trại đã bật.
4. **Prototype Starter Assets**: các thông số và kiến trúc chưa được xác nhận khớp toàn bộ PlayerMotor/InputReader của spec. Không tự thay toàn controller hoặc ép mọi số cân bằng khi chỉ đang đọc tài liệu.
5. **Chưa có implementation custom** của GameSession/WorldStateStore/SaveService, inventory/crafting/quest, lantern/puzzles, damage/AI/Hộ vệ, menu/HUD. Đừng báo chúng xong dựa trên thư mục hoặc placeholder.
6. Chưa có bằng chứng build Windows hoạt động trên máy thứ hai, benchmark60FPS, camera/player ổn định trong Play Mode, hay full tuyến25–35 phút.
7. Giả định nhóm2–3/người15h mỗi tuần/máy đo là trong kế hoạch. Chưa có xác nhận số thành viên thực tế, giờ thực tế, máy tham chiếu hoặc thời điểm bắt đầu tuần1.
8. Danh mục asset kiểm tra nguồn29/10/2026, chưa kiểm chứng lại các URL/license/khả năng tương thích trong lượt này; chỉ đọc và ghi nhớ dữ liệu nguồn.

## Git và bảo toàn công việc người dùng

Trước lượt đọc spec đã có nhiều thay đổi chưa commit: xóa SampleScene; sửa TutorialInfo,90_TestMechanics,02_HangLinhMach,manifest/lock,TagManager; Starter Assets,material,player prefab và cấu hình ProBuilder mới. `spec/` cũng chưa được Git theo dõi. Những thay đổi này có trước tài liệu ghi nhớ, không được reset hoặc gom thành thay đổi của lượt đọc spec.

Commit đầu `ad9f245` chỉ là nền cũ, không chứng minh các công việc phía sau đã commit. Lượt đọc spec không tạo commit mới.

Khi kiểm tra cuối lượt, workspace còn xuất hiện `Assets/LoafbrrAssets/` và thay đổi `URPProjectSettings.asset` (thêm `m_ProjectSettingFolderPath: URPDefaultResources`). Thời gian ghi một số material blockout và EditorBuildSettings cũng thay đổi trong lúc đọc. Các thao tác của assistant trong lượt này chỉ ghi bốn tài liệu Markdown; không sửa hoặc hoàn tác các thay đổi đồng thời đó. SHA-256 của cả bốn file spec vẫn khớp bản đã đọc. Vì workspace có hoạt động đồng thời, không dùng hash tổng dự án để tuyên bố mọi file game bất biến.

## Cách tiếp nối qua chat mới

- Mở đúng repository, đọc `AGENTS.md`, PROJECT_MEMORY và file này; sau đó đọc phần spec liên quan theo SPEC_INDEX và kiểm tra file hiện hành.
- Các file là ghi nhớ bền trên đĩa. Chúng không tạo “trí nhớ toàn tài khoản” hay tự có mặt ở chat mở ngoài dự án này. Khi chuyển máy, cần mang theo/đưa các tài liệu vào bản sao repository.
- `AGENTS.md` được dùng làm hướng dẫn repository để dẫn agent tới tài liệu ngữ cảnh, theo [hướng dẫn chính thức OpenAI về workflow Codex](https://developers.openai.com/cookbook/examples/codex/iterating-development-workflows-with-codex).
- Khi được giao công việc mới, làm theo phạm vi người dùng giao; nếu sửa hệ thống/thiết kế, cập nhật ngày, bằng chứng kiểm thử và các khác biệt. Backlog không phải ủy quyền làm tiếp ngay.
- Gợi ý tiếp nối hiện tại (chỉ khi người dùng yêu cầu): kiểm tra chơi thử trại/controller/camera, thống nhất khác biệt nền tảng, sau đó đi theo thứ tự phụ thuộc trong kế hoạch. Không tự dựng cả tuyến hoặc nhập asset hàng loạt.
