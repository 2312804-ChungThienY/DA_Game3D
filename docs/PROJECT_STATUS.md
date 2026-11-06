# Trạng thái thực tế — Sơn Đoòng

## Trại Giếng Trời C1 theo phác thảo đã duyệt — 06/11/2026

Đã sửa `Assets/_Project/Scenes/02_HangLinhMach.unity` ở `ENV_01_TraiGiengTroi` và đầu đường nối sang Vườn Rêu: tia nắng, 14 thạch nhũ có LOD, 30 phiến lát bất quy tắc, 6 dương xỉ rìa và biến thể màu vách. Giữ gameplay root, `cp_trai`, bệ đèn, phong ấn, collider nền và NavMesh. [Ảnh, file và kiểm chứng](C1_GIENG_TROI_WORKLOG.md): Unity Edit Mode validate 0 issues, 5 điểm NavMesh/4 đoạn PathComplete, 0 collider mới; Console không có lỗi. Chưa kiểm Play Mode sau lượt art, Windows build, chơi tay hoặc FPS; cần người dùng duyệt ảnh trước khi chuyển sang khu C1 kế tiếp.

## Chuẩn art Mid Poly Stylized Việt Nam — 06/11/2026 (quyết định, chưa chuyển asset)

Người dùng chốt texture 1K mặc định, 2K cho nhân vật/công trình chính, material URP Lit và LOD cho vật thể lớn. Đã cập nhật tài liệu hướng dẫn và project skills liên quan; không sửa model, texture, material, prefab hay scene và không chạy test Unity/Blender trong lượt này. Chưa đo hiệu năng theo chuẩn mới; số polygon, ngưỡng chuyển LOD, bảng màu và phạm vi chuyển đổi C1/C2 vẫn cần quyết định khi triển khai.

## Bốn project skills Codex — 06/11/2026 (chỉ hướng dẫn)

Đã tạo `.agents/skills/unity-scene-builder`, `blender-asset-creator`, `unity-test-and-debug`, `vietnam-world-builder` theo `skill-creator`. Mỗi skill có phạm vi kích hoạt, quy trình MCP, giới hạn thao tác và điều kiện kiểm chứng riêng; phong cách Stylized Việt Nam cho phép chi tiết cao hơn. Đã kiểm tra chỉ đọc Unity MCP (Editor mở `MCP_Test`, Edit Mode) và Blender bridge (`Scene`: Cube/Light/Camera); không chỉnh scene, model hay chạy test game trong lượt tạo skill. Bộ `quick_validate.py` của skill-creator chưa chạy được vì môi trường Python thiếu `PyYAML`; đã kiểm tra frontmatter/cấu trúc riêng. Cần mở phiên Codex mới để thử kích hoạt từng skill.

## Định hướng Stylized Việt Nam —06/11/2026 (chỉ quyết định thiết kế)

Ở thời điểm ghi nhận, người dùng chọn **Stylized Việt Nam** và **cho phép mức chi tiết cao hơn**; low poly không còn là giới hạn bắt buộc. Đã đồng bộ AGENTS, ART_BIBLE, GAME_DESIGN, TASKS, MEMORY, MATRIX và chỉ mục; chưa đổi scene, prefab, model, material, shader hoặc build. C1/C2 vẫn là bản dựng/kiểm thử trước quyết định này. Khi đó chưa có concept, bảng màu hoặc ngân sách polygon/texture/LOD định lượng; chuẩn mới nhất xem đầu tài liệu. Không ghi kết quả Play Mode/build cũ là kiểm chứng cho phong cách mới.

## Bộ hướng dẫn Codex —06/11/2026 (chỉ tài liệu)

Đã rà soát repository và tạo bộ định hướng [AGENTS](../AGENTS.md), [ART_BIBLE](ART_BIBLE.md), [GAME_DESIGN](GAME_DESIGN.md), [TECHNICAL_DESIGN](TECHNICAL_DESIGN.md), [TASKS](TASKS.md); cập nhật [chỉ mục](README.md). Nguồn quyết định/bằng chứng hiện hành vẫn ở MEMORY/STATUS/MATRIX/ARCHITECTURE/spec; các tệp mới đánh dấu điều chưa chốt thay vì đặt yêu cầu game mới.

Kiểm tĩnh xác nhận Unity `6000.4.8f1`, URP qua Graphics/Quality, package trong manifest, Git LFS cài nhưng chưa theo dõi file, `.gitignore` bỏ cache. Unity MCP chỉ đọc trạng thái Editor: `MCP_Test` đang mở, Edit Mode, scene sạch; Blender MCP chỉ đọc scene `Scene` gồm Cube/Light/Camera. Không sửa code, scene, prefab, asset, package hoặc cache; không chạy Play Mode/build/test trong lượt tài liệu. Kiểm tra liên kết Markdown và `git diff --check` đạt; các lượt kiểm thử gameplay trước đây không được chạy lại ở đây.

## Ba trống giải đố Đông Sơn mới —06/11/2026 (mới nhất)

Đã thay ba `VIS_DongSon_Playable` ở Điện Trống trong Preview→Main bằng FBX/texture2K từ ZIP người dùng: **ndnguyen3d, CC BY4.0** đã xác minh API nguồn và ghi credit. URP PBR giữ hoa văn gốc, bảng Mặt Trời/Cánh Chim/Sóng Nước tách khỏi thân, ba spot không shadow giúp đọc hoa văn. Giữ component/root/collider/ref, chuỗi/giấy/Journal/ID/schema4; không đổi An hoặc model đàn/mảnh ở nơi khác. [Worklog/ảnh/giấy phép](DONGSON_DRUM_WORKLOG.md).

[12Editor](reports/DongSonDrums_Editor_2026-11-06.json), [33Play](reports/DongSonDrums_PlayMode_2026-11-06.json), [40delivery](reports/DongSonDrums_Delivery_2026-11-06.json) đạt,0failures/0runtime errors ở bài cuối. API gõ sai/làm lại/chuỗi đúng, ray chọn collider, cửa/thưởng và native save/menu/Continue đã thử; fixture đèn/phong ấn/vị trí, không E tay/toàn tuyến. Nguồn/meta/prefab overrides/save cá nhân được kiểm, C2/NavMeshC1/PlayerMotor/script puzzle giữ SHA. [Editor cuối](reports/DongSonDrums_EditorState_2026-11-06.json) dừngPlay/Boot sạch. Chưa build lại Windows trong lượt này; executable trước vẫn dùng model cũ. Cần duyệt hình/người mới bằng tay; không coi model mới là âm gõ xác thực hoặc bằng chứng hiện vật khảo cổ cụ thể.

## C2 lượt7 — đồ họa/âm thanh/HUD và Windows —06/11/2026

Đã thực hiện “Tiếp theo”. [Worklog/ảnh](C2_ENVIRONMENT_WORKLOG.md), [asset/âm thanh](CHAPTER02_ASSET_CREDITS.md). Scene03 có bản dựng đủ năm khu: vách kín/đá/cây theo cụm, nhà sàn mái cong/chuông, cầu và trụ đậu, bệ/cổng A24/A22, cổ vật và Chim Lạc cách điệu tự tạo. Giữ An, collider tuyến, hai khe cầu, ID/schema4 và NavMesh C2. Chưa xem đây là duyệt mỹ thuật/rig cuối hoặc nghiệm thu người mới10–15phút; C3 chưa triển khai.

`Chapter02EnvironmentArt/ArtSetup` chỉ author C2. `Chapter02Atmosphere`, `C2_HUD_Theme` có tên khu/tiến độ riêng, nguồn gió3D và cue theo FSM; không ghi state hoặc điều khiển actor. Sửa hint đè thanh đèn; InventoryPanel C2 hiển thị đúng mảnh Mặt Trời Lông Chim/năng lực/công cụ và trạm chế C2, nhánh C1 giữ cũ. Palette C2 dùng group Music/SFX/UI hiện có, cue/nhạc tổng hợp có script nguồn, crossfade giao chiến/pausing; không tải hoặc mua bộ mới.

[10Editor](reports/C2_Art_Editor_2026-11-06.json), [77Art/UI/audio Play](reports/C2_Art_PlayMode_2026-11-06.json), [140tuyến P1–P3](reports/C2_Art_Traversal_2026-11-06.json), [123cầu/quái ném](reports/C2_Art_Bridge_2026-11-06.json), [87đấu trường](reports/C2_Art_Arena_2026-11-06.json), [71kết chương](reports/C2_Art_Ending_2026-11-06.json) đạt,0failures/0unexpected runtime errors ở các report cuối. E/RMB/W/Space/LMB qua Input System thật trong các bài tương ứng; camera40góc, hai khe/hai gờ, state/save lỗi/thử lại/Continue có hồi quy. C1 completion/vị trí đầu/aim/win dùng fixture tùy bài, AI cô lập; chưa một lượt tay liền mạch toàn game. Helper đọc progress chia sẻ file, probe cầu chờ focus và đi vòng capsule NPC; đấu trường cần Game View phóng để nhận focus, không bỏ guard input.

[Windows development](reports/C2_Art_Build_2026-11-06.json) ở `Builds/Chapter02/Windows/SonDoong.exe`; [20kiểm standalone](reports/C2_Art_Windows_2026-11-06.json) có Boot/NewGame/C1→C2/phục dựng/ba câu/kết/menu/Continue, save cá nhân trả nguyên bytes. Có số liệu120frame/góc đứng yên P1/P3/P5,1920×1080/PC (Vừa), RTX3060Laptop/i5-12500H; không chứng nhận60FPS toàn tuyến hoặc máy tham chiếu đã thống nhất. Lượt cold build có485cảnh báo kernel compute; build cuối incremental0errors/0warnings, không tuyên bố sạch cold build. Log thoát đầu có GPU buffers/119allocations; stack trace chỉ GPU Resident Drawer. `PlayerGraphicsShutdown` dùng API public disabled→reinitialize **chỉ khi player quit**, không sửa pipeline trong lúc chơi/Editor. [Audit log](reports/C2_Art_WindowsLogAudit_2026-11-06.json) và diagnostic run xác nhận hết các cảnh báo buffer/TempJob đó sau sửa.

