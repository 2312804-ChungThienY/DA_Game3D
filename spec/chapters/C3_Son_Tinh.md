# Chương 3 — Sơn Tinh: Thành Đá

Thiết kế gốc: phần 47 của kế hoạch. Nền tảng kỹ thuật: `00_Nen_tang_dung_chung.md`. Scene: `04_ThanhDaSonTinh`.

**Điều chỉnh so với phần 47:** mức cao của khối đất đổi từ 0 / 1,5 / 3 m thành **0 / 1 / 2 m**, vì An chỉ nhảy cao 1,2 m. Hai mức liền nhau luôn nhảy lên được.

---

## PHẦN 1 — TÓM TẮT TỔNG QUAN

**Bối cảnh và cốt truyện.** Đi xuống tầng giữa của hang, An tới một **thành đá cổ**: cổng xếp đá lớn, hoa văn hình núi, ánh lửa cam. Đây là nơi ngự của **Sơn Tinh**, thần núi. Hắc Sương khơi lại mối thù núi và nước để làm linh mạch mất cân bằng, và nhốt Sơn Tinh dưới lòng thành. Mục tiêu của An: tìm 3 mảnh **Vành Núi** của Trống Đồng, đánh bại con **Rết Đá** đang canh điện thờ, đánh thức Sơn Tinh.

**Người chơi làm gì.**
- Nhận **Ấn Núi**: đứng gần ô đất có dấu, bấm E để nâng hạ khối đất qua 3 mức, dùng làm bậc thang, làm cầu qua vực, làm tường chặn quái.
- Rọi đèn xem "dáng núi" khắc trên tường rồi dựng 4 khối đất đúng dáng để mở cổng.
- Đánh Hắc Ảnh và Hộ Vệ Đá Nhỏ trong hầm.
- Đánh trùm Rết Đá: nghe đất rung ở hang nào thì nâng khối đất bịt hang đó, rết đâm đầu vào đá, kẹt lại và lộ đầu để chém.

**Điểm nhấn.** Truyền thuyết **Sơn Tinh – Thủy Tinh** được kể qua các bia đá. Kiến thức: nước Văn Lang, các vua Hùng, kinh đô Phong Châu, sính lễ "voi chín ngà, gà chín cựa, ngựa chín hồng mao", hang động đá vôi hình thành ra sao.

**Thời lượng mục tiêu:** 10–15 phút.

---

## PHẦN 2 — ĐẶC TẢ KỸ THUẬT VÀ LỘ TRÌNH THỰC THI

### 1 Bố cục map và greybox

Map đi theo hướng +Z và thấp dần qua các dốc (độ dốc ≤ 30°).

| Phòng | Vùng (X × Z, cao độ sàn) | Đối tượng chính |
| --- | --- | --- |
| P1 Cổng Thành Đá | X −8..8, Z −8..8, y = 0, tường cao 8 | `Spawn_C3` (0, 0.2, −6); `CP_C3_Start` lửa trại (−5, 0, −5); `Pickup_AnNui` (0, 1, −2); `Lore_C3_P1`; hai ô đất hướng dẫn `Tile_P1_1` (3, 0, 4) và `Tile_P1_2` (3, 0, 6), cùng đáy y = 0; gờ `Ledge_P1` X 1..5, Z 7..8 mặt trên y = 2; `QuizPedestal_C3_P1` trên gờ |
| P2 Mỏ Cột Núi | X −6..6, Z 10..30, y = 0 | Vực rộng 6 m cắt ngang Z 17..23; 3 ô đất cầu `Tile_P2_1..3` 2 × 2 tại X = 0, Z = 18, 20, 22, **đáy y = −2**, nên mức 2 có mặt trên y = 0, bằng sàn; `Lore_C3_P2` bờ bắc; `QuizPedestal_C3_P2` bờ bắc; dốc xuống P3 ở Z 28..30 |
| P3 Sân Dáng Núi | X −7..7, Z 32..46, y = −2 | `CP_C3_Mid` bàn thờ cửa vào; 4 ô `Tile_P3_S1..S4` 2 × 2 thẳng hàng tại X = −4.5, −1.5, 1.5, 4.5, Z = 40, đáy y = −2; `Relief_C3_Shape` trên tường bắc; `Gate_P3` cửa đá Z = 46; hốc sau cổng có `QuizPedestal_C3_P3`; `Lore_C3_P3` |
| P4 Hầm Hộ Vệ | X −5..5, Z 48..72, y = −2 | 2 Hắc Ảnh, 2 Hộ Vệ Đá Nhỏ; 3 ô chặn `Tile_P4_W1..W3` sát tường, đáy y = −2; sân cuối Z 66..72: `Lore_C3_P4`, `QuizPedestal_C3_P4`, `CP_C3_Arena`, `Door_C3_Arena` |
| P5 Điện Sơn Tinh | X −9..9, Z 76..94, y = −4 (dốc từ P4) | 4 miệng hang rộng 3, cao 2 ở giữa mỗi tường (`Hole_N/E/S/W`), sau miệng là tường ranh giới cách 1 m; 4 ô `ArenaTile_N/E/S/W` 2,5 × 2,5 đặt cách miệng hang 1,5 m, đáy y = −4 (mức 2 cao 2 m che kín miệng); `ArenaGate_C3`; `Boss_RetDa`; `RelicAltar_C3`; `Lore_C3_P5`; `LinhThan_C3`; `ChapterExit_C3` |

