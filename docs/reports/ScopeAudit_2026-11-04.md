# Đối chiếu phạm vi, chương và hợp đồng nền tảng — 04/11/2026

Ngày đối chiếu theo Asia/Bangkok. Báo cáo này ghi nhận nội dung nguồn và file hiện hành; không triển khai thêm gameplay, không chạy lại Unity, Play Mode hoặc build trong lượt audit.

## Phạm vi đọc và giới hạn bằng chứng

- Đọc 9 tài liệu đặc tả: bốn file gốc trong `spec/` và năm file trong `spec/chapters/`. Đối chiếu các tài liệu điều hướng `AGENTS.md`, `PROJECT_MEMORY.md`, `PROJECT_STATUS.md`, `SPEC_INDEX.md`.
- Đọc/đối chiếu 33 file C# liên quan: Core (10), Knowledge (7), Quests (2), Chapters/C2 (4), bốn Editor builder (`AnAnimationBuilder`, `BuilderUtil`, `ChapterBuilder`, `SharedPrefabBuilder`), ba `PlaytestBot*.cs`, `UI/QuizPanel`, `UI/JournalPanel`, `Interaction/ChapterExit`.
- Đối chiếu danh sách scene và Build Settings; 15 prefab Shared; dữ liệu Knowledge/C2, các override của cổng cuối chương 2 và danh sách báo cáo đã lưu trong `docs/reports/`.
- Có scene/prefab/code là bằng chứng hiện diện và cấu hình. Điều đó chưa chứng minh đạt nghiệm thu Play Mode, thời lượng người chơi mới hoặc Windows build.
- Không xác minh lại URL, giấy phép hiện hành hoặc nội dung lịch sử/tự nhiên trên web trong lượt audit. Không lấy cờ `verified: 1` làm bằng chứng đã đối chiếu nguồn độc lập.

## Thứ tự ưu tiên của yêu cầu

| Nguồn | Nội dung và cách áp dụng |
| --- | --- |
| Yêu cầu người dùng ngày 04/11/2026 trong phiên hiện tại | Chọn **3D hoạt hình low poly**, dựng môi trường trước, Trại Giếng Trời làm khu mẫu trong `92_EnvironmentPreview`, sau kiểm tra ghép vào `ENV_01_Camp`; chỉ làm thêm nhân vật/quái khi người dùng yêu cầu. Trong lượt đồng bộ tài liệu, người dùng xác nhận **giữ 5 chương, ưu tiên môi trường chương 1**; lịch 7 ngày là dự kiến cũ, chưa chốt hạn mới. Những quyết định này thay concept bán thực tế, thứ tự mỹ thuật và lịch cũ ở phạm vi đã chỉ rõ. Dấu vết môi trường trên đĩa: `docs/CAMP_ENVIRONMENT.md`; quyết định phạm vi được ghi trong các tài liệu lõi đồng bộ cùng ngày. |
| `spec/Ke_hoach_game_Son_Doong_12_tuan.md:1137` (§40–54) | Điều chỉnh ngày 01/11/2026 đã chốt **5 chương ngắn, 65–95 phút**, lớp Tri thức, hai cái kết, Quà Kim Quy và lịch sản xuất giả định 7 ngày. Nguồn ghi rõ ưu tiên phần 40–54 khi mâu thuẫn với phần 1–39 (`:1143`). |
| Cùng file, `:1534` (§55), và `spec/chapters/` | Nêu đặc tả chi tiết C2–C5 và sáu điều chỉnh ưu tiên hơn phần 46–49: đất 0/1/2 m; Focus theo camera có góc ngẩng; dùng chung puzzle chuỗi; đền đấu trường C4 chỉ rút nước 6 giây; thêm mốc/bệ C5; gợi ý thứ tự trống và `c1_relic_restored`. |
| Phần 1–39 của kế hoạch | Giữ luật kỹ thuật tương thích: một motor, input thống nhất, world state, save nguyên tử, quest suy từ dữ kiện, hai puzzle C1, thông số đèn. Phạm vi một chương, lịch 12 tuần, né ở C1, nỏ/móc dây là nội dung đã được phần 40–55 thay. |
| `spec/Tên_dự_án.md` | Tầm nhìn và cảm hứng, không tự biến mọi nỏ/leo/thuyền/parry thành tính năng bắt buộc. |
| `spec/Các_kỹ_thuật_làm_game.md` và danh mục asset | Danh mục tham khảo/ứng viên. Không phải bằng chứng đã triển khai hoặc lệnh tải toàn bộ. |

