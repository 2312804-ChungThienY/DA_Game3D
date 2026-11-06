# Kiến trúc và cấu hình hiện có — cập nhật 06/11/2026

**Trống C1 mới06/11:** `DongSonDrumSetup` thay riêng `VIS_DongSon_Playable` dưới `Drum_A/Drum_B/Drum_C` trong Main/Preview, giữ component/collider/ref/ID/transform root. FBX ndnguyen3d ở `Art/DongSonDrum/Source`, vật liệu `DongSon_EngravedBronze`: BaseMap2K, BC5 normal, BC7 metallic R/smoothness A (1−roughness), mipmap/trilinear/aniso16, không giảm mesh. Bảng ký hiệu nhỏ tách khỏi thân, có chân đỡ và ba spot không shadow. Không bake lại NavMesh. Không gọi `Chapter01RevisionArt.Install` để áp bản thay này vì author cũ vẫn chọn A20. [Worklog/credit](DONGSON_DRUM_WORKLOG.md).

**C2 lượt7 — trình bày:** `Chapter02EnvironmentArt` chỉ author scene03, nhóm `World_Geometry/ART_C2_Environment`, mesh/material ở `Art/Chapter02Environment`. Giữ collision bản khối và NavMesh; model nhập tắt collider, không bake lại. Mesh vách kín có UV theo mét, mặt đứng gồ nhẹ trong envelope collider; mặt sàn phẳng. Model bệ/cổng/vật nhặt nằm dưới object gameplay đang có và tuân theo visual/ref hiện hữu. `Chapter02Atmosphere` đọc player Z/FSM/world để hiện tên khu/tiến độ, phát cue và gió3D; không ghi state/save hoặc dịch chuyển root. `C2_HUD_Theme` dùng ref HudBars/ChapterQuestTracker/InteractionScanner sẵn có. InventoryPanel chỉ nhánh scene03 hiển thị mảnh C2/công cụ/gió và mô tả chế đúng trạm; nhánh C1 giữ hiện trạng.

`SoundPalette` thêm optional wind/bell/bird, `SoundCue` nối enum ở cuối. Soundscape giữ mặc định C1; scene03 chọn `Resources/C2SoundPalette`, chọn lại ambience và crossfade khi Thrower/ChimLac giao chiến; boss active giữ nhạc dù xa14m. `SequenceInputNode` chỉ C2 phát tiếng chuông theo nodeId, không đổi chuỗi. Audio mới ở `_Project/Audio/C2` do `SourceAssets/Tools/c2_audio.py` tổng hợp; mixer/PlayerPrefs keys giữ nguyên. [Worklog và kiểm chứng](C2_ENVIRONMENT_WORKLOG.md).

`PlayerGraphicsShutdown` đăng ký Application.quitting chỉ trong player, dùng public IGPUResidentRenderPipeline.gpuResidentDrawerMode=Disabled rồi ReinitializeGPUResidentDrawer để dispose GPU buffers theo code package hiện cài. Không subscribe trong Editor, không đổi chế độ khi chơi hoặc ghi asset trên đĩa. Native thường/trace sau sửa không còn cảnh báo22buffer/119persistent allocations/TempJob đã quan sát ở lượt Windows đầu.

**C2 lượt6 — kết chương:** `RelicAltar` thêm guard và batch silent→SaveAt→PublishChange, save lỗi Restore trước event/thẻ, optional `restorationCard` chỉ gán C2. `QuizPanel.Ask(quiz,solved,cancelled=null,context=null)` tương thích callsite cũ; Esc/disable gọi cancelled, chọn đúng chỉ gọi solved. `QuizSequence` giữ running/index/generation/RestoreVersion trong RAM; Restore hủy đúng panel sở hữu, unrelated Changed giữ lượt. Save cuối thất bại rollback riêng cờ hoàn tất, giữ Answered/FirstTry/mistake từ câu đã trả lời và `IsAwaitingSave`; E thử lưu lại. Thành công mới PublishChange/thẻ/callback có guard generation; đã xong E đọc lại thẻ không thưởng/event. `ChapterMemoryNotes` chứa lời Chim Lạc hư cấu; Journal suy từ `c2_linhthan_done`, không thêm save field. `ChapterExit` kiểm cả scene lẫn checkpoint đích; nhánh chưa có C3 dùng optional saveService/exitCheckpointId C2 để SaveAt trước kết/menu, không đăng ký C3 giả. [Lượt6](C2_ENDING_WORKLOG.md)7Editor/71Play đạt; hồi quy hẹp altar/nút kết C1, chưa full C1/C2 hoặc Windows mới.

**C2 lượt5 — đấu trường:** `BossChimLac` lấy lại độ cao giữa hai lao pha2, đọc enrage ngay trong nhịp đậu, quay về An khi đậu/quạt; dấu điểm đáp có ref riêng, sweep Environment và LOS damage, đáp ởfloorY, tránh LookRotation0. Timer/đổi pha chờ qua Pause; disable/reset/death xóa dấu báo và cờ GustIsActive. `BossBase.FilterDamage` chặn Waiting/dead/flight; thắng phân biệt `DefeatSavePending`, public `TrySaveDefeat()` cho phép thử lưu lại, không tạo ID/save schema mới. `ArenaGate.requiredDoorId` tùy chọn, C2 gán `c2_door_arena`; guard Pause/dead/disabled boss. C1 không gán field mới.

