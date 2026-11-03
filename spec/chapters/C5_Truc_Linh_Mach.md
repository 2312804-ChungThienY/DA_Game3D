# Chương 5 — Trục Linh Mạch: màn kết

Thiết kế gốc: phần 49 của kế hoạch. Nền tảng kỹ thuật: `00_Nen_tang_dung_chung.md`. Scene: `06_TrucLinhMach`.

**Yêu cầu thêm cho chương 1** (ghi vào việc C1-R06):
- Thẻ thức tỉnh Kim Quy có câu gợi ý "Vành của ta vang cuối cùng".
- ID phục dựng chương 1 trong bản lưu v4 là `c1_relic_restored` (Puzzles).

---

## PHẦN 1 — TÓM TẮT TỔNG QUAN

**Bối cảnh và cốt truyện.** Bốn Linh Thần đã thức tỉnh. An đi theo **Trục Linh Mạch**, con đường ánh sáng nối bốn vùng, xuống tận lõi sâu nhất của Sơn Đoòng, nơi Hắc Sương sinh ra. Ở đây mọi ánh sáng tắt, chỉ còn Lồng Đèn Sen. An phải:
- vượt hành lang tối đầy quái;
- dùng cả bốn năng lực để qua Cầu Tứ Linh;
- giữ vững Chiến Lũy;
- đặt bốn cổ vật lên **Đàn Đồng Tế**, trả lời **Đại Vấn Đáp**, gõ **Trống Đồng bốn vành**;
- đánh bại **Hắc Sương Vương** qua bốn pha, mỗi pha có một Linh Thần ra tay giúp;
- chạy thoát khỏi hang đang sụp trong 90 giây.

**Người chơi làm gì.**
- Chiến đấu với quái hỗn hợp của cả game trong bóng tối.
- Giải câu đố liên hoàn dùng đèn, nước, đất, gió liên tiếp.
- Trả lời 5 câu tổng hợp kiến thức cả game.
- Đánh trùm cuối 4 pha: phá màn sương bằng đèn → cưỡi cột gió chiếu trùm trên cao → đứng trên khối đất né sóng sương → rút nước lộ lõi.
- Chạy thoát có đếm ngược.

**Điểm nhấn và bí mật.**
- **Bí mật Vành Ký Ức:** sau Đàn Đồng Tế có một phiến đá không chữ. Ai để ý các manh mối trong Nhật ký (mở dần khi trả lời đúng ngay lần đầu) sẽ biết đứng đúng chỗ bốn luồng sáng Linh Thần gặp nhau và rọi đèn vào phiến đá. Phiến đá hỏi hai câu về cội nguồn; trả lời xong nhận **Vành Ký Ức**, vành thứ năm của Trống Đồng.
- **Kết 1 "Ánh Sáng Trở Lại":** Hắc Sương bị phong ấn, An thoát lên mặt đất.
- **Kết 2 "Ký Ức Không Tắt"** (cần Vành Ký Ức): Hắc Sương, hiện thân của sự lãng quên, tan hẳn. Bốn Linh Thần tiễn An, phần kết điểm lại từng chương và hiện toàn bộ kiến thức đã thu thập.
- Kiến thức: từ Văn Lang đến Âu Lạc, trống đồng ở Đông Nam Á, Giỗ Tổ Hùng Vương, tín ngưỡng thờ cúng Hùng Vương được UNESCO ghi danh.

**Thời lượng mục tiêu:** 20–25 phút, khó hơn các chương trước.

---

## PHẦN 2 — ĐẶC TẢ KỸ THUẬT VÀ LỘ TRÌNH THỰC THI

### 1 Bố cục map và greybox

Map đi theo +Z. Ánh sáng môi trường P1 gần như tắt hẳn (Environment Lighting Intensity 0,05, không đèn); các phòng sau có đèn yếu.