Ở thời điểm bắt đầu audit, `AGENTS.md:17`–`:20` và đầu `PROJECT_MEMORY.md` vẫn mô tả một chương/12 tuần. `SPEC_INDEX.md` đã có bảng phần 40–55 nhưng tiêu đề và mô tả nguồn vẫn ghi đủ 39 phần. Các mô tả đó cần được đồng bộ, không dùng làm căn cứ bỏ yêu cầu năm chương.

## Những thay đổi lớn so với kế hoạch 29/10/2026

| Chủ đề | Yêu cầu hiện hành từ nguồn |
| --- | --- |
| Phạm vi | C1 Kim Quy, C2 Chim Lạc, C3 Sơn Tinh, C4 Thủy Tinh, C5 Trục Linh Mạch. Tổng mục tiêu 65–95 phút, chưa phải thời lượng đã đo. |
| Nhịp từng chương | C1 15–25 phút; C2–C4 mỗi chương 10–15 phút; C5 20–25 phút. C1 giữ năm khu đã dựng. C2–C4 năm phòng, C5 sáu phòng. |
| Di chuyển | C1 không lộn né; hoàn thành C1 cấp `movement_kit`, mở lộn/hướng/chạy+lộn/chém sau lộn/né đúng lúc/chui khe cho C2–C5. Khóa mục tiêu bằng chuột giữa có từ C1. |
| Cơ chế theo chương | Cột Gió ở C2, Ấn Núi ở C3, Ngọc Nước ở C4; C5 kết hợp năng lực cũ, không thêm bộ di chuyển mới. |
| Tiến trình C2–C4 | P1–P3 nhận ba mảnh qua Bệ Vấn Đáp; P4 nhận công cụ trùm và thuốc; P5 trùm, phục dựng và ba câu Linh Thần Vấn Đáp. C1 không có Bệ Vấn Đáp. |
| Tri thức | Bia/câu hỏi gắn nhãn Lịch sử, Truyền thuyết hoặc Tự nhiên. Đúng ngay lần đầu ghi ID, điểm suy từ tập `QuizFirstTry`. C5 có Đại Vấn Đáp và câu hỏi bí mật. Không hỏi hư cấu game như sự thật lịch sử. |
| Kết thúc | Hai kết C5; Kết 2 cần Vành Ký Ức. Trùm cuối bốn pha, mốc đầu pha; thoát hang 90 giây; Continue sau kết về đàn, có cách xem lại kết. |
| Nỏ/móc/đi lại khác | §40 bỏ nỏ và móc dây; không đu dây, cưỡi chim hoặc bơi. Không phục hồi các tính năng này từ tầm nhìn cũ. |
| Save | `schemaVersion=4` theo hợp đồng chương; snapshot mang chương/scene/checkpoint, inventory, StateSet và số nguyên cho đất/nước/pha trùm. Không suy schema hiện tại từ các báo cáo v5 trước merge. |
| Hình ảnh và thứ tự công việc | Quyết định 04/11/2026 chọn low poly và cho dựng môi trường trại trước. Đây là ngoại lệ hiện hành so với concept semirealistic và quy trình A–B tất cả chương rồi mới C–D trong §41/51. |
| Lịch | Người dùng xác nhận trong lượt đồng bộ ngày 04/11: lịch 7 ngày chỉ là dự kiến cũ, chưa chốt hạn mới. Giữ lịch 7 ngày/12 tuần làm lịch sử; không dùng làm cam kết hiện hành và không tự gán ngày lịch đã qua thành hoàn thành. |

Nguồn chính: kế hoạch `:1145`, `:1167`, `:1184`, `:1216`, `:1240`, `:1274`, `:1300`, `:1326`, `:1352`, `:1403`, `:1478`, `:1506`, `:1534`.

## Bảng yêu cầu và trạng thái C1–C5

