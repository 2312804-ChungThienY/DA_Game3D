# Ghi nhớ thiết kế — Sơn Đoòng: Cõi Mộng Kỳ Thần

Đọc toàn bộ 4 file nguồn ngày **30/10/2026**, múi giờ Asia/Bangkok. Bản kế hoạch nguồn: **v1.0, 29/10/2026**. Tài liệu này tổng hợp thiết kế, không khẳng định các chức năng đã tồn tại. Trạng thái triển khai nằm trong [PROJECT_STATUS.md](PROJECT_STATUS.md); chỉ mục và dấu vết nguồn ở [SPEC_INDEX.md](SPEC_INDEX.md).

## 1. Game đang xây dựng

- Tên dự kiến: **Sơn Đoòng: Cõi Mộng Kỳ Thần**, tên tiếng Anh trong tài liệu ý tưởng: *Echoes of the Ancient Realm*.
- Phiêu lưu hành động 3D, khám phá và giải đố, góc nhìn thứ ba; Windows x64, chơi đơn, ngoại tuyến, bàn phím/chuột, một ngôn ngữ tiếng Việt.
- Đồ họa URP, bán thực tế với sắc thái huyền ảo Việt Nam. Cảm hứng Đông Sơn/Hồng Bàng, hang Sơn Đoòng–Phong Nha; không phải trò sinh tồn thế giới mở.
- **An** là nhà khảo cổ trẻ rơi vào không gian rạn nứt thời gian dưới Sơn Đoòng. **Hắc Sương** suy thoái linh mạch, khiến hộ vệ cổ mất kiểm soát. An dùng **Lồng Đèn Sen** và những mảnh **Trống Đồng** để phục hồi liên hệ với các thực thể hộ mạch.
- Kim Quy, Chim Lạc, Sơn Tinh, Thủy Tinh được gọi là **Linh Thần trong thế giới hư cấu của game**. Không trình bày cách nhóm này như một phân loại lịch sử.
- Vòng chơi khái niệm: khám phá/di chuyển → giải đố cổ vật → thu thập → chiến đấu/thức tỉnh. Trong chương đầu, tiến trình cụ thể dùng ba mảnh trống A/B/C, hai puzzle và Hộ vệ Đá.
- Ba trụ cột: ánh sáng giúp đọc đường và thay đổi gameplay; puzzle có gợi ý đủ rõ trong môi trường; combat ưu tiên đọc báo hiệu, né, chiếu đèn và phản công.
- Kể chuyện qua môi trường, **ba văn tự ngắn**, dòng nhiệm vụ, mở đầu bằng ảnh tĩnh/camera ngắn và kết bằng ánh sáng/âm thanh/lời dẫn.

Nguồn: Tên_dự_án; kế hoạch §1–4, §21, §33–34.

## 2. Phạm vi đồ án và phần chưa làm

Kế hoạch cho **2–3 người / 12 tuần**, hoàn thành **một chương Kim Quy** có đầu, thử thách, kết; mục tiêu trải nghiệm **25–35 phút**. Không cần hoàn thành bốn vùng Linh Thần trong kỳ này.

| Bắt buộc | Giới hạn |
| --- | --- |
| Màn chơi | 1 chương, 5 không gian liên kết |
| An | Đi, chạy, nhảy, né; điều khiển ổn định |
| Lồng Đèn Sen | Hiện văn tự, xóa phong ấn, làm yếu địch; kiểm tra vật cản |
| Puzzle | 1 Trống Đồng, 1 ánh sáng/gương; gợi ý, thử lại, lưu solved |
| Combat | Dao găm, 1 loại Hắc Ảnh, 1 Hộ vệ Đá |
| Tiến trình | 3 mảnh trống, 1 tuyến nhiệm vụ |
| Vật phẩm | Túi 12 ô, 1 công thức thuốc, vật nhiệm vụ ngoài túi |
| Hỗ trợ | Menu, HUD, pause, settings, save/load/respawn, âm thanh |
| Bàn giao | Build Windows x64, source, tài liệu, nguồn asset, test và số liệu đo |

