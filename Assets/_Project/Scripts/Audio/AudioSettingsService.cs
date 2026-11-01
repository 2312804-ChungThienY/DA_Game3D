using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-600)]
public class AudioSettingsService : MonoBehaviour
{
    public const float DefaultMaster = 1f;
    public const float DefaultMusic = 0.7f;
    public const float DefaultSfx = 1f;
    private const string MasterKey = "sondoong.audio.master";
    private const string MusicKey = "sondoong.audio.music";
    private const string SfxKey = "sondoong.audio.sfx";

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string masterParameter = "MasterVolume";
    [SerializeField] private string musicParameter = "MusicVolume";
    [SerializeField] private string sfxParameter = "SFXVolume";
    [Tooltip("Optional music sources: turn Play On Awake off. They start after mixer settings apply.")]
    [SerializeField] private AudioSource[] startAfterApply;

    private float master = DefaultMaster;
    private float music = DefaultMusic;
    private float sfx = DefaultSfx;
    private bool started;
    private bool dirty;
    private readonly HashSet<string> missingParameters = new HashSet<string>();

    public float Master => master;
    public float Music => music;
    public float Sfx => sfx;
    public event Action Changed;

    private void Awake()
    {
        master = ReadLevel(MasterKey, DefaultMaster);
        music = ReadLevel(MusicKey, DefaultMusic);
        sfx = ReadLevel(SfxKey, DefaultSfx);
        if (mixer == null || string.IsNullOrWhiteSpace(masterParameter) ||
            string.IsNullOrWhiteSpace(musicParameter) || string.IsNullOrWhiteSpace(sfxParameter))
        {
            Debug.LogError($"{name}: Assign the shared AudioMixer and parameter names.", this);
            enabled = false;
        }
    }

    private void Start()
    {
        started = true;
        ApplyAll();
        Changed?.Invoke();
        if (startAfterApply == null) return;
        foreach (AudioSource source in startAfterApply)
            if (source != null && source.isActiveAndEnabled && source.clip != null) source.Play();
    }

    private static float ReadLevel(string key, float fallback)
    {
        float value = PlayerPrefs.GetFloat(key, fallback);
        if (float.IsNaN(value) || float.IsInfinity(value)) value = fallback;
        return Mathf.Clamp01(value);
    }

    public static float ToDecibels(float value)
    {
        return value <= 0.0001f ? -80f : 20f * Mathf.Log10(Mathf.Clamp01(value));
    }

    public void SetMaster(float value) => SetLevel(ref master, value, MasterKey, masterParameter);
    public void SetMusic(float value) => SetLevel(ref music, value, MusicKey, musicParameter);
    public void SetSfx(float value) => SetLevel(ref sfx, value, SfxKey, sfxParameter);

    private void SetLevel(ref float current, float value, string key, string parameter)
    {
        if (float.IsNaN(value) || float.IsInfinity(value)) return;
        value = Mathf.Clamp01(value);
        if (Mathf.Approximately(current, value)) return;
        current = value;
        PlayerPrefs.SetFloat(key, value); // In-memory change, not a disk flush per drag.
        dirty = true;
        if (started) Apply(parameter, current);
        Changed?.Invoke();
    }

    private void ApplyAll()
    {
        Apply(masterParameter, master);
        Apply(musicParameter, music);
        Apply(sfxParameter, sfx);
    }

    private void Apply(string parameter, float value)
    {
        if (mixer != null && !mixer.SetFloat(parameter, ToDecibels(value)) && missingParameters.Add(parameter))
            Debug.LogError($"AudioMixer parameter is missing: {parameter}", this);
    }

    public void ResetDefaults()
    {
        SetMaster(DefaultMaster);
        SetMusic(DefaultMusic);
        SetSfx(DefaultSfx);
        if (started) ApplyAll();
        SaveChanges();
    }

    public void SaveChanges()
    {
        if (!dirty) return;
        PlayerPrefs.Save();
        dirty = false;
    }

    private void OnApplicationPause(bool paused) { if (paused) SaveChanges(); }
    private void OnApplicationQuit() => SaveChanges();
    private void OnDestroy() => SaveChanges();
}
