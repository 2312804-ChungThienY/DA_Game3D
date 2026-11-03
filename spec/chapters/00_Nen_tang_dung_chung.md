# Nền tảng dùng chung cho chương 2–5

Phiên bản 01/11/2026. Đọc file này trước mọi file chương. Thiết kế tổng thể nằm ở phần 40–54 của `spec/Ke_hoach_game_Son_Doong_12_tuan.md`.

Các hệ thống dưới đây được xây trong lúc hoàn thiện chương 1 (phần 45, lịch ngày 1–3). **Agent làm chương 2–5 không viết lại các hệ này.** Nếu một API dưới đây chưa tồn tại trong code, dừng lại và báo người dùng; không tự chế một bản khác.

## 1 Quy ước

| Hạng mục | Quy ước |
| --- | --- |
| Script dùng chung | `Assets/_Project/Scripts/<Module>/` (Core, Player, Interaction, Combat, AI, Puzzles, Inventory, Quests, Save, UI, Knowledge) |
| Script riêng chương | `Assets/_Project/Scripts/Chapters/C2/` … `C5/` |
| Prefab | `Assets/_Project/Prefabs/Shared/`, `Assets/_Project/Prefabs/Chapters/C2/` …; tên bắt đầu `PF_` |
| Dữ liệu ScriptableObject | `Assets/_Project/Data/Chapters/C2/` …; tiền tố `QZ_` câu hỏi, `LR_` bia, `ED_` địch, `SQ_` câu đố chuỗi, `QS_` nhiệm vụ, `SC_` thẻ truyện |
| Scene | `03_GiengTroiChimLac`, `04_ThanhDaSonTinh`, `05_SongNgamThuyTinh`, `06_TrucLinhMach` trong `Assets/_Project/Scenes/` |
| ID lưu | Chữ thường, tiền tố chương `c2_`…`c5_`. Không đổi ID sau khi đã có bản lưu. Mọi ID phải có trong `WorldIdCatalog` |
| Gốc scene | `World_Geometry`, `Gameplay`, `Lighting`, `Cameras`, `UI`, `ChapterContext` (position/rotation 0, scale 1) |
| Đơn vị | Mét, trục Y hướng lên. Kích thước ghi "dài × rộng × cao" = Z × X × Y |
| Chữ | Tiếng Việt có dấu, TMP font asset từ Noto Sans (A31) |
| Blockout | Chỉ Cube, Cylinder, Sphere, Plane, ProBuilder. Vật liệu màu đơn sắc URP/Lit trong `Materials/Blockout/` |

Màu blockout: sàn xám `M_BO_Floor`, tường nâu `M_BO_Wall`, vật tương tác vàng `M_BO_Interact`, cơ chế chương xanh lơ `M_BO_Mechanic`, nguy hiểm đỏ trong suốt `M_BO_Telegraph`, nước xanh dương trong suốt `M_BO_Water`, quái tím `M_BO_Enemy`, trùm tím đậm `M_BO_Boss`.

## 2 Layer và va chạm

| Index | Layer | Dùng cho |
| --- | --- | --- |
| 0 | Default | Object không va chạm gameplay |
| 2 | Ignore Raycast | Mọi trigger volume (cột gió, trigger đấu trường, trigger mốc lưu) để không chặn raycast |
| 3 | Environment | Sàn, tường, khối đất, bè nổi, tường ranh giới |
| 4 | Water | Trigger thể tích nước |
| 6 | Player | Capsule của An |
| 7 | Enemy | Collider thân địch và trùm |
| 8 | Interactable | Collider vật tương tác (bia, bệ, chuông, đền thờ, ô đất có dấu dùng thêm collider con layer này) |
| 9 | Hurtbox | Điểm yếu của trùm (đầu rết, lõi) |
| 10 | LanternTarget | Collider nhận chiếu đèn |
| 12 | **Hazard** (thêm mới) | KillZone |

Ma trận va chạm thêm: Hazard chỉ va Player. Water va Player và Enemy. Các cặp khác giữ như chương 1.