- Một ô lưu chính và một backup. Dùng model/animation có sẵn phù hợp, nhưng gameplay cần tự xây dựng theo hợp đồng của dự án.
- **Nỏ thần, móc dây tại điểm cố định, nhánh phụ** chỉ được xét sau mốc tích hợp tuần 8, còn giờ kiểm thử và hết lỗi chặn tuyến. Với người thứ ba, kế hoạch ưu tiên tối đa **một** mở rộng phù hợp; không mặc định thêm cả ba.
- Tuyến bắt buộc hoàn thành được khi các mở rộng tắt. Parry xuất hiện trong ý tưởng lớn nhưng chưa là yêu cầu triển khai bản bắt buộc của kế hoạch.
- Không có cưỡi ngựa, chặt cây/đào khoáng, xây nhà, trồng trọt, câu cá, đói/khát/nhiệt độ, thời tiết động, chế tạo sinh tồn nhiều công thức, nhiều người chơi hay thế giới mở.
- Chưa cần vứt đồ, độ bền dao, mua bán, kéo thả inventory, XP/skill tree. Không coi tài liệu kỹ thuật tổng quát là danh sách tính năng phải làm.
- Chương sau: Chim Lạc (giếng trời, móc dây/leo/nỏ), Sơn Tinh (đá/trọng lực/địa hình), Thủy Tinh (thuyền/mực nước/cửa van), rồi phục hồi trục linh mạch. Ước lượng riêng sau chương 1.

## 3. Tuyến chơi, bố cục và điều kiện tiến trình

**Trại Giếng Trời → Vườn Rêu → Điện Trống → Hành Lang Quang → Đàn Kim Quy.**

| Khu | Vai trò và mục tiêu | Thời lượng mục tiêu |
| --- | --- | --- |
| Trại Giếng Trời | Học di chuyển/nhìn/nhảy/E, nhận đèn và dao một lần, kích hoạt mốc lưu | 3–4 phút |
| Vườn Rêu | Rọi văn tự, giữ Focus xóa phong ấn, nhận mảnh A | 4–6 phút |
| Điện Trống | Đọc ba ký hiệu, gõ trình tự, mở cửa/bệ nhận mảnh B; mở lối tắt về trại | 5–7 phút |
| Hành Lang Quang | Giao chiến ngắn với Hắc Ảnh trước/sát đoạn này; xoay hai gương, nhận mảnh C; mốc trước đấu trường | 6–8 phút |
| Đàn Kim Quy | Cổng kiểm tra 3 mảnh + 2 puzzle; Hộ vệ; tương tác phục dựng trống; kết | 7–10 phút |

- Tổng 25–35 phút cần đo người chơi mới; không kéo dài bằng đi lạc hoặc lặp lại cố ý. Các khoảng từng khu là mục tiêu, không phải thời gian đã đo.
- Một dấu sáng dẫn mục tiêu kế tiếp, vật tương tác gần tầm mắt, vùng an toàn trước luật mới. Dạy chiếu đèn lên Hắc Ảnh trước Hộ vệ.
- Lối chính bắt đầu tối thiểu **2,5 m**; bậc thường ≤ **0,3 m**; phòng puzzle khoảng **12 × 12 m**; đấu trường khoảng **18 × 18 m**; hiệu chỉnh theo controller/NavMesh. Vực ngoài thử thách có rào/vách rõ.
- Nền trại **20 × 20 m** là đề xuất dựng thử của người dùng trong chat, không phải kích thước khóa trong spec.
- Các object gốc `World_Geometry`, `Gameplay`, `Lighting`, `Cameras`, `UI` có position/rotation 0 và scale 1 theo yêu cầu dựng scene của người dùng.
- Quest suy ra từ dữ kiện WorldStateStore, **không lưu bộ đếm tăng theo số event**. Event có thể phát lại khi tải.

| Quest ID | Điều kiện |
| --- | --- |
| `q_get_lantern` | Đã cấp `equipment_lantern` |
| `q_fragment_a` | Tập mảnh chứa `fragment_a` |
| `q_drum` | `pz_trong_01` solved và có `fragment_b` |
| `q_light` | `pz_guong_01` solved và có `fragment_c` |
| `q_guardian` | `guardian_01` đã bị đánh bại |
| `q_restore` | `relicRestored == true` |

Thắng: đủ 3 mảnh + 2 puzzle solved + Hộ vệ chết + đã kích hoạt bệ trống. Thua: HP 0 hoặc rơi vùng chết → phục hồi **toàn bộ snapshot** checkpoint, không giữ một phần đồ sau save.

Nguồn: kế hoạch §3–4, §16, §18, §23.

## 4. Kiến trúc và vòng đời

**Tách ba loại dữ liệu:** ScriptableObject chỉ dữ liệu thiết kế; runtime state là dữ liệu thay đổi; save DTO chỉ dữ liệu thuần và ID. Không ghi máu/số lượng/solved đang chơi trực tiếp vào asset SO.