[Delivery](reports/C2_Art_Delivery_2026-11-06.json) kiểm SHA hai scene C1/PlayerMotor/NavMesh C2/C1 palette, metadata/save; [Editor cuối](reports/C2_Art_EditorState_2026-11-06.json). Backup theo `Logs/C2Art-backup.json`. Còn cần duyệt hình/animation Chim Lạc, nghe cân âm, trận tay200HP/toàn tuyến và thời lượng người mới; nguồn license riêng A22/A24 vẫn theo hồ sơ Sketchfab chưa đủ chi tiết. Không tự chuyển sang dựng C3.

## C2 lượt6 — phục dựng/Linh Thần/kết chương —06/11/2026

Đã thực hiện yêu cầu “tiếp theo”. [Worklog/ảnh](C2_ENDING_WORKLOG.md), [hàng đợi](CHAPTER02_MECHANICS_PLAN.md). **Tiếp theo lượt7: đồ họa/âm thanh/UI C2 và nghiệm thu**, chờ yêu cầu; không dựng C3 hoặc thay An.

`RelicAltar` commit/save trước công bố, rollback khi lỗi và chặn gọi lồng; C2 có thẻ phục dựng riêng dẫn sang Linh Thần. `QuizSequence` có context1/3–3/3, Esc/restore hủy lượt cũ, chống E lặp, pending-save và E thử lưu sau câu cuối; chưa ghi được thì chưa mở cổng. Đọc lại lời Chim Lạc không trao thưởng/event. `ChapterMemoryNotes`/Journal giữ lời “Vành của ta đứng thứ nhất” theo cờ hoàn tất, không đổi schema4/ID. `ChapterExit_C2` lưu túi mới nhất trước thẻ kết; lỗi giữ game để thử lại; nói rõ C3 chưa có và về menu/Continue lại đỉnh tổ.

[7Editor](reports/C2_Ending_Editor_2026-11-06.json)/[71Play Mode](reports/C2_Ending_PlayMode_2026-11-06.json) đạt,0failures/0unexpected runtime errors,2diagnostic I/O dự kiến. Thiếu từng mảnh/trùm, save chưa ready/file khóa, retry/Esc/restore/world addition, điểm/cờ sai/không thưởng lặp, native lưu giữa/cuối/menu/Continue, Nhật ký/mốc an toàn đã thử. **E thật qua InputReader/scanner** tại bệ/Linh Thần/thử lưu/cổng; chọn đáp án và nút thẻ qua production API/listener. Hồi quy hẹp shared C1 altar/save lỗi/Phúc/nút chuyển C2 đạt. C1/C2 progression và boss đã thắng là fixture, AI dừng; chưa full C2 tay, cân nhịp/FPS/restart process hoặc Windows mới. Bệ/Linh Thần/cổng/art C2 còn placeholder.

Save primary/bak/tmp và Console Error Pause trả nguyên trạng. Hai scene C1, PlayerMotor/NavMesh C2 giữ SHA; [delivery](reports/C2_Ending_Delivery_2026-11-06.json), [Editor cuối](reports/C2_Ending_EditorState_2026-11-06.json). Backup `SourceAssets/SceneBackups/2026-11-06_C2Ending/004128`. Lỗi đầu bài thử E do harness không chờ An tiếp đất đã sửa ở harness, không bỏ guard game.

## C2 lượt5 — cơ chế đấu trường Chim Lạc —06/11/2026

Đã làm theo yêu cầu “tiếp theo”. [Worklog/ảnh](C2_ARENA_WORKLOG.md), [hàng đợi](CHAPTER02_MECHANICS_PLAN.md): **tiếp theo lượt6, phục dựng/Linh Thần/kết C2**, chờ yêu cầu. Cân nhịp người chơi thật, full camera/art/audio/model trùm/FPS và Windows còn ở lượt7; C3 chưa triển khai.

`BossChimLac` có lấy độ cao giữa hai lao pha2, dấu điểm đáp, sweep vật cản/LOS, đứng đất đúngfloorY, đọc enrage trong nhịp đậu, quay về An; pause/disable/reset xóa hoặc giữ cảnh báo đúng trạng thái. Giữ200HP, Focus1.5s từ hover, stun4s/x1.5/cap60, gust.8+2s, dive1+.4s, grounded1.5s, flight1s. `BossBase` chặn damage trước kích hoạt/sau chết/bay, phân biệt DefeatSavePending và TrySaveDefeat. Thắng mà chưa ghi được file vẫn mở lối trong phiên, có UI báo/thử lại, không báo giả đã lưu. ArenaGate guard cửa C2/Pause/dead/disabled boss. Focus collider không còn làm vật cản dưới chân An; chỉ ignore cặp với controller, không đổi collision matrix.

Scene03 có combat proxy chim do dự án tạo dưới `VIS_ChimLac_Combat`, không tải asset hoặc thay An; pose cánh/cổ/sao theo FSM, root vẫn một controller. `ChimLacArenaHUD`: thanh máu, tiến độ Focus, nhãn pha/hướng dẫn nền tối gần đáy và retry-save panel qua IUiPauseOwner. Dấu đáp không collider; NavMesh/C1 pipeline giữ nguyên, mesh mới dùng fallback render C2 hiện có. Không coi proxy là model/rig cuối.

[Editor](reports/C2_Arena_Editor_2026-11-06.json): **8checks,0failures**. [Play Mode cuối](reports/C2_Arena_PlayMode_2026-11-06.json): **87checks,0failures,0runtime errors**. Có kiểm gate/điều kiện gió, focus/pause/grace/delta lỗi, stun/cap/reset, gust thật có/không cột che, swept dive/né/không homing/vật cản, hai cú pha2/từng hit20, disable sạch. Native trước trận/death/Respawn khôi phục200HP/Waiting/cổng/focus/spawn; save thất bại có UI thử lại, Retry lưu được và Continue không hồi sinh trùm. **Input thật qua InputReader:** W qua trigger, RMB scanner gây choáng trên hover, Space/W/Shift xuống áp sát, LMB CombatController gây damage. [Input riêng19checks](reports/C2_Arena_Input_2026-11-06.json) có trước chỉnh tương phản HUD;87cuối đã chạy lại sau chỉnh.

C1 completion/C2 prerequisites/vị trí đầu/aim camera/damage chiến thắng là fixture; AI P4 dừng, đòn riêng gọi coroutine production sau cô lập vòng AI; pha2 dùng vòng AI thật. Không gọi đây là trận200HP tay/toàn C2 đã cân. Save primary/bak/tmp và Console Error Pause trả nguyên trạng. Hai scene C1, PlayerMotor, NavMesh C2 giữ SHA. Shared BossBase/ArenaGate sửa, chưa full C1 regression; hợp đồng Waiting không nhận damage thay ca reset-damage lịch sử lượt0. [Delivery](reports/C2_Arena_Delivery_2026-11-06.json), [Editor cuối](reports/C2_Arena_EditorState_2026-11-06.json). Backup `SourceAssets/SceneBackups/2026-11-05_C2Arena/234952` trước sửa.

## C2 lượt4 — Cầu Mây Gãy và Hắc Ảnh Ném —05/11/2026

Đã làm theo yêu cầu “Lượt tiếp theo”. [Worklog/ảnh](C2_BRIDGE_WORKLOG.md), [hàng đợi](CHAPTER02_MECHANICS_PLAN.md). Lượt5 lúc đó là phần tiếp nối và hiện có kiểm chứng ở trên. C2 còn greybox, chưa full art/audio hoặc Windows mới.

Scene03 dùng Ghost A08/animation dự án cho hai tuần tra và hai quái ném; bật choáng4s/immunity3s, nhả chiếu để choáng lại, sao/cúi đầu, giữ màu thân. ThrowerPresentation đọc FSM, không thêm motor/root motion. Thrower về cấu hình spec14m/.6s/2.5s, đạn7m/s/12damage, melee10/hồi1.6s; LOS từ tay, gần mà đang hồi melee không xen ném. EnemyProjectile xử lý overlap đầu/Environment ưu tiên, consumed trước damage, chặn launch lỗi; receiver chặn delta NaN/Infinity. RequirementDoor chỉ công bố mở sau lưu thành công, lỗi rollback và thử lại được. Giữ collider cánh cửa Environment, E vẫn tìm script ở cha.

Thêm hai khối nấp `ART_C2_Bridge`, bake asset NavMesh C2 cũ; không nới jump/motor hoặc lấp khe cầu. Lỗi render BatchDrawCommand xuất hiện trong phiên chuyển scene; C2 thêm component Unity DisallowGPUDrivenRendering áp root/con để dùng đường vẽ thường, không đổi pipeline/C1. Có nguồn Unity và giới hạn FPS trong worklog.

[Editor](reports/C2_Bridge_Editor_2026-11-05.json): **25checks,0failures**. [Play Mode tích hợp](reports/C2_Bridge_PlayMode_2026-11-05.json): **117checks,0failures,0unexpected runtime errors**;1diagnostic I/O dự kiến khi khóa file tạm. Đã thử đạn/vách/overlap/pause/expiry, AI hai thrower/choáng/recovery/hướng model/không tint; từng điều kiện thiếu cửa, save-not-ready và khóa file không mất save chính, mở lại/menu/Continue/Respawn. **E/RMB/W/Space qua InputReader thật**: chọn cửa/chiếu quái và vách chắn, hai khe1.5m, hai gờ y21, trigger checkpoint ghi file mới. [Input riêng44checks](reports/C2_Bridge_Input_2026-11-05.json) cũng đạt. Helper chọn đúng Unity hiện tại/Ctrl+2, giữ guard focus; Console Error Pause tạm tắt cho ca lỗi I/O dự kiến rồi trả giá trị gốc.

