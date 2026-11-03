# Chương 4 — Thủy Tinh: Sông Ngầm

Thiết kế gốc: phần 48 của kế hoạch. Nền tảng kỹ thuật: `00_Nen_tang_dung_chung.md`. Scene: `05_SongNgamThuyTinh`.

**Điều chỉnh so với phần 48:** trong đấu trường, đền thờ chỉ **rút nước** (về mức Thấp, 6 giây rồi tự dâng lại), không có mức Cao. Nhờ vậy trận đấu có một nhịp rõ ràng: rút nước → đánh → nước dâng → né.

---

## PHẦN 1 — TÓM TẮT TỔNG QUAN

**Bối cảnh và cốt truyện.** Xuống tầng sâu nhất, An gặp **sông ngầm**: thạch nhũ phát sáng tím đỏ, rêu phát quang, đền thờ ngập nửa nước, thuyền độc mộc cũ mắc trên đá. Sơn Tinh dặn An đừng tin Thủy Tinh, nhưng càng đi An càng thấy Thủy Tinh mới là người bị hại: ông bị Hắc Sương giam dưới đáy vực, canh giữ bởi con **Thuồng Luồng**. Mục tiêu: tìm 3 mảnh **Vành Sóng** của Trống Đồng, đánh bại Thuồng Luồng, giải thoát Thủy Tinh, hòa giải núi và nước.

**Người chơi làm gì.**
- Nhận **Ngọc Nước**: tại các đền thờ nhỏ, đổi mực nước giữa Thấp, Giữa, Cao.
- Nước thấp thì đi được dưới đáy hồ; nước cao thì bè nổi thành bậc để lên chỗ cao.
- Dùng tiếp Ấn Núi để dựng bậc thoát khỏi đáy hồ.
- Lấy chìa ở gian dưới khi nước rút, đặt chìa ở gian trên khi nước dâng.
- Đánh Hắc Ảnh Nước trồi lên từ sông, và Hắc Ảnh Vỏ Đá phải rọi đèn cho vỡ giáp.
- Trận trùm: rút nước để Thuồng Luồng mắc cạn, chạy xuống đáy hồ chém, rồi chạy lên trước khi nước dâng.

**Điểm nhấn.** Đền ngập nước và sông ngầm phát sáng; câu chuyện Sơn Tinh – Thủy Tinh được kể từ phía "nước". Kiến thức: thạch nhũ hình thành thế nào, truyền thuyết giải thích lũ lụt, nghề trồng lúa nước của cư dân Văn Lang, hoa văn thuyền trên trống đồng.

**Thời lượng mục tiêu:** 10–15 phút.

---

## PHẦN 2 — ĐẶC TẢ KỸ THUẬT VÀ LỘ TRÌNH THỰC THI

### 1 Bố cục map và greybox

Mỗi phòng có nước có một `WaterBody` riêng với 3 mức (cao độ mặt nước, đơn vị m).