| Phòng | Vùng (X × Z, cao độ) | Đối tượng chính |
| --- | --- | --- |
| P1 Cửa Linh Mạch | X −3..3, Z 0..40, y = 0, trần y = 5 | `Spawn_C5` (0, 0.2, 2); `CP_C5_Start` lửa trại duy nhất của chương (−2, 0, 4); `Lore_C5_P1` (Z 10); nhóm A tại Z 14–20: 2 `PF_HacAnh`, 1 `PF_HacAnhVoDa`; gờ đông X 3..5, Z 30..33, y = 2 có 1 `PF_HacAnhNem`; nhóm B tại Z 28–36: 2 `PF_HacAnh`; `QuizPedestal_C5_P1` (Z 38) |
| P2 Cầu Tứ Linh | X −8..8, Z 42..66. Sàn vào Z 42..46, y = 0. Bể nước Z 46..60, đáy y = −3. Bệ giữa `Plat_C5_Mid` X −2..2, Z 58..62, mặt trên y = 2. Gờ ra `Ledge_C5_Exit` X −3..3, Z 62..66, mặt trên y = 7 | `Seal_C5_Shrine` (LanternSealGate, chặn đền); `Shrine_C5_P2` (Cycle) sau phong ấn; `WaterBody_C5_P2` mức −2.9 / −1.5 / 0.6 (mặc định Giữa); `Raft_C5` 3 × 3 tâm (0, ·, 53); `Tile_C5_P2` (Ấn Núi) tại (0, ·, 56.5), đáy y = 0 (mức 1 = mặt trên y = 1); `Bell_C5` trên bệ giữa; `WindColumn_C5_P2` tâm (0, 2, 61), `hoverTopY = 8` (tắt tới khi gõ chuông); `Lore_C5_P2` trên bệ giữa |
| P3 Chiến Lũy | X −9..9, Z 68..86, y = 7 | `CP_C5_Bridge` (CheckpointTrigger) ở lối vào Z 68; `ArenaGate_C5_Wave` (trigger Z 71); khối đứng cao (X 7..9, Z 76..79, y = 9) cho Hắc Ảnh Ném; cửa ra Z 86; sau cửa: `QuizPedestal_C5_P3`, `Lore_C5_P3` |
| P4 Đàn Đồng Tế | X −8..8, Z 88..104, y = 7 | `CP_C5_Altar` (CheckpointTrigger, Z 89); `RelicPlacementAltar` tâm (0, 7, 96); 4 bệ vành ở bán kính 3: Bắc `Ring_Sun`, Đông `Ring_Mountain`, Nam `Ring_Wave`, Tây `Ring_Turtle`; `DaiVanDap_C5` (QuizSequence) trước đàn (0, 7, 92.5); điểm giao `MemoryPoint` (0, 7, 101); `MemoryStele` (0, 7, 103.5) quay về −Z; `Gate_P4_Boss` tường bắc Z 104; `Lore_C5_P4` |
| P5 Lõi Hắc Sương | Tròn tâm C = (0, 7, 118), bán kính 12 | `CP_C5_Boss` (CheckpointTrigger) tại (0, 7, 105); `ArenaGate_C5` (trigger (0, 7, 108)); hồ giữa bán kính 4, đáy y = 5, 2 dốc ở hướng đông và tây; `WaterBody_C5_Boss` mức Thấp 5.1 / Giữa 6.6; `Shrine_C5_Boss` (DrainOnly) tại C + (−10, 0, 0); `WindColumn_C5_Boss` tại C + (4.2, 0, 4.2), `hoverTopY = C.y + 8.5`; 4 ô `BossTile_N/E/S/W` bán kính 8, đáy y = 7; `LightDome_KimQuy` tâm (0, 7, 109.5), bán kính 3; `Boss_HacSuongVuong`; `Lore_C5_P5` ở hành lang trước cổng; cổng bắc ra P6 ở Z 130 |
| P6 Lối Lên Mặt Đất | Hành lang X −3..3 từ Z 132 | E1 đá rơi Z 132..152, y = 7; E2 tường cao 2 m tại Z 152 với `EscapeTile_1` (mức 1); E3 kênh Z 156..162 đáy y = 5, `WaterBody_C5_Escape` mặc định Giữa (6.6) với `Shrine_C5_Escape` (DrainOnly, giữ 8 giây); E4 giếng tại Z 166 với `WindColumn_C5_Escape`, `hoverTopY = 18`, gờ thoát y = 17; `EscapeStartTrigger` Z 131; `EscapeExitTrigger` trên gờ thoát |

**Collider và layer:** như các chương trước (sàn, tường, bè, khối đất là Environment; vật tương tác là Interactable; trigger là Ignore Raycast; nước là Water). `LightDome_KimQuy` là SphereCollider trigger, layer Ignore Raycast. `HacSuong_Core` là Hurtbox. KillZone tâm (0, −8, 85), 40 × 4 × 200, layer Hazard.

**Ranh giới:** tường kín ở mọi phòng; đấu trường P5 có tường vòng bán kính 12,5.

