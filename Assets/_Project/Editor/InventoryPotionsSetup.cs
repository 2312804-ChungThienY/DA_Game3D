using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public static class InventoryPotionsSetup
{
    const string RequestPath="Library/SonDoongInventorySetup.request.json";
    const string ScenePath="Assets/_Project/Scenes/02_HangLinhMach.unity";
    const string ControlsPath="Assets/_Project/Input/PlayerControls.inputactions";
    static double next;
    static bool running;
    [Serializable] public class Request { public string action; }
    [Serializable] public class Report { public string action;public List<string> checks=new List<string>();public List<string> failures=new List<string>(); }
    static InventoryPotionsSetup(){EditorApplication.update+=Poll;}
    static void Poll()
    {
        if(running||EditorApplication.timeSinceStartup<next||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(RequestPath))return;
        next=EditorApplication.timeSinceStartup+.5;running=true;var report=new Report();
        try
        {
            report.action=JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath)).action;
            if(EditorApplication.isPlaying){EditorApplication.ExitPlaymode();throw new InvalidOperationException("Exited Play Mode; retry after Editor is ready.");}
            if(report.action=="build")Build(report);
            else if(report.action=="play-test")
            {
                UnityEditor.SessionState.SetBool("SonDoong.InventoryTest",true);
                EditorSceneManager.OpenScene("Assets/_Project/Scenes/00_Boot.unity");
                EditorApplication.EnterPlaymode();report.checks.Add("Requested Play Mode test from Boot.");
            }
            else if(report.action=="open-main")
            {
                if(SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Open scene has unsaved changes.");
                EditorSceneManager.OpenScene(ScenePath);report.checks.Add("Opened main scene in Edit Mode.");
            }
            else WithScene(report);
        }
        catch(Exception e){report.failures.Add(e.ToString());}
        finally{Directory.CreateDirectory("Logs");File.WriteAllText("Logs/InventoryPotionsSetup.json",JsonUtility.ToJson(report,true));File.Delete(RequestPath);running=false;}
    }
    static IEnumerable<T> Components<T>(Scene scene) where T:Component =>scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<T>(true));
    static void Set(UnityEngine.Object obj,string field,UnityEngine.Object value){var so=new SerializedObject(obj);so.FindProperty(field).objectReferenceValue=value;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(obj);}
    static T Field<T>(UnityEngine.Object obj,string name) where T:UnityEngine.Object =>new SerializedObject(obj).FindProperty(name).objectReferenceValue as T;
    static void Folder(string path){if(AssetDatabase.IsValidFolder(path))return;Folder(Path.GetDirectoryName(path).Replace('\\','/'));AssetDatabase.CreateFolder(Path.GetDirectoryName(path).Replace('\\','/'),Path.GetFileName(path));}
    static void WithScene(Report report)
    {
        var previous=SceneManager.GetActiveScene();var scene=SceneManager.GetSceneByPath(ScenePath);bool opened=scene.IsValid()&&scene.isLoaded;
        if(!opened)scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
        try
        {
            if(report.action=="install")
            {
                if(scene.isDirty)throw new InvalidOperationException("Scene has unsaved edits. Save it before installation.");
                SceneManager.SetActiveScene(scene);Install(scene,report);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
            }
            else if(report.action=="validate")Validate(scene,report);
            else
            {
                report.checks.Add("playing="+EditorApplication.isPlaying+", dirty="+scene.isDirty);
                var input=Components<PlayerInput>(scene).Single();report.checks.Add("An="+input.name+" pos="+input.transform.position+" actions="+AssetDatabase.GetAssetPath(input.actions)+" map="+input.defaultActionMap);
                foreach(var text in Components<TMP_Text>(scene))report.checks.Add(text.name+": "+text.rectTransform.anchorMin+" / "+text.rectTransform.anchoredPosition+" / "+text.rectTransform.sizeDelta);
                foreach(var transform in Components<Transform>(scene).Where(t=>t.name.Contains("Camp")||t.name.Contains("Lantern")||t.name.Contains("Spawn")))report.checks.Add(transform.name+" at "+transform.position);
            }
        }
        finally{if(previous.IsValid()&&previous.isLoaded)SceneManager.SetActiveScene(previous);if(!opened)EditorSceneManager.CloseScene(scene,true);}
    }
    static ItemDefinition Item(string name,string id,string display)
    {
        Folder("Assets/_Project/Data/Items");string path="Assets/_Project/Data/Items/"+name+".asset";
        var item=AssetDatabase.LoadAssetAtPath<ItemDefinition>(path);if(item==null){item=ScriptableObject.CreateInstance<ItemDefinition>();AssetDatabase.CreateAsset(item,path);}
        var so=new SerializedObject(item);so.FindProperty("itemId").stringValue=id;so.FindProperty("displayName").stringValue=display;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(item);return item;
    }
    static TMP_FontAsset Font()
    {
        const string path="Assets/_Project/UI/Fonts/Inventory_Vietnamese.asset";
        var font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);if(font!=null)return font;
        var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/A31 - Noto Sans/static/NotoSans-Regular.ttf");
        font=TMP_FontAsset.CreateFontAsset(source,36,6,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024,AtlasPopulationMode.Dynamic,true);
        font.name="Inventory_Vietnamese";
        string chars=new string(Enumerable.Range(32,95).Concat(Enumerable.Range(160,96)).Concat(Enumerable.Range(0x1EA0,90)).Select(c=>(char)c).ToArray())+"ĂăĐđĨĩŨũƠơƯư";
        if(!font.TryAddCharacters(chars,out string missing))throw new InvalidOperationException("Missing font glyphs: "+missing);
        font.atlasPopulationMode=AtlasPopulationMode.Static;
        AssetDatabase.CreateAsset(font,path);AssetDatabase.AddObjectToAsset(font.material,font);
        foreach(var texture in font.atlasTextures){AssetDatabase.AddObjectToAsset(texture,font);EditorUtility.SetDirty(texture);}
        var data=new SerializedObject(font);data.FindProperty("m_ClearDynamicDataOnBuild").boolValue=false;data.FindProperty("m_SourceFontFilePath").stringValue="";data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(font);EditorUtility.SetDirty(font.material);return font;
    }
    static TMP_Text Text(Transform parent,string name,Vector2 position,Vector2 size,TMP_FontAsset font,string value)
    {
        var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);
        var text=go.AddComponent<TextMeshProUGUI>();var rect=text.rectTransform;rect.anchorMin=rect.anchorMax=Vector2.zero;rect.pivot=Vector2.zero;rect.anchoredPosition=position;rect.sizeDelta=size;
        text.font=font;text.fontSize=20;text.color=new Color(.94f,.97f,.88f);text.alignment=TextAlignmentOptions.TopLeft;text.raycastTarget=false;text.text=value;return text;
    }
    static void Install(Scene scene,Report report)
    {
        var input=Components<PlayerInput>(scene).Single();var an=input.gameObject;var world=Components<WorldStateStore>(scene).Single();var save=Components<SaveService>(scene).Single();
        var inventory=an.GetComponent<PlayerInventory>()??an.AddComponent<PlayerInventory>();var potion=an.GetComponent<PotionUseController>()??an.AddComponent<PotionUseController>();
        var config=new SerializedObject(potion);config.FindProperty("useCooldown").floatValue=2;config.ApplyModifiedPropertiesWithoutUndo();Set(save,"inventory",inventory);
        Folder("Assets/_Project/Input");
        if(!File.Exists(ControlsPath))AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(input.actions),ControlsPath);
        var controls=AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
        var map=controls.FindActionMap(input.defaultActionMap,true);var action=map.FindAction("UsePotion",false)??map.AddAction("UsePotion",InputActionType.Button);
        if(!action.bindings.Any(b=>b.path=="<Keyboard>/q"))action.AddBinding("<Keyboard>/q");
        File.WriteAllText(ControlsPath,controls.ToJson());AssetDatabase.ImportAsset(ControlsPath,ImportAssetOptions.ForceSynchronousImport);
        Set(input,"m_Actions",AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath));PrefabUtility.RecordPrefabInstancePropertyModifications(input);
        var items=new[]{Item("IT_LaCam",InventoryRules.Herb,"Lá cẩm"),Item("IT_LinhChi",InventoryRules.Mushroom,"Linh chi"),Item("IT_ThuocMau",InventoryRules.HealthPotion,"Thuốc máu")};
        string[] names={"Pickup_LaCam_01","Pickup_LinhChi_01","Pickup_ThuocMau_01"};string[] ids={"pickup_la_cam_001","pickup_linh_chi_001","pickup_thuoc_mau_001"};
        var gameplay=scene.GetRootGameObjects().Single(g=>g.name=="Gameplay").transform;
        var parent=gameplay.Find("InventoryPickups");if(parent==null){parent=new GameObject("InventoryPickups").transform;parent.SetParent(gameplay,false);}
        Color[] colors={new Color(.35f,.7f,.38f),new Color(.7f,.34f,.2f),new Color(.8f,.18f,.3f)};
        Folder("Assets/_Project/Materials/Inventory");
        for(int i=0;i<3;i++)
        {
            var found=parent.Find(names[i]);var go=found!=null?found.gameObject:GameObject.CreatePrimitive(i==2?PrimitiveType.Sphere:PrimitiveType.Cube);
            go.name=names[i];go.transform.SetParent(parent,false);go.transform.localPosition=new Vector3(-2.5f+i*1.1f,1.05f,-1.5f);go.transform.localScale=new Vector3(.45f,.45f,.45f);go.layer=LayerMask.NameToLayer("Interactable");go.GetComponent<Collider>().isTrigger=false;
            string matPath="Assets/_Project/Materials/Inventory/MAT_"+names[i]+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,matPath);}material.SetColor("_BaseColor",colors[i]);EditorUtility.SetDirty(material);go.GetComponent<Renderer>().sharedMaterial=material;
            var pedestal=parent.Find("Pedestal_"+i);
            var stand=pedestal!=null?pedestal.gameObject:GameObject.CreatePrimitive(PrimitiveType.Cube);
            stand.name="Pedestal_"+i;stand.transform.SetParent(parent,false);stand.transform.localPosition=new Vector3(-2.5f+i*1.1f,.4f,-1.5f);stand.transform.localScale=new Vector3(.8f,.8f,.8f);stand.layer=LayerMask.NameToLayer("Environment");
            stand.GetComponent<Renderer>().sharedMaterial=material;
            var pickup=go.GetComponent<WorldItemPickup>()??go.AddComponent<WorldItemPickup>();var so=new SerializedObject(pickup);so.FindProperty("pickupId").stringValue=ids[i];so.ApplyModifiedPropertiesWithoutUndo();Set(pickup,"item",items[i]);Set(pickup,"inventory",inventory);Set(pickup,"worldState",world);go.SetActive(true);
        }
        var canvas=Components<Canvas>(scene).First(c=>c.name=="Canvas_Gameplay").transform;var font=Font();
        foreach(string name in new[]{"TXT_Inventory","TXT_PotionCount","InventoryUI"}){var old=canvas.Find(name);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);}
        var inventoryText=Text(canvas,"TXT_Inventory",new Vector2(30,35),new Vector2(400,355),font,"TÚI ĐỒ (0/12)\n"+string.Join("\n",Enumerable.Range(1,12).Select(i=>i.ToString("00")+". --")));
        var potionText=Text(canvas,"TXT_PotionCount",new Vector2(30,400),new Vector2(400,35),font,"[Q] Thuốc: 0");
        var hudObject=new GameObject("InventoryUI");hudObject.transform.SetParent(canvas,false);var hud=hudObject.AddComponent<InventoryHUD>();Set(hud,"inventory",inventory);Set(hud,"inventoryText",inventoryText);Set(hud,"potionText",potionText);
        var hudData=new SerializedObject(hud);var definitions=hudData.FindProperty("definitions");definitions.arraySize=3;for(int i=0;i<3;i++)definitions.GetArrayElementAtIndex(i).objectReferenceValue=items[i];hudData.ApplyModifiedPropertiesWithoutUndo();
        // Keep HUD below existing modal panels in draw order.
        inventoryText.transform.SetSiblingIndex(0);potionText.transform.SetSiblingIndex(1);hudObject.transform.SetSiblingIndex(2);
        report.checks.Add("Installed inventory, Q action, item assets, pickups and HUD; existing save/gameplay references preserved.");Validate(scene,report);
    }
    static void Validate(Scene scene,Report report)
    {
        var input=Components<PlayerInput>(scene).Single();var inventory=Components<PlayerInventory>(scene).Single();var potion=Components<PotionUseController>(scene).Single();var world=Components<WorldStateStore>(scene).Single();var save=Components<SaveService>(scene).Single();
        if(inventory.gameObject!=input.gameObject||potion.gameObject!=input.gameObject||Field<PlayerInventory>(save,"inventory")!=inventory)throw new InvalidOperationException("An/SaveService inventory wiring invalid");
        var action=input.actions.FindActionMap(input.defaultActionMap,true).FindAction("UsePotion",true);
        if(action.type!=InputActionType.Button||!action.bindings.Any(b=>b.path=="<Keyboard>/q"&&string.IsNullOrEmpty(b.interactions))||new SerializedObject(potion).FindProperty("useCooldown").floatValue!=2)throw new InvalidOperationException("Q binding/cooldown invalid");
        var pickups=Components<WorldItemPickup>(scene).ToArray();var ids=new HashSet<string>();
        if(pickups.Length<3)throw new InvalidOperationException("Missing initial pickups");
        foreach(var pickup in pickups)
        {
            if(!pickup.IsConfigured||pickup.Inventory!=inventory||pickup.WorldState!=world||!ids.Add(pickup.PickupId)||!pickup.gameObject.activeSelf||pickup.gameObject.layer!=LayerMask.NameToLayer("Interactable")||pickup.GetComponent<Collider>().isTrigger||pickup.GetComponents<MonoBehaviour>().OfType<IInteractable>().Count()!=1)throw new InvalidOperationException("Pickup invalid: "+pickup.name);
        }
        foreach(string id in new[]{"pickup_la_cam_001","pickup_linh_chi_001","pickup_thuoc_mau_001"})if(!ids.Contains(id))throw new InvalidOperationException("Missing initial pickup: "+id);
        var hud=Components<InventoryHUD>(scene).Single();if(Field<PlayerInventory>(hud,"inventory")!=inventory||Field<TMP_Text>(hud,"inventoryText")==null||Field<TMP_Text>(hud,"potionText")==null)throw new InvalidOperationException("HUD wiring invalid");
        if(new SaveData().version!=5||!GameSaveStorage.IsValid(new SaveData()))throw new InvalidOperationException("Initial v5 snapshot invalid");
        report.checks.Add("Validated An + SaveService, Button Q/2s cooldown, three unique active Interactable pickups, HUD references and initial v5 snapshot.");
        foreach(var item in AssetDatabase.FindAssets("t:ItemDefinition",new[]{"Assets/_Project/Data/Items"}).Select(g=>AssetDatabase.LoadAssetAtPath<ItemDefinition>(AssetDatabase.GUIDToAssetPath(g))))report.checks.Add(item.name+": "+item.ItemId+", stack="+item.MaxStack);
    }
    static void Build(Report result)
    {
        var validation=new Report{action="validate"};WithScene(validation);result.checks.AddRange(validation.checks);Directory.CreateDirectory("Builds/InventoryPotionsValidation");var preloads=PlayerSettings.GetPreloadedAssets();UnityEditor.Build.Reporting.BuildReport report;
        try{report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName="Builds/InventoryPotionsValidation/SonDoong.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});}
        finally{PlayerSettings.SetPreloadedAssets(preloads);AssetDatabase.SaveAssets();}
        foreach(var step in report.steps)foreach(var message in step.messages)if(message.type==LogType.Error||message.type==LogType.Exception)result.failures.Add(message.content);
        result.checks.Add("Windows build: "+report.summary.result+", errors="+report.summary.totalErrors+", warnings="+report.summary.totalWarnings);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Build failed");
    }
}
