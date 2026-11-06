using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class SliderValueLabel:MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] bool decimals;
    TMP_Text label;
    void OnEnable(){label=GetComponent<TMP_Text>();if(slider){slider.onValueChanged.AddListener(Refresh);Refresh(slider.value);}}
    void OnDisable(){if(slider)slider.onValueChanged.RemoveListener(Refresh);}
    void Refresh(float value){if(label)label.text=decimals?value.ToString("0.00"):Mathf.RoundToInt(value*100)+"%";}
}