| Chương | Yêu cầu theo đặc tả | Hiện có trên đĩa | Mức kiểm chứng/phần thiếu |
| --- | --- | --- | --- |
| C1 Kim Quy | Năm khu, đèn, trống A–C–B, gương, dao, Hắc Ảnh, Hộ vệ, phục dựng, Quà Kim Quy; ba bia, chưa có câu hỏi. | `02_HangLinhMach.unity`; các hệ nền hiện hành; `QuestTracker`; ba LR C1; `ChapterExit_C1` sang C2. Trại có ART mới và bàn E riêng. | Báo cáo trại ngày 04/11 có 53 kiểm tra tự động; không thay cho nghiệm thu toàn tuyến sau merge. Lịch sử nhánh đã ghi bot đi hết C1, nhưng lượt audit không chạy lại. C1-R11 đổi ID mảnh chưa làm; animation còn blockout; Windows toàn tuyến hiện hành chưa có bằng chứng. |
| C2 Chim Lạc | Năm phòng đi lên cao; Cột Gió; chuông 2–3–1; bốn bệ; hai Hắc Ảnh và hai Hắc Ảnh Ném; Chim Lạc; phục dựng; ba câu cuối; sang C3. | `03_GiengTroiChimLac.unity`, NavMeshData, `WindColumn`, `ThrowerBrain`, `BossChimLac`, `ChimLacFocusReceiver`; năm LR, bảy QZ; SQ và hai StoryCard; 15 prefab dùng chung. Có `ChapterQuestTracker`. | Lịch sử ghi `c2s` đạt C2, cần phân biệt bằng chứng cũ với retest sau merge. Cổng cuối trỏ C3 nhưng hiện fallback màn kết vì chưa có C3. Không thể ghi nghiệm thu “sang C3 và Continue C3” đã đạt. Cầu/trùm cần cân bằng người chơi thật; tri thức cần KN01. |
| C3 Sơn Tinh | Năm phòng; ô đất 0/1/2 m, dáng núi 2–0–1–2, Hộ vệ nhỏ, Rết Đá, Vành Núi, ba câu cuối; sang C4. | Chỉ có `spec/chapters/C3_Son_Tinh.md`; tên năng lực `ability_earth` đã khai trong WorldIds. | Không có scene `04_ThanhDaSonTinh`, thư mục script C3, prefab/data chương hoặc ID tiến trình/checkpoint C3 trong WorldIds. Thiếu `GetInt/SetInt` và int-state nền. Chưa triển khai, chưa thử. |
| C4 Thủy Tinh | Năm phòng; ba mức nước, bè, cửa van, chìa đền ngập, nước sâu đưa về chỗ an toàn, địch nước/vỏ đá, Thuồng Luồng; đền đấu trường rút 6 giây. | Chỉ có `spec/chapters/C4_Thuy_Tinh.md`; tên `ability_water` đã khai trong WorldIds. | Không có scene `05_SongNgamThuyTinh`, script/data chương; thiếu hệ đất dùng lại từ C3, nước, bè, safe-ground, áo giáp, trùm và int-state. Chưa triển khai, chưa thử. |
| C5 Trục Linh Mạch | Sáu phòng, phối hợp năng lực, hai đợt Chiến Lũy, Đại Vấn Đáp, trống bốn vành, Vành Ký Ức, Hắc Sương Vương bốn pha, thoát hang, hai kết. | Chỉ có `spec/chapters/C5_Truc_Linh_Mach.md`. | Không có scene `06_TrucLinhMach`, script/data/ID/checkpoint C5, bộ kết hoặc phase state. `LanternController.AddEnergy` có thể dùng cho nền nhưng không có nghĩa đã làm C5. Chưa triển khai, chưa thử. |

Build Settings hiện chỉ bật bốn scene: `00_Boot`, `01_MainMenu`, `02_HangLinhMach`, `03_GiengTroiChimLac` (`ProjectSettings/EditorBuildSettings.asset:7`). Các scene thử và `Template_Chapter` không trong build. Chỉ có `Scripts/Chapters/C2`.

`ChapterExit_C2` có override `nextScene=04_ThanhDaSonTinh`, `nextCheckpointId=c3_cp_start`, `requiredPuzzleId=c2_linhthan_done` trong khối tại `03_GiengTroiChimLac.unity:7344`. `ChapterExit.cs:51` cố ý xử lý chương chưa có bằng màn kết. `WorldIds.cs:47` hiện chỉ ánh xạ bốn mốc C1 và ba mốc C2.

## Hợp đồng nền tảng cần mô tả đúng

