#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// Runs only when explicitly requested by the development-build command line.
public class AudioSettingsSmokeProbe : MonoBehaviour
{
    static readonly string[] Keys={"sondoong.audio.master","sondoong.audio.music","sondoong.audio.sfx"};
    const BindingFlags Fields=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    string output,phase,backupPath;
    readonly ProbeReport report=new ProbeReport();
    bool completed;
    [Serializable] public class ProbeReport
    {
        public string phase;
        public List<string> checks=new List<string>();
        public List<string> failures=new List<string>();
        public List<string> runtimeErrors=new List<string>();
        public List<string> notes=new List<string>();
    }
    [Serializable] public class PreferencesBackup { public bool[] exists;public float[] values; }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        var args=Environment.GetCommandLineArgs();
        if(!args.Contains("-audio-self-test"))return;
        var obj=new GameObject("AudioSettingsSmokeProbe");DontDestroyOnLoad(obj);
        var probe=obj.AddComponent<AudioSettingsSmokeProbe>();
        probe.phase=Argument(args,"-audio-phase","write");probe.output=Argument(args,"-audio-report",Path.Combine(Application.persistentDataPath,"audio-probe.json"));
        probe.backupPath=Argument(args,"-audio-backup",probe.output+".prefs.json");probe.report.phase=probe.phase;
        Application.runInBackground=true;Application.logMessageReceived+=probe.CaptureLog;
    }
    static string Argument(string[] args,string name,string fallback){int i=Array.IndexOf(args,name);return i>=0&&i+1<args.Length?args[i+1]:fallback;}
    void CaptureLog(string message,string stack,LogType type){if((type==LogType.Error||type==LogType.Exception||type==LogType.Assert)&&report.runtimeErrors.Count<20)report.runtimeErrors.Add(message);}
    IEnumerator Start()
    {
        var routine=Run();
        while(true)
        {
            bool next;object current=null;
            try{next=routine.MoveNext();if(next)current=routine.Current;}
            catch(Exception e){report.failures.Add(e.ToString());RestorePreferences();Finish();yield break;}
            if(!next)break;
            yield return current;
        }
        Finish();
    }
    void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);report.checks.Add(message);}
    static T Find<T>() where T:UnityEngine.Object=>Resources.FindObjectsOfTypeAll<T>().FirstOrDefault(o=>o is Component c&&c.gameObject.scene.IsValid()&&c.gameObject.scene.isLoaded);
    static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,Fields).GetValue(obj);
    static string FileHash(string path)
    {
        if(!File.Exists(path))return "absent";
        using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path)));
    }
    void BackupPreferences()
    {
        if(File.Exists(backupPath))throw new InvalidOperationException("Existing preference backup; restore it before a new write test.");
        var data=new PreferencesBackup{exists=Keys.Select(PlayerPrefs.HasKey).ToArray(),values=Keys.Select(k=>PlayerPrefs.GetFloat(k,0)).ToArray()};
        Directory.CreateDirectory(Path.GetDirectoryName(backupPath));File.WriteAllText(backupPath,JsonUtility.ToJson(data,true));
    }
    void RestorePreferences()
    {
        if(!File.Exists(backupPath))return;
        var data=JsonUtility.FromJson<PreferencesBackup>(File.ReadAllText(backupPath));
        for(int i=0;i<Keys.Length;i++){if(data.exists[i])PlayerPrefs.SetFloat(Keys[i],data.values[i]);else PlayerPrefs.DeleteKey(Keys[i]);}
        PlayerPrefs.Save();
        report.notes.Add("Original user audio preferences restored; no DeleteAll used.");
    }
    void Values(AudioSettingsService service,float master,float music,float sfx,string context)
    {
        Check(Mathf.Approximately(service.Master,master)&&Mathf.Approximately(service.Music,music)&&Mathf.Approximately(service.Sfx,sfx),context);
        var mixer=Field<AudioMixer>(service,"mixer");
        foreach(var pair in new[]{new KeyValuePair<string,float>("MasterVolume",master),new KeyValuePair<string,float>("MusicVolume",music),new KeyValuePair<string,float>("SFXVolume",sfx)})
        {
            Check(mixer.GetFloat(pair.Key,out float actual)&&Mathf.Abs(actual-AudioSettingsService.ToDecibels(pair.Value))<.01f,"Native mixer "+pair.Key+" matches "+context);
        }
    }
    void Sliders(AudioSettingsPanel panel,float master,float music,float sfx)
    {
        Field<Slider>(panel,"masterSlider").value=master;Field<Slider>(panel,"musicSlider").value=music;Field<Slider>(panel,"sfxSlider").value=sfx;
    }
    IEnumerator Run()
    {
        float timeout=Time.realtimeSinceStartup+90;
        while(SceneManager.GetActiveScene().name!="01_MainMenu"||Find<MainMenuController>()==null)
        {if(Time.realtimeSinceStartup>timeout)throw new TimeoutException("Boot did not reach menu");yield return null;}
        yield return null;yield return null;
        var menu=Find<MainMenuController>();var service=Find<AudioSettingsService>();
        Check(menu!=null&&service!=null&&menu.isActiveAndEnabled,"Boot/menu and SceneAudio initialized");
        if(phase=="read")
        {
            Values(service,.42f,.31f,.63f,"preferences persisted across standalone launches");
            RestorePreferences();yield break;
        }
        BackupPreferences();
        string saveHash=FileHash(GameSaveStorage.SavePath),backupHash=FileHash(GameSaveStorage.BackupPath);
        Check(AudioSettingsService.ToDecibels(0)==-80f&&AudioSettingsService.ToDecibels(1)==0f&&Mathf.Abs(AudioSettingsService.ToDecibels(.5f)+6.0206f)<.001f,"Linear-to-dB conversion: mute/unity/half gain");
        menu.OpenAudioSettings();var panel=Field<GameObject>(menu,"audioSettingsPanel").GetComponent<AudioSettingsPanel>();
        Check(panel.gameObject.activeInHierarchy,"Main menu opens audio panel");
        yield return new WaitForSecondsRealtime(.2f);
        Check(!Field<Button>(menu,"newGameButton").interactable&&!Field<Button>(menu,"quitButton").interactable,"Audio modal locks main menu buttons");
        Sliders(panel,.42f,.31f,.63f);Values(service,.42f,.31f,.63f,"menu sliders apply immediately");
        Check(Field<TMP_Text>(panel,"masterValue").text=="42%","Percentage label follows slider");
        panel.ResetDefaults();Values(service,1,.7f,1,"defaults restore only audio settings");
        Check(FileHash(GameSaveStorage.SavePath)==saveHash&&FileHash(GameSaveStorage.BackupPath)==backupHash,"Reset Defaults preserves checkpoint files");
        for(int i=0;i<5;i++){menu.CloseAudioSettings();menu.OpenAudioSettings();}
        var changed=typeof(AudioSettingsService).GetField("Changed",Fields).GetValue(service) as Delegate;
        Check(changed!=null&&changed.GetInvocationList().Length==1,"Repeated panel opens do not duplicate Changed subscriptions");
        Sliders(panel,.42f,.31f,.63f);menu.CloseAudioSettings();
        Check(!panel.gameObject.activeSelf,"Main menu closes audio panel");
        service.SetMaster(1);service.SetMusic(1);service.SetSfx(1);
        var mixer=Field<AudioMixer>(service,"mixer");
        var clip=AudioClip.Create("Verification tone",24000,1,48000,false);
        var pcm=new float[24000];for(int i=0;i<pcm.Length;i++)pcm[i]=.05f*Mathf.Sin(2*Mathf.PI*440*i/48000f);clip.SetData(pcm,0);
        var music=gameObject.AddComponent<AudioSource>();music.clip=clip;music.loop=true;music.playOnAwake=false;music.outputAudioMixerGroup=mixer.FindMatchingGroups("Music").First(g=>g.name=="Music");music.Play();
        yield return new WaitForSecondsRealtime(.35f);float baseline=OutputRms();
        if(baseline>0.00001f)
        {
            service.SetMusic(0);yield return new WaitForSecondsRealtime(.35f);Check(OutputRms()<baseline*.03f,"Music slider mutes Music output stream");
            music.Stop();var effect=gameObject.AddComponent<AudioSource>();effect.clip=clip;effect.loop=true;effect.playOnAwake=false;effect.outputAudioMixerGroup=mixer.FindMatchingGroups("SFX").First(g=>g.name=="SFX");effect.Play();
            yield return new WaitForSecondsRealtime(.35f);Check(OutputRms()>baseline*.2f,"Music mute leaves SFX output audible");
            service.SetSfx(0);yield return new WaitForSecondsRealtime(.35f);Check(OutputRms()<baseline*.03f,"SFX slider mutes SFX output stream");
            service.SetSfx(1);service.SetMaster(0);yield return new WaitForSecondsRealtime(.35f);Check(OutputRms()<baseline*.03f,"Master slider mutes routed output stream");
            effect.Stop();Destroy(effect);
        }
        else report.notes.Add("No listener output stream in unattended mode; mixer native parameters and routing are tested, listening remains manual.");
        Destroy(music);Destroy(clip);
        service.SetMaster(.42f);service.SetMusic(.31f);service.SetSfx(.63f);service.SaveChanges();
        SceneManager.LoadScene("02_HangLinhMach");
        timeout=Time.realtimeSinceStartup+90;
        while(SceneManager.GetActiveScene().name!="02_HangLinhMach"||Find<PauseMenuController>()==null)
        {if(Time.realtimeSinceStartup>timeout)throw new TimeoutException("Gameplay did not load");yield return null;}
        for(int i=0;i<8;i++)yield return null;
        service=Find<AudioSettingsService>();Values(service,.42f,.31f,.63f,"menu-to-gameplay keeps settings");
        var pause=Find<PauseMenuController>();pause.PauseGameplay();
        Check(pause.IsPaused&&Time.timeScale==0&&AudioListener.pause,"Pause freezes gameplay and owned audio");
        pause.OpenAudioSettings();panel=Field<GameObject>(pause,"audioSettingsPanel").GetComponent<AudioSettingsPanel>();
        Check(panel.gameObject.activeInHierarchy&&Time.timeScale==0,"Audio settings opens inside pause without resuming");
        Sliders(panel,.28f,.17f,.53f);pause.CloseAudioSettings();
        Check(pause.IsPaused&&Time.timeScale==0&&Field<GameObject>(pause,"pausePanel").activeSelf&&!panel.gameObject.activeSelf,"Closing audio returns to pause, not gameplay");
        Values(service,.28f,.17f,.53f,"pause sliders apply while gameplay stays paused");
        var motor=Find<StarterAssets.ThirdPersonController>();
        Check(motor!=null&&motor.SfxOutput!=null&&motor.SfxOutput.name=="SFX"&&motor.SfxOutput.audioMixer==mixer,"An footsteps and landing use the shared SFX group");
        RoutedAudio.PlayClipAtPoint(AudioClip.Create("Routing verification",480,1,48000,false),motor.transform.position,.1f,motor.SfxOutput);
        var routed=Resources.FindObjectsOfTypeAll<AudioSource>().Where(a=>a.gameObject.scene.IsValid()).FirstOrDefault(a=>a.name=="OneShot_SFX");
        Check(routed!=null&&routed.outputAudioMixerGroup==motor.SfxOutput,"Temporary one-shot source routes into SFX");
        Check(FileHash(GameSaveStorage.SavePath)==saveHash&&FileHash(GameSaveStorage.BackupPath)==backupHash,"Audio changes across scenes preserve save progress");
        report.notes.Add("Esc settings-first branch is wired in PauseMenuController; unattended test verifies its close operation and pause state without synthesizing focused keyboard input.");
        SceneManager.LoadScene("01_MainMenu");yield return null;yield return null;yield return null;
        service=Find<AudioSettingsService>();Values(service,.28f,.17f,.53f,"gameplay-to-menu keeps settings");
        service.SetMaster(.42f);service.SetMusic(.31f);service.SetSfx(.63f);service.SaveChanges();
        Check(report.runtimeErrors.Count==0,"No runtime errors during audio smoke test");
    }
    static float OutputRms(){var data=new float[2048];AudioListener.GetOutputData(data,0);double sum=0;foreach(float x in data)sum+=x*x;return (float)Math.Sqrt(sum/data.Length);}
    void Finish()
    {
        if(completed)return;completed=true;Application.logMessageReceived-=CaptureLog;
        Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonUtility.ToJson(report,true));
        Application.Quit(report.failures.Count==0?0:1);
    }
    void OnApplicationQuit(){if(!completed){report.failures.Add("Player exited before the smoke test completed");RestorePreferences();Finish();}}
}
#endif
