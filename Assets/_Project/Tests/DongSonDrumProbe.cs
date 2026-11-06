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

// Opt-in art regression. Native services/API and physics rays, not physical keyboard input.
public sealed class DongSonDrumProbe:MonoBehaviour
{
    [Serializable]class Report
    {
        public string scope="Editor Play Mode: supplied FBX, native drum API/wrong/reset/solved/reward/save/Continue and plausible camera-to-collider rays. Position and prerequisite lantern/seal fixtures; no manual E or full chapter playthrough.";
        public List<string> checks=new List<string>(),failures=new List<string>(),errors=new List<string>();
    }
    readonly Report report=new Report();string[] paths,hashes;byte[][] originals;bool finished,completed,oldBackground;
    double started;
    static T Find<T>()where T:Component=>UnityEngine.Object.FindAnyObjectByType<T>();
    static T Field<T>(object o,string name)=>(T)o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);
    static string Hash(string p){if(!File.Exists(p))return "absent";using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(p)));}
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init()
    {
        if(!SessionState.GetBool("SonDoong.DongSonDrumsTest",false))return;SessionState.SetBool("SonDoong.DongSonDrumsTest",false);
        var go=new GameObject("DongSon_Drum_Regression");DontDestroyOnLoad(go);go.AddComponent<DongSonDrumProbe>();
    }
    void Check(bool pass,string message){if(!pass)throw new InvalidOperationException(message);report.checks.Add(message);}
    void Log(string s,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)report.errors.Add(s);}
    IEnumerator Start()
    {
        started=EditorApplication.timeSinceStartup;oldBackground=Application.runInBackground;Application.runInBackground=true;Application.logMessageReceived+=Log;
        string p=Path.Combine(Application.persistentDataPath,"save_v1.json");paths=new[]{p,p+".bak",p+".tmp"};hashes=paths.Select(Hash).ToArray();originals=paths.Select(x=>File.Exists(x)?File.ReadAllBytes(x):null).ToArray();
        string recovery="Logs/DongSonDrumSaveRecovery";Directory.CreateDirectory(recovery);
        for(int i=0;i<paths.Length;i++)if(originals[i]!=null)File.WriteAllBytes(recovery+"/"+i+".original",originals[i]);
        File.WriteAllText(recovery+"/manifest.json",JsonUtility.ToJson(new SaveManifest{paths=paths,hashes=hashes},true));
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
    [Serializable]class SaveManifest{public string[] paths,hashes;}
    void Update(){if(!finished&&EditorApplication.timeSinceStartup-started>100){report.failures.Add("Watchdog timeout");Finish();}}
    void OnDestroy(){if(!finished&&paths!=null)Finish();}
    void Finish()
    {
        if(finished)return;finished=true;
        if(!completed&&report.failures.Count==0)report.failures.Add("Regression interrupted");
        try
        {
            if(paths!=null)for(int i=0;i<paths.Length;i++){if(originals[i]!=null)File.WriteAllBytes(paths[i],originals[i]);else if(File.Exists(paths[i]))File.Delete(paths[i]);}
            Check(paths!=null&&paths.Select(Hash).SequenceEqual(hashes),"Personal primary/bak/tmp save bytes and absence restored");
        }catch(Exception e){report.failures.Add("Restore: "+e);}
        Application.runInBackground=oldBackground;Application.logMessageReceived-=Log;
        Directory.CreateDirectory("docs/reports");File.WriteAllText("docs/reports/DongSonDrums_PlayMode_2026-11-06.json",JsonUtility.ToJson(report,true));
        EditorApplication.isPlaying=false;
        EditorApplication.delayCall+=()=>{if(!EditorApplication.isPlayingOrWillChangePlaymode)UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/_Project/Scenes/02_HangLinhMach.unity");};
    }
    IEnumerator WaitScene(string scene)
    {
        float end=Time.realtimeSinceStartup+35;
        while(Time.realtimeSinceStartup<end)
        {
            if(SceneManager.GetActiveScene().name==scene&&AppRoot.Instance&&(scene==GameSession.MainMenuScene||Find<SaveService>()?.IsReady==true)){yield return null;yield return null;yield break;}
            yield return null;
        }
        throw new TimeoutException(scene);
    }
    IEnumerator Wait(Func<bool> condition,float seconds,string label)
    {
        float end=Time.realtimeSinceStartup+seconds;while(!condition()&&Time.realtimeSinceStartup<end)yield return null;Check(condition(),label);
    }
    IEnumerator Run()
    {
        yield return WaitScene(GameSession.MainMenuScene);AppRoot.Instance.Session.NewGame();yield return WaitScene(GameSession.FirstChapterScene);
        var motor=Find<PlayerMotor>();var world=Find<WorldStateStore>();var puzzle=Find<DrumPuzzleController>();
        Find<LanternPickup>().Interact(motor.gameObject);Find<SealTarget>().ApplySavedState(true);
        Check(motor.GetComponent<LanternController>().HasLantern&&Find<SealTarget>().IsSolved,"Prerequisite fixture grants real lantern and clears garden seal for a valid native checkpoint snapshot");
        // Isolate art/puzzle checks from unrelated AI, keep their physical capsules in place.
        foreach(var ai in UnityEngine.Object.FindObjectsByType<EnemyBrain>(FindObjectsSortMode.None))ai.enabled=false;
        foreach(var ai in UnityEngine.Object.FindObjectsByType<StoneGuardian>(FindObjectsSortMode.None))ai.enabled=false;
        var drums=UnityEngine.Object.FindObjectsByType<DrumInteractable>(FindObjectsSortMode.None).ToDictionary(d=>Field<DrumNote>(d,"note"));
        Check(drums.Count==3&&drums.Values.All(d=>d.isActiveAndEnabled),"Three valid original DrumInteractable roots active");
        Check(!world.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId),"Fresh native C1 starts unsolved");
        foreach(var pair in drums)
        {
            var d=pair.Value;var v=d.transform.Find("VIS_DongSon_Playable");
            Check(v&&v.GetComponentsInChildren<MeshFilter>(true).All(m=>AssetDatabase.GetAssetPath(m.sharedMesh)=="Assets/_Project/Art/DongSonDrum/Source/source/TrongDongDongSonArt.fbx"),d.name+": supplied FBX still active in Play");
            Check(v.GetComponentsInChildren<Renderer>(true).All(rr=>rr.enabled&&rr.sharedMaterials.All(mat=>mat&&mat.shader.name=="Universal Render Pipeline/Lit"&&mat.GetTexture("_BaseMap")&&mat.GetTexture("_BumpMap")&&mat.GetTexture("_MetallicGlossMap"))),d.name+": complete URP PBR maps on all parts");
            Check(!v.GetComponentsInChildren<Collider>(true).Any(c=>c.enabled),d.name+": no duplicate model collision");
            var col=d.GetComponent<Collider>();var from=col.bounds.center+new Vector3(0,1,-2.3f);var delta=col.bounds.center-from;
            Check(Physics.Raycast(from,delta.normalized,out var hit,4,LayerMask.GetMask("Environment","Interactable"),QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<DrumInteractable>()==d,d.name+": unobstructed plausible eye ray selects correct original interaction collider");
            Check(d.Prompt=="Gõ trống "+DrumInteractable.NoteName(pair.Key),d.name+": Vietnamese Sun/Bird/Water prompt retained");
        }
        motor.Teleport(new Vector3(-14,.2f,141),Quaternion.identity);motor.GetComponent<PlayerCameraController>().SetAngles(0,16);yield return new WaitForSecondsRealtime(.8f);
        yield return new WaitForEndOfFrame();var photo=ScreenCapture.CaptureScreenshotAsTexture();if(photo){File.WriteAllBytes("docs/images/dongson-gameview-2026-11-06.png",photo.EncodeToPNG());Destroy(photo);}
        drums[DrumNote.B].Interact(motor.gameObject);
        yield return Wait(()=>puzzle.State==DrumPuzzleState.Unsolved&&puzzle.CanAcceptInput,5,"Wrong first drum resets attempt without solving");
        drums[DrumNote.A].Interact(motor.gameObject);yield return Wait(()=>puzzle.CanAcceptInput,2,"Sun note debounce completes");
        Find<DrumPuzzleReset>().Interact(motor.gameObject);Check(puzzle.State==DrumPuzzleState.Unsolved,"Existing reset interaction clears partial attempt");
        drums[DrumNote.A].Interact(motor.gameObject);yield return Wait(()=>puzzle.CanAcceptInput,2,"New Sun input accepted");
        world.TryAdd(StateSet.Flags,WorldIds.ClueDrum);yield return null;
        Check(puzzle.State==DrumPuzzleState.Listening,"Unrelated clue/world change retains partial sequence");
        drums[DrumNote.C].Interact(motor.gameObject);yield return Wait(()=>puzzle.CanAcceptInput,2,"Bird input accepted second");
        drums[DrumNote.B].Interact(motor.gameObject);yield return Wait(()=>puzzle.State==DrumPuzzleState.Solved,2,"Sun → Bird → Water solves existing sequence");
        Check(world.HasSolvedPuzzle(WorldStateStore.DrumPuzzleId),"Original pz_trong_01 solved ID persisted in world state");
        Check(!Field<GameObject>(puzzle,"door").activeSelf&&Field<GameObject>(puzzle,"rewardDisplay").activeSelf,"Existing door opens and reward display appears");
        Check(drums.Values.All(d=>!d.CanInteract(motor.gameObject)),"Solved drums cannot retrigger sequence/reward");
        Check(Find<SaveService>().SaveAt(SaveService.CampCheckpointId),"Native schema4 save succeeds with solved supplied drums");
        AppRoot.Instance.Session.ReturnToMainMenu();yield return WaitScene(GameSession.MainMenuScene);
        AppRoot.Instance.Session.Continue();yield return WaitScene(GameSession.FirstChapterScene);yield return new WaitForSecondsRealtime(.3f);
        Check(Find<WorldStateStore>().HasSolvedPuzzle(WorldStateStore.DrumPuzzleId)&&Find<DrumPuzzleController>().State==DrumPuzzleState.Solved,"Menu → Continue restores solved drum sequence");
        Check(UnityEngine.Object.FindObjectsByType<DrumInteractable>(FindObjectsSortMode.None).All(d=>d.transform.Find("VIS_DongSon_Playable").gameObject.activeInHierarchy),"All three supplied drum models survive scene reload/Continue");
        Check(report.errors.Count==0,"No runtime errors in scoped art/puzzle/save regression");completed=true;
    }
}
#endif
