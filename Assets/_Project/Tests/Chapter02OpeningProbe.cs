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

// Stage 1 integration: actual C1 altar/exit/end button, scene loads, native saves and Continue.
// Progression is a fixture; no synthetic keyboard or claim of playing the full chapters.
public sealed class Chapter02OpeningProbe : MonoBehaviour
{
    [Serializable] class Report
    {
        public string mode = "Editor Play Mode; actual C1-to-C2 scene loads, native save/menu/Continue, API interactions with physics aim; C1 completion prerequisites are fixtures; no manual E or full traversal";
        public List<string> checks = new List<string>(), failures = new List<string>(), errors = new List<string>(), screenshots = new List<string>();
    }
    [Serializable] class Backup { public string[] paths, hashes; public bool[] existed; }
    readonly Report report = new Report(); Backup backup; string folder; bool finished, completed, oldBackground; float progressAt;
    static T Find<T>() where T : Component => UnityEngine.Object.FindAnyObjectByType<T>();
    static T[] All<T>() where T : Component => UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include,FindObjectsSortMode.None);
    static T Get<T>(object o,string field) => (T)o.GetType().GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);
    static string Hash(string p) { if (!File.Exists(p)) return "absent"; using(var h=SHA256.Create()) return BitConverter.ToString(h.ComputeHash(File.ReadAllBytes(p))); }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Init()
    {
        if (!SessionState.GetBool("SonDoong.C2OpeningTest",false)) return;
        SessionState.SetBool("SonDoong.C2OpeningTest",false);
        var g = new GameObject("C2OpeningProbe"); DontDestroyOnLoad(g); g.AddComponent<Chapter02OpeningProbe>();
    }
    void Awake()
    {
        Application.logMessageReceived += Log; oldBackground=Application.runInBackground; Application.runInBackground=true;
        folder="Temp/C2OpeningBackup_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff"); Directory.CreateDirectory(folder);
        string p=Path.Combine(Application.persistentDataPath,"save_v1.json"); backup=new Backup{paths=new[]{p,p+".bak",p+".tmp"}};
        backup.existed=backup.paths.Select(File.Exists).ToArray(); backup.hashes=backup.paths.Select(Hash).ToArray();
        for(int i=0;i<backup.paths.Length;i++) if(backup.existed[i])File.Copy(backup.paths[i],folder+"/"+i+".original");
        File.WriteAllText(folder+"/manifest.json",JsonUtility.ToJson(backup,true)); progressAt=Time.realtimeSinceStartup;
    }
    void Log(string text,string stack,LogType type) { if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert) report.errors.Add(text); }
    void Check(bool ok,string message)
    {
        if(!ok)throw new InvalidOperationException(message); report.checks.Add(message); progressAt=Time.realtimeSinceStartup;
        Directory.CreateDirectory("Logs");File.WriteAllText("Logs/C2Opening-progress.json",JsonUtility.ToJson(report,true));
    }
    void Update(){if(!finished&&Time.realtimeSinceStartup-progressAt>70){report.failures.Add("Probe stalled; session="+AppRoot.Instance?.Session.State);Finish();}}
    IEnumerator Start()
    {
        var stack=new Stack<IEnumerator>();stack.Push(Run());
        while(stack.Count>0)
        {
            bool more=false;object current=null;Exception failure=null;
            try{more=stack.Peek().MoveNext();if(more)current=stack.Peek().Current;}catch(Exception e){failure=e;}
            if(failure!=null){report.failures.Add(failure.ToString());break;}
            if(!more){stack.Pop();continue;}if(current is IEnumerator nested)stack.Push(nested);else yield return current;
        }
        Finish();
    }
    void Finish()
    {
        if(finished)return;finished=true;if(!completed&&report.failures.Count==0)report.failures.Add("Interrupted before completion");
        Time.timeScale=1;
        try
        {
            for(int i=0;i<backup.paths.Length;i++){if(backup.existed[i])File.Copy(folder+"/"+i+".original",backup.paths[i],true);else if(File.Exists(backup.paths[i]))File.Delete(backup.paths[i]);}
            Check(backup.paths.Select(Hash).SequenceEqual(backup.hashes),"Personal save/backup/temp restored byte-for-byte");
        }catch(Exception e){report.failures.Add("Save restore: "+e);}
        Application.runInBackground=oldBackground;Application.logMessageReceived-=Log;
        Directory.CreateDirectory("docs/reports");File.WriteAllText("docs/reports/C2_Opening_PlayMode_2026-11-05.json",JsonUtility.ToJson(report,true));
        EditorApplication.isPlaying=false;
    }
    void OnDestroy(){if(!finished)Finish();}
    IEnumerator Wait(Func<bool> ready,float seconds,string label)
    {float end=Time.realtimeSinceStartup+seconds;while(!ready()&&Time.realtimeSinceStartup<end)yield return null;Check(ready(),label);}
    IEnumerator Scene(string scene)
    {
        yield return Wait(()=>SceneManager.GetActiveScene().name==scene&&AppRoot.Instance&&(scene==GameSession.MainMenuScene?AppRoot.Instance.Session.State==GameSession.SessionState.MainMenu:AppRoot.Instance.Session.State==GameSession.SessionState.Playing&&Find<SaveService>()?.IsReady==true),60,"Ready scene: "+scene);
        for(int i=0;i<5;i++)yield return null;
    }
    IEnumerator Screenshot(string name)
    {
        string path="docs/images/c2-opening-"+name+"-2026-11-05.png";Directory.CreateDirectory("docs/images");var requestedAt=DateTime.UtcNow;ScreenCapture.CaptureScreenshot(path);
        float end=Time.realtimeSinceStartup+8;while((!File.Exists(path)||File.GetLastWriteTimeUtc(path)<requestedAt)&&Time.realtimeSinceStartup<end)yield return null;
        Check(File.Exists(path)&&File.GetLastWriteTimeUtc(path)>=requestedAt,"Fresh Game View screenshot: "+name);report.screenshots.Add(path);
    }
    IEnumerator MenuContinue(string scene)
    {AppRoot.Instance.Session.ReturnToMainMenu();yield return Scene(GameSession.MainMenuScene);AppRoot.Instance.Session.Continue();yield return Scene(scene);}
    void SafeSpawn(string id)
    {
        var cp=All<MonoBehaviour>().OfType<ICheckpoint>().Single(c=>c.CheckpointId==id);var motor=Find<PlayerMotor>();
        Check(Vector3.Distance(motor.transform.position,cp.SpawnPoint.position)<.45f,"Restored at "+id);
        Check(Physics.Raycast(motor.transform.position+Vector3.up*.2f,Vector3.down,1,1<<3,QueryTriggerInteraction.Ignore),"Spawn has solid floor: "+id);
        Check(Physics.OverlapCapsule(motor.transform.position+Vector3.up*.4f,motor.transform.position+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore).Length==0,"Spawn has clear player capsule: "+id);
    }
    void Carry()
    {
        var w=Find<WorldStateStore>();var inv=Find<InventoryService>();
        Check(w.Has(StateSet.Puzzles,WorldIds.RelicRestoredC1)&&w.Has(StateSet.Bosses,WorldIds.GuardianBoss)&&w.Has(StateSet.Abilities,WorldIds.MovementKit),"C1 restoration/boss/Phuc Kim Quy carried to C2");
        Check(w.Has(StateSet.Lore,"c1_lore_1")&&w.Has(StateSet.Flags,WorldIds.GuideCampIntro),"C1 lore and journal paper carried to C2");
        Check(Find<LanternController>().HasLantern&&w.Has(StateSet.Flags,WorldIds.FlagLantern),"Lantern remains owned across chapter load");
        Check(inv.Count(InventoryService.ThuocMau)==2&&inv.Count(InventoryService.LaCam)==3&&inv.Count(InventoryService.LinhChi)==2,"Exact inventory quantities carried without duplication");
    }
    // Actual ray/clear approach, but interaction invoked through the public API.
    void AimAt(Component component,IInteractable target)
    {
        var collider=component.GetComponentsInChildren<Collider>().First(c=>c.enabled&&!c.isTrigger&&c.gameObject.layer==8);
        var point=collider.bounds.center;var motor=Find<PlayerMotor>();bool placed=false;
        foreach(var dir in new[]{Vector3.back,Vector3.left,Vector3.right,Vector3.forward})
        {
            var p=point+dir*1.8f;
            if(!Physics.Raycast(p+Vector3.up*2,Vector3.down,out var floor,4,1<<3,QueryTriggerInteraction.Ignore)||floor.normal.y<.7f)continue;
            p=floor.point+Vector3.up*.08f;
            if(Physics.CheckCapsule(p+Vector3.up*.4f,p+Vector3.up*1.4f,.28f,1<<3,QueryTriggerInteraction.Ignore))continue;
            motor.Teleport(p,Quaternion.LookRotation(new Vector3(point.x-p.x,0,point.z-p.z)));
            var brain=Camera.main.GetComponent<Unity.Cinemachine.CinemachineBrain>();brain.enabled=false;
            Camera.main.transform.position=p+Vector3.up*1.45f;Camera.main.transform.LookAt(point);Physics.SyncTransforms();
            var ray=Camera.main.ViewportPointToRay(new Vector3(.5f,.5f));
            if(Physics.Raycast(ray,out var hit,4,(1<<3)|(1<<8),QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<IInteractable>()==target){placed=true;break;}
        }
        Check(placed&&Vector3.Distance(motor.transform.position,point)<3&&target.CanInteract(motor.gameObject),"Unobstructed interaction aim within reach: "+component.name);
    }
    IEnumerator Run()
    {
        yield return Scene(GameSession.FirstChapterScene);
        var world=Find<WorldStateStore>();world.Restore(new WorldSnapshot());
        Check(!Find<ChapterExit>().CanInteract(Find<PlayerMotor>().gameObject),"C1 exit stays locked before restoration");
        foreach(string id in new[]{WorldStateStore.DrumA,WorldStateStore.DrumB,WorldStateStore.DrumC})world.TryAdd(StateSet.Fragments,id);
        foreach(string id in new[]{WorldStateStore.DrumPuzzleId,WorldStateStore.MirrorPuzzleId})world.TryAdd(StateSet.Puzzles,id);
        world.TryAdd(StateSet.Bosses,WorldIds.GuardianBoss);world.TryAdd(StateSet.Doors,WorldIds.DoorKimQuy);world.TryAdd(StateSet.Seals,WorldIds.SealGarden);
        world.TryAdd(StateSet.Lore,"c1_lore_1");world.TryAdd(StateSet.Flags,WorldIds.GuideCampIntro);Find<LanternController>().ApplySavedState(true);
        Find<InventoryService>().Restore(new List<InventorySlotDto>{new InventorySlotDto{slot=0,itemId=InventoryService.ThuocMau,quantity=2},new InventorySlotDto{slot=1,itemId=InventoryService.LaCam,quantity=3},new InventorySlotDto{slot=2,itemId=InventoryService.LinhChi,quantity=2}});
        Check(Find<SaveService>().SaveAt(SaveService.KimQuyCheckpointId),"Saved protected pre-restoration C1 fixture using native SaveService");
        File.Copy(backup.paths[0],folder+"/pre-restoration.save");
        Find<RelicAltar>().Interact(Find<PlayerMotor>().gameObject);
        var story=Find<StoryCardPanel>();
        Check(story.IsOpen&&AppRoot.Instance.Session.State==GameSession.SessionState.ChapterComplete,"Actual C1 restoration opens chapter completion");
        Check(Get<string>(story,"nextScene")==GameSession.SecondChapterScene&&Get<string>(story,"nextCheckpoint")=="c2_cp_start","Completion button targets same C2 entry as world portal");
        Check(Get<TMPro.TMP_Text>(story,"bodyText").text.Contains("Giếng Trời Chim Lạc"),"C1 ending introduces next destination without claiming Chim Lac restored");
        yield return MenuContinue(GameSession.FirstChapterScene);SafeSpawn(SaveService.KimQuyCheckpointId);
        var exit=Find<ChapterExit>();AimAt(exit,exit);exit.Interact(Find<PlayerMotor>().gameObject);
        yield return Scene(GameSession.SecondChapterScene);Carry();SafeSpawn("c2_cp_start");
        Check(SaveService.TryReadSaved(out var first)&&first.schemaVersion==4&&first.sceneName==GameSession.SecondChapterScene&&first.checkpointId=="c2_cp_start","Chapter transfer commits native schema4 at C2 start");
        Check(!Find<WorldStateStore>().Has(StateSet.Abilities,WorldIds.AbilityWind)&&Find<ChapterQuestTracker>().CurrentObjective.Contains("Nhận Lông"),"Arrival objective requires finding feather, not restoring relic early");
        var projected=Camera.main.WorldToViewportPoint(Find<AbilityPickup>().transform.position);
        Check(projected.z>0&&projected.x>.57f&&projected.x<.9f&&projected.y>.05f&&projected.y<.95f,"Arrival feather appears beside player silhouette in actual rear camera");
        yield return Screenshot("arrival");
        yield return MenuContinue(GameSession.SecondChapterScene);Carry();SafeSpawn("c2_cp_start");
        Check(!Find<StoryCardPanel>().IsOpen,"Continue at C2 does not replay C1 completion or auto-grant wind");
        var note=All<PuzzleCluePickup>().Single(c=>c.ClueId==WorldIds.GuideWindC2);AimAt(note,note);note.Interact(Find<PlayerMotor>().gameObject);
        Check(note.IsFound&&Find<StoryCardPanel>().IsOpen,"Arrival guide opens and records optional journal flag");
        var body=Get<TMPro.TMP_Text>(Find<StoryCardPanel>(),"bodyText");var scroll=body.GetComponentInParent<ScrollRect>();
        Check(scroll&&scroll.content==body.rectTransform&&scroll.viewport.GetComponent<RectMask2D>()&&scroll.verticalScrollbar,"Story has clipped scrolling content and a visible scrollbar");
        Canvas.ForceUpdateCanvases();var corners=new Vector3[4];scroll.viewport.GetWorldCorners(corners);var button=Get<Button>(Find<StoryCardPanel>(),"continueButton");var buttonCorners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(buttonCorners);
        Check(corners[0].y>buttonCorners[1].y,"Story text viewport stays above Continue button");
        yield return Screenshot("guide");scroll.verticalNormalizedPosition=0;yield return null;yield return Screenshot("guide-bottom");Find<StoryCardPanel>().CloseFromBack();
        Find<JournalPanel>().Open();
        Check(Get<TMPro.TMP_Text>(Find<JournalPanel>(),"knowledgeText").text.Contains("Theo luồng gió")&&Get<TMPro.TMP_Text>(Find<JournalPanel>(),"knowledgeText").text.Contains("Trại Giếng Trời"),"Journal reads both carried C1 and new C2 guides");Find<JournalPanel>().Close();
        Check(Get<TMPro.TMP_Text>(Find<JournalPanel>(),"knowledgeText").GetComponentInParent<ScrollRect>(true),"C2 Journal supports scrolling carried and new notes");
        var beforeWind=Find<WorldStateStore>().Export();var pickup=Find<AbilityPickup>();AimAt(pickup,pickup);pickup.Interact(Find<PlayerMotor>().gameObject);
        Check(Find<WorldStateStore>().Has(StateSet.Abilities,WorldIds.AbilityWind)&&Find<StoryCardPanel>().IsOpen,"Feather grants wind and opens updated tutorial card");
        yield return Screenshot("feather-tutorial");Find<StoryCardPanel>().CloseFromBack();
        Check(pickup.gameObject.activeInHierarchy&&!pickup.CanInteract(Find<PlayerMotor>().gameObject)&&!pickup.GetComponentsInChildren<Collider>().Any(c=>c.enabled),"Collected feather hides colliders but retains restore listener");
        Find<WorldStateStore>().Restore(beforeWind);
        Check(pickup.CanInteract(Find<PlayerMotor>().gameObject)&&pickup.GetComponentsInChildren<Renderer>().Any(r=>r.enabled),"Earlier snapshot restores feather model and interaction");
        pickup.Interact(Find<PlayerMotor>().gameObject);Find<StoryCardPanel>().CloseFromBack();
        Check(Find<ChapterQuestTracker>().CurrentObjective.Contains("cột gió"),"HUD advances to wind tutorial after pickup");
        var cp=All<CheckpointAltar>().Single(c=>c.CheckpointId=="c2_cp_start");cp.Interact(Find<PlayerMotor>().gameObject);if(Find<CraftingPanel>()?.IsOpen==true)Find<CraftingPanel>().Close();
        Check(SaveService.TryReadSaved(out var saved)&&saved.world.sets.Any(s=>s.set==StateSet.Abilities.ToString()&&s.ids.Contains(WorldIds.AbilityWind)),"Start checkpoint saves earned wind ability");
        yield return MenuContinue(GameSession.SecondChapterScene);Carry();SafeSpawn("c2_cp_start");
        Check(Find<WorldStateStore>().Has(StateSet.Flags,WorldIds.GuideWindC2)&&Find<WorldStateStore>().Has(StateSet.Abilities,WorldIds.AbilityWind),"Continue retains new guide and earned ability");
        Check(!Find<AbilityPickup>().CanInteract(Find<PlayerMotor>().gameObject)&&!Find<StoryCardPanel>().IsOpen,"Continue does not duplicate feather or tutorial");
        AppRoot.Instance.Session.Pause();AppRoot.Instance.Session.LoadChapter(GameSession.FirstChapterScene,SaveService.KimQuyCheckpointId);
        Check(SceneManager.GetActiveScene().name==GameSession.SecondChapterScene&&AppRoot.Instance.Session.State==GameSession.SessionState.Paused,"Paused chapter-load request cannot bypass UI input lock");AppRoot.Instance.Session.Resume();
        // Rewind only the protected test slot to exercise the other real entry route.
        AppRoot.Instance.Session.ReturnToMainMenu();yield return Scene(GameSession.MainMenuScene);File.Copy(folder+"/pre-restoration.save",backup.paths[0],true);
        AppRoot.Instance.Session.Continue();yield return Scene(GameSession.FirstChapterScene);
        Find<RelicAltar>().Interact(Find<PlayerMotor>().gameObject);Get<Button>(Find<StoryCardPanel>(),"continueButton").onClick.Invoke();
        yield return Scene(GameSession.SecondChapterScene);Carry();SafeSpawn("c2_cp_start");
        Check(SaveService.TryReadSaved(out var buttonSave)&&buttonSave.checkpointId=="c2_cp_start"&&buttonSave.sceneName==GameSession.SecondChapterScene,"Actual end-card button also commits C2 entry save");
        Check(report.errors.Count==0,"No runtime errors during transition/opening regression");completed=true;
    }
}
#endif
