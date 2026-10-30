# Trạng thái thực tế và điểm tiếp nối

**Cập nhật: 30/10/2026, Asia/Bangkok.** Đây là ảnh chụp từ file dự án và các lượt làm việc trước. Đọc lại file hiện hành trước khi sửa vì người dùng cũng chỉnh trong Unity giữa các lượt chat. Thiết kế đích ở [PROJECT_MEMORY.md](PROJECT_MEMORY.md), nguồn ở [SPEC_INDEX.md](SPEC_INDEX.md).

## Yêu cầu gần nhất

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