Vị trí đầu bài/C1 completion là fixture; AI dừng để đo từng quái hoặc đi cầu, bật lại kiểm spawn an toàn. Không phải chơi tay/cân bằng toàn P4/P5, full C2 hoặc kiểm FPS. Save primary/bak/tmp trả byte-for-byte; hai scene C1, PlayerMotor và EnemyPresentation giữ SHA. Shared door/projectile/receiver có sửa, chưa chạy lại toàn tuyến C1. Backup `SourceAssets/SceneBackups/2026-11-05_C2Bridge`; [Delivery](reports/C2_Bridge_Delivery_2026-11-05.json), [Editor cuối](reports/C2_Bridge_EditorState_2026-11-05.json).

## C2 lượt3 — chuông, bia, quiz và phần thưởng —05/11/2026

Đã làm lượt3 theo yêu cầu “thực hiện tiếp”. [Worklog/ảnh](C2_KNOWLEDGE_WORKLOG.md), [nguồn tri thức](C2_KNOWLEDGE_SOURCES.md), [hàng đợi](CHAPTER02_MECHANICS_PLAN.md). Lượt4 lúc đó là phần kế tiếp và hiện đã đạt ở trên. Không dựng C3 hoặc thay An; C2 còn greybox, art/audio đầy đủ ở lượt7.

Scene03 thêm `ART_C2_Knowledge`, giấy tùy chọn `c2_clue_bells`, dấu lông tương ứng chuông/phù điêu2–3–1, P3 yêu cầu giải chuông trước bệ. Quiz có card/vùng giải thích cuộn/nút tách rõ. Nội dung5bia/7câu đối chiếu nguồn trực tiếp; `Chapter02KnowledgeBuildGate` kiểm riêng C2. Sửa hướng dẫn bia P1 nằm đáy giếng, bệ ở gờ cao. NavMesh C2 bake vào asset cũ, giữ GUID.

`RewardService`, `InventoryService`, `WorldStateStore`: toàn bộ item/world/claim commit trước event, thiếu chỗ/ID lỗi không trao gì; chống gọi thưởng lồng và claim lặp. `QuizPedestal` đưa claim vào giao dịch; P4 đã trả lời mà túi đầy giữ đủ tool/thuốc để nhận sau. `QuizPanel` bỏ static wrongThisSession, dùng Flags `quiz_wrong_<quizId>` theo snapshot checkpoint, chặn index lỗi/chọn khi chờ retry; Answered/FirstTry cùng batch. Không autosave khi sai, không đổi schema4/ID cũ. Sai chưa từng được lưu trong bản trước không thể suy ngược.

[Editor](reports/C2_Knowledge_Editor_2026-11-05.json): **23checks,0failures**, cấu hình/source, missing scripts, đường NavMesh cạnh giấy và build gate từ chối fixture chưa verified. [Play Mode](reports/C2_Knowledge_PlayMode_2026-11-05.json): **110checks,0failures,0runtime errors**. Năm bia kiểm đèn/tầm/vách/Journal; chuông sai/đúng và giải khác nhận mảnh; giấy/bệ có floor/capsule/ray trong3m. Bốn bệ/bảy câu kiểm sai/thoát/retry/không lặp; batch nhiều item đủ/thiếu chỗ, observer thấy toàn giao dịch, chống reentry. Native SaveAt/Menu/Continue kiểm P4 answered-unclaimed khi đầy túi, nhận đúng tool+2thuốc sau có chỗ, không nhân đồ khi Continue; cờ sai/giấy/lore/chuông được giữ khi scene/panel mới. Fresh snapshot xóa phạt cũ; save legacy Answered không có điểm không kiếm lại điểm qua bệ.

Phép thử dùng API/nút UI và vị trí fixture/physics, AI dừng, C1 completion là fixture; **không E tay, toàn tuyến C2, restart process hoặc Windows mới**. Save cá nhân primary/bak/tmp trả byte-for-byte. Backup `SourceAssets/SceneBackups/2026-11-05_C2Knowledge/192230`; [bàn giao SHA/meta](reports/C2_Knowledge_Delivery_2026-11-05.json), [Editor cuối](reports/C2_Knowledge_EditorState_2026-11-05.json). Ba bia C1 và QuizSequence kết C2/save lỗi không nằm trong nghiệm thu lượt3. Bản Windows C1 cũ chưa chứa sửa C2 hiện tại.

## C2 lượt2 — cột gió/tuyến P1–P3 —05/11/2026

Đã thực hiện theo lệnh tiếp theo. [Worklog/ảnh](C2_TRAVERSAL_WORKLOG.md), [hàng đợi](CHAPTER02_MECHANICS_PLAN.md). Lượt3 lúc đó là phần kế tiếp; hiện đã đạt ở mục trên.

Scene03 có `ART_C2_Traversal` với chín dấu đáp/lối trống, vật liệu URP trong suốt riêng cho7visual cột gió để thấy bệ phía sau; không đổi trigger/ability/tool. Camera C2 thêm `CaveCameraCollisionGuard` Cinemachine3 sẵn có. Vách/cửa/rào hiện hữu đủ khoảng trống trên tuyến đã thử, không cần dựng cầu tắt hay sửa thông số nhảy. Hai scene C1, PlayerMotor/WindColumn và NavMesh C2 **giữ SHA so với trước lượt**. Không thay An, ID/save/schema4 hoặc tải thêm asset.

[Editor](reports/C2_Traversal_Editor_2026-11-05.json): **20checks,0failures**, missing scripts0, guard/material/clearance tại cửa/lối trống và hai spawn. [Play Mode](reports/C2_Traversal_PlayMode_2026-11-05.json): **150checks,0failures,0runtime errors**. Di chuyển W bằng Input System/InputReader/PlayerMotor thật, bốn lần Space rời hover và đáp ở5/9/13/18m; pause đóng băng lift;40góc camera tại5điểm; chuông2–3–1 bật cột cuối; tới lối P4. Không teleport giữa bốn đoạn leo. Checkpoint giữa lưu qua SaveService thật; fixture rơi dưới map gây chết qua KillZone, Respawn và menu/Continue giữ hai mảnh đầu/gió, bỏ mảnh3/chuông/cột cuối chưa lưu.

Người dùng xác nhận focus; bài thử dùng helper kích hoạt đúng cửa sổ Unity, tự Pause/Resume khi OS mất focus, không bỏ guard sản phẩm. Tương tác pickup/quiz/chuông/checkpoint qua API sau kiểm tầm3m/LOS; C1 hoàn tất là fixture hợp lệ, AI chiến đấu dừng. Một số lượt phát triển harness dừng do focus, điểm đứng nằm trong collider pickup hoặc fixture thiếu điều kiện C1; giữ trong Logs, không coi là lỗi gameplay đã sửa. [Bàn giao](reports/C2_Traversal_Delivery_2026-11-05.json) xác nhận save chính/bak/tmp trả nguyên bytes, meta giữ GUID và SHA những phần không đổi. Chưa thử full P4/P5, tất cả đường tùy chọn/quay ngược, chơi tay/cân bằng/FPS hoặc build Windows mới; art C2 còn greybox và sẽ làm ở lượt7.

## C2 lượt1 — chuyển chương và mở đầu —05/11/2026

Đã thực hiện lượt1 theo yêu cầu “thực hiện tiếp”. [Worklog/ảnh](C2_OPENING_WORKLOG.md), [hàng đợi](CHAPTER02_MECHANICS_PLAN.md). Lượt2 hiện đã đạt ở trên; các giới hạn bên dưới thuộc mốc lượt1.

Phát hiện cổng C1 tại(-7,0,307) bị vách/trụ29–30 bao kín sau mở rộng; chuyển nguyên root `ChapterExit_C1` ra sân(-6,0,300) trong Main/Preview, giữ script/ref/điều kiện/đích. Lời dẫn Kim Quy nhắc luồng gió tới Chim Lạc. C2 có `ART_C2_Opening`, giấy `c2_guide_wind` đọc E/J, lông low poly của dự án tại(1.6,1.1,-2.4), dấu đường/vùng sáng và hai mục tiêu mở đầu. Story/Journal dùng khung cuộn riêng để chữ không đè nút. AbilityPickup giữ listener sống để restore trước nhặt hiện lại đúng hình/collider; GameSession đặt transfer snapshot trước LoadScene và chặn chuyển lúc pause/menu. Giữ ID cũ, schema4, model/rig An; không tải asset mới.

[Editor](reports/C2_Opening_Editor_2026-11-05.json): **21checks,0failures**; cổng có capsule/ray thông và NavMesh nối từ checkpoint cuối, ba scene không missing scripts, NavMesh C2 giữ asset riêng. [Play Mode](reports/C2_Opening_PlayMode_2026-11-05.json): **74checks,0failures,0runtime errors**, dùng SaveService thật tạo fixture C1 trước phục dựng, bệ/cổng/nút cuối C1 thật, chuyển scene và menu/Continue thật. Giữ đúng2thuốc/3lá/2chi, đèn, Phúc Kim Quy, lore/giấy C1; save `c2_cp_start`; nhặt giấy/lông, restore trước nhặt, lưu ở lửa và Continue giữ dữ kiện. Cổng bị khóa trước phục dựng; pause không cho chuyển; kiểm vị trí lông cạnh An và vùng chữ cuộn tách nút.

