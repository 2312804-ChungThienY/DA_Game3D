using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

[InitializeOnLoad]
public static class CampCraftingSetup
{
    const string RequestPath="Library/SonDoongCraftingSetup.request.json";
    const string ScenePath="Assets/_Project/Scenes/02_HangLinhMach.unity";
    static double next;static bool running;
    [Serializable] public class Request {public string action;}
    [Serializable] public class Report {public string action;public List<string> checks=new List<string>();public List<string> failures=new List<string>();}
    static CampCraftingSetup(){EditorApplication.update+=Poll;}
    static void Poll()
    {
        if(running||EditorApplication.timeSinceStartup<next||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(RequestPath))return;
        next=EditorApplication.timeSinceStartup+.5;running=true;var report=new Report();
        try
        {
            report.action=JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath)).action;
            if(EditorApplication.isPlaying){EditorApplication.ExitPlaymode();throw new InvalidOperationException("Exited Play Mode; retry after Editor is ready.");}
            if(report.action=="contracts"){CraftingContractChecks.Run();report.checks.Add("PASS: all 7 supplied crafting contracts.");}
            else if(report.action=="play-test")
            {
                if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Open scene has unsaved changes.");
                SessionState.SetBool("SonDoong.CraftingTest",true);EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Boot.unity");EditorApplication.EnterPlaymode();report.checks.Add("Requested crafting Play Mode test from Boot.");
            }
            else if(report.action=="build")Build(report);
            else if(report.action=="open-main")
            {
                if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Open scene has unsaved changes.");
                EditorSceneManager.OpenScene(ScenePath);report.checks.Add("Opened main scene in Edit Mode.");
            }
            else WithScene(report,report.action=="install");
        }
        catch(Exception e){report.failures.Add(e.ToString());}
        finally{Directory.CreateDirectory("Logs");File.WriteAllText("Logs/CampCraftingSetup.json",JsonUtility.ToJson(report,true));File.Delete(RequestPath);running=false;}
    }
    static IEnumerable<T> Components<T>(Scene s) where T:Component =>s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<T>(true));
    static void Set(UnityEngine.Object o,string name,UnityEngine.Object value){var so=new SerializedObject(o);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
    static T Ref<T>(UnityEngine.Object o,string name) where T:UnityEngine.Object =>new SerializedObject(o).FindProperty(name).objectReferenceValue as T;
    static void WithScene(Report report,bool install=false)
    {
        Scene prior=SceneManager.GetActiveScene();var scene=SceneManager.GetSceneByPath(ScenePath);bool opened=scene.IsValid()&&scene.isLoaded;
        if(!opened)scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
        try
        {
            if(install)
            {
                if(scene.isDirty)throw new InvalidOperationException("Scene has unsaved edits; save before install.");
                SceneManager.SetActiveScene(scene);Install(scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            }
            Validate(scene,report);
        }
        finally{if(prior.IsValid()&&prior.isLoaded)SceneManager.SetActiveScene(prior);if(!opened)EditorSceneManager.CloseScene(scene,true);}
    }
    static void Install(Scene scene)
    {
        var inventory=Components<PlayerInventory>(scene).Single();var world=Components<WorldStateStore>(scene).Single();var camp=Components<SaveCheckpoint>(scene).Single(c=>c.CheckpointId=="cp_trai");
        var gameplay=scene.GetRootGameObjects().Single(g=>g.name=="Gameplay").transform;
        var existing=gameplay.Find("Camp_CraftingStation");var root=existing!=null?existing.gameObject:new GameObject("Camp_CraftingStation");root.transform.SetParent(gameplay,false);root.transform.position=camp.transform.position+Vector3.left*1.8f;root.transform.rotation=Quaternion.identity;root.transform.localScale=Vector3.one;root.layer=LayerMask.NameToLayer("Interactable");
        var old=root.transform.Find("TableVisual");var table=old!=null?old.gameObject:GameObject.CreatePrimitive(PrimitiveType.Cube);table.name="TableVisual";table.transform.SetParent(root.transform,false);table.transform.localPosition=new Vector3(0,.35f,0);table.transform.localRotation=Quaternion.identity;table.transform.localScale=new Vector3(1.2f,.7f,.8f);table.layer=root.layer;table.GetComponent<BoxCollider>().isTrigger=false;
        const string matPath="Assets/_Project/Materials/Inventory/MAT_CampCraftingTable.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,matPath);}material.SetColor("_BaseColor",new Color(.32f,.19f,.075f));EditorUtility.SetDirty(material);table.GetComponent<Renderer>().sharedMaterial=material;
        var canvas=Components<Canvas>(scene).Single(c=>c.name=="Canvas_Gameplay").transform;var feedback=canvas.Find("TXT_CraftFeedback");
        if(feedback==null){var go=new GameObject("TXT_CraftFeedback",typeof(RectTransform));go.transform.SetParent(canvas,false);feedback=go.transform;}
        var text=feedback.GetComponent<TextMeshProUGUI>()??feedback.gameObject.AddComponent<TextMeshProUGUI>();var rect=text.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,0);rect.pivot=new Vector2(.5f,0);rect.anchoredPosition=new Vector2(0,200);rect.sizeDelta=new Vector2(850,70);
        text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/UI/Fonts/Inventory_Vietnamese.asset");text.fontSize=24;text.color=new Color(1,.92f,.62f);text.alignment=TextAlignmentOptions.Center;text.textWrappingMode=TextWrappingModes.Normal;text.raycastTarget=false;text.text="";text.transform.SetSiblingIndex(3);
        var station=root.GetComponent<CampCraftingStation>()??root.AddComponent<CampCraftingStation>();Set(station,"inventory",inventory);Set(station,"worldState",world);Set(station,"feedbackText",text);
        var data=new SerializedObject(station);data.FindProperty("interactionDistance").floatValue=3;data.FindProperty("craftInterval").floatValue=.35f;data.ApplyModifiedPropertiesWithoutUndo();
    }
    static void Validate(Scene scene,Report report)
    {
        var station=Components<CampCraftingStation>(scene).Single();var inventory=Components<PlayerInventory>(scene).Single();var world=Components<WorldStateStore>(scene).Single();var checkpoint=Components<SaveCheckpoint>(scene).Single(c=>c.CheckpointId=="cp_trai");var table=station.transform.Find("TableVisual");
        if(station.name!="Camp_CraftingStation"||station.GetComponent<SaveCheckpoint>()!=null||Ref<PlayerInventory>(station,"inventory")!=inventory||Ref<WorldStateStore>(station,"worldState")!=world)throw new InvalidOperationException("Station references invalid");
        if(table==null||table.GetComponent<BoxCollider>()==null||table.GetComponent<BoxCollider>().isTrigger||station.gameObject.layer!=LayerMask.NameToLayer("Interactable")||table.gameObject.layer!=station.gameObject.layer||!ReferenceEquals(table.GetComponentInParent<IInteractable>(),station))throw new InvalidOperationException("Table collider/interaction layers invalid");
        float distance=Vector3.Distance(checkpoint.transform.position,station.transform.position);if(distance<1.5f||distance>2.01f)throw new InvalidOperationException("Table must be 1.5-2m from camp checkpoint");
        var data=new SerializedObject(station);if(data.FindProperty("interactionDistance").floatValue!=3||Mathf.Abs(data.FindProperty("craftInterval").floatValue-.35f)>.001f)throw new InvalidOperationException("Station range/debounce invalid");
        var text=Ref<TMP_Text>(station,"feedbackText");if(text==null||text.name!="TXT_CraftFeedback"||text.raycastTarget||text.fontSize!=24||!text.font.HasCharacters("Đã chế 1 thuốc máu. Cần lá cẩm và linh chi. Không đủ chỗ.",out uint[] missing))throw new InvalidOperationException("Feedback/font invalid");
        if(!Components<PlayerInput>(scene).Single().actions.FindAction("Interact",true).bindings.Any(b=>b.path=="<Keyboard>/e"))throw new InvalidOperationException("Existing E binding missing");
        report.checks.Add("Station separate from checkpoint, 1.8m away; parent/child Interactable, solid BoxCollider, scanner parent interaction validated.");
        report.checks.Add("An inventory and GameSession world references, 3m range, 0.35s craft interval and Vietnamese feedback HUD validated.");
    }
    static void Build(Report result)
    {
        WithScene(result);CraftingContractChecks.Run();result.checks.Add("PASS: all 7 supplied crafting contracts.");Directory.CreateDirectory("Builds/CampCraftingValidation");var preloads=PlayerSettings.GetPreloadedAssets();UnityEditor.Build.Reporting.BuildReport report;
        try{report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/CampCraftingValidation/SonDoong.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});}
        finally{PlayerSettings.SetPreloadedAssets(preloads);AssetDatabase.SaveAssets();}
        foreach(var step in report.steps)foreach(var message in step.messages)if(message.type==LogType.Error||message.type==LogType.Exception)result.failures.Add(message.content);
        result.checks.Add("Windows build: "+report.summary.result+", errors="+report.summary.totalErrors+", warnings="+report.summary.totalWarnings);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Build failed");
    }
}
