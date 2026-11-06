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
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// Explicit opt-in integration test; never runs in a player build or normal Play session.
public sealed class Chapter01RevisionProbe:MonoBehaviour
{
    [Serializable] class Report{public string mode="Editor Play Mode: native UI/services, real enemy FSM/health/animation, synthetic mouse only when focused; positional fixtures are reported explicitly";public List<string> checks=new List<string>(),failures=new List<string>(),errors=new List<string>(),notes=new List<string>();}
    [Serializable] class Backup{public string[] paths;public bool[] existed;public string[] hashes;}
    readonly Report report=new Report();Backup backup;string folder;bool finished,completed,stunOnly;
    readonly string[] floatKeys={"settings.masterVolume","settings.musicVolume","settings.sfxVolume","settings.mouseSensitivity.v2"};
    readonly string[] intKeys={"settings.quality","settings.fullscreen"};
    bool[] prefExists;float[] prefFloats;int[] prefInts;int oldQuality;float oldListener;bool oldPause,oldBackground;
    Mouse testMouse,oldMouse;Keyboard testKeyboard,oldKeyboard;PlayerMotor motor;PlayerVitals vitals;
    static T Find<T>()where T:Component=>UnityEngine.Object.FindAnyObjectByType<T>();
    static T Field<T>(object o,string name)=>(T)o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);
    static void Set(object o,string name,object value)=>o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(o,value);
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){if(!SessionState.GetBool("SonDoong.C1RevisionTest",false))return;SessionState.SetBool("SonDoong.C1RevisionTest",false);var g=new GameObject("C1RevisionProbe");DontDestroyOnLoad(g);var probe=g.AddComponent<Chapter01RevisionProbe>();probe.stunOnly=SessionState.GetBool("SonDoong.C1RevisionStunOnly",false);SessionState.SetBool("SonDoong.C1RevisionStunOnly",false);}
    void Check(bool pass,string message){if(!pass)throw new InvalidOperationException(message);report.checks.Add(message);Progress();}
    float lastProgressAt;
    void Progress(){lastProgressAt=Time.realtimeSinceStartup;Directory.CreateDirectory("Logs");File.WriteAllText("Logs/C1Revision-progress.json",JsonUtility.ToJson(report,true));}
    void Update(){if(!finished&&backup!=null&&Time.realtimeSinceStartup-lastProgressAt>50){report.failures.Add("Probe stalled: session="+AppRoot.Instance?.Session.State+", scale="+Time.timeScale+", playerDead="+vitals?.IsDead+", editorPause="+EditorApplication.isPaused);Finish();}}
    void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert){report.errors.Add(text);Progress();}}
    static string Hash(string path){if(!File.Exists(path))return "absent";using(var s=SHA256.Create())return BitConverter.ToString(s.ComputeHash(File.ReadAllBytes(path)));}
    void BackupState()
    {
        folder="Temp/CampEnvironmentPlayBackup_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff");Directory.CreateDirectory(folder);
        string p=Path.Combine(Application.persistentDataPath,"save_v1.json");var paths=new[]{p,p+".bak",p+".tmp"};backup=new Backup{paths=paths,existed=paths.Select(File.Exists).ToArray(),hashes=paths.Select(Hash).ToArray()};
        for(int i=0;i<paths.Length;i++)if(backup.existed[i])File.Copy(paths[i],folder+"/"+i+".original");File.WriteAllText(folder+"/manifest.json",JsonUtility.ToJson(backup,true));
        prefExists=floatKeys.Concat(intKeys).Select(PlayerPrefs.HasKey).ToArray();prefFloats=floatKeys.Select(k=>PlayerPrefs.GetFloat(k)).ToArray();prefInts=intKeys.Select(k=>PlayerPrefs.GetInt(k)).ToArray();
        oldQuality=QualitySettings.GetQualityLevel();oldListener=AudioListener.volume;oldPause=AudioListener.pause;oldBackground=Application.runInBackground;Application.runInBackground=true;
    }
    void Finish()
    {
        if(finished)return;finished=true;if(!completed&&report.failures.Count==0)report.failures.Add("Integration interrupted before final acceptance.");
        try
        {
            if(backup!=null){for(int i=0;i<backup.paths.Length;i++){if(backup.existed[i])File.Copy(folder+"/"+i+".original",backup.paths[i],true);else if(File.Exists(backup.paths[i]))File.Delete(backup.paths[i]);}Check(backup.paths.Select(Hash).SequenceEqual(backup.hashes),"Personal primary/backup/temp saves restored byte-for-byte");}
            if(prefExists!=null){AppRoot.Instance?.Settings?.Flush();for(int i=0;i<floatKeys.Length;i++){if(prefExists[i])PlayerPrefs.SetFloat(floatKeys[i],prefFloats[i]);else PlayerPrefs.DeleteKey(floatKeys[i]);}for(int i=0;i<intKeys.Length;i++){if(prefExists[floatKeys.Length+i])PlayerPrefs.SetInt(intKeys[i],prefInts[i]);else PlayerPrefs.DeleteKey(intKeys[i]);}PlayerPrefs.Save();QualitySettings.SetQualityLevel(oldQuality,true);AudioListener.volume=oldListener;AudioListener.pause=oldPause;Application.runInBackground=oldBackground;report.checks.Add("Original settings keys/quality restored");}
        }catch(Exception e){report.failures.Add("Restoration: "+e);}
        if(testMouse!=null)InputSystem.RemoveDevice(testMouse);if(oldMouse!=null)oldMouse.MakeCurrent();if(testKeyboard!=null)InputSystem.RemoveDevice(testKeyboard);if(oldKeyboard!=null)oldKeyboard.MakeCurrent();Application.logMessageReceived-=Log;
        Directory.CreateDirectory("docs/reports");File.WriteAllText(stunOnly?"docs/reports/C1Revision_Stun_2026-11-05.json":"docs/reports/C1Revision_PlayMode_2026-11-05.json",JsonUtility.ToJson(report,true));
        EditorApplication.isPlaying=false;EditorApplication.delayCall+=()=>{if(!EditorApplication.isPlayingOrWillChangePlaymode)UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Project/Scenes/02_HangLinhMach.unity");};
    }
    void OnDestroy(){if(!finished)Finish();}
    IEnumerator Start()
    {
        lastProgressAt=Time.realtimeSinceStartup;Application.logMessageReceived+=Log;var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0){object current=null;bool more=false;Exception error=null;try{more=stack.Peek().MoveNext();if(more)current=stack.Peek().Current;}catch(Exception e){error=e;}if(error!=null){report.failures.Add(error.ToString());break;}if(!more){stack.Pop();continue;}if(current is IEnumerator nested)stack.Push(nested);else yield return current;}Finish();
    }
    IEnumerator Frames(int n){for(int i=0;i<n;i++)yield return null;}
    IEnumerator WaitScene(string name)
    {
        float end=Time.realtimeSinceStartup+60;while(Time.realtimeSinceStartup<end){if(SceneManager.GetActiveScene().name==name&&AppRoot.Instance&&(name==GameSession.MainMenuScene||(AppRoot.Instance.Session.State==GameSession.SessionState.Playing&&Find<SaveService>()?.IsReady==true))){yield return Frames(4);yield break;}yield return null;}throw new TimeoutException(name);
    }
    IEnumerator Wait(Func<bool> ready,float seconds,string label){float until=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<until)yield return null;Check(ready(),label);}
    void Position(Vector3 p,float yaw=0,float pitch=8){motor.Teleport(p,Quaternion.Euler(0,yaw,0));motor.GetComponent<PlayerCameraController>().SetAngles(yaw,pitch);Physics.SyncTransforms();}
    void Capture(string label,int width=1600,int height=900)
    {
        var cam=Camera.main;Check(cam!=null,"Camera present for "+label);var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c=>c.isRootCanvas&&c.renderMode==RenderMode.ScreenSpaceOverlay).ToArray();
        var target=new RenderTexture(width,height,24);target.Create();var previous=RenderTexture.active;var tex=new Texture2D(width,height,TextureFormat.RGB24,false);var oldTarget=cam.targetTexture;
        try
        {
            cam.targetTexture=target;
            foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=.5f;}Canvas.ForceUpdateCanvases();
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});RenderTexture.active=target;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();Directory.CreateDirectory("docs/images");File.WriteAllBytes("docs/images/c1-revision-"+label+"-2026-11-05.png",tex.EncodeToPNG());
        }
        finally{foreach(var canvas in canvases){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}cam.targetTexture=oldTarget;Canvas.ForceUpdateCanvases();RenderTexture.active=previous;target.Release();Destroy(target);Destroy(tex);}
    }
    IEnumerator NativeCapture(string label)
    {
        yield return new WaitForEndOfFrame();var texture=ScreenCapture.CaptureScreenshotAsTexture();
        if(texture){File.WriteAllBytes("docs/images/c1-revision-native-"+label+"-2026-11-05.png",texture.EncodeToPNG());report.notes.Add("Native Game View capture "+label+": "+texture.width+"x"+texture.height);Destroy(texture);}
    }
    IEnumerator Run()
    {
        BackupState();yield return WaitScene(GameSession.MainMenuScene);
        if(stunOnly){AppRoot.Instance.Session.NewGame();yield return WaitScene(GameSession.FirstChapterScene);motor=Find<PlayerMotor>();vitals=motor.GetComponent<PlayerVitals>();Find<LanternPickup>().Interact(motor.gameObject);yield return Enemies();Check(report.errors.Count==0,"No runtime errors in isolated stun/combat regression");completed=true;yield break;}
        yield return new WaitForSecondsRealtime(1.2f);Check(Find<UnityEngine.EventSystems.EventSystem>()!=null,"Boot menu has an EventSystem");
        Check(UnityEngine.Object.FindObjectsByType<UnityEngine.EventSystems.EventSystem>(FindObjectsSortMode.None).Length==1,"Exactly one EventSystem");Capture("menu");yield return Frames(2);yield return NativeCapture("menu");
        var root=AppRoot.Instance;var settings=root.Settings;var ui=root.UI;Check(settings.AudioReady&&SoundscapeRuntime.Instance.Palette,"Native mixer and sound palette loaded");
        Check(SoundscapeRuntime.Instance.MusicA.isPlaying||SoundscapeRuntime.Instance.MusicB.isPlaying,"Menu music source playing");
        ui.OpenSettings();yield return Frames(3);Capture("settings",1280,720);yield return Frames(2);yield return NativeCapture("settings");
        var music=Field<Slider>(ui,"musicSlider");var sfx=Field<Slider>(ui,"sfxSlider");var master=Field<Slider>(ui,"volumeSlider");Check(music&&sfx&&master,"Master/Music/SFX sliders wired");
        master.value=.6f;music.value=.3f;sfx.value=0;yield return Frames(2);settings.Mixer.GetFloat("MasterVolume",out float db);Check(Mathf.Abs(db-SettingsService.VolumeToDecibels(.6f))<.01f,"Master gain reaches native mixer");settings.Mixer.GetFloat("MusicVolume",out db);Check(Mathf.Abs(db-SettingsService.VolumeToDecibels(.3f))<.01f,"Music independent gain reaches native mixer");settings.Mixer.GetFloat("SFXVolume",out db);Check(db<=-79.9f,"SFX slider zero mutes its mixer bus");
        sfx.value=.7f;settings.SetQuality(0);Check(QualitySettings.GetQualityLevel()==0,"Low quality selection applies");settings.SetQuality(1);ui.CloseSettings();Check(Mathf.Abs(PlayerPrefs.GetFloat("settings.musicVolume")-.3f)<.001f,"Settings flushed on close");
        root.Session.NewGame();yield return WaitScene(GameSession.FirstChapterScene);motor=Find<PlayerMotor>();vitals=motor.GetComponent<PlayerVitals>();yield return Frames(10);
        Check(motor.transform.position.z<0&&motor.transform.position.y>-.1f,"New Game safe camp spawn");Check(GameObject.Find("C1_HUD_Theme"),"Chapter HUD theme present");
        var quest=Find<QuestTracker>();Check(Field<TMPro.TMP_Text>(quest,"objectiveText").text.Contains("Vườn Rêu"),"Opening objective directs player to Vườn Rêu");Check(Field<TMPro.TMP_Text>(quest,"progressText").text.Contains("0/3")&&Field<TMPro.TMP_Text>(quest,"progressText").text.Contains("0/2"),"HUD starts at 0/3 fragments and 0/2 puzzles");Capture("spawn");
        root.Session.Pause();yield return Frames(2);Check(AudioListener.pause&&Time.timeScale==0,"Pause freezes world and world audio");ui.OpenSettings();yield return Frames(2);Capture("pause-settings",1600,1000);Check(!root.Input.GameplayActive,"Settings from pause keeps gameplay input locked");ui.HandleBack();Check(root.Session.State==GameSession.SessionState.Paused,"Back closes settings into pause");root.Session.Resume();yield return Frames(3);Check(!AudioListener.pause&&Time.timeScale>0,"Resume restores world audio/time");
        Find<InventoryPanel>().Open();yield return Frames(2);Capture("inventory");Find<InventoryPanel>().Close();Find<JournalPanel>().Open();yield return Frames(2);Capture("journal");Find<JournalPanel>().Close();
        yield return CampInput();
        Find<LanternPickup>().Interact(motor.gameObject);Check(motor.GetComponent<LanternController>().HasLantern,"Lantern pickup grants existing lantern ability");Check(motor.GetComponent<LanternController>().GetComponentsInChildren<LotusLanternVisual>(true).Length==1,"Lotus model attached to existing lantern visual");
        yield return RevisionWorld();
        foreach(var p in new[]{new Vector3(25,0,219),new Vector3(26,0,225),new Vector3(0,0,270),new Vector3(8,0,263.5f)})
        {
            Position(p);foreach(float pitch in new[]{-35f,-20f,4f,45f,65f})foreach(float yaw in new[]{0f,90f,180f,270f})
            {motor.GetComponent<PlayerCameraController>().SetAngles(yaw,pitch);yield return new WaitForSecondsRealtime(.2f);var pivot=motor.transform.Find("PlayerCameraRoot").position;var delta=Camera.main.transform.position-pivot;bool blocked=Physics.SphereCast(pivot,.12f,delta.normalized,out var hit,Mathf.Max(0,delta.magnitude-.2f),LayerMask.GetMask("Environment"),QueryTriggerInteraction.Ignore)||Physics.CheckSphere(Camera.main.transform.position,.06f,LayerMask.GetMask("Environment"),QueryTriggerInteraction.Ignore);
                if(blocked){report.notes.Add("Camera="+Camera.main.transform.position+", pivot="+pivot+", motor="+motor.transform.position+", rayHit="+(hit.collider?hit.collider.name:"none")+", overlap="+string.Join(";",Physics.OverlapSphere(Camera.main.transform.position,.06f,1<<3,QueryTriggerInteraction.Ignore).Select(c=>c.name+":"+c.bounds)));var vcam=Find<Unity.Cinemachine.CinemachineCamera>();report.notes.Add("Guard="+vcam.GetComponent<CaveCameraCollisionGuard>()+", Follow="+vcam.Follow+", FollowPosition="+(vcam.Follow?vcam.Follow.position.ToString():"none"));Capture("camera-overlap");}
                Check(!blocked,"Camera clears wall: "+p+", yaw="+yaw+", pitch="+pitch);}
        }
        Position(new Vector3(25,0,219),0,-25);yield return Frames(10);Capture("gate-look-up");yield return Frames(2);yield return NativeCapture("gate-look-up");Position(new Vector3(25,0,219),0,45);yield return Frames(10);Capture("gate-high-angle");Position(new Vector3(8,0,263.5f),-45,8);yield return Frames(10);Capture("pre-arena");
        yield return Enemies();
        Find<SealTarget>().ReceiveFocus(1.1f);var world=Find<WorldStateStore>();world.TryAdd(StateSet.Fragments,WorldStateStore.DrumA);world.TryAdd(StateSet.Fragments,WorldStateStore.DrumB);world.TryAdd(StateSet.Fragments,WorldStateStore.DrumC);world.TryAdd(StateSet.Puzzles,WorldStateStore.DrumPuzzleId);world.TryAdd(StateSet.Puzzles,WorldStateStore.MirrorPuzzleId);world.TryAdd(StateSet.Doors,WorldIds.DoorKimQuy);
        report.notes.Add("Completion setup uses a 3-fragment/2-puzzle fixture; the separate full-route regression verifies actual puzzle solving and traversal.");
        var altar=Find<RelicAltar>();Check(!altar.IsRestored,"Altar not prematurely restored after boss victory");Position(new Vector3(0,0,304.7f));
        var saveService=Find<SaveService>();Set(saveService,"ready",false);altar.Interact(motor.gameObject);Check(!altar.IsRestored&&!world.Has(StateSet.Abilities,WorldIds.MovementKit)&&!Find<StoryCardPanel>().IsOpen,"Save-not-ready failure rolls back restoration/reward and keeps ending closed");Set(saveService,"ready",true);
        altar.Interact(motor.gameObject);yield return Frames(3);Check(altar.IsRestored&&world.Has(StateSet.Abilities,WorldIds.MovementKit),"Altar grants restoration and movement reward after real boss death");Check(SaveService.TryReadSaved(out var save)&&save.checkpointId=="cp_kim_quy","Restoration commits safe final checkpoint");Check(Find<StoryCardPanel>().IsOpen,"Chapter end story opens after save success");Capture("chapter-end");
        root.Session.ReturnToMainMenu();yield return WaitScene(GameSession.MainMenuScene);root.Session.Continue();yield return WaitScene(GameSession.FirstChapterScene);yield return Frames(8);Check(Find<WorldStateStore>().Has(StateSet.Flags,WorldIds.ClueDrum)&&Find<WorldStateStore>().Has(StateSet.Flags,WorldIds.ClueMirror),"Continue retains both clue flags in native schema 4");Check(Find<WorldStateStore>().Has(StateSet.Bosses,WorldIds.GuardianBoss),"Continue retains actual defeated boss");Check(!UnityEngine.Object.FindObjectsByType<StoneGuardian>(FindObjectsSortMode.None).Any(),"Defeated guardian stays inactive after Continue");Check(Find<RelicAltar>().IsRestored,"Continue retains restoration and safe final state");Check(Find<PlayerMotor>().transform.position.y>-.1f,"Continue spawn remains on safe floor");
        Find<PlayerVitals>().Kill();yield return Frames(3);Check(root.Session.State==GameSession.SessionState.Dead,"Death switches to death UI");Capture("death");root.Session.Respawn();yield return WaitScene(GameSession.FirstChapterScene);yield return Frames(5);Check(!Find<PlayerVitals>().IsDead&&root.Session.State==GameSession.SessionState.Playing,"Respawn reloads committed final checkpoint");Check(report.errors.Count==0,"No runtime errors during integration");completed=true;
    }
    IEnumerator CampInput()
    {
        Check(Application.isFocused,"Unity focused for real Input System E routing");oldKeyboard=Keyboard.current;testKeyboard=InputSystem.AddDevice<Keyboard>("C1RevisionKeyboard");testKeyboard.MakeCurrent();
        Check(!Find<CampCraftingTable>(),"Crafting table removed from C1");
        var inventory=Find<InventoryService>();var original=inventory.Export();inventory.TryAdd(InventoryService.LaCam,1);inventory.TryAdd(InventoryService.LinhChi,1);
        var panel=Find<InventoryPanel>();panel.Open();yield return Frames(3);Check(panel.IsOpen&&Time.timeScale==0,"Inventory crafting works in paused UI session");
        var button=Field<Button>(panel,"craftPotionButton");Check(button&&button.interactable,"Craft health potion button wired and eligible");Capture("inventory-ready",1600,1000);yield return NativeCapture("inventory-ready");int before=inventory.Count(InventoryService.ThuocMau);button.onClick.Invoke();
        Check(inventory.Count(InventoryService.ThuocMau)==before+1&&inventory.Count(InventoryService.LaCam)==0&&inventory.Count(InventoryService.LinhChi)==0,"UI button atomically consumes recipe and produces one potion");
        Check(!panel.TryCraftPotion(),"Rapid second craft rejected");yield return new WaitForSecondsRealtime(.3f);Check(!panel.TryCraftPotion(),"Missing ingredients do not craft");Capture("inventory-recipe",1600,1000);yield return NativeCapture("inventory-recipe");
        var full=new List<InventorySlotDto>{new InventorySlotDto{slot=0,itemId=InventoryService.LaCam,quantity=10},new InventorySlotDto{slot=1,itemId=InventoryService.LinhChi,quantity=10}};
        for(int i=2;i<12;i++)full.Add(new InventorySlotDto{slot=i,itemId=InventoryService.ThuocMau,quantity=5});inventory.Restore(full);yield return new WaitForSecondsRealtime(.3f);
        Check(!panel.TryCraftPotion()&&inventory.Count(InventoryService.LaCam)==10&&inventory.Count(InventoryService.LinhChi)==10&&inventory.Count(InventoryService.ThuocMau)==50,"Full bag failure preserves both ingredients and potion count");
        var freed=new List<InventorySlotDto>{new InventorySlotDto{slot=0,itemId=InventoryService.LaCam,quantity=10},new InventorySlotDto{slot=1,itemId=InventoryService.LaCam,quantity=1},new InventorySlotDto{slot=2,itemId=InventoryService.LinhChi,quantity=10},new InventorySlotDto{slot=3,itemId=InventoryService.LinhChi,quantity=1}};
        for(int i=4;i<12;i++)freed.Add(new InventorySlotDto{slot=i,itemId=InventoryService.ThuocMau,quantity=5});inventory.Restore(freed);yield return new WaitForSecondsRealtime(.3f);
        Check(panel.TryCraftPotion()&&inventory.Count(InventoryService.ThuocMau)==41,"Craft succeeds when consuming ingredients frees bag capacity");
        inventory.Restore(original);inventory.TryAdd(InventoryService.ThuocMau,1);panel.Close();yield return Frames(4);Check(!panel.TryCraftPotion(),"Craft command rejected outside inventory UI");
        var scanner=motor.GetComponent<InteractionScanner>();var cam=Camera.main;var brain=cam.GetComponent<Unity.Cinemachine.CinemachineBrain>();bool wasEnabled=brain.enabled;brain.enabled=false;
        try
        {
            var cp=UnityEngine.Object.FindObjectsByType<CheckpointAltar>(FindObjectsSortMode.None).Single(c=>c.CheckpointId=="cp_trai");Position(new Vector3(-2,0,0));cam.transform.position=motor.transform.position+Vector3.up*1.5f+Vector3.back*2;cam.transform.LookAt(cp.GetComponentsInChildren<Collider>().First(c=>c.enabled&&!c.isTrigger).bounds.center);yield return Frames(4);
            Check(ReferenceEquals(Field<IInteractable>(scanner,"currentTarget"),cp),"Scanner chooses camp checkpoint after table removal");InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.E));yield return Frames(2);InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());yield return Frames(3);Check(SaveService.TryReadSaved(out var save)&&save.checkpointId=="cp_trai","Synthetic E saves at camp checkpoint");
        }
        finally{brain.enabled=wasEnabled;InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());}
    }
    IEnumerator RevisionWorld()
    {
        var particles=UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None);var flame=particles.Single(p=>p.name=="Fire_Flames");yield return new WaitForSeconds(.3f);Check(flame.isPlaying&&flame.particleCount>0,"Camp flames emit moving particles");
        var sample=new ParticleSystem.Particle[128];int count=flame.GetParticles(sample);var before=sample[0].position;yield return new WaitForSeconds(.1f);flame.GetParticles(sample);Check(sample[0].position!=before,"Flame particles move over time");
        Check(RenderSettings.fog&&RenderSettings.fogDensity>0&&particles.Count(p=>p.name.StartsWith("Low_Mist_"))==5,"Base cave fog and five sparse banks installed");
        var world=Find<WorldStateStore>();var clues=UnityEngine.Object.FindObjectsByType<PuzzleCluePickup>(FindObjectsSortMode.None);Check(clues.Length==2&&!clues.Any(c=>c.IsFound),"Both optional clue papers start discoverable");Check(Find<DrumPuzzleController>().CanAcceptInput,"Drums can be tried before discovering paper");
        Check(!Find<DrumVisualLabel>()&&!UnityEngine.Object.FindObjectsByType<TMPro.TextMeshPro>(FindObjectsSortMode.None).Any(t=>new[]{"A","B","C"}.Contains(t.text.Trim())),"Legacy runtime A/B/C world labels absent");
        Check(!UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Any(f=>f.sharedMesh&&f.sharedMesh.name.Contains("_Beveled_Beveled")),"No damaged double-beveled path meshes in active chapter");
        foreach(var clue in clues)
        {
            var snapshot=world.Export();var cam=Camera.main;var brain=cam.GetComponent<Unity.Cinemachine.CinemachineBrain>();brain.enabled=false;
            try
            {
                Position(new Vector3(clue.transform.position.x,0,clue.transform.position.z-1.6f));yield return new WaitForSeconds(.25f);cam.transform.position=motor.transform.position+Vector3.up*1.5f+Vector3.back*2;cam.transform.LookAt(clue.GetComponent<Collider>().bounds.center);yield return Frames(4);
                if(!ReferenceEquals(Field<IInteractable>(motor.GetComponent<InteractionScanner>(),"currentTarget"),clue))
                {var scan=motor.GetComponent<InteractionScanner>();var ray=cam.ViewportPointToRay(new Vector3(.5f,.5f));Physics.Raycast(ray,out var hit,25,Field<LayerMask>(scan,"detectionMask"),QueryTriggerInteraction.Ignore);var origin=Field<Transform>(scan,"interactionOrigin");var delta=clue.transform.position-origin.position;Physics.Raycast(origin.position,delta.normalized,out var block,delta.magnitude,Field<LayerMask>(scan,"detectionMask"),QueryTriggerInteraction.Ignore);report.notes.Add("Paper aim focus="+Application.isFocused+", player="+motor.transform.position+", camera="+cam.transform.position+", hit="+(hit.collider?hit.collider.name:"none")+", origin="+origin.position+", block="+(block.collider?block.collider.name:"none")+", can="+clue.CanInteract(motor.gameObject));Capture("paper-aim-failed");}
                Check(ReferenceEquals(Field<IInteractable>(motor.GetComponent<InteractionScanner>(),"currentTarget"),clue),"Scanner selects optional paper: "+clue.ClueId);
                InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.E));yield return Frames(2);InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());yield return Frames(3);Check(clue.IsFound&&Find<StoryCardPanel>().IsOpen,"E collects paper and opens readable story: "+clue.ClueId);Capture(clue.ClueId);Find<StoryCardPanel>().CloseFromBack();yield return Frames(3);
            }
            finally{brain.enabled=true;InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());}
            Check(!Field<GameObject>(clue,"paperVisual").activeSelf&&!Field<Collider>(clue,"interactionCollider").enabled&&clue.gameObject.activeSelf,"Collected paper hides only visual and collider");
            var collected=world.Export();world.Restore(snapshot);Check(!clue.IsFound&&Field<GameObject>(clue,"paperVisual").activeSelf,"World restore can make paper discoverable again");world.Restore(collected);
        }
        Find<JournalPanel>().Open();yield return Frames(3);Check(Field<TMPro.TMP_Text>(Find<JournalPanel>(),"knowledgeText").text.Contains("Mặt Trời")&&Field<TMPro.TMP_Text>(Find<JournalPanel>(),"knowledgeText").text.Contains("hai gương"),"Journal retains both discovered clue texts");Capture("journal-clues",1600,1000);yield return NativeCapture("journal-clues");Find<JournalPanel>().Close();
        foreach(var item in Find<InventoryService>().Catalog.Where(i=>i.UsesSlot))Check(item.icon,"Imported model icon available for "+item.itemId);
        foreach(var relic in UnityEngine.Object.FindObjectsByType<RelicPickup>(FindObjectsSortMode.None)){var visual=relic.transform.Find("VIS_DrumFragment");Check(visual&&Mathf.DeltaAngle(visual.eulerAngles.x,-65)<1,"Relic face set toward approaching route: "+relic.name);}
        Position(new Vector3(0,0,-5));yield return Frames(12);Capture("camp-fire");yield return NativeCapture("camp-fire");
        Position(new Vector3(-14,0,140));yield return Frames(12);Capture("three-drums");yield return NativeCapture("three-drums");
        Position(new Vector3(18,0,65));yield return Frames(12);Capture("relic-pedestal");
        Position(new Vector3(23.4f,0,210));yield return Frames(8);var camera=Camera.main;var cameraBrain=camera.GetComponent<Unity.Cinemachine.CinemachineBrain>();cameraBrain.enabled=false;
        try{camera.transform.position=new Vector3(27.5f,2.2f,210.5f);camera.transform.LookAt(new Vector3(27.5f,1.4f,215));yield return Frames(3);Capture("mirror-frames");yield return NativeCapture("mirror-frames");}finally{cameraBrain.enabled=true;}
    }
    IEnumerator Stun(EnemyHealth health,bool guardian)
    {
        var receiver=health.GetComponent<EnemyLanternReceiver>();float duration=guardian?2:4;var boss=health.GetComponent<StoneGuardian>();var ghost=health.GetComponent<EnemyBrain>();
        Check(Vector3.Dot(health.transform.forward,health.transform.Find("VIS_ImportedEnemy").forward)>.99f,"Enemy model and gameplay forward agree: "+health.name);
        if(boss)boss.Activate();Position(health.transform.position+Vector3.back*1.6f);vitals.RestoreFull();
        yield return Wait(()=>guardian?boss.State==StoneGuardian.GuardianState.Sweep:ghost.State==EnemyState.Attack,8,"Attack windup before stun: "+health.name);
        var head=health.GetComponentsInChildren<Transform>().First(t=>t.name==(guardian?"B-head":"Head"));var oldPose=head.localRotation;
        receiver.ReceiveFocus(1.01f);yield return Frames(4);Check(receiver.IsStunned&&receiver.IsWeakened&&Mathf.Abs(receiver.StunRemaining-duration)<.35f,"Continuous lantern focus weakens and stuns for expected duration: "+health.name);
        Check(guardian?boss.State==StoneGuardian.GuardianState.Stunned:ghost.State==EnemyState.Stunned,"Attack FSM interrupted by stun: "+health.name);
        float hp=vitals.CurrentHealth;var pos=health.transform.position;yield return new WaitForSeconds(.3f);
        Check(Quaternion.Angle(oldPose,head.localRotation)>1,"Enemy rig bows during stun: "+health.name);Check(health.GetComponentInChildren<EnemyStunIndicator>()&&health.GetComponentsInChildren<Transform>().Any(t=>t.name=="VIS_StunStars"&&t.gameObject.activeInHierarchy),"Visible stun indicator above enemy: "+health.name);Capture(guardian?"guardian-stunned":"shadow-stunned");
        var marker=Field<Transform>(health.GetComponent<EnemyStunIndicator>(),"indicator");var toMarker=marker.position-Camera.main.transform.position;bool covered=Physics.Raycast(Camera.main.transform.position,toMarker.normalized,toMarker.magnitude-.05f,LayerMask.GetMask("Enemy","Environment"),QueryTriggerInteraction.Ignore);Check(!covered,"Stun marker is in front of body and clear of environment at close range: "+health.name);
        float remaining=receiver.StunRemaining;AppRoot.Instance.Session.Pause();yield return new WaitForSecondsRealtime(.35f);Check(Mathf.Abs(receiver.StunRemaining-remaining)<.05f,"Pause freezes stun clock: "+health.name);AppRoot.Instance.Session.Resume();
        bool protectedDuringStun=true;float maxDisplacement=0;while(receiver.IsStunned){maxDisplacement=Mathf.Max(maxDisplacement,Vector3.Distance(health.transform.position,pos));protectedDuringStun&=vitals.CurrentHealth==hp;receiver.ReceiveFocus(Time.deltaTime);yield return null;}
        report.notes.Add("Stun isolation "+health.name+": hp="+hp+" to "+vitals.CurrentHealth+", max movement while stunned="+maxDisplacement+", position="+pos+" -> "+health.transform.position);
        Check(protectedDuringStun&&maxDisplacement<.25f,"Stun prevents damage and movement throughout duration: "+health.name);
        Check(receiver.StunImmunityRemaining>2.5f,"Three second recovery immunity follows stun: "+health.name);receiver.ReceiveFocus(2f);Check(!receiver.IsStunned,"Recovery immunity rejects immediate restun: "+health.name);
        vitals.IsInvulnerable=true;report.notes.Add("Player invulnerability enabled only during post-stun immunity/attack recovery fixtures; the protected stun interval above used real damage eligibility.");
        Position(health.transform.position+Vector3.back*5);float end=Time.time+3.1f;while(Time.time<end){receiver.ReceiveFocus(Time.deltaTime);yield return null;}Check(!receiver.IsStunned,"Holding same beam cannot chain stun: "+health.name);
        yield return new WaitForSeconds(.3f);
        if(!guardian)
        {
            oldMouse=Mouse.current;testMouse=InputSystem.AddDevice<Mouse>("C1RevisionMouse");testMouse.MakeCurrent();var cam=Camera.main;var cm=cam.GetComponent<Unity.Cinemachine.CinemachineBrain>();cm.enabled=false;ghost.enabled=false;var agent=ghost.GetComponent<UnityEngine.AI.NavMeshAgent>();agent.isStopped=true;agent.ResetPath();agent.velocity=Vector3.zero;
            try
            {
                Position(health.transform.position+Vector3.back*5);cam.transform.position=motor.transform.position+Vector3.up*1.5f+Vector3.back*2;cam.transform.LookAt(receiver.AimPoint);yield return Frames(5);InputSystem.QueueStateEvent(testMouse,new MouseState().WithButton(MouseButton.Right,true));
                float limit=Time.realtimeSinceStartup+3;while(!receiver.IsStunned&&Time.realtimeSinceStartup<limit){cam.transform.LookAt(receiver.AimPoint);yield return null;}
                var lamp=motor.GetComponent<LanternController>();var scanner=motor.GetComponent<LanternFocusScanner>();var source=Field<Transform>(scanner,"focusSource");var offset=receiver.AimPoint.position-source.position;Physics.Raycast(source.position,offset.normalized,out var hit,offset.magnitude,Field<LayerMask>(scanner,"visibilityMask"),QueryTriggerInteraction.Ignore);
                report.notes.Add("RMB focus="+Application.isFocused+", held="+AppRoot.Instance.Input.IsHeld(AppRoot.Instance.Input.Focus)+", lamp="+lamp.IsFocusing+", energy="+lamp.Energy+", range="+offset.magnitude+", hit="+(hit.collider?hit.collider.name:"none")+", charge="+Field<float>(receiver,"stunCharge")+", canHold="+Field<bool>(receiver,"canStunOnThisHold"));
                if(!receiver.IsStunned)Capture("rmb-failed");Check(receiver.IsStunned&&offset.magnitude>4,"Real RMB InputReader and LanternFocusScanner stun stationary Hắc Ảnh from five metres");
            }
            finally{InputSystem.QueueStateEvent(testMouse,new MouseState());cm.enabled=true;ghost.enabled=true;}
        }
        else receiver.ReceiveFocus(1.01f);
        Check(receiver.IsStunned,"Release and fresh charge can stun again after immunity: "+health.name);
        yield return new WaitForSeconds(duration+.1f);vitals.IsInvulnerable=false;vitals.RestoreFull();
    }
    IEnumerator Enemies()
    {
        var ghosts=UnityEngine.Object.FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None).OrderBy(g=>g.name).ToArray();Check(ghosts.Length==2,"Both Hắc Ảnh actors preserved");
        foreach(var ghost in ghosts){var animator=ghost.GetComponentInChildren<Animator>();Check(animator&&animator.runtimeAnimatorController&&ghost.GetComponentInChildren<EnemyPresentation>(),"Ghost FBX/controller/presentation: "+ghost.name);Check(ghost.GetComponent<UnityEngine.AI.NavMeshAgent>().isOnNavMesh,"Ghost on native baked NavMesh: "+ghost.name);}
        ghosts[1].enabled=false;ghosts[1].GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped=true;report.notes.Add("Second ghost AI temporarily stopped to isolate damage cancellation fixture.");
        yield return Stun(ghosts[0].GetComponent<EnemyHealth>(),false);
        var g=ghosts[0];Position(g.transform.position+Vector3.back*1.5f,0,8);float hp=vitals.CurrentHealth;yield return Wait(()=>g.State==EnemyState.Attack,8,"Hắc Ảnh enters real attack FSM");yield return Frames(4);Capture("shadow-attack");yield return new WaitForSeconds(.9f);Check(vitals.CurrentHealth<hp,"Hắc Ảnh attack damages player through existing combat");vitals.RestoreFull();
        foreach(var ghost in ghosts){ghost.enabled=false;var agent=ghost.GetComponent<UnityEngine.AI.NavMeshAgent>();agent.isStopped=true;agent.ResetPath();}
        var boss=Find<StoneGuardian>();var health=boss.GetComponent<EnemyHealth>();var animatorBoss=boss.GetComponentInChildren<Animator>();Check(animatorBoss&&animatorBoss.runtimeAnimatorController&&animatorBoss.avatar==null,"Guardian uses authored generic rig clips without root motion");
        yield return Stun(health,true);
        Position(boss.transform.position+Vector3.back*2,0,8);boss.Activate();yield return Wait(()=>boss.State==StoneGuardian.GuardianState.Sweep,5,"Guardian enters sweep with real windup");yield return new WaitForSeconds(.35f);var bone=boss.GetComponentsInChildren<Transform>().First(t=>t.name=="B-upperArm.R");var before=bone.localRotation;yield return new WaitForSeconds(.3f);Check(Quaternion.Angle(before,bone.localRotation)>1,"Guardian rig bones animate during sweep");Capture("guardian-sweep");hp=vitals.CurrentHealth;yield return new WaitForSeconds(.6f);Check(vitals.CurrentHealth<hp,"Guardian sweep applies damage after windup");vitals.RestoreFull();
        Position(boss.transform.position+Vector3.back*5,0,8);yield return Wait(()=>boss.State==StoneGuardian.GuardianState.Slam,6,"Guardian enters ranged slam");yield return new WaitForSeconds(.25f);Capture("guardian-slam");Position(boss.transform.position+Vector3.right*7,0,8);hp=vitals.CurrentHealth;yield return new WaitForSeconds(1.3f);Check(vitals.CurrentHealth==hp,"Moving out of marked slam area avoids damage");
        if(Application.isFocused)
        {
            if(testMouse==null){oldMouse=Mouse.current;testMouse=InputSystem.AddDevice<Mouse>("C1IntegrationMouse");}testMouse.MakeCurrent();vitals.IsInvulnerable=true;Set(boss,"moveSpeed",0f);Position(boss.transform.position+Vector3.back*1.5f,0,8);yield return Frames(5);float initial=health.Current;int attacks=0;
            while(!health.IsDead&&attacks++<30){motor.GetComponent<PlayerStamina>().RestoreFull();InputSystem.QueueStateEvent(testMouse,new MouseState().WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left,true));yield return Frames(2);InputSystem.QueueStateEvent(testMouse,new MouseState());yield return new WaitForSeconds(.75f);}
            Check(health.Current<initial&&health.IsDead,"Synthetic left mouse defeats guardian through CombatController (stationary combat fixture)");vitals.IsInvulnerable=false;
        }
        else
        {
            report.notes.Add("OS focus unavailable: repeated mouse attacks deferred. Boss death exercised via real EnemyHealth damage API, not a world boss flag fixture.");health.TryTakeDamage(new DamageInfo(motor.gameObject,health.Max+1,DamageTeam.Player));
        }
        Check(Find<WorldStateStore>().Has(StateSet.Bosses,WorldIds.GuardianBoss),"Real guardian death writes original boss world ID");yield return new WaitForSeconds(1.1f);Check(!boss.gameObject.activeSelf,"Guardian death finishes and hides actor");
    }
}
#endif
