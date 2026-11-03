# Chương 2 — Chim Lạc: Giếng Trời

Thiết kế gốc: phần 46 của kế hoạch. Nền tảng kỹ thuật: `00_Nen_tang_dung_chung.md`. Scene: `03_GiengTroiChimLac`.

---

## PHẦN 1 — TÓM TẮT TỔNG QUAN

**Bối cảnh và cốt truyện.** Sau khi Kim Quy thức tỉnh, An đi theo luồng gió lạ tới một **giếng trời khổng lồ** trong Sơn Đoòng: hố sụt có nắng xiên xuống, rêu và dương xỉ phủ vách, nhà sàn bằng đá treo trên cao. Hắc Sương đã bịt kín giếng trời và nhập vào **Chim Lạc**, Linh Thần của bầu trời. Mục tiêu của An: leo lên đỉnh giếng, tìm 3 mảnh **Mặt Trời Lông Chim** của Trống Đồng, giải thoát Chim Lạc.

**Người chơi làm gì.**
- Nhận **Lông Chim Lạc**, bước vào **cột gió** để bay lên cao rồi nhảy sang bệ khác.
- Dùng bộ lộn né vừa nhận từ Kim Quy.
- Rọi đèn đọc bia đá, trả lời câu hỏi để mở bệ lấy mảnh cổ vật.
- Gõ 3 chuông gió theo đúng thứ tự khắc trên vách.
- Đánh Hắc Ảnh trên cây cầu gãy và Hắc Ảnh Ném đứng trên gờ cao.
- Đánh trùm Chim Lạc trên đỉnh tổ: chờ chim đậu, cưỡi cột gió lên ngang tầm và rọi đèn cho chim choáng rơi xuống, rồi chém.

**Điểm nhấn.** Chương sáng sủa nhất game: nắng xuyên giếng trời, khu rừng mọc ngay trong lòng hang, nhà sàn đá khắc chim bay quanh mặt trời giống hoa văn trống đồng. Kiến thức: hố sụt Sơn Đoòng, hoa văn mặt trống đồng, đời sống người Lạc Việt.

**Thời lượng mục tiêu:** 10–15 phút.

---

## PHẦN 2 — ĐẶC TẢ KỸ THUẬT VÀ LỘ TRÌNH THỰC THI

### 1 Bố cục map và greybox

Map đi **lên cao**. Mọi phòng có trần mở, tường bao cao bằng độ cao phòng cộng 4 m.

