using System;
using UnityEngine;
using UnityEngine.Audio;

// Cài đặt người chơi, lưu riêng bằng PlayerPrefs (không nằm trong file tiến trình).
public class SettingsService : MonoBehaviour
{
    private const string MasterVolumeKey = "settings.masterVolume";
    private const string MusicVolumeKey = "settings.musicVolume";
    private const string SfxVolumeKey = "settings.sfxVolume";
    private const string QualityKey = "settings.quality";
    // Đổi khóa để bỏ giá trị cũ quá nhạy đã lưu trên máy.
    private const string SensitivityKey = "settings.mouseSensitivity.v2";
    private const string FullscreenKey = "settings.fullscreen";

    public const float MinSensitivity = 0.03f;
    public const float MaxSensitivity = 0.6f;

    public float MasterVolume { get; private set; } = 1f;
    public float MusicVolume { get; private set; } = .7f;
    public float SfxVolume { get; private set; } = 1f;
    public int QualityLevel { get; private set; } = 1;
    public AudioMixer Mixer { get; private set; }
    public bool AudioReady { get; private set; }
    private bool dirty;
    private float flushAt;
    public const float DefaultSensitivity = 0.15f;

    public float MouseSensitivity { get; private set; } = DefaultSensitivity;
    public bool Fullscreen { get; private set; } = true;

    public event Action Changed;

    private void Awake()
    {
        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, PlayerPrefs.GetFloat("sondoong.audio.master",1f)));
        MusicVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, PlayerPrefs.GetFloat("sondoong.audio.music",.7f)));
        SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, PlayerPrefs.GetFloat("sondoong.audio.sfx",1f)));
        QualityLevel = Mathf.Clamp(PlayerPrefs.GetInt(QualityKey,1),0,1);
        Mixer = Resources.Load<AudioMixer>("Audio/MainAudioMixer");
        MouseSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity), MinSensitivity, MaxSensitivity);
        Fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        ApplyDisplay();
    }
    private void Start(){ApplyAudio();QualitySettings.SetQualityLevel(Mathf.Min(QualityLevel,QualitySettings.names.Length-1),true);}
    private void Update(){if(dirty&&Time.unscaledTime>=flushAt)Flush();}
    private void OnApplicationPause(bool paused){if(paused)Flush();}
    private void OnApplicationQuit()=>Flush();
    private void OnDestroy()=>Flush();

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        ApplyAudio();Notify();
    }
    public void SetMusicVolume(float value){MusicVolume=Mathf.Clamp01(value);PlayerPrefs.SetFloat(MusicVolumeKey,MusicVolume);ApplyAudio();Notify();}
    public void SetSfxVolume(float value){SfxVolume=Mathf.Clamp01(value);PlayerPrefs.SetFloat(SfxVolumeKey,SfxVolume);ApplyAudio();Notify();}
    public void SetQuality(int value){QualityLevel=Mathf.Clamp(value,0,1);PlayerPrefs.SetInt(QualityKey,QualityLevel);QualitySettings.SetQualityLevel(Mathf.Min(QualityLevel,QualitySettings.names.Length-1),true);Notify();}

    public void SetMouseSensitivity(float value)
    {
        MouseSensitivity = Mathf.Clamp(value, MinSensitivity, MaxSensitivity);
        PlayerPrefs.SetFloat(SensitivityKey, MouseSensitivity);
        Notify();
    }

    public void SetFullscreen(bool value)
    {
        Fullscreen = value;
        PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
        ApplyDisplay();Notify();
    }

    public static float VolumeToDecibels(float value)=>value<=.0001f?-80f:20f*Mathf.Log10(Mathf.Clamp01(value));
    public void ApplyAudio()
    {
        AudioListener.volume = Mixer!=null?1f:MasterVolume;
        AudioReady = Mixer!=null&&Mixer.SetFloat("MasterVolume",VolumeToDecibels(MasterVolume))&&Mixer.SetFloat("MusicVolume",VolumeToDecibels(MusicVolume))&&Mixer.SetFloat("SFXVolume",VolumeToDecibels(SfxVolume));
    }
    private void ApplyDisplay()
    {
        // Không đổi chế độ cửa sổ của Editor.
        if (!Application.isEditor)
            Screen.fullScreenMode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

    }
    private void Notify(){dirty=true;flushAt=Time.unscaledTime+.4f;Changed?.Invoke();}
    public void Flush(){if(!dirty)return;PlayerPrefs.Save();dirty=false;}
}
