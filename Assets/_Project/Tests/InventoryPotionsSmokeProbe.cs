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
public class InventoryPotionsSmokeProbe : MonoBehaviour
{
    const BindingFlags Fields=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    [Serializable] public class Report {public string mode;public List<string> checks=new List<string>();public List<string> failures=new List<string>();public List<string> runtimeErrors=new List<string>();public List<string> notes=new List<string>();}
    [Serializable] public class Backup {public string[] paths;public bool[] existed;public string[] hashes;}
    readonly Report report=new Report();
    string output,backupFolder;Backup original;bool finished;
    PlayerInput input;PlayerInventory inventory;PlayerVitals vitals;PotionUseController potion;WorldStateStore world;SaveService save;
    static T Find<T>() where T:Component=>Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(c=>c.gameObject.scene.IsValid()&&c.gameObject.scene.isLoaded);
    static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,Fields).GetValue(obj);
    static string Arg(string[] args,string key,string fallback){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        var args=Environment.GetCommandLineArgs();bool editor=false;
#if UNITY_EDITOR
        editor=UnityEditor.SessionState.GetBool("SonDoong.InventoryTest",false);
        if(editor)UnityEditor.SessionState.SetBool("SonDoong.InventoryTest",false);
#endif
        if(!editor&&!args.Contains("-inventory-self-test"))return;
        var obj=new GameObject("InventoryPotionsSmokeProbe");DontDestroyOnLoad(obj);var probe=obj.AddComponent<InventoryPotionsSmokeProbe>();
        probe.output=Arg(args,"-inventory-report",Path.GetFullPath("Logs/InventoryPlayMode.json"));
        probe.backupFolder=Arg(args,"-inventory-backup",Path.GetFullPath("Temp/InventoryPotionsBackup_20261102/PlayMode_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")));
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
    void Bind(){input=Find<PlayerInput>();inventory=Find<PlayerInventory>();vitals=Find<PlayerVitals>();potion=Find<PotionUseController>();world=Find<WorldStateStore>();save=Find<SaveService>();}
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
    IEnumerator Run()
    {
        yield return WaitScene("01_MainMenu");Check(SceneManager.GetActiveScene().name=="01_MainMenu","Boot reaches main menu");
        BackupSaves();var menu=Find<MainMenuController>();menu.NewGame();if(Field<GameObject>(menu,"newGameConfirmPanel").activeSelf)menu.ConfirmNewGame();
        yield return WaitScene("02_HangLinhMach");Bind();
        Check(input!=null&&input.inputIsActive&&save!=null&&save.enabled&&Time.timeScale==1,"New Game initializes gameplay and SaveService");
        Check(inventory.UsedSlots==0&&inventory.ExportSlots().Count==12&&world.ExportCollectedPickups().Count==0,"New Game starts with 12 empty slots and uncollected pickups");
        var initial=Read();Check(initial.version==5&&initial.inventory.Count==12,"New Game writes schema v5");
        var q=input.actions.FindAction("UsePotion",true);Check(q.type==InputActionType.Button&&q.bindings.Any(b=>b.path=="<Keyboard>/q"&&string.IsNullOrEmpty(b.interactions)),"Gameplay UsePotion binds Q without repeat interactions");
        Collect("pickup_la_cam_001");Collect("pickup_linh_chi_001");Collect("pickup_thuoc_mau_001");
        Check(inventory.Count(InventoryRules.Herb)==1&&inventory.Count(InventoryRules.Mushroom)==1&&inventory.Count(InventoryRules.HealthPotion)==1,"Three pickups add one item each without duplicates");
        var hud=Find<InventoryHUD>();Check(Field<TMP_Text>(hud,"inventoryText").text.Contains("Lá cẩm")&&Field<TMP_Text>(hud,"potionText").text.Contains("1"),"HUD shows item names and potion count");
#if UNITY_EDITOR
        ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/InventoryHUD_PlayMode.png"));
        yield return null;
#endif
        Check(!potion.TryUsePotion()&&inventory.Count(InventoryRules.HealthPotion)==1,"Full health does not consume a potion");
        Damage(60);Check(potion.TryUsePotion()&&vitals.CurrentHealth==75&&inventory.Count(InventoryRules.HealthPotion)==0,"Potion heals 40 to 75 and removes exactly one");
        inventory.TryAdd(InventoryRules.HealthPotion,5);Check(!potion.TryUsePotion(),"Rapid repeated potion command is blocked by cooldown");
        yield return new WaitForSeconds(2.1f);Check(potion.TryUsePotion()&&vitals.CurrentHealth==100,"Potion healing clamps at maximum health");
        Damage(60);var pause=Find<PauseMenuController>();pause.PauseGameplay();int before=inventory.Count(InventoryRules.HealthPotion);float at=Time.time;
        Check(Time.timeScale==0&&!potion.TryUsePotion()&&inventory.Count(InventoryRules.HealthPotion)==before,"Pause blocks potion use without consuming items");
        yield return new WaitForSecondsRealtime(2.2f);Check(Mathf.Abs(Time.time-at)<.01f,"Cooldown simulation clock does not advance during pause");
        pause.ResumeGameplay();yield return new WaitForSecondsRealtime(.3f);Check(Time.timeScale==1&&!potion.TryUsePotion(),"Unpausing does not skip the remaining potion cooldown");
        yield return new WaitForSeconds(2.1f);Check(potion.TryUsePotion(),"Potion succeeds after two simulation seconds");
        Check(save.ApplySnapshot(initial),"Restore empty snapshot before capacity tests (debug items not saved)");
        inventory.TryAdd(InventoryRules.HealthPotion,6);var slots=inventory.ExportSlots();Check(slots[0].quantity==5&&slots[1].quantity==1,"Six potions split into stacks of five and one");
        var add=inventory.TryAdd(InventoryRules.Herb,120);Check(add.Added==100&&add.Remaining==20&&inventory.UsedSlots==12,"TryAdd reports accepted and remaining quantity at 12-slot capacity");
        Check(save.ApplySnapshot(initial),"Reset capacity test inventory without saving it");inventory.TryAdd(InventoryRules.Herb,120);
        var full=Pickup("pickup_la_cam_001");full.Interact(input.gameObject);Check(full.gameObject.activeSelf&&!world.HasCollectedPickup(full.PickupId)&&inventory.Count(InventoryRules.Herb)==120&&full.Prompt=="Túi đầy","Full bag leaves pickup visible and its ID uncollected");
        Check(save.ApplySnapshot(initial),"Debug full-bag state discarded without a checkpoint write");
        Collect("pickup_thuoc_mau_001");ulong old=SceneManager.GetActiveScene().handle.GetRawData();Reload();yield return WaitScene("02_HangLinhMach",old);Bind();
        Check(inventory.Count(InventoryRules.HealthPotion)==0&&Pickup("pickup_thuoc_mau_001").gameObject.activeSelf,"Save-before-pickup rollback restores empty bag and visible pickup");
        Collect("pickup_la_cam_001");Collect("pickup_linh_chi_001");Collect("pickup_thuoc_mau_001");Check(save.SaveAtCamp(),"Checkpoint saves inventory and pickup IDs together");var collected=Read();
        old=SceneManager.GetActiveScene().handle.GetRawData();Reload();yield return WaitScene("02_HangLinhMach",old);Bind();
        Check(inventory.Count(InventoryRules.HealthPotion)==1&&world.ExportCollectedPickups().Count==3&&!Pickup("pickup_thuoc_mau_001").gameObject.activeSelf,"Save-after-pickup reload preserves items and hides collected pickups");
        Damage(60);Check(potion.TryUsePotion(),"Restored potion can be used");old=SceneManager.GetActiveScene().handle.GetRawData();Reload();yield return WaitScene("02_HangLinhMach",old);Bind();
        Check(inventory.Count(InventoryRules.HealthPotion)==1&&!Pickup("pickup_thuoc_mau_001").gameObject.activeSelf,"Unsaved potion use rolls back to saved quantity without respawning pickup");
        Damage(60);Check(potion.TryUsePotion()&&save.SaveAtCamp(),"Potion use followed by checkpoint save succeeds");var used=Read();
        old=SceneManager.GetActiveScene().handle.GetRawData();Reload();yield return WaitScene("02_HangLinhMach",old);Bind();
        Check(inventory.Count(InventoryRules.HealthPotion)==0&&!Pickup("pickup_thuoc_mau_001").gameObject.activeSelf,"Saved potion consumption survives reload; pickup stays hidden");
        var primaryBytes=File.ReadAllBytes(GameSaveStorage.SavePath);File.WriteAllText(GameSaveStorage.SavePath,"{}");
        Check(GameSaveStorage.TryReadBest(out SaveData fallback,out bool usedBackup)&&usedBackup&&fallback.version==5,"Corrupt primary falls back to a valid v5 backup");File.WriteAllBytes(GameSaveStorage.SavePath,primaryBytes);
        // Legacy files are temporary fixtures only; they never replace the user's originals.
        for(int version=1;version<=4;version++)
        {
            string checkpoint=version<3?"":",\"checkpointId\":\"cp_trai\"";
            string json="{\"version\":"+version+checkpoint+",\"hasLantern\":true,\"campSealSolved\":true,\"collectedRelics\":[\"relic_drum_a\"],\"solvedPuzzles\":[],\"defeatedEnemies\":[]}";
            string path=Path.Combine(backupFolder,"legacy_v"+version+".json");File.WriteAllText(path,json);
            Check(GameSaveStorage.TryReadOne(path,out SaveData migrated)&&migrated.version==5&&migrated.hasLantern&&migrated.collectedRelics.Contains(WorldStateStore.DrumA)&&migrated.inventory.Count==12&&migrated.inventory.All(s=>s.quantity==0)&&migrated.collectedPickups.Count==0,"Legacy v"+version+" migrates preserving story progress with an empty inventory");
        }
        string incomplete=Path.Combine(backupFolder,"incomplete_v5.json");
        File.WriteAllText(incomplete,"{\"version\":5,\"checkpointId\":\"cp_trai\",\"collectedRelics\":[],\"solvedPuzzles\":[],\"defeatedEnemies\":[]}");
        Check(!GameSaveStorage.TryReadOne(incomplete,out _),"Incomplete v5 file cannot silently reset inventory to empty");
        string noPickupIds=JsonUtility.ToJson(new SaveData()).Replace("\"collectedPickups\":[],", "");
        File.WriteAllText(incomplete,noPickupIds);
        Check(!GameSaveStorage.TryReadOne(incomplete,out _),"V5 file missing pickup IDs is rejected instead of respawning collected items");
        var invalid=new SaveData();invalid.inventory[0]=new InventorySlotData(InventoryRules.HealthPotion,6);Check(!GameSaveStorage.IsValid(invalid),"Save validation rejects oversized stacks");
        invalid=new SaveData();invalid.inventory.RemoveAt(0);Check(!GameSaveStorage.IsValid(invalid),"Save validation rejects wrong slot count");
        invalid=new SaveData();invalid.inventory[0]=new InventorySlotData("unknown",1);Check(!GameSaveStorage.IsValid(invalid),"Save validation rejects unknown item IDs");
        invalid=new SaveData();invalid.collectedPickups.AddRange(new[]{"pickup_a","pickup_a"});Check(!GameSaveStorage.IsValid(invalid),"Save validation rejects duplicate pickup IDs");
        invalid=new SaveData();invalid.collectedPickups.Add("bad id");Check(!GameSaveStorage.IsValid(invalid),"Save validation rejects malformed pickup IDs");
        inventory.TryAdd(InventoryRules.HealthPotion,1);Damage(1000);before=inventory.Count(InventoryRules.HealthPotion);
        Check(vitals.IsDead&&!potion.TryUsePotion()&&inventory.Count(InventoryRules.HealthPotion)==before,"Death locks potion use without consuming inventory");
        old=SceneManager.GetActiveScene().handle.GetRawData();Find<CheckpointRespawnFlow>().ReloadWithSnapshot(used);yield return WaitScene("02_HangLinhMach",old);Bind();
        Check(!vitals.IsDead&&vitals.CurrentHealth==100&&inventory.Count(InventoryRules.HealthPotion)==0&&!Pickup("pickup_thuoc_mau_001").gameObject.activeSelf,"Death/respawn flow restores full health and exact saved inventory/world state");
        Check(CheckpointRespawnFlow.OpenMainMenu(),"Gameplay returns to menu");yield return WaitScene("01_MainMenu");Find<MainMenuController>().ContinueGame();yield return WaitScene("02_HangLinhMach");Bind();
        Check(input.inputIsActive&&inventory.Count(InventoryRules.Herb)==1&&inventory.Count(InventoryRules.HealthPotion)==0,"Continue from menu restores the v5 inventory");
        var final=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(collected));final.checkpointId="cp_truoc_dan";final.hasLantern=true;final.campSealSolved=true;final.collectedRelics=new List<string>{WorldStateStore.DrumA,WorldStateStore.DrumB,WorldStateStore.DrumC};final.solvedPuzzles=new List<string>{WorldStateStore.DrumPuzzleId,WorldStateStore.MirrorPuzzleId};final.defeatedEnemies=new List<string>{WorldStateStore.GuardianId};
        Check(save.ApplySnapshot(final)&&save.TryCompleteChapter(),"Chapter completion writes a v5 snapshot with inventory");var completed=Read();Check(completed.relicRestored&&completed.inventory.Any(s=>s.itemId==InventoryRules.HealthPotion&&s.quantity==1),"Completed snapshot preserves remaining potions");
        Check(CheckpointRespawnFlow.OpenMainMenu(),"Completed game returns to menu");yield return WaitScene("01_MainMenu");Find<MainMenuController>().ContinueGame();yield return WaitScene("02_HangLinhMach");Bind();
        Check(world.IsRelicRestored&&!input.inputIsActive&&inventory.Count(InventoryRules.HealthPotion)==1&&!potion.TryUsePotion(),"Continue completed chapter preserves inventory and blocks potion use");
        Check(CheckpointRespawnFlow.OpenMainMenu(),"Return to menu for new-game reset test");yield return WaitScene("01_MainMenu");menu=Find<MainMenuController>();menu.NewGame();Check(Field<GameObject>(menu,"newGameConfirmPanel").activeSelf,"Existing save requires New Game confirmation");menu.ConfirmNewGame();yield return WaitScene("02_HangLinhMach");Bind();
        Check(inventory.UsedSlots==0&&world.ExportCollectedPickups().Count==0&&Pickup("pickup_thuoc_mau_001").gameObject.activeSelf,"Confirmed New Game clears bag and respawns pickups");
        Check(report.runtimeErrors.Count==0,"No unexpected runtime errors during integration test");
        report.notes.Add("Interaction and potion commands use the same public paths as E/Q. Physical keyboard, aim and mouse feel still require a focused manual play-through.");
        report.notes.Add("Temporary full-bag items were discarded via snapshot restore before any checkpoint save.");
    }
}
#endif
