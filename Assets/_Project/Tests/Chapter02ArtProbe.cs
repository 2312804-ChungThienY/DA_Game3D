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
public sealed class Chapter02ArtProbe : MonoBehaviour
{
    [Serializable] class Report
    {
        public string mode="Editor Play Mode; C1 completed fixture, actual scene exit transfer; C2 art/physics/camera/audio/UI/native Continue. Traversal and combat regressions are separate reports; no manual/full-chapter duration or hardware performance certification.";
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
    {if(!SessionState.GetBool("SonDoong.C2ArtTest",false))return;SessionState.SetBool("SonDoong.C2ArtTest",false);var g=new GameObject("C2ArtProbe");DontDestroyOnLoad(g);g.AddComponent<Chapter02ArtProbe>();}
    void Awake()
    {
        inputOnly=SessionState.GetBool("SonDoong.C2ArtInputOnly",false);SessionState.SetBool("SonDoong.C2ArtInputOnly",false);
        Application.logMessageReceived+=Log;oldBackground=Application.runInBackground;Application.runInBackground=true;
        string p=Path.Combine(Application.persistentDataPath,"save_v1.json");backup=new Backup{paths=new[]{p,p+".bak",p+".tmp"}};
        backup.existed=backup.paths.Select(File.Exists).ToArray();backup.hashes=backup.paths.Select(Hash).ToArray();
        folder="Temp/C2ArtBackup_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");Directory.CreateDirectory(folder);
        for(int i=0;i<backup.paths.Length;i++)if(backup.existed[i])File.Copy(backup.paths[i],folder+"/"+i+".original");
        File.WriteAllText(folder+"/manifest.json",JsonUtility.ToJson(backup,true));progressAt=Time.realtimeSinceStartup;
        var console=typeof(Editor).Assembly.GetType("UnityEditor.ConsoleWindow",true);var flags=console.GetNestedType("ConsoleFlags",BindingFlags.Public|BindingFlags.NonPublic);errorPauseFlag=Enum.Parse(flags,"ErrorPause");
        var bindings=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;originalErrorPause=(bool)console.GetMethod("HasFlag",bindings).Invoke(null,new[]{errorPauseFlag});consoleSetFlag=console.GetMethod("SetFlag",bindings);consoleSetFlag.Invoke(null,new object[]{errorPauseFlag,false});report.notes.Add("Console Error Pause temporarily disabled for expected I/O diagnostic; original="+originalErrorPause);
        SceneManager.sceneLoaded+=Loaded;Isolate();
    }
    void Loaded(Scene s,LoadSceneMode mode)=>Isolate();
    void Isolate(){foreach(var x in All<CheckpointTrigger>())x.enabled=false;foreach(var x in All<ArenaGate>())x.enabled=false;foreach(var x in All<EnemyBrain>())x.enabled=false;foreach(var x in All<ThrowerBrain>())x.enabled=false;}
    void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){if(expectedSaveFailure&&s.Contains("Không lưu được game"))report.expectedErrors.Add(s);else report.errors.Add(s);}}
    void Check(bool ok,string s){if(!ok)throw new InvalidOperationException(s);report.checks.Add(s);progressAt=Time.realtimeSinceStartup;File.WriteAllText("Logs/C2Art-progress.json",JsonUtility.ToJson(report,true));}
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
        Directory.CreateDirectory("docs/reports");File.WriteAllText(inputOnly?"Logs/C2Art-input-development.json":"docs/reports/C2_Art_PlayMode_2026-11-06.json",JsonUtility.ToJson(report,true));EditorApplication.isPlaying=false;
    }
    void OnDestroy(){if(!finished)Finish();}
    IEnumerator Wait(Func<bool> ready,float seconds,string label){float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),label);}
    IEnumerator Ready(string name)
    {yield return Wait(()=>SceneManager.GetActiveScene().name==name&&AppRoot.Instance&&(name==GameSession.MainMenuScene?AppRoot.Instance.Session.State==GameSession.SessionState.MainMenu:AppRoot.Instance.Session.State==GameSession.SessionState.Playing&&Find<SaveService>()?.IsReady==true),55,"Ready: "+name);for(int i=0;i<5;i++)yield return null;if(name!=GameSession.MainMenuScene){motor=Find<PlayerMotor>();vitals=Find<PlayerVitals>();world=Find<WorldStateStore>();inventory=Find<InventoryService>();panel=Find<QuizPanel>();Isolate();}}
    IEnumerator Continue(){AppRoot.Instance.Session.ReturnToMainMenu();yield return Ready(GameSession.MainMenuScene);AppRoot.Instance.Session.Continue();yield return Ready(GameSession.SecondChapterScene);}
    IEnumerator Screenshot(string name)
    {string path="docs/images/c2-art-"+name+"-2026-11-06.png";Directory.CreateDirectory("docs/images");var at=DateTime.UtcNow;ScreenCapture.CaptureScreenshot(path);yield return Wait(()=>File.Exists(path)&&File.GetLastWriteTimeUtc(path)>=at,8,"Fresh screenshot: "+name);report.screenshots.Add(path);}
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
    void Keys(params Key[] keys){if(testKeyboard!=null)InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(keys));}
    IEnumerator Frames(int n){for(int i=0;i<n;i++)yield return null;}
    IEnumerator CameraSweep(string name)
    {
        var look=motor.GetComponent<PlayerCameraController>();var cam=Camera.main;var pivot=motor.transform.Find("PlayerCameraRoot");
        foreach(float pitch in new[]{-15f,35f})for(int yaw=0;yaw<360;yaw+=90)
        {
            look.SetAngles(yaw,pitch);yield return new WaitForSecondsRealtime(.18f);var d=cam.transform.position-pivot.position;
            bool blocked=Physics.SphereCast(pivot.position,.14f,d.normalized,out var hit,Mathf.Max(0,d.magnitude-.18f),1<<3,QueryTriggerInteraction.Ignore);
            Check(!blocked&&Physics.OverlapSphere(cam.transform.position,.12f,1<<3,QueryTriggerInteraction.Ignore).Length==0,"Camera clear: "+name+" yaw="+yaw+" pitch="+pitch);
        }
    }
    IEnumerator Run()
    {
        yield return Ready(GameSession.SecondChapterScene);Fixture();
        var initial=world.Export();initial.Get(StateSet.Abilities).Remove(WorldIds.AbilityWind);world.Restore(initial);
        inventory.TryAdd(InventoryService.ThuocMau,2);inventory.TryAdd(InventoryService.LaCam,3);
        AppRoot.Instance.Session.LoadChapter(GameSession.FirstChapterScene,SaveService.KimQuyCheckpointId);yield return Ready(GameSession.FirstChapterScene);
        var sound=SoundscapeRuntime.Instance;yield return new WaitForSecondsRealtime(.7f);
        Check(sound&&sound.Palette.name=="C1SoundPalette","C1 keeps its own sound palette");
        var c1bag=Find<InventoryPanel>();c1bag.Open();Check(Get<TMP_Text>(c1bag,"keyItemsText").text.Contains("Mảnh Trống Đồng"),"C1 inventory keeps its existing relic display");c1bag.Close();
        All<ChapterExit>().Single().Interact(motor.gameObject);yield return Ready(GameSession.SecondChapterScene);yield return new WaitForSecondsRealtime(1.2f);
        Check(sound.Palette.name=="C2SoundPalette"&&sound.Ambience.isPlaying&&sound.Ambience.clip==sound.Palette.wind,"Real C1 exit binds C2 palette and running wind ambience");
        Check(inventory.Count(InventoryService.ThuocMau)==2&&inventory.Count(InventoryService.LaCam)==3&&world.Has(StateSet.Abilities,WorldIds.MovementKit),"C1-to-C2 transfer preserves inventory and movement kit");
        var env=All<Transform>().Single(t=>t.name=="ART_C2_Environment");Check(!env.GetComponentsInChildren<Collider>(true).Any(c=>c.enabled),"Art group adds no active collision to traversal");
        Check(RenderSettings.fog&&RenderSettings.fogDensity<.012f&&RenderSettings.sun&&RenderSettings.sun.shadows!=LightShadows.None,"Thin fog and shadowed warm skylight configured");
        Check(sound.Palette.music==sound.MusicA.outputAudioMixerGroup&&sound.Palette.sfx==sound.Ambience.outputAudioMixerGroup,"Music and ambience respect existing mixer groups");
        Check(All<Chapter02Atmosphere>().Length==1&&All<WindColumn>().All(w=>w.GetComponent<AudioSource>()&&w.GetComponent<AudioSource>().outputAudioMixerGroup==sound.Palette.sfx),"One atmosphere owner and seven spatial winds routed to SFX");
        foreach(var p in new[]{new Vector3(0,.1f,-6),new Vector3(4.5f,9.1f,13.6f),new Vector3(6.5f,13.1f,27),new Vector3(5,18.1f,43),new Vector3(5,18.1f,76)})
        {
            Position(p);motor.enabled=true;yield return Frames(8);Check(!Physics.CheckCapsule(motor.transform.position+Vector3.up*.4f,motor.transform.position+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore),"Low-view standing clearance: "+p);
            yield return CameraSweep(p.ToString());motor.GetComponent<PlayerCameraController>().SetAngles(p.z>24&&p.z<35?-25:0,8);yield return new WaitForSecondsRealtime(.25f);yield return Screenshot("p"+(p.z<10?1:p.z<24?2:p.z<40?3:p.z<73?4:5));
        }
        Position(new Vector3(5,18.1f,76));motor.enabled=true;
        var panel=Find<InventoryPanel>();panel.Open();Check(panel.IsOpen&&Time.timeScale==0&&AudioListener.pause,"Inventory opens and pauses world/SFX");
        Check(Get<TMP_Text>(panel,"keyItemsText").text.Contains("Mặt Trời Lông Chim")&&Get<TMP_Text>(panel,"keyItemsText").text.Contains("0/3 mảnh"),"C2 inventory counts only its own fragments and names current relic");
        Check(Get<TMP_Text>(panel,"detailText").text.Contains("lửa trại đáy giếng"),"C2 potion description matches its own crafting station");yield return Screenshot("inventory");panel.Close();
        var journal=Find<JournalPanel>();journal.Open();Check(journal.IsOpen&&Time.timeScale==0,"Journal opens without hidden input trap");journal.Close();
        var s=Find<StoryCardPanel>();s.Show("Giếng Trời Chim Lạc","Kiểm tra bảng lời dẫn và cuộn nội dung.");Check(s.IsOpen&&Time.timeScale==0,"Story panel remains usable after HUD skin");Get<Button>(s,"continueButton").onClick.Invoke();
        Check(Time.timeScale==1&&!AudioListener.pause&&AppRoot.Instance.Session.State==GameSession.SessionState.Playing,"Closing panels restores gameplay time and audio");
        var boss=All<BossChimLac>().Single();boss.Activate();yield return new WaitForSeconds(1.6f);Check((sound.MusicA.clip==sound.Palette.battleMusic&&sound.MusicA.isPlaying)||(sound.MusicB.clip==sound.Palette.battleMusic&&sound.MusicB.isPlaying),"Chim Lac activation crossfades to battle music");boss.ResetBoss();
        // Native persistence remains valid after art/audio are attached.
        Check(Find<SaveService>().SaveAt("c2_cp_start"),"Checkpoint saves with art/audio components installed");yield return Continue();
        Check(Vector3.Distance(motor.transform.position,All<CheckpointAltar>().Single(c=>c.CheckpointId=="c2_cp_start").SpawnPoint.position)<.6f&&inventory.Count(InventoryService.ThuocMau)==2,"Continue returns to safe start and preserves inventory");
        yield return new WaitForSecondsRealtime(1);Check(sound.Palette.name=="C2SoundPalette"&&sound.Ambience.isPlaying,"Audio rebinds once after Continue");
        Check(All<Chapter02Atmosphere>().Length==1,"Continue creates no duplicate atmosphere owner");
        completed=true;
    }
}
#endif

