using System;
using UnityEngine;

// Cài đặt người chơi, lưu riêng bằng PlayerPrefs (không nằm trong file tiến trình).
public class SettingsService : MonoBehaviour
{
    private const string MasterVolumeKey = "settings.masterVolume";
    // Đổi khóa để bỏ giá trị cũ quá nhạy đã lưu trên máy.
    private const string SensitivityKey = "settings.mouseSensitivity.v2";
    private const string FullscreenKey = "settings.fullscreen";

    public const float MinSensitivity = 0.03f;
    public const float MaxSensitivity = 0.6f;

    public float MasterVolume { get; private set; } = 1f;
    public const float DefaultSensitivity = 0.15f;

    public float MouseSensitivity { get; private set; } = DefaultSensitivity;
    public bool Fullscreen { get; private set; } = true;

    public event Action Changed;

    private void Awake()
    {
        MasterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        MouseSensitivity = Mathf.Clamp(PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity), MinSensitivity, MaxSensitivity);
        Fullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        Apply();
    }

    public void SetMasterVolume(float value)
    {
        MasterVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, MasterVolume);
        Apply();
    }

    public void SetMouseSensitivity(float value)
    {
        MouseSensitivity = Mathf.Clamp(value, MinSensitivity, MaxSensitivity);
        PlayerPrefs.SetFloat(SensitivityKey, MouseSensitivity);
        Apply();
    }

    public void SetFullscreen(bool value)
    {
        Fullscreen = value;
        PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
        Apply();
    }

    private void Apply()
    {
        AudioListener.volume = MasterVolume;

        // Không đổi chế độ cửa sổ của Editor.
        if (!Application.isEditor)
            Screen.fullScreenMode = Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;

        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
