# Rà soát runtime và cấu hình dự án — 04/11/2026

Đây là đối chiếu tĩnh trên workspace sau merge và sau lượt dựng Trại Giếng Trời. Lượt rà soát tài liệu này không chạy Unity, không Play Mode, không build và không sửa code/scene/asset. Các báo cáo kiểm thử được nhắc phía dưới là bằng chứng đã lưu từ những lượt trước, có phạm vi riêng.

## Phạm vi đã đọc

Đã đọc mã của **69 file C# runtime** trong chín module: Player 11, Combat 5, AI 7, Interaction 13, Inventory 2, Save 4, Puzzles 13, Audio 1, UI 13. Đọc thêm `Core/InputReader.cs` và `Core/SettingsService.cs` để đối chiếu các đường gọi dùng chung. Core/Knowledge/Quests/Chapters và spec chương được rà soát riêng trong lượt đồng bộ chung của dự án.

Đã kiểm kê cấu trúc serialized của **10 scene**, **33 prefab** trong `_Project/Prefabs` cùng `Resources/PF_AppRootUI.prefab`, **37 ScriptableObject** trong `_Project/Data`, tệp input JSON, manifest/lock package, Build Settings, Windows Build Profile, TagManager và metadata phiên bản/serialization. Đọc field thiết kế trong Data để kiểm tra ID, kiểu, tham chiếu và thông số; việc kiểm chứng nội dung lịch sử/địa lý của Lore/Quiz không nằm trong phép kiểm kê cấu hình này. Mesh, texture, audio, NavMeshData và các thư viện bên thứ ba được đối chiếu qua metadata/tham chiếu, không coi là đã kiểm thử nội dung nhị phân.

## Môi trường và scene

| Nội dung | Hiện trạng đọc từ tệp |
| --- | --- |
| Editor | `6000.4.8f1`, revision `f8b72d3d7343` trong `ProjectSettings/ProjectVersion.txt` |
| Package | URP `17.4.0`, Cinemachine `3.1.7`, Input System `1.19.0`, AI Navigation `2.0.12`, Animation Rigging `1.4.1`, ProBuilder `6.0.9`, Test Framework `1.6.0` |
| Công cụ MCP | `com.coplaydev.unity-mcp` có trong manifest; lock ghi commit `81542a7e86292b5c4c21b1782c5af23b567f45ce`. Đây là công cụ Editor, không phải chức năng gameplay |
| Serialization | `EditorSettings.asset:7` có `m_SerializationMode: 2` |
| Build Settings | Bốn scene bật theo thứ tự `00_Boot`, `01_MainMenu`, `02_HangLinhMach`, `03_GiengTroiChimLac` |
| Windows profile | `Assets/Settings/Build Profiles/Windows.asset:21` có `m_OverrideGlobalSceneList: 0`, dùng danh sách scene toàn cục |
| Scene còn lại | `90_TestMechanics`, `91_TestCombat`, `92_EnvironmentPreview`, `92_TestSave`, `93_TestEnvironment`, `Template_Chapter`; có file không đồng nghĩa scene có đủ flow runtime |
| Chương chưa có scene | Không có `04_ThanhDaSonTinh`, `05_SongNgamThuyTinh`, `06_TrucLinhMach` trong `_Project/Scenes` tại thời điểm đọc |

Layer hiện tại trong `ProjectSettings/TagManager.asset`: Environment 3, Water 4, Player 6, Enemy 7, Interactable 8, Hurtbox 9, LanternTarget 10, PuzzleBeam 11, Hazard 12. Mask **9** là Default + Environment, không phải layer Hurtbox 9.

Hai scene chương có NavMeshSurface với tham chiếu NavMeshData không rỗng (`02_HangLinhMach.unity:191`, `03_GiengTroiChimLac.unity:190`), thu collider (`m_UseGeometry: 1`). Dòng `m_NavMeshData: {fileID: 0}` ở phần NavMeshSettings đầu scene không phải bằng chứng Surface không có dữ liệu. Chưa kiểm tra độ khớp NavMesh sau khi thêm collider trang trí trại, đường địch trong Play Mode hoặc build.