`ChimLacFocusReceiver` bỏ va chạm riêng giữa focus collider và CharacterController, vẫn raycastable, duy trì lại cặp ignore khi controller enable. `ChimLacPresentation` chỉ pose các mesh tự tạo `VIS_ChimLac_Combat`, không viết root; đây là combat proxy. `ChimLacArenaHUD` đọc FSM/Focus, thanh máu cũ, hướng dẫn nền tối gần đáy, progress Focus và retry panel theo IUiPauseOwner. Retry thành công mới bỏ pending; Esc có thể về game/dùng bệ. Scene thêm ART_C2_Arena/dấu đáp không collider, không bake NavMesh. [Lượt5](C2_ARENA_WORKLOG.md):8Editor/87Play,0failures/errors, W/RMB/Space/LMB thật và native death/Continue/retry-save; C1 progression/aim/damage win là fixture, chưa trận tay trọn vẹn/FPS/Windows. Kế tiếp6, art/cân tay7.

**C2 lượt4 — cầu/quái ném:** Scene03 dùng Ghost A08 và controller `Art/Chapter01Completion/Animations/AC_HacAnh.controller` có clip Stunned cho hai EnemyBrain và hai ThrowerBrain. Asset C1 dùng lại ở chế độ đọc; không tạo motor thứ hai, thrower không có NavMeshAgent. `ThrowerPresentation` riêng đọc FSM để phát Idle/Attack/Hurt/Death/Stunned, không sở hữu sát thương hoặc chuyển động. Cả bốn receiver C2 bật choáng4s/immunity3s, phải nhả chiếu mới gây choáng lại; có sao và cúi đầu, không đổi màu thân. Đây thay cấu hình chưa bật choáng tại mốc lượt0 ở dưới.

ThrowerBrain về cấu hình C2 spec14m/.6s báo/2.5s hồi/7m/s/12damage, melee2m/10damage/1.6s hồi. LOS lấy từ hand; gần mà melee đang hồi thì không xen ném. `EnemyProjectile` kiểm overlap ban đầu trước SphereCast, Environment ưu tiên khi chồng với player, consumed trước damage; chặn launch NaN/Infinity/zero. `EnemyLanternReceiver.ReceiveFocus` chặn delta không hữu hạn. `RequirementDoor` chuẩn bị Doors trong batch chưa phát event, SaveAt thất bại thì Restore snapshot và vẫn đóng; thành công mới PublishChange, có guard chống E lồng. Giữ collider cánh cửa Environment và tìm IInteractable ở cha, không cần đổi layer để scanner nhận được. C2 thêm hai khối nấp trong `ART_C2_Bridge`, bake NavMesh C2 vào asset cũ; hai khe cầu/cột gió/đường đi chính giữ nguyên.

Riêng root Scene03 có MeshRenderer thêm component Unity `DisallowGPUDrivenRendering` áp con để tránh lỗi BatchDrawCommand quan sát trong phiên chuyển scene; không sửa GraphicsSettings/pipeline C1. Helper truy xuất đúng component internal trong assembly `Unity.RenderPipelines.GPUDriven.Runtime`, không tạo cơ chế render thứ hai. [Lượt4](C2_BRIDGE_WORKLOG.md)25Editor/117Play đạt,1diagnostic I/O dự kiến/0unexpected errors; có input thật và restore save/cờ Console, chưa Windows/FPS/toàn C2. Console Error Pause chỉ tạm đổi trong probe rồi trả lại.

**C2 lượt3 — tri thức và phần thưởng:** `RewardService.TryGrant(rewards,out reason,pickupClaim=null)` chuẩn bị mọi item trên một bản nháp túi; kiểm ID toàn bộ batch, commit world/item/claim trước khi phát event. Có chặn gọi thưởng lồng trong listener. `InventoryService.PrepareAdditions/CommitAdditions/PublishChange` và `WorldStateStore.TryAddBatch/PublishChange` là API internal cho giao dịch này; RestoreVersion vẫn chỉ đổi khi restore. `QuizPedestal` nhận claim qua RewardService, P3 yêu cầu `c2_pz_chuong_gio`. `QuizPanel` bỏ static wrongThisSession, lưu `quiz_wrong_<quizId>` vào Flags, có trạng thái chờ nút thử lại và chặn chỉ số đáp án lỗi; Answered/FirstTry công bố cùng batch. Cờ sai tuân theo snapshot checkpoint như tiến trình khác; không ép autosave khi sai. Save cũ đã trả lời nhưng không có điểm không thể kiếm lại điểm qua bệ; lỗi sai chưa từng lưu trong bản cũ không thể suy ngược. Schema4/ID cũ giữ nguyên.