| Phòng | Vùng (X × Z, cao độ sàn) | Đối tượng chính |
| --- | --- | --- |
| P1 Đáy Giếng Gió | X −8..8, Z −8..8, sàn y = 0, tường cao 20 | `Spawn_C2` (0, 0.2, −6) nhìn +Z; `CP_C2_Start` (−5, 0, −5) lửa trại; `Pickup_LongChimLac` (0, 1, −2); `Lore_C2_P1` (−7.5, 1.5, 2) trên tường tây; `WindColumn_P1_Tut` tâm (5, 0, 3); gờ `Ledge_P1` X 2..8, Z 5..8, mặt trên y = 5; `QuizPedestal_C2_P1` (6, 5, 7); cửa ra bắc rộng 3 m ở Z = 8, cao độ 5 |
| P2 Vách Gió | X −1..11, Z 10..22, sàn y = 5, tường cao 18 | `Plat_P2_A` 4 × 4, mặt trên y = 9; `Plat_P2_B` 4 × 4, mặt trên y = 13; `WindColumn_P2_1` (sàn lên A), `WindColumn_P2_2` (A lên B); `Lore_C2_P2` trên A; `QuizPedestal_C2_P2` trên B; cửa ra bắc ở cao độ 13 |
| P3 Nhà Sàn Đá Treo | X −2..12, Z 24..38, sàn y = 13 | `CP_C2_Mid` (bàn thờ) ngay cửa vào; 3 nhà sàn `House_1..3` (sàn 3 × 3, mặt trên y = 14.5, 4 cột Cube 0,3 m); `Bell_1..3` trên mỗi nhà sàn; `Relief_C2_Order` trên tường bắc; `Lore_C2_P3`; `WindColumn_P3_Final` (tắt lúc đầu) lên gờ `Ledge_P3` mặt trên y = 18; `QuizPedestal_C2_P3` trên gờ; cửa ra bắc ở cao độ 18 |
| P4 Cầu Mây Gãy | Cầu rộng X 3..7, Z 40..64, mặt cầu y = 18 | Hai khe hở 1,5 m tại Z 48–49,5 và Z 56–57,5; lan can cao 0,6 m hai bên trừ chỗ khe; gờ `Ledge_P4_L` (X −2..1, Z 50..53) và `Ledge_P4_R` (X 9..12, Z 50..53) mặt trên y = 21; `WindColumn_P4_L/R` từ cầu lên gờ; sân cuối X 1..9, Z 64..72, y = 18: `Lore_C2_P4`, `QuizPedestal_C2_P4`, `CP_C2_Arena` (CheckpointTrigger), `Door_C2_Arena` (RequirementDoor) |
| P5 Đỉnh Tổ Chim | Tròn tâm (5, 18, 82), bán kính 9, sàn y = 18 | Tường viền cao 1 m, hở 2 khe rộng 3 m ở hướng đông và tây (bên dưới là KillZone); 3 cột đậu `Perch_1..3` 1,5 × 1,5, cao tới y = 24, đặt ở bán kính 7 tại góc 90°, 210°, 330°; `WindColumn_Arena` tâm đấu trường, đỉnh lơ lửng y = 24.5; `ArenaGate_C2`; `Boss_ChimLac`; `RelicAltar_C2` (5, 18, 89); `Lore_C2_P5`; `LinhThan_C2` (QuizSequence); `ChapterExit_C2` |

**Quy tắc đặt cột gió:** tâm cột cách mép bệ đích 1,5 m; đỉnh lơ lửng `hoverTopY` = mặt trên bệ đích + 1,0 m. Đi từ cột sang bệ bằng nhảy trong lúc lơ lửng.

**Collider và layer:**

| Object | Collider | Layer |
| --- | --- | --- |
| Sàn, tường, bệ, nhà sàn, cột đậu, cầu, lan can | BoxCollider | Environment |
| Cột gió | CapsuleCollider trigger (bán kính 1,2, cao bằng `hoverTopY` − sàn + 1) | Ignore Raycast |
| Hình cột gió | Cylinder không collider, `M_BO_Mechanic` trong suốt | Default |
| Bia, bệ vấn đáp, chuông, vật nhặt, bàn thờ | BoxCollider | Interactable |
| Trigger mốc lưu, trigger đấu trường | BoxCollider trigger | Ignore Raycast |
| KillZone | BoxCollider trigger: tâm (5, −10, 40), kích thước 60 × 4 × 120 | Hazard |

**Ranh giới:** P1–P3 kín tường. P4: tường ranh giới vô hình cao 6 m dọc X = 1 và X = 9 bao ngoài lan can, trừ đoạn có gờ (hai gờ có tường riêng). P5: tường ranh giới ở bán kính 9,5, trừ hai khe đông và tây (thử thách đẩy gió).

### 2 Lập trình cơ chế mới: Cột Gió

**`Scripts/Chapters/C2/WindColumn.cs : MonoBehaviour`**

| Field | Kiểu | Mặc định |
| --- | --- | --- |
| `columnId` | string | (bắt buộc, ví dụ `c2_wind_p3_final`) |
| `startsActive` | bool | true |
| `requiredAbility` | string | `PlayerAbilities.Wind` |
| `requiredBossTool` | string | "" (đấu trường: `c2_tool_long_gio_lon`) |
| `liftSpeed` | float | 6 |
| `hoverTopY` | float | (theo quy tắc trên) |
| `visual` | GameObject | Cylinder con |
| `context` | ChapterContext | |