Mỗi map có:
- **KillZone**: `PF_KillZone`, BoxCollider trigger, layer Hazard, đặt thấp hơn sàn thấp nhất 10 m, phủ toàn map cộng lề 20 m mỗi phía.
- **Tường ranh giới**: `PF_BoundaryWall`, BoxCollider không renderer, layer Environment, cao 6 m, đặt dọc mọi mép mở không phải thử thách.

## 3 Hợp đồng code có sẵn sau chương 1

Giữ nguyên các interface đã có:

```csharp
public interface IInteractable { string Prompt { get; } bool CanInteract(GameObject interactor); void Interact(GameObject interactor); }
public interface ILanternTarget { Transform AimPoint { get; } bool CanReceiveFocus { get; } void ReceiveFocus(float deltaTime); }
public interface IDamageable { bool TryTakeDamage(DamageInfo damage); }
```

Bổ sung trong chương 1:

```csharp
// Combat/DamageInfo.cs: thêm AttackId, giữ constructor cũ (AttackId = null).
public struct DamageInfo { public GameObject Source; public float Amount; public DamageTeam SourceTeam; public string AttackId; }

// Interaction/IInteractRange.cs: vật tương tác muốn tầm khác 3 m.
public interface IInteractRange { float MaxInteractDistance { get; } }
// InteractionScanner: nếu target implement IInteractRange thì dùng MaxInteractDistance thay interactDistance; cameraRayDistance = 25.

// Core/StateSet.cs
public enum StateSet { Fragments, Puzzles, Pickups, Seals, Doors, Bosses, BossTools, Abilities, Lore, QuizAnswered, QuizFirstTry, Flags }

// Core/WorldStateStore.cs (bản tổng quát; giữ hàm cũ của chương 1 làm wrapper)
public event Action Changed;                       // phát sau mỗi thay đổi thực sự, phát đúng 1 lần sau Restore
public bool Has(StateSet set, string id);
public bool TryAdd(StateSet set, string id);       // false nếu đã có hoặc id không có trong catalog (LogError)
public int Count(StateSet set);
public int GetInt(string key, int fallback = 0);
public void SetInt(string key, int value);         // key phải có trong catalog.intKeys
public WorldSnapshot Export();
public void Restore(WorldSnapshot snapshot);       // thay toàn bộ, không trộn

// Core/WorldIdCatalog.cs : ScriptableObject — danh sách ID hợp lệ theo StateSet và danh sách intKeys.

// Player/PlayerMotor.cs: nơi duy nhất gọi CharacterController.Move.
public bool IsGrounded { get; }
public bool IsHovering { get; }                    // đang lơ lửng ở đỉnh cột gió
public void Teleport(Vector3 position, Quaternion rotation);
public void RequestLift(float upSpeed, float hoverTopY);   // gọi mỗi frame khi trong cột gió; hết gọi 0,1 s thì trả trọng lực
public void AddExternalVelocity(Vector3 horizontalVelocity); // gọi mỗi frame (gió đẩy, xoáy nước); cộng vào vận tốc ngang frame đó
// Nhảy được khi IsGrounded hoặc IsHovering.

// Player/PlayerAbilities.cs
public const string MovementKit = "movement_kit", Wind = "ability_wind", Earth = "ability_earth", Water = "ability_water";
public bool Has(string abilityId);                 // đọc WorldStateStore.Has(StateSet.Abilities, id)

// Player/PlayerVitals.cs: thêm
public void Kill();                                // máu về 0, phát Died (dùng cho KillZone)
public void RestoreFull();

// Player/LanternController.cs: không đổi API. LanternFocusScanner lấy hướng chiếu = Camera.main.transform.forward
// (có góc ngẩng) và gốc = LanternSocket, để chiếu được mục tiêu trên cao.

// Core/GameSession.cs (trong AppRoot, DontDestroyOnLoad)
public SessionState State { get; }                 // MainMenu, Loading, Playing, Paused, Dead, ChapterComplete
public bool SaveAtCheckpoint(string checkpointId); // capture → validate → ghi file nguyên tử
public void Respawn();                             // tải lại bản lưu chính của chương hiện tại
public void LoadChapter(string sceneName, string checkpointId);
public void PushUiPause(object owner); public void PopUiPause(object owner); // timeScale 0, UI map, cursor

// Core/AppRoot.cs: singleton duy nhất được phép. AppRoot.Instance.World / .Session / .Inventory / .Rewards
// Core/ChapterContext.cs: mỗi scene có 1; Awake lấy dịch vụ từ AppRoot; object trong scene tham chiếu ChapterContext qua Inspector.
```

