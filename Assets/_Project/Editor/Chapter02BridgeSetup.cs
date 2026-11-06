using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Chapter02BridgeSetup
{
    const string Request="Library/SonDoongC2Bridge.request.json";
    const string Ghost="Assets/A08 - Ultimate Monsters/Flying/FBX/Ghost.fbx";
    const string C1Art=Chapter01CompletionArt.Art;
    [Serializable]class Command{public string action;}
    [Serializable]class Report{public string action;public List<string> checks=new List<string>(),failures=new List<string>(),notes=new List<string>();}
    static bool busy;
    static Chapter02BridgeSetup()=>EditorApplication.update+=Poll;
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
            {SessionState.SetBool("SonDoong.C2BridgeTest",true);SessionState.SetBool("SonDoong.C2BridgeInputOnly",r.action=="input-test");EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();EditorApplication.isPaused=false;EditorApplication.EnterPlaymode();}
            else
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();
                try
                {
                    var s=EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);
                    if(r.action=="apply")
                    {
                        Apply(s);foreach(var c in All<Component>(s))if(c&&PrefabUtility.IsPartOfPrefabInstance(c))PrefabUtility.RecordPrefabInstancePropertyModifications(c);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();
                    }
                    Inspect(s,r,r.action!="inspect");
                }finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
        }catch(Exception e){r.failures.Add(e.ToString());}
        finally{Directory.CreateDirectory("Logs");File.WriteAllText("Logs/C2Bridge-"+r.action+".json",JsonUtility.ToJson(r,true));File.Delete(Request);busy=false;}
    }
    static void Apply(Scene s)
    {
        foreach(var health in All<EnemyHealth>(s).Where(h=>h.GetComponent<ThrowerBrain>()||h.GetComponent<EnemyBrain>()))InstallGhost(health);
        foreach(var b in All<ThrowerBrain>(s)){BuilderUtil.Set(b,"detectRange",14f);BuilderUtil.Set(b,"throwCooldown",2.5f);}
        var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Project/Data/Enemies/ED_HacAnhNem.asset");definition.detectionRadius=14;EditorUtility.SetDirty(definition);
        var root=All<Transform>(s).Single(t=>t.name=="World_Geometry");var old=root.Find("ART_C2_Bridge");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
        var art=new GameObject("ART_C2_Bridge").transform;art.SetParent(root,false);
        foreach(var p in new[]{new Vector3(3.9f,18,44.5f),new Vector3(6.1f,18,60.5f)})
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Bridge_Cover_Stone";g.layer=3;g.transform.SetParent(art,false);g.transform.position=p+Vector3.up*.95f;g.transform.localScale=new Vector3(.8f,1.9f,1.1f);
            g.GetComponent<Renderer>().sharedMaterial=All<Transform>(s).Single(t=>t.name=="Ledge_P4_L").GetComponent<Renderer>().sharedMaterial;
        }
        var nav=All<Unity.AI.Navigation.NavMeshSurface>(s).Single();var saved=nav.navMeshData;nav.BuildNavMesh();var generated=nav.navMeshData;
        if(saved&&generated!=saved){EditorUtility.CopySerialized(generated,saved);nav.RemoveData();nav.navMeshData=saved;nav.AddData();EditorUtility.SetDirty(saved);UnityEngine.Object.DestroyImmediate(generated);}
        // Local fallback for invalid GPU Resident Drawer batches on C2 unload/reload.
        // Use Unity's own exclusion component; leave project/C1 render pipeline settings intact.
        var exclusion=Type.GetType("UnityEngine.Rendering.DisallowGPUDrivenRendering, Unity.RenderPipelines.GPUDriven.Runtime",true);
        foreach(var sceneRoot in s.GetRootGameObjects().Where(g=>g.GetComponentsInChildren<MeshRenderer>(true).Length>0))
        {
            var component=sceneRoot.GetComponent(exclusion)??sceneRoot.AddComponent(exclusion);
            exclusion.GetProperty("applyToChildrenRecursively").SetValue(component,true);
        }
    }
    static void InstallGhost(EnemyHealth health)
    {
        var model=health.transform.Find("VIS_ImportedEnemy");var brain=health.GetComponent<EnemyBrain>();var thrower=health.GetComponent<ThrowerBrain>();
        if(!model)
        {
            var warning=new List<Transform>();if(brain&&Ref<GameObject>(brain,"telegraph"))warning.Add(Ref<GameObject>(brain,"telegraph").transform);
            if(thrower){warning.Add(Ref<Transform>(thrower,"aimTelegraph"));warning.Add(Ref<GameObject>(thrower,"meleeTelegraph").transform);}
            foreach(var rr in health.GetComponentsInChildren<Renderer>(true))if(!warning.Any(t=>rr.transform==t||rr.transform.IsChildOf(t)))rr.enabled=false;
            model=Chapter01CompletionArt.Model(Ghost,"VIS_ImportedEnemy",health.transform,health.transform.position+Vector3.up*.08f,1.9f).transform;
            foreach(var rr in model.GetComponentsInChildren<Renderer>(true))
            {
                rr.gameObject.layer=7;rr.sharedMaterials=rr.sharedMaterials.Select(m=>AssetDatabase.LoadAssetAtPath<Material>(C1Art+"/Materials/Shadow_"+m.name+".mat")??throw new InvalidOperationException("Project Ghost material missing: "+m.name)).ToArray();
                if(rr is SkinnedMeshRenderer skinned)skinned.updateWhenOffscreen=true;
            }
            var animator=model.GetComponentInChildren<Animator>(true);if(!animator)animator=model.gameObject.AddComponent<Animator>();animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<AnimatorController>(C1Art+"/Animations/AC_HacAnh.controller");
            animator.runtimeAnimatorController.animationClips.First(c=>c.name.Contains("Flying_Idle")).SampleAnimation(animator.gameObject,0);
            Component presentation=thrower?(Component)model.gameObject.AddComponent<ThrowerPresentation>():model.gameObject.AddComponent<EnemyPresentation>();
            BuilderUtil.Set(presentation,"health",health);BuilderUtil.Set(presentation,"brain",thrower?(UnityEngine.Object)thrower:brain);BuilderUtil.Set(presentation,"animator",animator);
            if(thrower)BuilderUtil.Set(presentation,"receiver",health.GetComponent<EnemyLanternReceiver>());
        }
        model.localRotation=Quaternion.identity;
        var receiver=health.GetComponent<EnemyLanternReceiver>();BuilderUtil.Set(receiver,"enableStun",true);BuilderUtil.Set(receiver,"stunDuration",4f);BuilderUtil.Set(receiver,"stunRecoveryImmunity",3f);BuilderUtil.Set(receiver,"tintRenderer",(Renderer)null);
        if(!health.GetComponent<EnemyStunIndicator>())
        {
            var marker=new GameObject("VIS_StunStars").transform;marker.SetParent(health.transform,false);var stars=new List<Transform>();
            for(int i=0;i<3;i++){var star=new GameObject("Star_"+i,typeof(MeshFilter),typeof(MeshRenderer));star.layer=2;star.transform.SetParent(marker,false);star.GetComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(C1Art+"/Meshes/Stun_Star.asset");star.GetComponent<Renderer>().sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(C1Art+"/Materials/Stun_Stars.mat");stars.Add(star.transform);}
            var indicator=health.gameObject.AddComponent<EnemyStunIndicator>();BuilderUtil.Set(indicator,"receiver",receiver);BuilderUtil.Set(indicator,"indicator",marker);BuilderUtil.Set(indicator,"stars",stars.ToArray());BuilderUtil.Set(indicator,"height",2.25f);marker.gameObject.SetActive(false);
        }
    }
    static void Inspect(Scene s,Report r,bool validate)
    {
        Action<bool,string> check=(ok,label)=>{if(ok)r.checks.Add(label);else r.failures.Add(label);};
        check(All<Transform>(s).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"No missing scripts");
        foreach(var t in All<Transform>(s).Where(t=>t.name.StartsWith("Bridge_")||t.name.StartsWith("Ledge_P4")||t.name.StartsWith("Rail_")||t.name=="CP_C2_Arena"))r.notes.Add("GEOMETRY "+t.name+" pos="+t.position+" scale="+t.lossyScale);
        foreach(var enemy in All<EnemyHealth>(s).Where(h=>h.GetComponent<EnemyBrain>()||h.GetComponent<ThrowerBrain>()))
        {
            r.notes.Add("ENEMY "+enemy.name+" at "+enemy.transform.position+" | "+JsonUtility.ToJson(enemy.GetComponent<ThrowerBrain>())+" | "+JsonUtility.ToJson(enemy.GetComponent<EnemyLanternReceiver>()));
            if(!validate)continue;var model=enemy.transform.Find("VIS_ImportedEnemy");check(model&&Quaternion.Angle(model.localRotation,Quaternion.identity)<.1f,"Imported Ghost aligned +Z: "+enemy.name);
            var animator=model.GetComponentInChildren<Animator>();check(animator&&!animator.applyRootMotion&&animator.runtimeAnimatorController.animationClips.Any(c=>c.name.Contains("Stunned")),"Bowed stun animation, no root motion: "+enemy.name);
            check(new SerializedObject(enemy.GetComponent<EnemyLanternReceiver>()).FindProperty("enableStun").boolValue&&enemy.GetComponent<EnemyStunIndicator>(),"Focus stun and star indicator configured: "+enemy.name);
            if(enemy.GetComponent<ThrowerBrain>())check(!enemy.GetComponent<UnityEngine.AI.NavMeshAgent>(),"Stationary thrower has no movement agent: "+enemy.name);
        }
        var door=All<RequirementDoor>(s).Single();r.notes.Add("DOOR "+JsonUtility.ToJson(door));var cp=All<CheckpointTrigger>(s).Single();r.notes.Add("CP "+JsonUtility.ToJson(cp)+" spawn="+cp.SpawnPoint.position);
        if(!validate)return;
        foreach(var thrower in All<ThrowerBrain>(s))
        {
            var so=new SerializedObject(thrower);check(so.FindProperty("detectRange").floatValue==14&&so.FindProperty("throwCooldown").floatValue==2.5f,"Thrower tuning follows C2 spec: "+thrower.name);
            check(Vector3.Distance(cp.SpawnPoint.position,thrower.transform.position)>14,"Checkpoint spawn outside throw range: "+thrower.name);
        }
        check(Physics.Raycast(cp.SpawnPoint.position+Vector3.up*.1f,Vector3.down,1,1<<3,QueryTriggerInteraction.Ignore),"Arena checkpoint has solid floor");
        check(!Physics.CheckCapsule(cp.SpawnPoint.position+Vector3.up*.4f,cp.SpawnPoint.position+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore),"Arena checkpoint has clear capsule");
        var req=new SerializedObject(door).FindProperty("requirements");check(req.arraySize==4,"Arena door retains four prerequisites");
        foreach(float z in new[]{48.75f,56.75f})check(!Physics.Raycast(new Vector3(5,19,z),Vector3.down,2,1<<3,QueryTriggerInteraction.Ignore),"Original bridge gap remains open: "+z);
        var exclusion=Type.GetType("UnityEngine.Rendering.DisallowGPUDrivenRendering, Unity.RenderPipelines.GPUDriven.Runtime",true);
        check(All<MeshRenderer>(s).All(m=>m.transform.root.GetComponent(exclusion)!=null&&(bool)exclusion.GetProperty("applyToChildrenRecursively").GetValue(m.transform.root.GetComponent(exclusion))),"C2 meshes use regular rendering for safe scene unload/reload");
    }
}