`PuzzleCluePickup.AllClueIds` hiện có7 (C1 năm giấy, C2 hướng dẫn gió + `c2_clue_bells`); danh sách Chapter1 vẫn5. Scene03 thêm `World_Geometry/ART_C2_Knowledge`, dấu lông trên chuông/phù điêu, giấy tùy chọn và card quiz có vùng giải thích cuộn. `Chapter02KnowledgeSetup` xử lý request apply/validate/inspect/test, từ chối Play/scene chưa lưu; `Chapter02KnowledgeProbe` chỉ chạy khi opt-in. Nguồn năm bia/bảy câu và phạm vi build gate riêng C2 ở [C2_KNOWLEDGE_SOURCES](C2_KNOWLEDGE_SOURCES.md). Không đổi model An hoặc scene C1.

**C2 lượt2 — tuyến P1–P3:** scene03 thêm `ART_C2_Traversal` (chín dấu không collider, layer2), visual7cột dùng `Art/Chapter02Traversal/Wind_Transparent.mat` (URP SurfaceTransparent, alpha.12, ZWrite0), không đổi trigger/ID. `PlayerFollowCamera` thêm `CaveCameraCollisionGuard` hiện hữu, không tạo motor camera thứ hai. Motor/wind runtime, NavMesh C2 và hai scene C1 không đổi trong lượt2. [Kiểm chứng](C2_TRAVERSAL_WORKLOG.md):20Editor/150Play,4Space qua InputReader và40góc camera; probe giữ focus thật, Pause/Resume test khi mất OS focus; không bypass guard input.

**C2 lượt1 — mở đầu/chuyển chương:** `GameSession.LoadChapter` chỉ Playing/ChapterComplete, truyền snapshot vào LoadScene trước sceneLoaded. `AbilityPickup` không deactivate root; ẩn/hiện renderer/collider theo defaults để restore trước nhận hoạt động. `PuzzleCluePickup.Chapter1ClueIds` giữ5giấy; `AllClueIds` có6, thêm Flag `c2_guide_wind` cho Journal xuyên chương. Main/Preview dời nguyên `ChapterExit_C1` khỏi trụ đá tới(-6,0,300); ID/ref/đích cũ. Scene03 `ART_C2_Opening`, `VIS_WindFeather`, SC/two objective mở đầu; Story/Journal dùng `Chapter01UISetup.ReadingPanels` riêng, không skin toàn HUD. NavMesh C2 cập nhật asset cũ, không ghi NavMesh C1. [Chi tiết](C2_OPENING_WORKLOG.md);21Editor/74Play,0failures/errors; chưa full C2/Windows mới.

**C2 — rà soát cơ chế05/11:** `WorldStateStore.RestoreVersion` tăng khi thay snapshot (RAM, không serialize); `SequencePuzzleController` so version trong handler Changed để hủy input/coroutine đang chờ khi restore, vẫn giữ lượt khi thêm dữ kiện khác. `BossBase.ResetBoss` xóa bất tử/stun cap và gọi hook `OnReset`; `BossChimLac` xóa FSM/focus/telegraph. `ChimLacFocusReceiver` chỉ nhận khi chim đậu, trận đang chạy và An hover trong cột gió bật, tránh bỏ qua cơ chế với enemyRange10m. `ThrowerBrain` dùng khoảng cách3D+LOS, damage độc lập renderer tùy chọn, tôn trọng `EnemyLanternReceiver.IsStunned` nếu được bật; scene C2 hiện chưa bật stun. `WindColumn` đồng bộ visual/IsActive khi enable/disable. [Lộ trình và bằng chứng](CHAPTER02_MECHANICS_PLAN.md),21kiểm Editor/56kiểm Play Mode theo component; chưa toàn tuyến/Windows mới.

**Giấy hướng dẫn/bệ mới nhất05/11:** `PuzzleCluePickup.Chapter1ClueIds` có5giấy (intro trại, điều khiển, trống, gương, trước Kim Quy), dùng chung Story/Journal. Ba Flags mới `c1_guide_camp_intro`/`c1_guide_camp_controls`/`c1_guide_kim_quy`, schema4/ID cũ giữ nguyên. `StoryCardPanel` reset cuộn về đầu khi mở. `Chapter01GuidePedestalSetup` căn A24 theo mesh bounds biến đổi thay vì bounds cache của renderer inactive, bỏ `Top_Bronze_Rim`, đặt phần model vật phẩm lên mặt bệ; không dời root pickup. `LanternPedestal/VIS_LanternPedestal_A24` độc lập `PF_LanternPickup`, tồn tại cả khi đèn đã được nhận. Main/Preview có15mesh A24 được căn lại.

**Điều chỉnh màu suy yếu mới nhất05/11:** `EnemyLanternReceiver` chỉ tính weaken/stun, không ghi tint; `EnemyPresentation` không ghi emission theo weakness. `EnemyStunIndicator` vẫn hiện ngôi sao khi choáng. Giữ multiplier×1,5, clip cúi đầu và FSM khóa đòn; không đổi ID/save hoặc ánh sáng môi trường.

## Revision mới nhất 05/11 — sau phản hồi lửa/gương/trống

