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
public sealed class Chapter02ArenaProbe : MonoBehaviour
{
    [Serializable] class Report
    {
        public string mode="Editor Play Mode; real C2 boss/arena components, isolated attack coroutines and native save/Continue; real W/RMB/Space/LMB via InputReader for encounter input; C1 completion/initial positions are fixtures; no full C2/manual/build claim";
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
    {if(!SessionState.GetBool("SonDoong.C2ArenaTest",false))return;SessionState.SetBool("SonDoong.C2ArenaTest",false);var g=new GameObject("C2ArenaProbe");DontDestroyOnLoad(g);g.AddComponent<Chapter02ArenaProbe>();}
    void Awake()
    {
        inputOnly=SessionState.GetBool("SonDoong.C2ArenaInputOnly",false);SessionState.SetBool("SonDoong.C2ArenaInputOnly",false);
        Application.logMessageReceived+=Log;oldBackground=Application.runInBackground;Application.runInBackground=true;
        string p=Path.Combine(Application.persistentDataPath,"save_v1.json");backup=new Backup{paths=new[]{p,p+".bak",p+".tmp"}};
        backup.existed=backup.paths.Select(File.Exists).ToArray();backup.hashes=backup.paths.Select(Hash).ToArray();
        folder="Temp/C2ArenaBackup_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");Directory.CreateDirectory(folder);
        for(int i=0;i<backup.paths.Length;i++)if(backup.existed[i])File.Copy(backup.paths[i],folder+"/"+i+".original");
        File.WriteAllText(folder+"/manifest.json",JsonUtility.ToJson(backup,true));progressAt=Time.realtimeSinceStartup;
        var console=typeof(Editor).Assembly.GetType("UnityEditor.ConsoleWindow",true);var flags=console.GetNestedType("ConsoleFlags",BindingFlags.Public|BindingFlags.NonPublic);errorPauseFlag=Enum.Parse(flags,"ErrorPause");
        var bindings=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;originalErrorPause=(bool)console.GetMethod("HasFlag",bindings).Invoke(null,new[]{errorPauseFlag});consoleSetFlag=console.GetMethod("SetFlag",bindings);consoleSetFlag.Invoke(null,new object[]{errorPauseFlag,false});report.notes.Add("Console Error Pause temporarily disabled for expected I/O diagnostic; original="+originalErrorPause);
        SceneManager.sceneLoaded+=Loaded;Isolate();
    }
    void Loaded(Scene s,LoadSceneMode mode)=>Isolate();
    void Isolate(){foreach(var x in All<CheckpointTrigger>())x.enabled=false;foreach(var x in All<ArenaGate>())x.enabled=false;foreach(var x in All<EnemyBrain>())x.enabled=false;foreach(var x in All<ThrowerBrain>())x.enabled=false;}
    void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){if(expectedSaveFailure&&s.Contains("Không lưu được game"))report.expectedErrors.Add(s);else report.errors.Add(s);}}
    void Check(bool ok,string s){if(!ok)throw new InvalidOperationException(s);report.checks.Add(s);progressAt=Time.realtimeSinceStartup;File.WriteAllText("Logs/C2Arena-progress.json",JsonUtility.ToJson(report,true));}
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
        Directory.CreateDirectory("docs/reports");File.WriteAllText(inputOnly?"Logs/C2Arena-input-development.json":"docs/reports/C2_Arena_PlayMode_2026-11-06.json",JsonUtility.ToJson(report,true));EditorApplication.isPlaying=false;
    }
    void OnDestroy(){if(!finished)Finish();}
    IEnumerator Wait(Func<bool> ready,float seconds,string label){float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),label);}
    IEnumerator Ready(string name)
    {yield return Wait(()=>SceneManager.GetActiveScene().name==name&&AppRoot.Instance&&(name==GameSession.MainMenuScene?AppRoot.Instance.Session.State==GameSession.SessionState.MainMenu:AppRoot.Instance.Session.State==GameSession.SessionState.Playing&&Find<SaveService>()?.IsReady==true),55,"Ready: "+name);for(int i=0;i<5;i++)yield return null;if(name!=GameSession.MainMenuScene){motor=Find<PlayerMotor>();vitals=Find<PlayerVitals>();world=Find<WorldStateStore>();inventory=Find<InventoryService>();panel=Find<QuizPanel>();Isolate();}}
    IEnumerator Continue(){AppRoot.Instance.Session.ReturnToMainMenu();yield return Ready(GameSession.MainMenuScene);AppRoot.Instance.Session.Continue();yield return Ready(GameSession.SecondChapterScene);}
    IEnumerator Screenshot(string name)
    {string path="docs/images/c2-arena-"+name+"-2026-11-06.png";Directory.CreateDirectory("docs/images");var at=DateTime.UtcNow;ScreenCapture.CaptureScreenshot(path);yield return Wait(()=>File.Exists(path)&&File.GetLastWriteTimeUtc(path)>=at,8,"Fresh screenshot: "+name);report.screenshots.Add(path);}
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
    BossChimLac boss;EnemyHealth health;ChimLacFocusReceiver focus;WindColumn wind;ArenaGate gate;
    void Keys(params Key[] keys){if(testKeyboard!=null)InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(keys));}
    void Bind(){boss=All<BossChimLac>().Single();health=boss.GetComponent<EnemyHealth>();focus=Get<ChimLacFocusReceiver>(boss,"focus");wind=All<WindColumn>().Single(w=>w.name=="WindColumn_Arena");gate=Find<ArenaGate>();}
    IEnumerator Frames(int n){for(int i=0;i<n;i++)yield return null;}
    IEnumerator Attack(string name)=>(IEnumerator)boss.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(boss,null);
    void StopAI(){var co=Get<Coroutine>(boss,"ai");if(co!=null)boss.StopCoroutine(co);Set(boss,"ai",null);}
    void Progression()
    {
        Fixture();foreach(string id in new[]{"c2_frag_1","c2_frag_2","c2_frag_3"})world.TryAdd(StateSet.Fragments,id);world.TryAdd(StateSet.BossTools,"c2_tool_long_gio_lon");world.TryAdd(StateSet.Doors,"c2_door_arena");world.TryAdd(StateSet.Puzzles,"c2_pz_chuong_gio");world.TryAdd(StateSet.Puzzles,"c2_wind_p3_final");
    }
    IEnumerator Perch(bool stop=true)
    {boss.ResetBoss();boss.Activate();yield return Wait(()=>boss.State==BossChimLac.ChimState.Perched,3,"Boss reaches perched state");if(stop)StopAI();}
    IEnumerator Run()
    {
        yield return Ready(GameSession.SecondChapterScene);Bind();Progression();
        if(!inputOnly){yield return FocusAndStun();yield return Gust();yield return Dives();yield return PhaseAndReset();yield return Persistence();}
        else Check(Find<SaveService>().SaveAt("c2_cp_arena"),"Input fixture saved before encounter");
        yield return InputEncounter();Check(report.errors.Count==0,"No unexpected runtime errors/assertions/exceptions");completed=true;
    }
    IEnumerator FocusAndStun()
    {
        Check(!health.TryTakeDamage(new DamageInfo(motor.gameObject,20,DamageTeam.Player)),"Waiting boss cannot be damaged before encounter");Check(!boss.TrySaveDefeat(),"Live boss cannot save a false victory");
        var doors=world.Export();doors.sets.Single(s=>s.set==StateSet.Doors.ToString()).ids.Remove("c2_door_arena");var original=world.Export();world.Restore(doors);gate.enabled=true;gate.SendMessage("OnTriggerEnter",motor.GetComponent<CharacterController>());Check(!gate.IsLocked&&!boss.IsActive,"Arena trigger cannot bypass the locked progression door");world.Restore(original);AppRoot.Instance.Session.Pause();gate.SendMessage("OnTriggerEnter",motor.GetComponent<CharacterController>());Check(!gate.IsLocked&&!boss.IsActive,"Paused trigger cannot begin boss encounter");AppRoot.Instance.Session.Resume();gate.enabled=false;
        var complete=world.Export();var noTool=world.Export();noTool.sets.Single(s=>s.set==StateSet.BossTools.ToString()).ids.Clear();world.Restore(noTool);Position(wind.transform.position+Vector3.up*.1f);motor.enabled=true;yield return new WaitForSeconds(.5f);Check(!wind.IsActive&&!motor.IsHovering&&motor.transform.position.y<19,"Arena wind unavailable without boss tool");world.Restore(complete);yield return Wait(()=>motor.IsHovering,5,"Actual arena wind lifts player to hover");Check(Mathf.Abs(motor.transform.position.y-24.5f)<.35f,"Arena hover height remains 24.5m");
        yield return Perch(false);Check(focus.CanReceiveFocus,"Hovering in active wind enables perched focus");focus.ReceiveFocus(.2f);AppRoot.Instance.Session.Pause();var p=boss.transform.position;focus.ReceiveFocus(.8f);yield return new WaitForSecondsRealtime(.2f);Check(Mathf.Approximately(focus.Progress,.2f)&&boss.transform.position==p,"Pause freezes perched boss and focus charge");AppRoot.Instance.Session.Resume();focus.ReceiveFocus(float.NaN);focus.ReceiveFocus(float.PositiveInfinity);Check(focus.Progress==.2f,"Invalid focus delta does not corrupt charge");yield return new WaitForSeconds(.3f);focus.ReceiveFocus(.05f);Check(Mathf.Approximately(focus.Progress,.05f),"Focus interruption beyond grace resets charge");
        float end=Time.time+2;while(!focus.StunTriggered&&Time.time<end){focus.ReceiveFocus(Time.deltaTime);yield return null;}yield return Wait(()=>boss.State==BossChimLac.ChimState.Stunned,1,"1.5 seconds sustained focus triggers stun");Check(!focus.CanReceiveFocus,"Stunned boss does not accept extra focus");float start=Time.time;
        health.TryTakeDamage(new DamageInfo(motor.gameObject,20,DamageTeam.Player));health.TryTakeDamage(new DamageInfo(motor.gameObject,20,DamageTeam.Player));Check(health.Current==140&&!health.TryTakeDamage(new DamageInfo(motor.gameObject,20,DamageTeam.Player)),"Stun applies x1.5 damage with strict 60-damage cap");
        Position(new Vector3(5,18.1f,79));yield return new WaitForSeconds(.55f);Check(Mathf.Abs(boss.transform.position.y-18)<.05f,"Stunned boss lands on arena floor");Check(boss.GetComponentInChildren<ChimLacPresentation>().transform.Find("Stun_Stars").gameObject.activeInHierarchy,"Boss stun has stars and bowed proxy");yield return Screenshot("stunned");
        yield return Wait(()=>boss.State==BossChimLac.ChimState.FlyToPerch,4,"Four-second stun returns to flight");Check(Time.time-start>=3.8f,"Stun duration is not shortened by damage cap");Check(!health.TryTakeDamage(new DamageInfo(motor.gameObject,20,DamageTeam.Player)),"Flight is invulnerable");boss.ResetBoss();Check(health.Current==200&&boss.State==BossChimLac.ChimState.Waiting&&!focus.StunTriggered&&!focus.CanReceiveFocus,"Reset clears HP/FSM/focus/invulnerability window");
        Position(new Vector3(5,18.1f,79));yield return Perch();focus.ReceiveFocus(2);Check(focus.Progress==0,"Ground focus cannot bypass wind requirement");boss.ResetBoss();
    }
    IEnumerator Gust()
    {
        wind.enabled=false;Position(new Vector3(5,18.1f,82));motor.enabled=true;yield return new WaitForSeconds(.2f);yield return Perch();Vector3 start=motor.transform.position;var co=StartCoroutine(Attack("Gust"));yield return new WaitForSeconds(.5f);Check(!boss.GustIsActive&&Vector3.Distance(start,motor.transform.position)<.1f,"Gust warning precedes physical push");yield return Wait(()=>boss.GustIsActive,1,"Gust becomes active after warning");
        AppRoot.Instance.Session.Pause();var at=motor.transform.position;yield return new WaitForSecondsRealtime(.2f);Check(motor.transform.position==at,"Pause freezes active gust displacement");AppRoot.Instance.Session.Resume();yield return Wait(()=>!boss.GustIsActive,3,"Gust ends after active window");float distance=Vector3.Distance(start,motor.transform.position);Check(distance>6&&distance<8.8f,"Ground gust pushes at bounded 4m/s over two seconds");boss.ResetBoss();
        wind.enabled=true;Position(wind.transform.position+Vector3.up*.1f);motor.enabled=true;yield return Wait(()=>motor.IsHovering,5,"Arena wind restores hover after gust fixture");yield return Perch();start=motor.transform.position;co=StartCoroutine(Attack("Gust"));yield return Wait(()=>boss.GustIsActive,1.2f,"Shielded gust reaches active window");yield return new WaitForSeconds(.5f);Check(Vector3.Distance(start,motor.transform.position)<.15f,"Active wind shields hovering player from gust");StopCoroutine(co);boss.ResetBoss();Check(!Get<GameObject>(boss,"gustTelegraph").activeSelf&&!boss.GustIsActive,"Reset clears gust telegraph and push phase");wind.enabled=false;
    }
    IEnumerator Dives()
    {
        Position(new Vector3(5,18.1f,82));vitals.RestoreFull();yield return Perch();var marker=Get<Transform>(boss,"diveLandingMarker");var co=StartCoroutine(Attack("DiveAt"));yield return Frames(3);Check(boss.State==BossChimLac.ChimState.DiveTelegraph&&marker.gameObject.activeSelf,"Dive shows path and floor landing marker before hit");Vector3 locked=marker.position;AppRoot.Instance.Session.Pause();var pos=boss.transform.position;yield return new WaitForSecondsRealtime(.2f);Check(boss.transform.position==pos&&marker.position==locked,"Pause freezes dive warning and locked destination");AppRoot.Instance.Session.Resume();yield return Wait(()=>boss.State==BossChimLac.ChimState.Dive,2,"Dive follows one-second warning");yield return new WaitForSeconds(.5f);Check(vitals.CurrentHealth==80,"One full swept dive deals exactly 20 damage once");Check(Mathf.Abs(boss.transform.position.y-18)<.05f&&!marker.gameObject.activeSelf,"Dive ends at floor and clears landing warning");StopCoroutine(co);
        boss.ResetBoss();vitals.RestoreFull();Position(new Vector3(5,18.1f,82));yield return Perch();co=StartCoroutine(Attack("DiveAt"));yield return new WaitForSeconds(.3f);locked=marker.position;Position(new Vector3(5,18.1f,85));yield return new WaitForSeconds(1.2f);Check(vitals.CurrentHealth==100&&Vector3.Distance(new Vector3(boss.transform.position.x,0,boss.transform.position.z),new Vector3(locked.x,0,locked.z))<.05f,"Moving away avoids dive; attack does not home after warning lock");StopCoroutine(co);
        boss.ResetBoss();Position(new Vector3(5,18.1f,82));vitals.RestoreFull();yield return Perch();var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=3;wall.transform.position=new Vector3(6.1f,20,82);wall.transform.localScale=new Vector3(.2f,5,4);Physics.SyncTransforms();co=StartCoroutine(Attack("DiveAt"));yield return new WaitForSeconds(1.6f);Check(vitals.CurrentHealth==100&&boss.transform.position.x>7,"Solid obstacle blocks swept dive and damage");Destroy(wall);StopCoroutine(co);boss.ResetBoss();yield return Frames(2);
    }
    IEnumerator PhaseAndReset()
    {
        Position(new Vector3(5,18.1f,82));vitals.RestoreFull();yield return Perch(false);health.TryTakeDamage(new DamageInfo(motor.gameObject,100,DamageTeam.Player));Check(boss.Enraged&&health.Current==100,"Phase two starts at <=100 HP");float at=Time.time;int last=Get<int>(boss,"diveCount"),telegraphs=0;var previous=boss.State;bool secondAirborne=false;float deadline=Time.time+13;
        while(Time.time<deadline)
        {
            if(boss.State==BossChimLac.ChimState.DiveTelegraph&&previous!=boss.State){telegraphs++;if(telegraphs==2)secondAirborne=boss.transform.position.y>24;}
            previous=boss.State;if(Get<int>(boss,"diveCount")-last>=2&&boss.State==BossChimLac.ChimState.Landed)break;yield return null;
        }
        Check(Get<int>(boss,"diveCount")-last==2&&telegraphs==2&&secondAirborne,"Enraged combo performs two separately warned airborne dives");Check(vitals.CurrentHealth==60,"Two dives each hit once, not once per frame");
        boss.ResetBoss();Check(health.Current==200&&!boss.Enraged&&!Get<Transform>(boss,"diveTelegraph").gameObject.activeSelf&&!Get<Transform>(boss,"diveLandingMarker").gameObject.activeSelf,"Reset after phase two restores clean first-phase encounter");
        yield return Perch();var co=StartCoroutine(Attack("DiveAt"));yield return Frames(2);boss.enabled=false;yield return Frames(2);Check(!Get<Transform>(boss,"diveTelegraph").gameObject.activeSelf&&!Get<Transform>(boss,"diveLandingMarker").gameObject.activeSelf,"Disabling boss hides world-space warnings");StopCoroutine(co);boss.enabled=true;boss.ResetBoss();wind.enabled=true;
    }
    IEnumerator Persistence()
    {
        Position(new Vector3(5,18.2f,66.5f));motor.enabled=true;Check(Find<SaveService>().SaveAt("c2_cp_arena"),"Pre-fight checkpoint saved with canonical C1/C2 progression");File.Copy(backup.paths[0],folder+"/prefight.save",true);yield return Perch(false);health.TryTakeDamage(new DamageInfo(motor.gameObject,60,DamageTeam.Player));vitals.Kill();yield return Frames(2);AppRoot.Instance.Session.Respawn();yield return Ready(GameSession.SecondChapterScene);Bind();
        Check(health.Current==200&&!boss.IsActive&&boss.State==BossChimLac.ChimState.Waiting&&!gate.IsLocked&&!focus.StunTriggered,"Death/Respawn resets damaged boss, gate, focus and HUD");Check(Vector3.Distance(motor.transform.position,All<CheckpointTrigger>().Single().SpawnPoint.position)<.4f,"Death returns to safe pre-arena spawn");
        gate.enabled=true;gate.SendMessage("OnTriggerEnter",motor.GetComponent<CharacterController>());yield return Wait(()=>boss.State==BossChimLac.ChimState.Perched,3,"Gate starts victory-save fixture");Check(gate.IsLocked,"Rear gate is locked before boss defeat");StopAI();var save=Find<SaveService>();Set(save,"ready",false);health.TryTakeDamage(new DamageInfo(motor.gameObject,200,DamageTeam.Player));yield return Frames(3);var hud=Find<ChimLacArenaHUD>();Check(boss.DefeatSavePending&&hud.IsOpen&&Time.timeScale==0&&world.Has(StateSet.Bosses,"c2_boss_chim_lac")&&!gate.IsLocked,"Failed victory save preserves live win, opens retry UI and unlocks arena");Check(!SaveService.TryReadSaved(out var old)||!old.world.sets.Any(s=>s.set==StateSet.Bosses.ToString()&&s.ids.Contains("c2_boss_chim_lac")),"Failed victory is not falsely present in native save");yield return Screenshot("save-retry");
        Set(save,"ready",true);Get<Button>(hud,"retryButton").onClick.Invoke();Check(!boss.DefeatSavePending&&!hud.IsOpen&&AppRoot.Instance.Session.State==GameSession.SessionState.Playing,"Retry button saves once and returns to gameplay");Check(SaveService.TryReadSaved(out var won)&&won.checkpointId=="c2_cp_arena"&&won.world.sets.Any(s=>s.set==StateSet.Bosses.ToString()&&s.ids.Contains("c2_boss_chim_lac")),"Native checkpoint contains successful boss victory");yield return Continue();Bind();boss.ResetBoss();Check(!boss.gameObject.activeSelf&&!boss.IsActive&&!gate.IsLocked&&world.Has(StateSet.Bosses,"c2_boss_chim_lac"),"Continue keeps defeated boss hidden and gate unlocked; Reset cannot resurrect it");
        // Fresh encounter fixture only for real-input test; preserve the native pre-fight checkpoint.
        world.Restore(new WorldSnapshot());AppRoot.Instance.Session.ReturnToMainMenu();yield return Ready(GameSession.MainMenuScene);File.Copy(folder+"/prefight.save",backup.paths[0],true);AppRoot.Instance.Session.Continue();yield return Ready(GameSession.SecondChapterScene);Bind();
    }
    IEnumerator FocusInput()
    {
        const string focusRequest="Library/SonDoongC2Arena.await-focus";
        report.notes.Add("Awaiting Game View; focus="+Application.isFocused+", state="+AppRoot.Instance.Session.State+", gameplayEnabled="+AppRoot.Instance.Input.GameplayEnabled+"; no input guard bypass");
        File.WriteAllText(focusRequest,"Awaiting focused Game View with no buttons held");File.WriteAllText("Logs/C2Arena-progress.json",JsonUtility.ToJson(report,true));float end=Time.realtimeSinceStartup+180;
        while((!Application.isFocused||!AppRoot.Instance.Input.GameplayActive)&&Time.realtimeSinceStartup<end){progressAt=Time.realtimeSinceStartup;yield return null;}
        File.Delete(focusRequest);Check(Application.isFocused&&AppRoot.Instance.Input.GameplayActive,"Real Game View focus and production input guard active");
    }
    IEnumerator Drive(Vector3 target,float tolerance=.25f,bool sprint=false,float seconds=8)
    {
        float end=Time.realtimeSinceStartup+seconds;
        while(true)
        {
            if(!Application.isFocused||!AppRoot.Instance.Input.GameplayActive){Keys();float at=Time.realtimeSinceStartup;yield return FocusInput();end+=Time.realtimeSinceStartup-at;}
            Vector3 d=target-motor.transform.position;d.y=0;if(d.magnitude<tolerance)break;if(vitals.IsDead||Time.realtimeSinceStartup>end)throw new InvalidOperationException("Arena controller route blocked at "+motor.transform.position+" toward "+target);
            motor.GetComponent<PlayerCameraController>().SetAngles(Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg,8);Keys(sprint?new[]{Key.W,Key.LeftShift}:new[]{Key.W});yield return null;
        }
        Keys();yield return Frames(2);Check(true,"Controller reached arena point "+target);
    }
    IEnumerator InputEncounter()
    {
        Bind();yield return FocusInput();oldKeyboard=Keyboard.current;testKeyboard=InputSystem.AddDevice<Keyboard>("C2ArenaKeyboard");testKeyboard.MakeCurrent();oldMouse=Mouse.current;testMouse=InputSystem.AddDevice<Mouse>("C2ArenaMouse");testMouse.MakeCurrent();InputSystem.QueueStateEvent(testMouse,new MouseState());yield return Frames(3);
        boss.ResetBoss();Position(new Vector3(5,18.2f,70));motor.enabled=true;gate.enabled=true;yield return Drive(new Vector3(5,18,76),.25f,true);Check(gate.IsLocked&&boss.IsActive,"Walking through actual entry trigger starts boss and locks rear gate");yield return Screenshot("entry");
        // Isolate aiming from encounter timing: reset on the same floor, then rise using the real wind.
        gate.Unlock();boss.ResetBoss();yield return Drive(wind.transform.position,.15f,true);yield return Wait(()=>motor.IsHovering,5,"Actual arena wind reaches hover during input test");yield return Perch(false);
        Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>().enabled=false;Camera.main.transform.position=motor.transform.position+new Vector3(0,1.5f,-.2f);Camera.main.transform.LookAt(focus.AimPoint.position);InputSystem.QueueStateEvent(testMouse,new MouseState{buttons=2});float end=Time.realtimeSinceStartup+2.6f;while(!focus.StunTriggered&&Time.realtimeSinceStartup<end)yield return null;Check(focus.StunTriggered&&Find<LanternController>().IsFocusing,"Real RMB/InputReader/scanner triggers Chim Lac stun while hovering");InputSystem.QueueStateEvent(testMouse,new MouseState());yield return Wait(()=>boss.State==BossChimLac.ChimState.Stunned,1,"Real-input focus puts boss into stunned state");yield return Screenshot("hover-focus");
        Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>().enabled=true;float before=motor.transform.position.y;Keys(Key.Space);yield return new WaitForSeconds(.1f);Keys();Check(motor.transform.position.y>before+.1f,"Real Space jumps out of arena hover");
        Vector3 perch=Get<Transform[]>(boss,"perchTops")[boss.PerchIndex].position;Vector3 inward=wind.transform.position-perch;inward.y=0;Vector3 destination=perch+inward.normalized*2.9f;destination.y=18;yield return Drive(destination,.2f,true,4);float landEnd=Time.realtimeSinceStartup+4;while(!motor.IsGrounded&&Time.realtimeSinceStartup<landEnd)yield return null;
        report.notes.Add("Landing: pos="+motor.transform.position+", vertical="+Get<float>(motor,"verticalVelocity")+", hovering="+motor.IsHovering+", CCground="+motor.GetComponent<CharacterController>().isGrounded+", state="+AppRoot.Instance.Session.State+", ignoredFocus="+Physics.GetIgnoreCollision(motor.GetComponent<CharacterController>(),focus.GetComponent<Collider>()));Check(motor.IsGrounded,"Player lands near stunned boss");
        var direction=boss.transform.position-motor.transform.position;motor.GetComponent<PlayerCameraController>().SetAngles(Mathf.Atan2(direction.x,direction.z)*Mathf.Rad2Deg,8);float hp=health.Current;InputSystem.QueueStateEvent(testMouse,new MouseState{buttons=1});yield return new WaitForSeconds(.08f);InputSystem.QueueStateEvent(testMouse,new MouseState());yield return new WaitForSeconds(.4f);Check(health.Current<hp,"Actual LMB combat hit damages grounded boss after focus and descent");
        Keys();boss.ResetBoss();gate.Unlock();
    }
}
#endif