| Thành phần | Trách nhiệm |
| --- | --- |
| GameBootstrap / AppRoot | Tạo dịch vụ chung đúng một lần, Boot → MainMenu |
| GameSession | NewGame, Continue, Pause, Respawn, trạng thái phiên/checkpoint |
| InputReader | Một cách đọc input, đổi map, xóa input khi khóa |
| PlayerMotor | Nơi duy nhất dịch chuyển An: Move/Jump/Dodge |
| PlayerVitals | Máu/thể lực, tiêu hao, hồi phục, nhận damage |
| InteractionScanner | Một mục tiêu gần, hợp lệ, nhìn thấy |
| LanternController | Năng lượng, thời gian chiếu, tác động ILanternTarget |
| CombatController | Thời gian đòn, active window, tập đã trúng, hủy đòn |
| EnemyBrain | FSM, mục tiêu, vùng sinh, NavMeshAgent |
| InventoryService | TryAdd/TryRemove/CanAdd; không lượng âm |
| CraftingService | Giao dịch TryCraft nguyên tử |
| PuzzleController | TryInput/ResetAttempt/ApplySolvedState |
| WorldStateStore | Tập ID/cờ, nhận và hoàn thành một lần |
| QuestSystem | Suy bước nhiệm vụ từ trạng thái thế giới |
| SaveService | Capture/Validate/Write/Restore snapshot nhất quán |
| AudioService | AudioMixer, SFX/ambience/nhạc |

- UI gửi yêu cầu như TryCraft/TryUsePotion, không trực tiếp trừ vật phẩm/máu. Combat gọi IDamageable, lọc đội; quest không điều khiển hitbox.
- `IInteractable`: CanInteract, lời nhắc, thực hiện. `IDamageable`: DamageInfo có nguồn, đội, damage, mã đòn. `ILanternTarget`: bắt đầu/tiếp nhận/kết thúc chiếu, tiến độ. `ISaveParticipant` chỉ khi thực sự cần nhiều nơi đóng góp dữ liệu.
- Môi trường có PersistentId; đăng ký event OnEnable, hủy OnDisable. Đối tượng bị hủy không phát thưởng/gọi UI nữa.
- Dùng State, Observer, cấu hình/Strategy đơn giản; factory giới hạn. Pool chỉ sau đo đạc, reset sạch. Chưa cần event bus tổng quát, DI lớn, nhiều Singleton, Addressables hoặc Resources tải hàng loạt.
- NewGame: Boot tạo AppRoot → menu → xác nhận thay ô lưu cũ nếu có → state mặc định → scene → áp state → bật input.
- Continue/Respawn: xác thực file → scene và ID registry sẵn sàng → thay toàn bộ runtime state → áp pickup/cửa/puzzle/địch độc nhất → đặt An tại checkpoint → HUD → bật AI/input. Không trộn hai phiên.
- Phiên: MainMenu → Loading → Playing; Playing có Paused, Dead, ChapterComplete. Loading/Dead/Complete chặn Gameplay; pause dùng UI map; resume xóa lệnh giữ cũ.

Nguồn: kế hoạch §5–8, §18–19.

## 5. Controller, camera, input

Prefab đích: Player có CharacterController, PlayerMotor, PlayerVitals, InputReader, CombatController, InteractionScanner; VisualRoot có model/Animator/socket; CameraPivot có yaw/pitch độc lập model. Prototype Starter Assets hiện tại chưa đồng nghĩa đã đạt kiến trúc này.

| Giá trị khởi điểm theo spec | Giá trị |
| --- | --- |
| Capsule | Cao 1,8 m; radius 0,3; center Y 0,9 |
| Bậc / dốc | Step Offset 0,3; Slope Limit 45° |
| Đi / chạy | 3,5 / 5,5 m/s |
| Nhảy / gravity | Cao 1,2 m; Y −20 m/s² |
| Camera | Khoảng cách 3,5 m; pivot cao 1,5 m; pitch −35°..65° |
| Camera collision | Radius ban đầu 0,2 m; near clip khoảng 0,1 m |

- Hướng camera chiếu XZ, chuẩn hóa di chuyển chéo, tự tính gravity và chạm trần/đất; một nơi gọi Controller.Move. Apply Root Motion tắt trong bản đầu. Animator nhận Speed/Grounded/VerticalSpeed và Attack/Dodge/Hurt/Dead.
- Main Camera có CinemachineBrain; CinemachineCamera 3 theo pivot/PlayerCameraRoot bằng Third Person Follow. Mask tránh vật cản chứa Environment, loại Player/VFX. Kiểm tra target không None, ground mask chứa nền, Physics bật Player–Environment. Mask đúng nhưng `Avoid Obstacles` tắt vẫn chưa có tránh vật cản.