## Input, actor và camera

`Core/InputReader.cs:34` lấy `InputSystem.actions`, tìm hai map **Gameplay/UI**; không dùng map `Player` của Starter Assets để điều khiển actor hiện hành. Tệp dùng chung là `_Project/Input/PlayerControls.inputactions`, GUID `5e6a5a87de4955a4496d4037b67e7727`, khớp Build Settings. UI luôn bật; `SetGameplayEnabled` bật/tắt Gameplay và các giá trị Move/Look được trả zero khi Gameplay bị khóa (`InputReader.cs:29–32,78`).

| Action | Binding hiện có |
| --- | --- |
| Move / Look | WASD / Pointer delta |
| Jump / Sprint | Space / Left Shift |
| Dodge / Attack | Left Ctrl / chuột trái |
| ToggleLantern / Focus | F / chuột phải |
| Interact / UsePotion | E / Q |
| LockOn | Chuột giữa |
| Inventory / Journal | I / J |
| Pause / DebugLoad | Esc / F9 |

Gameplay chưa có binding gamepad. UI vẫn có các binding điều hướng/Submit/Cancel chuẩn nhiều thiết bị; điều này không chứng minh game chơi được bằng gamepad.

`PF_An_Prototype.prefab` là variant giữ model/Animator/CharacterController từ A01. Ba component Starter Assets gồm ThirdPersonController, StarterAssetsInputs và PlayerInput được remove qua override (`PF_An_Prototype.prefab:610–614`, đối chiếu fileID trong `A01/.../PlayerArmature.prefab`). Prefab thêm `PlayerMotor`, `PlayerCameraController`, `CombatController`, `PlayerAbilities`, `LockOnController`, `DodgeController`, `PlayerAnimator` cùng vitals/stamina/scanner/lantern. Không mô tả actor hiện hành là vẫn do ThirdPersonController dịch chuyển.

`PlayerMotor.cs:281` là nơi gọi CharacterController.Move cho actor lúc chơi thường, hỗ trợ vận tốc gió/cột nâng, dash và capsule thấp. Tham số prefab: đi 3.5, chạy 5.5, Focus 2 m/s, chạy tốn 8 stamina/s; nhảy 1.2 m, gravity −20; capsule radius 0.3, center Y 0.9, step 0.3. `PlayerAnimator` tắt Apply Root Motion; skeleton chỉ được xoay để hiển thị lộn/ngã, không dịch chuyển controller.

Camera của hai scene chương bật Cinemachine 3 obstacle avoidance, radius 0.2, mask 9 (`02_HangLinhMach.unity:14384`, `03_GiengTroiChimLac.unity:21767`). `PlayerCameraController` xoay pivot độc lập theo yaw/pitch −35°..65°. `LockOnController` chọn EnemyHealth đang bật trong màn hình, tầm ngang 15 m, bỏ khóa sau hơn 1 s bị che và quay hướng actor/camera về mục tiêu. Đây là cấu hình/code đọc được; phạm vi thử camera thực tế xem mục bằng chứng.

## Vitals, đánh và bộ di chuyển

| Hệ | Hợp đồng hiện có |
| --- | --- |
| Máu | PlayerVitals mặc định 100 HP, lọc team/giá trị NaN/Infinity, event Changed/Died; Kill bỏ qua bất tử, Heal chặn chết/đầy máu |
| Thể lực | 100, hồi 20/s sau 1 s; TrySpend là đường trừ duy nhất; pause/chết không hồi |
| Dao | 20 damage, 0.65 s/đòn, active 0.15–0.30, range 1.8, radius 0.6, tiêu 10 stamina; HashSet dedup mỗi IDamageable/mỗi đòn |
| Lộn | Phải có `movement_kit`; tiêu 25, dài 0.45 s, quãng 3.6 m hoặc ×1.5 khi chạy, interval0.65; i-frame0.10–0.30 |
| Né đúng lúc | Cửa sổ đầu0.15 s trong i-frame, cộng15 năng lượng đèn, slow motion0.3× trong0.3 s; nguồn damage phải team Enemy |
| Lộn + chém | Trong0.25 s sau lộn: damage×1.25, dash6 m/s trong0.15 s |
| Chui khe | Hạ capsule còn một nửa, kiểm tra trên đầu khi đứng dậy; có đoạn bò theo hướng lộn |