| Phòng | Vùng và cao độ | Mức nước Thấp / Giữa / Cao | Đối tượng chính |
| --- | --- | --- | --- |
| P1 Bến Ngầm | X −8..8, Z −8..8. Bờ Z −8..0, y = 0. Hồ Z 0..8, đáy y = −2.5 | −2.3 / −1.2 / 0.3 (mặc định Thấp) | `Spawn_C4` (0, 0.2, −6); `CP_C4_Start` lửa trại; `Pickup_NgocNuoc` (0, 1, −3); `Shrine_P1` (−4, 0, −1); `Raft_P1` 3 × 3 tâm (0, ·, 3.5); đảo đá `Islet_P1` X 3..7, Z 5..8, mặt trên y = 0.5; `QuizPedestal_C4_P1` trên đảo; `Lore_C4_P1` |
| P2 Hồ Cửa Van | X −7..7, Z 10..28. Bờ nam Z 10..14 và bờ bắc Z 26..28, y = 0. Hồ đáy y = −2 | −1.9 / −0.5 / 1.3 (mặc định Giữa) | `Shrine_P2` bờ nam; `Raft_P2` 3 × 3 tâm (2.5, ·, 19.5); bệ `Plat_LeverA` X 5..7, Z 18..21, mặt trên y = 2; `Lever_A` trên bệ; hốc đáy hồ X −7..−5, Z 20..22, y = −2, chứa `Lever_B`; `Tile_P2_1` (Ấn Núi) tại (−3, ·, 15), đáy y = −2; `Gate_P2` ở bờ bắc; `QuizPedestal_C4_P2` sau cổng; `Lore_C4_P2` |
| P3 Đền Ngập | X −7..7, Z 32..46. Lối vào Z 32..34, y = 0. Sảnh ngập đáy y = −3. Dốc nam từ đáy lên lối vào | −2.9 / −1.0 / 2.3 (mặc định Giữa) | `CP_C4_Mid` bàn thờ ở lối vào; `Shrine_P3` lối vào; gian dưới (cửa ở tường tây, sàn y = −3) chứa `Key_DenNgap`; `Raft_P3` 3 × 3 tâm (3, ·, 41); gian trên X 5..7, Z 40..44, sàn y = 3, chứa `KeySocket_DenNgap`; `Relief_C4_Temple`; `Gate_P3` tường bắc y = 0; hốc sau cổng có `QuizPedestal_C4_P3`; `Lore_C4_P3` |
| P4 Thác Ngược | X −6..6, Z 50..74. Lối đi tây X −6..−2, y = 0. Kênh X −2..6, đáy y = −2 | Cố định Giữa (−0.6), không có đền thờ | Trigger `Spawn_Trigger_P4` tại Z 56; 3 điểm trồi `RisePoint_1..3` mép kênh; 2 Hắc Ảnh Vỏ Đá trên lối đi; sân cuối Z 68..74: `Lore_C4_P4`, `QuizPedestal_C4_P4`, `CP_C4_Arena`, `Door_C4_Arena` |
| P5 Vực Thuồng Luồng | Tròn tâm (0, 0, 86). Lối vòng bán kính 7..10, y = 0. Hồ bán kính < 7, đáy y = −3. Hai dốc rộng 2 m ở hướng bắc và nam, từ lối vòng xuống đáy | Thấp −2.9 / Giữa −0.5 (mặc định Giữa) | `Shrine_Arena` (−8.5, 0, 86); 3 điểm trồi `Surface_1..3` bán kính 4, góc 90°, 210°, 330°; `ArenaGate_C4`; `Boss_ThuongLuong`; `RelicAltar_C4` ở hướng bắc lối vòng; `Lore_C4_P5`; `LinhThan_C4`; `ChapterExit_C4` |

**Quy tắc độ cao:** mặt bè = mặt nước + 0,2 m. Bậc cần nhảy giữa hai mặt đứng ≤ 1,0 m. Khoảng hở ngang giữa bè và bờ ≤ 1,5 m.

**Collider và layer:**

| Object | Collider | Layer |
| --- | --- | --- |
| Sàn, tường, dốc, đảo, bệ, gian thờ | BoxCollider hoặc MeshCollider (dốc) | Environment |
| Bè nổi `Raft_*` | BoxCollider + Rigidbody kinematic | Environment |
| Mặt nước (hình) | Plane, `M_BO_Water`, không collider | Default |
| Thể tích nước | BoxCollider trigger, đáy = đáy hồ, đỉnh = mặt nước hiện tại | Water |
| Đền thờ, cần gạt, chìa, ổ chìa, bia, bệ, ô đất có dấu | BoxCollider | Interactable |
| Đầu Thuồng Luồng `ThuongLuong_Head` | SphereCollider bán kính 0,9 | Hurtbox |
| Thân Thuồng Luồng (10 đốt) | BoxCollider | Enemy |
| KillZone | tâm (0, −13, 45), 40 × 4 × 110 | Hazard |

**Ranh giới:** mọi phòng có tường kín. Không có vực; nước sâu xử lý bằng `DeepWaterHandler` (mục 2).

### 2 Lập trình cơ chế mới: Ngọc Nước

**`Scripts/Chapters/C4/WaterBody.cs : MonoBehaviour`**

| Field | Kiểu | Mặc định |
| --- | --- | --- |
| `waterId` | string | (ví dụ `c4_water_p1`) |
| `levels` | float[3] | theo bảng mục 1 |
| `startLevel` | int | theo bảng |
| `saveLevel` | bool | true (đấu trường: false) |
| `surface` | Transform | Plane mặt nước |
| `volume` | BoxCollider | trigger layer Water |
| `bedY` | float | đáy hồ |
| `rafts` | FloatingPlatform[] | |
| `moveDuration` | float | 1,5 |