| Phím | Action/hành vi |
| --- | --- |
| WASD / chuột | Move / Look |
| Shift / Space | Sprint / Jump |
| Ctrl trái | Dodge; hướng di chuyển hoặc hướng nhìn nếu đứng yên |
| Chuột trái | Attack |
| F / giữ chuột phải | ToggleLantern / Focus |
| E / Q | Interact / UsePotion |
| I / J | Inventory / Journal |
| Esc | Pause hoặc Back lớp menu trên cùng |

- Thiết kế `PlayerControls.inputactions`: Gameplay và UI; Move/Look Vector2, còn lại Button. Không trộn input callback và polling cũ cho cùng action. Đổi map xóa Move/Focus/Attack.
- Ưu tiên Dead > Loading/khóa phiên > Hurt > Dodge > Attack > Interact > Locomotion. Trên không điều hướng hạn chế, không mở puzzle interaction/né lặp.
- Focus đi 2 m/s, không Sprint. Né: tốn 25 stamina, dài 0,45 s, bất tử 0,10–0,30 s, khoảng giữa hai lần bắt đầu ≥0,65 s, chốt hướng khi bắt đầu, vẫn va tường qua motor.
- Túi/nhật ký/chế thuốc pause thế giới; UI dùng unscaled time, gameplay dùng simulation time. Resume không để click UI thành đòn đánh.

Nguồn: kế hoạch §9–10, §37. Thông số là khởi điểm cần cân bằng, không tự nhận đã áp dụng trong prefab.

## 6. Tương tác và Lồng Đèn Sen

- Ray/sphere cast từ tâm camera, mask gồm Environment và Interactable, lấy vật gần nhất. Kiểm tra **An đến vật ≤3 m** và LOS từ nhân vật, không lấy khoảng ray camera làm tầm tay.
- Một lời nhắc; nhấn E kiểm tra lại CanInteract. Pickup đi qua đường tương tác này, không có trigger đồng thời tự cấp đồ.
- Đèn thường: miễn năng lượng, reveal văn tự trong **5 m** và có LOS; lưu đã đọc nếu là lore nhiệm vụ.
- Focus: **8 m**, góc toàn phần **60°**, kiểm tra vật cản bằng raycast Environment, nguồn/hướng hợp lý với tay cầm đèn.
- Seal: giữ **1,5 s**; mất chiếu quá **0,25 s** reset tiến độ. Ghi cleared trước, rồi mở đường/VFX một lần.
- Địch: chiếu liên tục **1 s** → nhận damage **×1,5** trong **4 s**; chiếu tiếp làm mới thời hạn, không choáng vô hạn. Hiệu ứng tạm không lưu.
- Năng lượng tối đa **100**; Focus tiêu **12/s**; ngừng Focus **1 s** rồi hồi **20/s**, kể cả đèn tắt. Hết năng lượng dừng Focus/báo ngắn, luôn có thể đợi hồi để qua cửa; không nhiên liệu hiếm bắt buộc.
- Gameplay LOS/cone/time độc lập Light/VFX và chất lượng đồ họa; có Gizmos để kiểm tra. Pause không cộng tiến độ.

Nguồn: kế hoạch §11, §24.

## 7. Hai puzzle

**Trống Đồng — `pz_trong_01`:**

- Ba ký hiệu khác hình A/B/C; lời giải mẫu **A → C → B** lộ qua đèn. Kiểm tra thứ tự, không chấm nhịp mili giây; không giới thiệu chuỗi gameplay là bản nhạc cổ xác thực.
- Debounce **0,2 s**; Unsolved/Listening/Failed/Solved; giữ currentIndex của lượt hiện tại.
- Đúng tăng index; đủ chuỗi ghi solved trước rồi mở cửa/bệ. Sai phản hồi rồi reset index về 0 sau khoảng ngắn; không mất nguyên liệu/khóa cửa.
- Reset chỉ xóa lượt chưa xong. Solved bỏ input; ApplySolvedState gọi nhiều lần an toàn. **Nhận fragment_b là thao tác riêng**, kiểm tra chưa nhận.
- Prefab dự kiến Puzzle_Drum_01 + PersistentId/controller; Drum_A/B/C có collider/interactable/noteId; gán cửa, bệ, renderer ký hiệu, AudioSource.
- Test ACB, AB, ACA, giữ/spam, reset, rời phòng, chết/load; giải được khi tắt âm.