| Hợp đồng thiết kế | Mã hiện hành | Kết luận khi đồng bộ docs |
| --- | --- | --- |
| `AppRoot.Instance.World/Inventory/Rewards` và `ChapterContext` (00 nền tảng `:106`) | `Core/AppRoot.cs:11` chỉ có Settings/Input/Session/UI; WorldState, Inventory và RewardService ở mỗi scene; `GameSession.LoadChapter` chuyển bằng snapshot (`:92`). Không có lớp ChapterContext. | Mô tả API hiện có để tiếp nối được; giữ chênh lệch kiến trúc như việc chưa chốt/hoàn thiện, không tự tuyên bố hợp đồng spec đã đủ. |
| `WorldIdCatalog` SO, kiểm tra Persistent IDs trước build (00 `:16`, `:78`, `:236`) | `Core/WorldIds.cs:4` ghi rõ SO chưa làm, dùng danh mục tĩnh. Không tìm thấy menu `Validate Persistent IDs` trong Editor. | Không hướng dẫn gọi menu chưa có. Không đổi ID đang có chỉ vì tên trong source dự kiến khác. |
| `WorldStateStore.GetInt/SetInt` (00 `:73`) | `WorldStateStore.cs:38` Export và `:52` Restore chỉ xử lý sets. `WorldSnapshot` có trường ints nhưng chưa có API runtime tương ứng. | Ghi nền int-state chưa triển khai; đây là phụ thuộc thật của C3/C4/C5. Không nói lưu đất/nước/pha trùm đã hỗ trợ chỉ vì DTO có `ints`. |
| `GameSession.SaveAtCheckpoint` (00 `:101`) | Không có phương thức này; hiện các object gọi `SaveService.SaveAt`. `PushUiPause` nhận `IUiPauseOwner`, trả bool (`GameSession.cs:161`), khác chữ ký mẫu nhận object/void. | Ghi đúng chữ ký đang dùng; giữ mapping tới hợp đồng thiết kế. Không để agent gọi API mẫu không tồn tại. |
| `QuestStepList` SO (00 `:132`) | `ChapterQuestTracker.cs:10` có lớp Step và danh sách serialized trực tiếp trên component (`:19`); C1 dùng `QuestTracker`, C2 dùng ChapterQuestTracker. | Không khẳng định đã có SO QS. Tách yêu cầu quest suy từ state khỏi cách lưu cấu hình component/SO. |
| `TelegraphMarker`, `DamageVolume` (00 `:177`) | Không tìm thấy hai lớp này; đòn C2 tự điều khiển telegraph và phép quét trong `BossChimLac`/`ThrowerBrain`. | Ghi rõ hiện tại dùng hiện thực riêng; chưa có component dùng chung như đặc tả chương sau. |
| C1-R11 đổi ID mảnh (`spec chính:1271`; 00 `:224`) | `WorldStateStore.cs:8` vẫn dùng `relic_drum_a`, `relic_drum_b`, `relic_drum_c`. | Tài liệu hiện trạng phải ghi đúng các ID này. Yêu cầu đổi sang fragment_a/b/c vẫn là gap; không đổi save/code trong lượt docs. |
| Điểm đúng lần đầu phải theo tiến trình bền vững | `QuizPanel.cs:20` dùng static `wrongThisSession`, chỉ Clear khi SubsystemRegistration (`:29`), ghi đúng ở `:86`. | Đã có QuizAnswered/QuizFirstTry; chưa lưu “đã sai”. Thoát/mở ứng dụng hoặc Chơi mới trong cùng ứng dụng có thể làm ngữ nghĩa lần đầu lệch yêu cầu. Cần xác nhận/sửa trước khi dùng điểm mở bí mật C5. |
| `verified=false` phải chặn Release (00 `:150`) | LoreDefinition/QuizDefinition chỉ khai báo field; không có validator build đọc field trong code Editor đang có. | Ghi gate nội dung chưa triển khai, KN01 chưa nghiệm thu. |

Khác biệt cân bằng có ghi nhận: `ThrowerBrain.cs:14` dùng 10 m thay 14 m, `:16` dùng 3 giây thay 2,5 giây. Comment và lịch sử ghi đã giảm sau thử cầu; các thông số nguồn là điểm cân bằng ban đầu, không cần tự ép code về source trong lượt đồng bộ docs.

Giới hạn giao dịch phần thưởng cần lưu ý cho chương sau: `RewardService.cs:41` gọi `CanAdd` riêng từng loại Item trên túi gốc, sau đó `:54` thêm lần lượt. Không có phép thử tổng hợp tất cả loại Item trên cùng một bản nháp như giao dịch chế thuốc. Với phần thưởng nhiều loại Item cạnh tranh cùng ô trống, chưa có bằng chứng bảo đảm nguyên tử; bệ P4 C2 hiện chỉ trao một loại Item là thuốc kèm BossTool. Không mở rộng tuyên bố “mọi phần thưởng nhiều loại đều nguyên tử” chỉ từ comment của lớp.

