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
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Explicit opt-in: real InputReader/PlayerMotor W + Space, wind triggers, collision and gravity.
// Positional fixtures only initialize/reset negative cases; each reported ascent uses continuous movement.
public sealed class Chapter02TraversalProbe : MonoBehaviour
{
    [Serializable] class Report
    {
        public string mode="Editor Play Mode; synthetic keyboard through real InputReader with production focus guards; real PlayerMotor/CharacterController/trigger/gravity; API quiz/checkpoints; combat AI disabled";
        public List<string> checks=new List<string>(),failures=new List<string>(),errors=new List<string>(),notes=new List<string>(),screenshots=new List<string>();
    }
    [Serializable] class Backup{public string[] paths,hashes;public bool[] existed;}
    readonly Report report=new Report();Backup backup;string folder;bool finished,completed,oldBackground;float lastProgress;
    Keyboard testKeyboard,oldKeyboard;PlayerMotor motor;PlayerVitals vitals;WorldStateStore world;
    static T Find<T>()where T:Component=>UnityEngine.Object.FindAnyObjectByType<T>();
    static T[] All<T>()where T:Component=>UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None);
    static T Get<T>(object o,string n)=>(T)o.GetType().GetField(n,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(o);
    static string Hash(string p){if(!File.Exists(p))return "absent";using(var h=SHA256.Create())return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p)));}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init()
    {
        if(!SessionState.GetBool("SonDoong.C2TraversalTest",false))return;SessionState.SetBool("SonDoong.C2TraversalTest",false);
        var g=new GameObject("C2TraversalProbe");DontDestroyOnLoad(g);g.AddComponent<Chapter02TraversalProbe>();
    }
    void Awake()
    {
        Application.logMessageReceived+=Log;SceneManager.sceneLoaded+=OnScene;
        oldBackground=Application.runInBackground;Application.runInBackground=true;
        folder="Temp/C2TraversalBackup_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");Directory.CreateDirectory(folder);
        string p=Path.Combine(Application.persistentDataPath,"save_v1.json");backup=new Backup{paths=new[]{p,p+".bak",p+".tmp"}};
        backup.existed=backup.paths.Select(File.Exists).ToArray();backup.hashes=backup.paths.Select(Hash).ToArray();
        for(int i=0;i<backup.paths.Length;i++)if(backup.existed[i])File.Copy(backup.paths[i],folder+"/"+i+".original");
        File.WriteAllText(folder+"/manifest.json",JsonUtility.ToJson(backup,true));QuietScene();lastProgress=Time.realtimeSinceStartup;
    }
    void QuietScene(){foreach(var b in All<EnemyBrain>())b.enabled=false;foreach(var b in All<ThrowerBrain>())b.enabled=false;foreach(var a in All<ArenaGate>())a.enabled=false;foreach(var c in All<CheckpointTrigger>())c.enabled=false;}
    void OnScene(Scene s,LoadSceneMode m){if(s.name==GameSession.SecondChapterScene)QuietScene();}
    void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(s);}
    void Check(bool pass,string text)
    {
        if(!pass)throw new InvalidOperationException(text);report.checks.Add(text);lastProgress=Time.realtimeSinceStartup;Progress();
    }
    void Progress(){Directory.CreateDirectory("Logs");File.WriteAllText("Logs/C2Traversal-progress.json",JsonUtility.ToJson(report,true));}
    void Update(){if(!finished&&Time.realtimeSinceStartup-lastProgress>90){report.failures.Add("Stalled; focus="+Application.isFocused+", session="+AppRoot.Instance?.Session.State);Finish();}}
    void Keys(params Key[] keys){if(testKeyboard!=null)InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(keys));}
    IEnumerator Start()
    {
        var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0)
        {
            object current=null;bool more=false;Exception error=null;
            try{more=stack.Peek().MoveNext();if(more)current=stack.Peek().Current;}catch(Exception e){error=e;}
            if(error!=null){report.failures.Add(error.ToString());break;}
            if(!more){stack.Pop();continue;}if(current is IEnumerator nested)stack.Push(nested);else yield return current;
        }
        Finish();
    }
    void Finish()
    {
        if(finished)return;finished=true;if(!completed&&report.failures.Count==0)report.failures.Add("Interrupted before acceptance");
        Keys();Time.timeScale=1;
        if(testKeyboard!=null)InputSystem.RemoveDevice(testKeyboard);if(oldKeyboard!=null)oldKeyboard.MakeCurrent();
        try
        {
            for(int i=0;i<backup.paths.Length;i++){if(backup.existed[i])File.Copy(folder+"/"+i+".original",backup.paths[i],true);else if(File.Exists(backup.paths[i]))File.Delete(backup.paths[i]);}
            Check(backup.paths.Select(Hash).SequenceEqual(backup.hashes),"Personal save/backup/temp restored byte-for-byte");
        }catch(Exception e){report.failures.Add("Restore: "+e);}
        SceneManager.sceneLoaded-=OnScene;Application.logMessageReceived-=Log;Application.runInBackground=oldBackground;
        Directory.CreateDirectory("docs/reports");File.WriteAllText("docs/reports/C2_Traversal_PlayMode_2026-11-05.json",JsonUtility.ToJson(report,true));EditorApplication.isPlaying=false;
    }
    void OnDestroy(){if(!finished)Finish();}
    IEnumerator Wait(Func<bool> ready,float seconds,string label)
    {float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),label);}
    IEnumerator Ready()
    {
        yield return Wait(()=>AppRoot.Instance&&AppRoot.Instance.Session.State==GameSession.SessionState.Playing&&Find<SaveService>()?.IsReady==true,45,"C2 ready for controller traversal");
        motor=Find<PlayerMotor>();vitals=Find<PlayerVitals>();world=Find<WorldStateStore>();
    }
    IEnumerator Focus(bool gameplay=true)
    {
        report.notes.Add("Awaiting Game View: focused="+Application.isFocused+", gameplayEnabled="+AppRoot.Instance.Input.GameplayEnabled+", batch="+Application.isBatchMode+"; no guard bypass");Progress();
        float end=Time.realtimeSinceStartup+180;
        while(!(Application.isFocused&&(!gameplay||AppRoot.Instance.Input.GameplayActive))&&Time.realtimeSinceStartup<end)
        {lastProgress=Time.realtimeSinceStartup;yield return null;}
        Check(Application.isFocused&&(!gameplay||AppRoot.Instance.Input.GameplayActive),"Real Game View focus"+(gameplay?"/input active":" while test is paused"));
    }
    IEnumerator Drive(Vector3 destination,float tolerance=.22f,float timeout=12)
    {
        float end=Time.realtimeSinceStartup+timeout;float lastMoved=Time.realtimeSinceStartup;Vector3 previous=motor.transform.position;
        while(true)
        {
            if(!Application.isFocused||!AppRoot.Instance.Input.GameplayActive)
            {
                Keys();float interrupted=Time.realtimeSinceStartup;
                if(!Application.isFocused)
                {
                    AppRoot.Instance.Session.Pause();yield return Focus(false);AppRoot.Instance.Session.Resume();
                    report.notes.Add("Harness paused real game on OS focus loss; resumed without moving player fixture");
                }
                yield return Focus();end+=Time.realtimeSinceStartup-interrupted;lastMoved=Time.realtimeSinceStartup;previous=motor.transform.position;continue;
            }
            var delta=destination-motor.transform.position;delta.y=0;if(delta.magnitude<tolerance)break;
            if((motor.transform.position-previous).sqrMagnitude>.0025f){previous=motor.transform.position;lastMoved=Time.realtimeSinceStartup;}
            if(Time.realtimeSinceStartup>end||Time.realtimeSinceStartup-lastMoved>2.5f){Keys();throw new InvalidOperationException("Controller blocked towards "+destination+"; position="+motor.transform.position+", hover="+motor.IsHovering);}
            if(vitals.IsDead||AppRoot.Instance.Session.State!=GameSession.SessionState.Playing)throw new InvalidOperationException("Unexpected death/session during path at "+motor.transform.position);
            motor.GetComponent<PlayerCameraController>().SetAngles(Mathf.Atan2(delta.x,delta.z)*Mathf.Rad2Deg,8);Keys(Key.W);yield return null;
        }
        Keys();yield return null;Check(true,"WASD/controller reached "+destination+" at "+motor.transform.position);
    }
    IEnumerator Land(float y,string name)
    {
        yield return Wait(()=>motor.IsGrounded&&!motor.IsHovering,4,"Grounded on "+name);
        Check(Mathf.Abs(motor.transform.position.y-y)<.28f,"Correct landing height "+name+" y="+motor.transform.position.y);
    }
    IEnumerator Ride(string name,Vector3 exit,float landingY,bool pause=false)
    {
        var wind=All<WindColumn>().Single(w=>w.name==name);
        yield return Drive(wind.transform.position,.2f);
        yield return Wait(()=>motor.IsHovering,5,"Actual wind hover: "+name);
        Check(Mathf.Abs(motor.transform.position.y-wind.HoverTopY)<.45f,"Hover top respected: "+name);
        if(pause)
        {
            float y=motor.transform.position.y;AppRoot.Instance.Session.Pause();yield return new WaitForSecondsRealtime(.35f);
            Check(Mathf.Abs(motor.transform.position.y-y)<.02f,"Pause freezes lift and gravity");AppRoot.Instance.Session.Resume();yield return Focus();
        }
        yield return Focus();Keys();yield return null;
        float before=motor.transform.position.y,jumpDeadline=Time.time+2f,releaseAt=0,nextPress=0;bool held=false;int presses=0;
        while(motor.transform.position.y<=before+.15f&&Time.time<jumpDeadline)
        {
            if(!Application.isFocused||!AppRoot.Instance.Input.GameplayActive)
            {
                Keys();held=false;AppRoot.Instance.Session.Pause();yield return Focus(false);AppRoot.Instance.Session.Resume();yield return Focus();
                nextPress=Time.time+.08f;report.notes.Add("Harness paused during jump focus interruption; Space reissued only while hovering");
            }
            if(held&&Time.time>=releaseAt){Keys();held=false;nextPress=Time.time+.08f;}
            if(!held&&motor.IsHovering&&Time.time>=nextPress&&presses<3){Keys(Key.Space);held=true;releaseAt=Time.time+.08f;presses++;}
            yield return null;
        }
        Keys();report.notes.Add("Jump "+name+": real Space presses="+presses+", rise="+(motor.transform.position.y-before)+", focused="+Application.isFocused);
        Check(motor.transform.position.y>before+.15f,"Real Space jump rises from hover: "+name);
        Check(!motor.IsHovering,"Jump releases hovering: "+name);
        yield return Drive(exit,.25f,8);yield return Land(landingY,name+" destination");
    }
    void Quiz(string id)
    {
        var p=All<QuizPedestal>().Single(q=>Get<QuizDefinition>(q,"quiz").quizId==id);Reachable(p);p.Interact(motor.gameObject);
        var panel=Find<QuizPanel>();Check(panel.IsOpen,"Reached real reward pedestal "+id);panel.Choose(panel.Current.correctIndex);
    }
    void Reachable(Component target)
    {
        var origin=Get<Transform>(motor.GetComponent<InteractionScanner>(),"interactionOrigin").position;
        var collider=target.GetComponentsInChildren<Collider>().First(c=>c.enabled&&!c.isTrigger&&c.gameObject.layer==8);
        var point=collider.bounds.center;
        Check(Vector3.Distance(origin,point)<=3.05f&&!Physics.Linecast(origin,point,1<<3,QueryTriggerInteraction.Ignore),"Within hand range and environment LOS: "+target.name);
    }
    IEnumerator CameraSweep(string name)
    {
        Keys();var look=motor.GetComponent<PlayerCameraController>();var cam=Camera.main;var pivot=motor.transform.Find("PlayerCameraRoot");
        foreach(float pitch in new[]{-15f,35f})for(int yaw=0;yaw<360;yaw+=90)
        {
            look.SetAngles(yaw,pitch);yield return new WaitForSecondsRealtime(.18f);
            var d=cam.transform.position-pivot.position;
            bool blocked=Physics.SphereCast(pivot.position,.14f,d.normalized,out var hit,Mathf.Max(0,d.magnitude-.18f),1<<3,QueryTriggerInteraction.Ignore);
            Check(!blocked&&Physics.OverlapSphere(cam.transform.position,.12f,1<<3,QueryTriggerInteraction.Ignore).Length==0,"Camera clear "+name+" yaw="+yaw+" pitch="+pitch+(blocked?" hit="+hit.collider.name:""));
        }
    }
    IEnumerator Screenshot(string name)
    {
        var look=motor.GetComponent<PlayerCameraController>();look.SetAngles(0,8);yield return new WaitForSecondsRealtime(.2f);
        string p="docs/images/c2-traversal-"+name+"-2026-11-05.png";Directory.CreateDirectory("docs/images");var start=DateTime.UtcNow;ScreenCapture.CaptureScreenshot(p);
        yield return Wait(()=>File.Exists(p)&&File.GetLastWriteTimeUtc(p)>=start,8,"Fresh screenshot "+name);report.screenshots.Add(p);
    }
    IEnumerator Run()
    {
        yield return Ready();yield return Focus();oldKeyboard=Keyboard.current;testKeyboard=InputSystem.AddDevice<Keyboard>("C2TraversalKeyboard");testKeyboard.MakeCurrent();Keys();yield return null;yield return null;yield return Focus();
        world.Restore(new WorldSnapshot());
        // Direct C2 Editor entry needs the same canonical C1 prerequisites as a real transfer.
        world.TryAdd(StateSet.Flags,WorldIds.FlagLantern);world.TryAdd(StateSet.Seals,WorldIds.SealGarden);
        foreach(string id in new[]{WorldStateStore.DrumA,WorldStateStore.DrumB,WorldStateStore.DrumC})world.TryAdd(StateSet.Fragments,id);
        foreach(string id in new[]{WorldStateStore.DrumPuzzleId,WorldStateStore.MirrorPuzzleId,WorldIds.RelicRestoredC1})world.TryAdd(StateSet.Puzzles,id);
        world.TryAdd(StateSet.Doors,WorldIds.DoorKimQuy);world.TryAdd(StateSet.Bosses,WorldIds.GuardianBoss);
        world.TryAdd(StateSet.Abilities,WorldIds.MovementKit);Find<LanternController>().ApplySavedState(true);
        yield return Drive(new Vector3(5,0,3.5f));yield return new WaitForSeconds(.35f);
        Check(motor.transform.position.y<.35f&&!motor.IsHovering,"Entering tutorial column without wind does not lift");
        yield return Drive(new Vector3(3.5f,0,-3.7f));yield return Drive(new Vector3(1.6f,0,-3.7f));Reachable(Find<AbilityPickup>());Find<AbilityPickup>().Interact(motor.gameObject);Find<StoryCardPanel>().CloseFromBack();
        Check(world.Has(StateSet.Abilities,WorldIds.AbilityWind),"Wind earned at actual pickup before climbing");Check(Find<SaveService>().SaveAt("c2_cp_start"),"Saved initial wind checkpoint");
        yield return Drive(new Vector3(3.5f,0,-3));yield return Ride("WindColumn_P1_Tut",new Vector3(5,5,6.2f),5,true);yield return CameraSweep("P1 ledge");Quiz("c2_qz_p1");yield return Focus();
        yield return Drive(new Vector3(5,5,8.6f));yield return Drive(new Vector3(5,5,11));
        yield return Ride("WindColumn_P2_1",new Vector3(4.5f,9,13.6f),9);yield return CameraSweep("P2 platform A");yield return Screenshot("platform-a");
        yield return Ride("WindColumn_P2_2",new Vector3(8.6f,13,16.6f),13);yield return CameraSweep("P2 platform B");Quiz("c2_qz_p2");yield return Focus();
        yield return Drive(new Vector3(9.2f,13,20));yield return Drive(new Vector3(9.2f,13,24.6f));yield return Drive(new Vector3(8,13,26));
        var cp=All<CheckpointAltar>().Single(c=>c.CheckpointId=="c2_cp_mid");Reachable(cp);cp.Interact(motor.gameObject);Find<CraftingPanel>()?.Close();
        Check(SaveService.TryReadSaved(out var save)&&save.checkpointId=="c2_cp_mid","Mid checkpoint saved after physical P1-P2 route");
        var mid=world.Export();Check(world.Has(StateSet.Fragments,"c2_frag_1")&&world.Has(StateSet.Fragments,"c2_frag_2"),"Two rewards retained on reaching P3");
        yield return CameraSweep("P3 checkpoint");
        var seq=Find<SequencePuzzleController>();var final=All<WindColumn>().Single(w=>w.name=="WindColumn_P3_Final");Check(!final.IsActive,"Final P3 wind closed before bells");
        var stands=new[]{new Vector3(9.5f,13,27.6f),new Vector3(2.8f,13,34.5f),new Vector3(.5f,13,27.2f)};
        var order=new[]{"bell_2","bell_3","bell_1"};
        for(int i=0;i<3;i++)
        {
            if(i==1){yield return Drive(new Vector3(6.5f,13,27));yield return Drive(new Vector3(3,13,31));}
            if(i==2){yield return Drive(new Vector3(3,13,31));yield return Drive(new Vector3(3,13,27.2f));}
            yield return Drive(stands[i]);var node=All<SequenceInputNode>().Single(n=>Get<string>(n,"nodeId")==order[i]);Reachable(node);Check(node.CanInteract(motor.gameObject),"Bell available after reaching it: "+order[i]);node.Interact(motor.gameObject);yield return new WaitForSeconds(.3f);
        }
        Check(seq.IsSolved&&final.IsActive,"Actual bell sequence activates final wind after walking around P3");
        yield return Drive(new Vector3(3,13,27.2f));yield return Drive(new Vector3(3,13,31));
        yield return Ride("WindColumn_P3_Final",new Vector3(5.6f,18,35.6f),18);yield return CameraSweep("P3 high ledge");Quiz("c2_qz_p3");yield return Focus();
        yield return Drive(new Vector3(5,18,38.6f));yield return Land(18,"P3-to-P4 exit link");yield return Screenshot("p3-exit");
        // The fallen-position fixture isolates KillZone/save restoration, not player skill.
        Keys();motor.Teleport(new Vector3(5,-7.9f,25),Quaternion.identity);
        yield return Wait(()=>vitals.IsDead&&AppRoot.Instance.Session.State==GameSession.SessionState.Dead,4,"Real gravity/kill trigger causes death below map");
        AppRoot.Instance.Session.Respawn();yield return Ready();yield return Focus();
        Check(Vector3.Distance(motor.transform.position,All<CheckpointAltar>().Single(c=>c.CheckpointId=="c2_cp_mid").SpawnPoint.position)<.5f,"Death respawns at saved mid checkpoint");
        Check(!world.Has(StateSet.Fragments,"c2_frag_3")&&!world.Has(StateSet.Puzzles,"c2_pz_chuong_gio")&&!All<WindColumn>().Single(w=>w.name=="WindColumn_P3_Final").IsActive,"Respawn replaces unsaved P3 reward/bell/wind state");
        Check(world.Has(StateSet.Fragments,"c2_frag_2")&&world.Has(StateSet.Abilities,WorldIds.AbilityWind),"Respawn preserves saved P1-P2 progress and wind");
        AppRoot.Instance.Session.ReturnToMainMenu();yield return Wait(()=>AppRoot.Instance.Session.State==GameSession.SessionState.MainMenu,20,"Menu after mid checkpoint");AppRoot.Instance.Session.Continue();yield return Ready();
        Check(Vector3.Distance(motor.transform.position,All<CheckpointAltar>().Single(c=>c.CheckpointId=="c2_cp_mid").SpawnPoint.position)<.5f,"Continue loads the same safe mid checkpoint");
        Check(!Physics.CheckCapsule(motor.transform.position+Vector3.up*.4f,motor.transform.position+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore),"Mid spawn capsule is clear after Continue");
        Check(report.errors.Count==0,"No runtime errors in P1-P3 traversal");completed=true;
    }
}
#endif