**Ánh sáng — `pz_guong_01`:**

- Một nguồn cố định, tối đa **2 gương**, mỗi gương **4 nấc 90°**, ngọc nhận sáng; lưu cấu hình lời giải trong thiết kế. Không tách tia/đổi màu/quang học động phức tạp.
- Ngọc nhận liên tục **1 s** → solved và mở bệ fragment_c. Trước solved mất tia reset; sau solved khóa kết quả/cửa, khóa xoay hoặc dựng lại hình solved khi load. Không cần lưu lượt xoay dang dở.
- Raycast là logic; LineRenderer chỉ vẽ. Mask có cả vật cản/mục tiêu; tường chặn tia; đúng mặt phản xạ mới dùng Vector3.Reflect(direction, hit.normal), offset điểm bắt đầu tránh tự va.
- Giới hạn **3 phản xạ**, tổng chiều dài **40 m**, mặt lưng không phản xạ.
- Test tia thẳng → 1 gương → 2 gương; thử đủ **16 tổ hợp**, tường chắn, hai gương chiếu nhau. Chỉ nối thưởng sau khi logic ổn.

Nguồn: kế hoạch §12–13, §24, §35.

## 8. Combat, AI và Hộ vệ

| Chỉ số khởi điểm | An | Hắc Ảnh | Hộ vệ Đá |
| --- | --- | --- | --- |
| HP | 100 | 60 | 180 |
| Damage | Dao 20 | 15 | Quét 20, nện 25 |
| Stamina | 100 | Không mô phỏng | Không mô phỏng |

- Sprint 8 stamina/s, dao 10/đòn, né 25; hồi 20/s sau 1 s không tiêu. Thuốc hồi 35, chặn ở max HP và không dùng khi đầy.
- Đòn dao **0,65 s**: windup 0–0,15; active 0,15–0,30; recovery đến hết; tầm khoảng **1,8 m**. Hướng chốt đầu đòn, chỉ quay giới hạn lúc lấy đà.
- OverlapCapsule/vùng quét phù hợp, tìm IDamageable gốc, lọc đội/LOS. Tập mục tiêu theo mã đòn: nhiều collider một địch chỉ nhận một lần.
- CombatController nắm thời gian logic; Animation Events cho SFX/VFX, không làm nguồn damage duy nhất. FPS thấp phải kiểm tra giao nhau của khoảng frame với active window.
- Hurt có thể ngắt windup theo luật; Dead luôn hủy đòn/hitbox. Reset invulnerability, speed, tập trúng khi thoát trạng thái. Không tranh quyền dịch chuyển với motor.
- Hắc Ảnh: Idle/Patrol 2–3 điểm → Chase → Attack; nhận biết **10 m**, góc toàn phần **120°**, LOS; đánh gần **1,7 m**, cooldown **1,6 s**. Mất dấu >3 s → Search ngắn → Return; leash khoảng **18 m** từ điểm sinh.
- Trước damage kiểm tra đích sống/tầm/tường; Animator Attack không đủ để gây damage. Dead dừng Agent và đánh, tan sương, loại khỏi phiên.
- Agent là nguồn dịch chuyển địch; không thêm Rigidbody động/root motion tranh quyền. NavMeshSurface, Agent Type đúng, vùng nguy hiểm Not Walkable, cửa đóng chặn thống nhất (ví dụ obstacle carving).
- Địch hữu hạn; tối đa **3 Hắc Ảnh hoạt động gần An**. Địch thường reset ở điểm sinh khi load; Hộ vệ độc nhất theo ID defeated.
- Hộ vệ dùng chung EnemyBrain/damage/weaken, không framework boss riêng. Quét: báo **0,8 s**, active **0,2 s**, damage20, hồi **1,2 s**. Nện: báo **1,2 s**, radius khoảng **3 m**, active **0,25 s**, damage25, hồi **1,5 s**.
- Gần → quét, trung bình → nện, xa → đuổi. Có báo hiệu đọc được, không vùng damage vô hình sau khi hiệu ứng hết.
- Checkpoint ngoài cổng; vào trận khóa cổng. Chết phục hồi toàn snapshot ngoài cổng, không resume nửa trận. Thắng ghi `guardian_01`, mở cổng/bệ.
- Focus 1 s làm yếu Hộ vệ 4 s theo luật chung; không đòi vật phẩm hiếm hay năng lượng đầy khi vào. Trận thắng mục tiêu **1–3 phút**, đo người mới trước chỉnh HP.
- Phục dựng đủ điều kiện: `relicRestored=true`, checkpoint về `cp_kim_quy`, save an toàn, camera/ánh sáng/nhạc kết, khóa thưởng lại. Continue không sinh lại Hộ vệ/mảnh.

