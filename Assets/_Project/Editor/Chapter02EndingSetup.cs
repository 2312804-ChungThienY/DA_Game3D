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
public static class Chapter02EndingSetup
{
    const string Request="Library/SonDoongC2Ending.request.json";
    const string Art="Assets/_Project/Art/Chapter02Ending";
    [Serializable]class Command{public string action;}
    [Serializable]class Report{public string action;public List<string> checks=new List<string>(),failures=new List<string>(),notes=new List<string>();}
    static bool busy;
    static Chapter02EndingSetup()=>EditorApplication.update+=Poll;
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
            {SessionState.SetBool("SonDoong.C2EndingTest",true);SessionState.SetBool("SonDoong.C2EndingInputOnly",r.action=="input-test");EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);EditorWindow.GetWindow(Type.GetType("UnityEditor.GameView,UnityEditor")).Focus();EditorApplication.isPaused=false;EditorApplication.EnterPlaymode();}
            else
            {
                var setup=EditorSceneManager.GetSceneManagerSetup();
                try{var s=EditorSceneManager.OpenScene(Chapter02MechanicsAudit.ScenePath);if(r.action=="apply"){Apply(s);foreach(var c in All<Component>(s))if(c&&PrefabUtility.IsPartOfPrefabInstance(c))PrefabUtility.RecordPrefabInstancePropertyModifications(c);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();}Inspect(s,r);}
                finally{EditorSceneManager.RestoreSceneManagerSetup(setup);}
            }
        }catch(Exception e){r.failures.Add(e.ToString());}
        finally{File.WriteAllText("Logs/C2Ending-"+r.action+".json",JsonUtility.ToJson(r,true));File.Delete(Request);busy=false;}
    }

    static void Apply(Scene s)
    {
        var altar=All<RelicAltar>(s).Single();
        var save=All<SaveService>(s).Single();
        var card=AssetDatabase.LoadAssetAtPath<StoryCard>("Assets/_Project/Data/Chapters/C2/SC_C2_Awaken.asset");
        card.title=ChapterMemoryNotes.ChimLacTitle;card.body=ChapterMemoryNotes.ChimLacBody;EditorUtility.SetDirty(card);
        const string path="Assets/_Project/Data/Chapters/C2/SC_C2_RelicRestored.asset";
        var restored=AssetDatabase.LoadAssetAtPath<StoryCard>(path);
        if(!restored){restored=ScriptableObject.CreateInstance<StoryCard>();AssetDatabase.CreateAsset(restored,path);}
        restored.title="Mặt Trời Lông Chim đã phục dựng";
        restored.body="Ba mảnh cổ vật khớp lại trên bệ. Những đường lông chim đón ánh sáng, luồng gió trên đỉnh tổ trở nên yên dịu.\n\nChim Lạc đang chờ ở bên phải bệ. Hãy trò chuyện để nghe lời dặn trước chặng đường tới Thành Đá Sơn Tinh.\n\nTiến trình đã được lưu tại đỉnh tổ. Lời dẫn này là hư cấu của game.";
        EditorUtility.SetDirty(restored);BuilderUtil.Set(altar,"restorationCard",restored);
        var exit=All<ChapterExit>(s).Single();BuilderUtil.Set(exit,"saveService",save);BuilderUtil.Set(exit,"exitCheckpointId","c2_cp_arena");
        BuilderUtil.Set(exit,"label","Rời đỉnh tổ — kết thúc Chương 2");
        BuilderUtil.Set(exit,"endTitle","Chương 2 hoàn thành — Giếng Trời Chim Lạc");
        BuilderUtil.Set(exit,"endBody","Mặt Trời Lông Chim đã được phục dựng và lời Chim Lạc đã được ghi trong Nhật ký.\n\nLinh mạch dẫn An tới Thành Đá Sơn Tinh. Chương 3 chưa có trong phiên bản hiện tại.\n\nTiến trình và túi đồ đã được lưu tại đỉnh tổ. Chọn Continue từ menu để trở lại đây.");
    }

    static void Inspect(Scene s,Report r)
    {
        Action<bool,string> check=(ok,label)=>{if(ok)r.checks.Add(label);else r.failures.Add(label);};
        var altar=All<RelicAltar>(s).Single();var seq=All<QuizSequence>(s).Single();var exit=All<ChapterExit>(s).Single();
        check(All<Transform>(s).All(t=>GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)==0),"No missing scripts");
        check(new SerializedObject(altar).FindProperty("fragmentIds").arraySize==3&&new SerializedObject(altar).FindProperty("bossId").stringValue=="c2_boss_chim_lac","Restoration requires three fragments and Chim Lac");
        check(!new SerializedObject(altar).FindProperty("endsChapter").boolValue&&Ref<StoryCard>(altar,"restorationCard"),"Restoration leads to spirit, not chapter exit");
        check(new SerializedObject(seq).FindProperty("requiredPuzzleId").stringValue=="c2_relic_restored"&&new SerializedObject(seq).FindProperty("quizzes").arraySize==3,"Three spirit questions gated by restoration");
        check(Ref<StoryCard>(seq,"completionCard").body.Contains("Vành của ta đứng thứ nhất"),"Ending contains persistent C5 ring hint");
        check(Ref<SaveService>(exit,"saveService")&&new SerializedObject(exit).FindProperty("exitCheckpointId").stringValue=="c2_cp_arena","Unavailable-next-chapter exit saves current inventory at safe C2 checkpoint");
        check(!GameSession.CanLoadScene("04_ThanhDaSonTinh")&&WorldIds.SceneOfCheckpoint("c3_cp_start")==null,"C3 remains unavailable; no fake destination registration");
        r.notes.Add("Altar: "+JsonUtility.ToJson(altar));r.notes.Add("Sequence: "+JsonUtility.ToJson(seq));r.notes.Add("Exit: "+JsonUtility.ToJson(exit));
    }
}