Đây là API/physics aim và vị trí fixture, **không E bằng bàn phím, đánh hết C1, đi toàn tuyến C2 hay Windows mới**. C1 prerequisites/boss được đặt bằng fixture; save cá nhân chính/bak/tmp trả byte-for-byte sau từng lượt thử. Lượt đầu dừng ở cổng đã được giữ riêng, không che lỗi bằng bỏ kiểm tra. Bản Windows C1 ở dưới có trước sửa C2 hiện tại. Backup `SourceAssets/SceneBackups/2026-11-05_C2Opening`; stage2–7 chưa nghiệm thu.

## Chương2 — chia việc, rà soát và sửa cơ chế có sẵn —05/11/2026 (lượt0)

Người dùng chọn **chia việc, kiểm tra và sửa phần đã có trước**. [Lộ trình C2](CHAPTER02_MECHANICS_PLAN.md) chia7lượt; ở thời điểm lượt0, phần kế tiếp là1. Lượt1 hiện đã đạt ở trên. Không tự triển khai C3. Scene03 vẫn là blockout; lượt0 không thay đồ họa/An, ID/schema4 hoặc nội dung scene.

Đã sửa `ThrowerBrain` đánh xuyên tầng/phụ thuộc telegraph hình ảnh, hủy đòn khi receiver được cấu hình choáng; `ChimLacFocusReceiver` buộc dùng cột gió khi chim đậu, chặn tiến độ trong pause/delta lỗi; `BossBase/BossChimLac` reset sạch bất tử/trần choáng/FSM/tiến độ/telegraph; `WindColumn` đồng bộ khi disable/enable và đọc đúng actor. `WorldStateStore.RestoreVersion` chỉ trong RAM giúp `SequencePuzzleController` hủy input đang chờ khi thay snapshot, không hủy lượt khi world có dữ kiện khác. Receiver C2 hiện **chưa bật choáng trong scene**; probe bật bằng fixture để kiểm hợp đồng, không tự bổ sung art/sao/animation quái C2.

[Editor](reports/C2_Mechanics_Editor_2026-11-05.json): **21 checks,0failures**, xác nhận7cột gió,4bệ,5bia,3chuông,2quái ném,1boss,3checkpoint đúng catalog và refs. [Play Mode](reports/C2_Mechanics_PlayMode_2026-11-05.json): **56 checks,0failures,0runtime errors**; trigger nâng/lơ lửng/ra cột thật, chuông sai/đúng/restore, bốn quiz và thưởng không lặp, quái ném, Focus trên cao/pause/grace, stun60damagecap và reset giữa choáng/bay. API/vị trí fixture, không phím E/RMB tay; dừng AI khác và autosave trigger để cô lập. Save cá nhân chính/bak/tmp được bảo vệ và trả byte-for-byte.

Chưa thử đi toàn tuyến C1→C2 hiện hành, nhảy cầu/tất cả bệ bằng input, pha cuồng nộ/giết boss/lưu-thua-thắng cuối C2 hoặc Windows mới. Bản Windows C1 dưới đây **có trước sửa C2 này**. Khoảng thiếu còn giữ: điểm quiz sai xuyên restart, giao dịch thưởng nhiều loại item, xử lý save lỗi ở QuizSequence, xác minh nguồn tri thức, cân bằng/đồ họa/audio. Backup `SourceAssets/SceneBackups/2026-11-05_C2MechanicsAudit`; scene03/meta giữ SHA so với backup. Kết quả này thay dòng “chưa có report C2 hiện hành” trong kiểm kê cũ, không biến nhật ký bot cũ thành nghiệm thu mới.

## Giấy hướng dẫn và căn bệ/vật phẩm —05/11/2026

Đã ghép Main/Preview, backup `SourceAssets/SceneBackups/2026-11-05_GuidesPedestals`. [Editor report](reports/C1_GuidesPedestals_Editor_2026-11-05.json) xác nhận15mesh A24 mỗi scene được căn đúng tâm/chạm nền, không còn `Top_Bronze_Rim`, ba mảnh đặt giữa và cách mặt bệ2,5cm. Nguyên nhân lệch là co Fit sau khi bù origin mà không căn lại tâm; đã sửa cả factory và instance hiện hữu bằng mesh bounds biến đổi, kể cả reward chưa active.

Thêm hai giấy tại trại (giới thiệu và điều khiển/lưu/chế thuốc), một giấy chuẩn bị trước Đàn Kim Quy; tổng5giấy, dùng chung E/Story/Journal và Flags. Ba ID mới `c1_guide_camp_intro`, `c1_guide_camp_controls`, `c1_guide_kim_quy`; ID cũ và schema4 giữ nguyên. Bệ đèn trước đây chỉ còn collider do renderer tắt; nay có A24 độc lập với pickup, các crystal/checkpoint/giấy được căn lại. Story mở ở đầu trang cho nội dung dài.

[Play Mode](reports/C1_GuidesPedestals_PlayMode_2026-11-05.json): **238 checks, 0 failures, 0 runtime errors**; [toàn tuyến](reports/C1_GuidesPedestals_Traversal_2026-11-05.json): **542 checks, 0 failures, 0 runtime errors**. Bệ đèn còn sau khi nhặt và Continue; năm giấy được thu thập/đọc lại/lưu. Unity mất focus nên phần tương tác của lượt này dùng API và physics ray, không xác nhận thao tác E/RMB bằng tay; guard input runtime giữ nguyên. Bài toàn tuyến dừng AI và dùng cờ thắng boss để cô lập đường đi/save. [Kiểm tra tiếp cận](reports/C1_GuidesPedestals_Access_2026-11-05.json) xác nhận năm giấy ở cả Main/Preview có chỗ đứng đủ khoảng trống capsule, nền đi được và tia tương tác không bị che.

[Windows x64](reports/C1_GuidesPedestals_Build_2026-11-05.json): **Succeeded, 0 errors, 485 shader warnings** từ AI Inference/Sentis; [BuildReport](reports/C1_GuidesPedestals_NativeBuildReport_2026-11-05.json). Bản cập nhật tại `Builds/Chapter01Revision/Windows/SonDoong.exe`, giữ cùng Data/DLL/CREDITS. [Bàn giao](reports/C1_GuidesPedestals_Delivery_2026-11-05.json) xác nhận SHA, credit và save cá nhân trả nguyên trạng; [Editor cuối](reports/C1_GuidesPedestals_EditorState_2026-11-05.json) dừng Play, mở Main sạch. Chưa chơi tay bản Windows hoặc nghiệm thu FPS. [Chi tiết và ảnh](C1_GUIDES_PEDESTALS.md); báo cáo các mục bên dưới là lịch sử trước bước này.

## Bỏ hiệu ứng màu suy yếu, giữ sao choáng —05/11/2026

Theo xác nhận mới nhất: `EnemyLanternReceiver` không còn ghi tint vào material; `EnemyPresentation` không còn thêm emission theo trạng thái suy yếu. `EnemyStunIndicator` giữ ngôi sao khi choáng. Cơ chế suy yếu/sát thương×1,5, choáng4s/2s, cúi đầu và khóa đòn giữ nguyên. Các ref serialized tint/surfaces cũ giữ ẩn để tương thích prefab/builder, không điều khiển material nữa.

[Play Mode riêng](reports/C1_NoWeaknessVisuals_PlayMode_2026-11-05.json): **52checks/0failures/0errors**, kiểm sao vẫn hiện, màu/emission của FBX trước/sau không đổi, multiplier×1,5 và khóa đòn/miễn choáng/pause cho cả hai actor. Focus cấp qua receiver API trong bài kiểm hình này; combat có fixture như ghi trong report. Save chính/bak/tmp và thiết lập cũ đã trả lại. [Windows cập nhật](reports/C1_NoWeaknessVisuals_Build_2026-11-05.json): **Succeeded/0errors/0warnings**, cùng đường dẫn `Builds/Chapter01Revision/Windows/SonDoong.exe`; chưa chơi tay bản Windows. [SHA và save](reports/C1_NoWeaknessVisuals_Delivery_2026-11-05.json); [Editor cuối](reports/C1_NoWeaknessVisuals_EditorState_2026-11-05.json) mở Main sạch, đã dừng Play. Các báo cáo Revision bên dưới thuộc mốc trước thay đổi này.

## Revision lửa/trống/gương/túi đồ C1 — 05/11/2026 (trước bỏ màu suy yếu)

Main và Preview đã có nhóm `World_Geometry/ART_C1_Revision`, mô hình tương tác vẫn nằm dưới root gameplay cũ. Chi tiết file/công cụ và bằng chứng ở [C1_REVISION_WORKLOG](C1_REVISION_WORKLOG.md). Không thay model/rig An.