Nguồn: kế hoạch §14–16, §25.

## 9. Inventory và chế thuốc

**Chín định nghĩa**: `item_la_cam`, `item_linh_chi`, `item_thuoc_mau`, `equipment_dao`, `equipment_lantern`, `fragment_a`, `fragment_b`, `fragment_c`, `relic_kim_quy`.

- Túi **12 ô**, nguyên liệu stack **10**, thuốc stack **5**. Chọn ô/xem số lượng; chưa cần drag/drop/trọng lượng.
- Dao/đèn cấp một lần, không chiếm ô/không vứt. Ba mảnh ở tập ID riêng, không mất do đầy túi. Cổ vật hoàn chỉnh suy từ relicRestored, hiển thị nhật ký.
- TryAdd trả lượng nhận và dư. Pickup chỉ đánh dấu collected khi hết lượng. Bản bắt buộc mỗi pickup đúng **1 đơn vị**; nếu mở rộng stack ngoài thế giới phải lưu phần còn lại.
- Q: sống, chưa full HP, có thuốc, hết cooldown **2 s**; một lệnh trừ 1 thuốc + hồi35. Không tiêu hai lần cùng press. Dùng nhanh ngoài menu, túi chỉ xem.
- Craft tại trại: **1 lá cẩm + 1 linh chi → 1 thuốc máu**. Kiểm tra inventory **sau giao dịch**: ô trống do tiêu nguyên liệu có thể dùng chứa kết quả.
- Thử trên bản thay đổi tạm, commit toàn bộ khi trừ/thêm đều hợp lệ; thất bại trả lý do và giữ túi nguyên. UI không Remove/Add riêng.
- Mỗi craft thành công tại trại yêu cầu save an toàn. Ở bước tích hợp **02/11/2026**, yêu cầu hiện hành dùng **E chế trực tiếp**, rồi người chơi **E tại checkpoint để lưu thủ công**; thông báo chế thành công chỉ xác nhận runtime inventory. Chưa có panel chế thuốc, không tự gọi SaveAtCamp sau mỗi lần chế.

Nguồn: kế hoạch §17, §25.

## 10. Save, load, checkpoint — các quy tắc phải giữ

- Checkpoint ổn định: **`cp_trai`, `cp_dien_trong`, `cp_truoc_dan`, `cp_kim_quy`**.
- Save tại trại, sau puzzle lúc an toàn, sau phục dựng. Vị trí respawn là Transform checkpoint đang kích hoạt, không vị trí giữa nhảy/trận. Tại mốc hồi đầy HP/stamina/lantern energy.
- Snapshot: inventory, equipment, fragments, cửa/puzzle, seals, pickups, lore, unique enemies, relicRestored, thời gian chơi và checkpoint/scene. Reset buff tạm và puzzle dở khi load.
- ID bền cho puzzle/pickup/seal/checkpoint/địch độc nhất; không tên object, hierarchy index, GetInstanceID. Công cụ Editor/build phát hiện ID rỗng/trùng sau duplicate.
- Header `schemaVersion`, `buildVersion`, `timestamp`; DTO Serializable List/array, runtime HashSet chuyển list; JsonUtility không lưu Dictionary trực tiếp. Không serialize GameObject/SO thành state.
- Ví dụ nguồn: sceneId logic `HangLinhMach` (cần catalog ánh xạ scene file), schema1, build0.1.0; solvedPuzzles, clearedSeals, collectedPickups, readLoreIds, fragments, defeatedUniqueEnemies, equipmentGranted, relicRestored, elapsedPlaySeconds. Header có timestamp dù ví dụ JSON minh họa không đưa trường này.
- Capture main thread sau giao dịch. Một writer; ghi `save.tmp` cùng thư mục `save.json`, flush/close, đọc lại validate, thay chính có backup (Windows) hoặc rename lần đầu. Dùng `Application.persistentDataPath`.
- Ghi lỗi giữ file cũ, chỉ báo đã lưu sau thành công. Load ưu tiên chính, hỏng thử backup hợp lệ. Schema mới hơn không hỗ trợ: từ chối có kiểm soát, giữ file gốc; ID lạ xử lý rõ.
- Restore đợi scene/registry, thay toàn state, apply thế giới, đặt An, HUD, cuối cùng AI/input. Sau save nhặt/dùng rồi chết phải rollback cả đồ trong túi lẫn đồ thế giới; không trộn phiên gây nhân đôi.
- Không cần mã hóa offline. Checksum kiểm tra hỏng, không chống sửa save; không BinaryFormatter hoặc coi Base64 là bảo vệ.
- Settings dùng PlayerPrefs riêng: volume, sensitivity, window/fullscreen, quality. Save tối thiểu từ tuần2, không đợi cuối kỳ.