| Hàm | Logic |
| --- | --- |
| `Awake` | Kiểm tra tham chiếu; `ApplyInstant(startLevel)` |
| `OnEnable` / `OnDisable` | Đăng ký và hủy `World.Changed` → nếu `saveLevel` thì `ApplyInstant(World.GetInt("water:" + waterId, startLevel))` khi không đang chuyển |
| `int Level { get; }`, `float SurfaceY { get; }`, `event Action<WaterBody> LevelChanged` | |
| `bool IsAnyRaftOccupied()` | Với mỗi bè: `Physics.CheckBox` vùng 0,5 m trên mặt bè, mask Player |
| `bool TrySetLevel(int target, float duration)` | Bỏ qua nếu đang chuyển hoặc `IsAnyRaftOccupied()`; nếu `saveLevel` thì `World.SetInt("water:" + waterId, target)`; coroutine lerp `SurfaceY` trong `duration`: cập nhật `surface.position.y`, kích thước và tâm `volume` (từ `bedY` tới `SurfaceY`), và gọi `raft.FollowSurface(SurfaceY)`; xong thì phát `LevelChanged` |
| `ApplyInstant(int level)` | Đặt ngay không hiệu ứng (dùng khi tải) |

**`FloatingPlatform.cs`:** `float topOffset = 0.2f`; `FollowSurface(float y)`: `rb.MovePosition` để mặt trên = y + `topOffset`.

**`WaterShrine.cs : MonoBehaviour, IInteractable`**

| Field | Kiểu | Mặc định |
| --- | --- | --- |
| `target` | WaterBody | |
| `mode` | enum { Cycle, DrainOnly } | Cycle |
| `requiredAbility` | string | `PlayerAbilities.Water` |
| `requiredBossTool` | string | "" (đấu trường: `c4_tool_ngoc_trieu`) |
| `drainHoldTime` | float | 6 (DrainOnly) |
| `refillDuration` | float | 3 (DrainOnly) |
| `cooldown` | float | 10 (DrainOnly) |

- `Prompt`: Cycle thì "Đổi mực nước (hiện: Giữa)"; DrainOnly thì "Rút nước".
- `CanInteract`: có ability và tool (nếu yêu cầu), hết cooldown, `!target.IsAnyRaftOccupied()`.
- `Interact`:
  - Cycle: `target.TrySetLevel((Level + 1) % 3, 1.5f)`.
  - DrainOnly: `TrySetLevel(0, 1.0f)` → chờ `drainHoldTime` → hiện cảnh báo "Nước sắp dâng" 1 giây → `TrySetLevel(1, refillDuration)`. Phát `event Action Drained` và `event Action Refilling`.

**`DeepWaterHandler.cs`** (gắn trên mỗi WaterBody):
- `OnTriggerStay` Player: nếu `SurfaceY − chân An > 1.2` và đã quá 1 giây từ lần xử lý trước thì `PlayerVitals.TryTakeDamage(10, Neutral)` và `motor.Teleport(SafeGroundTracker.LastSafePosition)`.

**`SafeGroundTracker.cs`** (trên An):
- Mỗi 0,25 giây: nếu `motor.IsGrounded`, An không ở trong trigger nào thuộc layer Water và mặt đất không phải `FloatingPlatform`, thì lưu vị trí.
- Giá trị khởi đầu là spawnPoint của mốc lưu.

**`SluiceLever.cs : IInteractable`:** `string leverId`. Gạt một lần, `TryAdd(Puzzles, leverId)`, cần gạt xoay 90°. `SluiceGatePuzzle`: đủ `c4_lever_a` và `c4_lever_b` thì `TryAdd(Puzzles, "c4_pz_cua_van")`, tắt `Gate_P2`.

**`TempleKey`:** `Key_DenNgap` là `IInteractable`, `TryAdd(Pickups, "c4_key_den")`, ẩn. `KeySocket_DenNgap` có `CanInteract` khi có chìa; E thì `TryAdd(Puzzles, "c4_pz_den_ngap")`, hiện chìa trong ổ, tắt `Gate_P3`.

**Ấn Núi trong chương 4:** dùng lại `EarthTile` của chương 3. `Tile_P2_1` (đáy y = −2): mức 1 cho bậc từ đáy hồ lên bờ nam. Trình tự P2:
1. Rút nước Thấp.
2. Xuống đáy, gạt `Lever_B`.
3. Nâng `Tile_P2_1` lên mức 1 để leo ra.
4. Dâng nước Cao.
5. Lên bè, nhảy lên bệ, gạt `Lever_A`.

**Tương tác với Lồng Đèn:**
- `Relief_C4_Temple` là `LanternRevealSurface`: hình "nước rút mở cửa dưới, nước dâng nâng bè lên cao".
- Hắc Ảnh Vỏ Đá dùng `ArmorShellReceiver` (mục 3).
- Đầu Thuồng Luồng nhận Focus khi mắc cạn (làm yếu ×1,5).

**Trống Đồng:** không dùng. Thẻ `SC_C4_Awaken` có câu gợi ý "Vành của ta đứng thứ ba".

### 3 AI kẻ thù và đấu trường