- `InventoryPanel.TryCraftPotion` và `InventoryService.CanCraft/TryCraft`: chế trong túi I, không autosave; `CampCraftingTable` không còn trong Main/Preview. C2 giữ adapter CraftingPanel cũ.
- `PuzzleCluePickup` lưu hai Flags `c1_clue_drum`/`c1_clue_mirror` trong schema4. Root luôn active, chỉ ẩn model/collider khi đã nhặt; `JournalPanel` đọc lại nội dung. Không đổi ID puzzle/enum và không bắt buộc giấy trước khi giải.
- `EnemyLanternReceiver` opt-in `enableStun` cho ba actor C1: Hắc Ảnh4s/Hộ vệ2s, immunity3s, phải bắt đầu lượt chiếu mới. `EnemyBrain`/`StoneGuardian` thêm Stunned, hủy đòn/telegraph và chặn damage; `EnemyPresentation` phát clip cúi đầu, `EnemyStunIndicator` hiện ba sao. Một motor/actor như cũ. `VIS_ImportedEnemy.localRotation=identity` sửa bù yaw180 sai; không xoay root AI.
- `ART_C1_Revision`: lửa particle/ghế gỗ đều tỉ lệ, giấy, model A20/A22/A23/A24 dưới root tương tác cũ, sương URP Exp2 + năm bank particle thưa. Source model giữ nguyên; FBX tối ưu riêng ở `Art/Chapter01Revision`. Header đá khép trên cửa tới6,15m. Chi tiết kiểm chứng mới ở [C1_REVISION_WORKLOG](C1_REVISION_WORKLOG.md).

## Mốc hoàn thiện C1 trước revision

- AppRoot tạo thêm `SoundscapeRuntime` sau GameSession. `SoundPalette` ở Resources; mixer chuyển bằng AssetDatabase sang `Resources/Audio/MainAudioMixer.mixer`, giữ GUID/group. SettingsService đủ Master/Music/SFX, fallback khóa âm cũ, quality và debounce flush; xem AUDIO_SETTINGS.
- `EnemyPresentation` chỉ trình bày FSM/health, không sở hữu dịch chuyển/sát thương. Hắc Ảnh giữ EnemyBrain + một NavMeshAgent; Ghost A08 dùng năm clip nguồn. Hộ vệ giữ StoneGuardian transform motor, thêm CapsuleCast cản Environment; Golem Kevin dùng sáu clip bone do dự án tạo, generic Animator/avatar null/root motion off. Model dưới `VIS_ImportedEnemy`, gameplay root/ID/ref giữ nguyên.
- Đèn sen do dự án dựng dưới visual cũ, `LotusLanternVisual` giữ dáng treo và kích thước; `CameraBodyOcclusion` chỉ ẩn body khi camera ép sát. PlayerMotor.Teleport thông báo warp cho Cinemachine 3 để camera không kéo từ vị trí cũ qua vách. Không thay FBX/rig An.
- `ENV_C1_ClosedJunctions` vá tám đầu nối hang. Cửa FBX/sigil, vật nhặt/tinh thể/mảnh trống đều dưới root cũ. UI mới có marker `C1_UI_Theme`, `C1_MainMenu_Theme`, `C1_HUD_Theme`. QuestTracker đọc cùng ID cũ, thêm progressText cho 3 mảnh/2 câu đố.
- RelicAltar chụp snapshot world trước khi phục dựng; SaveAt trả false thì rollback restoration/ability, không mở kết chương; chỉ phát sự kiện/thưởng hoàn tất sau lưu thành công. Schema native4/legacy migration và mọi WorldIds giữ nguyên.

Chi tiết/bằng chứng mới ở [C1 completion](CHAPTER01_COMPLETION_WORKLOG.md). Các mốc kiểm kê 04/11 bên dưới là nền trước thay đổi này nếu có khác biệt.

Đối chiếu mã và serialized asset sau merge `ba58ff7`, gồm các thay đổi chưa commit trong workspace. Đây là mô tả **thực thi hiện tại**. Thiết kế phải đạt nằm trong [PROJECT_MEMORY](PROJECT_MEMORY.md) và [bảng yêu cầu](REQUIREMENTS_MATRIX.md). Bằng chứng chi tiết có dòng nguồn tại [RuntimeAudit](reports/RuntimeAudit_2026-11-04.md) và [ScopeAudit](reports/ScopeAudit_2026-11-04.md).

## Môi trường, scene và build

**C1 mở rộng 05/11:** world hiện hành là `World_Geometry/ENV_C1_ExpandedWorld`; hình cảnh cũ được lưu inactive. Gameplay root/ref/ID giữ, vị trí chuyển theo năm khoang; checkpoint dùng spawn mới, guardian bounds/cổng/kill volume căn theo địa hình mới. Xem [bản mở rộng](CHAPTER01_EXPANDED.md). Tọa độ/cấu trúc art 04/11 trong phần sau là lịch sử nếu khác bảng mới.