- Bỏ bàn C1, thêm nút **I → Chế bình máu**, công thức1 Lá cẩm+1 Linh chi→1 Bình máu; giao dịch kiểm sức chứa sau khi trừ nguyên liệu, debounce0,25s unscaled, không autosave. E lửa giữ lưu riêng.
- Lửa particle chuyển động/khói/tàn/ánh dao động, ghế WoodLog dùng scale đều, bỏ vật cản của ghế cũ. Ba A20 có dấu Mặt Trời/Cánh Chim/Sóng Nước, gỡ `DrumVisualLabel` tự sinh A/B/C; enum/ID nội bộ giữ nguyên.
- A22 cửa có hoa văn đồng và đá khép lên tới6,15m; A23 gương được fit theo mặt phẳng riêng trước yaw, mặt bạc/đồng; A24 bệ cho ba mảnh và checkpoint. FBX tối ưu riêng, không sửa asset nguồn. Các refs đường đá bị bevel hai lần đã trả về mesh còn nguyên; generator chặn lặp.
- Hai giấy gợi ý tùy chọn nhặt bằng E, mở Story và giữ ở Nhật ký bằng Flags mới; không đổi schema4. Có scrollbar, icon túi và card UI mới. URP fog Exp2+năm bank particle thưa.
- Sửa visual yaw180 của cả hai Hắc Ảnh/Hộ vệ; `EnemyLanternReceiver` choáng4s/2s, immunity3s và lượt chiếu mới, cúi đầu/dấu sao/khóa đòn. Hắc Ảnh xóa path/velocity NavMesh khi choáng. Focus callback của InputReader hoãn đọc binding tới Update để tránh lỗi khi Unity tháo device/đổi layout.
- [Stun/combat](reports/C1Revision_Stun_2026-11-05.json): **50 checks** sau sửa marker bị thân che ở cự ly gần; [Integration](reports/C1Revision_PlayMode_2026-11-05.json): **213 checks**; [Traversal](reports/C1Revision_Traversal_2026-11-05.json): **542 checks**. Tất cả **0 failures/0 errors**. Toàn tuyến giải trống/gương thật, ba mảnh, cổng, checkpoint/Continue/rơi/hồi sinh; bài Integration kiểm death boss qua LMB/CombatController. Lượt Stun cuối mất focus ở đoạn chém nên kiểm boss death qua EnemyHealth API, ghi rõ trong report. Phép thử có fixture vị trí, mục tiêu đứng yên ở bài tầm chiếu5m và bất tử khi chém boss; khoảng kiểm khóa damage do choáng dùng người thử nhận damage thật.
- [Editor validation](reports/C1Revision_EditorValidation_2026-11-05.json): Main/Preview không missing scripts/material lỗi, đủ5A22/2A23/3A20,4spawn trên NavMesh, crown collider khép. [Metadata](reports/C1Revision_Metadata_2026-11-05.json):15meta trong backup trước revision giữ SHA; [Editor log](reports/C1Revision_EditorLog_2026-11-05.json) không có CS error/exception/assert/meta-write failure mới từ mốc Integration.
- Save cá nhân chính/backup/tmp được trả byte-for-byte sau các probe; PlayerPrefs được trả sau Integration. [Windows Revision cuối](reports/C1Revision_Build_2026-11-05.json) **Succeeded,0errors,0warnings** ở lượt incremental sau chỉnh marker; size292.831.876bytes. Lượt build đầy đủ trước đó có485cảnh báo Sentis, giữ trong `C1Revision_*_FullShaders_2026-11-05.json`; cache khiến lượt cuối không phát lại, không coi đã sửa shader package. Chạy `Builds/Chapter01Revision/Windows/SonDoong.exe` với Data/DLL/CREDITS cùng thư mục. Các kết quả Completion/Expanded dưới đây là lịch sử trước revision.

Người dùng xác nhận A22/A23/A24 từ Sketchfab free; URL/tác giả/giấy phép từng mẫu chưa nằm trong gói, đã ghi đúng mức biết được ở credit. Chưa cần thêm asset. Chưa nghiệm thu người chơi mới, FPS/máy thứ hai hoặc chơi tay bản Windows.