Miệng hang phía nam (`Hole_S`) nằm cạnh cửa vào; cửa vào đấu trường lệch về tây nam (X −6..−3, Z 76) để không trùng miệng hang.

**Collider và layer:**

| Object | Collider | Layer |
| --- | --- | --- |
| Sàn, tường, dốc, gờ | BoxCollider (dốc dùng ProBuilder + MeshCollider) | Environment |
| Khối đất (`Block` con của ô) | BoxCollider, cao 4 m kéo xuống dưới để không hở khe | Environment |
| Dấu ô đất (`Rune` con, mặt phẳng 1 × 1 ở cạnh khối) | BoxCollider | Interactable |
| Bia, bệ, bàn thờ, vật nhặt, cổng có yêu cầu | BoxCollider | Interactable |
| Đầu rết `RetDa_Head` | SphereCollider bán kính 0,8 | Hurtbox |
| Thân rết `RetDa_Segment_1..8` | BoxCollider | Enemy |
| KillZone | tâm (0, −14, 45), 40 × 4 × 120 | Hazard |

**Ranh giới:** mọi phòng có tường kín. Vực P2 không có tường (là thử thách); KillZone bên dưới.

### 2 Lập trình cơ chế mới: Ấn Núi

**`Scripts/Chapters/C3/EarthTile.cs : MonoBehaviour, IInteractable, IInteractRange`**

| Field | Kiểu | Mặc định |
| --- | --- | --- |
| `tileId` | string | (bắt buộc, ví dụ `c3_tile_p2_1`) |
| `block` | Transform | Cube con |
| `baseY` | float | cao độ đáy ô |
| `levelHeights` | float[3] | { 0, 1, 2 } |
| `startLevel` | int | 0 |
| `moveDuration` | float | 0,6 |
| `remote` | bool | false (true cho `ArenaTile_*`) |
| `remoteRange` | float | 25 |
| `autoLowerAfter` | float | 0 (= không tự hạ; `ArenaTile_*` = 3) |
| `requiredAbility` | string | `PlayerAbilities.Earth` |
| `requiredBossToolForRemote` | string | `c3_tool_an_nui_lon` |
| `navObstacle` | NavMeshObstacle | carve = true |

| Hàm | Logic |
| --- | --- |
| `Awake` | Kiểm tra tham chiếu; đặt `block` tại `levelHeights[startLevel]` |
| `OnEnable` / `OnDisable` | Đăng ký và hủy `World.Changed` → `ApplyFromState()` |
| `ApplyFromState()` | `level = World.GetInt("tile:" + tileId, startLevel)`; nếu không đang di chuyển thì đặt `block` ngay đúng mức; `IsLocked = World.Has(Puzzles, lockedByPuzzleId)` nếu có |
| `Prompt` | "Nâng khối đất" hoặc "Hạ khối đất" theo mức kế tiếp |
| `MaxInteractDistance` | `remote && World.Has(BossTools, requiredBossToolForRemote) ? remoteRange : 3` |
| `CanInteract` | Có `requiredAbility`, không `IsLocked`, không đang di chuyển, và `!Physics.CheckBox(vùng khối ở mức đích, layer Player \| Enemy)` (không nâng khi An hoặc địch đang đứng trên hay trong khối) |
| `Interact` | `next = (level + 1) % 3` (ô đấu trường: luôn `next = 2`); `World.SetInt("tile:" + tileId, next)` (ô đấu trường không lưu); bắt đầu coroutine di chuyển `block.localPosition.y` tới `levelHeights[next]` trong `moveDuration` (dùng `Rigidbody` kinematic `MovePosition`); xong thì phát `event Action<EarthTile> LevelChanged`; `navObstacle.enabled = next > 0` |
| `Update` | Nếu `autoLowerAfter > 0` và ở mức 2 quá `autoLowerAfter` giây thì hạ về 0 |