Nguồn: `Combat/CombatController.cs:11–27,116`, `Player/DodgeController.cs:104`, `Player/PlayerMotor.cs:168,184`, `Player/PlayerVitals.cs:29`, `Player/PlayerStamina.cs:50`.

**Khác biệt cần giữ trong tài liệu:** chương 1 mới chưa có `movement_kit`, nên Ctrl không lộn trước khi hoàn thành bệ Kim Quy; binding tồn tại không có nghĩa năng lực đã mở. CombatController hiện không có kiểm ray/linecast vật cản trong `ApplyHits`; không khẳng định dao đã chống đánh xuyên tường. Code motor/scanner cũng chưa triển khai toàn bộ state priority Hurt/Attack/Interact được mô tả trong thiết kế cũ; ví dụ scanner không tự chặn thao tác trong lúc đánh/lộn và motor không tự khóa locomotion theo IsAttacking.

## Lồng Đèn Sen và tương tác

- LanternController: sở hữu đèn một lần, F bật/tắt, Focus tốn12/s, max100, hồi20/s sau1 s; hết năng lượng phải nhả chuột để Focus lại. Light/VFX không làm nguồn kiểm tra gameplay.
- LanternFocusScanner: range8 m, **nón ngang90°**, dung sai dọc45°; lấy hướng camera, nguồn ở đèn, kiểm LOS với visibility mask Environment. Prefab ghi các giá trị này tại `PF_An_Prototype.prefab:732–740`.
- SealTarget: hold **1 s**, grace0.25 s. Prefab `PF_SealPrototype.prefab:52` cũng ghi1 s. Solved giữ trên component; SaveService.Capture đồng bộ nó vào WorldState trước khi ghi file.
- RevealSurface:5 m, kiểm mỗi0.2 s, đèn thường và LOS; là hiển thị tạm. LoreStele/WorldState chịu trách nhiệm lưu đã đọc, không tự coi một surface reveal là Lore đã ghi.
- InteractionScanner: ray từ tâm camera, kiểm khoảng cách từ InteractionOrigin tới hit point và LOS từ actor; tầm tay3 m hoặc `IInteractRange`. **Code default camera ray25 m nhưng prefab thực tế15 m** (`PF_An_Prototype.prefab:693–694`); không thấy scene override giá trị này.
- IDamageable hiện là `TryTakeDamage(DamageInfo)`; ILanternTarget là AimPoint, CanReceiveFocus, ReceiveFocus; IInteractable là Prompt, CanInteract, Interact. Không dùng bộ tên interface dự kiến chưa tồn tại để hướng dẫn sửa code.

Cone90°/hold1 s là cấu hình hiện hành, khác con số60°/1.5 s của phần thiết kế cũ. Nguồn chấp thuận là mục **“Sửa theo phản hồi người dùng chơi thử (02/11/2026)”** trong lịch sử PROJECT_STATUS: người dùng yêu cầu Focus dễ hơn và giảm độ nhạy; cấu hình90° ngang/±45° dọc, hold1 s, sensitivity default0.15 trong0.03..0.6 đã được ghi tại đó. Đây là override có nguồn, không phải lỗi cần tự đổi về số cũ.

## Inventory, pickup và bàn chế

`InventoryService` có12 ô và chỉ Material/Consumable chiếm ô (`InventoryService.cs:8`, `ItemDefinition.cs:15`). Catalog đang có chín định nghĩa mới `ID_*`; ba vật dùng ô là lá cẩm/linh chi maxStack10 và thuốc maxStack5. Equip/Fragment/Relic không tự biến thành ô túi: thông tin tiến trình nằm ở các tập WorldState và component sở hữu.

