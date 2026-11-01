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
public class CampCraftingSmokeProbe : MonoBehaviour
{
    const BindingFlags Fields=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    [Serializable] public class Report {public string mode;public List<string> checks=new List<string>();public List<string> failures=new List<string>();public List<string> runtimeErrors=new List<string>();public List<string> notes=new List<string>();}
    [Serializable] public class Backup {public string[] paths;public bool[] existed;public string[] hashes;}
    readonly Report report=new Report();
    string output,backupFolder;Backup original;bool finished;
    CampCraftingStation station;PlayerInput input;PlayerInventory inventory;PlayerVitals vitals;PotionUseController potion;WorldStateStore world;SaveService save;
    static T Find<T>() where T:Component=>Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(c=>c.gameObject.scene.IsValid()&&c.gameObject.scene.isLoaded);
    static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,Fields).GetValue(obj);
    static string Arg(string[] args,string key,string fallback){int i=Array.IndexOf(args,key);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        var args=Environment.GetCommandLineArgs();bool editor=false;
#if UNITY_EDITOR
        editor=UnityEditor.SessionState.GetBool("SonDoong.CraftingTest",false);
        if(editor)UnityEditor.SessionState.SetBool("SonDoong.CraftingTest",false);
#endif
        if(!editor&&!args.Contains("-crafting-self-test"))return;
        var obj=new GameObject("CampCraftingSmokeProbe");DontDestroyOnLoad(obj);var probe=obj.AddComponent<CampCraftingSmokeProbe>();
        probe.output=Arg(args,"-crafting-report",Path.GetFullPath("Logs/CraftingPlayMode.json"));
        probe.backupFolder=Arg(args,"-crafting-backup",Path.GetFullPath("Temp/CampCraftingBackup_20261102/PlayMode_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")));
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
    void Bind(){station=Find<CampCraftingStation>();input=Find<PlayerInput>();inventory=Find<PlayerInventory>();vitals=Find<PlayerVitals>();potion=Find<PotionUseController>();world=Find<WorldStateStore>();save=Find<SaveService>();}
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
    void NearTable()=>MoveAn(station.transform.position+new Vector3(0,.2f,-1.6f));
    string Bag()=>JsonUtility.ToJson(new SaveData{inventory=inventory.ExportSlots()});
    void CraftCounts(int herbs,int mushrooms,int potions,string message)
    {
        station.Interact(input.gameObject);
        Check(inventory.Count(InventoryRules.Herb)==herbs&&inventory.Count(InventoryRules.Mushroom)==mushrooms&&inventory.Count(InventoryRules.HealthPotion)==potions,message);
    }
    void Blocked(string message)
    {
        string before=Bag();Check(!station.CanInteract(input.gameObject),message+" (CanInteract)");station.Interact(input.gameObject);Check(before==Bag(),message+" (inventory unchanged)");
    }
    static List<InventorySlotData> Full(int herbs,int mushrooms,int potions)
    {
        var slots=InventoryRules.EmptySlots();slots[0]=new InventorySlotData(InventoryRules.Herb,herbs);slots[1]=new InventorySlotData(InventoryRules.Mushroom,mushrooms);
        for(int i=2;i<slots.Count;i++)slots[i]=new InventorySlotData(InventoryRules.HealthPotion,potions);return slots;
    }
    IEnumerator Run()
    {
        yield return WaitScene("01_MainMenu");Check(SceneManager.GetActiveScene().name=="01_MainMenu","Boot reaches main menu");BackupSaves();
        var menu=Find<MainMenuController>();menu.NewGame();if(Field<GameObject>(menu,"newGameConfirmPanel").activeSelf)menu.ConfirmNewGame();yield return WaitScene("02_HangLinhMach");Bind();
        Check(station!=null&&station.enabled&&input.inputIsActive&&Time.timeScale==1,"New Game initializes crafting station and player");
        Collect("pickup_la_cam_001");Collect("pickup_linh_chi_001");Check(inventory.Count(InventoryRules.HealthPotion)==0,"Recipe starts with ingredients and no potions");
        Check(save.SaveAtCamp(),"Save ingredient snapshot before crafting");var ingredients=Read();string savedHash=Hash(GameSaveStorage.SavePath);string backupHash=Hash(GameSaveStorage.BackupPath);NearTable();yield return new WaitForSeconds(.5f);
        var box=station.transform.Find("TableVisual").GetComponent<BoxCollider>();Vector3 origin=input.transform.position+Vector3.up;Vector3 offset=box.bounds.center-origin;
        Check(Physics.Raycast(origin,offset.normalized,out RaycastHit hit,3,LayerMask.GetMask("Environment","Interactable"),QueryTriggerInteraction.Ignore)&&ReferenceEquals(hit.collider.GetComponentInParent<IInteractable>(),station),"Table collider is reachable through the scanner's parent-interaction path");
        int changes=0;Action observe=()=>changes++;inventory.Changed+=observe;float health=vitals.CurrentHealth;
        CraftCounts(0,0,1,"E interaction converts one herb and one mushroom into one potion");inventory.Changed-=observe;
        Check(changes==1&&vitals.CurrentHealth==health,"Successful crafting notifies inventory once and does not heal");
        var feedback=Field<TMP_Text>(station,"feedbackText");Check(feedback.text.Contains("Đã chế")&&feedback.text.Contains("checkpoint"),"Vietnamese feedback explains successful craft still needs checkpoint save");
        Check(Hash(GameSaveStorage.SavePath)==savedHash&&Hash(GameSaveStorage.BackupPath)==backupHash,"Crafting does not write or overwrite a checkpoint");
        Check(world.HasCollectedPickup("pickup_la_cam_001")&&world.HasCollectedPickup("pickup_linh_chi_001"),"Crafting preserves ingredient pickup IDs");
        Check(Field<TMP_Text>(Find<InventoryHUD>(),"potionText").text.Contains("1"),"Inventory HUD updates after crafting");
#if UNITY_EDITOR
        ScreenCapture.CaptureScreenshot(Path.GetFullPath("Logs/CraftingHUD_PlayMode.png"));yield return null;
#endif
        ulong old=SceneManager.GetActiveScene().handle.GetRawData();Reload();yield return WaitScene("02_HangLinhMach",old);Bind();
        Check(inventory.Count(InventoryRules.Herb)==1&&inventory.Count(InventoryRules.Mushroom)==1&&inventory.Count(InventoryRules.HealthPotion)==0&&!Pickup("pickup_la_cam_001").gameObject.activeSelf&&!Pickup("pickup_linh_chi_001").gameObject.activeSelf,"Unsaved craft rolls back all quantities while saved ingredient pickups stay hidden");
        NearTable();yield return new WaitForSeconds(.4f);CraftCounts(0,0,1,"Craft again after rollback");Check(save.SaveAtCamp(),"Manual checkpoint commits crafted potion");
        Check(CheckpointRespawnFlow.OpenMainMenu(),"Return to menu after saving craft");yield return WaitScene("01_MainMenu");Find<MainMenuController>().ContinueGame();yield return WaitScene("02_HangLinhMach");Bind();
        Check(inventory.Count(InventoryRules.Herb)==0&&inventory.Count(InventoryRules.Mushroom)==0&&inventory.Count(InventoryRules.HealthPotion)==1&&!Pickup("pickup_la_cam_001").gameObject.activeSelf&&!Pickup("pickup_linh_chi_001").gameObject.activeSelf,"Continue keeps crafted potion, spent ingredients and hidden pickups");
        Damage(60);Check(potion.TryUsePotion()&&vitals.CurrentHealth==75&&inventory.Count(InventoryRules.HealthPotion)==0,"Q command uses crafted potion and heals 40 to 75");Check(save.SaveAtCamp(),"Save consumed crafted potion at camp");var consumed=Read();
        old=SceneManager.GetActiveScene().handle.GetRawData();Reload();yield return WaitScene("02_HangLinhMach",old);Bind();
        Check(inventory.Count(InventoryRules.HealthPotion)==0&&!Pickup("pickup_la_cam_001").gameObject.activeSelf&&!Pickup("pickup_linh_chi_001").gameObject.activeSelf,"Saved consumption survives reload without respawning ingredients");
        NearTable();yield return new WaitForSeconds(.4f);inventory.TryAdd(InventoryRules.Herb,2);string before=Bag();changes=0;inventory.Changed+=observe;station.Interact(input.gameObject);inventory.Changed-=observe;
        Check(Bag()==before&&changes==0&&Field<TMP_Text>(station,"feedbackText").text.Contains("Cần 1 lá cẩm"),"Missing mushroom preserves full inventory and reports required ingredients");
        yield return new WaitForSeconds(.36f);inventory.RestoreSlots(Full(2,2,5));before=Bag();changes=0;inventory.Changed+=observe;station.Interact(input.gameObject);inventory.Changed-=observe;
        Check(Bag()==before&&changes==0&&Field<TMP_Text>(station,"feedbackText").text.Contains("Không đủ chỗ"),"No output space preserves ingredients and emits no Changed event");
        yield return new WaitForSeconds(.36f);inventory.RestoreSlots(Full(1,2,5));CraftCounts(0,1,51,"Full 12-slot bag can reuse an ingredient slot emptied by the recipe");
        yield return new WaitForSeconds(.36f);inventory.RestoreSlots(Full(2,2,4));CraftCounts(1,1,41,"Full bag can add crafted potion into a partial stack");
        yield return new WaitForSeconds(.36f);var small=InventoryRules.EmptySlots();InventoryRules.AddToDraft(small,InventoryRules.Herb,2);InventoryRules.AddToDraft(small,InventoryRules.Mushroom,2);inventory.RestoreSlots(small);
        CraftCounts(1,1,1,"First craft in interval succeeds");Blocked("Same-frame repeated E command is debounced");yield return new WaitForSeconds(.36f);CraftCounts(0,0,2,"Craft succeeds again after 0.35 simulation seconds");
        yield return new WaitForSeconds(.36f);Check(save.ApplySnapshot(ingredients),"Discard all debug capacity/interval inventories without saving");NearTable();yield return new WaitForSeconds(.4f);
        var pause=Find<PauseMenuController>();pause.PauseGameplay();Blocked("Pause blocks crafting");pause.ResumeGameplay();yield return new WaitForSecondsRealtime(.3f);Check(Time.timeScale==1,"Resume restores gameplay after crafting guard test");
        MoveAn(station.transform.position+new Vector3(0,.2f,-5));Blocked("Distance over three metres blocks crafting");NearTable();yield return new WaitForSeconds(.4f);
        var combat=input.GetComponent<PlayerCombatController>();combat.GetType().GetMethod("TryStartAttack",Fields).Invoke(combat,null);Check(combat.IsAttacking,"Real combat attack state started for station guard test");Blocked("Attacking blocks crafting");combat.GetType().GetMethod("CancelAttack",Fields).Invoke(combat,null);
        var motor=input.GetComponent<StarterAssets.ThirdPersonController>();bool dodged=(bool)motor.GetType().GetMethod("TryStartDodge",Fields).Invoke(motor,null);Check(dodged&&motor.IsDodging,"Real dodge state started for station guard test");Blocked("Dodging blocks crafting");motor.GetType().GetMethod("CancelDodge",Fields).Invoke(motor,null);
        Check(save.ApplySnapshot(consumed),"Restore saved real inventory after guard tests");NearTable();yield return new WaitForSeconds(.4f);Damage(1000);Blocked("Dead player cannot craft");
        old=SceneManager.GetActiveScene().handle.GetRawData();Find<CheckpointRespawnFlow>().ReloadWithSnapshot(consumed);yield return WaitScene("02_HangLinhMach",old);Bind();
        var completed=JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(consumed));completed.checkpointId="cp_kim_quy";completed.hasLantern=true;completed.campSealSolved=true;completed.relicRestored=true;completed.collectedRelics=new List<string>{WorldStateStore.DrumA,WorldStateStore.DrumB,WorldStateStore.DrumC};completed.solvedPuzzles=new List<string>{WorldStateStore.DrumPuzzleId,WorldStateStore.MirrorPuzzleId};completed.defeatedEnemies=new List<string>{WorldStateStore.GuardianId};
        Check(save.ApplySnapshot(completed),"Apply a completed chapter snapshot without modifying disk");NearTable();Blocked("Completed chapter blocks crafting");
        Check(GameSaveStorage.IsValid(Read()),"Last saved inventory remains a valid v5 snapshot");
        Check(report.runtimeErrors.Count==0,"No unexpected runtime errors during crafting integration test");
        report.notes.Add("Invoked station Interact and the real combat/dodge commands through their methods; physical E/Q key presses and aiming feel remain manual checks.");
        report.notes.Add("Debug capacity inventories were discarded before checkpoint writes. Original user saves restored on completion.");
    }
}
#endif
