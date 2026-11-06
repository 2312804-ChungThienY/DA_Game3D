using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Chapter02KnowledgeSetup
{
    const string Request="Library/SonDoongC2Knowledge.request.json";
    const string Museum2017="https://baotanglichsu.vn/VI/Articles/1001/19707/bao-vat-quoc-gia-viet-nam-trong-djong-ngoc-lu.html";
    const string Museum2013="https://baotanglichsu.vn/vi/Articles/1001/13602/gioi-thieu-bao-vat-quoc-gia-1-trong-djong-ngoc-lu-hien-vat-tieu-bieu-nhat-cua-nen-van-hoa-djong-son.html";
    const string Oxalis="https://oxalisadventure.com/cave/son-doong-cave/";
    const string Museum2021="https://baotanglichsu.vn/DataFiles/2021/10/News/Ti%E1%BA%BFng%20Vi%E1%BB%87t/12.10.2021/Thong%20bao%20khoa%20hoc/Net%20dac%20sac%20cua%20nhom%20Bao%20vat%20Quoc%20gia...%20Nguyen%20Van%20Doan.pdf";
    [Serializable] class Command{public string action;}
    [Serializable] class Report{public string action;public List<string> checks=new List<string>(),failures=new List<string>(),notes=new List<string>();}
    static bool busy;
    static Chapter02KnowledgeSetup()=>EditorApplication.update+=Poll;
    static T[] All<T>(Scene s)where T:Component=>s.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static T Ref<T>(UnityEngine.Object o,string f)where T:UnityEngine.Object=>Chapter01RevisionSetup.Ref<T>(o,f);
    static void Guard(){if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode first.");for(int i=0;i<SceneManager.sceneCount;i++)if(SceneManager.GetSceneAt(i).isDirty)throw new InvalidOperationException("Unsaved user scene: "+SceneManager.GetSceneAt(i).path);}
    static void Poll()
    {
        if(busy||EditorApplication.isCompiling||EditorApplication.isUpdating||!File.Exists(Request))return;busy=true;var r=new Report();
        try
        {
            r.action=JsonUtility.FromJson<Command>(File.ReadAllText(Request)).action;Guard();
            if(r.action=="apply"||r.action=="validate"||r.action=="inspect")
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();
                try
                {
                    var s=EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);
                    if(r.action=="apply"){Content();Apply(s);foreach(var c in All<Component>(s))if(c&&PrefabUtility.IsPartOfPrefabInstance(c))PrefabUtility.RecordPrefabInstancePropertyModifications(c);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}
                    ValidateContent(r.checks.Add,r.failures.Add);
                    foreach(var lore in All<LoreStele>(s))r.notes.Add("LORE "+Ref<LoreDefinition>(lore,"lore").loreId+" at "+lore.transform.position+" | "+JsonUtility.ToJson(Ref<LoreDefinition>(lore,"lore")));
                    foreach(var quiz in All<QuizPedestal>(s))r.notes.Add("PEDESTAL "+quiz.name+" | "+JsonUtility.ToJson(quiz));
                    if(r.action!="inspect")
                    {
                        if(All<PuzzleCluePickup>(s).Count(p=>p.ClueId==WorldIds.ClueBellsC2)!=1)r.failures.Add("Bell paper missing/duplicated");else r.checks.Add("One optional bell clue paper");
                        var panel=All<QuizPanel>(s).Single();if(!Ref<TMP_Text>(panel,"explanationText").GetComponentInParent<ScrollRect>(true))r.failures.Add("Explanation scroll missing");else r.checks.Add("Quiz explanation scroll and separate buttons configured");
                        if(All<Transform>(s).Count(t=>t.name=="C2_Bell_Glyph")!=3)r.failures.Add("Three bell glyphs required");else r.checks.Add("Three matching bell glyphs");
                        ValidateScene(s,r);
                        if(r.action=="validate")ValidateBuildGate(r);
                    }
                }
                finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
            else if(r.action=="test")
            {SessionState.SetBool("SonDoong.C2KnowledgeTest",true);EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();EditorApplication.isPaused=false;EditorApplication.EnterPlaymode();}
            else throw new InvalidOperationException(r.action);
        }
        catch(Exception e){r.failures.Add(e.ToString());}
        finally{Directory.CreateDirectory("Logs");File.WriteAllText("Logs/C2Knowledge-"+r.action+".json",JsonUtility.ToJson(r,true));File.Delete(Request);busy=false;}
    }
    static LoreDefinition Lore(int n)=>AssetDatabase.LoadAssetAtPath<LoreDefinition>("Assets/_Project/Data/Knowledge/LR_c2_lore_p"+n+".asset");
    static QuizDefinition Quiz(string id)=>AssetDatabase.LoadAssetAtPath<QuizDefinition>("Assets/_Project/Data/Knowledge/QZ_"+id+".asset");
    static void Content()
    {
        string[] bodies={
            "Ở những nơi trần hang Sơn Đoòng sụt mở, nắng có thể xuống tới bên trong. Ánh sáng cùng độ ẩm giúp cây cối phát triển, tạo nên vùng rừng trong lòng hang.",
            "Trên mặt trống Ngọc Lũ, họa tiết ở tâm có dạng ngôi sao nhiều tia. Bảo tàng Lịch sử Quốc gia giới thiệu hình này như biểu tượng mặt trời.",
            "Hoa văn trống Ngọc Lũ có cảnh nhà sàn với mái uốn cong. Hình nhà cùng các sinh hoạt quanh nó là tư liệu để tìm hiểu cư dân Đông Sơn. Nhà sàn đá treo trong màn chơi là thiết kế hư cấu.",
            "Trên trống Ngọc Lũ có những cảnh người dùng chày giã gạo, người múa với trang phục lông chim và hình thuyền. Các cảnh này gợi những mặt sinh hoạt của cư dân xưa.",
            "Khi mô tả các vành hoa văn trống Ngọc Lũ, Bảo tàng Lịch sử Quốc gia dùng tên chim Lạc cho một số hình tượng chim. Đây là tên của hình tượng trang trí, không phải bằng chứng xác định một loài chim hiện đại. Linh Thần Chim Lạc của game là hư cấu."
        };
        for(int i=1;i<=5;i++){var l=Lore(i);l.body=bodies[i-1];l.sourceRef=(i==1?Oxalis:i==2?Museum2021:i==3||i==4?Museum2013:Museum2017)+" | đối chiếu 05/11/2026";l.verified=true;EditorUtility.SetDirty(l);}
        var p1=Quiz("c2_qz_p1");p1.question="Điều gì giúp cây nhận ánh sáng ở bên trong Sơn Đoòng?";p1.explanation="Các khoảng trần hang sụt mở đưa ánh sáng vào, giúp cây phát triển cùng điều kiện ẩm.";
        var p2=Quiz("c2_qz_p2");p2.question="Họa tiết ở tâm mặt trống Ngọc Lũ có dạng gì?";p2.explanation="Đó là hình nhiều tia như ngôi sao; bảo tàng giới thiệu hình tượng mặt trời.";
        var p3=Quiz("c2_qz_p3");p3.question="Kiểu nhà nào xuất hiện trong hoa văn trống Ngọc Lũ?";p3.explanation="Bia mô tả nhà sàn với phần mái uốn cong trên hoa văn trống.";
        var p4=Quiz("c2_qz_p4");p4.question="Sinh hoạt nào được thể hiện trong hoa văn trống Ngọc Lũ?";p4.explanation="Cảnh dùng chày giã gạo là một trong những cảnh sinh hoạt trên trống.";
        var a=Quiz("c2_qz_lt1");a.question="Hình nhiều tia ở tâm trống trong bia được giới thiệu như biểu tượng gì?";a.explanation="Bia nhắc cách bảo tàng giới thiệu hình tượng mặt trời.";
        var b=Quiz("c2_qz_lt2");b.question="Hoa văn nhà sàn trên trống gắn với văn hóa cư dân nào trong các đáp án?";b.explanation="Bia giới thiệu cư dân Đông Sơn, gắn với lịch sử Lạc Việt.";
        var c=Quiz("c2_qz_lt3");c.question="Tên nào được bảo tàng dùng cho một số hình tượng chim trên trống Ngọc Lũ?";c.explanation="Tên chim Lạc được dùng khi mô tả hình tượng chim; không khẳng định một loài sinh học.";
        foreach(var id in new[]{"c2_qz_p1","c2_qz_p2","c2_qz_p3","c2_qz_p4","c2_qz_lt1","c2_qz_lt2","c2_qz_lt3"})
        {var q=Quiz(id);q.sourceRef=q.relatedLore.sourceRef;q.verified=true;EditorUtility.SetDirty(q);}
    }
    public static void ValidateContent(Action<string> pass,Action<string> fail)
    {
        var lore=AssetDatabase.FindAssets("t:LoreDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<LoreDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(l=>l&&l.loreId.StartsWith("c2_")).ToArray();
        var quizzes=AssetDatabase.FindAssets("t:QuizDefinition").Select(g=>AssetDatabase.LoadAssetAtPath<QuizDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(q=>q&&q.quizId.StartsWith("c2_")).ToArray();
        if(lore.Length!=5||quizzes.Length!=7)fail("Expected five C2 lore and seven quizzes");else pass("Five lore/seven quiz assets found");
        foreach(var l in lore)if(!l.verified||!l.sourceRef.Contains("https://")||string.IsNullOrWhiteSpace(l.body))fail("Unverified or empty lore: "+l.name);else pass("Verified/source present: "+l.loreId);
        foreach(var q in quizzes)if(!q.verified||!q.IsWellFormed||!q.relatedLore||!q.sourceRef.Contains("https://")||string.IsNullOrWhiteSpace(q.explanation)||q.options.Any(string.IsNullOrWhiteSpace)||q.options.Distinct().Count()!=4)fail("Invalid/unverified quiz: "+q.name);else pass("Quiz/options/lore/source valid: "+q.quizId);
    }
    static void ValidateScene(Scene s,Report r)
    {
        Action<bool,string> check=(ok,label)=>{if(ok)r.checks.Add(label);else r.failures.Add(label);};
        check(All<Transform>(s).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"Scene has no missing scripts");
        check(All<LoreStele>(s).Length==5&&All<QuizPedestal>(s).Length==4&&All<SequenceInputNode>(s).Length==3,"Five steles, four pedestals, three bells preserved");
        var p3=All<QuizPedestal>(s).Single(p=>Ref<QuizDefinition>(p,"quiz").quizId=="c2_qz_p3");check(new SerializedObject(p3).FindProperty("requiredPuzzleId").stringValue=="c2_pz_chuong_gio","P3 pedestal requires solved bells");
        check(All<SequenceInputNode>(s).All(n=>!n.transform.Find("C2_Bell_Glyph").GetComponentsInChildren<Collider>(true).Any()),"Bell signs are decoration without new colliders");
        var paths=new UnityEngine.AI.NavMeshPath();bool from=UnityEngine.AI.NavMesh.SamplePosition(new Vector3(9.2f,13,26),out var a,1.5f,UnityEngine.AI.NavMesh.AllAreas),to=UnityEngine.AI.NavMesh.SamplePosition(new Vector3(5,13,32.8f),out var b,1.5f,UnityEngine.AI.NavMesh.AllAreas);
        check(from&&to&&UnityEngine.AI.NavMesh.CalculatePath(a.position,b.position,UnityEngine.AI.NavMesh.AllAreas,paths)&&paths.status==UnityEngine.AI.NavMeshPathStatus.PathComplete,"Mid-checkpoint route to final wind stays connected beside new paper");
    }
    static void ValidateBuildGate(Report r)
    {
        new Chapter02KnowledgeBuildGate().OnPreprocessBuild(null);r.checks.Add("C2 build gate accepts verified data");
        var q=Quiz("c2_qz_p1");bool original=q.verified,blocked=false;
        try{q.verified=false;try{new Chapter02KnowledgeBuildGate().OnPreprocessBuild(null);}catch(BuildFailedException){blocked=true;}}
        finally{q.verified=original;}
        if(blocked)r.checks.Add("C2 build gate rejects unverified quiz (in-memory fixture restored)");else r.failures.Add("C2 build gate did not reject unverified data");
    }
    static Transform Group(string name,Transform p){var g=new GameObject(name);g.transform.SetParent(p,false);g.layer=2;return g.transform;}
    static void Glyph(Transform p,string name,Vector3 position,int feathers)
    {
        var g=Group(name,p);g.position=position;var scale=p.lossyScale;g.localScale=new Vector3(1/scale.x,1/scale.y,1/scale.z);g.gameObject.AddComponent<FaceCameraBillboard>();
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(Chapter02OpeningSetup.Art+"/Feather_LowPoly.asset");var mat=AssetDatabase.LoadAssetAtPath<Material>(Chapter02OpeningSetup.Art+"/Feather_Gold.mat");
        for(int i=0;i<feathers;i++){var f=Group("Carved_Feather",g);f.localPosition=new Vector3((i-(feathers-1)*.5f)*.19f,0,0);f.localScale=Vector3.one*.3f;f.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;f.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;}
    }
    static void Apply(Scene s)
    {
        var world=All<Transform>(s).Single(t=>t.name=="World_Geometry");var old=world.Find("ART_C2_Knowledge");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);var root=Group("ART_C2_Knowledge",world);
        Chapter01GuidePedestalSetup.AddGuide(s,root,WorldIds.ClueBellsC2,"Guide_C2_Bells",new Vector3(5.8f,13,28));
        foreach(var node in All<SequenceInputNode>(s))
        {
            var so=new SerializedObject(node);string id=so.FindProperty("nodeId").stringValue;int n=id.EndsWith("1")?1:id.EndsWith("2")?2:3;
            BuilderUtil.Set(node,"prompt","Gõ chuông "+(n==1?"Một":n==2?"Hai":"Ba")+" lông");
            var prior=node.transform.Find("C2_Bell_Glyph");if(prior)UnityEngine.Object.DestroyImmediate(prior.gameObject);
            Glyph(node.transform,"C2_Bell_Glyph",node.GetComponentInChildren<Collider>().bounds.center+Vector3.up*.65f,n);
        }
        var relief=All<Transform>(s).Single(t=>t.name=="Relief_C2_Order");var reveal=relief.GetComponent<LanternRevealSurface>();var content=Ref<GameObject>(reveal,"revealContent");
        foreach(var t in content.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="C2_Relief_Glyphs").ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
        var glyphs=Group("C2_Relief_Glyphs",content.transform);var origin=Ref<Transform>(reveal,"revealPoint").position;
        for(int i=0;i<3;i++)Glyph(glyphs,"Order_Glyph_"+i,origin+new Vector3((i-1)*.8f,.5f,0),new[]{2,3,1}[i]);
        foreach(var text in content.GetComponentsInChildren<TMP_Text>(true))text.text="BẬT ĐÈN ĐỌC DẤU LÔNG\nHai lông → Ba lông → Một lông\nĐọc từ trái sang phải. Thứ tự hư cấu của game.";
        var pedestal=All<QuizPedestal>(s).Single(p=>Ref<QuizDefinition>(p,"quiz").quizId=="c2_qz_p3");BuilderUtil.Set(pedestal,"requiredPuzzleId","c2_pz_chuong_gio");
        var pickup=All<AbilityPickup>(s).Single();var card=Ref<StoryCard>(pickup,"card");card.body=card.body.Replace("Trên gờ có bia và Bệ Vấn Đáp. Bật đèn thường bằng F, đọc bia rồi trả lời để nhận mảnh đầu tiên.","Bia đá ở phía trái đáy giếng. Bật F và đọc bia trước khi lên gờ; Bệ Vấn Đáp trên gờ giữ mảnh đầu tiên.");EditorUtility.SetDirty(card);
        QuizLayout(All<QuizPanel>(s).Single());
        var nav=All<Unity.AI.Navigation.NavMeshSurface>(s).Single();var saved=nav.navMeshData;nav.BuildNavMesh();var generated=nav.navMeshData;
        if(saved&&generated!=saved){EditorUtility.CopySerialized(generated,saved);nav.RemoveData();nav.navMeshData=saved;nav.AddData();EditorUtility.SetDirty(saved);UnityEngine.Object.DestroyImmediate(generated);}
    }
    static RectTransform Place(Transform t,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size)
    {var r=(RectTransform)t;r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=pos;r.sizeDelta=size;r.localScale=Vector3.one;return r;}
    static void Text(TMP_Text t,float size){t.enabled=true;t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BuilderUtil.FontPath);t.fontSize=size;t.enableAutoSizing=false;t.color=new Color(.94f,.95f,.88f);t.alignment=TextAlignmentOptions.TopLeft;t.textWrappingMode=TextWrappingModes.Normal;t.raycastTarget=false;}
    static void ButtonStyle(Button b,Transform parent,Vector2 p,Vector2 size)
    {Place(b.transform,parent,new Vector2(0,1),new Vector2(0,1),p,size);b.gameObject.SetActive(true);var im=b.GetComponent<Image>();im.enabled=true;im.color=new Color(.1f,.22f,.19f);b.targetGraphic=im;var t=b.GetComponentInChildren<TMP_Text>(true);Text(t,20);Place(t.transform,b.transform,Vector2.zero,Vector2.zero,new Vector2(14,10),size-new Vector2(28,20));t.alignment=TextAlignmentOptions.MidlineLeft;}
    static void QuizLayout(QuizPanel quiz)
    {
        var panel=Ref<GameObject>(quiz,"panel");if(panel.transform.Find("C2_Quiz_Card"))return;
        foreach(var graphic in panel.GetComponentsInChildren<Graphic>(true))graphic.enabled=false;
        var background=panel.GetComponent<Image>();background.enabled=true;background.color=new Color(.01f,.03f,.03f,.9f);background.raycastTarget=true;
        var g=new GameObject("C2_Quiz_Card",typeof(RectTransform),typeof(Image));var card=Place(g.transform,panel.transform,Vector2.one*.5f,Vector2.one*.5f,Vector2.zero,new Vector2(1060,820));g.GetComponent<Image>().color=new Color(.035f,.09f,.085f);
        var header=Ref<TMP_Text>(quiz,"headerText");Place(header.transform,card,new Vector2(0,1),new Vector2(0,1),new Vector2(42,-32),new Vector2(975,55));Text(header,26);
        var question=Ref<TMP_Text>(quiz,"questionText");Place(question.transform,card,new Vector2(0,1),new Vector2(0,1),new Vector2(42,-100),new Vector2(975,125));Text(question,25);
        var so=new SerializedObject(quiz);var buttons=so.FindProperty("optionButtons");for(int i=0;i<4;i++)ButtonStyle((Button)buttons.GetArrayElementAtIndex(i).objectReferenceValue,card,new Vector2(42+(i%2)*495,-245-(i/2)*120),new Vector2(475,100));
        var body=Ref<TMP_Text>(quiz,"explanationText");var view=new GameObject("Explanation_Scroll",typeof(RectTransform),typeof(Image),typeof(RectMask2D),typeof(ScrollRect));Place(view.transform,card,new Vector2(0,1),new Vector2(0,1),new Vector2(42,-495),new Vector2(975,205));view.GetComponent<Image>().color=new Color(.025f,.06f,.055f);
        Place(body.transform,view.transform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-10),new Vector2(940,185));Text(body,21);body.overflowMode=TextOverflowModes.Overflow;var fit=body.GetComponent<ContentSizeFitter>()??body.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;var scroll=view.GetComponent<ScrollRect>();scroll.viewport=(RectTransform)view.transform;scroll.content=body.rectTransform;scroll.horizontal=false;scroll.scrollSensitivity=32;scroll.movementType=ScrollRect.MovementType.Clamped;
        ButtonStyle(Ref<Button>(quiz,"retryButton"),card,new Vector2(650,-735),new Vector2(350,55));
        var close=new GameObject("C2_CloseQuiz",typeof(RectTransform),typeof(Image),typeof(Button));var label=new GameObject("Label",typeof(RectTransform),typeof(TextMeshProUGUI));label.transform.SetParent(close.transform,false);label.GetComponent<TMP_Text>().text="Đóng (Esc)";ButtonStyle(close.GetComponent<Button>(),card,new Vector2(42,-735),new Vector2(280,55));UnityEventTools.AddPersistentListener(close.GetComponent<Button>().onClick,quiz.CloseFromBack);panel.SetActive(false);
    }
}

public sealed class Chapter02KnowledgeBuildGate:IPreprocessBuildWithReport
{
    public int callbackOrder=>20;
    public void OnPreprocessBuild(BuildReport report)
    {
        if(!EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path==Chapter02MechanicsAudit.ScenePath))return;
        var failures=new List<string>();Chapter02KnowledgeSetup.ValidateContent(_=>{},failures.Add);if(failures.Count>0)throw new BuildFailedException(string.Join("\n",failures));
    }
}
