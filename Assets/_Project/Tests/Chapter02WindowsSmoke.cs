#if DEVELOPMENT_BUILD || UNITY_EDITOR
using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Explicit command-line smoke test for the development player; never runs in normal games.
public sealed class Chapter02WindowsSmoke : MonoBehaviour
{
    [Serializable]class Report
    {
        public string mode="Windows development player, automatic API fixture for C1 completion and C2 boss victory; actual Boot/menu/NewGame/transfer/restoration/quiz/native Continue. No manual input or full combat/performance certification.";
        public string unity,gpu,cpu,quality;public int width,height;public List<string> checks=new List<string>(),failures=new List<string>(),errors=new List<string>();public List<Timing> staticViewTimings=new List<Timing>();
    }
    [Serializable]class Timing{public string area;public int frames=120;public float medianMs,p95Ms,medianFps;}
    Report report=new Report();string output;string[] saves,hashes;byte[][] originals;bool finished;float at;
    static T Find<T>()where T:Component=>UnityEngine.Object.FindAnyObjectByType<T>();
    static T Field<T>(object o,string n)=>(T)o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);
    static string Hash(string p){if(!File.Exists(p))return "absent";using(var s=SHA256.Create())return BitConverter.ToString(s.ComputeHash(File.ReadAllBytes(p)));}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init()
    {
        if(Application.isEditor||!Environment.GetCommandLineArgs().Contains("-c2-delivery-smoke"))return;
        var g=new GameObject("C2WindowsSmoke");DontDestroyOnLoad(g);g.AddComponent<Chapter02WindowsSmoke>();
    }
    void Awake()
    {
        var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-c2-report");output=index>=0&&index+1<args.Length?args[index+1]:Path.Combine(Application.persistentDataPath,"C2WindowsSmoke.json");
        Application.runInBackground=true;Application.logMessageReceived+=Log;at=Time.realtimeSinceStartup;
        report.unity=Application.unityVersion;report.gpu=SystemInfo.graphicsDeviceName;report.cpu=SystemInfo.processorType;report.width=Screen.width;report.height=Screen.height;report.quality=QualitySettings.names[QualitySettings.GetQualityLevel()];
        string p=Path.Combine(Application.persistentDataPath,"save_v1.json");saves=new[]{p,p+".bak",p+".tmp"};originals=saves.Select(x=>File.Exists(x)?File.ReadAllBytes(x):null).ToArray();hashes=saves.Select(Hash).ToArray();
        string dir=Path.Combine(Application.persistentDataPath,"C2SmokeBackup_"+DateTime.UtcNow.ToString("yyyyMMdd_HHmmssfff"));Directory.CreateDirectory(dir);for(int i=0;i<3;i++)if(originals[i]!=null)File.WriteAllBytes(Path.Combine(dir,i+".original"),originals[i]);
    }
    void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(s);}
    void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);report.checks.Add(message);at=Time.realtimeSinceStartup;}
    void Update(){if(!finished&&Time.realtimeSinceStartup-at>90){report.failures.Add("Smoke timeout");Finish();}}
    IEnumerator Wait(Func<bool> ok,string label){float until=Time.realtimeSinceStartup+60;while(!ok()&&Time.realtimeSinceStartup<until)yield return null;Check(ok(),label);}
    IEnumerator Ready(string scene)=>Wait(()=>SceneManager.GetActiveScene().name==scene&&AppRoot.Instance&&(scene==GameSession.MainMenuScene?AppRoot.Instance.Session.State==GameSession.SessionState.MainMenu:AppRoot.Instance.Session.State==GameSession.SessionState.Playing&&Find<SaveService>()&&Find<SaveService>().IsReady),"Ready "+scene);
    IEnumerator Start()
    {
        var stack=new Stack<IEnumerator>();stack.Push(Run());while(stack.Count>0){bool more=false;object current=null;Exception error=null;try{more=stack.Peek().MoveNext();if(more)current=stack.Peek().Current;}catch(Exception e){error=e;}if(error!=null){report.failures.Add(error.ToString());break;}if(!more){stack.Pop();continue;}if(current is IEnumerator nested)stack.Push(nested);else yield return current;}Finish();
    }
    IEnumerator Run()
    {
        yield return Ready(GameSession.MainMenuScene);AppRoot.Instance.Session.NewGame();yield return Ready(GameSession.FirstChapterScene);
        var world=Find<WorldStateStore>();var inventory=Find<InventoryService>();inventory.Restore(new List<InventorySlotDto>());inventory.TryAdd(InventoryService.ThuocMau,2);
        foreach(var id in new[]{WorldStateStore.DrumA,WorldStateStore.DrumB,WorldStateStore.DrumC})world.TryAdd(StateSet.Fragments,id);
        foreach(var id in new[]{WorldStateStore.DrumPuzzleId,WorldStateStore.MirrorPuzzleId})world.TryAdd(StateSet.Puzzles,id);
        world.TryAdd(StateSet.Seals,WorldIds.SealGarden);world.TryAdd(StateSet.Doors,WorldIds.DoorKimQuy);world.TryAdd(StateSet.Bosses,WorldIds.GuardianBoss);world.TryAdd(StateSet.Flags,WorldIds.FlagLantern);Find<LanternController>().ApplySavedState(true);
        Find<RelicAltar>().Interact(Find<PlayerMotor>().gameObject);var story=Find<StoryCardPanel>();Check(story.IsOpen&&world.Has(StateSet.Puzzles,WorldIds.RelicRestoredC1),"C1 altar completes and saves in player");Field<Button>(story,"continueButton").onClick.Invoke();
        yield return Ready(GameSession.SecondChapterScene);yield return new WaitForSecondsRealtime(.8f);world=Find<WorldStateStore>();inventory=Find<InventoryService>();
        Check(inventory.Count(InventoryService.ThuocMau)==2&&world.Has(StateSet.Abilities,WorldIds.MovementKit),"Native C1 to C2 transfer preserves inventory/kit");
        Check(Find<Chapter02Atmosphere>()&&SoundscapeRuntime.Instance.Palette.name=="C2SoundPalette"&&SoundscapeRuntime.Instance.Ambience.isPlaying,"C2 art/audio present in standalone player");
        foreach(var id in new[]{"c2_frag_1","c2_frag_2","c2_frag_3"})world.TryAdd(StateSet.Fragments,id);
        foreach(var id in new[]{"c2_pz_chuong_gio","c2_wind_p3_final"})world.TryAdd(StateSet.Puzzles,id);
        world.TryAdd(StateSet.Abilities,WorldIds.AbilityWind);world.TryAdd(StateSet.BossTools,"c2_tool_long_gio_lon");world.TryAdd(StateSet.Doors,"c2_door_arena");world.TryAdd(StateSet.Bosses,"c2_boss_chim_lac");
        QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;
        yield return Sample(new Vector3(0,.2f,-6),"P1");yield return Sample(new Vector3(6.5f,13.2f,27),"P3");yield return Sample(new Vector3(5,18.2f,76),"P5");
        Find<PlayerMotor>().Teleport(new Vector3(5,18.2f,82),Quaternion.identity);Find<RelicAltar>().Interact(Find<PlayerMotor>().gameObject);story=Find<StoryCardPanel>();Check(story.IsOpen&&world.Has(StateSet.Puzzles,"c2_relic_restored"),"C2 restoration works and saves in player");Field<Button>(story,"continueButton").onClick.Invoke();
        var spirit=Find<QuizSequence>();spirit.Interact(Find<PlayerMotor>().gameObject);var quiz=Find<QuizPanel>();for(int i=0;i<3;i++){Check(quiz.IsOpen,"Spirit question "+i);quiz.Choose(quiz.Current.correctIndex);}Check(world.Has(StateSet.Puzzles,"c2_linhthan_done")&&story.IsOpen,"Player quiz sequence persists completion");Field<Button>(story,"continueButton").onClick.Invoke();
        Find<ChapterExit>().Interact(Find<PlayerMotor>().gameObject);Check(AppRoot.Instance.Session.State==GameSession.SessionState.ChapterComplete,"Unavailable C3 uses chapter-end fallback in player");Field<Button>(story,"continueButton").onClick.Invoke();yield return Ready(GameSession.MainMenuScene);AppRoot.Instance.Session.Continue();yield return Ready(GameSession.SecondChapterScene);
        Check(Find<WorldStateStore>().Has(StateSet.Puzzles,"c2_linhthan_done")&&Find<InventoryService>().Count(InventoryService.ThuocMau)==2,"Standalone Continue preserves ending and inventory");
        Check(SaveService.TryReadSaved(out var saved)&&saved.schemaVersion==4&&saved.checkpointId=="c2_cp_arena","Native player save at safe final C2 checkpoint");
    }
    IEnumerator Sample(Vector3 p,string area)
    {
        report.quality=QualitySettings.names[QualitySettings.GetQualityLevel()];report.width=Screen.width;report.height=Screen.height;
        var motor=Find<PlayerMotor>();motor.Teleport(p,Quaternion.identity);motor.GetComponent<PlayerCameraController>().SetAngles(0,8);
        for(int i=0;i<30;i++)yield return null;
        var times=new List<float>();for(int i=0;i<120;i++){yield return null;times.Add(Time.unscaledDeltaTime*1000);}
        times.Sort();report.staticViewTimings.Add(new Timing{area=area,medianMs=times[60],p95Ms=times[114],medianFps=1000/times[60]});Check(true,"Captured 120 unthrottled static-view frames: "+area);
    }
    void Finish()
    {
        if(finished)return;finished=true;
        try{for(int i=0;i<3;i++){if(originals[i]!=null)File.WriteAllBytes(saves[i],originals[i]);else if(File.Exists(saves[i]))File.Delete(saves[i]);}Check(saves.Select(Hash).SequenceEqual(hashes),"Original personal saves restored byte-for-byte");}catch(Exception e){report.failures.Add("Save restoration: "+e);}
        Application.logMessageReceived-=Log;Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonUtility.ToJson(report,true));Application.Quit(report.failures.Count+report.errors.Count==0?0:1);
    }
    void OnDestroy(){if(!finished)Finish();}
}
#endif
