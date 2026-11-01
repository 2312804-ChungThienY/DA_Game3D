using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public static class QuestHUDSetup
{
    const string RequestPath="Library/SonDoongQuestHUD.request.json";
    const string ScenePath="Assets/_Project/Scenes/02_HangLinhMach.unity";
    static double next;static bool running;
    [Serializable] public class Request {public string action;}
    [Serializable] public class Report {public string action;public List<string> checks=new List<string>();public List<string> failures=new List<string>();}
    static QuestHUDSetup(){EditorApplication.update+=Poll;}
    static void Poll()
    {
        if(running||EditorApplication.timeSinceStartup<next||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(RequestPath))return;
        next=EditorApplication.timeSinceStartup+.5;running=true;var report=new Report();
        try
        {
            report.action=JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath)).action;
            if(EditorApplication.isPlaying){EditorApplication.ExitPlaymode();throw new InvalidOperationException("Exited Play Mode; retry after Editor is ready.");}
            if(report.action=="contracts"){ChapterObjectiveChecks.Run();report.checks.Add("PASS: 128 objective combinations plus completed-to-new-game reset.");}
            else if(report.action=="play-test")
            {
                if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Open scene has unsaved edits.");
                SessionState.SetBool("SonDoong.QuestHUDTest",true);EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Boot.unity");EditorApplication.EnterPlaymode();report.checks.Add("Requested objective HUD Play Mode test from Boot.");
            }
            else if(report.action=="open-main")
            {
                if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Open scene has unsaved edits.");EditorSceneManager.OpenScene(ScenePath);report.checks.Add("Opened main scene in Edit Mode.");
            }
            else WithScene(report);
        }
        catch(Exception e){report.failures.Add(e.ToString());}
        finally{Directory.CreateDirectory("Logs");File.WriteAllText("Logs/QuestHUDSetup.json",JsonUtility.ToJson(report,true));File.Delete(RequestPath);running=false;}
    }
    static IEnumerable<T> Components<T>(Scene s) where T:Component=>s.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<T>(true));
    static void Set(UnityEngine.Object o,string name,UnityEngine.Object value){var so=new SerializedObject(o);so.FindProperty(name).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
    static T Ref<T>(UnityEngine.Object o,string name) where T:UnityEngine.Object=>new SerializedObject(o).FindProperty(name).objectReferenceValue as T;
    static void WithScene(Report report)
    {
        Scene prior=SceneManager.GetActiveScene();var scene=SceneManager.GetSceneByPath(ScenePath);bool opened=scene.IsValid()&&scene.isLoaded;
        if(!opened)scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
        try
        {
            if(report.action=="install")
            {
                if(scene.isDirty)throw new InvalidOperationException("Scene has unsaved edits; save before install.");SceneManager.SetActiveScene(scene);Install(scene);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            }
            Validate(scene,report);if(report.action=="preview")Preview(scene,report);
        }
        finally{if(prior.IsValid()&&prior.isLoaded)SceneManager.SetActiveScene(prior);if(!opened)EditorSceneManager.CloseScene(scene,true);}
    }
    static TMP_Text Text(Transform parent,string name,Vector2 position,Vector2 size,float fontSize)
    {
        var found=parent.Find(name);var go=found!=null?found.gameObject:new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var text=go.GetComponent<TextMeshProUGUI>()??go.AddComponent<TextMeshProUGUI>();var rect=text.rectTransform;
        rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);rect.anchoredPosition=position;rect.sizeDelta=size;
        text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/UI/Fonts/Inventory_Vietnamese.asset");text.fontSize=fontSize;text.color=Color.white;text.alignment=TextAlignmentOptions.TopLeft;text.textWrappingMode=TextWrappingModes.Normal;text.raycastTarget=false;return text;
    }
    static void Install(Scene scene)
    {
        var canvas=Components<Canvas>(scene).Single(c=>c.name=="Canvas_Gameplay").transform;
        foreach(var legacy in Components<QuestTracker>(scene)){legacy.enabled=false;var text=Ref<TMP_Text>(legacy,"objectiveText");if(text!=null)text.gameObject.SetActive(false);EditorUtility.SetDirty(legacy);}
        var old=canvas.Find("PNL_Quest");var root=old!=null?old.gameObject:new GameObject("PNL_Quest",typeof(RectTransform));root.transform.SetParent(canvas,false);var rect=root.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=Vector2.one;rect.pivot=Vector2.one;rect.anchoredPosition=new Vector2(-30,-30);rect.sizeDelta=new Vector2(480,210);
        var image=root.GetComponent<Image>()??root.AddComponent<Image>();image.color=new Color(0,0,0,.55f);image.raycastTarget=false;
        var objective=Text(root.transform,"TXT_Objective",new Vector2(18,-14),new Vector2(444,125),24);objective.text="MỤC TIÊU\n"+ChapterObjectiveRules.Description(ChapterObjective.FindFragmentA);
        var progress=Text(root.transform,"TXT_QuestProgress",new Vector2(18,-148),new Vector2(444,44),20);progress.text="Mảnh trống: 0/3  |  Câu đố: 0/2";
        var ui=canvas.Find("QuestUI");var hudObject=ui!=null?ui.gameObject:new GameObject("QuestUI");hudObject.transform.SetParent(canvas,false);hudObject.SetActive(true);var hud=hudObject.GetComponent<ChapterQuestHUD>()??hudObject.AddComponent<ChapterQuestHUD>();hud.enabled=true;
        Set(hud,"worldState",Components<WorldStateStore>(scene).Single());Set(hud,"objectiveText",objective);Set(hud,"progressText",progress);Set(hud,"checklistText",null);root.SetActive(true);root.transform.SetSiblingIndex(4);hudObject.transform.SetSiblingIndex(5);
    }
    static void Validate(Scene scene,Report report)
    {
        var hud=Components<ChapterQuestHUD>(scene).Single();var objective=Ref<TMP_Text>(hud,"objectiveText");var progress=Ref<TMP_Text>(hud,"progressText");var root=objective.transform.parent.GetComponent<RectTransform>();
        if(hud.name!="QuestUI"||!hud.enabled||!hud.gameObject.activeSelf||Ref<WorldStateStore>(hud,"worldState")!=Components<WorldStateStore>(scene).Single()||Ref<TMP_Text>(hud,"checklistText")!=null)throw new InvalidOperationException("QuestUI references/state invalid");
        if(root.name!="PNL_Quest"||root.anchorMin!=Vector2.one||root.anchorMax!=Vector2.one||root.pivot!=Vector2.one||root.anchoredPosition!=new Vector2(-30,-30)||root.sizeDelta!=new Vector2(480,210)||root.GetComponent<Image>().raycastTarget||Mathf.Abs(root.GetComponent<Image>().color.a-.55f)>.001f)throw new InvalidOperationException("Quest panel geometry/image invalid");
        var texts=new[]{objective,progress};var positions=new[]{new Vector2(18,-14),new Vector2(18,-148)};var sizes=new[]{new Vector2(444,125),new Vector2(444,44)};
        for(int i=0;i<2;i++)if(texts[i].rectTransform.anchorMin!=new Vector2(0,1)||texts[i].rectTransform.anchorMax!=new Vector2(0,1)||texts[i].rectTransform.pivot!=new Vector2(0,1)||texts[i].rectTransform.anchoredPosition!=positions[i]||texts[i].rectTransform.sizeDelta!=sizes[i]||texts[i].fontSize!=(i==0?24:20)||texts[i].raycastTarget||texts[i].alignment!=TextAlignmentOptions.TopLeft)throw new InvalidOperationException("Quest text geometry invalid");
        foreach(var legacy in Components<QuestTracker>(scene))if(legacy.enabled||Ref<TMP_Text>(legacy,"objectiveText").gameObject.activeSelf)throw new InvalidOperationException("Legacy objective HUD still active");
        string chars="MỤC TIÊU HOÀN THÀNH Mảnh trống Câu đố"+string.Join(" ",Enum.GetValues(typeof(ChapterObjective)).Cast<ChapterObjective>().Select(ChapterObjectiveRules.Description));
        if(!objective.font.HasCharacters(chars,out uint[] missing))throw new InvalidOperationException("Quest font lacks Vietnamese glyphs");
        report.checks.Add("Active QuestUI observes the scene WorldState; optional checklist unset; legacy QuestTracker and text disabled.");report.checks.Add("Panel/text anchors, dimensions, fonts, alpha and raycast settings match the requested HUD.");
    }
    static void Preview(Scene source,Report report)
    {
        var sourcePanel=Components<ChapterQuestHUD>(source).Single();var original=Ref<TMP_Text>(sourcePanel,"objectiveText").transform.parent.gameObject;
        Scene prior=SceneManager.GetActiveScene();var temporary=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);var priorTarget=RenderTexture.active;
        try
        {
            SceneManager.SetActiveScene(temporary);var camera=new GameObject("QuestPreviewCamera").AddComponent<Camera>();camera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.2f,.26f,.22f);camera.cullingMask=1<<30;
            foreach(var size in new[]{new Vector2Int(1920,1080),new Vector2Int(1280,720),new Vector2Int(1600,1000)})
            {
                var target=new RenderTexture(size.x,size.y,24,RenderTextureFormat.ARGB32);target.Create();camera.targetTexture=target;
                var canvasObject=new GameObject("QuestPreviewCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;
                var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
                var clone=UnityEngine.Object.Instantiate(original,canvas.transform,false);foreach(var t in canvasObject.GetComponentsInChildren<Transform>(true))t.gameObject.layer=30;Canvas.ForceUpdateCanvases();
                foreach(var text in clone.GetComponentsInChildren<TMP_Text>()){text.ForceMeshUpdate(true,true);if(text.preferredHeight>text.rectTransform.rect.height+1)throw new InvalidOperationException("Preview text overflows: "+size+" "+text.name);}
                Canvas.ForceUpdateCanvases();UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(camera,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});RenderTexture.active=target;var image=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
                string path="Logs/QuestHUD_"+size.x+"x"+size.y+".png";File.WriteAllBytes(path,image.EncodeToPNG());report.checks.Add("Quest panel rendered without text overflow at "+size+": "+path);
                UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(canvasObject);camera.targetTexture=null;RenderTexture.active=priorTarget;target.Release();UnityEngine.Object.DestroyImmediate(target);
            }
        }
        finally{RenderTexture.active=priorTarget;if(prior.IsValid()&&prior.isLoaded)SceneManager.SetActiveScene(prior);EditorSceneManager.CloseScene(temporary,true);}
    }
}