| Hạng mục | Hiện có |
| --- | --- |
| Unity | `6000.4.8f1`, revision `f8b72d3d7343`; không tự đổi về 6.3 LTS trong spec |
| Package chính | URP 17.4.0; Cinemachine 3.1.7; Input System 1.19.0; AI Navigation 2.0.12; ProBuilder 6.0.9; Animation Rigging 1.4.1; Test Framework 1.6.0 |
| Serialize | Force Text, metadata nhìn thấy được; GUID giữ cùng asset |
| Build scene bật | `00_Boot` → `01_MainMenu` → `02_HangLinhMach` → `03_GiengTroiChimLac` |
| Windows Build Profile | `Assets/Settings/Build Profiles/Windows.asset`, dùng danh sách scene toàn cục |
| Scene thử | `90_TestMechanics`, `91_TestCombat`, `92_TestSave`, `93_TestEnvironment`, `92_EnvironmentPreview`; không nằm trong danh sách build |
| Khuôn | `Template_Chapter`; chưa phải một chương chơi hoàn chỉnh |
| Chưa có scene | `04_ThanhDaSonTinh`, `05_SongNgamThuyTinh`, `06_TrucLinhMach` |
| Quality hiện có | `Mobile`, `PC`; UI Thấp/Vừa ánh xạ 0/1, không đổi tên quality asset |

Nguồn: `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/EditorBuildSettings.asset`, `QualitySettings.asset`. Có package MCP không có nghĩa phiên chat nào cũng đang kết nối hoặc đã thử Editor.

## Dịch vụ và vòng đời

`Scripts/Core/AppRoot.cs` tự tạo trước scene đầu và tồn tại qua scene. Nó tạo **SettingsService, InputReader, GameSession, SoundscapeRuntime** và nạp `Assets/_Project/Resources/PF_AppRootUI.prefab`. UI giữ một EventSystem và các panel chung.

Object **`GameSession` trong scene chương** chứa dữ liệu/runtime khác với component `GameSession` trên AppRoot: `WorldStateStore`, `InventoryService`, `SaveService`, reward và các tham chiếu chương nằm theo scene. Chuyển chương mang snapshot qua `GameSession.LoadChapter`; không mô tả World/Inventory là đã chuyển lên AppRoot như spec đích.

Trạng thái phiên: `Boot`, `MainMenu`, `Loading`, `Playing`, `Paused`, `Dead`, `InMenu`, `ChapterComplete`. `PushUiPause/PopUiPause` quản lý chủ sở hữu panel. `ReturnToMainMenu`, `NewGame`, `Continue`, `Respawn`, `LoadChapter` điều khiển scene. Continue/Respawn tải lại scene rồi áp snapshot. `ChapterExit` hiện màn kết nếu scene kế tiếp chưa có; C2 chưa chuyển sang C3 thật.

**Khác biệt còn lại:** `NewGame()` gọi xóa các file save trước khi scene mới tải thành công. Yêu cầu bảo toàn ô cũ khi khởi tạo thất bại vẫn cần xử lý, không coi phép thử New Game thành công là đã kiểm được nhánh lỗi này.

## Input, actor và camera

**Bản melee mới nhất:** [MeleeVisual](reports/MeleeVisual_2026-11-04.md), 106 checks Play Mode +23 pose Editor. PlayerAnimator CrossFade layer Melee, Slash1/Slash2/Lunge .65 s; native Idle body/feet giữ ổn định, KnifeGrip mask chỉ ngón phải. PlayerMeleeVisual quản lý dao/trail, không damage/movement; prefab dùng Knife A16 ở RightHand và không còn cube SlashFX. 9 clip/3 mask; AnAnimationBuilder giữ controller GUID khi rebuild.

**Bản sửa mới sau phản hồi người chơi:** [CharacterAim](reports/CharacterAim_2026-11-04.md), 91 checks. Look được đọc ở Update −80 trước Combat/Motor; LateUpdate chỉ áp pivot. Rê chuột lên nhìn lên. Di chuyển dùng yaw của frame hiện hành; đòn không lock-on theo camera XZ, có lock-on theo mục tiêu; dừng trượt và locomotion animation khi bắt đầu đánh.

Cập nhật điều khiển 04/11 sau audit: [76 kiểm tra Play Mode](reports/CharacterPolish_PlayMode_2026-11-04.json), [phạm vi và thông số](reports/CharacterPolish_2026-11-04.md). Đây là input giả lập qua chính Input System và API thật, không phải chơi tay hoặc benchmark FPS.

Asset input đang dùng là `Assets/_Project/Input/PlayerControls.inputactions`, GUID `5e6a5a87de4955a4496d4037b67e7727`, hai map **Gameplay/UI**. `InputReader` lấy `InputSystem.actions`. UI luôn được bật; Gameplay theo trạng thái phiên. `GameplayActive`, `WasPressed`, `IsHeld` chặn input khi mất focus; `Revision` xóa bộ đệm lệnh khi đổi map/focus. Nút đang giữ lúc mở lại map phải nhả trước; hai frame chuyển tiếp chặn delta chuột để tránh giật do khóa con trỏ. Map `Player`/PlayerInput/StarterAssetsInputs của prototype cũ không phải đường input của actor hiện hành.