| Prefab | Nguồn | HP | Sát thương | Tốc độ | Nhận biết | Leash | Số lượng |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PF_HacAnhNuoc` | Hắc Ảnh, `ED_HacAnhNuoc` | 70 | 15 | 2 / 3 | 12 m, 360° | 14 m | 3, trồi ở P4 |
| `PF_HacAnhVoDa` | Hắc Ảnh, `ED_HacAnhVoDa` | 60 | 18 | 2 / 2,5 | 10 m, 120° | 16 m | 2, P4 |
| `Boss_ThuongLuong` | Mới, `BossThuongLuong : BossBase` | 240 | Quất đuôi 22 | Script | Luôn biết | — | 1 |

**Hắc Ảnh Nước:** `EnemyBrain` mặc định, thêm trạng thái mở đầu `Rising`. Khi An vào `Spawn_Trigger_P4` lần đầu trong phiên: 3 con xuất hiện ở `RisePoint_*` dưới mặt nước 2 m, nâng lên lối đi trong 1 giây (bất tử, không đánh), rồi vào `Chase`. Không lưu; tải lại thì chưa trồi.

**Hắc Ảnh Vỏ Đá:** `EnemyBrain` mặc định, thay `EnemyLanternReceiver` bằng **`ArmorShellReceiver : MonoBehaviour, ILanternTarget`**:
- `bool ShellIntact = true`; `float DamageMultiplier => ShellIntact ? 0f : (IsWeakened ? 1.5f : 1f)`.
- `ReceiveFocus`: nếu `ShellIntact` thì cộng tiến độ; đủ 1,5 giây (mất chiếu quá 0,25 giây thì về 0) thì `ShellIntact = false`, ẩn khối giáp. Sau đó chạy luật làm yếu như `EnemyLanternReceiver`.
- `EnemyHealth.TryTakeDamage` trả false khi multiplier = 0, kèm phản hồi "keng" (log hoặc hiệu ứng blockout).

**Trùm Thuồng Luồng** — `Scripts/Chapters/C4/BossThuongLuong.cs : BossBase`. Blockout: đầu Cube 1,4 m và 10 đốt Cube 1 m theo đầu (dùng chung `SegmentChain` với Rết Đá chương 3; tách script `SegmentChain.cs` vào `Scripts/AI/` khi làm chương 4).

| State | Thời gian | Hành vi | Chuyển |
| --- | --- | --- | --- |
| `Submerged` | 1,5 giây | Ẩn dưới nước, di chuyển tới `Surface_i` ngẫu nhiên khác lần trước. Không bị đánh trong 8 giây thì hồi 2 HP/giây (tối đa 240) | → `SurfaceTelegraph` |
| `SurfaceTelegraph` | 1,0 giây (≤ 120 HP: 0,8 giây) | Vòng sóng `TelegraphMarker` tại điểm trồi, sau đó hộp dài 10 × rộng 1,5 hướng về An (chốt cuối báo trước) | → `TailSlam` |
| `TailSlam` | Active 0,3 giây | `DamageVolume` hộp theo đường đã chốt, `attackId = "tl_slam_" + n`, sát thương 22 (≤ 120 HP: quất 2 lần liên tiếp, lần 2 chốt lại hướng) | → `Submerged` |
| `Stranded` | Đến khi `Shrine_Arena.Refilling` | Vào ngay khi `Shrine_Arena.Drained`: đầu và thân nằm trên đáy tại điểm hiện tại; `ThuongLuong_Head` nhận sát thương; `BeginStunWindow(6, 80)`; Focus đầu 1 giây thì làm yếu | `Refilling` → `Submerged` |
| `Dead` | | Ẩn, `TryAdd(Bosses, c4_boss_thuong_luong)`, mở cổng, đặt nước về Giữa | |

`Shrine_Arena` ở chế độ DrainOnly và cần `c4_tool_ngoc_trieu`. An còn ở đáy khi nước dâng thì sau 3 giây quá sâu → `DeepWaterHandler` đưa lên lối vòng, trừ 10 máu.

**Khóa đấu trường:** `ArenaGate_C4.enterTrigger` (0, 0, 77.5) kích thước 4 × 3 × 2, ngay sau cửa nam lối vòng; bật `Gate_C4_Back`. Chết → về `c4_cp_arena`, trùm reset, nước về Giữa, cổng mở.

**Cổng vào đấu trường:** `Door_C4_Arena` cần `c4_frag_1..3` và `c4_tool_ngoc_trieu`.

### 4 Lưu tải và trạng thái thế giới

| ID | StateSet / key | Đặt bởi |
| --- | --- | --- |
| `c4_cp_start`, `c4_cp_mid`, `c4_cp_arena` | checkpointId | Mốc lưu |
| `ability_water` | Abilities | `Pickup_NgocNuoc` |
| `c4_lore_p1..p5` | Lore | `LoreStele` |
| `c4_qz_p1..p4`, `c4_qz_lt1..lt3` | QuizAnswered, QuizFirstTry | `QuizPanel` |
| `c4_ped_p1..p4` | Pickups | `QuizPedestal` |
| `c4_frag_1..3` | Fragments | Bệ P1, P2, P3 |
| `c4_tool_ngoc_trieu` | BossTools | Bệ P4 (kèm 2 `item_thuoc_mau`) |
| `water:c4_water_p1`, `water:c4_water_p2`, `water:c4_water_p3` | intKeys (0–2) | `WaterBody.TrySetLevel` |
| `tile:c4_tile_p2_1` | intKeys | `EarthTile` |
| `c4_lever_a`, `c4_lever_b`, `c4_pz_cua_van` | Puzzles | Cần gạt, `SluiceGatePuzzle` |
| `c4_key_den` | Pickups | `Key_DenNgap` |
| `c4_pz_den_ngap` | Puzzles | `KeySocket_DenNgap` |
| `c4_door_arena` | Doors | `Door_C4_Arena` |
| `c4_boss_thuong_luong` | Bosses | `BossThuongLuong` |
| `c4_relic_restored`, `c4_linhthan_done` | Puzzles | `RelicAltar_C4`, `LinhThan_C4` |

Nước P4 và đấu trường không lưu.

**Restore:** `Changed` →
- `WaterBody.ApplyInstant` từng phòng và bè theo mặt nước;
- `EarthTile.ApplyFromState`;
- cần gạt xoay đúng, cổng mở đúng;
- chìa ẩn nếu đã nhặt;
- ổ chìa hiện chìa nếu đã đặt;
- các bệ, ArenaGate, RelicAltar, ChapterExit như chương 2.

Sau khi đặt nước và khối: `Physics.SyncTransforms()`, rồi teleport An. Đặt `SafeGroundTracker.LastSafePosition` = spawnPoint của mốc.

**Nhiệm vụ `QS_C4`:**
1. Nhận Ngọc Nước.
2. Dâng nước, lên đảo đá (`c4_frag_1`).
3. Mở hai cửa van (`c4_pz_cua_van`).
4. Lấy mảnh thứ hai.
5. Lấy chìa gian dưới (`c4_key_den`).
6. Đặt chìa gian trên (`c4_pz_den_ngap`).
7. Lấy mảnh thứ ba.
8. Vượt Thác Ngược, nhận Ngọc Triều.
9. Đánh bại Thuồng Luồng.
10. Phục dựng Vành Sóng.
11. Trả lời Thủy Tinh.

**Nội dung tri thức:** theo phần 48 (trung bình đến khó). Linh Thần hỏi từ bia P2, P3, P4.

### 5 Kiểm thử và nghiệm thu

| # | Làm gì | Đạt khi |
| --- | --- | --- |
| 1 | Đi P1 → P5; cố tình rơi xuống nước sâu | Không lọt sàn; nước sâu thì về chỗ đứng an toàn gần nhất, trừ 10 máu, không bị đưa về đáy hồ |
| 2 | Đổi mực nước ở P1 qua 3 mức; thử đổi khi đang đứng trên bè | Bè theo mặt nước; đứng trên bè thì đền thờ không cho đổi |
| 3 | Làm trình tự P2 (rút → gạt B → nâng ô → dâng → gạt A) | Cổng mở khi đủ hai cần; leo ra khỏi đáy bằng ô đất được |
| 4 | Lưu ở `CP_C4_Mid`, lấy chìa, đổi nước, bấm F9 | Chìa về chỗ cũ, nước về mức lúc lưu, An không kẹt dưới nước |
| 5 | Đánh Hắc Ảnh Vỏ Đá khi chưa và sau khi rọi đèn | Chưa rọi: không mất máu. Rọi 1,5 giây: vỡ giáp, đánh được |
| 6 | Đánh Thuồng Luồng: rút nước, xuống đáy chém, đứng lại khi nước dâng; chết một lần rồi thắng | Mắc cạn khi nước rút; nước dâng thì An bị đưa lên lối vòng; chết thì reset; thắng thì không sống lại sau F9 |
| 7 | Phục dựng, trả lời Thủy Tinh, qua cổng | Sang `06_TrucLinhMach`; cả 3 năng lực còn dùng được |

Nghiệm thu: 7 bước đạt, Console không lỗi, chơi hết chương trong 10–15 phút.
