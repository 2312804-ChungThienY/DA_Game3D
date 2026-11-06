using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class Chapter01RevisionSetup
{
    public const string Art="Assets/_Project/Art/Chapter01Revision";
    public const string Main="Assets/_Project/Scenes/02_HangLinhMach.unity",Preview="Assets/_Project/Scenes/92_EnvironmentPreview.unity";
    const string RequestPath="Library/SonDoongC1Revision.request.json";
    static bool busy;static double next;
    [Serializable] class Request{public string action;}
    [Serializable] class Report{public string action;public List<string> checks=new List<string>(),failures=new List<string>();}
    static Chapter01RevisionSetup(){EditorApplication.update+=Poll;}
    public static T[] All<T>(Scene scene)where T:Component=>scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    public static T Ref<T>(UnityEngine.Object o,string field)where T:UnityEngine.Object=>new SerializedObject(o).FindProperty(field)?.objectReferenceValue as T;
    public static void Folder(string path)
    {
        if(AssetDatabase.IsValidFolder(path))return;
        Directory.CreateDirectory(path);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate|ImportAssetOptions.ImportRecursive|ImportAssetOptions.ForceSynchronousImport);
        if(!AssetDatabase.IsValidFolder(path))throw new InvalidOperationException("Unity has not registered folder: "+path);
    }
    static void Guard(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved scene: "+SceneManager.GetSceneAt(i).path);}
    static void Poll()
    {
        if(busy||EditorApplication.timeSinceStartup<next||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(RequestPath))return;busy=true;next=EditorApplication.timeSinceStartup+1;var r=new Report();
        try
        {
            r.action=JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath)).action;Guard();
            if(r.action=="inspect")Inspect(r);
            else if(r.action=="inspect-guides-pedestals")
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();try{var scene=EditorSceneManager.OpenScene(Main,OpenSceneMode.Single);Chapter01GuidePedestalSetup.Inspect(scene,r.checks.Add);}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if(r.action=="validate-guides-pedestals")
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();try{foreach(string path in new[]{Main,Preview}){var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);Physics.SyncTransforms();Chapter01GuidePedestalSetup.Validate(scene,r.checks.Add);}}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if(r.action=="repair-guides-pedestals")
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();string backup="SourceAssets/SceneBackups/2026-11-05_GuidesPedestals/Apply_"+DateTime.UtcNow.ToString("HHmmssfff");Directory.CreateDirectory(backup);
                try{foreach(string path in new[]{Main,Preview}){File.Copy(path,backup+"/"+Path.GetFileName(path));File.Copy(path+".meta",backup+"/"+Path.GetFileName(path)+".meta");var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);Chapter01GuidePedestalSetup.Apply(scene,r.checks.Add);Physics.SyncTransforms();Chapter01GuidePedestalSetup.Validate(scene,r.checks.Add);EditorSceneManager.SaveScene(scene);}AssetDatabase.SaveAssets();r.checks.Add("Backup: "+backup);}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if(r.action=="apply")
            {
                Folder(Art);
                foreach(string duplicate in new[]{Art+" 1",Art+" 2"})if(Directory.Exists(duplicate)&&!Directory.GetFiles(duplicate,"*",SearchOption.AllDirectories).Any(f=>!f.EndsWith(".meta")))AssetDatabase.DeleteAsset(duplicate);
                string backup="SourceAssets/SceneBackups/2026-11-05_C1Revision/Apply_"+DateTime.UtcNow.ToString("HHmmssfff");Directory.CreateDirectory(backup);var setup=EditorSceneManager.GetSceneManagerSetup();
                try
                {
                    Chapter01RevisionUI.Shared(r.checks.Add);
                    foreach(string path in new[]{Main,Preview})
                    {File.Copy(path,backup+"/"+Path.GetFileName(path));File.Copy(path+".meta",backup+"/"+Path.GetFileName(path)+".meta");var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);Chapter01RevisionArt.Install(scene,r.checks.Add);Chapter01RevisionUI.Icons(scene,r.checks.Add);Chapter01RevisionUI.Gameplay(scene,r.checks.Add);EditorSceneManager.SaveScene(scene);}
                    const string menu="Assets/_Project/Scenes/01_MainMenu.unity";File.Copy(menu,backup+"/01_MainMenu.unity");var mainMenu=EditorSceneManager.OpenScene(menu,OpenSceneMode.Single);foreach(var root in mainMenu.GetRootGameObjects())Chapter01RevisionUI.Skin(root);EditorSceneManager.SaveScene(mainMenu);AssetDatabase.SaveAssets();r.checks.Add("Backups: "+backup);
                }
                finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if(r.action=="repair-mirrors")
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();try{foreach(string path in new[]{Main,Preview}){var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);Chapter01RevisionArt.RepairMirrors(scene,r.checks.Add);EditorSceneManager.SaveScene(scene);}AssetDatabase.SaveAssets();}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if(r.action=="refine-details")
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();try{foreach(string path in new[]{Main,Preview}){var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);Chapter01RevisionArt.RefineDetails(scene,r.checks.Add);Chapter01RevisionUI.RefineDetails(scene,r.checks.Add);EditorSceneManager.SaveScene(scene);}AssetDatabase.SaveAssets();}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if(r.action=="repair-crowns")
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();try{foreach(string path in new[]{Main,Preview}){var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);foreach(var cap in All<Transform>(scene).Where(t=>t.name.StartsWith("Door_Crown_Closure_"))){var b=cap.GetComponent<Renderer>().bounds;float top=6.15f;cap.position=new Vector3(cap.position.x,(b.min.y+top)*.5f,cap.position.z);cap.localScale=new Vector3(cap.localScale.x,top-b.min.y,cap.localScale.z);}EditorSceneManager.SaveScene(scene);r.checks.Add(path+": stone headers overlap full original arch crown to 6.15m.");}}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if(r.action=="repair-fire")
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();try{foreach(string path in new[]{Main,Preview}){var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);foreach(var t in All<Transform>(scene).Where(t=>t.name=="Bonfire_Fire"))t.gameObject.SetActive(false);foreach(var ps in All<ParticleSystem>(scene).Where(p=>p.name=="Fire_Flames")){var rr=ps.GetComponent<ParticleSystemRenderer>();rr.renderMode=ParticleSystemRenderMode.Stretch;rr.lengthScale=2.5f;rr.velocityScale=.25f;var main=ps.main;main.startLifetime=new ParticleSystem.MinMaxCurve(.55f,.9f);main.startSpeed=new ParticleSystem.MinMaxCurve(.8f,1.25f);main.startSize=new ParticleSystem.MinMaxCurve(.14f,.28f);main.startColor=new Color(1,.7f,.27f,.9f);var emission=ps.emission;emission.rateOverTime=52;}EditorSceneManager.SaveScene(scene);r.checks.Add(path+": old static fire mesh inactive; animated stretched flame particles refined.");}}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if(r.action=="play-test"||r.action=="traversal-test"||r.action=="stun-test"||r.action=="no-weakness-visuals-test"||r.action=="guides-pedestals-test"||r.action=="guides-pedestals-traversal")
            {
                if(!r.action.EndsWith("traversal")&&r.action!="traversal-test"){SessionState.SetBool("SonDoong.C1RevisionTest",true);SessionState.SetBool("SonDoong.C1RevisionStunOnly",r.action=="stun-test"||r.action=="no-weakness-visuals-test");SessionState.SetBool("SonDoong.NoWeaknessVisualsTest",r.action=="no-weakness-visuals-test");SessionState.SetBool("SonDoong.GuidesPedestalsTest",r.action=="guides-pedestals-test");}
                else{SessionState.SetBool("SonDoong.ExpandedC1Test",true);SessionState.SetBool("SonDoong.C1RevisionTraversal",true);SessionState.SetBool("SonDoong.GuidesPedestalsTraversal",r.action=="guides-pedestals-traversal");SessionState.SetBool("SonDoong.ExpandedC1GeometryOnly",false);}
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Boot.unity",OpenSceneMode.Single);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();EditorApplication.EnterPlaymode();r.checks.Add("Opt-in revision test requested; personal save files are backed up and restored by probe.");
            }
            else if(r.action=="validate")
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();try{foreach(string path in new[]{Main,Preview})
                {
                    var scene=EditorSceneManager.OpenScene(path,OpenSceneMode.Single);var transforms=All<Transform>(scene);int missing=transforms.Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));if(missing>0)r.failures.Add(path+": missing scripts="+missing);
                    foreach(var rr in All<Renderer>(scene).Where(rr=>rr.enabled&&rr.gameObject.activeInHierarchy))foreach(var mat in rr.sharedMaterials)if(!mat||!mat.shader||!mat.shader.isSupported||mat.shader.name=="Hidden/InternalErrorShader"||mat.shader.name=="Standard")r.failures.Add(path+": unsupported material on "+rr.name);
                    if(All<CampCraftingTable>(scene).Length!=0)r.failures.Add(path+": old table remains");
                    if(All<PuzzleCluePickup>(scene).Length!=PuzzleCluePickup.AllClueIds.Count||All<EnemyStunIndicator>(scene).Length!=3)r.failures.Add(path+": clue/indicator count mismatch");
                    foreach(var receiver in All<EnemyLanternReceiver>(scene)){var so=new SerializedObject(receiver);bool boss=receiver.GetComponent<StoneGuardian>();if(!so.FindProperty("enableStun").boolValue||so.FindProperty("stunDuration").floatValue!=(boss?2:4))r.failures.Add("Stun configuration: "+receiver.name);}
                    foreach(var cp in All<CheckpointAltar>(scene))if(!UnityEngine.AI.NavMesh.SamplePosition(cp.SpawnPoint.position,out var hit,.65f,UnityEngine.AI.NavMesh.AllAreas))r.failures.Add("Spawn off NavMesh: "+cp.CheckpointId);else r.checks.Add(path+": safe NavMesh spawn "+cp.CheckpointId);
                    foreach(var cap in transforms.Where(t=>t.name.StartsWith("Door_Crown_Closure_")))if(!Physics.Raycast(cap.position+Vector3.back*2,Vector3.forward,4,1<<3,QueryTriggerInteraction.Ignore))r.failures.Add("Crown is open: "+cap.name);
                    foreach(var pair in new[]{new KeyValuePair<string,int>("VIS_Portal_A22",5),new KeyValuePair<string,int>("VIS_A23_Mirror",2),new KeyValuePair<string,int>("VIS_DongSon_Playable",3)}){int count=transforms.Count(t=>t.name==pair.Key);r.checks.Add(path+": "+pair.Key+" instances="+count);if(count!=pair.Value)r.failures.Add("Asset instance count mismatch: "+pair.Key);}
                    if(!Ref<UnityEngine.UI.Button>(All<InventoryPanel>(scene).Single(),"craftPotionButton"))r.failures.Add("Inventory recipe button missing");
                    r.checks.Add(path+": materials supported, missing scripts="+missing+", "+All<PuzzleCluePickup>(scene).Length+" papers, three stun indicators, table absent, crown collision closed.");
                }}finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if(r.action=="build")
            {
                Directory.CreateDirectory("Builds/Chapter01Revision/Windows");var build=UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/Chapter01Revision/Windows/SonDoong.exe",target=UnityEditor.BuildTarget.StandaloneWindows64,options=UnityEditor.BuildOptions.None});
                r.checks.Add("Native Windows x64: "+build.summary.result+", errors="+build.summary.totalErrors+", warnings="+build.summary.totalWarnings+", size="+build.summary.totalSize);if(build.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)r.failures.Add("Windows build failed");
            }
            else throw new InvalidOperationException("Unknown revision action: "+r.action);
        }
        catch(Exception e){r.failures.Add(e.ToString());}
        finally{Directory.CreateDirectory("Logs");File.WriteAllText("Logs/C1Revision-"+r.action+".json",JsonUtility.ToJson(r,true));File.Delete(RequestPath);busy=false;}
    }
    static void Inspect(Report r)
    {
        var setup=EditorSceneManager.GetSceneManagerSetup();try
        {
            var scene=EditorSceneManager.OpenScene(Main,OpenSceneMode.Single);
            foreach(var t in All<Transform>(scene).Where(t=>t.GetComponent<DrumInteractable>()||t.GetComponent<MirrorRotator>()||t.GetComponent<CheckpointAltar>()||t.GetComponent<RelicPickup>()||t.GetComponent<SealTarget>()||t.name.StartsWith("Door")||t.name.StartsWith("Ancient_Arch")||t.name.StartsWith("Puzzle_Arch")))
            {r.checks.Add(t.name+" pos="+t.position+" scale="+t.lossyScale+" rot="+t.eulerAngles+" bounds="+CampEnvironmentSetup.BoundsOf(t.gameObject));foreach(var c in t.GetComponents<Collider>())r.checks.Add("  collider "+c.GetType().Name+" "+c.bounds);}
            foreach(var t in All<Transform>(scene).Where(t=>t.GetComponent<Renderer>()&&t.gameObject.activeInHierarchy&&(t.position-new Vector3(-4,0,0)).sqrMagnitude<12))r.checks.Add("CAMP "+t.name+" pos="+t.position+" size="+t.GetComponent<Renderer>().bounds.size);
            foreach(var health in All<EnemyHealth>(scene))
            {
                var visual=health.transform.Find("VIS_ImportedEnemy");if(!visual)continue;var animator=visual.GetComponentInChildren<Animator>();r.checks.Add("ACTOR "+health.name+" bones="+string.Join(",",animator.GetComponentsInChildren<Transform>().Select(t=>t.name).Take(48)));
                foreach(var t in animator.GetComponentsInChildren<Transform>().Where(t=>t.name.IndexOf("head",StringComparison.OrdinalIgnoreCase)>=0||t.name.IndexOf("spine",StringComparison.OrdinalIgnoreCase)>=0))r.checks.Add("BONE "+t.name+" local="+t.localRotation.eulerAngles+" forward="+t.forward+" up="+t.up);
                CaptureActor(visual.gameObject,health.name,r.checks.Add);
            }
            foreach(string path in new[]{"Assets/A20 - Trống đồng/source/filetex.fbx",Art+"/Models/Portal_A22.fbx",Art+"/Models/Mirror_A23.fbx",Art+"/Models/Pedestal_A24.fbx","Assets/A16 - Survival Pack/FBX/WoodLog.fbx"})
            {var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(model)r.checks.Add("MODEL "+path+" "+CampEnvironmentSetup.BoundsOf(model)+" meshes="+string.Join(",",model.GetComponentsInChildren<MeshFilter>().Select(m=>m.sharedMesh?m.sharedMesh.vertexCount.ToString():"none")));else r.checks.Add("MODEL NOT READY "+path);}
        }
        finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
    }
    static void CaptureActor(GameObject original,string label,Action<string> check)
    {
        var model=UnityEngine.Object.Instantiate(original);model.name="Inspection_Model";model.transform.position=new Vector3(800,40,0);model.transform.rotation=Quaternion.identity;model.transform.localScale=Vector3.one;var bounds=CampEnvironmentSetup.BoundsOf(model);
        var camGo=new GameObject("Inspection_Camera");var camera=camGo.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.07f,.10f,.11f);camera.orthographic=true;camera.orthographicSize=bounds.size.y*.62f;camera.nearClipPlane=.05f;camera.farClipPlane=30;
        var lightGo=new GameObject("Inspection_Light");var light=lightGo.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.4f;light.transform.rotation=Quaternion.Euler(35,-30,0);
        var target=new RenderTexture(640,640,24);target.Create();var previous=RenderTexture.active;var tex=new Texture2D(640,640,TextureFormat.RGB24,false);
        try{for(int i=0;i<2;i++){camera.transform.position=bounds.center+Vector3.forward*(i==0?-8:8);camera.transform.LookAt(bounds.center);RenderPipeline.SubmitRenderRequest(camera,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});RenderTexture.active=target;tex.ReadPixels(new Rect(0,0,640,640),0,0);tex.Apply();Directory.CreateDirectory("docs/images");File.WriteAllBytes("docs/images/c1-revision-before-"+label+"-"+(i==0?"front":"back")+".png",tex.EncodeToPNG());}check("Rendered imported rig from both travel-axis directions: "+label);}
        finally{RenderTexture.active=previous;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(model);UnityEngine.Object.DestroyImmediate(camGo);UnityEngine.Object.DestroyImmediate(lightGo);}
    }
}