| Phím | Action |
| --- | --- |
| WASD / chuột | Move / Look |
| Space / Shift trái | Jump / Sprint |
| Ctrl trái / chuột trái | Dodge / Attack |
| F / chuột phải | ToggleLantern / Focus |
| E / Q | Interact / UsePotion |
| Chuột giữa | LockOn |
| I / J | Inventory / Journal |
| Esc / F9 | Pause / DebugLoad; tải debug chỉ Editor/Development |

`Prefabs/Player/PF_An_Prototype.prefab` giữ model/Animator/CharacterController từ A01, remove ba component `ThirdPersonController`, `StarterAssetsInputs`, `PlayerInput` qua override. Actor dùng `PlayerActionState`, `PlayerMotor`, `PlayerCameraController`, `PlayerVitals`, `PlayerStamina`, `CombatController`, `DodgeController`, `PlayerAbilities`, `PlayerAnimator`, `LockOnController`, scanner/đèn. Các source Starter Assets vẫn tồn tại để nhập/biên dịch không chứng minh chúng đang điều khiển An.

- Motor: đi 3.5, chạy 5.5, Focus 2 m/s; nhảy 1.2 m, gravity −20; capsule cao 1.8, radius .3, center Y .9, step .3. `PlayerMotor` sở hữu dịch chuyển, tăng/giảm tốc 30/45 m/s², xoay tối đa .06 s, đệm nhảy .15 s và coyote .12 s. Ground check SphereCast xét normal, chạm trần xóa vận tốc đi lên; chia bước dịch chuyển lớn và đo vận tốc tổng. Teleport hủy đòn/lộn, trả capsule đứng và reset pose/bộ đệm. Có `Teleport`, `RequestLift`, `AddExternalVelocity`, `BeginDash/CancelDash`, `SetLow`.
- `PlayerActionState` chia quyền theo sống/pause/focus/hit reaction .18 s/đánh/lộn. Nhận sát thương hủy đòn và dash; E và Q bị chặn trong hành động xung đột, E yêu cầu chạm nền hoặc hover. Lộn vẫn phải có movement_kit.
- Camera: Cinemachine 3, chỉ `PlayerCameraController` ghi pivot trong LateUpdate; LockOn cung cấp điểm ngắm, marker cập nhật trước render Canvas. Điểm ngắm dự phòng cao 1.2 m; bỏ khóa khi bị che, không tranh góc với input chuột. Pitch −35°..65°. Hai scene chương bật tránh vật cản, radius .2, mask 9 = Default + Environment.
- Scene thử: `90_TestMechanics` đã bật Avoid Obstacles (.2/mask9); `91_TestCombat` vẫn tắt, không suy kết quả camera ở trại áp dụng cho mọi scene thử.
- Chương 1 chưa có `movement_kit` nên Ctrl không lộn. Sau phục dựng C1 mở bộ di chuyển; đừng tự thêm né sẵn cho Chơi mới C1.
- Animator hiện có clip tự tạo và thao tác xoay skeleton để thử lộn/ngã, root motion tắt. Đây là animation blockout; chưa phải bộ nhân vật cuối.

## Tương tác và ánh đèn

**Cập nhật mới nhất FocusCombat 04/11:** IEnemyLanternTarget phân loại quái (EnemyLanternReceiver/ChimLacFocusReceiver), scanner query max(8,10) và lọc tầm từng target: quái 10 m, puzzle 8 m. FocusLight range 12 m/góc 90° chỉ là falloff hình ảnh. LMB ngắt Focus tới lần nhả RMB, vệt dao cũ luôn disabled/không emitting; giữ animation và dao. [116 checks và ảnh](reports/FocusCombat_2026-11-04.md).

`InteractionScanner` lấy collider đầu tiên từ tâm camera, tìm `IInteractable` trên cha, đo từ `InteractionOrigin`, kiểm LOS và `CanInteract`, rồi nhận E. `IInteractRange` có thể thay tầm 3 m. Prefab An thực lưu **cameraRayDistance 15 m**, dù giá trị mặc định trong code là 25 m; khi hướng dẫn Inspector phải dùng giá trị serialized.

Mask scanner 2313 gồm Default/Environment/Interactable/PuzzleBeam. Scanner dùng PlayerActionState để chặn Attack/Dodge/hit reaction/dead/trên không; prompt chỉ ghi khi đổi nội dung. CampCraftingTable kiểm cùng action policy ngoài sống/Playing/range/debounce. Ràng buộc đúng actor sở hữu inventory vẫn là khoảng thiếu riêng; không coi guard trạng thái là đã giải quyết sở hữu inventory.

Đèn target mask 1024 (LanternTarget), visibility mask 8 (Environment). Các chỉ số hiện hành đã được người dùng chỉnh ngày 02/11: **Focus ngang 90°, dung sai dọc ±45°, phong ấn 1 giây**, độ nhạy mặc định .15 trong khoảng .03–.6. Không đổi về nón 60°/1.5 giây chỉ để khớp spec gốc.

Dao giữ hướng lúc bắt đầu, có bộ đệm .16 s gần cuối recovery, kiểm LOS Default/Environment trước mỗi mục tiêu và dedup theo IDamageable. Cửa sổ active chính xác .15–.30 s. Hủy hành động xóa lệnh đệm; lộn kết thúc bị hủy không tạo cửa sổ lunge.