Quy tắc chung cho mọi script mới:
- `Awake` kiểm tra tham chiếu bắt buộc; thiếu thì `Debug.LogError($"{name}: thiếu ...", this)` và `enabled = false`.
- Đăng ký event trong `OnEnable`, hủy trong `OnDisable`.
- Khi `Time.timeScale == 0` không cộng tiến độ, không gây sát thương.
- Không dùng `FindObjectOfType`, `GameObject.Find`, `Resources.Load` trong Update.
- Chỉ PlayerMotor di chuyển An. Chỉ NavMeshAgent di chuyển địch thường. Trùm di chuyển bằng chính script trùm (transform), không dùng Agent.
- Mọi sát thương đi qua `IDamageable.TryTakeDamage` với `DamageInfo`.

## 4 Thành phần dùng chung

### Môi trường và tiến trình

| Prefab / Script | Thành phần | Hành vi |
| --- | --- | --- |
| `PF_CheckpointAltar` / `CheckpointAltar : MonoBehaviour, IInteractable` | `string checkpointId`, `Transform spawnPoint`, `bool allowCrafting` | E: hồi đầy máu, thể lực, đèn; `GameSession.SaveAtCheckpoint(checkpointId)`; nếu `allowCrafting` mở bảng chế thuốc |
| `PF_CheckpointTrigger` / `CheckpointTrigger` | `string checkpointId`, `Transform spawnPoint`, BoxCollider trigger layer Ignore Raycast | Player vào lần đầu: lưu tự động (dùng trước đấu trường, đầu pha trùm) |
| `PF_KillZone` / `KillZone` | trigger layer Hazard | Player vào: `PlayerVitals.Kill()` |
| `PF_AbilityPickup` / `AbilityPickup : IInteractable` | `string abilityId`, `string displayName`, `StoryCard card` | E: `TryAdd(Abilities)`, hiện thẻ hướng dẫn, ẩn object; `Changed` → ẩn nếu đã có |
| `PF_RequirementDoor` / `RequirementDoor : IInteractable` | `List<StateRequirement> requirements`, `GameObject door`, `string doorId` | Mở khi đủ mọi yêu cầu; lời nhắc liệt kê thứ còn thiếu; mở thì `TryAdd(Doors, doorId)` |
| `PF_RelicAltar` / `RelicAltar : IInteractable` | `string[] fragmentIds` (3), `string bossId`, `string restoredId`, `GameObject restoredVisual`, `UnityEvent onRestored` | Đủ 3 mảnh và trùm đã chết: `TryAdd(Puzzles, restoredId)`, hiện cổ vật, gọi `onRestored` |
| `PF_ChapterExit` / `ChapterExit` | `string nextScene`, `string nextCheckpointId`, `string requiredPuzzleId` | Khi `requiredPuzzleId` có trong Puzzles: hiện cổng sáng; E → `GameSession.LoadChapter` |
| `StateRequirement` | `[Serializable] { StateSet set; string id; string displayName; }` | Dùng chung cho cửa, nhiệm vụ |
| `QuestStepList` (SO) / `ChapterQuestTracker` | `steps: { string text; List<StateRequirement> requirements }` | HUD hiện step đầu tiên còn thiếu yêu cầu; suy từ world state, không đếm event |
| `StoryCard` (SO) / `StoryCardPanel` | `string title`, `[TextArea] body`, `Sprite image` (để trống ở blockout) | Bảng ảnh tĩnh và lời dẫn, pause gameplay, nút Tiếp tục |

### Lồng Đèn và câu đố