TryAdd trả số nhận, lấp chồng trước; TryRemove lấy từ ô cuối; TryCraft thử toàn giao dịch trên bản sao và chỉ đổi slots khi thêm đầu ra thành công (`InventoryService.cs:81`). ItemPickup hiện mỗi object cấp1 đơn vị, đánh dấu Pickups sau khi túi nhận đủ. PotionUser dùng Q, hồi35, cooldown2 s theo Time.time; không dùng khi chết, pause, đầy HP hoặc thiếu thuốc.

Trại chương1 hiện có `Gameplay/Camp_CraftingStation` và `CampCraftingTable`, bàn tại `(-3,0,3)`. E đổi1 lá +1 linh chi thành1 thuốc và nhắc lưu ở lửa; không ghi save (`CampCraftingTable.cs:18–36`). Checkpoint `cp_trai` giữ SaveService/SpawnPoint cũ và có `craftingPanel: {fileID:0}` (`02_HangLinhMach.unity:518–523`). Không hướng dẫn người dùng tìm CampCraftingStation/PlayerInventory/HealthPotionCrafting của phiên bản trước merge.

CraftingPanel và CheckpointAltar vẫn là API dùng chung: nếu được gán panel, checkpoint lưu rồi mở panel; panel chế thành công gọi SaveAfterCraft (`UI/CraftingPanel.cs:73–79`, `Save/CheckpointAltar.cs:41–48`). Trong scene02 hiện cả bốn CheckpointAltar đều không gán panel. Vì thế cần phân biệt **code panel còn tồn tại** với **đường thao tác đang dùng ở trại**.

## World state, lưu và khôi phục

SaveData hiện **schema4**, không phải phiên bản5 của hệ trước merge (`Save/SaveData.cs:8`). DTO có buildVersion, timestampUtcTicks, chapterId, sceneName, checkpointId, inventory, WorldSnapshot.sets/ints, elapsedPlaySeconds. Không lưu HP/stamina/năng lượng đèn: hồi đầy khi tải/lưu ở mốc. Cờ sở hữu đèn nằm trong Flags; trạng thái phục dựng hiện ở Puzzles `c1_relic_restored`, bộ di chuyển ở Abilities `movement_kit`.

SaveService mỗi scene chương giữ tham chiếu dữ liệu runtime. File vẫn là `Application.persistentDataPath/save_v1.json`, backup `.bak`, tạm `.tmp` (`SaveService.cs:43–45`). Tên file `v1` không biểu thị schema đang ghi.

| Luồng | Thực hiện hiện tại |
| --- | --- |
| Lưu | Capture → IsValid → ghi tmp UTF8 không BOM → Flush(true) → đọc/validate tmp → File.Replace hoặc Move → hồi tài nguyên/báo thành công (`SaveService.cs:119,216`) |
| Đọc | Chính rồi backup; schema4 đọc trực tiếp + Normalize, legacy `version1..5` khi `schemaVersion0` được migrate trong bộ nhớ; bản lạ/cao hơn bị từ chối (`SaveService.cs:272–301`) |
| Migration | Chuyển Flags relic_restored cũ sang Puzzles c1_relic_restored; chuyển ID pickup `_001` sang `_01`, legacy thiếu checkpoint v1–2 về cp_trai; file cũ chưa bị ghi lại chỉ vì đọc (`SaveService.cs:305,317`) |
| Apply | Restore world → Restore inventory → đèn/đồ chương1/puzzle → Teleport checkpoint → reset góc camera → hồi đầy tài nguyên (`SaveService.cs:175`) |
| Mốc | ICheckpoint gồm CheckpointAltar và CheckpointTrigger; scene02 có cp_trai/cp_dien_trong/cp_truoc_dan/cp_kim_quy; trigger lưu lần đầu trong phiên |
| F9 | Chỉ Editor/Development và Gameplay bật; gọi GameSession.Respawn để nạp lại scene và áp snapshot |

Luồng restore đầy đủ dựa vào nạp lại scene, không khẳng định gọi WorldState.Restore riêng có thể bật lại mọi pickup đã inactive và unsubscribe event. `ChapterOf` hiện ánh xạ riêng C2, trường hợp khác trảC1 (`SaveService.cs:257`); chưa phải hệ save hoàn chỉnh cho C3–C5.

