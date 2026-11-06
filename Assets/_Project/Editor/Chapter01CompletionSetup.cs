using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Chapter01CompletionSetup
{
    const string RequestPath="Library/SonDoongC1Completion.request.json";
    const string Main="Assets/_Project/Scenes/02_HangLinhMach.unity";
    static double next;static bool busy;
    [Serializable] class Request{public string action;}
    [Serializable] class Report{public string action;public List<string> checks=new List<string>(),failures=new List<string>();}
    static Chapter01CompletionSetup(){EditorApplication.update+=Poll;}
    static IEnumerable<T> All<T>(Scene s)where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true));
    static void Guard(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");foreach(var s in Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt))if(s.isDirty)throw new InvalidOperationException("Unsaved scene: "+s.path);}
    static void Poll()
    {
        if(busy||EditorApplication.timeSinceStartup<next||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(RequestPath))return;
        busy=true;next=EditorApplication.timeSinceStartup+1;var r=new Report();
        try
        {
            r.action=JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath)).action;
            if(r.action=="stop-play"){EditorApplication.isPaused=false;EditorApplication.isPlaying=false;r.checks.Add("Stopped the user's Play session for the requested repairs; no save/new-game operation invoked.");}
            else if(r.action=="status"){r.checks.Add("playing="+EditorApplication.isPlaying+", compiling="+EditorApplication.isCompiling);foreach(var s in Enumerable.Range(0,SceneManager.sceneCount).Select(SceneManager.GetSceneAt))r.checks.Add(s.path+" dirty="+s.isDirty);}
            else
            {
                Guard();
                if(r.action=="repair-import")RepairImport(r);
                else if(r.action=="play-test")
                {
                    SessionState.SetBool("SonDoong.C1CompletionTest",true);
                    EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Boot.unity",OpenSceneMode.Single);
                    var game=EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor"));game.Focus();
                    EditorApplication.isPlaying=true;r.checks.Add("Opt-in C1 integration test started; test backs up saves and settings before New Game.");
                }
                else if(r.action=="traversal-test")
                {
                    SessionState.SetBool("SonDoong.ExpandedC1Test",true);SessionState.SetBool("SonDoong.ExpandedC1GeometryOnly",false);SessionState.SetBool("SonDoong.C1CompletionTraversal",true);
                    EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Boot.unity");EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();EditorApplication.EnterPlaymode();r.checks.Add("Current full-route/crafting/puzzles/save regression requested, with isolated boss fixture.");
                }
                else if(r.action=="build")
                {
                    Directory.CreateDirectory("Builds/Chapter01Complete/Windows");
                    var build=UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Chapter01Complete/Windows/SonDoong.exe",target=UnityEditor.BuildTarget.StandaloneWindows64,options=UnityEditor.BuildOptions.None});
                    r.checks.Add("Native Windows x64: "+build.summary.result+", errors="+build.summary.totalErrors+", warnings="+build.summary.totalWarnings+", size="+build.summary.totalSize);
                    if(build.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)r.failures.Add("Windows build failed");
                }
                else if(r.action=="inventory")Inventory(r);
                else if(r.action=="validate")
                {
                    var setup=EditorSceneManager.GetSceneManagerSetup();try{foreach(string path in new[]{Main,"Assets/_Project/Scenes/92_EnvironmentPreview.unity"})
                    {
                        var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);var transforms=All<Transform>(scene).ToArray();int missing=transforms.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));if(missing>0)r.failures.Add(path+": missing scripts="+missing);
                        var active=All<Renderer>(scene).Where(rr=>rr.enabled&&rr.gameObject.activeInHierarchy).ToArray();
                        foreach(var rr in active)foreach(var material in rr.sharedMaterials)
                        {if(!material||!material.shader||material.shader.name=="Hidden/InternalErrorShader"||!material.shader.isSupported)r.failures.Add(path+": invalid material on "+rr.name);else if(material.shader.name=="Standard")r.failures.Add(path+": Standard shader in active URP scene on "+rr.name+": "+AssetDatabase.GetAssetPath(material));}
                        r.checks.Add(path+": active renderers="+active.Length+", missing scripts="+missing+", FBX crystal groups="+transforms.Count(t=>t.name=="VIS_Crystal_FBX")+", lantern groups="+transforms.Count(t=>t.name=="VIS_LotusLantern")+", junctions="+transforms.Count(t=>t.name.StartsWith("Vestibule_")));
                        foreach(var cp in All<CheckpointAltar>(scene)){if(!UnityEngine.AI.NavMesh.SamplePosition(cp.SpawnPoint.position,out var hit,.65f,UnityEngine.AI.NavMesh.AllAreas))r.failures.Add("Spawn not on NavMesh: "+cp.CheckpointId);else r.checks.Add("NavMesh safe spawn: "+cp.CheckpointId);}
                    }}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
                }
                else if(r.action=="fix-paver-normals")Chapter01PropSetup.FixPaverNormals(r.checks.Add);
                else if(r.action=="install-audio")Chapter01SoundSetup.Install(r.checks.Add);
                else if(r.action=="refresh-menu-art")
                {
                    var setup=EditorSceneManager.GetSceneManagerSetup();try{var scene=EditorSceneManager.OpenScene(Main,OpenSceneMode.Single);Chapter01UISetup.RefreshMenuPicture(scene,r.checks.Add);}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
                }
                else if(r.action=="refine-ui")
                {
                    const string prefab="Assets/_Project/Resources/PF_AppRootUI.prefab";var ui=PrefabUtility.LoadPrefabContents(prefab);try{Chapter01UISetup.Refine(ui);PrefabUtility.SaveAsPrefabAsset(ui,prefab);}finally{PrefabUtility.UnloadPrefabContents(ui);}
                    var setup=EditorSceneManager.GetSceneManagerSetup();try{foreach(string path in new[]{"Assets/_Project/Scenes/01_MainMenu.unity",Main,"Assets/_Project/Scenes/92_EnvironmentPreview.unity"}){var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);foreach(var root in scene.GetRootGameObjects())Chapter01UISetup.Refine(root);EditorSceneManager.SaveScene(scene);}AssetDatabase.SaveAssets();}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}r.checks.Add("Frame centers transparent; menu Sprite Single assigned; dropdown and guardian health palette matched.");
                }
                else if(r.action=="install-props")
                {
                    string folder="SourceAssets/SceneBackups/2026-11-05_Chapter01Completion/Props_"+DateTime.UtcNow.ToString("HHmmssfff");Directory.CreateDirectory(folder);var setup=EditorSceneManager.GetSceneManagerSetup();
                    try{foreach(string path in new[]{Main,"Assets/_Project/Scenes/92_EnvironmentPreview.unity"}){File.Copy(path,folder+"/"+Path.GetFileName(path));var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);Chapter01PropSetup.Install(scene,r.checks.Add);EditorSceneManager.SaveScene(scene);}AssetDatabase.SaveAssets();}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
                    r.checks.Add("Scene backup: "+folder);
                }
                else if(r.action=="install-ui")
                {
                    string folder="SourceAssets/SceneBackups/2026-11-05_Chapter01Completion/UI_"+DateTime.UtcNow.ToString("HHmmssfff");Directory.CreateDirectory(folder);File.Copy("Assets/_Project/Resources/PF_AppRootUI.prefab",folder+"/PF_AppRootUI.prefab");var setup=EditorSceneManager.GetSceneManagerSetup();
                    try
                    {
                        Chapter01UISetup.SharedUI(r.checks.Add);
                        foreach(string path in new[]{"Assets/_Project/Scenes/01_MainMenu.unity",Main,"Assets/_Project/Scenes/92_EnvironmentPreview.unity"})
                        {File.Copy(path,folder+"/"+Path.GetFileName(path));var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);if(path.Contains("MainMenu"))Chapter01UISetup.Menu(scene,r.checks.Add);else Chapter01UISetup.Gameplay(scene,r.checks.Add);EditorSceneManager.SaveScene(scene);}
                        AssetDatabase.SaveAssets();
                    }
                    finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
                    r.checks.Add("UI prefab and scene backups: "+folder);
                }
                else if(r.action=="install-actors")
                {
                    string folder="SourceAssets/SceneBackups/2026-11-05_Chapter01Completion/Actors_"+DateTime.UtcNow.ToString("HHmmssfff");Directory.CreateDirectory(folder);var setup=EditorSceneManager.GetSceneManagerSetup();
                    try{foreach(string path in new[]{Main,"Assets/_Project/Scenes/92_EnvironmentPreview.unity"}){File.Copy(path,folder+"/"+Path.GetFileName(path));var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);Chapter01ActorsSetup.Install(scene,r.checks.Add);EditorSceneManager.SaveScene(scene);}AssetDatabase.SaveAssets();}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
                    r.checks.Add("Scene backup: "+folder);
                }
                else if(r.action=="repair-hull")
                {
                    string folder="SourceAssets/SceneBackups/2026-11-05_Chapter01Completion/Hull_"+DateTime.UtcNow.ToString("HHmmssfff");Directory.CreateDirectory(folder);
                    var setup=EditorSceneManager.GetSceneManagerSetup();
                    try{foreach(string path in new[]{Main,"Assets/_Project/Scenes/92_EnvironmentPreview.unity"})
                    {
                        File.Copy(path,folder+"/"+Path.GetFileName(path));File.Copy(path+".meta",folder+"/"+Path.GetFileName(path)+".meta");
                        var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);Chapter01CompletionArt.RepairHull(scene,r.checks.Add);EditorSceneManager.SaveScene(scene);
                    }AssetDatabase.SaveAssets();}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
                    r.checks.Add("Scene and metadata backup: "+folder);
                }
                else throw new InvalidOperationException("Unknown action: "+r.action);
            }
        }
        catch(Exception e){r.failures.Add(e.ToString());}
        finally{Directory.CreateDirectory("Logs");File.WriteAllText("Logs/C1Completion-"+r.action+".json",JsonUtility.ToJson(r,true));File.Delete(RequestPath);busy=false;}
    }
    static void RepairImport(Report r)
    {
        string backup="SourceAssets/SceneBackups/2026-11-05_Chapter01Completion/Metadata_"+DateTime.UtcNow.ToString("HHmmssfff");Directory.CreateDirectory(backup);
        foreach(string path in new[]{"Assets/DungeonModularPack/Textures/T_Step_N.tga","Assets/DungeonModularPack/Textures/T_Tile_N.tga","Assets/DungeonModularPack/Prefabs/Torch_B.prefab"})
        {
            string meta=path+".meta",before=AssetDatabase.AssetPathToGUID(path);File.Copy(meta,backup+"/"+Path.GetFileName(meta));
            using(var stream=new FileStream(meta,FileMode.Open,FileAccess.ReadWrite,FileShare.ReadWrite))r.checks.Add("Unity process can open metadata for writing: "+meta);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ForceSynchronousImport);
            var importer=AssetImporter.GetAtPath(path);if(!importer)throw new InvalidOperationException("Importer missing: "+path);
            importer.SaveAndReimport();
            string after=AssetDatabase.AssetPathToGUID(path);if(before!=after||string.IsNullOrEmpty(after))throw new InvalidOperationException("GUID changed: "+path);
            r.checks.Add("Reimport and metadata write succeeded, GUID retained "+after+": "+path);
        }
        AssetDatabase.SaveAssets();r.checks.Add("Metadata backup: "+backup);
    }
    static void Inventory(Report r)
    {
        foreach(string path in AssetDatabase.FindAssets("t:Model",new[]{"Assets/A08 - Ultimate Monsters","Assets/Kevin Iglesias"}).Select(AssetDatabase.GUIDToAssetPath).Where(p=>p.Contains("Ghost")||p.Contains("Golem")))
        {
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);var clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__"));
            r.checks.Add(path+" | "+CampEnvironmentSetup.BoundsOf(model)+" | clips="+string.Join(",",clips.Select(c=>c.name+":"+c.length.ToString("F2"))));
        }
        var s=SceneManager.GetActiveScene();if(s.path!=Main)throw new InvalidOperationException("Open C1 first for scene inventory.");
        foreach(var t in All<Transform>(s).Where(t=>t.GetComponents<MonoBehaviour>().Any(c=>c is EnemyHealth||c is StoneGuardian||c is EnemyBrain||c is LanternPickup||c is SealTarget||c is RequirementDoor||c is LightPuzzleReceiver)))r.checks.Add(t.name+" | "+t.position+" | "+string.Join(",",t.GetComponents<MonoBehaviour>().Where(c=>c).Select(c=>c.GetType().Name)));
    }
}