| Script | Thành phần | Hành vi |
| --- | --- | --- |
| `LanternRevealSurface` | `GameObject revealContent`, `Transform revealPoint`, `float revealRange = 5f`, `float checkInterval = 0.2f` | Mỗi 0,2 giây: hiện `revealContent` nếu `lantern.IsLanternOn`, khoảng cách ≤ 5 m và Raycast Environment từ LanternSocket tới `revealPoint` không bị chặn; ngược lại ẩn. Không lưu trạng thái |
| `SequencePuzzleDefinition` (SO) | `string puzzleId`, `string[] sequence`, `float inputDelay = 0.2f`, `float failDelay = 0.7f` | Dữ liệu câu đố chuỗi dùng chung |
| `SequencePuzzleController` | `SequencePuzzleDefinition definition`, `TMP_Text statusText` (chữ 3D cạnh câu đố, không đặt trên HUD), `UnityEvent onSolved` | `bool TryInput(string nodeId)`: bỏ qua nếu solved hoặc đang chờ; so `sequence[currentIndex]`; đúng thì tăng; đủ thì `TryAdd(Puzzles, puzzleId)` rồi `onSolved`; sai thì báo sai, chờ `failDelay`, `currentIndex = 0`. Khi `Changed`: chỉ áp trạng thái solved, **không reset lượt đang thử** |
| `SequenceInputNode : IInteractable` | `string nodeId`, `string prompt`, `SequencePuzzleController controller` | E → `controller.TryInput(nodeId)` |
| `LanternSealGate : ILanternTarget` | Giống `SealTarget` chương 1 nhưng lưu qua `TryAdd(Seals, sealId)` | Focus 1,5 giây, mất chiếu quá 0,25 giây thì về 0 |

### Tri thức

| Script | Thành phần | Hành vi |
| --- | --- | --- |
| `ContentTag` | `enum { LichSu, TruyenThuyet, TuNhien }` | Nhãn hiển thị |
| `LoreDefinition` (SO) | `string loreId`, `ContentTag tag`, `string title`, `[TextArea] body`, `string sourceRef`, `bool verified` | `verified = false` thì build Release báo lỗi |
| `QuizDefinition` (SO) | `string quizId`, `ContentTag tag`, `[TextArea] question`, `string[] options` (đúng 4), `int correctIndex`, `[TextArea] explanation`, `string relatedLoreId`, `string sourceRef`, `bool verified` | Câu sắp xếp mốc thời gian viết thành 4 phương án thứ tự |
| `LoreStele` | `LoreDefinition lore`, `LanternRevealSurface reveal`, `TMP_Text text3D` | Lần đầu hiện chữ: `TryAdd(Lore, loreId)`; Nhật ký mục Tri thức đọc tập Lore |
| `QuizPanel` (UI) | 4 nút, ô giải thích, nút Thử lại | `void Ask(QuizDefinition q, Action onSolved)`: `PushUiPause`. Sai: hiện `explanation` và tên bia liên quan, nút Thử lại. Đúng: `TryAdd(QuizAnswered, id)`; nếu đúng ở lần chọn đầu tiên của lần mở này và chưa từng trả lời sai câu này thì `TryAdd(QuizFirstTry, id)`; `PopUiPause`; gọi `onSolved` |
| `RewardEntry` | `{ RewardType type; string id; int quantity = 1; }`, `RewardType { Fragment, BossTool, Item, Ability, Flag }` | |
| `RewardService` | `bool TryGrant(List<RewardEntry> rewards, out string failReason)` | Kiểm tra `InventoryService.CanAdd` cho mọi Item trước; thiếu chỗ thì không trao gì, trả "Túi đầy" |
| `QuizPedestal : IInteractable` | `string pedestalId`, `QuizDefinition quiz`, `List<RewardEntry> rewards`, `GameObject openedVisual` | CanInteract khi `!Has(Pickups, pedestalId)`. E: nếu chưa `Has(QuizAnswered, quizId)` thì `QuizPanel.Ask`; sau đó `RewardService.TryGrant`; thành công thì `TryAdd(Pickups, pedestalId)`, hiện `openedVisual`. Túi đầy: giữ bệ, lần sau E trao lại không hỏi lại |
| `QuizSequence : IInteractable` | `string sequenceId`, `QuizDefinition[] quizzes`, `StoryCard completionCard`, `UnityEvent onCompleted` | Hỏi lần lượt; tiến độ giữa chừng không lưu; xong: `TryAdd(Puzzles, sequenceId)`, hiện thẻ, gọi `onCompleted` |

