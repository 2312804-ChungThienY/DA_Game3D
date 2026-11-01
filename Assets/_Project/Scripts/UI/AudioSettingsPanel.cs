using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsPanel : MonoBehaviour
{
    [SerializeField] private AudioSettingsService settings;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text masterValue;
    [SerializeField] private TMP_Text musicValue;
    [SerializeField] private TMP_Text sfxValue;
    private bool ready;

    private void Awake()
    {
        ready = settings != null && masterSlider != null && musicSlider != null && sfxSlider != null;
        if (!ready)
        {
            Debug.LogError($"{name}: Assign service and three sliders.", this);
            enabled = false;
            return;
        }
        Configure(masterSlider);
        Configure(musicSlider);
        Configure(sfxSlider);
    }

    private static void Configure(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void OnEnable()
    {
        if (!ready) return;
        settings.Changed += Refresh;
        masterSlider.onValueChanged.AddListener(settings.SetMaster);
        musicSlider.onValueChanged.AddListener(settings.SetMusic);
        sfxSlider.onValueChanged.AddListener(settings.SetSfx);
        Refresh();
    }

    private void OnDisable()
    {
        if (!ready || settings == null) return;
        settings.Changed -= Refresh;
        masterSlider.onValueChanged.RemoveListener(settings.SetMaster);
        musicSlider.onValueChanged.RemoveListener(settings.SetMusic);
        sfxSlider.onValueChanged.RemoveListener(settings.SetSfx);
        settings.SaveChanges();
    }

    private void Refresh()
    {
        masterSlider.SetValueWithoutNotify(settings.Master);
        musicSlider.SetValueWithoutNotify(settings.Music);
        sfxSlider.SetValueWithoutNotify(settings.Sfx);
        if (masterValue != null) masterValue.text = $"{Mathf.RoundToInt(settings.Master * 100f)}%";
        if (musicValue != null) musicValue.text = $"{Mathf.RoundToInt(settings.Music * 100f)}%";
        if (sfxValue != null) sfxValue.text = $"{Mathf.RoundToInt(settings.Sfx * 100f)}%";
    }

    public void ResetDefaults()
    {
        if (ready) settings.ResetDefaults();
    }
}
