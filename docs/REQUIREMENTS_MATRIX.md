# Yêu cầu hiện hành và phần còn thiếu — cập nhật 06/11/2026

Đây là bảng đối chiếu **yêu cầu với thực tế**, không phải lệnh triển khai toàn bộ backlog. Nguồn thiết kế đầy đủ ở [SPEC_INDEX](SPEC_INDEX.md); tên API/ID đang dùng ở [CURRENT_ARCHITECTURE](CURRENT_ARCHITECTURE.md). Sau đợt đồng bộ tài liệu, lượt điều khiển 04/11 đã cập nhật các hàng liên quan theo [CharacterPolish](reports/CharacterPolish_2026-11-04.md); không coi backlog còn lại là lệnh tự triển khai.

**Quyết định hiện hành:** 5 chương; ưu tiên C1; lịch cũ không là hạn mới; ba slider Master/Music/SFX; HUD mở đầu Vườn Rêu, 3 mảnh/2 câu đố; **Mid Poly Stylized Việt Nam**, texture 1K mặc định/2K cho nhân vật và công trình chính, URP Lit, LOD cho vật thể lớn. Yêu cầu 05/11 đã cho phép hoàn thiện Hắc Ảnh/Hộ vệ/đèn/UI/audio bằng asset miễn phí đã nhập; vẫn không thay model An. Các mốc kiểm chứng cũ không tự nghiệm thu phần mới.

**Phản hồi mới nhất 05/11:** chế bình máu trong túi thay bàn; lửa động/ghế gỗ dày; ba trống A20 và ký hiệu Mặt Trời/Cánh Chim/Sóng Nước; A22 cửa/A23 gương/A24 bệ theo thư mục thực tế. Hai giấy gợi ý nhặt bằng E/lưu Nhật ký, không là prerequisite. Hắc Ảnh choáng4s/Hộ vệ2s, immunity3s và lượt giữ mới, cúi đầu/dấu sao/khóa đòn; sương mỏng URP và UI mới. Triển khai/kiểm chứng tại [C1_REVISION_WORKLOG](C1_REVISION_WORKLOG.md); bằng chứng Completion trước revision chỉ là mốc nền.

**Xác nhận tiếp theo05/11:** giữ ngôi sao báo choáng, bỏ hiệu ứng đổi màu/phát sáng trên thân quái khi suy yếu. Cơ chế damage/choáng giữ nguyên; [52 kiểm tra Play Mode](reports/C1_NoWeaknessVisuals_PlayMode_2026-11-05.json) đạt, không dùng ảnh cơ thể đổi màu trong lịch sử làm yêu cầu hiện hành.

**Giấy hướng dẫn/bệ05/11:** thêm giới thiệu game và điều khiển/lưu/chế trong trại, hướng dẫn chuẩn bị trước cổng Kim Quy; giấy tùy chọn, đọc lại J. Sửa tâm mesh A24, gỡ tấm vuông bổ sung và đặt vật phẩm đúng mặt bệ; khôi phục bệ đèn tách khỏi pickup. Xem PROJECT_STATUS cho Editor/Play/build sau thay đổi, không dùng các ảnh bệ lệch trong mốc cũ.

**Cách đọc trạng thái:** “Có code/cấu hình” không đồng nghĩa đã Play/build; “Thử hẹp” chỉ đúng phạm vi báo cáo; “Lịch sử” không được dùng như kết quả hồi quy hiện tại. Code thiếu chức năng không tự hủy yêu cầu.

Các mã có gạch nối như SCOPE-01, KN-02, DATA-01 là mã tra cứu riêng của bảng. C1-Fxx/C1-Rxx và MV/LK/QZ/AB/BS/CH/EN/KN01/PF01/RL01 dẫn tới mã gốc trong spec; không coi mã tự đặt là ca kiểm thử chính thức mới.

**Phản hồi chơi thử tiếp theo 04/11:** chuột khó và đánh sai hướng đã được sửa; [91 checks mới](reports/CharacterAim_2026-11-04.md) gồm hướng camera/đánh/chuột dọc. Cảm giác người chơi vẫn là tiêu chí duyệt, không suy từ số checks.