## Nguồn kiểm thử và tri thức

Lịch sử hiện lưu trong `docs/history/PROJECT_STATUS_before_sync_2026-11-04.md`, mục “Tự test toàn bộ + Ngày 3 (02/11/2026)”, ghi bot `c1c2` qua C1 rồi dừng ở P4 C2, tiếp đó `c2s` đi hết C2. Bot có trong `Scripts/DevTools/PlaytestBot*.cs`, report mặc định ghi `Application.temporaryCachePath/playtest_report.txt` (`PlaytestBot.cs:16`). Không có bản báo cáo C2 riêng được giữ trong `docs/reports/` hiện tại. Không tái tạo số liệu hoặc biến lịch sử thành kết quả test mới.

Kịch bản bot C1 còn chờ CraftingPanel mở khi E ở lửa (`PlaytestBot.Chapter1.cs:53`–`:54`). Sau khi bàn riêng được thêm ngày 04/11, E ở checkpoint chỉ lưu; vì vậy bot `c1`/`c1c2` cần được cập nhật trước lần tái kiểm thử tiếp theo. Lượt audit tài liệu không chạy bot này vì nó còn có nhánh ghi đè save thật.

Trên đĩa có 8 LoreDefinition và 7 QuizDefinition, đều `verified: 1`, `sourceRef` chủ yếu là tên SGK, truyền thuyết, website VQG/UNESCO. Lịch sử ngày 3 ghi rõ đã đối chiếu bằng kiến thức chung của AI và còn chờ người dùng duyệt nguồn. Vì thế cần ghi **“đã gắn nhãn/cờ và nguồn tham khảo, chưa có hồ sơ KN01 xác minh độc lập”**. Không tuyên bố mọi mục đã được kiểm chứng chỉ từ serialized bool.

## Các quyết định khi lập công việc tiếp theo

Bốn câu hỏi của lượt đồng bộ đã được trả lời: 5 chương/môi trường C1 trước, chưa hạn mới, giữ Master/Music/SFX và HUD Vườn Rêu/progress3/2. Các điểm dưới là phụ thuộc khi lập kế hoạch triển khai riêng, không chặn việc ghi đúng hiện trạng và không tự thay yêu cầu nguồn.

1. **Hạn nộp và lịch thực hiện mới khi cần lập kế hoạch.** Phạm vi đã được người dùng xác nhận là năm chương, trước mắt ưu tiên môi trường C1; không hỏi lại hoặc tự rút xuống hai chương. Người dùng xác nhận lịch 7 ngày là dự kiến cũ và chưa chốt hạn mới, nên không tạo deadline hoặc coi các ngày lịch đã hoàn thành.
2. **Giữ kiến trúc hiện tại hay hoàn thiện đúng các hợp đồng nền tảng trước C3?** Hệ hiện hành dùng WorldIds tĩnh và dịch vụ theo scene/snapshot; spec yêu cầu thêm catalog SO, ChapterContext và AppRoot.World/Inventory/Rewards. Có thể ghi mapping hiện trạng ngay, nhưng việc đổi yêu cầu nguồn cần quyết định riêng.
3. **Ngữ nghĩa điểm Tri thức sau trả lời sai rồi khởi động lại/Chơi mới.** Source muốn “đúng ngay lần đầu” theo tập bền; hiện code chỉ nhớ sai trong vòng đời ứng dụng. Không quyết định thay luật bằng cách sửa chữ docs cho khớp bug.
4. **Cách duyệt nguồn Tri thức trước build.** KN01 vẫn cần hồ sơ nguồn tin cậy và build gate; người dùng có thể giao lượt riêng để kiểm chứng. Các ví dụ đang có không nên được coi đã nghiệm thu học thuật.

Các điểm không cần hỏi lại để đọc hiểu hiện trạng: phong cách low poly, môi trường trước nhân vật/quái, ba bộ A09/A11/A16 và bố cục trại đã được người dùng chỉ rõ ngày 04/11/2026. Bệ Vấn Đáp theo phòng là cách đặc tả C2 đang hiện thực; không tự đổi thành chia theo chương chỉ vì §54 còn ghi giả định cũ.