Nguồn: kế hoạch §7, §18–19, §26, §36.

## 11. UI, art, audio và hiệu năng

- UI: MainMenu (New/Continue/Settings/Exit), HUD HP/stamina/đèn/thuốc/objective, một prompt E, túi12 ô, nhật ký mục tiêu/3 lore, crafting và lý do thất bại, pause, chết, kết. Continue chỉ khi save hợp lệ.
- Font TMP có đủ dấu Việt, icon kèm nhãn; puzzle khác hình chứ không chỉ màu/âm. Canvas reference1920×1080, test1280×720/1080p/16:10; HUD + panel menu có thứ tự; **một EventSystem + InputSystemUIInputModule**.
- Pause/menu đổi map/cursor đúng trạng thái phiên, không bật Gameplay khi đang Dead/Loading. Master/Music/SFX volume, mouse sensitivity, cửa sổ/toàn màn hình, Low/Medium.
- Vòm hang bằng mesh ghép; Terrain cho nền/giếng trời, không dùng heightmap để tạo toàn hang rỗng. Phòng/đường nối kiểm soát ánh sáng/camera/AI.
- Tầng giếng trời xanh/nắng xuyên sương; di tích đá, Chim Lạc/hoa văn Việt; tầng sâu tinh thể tím/đỏ. Phong cách cần thống nhất dù asset từ nhiều nguồn.
- Baked light cho tĩnh, ít dynamic light và probe nhân vật; nước đơn giản. URP không mặc định có volumetric fog tương đương HDRP; fog nền/particle/mesh mờ có đo chi phí.
- An cần Idle/Walk/Run/Jump/Fall/Land/Attack/Dodge/Hurt/Death/Interact. Hắc Ảnh Idle/Walk/Attack/Hurt/Death; Hộ vệ thêm 2 đòn báo trước. Socket trước IK; root motion không tranh motor.
- AudioMixer Master/Music/SFX/Ambience/UI; ambience nước/gió 3D, UI2D; fade khám phá/combat/kết. Bước chân event/quãng đường, không mỗi frame; pause giữ âm UI.
- Nhạc cụ Việt là định hướng bản sắc; âm phép/kim loại/ambient tạm không được gọi là bản thu trống đồng hoặc nhạc cụ Việt xác thực.
- Mục tiêu **60 FPS ở1080p Medium trên máy tham chiếu nhóm chọn tuần1**, không cam kết mọi máy. Chưa có kết quả đo thì ghi chưa đo.
- Đo build tuyến3 phút ×3 lượt, median/P95 frame time (60FPS≈16,7ms), tránh giật lặp >33ms; vòng game→menu3 lần kiểm tra bộ nhớ; save/load10 lần; 3 địch ở cảnh nặng; low/medium luôn đọc báo hiệu.
- Profile CPU/GPU Development Build, Deep Profile chỉ truy vết hẹp; sửa đèn/bóng/transparency/VFX/renderers/alloc theo bằng chứng; đo lại Release. Cache, không FindObjectOfType/Resources trong Update; LOD/culling/collider đơn giản.
- Quality chỉ đổi hiển thị, không đổi beam ray, lantern detection, damage hay thời gian AI.

Nguồn: kế hoạch §20–22, §34–39; tài liệu ý tưởng phần art/audio.

## 12. Lộ trình 12 tuần và điều kiện nghiệm thu

Giả định 2×15 giờ/tuần×12 = **360 giờ**: 300 thực hiện +60 dự phòng. Ba người cùng giờ =540, phần thêm180 không tự thành tính năng bắt buộc. A chủ trì điều khiển/đèn/combat; B môi trường/puzzle/inventory/quest/save/UI; C nếu có nhận art/UI/audio/test. Cả nhóm cùng hiểu build/save.