### 2 Lập trình cơ chế mới

Chương 5 **không có năng lực di chuyển mới**; dùng lại Cột Gió, Ấn Núi, Ngọc Nước, Lồng Đèn. Script mới:

**Bổ sung API nền tảng:** `LanternController.AddEnergy(float amount)` cộng năng lượng có giới hạn tối đa. `EnemyProjectile` bị hủy khi đi vào trigger có component `ProjectileBlocker`.

**`RelicPlacementAltar.cs : IInteractable`**
- Yêu cầu Puzzles: `c1_relic_restored`, `c2_relic_restored`, `c3_relic_restored`, `c4_relic_restored`.
- E: `TryAdd(Puzzles, "c5_relics_placed")`, hiện 4 cổ vật trên 4 bệ, bật `DaiVanDap_C5`.

**`DaiVanDap_C5` (QuizSequence):** `sequenceId = c5_dai_van_dap`, 5 câu `QZ_C5_Dai_1..5`, mỗi câu từ bia của một chương 2–5, câu 5 là sắp xếp mốc thời gian. `onCompleted` bật 4 `SequenceInputNode` của các bệ vành.

**Trống bốn vành:** `SequencePuzzleController` với `SQ_C5_Rings` (`puzzleId = c5_pz_trong_bon_vanh`, `sequence = { "ring_sun", "ring_mountain", "ring_wave", "ring_turtle" }`). Thứ tự này lấy từ câu gợi ý trong thẻ thức tỉnh của chương 2, 3, 4 và 1. `onSolved`:
- bật 4 tia sáng `Beam_*` (Cylinder mảnh, không collider, màu riêng mỗi Linh Thần) từ 4 bệ hội tụ tại `MemoryPoint`;
- mở `Gate_P4_Boss`.

**Cơ chế ẩn — `MemoryStele.cs : MonoBehaviour, ILanternTarget`**

| Field | Kiểu | Mặc định |
| --- | --- | --- |
| `memoryPoint` | Transform | `MemoryPoint` |
| `standRadius` | float | 0,8 |
| `requiredFocusTime` | float | 3 |
| `graceTime` | float | 0,25 |
| `revealContent` | GameObject | chữ trên bia |
| `quiz` | QuizSequence | `CoiNguon_C5` (`sequenceId = c5_qz_coi_nguon`, 2 câu `QZ_C5_CoiNguon_1..2`) |

- `CanReceiveFocus`: `World.Has(Puzzles, c5_pz_trong_bon_vanh)` và khoảng cách ngang từ An tới `memoryPoint` ≤ `standRadius` và chưa `Has(Flags, "hidden_memory_ring")`.
- `ReceiveFocus`: cộng tiến độ (mất chiếu quá `graceTime` thì về 0); đủ 3 giây thì hiện `revealContent` và bật `quiz` để tương tác E.
- `quiz.onCompleted`: `World.TryAdd(Flags, "hidden_memory_ring")`, hiện thẻ `SC_C5_VanhKyUc`, rồi `GameSession.SaveAtCheckpoint("c5_cp_altar")` (lưu ngay).
- Bia không có lời nhắc, không có trong nhiệm vụ, không hiện trên HUD.

**Manh mối trong Nhật ký — `MemoryClueList`:** 4 dòng, mỗi dòng mở khi Điểm Tri Thức ≥ 5, 10, 15, 20:
1. "Trống còn một vành chưa từng được khắc trên đàn."
2. "Vành ấy chỉ hiện nơi bốn ánh sáng Linh Thần gặp nhau."
3. "Bốn ánh sáng chỉ thắp lên khi trống bốn vành đã vang."
4. "Đứng giữa giao điểm, rọi đèn vào phiến đá không chữ sau đàn."

**Cầu Tứ Linh (P2), trình tự bắt buộc:**
1. Focus phá `Seal_C5_Shrine` (`c5_seal_cau`).
2. Đền thờ dâng nước Cao, bè lên mặt trên y = 0.8.
3. Đứng trên bè, nâng `Tile_C5_P2` lên mức 1 (mặt trên y = 1).
4. Nhảy lên ô rồi lên `Plat_C5_Mid` (y = 2).
5. Gõ `Bell_C5`: `WindColumn_C5_P2.Activate()` (`c5_wind_p2`).
6. Bay lên `Ledge_C5_Exit`.

**Thoát hang — `EscapeSequence.cs`:**

