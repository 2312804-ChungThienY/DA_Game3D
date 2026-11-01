#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Opt-in integration probe. Never runs during normal play or in release builds.
public class QuestHUDSmokeProbe : MonoBehaviour
{
    const BindingFlags Fields=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    [Serializable] public class Report {public string mode;public List<string> checks=new List<string>();public List<string> failures=new List<string>();public List<string> runtimeErrors=new List<string>();public List<string> notes=new List<string>();}
    [Serializable] public class Backup {public string[] paths;public bool[] existed;public string[] hashes;}
    readonly Report report=new Report();
    string output,backupFolder;Backup original;bool finished;
    ChapterQuestHUD questHUD;PlayerInput input;PlayerInventory inventory;PlayerVitals vitals;PotionUseController potion;WorldStateStore world;SaveService save;
    static T Find<T>() where T:Component=>Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(c=>c.gameObject.scene.IsValid()&&c.gameObject.scene.isLoaded);
    static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,Fields).GetValue(obj);
    static string Arg(string[] args,string key,string fallback){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        var args=Environment.GetCommandLineArgs();bool editor=false;
#if UNITY_EDITOR
        editor=UnityEditor.SessionState.GetBool("SonDoong.QuestHUDTest",false);
        if(editor)UnityEditor.SessionState.SetBool("SonDoong.QuestHUDTest",false);
#endif
        if(!editor&&!args.Contains("-quest-hud-self-test"))return;
        var obj=new GameObject("QuestHUDSmokeProbe");DontDestroyOnLoad(obj);var probe=obj.AddComponent<QuestHUDSmokeProbe>();
        probe.output=Arg(args,"-quest-hud-report",Path.GetFullPath("Logs/QuestHUDPlayMode.json"));
        probe.backupFolder=Arg(args,"-quest-hud-backup",Path.GetFullPath("Temp/QuestHUDBackup_20261102/PlayMode_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")));
        probe.report.mode=editor?"Editor Play Mode":"Windows Development";Application.runInBackground=true;Application.logMessageReceived+=probe.Capture;
    }
    void Capture(string message,string stack,LogType type){if((type==LogType.Error||type==LogType.Exception||type==LogType.Assert)&&report.runtimeErrors.Count<20)report.runtimeErrors.Add(message);}
    void Check(bool pass,string message){if(!pass)throw new InvalidOperationException(message);report.checks.Add(message);}
    static string Hash(string path){if(!File.Exists(path))return "absent";using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)));}
    void BackupSaves()
    {
        if(Directory.Exists(backupFolder))throw new InvalidOperationException("Use a new backup directory.");Directory.CreateDirectory(backupFolder);
        var paths=new[]{GameSaveStorage.SavePath,GameSaveStorage.BackupPath,GameSaveStorage.SavePath+".tmp"};
        original=new Backup{paths=paths,existed=paths.Select(File.Exists).ToArray(),hashes=paths.Select(Hash).ToArray()};
        for(int i=0;i<paths.Length;i++)if(original.existed[i])File.Copy(paths[i],Path.Combine(backupFolder,i+".original"));
        File.WriteAllText(Path.Combine(backupFolder,"manifest.json"),JsonUtility.ToJson(original,true));
    }
    void RestoreSaves()
    {
        if(original==null)return;
        for(int i=0;i<original.paths.Length;i++)
        {
            if(original.existed[i])File.Copy(Path.Combine(backupFolder,i+".original"),original.paths[i],true);
            else if(File.Exists(original.paths[i]))File.Delete(original.paths[i]);
        }
        Check(original.paths.Select(Hash).SequenceEqual(original.hashes),"Original primary/backup/temp saves restored byte-for-byte");original=null;
    }
    IEnumerator Start()
    {
        var routine=Run();
        while(true)
        {
            bool more;object current=null;
            try{more=routine.MoveNext();if(more)current=routine.Current;}
            catch(Exception e){report.failures.Add(e.ToString());Finish();yield break;}
            if(!more)break;yield return current;
        }
        Finish();
    }
    void Finish()
    {
        if(finished)return;finished=true;
        try{RestoreSaves();}catch(Exception e){report.failures.Add("Save restoration: "+e);}
        Application.logMessageReceived-=Capture;Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonUtility.ToJson(report,true));
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying=false;
#else
        Application.Quit(report.failures.Count+report.runtimeErrors.Count==0?0:1);
