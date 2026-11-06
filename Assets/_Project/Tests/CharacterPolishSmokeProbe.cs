#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Controls;

// Opt-in regression probe, created only by the environment Editor tool.
// It uses the existing actor/controller, never changes player or enemy assets.
public sealed class CharacterPolishSmokeProbe : MonoBehaviour
{
    [Serializable] public class Report
    {
        public string mode="Unity Editor Play Mode (API/controller probes, virtual keyboard/mouse through Input System; not manual play)";
        public List<string> checks=new List<string>();
        public List<string> failures=new List<string>();
        public List<string> errors=new List<string>();
    }
    [Serializable] class Backup { public string[] paths;public bool[] existed;public string[] hashes; }
    readonly Report report=new Report();
    Backup original;string folder;bool finished;
    Keyboard testKeyboard, originalKeyboard;
    Mouse testMouse, originalMouse;
    float oldCapture; int oldFrameRate, oldVsync;
    PlayerMotor motor;CharacterController controller;CampCraftingTable table;CheckpointAltar checkpoint;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        if(!SessionState.GetBool("SonDoong.CharacterTest",false))return;
        SessionState.SetBool("SonDoong.CharacterTest",false);
        var go=new GameObject("CharacterPolishSmokeProbe");DontDestroyOnLoad(go);go.AddComponent<CharacterPolishSmokeProbe>();
    }
    static T Find<T>() where T:Component=>UnityEngine.Object.FindAnyObjectByType<T>();
    void Check(bool pass,string message)
    {
        if(!pass)throw new InvalidOperationException(message);
        report.checks.Add(message);
        File.WriteAllText("Logs/FocusCombat-progress.json",JsonUtility.ToJson(report,true));
    }
    void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(text);}
    static string Hash(string path){if(!File.Exists(path))return "absent";using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)));}
    void BackupFiles()
    {
        folder="Temp/CharacterPolishPlayBackup_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");Directory.CreateDirectory(folder);
        string primary=Path.Combine(Application.persistentDataPath,"save_v1.json");
        var paths=new[]{primary,primary+".bak",primary+".tmp"};
        original=new Backup{paths=paths,existed=paths.Select(File.Exists).ToArray(),hashes=paths.Select(Hash).ToArray()};
        for(int i=0;i<paths.Length;i++)if(original.existed[i])File.Copy(paths[i],folder+"/"+i+".original");
        File.WriteAllText(folder+"/manifest.json",JsonUtility.ToJson(original,true));
    }
    void RestoreFiles()
    {
        if(original==null)return;
        for(int i=0;i<original.paths.Length;i++)
        {
            if(original.existed[i])File.Copy(folder+"/"+i+".original",original.paths[i],true);
            else if(File.Exists(original.paths[i]))File.Delete(original.paths[i]);
        }
        Check(original.paths.Select(Hash).SequenceEqual(original.hashes),"Original primary/backup/tmp saves restored byte-for-byte");original=null;
    }
    IEnumerator Start()
    {
        Application.logMessageReceived+=Log;Application.runInBackground=true;
        var routines=new Stack<IEnumerator>();routines.Push(Run());
        while(routines.Count>0)
        {
            bool more=false;object current=null;Exception error=null;
            try{more=routines.Peek().MoveNext();if(more)current=routines.Peek().Current;}
            catch(Exception e){error=e;}
            if(error!=null){report.failures.Add(error.ToString());break;}
            if(!more){routines.Pop();continue;}
            if(current is IEnumerator nested)routines.Push(nested);else yield return current;
        }
        Finish();
    }
    void Finish()
    {
        if(finished)return;finished=true;
        try{RestoreFiles();}catch(Exception e){report.failures.Add("Save restoration: "+e);}
        Time.captureDeltaTime=oldCapture; Application.targetFrameRate=oldFrameRate; QualitySettings.vSyncCount=oldVsync;
        if(testKeyboard!=null)InputSystem.RemoveDevice(testKeyboard);
        if(testMouse!=null)InputSystem.RemoveDevice(testMouse);
        if(originalKeyboard!=null)originalKeyboard.MakeCurrent();
        if(originalMouse!=null)originalMouse.MakeCurrent();
        Application.logMessageReceived-=Log;
        Directory.CreateDirectory("docs/reports");File.WriteAllText("docs/reports/FocusCombat_PlayMode_2026-11-04.json",JsonUtility.ToJson(report,true));
        string previous=SessionState.GetString("SonDoong.CharacterTest.PreviousScene",string.Empty);
        EditorApplication.isPlaying=false;
        if(!string.IsNullOrEmpty(previous))EditorApplication.delayCall+=()=>
        {
            if(!EditorApplication.isPlayingOrWillChangePlaymode)UnityEditor.SceneManagement.EditorSceneManager.OpenScene(previous);
        };
    }
    void OnDestroy(){if(!finished)Finish();}
    IEnumerator WaitScene(string name)
    {
        float until=Time.realtimeSinceStartup+60;
        while(Time.realtimeSinceStartup<until)
        {
            if(SceneManager.GetActiveScene().name==name&&AppRoot.Instance!=null&&
                (name==GameSession.MainMenuScene||AppRoot.Instance.Session.State==GameSession.SessionState.Playing)&&
                (name!=GameSession.FirstChapterScene||Find<SaveService>()?.IsReady==true))
            {yield return null;yield return null;yield break;}
            yield return null;
        }
    }
    void Bind()
    {
        motor=Find<PlayerMotor>();controller=motor.GetComponent<CharacterController>();table=Find<CampCraftingTable>();
        checkpoint=UnityEngine.Object.FindObjectsByType<CheckpointAltar>().Single(c=>c.CheckpointId=="cp_trai");
    }
    Vector3 home = new Vector3(1000f, 20f, 1000f);
    PlayerActionState actions;
    PlayerStamina stamina;
    PlayerVitals vitals;
    CombatController combat;
    DodgeController dodge;
    void Keys(params Key[] keys) => InputSystem.QueueStateEvent(testKeyboard, new KeyboardState(keys));
    void MouseButton(bool down) => InputSystem.QueueStateEvent(testMouse, new MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left,down));
    IEnumerator Frames(int count) { for(int i=0;i<count;i++)yield return null; }
    IEnumerator Settle()
    {
        Keys();MouseButton(false); motor.Teleport(home,Quaternion.identity);
        motor.GetComponent<PlayerCameraController>().SetAngles(0,0);
        stamina.RestoreFull();yield return Frames(8);
    }
    IEnumerator Swing()
    {
        MouseButton(true);yield return Frames(2);MouseButton(false);yield return new WaitForSeconds(.75f);
    }
    GameObject Box(string label,Vector3 center,Vector3 scale)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=label;go.layer=3;
        go.transform.position=center;go.transform.localScale=scale;return go;
    }
    void MeleeSnapshot(string name)
    {
        var cameraGo=new GameObject("MeleeReviewCamera");var cam=cameraGo.AddComponent<Camera>();
        cam.fieldOfView=38;cam.nearClipPlane=.05f;cam.farClipPlane=15;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.12f,.16f,.2f);
        cameraGo.transform.position=motor.transform.position+new Vector3(2.8f,1.9f,3.3f);cameraGo.transform.LookAt(motor.transform.position+Vector3.up*1.05f);
        var target=new RenderTexture(900,1000,24);target.Create();var previous=RenderTexture.active;var texture=new Texture2D(900,1000,TextureFormat.RGB24,false);
        try
        {
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});
            RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,900,1000),0,0);texture.Apply();
            Directory.CreateDirectory("docs/images");File.WriteAllBytes("docs/images/focus-fix-melee-"+name+"-2026-11-04.png",texture.EncodeToPNG());
        }
        finally{RenderTexture.active=previous;target.Release();Destroy(target);Destroy(texture);Destroy(cameraGo);}
    }
    EnemyLanternReceiver FocusEnemyAt(float distance,EnemyDefinition definition)
    {
        var go=GameObject.CreatePrimitive(PrimitiveType.Capsule);go.SetActive(false);go.name="FocusDistanceEnemy";go.layer=7;
        go.transform.position=home+Vector3.up+Vector3.forward*distance;
        var health=go.AddComponent<EnemyHealth>();var receiver=go.AddComponent<EnemyLanternReceiver>();
        var aim=new GameObject("FocusAim");aim.layer=10;aim.transform.SetParent(go.transform,false);aim.transform.localPosition=Vector3.up*.3f;
        aim.AddComponent<SphereCollider>().radius=.2f;
        var hp=new SerializedObject(health);hp.FindProperty("definition").objectReferenceValue=definition;hp.ApplyModifiedPropertiesWithoutUndo();
        var focus=new SerializedObject(receiver);focus.FindProperty("definition").objectReferenceValue=definition;focus.FindProperty("aimPoint").objectReferenceValue=aim.transform;
        focus.FindProperty("tintRenderer").objectReferenceValue=go.GetComponent<Renderer>();focus.ApplyModifiedPropertiesWithoutUndo();go.SetActive(true);Physics.SyncTransforms();return receiver;
    }
    IEnumerator Run()
    {
        oldCapture=Time.captureDeltaTime;oldFrameRate=Application.targetFrameRate;oldVsync=QualitySettings.vSyncCount;
        originalKeyboard=Keyboard.current;originalMouse=Mouse.current;
        BackupFiles();yield return WaitScene(GameSession.MainMenuScene);
        Check(SceneManager.GetActiveScene().name==GameSession.MainMenuScene,"Boot reaches menu");
        AppRoot.Instance.Session.NewGame();yield return WaitScene(GameSession.FirstChapterScene);Bind();
        var viewType=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        if(viewType!=null)EditorWindow.GetWindow(viewType).Focus();
        yield return new WaitForSecondsRealtime(.3f);
        float focusDeadline=Time.realtimeSinceStartup+8f;
        while(!Application.isFocused&&Time.realtimeSinceStartup<focusDeadline)
        {
            if(viewType!=null)EditorWindow.GetWindow(viewType).Focus();
            yield return new WaitForSecondsRealtime(.25f);
        }
        Check(Application.isFocused,"Unity focused for real Input System action tests");
        testKeyboard=InputSystem.AddDevice<Keyboard>("CharacterTestKeyboard");testKeyboard.MakeCurrent();
        testMouse=InputSystem.AddDevice<Mouse>("CharacterTestMouse");testMouse.MakeCurrent();
        actions=motor.GetComponent<PlayerActionState>();stamina=motor.GetComponent<PlayerStamina>();
        vitals=motor.GetComponent<PlayerVitals>();combat=motor.GetComponent<CombatController>();dodge=motor.GetComponent<DodgeController>();
        Check(actions!=null,"Prefab instance has shared action policy");
        var floor=Box("CharacterTestFloor",home+Vector3.down*.3f,new Vector3(60,.6f,60));Physics.SyncTransforms();
        QualitySettings.vSyncCount=0;Application.targetFrameRate=120;
        foreach(int fps in new[]{30,60,120})
        {
            Time.captureDeltaTime=1f/fps;
            yield return Settle();Check(motor.IsGrounded,"Grounded on flat floor @"+fps);
            Vector3 before=motor.transform.position;
            Keys(Key.W);yield return Frames(fps);Keys();yield return Frames(1);
            float walk=Vector3.Distance(before,motor.transform.position);
            Check(walk>3.05f&&walk<3.7f,"Walk speed without excess inertia @"+fps+" = "+walk);
            Vector3 released=motor.transform.position;yield return Frames(Mathf.CeilToInt(fps*.2f));
            Check(Vector3.Distance(released,motor.transform.position)<.2f,"Quick stop @"+fps);
            yield return Settle();before=motor.transform.position;
            Keys(Key.W,Key.D);yield return Frames(fps);Keys();yield return Frames(1);
            float diagonal=Vector3.Distance(before,motor.transform.position);
            Check(Mathf.Abs(diagonal-walk)<.18f,"Diagonal speed normalized @"+fps);
            yield return Settle();before=motor.transform.position;
            Keys(Key.W,Key.LeftShift);yield return Frames(fps);Keys();yield return Frames(1);
            Check(Vector3.Distance(before,motor.transform.position)>4.8f,"Sprint speed @"+fps);
            Check(stamina.CurrentStamina<94f&&stamina.CurrentStamina>89f,"Sprint stamina cost @"+fps);
            yield return Settle();float top=home.y;
            Keys(Key.Space);yield return Frames(2);Keys();
            for(int frame=0;frame<fps;frame++){top=Mathf.Max(top,motor.transform.position.y);yield return null;}
            Check(top-home.y>1.05f&&top-home.y<1.35f,"Jump height @"+fps+" = "+(top-home.y));
            Check(motor.IsGrounded,"Jump lands @"+fps);
        }
        Time.captureDeltaTime=1f/60;
        yield return Settle();
        var poseAnimator=motor.GetComponent<Animator>();int meleeLayer=poseAnimator.GetLayerIndex("Melee");
        var weapon=motor.GetComponent<PlayerMeleeVisual>();
        Check(weapon!=null&&weapon.KnifeGrip!=null&&weapon.KnifeGrip.parent==poseAnimator.GetBoneTransform(HumanBodyBones.RightHand),"Knife is attached to the actual right-hand bone");
        Check(meleeLayer>=0&&poseAnimator.GetLayerIndex("KnifeGrip")>=0,"Melee action and finger-grip layers configured");
        for(int strike=0;strike<2;strike++)
        {
            float footY=poseAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).position.y;
            Vector3 wrist=poseAnimator.GetBoneTransform(HumanBodyBones.RightHand).position;
            MouseButton(true);yield return Frames(2);MouseButton(false);yield return Frames(10);yield return new WaitForEndOfFrame();
            string state=strike==0?"Slash1":"Slash2";
            Check(poseAnimator.GetCurrentAnimatorStateInfo(meleeLayer).shortNameHash==Animator.StringToHash(state),"Melee clip plays "+state+" without stale attack triggers");
            Check(Vector3.Distance(wrist,poseAnimator.GetBoneTransform(HumanBodyBones.RightHand).position)>.2f,"Humanoid wrist follows visible swing path for "+state);
            Check(Mathf.Abs(poseAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).position.y-footY)<.08f,"Foot height remains stable during "+state);
            Check(Vector3.Distance(weapon.KnifeGrip.position,poseAnimator.GetBoneTransform(HumanBodyBones.RightHand).position)<.08f,"Knife stays in palm during "+state);
            Check(combat.IsInDamageWindow&&!weapon.IsTrailEmitting,"Knife damage window has no luminous beam/ribbon for "+state);
            MeleeSnapshot(state);yield return Frames(23);
            Check(!weapon.IsTrailEmitting,"Blade trail stops during recovery for "+state);
            yield return Frames(9);
        }
        yield return Settle();
        var cameraControl=motor.GetComponent<PlayerCameraController>();
        float sensitivity=AppRoot.Instance.Settings.MouseSensitivity;
        InputSystem.QueueStateEvent(testMouse,new MouseState {delta=new Vector2(100,0)});yield return Frames(2);
        Check(Mathf.Abs(Mathf.DeltaAngle(cameraControl.Yaw,100*sensitivity))<1f,"Mouse look respects configured sensitivity");
        InputSystem.QueueStateEvent(testMouse,new MouseState {delta=new Vector2(0,10000)});yield return Frames(2);
        Check(Mathf.Abs(cameraControl.Pitch+35f)<.1f,"Mouse up looks up and reaches upward pitch limit");
        InputSystem.QueueStateEvent(testMouse,new MouseState {delta=new Vector2(0,-10000)});yield return Frames(2);
        Check(Mathf.Abs(cameraControl.Pitch-65f)<.1f,"Mouse down looks down and reaches downward pitch limit");
        foreach(float angle in new[]{0f,90f,-90f,180f})
        {
            yield return Settle();cameraControl.SetAngles(angle,0);
            Vector3 forward=Quaternion.Euler(0,angle,0)*Vector3.forward;
            var aimDummy=new GameObject("CameraAimRegression");aimDummy.layer=7;
            aimDummy.transform.position=home+forward*1.2f;
            var aimTarget=aimDummy.AddComponent<CharacterPolishDamageTarget>();
            var aimCollider=aimDummy.AddComponent<CapsuleCollider>();aimCollider.center=Vector3.up;aimCollider.height=1.8f;aimCollider.radius=.3f;
            Physics.SyncTransforms();MouseButton(true);yield return Frames(2);MouseButton(false);
            Check(Vector3.Dot(motor.transform.forward,forward)>.999f,"Stationary attack faces camera yaw "+angle);
            Check(Mathf.Abs(Mathf.DeltaAngle(motor.transform.Find("PlayerCameraRoot").eulerAngles.y,angle))<.1f,"Actor attack rotation does not pull world camera pivot yaw "+angle);
            yield return Frames(45);Check(aimTarget.Hits==1,"Camera-facing strike hits correct target yaw "+angle);
            Destroy(aimDummy);yield return Frames(1);
        }
        yield return Settle();Keys(Key.S);yield return Frames(12);
        Vector3 attackStart=motor.transform.position;
        MouseButton(true);yield return Frames(2);MouseButton(false);Keys();
        Check(Vector3.Dot(motor.transform.forward,Vector3.forward)>.999f,"Attack while retreating follows camera, not backward movement");
        Check(Vector3.Distance(motor.transform.position,attackStart)<.05f,"Attack stops stale movement without sliding backward");
        yield return Frames(45);yield return Settle();Keys(Key.A);
        InputSystem.QueueStateEvent(testMouse,new MouseState {delta=new Vector2(100,0)}.WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left,true));
        yield return Frames(2);MouseButton(false);Keys();
        Check(Mathf.Abs(Mathf.DeltaAngle(motor.transform.eulerAngles.y,100*sensitivity))<1f,"Mouse turn and attack in same frame use current view, not WASD or last rendered camera");
        yield return Frames(45);
        yield return Settle();
        floor.GetComponent<Collider>().enabled=false;yield return Frames(3);
        Check(!motor.IsGrounded,"Losing support clears grounded state");
        Keys(Key.Space);yield return Frames(2);Keys();yield return Frames(12);
        Check(motor.transform.position.y>home.y+.5f,"Coyote jump works shortly after leaving ledge");
        floor.GetComponent<Collider>().enabled=true;
        yield return Settle();
        var wall=Box("CharacterTestWall",home+new Vector3(0,1.5f,2),new Vector3(8,3,.3f));Physics.SyncTransforms();
        Keys(Key.W);yield return Frames(80);Keys();
        Check(motor.transform.position.z<home.z+1.7f,"Movement cannot cross wall");
        yield return Settle();
        var ceiling=Box("CharacterTestCeiling",home+Vector3.up*2.05f,new Vector3(4,.2f,4));Physics.SyncTransforms();
        Keys(Key.Space);yield return Frames(2);Keys();yield return Frames(35);
        Check(motor.transform.position.y<home.y+.1f&&motor.IsGrounded,"Ceiling hit cancels upward velocity and lands promptly");
        Destroy(ceiling);
        motor.Teleport(home+new Vector3(0,.8f,1.6f),Quaternion.identity);yield return Frames(3);
        Check(!motor.IsGrounded,"A vertical wall does not count as ground");
        Destroy(wall);yield return Frames(2);
        yield return Settle();
        // A press just before touchdown must survive until the grounded frame.
        motor.Teleport(home+Vector3.up*.4f,Quaternion.identity);yield return Frames(6);
        Keys(Key.Space);yield return Frames(2);Keys();yield return Frames(17);
        Check(motor.transform.position.y>home.y+.3f,"Jump input buffered before landing");
        yield return Settle();
        Check(!dodge.TryStartRoll(),"C1 cannot dodge before movement_kit reward");
        var dummy=new GameObject("CharacterTestDamageTarget");dummy.layer=7;dummy.transform.position=home+Vector3.forward*1.2f;
        var damageTarget=dummy.AddComponent<CharacterPolishDamageTarget>();
        var collider1=dummy.AddComponent<CapsuleCollider>();collider1.center=Vector3.up;collider1.height=1.8f;collider1.radius=.3f;
        var child=new GameObject("SecondHurtbox");child.layer=7;child.transform.SetParent(dummy.transform,false);
        child.AddComponent<SphereCollider>().center=Vector3.up;
        Physics.SyncTransforms();yield return Swing();
        Check(damageTarget.Hits==1,"One attack damages a target once with multiple colliders");
        wall=Box("CharacterTestCombatWall",home+new Vector3(0,1,.65f),new Vector3(3,2,.18f));Physics.SyncTransforms();
        yield return Swing();Check(damageTarget.Hits==1,"Knife cannot damage through environment wall");
        Destroy(wall);yield return Frames(2);
        int attackCount=0;System.Action<bool> counter=_=>attackCount++;combat.AttackStarted+=counter;
        MouseButton(true);yield return Frames(2);MouseButton(false);yield return Frames(29);
        MouseButton(true);yield return Frames(2);MouseButton(false);yield return Frames(55);
        Check(attackCount==2,"Late attack input produces exactly one buffered follow-up");
        combat.AttackStarted-=counter;Destroy(dummy);
        yield return Settle();MouseButton(true);yield return Frames(2);MouseButton(false);
        Check(combat.IsAttacking&&!actions.CanInteract&&!actions.CanMove,"Attack owns locomotion and interaction");
        vitals.TryTakeDamage(new DamageInfo(null,1,DamageTeam.Neutral));
        Check(!combat.IsAttacking&&actions.IsHurt,"Receiving damage cancels attack with short hit reaction");
        yield return Frames(20);
        var world=Find<WorldStateStore>();world.TryAdd(StateSet.Abilities,PlayerAbilities.MovementKit);
        yield return Settle();Vector3 rollStart=motor.transform.position;
        Keys(Key.D,Key.LeftCtrl);yield return Frames(2);Keys();yield return Frames(34);
        Vector3 rolled=motor.transform.position-rollStart;
        Check(rolled.x>3.1f&&Mathf.Abs(rolled.z)<.3f,"Dodge uses direction pressed in same frame, distance="+rolled.magnitude);
        Check(!dodge.IsDodging&&!vitals.IsInvulnerable&&!motor.IsLow,"Dodge clears invulnerability and restores standing capsule");
        yield return Settle();Keys(Key.LeftCtrl);yield return Frames(2);Keys();
        motor.Teleport(home,Quaternion.identity);
        Check(!dodge.IsDodging&&!motor.IsLow&&!motor.IsDashing&&float.IsNegativeInfinity(dodge.LastRollEndTime),"Teleport aborts roll without stale lunge window or low capsule");
        yield return Frames(45);
        yield return Settle();
        Keys(Key.W,Key.LeftShift,Key.LeftCtrl);Vector3 sprintRollStart=motor.transform.position;
        yield return Frames(2);Keys();yield return Frames(34);
        Check(Vector3.Distance(sprintRollStart,motor.transform.position)>5.05f,"Sprint dodge preserves distance multiplier");
        yield return Frames(12);yield return Settle();
        var lantern=motor.GetComponent<LanternController>();lantern.TryReceiveLantern();
        float healthBefore=vitals.CurrentHealth;int perfectBefore=dodge.PerfectDodgeCount;int lungesBefore=combat.LungeCount;
        Check(dodge.TryStartRoll(),"Unlocked roll starts through public action API");
        while(dodge.RollProgress<.3f)yield return null;
        Check(vitals.IsInvulnerable,"Roll enters configured invulnerability window");
        vitals.TryTakeDamage(new DamageInfo(null,10,DamageTeam.Enemy));
        Check(vitals.CurrentHealth==healthBefore&&dodge.PerfectDodgeCount==perfectBefore+1,"Perfect dodge prevents damage and awards once");
        while(dodge.IsDodging&&dodge.RollProgress<.75f)yield return null;
        MouseButton(true);yield return Frames(2);MouseButton(false);
        while(dodge.IsDodging)yield return null;
        yield return Frames(10);yield return new WaitForEndOfFrame();
        Check(motor.GetComponent<Animator>().GetCurrentAnimatorStateInfo(meleeLayer).shortNameHash==Animator.StringToHash("Lunge"),"Post-roll attack uses distinct thrust clip");
        MeleeSnapshot("Lunge");yield return Frames(45);yield return new WaitForSecondsRealtime(.4f);
        Check(combat.LungeCount==lungesBefore+1,"Attack buffered near roll end produces exactly one lunge");
        yield return Settle();stamina.TrySpend(98f);Keys(Key.W,Key.LeftShift);yield return Frames(150);
        Check(!motor.IsSprinting&&stamina.CurrentStamina>1f,"Holding sprint after exhaustion does not pulse sprint and drain recovery");
        Keys();yield return Frames(2);
        // Buttons held when the map resumes must not fire actions.
        AppRoot.Instance.Session.Pause();MouseButton(true);Keys(Key.Space);yield return Frames(3);
        AppRoot.Instance.Session.Resume();yield return Frames(8);
        Check(!combat.IsAttacking&&motor.transform.position.y<home.y+.1f,"Held jump/attack do not fire on resume");
        Keys();MouseButton(false);yield return Frames(3);MouseButton(true);yield return Frames(2);MouseButton(false);
        Check(combat.IsAttacking,"Fresh click works after resume and release");
        yield return Frames(45);
        yield return Settle();
        for(int frame=0;frame<75;frame++){motor.RequestLift(6f,home.y+4f);yield return null;}
        Check(motor.IsHovering&&motor.transform.position.y>home.y+3.5f,"Wind lift reaches hover height");
        Keys(Key.Space);
        for(int frame=0;frame<12;frame++){motor.RequestLift(6f,home.y+4f);yield return null;if(frame==1)Keys();}
        Check(motor.transform.position.y>home.y+4.2f,"Jump escapes hover without wind swallowing the command");
        yield return Settle();
        var lockOn=motor.GetComponent<LockOnController>();
        var sourceEnemy=Find<EnemyHealth>();
        var lockDummy=new GameObject("CharacterTestLockTarget");lockDummy.SetActive(false);lockDummy.layer=7;
        lockDummy.transform.position=home+Vector3.forward*8;
        var enemy=lockDummy.AddComponent<EnemyHealth>();
        var enemyConfig=new SerializedObject(enemy);
        enemyConfig.FindProperty("definition").objectReferenceValue=new SerializedObject(sourceEnemy).FindProperty("definition").objectReferenceValue;
        enemyConfig.ApplyModifiedPropertiesWithoutUndo();lockDummy.SetActive(true);
        motor.GetComponent<PlayerCameraController>().SetAngles(0,0);yield return Frames(45);
        Check(lockOn.TryLock()&&lockOn.Target==enemy,"Lock-on selects visible target (camera="+Camera.main.transform.position+", enemyHP="+enemy.Current+")");
        lockDummy.transform.position+=Vector3.right*3;yield return Frames(35);
        float expectedYaw=Mathf.Atan2(3f,8f)*Mathf.Rad2Deg;
        Check(Mathf.Abs(Mathf.DeltaAngle(motor.GetComponent<PlayerCameraController>().Yaw,expectedYaw))<1.5f,"Camera smoothly follows lock target through a single pivot writer");
        wall=Box("CharacterTestLockWall",home+new Vector3(1.5f,1,4f),new Vector3(5,4,.3f));Physics.SyncTransforms();
        yield return Frames(70);Check(!lockOn.HasTarget&&motor.FacingTarget==null,"Lock-on releases after prolonged occlusion");
        Destroy(wall);Destroy(lockDummy);yield return Frames(2);
        yield return Settle();
        lantern.RefillEnergy();
        var focusDummy=new GameObject("CharacterTestFocusTarget");focusDummy.layer=10;focusDummy.transform.position=home+new Vector3(0,1.3f,3f);
        var focusTarget=focusDummy.AddComponent<CharacterPolishFocusTarget>();
        for(int c=0;c<70;c++)focusDummy.AddComponent<SphereCollider>().radius=.2f;
        Physics.SyncTransforms();
        InputSystem.QueueStateEvent(testMouse,new MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Right,true));
        yield return Frames(30);
        Check(lantern.IsFocusing&&lantern.Energy>93f&&lantern.Energy<95f,"Focus consumes energy at configured rate");
        Check(focusTarget.Seconds>.42f&&focusTarget.Seconds<.6f,"Focus scanner handles over 64 colliders and deduplicates target");
        Vector3 focusStart=motor.transform.position;Keys(Key.W,Key.LeftShift);yield return Frames(30);Keys();
        Check(!motor.IsSprinting&&Vector3.Distance(focusStart,motor.transform.position)<1.2f,"Focus limits movement speed and suppresses sprint");
        InputSystem.QueueStateEvent(testMouse,new MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Right,true).WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left,true));
        yield return Frames(2);Check(combat.IsAttacking&&!lantern.IsFocusing,"Attack takes priority over held Focus");
        MouseButton(false);Destroy(focusDummy);yield return Frames(45);yield return Settle();
        var focusLight=(Light)new SerializedObject(lantern).FindProperty("focusLight").objectReferenceValue;
        var definition=(EnemyDefinition)new SerializedObject(Find<EnemyHealth>()).FindProperty("definition").objectReferenceValue;
        var farEnemy=FocusEnemyAt(9.3f,definition);lantern.RefillEnergy();
        var puzzleProbe=new GameObject("FocusPuzzleRangeSentinel");puzzleProbe.layer=10;puzzleProbe.transform.position=home+new Vector3(.4f,1.3f,9.3f);
        var farPuzzle=puzzleProbe.AddComponent<CharacterPolishFocusTarget>();puzzleProbe.AddComponent<SphereCollider>().radius=.2f;Physics.SyncTransforms();
        InputSystem.QueueStateEvent(testMouse,new MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Right,true));yield return Frames(75);
        Check(farEnemy.IsWeakened,"RMB weakens real enemy receiver at 9.3m from actor (beyond previous 8m reach)");
        Check(farPuzzle.Seconds<.001f,"Puzzle Focus remains limited to 8m while enemy reach increases");
        Check(focusLight.enabled&&focusLight.range>=12f&&Mathf.Abs(focusLight.spotAngle-90f)<.1f,"Focus light supports extended reach and matches the 90-degree aim cone");
        InputSystem.QueueStateEvent(testMouse,new MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Right,true).WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left,true));yield return Frames(2);
        Check(combat.IsAttacking&&!lantern.IsFocusing&&!focusLight.enabled&&!weapon.IsTrailEmitting,"LMB interrupts RMB and emits neither Focus beam nor knife ribbon");
        InputSystem.QueueStateEvent(testMouse,new MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Right,true));yield return Frames(50);
        Check(!lantern.IsFocusing&&!focusLight.enabled,"Holding RMB after attack does not restart Focus until release");
        Check(farEnemy.GetComponent<EnemyHealth>().Current==farEnemy.GetComponent<EnemyHealth>().Max,"Melee click does not damage a distant enemy");
        MouseButton(false);yield return Frames(3);InputSystem.QueueStateEvent(testMouse,new MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Right,true));yield return Frames(3);
        Check(lantern.IsFocusing&&focusLight.enabled,"Fresh RMB press after release resumes Focus");
        MouseButton(false);Destroy(farEnemy.gameObject);Destroy(puzzleProbe);yield return Frames(3);
        var tooFar=FocusEnemyAt(10.8f,definition);InputSystem.QueueStateEvent(testMouse,new MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Right,true));yield return Frames(75);
        Check(!tooFar.IsWeakened,"Enemy beyond 10m Focus-source distance does not receive weakening");
        Destroy(tooFar.gameObject);var blockedEnemy=FocusEnemyAt(9.3f,definition);
        var focusWall=Box("FocusRangeWall",home+new Vector3(0,1.5f,5),new Vector3(4,4,.3f));Physics.SyncTransforms();yield return Frames(75);
        Check(!blockedEnemy.IsWeakened,"Environment wall blocks longer-range enemy Focus");
        MouseButton(false);Destroy(focusWall);Destroy(blockedEnemy.gameObject);yield return Frames(3);yield return Settle();
        MouseButton(true);yield return Frames(2);MouseButton(false);yield return Frames(10);yield return new WaitForEndOfFrame();
        Check(combat.IsInDamageWindow&&!lantern.IsFocusing&&!focusLight.enabled&&!weapon.IsTrailEmitting,"LMB alone remains a physical knife strike with no light or ribbon effect");
        MeleeSnapshot("NoBeam");yield return Frames(45);yield return Settle();
        var interactDummy=Box("CharacterTestInteract",home+new Vector3(0,1.3f,2),new Vector3(1,2,1));interactDummy.layer=8;
        var interaction=interactDummy.AddComponent<CharacterPolishInteractTarget>();Physics.SyncTransforms();yield return Frames(30);
        Keys(Key.E);yield return Frames(2);Keys();yield return Frames(3);
        Check(interaction.Calls==1,"Camera scanner E interacts with exactly one visible target");
        MouseButton(true);yield return Frames(2);MouseButton(false);Keys(Key.E);yield return Frames(2);Keys();
        Check(interaction.Calls==1,"E does not interact while attack owns actor");
        Destroy(interactDummy);yield return Frames(45);yield return Settle();
        var inventory=Find<InventoryService>();
        inventory.TryAdd(InventoryService.ThuocMau,2);vitals.TryTakeDamage(new DamageInfo(null,40,DamageTeam.Neutral));
        yield return Frames(15);int beforePotions=inventory.Count(InventoryService.ThuocMau);
        Check(motor.GetComponent<PotionUser>().TryUsePotion()&&inventory.Count(InventoryService.ThuocMau)==beforePotions-1,"Potion heals and consumes exactly one item");
        Check(!motor.GetComponent<PotionUser>().TryUsePotion(),"Potion cooldown prevents duplicate consumption");
        motor.Teleport(table.transform.position+Vector3.back*1.5f,Quaternion.identity);yield return Frames(8);
        inventory.TryAdd(InventoryService.LaCam,1);inventory.TryAdd(InventoryService.LinhChi,1);
        beforePotions=inventory.Count(InventoryService.ThuocMau);
        Check(table.CanInteract(motor.gameObject),"Craft table reachable with idle grounded actor");table.Interact(motor.gameObject);
        Check(inventory.Count(InventoryService.ThuocMau)==beforePotions+1,"Craft interaction still creates one potion");
        yield return Frames(25);MouseButton(true);yield return Frames(2);MouseButton(false);
        Check(!table.CanInteract(motor.gameObject),"Craft table rejects interaction during attack");
        yield return Frames(45);
        world.Restore(new WorldSnapshot());lantern.ApplySavedState(false);
        Check(Find<SaveService>().SaveAtCamp(),"Checkpoint save succeeds after character changes");
        AppRoot.Instance.Session.ReturnToMainMenu();yield return WaitScene(GameSession.MainMenuScene);
        AppRoot.Instance.Session.Continue();yield return WaitScene(GameSession.FirstChapterScene);Bind();
        actions=motor.GetComponent<PlayerActionState>();vitals=motor.GetComponent<PlayerVitals>();
        yield return Frames(20);
        Check(Vector3.Distance(motor.transform.position,checkpoint.SpawnPoint.position)<.4f&&motor.IsGrounded&&!motor.IsLow,"Continue restores actor on safe checkpoint floor with standing capsule (position="+motor.transform.position+", spawn="+checkpoint.SpawnPoint.position+", grounded="+motor.IsGrounded+")");
        // Current GameSession keeps time running on death. Also test a frozen visual update explicitly.
        vitals.Kill();yield return new WaitForSecondsRealtime(.85f);
        var animator=motor.GetComponent<Animator>();
        Check(AppRoot.Instance.Session.State==GameSession.SessionState.Dead&&animator.GetBool("Dead"),"Death enters correct session and Animator state");
        Time.timeScale=0f;yield return new WaitForSecondsRealtime(.25f);
        Check(animator.updateMode==AnimatorUpdateMode.UnscaledTime,"Death presentation supports frozen gameplay time");
        Check(!actions.CanInteract&&!actions.CanMove,"Dead actor cannot move or interact");
        Check(report.errors.Count==0,"No runtime errors during character regression");
    }
}

public sealed class CharacterPolishDamageTarget : MonoBehaviour, IDamageable
{
    public int Hits { get; private set; }
    public bool TryTakeDamage(DamageInfo damage) { Hits++; return true; }
}
public sealed class CharacterPolishFocusTarget : MonoBehaviour, ILanternTarget
{
    public Transform AimPoint => transform;
    public bool CanReceiveFocus => true;
    public float Seconds { get; private set; }
    public void ReceiveFocus(float deltaTime) { Seconds+=deltaTime; }
}
public sealed class CharacterPolishInteractTarget : MonoBehaviour, IInteractable
{
    public string Prompt => "Character regression interaction";
    public int Calls { get; private set; }
    public bool CanInteract(GameObject interactor) => true;
    public void Interact(GameObject interactor) { Calls++; }
}
#endif