**Melee mới sau xác nhận cả động tác:** [106 checks +23 pose](reports/MeleeVisual_2026-11-04.md), hai nhát chém/lunge/dao gắn tay/trail đã có; model mannequin và duyệt cảm giác vẫn chưa phải nghiệm thu cuối.

**FocusCombat mới nhất:** [116 checks](reports/FocusCombat_2026-11-04.md), quái 10 m/puzzle 8 m, LMB không beam/ribbon và ngắt RMB có latch nhả nút; các mô tả vệt dao trước đó là lịch sử.

## Phạm vi, phong cách và thứ tự

**Bổ sung 05/11:** được mở rộng kích thước và địa hình C1, dùng asset free đã nhập và nối đẹp năm khu. Bản scene mới ở [CHAPTER01_EXPANDED](CHAPTER01_EXPANDED.md); kết quả 145 checks ngày 04/11 không tự áp sang bản mở rộng. Các kết quả mới ghi riêng tại PROJECT_STATUS.

| Mã | Yêu cầu / nguồn ưu tiên | Hiện trạng và việc còn lại |
| --- | --- | --- |
| SCOPE-01 | 5 chương Kim Quy, Chim Lạc, Sơn Tinh, Thủy Tinh, Trục Linh Mạch; §40, người dùng 04/11 | Có C1/C2, thiếu scene/runtime C3–C5; chưa phải game 5 chương hoàn chỉnh |
| SCOPE-02 | Tổng mục tiêu 65–95 phút; C1 15–25, C2–C4 mỗi chương 10–15, C5 20–25; §40/42/45/49 | Mục tiêu thiết kế; chưa có đo playtest người mới. 25–35 phút C1 ở bản 29/10 không còn là mục tiêu hiện hành |
| SCOPE-03 | Windows x64, chơi đơn offline, tiếng Việt, bàn phím/chuột; §1/40 | Có Windows/input/TMP. C1 mở rộng có 569 checks Editor Play Mode; bản Windows và kết quả build cuối ở PROJECT_STATUS. Chưa chơi tay toàn build hoặc thử máy thứ hai |
| SCOPE-04 | Một độ khó; không nỏ/móc dây/đu dây/cưỡi chim/bơi; §40–43 | Giữ phạm vi; không khôi phục nhánh “mở rộng tuần 8” của bản cũ thành việc bắt buộc |
| SCOPE-05 | Mid Poly Stylized Việt Nam; texture 1K mặc định, 2K nhân vật/công trình chính, URP Lit, LOD vật thể lớn; người dùng 06/11. Low poly/bảng màu định tính là mốc 04–05/11 | C1/C2 và asset hiện có được dựng trước quyết định mới; chưa xác minh đã chuyển phong cách/texture/material/LOD. C1 mở rộng năm khoang/bốn đường nối, có ảnh/credit/kiểm thử ở CHAPTER01_EXPANDED/STATUS. Cần concept, ngân sách polygon/ngưỡng LOD, duyệt hình bằng tay và đo hiệu năng khi có phạm vi triển khai |
| SCOPE-06 | Hoàn thiện C1 trừ model An; cho phép Hắc Ảnh/Hộ vệ, UI/audio/đèn; người dùng 05/11 | Ghost A08 + Golem Kevin dưới root cũ, controller trình bày riêng; không tạo C3–C5 hoặc thay model An |
| SCOPE-07 | Lịch 7 ngày và 12 tuần chỉ để tham chiếu; người dùng xác nhận 04/11 | Chưa chốt hạn mới. Không gắn các “ngày 1–7” vào lịch hiện tại hoặc tự đặt deadline |
| SCOPE-08 | Cơ chế rẻ, mỗi chương C2–C4 thêm một năng lực; duyệt asset trước nhập; §41/51 | C1/C2 có blockout; A09/A11/A16 đã được chọn cho trại. Không có duyệt mặc định cho mọi asset/chương còn lại |

## Nền tảng và chương 1