#endif
    }
    void Bind(){questHUD=Find<ChapterQuestHUD>();input=Find<PlayerInput>();inventory=Find<PlayerInventory>();vitals=Find<PlayerVitals>();potion=Find<PotionUseController>();world=Find<WorldStateStore>();save=Find<SaveService>();}
    WorldItemPickup Pickup(string id)=>Resources.FindObjectsOfTypeAll<WorldItemPickup>().Single(p=>p.gameObject.scene==SceneManager.GetActiveScene()&&p.PickupId==id);
    void Collect(string id){var pickup=Pickup(id);Check(pickup.CanInteract(input.gameObject),"Pickup is interactable: "+id);pickup.Interact(input.gameObject);Check(!pickup.gameObject.activeSelf&&world.HasCollectedPickup(id),"Pickup collected exactly once: "+id);pickup.Interact(input.gameObject);}
    IEnumerator WaitScene(string name,ulong oldHandle=ulong.MaxValue)
    {
        float until=Time.realtimeSinceStartup+60;
        while(Time.realtimeSinceStartup<until)
        {
            if(SceneManager.GetActiveScene().name==name&&SceneManager.GetActiveScene().handle.GetRawData()!=oldHandle&&!CheckpointRespawnFlow.IsTransitioning)
            {yield return null;yield return null;break;}
            yield return null;
        }
        // Assertions are in the parent iterator so its exception guard owns cleanup.
    }
    void Damage(float amount){Check(vitals.TryTakeDamage(new DamageInfo(gameObject,amount,DamageTeam.Enemy)),"Test damage accepted: "+amount);}
    SaveData Read(){Check(GameSaveStorage.TryReadBest(out SaveData data,out bool backup)&&!backup,"Primary save reads and validates");return data;}
    void Reload(){save.LoadGame();}
    void MoveAn(Vector3 position)
    {
        var controller=input.GetComponent<CharacterController>();bool was=controller.enabled;controller.enabled=false;input.transform.position=position;controller.enabled=was;Physics.SyncTransforms();
    }
    void HUD(ChapterObjective objective,int fragments,int puzzles)
    {
        string expected=(objective==ChapterObjective.Complete?"HOÀN THÀNH\n":"MỤC TIÊU\n")+ChapterObjectiveRules.Description(objective);
        var text=Field<TMP_Text>(questHUD,"objectiveText");var progress=Field<TMP_Text>(questHUD,"progressText");
        Check(text.text==expected&&progress.text==$"Mảnh trống: {fragments}/3  |  Câu đố: {puzzles}/2","HUD objective/progress: "+objective);
        Canvas.ForceUpdateCanvases();text.ForceMeshUpdate(true,true);progress.ForceMeshUpdate(true,true);
        Check(text.preferredHeight<=text.rectTransform.rect.height+1&&progress.preferredHeight<=progress.rectTransform.rect.height+1,"Vietnamese objective text fits fixed rectangles: "+objective);
    }
    void Relic(string id)
    {
        var pickup=Resources.FindObjectsOfTypeAll<RelicPickup>().Single(p=>p.gameObject.scene==SceneManager.GetActiveScene()&&Field<string>(p,"relicId")==id);
        Check(pickup.CanInteract(input.gameObject),"Gameplay relic pickup ready: "+id);pickup.Interact(input.gameObject);
    }
    int Subscriptions()
    {
        var changed=(Delegate)typeof(WorldStateStore).GetField("Changed",Fields).GetValue(world);
        return changed==null?0:changed.GetInvocationList().Count(d=>ReferenceEquals(d.Target,questHUD));
    }
    string State()=>JsonUtility.ToJson(new SaveData{collectedRelics=world.ExportRelics(),solvedPuzzles=world.ExportSolvedPuzzles(),defeatedEnemies=world.ExportDefeatedEnemies(),relicRestored=world.IsRelicRestored,collectedPickups=world.ExportCollectedPickups(),inventory=inventory.ExportSlots()});
    IEnumerator Run()
    {
        yield return WaitScene("01_MainMenu");Check(SceneManager.GetActiveScene().name=="01_MainMenu","Boot reaches menu");BackupSaves();var menu=Find<MainMenuController>();menu.NewGame();if(Field<GameObject>(menu,"newGameConfirmPanel").activeSelf)menu.ConfirmNewGame();yield return WaitScene("02_HangLinhMach");Bind();
        Check(questHUD!=null&&questHUD.isActiveAndEnabled&&input.inputIsActive,"New Game activates the chapter HUD");HUD(ChapterObjective.FindFragmentA,0,0);
        Check(Resources.FindObjectsOfTypeAll<QuestTracker>().Where(q=>q.gameObject.scene==SceneManager.GetActiveScene()).All(q=>!q.enabled&&!Field<TMP_Text>(q,"objectiveText").gameObject.activeSelf),"Old objective component and old text remain disabled");
        string before=State();string saveHash=Hash(GameSaveStorage.SavePath);for(int i=0;i<10;i++)questHUD.Refresh();
        Check(State()==before&&saveHash==Hash(GameSaveStorage.SavePath),"Repeated HUD refresh never grants rewards, changes inventory or writes saves");
        Check(Subscriptions()==1,"HUD has exactly one WorldState subscription");questHUD.gameObject.SetActive(false);Check(Subscriptions()==0,"Disabling HUD unsubscribes");questHUD.gameObject.SetActive(true);Check(Subscriptions()==1,"Reenabling HUD restores exactly one subscription");
        Collect("pickup_la_cam_001");Collect("pickup_linh_chi_001");HUD(ChapterObjective.FindFragmentA,0,0);
        var station=Find<CampCraftingStation>();MoveAn(station.transform.position+new Vector3(0,.2f,-1.6f));yield return new WaitForSeconds(.4f);station.Interact(input.gameObject);Check(inventory.Count(InventoryRules.HealthPotion)==1,"Crafting still works with the new objective HUD");HUD(ChapterObjective.FindFragmentA,0,0);
        Damage(60);Check(potion.TryUsePotion()&&vitals.CurrentHealth==75,"Potion use still works with the new HUD");HUD(ChapterObjective.FindFragmentA,0,0);
#if UNITY_EDITOR
        ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/QuestHUD_PlayMode.png"));yield return null;
#endif
        // Set up received lantern/open seal; exercise existing gameplay reward APIs below.
        Check(save.ApplySnapshot(new SaveData{hasLantern=true,campSealSolved=true}),"Prepare legal chapter progression without modifying HUD or save schema");Relic(WorldStateStore.DrumA);HUD(ChapterObjective.SolveDrums,1,0);
        var drums=Find<DrumPuzzleController>();foreach(var note in new[]{DrumNote.A,DrumNote.C,DrumNote.B}){Check(drums.TryHit(note),"Existing drum gameplay accepts "+note);yield return new WaitForSeconds(.35f);}
        HUD(ChapterObjective.CollectFragmentB,1,1);Check(save.SaveAtCamp(),"Save checkpoint after solving drums but before collecting B");
        Relic(WorldStateStore.DrumB);HUD(ChapterObjective.SolveMirrors,2,1);Find<LightPuzzleReceiver>().TickLight(true,1.1f);HUD(ChapterObjective.CollectFragmentC,2,2);Relic(WorldStateStore.DrumC);HUD(ChapterObjective.DefeatGuardian,3,2);
        ulong old=SceneManager.GetActiveScene().handle.GetRawData();Reload();yield return WaitScene("02_HangLinhMach",old);Bind();HUD(ChapterObjective.CollectFragmentB,1,1);
        Check(!world.HasRelic(WorldStateStore.DrumB)&&!world.HasSolvedPuzzle(WorldStateStore.MirrorPuzzleId),"Loading before-B checkpoint rolls back future relic/puzzle state and objective");
        Relic(WorldStateStore.DrumB);Find<LightPuzzleReceiver>().TickLight(true,1.1f);Relic(WorldStateStore.DrumC);HUD(ChapterObjective.DefeatGuardian,3,2);
        var encounter=Find<GuardianEncounter>();encounter.GetType().GetMethod("OnTriggerEnter",Fields).Invoke(encounter,new object[]{input.GetComponent<CharacterController>()});Check(encounter.IsFightActive,"Existing Guardian encounter starts");
        var guardian=Field<EnemyVitals>(encounter,"guardianVitals");Check(guardian.TryTakeDamage(new DamageInfo(gameObject,1000,DamageTeam.Player)),"Existing damage/death event defeats Guardian");HUD(ChapterObjective.RestoreAltar,3,2);
        var altar=Find<RelicRestorationAltar>();Check(altar.CanInteract(input.gameObject),"Existing restoration altar becomes usable");altar.Interact(input.gameObject);HUD(ChapterObjective.Complete,3,2);Check(Read().relicRestored,"Restoration saves completion through the existing service");
        Check(CheckpointRespawnFlow.OpenMainMenu(),"Return to menu after chapter completion");yield return WaitScene("01_MainMenu");Find<MainMenuController>().ContinueGame();yield return WaitScene("02_HangLinhMach");Bind();HUD(ChapterObjective.Complete,3,2);
        Check(CheckpointRespawnFlow.OpenMainMenu(),"Open menu for New Game reset");yield return WaitScene("01_MainMenu");menu=Find<MainMenuController>();menu.NewGame();if(Field<GameObject>(menu,"newGameConfirmPanel").activeSelf)menu.ConfirmNewGame();yield return WaitScene("02_HangLinhMach");Bind();HUD(ChapterObjective.FindFragmentA,0,0);
        Check(report.runtimeErrors.Count==0,"No unexpected runtime errors during objective HUD integration test");
        report.notes.Add("Progression exercised through existing relic, drum, light-receiver, Guardian damage/death, altar and save APIs; this is not a manual traversal or an optical-mirror/combat balance test.");
        report.notes.Add("Physical aiming/key presses and full-route visual review remain manual; panel rendering/aspect checks are separate Editor previews.");
    }
}
#endif
