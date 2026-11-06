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
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Opt-in C2 combat/API regression followed by real InputReader bridge traversal.
public sealed class Chapter02EndingProbe : MonoBehaviour
{
    [Serializable] class Report
    {
        public string mode="Editor Play Mode; isolated ending with canonical completed-C1/C2 boss fixture, UI choices/API and real E via InputReader; native save/menu/Continue; no full chapter/manual/build claim";
        public List<string> checks=new List<string>(),failures=new List<string>(),errors=new List<string>(),screenshots=new List<string>(),notes=new List<string>(),expectedErrors=new List<string>();
    }
    [Serializable] class Backup{public string[] paths,hashes;public bool[] existed;}
    readonly Report report=new Report(); Backup backup;string folder;bool finished,completed,oldBackground;float progressAt;
    MethodInfo consoleSetFlag;object errorPauseFlag;bool originalErrorPause;
    bool inputOnly;
    PlayerMotor motor;WorldStateStore world;InventoryService inventory;QuizPanel panel;PlayerVitals vitals;Keyboard testKeyboard,oldKeyboard;Mouse testMouse,oldMouse;bool expectedSaveFailure;
    static T Find<T>()where T:Component=>UnityEngine.Object.FindAnyObjectByType<T>();
    static T[] All<T>()where T:Component=>UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None);
    static T Get<T>(object o,string n){for(Type t=o.GetType();t!=null;t=t.BaseType){var f=t.GetField(n,BindingFlags.Instance|BindingFlags.NonPublic);if(f!=null)return (T)f.GetValue(o);}throw new MissingFieldException(n);}
    static void Set(object o,string n,object v){for(Type t=o.GetType();t!=null;t=t.BaseType){var f=t.GetField(n,BindingFlags.Instance|BindingFlags.NonPublic);if(f!=null){f.SetValue(o,v);return;}}throw new MissingFieldException(n);}
    static string Hash(string p){if(!File.Exists(p))return "absent";using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p)));}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init()
    {if(!SessionState.GetBool("SonDoong.C2EndingTest",false))return;SessionState.SetBool("SonDoong.C2EndingTest",false);var g=new GameObject("C2EndingProbe");DontDestroyOnLoad(g);g.AddComponent<Chapter02EndingProbe>();}
    void Awake()
    {
        inputOnly=SessionState.GetBool("SonDoong.C2EndingInputOnly",false);SessionState.SetBool("SonDoong.C2EndingInputOnly",false);
        Application.logMessageReceived+=Log;oldBackground=Application.runInBackground;Application.runInBackground=true;
        string p=Path.Combine(Application.persistentDataPath,"save_v1.json");backup=new Backup{paths=new[]{p,p+".bak",p+".tmp"}};
        backup.existed=backup.paths.Select(File.Exists).ToArray();backup.hashes=backup.paths.Select(Hash).ToArray();
        folder="Temp/C2EndingBackup_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");Directory.CreateDirectory(folder);
        for(int i=0;i<backup.paths.Length;i++)if(backup.existed[i])File.Copy(backup.paths[i],folder+"/"+i+".original");
        File.WriteAllText(folder+"/manifest.json",JsonUtility.ToJson(backup,true));progressAt=Time.realtimeSinceStartup;
        var console=typeof(Editor).Assembly.GetType("UnityEditor.ConsoleWindow",true);var flags=console.GetNestedType("ConsoleFlags",BindingFlags.Public|BindingFlags.NonPublic);errorPauseFlag=Enum.Parse(flags,"ErrorPause");
        var bindings=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;originalErrorPause=(bool)console.GetMethod("HasFlag",bindings).Invoke(null,new[]{errorPauseFlag});consoleSetFlag=console.GetMethod("SetFlag",bindings);consoleSetFlag.Invoke(null,new object[]{errorPauseFlag,false});report.notes.Add("Console Error Pause temporarily disabled for expected I/O diagnostic; original="+originalErrorPause);
        SceneManager.sceneLoaded+=Loaded;Isolate();
    }
    void Loaded(Scene s,LoadSceneMode mode)=>Isolate();
    void Isolate(){foreach(var x in All<CheckpointTrigger>())x.enabled=false;foreach(var x in All<ArenaGate>())x.enabled=false;foreach(var x in All<EnemyBrain>())x.enabled=false;foreach(var x in All<ThrowerBrain>())x.enabled=false;}
    void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){if(expectedSaveFailure&&s.Contains("Không lưu được game"))report.expectedErrors.Add(s);else report.errors.Add(s);}}
    void Check(bool ok,string s){if(!ok)throw new InvalidOperationException(s);report.checks.Add(s);progressAt=Time.realtimeSinceStartup;File.WriteAllText("Logs/C2Ending-progress.json",JsonUtility.ToJson(report,true));}
    void Update(){if(!finished&&Time.realtimeSinceStartup-progressAt>70){report.failures.Add("Probe stalled");Finish();}}
    IEnumerator Start()
    {
        var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){bool more=false;object current=null;Exception error=null;try{more=stack.Peek().MoveNext();if(more)current=stack.Peek().Current;}catch(Exception e){error=e;}if(error!=null){report.failures.Add(error.ToString());break;}if(!more){stack.Pop();continue;}if(current is IEnumerator nested)stack.Push(nested);else yield return current;}Finish();
    }
    void Finish()
    {
        if(finished)return;finished=true;if(!completed&&report.failures.Count==0)report.failures.Add("Interrupted before completion");Time.timeScale=1;Keys();if(testMouse!=null){InputSystem.QueueStateEvent(testMouse,new MouseState());InputSystem.RemoveDevice(testMouse);}if(oldMouse!=null)oldMouse.MakeCurrent();if(testKeyboard!=null)InputSystem.RemoveDevice(testKeyboard);if(oldKeyboard!=null)oldKeyboard.MakeCurrent();
        try{for(int i=0;i<backup.paths.Length;i++){if(backup.existed[i])File.Copy(folder+"/"+i+".original",backup.paths[i],true);else if(File.Exists(backup.paths[i]))File.Delete(backup.paths[i]);}Check(backup.paths.Select(Hash).SequenceEqual(backup.hashes),"Personal primary/backup/temp saves restored byte-for-byte");}catch(Exception e){report.failures.Add("Save restore: "+e);}
        SceneManager.sceneLoaded-=Loaded;Application.runInBackground=oldBackground;Application.logMessageReceived-=Log;
        if(consoleSetFlag!=null){consoleSetFlag.Invoke(null,new object[]{errorPauseFlag,originalErrorPause});report.checks.Add("Console Error Pause restored to original setting: "+originalErrorPause);}
        Directory.CreateDirectory("docs/reports");File.WriteAllText(inputOnly?"Logs/C2Ending-input-development.json":"docs/reports/C2_Ending_PlayMode_2026-11-06.json",JsonUtility.ToJson(report,true));EditorApplication.isPlaying=false;
    }
    void OnDestroy(){if(!finished)Finish();}
    IEnumerator Wait(Func<bool> ready,float seconds,string label){float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),label);}
    IEnumerator Ready(string name)
    {yield return Wait(()=>SceneManager.GetActiveScene().name==name&&AppRoot.Instance&&(name==GameSession.MainMenuScene?AppRoot.Instance.Session.State==GameSession.SessionState.MainMenu:AppRoot.Instance.Session.State==GameSession.SessionState.Playing&&Find<SaveService>()?.IsReady==true),55,"Ready: "+name);for(int i=0;i<5;i++)yield return null;if(name!=GameSession.MainMenuScene){motor=Find<PlayerMotor>();vitals=Find<PlayerVitals>();world=Find<WorldStateStore>();inventory=Find<InventoryService>();panel=Find<QuizPanel>();Isolate();}}
    IEnumerator Continue(){AppRoot.Instance.Session.ReturnToMainMenu();yield return Ready(GameSession.MainMenuScene);AppRoot.Instance.Session.Continue();yield return Ready(GameSession.SecondChapterScene);}
    IEnumerator Screenshot(string name)
    {string path="docs/images/c2-ending-"+name+"-2026-11-06.png";Directory.CreateDirectory("docs/images");var at=DateTime.UtcNow;ScreenCapture.CaptureScreenshot(path);yield return Wait(()=>File.Exists(path)&&File.GetLastWriteTimeUtc(path)>=at,8,"Fresh screenshot: "+name);report.screenshots.Add(path);}
    void Position(Vector3 p){motor.enabled=true;motor.GetComponent<CharacterController>().enabled=true;motor.Teleport(p,Quaternion.identity);motor.enabled=false;Physics.SyncTransforms();}
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
        var interactable=(IInteractable)target;var col=target.GetComponentsInChildren<Collider>().First(c=>c.enabled&&!c.isTrigger&&c.GetComponentInParent<IInteractable>()==interactable);var point=col.bounds.center;bool placed=false;
        foreach(var dir in new[]{Vector3.back,Vector3.left,Vector3.right,Vector3.forward})
        {
            var p=point+dir*1.8f;if(!Physics.Raycast(p+Vector3.up*2,Vector3.down,out var floor,4,1<<3,QueryTriggerInteraction.Ignore))continue;p=floor.point+Vector3.up*.08f;
            if(Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore))continue;Position(p);
            Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>().enabled=false;Camera.main.transform.position=p+Vector3.up*1.45f;Camera.main.transform.LookAt(point);Physics.SyncTransforms();
            if(Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(.5f,.5f)),out var hit,4,(1<<3)|(1<<8),QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<IInteractable>()==interactable){placed=true;break;}
        }
        Check(placed&&Vector3.Distance(motor.transform.position,point)<3&&interactable.CanInteract(motor.gameObject),"Solid floor, clear capsule and interaction ray: "+target.name);
    }

    RelicAltar altar;QuizSequence sequence;ChapterExit exit;StoryCardPanel story;SaveService save;
    int restoredEvents,completedEvents;
    void Keys(params Key[] keys){if(testKeyboard!=null)InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(keys));}
    IEnumerator Frames(int n){for(int i=0;i<n;i++)yield return null;}
    void Bind()
    {
        altar=Find<RelicAltar>();sequence=Find<QuizSequence>();exit=Find<ChapterExit>();story=Find<StoryCardPanel>();save=Find<SaveService>();
        Get<UnityEngine.Events.UnityEvent>(altar,"onRestored").AddListener(()=>restoredEvents++);
        Get<UnityEngine.Events.UnityEvent>(sequence,"onCompleted").AddListener(()=>completedEvents++);
    }
    void Progression()
    {
        Fixture();foreach(string id in new[]{"c2_frag_1","c2_frag_2","c2_frag_3"})world.TryAdd(StateSet.Fragments,id);
        world.TryAdd(StateSet.BossTools,"c2_tool_long_gio_lon");world.TryAdd(StateSet.Doors,"c2_door_arena");
        world.TryAdd(StateSet.Puzzles,"c2_pz_chuong_gio");world.TryAdd(StateSet.Puzzles,"c2_wind_p3_final");world.TryAdd(StateSet.Bosses,"c2_boss_chim_lac");
        inventory.TryAdd(InventoryService.ThuocMau,2);inventory.TryAdd(InventoryService.LaCam,3);
        Position(new Vector3(5,18.1f,82));
    }
    void Remove(StateSet set,string id){var s=world.Export();s.Get(set).Remove(id);world.Restore(s);}
    void CloseStory(){Get<Button>(story,"continueButton").onClick.Invoke();}
    void AnswerAll()
    {
        for(int i=0;i<3;i++){Check(panel.IsOpen&&Get<TMP_Text>(panel,"headerText").text.Contains($"{i+1}/3"),"Spirit question context "+(i+1));panel.Choose(panel.Current.correctIndex);}
    }
    IEnumerator FocusInput()
    {
        report.notes.Add("Awaiting Game View; no input guard bypass");File.WriteAllText("Logs/C2Ending-progress.json",JsonUtility.ToJson(report,true));
        float end=Time.realtimeSinceStartup+180;while((!Application.isFocused||!AppRoot.Instance.Input.GameplayActive)&&Time.realtimeSinceStartup<end){progressAt=Time.realtimeSinceStartup;yield return null;}
        Check(Application.isFocused&&AppRoot.Instance.Input.GameplayActive,"Game View focused and production input guard active");
        if(testKeyboard==null){oldKeyboard=Keyboard.current;testKeyboard=InputSystem.AddDevice<Keyboard>("C2EndingKeyboard");testKeyboard.MakeCurrent();yield return Frames(3);}
    }
    IEnumerator PressE(Component target,Func<bool> result)
    {
        yield return FocusInput();AimAt(target);motor.enabled=true;
        yield return Wait(()=>Get<IInteractable>(Find<InteractionScanner>(),"currentTarget")== (IInteractable)target,3,"Production scanner selects grounded target: "+target.name);
        Keys(Key.E);yield return Frames(3);Keys();yield return Frames(2);Check(result(),"Real E/InputReader opens "+target.name);
    }
    IEnumerator Run()
    {
        yield return Ready(GameSession.SecondChapterScene);Bind();Progression();
        Check(!sequence.CanInteract(motor.gameObject)&&!Get<GameObject>(sequence,"visual").activeSelf&&!exit.CanInteract(motor.gameObject),"Spirit and portal locked before restoration");
        foreach(string id in new[]{"c2_frag_1","c2_frag_2","c2_frag_3","c2_boss_chim_lac"})
        {
            var before=world.Export();Remove(id.Contains("boss")?StateSet.Bosses:StateSet.Fragments,id);string snapshot=Snapshot();altar.Interact(motor.gameObject);
            Check(!altar.IsRestored&&!story.IsOpen&&Snapshot()==snapshot&&restoredEvents==0,"Missing prerequisite blocks altar: "+id);world.Restore(before);
        }
        string inv=JsonUtility.ToJson(new SaveData{inventory=inventory.Export()});
        Set(save,"ready",false);string initial=Snapshot();altar.Interact(motor.gameObject);Check(!altar.IsRestored&&!story.IsOpen&&Snapshot()==initial&&restoredEvents==0,"Unavailable save rolls restoration back and keeps event silent");Set(save,"ready",true);
        Check(save.SaveAt("c2_cp_arena"),"Baseline ending fixture saved with canonical progression");string baseline=Hash(backup.paths[0]);
        expectedSaveFailure=true;using(var locked=new FileStream(backup.paths[2],FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None)){altar.Interact(motor.gameObject);Check(!altar.IsRestored&&!story.IsOpen&&Hash(backup.paths[0])==baseline&&restoredEvents==0,"Locked temp file leaves restoration and primary save unchanged");}expectedSaveFailure=false;
        yield return PressE(altar,()=>altar.IsRestored&&story.IsOpen);Check(restoredEvents==1&&Get<GameObject>(altar,"restoredVisual").activeSelf&&Get<TMP_Text>(story,"bodyText").text.Contains("Chim Lạc"),"Restored visual and narrative follow successful single commit");
        Check(SaveService.TryReadSaved(out var restored)&&restored.schemaVersion==4&&restored.world.Get(StateSet.Puzzles).Contains("c2_relic_restored")&&!restored.world.Get(StateSet.Puzzles).Contains("c2_linhthan_done"),"Restoration saved independently before spirit questions");
        yield return Screenshot("restoration");CloseStory();altar.Interact(motor.gameObject);Check(restoredEvents==1&&!story.IsOpen,"Restoration cannot repeat reward/event");
        yield return PressE(sequence,()=>panel.IsOpen);var q=panel.Current;panel.Choose(-1);panel.Choose(4);Check(panel.Current==q&&!world.Has(StateSet.QuizAnswered,q.quizId),"Invalid answer indices ignored");
        int wrong=(q.correctIndex+1)%4;panel.Choose(wrong);panel.Choose(q.correctIndex);Check(panel.IsOpen&&world.Has(StateSet.Flags,WorldIds.QuizMistakeFlag(q.quizId))&&!world.Has(StateSet.QuizAnswered,q.quizId),"Wrong answer requires explicit retry and records mistake");
        yield return Screenshot("question");panel.CloseFromBack();Check(!panel.IsOpen&&sequence.CanInteract(motor.gameObject)&&Get<int>(sequence,"index")==0,"Esc cancellation resets transient sequence index");
        sequence.Interact(motor.gameObject);panel.Choose(q.correctIndex);Check(panel.IsOpen&&Get<int>(sequence,"index")==1&&!world.Has(StateSet.QuizFirstTry,q.quizId),"Retry after cancellation cannot recover lost first-try point");
        sequence.Interact(motor.gameObject);Check(Get<int>(sequence,"index")==1,"Repeated interaction cannot restart an open sequence");
        world.TryAdd(StateSet.Lore,"c2_lore_p5");Check(panel.IsOpen&&Get<int>(sequence,"index")==1,"Unrelated world addition preserves active conversation");
        world.Restore(world.Export());Check(!panel.IsOpen&&sequence.CanInteract(motor.gameObject)&&Get<int>(sequence,"index")==0,"Snapshot restore closes owned question and invalidates stale callbacks");
        Check(save.SaveAt("c2_cp_arena"),"Mid-conversation checkpoint stores answers/mistakes without sequence index");yield return Continue();Bind();
        Check(!sequence.IsDone&&Get<int>(sequence,"index")==0&&world.Has(StateSet.Flags,WorldIds.QuizMistakeFlag("c2_qz_lt1")),"Continue preserves grading but starts conversation from question one");
        sequence.Interact(motor.gameObject);Set(save,"ready",false);AnswerAll();
        Check(sequence.IsAwaitingSave&&!sequence.IsDone&&!exit.CanInteract(motor.gameObject)&&!story.IsOpen&&completedEvents==0,"Save failure after last answer does not unlock portal or announce completion");
        Check(world.Count(StateSet.QuizAnswered)==3&&world.Count(StateSet.QuizFirstTry)==2,"Last-question answers retained; wrong question never gains first-try point");Set(save,"ready",true);
        baseline=Hash(backup.paths[0]);expectedSaveFailure=true;using(var locked=new FileStream(backup.paths[2],FileMode.OpenOrCreate,FileAccess.ReadWrite,FileShare.None)){sequence.Interact(motor.gameObject);Check(sequence.IsAwaitingSave&&!sequence.IsDone&&Hash(backup.paths[0])==baseline,"Retry with locked file retains pending completion and safe native save");}expectedSaveFailure=false;
        yield return PressE(sequence,()=>sequence.IsDone&&story.IsOpen);Check(!sequence.IsAwaitingSave&&completedEvents==0&&!panel.IsOpen,"E retries only save, then shows spirit card before completion callback");
        Check(Get<TMP_Text>(story,"bodyText").text.Contains("Vành của ta đứng thứ nhất"),"Ending presents exact ring hint for later chapters");yield return Screenshot("awakening");CloseStory();Check(completedEvents==1&&exit.CanInteract(motor.gameObject),"Closing awakening card invokes completion once and portal is available");
        int points=world.Count(StateSet.QuizFirstTry);sequence.Interact(motor.gameObject);Check(story.IsOpen&&!panel.IsOpen,"Completed spirit can replay its message");CloseStory();Check(completedEvents==1&&world.Count(StateSet.QuizFirstTry)==points&&JsonUtility.ToJson(new SaveData{inventory=inventory.Export()})==inv,"Reading again grants no points/items/events");
        var journal=Find<JournalPanel>();journal.Open();Check(journal.IsOpen&&Get<TMP_Text>(journal,"knowledgeText").text.Contains("Vành của ta đứng thứ nhất"),"Journal derives persistent Chim Lac hint from completed sequence");journal.Close();
        Check(!GameSession.CanLoadScene("04_ThanhDaSonTinh")&&WorldIds.SceneOfCheckpoint("c3_cp_start")==null,"C3 remains an explicitly unavailable destination");
        inventory.TryRemove(InventoryService.ThuocMau,1);Set(save,"ready",false);exit.Interact(motor.gameObject);Check(!story.IsOpen&&AppRoot.Instance.Session.State==GameSession.SessionState.Playing,"Exit save failure leaves player in game for retry");Set(save,"ready",true);
        yield return PressE(exit,()=>story.IsOpen&&AppRoot.Instance.Session.State==GameSession.SessionState.ChapterComplete);
        Check(Get<TMP_Text>(story,"bodyText").text.Contains("Chương 3 chưa có")&&Get<Button>(story,"continueButton").GetComponentInChildren<TMP_Text>().text=="Về menu chính","Unavailable C3 ending states actual scope and returns to menu");
        Check(SaveService.TryReadSaved(out var final)&&final.checkpointId=="c2_cp_arena"&&final.world.Get(StateSet.Puzzles).Contains("c2_linhthan_done")&&final.inventory.Single(s=>s.itemId==InventoryService.ThuocMau).quantity==1,"Exit writes latest inventory and full ending to native C2 checkpoint");
        yield return Screenshot("chapter-end");CloseStory();yield return Ready(GameSession.MainMenuScene);AppRoot.Instance.Session.Continue();yield return Ready(GameSession.SecondChapterScene);Bind();
        Check(altar.IsRestored&&sequence.IsDone&&exit.CanInteract(motor.gameObject)&&inventory.Count(InventoryService.ThuocMau)==1&&world.Count(StateSet.QuizFirstTry)==points,"Menu Continue preserves full ending, inventory and score without duplicate awards");
        Check(!All<BossChimLac>().Single().gameObject.activeSelf&&Vector3.Distance(motor.transform.position,All<CheckpointTrigger>().Single(c=>c.CheckpointId=="c2_cp_arena").SpawnPoint.position)<.6f,"Continue returns to safe arena checkpoint with defeated boss hidden");
        journal=Find<JournalPanel>();journal.Open();Check(Get<TMP_Text>(journal,"knowledgeText").text.Contains("Vành của ta đứng thứ nhất"),"Hint survives panel/scene reconstruction after Continue");journal.Close();
        // Narrow shared-component regression; prior C1 gameplay is a fixture, no full C1 combat claim.
        AppRoot.Instance.Session.LoadChapter(GameSession.FirstChapterScene,SaveService.KimQuyCheckpointId);yield return Ready(GameSession.FirstChapterScene);
        altar=Find<RelicAltar>();story=Find<StoryCardPanel>();save=Find<SaveService>();
        Remove(StateSet.Puzzles,WorldIds.RelicRestoredC1);Remove(StateSet.Abilities,WorldIds.MovementKit);
        string c1before=Snapshot();Set(save,"ready",false);altar.Interact(motor.gameObject);
        Check(!altar.IsRestored&&!story.IsOpen&&Snapshot()==c1before,"Shared C1 altar also rolls back restoration/ability when save fails");Set(save,"ready",true);
        altar.Interact(motor.gameObject);Check(altar.IsRestored&&world.Has(StateSet.Abilities,WorldIds.MovementKit)&&story.IsOpen&&AppRoot.Instance.Session.State==GameSession.SessionState.ChapterComplete,"C1 restoration still grants movement kit and chapter-end card");
        CloseStory();yield return Ready(GameSession.SecondChapterScene);
        Check(SaveService.TryReadSaved(out var transferred)&&transferred.sceneName==GameSession.SecondChapterScene&&transferred.checkpointId=="c2_cp_start"&&transferred.world.Get(StateSet.Puzzles).Contains("c2_linhthan_done"),"C1 chapter-end button still transfers saved world to C2");
        completed=true;
    }
}
#endif