| Mã | Yêu cầu / nguồn | Có gì và khoảng thiếu |
| --- | --- | --- |
| C1-F01 / R01 | Một input asset Gameplay/UI, một PlayerMotor; §9/10/45 | Có PlayerControls/InputReader/PlayerMotor và PlayerActionState dùng chung. Thử input giả lập 30/60/120 bước thời gian: đi/chạy/chéo/dừng/nhảy/coyote/buffer/trần/tường; giữ phím qua resume không phát lệnh. Xem CharacterPolish report; không phải benchmark. |
| C1-F02 | Camera tránh vật cản trong 90_TestMechanics; §45 | Scene90 và hai scene chương đã bật; Camp probe qua24góc/3vịtrí. Scene91 vẫn tắt Avoid Obstacles, không coi mọi scene thử đã đạt |
| C1-F03 | Tường ranh giới/KillZone, rơi chết/hồi sinh; §45 | Có Hazard và KillZone trong scene chương. Hồi sinh Camp đã thử; toàn tuyến/hết mọi mép cần thử lại |
| C1-F04 | HUD tiếng Việt, debug puzzle chỉ hiện gần, màn chết đúng vị trí; §45 | Có TMP, HudBars, ProximityVisibility/UI chung. Chưa nghiệm thu mọi panel/tỷ lệ sau merge |
| C1-F05 | Ba dấu khác hình, đọc được và có gợi ý; §45 thay bởi phản hồi05/11; model rõ hoa văn theo ZIP06/11 | Ba trống ndnguyen3d/CC BY4.0 thay model A20 ở câu đố, texture2K/PBR; bảng Mặt Trời/Cánh Chim/Sóng Nước tách riêng, giấy gợi ý tùy chọn; không hiện A/B/C. Enum A–C–B/ID giữ nguyên. Xem DONGSON_DRUM_WORKLOG; chưa nghiệm thu người chơi mới |
| C1-F06 | Không vật liệu hồng/tím lỗi trong Windows build; §45 | Shader hỗ trợ trong Editor và có Windows Release; ảnh Editor/build thành công chưa thay thế kiểm tra hiển thị Windows bằng tay |
| C1-F07 | Build mở Boot, đúng các scene chương đã có, không scene thử; §45/50 | Hiện bật 00/01/02/03; C3–C5 chưa có. Windows profile dùng danh sách toàn cục |
| C1-F08 | WorldState đổi không reset lượt trống dở; §45 | Có code bảo vệ; bằng chứng tuyến trước merge lưu trong lịch sử, chưa chạy ca hồi quy này trong lượt tài liệu |
| TECH-STATE | WorldState tổng quát, catalog ID, dữ liệu design/runtime/save tách nhau; §5–7, chapters/00 | Có StateSet/WorldStateStore/WorldIds tĩnh. Thiếu WorldIdCatalog SO, int-state API và validator ID/build theo thiết kế |
| APP-STATE | Dịch vụ dùng chung và chuyển chương không trộn phiên; chapters/00 | AppRoot có Input/Settings/Session/UI; World/Inventory/Reward nằm theo scene và mang snapshot. Chưa khớp vị trí service trong spec, không tự gọi đã chuyển lên AppRoot |
| C1-R04 | Đàn Kim Quy/cổng kiểm đủ 3 mảnh + 2 puzzle; §16/45 | Bản mở rộng đã thử đi toàn tuyến, thu ba mảnh, giải trống/gương và mở cổng theo API; 149 mặt cắt kiểm vách đóng. Chưa nghiệm thu chiến đấu người thật; cờ boss là fixture trong phép thử save |
| C1-R05 | Hộ vệ báo quét/nện rõ, chạy né được ở C1, có save/reset đấu trường; §16/43/45 | Có code/scene và lịch sử bot. Cần cân bằng người thật, thử chết/thắng/Continue lại trên bản hiện hành |
| C1-R06 | Phục dựng cấp movement_kit, chỉ công bố kết sau save an toàn; §16/43/45 | RelicAltar rollback world khi SaveAt false, chỉ mở kết/thưởng sau lưu thành công; có probe lỗi/success/Continue |
| C1-R07 / HUD-01 | HUD HP/stamina/đèn/thuốc; mở đầu Vườn Rêu, 3 mảnh/2 puzzle; người dùng 02/11, 04/11 | QuestTracker hiện mở đầu Vườn Rêu, hai text mục tiêu/tiến độ; giữ ID cũ và phân biệt solved với nhận B/C. Báo cáo mới: C1Completion_PlayMode_2026-11-05 |
| C1-R08 / IT-01 | Túi 12 ô, 9 định nghĩa, stack 10/5, nhặt không mất khi đầy, Q +35/2s; §17/45 | Có InventoryService/ID_*.asset/ItemPickup/PotionUser/InventoryPanel. Chỉ nhánh Camp craft/save được thử gần nhất; bộ edge-case Windows 02/11 thuộc API cũ |
| IT-02 | C1: chế1+1→1 trong túi I, E checkpoint lưu riêng; người dùng05/11 thay yêu cầu bàn cũ | Bàn đã gỡ, InventoryPanel button gọi giao dịch CanCraft/TryCraft, debounce unscaled0,25s, không autosave; probe Revision kiểm thiếu nguyên liệu/túi đầy/ô vừa trống/spam/ngoài UI. C2 giữ panel altar riêng |
| IT-03 | Vật nhiệm vụ không chiếm ô, không trao lại, ID bền; §17/chapters/00 | Lượt3 C2: RewardService chuẩn bị mọi item trên cùng draft, commit inventory/world/claim trước event, chặn gọi lồng. Play thử batch2item khi chỉ còn1ô, dữ liệu sai, observer và claim lặp; P4 full bag/Continue/nhận lại đúng công cụ+2thuốc. Không đổi TryCraft |
| C1-R09 / AUDIO-01 | Ba thanh Tổng/Nhạc/Hiệu ứng, PlayerPrefs riêng, pause/settings đúng; người dùng 02/11, 04/11 | Đã nối ba slider/mixer dB, fallback khóa cũ, debounce flush. Probe mới kiểm gain/mute/pause; chưa thử qua hai lần mở Windows hoặc nghe cân âm |
| AUDIO-02 | Mixer routing/SFX/UI/ambience/nhạc/bước chân, pause/fade; §21/51 | SoundscapeRuntime/palette thêm menu/exploration/combat crossfade, ambience, UI/puzzle/enemy one-shots; footsteps giữ group GUID. Xem AUDIO_SETTINGS; trống A33/nhạc A38 vẫn âm thử |
| KN-01 | C1 có 3 bia thật, không Bệ Vấn Đáp; §44/45 | Có 3 bia/data; 8 bia và 7 câu C1/C2 có verified:1 nhưng chưa có hồ sơ KN01 độc lập/build gate |
| C1-R11 | ID mảnh chuẩn fragment_* có migration, không phá save cũ; §45 | Hiện vẫn relic_drum_a/b/c. Chỉ đổi cùng migration/catalog/ref; không sửa lẻ ID để khớp tài liệu |
| ART-01 | Art C1 mở rộng bằng asset miễn phí, giữ root/ref/ID/save; người dùng 04–05/11 | Năm khoang/bốn đường nối, tám vestibule, cửa/đèn/quái/vật nhặt FBX, UI/audio; backup theo lượt. Xem C1 completion; không suy đã bake GI, đạt FPS hoặc duyệt mỹ thuật bằng chơi tay |
| IN-01 | E chọn mục tiêu đầu tiên, LOS camera và nhân vật, tầm 3m, ưu tiên trạng thái; §10–11 | Scanner ray camera 15m, LOS/range; đã có guard Attack/Dodge/hit reaction/dead/trên không, prompt ổn định. Play Mode thử E chọn một mục tiêu và E bị chặn lúc đánh; chưa hồi quy mọi đối tượng/khu. |
| LT-01 | Reveal/Focus độc lập VFX, kiểm LOS, năng lượng hồi, pause không tăng tiến độ; §11 | Có controller/scanner/targets. Focus **90°/±45°, seal 1s** là chỉnh được người dùng chấp thuận 02/11, thay giá trị cũ 60°/1.5s |
| PZ-01 | Trống A→C→B, debounce .2s, reset sai, solved/load không trao lại; §12 | Có code/prefab/save refs. Lịch sử có bot; chưa hồi quy ca biên trên build hiện hành |
| PZ-02 | Hai gương ×4 nấc, 16 tổ hợp, ngọc 1s, tường/mặt lưng/giới hạn phản xạ; §13 | Có MirrorRotator/Beam/Receiver. Không còn probe 16 tổ hợp cũ; chưa thử lại toàn bộ sau merge |
| C1-R02 / CB-01 | Đòn .65s, active .15–.30, dedup nhiều collider, không chém xuyên tường; §14/45 | Đã sửa active .15–.30, đệm nối đòn .16s, giữ hướng chém, dedup và LOS Default/Environment. Play Mode qua nhiều collider/đòn, tường cản, nối 2 đòn và hủy khi bị đánh. Chưa cân bằng/trải nghiệm chiến đấu toàn tuyến. |
| C1-R03 / AI-01 | Một nguồn dịch chuyển, FSM/LOS/leash, checkpoint an toàn; §15/45/chapters/00 | Ghost dùng EnemyBrain/NavMeshAgent, Hộ vệ dùng StoneGuardian + CapsuleCast; model/clip chỉ trình bày. NavMesh/checkpoint/tấn công/damage/death được thử mới; chưa nghiệm thu cân bằng, mọi nhánh leash hoặc chơi tay trùm |

