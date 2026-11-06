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
public static class Chapter02ArenaSetup
{
    const string Request="Library/SonDoongC2Arena.request.json";
    const string Art="Assets/_Project/Art/Chapter02Arena";
    [Serializable]class Command{public string action;}
    [Serializable]class Report{public string action;public List<string> checks=new List<string>(),failures=new List<string>(),notes=new List<string>();}
    static bool busy;
    static Chapter02ArenaSetup()=>EditorApplication.update+=Poll;
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
            {SessionState.SetBool("SonDoong.C2ArenaTest",true);SessionState.SetBool("SonDoong.C2ArenaInputOnly",r.action=="input-test");EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();EditorApplication.isPaused=false;EditorApplication.EnterPlaymode();}
            else
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();
                try{var s=EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);if(r.action=="apply"){Apply(s);foreach(var c in All<Component>(s))if(c&&PrefabUtility.IsPartOfPrefabInstance(c))PrefabUtility.RecordPrefabInstancePropertyModifications(c);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}Inspect(s,r);}
                finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
        }catch(Exception e){r.failures.Add(e.ToString());}
        finally{File.WriteAllText("Logs/C2Arena-"+r.action+".json",JsonUtility.ToJson(r,true));File.Delete(Request);busy=false;}
    }
    static Transform Group(string name,Transform parent,Vector3 local)
    {var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=local;t.gameObject.layer=2;return t;}
    static Material Mat(string name,Color color)
    {string path=Art+"/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Smoothness",.2f);m.SetFloat("_Cull",0);EditorUtility.SetDirty(m);return m;}
    static Mesh BodyMesh()
    {
        string path=Art+"/Bird_Octa.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh)return mesh;
        var axes=new[]{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back};int[] index={0,4,3,0,3,5,0,5,2,0,2,4,1,3,4,1,5,3,1,2,5,1,4,2};
        mesh=new Mesh{name="Bird_Octa",vertices=index.Select(i=>axes[i]).ToArray(),triangles=Enumerable.Range(0,24).ToArray()};mesh.RecalculateNormals();mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,path);return mesh;
    }
    static Transform Shape(string name,Transform p,Vector3 pos,Vector3 size,Material mat,Mesh mesh)
    {var g=Group(name,p,pos);g.localScale=size;g.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;g.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;return g;}
    static RectTransform Rect(Transform t,Transform parent,Vector2 anchor,Vector2 pos,Vector2 size)
    {var r=(RectTransform)t;r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.pivot=new Vector2(.5f,1);r.anchoredPosition=pos;r.sizeDelta=size;r.localScale=Vector3.one;return r;}
    static GameObject Panel(string name,Transform p,Vector2 pos,Vector2 size,Color color)
    {var g=new GameObject(name,typeof(RectTransform),typeof(Image));Rect(g.transform,p,new Vector2(.5f,1),pos,size);g.GetComponent<Image>().color=color;return g;}
    static TMP_Text Text(string name,Transform p,string text,Vector2 pos,Vector2 size,int fontSize)
    {var g=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));Rect(g.transform,p,new Vector2(.5f,1),pos,size);var t=g.GetComponent<TMP_Text>();t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BuilderUtil.FontPath);t.text=text;t.fontSize=fontSize;t.color=new Color(.94f,.94f,.87f);t.alignment=TextAlignmentOptions.Center;t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;return t;}
    static Button Button(string name,Transform p,string text,Vector2 pos,Action<Button> callback)
    {var g=Panel(name,p,pos,new Vector2(310,55),new Color(.08f,.26f,.29f));var b=g.AddComponent<Button>();b.targetGraphic=g.GetComponent<Image>();Text("Label",g.transform,text,new Vector2(0,-12),new Vector2(295,35),22);callback(b);return b;}
    static void Apply(Scene s)
    {
        Chapter01RevisionSetup.Folder(Art);var boss=All<BossChimLac>(s).Single();var focus=Ref<ChimLacFocusReceiver>(boss,"focus");var gate=All<ArenaGate>(s).Single();BuilderUtil.Set(gate,"requiredDoorId","c2_door_arena");
        var capsule=boss.GetComponent<CapsuleCollider>();capsule.center=new Vector3(0,1.2f,0);capsule.height=2.4f;capsule.radius=.9f;
        foreach(string name in new[]{"Body","Wing_L","Wing_R","Beak"}){var t=boss.transform.Find(name);if(t&&t.GetComponent<Renderer>())t.GetComponent<Renderer>().enabled=false;}
        var old=boss.transform.Find("VIS_ChimLac_Combat");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);var model=Group("VIS_ChimLac_Combat",boss.transform,Vector3.zero);
        var teal=Mat("Bird_Teal",new Color(.09f,.34f,.37f));var bronze=Mat("Bird_Bronze",new Color(.70f,.50f,.21f));var pale=Mat("Bird_Ivory",new Color(.83f,.81f,.65f));var mesh=BodyMesh();
        Shape("Body",model,new Vector3(0,1,0),new Vector3(1,.7f,1.35f),teal,mesh);var neck=Group("Neck",model,new Vector3(0,1.1f,.75f));Shape("Neck_Length",neck,new Vector3(0,.45f,.1f),new Vector3(.22f,.65f,.23f),teal,mesh);Shape("Head",neck,new Vector3(0,1,.25f),new Vector3(.32f,.3f,.4f),pale,mesh);Shape("Beak",neck,new Vector3(0,.95f,.75f),new Vector3(.14f,.14f,.65f),bronze,mesh);
        var wings=new[]{Group("Left_Wing",model,new Vector3(-.65f,1.1f,0)),Group("Right_Wing",model,new Vector3(.65f,1.1f,0))};var feather=AssetDatabase.LoadAssetAtPath<Mesh>(Chapter02OpeningSetup.Art+"/Feather_LowPoly.asset");
        for(int side=0;side<2;side++)for(int i=0;i<5;i++){float sign=side==0?-1:1;var f=Shape("Flight_Feather_"+i,wings[side],new Vector3(sign*(.4f+i*.3f),-.03f,-.1f-i*.16f),new Vector3(.75f,1.8f-i*.1f,.7f),i%2==0?teal:bronze,feather);f.localRotation=Quaternion.Euler(90,sign*18,0);}
        for(int i=0;i<3;i++){var f=Shape("Tail_"+i,model,new Vector3((i-1)*.3f,.8f,-1.3f),new Vector3(.65f,2,.6f),bronze,feather);f.localRotation=Quaternion.Euler(90,0,(i-1)*12);}
        var stars=Group("Stun_Stars",model,new Vector3(0,2.6f,0));for(int i=0;i<3;i++){var star=Shape("Star_"+i,stars,new Vector3(Mathf.Cos(i*2.094f)*.6f,0,Mathf.Sin(i*2.094f)*.6f),Vector3.one*1.3f,AssetDatabase.LoadAssetAtPath<Material>(Chapter01CompletionArt.Art+"/Materials/Stun_Stars.mat"),AssetDatabase.LoadAssetAtPath<Mesh>(Chapter01CompletionArt.Art+"/Meshes/Stun_Star.asset"));star.gameObject.AddComponent<FaceCameraBillboard>();}stars.gameObject.SetActive(false);
        var presentation=model.gameObject.AddComponent<ChimLacPresentation>();BuilderUtil.Set(presentation,"boss",boss);BuilderUtil.Set(presentation,"leftWing",wings[0]);BuilderUtil.Set(presentation,"rightWing",wings[1]);BuilderUtil.Set(presentation,"neck",neck);BuilderUtil.Set(presentation,"stars",stars);
        var world=All<Transform>(s).Single(t=>t.name=="World_Geometry");var prior=world.Find("ART_C2_Arena");if(prior)UnityEngine.Object.DestroyImmediate(prior.gameObject);var art=Group("ART_C2_Arena",world,Vector3.zero);
        var marker=GameObject.CreatePrimitive(PrimitiveType.Cylinder);marker.name="Dive_Landing_Marker";marker.layer=2;marker.transform.SetParent(art,false);marker.transform.localScale=new Vector3(2.4f,.012f,2.4f);UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());marker.GetComponent<Renderer>().sharedMaterial=Mat("Dive_Warning",new Color(.8f,.12f,.08f));marker.SetActive(false);BuilderUtil.Set(boss,"diveLandingMarker",marker.transform);
        var canvas=All<Canvas>(s).Single(c=>c.name=="Canvas_Gameplay");var bar=Ref<GameObject>(boss,"bossBar");Rect(bar.transform,canvas.transform,new Vector2(.5f,1),new Vector2(0,-30),new Vector2(760,65));bar.GetComponent<Image>().color=new Color(.025f,.07f,.085f,.95f);var fill=Ref<RectTransform>(boss,"bossBarFill");fill.anchorMin=Vector2.zero;fill.anchorMax=Vector2.one;fill.offsetMin=Vector2.zero;fill.offsetMax=Vector2.zero;fill.GetComponent<Image>().color=new Color(.12f,.37f,.43f);var label=Ref<TMP_Text>(boss,"bossBarLabel");Rect(label.transform,bar.transform,new Vector2(.5f,1),new Vector2(0,-16),new Vector2(740,40));label.fontSize=25;label.color=Color.white;
        var oldHud=canvas.transform.Find("C2_Arena_HUD");if(oldHud)UnityEngine.Object.DestroyImmediate(oldHud.gameObject);var root=new GameObject("C2_Arena_HUD",typeof(RectTransform));Rect(root.transform,canvas.transform,new Vector2(.5f,1),Vector2.zero,new Vector2(1100,450));var hud=root.AddComponent<ChimLacArenaHUD>();BuilderUtil.Set(hud,"boss",boss);BuilderUtil.Set(hud,"focus",focus);
        var rootRect=(RectTransform)root.transform;rootRect.anchorMin=Vector2.zero;rootRect.anchorMax=Vector2.one;rootRect.offsetMin=rootRect.offsetMax=Vector2.zero;
        var hint=Panel("Combat_Hint_Backdrop",root.transform,new Vector2(0,205),new Vector2(1100,65),new Color(.025f,.065f,.08f,.94f));var hintRect=(RectTransform)hint.transform;hintRect.anchorMin=hintRect.anchorMax=new Vector2(.5f,0);hint.GetComponent<Image>().raycastTarget=false;BuilderUtil.Set(hud,"hintGroup",hint);BuilderUtil.Set(hud,"stateText",Text("Combat_Hint",hint.transform,"",new Vector2(0,-7),new Vector2(1050,57),22));hint.SetActive(false);
        var fg=Panel("Focus_Progress",root.transform,new Vector2(0,-178),new Vector2(660,36),new Color(.02f,.07f,.08f,.92f));var progress=Panel("Fill",fg.transform,Vector2.zero,new Vector2(660,36),new Color(.10f,.35f,.40f));var pr=(RectTransform)progress.transform;pr.anchorMin=Vector2.zero;pr.anchorMax=Vector2.one;pr.offsetMin=pr.offsetMax=Vector2.zero;BuilderUtil.Set(hud,"focusGroup",fg);BuilderUtil.Set(hud,"focusFill",pr);BuilderUtil.Set(hud,"focusText",Text("Label",fg.transform,"",new Vector2(0,-5),new Vector2(650,30),20));
        var retry=Panel("Victory_Save_Retry",root.transform,new Vector2(0,-300),new Vector2(790,270),new Color(.025f,.065f,.08f,.99f));BuilderUtil.Set(hud,"retryPanel",retry);Text("Title",retry.transform,"CHIM LẠC ĐÃ ĐƯỢC GIẢI THOÁT",new Vector2(0,-22),new Vector2(740,45),27);Text("Body",retry.transform,"Chưa lưu được chiến thắng. Hãy thử lưu lại trước khi thoát. Bạn vẫn có thể trở lại trò chơi và dùng bệ để lưu.",new Vector2(0,-85),new Vector2(730,95),23);
        BuilderUtil.Set(hud,"retryButton",Button("Retry",retry.transform,"Thử lưu lại",new Vector2(-175,-190),b=>UnityEventTools.AddPersistentListener(b.onClick,hud.Retry)));Button("Dismiss",retry.transform,"Trở lại trò chơi",new Vector2(175,-190),b=>UnityEventTools.AddPersistentListener(b.onClick,hud.CloseFromBack));retry.SetActive(false);
        var exclusion=Type.GetType("UnityEngine.Rendering.DisallowGPUDrivenRendering, Unity.RenderPipelines.GPUDriven.Runtime",true);foreach(var r in s.GetRootGameObjects().Where(g=>g.GetComponentsInChildren<MeshRenderer>(true).Length>0)){var c=r.GetComponent(exclusion)??r.AddComponent(exclusion);exclusion.GetProperty("applyToChildrenRecursively").SetValue(c,true);}
    }
    static void Inspect(Scene s,Report r)
    {
        Action<bool,string> check=(ok,label)=>{if(ok)r.checks.Add(label);else r.failures.Add(label);};var boss=All<BossChimLac>(s).Single();
        check(All<Transform>(s).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"No missing scripts");check(Ref<EnemyDefinition>(boss.GetComponent<EnemyHealth>(),"definition").maxHealth==200,"Chim Lac retains 200 HP");
        check(boss.transform.Find("VIS_ChimLac_Combat")&&boss.GetComponentInChildren<ChimLacPresentation>(true),"Project bird visual follows FSM without extra motor");check(!boss.GetComponent<UnityEngine.AI.NavMeshAgent>()&&!boss.GetComponent<Animator>(),"Boss root movement remains owned by BossChimLac");
        check(Ref<Transform>(boss,"diveLandingMarker")&&!Ref<Transform>(boss,"diveLandingMarker").GetComponent<Collider>(),"Dive landing warning without collider");check(All<ChimLacArenaHUD>(s).Length==1,"One arena HUD and victory-save retry owner");
        check(new SerializedObject(All<ArenaGate>(s).Single()).FindProperty("requiredDoorId").stringValue=="c2_door_arena","Arena trigger requires opened progression door");
        check(All<WindColumn>(s).Single(w=>w.name=="WindColumn_Arena").HoverTopY==24.5f,"Arena wind hover remains 24.5m");
        r.notes.Add("Boss: "+JsonUtility.ToJson(boss));r.notes.Add("Gate: "+JsonUtility.ToJson(All<ArenaGate>(s).Single()));r.notes.Add("Combat proxy only; full C2 environment art remains stage7.");
    }
}