**Giới hạn đã thấy khi đọc mã:** IsValid kiểm schema, checkpoint/scene, sets/ID thế giới/trùng, vị trí ô và tiến trình C1/điều kiện vào C2; chưa kiểm itemId/maxStack của schema4 theo catalog, toàn bộ world.ints, chapterId hay elapsedPlaySeconds như hợp đồng thiết kế đầy đủ. InventoryService.Restore bỏ ô sai bằng warning (`InventoryService.cs:119–136`). Vì vậy không ghi tài liệu rằng mọi save có vật phẩm lạ/quá chồng đã bị từ chối nguyên bản trước restore.

RelicAltar ghi restored và ability trước, gọi SaveAt nhưng bỏ qua bool, vẫn invoke onRestored và mở màn kết (`Puzzles/RelicAltar.cs:85–89`). RequirementDoor và BossBase cũng không xử lý kết quả SaveAt. Đây là phần còn lệch với yêu cầu giao dịch lưu thành công rồi mới thông báo/kết thúc; lượt đồng bộ chỉ ghi lại, không sửa hành vi.

## Địch, puzzle và UI

- EnemyBrain dùng NavMeshAgent và FSM Idle/Patrol/Chase/Attack/Search/Return/Dead. ED_HacAnh:60 HP, damage15, phát hiện10 m/120°, leash18 m, mất dấu3 s, weaken1 s → damage×1.5 trong4 s. Khi `agent.isOnNavMesh` false, Update bỏ qua; có prefab/script không chứng minh địch đi được trong scene.
- Hộ vệ dùng StoneGuardian,180 HP theo ED_HoVeDa. Bản này dịch chuyển transform của chính nó trong đấu trường phẳng, không dùng NavMeshAgent. Sweep báo0.8/active0.2/hồi1.2, damage20; Slam báo1.2/active0.25/hồi1.5, damage25/radius3. Không dùng ATK_Guardian_Sweep/Slam legacy làm bằng chứng đang cấu hình trùm.
- BossBase cung cấp bộ khung cho trùm sau: coroutine AI, bất tử, stun cap/filter, ghi Bosses, lưu tùy chọn khi chết. Chương2 có BossChimLac; chương3–5 chưa có code chuyên biệt trong workspace này.
- Trống C1 vẫn là DrumPuzzleController A–C–B, debounce0.2 s/fail0.7; gương bốn nấc90°, beam40 m/3 phản xạ/2 gương khác nhau, receiver1 s. SequencePuzzleController là hệ dùng chung chuỗi ID, khác DrumPuzzleController C1; tên file hiện là `Puzzles/SequencePuzzle.cs`.
- HUD hiện HudBars; InventoryPanel có12 ô chọn/xem chi tiết; JournalPanel đọc Lore và đếm QuizFirstTry; QuizPanel mở4 lựa chọn, retry, ghi QuizAnswered/QuizFirstTry; StoryCardPanel tạm dừng hoặc kết chương. StoryCard.image có trong dữ liệu nhưng Show(card) hiện chỉ truyền title/body, chưa render ảnh từ trường này.
- MainMenuController có New/Continue/Settings/Quit, xác nhận New nếu có save hợp lệ. AppRootUI quản lý pause/death/settings/confirm/toast; panelInventory/Journal bật riêng I/J để đóng trong khi Gameplay tắt. Không mô tả UI hiện tại theo Canvas/panel/class bị bỏ sau merge.
- SettingsService hiện chỉ volume Master qua AudioListener.volume, sensitivity0.03..0.6 (default0.15), fullscreen qua PlayerPrefs. Chưa có Music/SFX slider độc lập trong code hiện tại. `PlayerMotor.cs:359–361` có OnFootstep/OnLand rỗng; RoutedAudio khôi phục cho khả năng biên dịch legacy StarterAssets không chứng minh An hiện đang có âm bước chân.

## Asset nguồn và phần sót

Thư mục đã có: A01, A09, A10, A11, A12, A14, A16, A31. Việc có A14 hoặc bộ môi trường không có nghĩa mọi mẫu đã được nghiệm thu/được scene tham chiếu. Trại mới dùng A09/A11/A16; A10/A12/A14 chưa có trong ART_Camp của lượt dựng cảnh.