## Quà Kim Quy, tri thức và chương 2–5

| Mã | Yêu cầu / nguồn | Hiện trạng và mức kiểm chứng |
| --- | --- | --- |
| MV01 | C1 không lộn; mở movement_kit khi hoàn tất C1; §43 | Play Mode xác nhận C1 Chơi mới không lộn khi chưa có movement_kit; cấp kit riêng trong probe để thử cơ chế. Chưa coi đây là đã chạy lại toàn tuyến RelicAltar trao thưởng. |
| MV02–03 | Lộn 4 hướng, Shift xa ×1.5, lunge ×1.25, né đúng lúc, khe thấp; §43/53 | Play Mode qua hướng D trong cùng frame (3.6m), Shift xa hơn, bất tử/né đúng lúc/đệm lunge, reset capsule khi teleport và thoát hover. Chưa đủ 4 hướng/khe thấp/trùm thật và chất lượng animation cuối. |
| C1-R12 | Bộ animation di chuyển/lộn/dao/đèn/tương tác và lock-on dùng lại; §43/45 | Đã cache param, giữ bind pose skeleton, giảm xung đột layer đèn/lộn/chết và reset pose khi teleport. Fly của AC_An đã sửa InAir giữ GUID. Clip/model vẫn là blockout, cần duyệt bằng người chơi. |
| LK01 | Lock-on chuột giữa, tầm 15m, bỏ khóa chết/xa/che>1s; §43/53 | Camera có một nơi ghi pivot, điểm dự phòng trên thân và marker trước render Canvas. Play Mode qua chọn mục tiêu, bám mục tiêu di chuyển và nhả khóa sau che >1s; sensitivity/pitch clamp cũng đã thử. Chưa nghiệm thu camera toàn tuyến. |
| KN-02 / QZ01–03 | Bia có nhãn; sai được giải thích/thử lại/đủ đồ, điểm đúng lần đầu; §44 | C2 lượt3:110Play đạt, UI sai/đóng/retry,7câu/4bệ,5bia đèn/range/LOS/Journal. Cờ quiz_wrong_* theo snapshot, Answered/FirstTry cùng batch; native save/Menu/Continue panel mới giữ phạt sai. Chưa restart process Windows/full C2; cờ sai lịch sử chưa từng lưu không thể suy ngược |
| KN-03 | Data verified/sourceRef và chặn nội dung chưa xác minh vào build; §44/53 | [Nguồn C2](C2_KNOWLEDGE_SOURCES.md):5bia/7câu đối chiếu trực tiếp museum/Oxalis ngày05/11. Có Chapter02KnowledgeBuildGate khi scene03 enabled;23Editor kiểm cả nhận dữ liệu đúng/từ chối verified=false. Phạm vi chỉ C2; C1 chưa xác minh trong lượt này, chưa build Windows mới |
| C2-01 | 5 phòng, Cột Gió, chuông 2-3-1, mảnh/công cụ/trùm Chim Lạc; §46, chapters/C2 | [Lượt7](C2_ENVIRONMENT_WORKLOG.md): đủ năm khu có hình/âm/HUD,10Editor/77ArtPlay; hồi quy140tuyến/123cầu/87arena/71ending đạt, input thật; Windows20qua C1→C2/kết/Continue với fixture. Không đổi An/collision/ID/save. Còn duyệt mỹ thuật/rig, trận tay200HP/nghe cân âm/thời lượng người mới; không coi đây là full manual run |
| C2-02 | Bệ P1–P3 nhận mảnh; P4 công cụ/thuốc; P5 phục dựng + 3 câu Linh Thần; §42/46 | Lượt3 kiểm4bệ/thưởng nguyên tử/Continue không nhân đồ. [Lượt6](C2_ENDING_WORKLOG.md)7Editor/71Play đạt: thiếu mảnh/trùm, phục dựng/3câu, Esc/restore/retry-save, điểm/cờ sai/không thưởng lặp, Journal lời “Vành của ta đứng thứ nhất”, native menu/Continue. E thật tại các điểm kết; progression/boss thắng là fixture, chưa toàn tuyến/người mới/Windows |
| C2-03 | P4 quái ném, khe cầu, mốc an toàn/cửa3mảnh+công cụ; chapters/C2 | Lượt4 đạt phạm vi ở worklog: đạn quét/overlap/vách, hướng chốt/báo đòn/LOS/không melee qua tầng, model+stun không tint; input thật và checkpoint save, lỗi lưu giữ cửa/save chính. Ordinary enemies reload50HP; chưa cân nhịp giao chiến cùng lúc toàn P4 |
| C2-04 | P5 Chim Lạc200HP, dùng cột gió/Focus1.5s, stun4s/cap60, quạt/lao/pha2, reset và lưu thắng; chapters/C2 | Lượt5 cơ chế87Play; lượt7 chạy lại87đạt sau thay cảnh/model cách điệu, cùng HUD/audio. Không đổi An/collision matrix. Còn duyệt tạo hình/rig và cân trận tay200HP; số liệu/giới hạn trong worklog |
| C3-01 | Ấn Núi mức 0/1/2m, puzzle núi, Hộ vệ nhỏ/Rết Đá; §47/55, chapters/C3 | **Chưa triển khai** scene/code/data C3; thiếu int-state/collision-safe moving blocks nền tảng |
| C4-01 | Ngọc Nước, van, đền hạ nước 6s, quái nước/vỏ đá, Thuồng Luồng; §48/55, chapters/C4 | **Chưa triển khai** scene/code/data C4; cần Water/int-state/save và cơ chế boss đúng đặc tả |
| C5-01 | 6 phòng, ghép năng lực, Đại Vấn Đáp, bốn vành, Hắc Sương Vương 4 pha, chạy thoát; §49/55, chapters/C5 | **Chưa triển khai** scene/code/data C5, checkpoint theo pha và ending flow |
| C5-02 / EN01–02 | Vành Ký Ức, Kết 1/Kết 2, manh mối theo điểm; §49/53 | **Chưa triển khai**; không dùng nhánh completed C1/C2 thay cho kết toàn game |
| CH01 | Chuyển/Continue đúng chương/mốc, giữ năng lực/tri thức; §50/53 | Lượt1 kiểm C1→C2; lượt6 hồi quy hẹp nút cuối C1 vẫn chuyển C2. Cuối C2 lưu túi mới nhất, menu/Continue về c2_cp_arena giữ kết/điểm/Journal/trùm chết. C2→C3 là fallback nói rõ chưa có scene, không phải đạt cho5chương; chưa toàn tuyến/Windows mới |
| AB01 | Đất/nước/gió đã giải lưu đúng, restore không trộn phiên; chapters/00, C2–C5 | C2 có state theo tập; C3–C5 int-state và restore level nước/đất chưa có |