Điểm Tri Thức = `World.Count(StateSet.QuizFirstTry)`. Không lưu bộ đếm riêng.

### Địch và trùm

```csharp
// AI/EnemyDefinition.cs : ScriptableObject (giá trị mặc định = Hắc Ảnh chương 1)
public string enemyName = "Hắc Ảnh";
public float maxHealth = 60f, damage = 15f, walkSpeed = 2f, chaseSpeed = 3.5f;
public float detectionRadius = 10f, viewAngle = 120f, attackRange = 1.7f, attackCooldown = 1.6f;
public float attackTelegraph = 0.4f, attackActive = 0.15f, leashDistance = 18f, loseSightTime = 3f, searchTime = 2f;
public float weakenFocusTime = 1f, weakenDuration = 4f, weakenMultiplier = 1.5f;
```

| Script | Hành vi |
| --- | --- |
| `EnemyHealth : IDamageable` | `float Current`, `bool IsDead`, `event Action<EnemyHealth> Died`, `event Action Changed`. Nhận sát thương từ team Player; nhân `EnemyLanternReceiver.DamageMultiplier`; về 0 thì `Died` một lần. `ResetToFull()` |
| `EnemyLanternReceiver : ILanternTarget` | Cộng thời gian Focus (mất chiếu quá 0,25 giây thì về 0). Đủ `weakenFocusTime`: `weakenedUntil = Time.time + weakenDuration`. `bool IsWeakened`; `float DamageMultiplier => IsWeakened ? weakenMultiplier : 1f`. Hiệu ứng tạm, không lưu |
| `EnemyBrain` | FSM `enum EnemyState { Idle, Patrol, Chase, Attack, Search, Return, Stunned, Dead }`; Weakened là cờ chồng lên, không phải state |
| `TelegraphMarker` | `Show(Vector3 pos, Quaternion rot, Vector3 size, float duration, Shape shape)`: hiện khối mỏng vật liệu `M_BO_Telegraph`, không collider |
| `DamageVolume` | `Fire(string attackId, float activeDuration)`: trong khoảng active, mỗi FixedUpdate Overlap theo hình (Box/Sphere/Capsule) trên mask Player; lấy `IDamageable` gốc; mỗi `attackId` trúng một mục tiêu một lần |
| `EnemyProjectile` | Kinematic; mỗi frame SphereCast từ vị trí cũ tới mới, mask Player + Environment; chạm Environment thì hủy; chạm Player thì gây sát thương một lần rồi hủy; tự hủy sau 4 giây |

**FSM Hắc Ảnh (mặc định cho mọi địch dùng `EnemyBrain`):**

| Từ | Sang | Điều kiện |
| --- | --- | --- |
| Idle | Patrol | Có ≥ 2 điểm tuần tra, sau 1 giây |
| Idle, Patrol, Return | Chase | An trong `detectionRadius`, trong `viewAngle`, Raycast Environment không chặn, An còn sống |
| Chase | Attack | Khoảng cách ≤ `attackRange` và hết `attackCooldown` |
| Attack | Chase | Hết `attackTelegraph + attackActive + 0,3 s` hồi |
| Chase | Search | Mất LOS liên tục quá `loseSightTime` |
| Chase | Return | Khoảng cách tới điểm sinh > `leashDistance` |
| Search | Return | Hết `searchTime` |
| Return | Idle | Tới điểm sinh (≤ 0,5 m) |
| Bất kỳ | Stunned | Trùm hoặc cơ chế chương gọi `Stun(duration)` |
| Bất kỳ | Dead | `EnemyHealth.Died`: dừng Agent, tắt DamageVolume, tắt collider, ẩn sau 1 giây |
| Bất kỳ | Idle (đứng yên) | `GameSession.State != Playing` hoặc An đã chết: không tấn công |

