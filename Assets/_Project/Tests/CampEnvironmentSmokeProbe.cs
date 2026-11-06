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

// Opt-in regression probe, created only by the environment Editor tool.
// It uses the existing actor/controller, never changes player or enemy assets.
public sealed class CampEnvironmentSmokeProbe : MonoBehaviour
{
    [Serializable] public class Report
    {
        public string mode="Unity Editor Play Mode (API/controller probes, not manual keyboard play)";
        public List<string> checks=new List<string>();
        public List<string> failures=new List<string>();
        public List<string> errors=new List<string>();
    }
    [Serializable] class Backup { public string[] paths;public bool[] existed;public string[] hashes; }
    readonly Report report=new Report();
    Backup original;string folder;bool finished;bool chapterArt;bool expanded;bool geometryOnly;bool completion;bool revision;bool guidesPedestals;
    PlayerMotor motor;CharacterController controller;CampCraftingTable table;CheckpointAltar checkpoint;
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        bool chapter=SessionState.GetBool("SonDoong.Chapter01ArtTest",false);
        bool expansion=SessionState.GetBool("SonDoong.ExpandedC1Test",false);
        if(!SessionState.GetBool("SonDoong.CampEnvironmentTest",false)&&!chapter&&!expansion)return;
        SessionState.SetBool("SonDoong.CampEnvironmentTest",false);
        SessionState.SetBool("SonDoong.Chapter01ArtTest",false);
        SessionState.SetBool("SonDoong.ExpandedC1Test",false);
        var go=new GameObject("CampEnvironmentSmokeProbe");DontDestroyOnLoad(go);var probe=go.AddComponent<CampEnvironmentSmokeProbe>();probe.chapterArt=chapter;probe.expanded=expansion;
        probe.revision=SessionState.GetBool("SonDoong.C1RevisionTraversal",false);SessionState.SetBool("SonDoong.C1RevisionTraversal",false);
        probe.guidesPedestals=SessionState.GetBool("SonDoong.GuidesPedestalsTraversal",false);SessionState.SetBool("SonDoong.GuidesPedestalsTraversal",false);
        probe.completion=SessionState.GetBool("SonDoong.C1CompletionTraversal",false);SessionState.SetBool("SonDoong.C1CompletionTraversal",false);
        probe.geometryOnly=SessionState.GetBool("SonDoong.ExpandedC1GeometryOnly",false);SessionState.SetBool("SonDoong.ExpandedC1GeometryOnly",false);
        if(probe.geometryOnly)probe.report.mode="Unity Editor geometry/API regression; camp craft transaction uses an inventory fixture. E eligibility requires focused Game View and is not asserted.";
    }
    static T Find<T>() where T:Component=>UnityEngine.Object.FindAnyObjectByType<T>();
    void Check(bool pass,string message){if(!pass)throw new InvalidOperationException(message);report.checks.Add(message);if(expanded)File.WriteAllText("Logs/ExpandedC1-progress.json",JsonUtility.ToJson(report,true));}
    void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(text);}
    static string Hash(string path){if(!File.Exists(path))return "absent";using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)));}
    void BackupFiles()
    {
        folder="Temp/CampEnvironmentPlayBackup_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");Directory.CreateDirectory(folder);
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
        Application.logMessageReceived-=Log;
        Directory.CreateDirectory("docs/reports");File.WriteAllText(guidesPedestals?"docs/reports/C1_GuidesPedestals_Traversal_2026-11-05.json":revision?"docs/reports/C1Revision_Traversal_2026-11-05.json":completion?"docs/reports/C1Completion_Traversal_2026-11-05.json":expanded?"docs/reports/ExpandedC1_"+(geometryOnly?"Geometry":"PlayMode")+"_2026-11-05.json":"docs/reports/"+(chapterArt?"Chapter01Art":"CampEnvironment")+"_PlayMode_2026-11-04.json",JsonUtility.ToJson(report,true));
        EditorApplication.isPlaying=false;
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
        checkpoint=UnityEngine.Object.FindObjectsByType<CheckpointAltar>(FindObjectsSortMode.None).Single(c=>c.CheckpointId=="cp_trai");
    }
    IEnumerator Walk(Vector3 destination)
    {
        motor.enabled=false;var delta=destination-motor.transform.position;delta.y=0;int steps=Mathf.CeilToInt(delta.magnitude/.075f);delta/=Mathf.Max(steps,1);
        for(int i=0;i<steps;i++){controller.Move(delta+Vector3.down*.025f);if(i%8==0)yield return null;}
        var difference=motor.transform.position-destination;difference.y=0;
        Check(difference.magnitude<.18f,"Walked without snagging to "+destination+" (actual "+motor.transform.position+")");
        Check(motor.transform.position.y>-.1f&&motor.transform.position.y<.35f,"Safe floor at "+destination);
    }
    IEnumerator CameraOrbit(Vector3 position)
    {
        motor.Teleport(position,Quaternion.identity);motor.enabled=false;
        var look=motor.GetComponent<PlayerCameraController>();var cam=Camera.main;
        for(int angle=0;angle<360;angle+=45)
        {
            look.SetAngles(angle,4);yield return new WaitForSecondsRealtime(.35f);
            var pivot=motor.transform.Find("PlayerCameraRoot");Vector3 delta=cam.transform.position-pivot.position;
            bool blocked=Physics.SphereCast(pivot.position,.15f,delta.normalized,out RaycastHit hit,Mathf.Max(0,delta.magnitude-.18f),LayerMask.GetMask("Environment"),QueryTriggerInteraction.Ignore);
            Check(!blocked,"Camera clear at "+position+", yaw="+angle+(blocked?" blocked by "+hit.collider.name:""));
        }
    }
    void Snapshot(string label)
    {
        var cam=Camera.main;var target=new RenderTexture(1600,900,24);target.Create();var previous=RenderTexture.active;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);
        try
        {
            UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});
            RenderTexture.active=target;tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();
            File.WriteAllBytes(revision?"docs/images/c1-revision-route-"+label+"-2026-11-05.png":completion?"docs/images/c1-complete-route-"+label+"-2026-11-05.png":expanded?"docs/images/c1-expanded-play-"+label+"-2026-11-05.png":"docs/images/"+(chapterArt?"c1-play-":"camp-play-")+label+"-2026-11-04.png",tex.EncodeToPNG());
        }
        finally{RenderTexture.active=previous;target.Release();Destroy(target);Destroy(tex);}
    }
    IEnumerator Run()
    {
        BackupFiles();yield return WaitScene(GameSession.MainMenuScene);
        Check(SceneManager.GetActiveScene().name==GameSession.MainMenuScene,"Boot reached main menu");
        AppRoot.Instance.Session.NewGame();yield return WaitScene(GameSession.FirstChapterScene);Bind();
        Check(Vector3.Distance(motor.transform.position,checkpoint.SpawnPoint.position)<.35f,"New Game spawns on cp_trai safe floor");
        if(revision)
        {
            Check(table==null,"No crafting table in revised camp");yield return new WaitForSecondsRealtime(.6f);Snapshot("spawn");
            var inv=Find<InventoryService>();inv.TryAdd(InventoryService.LaCam,1);inv.TryAdd(InventoryService.LinhChi,1);var panel=Find<InventoryPanel>();int old=inv.Count(InventoryService.ThuocMau);panel.Open();yield return null;Check(panel.TryCraftPotion()&&inv.Count(InventoryService.ThuocMau)==old+1,"Inventory crafts potion before traversal");panel.Close();yield return null;
            checkpoint.Interact(motor.gameObject);Check(SaveService.TryReadSaved(out var campSave)&&campSave.checkpointId=="cp_trai","Camp checkpoint still saves after table removal");
            yield return Walk(new Vector3(1.1f,0,-3));yield return Walk(new Vector3(1.1f,0,4));yield return Walk(new Vector3(0,0,7));yield return Walk(new Vector3(0,0,16));yield return CameraOrbit(new Vector3(0,0,-5));yield return ExpandedRoute();yield break;
        }
        Check(table!=null,"Separate crafting table present");
        yield return new WaitForSecondsRealtime(.6f);Snapshot("spawn");
        var follow=Find<CinemachineThirdPersonFollow>();
        Check(follow!=null&&follow.AvoidObstacles.Enabled,"Cinemachine 3 obstacle avoidance enabled");
        var inventory=Find<InventoryService>();Check(inventory.TryAdd(InventoryService.LaCam,1)==1&&inventory.TryAdd(InventoryService.LinhChi,1)==1,"Recipe ingredients added for regression test");
        yield return Walk(new Vector3(1.1f,0,-3));yield return Walk(new Vector3(1.1f,0,4));yield return Walk(new Vector3(-3,0,1.6f));
        var hitPoint=table.transform.position+Vector3.up*.7f;
        bool hit=Physics.Raycast(motor.transform.position+Vector3.up*1.3f,(hitPoint-(motor.transform.position+Vector3.up*1.3f)).normalized,out RaycastHit tableHit,3,LayerMask.GetMask("Environment","Interactable"),QueryTriggerInteraction.Ignore);
        Check(hit&&tableHit.collider.GetComponentInParent<IInteractable>()==table,"Aim ray selects table separately from checkpoint");
        int potions=inventory.Count(InventoryService.ThuocMau);
        if(geometryOnly)
        {
            Check(inventory.TryCraft(new List<KeyValuePair<string,int>>{new KeyValuePair<string,int>(InventoryService.LaCam,1),new KeyValuePair<string,int>(InventoryService.LinhChi,1)},InventoryService.ThuocMau,1,out _),"Craft transaction fixture; focused E eligibility deferred");
        }
        else {Check(table.CanInteract(motor.gameObject),"Table is reachable");table.Interact(motor.gameObject);}
        Check(inventory.Count(InventoryService.LaCam)==0&&inventory.Count(InventoryService.LinhChi)==0&&inventory.Count(InventoryService.ThuocMau)==potions+1,geometryOnly?"Inventory fixture crafts exactly one potion":"E command crafts exactly one potion");
        motor.Teleport(new Vector3(-2,0,0),Quaternion.identity);
        hitPoint=checkpoint.transform.position+Vector3.up*.8f;
        hit=Physics.Raycast(motor.transform.position+Vector3.up*1.3f,(hitPoint-(motor.transform.position+Vector3.up*1.3f)).normalized,out RaycastHit cpHit,3,LayerMask.GetMask("Environment","Interactable"),QueryTriggerInteraction.Ignore);
        Check(hit&&cpHit.collider.GetComponentInParent<IInteractable>()==checkpoint,"Aim ray selects checkpoint separately from table");
        checkpoint.Interact(motor.gameObject);Check(SaveService.TryReadSaved(out var save)&&save.checkpointId=="cp_trai","Checkpoint saves cp_trai");
        motor.Teleport(new Vector3(-3,0,1.6f),Quaternion.identity);
        yield return Walk(new Vector3(-1.6f,0,1.6f));yield return Walk(new Vector3(-1.6f,0,5));yield return Walk(new Vector3(0,0,7));yield return Walk(new Vector3(0,0,16));
        yield return CameraOrbit(new Vector3(0,0,-5));yield return CameraOrbit(new Vector3(8.7f,0,-2));yield return CameraOrbit(new Vector3(0,0,10));
        if(expanded){yield return ExpandedRoute();yield break;}
        if(chapterArt)yield return ChapterRoute();
        AppRoot.Instance.Session.ReturnToMainMenu();yield return WaitScene(GameSession.MainMenuScene);
        Check(SceneManager.GetActiveScene().name==GameSession.MainMenuScene,"Returned to menu");AppRoot.Instance.Session.Continue();yield return WaitScene(GameSession.FirstChapterScene);Bind();
        Check(Vector3.Distance(motor.transform.position,checkpoint.SpawnPoint.position)<.35f,"Continue restores safe cp_trai spawn");
        Check(Find<InventoryService>().Count(InventoryService.ThuocMau)==potions+1,"Continue preserves crafted and saved potion");
        Check(report.errors.Count==0,"No runtime errors during environment regression");
    }
    IEnumerator ChapterRoute()
    {
        // Geometry regression isolates combat; it does not claim to complete a boss fight.
        foreach(var brain in UnityEngine.Object.FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None))
        {brain.enabled=false;var agent=brain.GetComponent<UnityEngine.AI.NavMeshAgent>();Check(agent&&agent.isOnNavMesh,"Enemy remains on baked NavMesh: "+brain.name);if(agent&&agent.isOnNavMesh){agent.isStopped=true;agent.ResetPath();}}
        foreach(var boss in UnityEngine.Object.FindObjectsByType<StoneGuardian>(FindObjectsSortMode.None))boss.enabled=false;
        var look=motor.GetComponent<PlayerCameraController>();
        Find<LanternPickup>()?.Interact(motor.gameObject);
        Check(motor.GetComponent<LanternController>().HasLantern,"Lantern collected before unlocking seals and saving chapter progress");
        var world=Find<WorldStateStore>();world.TryAdd(StateSet.Seals,WorldIds.SealGarden);
        foreach(var seal in UnityEngine.Object.FindObjectsByType<SealTarget>(FindObjectsSortMode.None))seal.ReceiveFocus(1.1f);
        yield return Walk(new Vector3(0,0,23));yield return Walk(new Vector3(1.5f,0,24));yield return Walk(new Vector3(1.5f,0,32));yield return Walk(new Vector3(0,0,32));
        yield return CameraOrbit(new Vector3(0,0,29));look.SetAngles(0,4);yield return new WaitForSecondsRealtime(.3f);Snapshot("garden");
        yield return Walk(new Vector3(0,0,39));yield return Walk(new Vector3(0,0,44));yield return Walk(new Vector3(0,0,50));
        var drum=Find<DrumPuzzleController>();
        foreach(var note in new[]{DrumNote.A,DrumNote.C,DrumNote.B})
        {
            Check(drum.TryHit(note),"Drum input accepted after art: "+note);float until=Time.realtimeSinceStartup+5;
            while(!drum.CanAcceptInput&&drum.State!=DrumPuzzleState.Solved&&Time.realtimeSinceStartup<until)yield return null;
        }
        Check(drum.State==DrumPuzzleState.Solved&&world.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId),"Drum A-C-B opens original puzzle door");
        yield return CameraOrbit(new Vector3(0,0,51));look.SetAngles(0,4);yield return new WaitForSecondsRealtime(.3f);Snapshot("drums");
        yield return Walk(new Vector3(1.0f,0,51));yield return Walk(new Vector3(1.0f,0,55));yield return Walk(new Vector3(0,0,59));
        yield return Walk(new Vector3(-2.5f,0,59));yield return Walk(new Vector3(-2.5f,0,61.4f));yield return Walk(new Vector3(-1.2f,0,61.4f));
        yield return Walk(new Vector3(-1.2f,0,65));yield return Walk(new Vector3(-1.5f,0,69));yield return Walk(new Vector3(1.5f,0,70));
        foreach(var mirror in UnityEngine.Object.FindObjectsByType<MirrorRotator>(FindObjectsSortMode.None))
        {
            Vector3 old=mirror.FrontNormal;mirror.Interact(motor.gameObject);
            Check(Vector3.Angle(old,mirror.FrontNormal)>89,"Mirror and attached frame rotate together");
            for(int i=0;i<3;i++)mirror.Interact(motor.gameObject);
        }
        var receiver=Find<LightPuzzleReceiver>();receiver.TickLight(true,1.1f);
        Check(receiver.IsSolved&&world.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId),"Mirror receiver opens original door with art installed (receiver API check)");
        yield return CameraOrbit(new Vector3(0,0,71));look.SetAngles(0,4);yield return new WaitForSecondsRealtime(.3f);Snapshot("light");
        motor.Teleport(new Vector3(1.5f,0,71),Quaternion.identity);
        yield return Walk(new Vector3(1.5f,0,75));yield return Walk(new Vector3(0,0,77));yield return Walk(new Vector3(1.1f,0,78));yield return Walk(new Vector3(1.1f,0,82));yield return Walk(new Vector3(0,0,84));
        foreach(string id in new[]{WorldStateStore.DrumA,WorldStateStore.DrumB,WorldStateStore.DrumC})world.TryCollectRelic(id);
        var gate=Find<RequirementDoor>();gate.Interact(motor.gameObject);Check(gate.IsOpen,"Original three-fragment/two-puzzle gate still opens");
        yield return Walk(new Vector3(0,0,89));yield return Walk(new Vector3(0,0,95));
        yield return CameraOrbit(new Vector3(0,0,97));look.SetAngles(0,4);yield return new WaitForSecondsRealtime(.3f);Snapshot("kimquy");
        var altar=Find<RelicAltar>();
        Check(altar.transform.Find("VIS_C1_KimQuy_Statue")!=null,"Statue model is a child of the existing relic altar gameplay root");
        bool altarHit=Physics.Raycast(new Vector3(0,1.3f,103.2f),Vector3.forward,out RaycastHit artHit,5,LayerMask.GetMask("Environment","Interactable"),QueryTriggerInteraction.Ignore);
        Check(altarHit&&artHit.collider.GetComponentInParent<IInteractable>()==altar,"Front ray through statue selects the original relic altar instead of blocking E");
        Check(GameObject.Find("ART_C1_Environment")!=null,"Five-area decoration root loaded in gameplay");
        // Restore the camp save used by the initial craft/Continue regression.
        checkpoint.Interact(motor.gameObject);
    }
    Transform Guide(int index)=>SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).Single(t=>t.name=="TEST_ROUTE_"+index);
    IEnumerator WalkLink(int index,bool stopBeforeGate=false)
    {
        var nodes=Guide(index).Cast<Transform>().ToArray();
        for(int i=0;i<nodes.Length;i++)
        {
            if(stopBeforeGate&&nodes[i].position.z>267)break;
            yield return Walk(nodes[i].position);
            if(i==nodes.Length/2){var look=motor.GetComponent<PlayerCameraController>();var next=nodes[Mathf.Min(i+3,nodes.Length-1)].position-motor.transform.position;look.SetAngles(Mathf.Atan2(next.x,next.z)*Mathf.Rad2Deg,4);yield return new WaitForSecondsRealtime(.15f);Snapshot("link"+(index+1));}
        }
        Check(true,"Traversed expanded curved link "+(index+1));
    }
    void CollectRelic(string id)
    {
        var item=UnityEngine.Object.FindObjectsByType<RelicPickup>(FindObjectsInactive.Include,FindObjectsSortMode.None)
            .Single(p=>new SerializedObject(p).FindProperty("relicId").stringValue==id);
        Check(item.CanInteract(motor.gameObject),"Existing relic is interactable: "+id);item.Interact(motor.gameObject);
        Check(Find<WorldStateStore>().HasRelic(id),"Existing relic collected once: "+id);
    }
    IEnumerator ExpandedRoute()
    {
        foreach(var brain in UnityEngine.Object.FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None))
        {brain.enabled=false;var agent=brain.GetComponent<UnityEngine.AI.NavMeshAgent>();Check(agent&&agent.isOnNavMesh,"Enemy on expanded NavMesh: "+brain.name);if(agent&&agent.isOnNavMesh){agent.isStopped=true;agent.ResetPath();}}
        foreach(var boss in UnityEngine.Object.FindObjectsByType<StoneGuardian>(FindObjectsSortMode.None))boss.enabled=false;
        Find<LanternPickup>()?.Interact(motor.gameObject);Check(motor.GetComponent<LanternController>().HasLantern,"Lantern acquired before expanded progression");
        var seal=Find<SealTarget>();seal.ReceiveFocus(1.1f);Check(seal.IsSolved,"Original camp seal opens the first expanded path");
        motor.Teleport(new Vector3(0,0,16),Quaternion.identity);motor.enabled=false;yield return WalkLink(0);
        yield return Walk(new Vector3(18,0,55));yield return Walk(new Vector3(21,0,55));yield return Walk(new Vector3(21,0,73));
        CollectRelic(WorldStateStore.DrumA);yield return CameraOrbit(new Vector3(21,0,74));Snapshot("garden");
        yield return Walk(new Vector3(21,0,89));yield return Walk(new Vector3(18,0,92));yield return WalkLink(1);
        yield return Walk(new Vector3(-14,0,138));yield return Walk(new Vector3(-12.5f,0,138));yield return Walk(new Vector3(-12.5f,0,146));
        var drum=Find<DrumPuzzleController>();
        foreach(var note in new[]{DrumNote.A,DrumNote.C,DrumNote.B})
        {Check(drum.TryHit(note),"Expanded drum accepts "+note);float until=Time.realtimeSinceStartup+5;while(!drum.CanAcceptInput&&drum.State!=DrumPuzzleState.Solved&&Time.realtimeSinceStartup<until)yield return null;}
        Check(drum.State==DrumPuzzleState.Solved,"A-C-B still opens the enlarged hall partition");
        yield return Walk(new Vector3(-14,0,146));yield return Walk(new Vector3(-14,0,150));CollectRelic(WorldStateStore.DrumB);
        yield return Walk(new Vector3(-17,0,150));yield return Walk(new Vector3(-17,0,155));yield return Walk(new Vector3(-14,0,155));yield return Walk(new Vector3(-14,0,162));
        yield return CameraOrbit(new Vector3(-17,0,156));Snapshot("drums");yield return Walk(new Vector3(-14,0,156));yield return Walk(new Vector3(-14,0,162));yield return WalkLink(2);
        yield return Walk(new Vector3(23.4f,0,204));yield return Walk(new Vector3(23.4f,0,212));yield return Walk(new Vector3(27,0,212));
        var mirrors=UnityEngine.Object.FindObjectsByType<MirrorRotator>(FindObjectsSortMode.None).OrderBy(x=>x.name).ToArray();
        var emitter=Find<MirrorBeamEmitter>();var receiver=Find<LightPuzzleReceiver>();
        var trace=typeof(MirrorBeamEmitter).GetMethod("TraceBeam",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);bool connected=false;
        for(int a=0;a<4&&!connected;a++)
        {
            for(int b=0;b<4;b++)
            {
                Physics.SyncTransforms();if((bool)trace.Invoke(emitter,null)){connected=true;break;}
                mirrors[1].Interact(motor.gameObject);
            }
            if(!connected)mirrors[0].Interact(motor.gameObject);
        }
        Check(connected,"Actual two-mirror reflected ray reaches receiver in the expanded canyon");receiver.TickLight(true,1.1f);
        Check(receiver.IsSolved,"Mirror puzzle opens its original door");
        yield return CameraOrbit(new Vector3(27,0,212));Snapshot("light");
        yield return Walk(new Vector3(26.05f,0,221));CollectRelic(WorldStateStore.DrumC);yield return Walk(new Vector3(26.05f,0,226));yield return Walk(new Vector3(25,0,234));
        yield return WalkLink(3,true);yield return Walk(new Vector3(8,0,263.5f));
        var pre=UnityEngine.Object.FindObjectsByType<CheckpointAltar>(FindObjectsSortMode.None).Single(c=>c.CheckpointId=="cp_truoc_dan");
        Check(pre.CanInteract(motor.gameObject),"Pre-boss checkpoint accessible in its new safe alcove");pre.Interact(motor.gameObject);
        Check(SaveService.TryReadSaved(out var saved)&&saved.checkpointId=="cp_truoc_dan","Pre-boss checkpoint saves its existing ID");
        yield return Walk(new Vector3(0,0,267));var gate=Find<RequirementDoor>();gate.Interact(motor.gameObject);Check(gate.IsOpen,"Three fragments and two puzzles open the relocated arena entrance");
        yield return Walk(new Vector3(0,0,272));yield return Walk(new Vector3(0,0,282));yield return Walk(new Vector3(7,0,290));
        yield return CameraOrbit(new Vector3(7,0,298));Snapshot("kimquy");
        var altar=Find<RelicAltar>();bool hits=Physics.Raycast(new Vector3(0,1.3f,304.7f),Vector3.forward,out var hit,5,LayerMask.GetMask("Environment","Interactable"),QueryTriggerInteraction.Ignore);
        Check(hits&&hit.collider.GetComponentInParent<IInteractable>()==altar,"Relocated statue still selects the existing relic altar");
        Find<WorldStateStore>().TryAdd(StateSet.Bosses,WorldIds.GuardianBoss); // geometry/save fixture, not a simulated boss victory
        var final=UnityEngine.Object.FindObjectsByType<CheckpointAltar>(FindObjectsSortMode.None).Single(c=>c.CheckpointId=="cp_kim_quy");
        Check(final.CanInteract(motor.gameObject),"Final checkpoint condition still checks original guardian ID");final.Interact(motor.gameObject);
        Check(SaveService.TryReadSaved(out saved)&&saved.checkpointId=="cp_kim_quy","New final spawn is saved using original checkpoint ID");
        Vector3 expected=final.SpawnPoint.position;
        AppRoot.Instance.Session.ReturnToMainMenu();yield return WaitScene(GameSession.MainMenuScene);AppRoot.Instance.Session.Continue();yield return WaitScene(GameSession.FirstChapterScene);Bind();
        Check(Vector3.Distance(motor.transform.position,expected)<.35f,"Continue restores the final checkpoint at its new location");
        float settleUntil=Time.realtimeSinceStartup+2;while(!controller.isGrounded&&Time.realtimeSinceStartup<settleUntil)yield return null;
        Check(motor.transform.position.z>300&&controller.isGrounded,"Continue stands safely in the enlarged sanctum");
        Check(Find<WorldStateStore>().RelicCount==3&&Find<WorldStateStore>().HasSolvedPuzzle(WorldStateStore.DrumPuzzleId)&&Find<WorldStateStore>().HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId),"Continue preserves all three fragments and both puzzle states");
        motor.Teleport(new Vector3(0,-12.5f,290),Quaternion.identity);yield return new WaitForSecondsRealtime(.5f);
        Check(motor.GetComponent<PlayerVitals>().IsDead,"Expanded lower kill volume catches a fall below the sanctum");
        AppRoot.Instance.Session.Respawn();yield return WaitScene(GameSession.FirstChapterScene);Bind();
        settleUntil=Time.realtimeSinceStartup+2;while(!controller.isGrounded&&Time.realtimeSinceStartup<settleUntil)yield return null;
        Check(Vector3.Distance(motor.transform.position,expected)<.4f&&controller.isGrounded&&!motor.GetComponent<PlayerVitals>().IsDead,"Death/Respawn returns safely to the relocated checkpoint");
        Check(report.errors.Count==0,"No runtime errors in the expanded chapter regression");
    }
}
#endif

