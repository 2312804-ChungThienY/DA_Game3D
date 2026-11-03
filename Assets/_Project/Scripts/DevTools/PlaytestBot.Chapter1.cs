#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public partial class PlaytestBot
{
    private static void ClickButton(string name)
    {
        foreach (Button b in FindObjectsByType<Button>(FindObjectsInactive.Exclude))
            if (b.name == name && b.interactable) { b.onClick.Invoke(); return; }
        throw new System.Exception($"Không thấy nút {name}");
    }

    private IEnumerator StartNewGameFromMenu()
    {
        yield return WaitUntil(() => AppRoot.Instance != null && Session.State == GameSession.SessionState.MainMenu, 15f, "Main Menu");
        if (Failed) yield break;
        ClickButton("BTN_NewGame");
        yield return WaitSeconds(0.3f);
        // Có bản lưu cũ thì hỏi xác nhận.
        if (Session.State == GameSession.SessionState.MainMenu) ClickButton("BTN_Yes");
        yield return WaitPlaying();
        Check(SceneManager.GetActiveScene().name == GameSession.FirstChapterScene, "Chơi mới mở 02_HangLinhMach");
    }

    private IEnumerator Chapter1()
    {
        yield return StartNewGameFromMenu();
        WorldStateStore world = Find<WorldStateStore>();
        InventoryService inv = Find<InventoryService>();
        LanternController lantern = Player.GetComponent<LanternController>();

        // --- Trại ---
        Hold(Key.LeftCtrl);
        yield return WaitSeconds(0.3f);
        Release(Key.LeftCtrl);
        DodgeController dodge = Player.GetComponent<DodgeController>();
        Check(dodge == null || !dodge.IsDodging, "MV01: Ctrl ở chương 1 không lộn");

        yield return WalkTo(new Vector3(0f, 0f, 0.4f));
        yield return Interact(new Vector3(0f, 1.2f, 2f), "Lồng Đèn");
        Check(lantern.HasLantern, "Nhận Lồng Đèn Sen");

        yield return WalkTo(new Vector3(-3f, 0f, 2.1f));
        yield return Interact(new Vector3(-3f, 0.35f, 3.5f), "Linh chi");
        Check(inv.Count(InventoryService.LinhChi) == 1, "Nhặt Linh chi");

        yield return WalkTo(new Vector3(-2.6f, 0f, -0.6f));
        yield return Interact(new Vector3(-4f, 0.7f, 0f), "lửa trại");
        yield return WaitUntil(() => Find<CraftingPanel>().IsOpen, 2f, "bảng chế thuốc");
        Check(Session.State == GameSession.SessionState.InMenu, "Lửa trại lưu và mở bảng chế (InMenu)");
        yield return Tap(Key.Escape);
        yield return WaitSeconds(0.3f);
        Check(Session.State == GameSession.SessionState.Playing, "Esc đóng bảng chế");

        // --- Phong ấn ---
        yield return WalkPath(new Vector3(0f, 0f, 6f), new Vector3(0f, 0f, 13.5f));
        SealTarget seal = Find<SealTarget>();
        yield return Aim(new Vector3(0f, 1.5f, 17.8f));
        SetMouse(MouseButton.Right, true);
        yield return WaitUntil(() => seal.IsSolved, 4f, "xóa phong ấn bằng Focus");
        SetMouse(MouseButton.Right, false);
        Check(seal.IsSolved, "Focus xóa phong ấn");

        // --- Vườn Rêu ---
        yield return WalkPath(new Vector3(0f, 0f, 21f), new Vector3(0f, 0f, 26.3f));
        yield return Interact(new Vector3(0f, 1.3f, 28f), "Mảnh Trống A");
        Check(world.HasRelic(WorldStateStore.DrumA), "Nhận mảnh A");
        yield return WalkTo(new Vector3(-4.4f, 0f, 26f));
        yield return Interact(new Vector3(-6f, 0.35f, 26f), "Lá cẩm");
        yield return WalkTo(new Vector3(4.4f, 0f, 27.5f));
        yield return Interact(new Vector3(6f, 0.35f, 27.5f), "Linh chi");
        Check(inv.Count(InventoryService.LaCam) == 1 && inv.Count(InventoryService.LinhChi) == 2, "Nhặt lá cẩm và linh chi ở Vườn Rêu");

        // --- Điện Trống ---
        yield return WalkPath(new Vector3(0f, 0f, 38f), new Vector3(0f, 0f, 44f), new Vector3(-3.4f, 0f, 48f));
        yield return Interact(new Vector3(-5f, 0.5f, 48f), "Lưu");
        yield return WalkTo(new Vector3(4.4f, 0f, 50f));
        yield return Interact(new Vector3(6f, 0.35f, 50f), "Lá cẩm");
        // Tầm tương tác 3 m: phải bước tới từng trống. Thử sai trước: A rồi B.
        Vector3 standA = new Vector3(-3f, 0f, 51.3f), standB = new Vector3(0f, 0f, 51.3f), standC = new Vector3(3f, 0f, 51.3f);
        yield return WalkTo(standA, 0.4f);
        yield return Interact(new Vector3(-3f, 0.6f, 53f), "trống A");
        yield return WalkTo(standB, 0.4f);
        yield return Interact(new Vector3(0f, 0.6f, 53f), "trống B");
        yield return WaitSeconds(1.2f);
        Check(!world.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId), "Gõ sai A-B không giải");
        yield return WalkTo(standA, 0.4f);
        yield return Interact(new Vector3(-3f, 0.6f, 53f), "trống A");
        yield return WalkTo(standC, 0.4f);
        yield return Interact(new Vector3(3f, 0.6f, 53f), "trống C");
        yield return WalkTo(standB, 0.4f);
        yield return Interact(new Vector3(0f, 0.6f, 53f), "trống B");
        yield return WaitUntil(() => world.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId), 3f, "giải trống A-C-B");
        Check(world.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId), "Gõ A-C-B giải câu đố trống");
        yield return WalkPath(new Vector3(1.6f, 0f, 51.3f), new Vector3(1.4f, 0f, 55f), new Vector3(0f, 0f, 58.4f));
        yield return Interact(new Vector3(0f, 1.2f, 60f), "Mảnh Trống B");
        Check(world.HasRelic(WorldStateStore.DrumB), "Nhận mảnh B");

        // --- Hành Lang Quang: Hắc Ảnh ---
        yield return WalkTo(new Vector3(0f, 0f, 63.5f));
        foreach (EnemyBrain enemy in FindObjectsByType<EnemyBrain>())
            yield return FightBrain(enemy);
        yield return UsePotionIfLow(60f);
        yield return WalkTo(new Vector3(-6.6f, 0f, 64f));
        yield return Interact(new Vector3(-8f, 0.35f, 64f), "Lá cẩm");
        yield return WalkTo(new Vector3(6.6f, 0f, 64f));
        yield return Interact(new Vector3(8f, 0.35f, 64f), "Linh chi");

        // --- Gương ---
        yield return WalkTo(new Vector3(0f, 0f, 69.6f));
        for (int i = 0; i < 3; i++) { yield return Interact(new Vector3(0f, 1.6f, 72f), "gương"); yield return WaitSeconds(0.35f); }
        yield return WalkTo(new Vector3(3.2f, 0f, 70f));
        for (int i = 0; i < 2; i++) { yield return Interact(new Vector3(5f, 1.6f, 72f), "gương"); yield return WaitSeconds(0.35f); }
        yield return WaitUntil(() => world.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId), 4f, "giải câu đố gương");
        Check(world.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId), "Xoay gương giải câu đố");
        yield return WalkPath(new Vector3(1.5f, 0f, 76f), new Vector3(0f, 0f, 79.3f));
        yield return Interact(new Vector3(0f, 1.2f, 81f), "Mảnh Trống C");
        Check(world.RelicCount == 3, "Đủ 3 mảnh");
        yield return WalkPath(new Vector3(0f, 0f, 78.5f), new Vector3(-5.5f, 0f, 77.5f));
        yield return Interact(new Vector3(-7f, 0.5f, 78f), "Lưu");
        bool safe = true;
        foreach (EnemyHealth e in EnemyHealth.Active)
            if (!e.IsDead && Vector3.Distance(e.transform.position, new Vector3(-7f, 0f, 76.5f)) < 10f) safe = false;
        Check(safe, "Mốc cp_truoc_dan không có quái trong 10 m");

        // --- Đàn Kim Quy ---
        yield return WalkPath(new Vector3(0f, 0f, 79.5f), new Vector3(0f, 0f, 83.8f));
        yield return Interact(new Vector3(1.5f, 0.35f, 85f), "Thuốc");
        yield return WalkTo(new Vector3(0f, 0f, 88f));
        yield return Interact(new Vector3(0f, 2f, 89.9f), "Mở cổng");
        Check(world.Has(StateSet.Doors, WorldIds.DoorKimQuy), "Mở cổng Đàn Kim Quy");
        yield return WalkTo(new Vector3(0f, 0f, 94.5f));
        Check(Find<ArenaGate>().IsLocked, "Đấu trường khóa cổng sau lưng");

        yield return FightGuardian();
        if (Failed) yield break;

        yield return WalkTo(new Vector3(0f, 0f, 103.2f), 0.6f, 20f);
        yield return Interact(new Vector3(0f, 1f, 105.5f), "Phục dựng");
        yield return WaitUntil(() => Session.State == GameSession.SessionState.ChapterComplete, 3f, "kết chương");
        Check(world.Has(StateSet.Puzzles, WorldIds.RelicRestoredC1) && world.Has(StateSet.Abilities, WorldIds.MovementKit),
            "Phục dựng trống, nhận Quà Kim Quy");
        yield return WaitSeconds(0.5f);
        Log($"Thời gian chơi chương 1 (bot): {Time.realtimeSinceStartup - startTime:0} giây");
    }

    // Trận Hộ vệ: đọc vùng báo đỏ để né bằng chạy, chiếu đèn khi trùm hồi thế, chém khi an toàn.
    private IEnumerator FightGuardian()
    {
        StoneGuardian guardian = Find<StoneGuardian>();
        EnemyHealth gh = guardian.GetComponent<EnemyHealth>();
        EnemyLanternReceiver weak = guardian.GetComponent<EnemyLanternReceiver>();
        GameObject sweep = guardian.transform.Find("SweepTelegraph").gameObject;
        GameObject slam = guardian.transform.Find("SlamTelegraph").gameObject;
        float start = Time.realtimeSinceStartup;
        float hpStart = Vitals.CurrentHealth;
        int dodges = 0, hitsTaken = 0;
        float lastHp = Vitals.CurrentHealth;

        while (!gh.IsDead)
        {
            if (Time.realtimeSinceStartup - start > 240f) { Fail("Trận Hộ vệ quá 4 phút"); yield break; }
            if (Vitals.IsDead) { Fail($"An chết trong trận Hộ vệ (máu Hộ vệ {gh.Current:0})"); yield break; }
            if (Vitals.CurrentHealth < lastHp) hitsTaken++;
            lastHp = Vitals.CurrentHealth;
            yield return UsePotionIfLow(45f);

            Vector3 p = Player.transform.position;
            Vector3 g = guardian.transform.position;

            if (slam.activeSelf)
            {
                // Chạy ra khỏi vòng 3 m.
                Vector3 center = slam.transform.position;
                Vector3 away = p - center; away.y = 0f;
                if (away.sqrMagnitude < 0.01f) away = Vector3.Cross(Vector3.up, g - p);
                Vector3 target = ClampArena(p + away.normalized * 4.5f, p, center);
                dodges++;
                yield return Escape(target, () => slam.activeSelf);
                continue;
            }

            if (sweep.activeSelf)
            {
                // Bước ngang khỏi vùng quét trước mặt trùm.
                Vector3 side = Vector3.Cross(Vector3.up, guardian.transform.forward).normalized;
                Vector3 target = ClampArena(p + side * 3.5f - guardian.transform.forward * 1.5f, p, g);
                dodges++;
                yield return Escape(target, () => sweep.activeSelf);
                continue;
            }

            float dist = Vector3.Distance(new Vector3(p.x, 0, p.z), new Vector3(g.x, 0, g.z));
            bool recovering = guardian.State == StoneGuardian.GuardianState.Sweep || guardian.State == StoneGuardian.GuardianState.Slam;

            if (!weak.IsWeakened && recovering && dist > 2.5f && Player.GetComponent<LanternController>().Energy > 20f)
            {
                yield return Aim(guardian.transform.position + Vector3.up * 1.8f);
                SetMouse(MouseButton.Right, true);
                float until = Time.realtimeSinceStartup + 1.3f;
                while (Time.realtimeSinceStartup < until && !sweep.activeSelf && !slam.activeSelf) yield return null;
                SetMouse(MouseButton.Right, false);
                continue;
            }

            if (dist > 1.9f)
            {
                FaceYaw(p, g);
                Hold(Key.W);
                yield return null;
                Release(Key.W);
                continue;
            }

            FaceYaw(p, g);
            yield return Click(MouseButton.Left);
            float wait = Time.realtimeSinceStartup + 0.6f;
            while (Time.realtimeSinceStartup < wait && !sweep.activeSelf && !slam.activeSelf) yield return null;
        }
        ReleaseAll();
        Log($"Hạ Hộ vệ Đá sau {Time.realtimeSinceStartup - start:0.0} giây; máu An {hpStart:0} → {Vitals.CurrentHealth:0}; né {dodges} lần; trúng đòn {hitsTaken} lần");
        Check(Find<WorldStateStore>().Has(StateSet.Bosses, WorldIds.GuardianBoss) && !Find<ArenaGate>().IsLocked, "Hộ vệ chết: ghi guardian_01, mở cổng");
        yield return WaitSeconds(1.2f);
    }

    private static Vector3 ClampArena(Vector3 target, Vector3 from, Vector3 threat)
    {
        Vector3 clamped = new Vector3(Mathf.Clamp(target.x, -7.5f, 7.5f), 0f, Mathf.Clamp(target.z, 92f, 106.5f));
        // Bị dồn vào góc: chạy vòng sang hướng vuông góc.
        if (Vector3.Distance(clamped, threat) < 3.6f)
        {
            Vector3 side = Vector3.Cross(Vector3.up, threat - from).normalized * 5f;
            Vector3 a = from + side, b = from - side;
            a = new Vector3(Mathf.Clamp(a.x, -7.5f, 7.5f), 0f, Mathf.Clamp(a.z, 92f, 106.5f));
            b = new Vector3(Mathf.Clamp(b.x, -7.5f, 7.5f), 0f, Mathf.Clamp(b.z, 92f, 106.5f));
            clamped = Vector3.Distance(a, threat) > Vector3.Distance(b, threat) ? a : b;
        }
        return clamped;
    }

    private IEnumerator Escape(Vector3 target, System.Func<bool> danger)
    {
        Hold(Key.LeftShift);
        Hold(Key.W);
        float until = Time.realtimeSinceStartup + 1.6f;
        while (danger() && Time.realtimeSinceStartup < until)
        {
            Vector3 p = Player.transform.position;
            if (Vector3.Distance(new Vector3(p.x, 0, p.z), target) < 0.4f) { Release(Key.W); }
            else { Hold(Key.W); FaceYaw(p, target); }
            yield return null;
        }
        Release(Key.W);
        Release(Key.LeftShift);
    }
}
#endif