Đòn Attack: `TelegraphMarker` hình hộp 1 × 1,5 m trước mặt trong `attackTelegraph`, rồi `DamageVolume.Fire` trong `attackActive`. Trước khi bắn kiểm tra An còn sống và không có Environment chắn.

| Script | Hành vi |
| --- | --- |
| `BossBase : MonoBehaviour` (abstract) | `string bossId`, `float maxHealth`, `EnemyHealth health`, `ArenaGate arena`. `void Activate()` bắt đầu coroutine `RunAI()`. `void ResetBoss()` dừng coroutine, đặt về vị trí đầu, đầy máu. `protected abstract IEnumerator RunAI()`. Hàm hỗ trợ `BeginStunWindow(float duration, float damageCap)`: trong cửa sổ, tổng sát thương nhận không vượt `damageCap`. Died: `TryAdd(Bosses, bossId)`, `arena.Unlock()` |
| `ArenaGate` | `GameObject gateBehind` (cửa sau lưng), `BoxCollider enterTrigger` (layer Ignore Raycast, đặt 2 m trong đấu trường), `BossBase boss`, `List<EnemyHealth> waveEnemies` (dùng cho trận quái). Player vào trigger và `!Has(Bosses, bossId)`: bật `gateBehind`, `boss.Activate()`. `Unlock()`: tắt `gateBehind`. Khi Respawn: `boss.ResetBoss()` và mở cổng nếu trùm chưa chết; nếu đã chết thì trùm ẩn, cổng mở |

## 5 Lưu tải

```csharp
[Serializable] public class SaveData {
    public int schemaVersion = 4;
    public string buildVersion;
    public long timestampUtcTicks;
    public string chapterId;            // "c1".."c5"
    public string sceneName;
    public string checkpointId;
    public List<InventorySlotDto> inventory;
    public WorldSnapshot world;         // mọi StateSet và intKeys
    public float elapsedPlaySeconds;
}
[Serializable] public class WorldSnapshot { public List<StringSetDto> sets; public List<IntEntryDto> ints; }
[Serializable] public class StringSetDto { public string set; public List<string> ids; }
[Serializable] public class IntEntryDto { public string key; public int value; }
```

- Máu, thể lực, năng lượng đèn đặt đầy khi tải tại mốc (phần 18), không lưu.
- Chuyển bản lưu v3 của chương 1 sang v4: `relic_drum_*` thành `fragment_a/b/c` trong Fragments; giữ Puzzles; `hasLantern` thành Abilities/Flags theo chương 1.
- **Quy trình Restore** (`GameSession.Respawn` hoặc Continue): tắt input và AI → tải scene `sceneName` nếu khác → `WorldStateStore.Restore(world)` (phát `Changed` một lần) → mọi object có trạng thái tự áp lại trong handler `Changed` của chính nó → mọi `ArenaGate` gọi reset → `PlayerMotor.Teleport` tới `spawnPoint` của checkpoint → `PlayerVitals.RestoreFull`, đèn đầy → cập nhật HUD → bật AI và input.
- Object có trạng thái **không tự đọc JSON**. Chỉ `SaveService` đọc và ghi file; object chỉ đọc `WorldStateStore`.

## 6 Thứ tự làm một chương (giai đoạn A Blockout)

1. Tạo scene từ `Template_Chapter.unity` (có 6 gốc, `ChapterContext`, An, camera, HUD, EventSystem). Thêm vào Build Settings.
2. Dựng phòng theo bảng mục 1 của file chương; thêm tường ranh giới và KillZone.
3. Thêm NavMeshSurface trên `World_Geometry` (collect layer Environment, agent Humanoid bán kính 0,5) và bake.
4. Viết script cơ chế mới của chương, thử trong phòng P1.
5. Đặt mốc lưu, bia, bệ, câu đố, quái, trùm theo file chương.
6. Tạo SO dữ liệu (QZ, LR, SQ, QS, SC, ED) và khai ID vào `WorldIdCatalog`.
7. Chạy `Tools/Son Doong/Validate Persistent IDs` (không ID trùng hoặc rỗng).
8. Chạy các bước kiểm thử của chương.

Kết thúc giai đoạn A khi đi được từ spawn đến `ChapterExit` không lỗi Console.