| Field | Kiểu | Mặc định |
| --- | --- | --- |
| `duration` | float | 90 |
| `timerText` | TMP_Text | HUD |
| `rockSpawner` | FallingRockSpawner | E1 |
| `resetTargets` | EarthTile, WaterBody, WaterShrine | E2–E4 |

- `EscapeStartTrigger`: bắt đầu đếm ngược và bật `rockSpawner`.
- `Update`: trừ thời gian (dừng khi pause).
- Về 0: `PlayerVitals.Kill()` → hồi sinh tại `c5_cp_escape`, toàn bộ E1–E4 về trạng thái đầu.
- `EscapeExitTrigger`: dừng đồng hồ, `EndingController.Play()`.

`FallingRockSpawner`: mỗi 1,2 giây chọn điểm trong hành lang E1 cách An 0–3 m phía trước, `TelegraphMarker` tròn bán kính 1,5 trong 1,0 giây, rồi `DamageVolume` hình cầu, sát thương 20.

**Kết thúc — `EndingController.cs`:**
- `Play()`: nếu `World.Has(Flags, "hidden_memory_ring")` thì chạy `SC_Ending2_01..06`, ngược lại `SC_Ending1_01..04` (StoryCardPanel lần lượt).
- `TryAdd(Flags, "ending_1_seen" hoặc "ending_2_seen")`; lưu tại `c5_cp_altar`; về Main Menu.
- Kết 2 có thêm màn danh đề liệt kê tiêu đề mọi `LoreDefinition` trong tập Lore.

**Chơi lại sau kết:** Continue sau kết mở `c5_cp_altar`. Khi trùm đã chết, giữa đấu trường P5 hiện `EndingShrine : IInteractable` ("Gõ trống kết thúc"): E → `EndingController.Play()`. Người chơi đã xem Kết 1 có thể quay lại tìm Vành Ký Ức rồi xem Kết 2.

**Tương tác với Lồng Đèn và Trống Đồng:**
- Đèn: hành lang tối P1 (`lantern.IsLanternOn` là nguồn sáng chính), phong ấn P2, bia ẩn, pha 1 và pha 2 của trùm.
- Trống: câu đố bốn vành ở P4 (`c5_pz_trong_bon_vanh` là điều kiện bật tia sáng và mở cổng trùm).

### 3 AI kẻ thù và đấu trường

| Prefab | Nguồn | HP | Sát thương | Vị trí |
| --- | --- | --- | --- | --- |
| `PF_HacAnh` | Chương 1 | 60 | 15 | P1 (4), P3 đợt 1 (2), đợt 2 (2) |
| `PF_HacAnhVoDa` | Chương 4 | 60 | 18 | P1 (1), P3 đợt 1 (1) |
| `PF_HacAnhNem` | Chương 2 | 50 | 12 | P1 gờ (1), P3 khối cao (1) |
| `PF_HoVeDa` | Hộ vệ Đá chương 1 (không lưu, không phải trùm) | 180 | Quét 20, nện 25 | P3 đợt 2 (1) |
| `Boss_HacSuongVuong` | Mới, `BossHacSuongVuong : BossBase` | 4 pha × 120 | Xem bảng pha | P5 |

**Chiến Lũy (P3):** `ArenaGate_C5_Wave` ở chế độ trận quái.
- An vào trigger: đóng cổng trước và sau, sinh đợt 1.
- Đợt 1 chết hết: chờ 2 giây, sinh đợt 2.
- Đợt 2 chết hết: `TryAdd(Puzzles, "c5_pz_chien_luy")`, mở cổng.
- Chết giữa trận: về `c5_cp_bridge`, mọi quái reset, cổng mở.

**Trùm Hắc Sương Vương** — `Scripts/Chapters/C5/BossHacSuongVuong.cs : BossBase`.
- Blockout: Capsule cao 5 m, vật liệu `M_BO_Boss`, Sphere `HacSuong_Core` (Hurtbox) ẩn ở đáy hồ.
- Bản đồ họa sẽ dùng rig humanoid của Hắc Ảnh phóng to (phần 49).
- `int phase` đọc từ `World.GetInt("c5_boss_phase", 1)` khi `Activate`.
- Máu mỗi pha 120; `EnemyHealth` reset đầy đầu mỗi pha.