| Hàm | Logic |
| --- | --- |
| `Awake` | Kiểm tra `context`, `visual`, collider là trigger; thiếu thì LogError và tắt |
| `OnEnable` / `OnDisable` | Đăng ký và hủy `World.Changed` → `Refresh()` |
| `Refresh()` | `IsActive = (startsActive \|\| World.Has(Puzzles, columnId)) && (requiredBossTool == "" \|\| World.Has(BossTools, requiredBossTool))`; bật hoặc tắt `visual` theo `IsActive` |
| `Activate()` | Gọi bởi câu đố: `World.TryAdd(Puzzles, columnId)` (cột mở bằng câu đố được lưu) |
| `OnTriggerStay(Collider other)` | Bỏ qua nếu `!IsActive`, `Time.timeScale == 0` hoặc không phải Player; lấy `PlayerMotor` bằng `GetComponentInParent`; nếu `context.Abilities.Has(requiredAbility)` thì `motor.RequestLift(liftSpeed, hoverTopY)` |

**Thay đổi trong `PlayerMotor` (đã có trong nền tảng):** khi có `RequestLift` trong 0,1 giây gần nhất: vận tốc dọc = `MoveTowards(v, liftSpeed, 30 * dt)`; khi chân ≥ `hoverTopY − 0,3` thì kẹp vận tốc dọc về 0, `IsHovering = true`; Animator dùng trạng thái trên không. Nhảy khi `IsHovering`. Hết lift thì trọng lực bình thường.

**Câu đố chuông gió (P3):** dùng `SequencePuzzleController` với `SQ_C2_Bells` (`puzzleId = c2_pz_chuong_gio`, `sequence = { "bell_2", "bell_3", "bell_1" }`). `Bell_1..3` là `SequenceInputNode`. `onSolved` → `WindColumn_P3_Final.Activate()`.

**Tương tác với Lồng Đèn:**
- `Relief_C2_Order` là `LanternRevealSurface`. Thứ tự 3 hình chim (chim số 2, 3, 1) chỉ hiện khi `lantern.IsLanternOn == true`, trong 5 m và không bị che.
- Trùm Chim Lạc nhận Focus qua `ChimLacFocusReceiver : ILanternTarget`; điều kiện `CanReceiveFocus = state == Perched`.

**Trống Đồng:** không dùng trong chương 2. Thẻ thức tỉnh `SC_C2_Awaken` có câu gợi ý "Vành của ta đứng thứ nhất" cho câu đố trống chương 5.

### 3 AI kẻ thù và đấu trường

