# Trạng thái thực tế và điểm tiếp nối

**Cập nhật: 02/11/2026, Asia/Bangkok.** Đây là ảnh chụp từ file dự án và các lượt làm việc trước. Đọc lại file hiện hành trước khi sửa vì người dùng cũng chỉnh trong Unity giữa các lượt chat. Thiết kế đích ở [PROJECT_MEMORY.md](PROJECT_MEMORY.md), nguồn ở [SPEC_INDEX.md](SPEC_INDEX.md).

## Yêu cầu gần nhất

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
