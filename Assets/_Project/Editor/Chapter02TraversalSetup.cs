using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

[InitializeOnLoad]
public static class Chapter02TraversalSetup
{
    const string Request="Library/SonDoongC2Traversal.request.json";
    const string Art="Assets/_Project/Art/Chapter02Traversal";
    [Serializable] class Command { public string action; }
    [Serializable] class Report { public string action; public List<string> checks=new List<string>(),failures=new List<string>(),notes=new List<string>(); }
    static bool busy;
    static Chapter02TraversalSetup()=>EditorApplication.update+=Poll;
    static T[] All<T>(Scene s)where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static void Guard()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");
        for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved user scene: "+SceneManager.GetSceneAt(i).path);
    }
    static void Poll()
    {
        if(busy||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(Request))return;
        busy=true;var r=new Report();
        try
        {
            r.action=JsonUtility.FromJson<Command>(File.ReadAllText(Request)).action;Guard();
            if(r.action=="inspect"||r.action=="apply"||r.action=="validate")
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();
                try
                {
                    var s=EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);
                    if(r.action=="apply"){Apply(s);foreach(var c in All<Component>(s))if(c&&PrefabUtility.IsPartOfPrefabInstance(c))PrefabUtility.RecordPrefabInstancePropertyModifications(c);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}
                    Inspect(s,r);
                }
                finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if(r.action=="test")
            {
                SessionState.SetBool("SonDoong.C2TraversalTest",true);EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);
                EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();EditorApplication.isPaused=false;EditorApplication.EnterPlaymode();
            }
            else throw new InvalidOperationException(r.action);
        }
        catch(Exception e){r.failures.Add(e.ToString());}
        finally{Directory.CreateDirectory("Logs");File.WriteAllText("Logs/C2Traversal-"+r.action+".json",JsonUtility.ToJson(r,true));File.Delete(Request);busy=false;}
    }
    static void Apply(Scene s)
    {
        Chapter01RevisionSetup.Folder(Art);string path=Art+"/Wind_Transparent.mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
        mat.SetColor("_BaseColor",new Color(.25f,.73f,.78f,.12f));mat.SetFloat("_Surface",1);mat.SetFloat("_Blend",0);mat.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);mat.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);mat.SetFloat("_ZWrite",0);mat.SetFloat("_Cull",0);mat.SetFloat("_Smoothness",.05f);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");mat.renderQueue=3000;EditorUtility.SetDirty(mat);
        foreach(var wind in All<WindColumn>(s))
        {
            var visual=Chapter01RevisionSetup.Ref<GameObject>(wind,"visual");
            foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true)){renderer.sharedMaterial=mat;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
        }
        foreach(var camera in All<CinemachineCamera>(s))if(!camera.GetComponent<CaveCameraCollisionGuard>())camera.gameObject.AddComponent<CaveCameraCollisionGuard>();
        var world=All<Transform>(s).Single(t=>t.name=="World_Geometry");var old=world.Find("ART_C2_Traversal");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var root=new GameObject("ART_C2_Traversal").transform;root.SetParent(world,false);
        string markPath=Art+"/Landing_Stone.mat";var stone=AssetDatabase.LoadAssetAtPath<Material>(markPath);if(!stone){stone=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(stone,markPath);}stone.color=new Color(.35f,.60f,.55f);stone.SetFloat("_Smoothness",.15f);EditorUtility.SetDirty(stone);
        foreach(var q in new[]{new Vector3(5,5.015f,6.2f),new Vector3(4.5f,9.015f,13.6f),new Vector3(8.6f,13.015f,16.6f),new Vector3(9.2f,13.015f,23),new Vector3(6.5f,13.015f,27),new Vector3(3,13.015f,31),new Vector3(2.8f,13.015f,33),new Vector3(3,13.015f,27.2f),new Vector3(5.6f,18.015f,35.6f)})
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Landing_And_Aisle_Mark";g.layer=2;g.transform.SetParent(root,false);g.transform.position=q;g.transform.localScale=new Vector3(.55f,.015f,.55f);g.GetComponent<Renderer>().sharedMaterial=stone;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
        }
    }
    static void Inspect(Scene s,Report r)
    {
        Physics.SyncTransforms();
        int missing=All<Transform>(s).Sum(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject));if(missing>0)r.failures.Add("Missing scripts="+missing);else r.checks.Add("No missing scripts in C2");
        foreach(var c in All<Collider>(s).Where(c=>c.gameObject.activeInHierarchy&&c.enabled&&c.gameObject.layer==3&&c.bounds.min.z<40&&c.bounds.max.z> -9))
            r.notes.Add("ENV "+c.name+" | "+c.bounds);
        foreach(var wind in All<WindColumn>(s).Where(w=>w.transform.position.z<39))
        {
            var so=new SerializedObject(wind);var col=wind.GetComponent<Collider>();var visual=Chapter01RevisionSetup.Ref<GameObject>(wind,"visual");
            r.notes.Add("WIND "+wind.name+" | foot="+wind.transform.position+" top="+wind.HoverTopY+" | trigger="+col.bounds+" | alpha="+visual.GetComponentInChildren<Renderer>().sharedMaterial.color.a);
        }
        foreach(var t in All<Transform>(s).Where(t=>t.GetComponent<CheckpointAltar>()||t.GetComponent<SequenceInputNode>()||t.GetComponent<QuizPedestal>()))if(t.position.z<40)r.notes.Add("TARGET "+t.name+" | "+t.position);
        foreach(var c in All<CinemachineCamera>(s))r.notes.Add("CAMERA "+c.name+" | guard="+!!c.GetComponent<CaveCameraCollisionGuard>()+" | "+string.Join(",",c.GetComponents<Component>().Select(x=>x.GetType().Name)));
        if(r.action=="apply"||r.action=="validate")
        {
            Action<bool,string> check=(ok,text)=>{if(ok)r.checks.Add(text);else r.failures.Add(text);};
            foreach(var camera in All<CinemachineCamera>(s))check(camera.GetComponent<CaveCameraCollisionGuard>(),"Cinemachine3 final collision guard: "+camera.name);
            foreach(var wind in All<WindColumn>(s)) {var material=Chapter01RevisionSetup.Ref<GameObject>(wind,"visual").GetComponentInChildren<Renderer>(true).sharedMaterial;check(material.GetFloat("_Surface")==1&&material.color.a<=.2f,"Transparent readable wind: "+wind.name);}
            foreach(var q in new[]{new Vector3(5,5.08f,8.6f),new Vector3(5,5.08f,9.75f),new Vector3(9.2f,13.08f,22.3f),new Vector3(9.2f,13.08f,23.75f),new Vector3(6.5f,13.08f,27),new Vector3(3,13.08f,31),new Vector3(2.8f,13.08f,34.5f),new Vector3(3,13.08f,27.2f),new Vector3(5,18.08f,38.6f)})
                check(!Physics.CheckCapsule(q+Vector3.up*.4f,q+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore),"Standing clearance at passage/aisle "+q);
            foreach(var cp in All<CheckpointAltar>(s).Where(c=>c.CheckpointId=="c2_cp_start"||c.CheckpointId=="c2_cp_mid"))
            {var q=cp.SpawnPoint.position;check(Physics.Raycast(q+Vector3.up*.2f,Vector3.down,1,1<<3,QueryTriggerInteraction.Ignore)&&!Physics.CheckCapsule(q+Vector3.up*.4f,q+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore),"Safe floor/capsule at "+cp.CheckpointId);}
        }
    }
}