| Prefab | Nguồn | HP | Sát thương | Tốc độ | Nhận biết | Leash | Số lượng, vị trí |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PF_HacAnh` | Chương 1, `ED_HacAnh` | 60 | 15 | 2 / 3,5 | 10 m, 120° | 18 m | 2 trên cầu P4, tuần tra Z 42↔62 |
| `PF_HacAnhNem` | Mới, `ED_HacAnhNem` | 50 | 12 (cầu), 10 (cận chiến) | Đứng yên | 14 m, 360°, cần LOS | — | 1 trên mỗi gờ P4 |
| `Boss_ChimLac` | Mới, `BossChimLac : BossBase` | 200 | Lao 20 | Bay theo script | Luôn biết vị trí An | — | 1 ở P5 |

**Hắc Ảnh Ném** — `Scripts/Chapters/C2/ThrowerBrain.cs`, không dùng NavMeshAgent, luôn xoay mặt về An:
- `Idle` → `Aim`: An trong 14 m và có LOS.
- `Aim`: hiện `TelegraphMarker` đường thẳng từ tay tới vị trí An trong 0,6 giây → `Throw`.
- `Throw`: sinh `EnemyProjectile` (bán kính 0,3, tốc độ 7 m/s, sát thương 12, hướng chốt lúc ném) → `Cooldown` 2,5 giây → `Aim` nếu còn LOS, ngược lại `Idle`.
- An trong 2 m: đòn cận chiến như Hắc Ảnh (sát thương 10, hồi 1,6 giây).
- Làm yếu: `EnemyLanternReceiver` (Focus 1 giây → nhận ×1,5 trong 4 giây). `Died` → `Dead`.

**Trùm Chim Lạc** — `Scripts/Chapters/C2/BossChimLac.cs : BossBase`. Blockout: thân Cube 1,5 × 1 × 2, hai cánh Cube dẹt, vật liệu `M_BO_Boss`.

| State | Thời gian | Hành vi | Chuyển |
| --- | --- | --- | --- |
| `Perched` | 3,5 giây (≤ 100 HP: 2,5 giây) | Đậu trên đỉnh `Perch_i`; nhận Focus | Đủ Focus 1,5 giây (mất chiếu quá 0,25 giây thì về 0) → `Stunned`; hết thời gian → `Gust` (50%) hoặc `DiveTelegraph` |
| `Gust` | Báo 0,8 giây, active 2,0 giây | Quạt gió: mỗi frame `motor.AddExternalVelocity(hướng từ trùm ra ngoài * 4)`, trừ khi An đang trong `WindColumn` đang bật | → `DiveTelegraph` |
| `DiveTelegraph` | 1,0 giây | `TelegraphMarker` hình hộp rộng 2 m từ đỉnh cột tới vị trí An (chốt lúc bắt đầu) | → `Dive` |
| `Dive` | 0,4 giây | Bay thẳng theo đường đã chốt; `DamageVolume` capsule bán kính 1, `attackId = "chimlac_dive_" + n`, sát thương 20 | → `Landed` (≤ 100 HP: lao thêm 1 lần trước khi `Landed`) |
| `Landed` | 1,5 giây | Đứng dưới sàn, nhận sát thương thường | → `FlyToPerch` |
| `Stunned` | 4 giây | Rơi xuống sàn dưới cột đang đậu; `BeginStunWindow(4, 60)`; được tính đang bị làm yếu ×1,5 | → `FlyToPerch` |
| `FlyToPerch` | 1,0 giây, bất tử | Bay tới cột ngẫu nhiên khác cột hiện tại | → `Perched` |
| `Dead` | | Ẩn, `TryAdd(Bosses, c2_boss_chim_lac)`, mở cổng | |

Muốn chiếu được trùm đang đậu (đỉnh cột y = 24, cách tâm 7 m), An phải đứng lơ lửng trên `WindColumn_Arena` (y = 24,5). Cột này chỉ bật khi đã có `c2_tool_long_gio_lon` (công cụ của P4).

**Khóa đấu trường:** `ArenaGate_C2.enterTrigger` đặt tại (5, 18, 75) kích thước 6 × 3 × 2. Player vào và trùm chưa chết → bật `Gate_C2_Back` ở cửa nam đấu trường, `Boss_ChimLac.Activate()`. Mở lại khi trùm chết. An chết → `Respawn` về `c2_cp_arena`, trùm reset, cổng mở.

**Cổng vào đấu trường:** `Door_C2_Arena` (RequirementDoor) cần `c2_frag_1`, `c2_frag_2`, `c2_frag_3` (Fragments) và `c2_tool_long_gio_lon` (BossTools).

### 4 Lưu tải và trạng thái thế giới

| ID | StateSet | Đặt bởi |
| --- | --- | --- |
| `c2_cp_start`, `c2_cp_mid`, `c2_cp_arena` | (checkpointId) | `CheckpointAltar`, `CheckpointTrigger` |
| `ability_wind` | Abilities | `Pickup_LongChimLac` |
| `c2_lore_p1` … `c2_lore_p5` | Lore | `LoreStele` |
| `c2_qz_p1` … `c2_qz_p4`, `c2_qz_lt1` … `c2_qz_lt3` | QuizAnswered, QuizFirstTry | `QuizPanel` |
| `c2_ped_p1` … `c2_ped_p4` | Pickups | `QuizPedestal` |
| `c2_frag_1`, `c2_frag_2`, `c2_frag_3` | Fragments | Phần thưởng bệ P1, P2, P3 |
| `c2_tool_long_gio_lon` | BossTools | Phần thưởng bệ P4 (kèm 2 `item_thuoc_mau`) |
| `c2_pz_chuong_gio` | Puzzles | `SQ_C2_Bells` |
| `c2_wind_p3_final` | Puzzles | `WindColumn_P3_Final.Activate()` |
| `c2_door_arena` | Doors | `Door_C2_Arena` |
| `c2_boss_chim_lac` | Bosses | `BossChimLac` |
| `c2_relic_restored` | Puzzles | `RelicAltar_C2` |
| `c2_linhthan_done` | Puzzles | `LinhThan_C2` |

**Lưu trong JSON:** toàn bộ qua `WorldSnapshot` (không có trường riêng của chương). Hắc Ảnh và Hắc Ảnh Ném không lưu; khi tải chúng về điểm sinh, đầy máu.

**Restore:** `WorldStateStore.Restore` → `Changed` →
- `AbilityPickup`, `QuizPedestal` ẩn hoặc hiện theo Abilities, Pickups;
- `WindColumn.Refresh`;
- `SequencePuzzleController` áp solved;
- `RequirementDoor` mở hoặc đóng;
- `ArenaGate` reset;
- `RelicAltar` hiện cổ vật nếu `c2_relic_restored`;
- `ChapterExit` hiện nếu `c2_linhthan_done`.

**Nhiệm vụ `QS_C2`:**
1. Nhận Lông Chim Lạc (`ability_wind`).
2. Lên gờ, lấy mảnh thứ nhất (`c2_frag_1`).
3. Vượt Vách Gió (`c2_frag_2`).
4. Gõ chuông gió theo thứ tự trên vách (`c2_pz_chuong_gio`).
5. Lấy mảnh thứ ba (`c2_frag_3`).
6. Qua cầu, nhận Lông Gió Lớn (`c2_tool_long_gio_lon`).
7. Giải thoát Chim Lạc (`c2_boss_chim_lac`).
8. Phục dựng Mặt Trời Lông Chim (`c2_relic_restored`).
9. Trả lời Chim Lạc (`c2_linhthan_done`).

**Nội dung tri thức:** 5 bia và câu hỏi theo bảng phần 46 (độ khó dễ). Mỗi `QZ_` trỏ `relatedLoreId` tới bia cùng phòng. 3 câu Linh Thần lấy từ bia P2, P3, P5.

### 5 Kiểm thử và nghiệm thu

Bạn có thể tự chơi theo các bước dưới, hoặc bảo AI chạy tự động qua MCP như lần thử chương 1.

| # | Làm gì | Đạt khi |
| --- | --- | --- |
| 1 | Đi từ P1 tới P5, cố tình nhảy xuống ở P4 và lao ra khe đấu trường | Không lọt sàn, không kẹt tường; rơi thì chết và hồi sinh ở mốc gần nhất |
| 2 | Bước vào cột gió trước và sau khi nhặt Lông Chim Lạc | Trước: không bay. Sau: bay lên, lơ lửng, nhảy sang bệ được |
| 3 | Gõ chuông sai thứ tự, rồi đúng (sau khi rọi đèn xem vách) | Sai: báo sai, về 0. Đúng: cột gió cuối bật, không bật lại lần hai |
| 4 | Trả lời sai một bệ vấn đáp rồi trả lời đúng | Hiện giải thích, nhận đủ mảnh; bệ không trao lần hai |
| 5 | Lưu ở `CP_C2_Mid`, lấy mảnh 3, bấm F9 | Mảnh 3 mất, bệ P3 và chuông về chưa giải, An đứng ở mốc |
| 6 | Vào đấu trường không có Lông Gió Lớn; sau đó đánh trùm, chết một lần rồi thắng | Cổng không mở khi thiếu đồ; chết thì trùm reset, cổng mở; thắng thì cổng mở hẳn, trùm không sống lại sau F9 |
| 7 | Phục dựng, trả lời Chim Lạc, qua cổng chương | Sang `04_ThanhDaSonTinh`, Continue mở đúng chương 3 |

Nghiệm thu: 7 bước đạt, Console không lỗi, chơi hết chương trong 10–15 phút.
