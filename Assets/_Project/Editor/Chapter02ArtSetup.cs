using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class Chapter02ArtSetup
{
    const string Request="Library/SonDoongC2Art.request.json";
    const string Art="Assets/_Project/Art/Chapter02Environment";
    [Serializable]class Command{public string action;}
    [Serializable]class Report{public string action;public List<string> checks=new List<string>(),failures=new List<string>(),notes=new List<string>();}
    static bool busy;
    static Chapter02ArtSetup()=>EditorApplication.update+=Poll;
    static T[] All<T>(Scene s)where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static T Ref<T>(UnityEngine.Object o,string f)where T:UnityEngine.Object=>Chapter01RevisionSetup.Ref<T>(o,f);
    static void Poll()
    {
        if(busy||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(Request))return;busy=true;var r=new Report();
        try
        {
            r.action=JsonUtility.FromJson<Command>(File.ReadAllText(Request)).action;
            if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
            for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved user scene: "+SceneManager.GetSceneAt(i).path);
            if(r.action=="test"||r.action=="input-test")
            {SessionState.SetBool("SonDoong.C2ArtTest",true);SessionState.SetBool("SonDoong.C2ArtInputOnly",r.action=="input-test");EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();EditorApplication.isPaused=false;EditorApplication.EnterPlaymode();}
            else if(r.action=="build")
            {
                var b=UnityEditor.BuildPipeline.BuildPlayer(new UnityEditor.BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(x=>x.enabled).Select(x=>x.path).ToArray(),locationPathName="Builds/Chapter02/Windows/SonDoong.exe",target=UnityEditor.BuildTarget.StandaloneWindows64,options=UnityEditor.BuildOptions.Development});
                r.notes.Add("Build result="+b.summary.result+", errors="+b.summary.totalErrors+", warnings="+b.summary.totalWarnings+", bytes="+b.summary.totalSize);if(b.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)r.failures.Add("Windows build failed");else r.checks.Add("Windows x64 build succeeded");
            }
            else
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();
                try{var s=EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);if(r.action=="apply"){Apply(s);foreach(var c in All<Component>(s))if(c&&PrefabUtility.IsPartOfPrefabInstance(c))PrefabUtility.RecordPrefabInstancePropertyModifications(c);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}Inspect(s,r);if(r.action=="render"||r.action=="apply")Capture(s,r);}
                finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
        }catch(Exception e){r.failures.Add(e.ToString());}
        finally{File.WriteAllText("Logs/C2Art-"+r.action+".json",JsonUtility.ToJson(r,true));File.Delete(Request);busy=false;}
    }

    static void Apply(Scene s) => Chapter02EnvironmentArt.Install(s);
    static void Inspect(Scene s,Report r)
    {
        Action<bool,string> check=(ok,n)=>{if(ok)r.checks.Add(n);else r.failures.Add(n);};
        check(All<Transform>(s).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"No missing scripts");
        foreach(var rr in All<Renderer>(s).Where(x=>x.enabled&&x.gameObject.activeInHierarchy))
            if(rr.sharedMaterials.Any(m=>!m||!m.shader||!m.shader.isSupported))r.failures.Add("Unsupported material: "+rr.name);
        var art=All<Transform>(s).FirstOrDefault(t=>t.name=="ART_C2_Environment");
        check(art,"Authored environment group exists");
        if(art)check(!art.GetComponentsInChildren<Collider>(true).Any(c=>c.enabled),"Environment decoration adds no enabled traversal colliders");
        check(All<WindColumn>(s).Length==7&&All<QuizPedestal>(s).Length==4&&All<LoreStele>(s).Length==5&&All<SequenceInputNode>(s).Length==3,"Seven winds, four pedestals, five steles and three bells retained");
        check(All<Chapter02Atmosphere>(s).Length==1,"One chapter atmosphere owner");
        check(AssetDatabase.LoadAssetAtPath<SoundPalette>("Assets/_Project/Resources/C2SoundPalette.asset"),"Chapter-specific audio palette");
        check(All<Unity.Cinemachine.CinemachineCamera>(s).All(c=>c.GetComponent<CaveCameraCollisionGuard>()),"Cinemachine collision guard retained");
        foreach(var cp in All<MonoBehaviour>(s).OfType<ICheckpoint>())
        {
            var p=cp.SpawnPoint.position;
            check(Physics.Raycast(p+Vector3.up*.2f,Vector3.down,1,1<<3,QueryTriggerInteraction.Ignore)&&!Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore),"Safe checkpoint: "+cp.CheckpointId);
        }
        r.notes.Add("Renderers="+All<Renderer>(s).Count(x=>x.enabled&&x.gameObject.activeInHierarchy)+"; decoration contains no gameplay colliders. Existing floor/wall/bridge/gap collision retained, no NavMesh rebake.");
        r.notes.Add("Geometry: "+string.Join(";",All<Renderer>(s).Where(x=>x.transform.IsChildOf(All<Transform>(s).First(t=>t.name=="World_Geometry"))&&!x.transform.IsChildOf(art)).Take(24).Select(x=>x.name+"="+x.bounds)));
    }
    static void Capture(Scene s,Report r)
    {
        var g=new GameObject("C2_EditorCapture");var cam=g.AddComponent<Camera>();cam.fieldOfView=65;cam.nearClipPlane=.1f;cam.farClipPlane=180;cam.backgroundColor=new Color(.52f,.70f,.74f);cam.clearFlags=CameraClearFlags.SolidColor;
        var light=new GameObject("Capture_Fill").AddComponent<Light>();light.type=LightType.Directional;light.transform.rotation=Quaternion.Euler(48,-30,0);light.intensity=.25f;
        var rt=new RenderTexture(1600,900,24);rt.Create();var old=RenderTexture.active;
        var eyes=new[]{new Vector3(0,1.7f,-6),new Vector3(4,10.7f,13),new Vector3(6.7f,14.7f,26),new Vector3(5,19.7f,42),new Vector3(5,19.7f,76)};
        var aims=new[]{new Vector3(4,6,4),new Vector3(8,15,20),new Vector3(4,16,32),new Vector3(5,20,65),new Vector3(5,23,84)};
        try{Directory.CreateDirectory("docs/images");for(int i=0;i<eyes.Length;i++){cam.transform.position=eyes[i];cam.transform.LookAt(aims[i]);UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=rt});RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();string p="docs/images/c2-art-editor-p"+(i+1)+"-2026-11-06.png";File.WriteAllBytes(p,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);r.notes.Add("Editor render: "+p);}}
        finally{RenderTexture.active=old;cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(light.gameObject);UnityEngine.Object.DestroyImmediate(g);}
    }
}