- A09 `MInesAndCaveSet/README.txt:1` ghi **Creative Commons 0/CC0**, khác cách danh mục và một số tài liệu cũ ghi Asset Store EULA. Báo cáo này ghi đúng nội dung tệp nhận được, không suy mọi bản phân phối có cùng điều khoản.
- A11 License.txt ghi CC0/Quaternius nhưng tiêu đề là “Ultimate Platformer Pack”, cần giữ dấu vết khác tên này khi tổng hợp nguồn.
- A16 License.txt ghi LowPoly Models by Quaternius, CC0 1.0.
- Các tệp mẫu gốc và `.meta` còn đi cùng nhau; lượt này không sửa/di chuyển chúng.

Năm ScriptableObject legacy sau không có script GUID tương ứng trong Assets tại thời điểm đối chiếu; không tìm thấy asset/scene/prefab khác trong `_Project` tham chiếu GUID của năm asset này:

| File | Script GUID thiếu |
| --- | --- |
| Data/Combat/ATK_Guardian_Slam.asset | `bb9597fb43434c159888f25af8670826` |
| Data/Combat/ATK_Guardian_Sweep.asset | `bb9597fb43434c159888f25af8670826` |
| Data/Items/IT_LaCam.asset | `dd4ebb065764821459b1bcabd28a6903` |
| Data/Items/IT_LinhChi.asset | `dd4ebb065764821459b1bcabd28a6903` |
| Data/Items/IT_ThuocMau.asset | `dd4ebb065764821459b1bcabd28a6903` |

Đây là đối tượng cần phân loại/dọn sau, không phải chỉ dẫn xóa ngay. Bảng nguồn hiện hành cần dùng các `ID_*`, EnemyDefinition và field StoneGuardian thực sự được scene tham chiếu.

## Phạm vi bằng chứng được phép kế thừa

| Báo cáo đã lưu | Kết quả ghi trong JSON | Giới hạn |
| --- | --- | --- |
| CampEnvironment_PlayMode_2026-11-04.json |53 checks,0 failures,0 errors | Sau merge: Boot/NewGame, tuyến trại→bàn→hành lang, aim ray riêng, craft, checkpoint,24 góc camera, menu/Continue, khôi phục file save gốc. Không bao phủ full C1/C2, combat/boss hoặc Windows build |
| CampEnvironment_Editor_2026-11-04.json | Preview/integration không failure;115 mẫu clearance | Kiểm tra hình ảnh/cấu hình/clearance, không phải bấm E/WASD thật, bake GI hoặc performance |
| 20261102_InventoryPlayMode/Windows.json |76 checks mỗi lượt,0 runtimeErrors | Hệ PlayerInventory/PotionUseController/save v5 trước merge; bằng chứng lịch sử, không tự chuyển thành nghiệm thu InventoryService/PotionUser/schema4 hiện tại |
| 20261102_CraftingPlayMode/Windows.json |57 checks mỗi lượt,0 runtimeErrors | Hệ chế cũ trước merge |
| 20261102_QuestHUDPlayMode.json |64 checks,0 runtimeErrors | ChapterQuestHUD cũ trước merge; không phải QuestTracker/ChapterQuestTracker đang dùng |
| 20261102_AudioPlayerWrite/Read.json |35+5 checks,0 runtimeErrors | Hệ mixer/settings cũ, không chứng minh bộ SettingsService hiện tại có3 kênh |
| EnvironmentAssetTrials_2026-10-30.json |0 failures | Bộ mẫu/cấu hình lúc đó; controller runtime mới đã thay sau merge |

Trong lượt đồng bộ hiện tại không tái chạy bất kỳ báo cáo nào. Các phần còn phải kiểm tra trong runtime/build gồm tuyến C1→C2, load legacy save thật, full-state rollback của code hiện hành, địch/NavMesh sau đổi geometry, combat qua vật cản, lỗi ghi file khi phục dựng, thao tác chuột/phím, bake GI và đo hiệu năng trên máy tham chiếu.
