using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Explicit local requests; never changes an open, unsaved scene.
[InitializeOnLoad]
public static class CharacterPolishSetup
{
    const string RequestPath = "Library/SonDoongCharacterPolish.request.json";
    const string PrefabPath = "Assets/_Project/Prefabs/Player/PF_An_Prototype.prefab";
    const string ControllerPath = "Assets/_Project/Animations/AC_An.controller";
    [Serializable] class Request { public string action; }
    [Serializable] class Report
    {
        public string action;
        public List<string> checks = new List<string>();
        public List<string> failures = new List<string>();
        public List<string> warnings = new List<string>();
    }
    static double next;
    static CharacterPolishSetup() { EditorApplication.update += Poll; }
    static void Poll()
    {
        if (EditorApplication.timeSinceStartup < next || EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(RequestPath)) return;
        next = EditorApplication.timeSinceStartup + 1;
        var report = new Report();
        try
        {
            report.action = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath)).action;
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Stop Play Mode first.");
            if (report.action == "install") Install(report);
            else if (report.action == "focus-install")
            {
                for(int i=0;i<SceneManager.sceneCount;i++)
                    if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save open scenes before configuring focus.");
                var root=PrefabUtility.LoadPrefabContents(PrefabPath);
                try
                {
                    var scanner=new SerializedObject(root.GetComponent<LanternFocusScanner>());
                    scanner.FindProperty("range").floatValue=8f;scanner.FindProperty("enemyRange").floatValue=10f;scanner.ApplyModifiedPropertiesWithoutUndo();
                    var lantern=new SerializedObject(root.GetComponent<LanternController>());
                    var light=(Light)lantern.FindProperty("focusLight").objectReferenceValue;
                    light.range=12f;light.spotAngle=90f;light.innerSpotAngle=50f;light.enabled=false;
                    var visual=new SerializedObject(root.GetComponent<PlayerMeleeVisual>());
                    var trail=(TrailRenderer)visual.FindProperty("bladeTrail").objectReferenceValue;
                    if(trail!=null){trail.emitting=false;trail.enabled=false;trail.Clear();}
                    PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                    report.checks.Add("Enemy Focus=10m, puzzle Focus=8m; visual light range=12m with 90-degree cone; melee ribbon disabled.");
                }
                finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            else if (report.action == "build-report")
            {
                var latest=UnityEditor.Build.Reporting.BuildReport.GetLatestReport();
                if(latest==null)throw new InvalidOperationException("No build report available");
                report.checks.Add("Latest Windows build: "+latest.summary.result+", errors="+latest.summary.totalErrors+", warnings="+latest.summary.totalWarnings+", bytes="+latest.summary.totalSize);
                foreach(var warning in latest.steps.SelectMany(step=>step.messages)
                            .Where(message=>message.type==LogType.Warning).Select(message=>message.content))
                    report.warnings.Add(warning);
            }
            else if (report.action == "build" || report.action == "build-aim" || report.action == "build-melee" || report.action == "build-focus")
            {
                for(int i=0;i<SceneManager.sceneCount;i++)
                    if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save open scene changes before building.");
                var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),
                    locationPathName=report.action == "build-focus"
                        ? "Builds/FocusCombatValidation/Windows/SonDoong.exe"
                        : report.action == "build-melee"
                        ? "Builds/MeleeVisualValidation/Windows/SonDoong.exe"
                        : report.action == "build-aim"
                        ? "Builds/CharacterAimValidation/Windows/SonDoong.exe"
                        : "Builds/CharacterPolishValidation/Windows/SonDoong.exe",
                    target=BuildTarget.StandaloneWindows64,
                    options=BuildOptions.None
                });
                report.checks.Add("Windows x64 release build: "+result.summary.result+", errors="+result.summary.totalErrors+", warnings="+result.summary.totalWarnings);
                if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)
                    throw new InvalidOperationException("Windows build did not succeed");
            }
            else if (report.action == "test")
            {
                for (int i=0;i<SceneManager.sceneCount;i++)
                    if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Save open scene changes first.");
                SessionState.SetString("SonDoong.CharacterTest.PreviousScene", SceneManager.GetActiveScene().path);
                SessionState.SetBool("SonDoong.CharacterTest", true);
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Boot.unity");
                EditorApplication.EnterPlaymode();
                report.checks.Add("Requested character Play Mode regression from Boot; probe restores original saves.");
            }
            else if (report.action == "inspect")
            {
                report.checks.Add("Editor idle: " + Application.unityVersion);
                foreach (var scene in Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt))
                    report.checks.Add(scene.path + " dirty=" + scene.isDirty);
            }
            else throw new InvalidOperationException("Unknown action");
        }
        catch (Exception e) { report.failures.Add(e.ToString()); }
        Directory.CreateDirectory("Logs");
        File.WriteAllText("Logs/CharacterPolish-"+report.action+".json", JsonUtility.ToJson(report,true));
        File.Delete(RequestPath);
    }
    static IEnumerable<AnimatorState> States(AnimatorStateMachine machine)
    {
        foreach(var child in machine.states) yield return child.state;
        foreach(var child in machine.stateMachines) foreach(var state in States(child.stateMachine)) yield return state;
    }
    static void Backup(string path)
    {
        string dest = "SourceAssets/CharacterPolishBackups/2026-11-04/" + path;
        Directory.CreateDirectory(Path.GetDirectoryName(dest));
        if(!File.Exists(dest)) File.Copy(path,dest);
        if(File.Exists(path+".meta")&&!File.Exists(dest+".meta"))File.Copy(path+".meta",dest+".meta");
    }
    static void Install(Report report)
    {
        for(int i=0;i<SceneManager.sceneCount;i++)
            if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Save open scenes before configuring character.");
        Backup(PrefabPath); Backup(ControllerPath);
        string prefabGuid=AssetDatabase.AssetPathToGUID(PrefabPath);
        var prefab=PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            if(prefab.GetComponent<PlayerActionState>()==null)prefab.AddComponent<PlayerActionState>();
            prefab.GetComponent<Animator>().applyRootMotion=false;
            var controller=prefab.GetComponent<CharacterController>();
            controller.minMoveDistance=0f;
            var audio=prefab.GetComponent<AudioSource>();
            if(audio==null)audio=prefab.AddComponent<AudioSource>();
            audio.playOnAwake=false;audio.spatialBlend=1f;audio.minDistance=1f;audio.maxDistance=15f;
            var mixer=AssetDatabase.LoadAssetAtPath<UnityEngine.Audio.AudioMixer>("Assets/_Project/Audio/Mixers/MainAudioMixer.mixer");
            if(mixer!=null)audio.outputAudioMixerGroup=mixer.FindMatchingGroups("SFX").FirstOrDefault();
            var motorConfig=new SerializedObject(prefab.GetComponent<PlayerMotor>());
            motorConfig.FindProperty("movementAudio").objectReferenceValue=audio;
            string[] clips=AssetDatabase.FindAssets("t:AudioClip Player_Footstep").Select(AssetDatabase.GUIDToAssetPath).OrderBy(p=>p).ToArray();
            var steps=motorConfig.FindProperty("footstepClips");steps.arraySize=clips.Length;
            for(int i=0;i<clips.Length;i++)steps.GetArrayElementAtIndex(i).objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(clips[i]);
            string land=AssetDatabase.FindAssets("t:AudioClip Player_Land").Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
            if(land!=null)motorConfig.FindProperty("landingClip").objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(land);
            motorConfig.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(prefab,PrefabPath);
            report.checks.Add("Configured shared action policy on An prefab; root motion off, minMoveDistance=0.");
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        if(AssetDatabase.AssetPathToGUID(PrefabPath)!=prefabGuid)throw new InvalidOperationException("Prefab GUID changed");
        var ac=AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        var states=ac.layers.SelectMany(layer=>States(layer.stateMachine))
            .Concat(AssetDatabase.LoadAllAssetsAtPath(ControllerPath).OfType<AnimatorState>()).Distinct().ToArray();
        var airborne=states.FirstOrDefault(state=>state.name=="InAir"&&state.motion!=null);
        if(airborne==null)airborne=states.FirstOrDefault(state=>state.name=="JumpStart"&&state.motion!=null);
        foreach(var state in states)
        {
            report.checks.Add("Animation state "+state.name+": "+(state.motion!=null?state.motion.name:"no motion"));
            if(state.name=="Fly"&&state.motion==null&&airborne!=null)
            {
                state.motion=airborne.motion; EditorUtility.SetDirty(state);
                report.checks.Add("Repaired missing Fly motion with existing InAir clip in project controller.");
            }
        }
        AssetDatabase.SaveAssets();
        // Existing independent scene actors predate RequireComponent; upgrade them safely.
        var active=SceneManager.GetActiveScene();
        foreach(string guid in AssetDatabase.FindAssets("t:Scene",new[]{"Assets/_Project/Scenes"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);
            var scene=SceneManager.GetSceneByPath(path); bool opened=!scene.isLoaded;
            if(opened)scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Additive);
            bool changed=false;
            try
            {
                foreach(var motor in scene.GetRootGameObjects().SelectMany(go=>go.GetComponentsInChildren<PlayerMotor>(true)))
                {
                    if(motor.GetComponent<PlayerActionState>()!=null)continue;
                    Backup(path);motor.gameObject.AddComponent<PlayerActionState>();changed=true;
                    report.checks.Add("Added action policy to independent actor: "+path+"/"+motor.name);
                }
                if(changed)EditorSceneManager.SaveScene(scene);
            }
            finally { if(opened)EditorSceneManager.CloseScene(scene,true); }
        }
        if(active.IsValid()&&active.isLoaded)SceneManager.SetActiveScene(active);
        report.checks.Add("Project scenes inspected; existing references and GUIDs retained.");
    }
}