## Save, nghiệm thu và hồ sơ

| Mã | Hợp đồng phải giữ | Hiện trạng / việc còn lại |
| --- | --- | --- |
| SV-01 | ID/catalog/scene-checkpoint hợp lệ, schema rõ, đọc legacy an toàn; §18–19/50 | Native schema4, legacy version1–5 có nhánh chuyển đổi; WorldIds mới có C1/C2. Không gọi API/state v5 prototype là hiện hành |
| SV-02 | Tạm→flush→validate→replace/backup; lỗi không làm mất bản cũ; §19 | Có đường ghi nguyên tử. Native validation chưa đủ item catalog/maxStack/world.ints/elapsed/header; cần ca hỏng file/backup/migration trên runtime mới |
| SV-03 | Chơi mới chỉ thay ô cũ sau khởi tạo thành công; yêu cầu menu 31/10 | UI có xác nhận, nhưng NewGame xóa save trước tải scene. Nhánh lỗi này còn thiếu bảo toàn dữ liệu |
| C1-R10 / SV-04 | Chết/Continue khôi phục toàn snapshot tại nền an toàn, đủ 4 mốc C1, không nhân đồ, F9 debug; §7/18/45/53 | Có 4 mốc. Camp save/Continue/thuốc đã thử hẹp; checkpoint khác, boss/quiz/puzzle/transfers cần hồi quy |
| SV-05 | State hoàn thành chỉ công bố sau save an toàn; §16/19 | Lượt4 RequirementDoor và lượt6 RelicAltar/QuizSequence rollback khi save lỗi, công bố/event sau thành công; file tạm khóa không mất save chính. Exit C2 giữ game nếu chưa lưu. Boss thắng dùng quy ước riêng lượt5: giữ win trong phiên, mở UI DefeatSavePending/thử lại, không báo giả đã lưu. Hồi quy altar C1 hẹp đạt; chưa lỗi chuyển scene/process/full Windows |
| UI-01 | Một EventSystem/UI map, pause/world time, Esc đóng panel trên; §20/chapters/00 | Có AppRootUI/GameSession/IUiPauseOwner. Cần test input giữ khi mở/đóng panel, không click thành đòn, tỷ lệ màn hình |
| UI-02 | Font Việt, Low/Medium và Fullscreen/Window; §20–22 | UI Noto Sans, Thấp/Vừa ánh xạ quality Mobile/PC hiện có, fullscreen; menu/settings/pause/death/confirm/túi/nhật ký/story đồng bộ. Ảnh c1-complete-* và báo cáo mới phân biệt capture với thử người dùng |
| PF01 | 60 FPS 1080p Medium trên máy tham chiếu, median/P95/overdraw/memory | Lượt7 đo120frame/góc đứng yên P1/P3/P5 trong development Windows1080p/PC(Vừa),RTX3060Laptop/i5-12500H: median3.26–3.86ms/P95 4.74–5.86ms. Chưa toàn tuyến/trận tay/overdraw/memory dài hạn hoặc chốt máy tham chiếu, không chứng nhận60FPS toàn game |
| RL01 | Windows build hiện hành, Boot→hai kết, máy khác không Unity, hồ sơ/credits | Lượt7 có development Windows C1/C2,20native checks với fixture; cleanup GPU khi quit có trace/kiểm lại. **Chưa đạt toàn game:** C3–C5/hai kết/máy khác/credits A22–24 chi tiết và nghiệm thu người mới còn thiếu; không dùng build cũ02/11 thay bằng chứng mới |
| TEST-01 | Test có phiên bản/máy/bước/kết quả/phạm vi, bảo toàn save cá nhân | Có report Camp53 mới và report cũ được phân loại lại. Bot C1 cũ chờ panel lửa không còn khớp, cần sửa trước chạy lại full route |
| ASSET-01 | License/source/phiên bản/chỉnh sửa/credit, không suy nhập=đạt | Nguồn A09 README ghi CC0 khác danh mục; A11 License ghi CC0 nhưng tiêu đề khác. Ghi khác biệt để đối chiếu bản phân phối; không tự sửa/xóa source |
| DATA-01 | Asset/script GUID đầy đủ, không xóa đồ cũ của người dùng khi audit | Có 5 SO legacy thiếu script được phát hiện: 2 ATK_Guardian +3 IT_*. Hiện không có ref trong _Project; chưa xóa hoặc gọi là lỗi gameplay đã tái hiện |
| DATA-02 | Animation dùng motion hợp lệ, không mất clip sau nhập/merge; §43 | Editor xác nhận Fly là state orphan có motion thiếu; đã thay bằng InAir hợp lệ trong AC_An.controller, giữ GUID. Không sửa A01 nguồn; 5 SO script legacy vẫn là gap độc lập. |

Chi tiết nguồn/các phần chưa có: [ScopeAudit](reports/ScopeAudit_2026-11-04.md), [RuntimeAudit](reports/RuntimeAudit_2026-11-04.md). Bằng chứng hiện tại và bước tiếp nối trong phạm vi đã được giao xem [PROJECT_STATUS](PROJECT_STATUS.md). Các khoảng thiếu ở đây cần công việc triển khai riêng khi người dùng yêu cầu.