| Pha | Linh Thần trợ lực (bật đầu pha) | Hành vi trùm | Cách gây sát thương |
| --- | --- | --- | --- |
| 1 Ánh sáng | **Kim Quy**: bật `LightDome_KimQuy`. Trong vòm: đèn hồi 30/giây (`AddEnergy`), cầu sương bị chặn (`ProjectileBlocker`) | Đi chậm 1,5 m/s về phía An. Có `FogShieldReceiver : ILanternTarget`: khiên còn thì nhận 0 sát thương. Mỗi 3 giây ném `FogOrb` (báo 0,6 giây, tốc độ 8, sát thương 15). An trong 3,5 m: quét (báo 0,8 giây, hộp 4 × 3 phía trước, sát thương 20) | Focus 2 giây phá khiên trong 8 giây, rồi chém |
| 2 Gió | **Chim Lạc**: bật `WindColumn_C5_Boss` | Bay lơ lửng tại C.y + 8 trên tâm. Mỗi 4 giây: vòng cảnh báo bán kính 3 tại vị trí An trong 1,2 giây → rơi xuống nện (active 0,25 giây, sát thương 25) → `Landed` 1,5 giây (đánh được) → bay lên 1 giây | Đánh lúc `Landed`; hoặc từ đỉnh cột gió Focus 1,5 giây khi trùm lơ lửng → `Stunned` 4 giây dưới sàn (`BeginStunWindow(4, 60)`) |
| 3 Núi | **Sơn Tinh**: mở khóa 4 `BossTile_*` (Ấn Núi thường, không tự hạ) | Đứng trên mặt hồ tâm. Báo 1,0 giây (sáng vòng) → phát 3 sóng sương liên tiếp cách nhau 1,5 giây; mỗi sóng là vòng lan từ bán kính 4 tới 12 với tốc độ 6 m/s, cao 1 m. Trúng nếu An trong dải [R − 0,5; R + 0,5], chân An dưới y = C.y + 0,9, không đang bất tử; sát thương 20; mỗi sóng một `attackId` → `Exhausted` 4 giây: quỳ ở mép hồ phía gần An | Đứng trên ô đất mức 1 hoặc lộn qua sóng để né. Chém lúc `Exhausted` (`BeginStunWindow(4, 60)`) |
| 4 Nước | **Thủy Tinh**: bật `Shrine_C5_Boss` (DrainOnly, giữ 6 giây, dâng 3 giây, hồi 10 giây, không cần công cụ) | Lặn xuống hồ. Xoáy nước 4 giây: An trong bán kính 10 bị `AddExternalVelocity` 2,5 m/s về tâm; vào nước sâu thì `DeepWaterHandler`. Giữa các xoáy ném `FogOrb` từ mặt hồ | Rút nước → lõi `HacSuong_Core` lộ ở đáy hồ, xoáy dừng. Chém lõi (`BeginStunWindow(6, 60)`); Focus lõi 1 giây thì làm yếu |

- **Chuyển pha:** máu pha về 0 → bất tử 3 giây, hiện băng chữ "[Tên Linh Thần] trợ lực!" (không pause) → bật trợ lực pha kế → `World.SetInt("c5_boss_phase", phase + 1)` → `GameSession.SaveAtCheckpoint("c5_cp_boss")`.
- **Chết trong trận:** về `c5_cp_boss`, trùm bắt đầu lại ở pha đã lưu với đầy máu pha; bật lại mọi trợ lực ≤ pha hiện tại.
- **Pha 4 về 0:** `Dead`, `TryAdd(Bosses, "c5_boss_hac_suong")`, mở cổng bắc, `SaveAtCheckpoint("c5_cp_escape")`.

**Khóa đấu trường:** `ArenaGate_C5.enterTrigger` (0, 7, 108) kích thước 4 × 3 × 2; bật `Gate_C5_Back`. Cổng bắc mở khi trùm chết.

### 4 Lưu tải và trạng thái thế giới