[Bàn giao cuối](reports/C1Revision_Delivery_2026-11-05.json) ghi SHA scene/EXE/runtime DLL, xác nhận sáu class runtime mới có trong bản chơi, bốn probe Editor bị loại, credit trùng nguồn và save cá nhân đã trả đúng hash. [Editor cuối](reports/C1Revision_EditorState_2026-11-05.json): Play=False, compile=False, mở `02_HangLinhMach`, dirty=False. [Ảnh kết quả và chi tiết](C1_REVISION_WORKLOG.md#ảnh-kết-quả).

## Hoàn thiện C1 theo ảnh lỗi — 05/11/2026 (trước revision)

Người dùng đã giao làm cả Hắc Ảnh/Hộ vệ, đèn sen, UI/audio/cửa và vật cảnh C1 bằng asset miễn phí đã nhập; **chỉ loại trừ model/rig An**. Không áp giới hạn chờ làm quái của lượt cũ lên công việc này. Chi tiết tại [CHAPTER01_COMPLETION_WORKLOG](CHAPTER01_COMPLETION_WORKLOG.md).

- Ba metadata DungeonModularPack đã ImportAsset + SaveAndReimport thành công, giữ GUID; [báo cáo](reports/C1Completion_Metadata_2026-11-05.json). Không có bằng chứng file readonly lúc kiểm tra, không khẳng định sai nguyên nhân cũ; không xóa meta/Library/ACL.
- Main + Preview có tám đầu nối hang khép kín, collider mặt ngược cho năm vách đá mỏng, NavMesh bake lại. Teleport báo warp cho Cinemachine3, có kiểm tra va chạm cuối pipeline và ẩn body lúc camera ép sát. Giữ giếng trời giữa khoang và toàn bộ root/ID/ref/save.
- Ghost A08 có năm clip; Golem Kevin có sáu clip bone do dự án tạo vì nguồn không kèm animation. EnemyPresentation theo FSM; AI/damage/motor hiện hành giữ nguyên. Cửa A10/sigil, tinh thể/bình A17, nấm A19/cây A11, mảnh đồng và đèn sen mới dưới object gameplay cũ. Không cần thêm asset trả phí.
- HUD mở đầu Vườn Rêu, 3 mảnh/2 câu đố; menu/settings/pause/death/confirm/toast/túi/nhật ký/story/boss bar đồng bộ, chữ Noto Sans. Ba slider âm lượng điều khiển mixer dB; có fallback khóa cũ, debounce flush, nhạc/ambience/crossfade/one-shots. Xem [audio](AUDIO_SETTINGS.md), [HUD](QUEST_HUD.md).
- RelicAltar rollback restoration/ability khi SaveAt false, không mở màn kết trước khi lưu thành công. Native schema4/migration legacy1–5 và WorldIds không đổi.
- [Integration](reports/C1Completion_PlayMode_2026-11-05.json): **150 checks, 0 failures/errors** sau sửa camera/collider; E qua Input System chọn riêng bàn và lửa; 80 góc camera có kiểm overlap; hoạt ảnh/quái gây damage; chuột trái qua CombatController; restoration/save lỗi/success/ending/Continue/death/respawn. Combat dùng fixture vị trí, trùm đứng yên và người thử bất tử khi chém, không là người chơi nghiệm thu cân bằng.
- [Traversal](reports/C1Completion_Traversal_2026-11-05.json): **569 checks, 0 failures/errors**, chạy lại sau collider mặt ngược: cả bốn đường nối, hai câu đố thật, ba mảnh, cổng và checkpoint/Continue/rơi/hồi sinh. Phần thắng boss ở bài này là fixture; bài Integration kiểm death/event boss qua health/combat thật.
- Mỗi probe trả save chính/backup/tmp byte-for-byte và Integration trả PlayerPrefs/chất lượng cũ. Ảnh native Game View 1920×1080: `docs/images/c1-complete-native-*`; capture UI thêm 1280×720/1600×900/1600×1000.

[Windows x64 Release mới](reports/C1Completion_Build_2026-11-05.json): **Succeeded, 0 errors, 485 shader warnings**, size theo BuildReport 282.066.148 bytes. Cảnh báo vẫn từ AI Inference/Sentis, không phải lỗi biên dịch game. Chạy `Builds/Chapter01Complete/Windows/SonDoong.exe` cùng Data/DLL/CREDITS; bản Chapter01Expanded dưới đây là mốc trước lượt hoàn thiện. Chưa nghiệm thu chơi tay Windows/máy thứ hai, FPS/bake GI hoặc cân âm trên loa; nhạc A38 và ba note A33 vẫn âm thử. Model An giữ nguyên theo yêu cầu.

[Bàn giao](reports/C1Completion_Delivery_2026-11-05.json) ghi SHA scene/EXE/assembly/report và xác nhận native assembly có runtime mới, không chứa ba probe Editor; credit bản chơi trùng nguồn. [Editor cuối](reports/C1Completion_EditorState_2026-11-05.json): dừng Play Mode, mở `02_HangLinhMach`, scene sạch. Các bước kiểm thử đã trả save cá nhân và thiết lập cũ.

## C1 mở rộng — 05/11/2026 (mốc nền trước lượt hoàn thiện)

Theo yêu cầu mới, đã thay địa hình hẹp bằng năm khoang lớn và bốn lối hang uốn cong trong `02_HangLinhMach`, sau khi dựng trong `92_EnvironmentPreview`. Nhóm hiện hành `World_Geometry/ENV_C1_ExpandedWorld`; hình cảnh trước đây lưu inactive. Các kích thước/tọa độ và kết quả ngày 04/11 bên dưới thuộc phiên bản trước mở rộng khi có khác biệt.

- Phạm vi dọc khoảng 330 m; hang cao 16–25 m, khoang rộng danh nghĩa 36–48 m. Đường nối có nền 10,5 m, dải lát đá giữa 4,5 m, cây/đá/trụ dẫn sáng ở mép. Cụm đá và dương xỉ đặt chạm nền; vòm có khe giếng trời.
- Asset đã nhập: A09/A10/A11/A14/A15/A16/A20; không cần thêm gói trả phí. Texture atlas A13 không được lát lặp; đã đổi sang bề mặt A14 để hết dải đen. Vật liệu, texture bản sao 1K và mesh mới ở `Assets/_Project/Art/Chapter01Expanded`.
- Giữ root/ref/ID gameplay và schema 4. Dời các khu theo nhóm, giữ hai gương cách 5 m; căn lại cổng, spawn, bounds đấu trường và KillZone. NavMesh bake lại bằng Physics Colliders. KillZone được tách khỏi nhóm cảnh lưu trữ để tiếp tục hoạt động.
- Backup scene và `.meta`: `SourceAssets/SceneBackups/2026-11-05_ExpandedC1`. SHA metadata của cả hai scene trùng bản đầu, [báo cáo GUID](reports/ExpandedC1_Metadata_2026-11-05.json).
- Công cụ mới `Assets/_Project/Editor/Chapter01ExpandedLayout.cs`; probe mở rộng trong `Assets/_Project/Tests/CampEnvironmentSmokeProbe.cs`. Không tự chạy stage/apply lên main đã mở rộng.
- Kiểm tra Editor: 206 mẫu tâm đường có nền và khoảng trống đi bộ; 149 mặt cắt kiểm tra vách/cổng đóng không có lối vòng tại các điểm lấy mẫu; bốn checkpoint có nền, material hỗ trợ, hai gương giữ khoảng cách. Cửa puzzle hẹp được căn rộng cùng vách, giữ object/collider/script gốc.
- [Play Mode](reports/ExpandedC1_PlayMode_2026-11-05.json): **569 checks, 0 failures, 0 runtime errors**, gồm Boot/New Game, chế đúng một thuốc, tia E chọn bàn và checkpoint riêng, bốn đường cong, A–C–B, tia phản xạ qua hai gương thật, ba mảnh/hai puzzle, cổng, 56 góc camera, lưu/Continue ở spawn mới, rơi vào KillZone và hồi sinh an toàn. Save chính/backup/tmp gốc được trả byte-for-byte.
- Phạm vi kiểm chứng: CharacterController/API, AI tạm dừng để đo đường; cờ thắng Hộ vệ dùng fixture cho phép thử save, không phải đánh trùm thật. Lượt đầu thiếu focus được lưu riêng; đã chỉnh đá lấn hai cửa hang. Tuyến đo trong Điện Trống được đưa về đường chính trước khi rẽ ra cửa để tránh cụm đá rìa. Check Continue chờ nhân vật ổn định chạm đất sau khi tải scene, không ép vị trí để làm phép thử đạt.
- [Windows x64 Release cuối](reports/ExpandedC1_Build_2026-11-05.json): **Succeeded, 0 errors, 485 shader warnings** từ gói AI Inference/Sentis (25 pixel shader, 460 ConvGeneric compute). [BuildReport gốc](reports/ExpandedC1_NativeBuildReport_2026-11-05.json); không sửa package ngoài phạm vi cảnh. Chạy `Builds/Chapter01Expanded/Windows/SonDoong.exe`, giữ cùng thư mục `SonDoong_Data` và DLL đi kèm. Toàn thư mục khoảng 275 MB, có `CREDITS.txt`.
- [Bàn giao](reports/ExpandedC1_Delivery_2026-11-05.json) ghi SHA scene/EXE/assembly/report, credit trùng nguồn và assembly Windows không chứa probe Editor. [Editor cuối](reports/ExpandedC1_Editor_2026-11-05.json) đã kiểm tra sau lượt hiển thị/normal map/bệ đỡ cuối. Unity đã dừng Play, mở `02_HangLinhMach`, scene sạch (dirty=False). Chưa chơi tay bản Windows.

[Bố cục, tọa độ, nguồn asset và ảnh](CHAPTER01_EXPANDED.md). Credit A20 Aaannnn/CC Attribution được giữ trong [CHAPTER01_ASSET_CREDITS](CHAPTER01_ASSET_CREDITS.txt) và `CREDITS.txt` cạnh bản Windows. Actor vẫn prototype; chưa bake GI, đo FPS/máy thứ hai hoặc nghiệm thu chiến đấu trùm bằng người chơi.

**Đồng bộ 04/11/2026, Asia/Bangkok.** Đối chiếu workspace trên nhánh `main`, HEAD `ba58ff7`, gồm thay đổi chưa commit sau merge và khu mẫu trại. Đợt đồng bộ tài liệu ban đầu chỉ đọc/đối chiếu; **lượt tiếp theo đã chỉnh cơ chế nhân vật theo yêu cầu mới**, chạy Play Mode và build Windows. Xem mục điều khiển bên dưới, không áp câu “chưa thử” trong audit cũ lên kết quả mới.

Đọc [PROJECT_MEMORY](PROJECT_MEMORY.md) để biết quyết định thiết kế; [CURRENT_ARCHITECTURE](CURRENT_ARCHITECTURE.md) để dùng đúng API/ID; [REQUIREMENTS_MATRIX](REQUIREMENTS_MATRIX.md) để tra yêu cầu và khoảng thiếu. Toàn bộ nhật ký cũ giữ tại [bản lưu trước đồng bộ](history/PROJECT_STATUS_before_sync_2026-11-04.md), không dùng các câu “hiện tại/chưa có” ở đó làm trạng thái mới.

## Quyết định hiện hành của người dùng

1. **5 chương**, ưu tiên **chương 1**. Yêu cầu 05/11 đã giao hoàn thiện cả Hắc Ảnh/Hộ vệ/UI/audio/đèn/cửa; model An vẫn loại trừ.
2. **Stylized Việt Nam**, cho phép chi tiết cao hơn, là quyết định phong cách mới nhất 06/11; low poly hoạt hình 3D là lựa chọn/bản dựng 04/11 trước đó. A09/A11/A16 đã dùng cho Trại Giếng Trời. Giữ collider nền/tuyến, gameplay root/ref/ID khi thay model.
3. **Chưa chốt hạn mới**. Lịch 7 ngày và 12 tuần chỉ là dự kiến/tham chiếu cũ.
4. Giữ **Master/Music/SFX** riêng; đã nối lại ba slider/mixer ở lượt 05/11.
5. Giữ HUD C1 bắt đầu **Vườn Rêu**, tiến độ **3 mảnh/2 câu đố**; QuestTracker đã khôi phục yêu cầu này ngày 05/11.
6. Trại C1 theo yêu cầu mới ngày05/11: **bỏ bàn chế, I mở túi để chế bình máu**, **E tại lửa lưu**; chế không tự lưu. C2 giữ altar mở panel chế.
7. **Điều khiển An nhanh, gọn, ít quán tính**, lựa chọn 04/11. Lượt 05/11 giữ cơ chế/rig An, sửa camera liên quan lỗi ảnh và làm quái đã được giao.

Các xác nhận phạm vi/lịch/audio/HUD được người dùng trả lời trong chính lượt đồng bộ ngày 04/11. Nguồn đã sửa ưu tiên trong AGENTS, MEMORY, SPEC_INDEX và bảng yêu cầu; không viết lại lịch sử hoặc tự giảm yêu cầu để khớp phần code còn thiếu.

## Kiểm kê nền ngày 04/11 (thay đổi C1 mới xem mốc 05/11 ở đầu)

| Phần | Trạng thái đã đối chiếu | Giới hạn |
| --- | --- | --- |
| Editor/package | Unity 6000.4.8f1; URP 17.4.0; Cinemachine 3.1.7; Input 1.19.0; AI Navigation 2.0.12; ProBuilder 6.0.9 | Khác 6.3 LTS trong spec gốc; không có quyết định đổi Editor |
| Code dự án | **109 file C#** trong `_Project` sau lượt art C1 (thêm Chapter01EnvironmentSetup sau lượt melee); input Gameplay/UI và cấu hình liên quan | Mã hiện diện/đọc được không phải chứng nhận nghiệm thu |
| Serialized asset | 10 scene, 35 prefab gồm Resources UI, 131 `.asset`, 154 material, 1 shader riêng, 1 controller, 9 clip, 3 mask, 1 mixer, 1 input, 1 asmdef trong `_Project` | `.asset` gồm 37 SO và dữ liệu khác; mesh/texture/audio nhập được kiểm kê metadata/ref, không kiểm nội dung nhị phân bằng Play |
| Build Settings | Bật đúng 00_Boot,01_MainMenu,02_HangLinhMach,03_GiengTroiChimLac; Windows profile dùng danh sách chung | Chưa có C3–C5; scene thử/Template không trong build |
| C1 | Năm khu, đèn/trống/gương/dao/Hắc Ảnh/Hộ vệ/cổng/phục dựng/save/UI/tri thức có code và cấu hình | Đã ghép art năm khu và thử hồi quy hình học/API; actor còn prototype, chưa duyệt chơi tay toàn tuyến/trùm |
| C2 | Scene 03, 5 phòng, Cột Gió, chuông, quái ném, Chim Lạc, quiz/reward/tri thức/chuyển chương có code/data | Bot nhánh cũ có lịch sử thử; chưa xác nhận lại full tuyến hiện hành/Windows |
| C3/C4/C5 | Có đặc tả trong `spec/chapters` | **Chưa có scene/runtime/data chương**; chưa có hai cái kết |
| Runtime | AppRoot: Settings/Input/Session/UI; WorldStateStore/InventoryService/SaveService theo scene; PlayerMotor điều khiển An | Chưa có ChapterContext/WorldIdCatalog SO/int-state API theo thiết kế |
| Save | Native schema 4, đọc legacy version 1–5; file `save_v1.json`+`.bak`/`.tmp` | Không còn dùng SaveData v5/PlayerInventory của nhánh prototype làm hợp đồng hiện hành |
| Art | C1 có ART_Camp và ART_C1_Environment cho năm khu; actor/quái còn prototype/khối, animation thử | Không gọi cảnh/model cuối hoặc đã bake/đạt FPS |

Kiểm kê máy đọc được và SHA nguồn nằm trong [ProjectAudit_2026-11-04.json](reports/ProjectAudit_2026-11-04.json). Chi tiết mã ở [RuntimeAudit](reports/RuntimeAudit_2026-11-04.md), phạm vi/chương ở [ScopeAudit](reports/ScopeAudit_2026-11-04.md). Có package `com.coplaydev.unity-mcp` trong manifest không chứng minh phiên này đã kết nối Editor.

## Khu mẫu Trại Giếng Trời

- Preview `Assets/_Project/Scenes/92_EnvironmentPreview.unity`; camera tham chiếu sau spawn, không actor mới. Cube mốc nằm trong `Layout_Markers_EditorOnly`, mặc định tắt.
- Màn chính: `World_Geometry/ENV_01_Camp/ART_Camp`; ENV được đổi tên từ Area_Camp, giữ object. Tắt hình thử cũ, giữ collider nền/vách; đá/lều có collider hộp, cây chặn thân, bụi/cỏ không collider.
- Nền trại 20×20 m, nền hành lang 4×10 m, lòng giữa vách 3 m. Vách bên 17–19 m, xa khoảng 13 m, giếng trời mở; đá xám lạnh/cây rêu/nắng ấm. Lửa trái, bàn giữa, lối đi phải trong camera tham chiếu.
- `Campfire_Placeholder` tại(-4,0,0) giữ CheckpointAltar `cp_trai`, spawn(0,.2,-5); `Gameplay/Camp_CraftingStation` tại(-3,0,3) dùng CampCraftingTable→InventoryService.TryCraft. Model tương tác là prefab con; không đổi ID pickup/save.
- A09/A11/A16 đã được người dùng nhập/chọn, không tải trùng. A10/A12/A14 không trong ART_Camp; A14 có trên đĩa. Vista cuối hành lang chỉ bật trong preview, main dùng Vườn Rêu hiện có.
- Backup scene+.meta: `SourceAssets/SceneBackups/2026-11-04_CampEnvironment`, có `BeforeIntegration_001345`. Công cụ Editor `CampEnvironmentSetup`; menu `Tools/Son Doong/Camp Environment`. Không chạy “stage/refine” để đọc tài liệu vì đó là thao tác dựng lại cảnh.
- [CAMP_ENVIRONMENT](CAMP_ENVIRONMENT.md) giữ ảnh/asset/cách kiểm tra. Scene có cấu hình đèn/probe thử, **chưa bake GI hoặc đo FPS thực**; Windows build mới và giới hạn nằm ở mục điều khiển.

## Bối cảnh toàn chương 1 theo năm ảnh — 04/11/2026

Đã dựng và kiểm tra trong `92_EnvironmentPreview`, ghép vào `02_HangLinhMach`. Năm khu có nền/vách/trần hang, nhũ đá, cây/rêu/dương xỉ, đạo cụ, khảm đồng/ngọc, tinh thể và ánh sáng riêng. Trại giữ `ART_Camp`; nhóm mới `World_Geometry/ART_C1_Environment` bổ sung năm khu, các model tương tác nằm dưới gameplay root cũ. Giữ ID/ref/spawn và collider cũ; thêm 17 collider đơn giản ở rìa/tượng, bake NavMesh bằng Physics Colliders. Không thêm actor/quái mới hoặc đổi tuyến/puzzle/save.

[Bản dựng, nguồn asset và năm ảnh](CHAPTER01_ENVIRONMENT.md). Đã kiểm tra **143 cụm cây chạm nền, 23 mesh rêu theo mặt đá**, material được hỗ trợ; tổng 720 renderer trang trí mới kể cả tượng dưới gameplay root, chưa phải số draw call/FPS. Tượng và collider của nó nằm dưới RelicAltar cũ để tia E không bị chặn bởi một object trang trí không có tương tác. A09/A10/A11/A16 đã có đủ; các chi tiết bổ sung là mesh/shader của dự án, không yêu cầu asset trả phí.

[Editor](reports/Chapter01Art_Editor_2026-11-04.json) và [Play Mode](reports/Chapter01Art_PlayMode_2026-11-04.json): **145 checks, 0 failures, 0 runtime errors**. Gồm Boot/Chơi mới, đi vòng các bệ qua năm khu, 56 góc camera, chế/lưu/Continue, A-C-B, quay gương, receiver/cổng và agent đứng trên NavMesh. Đây là controller/API; AI được dừng trong đoạn đo đường, receiver gương dùng API, không chứng nhận người thật đánh trùm/giải đầy đủ quang học. Các lượt probe đầu chạm bệ thưởng cũ khi đi thẳng; đã sửa tuyến đo đi vòng bệ. Một lượt chưa nhận đèn nên save từ chối đúng; đã bổ sung prerequisite nhận đèn vào phép thử, không sửa validator/save để làm phép thử đạt. Save chính/backup/tmp gốc được trả byte-for-byte.

[Windows x64 Release](reports/Chapter01Art_Build_2026-11-04.json): **Succeeded, 0 errors, 485 shader warnings Sentis**; `Builds/Chapter01Environment/Windows/SonDoong.exe`. Binary scene chứa nhóm art và model tượng gắn dưới altar cuối cùng; assembly không chứa probe Editor. Chưa chơi tay bản Windows.

Scene/.meta backup: `SourceAssets/SceneBackups/2026-11-04_Chapter01Art/`. Editor tool `Chapter01EnvironmentSetup`, menu `Tools/Son Doong/Chapter 01 Environment`. Chưa bake GI toàn chương, đo FPS trên máy tham chiếu hoặc duyệt chơi tay/hình cuối. Trạng thái này thay câu “ngoài trại còn blockout” ở audit trước; các báo cáo cũ giữ nguyên phạm vi lịch sử.

## Focus xa hơn và bỏ tia khi chém — 04/11/2026

Theo phản hồi mới: Focus quái tăng 8→10 m, puzzle giữ 8 m/reveal 5 m. FocusLight visual falloff 12 m/góc 90°. Tắt hẳn ribbon dao; LMB ngắt RMB và không tự chiếu lại cho tới khi thả/giữ lại RMB, có hint. Không thay animation/damage/stamina/AI/ID/schema.

[116 checks Play Mode](reports/FocusCombat_PlayMode_2026-11-04.json), 0 failures/errors: thêm kiểm tra quái 9.3 m, ngoài tầm, tường, puzzle giữ 8 m và tách LMB/RMB/không ranged damage. [Chi tiết + ảnh không tia](reports/FocusCombat_2026-11-04.md). Save gốc được trả byte-for-byte. Bản Windows mới: `Builds/FocusCombatValidation/Windows/SonDoong.exe`; các build trước giữ nguyên.

## Động tác dao sau xác nhận “cả hướng và động tác” — 04/11/2026

Đã làm lại Slash1/Slash2, thêm Lunge riêng, gắn Knife A16 vào RightHand, mask nắm cán và BladeTrail theo mũi dao trong .15–.30 s; bỏ cube SlashFX. Layer Melee fade/CrossFade giữ chân theo Idle A01, không root motion; roll/death/đèn giữ ưu tiên. Builder tái dựng tại chỗ, giữ GUID prefab/controller/2 clip cũ. Model vẫn mannequin A01, không phải tạo nhân vật/quái mới.

[Báo cáo + ảnh Play Mode](reports/MeleeVisual_2026-11-04.md): **106 checks Play Mode và 23 checks pose Editor, không failure/error**, gồm hướng camera và thêm wrist/foot/grip/trail/hai slash/lunge. Source save gốc đã được trả byte-for-byte. Đây là virtual input và số đo/ảnh engine, vẫn cần người dùng duyệt chơi tay.

[Windows Release mới](reports/MeleeVisual_Build_2026-11-04.json): **Succeeded, 0 errors, 485 shader warnings package Sentis**; `Builds/MeleeVisualValidation/Windows/SonDoong.exe`. Source backup: `SourceAssets/MeleeVisualBackups/2026-11-04`. Có 108 file C# _Project, 9 clip và 3 mask sau lượt này. Những report 76/91 ở dưới là các phiên bản trước, giữ để truy vết.

## Bản sửa hướng trước khi xác nhận cả động tác — 04/11/2026

Người dùng thử và báo điều khiển chuột khó, đánh không đúng. Đã sửa trục chuột dọc, đọc Look trước Combat/Motor trong cùng frame, dùng yaw hiện hành cho di chuyển và hướng camera cho đòn không lock-on. Đánh không còn lấy hướng S/A hoặc hướng đứng cũ; dừng trượt và locomotion animation trong đòn. Pivot được ghi sau chuyển động actor để tránh bị kéo theo.

[91 checks Play Mode](reports/CharacterAim_PlayMode_2026-11-04.json), 0 failures/errors: thêm 15 kiểm tra camera 4 hướng/hit đúng mục tiêu, đi lùi đánh và xoay+đánh cùng frame. Kết quả 76 checks bên dưới là phiên bản trước phản hồi, không dùng để bác bỏ trải nghiệm người chơi. [Nguyên nhân, phạm vi và bằng chứng](reports/CharacterAim_2026-11-04.md). Vẫn cần người dùng duyệt cảm giác tay; chưa thay model/clip động tác. Windows Release mới: `Builds/CharacterAimValidation/Windows/SonDoong.exe`, build thành công 0 errors/485 shader warnings Sentis; [build evidence](reports/CharacterAim_Build_2026-11-04.json). Bản build trước được giữ riêng.

## Điều khiển nhân vật — triển khai 04/11/2026

Đã áp dụng lên `PF_An_Prototype` và `AC_An` giữ GUID: phản hồi tăng/giảm tốc 30/45 m/s², xoay tối đa .06 s, đệm nhảy .15 s/coyote .12 s, ground normal/chạm trần/chia bước va chạm. `PlayerActionState` chia quyền đánh/lộn/đèn/E/Q và hit reaction; input giữ qua resume/focus không phát lại lệnh. Dao chốt hướng/LOS/dedup/đệm nối đòn; lộn lấy hướng cùng frame và reset đúng khi teleport. Camera chỉ có một nơi ghi pivot, lock-on ngắm thân, marker trước render Canvas; Animator giữ bind pose/cached params/reset pose, âm bước chân/tiếp đất nối SFX. Giữ tốc độ/ID/schema/luật C1 chưa lộn.

File chính: `Scripts/Player/{PlayerActionState,PlayerMotor,DodgeController,PlayerCameraController,LockOnController,PlayerAnimator,LanternController,LanternFocusScanner,PotionUser,PlayerVitals}.cs`, `Scripts/Combat/CombatController.cs`, `Scripts/Core/{InputReader,GameSession}.cs`, `Scripts/Interaction/{InteractionScanner,CampCraftingTable}.cs`, prefab An/controller dự án. Editor tool `CharacterPolishSetup`, opt-in `CharacterPolishSmokeProbe`; không thêm motor/input runtime thứ hai. Cả _Project hiện 106 file C# (103 ở audit ban đầu +3 file mới).

- [Nhân vật](reports/CharacterPolish_PlayMode_2026-11-04.json): **76 checks, 0 failures/errors**, virtual keyboard/mouse qua Input System, mô phỏng bước thời gian 30/60/120; gồm va chạm/nhảy/lộn/perfect/lunge/cột gió/lock-on/Focus/E/thuốc/craft/save/Continue/death. Không phải benchmark FPS hoặc chơi tay.
- [Trại chạy lại sau sửa](reports/CampEnvironment_PlayMode_2026-11-04.json): **53 checks, 0 failures/errors**, gồm 24 góc camera/3 vị trí, đường đi và craft/save/Continue.
- [Windows x64 Release](reports/CharacterPolish_Build_2026-11-04.json): **Succeeded, 0 errors, 485 shader warnings từ package AI Inference/Sentis**. Artifact: `Builds/CharacterPolishValidation/Windows/SonDoong.exe`. Khởi động nographics không managed exception; chưa thử gameplay có đồ họa trên Windows/máy thứ hai.
- Backup 38 file + SHA256 đã đối chiếu: `SourceAssets/CharacterPolishBackups/2026-11-04/manifest.json`; các probe đã trả primary/.bak/.tmp gốc byte-for-byte. Giữ thay đổi ngoài phạm vi, không tạo commit.

[Báo cáo chi tiết và giới hạn](reports/CharacterPolish_2026-11-04.md). Animation vẫn blockout; cần người chơi duyệt cảm giác tay, âm thanh, 4 hướng/khe thấp C2 và full C1→C2/boss. Actor sở hữu inventory, HUD/audio UI/save nhánh lỗi vẫn là gap riêng.

## Bằng chứng kiểm thử và phạm vi được kế thừa

| Bằng chứng | Có thể kết luận | Không được suy ra |
| --- | --- | --- |
| [Camp Play Mode 04/11](reports/CampEnvironment_PlayMode_2026-11-04.json) | **53 checks,0 failures,0 errors** sau merge: Boot/NewGame, đi quanh bàn tới hành lang, ray chọn bàn/lửa riêng, craft 1 thuốc, lưu cp_trai, 24 góc camera ở 3 vị trí, menu/Continue giữ thuốc, restore save gốc byte-for-byte | Không phải E/WASD thật, full C1/C2, edge-case craft, combat/boss hoặc Windows |
| [Camp Editor 04/11](reports/CampEnvironment_Editor_2026-11-04.json) |115 mẫu clearance, ảnh 4 góc, import/config/integration không failure trong lượt dựng trại | Không phải bake/performance hoặc nghiệm thu mỹ thuật của người chơi |
| Report Inventory/Crafting/Audio/Quest 02/11 trong `reports/` |76/57/35+5/64 checks và build của triển khai **trước merge** | Không gán các gate đó cho InventoryService/SettingsService/QuestTracker/schema 4 hiện tại |
| Nhật ký bot C1/C2 nhánh đã merge | Lịch sử mô tả nhiều phép thử Editor, `c2s` 212 s, một số thao tác khóa/khe kiểm tay | Không có raw report C2 hiện hành trong docs/reports; không phải Windows hoặc đo người chơi mới |
| [AssetTrials 30/10](reports/EnvironmentAssetTrials_2026-10-30.json), lịch sử import | Mẫu/phiên bản lúc đó có bằng chứng riêng;33 model vàGUID vẫn khớp manifest backup khi đọc lại | Không phải chạy lại import/Play với controller sau merge |

Bot `PlaytestBot.Chapter1.cs` vẫn đợi CraftingPanel mở khi E ở lửa; C1 nay dùng bàn riêng. Phải cập nhật kịch bản trước khi dùng bot toàn tuyến làm gate mới. Probe Camp là opt-in và không tự chạy khi chơi bình thường.

## Khoảng thiếu và rủi ro còn mở

Các mục dưới đây được phát hiện khi đọc mã/tài liệu, **được cập nhật theo lượt điều khiển tiếp sau**; các mục chưa giao ngoài phạm vi vẫn giữ nguyên. Nguồn/case chi tiết trong REQUIREMENTS_MATRIX và hai báo cáo audit.

| Nhóm | Hiện trạng cần giải quyết khi được giao |
| --- | --- |
| Bảo toàn save | NewGame xóa save trước tải scene thành công; RelicAltar vẫn công bố kết khi SaveAt false; native validation chưa đủ catalog/maxStack/ints/elapsed/header, Restore có thể bỏ slot sai |
| Combat/interact | Đã sửa LOS dao và action guard, có Play Mode. Ràng buộc actor sở hữu inventory ở bàn và full tuyến/case biên vẫn chưa đủ |
| Tri thức/reward | wrongThisSession không lưu qua restart; verified:1 không có hồ sơ KN01 độc lập/build gate; reward nhiều item chưa chứng minh nguyên tử |
| HUD/audio | C1 đã khớp Vườn Rêu/progress 3/2, ba slider/mixer và soundscape C1; còn nghe cân âm, nhạc cụ Việt và kiểm hai lần mở bản Windows; không suy thành hoàn thiện audio C2–C5 |
| Nền tảng C3–C5 | WorldIds có catalog nội dung/mốc C1/C2 và bốn ID năng lực chung, chưa WorldIdCatalog/ChapterContext/GetInt/SetInt; thiếu 3 scene, cơ chế, trùm, ending |
| ID | Runtime vẫn relic_drum_a/b/c; yêu cầu chuẩn fragment_* chưa làm. Không đổi lẻ, cần migration/ref/catalog |
| Dữ liệu legacy |5 SO còn trỏ script đã xóa: ATK_Guardian_Slam/Sweep và IT_LaCam/LinhChi/ThuocMau; chưa có ref tới chúng trong _Project, không tự xóa |
| Animation | Đã sửa Fly orphan bằng InAir trong AC_An giữ GUID, không sửa A01 nguồn. Animation vẫn blockout, chưa nghiệm thu mỹ thuật/cuối |
| Asset provenance | README A09 ghiC C0 khác catalog; License A11 có tiêu đề khác tên bộ; cần đối chiếu bản phân phối trước release, không sửa sourcelicense để khớp docs |
| Nghiệm thu | Chưa có chơi tay toàn C1→C2 hiện hành, toàngame 5 chương, gameplay Windows có đồ họa, máy thứhai, benchmark hoặc thời lượng người mới |

Quét tham chiếu serialized trong `_Project`, tra metadata ở Assets/Packages/PackageCache: **không trùng GUID asset, không thiếu/trùng local fileID ở 10 scene/34 prefab**; ở thời điểm audit có 6 tham chiếu ngoài chưa resolve (5 script legacy +1 motion Fly). Lượt điều khiển đã sửa Fly dự án, còn 5 SO script legacy chưa xử lý. Ba GUID nội bộ exposed parameter của mixer đã được loại khỏi phép kiểm asset-reference, không báo nhầm thành asset thiếu.

## Điểm tiếp nối

Đã xử lý yêu cầu điều khiển nhanh/gọn sau lượt đồng bộ; môi trường C1 vẫn là ưu tiên nội dung đã chốt. Các backlog khác không tự trở thành lệnh triển khai. Khi nhận công việc tiếp: đọc MEMORY/STATUS/MATRIX, kiểm lại file thật, chọn phần spec theo SPEC_INDEX, giữ thay đổi ngoài phạm vi. Nếu chuẩn bị build/nghiệm thu phải xử lý các gap bảo toàn dữ liệu và chạy hồi quy trên đúng phiên bản, không dùng report prototype cũ thay kết quả.

## Lịch sử và bảo toàn workspace

Merge `2312755_NguyenThangThieng` vào main tại `ba58ff7` đưa vào kiến trúc/chương mới; các Setup Audio/Crafting/Inventory/Quest cũ bị loại vì API không còn phù hợp. RoutedAudio/SonDoong.Audio được khôi phục phục vụ source A01 biên dịch. Nhật ký nhánh cũ và nhật ký prototype có chi tiết đầy đủ trong history, đã phân biệt khỏi trạng thái này.

Trước lượt tài liệu đã có thay đổi code/scene/font/audio và asset/report trại chưa commit; giữ nguyên. Đợt đồng bộ ban đầu chỉ thay tài liệu/báo cáo/AGENTS; lượt điều khiển sau đó có sửa code/prefab/controller và build như mục mới. Không reset, dọn asset nguồn hoặc tạo commit. Bản gốc MEMORY/STATUS được lưu trong docs/history trước khi thay nội dung; liên kết trong bản lưu đã đổi theo vị trí lưu trữ.
