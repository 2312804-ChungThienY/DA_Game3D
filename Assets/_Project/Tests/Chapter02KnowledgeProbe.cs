#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Opt-in integration with actual C2 components. Positional/API fixtures, no keyboard injection.
public sealed class Chapter02KnowledgeProbe : MonoBehaviour
{
    [Serializable] class Report
    {
        public string mode="Editor Play Mode; API/button events, physical reveal LOS, native checkpoint/menu/Continue; C1 completion and player positions are fixtures; no traversal/manual-input/build claim";
        public List<string> checks=new List<string>(),failures=new List<string>(),errors=new List<string>(),screenshots=new List<string>();
    }
    [Serializable] class Backup{public string[] paths,hashes;public bool[] existed;}
    readonly Report report=new Report(); Backup backup;string folder;bool finished,completed,oldBackground;float progressAt;
    PlayerMotor motor;WorldStateStore world;InventoryService inventory;QuizPanel panel;
    static T Find<T>()where T:Component=>UnityEngine.Object.FindAnyObjectByType<T>();
    static T[] All<T>()where T:Component=>UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None);
    static T Get<T>(object o,string n){for(Type t=o.GetType();t!=null;t=t.BaseType){var f=t.GetField(n,BindingFlags.Instance|BindingFlags.NonPublic);if(f!=null)return (T)f.GetValue(o);}throw new MissingFieldException(n);}
    static string Hash(string p){if(!File.Exists(p))return "absent";using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p)));}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init()
    {if(!SessionState.GetBool("SonDoong.C2KnowledgeTest",false))return;SessionState.SetBool("SonDoong.C2KnowledgeTest",false);var g=new GameObject("C2KnowledgeProbe");DontDestroyOnLoad(g);g.AddComponent<Chapter02KnowledgeProbe>();}
    void Awake()
    {
        Application.logMessageReceived+=Log;oldBackground=Application.runInBackground;Application.runInBackground=true;
        string p=Path.Combine(Application.persistentDataPath,"save_v1.json");backup=new Backup{paths=new[]{p,p+".bak",p+".tmp"}};
        backup.existed=backup.paths.Select(File.Exists).ToArray();backup.hashes=backup.paths.Select(Hash).ToArray();
        folder="Temp/C2KnowledgeBackup_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");Directory.CreateDirectory(folder);
        for(int i=0;i<backup.paths.Length;i++)if(backup.existed[i])File.Copy(backup.paths[i],folder+"/"+i+".original");
        File.WriteAllText(folder+"/manifest.json",JsonUtility.ToJson(backup,true));progressAt=Time.realtimeSinceStartup;
        SceneManager.sceneLoaded+=Loaded;Isolate();
    }
    void Loaded(Scene s,LoadSceneMode mode)=>Isolate();
    void Isolate(){foreach(var x in All<CheckpointTrigger>())x.enabled=false;foreach(var x in All<ArenaGate>())x.enabled=false;foreach(var x in All<EnemyBrain>())x.enabled=false;foreach(var x in All<ThrowerBrain>())x.enabled=false;}
    void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(s);}
    void Check(bool ok,string s){if(!ok)throw new InvalidOperationException(s);report.checks.Add(s);progressAt=Time.realtimeSinceStartup;File.WriteAllText("Logs/C2Knowledge-progress.json",JsonUtility.ToJson(report,true));}
    void Update(){if(!finished&&Time.realtimeSinceStartup-progressAt>70){report.failures.Add("Probe stalled");Finish();}}
    IEnumerator Start()
    {
        var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){bool more=false;object current=null;Exception error=null;try{more=stack.Peek().MoveNext();if(more)current=stack.Peek().Current;}catch(Exception e){error=e;}if(error!=null){report.failures.Add(error.ToString());break;}if(!more){stack.Pop();continue;}if(current is IEnumerator nested)stack.Push(nested);else yield return current;}Finish();
    }
    void Finish()
    {
        if(finished)return;finished=true;if(!completed&&report.failures.Count==0)report.failures.Add("Interrupted before completion");Time.timeScale=1;
        try{for(int i=0;i<backup.paths.Length;i++){if(backup.existed[i])File.Copy(folder+"/"+i+".original",backup.paths[i],true);else if(File.Exists(backup.paths[i]))File.Delete(backup.paths[i]);}Check(backup.paths.Select(Hash).SequenceEqual(backup.hashes),"Personal primary/backup/temp saves restored byte-for-byte");}catch(Exception e){report.failures.Add("Save restore: "+e);}
        SceneManager.sceneLoaded-=Loaded;Application.runInBackground=oldBackground;Application.logMessageReceived-=Log;
        Directory.CreateDirectory("docs/reports");File.WriteAllText("docs/reports/C2_Knowledge_PlayMode_2026-11-05.json",JsonUtility.ToJson(report,true));EditorApplication.isPlaying=false;
    }
    void OnDestroy(){if(!finished)Finish();}
    IEnumerator Wait(Func<bool> ready,float seconds,string label){float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),label);}
    IEnumerator Ready(string name)
    {yield return Wait(()=>SceneManager.GetActiveScene().name==name&&AppRoot.Instance&&(name==GameSession.MainMenuScene?AppRoot.Instance.Session.State==GameSession.SessionState.MainMenu:AppRoot.Instance.Session.State==GameSession.SessionState.Playing&&Find<SaveService>()?.IsReady==true),55,"Ready: "+name);for(int i=0;i<5;i++)yield return null;if(name!=GameSession.MainMenuScene){motor=Find<PlayerMotor>();world=Find<WorldStateStore>();inventory=Find<InventoryService>();panel=Find<QuizPanel>();Isolate();}}
    IEnumerator Continue(){AppRoot.Instance.Session.ReturnToMainMenu();yield return Ready(GameSession.MainMenuScene);AppRoot.Instance.Session.Continue();yield return Ready(GameSession.SecondChapterScene);}
    IEnumerator Screenshot(string name)
    {string path="docs/images/c2-knowledge-"+name+"-2026-11-05.png";Directory.CreateDirectory("docs/images");var at=DateTime.UtcNow;ScreenCapture.CaptureScreenshot(path);yield return Wait(()=>File.Exists(path)&&File.GetLastWriteTimeUtc(path)>=at,8,"Fresh screenshot: "+name);report.screenshots.Add(path);}
    void Position(Vector3 p){motor.enabled=true;motor.GetComponent<CharacterController>().enabled=true;motor.Teleport(p,Quaternion.identity);motor.enabled=false;motor.GetComponent<CharacterController>().enabled=false;Physics.SyncTransforms();}
    void Fixture()
    {
        world.Restore(new WorldSnapshot());inventory.Restore(new List<InventorySlotDto>());
        foreach(string id in new[]{WorldStateStore.DrumA,WorldStateStore.DrumB,WorldStateStore.DrumC})world.TryAdd(StateSet.Fragments,id);
        foreach(string id in new[]{WorldStateStore.DrumPuzzleId,WorldStateStore.MirrorPuzzleId,WorldIds.RelicRestoredC1})world.TryAdd(StateSet.Puzzles,id);
        world.TryAdd(StateSet.Seals,WorldIds.SealGarden);world.TryAdd(StateSet.Doors,WorldIds.DoorKimQuy);world.TryAdd(StateSet.Bosses,WorldIds.GuardianBoss);
        world.TryAdd(StateSet.Abilities,WorldIds.MovementKit);world.TryAdd(StateSet.Abilities,WorldIds.AbilityWind);world.TryAdd(StateSet.Flags,WorldIds.FlagLantern);Find<LanternController>().ApplySavedState(true);
        Position(new Vector3(0,.2f,-6));
    }
    string Snapshot()=>JsonUtility.ToJson(world.Export())+JsonUtility.ToJson(new SaveData{inventory=inventory.Export()});
    List<InventorySlotDto> Full(int n)=>Enumerable.Range(0,n).Select(i=>new InventorySlotDto{slot=i,itemId=InventoryService.LaCam,quantity=inventory.GetDefinition(InventoryService.LaCam).maxStack}).ToList();
    QuizPedestal Ped(string id)=>All<QuizPedestal>().Single(p=>Get<QuizDefinition>(p,"quiz").quizId==id);
    void AimAt(Component target)
    {
        var interactable=(IInteractable)target;var col=target.GetComponentsInChildren<Collider>().First(c=>c.enabled&&!c.isTrigger&&c.gameObject.layer==8);var point=col.bounds.center;bool placed=false;
        foreach(var dir in new[]{Vector3.back,Vector3.left,Vector3.right,Vector3.forward})
        {
            var p=point+dir*1.8f;if(!Physics.Raycast(p+Vector3.up*2,Vector3.down,out var floor,4,1<<3,QueryTriggerInteraction.Ignore))continue;p=floor.point+Vector3.up*.08f;
            if(Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore))continue;Position(p);
            Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>().enabled=false;Camera.main.transform.position=p+Vector3.up*1.45f;Camera.main.transform.LookAt(point);Physics.SyncTransforms();
            if(Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(.5f,.5f)),out var hit,4,(1<<3)|(1<<8),QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<IInteractable>()==interactable){placed=true;break;}
        }
        Check(placed&&Vector3.Distance(motor.transform.position,point)<3&&interactable.CanInteract(motor.gameObject),"Solid floor, clear capsule and interaction ray: "+target.name);
    }
    IEnumerator Run()
    {
        yield return Ready(GameSession.SecondChapterScene);Fixture();yield return Reveal();yield return Bells();yield return Questions();yield return Rewards();yield return Persistence();
        Check(report.errors.Count==0,"No runtime errors/assertions/exceptions");completed=true;
    }
    IEnumerator Reveal()
    {
        var lantern=Find<LanternController>();
        foreach(var stele in All<LoreStele>().OrderBy(x=>Get<LoreDefinition>(x,"lore").loreId))
        {
            var reveal=Get<LanternRevealSurface>(stele,"reveal");var point=Get<Transform>(reveal,"revealPoint").position;bool placed=false;
            foreach(var dir in new[]{Vector3.back,Vector3.left,Vector3.right,Vector3.forward})
            {
                Vector3 p=point+dir*1.8f;if(!Physics.Raycast(p+Vector3.up*1.2f,Vector3.down,out var hit,4,1<<3,QueryTriggerInteraction.Ignore))continue;p=hit.point+Vector3.up*.08f;
                if(Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore))continue;
                if(Physics.Linecast(p+Vector3.up*1.3f,point,1<<3,QueryTriggerInteraction.Ignore))continue;Position(p);placed=true;break;
            }
            Check(placed,"Clear physical reading approach: "+stele.name);lantern.ApplySavedState(false);yield return new WaitForSeconds(.3f);Check(!reveal.IsRevealed,"Lantern off hides: "+stele.name);
            lantern.ApplySavedState(true);yield return new WaitForSeconds(.4f);Check(reveal.IsRevealed&&stele.IsRead,"Lantern reveals and records lore: "+stele.name);
            stele.Interact(motor.gameObject);Check(Find<StoryCardPanel>().IsOpen,"Lore opens full reading panel: "+stele.name);Find<StoryCardPanel>().CloseFromBack();
            var block=GameObject.CreatePrimitive(PrimitiveType.Cube);block.layer=3;block.transform.position=(point+motor.transform.position+Vector3.up*1.3f)*.5f;block.transform.localScale=Vector3.one*.6f;Physics.SyncTransforms();yield return new WaitForSeconds(.3f);Check(!reveal.IsRevealed,"Environment occlusion hides lore: "+stele.name);Destroy(block);yield return null;
        }
        Position(new Vector3(0,.2f,-6));yield return new WaitForSeconds(.3f);Check(All<LoreStele>().All(l=>!Get<LanternRevealSurface>(l,"reveal").IsRevealed),"Distant lore stays hidden outside reveal range");
        Find<JournalPanel>().Open();Check(All<LoreStele>().All(l=>Get<TMP_Text>(Find<JournalPanel>(),"knowledgeText").text.Contains(Get<LoreDefinition>(l,"lore").title)),"Journal lists five discovered lore entries");yield return Screenshot("journal");Find<JournalPanel>().Close();
    }
    IEnumerator Bells()
    {
        var p=Ped("c2_qz_p3");p.Interact(motor.gameObject);Check(!panel.IsOpen&&!p.IsOpened,"P3 question/reward blocked before bells solved");
        var sequence=Find<SequencePuzzleController>();var definition=Get<SequencePuzzleDefinition>(sequence,"definition");var wind=All<WindColumn>().Single(w=>w.name=="WindColumn_P3_Final");
        Check(definition.sequence.SequenceEqual(new[]{"bell_2","bell_3","bell_1"}),"Bell solution preserved 2-3-1");
        foreach(var node in All<SequenceInputNode>()){int n=int.Parse(Get<string>(node,"nodeId").Substring(5));Check(node.transform.Find("C2_Bell_Glyph").childCount==n,"Physical feather count matches "+Get<string>(node,"nodeId"));Check(node.Prompt=="Gõ chuông "+(n==1?"Một":n==2?"Hai":"Ba")+" lông","Bell prompt uses feather signs");}
        var clue=All<PuzzleCluePickup>().Single(c=>c.ClueId==WorldIds.ClueBellsC2);Check(!clue.IsFound,"Bell clue not automatically acquired");
        var first=All<SequenceInputNode>().Single(n=>Get<string>(n,"nodeId")=="bell_1");first.Interact(motor.gameObject);yield return new WaitForSeconds(definition.inputDelay+definition.failDelay+.15f);Check(sequence.Progress==0&&!sequence.IsSolved&&!wind.IsActive,"Wrong bell resets with wind still locked");
        foreach(string id in definition.sequence){var node=All<SequenceInputNode>().Single(n=>Get<string>(n,"nodeId")==id);node.Interact(motor.gameObject);yield return new WaitForSeconds(definition.inputDelay+.1f);}
        Check(sequence.IsSolved&&wind.IsActive&&!p.IsOpened&&!world.Has(StateSet.Fragments,"c2_frag_3"),"Optional-paper-free solution unlocks wind without granting fragment");
        AimAt(clue);clue.Interact(motor.gameObject);Check(clue.IsFound&&Find<StoryCardPanel>().IsOpen,"Optional clue opens and records journal flag");Find<StoryCardPanel>().CloseFromBack();
        var relief=All<LanternRevealSurface>().Single(r=>r.name=="Relief_C2_Order");var at=Get<Transform>(relief,"revealPoint").position;Position(new Vector3(at.x,13.08f,at.z-2));
        Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>().enabled=false;Camera.main.transform.position=motor.transform.position+Vector3.up*1.5f;Camera.main.transform.LookAt(at);yield return new WaitForSeconds(.4f);
        Check(relief.IsRevealed,"Relief visible with lantern on near the bell floor");yield return Screenshot("relief");
    }
    IEnumerator Questions()
    {
        foreach(var pedestal in All<QuizPedestal>().OrderBy(p=>p.name))
        {
            AimAt(pedestal);var quiz=Get<QuizDefinition>(pedestal,"quiz");pedestal.Interact(motor.gameObject);Check(panel.IsOpen&&panel.Current==quiz&&Time.timeScale==0,"Question pauses gameplay: "+quiz.quizId);
            if(quiz.quizId=="c2_qz_p1")
            {
                var before=Snapshot();panel.Choose(-1);panel.Choose(4);Check(before==Snapshot(),"Invalid answer indices do not mutate progress");yield return Screenshot("question");
                Get<Button[]>(panel,"optionButtons")[(quiz.correctIndex+1)%4].onClick.Invoke();Check(panel.IsOpen&&!pedestal.IsOpened&&!world.Has(StateSet.QuizAnswered,quiz.quizId)&&world.Has(StateSet.Flags,WorldIds.QuizMistakeFlag(quiz.quizId)),"Wrong answer records penalty without reward");
                panel.Choose(quiz.correctIndex);Check(panel.IsOpen&&!pedestal.IsOpened,"Disabled options cannot bypass retry state");yield return Screenshot("wrong");
                var explanation=Get<TMP_Text>(panel,"explanationText");Check(explanation.GetComponentInParent<ScrollRect>()&&explanation.text.Contains(quiz.relatedLore.title),"Explanation scroll cites related lore");
                panel.CloseFromBack();pedestal.Interact(motor.gameObject);Check(panel.IsOpen,"Esc allows reopening without clearing wrong-answer penalty");
            }
            bool observerSawBoth=false;Action observe=()=>{if(world.Has(StateSet.QuizAnswered,quiz.quizId))observerSawBoth=quiz.quizId=="c2_qz_p1"?!world.Has(StateSet.QuizFirstTry,quiz.quizId):world.Has(StateSet.QuizFirstTry,quiz.quizId);};world.Changed+=observe;
            Get<Button[]>(panel,"optionButtons")[quiz.correctIndex].onClick.Invoke();world.Changed-=observe;
            Check(pedestal.IsOpened&&!panel.IsOpen&&Time.timeScale==1,"Correct button grants and resumes: "+quiz.quizId);Check(observerSawBoth,"Answered and first-try score visible together: "+quiz.quizId);
            string state=Snapshot();panel.Choose(quiz.correctIndex);pedestal.Interact(motor.gameObject);Check(state==Snapshot(),"Double-answer/reopened pedestal cannot duplicate: "+quiz.quizId);
        }
        Check(!world.Has(StateSet.QuizFirstTry,"c2_qz_p1")&&new[]{"c2_frag_1","c2_frag_2","c2_frag_3"}.All(id=>world.Has(StateSet.Fragments,id)),"Wrong-then-correct receives fragment but no first-try point");
        foreach(string id in new[]{"c2_qz_lt1","c2_qz_lt2","c2_qz_lt3"})
        {var q=UnityEditor.AssetDatabase.LoadAssetAtPath<QuizDefinition>("Assets/_Project/Data/Knowledge/QZ_"+id+".asset");int callbacks=0;Check(panel.Ask(q,()=>callbacks++),"Linh Than question available: "+id);panel.Choose((q.correctIndex+1)%4);Get<Button>(panel,"retryButton").onClick.Invoke();panel.Choose(q.correctIndex);panel.Choose(q.correctIndex);Check(callbacks==1&&world.Has(StateSet.QuizAnswered,id)&&!world.Has(StateSet.QuizFirstTry,id),"Retry correct records one callback without score: "+id);}
        yield return null;
    }
    IEnumerator Rewards()
    {
        Fixture();var service=Find<RewardService>();inventory.Restore(Full(11));string before=Snapshot();int invEvents=0,worldEvents=0;Action ie=()=>invEvents++,we=()=>worldEvents++;inventory.Changed+=ie;world.Changed+=we;
        var pair=new List<RewardEntry>{new RewardEntry{type=RewardType.Item,id=InventoryService.LinhChi},new RewardEntry{type=RewardType.Item,id=InventoryService.ThuocMau},new RewardEntry{type=RewardType.Fragment,id="c2_frag_1"}};
        Check(inventory.CanAdd(InventoryService.LinhChi,1)&&inventory.CanAdd(InventoryService.ThuocMau,1),"Each reward alone fits the remaining slot fixture");
        Check(!service.TryGrant(pair,out _,"c2_ped_p1")&&Snapshot()==before&&invEvents==0&&worldEvents==0,"Combined overflow grants nothing, emits no events and retains claim");
        var bad=new List<List<RewardEntry>>{null,new List<RewardEntry>(),new List<RewardEntry>{null},new List<RewardEntry>{new RewardEntry{type=RewardType.Item,id="unknown"}},new List<RewardEntry>{new RewardEntry{type=RewardType.Fragment,id="unknown"}},new List<RewardEntry>{new RewardEntry{type=RewardType.Item,id=InventoryService.LaCam,quantity=-1}},new List<RewardEntry>{new RewardEntry{type=RewardType.Item,id=InventoryService.LaCam,quantity=int.MaxValue},new RewardEntry{type=RewardType.Item,id=InventoryService.LaCam}},new List<RewardEntry>{new RewardEntry{type=(RewardType)99,id="c2_frag_1"}}};
        for(int i=0;i<bad.Count;i++)Check(!service.TryGrant(bad[i],out _)&&Snapshot()==before,"Invalid reward rejected atomically #"+i);
        inventory.Changed-=ie;world.Changed-=we;inventory.Restore(Full(10));bool complete=false,reentry=false;invEvents=worldEvents=0;
        Action atomic=()=>{invEvents++;complete=inventory.Count(InventoryService.LinhChi)==1&&inventory.Count(InventoryService.ThuocMau)==1&&world.Has(StateSet.Fragments,"c2_frag_1")&&world.Has(StateSet.Pickups,"c2_ped_p1");reentry=!service.TryGrant(pair,out _,"c2_ped_p2");};inventory.Changed+=atomic;world.Changed+=we;
        Check(service.TryGrant(pair,out _,"c2_ped_p1")&&complete&&reentry&&invEvents==1&&worldEvents==1,"Successful batch publishes complete inventory/world/claim once and rejects reentry");inventory.Changed-=atomic;world.Changed-=we;before=Snapshot();Check(!service.TryGrant(pair,out _,"c2_ped_p1")&&before==Snapshot(),"Claim blocks repeated batch rewards");
        Fixture();inventory.Restore(Full(12));var p4=Ped("c2_qz_p4");var q=Get<QuizDefinition>(p4,"quiz");p4.Interact(motor.gameObject);panel.Choose(q.correctIndex);
        Check(world.Has(StateSet.QuizAnswered,q.quizId)&&!p4.IsOpened&&!world.Has(StateSet.BossTools,"c2_tool_long_gio_lon")&&inventory.Count(InventoryService.ThuocMau)==0,"Actual P4 full bag records answer, retains both tool and potions");
        Check(Find<SaveService>().SaveAt("c2_cp_mid"),"Native save accepts answered/unclaimed pedestal");yield return Continue();Position(new Vector3(9.2f,13.2f,26));p4=Ped("c2_qz_p4");Check(!p4.IsOpened&&world.Has(StateSet.QuizAnswered,q.quizId)&&!world.Has(StateSet.BossTools,"c2_tool_long_gio_lon"),"Continue retains unclaimed P4 reward");
        inventory.TryRemove(InventoryService.LaCam,inventory.GetDefinition(InventoryService.LaCam).maxStack);p4.Interact(motor.gameObject);Check(p4.IsOpened&&!panel.IsOpen&&world.Has(StateSet.BossTools,"c2_tool_long_gio_lon")&&inventory.Count(InventoryService.ThuocMau)==2,"Freeing one slot grants exact P4 tool/two potions without asking again");
        Check(Find<SaveService>().SaveAt("c2_cp_mid"),"Native save accepts claimed P4");yield return Continue();p4=Ped("c2_qz_p4");p4.Interact(motor.gameObject);Check(p4.IsOpened&&!panel.IsOpen&&inventory.Count(InventoryService.ThuocMau)==2,"Continue does not duplicate claimed P4 reward");
    }
    IEnumerator Persistence()
    {
        Fixture();var p=Ped("c2_qz_p2");var q=Get<QuizDefinition>(p,"quiz");p.Interact(motor.gameObject);panel.Choose((q.correctIndex+1)%4);panel.CloseFromBack();
        var clue=All<PuzzleCluePickup>().Single(c=>c.ClueId==WorldIds.ClueBellsC2);clue.Interact(motor.gameObject);Find<StoryCardPanel>().CloseFromBack();world.TryAdd(StateSet.Lore,"c2_lore_p3");world.TryAdd(StateSet.Puzzles,"c2_pz_chuong_gio");
        Check(Find<SaveService>().SaveAt("c2_cp_mid"),"Wrong answer and clue stored through native checkpoint");Check(File.ReadAllText(backup.paths[0]).Contains(WorldIds.QuizMistakeFlag(q.quizId)),"Wrong-answer flag present in native JSON");yield return Continue();
        Check(world.Has(StateSet.Flags,WorldIds.ClueBellsC2)&&world.Has(StateSet.Lore,"c2_lore_p3")&&Find<SequencePuzzleController>().IsSolved,"Continue restores paper/lore/solved bells");
        p=Ped("c2_qz_p2");q=Get<QuizDefinition>(p,"quiz");p.Interact(motor.gameObject);panel.Choose(q.correctIndex);Check(p.IsOpened&&world.Has(StateSet.Flags,WorldIds.QuizMistakeFlag(q.quizId))&&!world.Has(StateSet.QuizFirstTry,q.quizId),"New scene/panel after Continue preserves wrong-answer score penalty");
        Fixture();world.TryAdd(StateSet.QuizAnswered,q.quizId);p=Ped("c2_qz_p2");p.Interact(motor.gameObject);Check(p.IsOpened&&!world.Has(StateSet.QuizFirstTry,q.quizId),"Legacy answered/no-point state never gains a point on reward retry");
        Fixture();p=Ped("c2_qz_p2");p.Interact(motor.gameObject);panel.Choose(q.correctIndex);Check(p.IsOpened&&world.Has(StateSet.QuizFirstTry,q.quizId)&&!world.Has(StateSet.Flags,WorldIds.QuizMistakeFlag(q.quizId)),"Fresh world snapshot clears prior run penalty and allows first-try point");
    }
}
#endif
