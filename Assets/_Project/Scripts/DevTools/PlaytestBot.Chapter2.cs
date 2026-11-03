#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

public partial class PlaytestBot
{
    // Chơi hết chương 1 rồi bấm "Sang chương tiếp theo" và chơi chương 2.
    private IEnumerator Chapter1Then2()
    {
        yield return Chapter1();
        if (Failed) yield break;
        yield return WaitSeconds(0.5f);
        ClickButton("BTN_Continue");
        yield return WaitUntil(() => SceneManager.GetActiveScene().name == GameSession.SecondChapterScene &&
                                     Session.State == GameSession.SessionState.Playing, 15f, "vào chương 2");
        yield return WaitSeconds(0.5f);
        yield return Chapter2();
    }

    // Bản lưu "đã xong chương 1" → Tiếp tục → cổng chương ở Đàn Kim Quy → chương 2.
    private IEnumerator ContinueThenChapter2()
    {
        SaveData data = new SaveData { checkpointId = SaveService.KimQuyCheckpointId, sceneName = GameSession.FirstChapterScene };
        void Add(StateSet set, params string[] ids) => data.world.sets.Add(new StringSetDto { set = set.ToString(), ids = new System.Collections.Generic.List<string>(ids) });
        Add(StateSet.Flags, WorldIds.FlagLantern);
        Add(StateSet.Seals, WorldIds.SealGarden);
        Add(StateSet.Fragments, WorldStateStore.DrumA, WorldStateStore.DrumB, WorldStateStore.DrumC);
        Add(StateSet.Puzzles, WorldStateStore.DrumPuzzleId, WorldStateStore.MirrorPuzzleId, WorldIds.RelicRestoredC1);
        Add(StateSet.Doors, WorldIds.DoorKimQuy);
        Add(StateSet.Bosses, WorldIds.GuardianBoss);
        Add(StateSet.Abilities, WorldIds.MovementKit);
        data.inventory.Add(new InventorySlotDto { slot = 0, itemId = InventoryService.ThuocMau, quantity = 1 });
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.persistentDataPath, "save_v1.json"), JsonUtility.ToJson(data, true));

        yield return WaitUntil(() => AppRoot.Instance != null && Session.State == GameSession.SessionState.MainMenu, 15f, "Main Menu");
        ClickButton("BTN_Continue");
        yield return WaitPlaying();
        Check(SceneManager.GetActiveScene().name == GameSession.FirstChapterScene, "Tiếp tục mở chương 1 tại bệ Kim Quy");
        yield return WalkTo(new Vector3(-2.6f, 0f, 104.5f));
        yield return Interact(new Vector3(-4f, 1.5f, 105.5f), "chương 2");
        yield return WaitUntil(() => SceneManager.GetActiveScene().name == GameSession.SecondChapterScene &&
                                     Session.State == GameSession.SessionState.Playing, 15f, "cổng chương mở chương 2");
        yield return WaitSeconds(0.5f);
        yield return Chapter2();
    }

    private IEnumerator Chapter2()
    {
        WorldStateStore world = Find<WorldStateStore>();
        InventoryService inv = Find<InventoryService>();
        Check(SceneManager.GetActiveScene().name == GameSession.SecondChapterScene, "Đang ở 03_GiengTroiChimLac");
        Check(SaveService.TryReadSaved(out SaveData save) && save.checkpointId == "c2_cp_start", "Chuyển chương lưu ở c2_cp_start");
        Check(world.Has(StateSet.Abilities, WorldIds.MovementKit) && Player.GetComponent<LanternController>().HasLantern &&
              world.Has(StateSet.Puzzles, WorldIds.RelicRestoredC1), "Mang theo Quà Kim Quy, Lồng Đèn và tiến trình chương 1");
        Log($"Túi mang sang: thuốc {inv.Count(InventoryService.ThuocMau)}, lá cẩm {inv.Count(InventoryService.LaCam)}, linh chi {inv.Count(InventoryService.LinhChi)}");

        // --- Lộn né (MV03 cơ bản) ---
        DodgeController dodge = Player.GetComponent<DodgeController>();
        PlayerStamina stamina = Player.GetComponent<PlayerStamina>();
        float st = stamina.CurrentStamina;
        Vector3 before = Player.transform.position;
        Cam.SetAngles(0f, 10f);
        Hold(Key.D);
        yield return null;
        yield return Tap(Key.LeftCtrl);
        bool rolled = dodge.IsDodging;
        Release(Key.D);
        yield return WaitSeconds(0.6f);
        float moved = Vector3.Distance(before, Player.transform.position);
        Check(rolled && st - stamina.CurrentStamina >= 24f && moved > 2.5f, $"Ctrl + D lộn sang phải (đi {moved:0.0} m, tốn 25 thể lực)");
        // Lộn vào tường phía nam: không xuyên tường.
        yield return WalkTo(new Vector3(0f, 0f, -6.5f));
        Cam.SetAngles(180f, 10f);
        yield return WaitSeconds(0.7f);
        Hold(Key.W);
        yield return null;
        yield return Tap(Key.LeftCtrl);
        Release(Key.W);
        yield return WaitSeconds(0.6f);
        Check(Player.transform.position.z > -8f + 0.2f, $"Lộn vào tường không xuyên tường (z = {Player.transform.position.z:0.00})");

        // Né đúng lúc: lộn thật bằng Ctrl, một đòn phe Enemy (gọi trực tiếp) rơi vào 0,10–0,25 s đầu bất tử.
        LanternController lantern = Player.GetComponent<LanternController>();
        yield return WaitSeconds(0.8f);
        float energyBefore = lantern.Energy;
        float hpBefore = Vitals.CurrentHealth;
        int perfectBefore = dodge.PerfectDodgeCount;
        Cam.SetAngles(0f, 10f);
        yield return Tap(Key.LeftCtrl);
        yield return WaitUntil(() => Vitals.IsInvulnerable, 1f, "khoảng bất tử khi lộn");
        Vitals.TryTakeDamage(new DamageInfo(null, 15f, DamageTeam.Enemy) { AttackId = "test_perfect" });
        bool slowed = Time.timeScale < 0.5f;
        yield return WaitSeconds(0.6f);
        Check(dodge.PerfectDodgeCount == perfectBefore + 1 && slowed && Vitals.CurrentHealth == hpBefore && Time.timeScale == 1f &&
              lantern.Energy >= Mathf.Min(lantern.MaxEnergy, energyBefore + 14f) - 0.01f,
            $"Né đúng lúc: không mất máu, chậm thời gian rồi trả về 1, đèn +15 ({energyBefore:0} → {lantern.Energy:0})");
        // Lộn + chém: chém ngay sau lộn là đòn đâm tới.
        CombatController combat = Player.GetComponent<CombatController>();
        int lunges = combat.LungeCount;
        yield return WaitSeconds(0.7f);
        yield return Tap(Key.LeftCtrl);
        yield return WaitUntil(() => !dodge.IsDodging, 1f, "kết thúc lộn");
        yield return Click(MouseButton.Left);
        yield return WaitSeconds(0.7f);
        Check(combat.LungeCount == lunges + 1, "Lộn + chém: chém ngay sau lộn thành đòn đâm tới (×1,25)");

        // --- Cột gió trước khi có Lông Chim Lạc ---
        yield return WalkTo(new Vector3(5f, 0f, 3.5f), 0.4f);
        yield return WaitSeconds(1.2f);
        Check(Player.transform.position.y < 0.5f, "Chưa có Lông Chim Lạc: cột gió không nâng");

        // --- Lông Chim Lạc, bia, nguyên liệu ---
        yield return WalkTo(new Vector3(0f, 0f, -3.4f));
        yield return Interact(new Vector3(0f, 1f, -2f), "Lông Chim Lạc");
        yield return WaitUntil(() => Find<StoryCardPanel>().IsOpen, 2f, "thẻ hướng dẫn Lông Chim Lạc");
        yield return Tap(Key.Escape);
        yield return WaitUntil(() => Session.State == GameSession.SessionState.Playing, 2f, "đóng thẻ bằng Esc");
        Check(world.Has(StateSet.Abilities, WorldIds.AbilityWind), "Nhận Lông Chim Lạc (ability_wind)");

        yield return WalkTo(new Vector3(-5.2f, 0f, 2f));
        yield return WaitUntil(() => world.Has(StateSet.Lore, "c2_lore_p1"), 3f, "đọc bia P1 khi rọi đèn");
        Check(world.Has(StateSet.Lore, "c2_lore_p1"), "Rọi đèn gần bia P1 ghi Tri thức");
        yield return WalkTo(new Vector3(-4.6f, 0f, 4f));
        yield return Interact(new Vector3(-6f, 0.35f, 4f), "Lá cẩm");
        yield return WalkTo(new Vector3(-3f, 0f, 4.6f));
        yield return Interact(new Vector3(-3f, 0.35f, 6f), "Linh chi");
        // Chế thuốc ở lửa trại chương 2.
        int potions = inv.Count(InventoryService.ThuocMau);
        yield return WalkTo(new Vector3(-3.6f, 0f, -4.4f));
        yield return Interact(new Vector3(-5f, 0.6f, -5f), "chế thuốc");
        yield return WaitUntil(() => Find<CraftingPanel>().IsOpen, 2f, "bảng chế thuốc chương 2");
        ClickButton("BTN_Craft");
        yield return WaitSeconds(0.2f);
        yield return Tap(Key.Escape);
        yield return WaitSeconds(0.3f);
        Check(inv.Count(InventoryService.ThuocMau) == potions + 1 && Session.State == GameSession.SessionState.Playing, "Chế 1 thuốc ở lửa trại đáy giếng");

        // --- Cột gió P1 lên gờ, Bệ Vấn Đáp trả lời sai rồi đúng ---
        yield return RideColumn(new Vector3(5f, 0f, 3.5f), 5.5f, new Vector3(5f, 5f, 6.2f));
        yield return Interact(new Vector3(6f, 0.9f + 5f, 7f), "Bệ Vấn Đáp");
        QuizPanel quiz = Find<QuizPanel>();
        yield return WaitUntil(() => quiz.IsOpen, 2f, "bảng câu hỏi");
        quiz.Choose((quiz.Current.correctIndex + 1) % 4);
        yield return WaitSeconds(0.3f);
        Check(quiz.IsOpen && !world.Has(StateSet.Fragments, "c2_frag_1"), "QZ01: trả lời sai hiện giải thích, chưa trao đồ");
        ClickButton("BTN_Retry");
        quiz.Choose(quiz.Current.correctIndex);
        yield return WaitSeconds(0.4f);
        Check(world.Has(StateSet.Fragments, "c2_frag_1") && world.Has(StateSet.QuizAnswered, "c2_qz_p1") &&
              !world.Has(StateSet.QuizFirstTry, "c2_qz_p1"), "QZ01: trả lời lại đúng nhận mảnh 1, không có điểm lần đầu");
        yield return Aim(new Vector3(6f, 5.9f, 7f));
        Check(!Find<QuizPedestal>().CanInteract(Player) || !Prompt.Contains("Bệ"), "Bệ P1 không trao lần hai");

        // --- P2 Vách Gió ---
        yield return WalkPath(new Vector3(5f, 5f, 8.6f), new Vector3(5f, 5f, 11f));
        yield return RideColumn(new Vector3(7.6f, 5f, 12.6f), 9.5f, new Vector3(4.5f, 9f, 13.6f));
        yield return WalkTo(new Vector3(3.2f, 9f, 14.5f));
        yield return WaitUntil(() => world.Has(StateSet.Lore, "c2_lore_p2"), 3f, "đọc bia P2");
        yield return RideColumn(new Vector3(5.4f, 9f, 16.2f), 13.5f, new Vector3(8.6f, 13f, 16.6f));
        yield return Interact(new Vector3(9.5f, 13.9f, 17.8f), "Bệ Vấn Đáp");
        yield return WaitUntil(() => quiz.IsOpen, 2f, "bảng câu hỏi P2");
        quiz.Choose(quiz.Current.correctIndex);
        yield return WaitSeconds(0.4f);
        Check(world.Has(StateSet.Fragments, "c2_frag_2") && world.Has(StateSet.QuizFirstTry, "c2_qz_p2"), "Đúng ngay lần đầu: mảnh 2 + Điểm Tri Thức");

        // --- P3 Nhà Sàn: lưu giữa chương, chuông gió ---
        yield return WalkPath(new Vector3(9.2f, 13f, 20f), new Vector3(9.2f, 13f, 24.6f), new Vector3(8f, 13f, 26f));
        yield return Interact(new Vector3(6.2f, 13.5f, 25.4f), "Lưu");
        Check(SaveService.TryReadSaved(out save) && save.checkpointId == "c2_cp_mid", "Lưu ở CP_C2_Mid");
        yield return BellsAndFragment3(world, quiz, true);
        if (Failed) yield break;

        // Ca 5: F9 sau khi lấy mảnh 3 → về mốc giữa, mảnh 3 và chuông về chưa giải.
        WorldStateStore oldWorld = world;
        yield return Tap(Key.F9);
        yield return WaitUntil(() => oldWorld == null, 5f, "F9 nạp lại scene");
        yield return WaitPlaying();
        world = Find<WorldStateStore>();
        quiz = Find<QuizPanel>();
        Check(!world.Has(StateSet.Fragments, "c2_frag_3") && !world.Has(StateSet.Puzzles, "c2_pz_chuong_gio") &&
              world.Has(StateSet.Fragments, "c2_frag_2") && Vector3.Distance(Player.transform.position, new Vector3(9.2f, 13.2f, 26f)) < 1f,
            "F9: mảnh 3 mất, chuông chưa giải, An đứng ở CP_C2_Mid");
        yield return BellsAndFragment3(world, quiz, false);
        if (Failed) yield break;

        // --- P4 Cầu Mây Gãy ---
        yield return WalkPath(new Vector3(5f, 18f, 38.6f), new Vector3(5f, 18f, 40.5f));
        foreach (EnemyBrain e in FindObjectsByType<EnemyBrain>())
            if (e.transform.position.z < 49f) yield return FightBrain(e);
        yield return JumpGap(new Vector3(5f, 18f, 46.6f), new Vector3(5f, 18f, 51f));
        foreach (EnemyBrain e in FindObjectsByType<EnemyBrain>())
            if (e.transform.position.z > 49f && e.transform.position.z < 57f) yield return FightBrain(e);
        // Lên gờ trái hạ Hắc Ảnh Ném rồi xuống.
        yield return RideColumn(new Vector3(2.4f, 18f, 51.5f), 21.5f, new Vector3(0.2f, 21f, 51.5f));
        yield return FightThrower("HacAnhNem_L");
        yield return RideColumn(new Vector3(2.4f, 21f, 51.5f), 21.5f, new Vector3(4.5f, 18f, 51.5f), true);
        yield return RideColumn(new Vector3(7.6f, 18f, 51.5f), 21.5f, new Vector3(9.8f, 21f, 51.5f));
        yield return FightThrower("HacAnhNem_R");
        yield return RideColumn(new Vector3(7.6f, 21f, 51.5f), 21.5f, new Vector3(5.5f, 18f, 51.5f), true);
        yield return JumpGap(new Vector3(5f, 18f, 54.6f), new Vector3(5f, 18f, 59f));
        yield return UsePotionIfLow(60f);
        yield return WalkTo(new Vector3(5f, 18f, 66f));
        // Mốc tự động phải an toàn: không có quái nào trong tầm 10 m.
        bool safe = true;
        foreach (EnemyHealth e in EnemyHealth.Active)
            if (!e.IsDead && Vector3.Distance(e.transform.position, Player.transform.position) < 10f) safe = false;
        Check(safe, "Mốc c2_cp_arena không có quái trong 10 m");
        yield return WalkTo(new Vector3(5f, 18f, 66f));
        Check(SaveService.TryReadSaved(out save) && save.checkpointId == "c2_cp_arena", "Mốc tự động c2_cp_arena khi tới sân cuối cầu");
        yield return WalkTo(new Vector3(3.4f, 18f, 67f));
        yield return WaitUntil(() => world.Has(StateSet.Lore, "c2_lore_p4"), 3f, "đọc bia P4");
        // Cổng đấu trường chưa đủ đồ.
        yield return WalkTo(new Vector3(5f, 18f, 70.5f));
        yield return Aim(new Vector3(5f, 19.5f, 72f));
        Check(Prompt.Contains("thiếu") && Prompt.Contains("Lông Gió Lớn"), "Cổng đấu trường báo thiếu Lông Gió Lớn");
        yield return WalkTo(new Vector3(6.6f, 18f, 67f));
        yield return Interact(new Vector3(7.8f, 18.9f, 67f), "Bệ Vấn Đáp");
        yield return WaitUntil(() => quiz.IsOpen, 2f, "bảng câu hỏi P4");
        quiz.Choose(quiz.Current.correctIndex);
        yield return WaitSeconds(0.4f);
        Check(world.Has(StateSet.BossTools, "c2_tool_long_gio_lon"), "Nhận Lông Gió Lớn và thuốc");
        yield return WalkTo(new Vector3(5f, 18f, 70.5f));
        yield return Interact(new Vector3(5f, 19.5f, 72f), "Mở cổng");
        Check(world.Has(StateSet.Doors, "c2_door_arena"), "Mở cổng Đỉnh Tổ Chim");

        // --- P5: chết một lần (rơi khe đông) rồi thắng ---
        yield return WalkTo(new Vector3(5f, 18f, 76.5f));
        Check(Find<ArenaGate>().IsLocked, "Đấu trường khóa cổng");
        yield return WalkTo(new Vector3(13.5f, 18f, 82f), 0.5f, 10f, true);
        Hold(Key.W);
        yield return WaitUntil(() => Session.State == GameSession.SessionState.Dead, 8f, "rơi khe đấu trường → chết");
        Release(Key.W);
        Check(Session.State == GameSession.SessionState.Dead, "Rơi khỏi đỉnh tổ thì chết");
        ClickButton("BTN_Respawn");
        yield return WaitPlaying();
        world = Find<WorldStateStore>();
        Check(Vector3.Distance(Player.transform.position, new Vector3(5f, 18.2f, 66.5f)) < 1f && !Find<ArenaGate>().IsLocked &&
              world.Has(StateSet.Doors, "c2_door_arena"), "Hồi sinh ở c2_cp_arena, cổng mở, đấu trường reset");
        yield return WalkTo(new Vector3(5f, 18f, 76.5f));
        yield return FightChimLac();
        if (Failed) yield break;

        // Ca 6: trùm không sống lại sau F9.
        yield return WaitSeconds(1.5f);
        WorldStateStore worldBeforeF9 = world;
        yield return Tap(Key.F9);
        yield return WaitUntil(() => worldBeforeF9 == null, 5f, "F9 sau khi thắng");
        yield return WaitPlaying();
        world = Find<WorldStateStore>();
        Check(world.Has(StateSet.Bosses, "c2_boss_chim_lac") && FindAnyObjectByType<BossChimLac>() == null, "F9 sau khi thắng: Chim Lạc không sống lại");

        // --- Phục dựng, Linh Thần Vấn Đáp, cổng chương ---
        quiz = Find<QuizPanel>();
        yield return WalkPath(new Vector3(5f, 18f, 76.5f), new Vector3(5f, 18f, 86.6f));
        yield return Interact(new Vector3(5f, 18.6f, 89f), "Phục dựng");
        Check(world.Has(StateSet.Puzzles, "c2_relic_restored"), "Phục dựng Mặt Trời Lông Chim");
        yield return WalkTo(new Vector3(7.6f, 18f, 85.4f));
        yield return Interact(new Vector3(9f, 19.2f, 86.5f), "Chim Lạc");
        for (int i = 0; i < 3; i++)
        {
            yield return WaitUntil(() => quiz.IsOpen, 2f, $"câu Linh Thần {i + 1}");
            quiz.Choose(quiz.Current.correctIndex);
            yield return WaitSeconds(0.3f);
        }
        yield return WaitUntil(() => Find<StoryCardPanel>().IsOpen, 2f, "thẻ Chim Lạc thức tỉnh");
        ClickButton("BTN_Continue");
        yield return WaitSeconds(0.3f);
        Check(world.Has(StateSet.Puzzles, "c2_linhthan_done") && world.Count(StateSet.QuizFirstTry) >= 6, $"Linh Thần Vấn Đáp xong, Điểm Tri Thức {world.Count(StateSet.QuizFirstTry)}");

        yield return Tap(Key.J);
        yield return WaitSeconds(0.3f);
        JournalPanel journal = Find<JournalPanel>();
        Check(journal.IsOpen && Session.State == GameSession.SessionState.InMenu, "J mở Nhật ký");
        yield return Tap(Key.J);
        yield return WaitSeconds(0.3f);
        Check(!journal.IsOpen && Session.State == GameSession.SessionState.Playing, "J đóng Nhật ký");

        yield return WalkTo(new Vector3(0.2f, 18f, 82f));
        yield return Interact(new Vector3(-1.4f, 19.5f, 82f), "Thành Đá");
        yield return WaitUntil(() => Session.State == GameSession.SessionState.ChapterComplete, 3f, "kết chương 2");
        Check(Session.State == GameSession.SessionState.ChapterComplete, "Cổng chương: chương 3 chưa có nên hiện màn kết chương 2");
        ClickButton("BTN_Continue");
        yield return WaitUntil(() => Session.State == GameSession.SessionState.MainMenu, 10f, "về menu");
        ClickButton("BTN_Continue");
        yield return WaitPlaying();
        Check(SceneManager.GetActiveScene().name == GameSession.SecondChapterScene && Find<WorldStateStore>().Has(StateSet.Puzzles, "c2_linhthan_done"),
            "CH01: Tiếp tục mở đúng chương 2 và mốc");
        Log($"Thời gian chơi (bot) tới đây: {Time.realtimeSinceStartup - startTime:0} giây");
    }

    private IEnumerator BellsAndFragment3(WorldStateStore world, QuizPanel quiz, bool tryWrong)
    {
        // Phù điêu hiện thứ tự khi rọi đèn.
        yield return WalkTo(new Vector3(5f, 13f, 30.2f));
        LanternRevealSurface relief = GameObject.Find("Relief_C2_Order").GetComponent<LanternRevealSurface>();
        yield return WaitUntil(() => relief.IsRevealed, 3f, "phù điêu hiện khi rọi đèn");
        Check(relief.IsRevealed, "Rọi đèn thấy phù điêu thứ tự chim");
        Vector3[] stand = { new Vector3(0.5f, 13f, 27.2f), new Vector3(9.5f, 13f, 27.6f), new Vector3(2.8f, 13f, 34.5f) };
        Vector3[] bell = { new Vector3(0.5f, 15.85f, 29f), new Vector3(9.5f, 15.85f, 29.5f), new Vector3(0.5f, 15.85f, 34.5f) };
        if (tryWrong)
        {
            yield return WalkTo(stand[0], 0.4f);
            yield return Interact(bell[0], "chim số 1");
            yield return WaitSeconds(1.1f);
            Check(!world.Has(StateSet.Puzzles, "c2_pz_chuong_gio") && !GameObject.Find("WindColumn_P3_Final").GetComponent<WindColumn>().IsActive,
                "Gõ chuông sai: không giải, cột gió cuối chưa bật");
        }
        foreach (int i in new[] { 1, 2, 0 })
        {
            yield return WalkTo(stand[i], 0.4f);
            yield return Interact(bell[i], $"chim số {i + 1}");
            yield return WaitSeconds(0.35f);
        }
        yield return WaitUntil(() => world.Has(StateSet.Puzzles, "c2_pz_chuong_gio"), 3f, "giải chuông gió 2-3-1");
        Check(GameObject.Find("WindColumn_P3_Final").GetComponent<WindColumn>().IsActive, "Chuông đúng: cột gió cuối bật");
        yield return RideColumn(new Vector3(5f, 13f, 32.8f), 18.5f, new Vector3(5.6f, 18f, 35.6f));
        yield return Interact(new Vector3(6.6f, 18.9f, 36.6f), "Bệ Vấn Đáp");
        yield return WaitUntil(() => quiz.IsOpen, 2f, "bảng câu hỏi P3");
        quiz.Choose(quiz.Current.correctIndex);
        yield return WaitSeconds(0.4f);
        Check(world.Has(StateSet.Fragments, "c2_frag_3"), "Nhận mảnh 3");
    }

    // Vào cột gió, chờ nâng tới gần đỉnh rồi bước ra phía đích.
    private IEnumerator RideColumn(Vector3 column, float minY, Vector3 exit, bool alreadyAbove = false)
    {
        yield return WalkTo(column, 0.35f);
        if (Failed) yield break;
        yield return WaitUntil(() => Player.transform.position.y >= minY, 6f, $"cột gió nâng tới y ≥ {minY}");
        yield return WalkTo(exit, 0.45f);
        yield return WaitUntil(() => Player.GetComponent<PlayerMotor>().IsGrounded, 4f, "chạm đất sau cột gió");
        yield return WaitSeconds(0.2f);
    }

    private IEnumerator JumpGap(Vector3 runUp, Vector3 landing)
    {
        yield return WalkTo(runUp, 0.4f);
        FaceYaw(Player.transform.position, landing);
        Hold(Key.LeftShift);
        Hold(Key.W);
        yield return WaitSeconds(0.35f);
        yield return Tap(Key.Space);
        yield return WaitSeconds(0.5f);
        Release(Key.W);
        Release(Key.LeftShift);
        yield return WaitUntil(() => Player.GetComponent<PlayerMotor>().IsGrounded, 3f, "tiếp đất sau khi nhảy khe");
        yield return WalkTo(landing, 0.6f);
        Check(Player.transform.position.y > 17.5f, $"Nhảy qua khe cầu (z = {Player.transform.position.z:0.0})");
    }

    private IEnumerator FightThrower(string name)
    {
        GameObject t = GameObject.Find(name);
        if (t == null) { Log($"{name} đã bị hạ"); yield break; }
        EnemyHealth h = t.GetComponent<EnemyHealth>();
        float hp = Vitals.CurrentHealth;
        float end = Time.realtimeSinceStartup + 25f;
        while (h != null && !h.IsDead && Time.realtimeSinceStartup < end && !Vitals.IsDead)
        {
            Vector3 p = Player.transform.position, e = t.transform.position;
            if (Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(e.x, 0, e.z)) > 1.3f) { FaceYaw(p, e); Hold(Key.W); yield return null; continue; }
            Release(Key.W);
            FaceYaw(p, e);
            yield return Click(MouseButton.Left);
            yield return WaitSeconds(0.7f);
            yield return Reflexes();
        }
        Release(Key.W);
        Check(h == null || h.IsDead, $"Hạ {name} (máu An {hp:0} → {Vitals.CurrentHealth:0}; đã lộn né đạn {dodgedProjectiles} lần)");
        yield return WaitSeconds(1.1f);
    }

    // Trận Chim Lạc: chờ chim đậu → lên cột gió giữa → rọi đèn cho choáng → xuống chém; né vệt lao bằng lộn.
    private IEnumerator FightChimLac()
    {
        BossChimLac chim = Find<BossChimLac>();
        EnemyHealth h = chim.GetComponent<EnemyHealth>();
        DodgeController dodge = Player.GetComponent<DodgeController>();
        PlayerMotor motor = Player.GetComponent<PlayerMotor>();
        Vector3 center = new Vector3(5f, 18f, 82f);
        float start = Time.realtimeSinceStartup;
        int stuns = 0, rolls = 0;
        int perfectBefore = dodge.PerfectDodgeCount;
        float hpStart = Vitals.CurrentHealth;
        BossChimLac.ChimState last = chim.State;

        while (!h.IsDead)
        {
            if (Time.realtimeSinceStartup - start > 300f) { Fail($"Trận Chim Lạc quá 5 phút (máu trùm {h.Current:0})"); yield break; }
            if (Vitals.IsDead) { Fail($"An chết trong trận Chim Lạc (máu trùm {h.Current:0})"); yield break; }
            yield return UsePotionIfLow(45f);
            if (chim.State == BossChimLac.ChimState.Stunned && last != BossChimLac.ChimState.Stunned) stuns++;
            last = chim.State;

            Vector3 p = Player.transform.position;
            Vector3 c = chim.transform.position;
            switch (chim.State)
            {
                case BossChimLac.ChimState.DiveTelegraph:
                {
                    // Lộn ngang vuông góc với đường lao khi sắp tới lúc lao.
                    Vector3 line = p - c; line.y = 0f;
                    FaceYaw(p, p + line);
                    SetMouse(MouseButton.Right, false);
                    yield return WaitSeconds(0.55f);
                    Hold(Key.D);
                    yield return null;
                    yield return Tap(Key.LeftCtrl);
                    rolls++;
                    Release(Key.D);
                    yield return WaitSeconds(0.5f);
                    break;
                }
                case BossChimLac.ChimState.Stunned:
                case BossChimLac.ChimState.Landed:
                {
                    SetMouse(MouseButton.Right, false);
                    float d = Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(c.x, 0, c.z));
                    if (motor.IsHovering || d > 1.8f) { FaceYaw(p, c); Hold(Key.W); yield return null; Release(Key.W); }
                    else { FaceYaw(p, c); yield return Click(MouseButton.Left); yield return WaitSeconds(0.55f); }
                    break;
                }
                case BossChimLac.ChimState.Perched:
                {
                    float d = Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(center.x, 0, center.z));
                    if (d > 0.5f && !motor.IsHovering) { FaceYaw(p, center); Hold(Key.W); yield return null; Release(Key.W); break; }
                    if (p.y < 23.5f) { yield return null; break; }
                    yield return Aim(chim.transform.position + Vector3.up * 0.7f);
                    SetMouse(MouseButton.Right, true);
                    yield return null;
                    break;
                }
                default:
                {
                    SetMouse(MouseButton.Right, false);
                    float d = Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(center.x, 0, center.z));
                    if (d > 0.5f) { FaceYaw(p, center); Hold(Key.W); yield return null; Release(Key.W); }
                    else yield return null;
                    break;
                }
            }
        }
        ReleaseAll();
        Log($"Hạ Chim Lạc sau {Time.realtimeSinceStartup - start:0.0} giây; máu An {hpStart:0} → {Vitals.CurrentHealth:0}; choáng {stuns} lần; lộn {rolls} lần; né đúng lúc {dodge.PerfectDodgeCount - perfectBefore}");
        Check(Find<WorldStateStore>().Has(StateSet.Bosses, "c2_boss_chim_lac") && !Find<ArenaGate>().IsLocked, "Chim Lạc được giải thoát, cổng mở");
    }
}
#endif