| Tuần | Nội dung/mốc | Giờ cơ sở + dự phòng |
| --- | --- | --- |
| 1 | Scope, project/repo, input, capsule, sơ đồ/ID/asset, máy đo; build2 máy | 20+10 |
| 2 | Controller/camera, Animator nền, GameSession/checkpoint/DTO/save tối thiểu | 26+4 |
| 3 | Interaction/lantern, Vườn Rêu/lore/inventory, mảnhA có save | 30+0 |
| 4 | Hai puzzle thử, trại→Điện Trống 8–12 phút; người mới thử | 30+0 |
| 5 | Damage/stamina/dodge/AI, một trận nhỏ; gương vào Hành Lang Quang | 28+2 |
| 6 | Hộ vệ, chết/đánh lại, thuốc/crafting nguyên tử | 28+2 |
| 7 | Full tuyến tạm, world state/save/quest/respawn/kết | 28+2 |
| 8 | Tích hợp0.8, toàn chương/Continue/respawn; gate xét mở rộng | 26+4 |
| 9 | Polish dựa playtest, art/light/audio; mở rộng chỉ nếu đã chọn | 26+4 |
| 10 | Đo/tối ưu, **khóa nội dung**, tắt mở rộng chưa qua test | 22+8 |
| 11 | Regression/backup/schema/scene cycles; RC1 có commit/Editor/packages | 20+10 |
| 12 | Release máy khác, hồ sơ/video/demo8–10 phút, tag v1.0-demo | 16+14 |

Gói công 300h: công cụ18, controller32, đèn28, puzzle32, combat/AI42, item18, quest/save28, level/art36, UI/audio22, test/build/docs44. Thẻ có ID/kết quả/người/giờ/phụ thuộc/test; trạng thái Chưa làm→Đang làm→Chờ tích hợp→Chờ kiểm tra→Hoàn thành. Tích hợp giữa tuần, build nghiệm thu cuối tuần.

- Triển khai theo phụ thuộc: nền project/blockout/controller/session → catalog/interaction/đèn/puzzles/đoạn đầu → damage/AI/Hộ vệ/inventory/quest → full save/art/UI/audio/đo/build. Save tối thiểu chạy song song sớm.
- Tuần4 trễ: dừng trang trí, sửa interaction/puzzle. Tuần8 trễ: bỏ nỏ/móc dây, giảm địch/cutscene, giữ save/gợi ý/test. Tuần10 còn blocker: chỉ sửa và giữ build dự phòng.
- CT01/02 movement/camera; IN01/02 LOS/range/reward; LT01/02 seal/energy; PZ01/02 trống/16 gương; CB01/02 single-hit/i-frame; AI01/02 LOS/leash/path; IT01/02 craft/Q; QS01 event replay; BS01 boss retry.
- SV01–07: trước puzzle, sau puzzle, rollback sau chết, backup hỏng chính, schema/ID lạ, kết chương, ID trùng. UI01/02 input qua menu/độ phân giải/tắt âm; PF01 đo build; RL01 full game máy không Unity.
- Ghi từng ca build/máy/bước/kết quả/người/bằng chứng. Không gọi “đã test build” nếu mới Editor; không gọi chức năng xong chỉ vì placeholder/static check.
- Bàn giao không crash/mất save/nhân đôi/blocker; known issues không thay được tính năng bắt buộc. Gói đủ thư mục build (không chỉ exe), source Assets/Packages/ProjectSettings/.meta, README/phím/save/hardware, design/phân công/scope log, test/perf evidence, Asset_Sources, video dự phòng.
- Demo8–10 phút: bối cảnh→đèn/seal→trống/gương→combat→save/Continue→kiến trúc/kết quả. Snapshot demo riêng đúng build, không ghi đè save cá nhân; vẫn cần bằng chứng chơi từ đầu đến cuối.

## 13. Mở rộng — lưu để hiểu, không tự triển khai

- Nỏ: damage15, cooldown1s, tốn20 năng lượng/phát; phím1 dao/2 nỏ; RMB dao=Focus, nỏ=aim (Focus tạm tắt), HUD đúng. Projectile sinh muzzle theo aim có kiểm tra vật cản, sweep/Rigidbody, bỏ chủ thể, damage một lần. Không yêu cầu nỏ để qua tuyến.
- Móc dây: anchor cố định, đường định sẵn, tầm khởi điểm12m; Traversal chặn Attack/Jump, capsule kiểm tra, hủy an toàn khi bị cản/điểm đến sai/chết/scene. Không bất tử mặc định, không save giữa đường; chỉ lối tắt/nhánh phụ.
- Leo/thuyền/XP/skill/NPC/recipe mới cho giai đoạn sau; một nguồn movement/input khi đổi chế độ; save schema migrate khi mở rộng vùng/phương tiện.

Nguồn: kế hoạch §32–33. Danh mục asset và phần tự làm xem SPEC_INDEX; không nhập thêm asset chỉ vì nó có trong bảng.