| ID | StateSet / key | Đặt bởi |
| --- | --- | --- |
| `c5_cp_start`, `c5_cp_bridge`, `c5_cp_altar`, `c5_cp_boss`, `c5_cp_escape` | checkpointId | Mốc lưu |
| `c5_lore_p1..p5` | Lore | `LoreStele` |
| `c5_qz_p1`, `c5_qz_p3`, `c5_qz_dai_1..5`, `c5_qz_coi_nguon_1..2` | QuizAnswered, QuizFirstTry | `QuizPanel` |
| `c5_ped_p1` (2 thuốc), `c5_ped_p3` (3 thuốc) | Pickups | `QuizPedestal` |
| `c5_seal_cau` | Seals | `Seal_C5_Shrine` |
| `water:c5_water_p2`, `tile:c5_tile_p2` | intKeys | `WaterBody`, `EarthTile` |
| `c5_wind_p2` | Puzzles | `WindColumn_C5_P2.Activate()` |
| `c5_pz_chien_luy` | Puzzles | `ArenaGate_C5_Wave` |
| `c5_relics_placed`, `c5_dai_van_dap`, `c5_pz_trong_bon_vanh` | Puzzles | Đàn, QuizSequence, trống bốn vành |
| `c5_boss_phase` | intKeys (1–4) | `BossHacSuongVuong` |
| `c5_boss_hac_suong` | Bosses | `BossHacSuongVuong` |
| `hidden_memory_ring`, `ending_1_seen`, `ending_2_seen` | Flags | `MemoryStele`, `EndingController` |

Không lưu: ô đấu trường, nước đấu trường và nước thoát hang, đồng hồ thoát hang, quái thường.

**Restore:** `Changed` →
- phong ấn, nước, ô đất, cột gió P2 áp lại;
- `ArenaGate_C5_Wave` mở nếu đã thắng, ngược lại reset quái;
- đàn hiện cổ vật nếu đã đặt;
- `DaiVanDap_C5` và các bệ vành bật theo Puzzles;
- tia sáng và cổng trùm bật nếu trống đã giải;
- `MemoryStele` hiện chữ nếu đã có cờ;
- trùm đọc `c5_boss_phase`;
- `EndingShrine` hiện nếu trùm đã chết.

**Nhiệm vụ `QS_C5`:**
1. Vượt hành lang tối.
2. Qua Cầu Tứ Linh (`c5_wind_p2`).
3. Giữ Chiến Lũy (`c5_pz_chien_luy`).
4. Đặt bốn cổ vật (`c5_relics_placed`).
5. Trả lời Đại Vấn Đáp.
6. Gõ Trống bốn vành.
7. Đánh bại Hắc Sương Vương.
8. Thoát lên mặt đất.

Vành Ký Ức không có trong nhiệm vụ.

**Nội dung tri thức:** theo phần 49 (khó, tổng hợp). `QZ_C5_CoiNguon_1`: chọn thứ tự đúng "Văn Lang → Âu Lạc" trong 4 phương án. `QZ_C5_CoiNguon_2`: ghép đúng truyền thuyết với nhân vật (ví dụ "Rùa Vàng trao móng làm lẫy nỏ — An Dương Vương").

### 5 Kiểm thử và nghiệm thu

| # | Làm gì | Đạt khi |
| --- | --- | --- |
| 1 | Đi P1 → P6; tắt đèn ở P1 | Tối gần hết khi tắt đèn; không lọt sàn hay kẹt; rơi thì hồi sinh đúng mốc |
| 2 | Làm Cầu Tứ Linh theo 6 bước; lưu ở `c5_cp_bridge`, chết, tải | Mỗi bước giữ đúng sau khi tải; không có cách bỏ qua bước |
| 3 | Trả lời sai một câu Đại Vấn Đáp; gõ trống sai rồi đúng thứ tự | Sai thì giải thích và làm lại; gõ đúng thì bật 4 tia sáng và mở cổng |
| 4 | Đánh trùm, chết ở pha 3 | Hồi sinh ở pha 3, trợ lực pha 1–3 còn bật |
| 5 | Thoát hang quá 90 giây một lần, rồi thoát kịp | Hết giờ thì về đầu đoạn thoát, mọi thứ reset; kịp thì vào kết |
| 6 | Chơi tới kết không lấy Vành Ký Ức | Kết 1; Continue mở Đàn Đồng Tế, có `EndingShrine` |
| 7 | Đứng đúng `MemoryPoint`, rọi bia 3 giây, trả lời 2 câu; chết; thắng hoặc dùng `EndingShrine` | Có cờ ngay cả sau khi chết; ra Kết 2 kèm danh đề kiến thức |

**Cách kiểm thử nhanh nếu bạn bận:**
- Bảo AI chạy qua MCP: AI đặt An tới từng điểm, bấm phím giả lập và đọc trạng thái như lần thử chương 1.
- Hoặc dùng menu Development `Tools/Son Doong/Debug/Grant Chapter 1–4 Complete` (thêm khi làm chương 5) để bắt đầu thẳng chương 5 với đủ năng lực và cổ vật.

Nghiệm thu: 7 bước đạt, Console không lỗi, cả hai kết xem được trong bản build.
