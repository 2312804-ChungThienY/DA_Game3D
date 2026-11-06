using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Explicit requests only: audit existing C2, without rebuilding the level.
[InitializeOnLoad]
public static class Chapter02MechanicsAudit
{
    public const string ScenePath = "Assets/_Project/Scenes/03_GiengTroiChimLac.unity";
    const string RequestPath = "Library/SonDoongC2Mechanics.request.json";
    [Serializable] class Request { public string action; }
    [Serializable] class Report
    {
        public string action;
        public List<string> checks = new List<string>(), failures = new List<string>(), notes = new List<string>();
    }
    static bool busy;
    static Chapter02MechanicsAudit() => EditorApplication.update += Poll;
    static T[] All<T>(Scene scene) where T : Component => scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray();
    static void Guard()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Unsaved user scene: " + SceneManager.GetSceneAt(i).path);
    }
    static void Poll()
    {
        if (busy || EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(RequestPath)) return;
        busy = true;
        var report = new Report();
        try
        {
            report.action = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath)).action;
            Guard();
            if (report.action == "audit")
            {
                var setup = EditorSceneManager.GetSceneManagerSetup();
                try { Inspect(EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single), report); }
                finally { EditorSceneManager.RestoreSceneManagerSetup(setup); }
            }
            else if (report.action == "test")
            {
                SessionState.SetString("SonDoong.C2AuditReturnScene", SceneManager.GetActiveScene().path);
                SessionState.SetBool("SonDoong.C2MechanicsTest", true);
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                EditorApplication.EnterPlaymode();
                report.notes.Add("Opt-in component regression; protected personal saves; no NewGame, no C3 authoring.");
            }
            else throw new InvalidOperationException("Unknown action: " + report.action);
        }
        catch (Exception e) { report.failures.Add(e.ToString()); }
        finally
        {
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/C2Mechanics-" + report.action + ".json", JsonUtility.ToJson(report, true));
            File.Delete(RequestPath);
            busy = false;
        }
    }
    static void Inspect(Scene scene, Report r)
    {
        Action<bool, string> check = (ok, text) => { if (ok) r.checks.Add(text); else r.failures.Add(text); };
        check(All<Transform>(scene).Sum(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)) == 0, "No missing scripts in C2");
        check(All<WindColumn>(scene).Length == 7, "Seven existing wind columns");
        check(All<QuizPedestal>(scene).Length == 4, "Four existing reward quiz pedestals");
        check(All<LoreStele>(scene).Length == 5, "Five existing lore steles");
        check(All<SequenceInputNode>(scene).Length == 3, "Three existing bells");
        check(All<ThrowerBrain>(scene).Length == 2, "Two existing throwers");
        check(All<BossChimLac>(scene).Length == 1, "One existing Chim Lac boss");
        foreach (var wind in All<WindColumn>(scene))
        {
            var so = new SerializedObject(wind);
            check(so.FindProperty("worldState").objectReferenceValue && so.FindProperty("visual").objectReferenceValue && wind.GetComponent<Collider>().isTrigger, "Wind references/trigger: " + wind.name);
            r.notes.Add(wind.name + ": position=" + wind.transform.position + ", hoverY=" + wind.HoverTopY + ", initiallyActive=" + so.FindProperty("startsActive").boolValue + ", tool=" + so.FindProperty("requiredBossTool").stringValue);
        }
        foreach (var receiver in All<EnemyLanternReceiver>(scene))
            r.notes.Add(receiver.name + ": stunEnabled=" + new SerializedObject(receiver).FindProperty("enableStun").boolValue);
        foreach (var quiz in All<QuizPedestal>(scene))
        {
            var so = new SerializedObject(quiz);
            var definition = so.FindProperty("quiz").objectReferenceValue as QuizDefinition;
            check(definition && definition.IsWellFormed && definition.relatedLore, "Quiz structure/lore reference: " + quiz.name);
            r.notes.Add(quiz.name + ": pedestalId=" + so.FindProperty("pedestalId").stringValue + ", position=" + quiz.transform.position);
        }
        foreach (var exit in All<ChapterExit>(scene))
        {
            var so = new SerializedObject(exit);
            r.notes.Add("C2 exit requires=" + so.FindProperty("requiredPuzzleId").stringValue + ", next=" + so.FindProperty("nextScene").stringValue + ", checkpoint=" + so.FindProperty("nextCheckpointId").stringValue);
        }
        var checkpoints = All<CheckpointAltar>(scene).Select(c => c.CheckpointId).Concat(All<CheckpointTrigger>(scene).Select(c => new SerializedObject(c).FindProperty("checkpointId").stringValue)).ToArray();
        foreach (var id in new[] { "c2_cp_start", "c2_cp_mid", "c2_cp_arena" })
            check(checkpoints.Count(x => x == id) == 1 && WorldIds.SceneOfCheckpoint(id) == GameSession.SecondChapterScene, "Unique registered checkpoint: " + id);
        r.notes.Add("Static configuration only. C2 remains blockout; this does not certify traversal, history sources, Windows or duration.");
    }
}