Scanner Focus dùng buffer NonAlloc tự tăng khi đầy, dedup target; Focus nhường Attack/Dodge/hit reaction. Âm bước chân/tiếp đất nhận Animation Events qua một AudioSource trên An, nối SFX MainAudioMixer với 10 clip bước chân và 1 clip tiếp đất có sẵn. Chưa coi đây là nghiệm thu nghe âm thanh/mixer UI.

Đèn vẫn có reveal, Focus, năng lượng và receiver riêng; visual Light không thay thế kiểm tra logic. `ILanternTarget` hiện có `AimPoint`, `CanReceiveFocus`, `ReceiveFocus(float)`, không phải API Begin/End giả định từ tài liệu cũ.

## Trạng thái thế giới, ID và save

`WorldStateStore` dùng Dictionary<StateSet, HashSet<string>> khi chạy; `Export/Restore` chuyển DTO và thay toàn bộ tập, phát Changed sau restore. 12 tập: Fragments, Puzzles, Pickups, Seals, Doors, Bosses, BossTools, Abilities, Lore, QuizAnswered, QuizFirstTry, Flags.

`WorldIds` là **catalog tĩnh**: ID nội dung/checkpoint hiện chỉ C1–C2, cùng bốn ID năng lực chung movement_kit/wind/earth/water. Chưa có SO `WorldIdCatalog`, `GetInt/SetInt` hay `ChapterContext`. `WorldSnapshot.ints` đã khai báo DTO nhưng store chưa export/restore int-state để dùng cho đất/nước C3–C5.

| Dữ liệu | ID hiện có |
| --- | --- |
| Mảnh C1 | `relic_drum_a`, `relic_drum_b`, `relic_drum_c`; chưa đổi thành `fragment_*` |
| Puzzle C1 | `pz_trong_01`, `pz_guong_01` |
| Đèn / phong ấn / cổng | Flags `equipment_lantern`; Seals `seal_vuon_01`; Doors `door_dan_kim_quy` |
| Hộ vệ / phục dựng C1 | Bosses `guardian_01`; Puzzles `c1_relic_restored` |
| Năng lực | `movement_kit`, `ability_wind`, `ability_earth`, `ability_water`; có ID không chứng minh C3/C4 đã triển khai |
| Checkpoint C1 | `cp_trai`, `cp_dien_trong`, `cp_truoc_dan`, `cp_kim_quy` |
| Checkpoint C2 | `c2_cp_start`, `c2_cp_mid`, `c2_cp_arena` |

**Native save schema 4** trong `SaveData.cs`: schemaVersion, buildVersion, timestampUtcTicks, chapterId, sceneName, checkpointId, inventory, world{sets, ints}, elapsedPlaySeconds. File vẫn là `Application.persistentDataPath/save_v1.json`; backup `.bak`, tạm `.tmp`. Số trong tên file không phải schema.

`SaveService` có nhánh migration **legacy version 1–5** sang schema 4 trong bộ nhớ. Legacy v5 và native schema 4 là hai định dạng khác nhau; không gọi chuyển đổi này là “hạ schema tùy ý”. Chỉ lần save thành công mới ghi native snapshot ra đĩa. Pickup legacy `_001` được ánh xạ sang `_01` tại nhánh migration.

Ghi qua file tạm, flush/đọc kiểm tra và replace/backup. Khôi phục hồi đầy HP/stamina/đèn, không lưu buff tạm. **Validation hiện còn thiếu:** native inventory chưa đối catalog/maxStack trước chấp nhận; `InventoryService.Restore` bỏ ô sai bằng warning; world.ints/elapsed/header chưa được kiểm hết. Chưa được tuyên bố mọi save hỏng đều bị từ chối mà không thay đổi đồ.

## Túi, bàn chế và HUD

`InventoryService` trên GameSession scene: 12 ô; `TryAdd/TryRemove/CanAdd/TryCraft`; giao dịch craft một công thức trên bản sao rồi commit. Có 9 `Data/Items/ID_*.asset`; nguyên liệu stack 10, thuốc 5. `PotionUser` trên actor dùng Q, hồi 35, cooldown 2 giây, không dùng lúc đầy máu/chết. `InventoryPanel` mở bằng I; `JournalPanel` bằng J.

Tại Trại Giếng Trời C1: bàn chế đã gỡ. `InventoryPanel` có nút Chế bình máu; giao dịch 1 Lá cẩm + 1 Linh chi → 1 Bình máu trong túi I, không autosave. `Campfire_Placeholder` `(-4,0,0)` vẫn chứa `CheckpointAltar` ID `cp_trai`, spawn `(0,.2,-5)`, E chỉ lưu, `craftingPanel` để trống. Checkpoint đầu C2 `c2_cp_start` đang nối `CraftingPanel` và tự lưu sau craft; đó là flow khác đang tồn tại.

C1 dùng **QuestTracker**, mở đầu **Vườn Rêu**, hiện **3 mảnh/2 câu đố** qua objectiveText/progressText; C2 dùng **ChapterQuestTracker** với steps serialized. Giữ tách solved và nhận B/C. `ChapterObjectiveRules/ChapterQuestHUD` cũ không còn là API hiện hành.