**Câu đố dáng núi (P3) — `Scripts/Chapters/C3/EarthShapePuzzle.cs`:**
- Field: `string puzzleId = "c3_pz_dang_nui"`, `EarthTile[] tiles` (S1..S4), `int[] targetLevels = { 2, 0, 1, 2 }`, `GameObject gate`.
- `OnEnable`: đăng ký `LevelChanged` của từng ô và `World.Changed`.
- Mỗi lần một ô đổi xong: nếu mọi `tiles[i].Level == targetLevels[i]` thì `World.TryAdd(Puzzles, puzzleId)`.
- `World.Changed`: `solved = Has(Puzzles, puzzleId)`; solved thì tắt `gate` và khóa 4 ô (đặt `lockedByPuzzleId`).
- Không có trạng thái thất bại; người chơi đổi ô tới khi đúng.

**Tương tác với Lồng Đèn:** `Relief_C3_Shape` là `LanternRevealSurface`. Hình 4 đỉnh núi cao, thấp, vừa, cao (tương ứng 2, 0, 1, 2) chỉ hiện khi `lantern.IsLanternOn == true` trong 5 m. Đầu Rết Đá nhận Focus khi đang kẹt (làm yếu ×1,5).

**Trống Đồng:** không dùng trong chương 3. Thẻ `SC_C3_Awaken` có câu gợi ý "Vành của ta đứng thứ hai".

### 3 AI kẻ thù và đấu trường

