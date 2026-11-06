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
public sealed class Chapter02BridgeProbe : MonoBehaviour
{
    [Serializable] class Report
    {
        public string mode="Editor Play Mode; real C2 physics/components, API combat/doors and native save/Continue; W/Space through InputReader for bridge/gaps; C1 completion/initial positions are fixtures; no full C2/manual/build claim";
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
    {if(!SessionState.GetBool("SonDoong.C2BridgeTest",false))return;SessionState.SetBool("SonDoong.C2BridgeTest",false);var g=new GameObject("C2BridgeProbe");DontDestroyOnLoad(g);g.AddComponent<Chapter02BridgeProbe>();}
    void Awake()
    {
        inputOnly=SessionState.GetBool("SonDoong.C2BridgeInputOnly",false);SessionState.SetBool("SonDoong.C2BridgeInputOnly",false);
        Application.logMessageReceived+=Log;oldBackground=Application.runInBackground;Application.runInBackground=true;
        string p=Path.Combine(Application.persistentDataPath,"save_v1.json");backup=new Backup{paths=new[]{p,p+".bak",p+".tmp"}};
        backup.existed=backup.paths.Select(File.Exists).ToArray();backup.hashes=backup.paths.Select(Hash).ToArray();
        folder="Temp/C2BridgeBackup_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");Directory.CreateDirectory(folder);
        for(int i=0;i<backup.paths.Length;i++)if(backup.existed[i])File.Copy(backup.paths[i],folder+"/"+i+".original");
        File.WriteAllText(folder+"/manifest.json",JsonUtility.ToJson(backup,true));progressAt=Time.realtimeSinceStartup;
        var console=typeof(Editor).Assembly.GetType("UnityEditor.ConsoleWindow",true);var flags=console.GetNestedType("ConsoleFlags",BindingFlags.Public|BindingFlags.NonPublic);errorPauseFlag=Enum.Parse(flags,"ErrorPause");
        var bindings=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;originalErrorPause=(bool)console.GetMethod("HasFlag",bindings).Invoke(null,new[]{errorPauseFlag});consoleSetFlag=console.GetMethod("SetFlag",bindings);consoleSetFlag.Invoke(null,new object[]{errorPauseFlag,false});report.notes.Add("Console Error Pause temporarily disabled for expected I/O diagnostic; original="+originalErrorPause);
        SceneManager.sceneLoaded+=Loaded;Isolate();
    }
    void Loaded(Scene s,LoadSceneMode mode)=>Isolate();
    void Isolate(){foreach(var x in All<CheckpointTrigger>())x.enabled=false;foreach(var x in All<ArenaGate>())x.enabled=false;foreach(var x in All<EnemyBrain>())x.enabled=false;foreach(var x in All<ThrowerBrain>())x.enabled=false;}
    void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){if(expectedSaveFailure&&s.Contains("Không lưu được game"))report.expectedErrors.Add(s);else report.errors.Add(s);}}
    void Check(bool ok,string s){if(!ok)throw new InvalidOperationException(s);report.checks.Add(s);progressAt=Time.realtimeSinceStartup;File.WriteAllText("Logs/C2Bridge-progress.json",JsonUtility.ToJson(report,true));}
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
        Directory.CreateDirectory("docs/reports");File.WriteAllText(inputOnly?"Logs/C2Bridge-input-development.json":"docs/reports/C2_Bridge_PlayMode_2026-11-05.json",JsonUtility.ToJson(report,true));EditorApplication.isPlaying=false;
    }
    void OnDestroy(){if(!finished)Finish();}
    IEnumerator Wait(Func<bool> ready,float seconds,string label){float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),label);}
    IEnumerator Ready(string name)
    {yield return Wait(()=>SceneManager.GetActiveScene().name==name&&AppRoot.Instance&&(name==GameSession.MainMenuScene?AppRoot.Instance.Session.State==GameSession.SessionState.MainMenu:AppRoot.Instance.Session.State==GameSession.SessionState.Playing&&Find<SaveService>()?.IsReady==true),55,"Ready: "+name);for(int i=0;i<5;i++)yield return null;if(name!=GameSession.MainMenuScene){motor=Find<PlayerMotor>();vitals=Find<PlayerVitals>();world=Find<WorldStateStore>();inventory=Find<InventoryService>();panel=Find<QuizPanel>();Isolate();}}
    IEnumerator Continue(){AppRoot.Instance.Session.ReturnToMainMenu();yield return Ready(GameSession.MainMenuScene);AppRoot.Instance.Session.Continue();yield return Ready(GameSession.SecondChapterScene);}
    IEnumerator Screenshot(string name)
    {string path="docs/images/c2-bridge-"+name+"-2026-11-05.png";Directory.CreateDirectory("docs/images");var at=DateTime.UtcNow;ScreenCapture.CaptureScreenshot(path);yield return Wait(()=>File.Exists(path)&&File.GetLastWriteTimeUtc(path)>=at,8,"Fresh screenshot: "+name);report.screenshots.Add(path);}
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
    void ClearShots(){foreach(var p in All<EnemyProjectile>())Destroy(p.gameObject);}
    GameObject Wall(Vector3 p,Vector3 size){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Probe_LOS_Wall";g.layer=3;g.transform.position=p;g.transform.localScale=size;Physics.SyncTransforms();return g;}
    IEnumerator Frames(int count){for(int i=0;i<count;i++)yield return null;}
    IEnumerator Run()
    {
        yield return Ready(GameSession.SecondChapterScene);Fixture();if(!inputOnly){yield return Projectiles();yield return Throwers();yield return Doors();}else{foreach(string id in new[]{"c2_frag_1","c2_frag_2","c2_frag_3"})world.TryAdd(StateSet.Fragments,id);world.TryAdd(StateSet.BossTools,"c2_tool_long_gio_lon");}yield return Traversal();
        Check(report.errors.Count==0,"No unexpected runtime errors/assertions/exceptions");completed=true;
    }
    IEnumerator Projectiles()
    {
        Position(new Vector3(5,18.1f,44));var prefab=Get<EnemyProjectile>(All<ThrowerBrain>().First(),"projectilePrefab");vitals.RestoreFull();
        EnemyProjectile Shot(Vector3 from,Vector3 direction,string id,float speed=9){var p=Instantiate(prefab,from,Quaternion.identity);p.Launch(direction,speed,12,gameObject,id);return p;}
        foreach(var stone in All<Transform>().Where(t=>t.name=="Bridge_Cover_Stone"))
        {Position(new Vector3(stone.position.x,18.1f,stone.position.z-1.4f));var p=Shot(stone.position+Vector3.forward*2,Vector3.back,"cover_"+stone.position.z);yield return Wait(()=>!p,1,"Actual bridge cover blocks projectile at "+stone.position);Check(vitals.CurrentHealth==100,"Bridge cover prevents damage");}
        Position(new Vector3(5,18.1f,44));
        var wall=Wall(new Vector3(5,19,42),new Vector3(1,2,.06f));var shot=Shot(new Vector3(5,19,40),Vector3.forward,"wall");yield return Wait(()=>!shot,2,"Swept projectile stopped by thin Environment wall");Check(vitals.CurrentHealth==100,"Wall prevents projectile damage");Destroy(wall);yield return Frames(2);
        shot=Shot(new Vector3(5,19,40),Vector3.forward,"one_hit");yield return Wait(()=>!shot,2,"Actual projectile reaches player capsule");Check(Mathf.Approximately(vitals.CurrentHealth,88),"Projectile applies exactly 12 damage once");yield return new WaitForSeconds(.2f);Check(vitals.CurrentHealth==88,"Destroyed projectile cannot hit a second time");
        vitals.RestoreFull();shot=Shot(motor.transform.position+Vector3.up*.9f,Vector3.forward,"initial_player");yield return Frames(2);Check(!shot&&vitals.CurrentHealth==88,"Projectile starting inside player is consumed with one hit");
        vitals.RestoreFull();wall=Wall(motor.transform.position+Vector3.up*.9f,Vector3.one*.4f);shot=Shot(wall.transform.position,Vector3.forward,"overlap_priority");yield return Frames(2);Check(!shot&&vitals.CurrentHealth==100,"Initial Environment overlap blocks damage even beside player");Destroy(wall);yield return Frames(2);
        shot=Shot(new Vector3(5,22,43),Vector3.forward,"pause");AppRoot.Instance.Session.Pause();var pos=shot.transform.position;yield return new WaitForSecondsRealtime(.2f);Check(shot&&shot.transform.position==pos,"Pause freezes projectile movement and lifetime");AppRoot.Instance.Session.Resume();Destroy(shot.gameObject);
        shot=Shot(new Vector3(5,28,45),Vector3.up,"lifetime");Set(shot,"lifetime",.12f);yield return Wait(()=>!shot,1,"Unobstructed projectile expires at configured lifetime");shot=Shot(new Vector3(5,28,45),new Vector3(float.NaN,0,1),"invalid");yield return Frames(2);Check(!shot,"Non-finite launch rejected without corrupting scene");
    }
    IEnumerator Throwers()
    {
        foreach(var b in All<ThrowerBrain>().OrderBy(x=>x.name))
        {
            ClearShots();var receiver=b.GetComponent<EnemyLanternReceiver>();var health=b.GetComponent<EnemyHealth>();var line=Get<Transform>(b,"aimTelegraph");
            Position(new Vector3(5,18.1f,51.5f));b.enabled=true;yield return Wait(()=>b.State==ThrowerBrain.ThrowerState.Aim,2,"Ranged aim with bridge LOS: "+b.name);Check(line.gameObject.activeSelf&&b.StateElapsed<b.AimDuration,"Visible aim warning precedes throw: "+b.name);
            float at=b.StateElapsed;AppRoot.Instance.Session.Pause();yield return new WaitForSecondsRealtime(.2f);Check(b.StateElapsed==at,"Pause freezes thrower warning: "+b.name);AppRoot.Instance.Session.Resume();
            var block=Wall((Get<Transform>(b,"hand").position+motor.transform.position+Vector3.up*1.2f)*.5f,Vector3.one*.8f);int count=Get<int>(b,"throws");yield return Frames(3);Check(b.State==ThrowerBrain.ThrowerState.Idle&&!line.gameObject.activeSelf&&Get<int>(b,"throws")==count,"LOS loss cancels warning before projectile: "+b.name);Destroy(block);yield return Frames(2);
            yield return Wait(()=>Get<int>(b,"throws")>count,2,"Actual AI launches after warning: "+b.name);var projectile=All<EnemyProjectile>().Single();var velocity=projectile.Velocity;Position(new Vector3(6,18.1f,53));yield return new WaitForSeconds(.12f);Check(projectile&&projectile.Velocity==velocity&&Mathf.Approximately(velocity.magnitude,7),"Projectile keeps locked direction and 7 m/s: "+b.name);b.enabled=false;ClearShots();
            Position(b.transform.position+Vector3.down*3);b.enabled=true;yield return new WaitForSeconds(.15f);Check(b.State!=ThrowerBrain.ThrowerState.Melee,"Cannot melee through a different floor: "+b.name);b.enabled=false;
            Vector3 near=b.transform.position+(b.name.EndsWith("L")?Vector3.right:Vector3.left)*1.1f;Position(near);vitals.RestoreFull();Set(b,"nextMelee",0f);b.enabled=true;yield return Wait(()=>b.State==ThrowerBrain.ThrowerState.Melee,1,"Close-range windup: "+b.name);yield return new WaitForSeconds(.5f);Check(vitals.CurrentHealth==90,"Melee deals exactly ten damage: "+b.name);count=Get<int>(b,"throws");yield return new WaitForSeconds(.5f);Check(vitals.CurrentHealth==90&&Get<int>(b,"throws")==count,"Melee cooldown neither repeats hit nor throws at close range: "+b.name);
            var head=health.GetComponentsInChildren<Transform>().Single(t=>t.name=="Head");var pose=head.localRotation;var skins=health.GetComponentsInChildren<SkinnedMeshRenderer>();var colors=skins.Select(r=>{var p=new MaterialPropertyBlock();r.GetPropertyBlock(p);return new[]{p.GetColor("_BaseColor"),p.GetColor("_EmissionColor")};}).ToArray();
            AppRoot.Instance.Session.Pause();float progress=Get<float>(receiver,"focusProgress");receiver.ReceiveFocus(1.1f);Check(Get<float>(receiver,"focusProgress")==progress,"Pause cannot charge lantern stun: "+b.name);AppRoot.Instance.Session.Resume();receiver.ReceiveFocus(float.NaN);receiver.ReceiveFocus(float.PositiveInfinity);Check(Get<float>(receiver,"focusProgress")==progress,"Invalid focus delta rejected: "+b.name);
            receiver.ReceiveFocus(1.01f);yield return Frames(4);Check(receiver.IsStunned&&receiver.IsWeakened&&b.State==ThrowerBrain.ThrowerState.Stunned,"Focus interrupts AI and enables stun: "+b.name);Check(receiver.StunRemaining>3.6f&&receiver.StunRemaining<=4,"Four-second shadow stun: "+b.name);yield return new WaitForSeconds(.18f);
            Check(Quaternion.Angle(pose,head.localRotation)>1&&health.transform.Find("VIS_StunStars").gameObject.activeInHierarchy,"Bowed imported rig and visible stars: "+b.name);
            Check(Vector3.Dot(b.transform.forward,(near-b.transform.position).normalized)>.95f&&Quaternion.Angle(health.transform.Find("VIS_ImportedEnemy").localRotation,Quaternion.identity)<.1f,"AI and model share forward attack direction: "+b.name);
            bool same=true;for(int i=0;i<skins.Length;i++){var p=new MaterialPropertyBlock();skins[i].GetPropertyBlock(p);same&=colors[i][0]==p.GetColor("_BaseColor")&&colors[i][1]==p.GetColor("_EmissionColor");}Check(skins.Length>0&&same,"Weakness leaves body colors/emission unchanged: "+b.name);
            Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>().enabled=false;Camera.main.transform.position=b.transform.position+new Vector3(2.8f,1.7f,-2.8f);Camera.main.transform.LookAt(b.transform.position+Vector3.up*1.1f);yield return Screenshot("stunned-"+b.name);
            float hp=vitals.CurrentHealth;count=Get<int>(b,"throws");float end=Time.time+4.1f;while(Time.time<end){receiver.ReceiveFocus(Time.deltaTime);yield return null;}
            Check(!receiver.IsStunned&&receiver.StunImmunityRemaining>2&&vitals.CurrentHealth==hp&&Get<int>(b,"throws")==count,"Continuous hold cannot chain stun or attack during four seconds: "+b.name);
            b.enabled=false;ClearShots();yield return new WaitForSeconds(3.1f);receiver.ReceiveFocus(1.01f);Check(receiver.IsStunned,"Released focus can stun again after recovery immunity: "+b.name);b.enabled=false;
        }
        Position(new Vector3(5,18.1f,66.5f));vitals.RestoreFull();yield return new WaitForSeconds(4.1f);foreach(var b in All<ThrowerBrain>())b.enabled=true;yield return new WaitForSeconds(.8f);Check(All<ThrowerBrain>().All(b=>b.State==ThrowerBrain.ThrowerState.Idle)&&vitals.CurrentHealth==100,"Arena checkpoint spawn is outside both ranged threats");foreach(var b in All<ThrowerBrain>())b.enabled=false;
    }
    IEnumerator Doors()
    {
        Fixture();Position(new Vector3(5,18.2f,66.5f));var door=Find<RequirementDoor>();var save=Find<SaveService>();var baseline=world.Export();
        foreach(string missing in new[]{"c2_frag_1","c2_frag_2","c2_frag_3","c2_tool_long_gio_lon"})
        {world.Restore(baseline);foreach(string id in new[]{"c2_frag_1","c2_frag_2","c2_frag_3"})if(id!=missing)world.TryAdd(StateSet.Fragments,id);if(missing!="c2_tool_long_gio_lon")world.TryAdd(StateSet.BossTools,"c2_tool_long_gio_lon");door.Interact(motor.gameObject);Check(!door.IsOpen&&Get<GameObject>(door,"door").activeSelf,"Door refuses missing prerequisite: "+missing);}
        world.TryAdd(StateSet.BossTools,"c2_tool_long_gio_lon");foreach(string id in new[]{"c2_frag_1","c2_frag_2","c2_frag_3"})world.TryAdd(StateSet.Fragments,id);
        Set(save,"ready",false);string before=Snapshot();door.Interact(motor.gameObject);Check(!door.IsOpen&&Snapshot()==before&&Get<GameObject>(door,"door").activeSelf,"Save-not-ready rolls door back with exact world/inventory");Set(save,"ready",true);
        Check(save.SaveAt("c2_cp_arena"),"Native pre-door arena checkpoint fixture saved");string hash=Hash(backup.paths[0]);
        using(var blocked=new FileStream(backup.paths[2],FileMode.Create,FileAccess.ReadWrite,FileShare.None))
        {expectedSaveFailure=true;door.Interact(motor.gameObject);expectedSaveFailure=false;Check(!door.IsOpen&&Get<GameObject>(door,"door").activeSelf&&Hash(backup.paths[0])==hash,"Locked temporary save file leaves door closed and primary save intact");}
        Check(report.expectedErrors.Count==1,"Exactly one expected I/O diagnostic captured for locked-save test");AimAt(door);door.Interact(motor.gameObject);Check(door.IsOpen&&!Get<GameObject>(door,"door").activeSelf,"All prerequisites and successful save open the actual door");Check(SaveService.TryReadSaved(out var data)&&data.checkpointId=="c2_cp_arena"&&data.world.sets.Any(s=>s.set==StateSet.Doors.ToString()&&s.ids.Contains("c2_door_arena")),"Door opening persists at arena checkpoint");
        yield return Continue();door=Find<RequirementDoor>();Check(door.IsOpen&&world.Has(StateSet.BossTools,"c2_tool_long_gio_lon"),"Continue restores opened door and earned tool");var cp=All<CheckpointTrigger>().Single();Check(Vector3.Distance(motor.transform.position,cp.SpawnPoint.position)<.4f&&Physics.Raycast(motor.transform.position+Vector3.up*.1f,Vector3.down,1,1<<3,QueryTriggerInteraction.Ignore),"Continue lands on safe arena checkpoint floor");
        var enemy=All<ThrowerBrain>().First();enemy.enabled=true;yield return Frames(2);enemy.GetComponent<EnemyHealth>().TryTakeDamage(new DamageInfo(motor.gameObject,100,DamageTeam.Player));yield return Frames(3);Check(enemy.State==ThrowerBrain.ThrowerState.Dead&&!enemy.GetComponent<EnemyLanternReceiver>().CanReceiveFocus,"Thrower death disables attacks and focus target");vitals.Kill();yield return Frames(2);AppRoot.Instance.Session.Respawn();yield return Ready(GameSession.SecondChapterScene);
        Check(All<ThrowerBrain>().All(b=>b.GetComponent<EnemyHealth>().Current==50&&!b.GetComponent<EnemyLanternReceiver>().IsStunned)&&Find<RequirementDoor>().IsOpen,"Death/Respawn reloads ordinary enemies at full health and preserves saved door");
        yield return Frames(3);
    }
    void Keys(params Key[] keys){if(testKeyboard!=null)InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(keys));}
    IEnumerator Focus()
    {
        report.notes.Add("Awaiting Game View; no input guard bypass");File.WriteAllText("Logs/C2Bridge-progress.json",JsonUtility.ToJson(report,true));float end=Time.realtimeSinceStartup+180;
        while((!Application.isFocused||!AppRoot.Instance.Input.GameplayActive)&&Time.realtimeSinceStartup<end){progressAt=Time.realtimeSinceStartup;yield return null;}
        Check(Application.isFocused&&AppRoot.Instance.Input.GameplayActive,"Real Game View focus and InputReader gameplay active");
    }
    IEnumerator Drive(Vector3 target,float tolerance=.22f,float limit=10,bool jump=false)
    {
        float end=Time.realtimeSinceStartup+limit;bool sentJump=false;float release=0,startY=motor.transform.position.y,peak=startY;
        while(true)
        {
            if(!Application.isFocused||!AppRoot.Instance.Input.GameplayActive){Keys();float at=Time.realtimeSinceStartup;AppRoot.Instance.Session.Pause();report.notes.Add("Awaiting Game View; path paused on focus loss");File.WriteAllText("Logs/C2Bridge-progress.json",JsonUtility.ToJson(report,true));while(!Application.isFocused&&Time.realtimeSinceStartup-at<180){progressAt=Time.realtimeSinceStartup;yield return null;}AppRoot.Instance.Session.Resume();yield return Focus();end+=Time.realtimeSinceStartup-at;}
            Vector3 delta=target-motor.transform.position;delta.y=0;if(delta.magnitude<tolerance)break;
            peak=Mathf.Max(peak,motor.transform.position.y);
            if(vitals.IsDead||Time.realtimeSinceStartup>end)throw new InvalidOperationException("Controller path failed at "+motor.transform.position+" toward "+target);
            motor.GetComponent<PlayerCameraController>().SetAngles(Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,8);
            if(jump&&!sentJump){Keys(Key.W,Key.Space);sentJump=true;release=Time.time+.09f;}else if(!jump||Time.time>=release)Keys(Key.W);
            yield return null;
        }
        Keys();yield return Frames(2);Check(true,"Controller reached "+target+" at "+motor.transform.position);if(jump)Check(peak>startY+.15f,"Actual Space input raises player during jump path");
    }
    IEnumerator JumpGap(float edge,float end)
    {
        yield return Drive(new Vector3(5,18,edge-.65f));yield return Wait(()=>motor.IsGrounded,2,"Grounded before bridge gap "+edge);float peak=motor.transform.position.y;
        var drive=Drive(new Vector3(5,18,end+.85f),.2f,4,true);while(drive.MoveNext()){peak=Mathf.Max(peak,motor.transform.position.y);yield return drive.Current;}
        yield return Wait(()=>motor.IsGrounded,3,"Landed after bridge gap "+edge);Check(peak>18.5f&&Mathf.Abs(motor.transform.position.y-18)<.3f,"Space crosses 1.5m gap with real jump/gravity: "+edge);
    }
    IEnumerator Traversal()
    {
        yield return Focus();oldKeyboard=Keyboard.current;testKeyboard=InputSystem.AddDevice<Keyboard>("C2BridgeKeyboard");testKeyboard.MakeCurrent();
        oldMouse=Mouse.current;testMouse=InputSystem.AddDevice<Mouse>("C2BridgeMouse");testMouse.MakeCurrent();InputSystem.QueueStateEvent(testMouse,new MouseState());yield return Frames(3);
        var beforeDoor=world.Export();beforeDoor.sets.Single(s=>s.set==StateSet.Doors.ToString()).ids.Remove("c2_door_arena");world.Restore(beforeDoor);var actualDoor=Find<RequirementDoor>();Position(new Vector3(5,18.2f,69.8f));motor.enabled=true;Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>().enabled=false;Camera.main.transform.position=motor.transform.position+Vector3.up*1.45f;Camera.main.transform.LookAt(new Vector3(5,20,72));
        yield return Wait(()=>Get<IInteractable>(Find<InteractionScanner>(),"currentTarget")==actualDoor,1,"Actual scanner selects door through Environment collider and parent script");Keys(Key.E);yield return new WaitForSeconds(.1f);Keys();Check(actualDoor.IsOpen,"Actual E input opens door and clears physical leaf");
        Position(new Vector3(5,18.1f,51.5f));motor.enabled=true;
        foreach(var b in All<ThrowerBrain>().OrderBy(x=>x.name))
        {
            var receiver=b.GetComponent<EnemyLanternReceiver>();Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>().enabled=false;Camera.main.transform.position=motor.transform.position+Vector3.up*1.45f;Camera.main.transform.LookAt(receiver.AimPoint.position);
            if(b.name.EndsWith("R"))
            {
                var source=Get<Transform>(Find<LanternFocusScanner>(),"focusSource");var wall=Wall((source.position+receiver.AimPoint.position)*.5f,Vector3.one*.8f);InputSystem.QueueStateEvent(testMouse,new MouseState{buttons=2});yield return new WaitForSeconds(1.15f);Check(Find<LanternController>().IsFocusing&&!receiver.IsStunned&&!receiver.IsWeakened,"Actual RMB scanner respects Environment occlusion");InputSystem.QueueStateEvent(testMouse,new MouseState());Destroy(wall);yield return new WaitForSeconds(.3f);
            }
            InputSystem.QueueStateEvent(testMouse,new MouseState{buttons=2});yield return Wait(()=>Find<LanternController>().IsFocusing,.7f,"Right mouse input enters actual lantern Focus");float until=Time.realtimeSinceStartup+2;while(!receiver.IsStunned&&Time.realtimeSinceStartup<until)yield return null;
            var scanner=Find<LanternFocusScanner>();var origin=Get<Transform>(scanner,"focusSource").position;var direction=receiver.AimPoint.position-origin;bool blocked=Physics.Raycast(origin,direction.normalized,out var hit,direction.magnitude,Get<LayerMask>(scanner,"visibilityMask"),QueryTriggerInteraction.Ignore);
            report.notes.Add("Focus diagnostic "+b.name+": source="+origin+", target="+receiver.AimPoint.position+", camera="+Camera.main.transform.forward+", scannerAim="+Get<Transform>(scanner,"aimTransform").forward+", progress="+Get<float>(receiver,"focusProgress")+", focusing="+Find<LanternController>().IsFocusing+", focus="+Application.isFocused+", blocked="+(blocked?hit.collider.name:"none")+", targetMask="+Get<LayerMask>(scanner,"targetMask").value+", overlaps="+Physics.OverlapSphere(origin,10,Get<LayerMask>(scanner,"targetMask"),QueryTriggerInteraction.Ignore).Length);
            Check(receiver.IsStunned,"Actual RMB/InputReader/scanner stuns ledge thrower: "+b.name);Check(receiver.IsWeakened&&receiver.DamageMultiplier==1.5f,"Focused thrower keeps weakness multiplier");InputSystem.QueueStateEvent(testMouse,new MouseState());yield return Frames(4);
        }
        Position(new Vector3(5,18.2f,39.5f));motor.enabled=true;var cp=All<CheckpointTrigger>().Single();Set(cp,"savedThisSession",false);cp.enabled=true;SaveService.TryReadSaved(out var earlier);Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>().enabled=true;yield return Frames(4);yield return Drive(new Vector3(5,18,46));yield return JumpGap(48,49.5f);yield return JumpGap(56,57.5f);yield return Drive(new Vector3(5,18,66.5f));
        Check(!vitals.IsDead&&motor.IsGrounded,"Main bridge route reaches arena yard through both gaps without teleport");Check(SaveService.TryReadSaved(out var autoSaved)&&autoSaved.timestampUtcTicks>earlier.timestampUtcTicks&&autoSaved.checkpointId=="c2_cp_arena","Walking into actual checkpoint trigger writes native arena save");yield return Screenshot("yard");
        // Initial placement at the middle bridge segment is a fixture; each side ascent uses wind + Space.
        foreach(string name in new[]{"WindColumn_P4_L","WindColumn_P4_R"})
        {
            Position(new Vector3(5,18.2f,51.5f));motor.enabled=true;yield return Frames(4);var w=All<WindColumn>().Single(x=>x.name==name);yield return Drive(w.transform.position,.18f);yield return Wait(()=>motor.IsHovering,5,"Actual side-column hover: "+name);float before=motor.transform.position.y;
            float x=name.EndsWith("L")?.2f:9.8f;yield return Drive(new Vector3(x,21,51.5f),.2f,4,true);yield return Wait(()=>motor.IsGrounded&&!motor.IsHovering,4,"Grounded on thrower ledge: "+name);Check(Mathf.Abs(motor.transform.position.y-21)<.3f&&before>21,"Side ledge reached with real wind/Space: "+name);
        }
        Position(new Vector3(5,18.2f,66.5f));motor.enabled=true;foreach(var b in All<ThrowerBrain>())b.enabled=true;foreach(var e in All<EnemyBrain>())e.enabled=true;yield return new WaitForSeconds(1);
        Check(vitals.CurrentHealth==100&&!All<ThrowerBrain>().Any(b=>b.State==ThrowerBrain.ThrowerState.Aim||b.State==ThrowerBrain.ThrowerState.Melee),"Live enemies do not attack restored checkpoint spawn");
        Keys();
    }
}
#endif