## Combat, trùm và tri thức

- `CombatController`: đòn .65 giây, active .15–.30, 20 damage, 10 stamina, OverlapCapsule/dedup và LOS chống chém xuyên Default/Environment; có lunge sau lộn ×1.25. Không có beam/ribbon từ LMB; Focus và attack tách theo hợp đồng InputReader/PlayerActionState.
- `EnemyBrain` dùng NavMeshAgent; `StoneGuardian` điều khiển Hộ vệ trong biên đấu trường; C2 có `ThrowerBrain`, `EnemyProjectile`, `BossChimLac`, `WindColumn`. Không nói Hộ vệ hiện dùng cùng NavMeshAgent như prototype cũ.
- `RelicAltar` chỉ công bố hoàn thành sau SaveAt thành công; thất bại rollback snapshot world, không trao năng lực/mở màn kết.
- Tri thức có LoreDefinition, QuizDefinition, LoreStele, QuizPanel/QuizSequence, RewardService/RewardEntry và Journal. Điểm suy từ QuizFirstTry; lỗi sai lưu trong Flags theo snapshot checkpoint, không dùng static. Kết C2/save lỗi có cập nhật lượt6, xem C2_ENDING_WORKLOG cho kiểm chứng và giới hạn.
- Năm bia/bảy câu C2 đã đối chiếu nguồn trực tiếp ngày05/11, có `Chapter02KnowledgeBuildGate` chặn dữ liệu C2 chưa xác minh/thiếu cấu hình khi build scene03. Ba bia C1 chưa nằm trong đợt xác minh này. Không coi field verified là bằng chứng tự thân cho mọi chương.

## Âm thanh, art và công cụ

SettingsService hiện điều khiển **Master/Music/SFX** qua exposed dB của mixer Resources; AudioListener.volume giữ 1 khi mixer tồn tại. Có sensitivity/fullscreen/quality và PlayerPrefs `settings.*`, fallback khóa âm cũ, debounce flush. Footsteps/land đã nối Animation Events vào SFX; SoundscapeRuntime thêm nhạc/ambience/feedback. Xem AUDIO_SETTINGS cho kiểm chứng và giới hạn nghe/cân âm.

Art C1 toàn chương sau yêu cầu mới 04/11: `World_Geometry/ART_C1_Environment` chứa năm nhóm `ENV_01_Camp_ContactFix` → `ENV_05_KimQuy`; asset riêng trong `_Project/Art/Chapter01`. Rêu lấy mặt tam giác đá, cây theo bounds nền; 17 collider đơn giản mới, giữ collider gốc. NavMeshSurface bake bằng Physics Colliders vào `Art/Chapter01/Meshes/NavMesh_C1_Environment.asset`; lá/cỏ/rêu không thành chướng ngại. Model tương tác bổ sung dưới root cũ, giữ script/ref/ID; prefab trang trí chung không chứa mọi model con gameplay. `Chapter01EnvironmentSetup` chỉ xử lý request tường minh, từ chối scene chưa lưu và dựng chồng art. `92_EnvironmentPreview` nay sao chép **toàn C1** cùng actor/gameplay hiện có để kiểm tra, không còn chỉ là preview trại. [Chi tiết và bằng chứng](CHAPTER01_ENVIRONMENT.md).

Art trại gốc vẫn giữ: `World_Geometry/ENV_01_Camp/ART_Camp`, prefab/material riêng trong `_Project`; model lửa/bàn đặt dưới gameplay root có script. Nền/vách blockout giữ collider, hình thử cũ tắt. A09/A11/A16 là bộ chính; A10/A12/A14 có trên đĩa nhưng không tham gia nhóm trại này. `92_EnvironmentPreview` dùng camera tham chiếu, không actor mới.

Editor tools còn có: `AnAnimationBuilder`, `BuilderUtil`, `ChapterBuilder`, `SharedPrefabBuilder`, `AssetImportDiagnostics`, `EnvironmentAssetTrials`, `CampEnvironmentSetup`. Bốn Setup cũ Audio/Crafting/Inventory/Quest đã bị xóa sau merge. Không dùng hướng dẫn menu của chúng làm flow hiện hành.

Bot `PlaytestBot.Chapter1` vẫn chờ panel chế khi E ở lửa, trái flow C1 mới; cần sửa bot trước khi chạy lại kịch bản toàn tuyến. Probe `CampEnvironmentSmokeProbe` opt-in có 53 kiểm tra sau merge, chỉ bao phủ đoạn trại/camera/craft/save/Continue. Lượt đồng bộ tài liệu này không kích hoạt tool/probe hoặc chạy lại kiểm thử.

Quét PPtr/GUID phát hiện thêm motion ở state `Fly` trong `Animations/AC_An.controller:716` trỏ GUID không tìm thấy; controller gốc A01 cũng chứa tham chiếu đó. Đây là phát hiện tĩnh cần xác nhận trong Unity, chưa phải lỗi animation đã tái hiện. Mixer có GUID nội bộ của exposed parameter, không được báo chúng là asset ngoài bị thiếu.
