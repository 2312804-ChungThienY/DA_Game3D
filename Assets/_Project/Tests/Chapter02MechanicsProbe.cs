#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Opt-in component regression in the real C2 scene. No synthetic input/focus bypass.
public sealed class Chapter02MechanicsProbe : MonoBehaviour
{
    [Serializable] class Report
    {
        public string mode = "Editor Play Mode; real C2 components/physics, API interactions, positional fixtures; not a full traversal or manual input test";
        public List<string> checks = new List<string>(), failures = new List<string>(), errors = new List<string>();
    }
    [Serializable] class Backup { public string[] paths, hashes; public bool[] existed; }
    readonly Report report = new Report();
    Backup backup; string folder; bool finished, completed, oldBackground;
    PlayerMotor motor; PlayerVitals vitals; WorldStateStore world;
    float lastProgress;
    static T Find<T>() where T : Component => UnityEngine.Object.FindAnyObjectByType<T>();
    static T[] All<T>() where T : Component => UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    static FieldInfo F(object o, string n)
    {
        for (Type t = o.GetType(); t != null; t = t.BaseType)
        { var f = t.GetField(n, BindingFlags.Instance | BindingFlags.NonPublic); if (f != null) return f; }
        throw new MissingFieldException(n);
    }
    static T Get<T>(object o, string n) => (T)F(o, n).GetValue(o);
    static void Set(object o, string n, object v) => F(o, n).SetValue(o, v);
    static string Hash(string p) { if (!File.Exists(p)) return "absent"; using (var s = SHA256.Create()) return BitConverter.ToString(s.ComputeHash(File.ReadAllBytes(p))); }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (!SessionState.GetBool("SonDoong.C2MechanicsTest", false)) return;
        SessionState.SetBool("SonDoong.C2MechanicsTest", false);
        new GameObject("C2MechanicsProbe").AddComponent<Chapter02MechanicsProbe>();
    }
    void Awake()
    {
        Application.logMessageReceived += Log;
        oldBackground = Application.runInBackground; Application.runInBackground = true;
        string p = Path.Combine(Application.persistentDataPath, "save_v1.json");
        backup = new Backup { paths = new[] { p, p + ".bak", p + ".tmp" } };
        backup.existed = backup.paths.Select(File.Exists).ToArray(); backup.hashes = backup.paths.Select(Hash).ToArray();
        folder = "Temp/C2MechanicsBackup_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff"); Directory.CreateDirectory(folder);
        for (int i = 0; i < backup.paths.Length; i++) if (backup.existed[i]) File.Copy(backup.paths[i], folder + "/" + i + ".original");
        File.WriteAllText(folder + "/manifest.json", JsonUtility.ToJson(backup, true));
        // No progression/autosave side effects from positional fixtures.
        foreach (var c in All<CheckpointTrigger>()) c.enabled = false;
        foreach (var a in All<ArenaGate>()) a.enabled = false;
        foreach (var e in All<EnemyBrain>()) e.enabled = false;
        foreach (var e in All<ThrowerBrain>()) e.enabled = false;
        lastProgress = Time.realtimeSinceStartup;
    }
    void Log(string message, string stack, LogType type)
    { if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert) report.errors.Add(message); }
    void Check(bool ok, string text)
    {
        if (!ok) throw new InvalidOperationException(text);
        report.checks.Add(text); lastProgress = Time.realtimeSinceStartup;
        Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/C2Mechanics-progress.json", JsonUtility.ToJson(report, true));
    }
    void Update() { if (!finished && Time.realtimeSinceStartup - lastProgress > 35f) { report.failures.Add("Probe timeout"); Finish(); } }
    IEnumerator Start()
    {
        var stack = new Stack<IEnumerator>(); stack.Push(Run());
        while (stack.Count > 0)
        {
            object current = null; bool more = false; Exception error = null;
            try { more = stack.Peek().MoveNext(); if (more) current = stack.Peek().Current; } catch (Exception e) { error = e; }
            if (error != null) { report.failures.Add(error.ToString()); break; }
            if (!more) { stack.Pop(); continue; }
            if (current is IEnumerator nested) stack.Push(nested); else yield return current;
        }
        Finish();
    }
    void Finish()
    {
        if (finished) return; finished = true;
        if (!completed && report.failures.Count == 0) report.failures.Add("Interrupted before completion");
        Time.timeScale = 1f;
        try
        {
            for (int i = 0; i < backup.paths.Length; i++)
            {
                if (backup.existed[i]) File.Copy(folder + "/" + i + ".original", backup.paths[i], true);
                else if (File.Exists(backup.paths[i])) File.Delete(backup.paths[i]);
            }
            Check(backup.paths.Select(Hash).SequenceEqual(backup.hashes), "Personal primary/backup/temp saves restored byte-for-byte");
        }
        catch (Exception e) { report.failures.Add("Save restoration: " + e); }
        Application.runInBackground = oldBackground; Application.logMessageReceived -= Log;
        Directory.CreateDirectory("docs/reports"); File.WriteAllText("docs/reports/C2_Mechanics_PlayMode_2026-11-05.json", JsonUtility.ToJson(report, true));
        EditorApplication.isPlaying = false;
        EditorApplication.delayCall += () =>
        {
            string path = SessionState.GetString("SonDoong.C2AuditReturnScene", "");
            if (!EditorApplication.isPlayingOrWillChangePlaymode && File.Exists(path)) EditorSceneManager.OpenScene(path);
        };
    }
    void OnDestroy() { if (!finished) Finish(); }
    IEnumerator Wait(Func<bool> condition, float seconds, string label)
    {
        float end = Time.realtimeSinceStartup + seconds;
        while (!condition() && Time.realtimeSinceStartup < end) yield return null;
        Check(condition(), label);
    }
    void Position(Vector3 p, bool freeze = false)
    {
        motor.GetComponent<CharacterController>().enabled = true;
        motor.Teleport(p, Quaternion.identity); motor.enabled = !freeze;
        if (freeze) motor.GetComponent<CharacterController>().enabled = false;
        Physics.SyncTransforms();
    }
    IEnumerator Run()
    {
        yield return Wait(() => AppRoot.Instance && AppRoot.Instance.Session.State == GameSession.SessionState.Playing && Find<SaveService>().IsReady, 15, "Direct C2 session initialized");
        motor = Find<PlayerMotor>(); vitals = motor.GetComponent<PlayerVitals>(); world = Find<WorldStateStore>();
        world.Restore(new WorldSnapshot());
        Check(!world.Has(StateSet.Abilities, WorldIds.AbilityWind), "Wind not implicitly granted on entering C2");
        WindColumn tutorial = All<WindColumn>().Single(x => x.name == "WindColumn_P1_Tut");
        Vector3 basePoint = tutorial.transform.position; basePoint.y = 0.1f;
        Position(basePoint); yield return new WaitForSeconds(.4f);
        Check(motor.transform.position.y < .5f && !motor.IsHovering, "No lift before wind ability");
        world.TryAdd(StateSet.Abilities, WorldIds.AbilityWind);
        yield return Wait(() => motor.IsHovering, 5, "Real tutorial trigger lifts player to hover");
        Check(Mathf.Abs(motor.transform.position.y - tutorial.HoverTopY) < .5f, "Hover clamps near configured top");
        float hoverY = motor.transform.position.y;
        Position(motor.transform.position + Vector3.left * 3);
        yield return new WaitForSeconds(.3f);
        Check(!motor.IsHovering && motor.transform.position.y < hoverY, "Leaving column releases hover and restores gravity");
        Position(new Vector3(0, .2f, -6), true);
        tutorial.enabled = false; Check(!tutorial.IsActive && !Get<GameObject>(tutorial, "visual").activeSelf, "Disabled wind hides visual and clears active flag");
        tutorial.enabled = true; Check(tutorial.IsActive && Get<GameObject>(tutorial, "visual").activeSelf, "Re-enabled wind refreshes immediately");
        yield return Bells();
        yield return Quizzes();
        yield return Thrower();
        yield return Boss();
        Check(report.errors.Count == 0, "No runtime errors/assertions/exceptions during C2 regression");
        completed = true;
    }
    IEnumerator Bells()
    {
        var sequence = Find<SequencePuzzleController>(); var final = All<WindColumn>().Single(x => x.name == "WindColumn_P3_Final");
        var definition = Get<SequencePuzzleDefinition>(sequence, "definition");
        Check(definition.sequence.SequenceEqual(new[] { "bell_2", "bell_3", "bell_1" }), "Existing bells use 2-3-1 order");
        var baseline = world.Export();
        sequence.TryInput("bell_1"); yield return new WaitForSeconds(1f);
        Check(sequence.Progress == 0 && !sequence.IsSolved && !final.IsActive, "Wrong bell resets without opening final wind");
        sequence.TryInput("bell_2"); yield return new WaitForSeconds(.3f);
        world.TryAdd(StateSet.Lore, "c2_lore_p1");
        Check(sequence.Progress == 1, "Unrelated world Changed preserves current bell attempt");
        sequence.TryInput("bell_3"); world.Restore(baseline); yield return new WaitForSeconds(.35f);
        Check(sequence.Progress == 0 && sequence.CanAcceptInput && !sequence.IsSolved, "Snapshot restore cancels pending delayed bell input");
        foreach (string id in definition.sequence) { Check(sequence.TryInput(id), "Accepted " + id); yield return new WaitForSeconds(.3f); }
        Check(sequence.IsSolved && final.IsActive, "Correct sequence opens persistent final wind");
        Check(!sequence.TryInput("bell_2"), "Solved sequence rejects duplicate reward input");
        var solved = world.Export(); world.Restore(baseline);
        Check(!final.IsActive && !sequence.IsSolved, "Earlier snapshot closes solved wind/sequence");
        world.Restore(solved); Check(final.IsActive && sequence.IsSolved, "Solved snapshot restores wind and puzzle");
    }
    IEnumerator Quizzes()
    {
        var panel = Find<QuizPanel>(); var inventory = Find<InventoryService>();
        foreach (var pedestal in All<QuizPedestal>().OrderBy(p => p.name))
        {
            var quiz = Get<QuizDefinition>(pedestal, "quiz");
            pedestal.Interact(motor.gameObject); Check(panel.IsOpen && panel.Current == quiz, "Quiz opens: " + pedestal.name);
            if (quiz.quizId == "c2_qz_p1")
            {
                panel.Choose((quiz.correctIndex + 1) % 4);
                Check(!pedestal.IsOpened && !world.Has(StateSet.QuizAnswered, quiz.quizId), "Wrong answer does not grant fragment");
                Get<Button>(panel, "retryButton").onClick.Invoke();
            }
            panel.Choose(quiz.correctIndex); yield return null;
            Check(pedestal.IsOpened && world.Has(StateSet.QuizAnswered, quiz.quizId), "Correct answer grants reward: " + pedestal.name);
            string before = JsonUtility.ToJson(world.Export()) + JsonUtility.ToJson(new SaveData { inventory = inventory.Export() });
            pedestal.Interact(motor.gameObject);
            Check(before == JsonUtility.ToJson(world.Export()) + JsonUtility.ToJson(new SaveData { inventory = inventory.Export() }), "Opened pedestal cannot reward twice: " + pedestal.name);
        }
        Check(!world.Has(StateSet.QuizFirstTry, "c2_qz_p1"), "Wrong-then-correct quiz does not earn first-try score");
        Check(new[] { "c2_frag_1", "c2_frag_2", "c2_frag_3" }.All(id => world.Has(StateSet.Fragments, id)) && world.Has(StateSet.BossTools, "c2_tool_long_gio_lon"), "All three fragments and boss tool granted by real pedestals");
    }
    IEnumerator Thrower()
    {
        var brain = All<ThrowerBrain>().First(); var receiver = brain.GetComponent<EnemyLanternReceiver>();
        Vector3 p = brain.transform.position;
        Position(p + Vector3.down * 3f, true); brain.enabled = true;
        yield return new WaitForSeconds(.55f);
        Check(brain.State != ThrowerBrain.ThrowerState.Melee, "Thrower cannot start melee across different floors");
        brain.enabled = false; Position(p + Vector3.forward * 1.3f, true); vitals.RestoreFull();
        // Optional telegraph renderer must not own the actual damage clock.
        Set(brain, "meleeTelegraph", null); Set(brain, "nextMelee", 0f); brain.enabled = true;
        float hp = vitals.CurrentHealth; yield return new WaitForSeconds(.65f);
        Check(Mathf.Approximately(hp - vitals.CurrentHealth, 10f), "Melee applies exactly one hit without optional telegraph visual");
        brain.enabled = false;
        Position(p + Vector3.forward * 1.3f, true); vitals.RestoreFull();
        Set(receiver, "enableStun", true); receiver.ReceiveFocus(1.1f); brain.enabled = true;
        yield return null; yield return null;
        Check(receiver.IsStunned && brain.State == ThrowerBrain.ThrowerState.Stunned, "Configured lantern stun interrupts thrower FSM");
        hp = vitals.CurrentHealth; int throws = Get<int>(brain, "throws"); yield return new WaitForSeconds(.8f);
        Check(vitals.CurrentHealth == hp && throws == Get<int>(brain, "throws") && !Get<Transform>(brain, "aimTelegraph").gameObject.activeSelf, "Stunned thrower neither attacks nor shows aiming telegraph");
        brain.enabled = false; Set(receiver, "stunnedUntil", float.NegativeInfinity);
        Position(new Vector3(0, .2f, -6), true);
    }
    IEnumerator Boss()
    {
        var boss = Find<BossChimLac>(); var focus = boss.GetComponent<ChimLacFocusReceiver>();
        if (focus == null) focus = Get<ChimLacFocusReceiver>(boss, "focus");
        var health = boss.GetComponent<EnemyHealth>();
        var arenaWind = All<WindColumn>().Single(x => x.name == "WindColumn_Arena");
        var state = world.Export();
        world.Restore(new WorldSnapshot());
        Check(!arenaWind.IsActive, "Arena wind requires boss tool");
        world.Restore(state); Check(arenaWind.IsActive, "Earned boss tool unlocks arena wind");
        boss.Activate(); yield return Wait(() => boss.State == BossChimLac.ChimState.Perched, 3, "Boss reaches real perched state");
        focus.ReceiveFocus(2f); Check(focus.Progress == 0 && !focus.StunTriggered, "Ground-level focus cannot bypass arena wind mechanic");
        boss.ResetBoss();
        Vector3 center = arenaWind.transform.position; center.y = Get<float>(boss, "floorY") + .1f;
        Position(center);
        yield return Wait(() => motor.IsHovering, 5, "Real arena wind lifts player to boss height");
        boss.Activate(); yield return Wait(() => boss.State == BossChimLac.ChimState.Perched, 3, "Boss perches while player hovers");
        Check(focus.CanReceiveFocus, "Hovering in active column enables perched focus");
        focus.ReceiveFocus(.2f); float progress = focus.Progress;
        Time.timeScale = 0; focus.ReceiveFocus(.8f);
        Check(focus.Progress == progress && !focus.StunTriggered, "Paused focus cannot add boss stun progress");
        Time.timeScale = 1;
        focus.ReceiveFocus(float.NaN); focus.ReceiveFocus(float.PositiveInfinity);
        Check(focus.Progress == progress, "Invalid focus delta cannot corrupt progress");
        yield return new WaitForSeconds(.3f); focus.ReceiveFocus(.05f);
        Check(Mathf.Approximately(focus.Progress, .05f), "Interrupted focus resets after grace interval");
        float deadline = Time.time + 2f;
        while (!focus.StunTriggered && Time.time < deadline) { focus.ReceiveFocus(Time.deltaTime); yield return null; }
        yield return Wait(() => boss.State == BossChimLac.ChimState.Stunned, 1, "Sustained airborne focus triggers real stunned state");
        // Freeze fixture outside wind so AI/damage measurements are independent of fall input.
        Position(center, true); yield return new WaitForSeconds(.6f);
        float start = health.Current;
        for (int i = 0; i < 4; i++) health.TryTakeDamage(new DamageInfo(motor.gameObject, 20f, DamageTeam.Player));
        Check(Mathf.Approximately(start - health.Current, 60f), "Chim Lac stun window caps total amplified damage at 60");
        boss.ResetBoss();
        Check(!boss.IsActive && boss.State == BossChimLac.ChimState.Waiting && !focus.StunTriggered && focus.Progress == 0 && health.Current == health.Max, "Reset clears boss FSM/focus/HP");
        Check(!Get<Transform>(boss, "diveTelegraph").gameObject.activeSelf && !Get<GameObject>(boss, "gustTelegraph").activeSelf, "Reset hides dive and gust telegraphs");
        Check(health.TryTakeDamage(new DamageInfo(motor.gameObject, 20f, DamageTeam.Player)) && Mathf.Approximately(health.Current, health.Max - 20f), "Reset clears stun damage cap and multiplier");
        boss.ResetBoss(); boss.Activate(); yield return null;
        boss.ResetBoss();
        Check(health.TryTakeDamage(new DamageInfo(motor.gameObject, 20f, DamageTeam.Player)), "Reset during invulnerable flight clears invulnerability");
        boss.ResetBoss();
    }
}
#endif