| Prefab | Nguồn | HP | Sát thương | Tốc độ | Nhận biết | Leash | Số lượng |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PF_HacAnh` | Chương 1 | 60 | 15 | 2 / 3,5 | 10 m, 120° | 18 m | 2, tuần tra P4 |
| `PF_HoVeDaNho` | Hộ vệ Đá chương 1 thu nhỏ 0,6 lần, `ED_HoVeDaNho` | 80 | 20 (nện, bán kính 2) | 1,8 / 2,2 | 8 m, 120° | 12 m | 2, đứng gác giữa P4 |
| `Boss_RetDa` | Mới, `BossRetDa : BossBase` | 220 | 25 (lao) | Script | Luôn biết | — | 1 |

**Hộ Vệ Đá Nhỏ:** dùng `EnemyBrain` với `ED_HoVeDaNho` (`attackRange = 2`, `attackTelegraph = 1.0`, `attackActive = 0.25`, `attackCooldown = 2.2`). Đòn là `TelegraphMarker` hình tròn bán kính 2 quanh thân, rồi `DamageVolume` hình cầu. Không có Patrol (Idle tại chỗ). Ô `Tile_P4_W*` nâng lên sẽ carve NavMesh nên chặn được đường đi của nó.

**Trùm Rết Đá** — `Scripts/Chapters/C3/BossRetDa.cs : BossBase`. Blockout: đầu Cube 1,2 m và 8 đốt Cube 1 m nối nhau; mỗi đốt lerp theo vị trí đốt trước (khoảng cách 1,1 m).

| State | Thời gian | Hành vi | Chuyển |
| --- | --- | --- | --- |
| `Burrowed` | 2–3 giây ngẫu nhiên | Ẩn toàn thân | → `Rumble` ở hang ngẫu nhiên khác hang trước |
| `Rumble` | 1,2 giây (≤ 110 HP: 0,9 giây) | Miệng hang phát sáng đỏ, `TelegraphMarker` vòng tròn trước miệng, camera rung nhẹ | Hết giờ: nếu `ArenaTile` của hang đó ở mức 2 → `Stuck`; ngược lại → `ChargeTelegraph` |
| `ChargeTelegraph` | 0,6 giây | `TelegraphMarker` hộp rộng 2,4 m từ hang tới hang đối diện | → `Charge` |
| `Charge` | Tốc độ 14 m/s | Đầu chạy thẳng tới hang đối diện, thân theo sau; `DamageVolume` capsule bán kính 1,2 theo đầu, `attackId = "retda_charge_" + n`, sát thương 25. Thân không nhận sát thương | Tới hang đối diện → `Burrowed` |
| `Stuck` | 4 giây | Đầu thò ra khỏi miệng hang trước khối đất; `RetDa_Head` nhận sát thương; `BeginStunWindow(4, 70)`; Focus đầu 1 giây thì làm yếu ×1,5 | → `Burrowed` |
| `Dead` | | Ẩn, `TryAdd(Bosses, c3_boss_ret_da)`, mở cổng | |

Do mỗi lần đổi mức của ô chỉ là bấm E từ xa, người chơi cần **Ấn Núi Lớn** (`c3_tool_an_nui_lon`). Với nó, `ArenaTile_*` có tầm 25 m và tự hạ sau 3 giây, nên kịp phản ứng trong 1,2 giây rung. Không có công cụ thì không vào được đấu trường.

**Khóa đấu trường:** `ArenaGate_C3.enterTrigger` (−4.5, −4, 78) kích thước 4 × 3 × 2; bật `Gate_C3_Back` ở cửa vào. Mở khi trùm chết. Chết → về `c3_cp_arena`, trùm reset, 4 ô đấu trường về mức 0, cổng mở.

**Cổng vào đấu trường:** `Door_C3_Arena` cần `c3_frag_1..3` và `c3_tool_an_nui_lon`.

### 4 Lưu tải và trạng thái thế giới

| ID | StateSet / key | Đặt bởi |
| --- | --- | --- |
| `c3_cp_start`, `c3_cp_mid`, `c3_cp_arena` | checkpointId | Mốc lưu |
| `ability_earth` | Abilities | `Pickup_AnNui` |
| `c3_lore_p1..p5` | Lore | `LoreStele` |
| `c3_qz_p1..p4`, `c3_qz_lt1..lt3` | QuizAnswered, QuizFirstTry | `QuizPanel` |
| `c3_ped_p1..p4` | Pickups | `QuizPedestal` |
| `c3_frag_1..3` | Fragments | Bệ P1, P2, P3 |
| `c3_tool_an_nui_lon` | BossTools | Bệ P4 (kèm 2 `item_thuoc_mau`) |
| `tile:c3_tile_p1_1`, `tile:c3_tile_p1_2`, `tile:c3_tile_p2_1..3`, `tile:c3_tile_p3_s1..s4`, `tile:c3_tile_p4_w1..w3` | intKeys (0–2) | `EarthTile.Interact` |
| `c3_pz_dang_nui` | Puzzles | `EarthShapePuzzle` |
| `c3_door_arena` | Doors | `Door_C3_Arena` |
| `c3_boss_ret_da` | Bosses | `BossRetDa` |
| `c3_relic_restored`, `c3_linhthan_done` | Puzzles | `RelicAltar_C3`, `LinhThan_C3` |

`ArenaTile_*` không lưu mức (luôn về 0 khi tải).

**Restore:** `Changed` →
- `EarthTile.ApplyFromState` đặt khối ngay đúng mức, không chạy hiệu ứng;
- `EarthShapePuzzle` áp solved, mở cổng, khóa ô;
- các bệ, cổng, ArenaGate, RelicAltar, ChapterExit như chương 2.

Sau khi đặt khối, gọi `Physics.SyncTransforms()` **trước** khi teleport An, để An không bị đặt xuyên khối.

**Nhiệm vụ `QS_C3`:**
1. Nhận Ấn Núi.
2. Dựng bậc lên gờ (`c3_frag_1`).
3. Dựng cầu qua vực (`c3_frag_2`).
4. Dựng đúng dáng núi (`c3_pz_dang_nui`).
5. Lấy mảnh thứ ba (`c3_frag_3`).
6. Qua hầm, nhận Ấn Núi Lớn.
7. Đánh bại Rết Đá.
8. Phục dựng Vành Núi.
9. Trả lời Sơn Tinh.

**Nội dung tri thức:** theo phần 47 (độ khó trung bình). Linh Thần hỏi từ bia P1, P4, P5.

### 5 Kiểm thử và nghiệm thu

| # | Làm gì | Đạt khi |
| --- | --- | --- |
| 1 | Đi P1 → P5; nhảy xuống vực P2 | Không lọt sàn hay kẹt dốc; rơi vực thì chết, hồi sinh ở mốc |
| 2 | Đứng trên một ô rồi thử nâng chính ô đó; nâng ô bên cạnh | Ô đang đứng không nâng (có lời nhắc); ô bên cạnh nâng mượt 0 → 1 → 2 → 0 |
| 3 | Dựng cầu P2 bằng 3 ô rồi đi qua | Mặt ô mức 2 bằng sàn, đi qua không vấp |
| 4 | Dựng dáng núi sai rồi đúng (sau khi rọi đèn xem vách) | Sai: không có gì xảy ra. Đúng: cổng mở, 4 ô khóa |
| 5 | Lưu ở `CP_C3_Mid`, đổi vài ô P3, bấm F9 | Ô về đúng mức lúc lưu, An không bị kẹt trong khối |
| 6 | Đánh Rết Đá: để nó lao một lần, bịt hang đúng lúc một lần; chết một lần rồi thắng | Lao có báo trước; bịt đúng thì rết kẹt 4 giây; chết thì reset; thắng thì không sống lại sau F9 |
| 7 | Phục dựng, trả lời Sơn Tinh, qua cổng | Sang `05_SongNgamThuyTinh`; Ấn Núi vẫn dùng được ở chương 4 |

Nghiệm thu: 7 bước đạt, Console không lỗi, chơi hết chương trong 10–15 phút.
