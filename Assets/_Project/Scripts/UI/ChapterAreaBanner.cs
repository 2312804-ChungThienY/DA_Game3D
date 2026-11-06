using TMPro;
using UnityEngine;

public sealed class ChapterAreaBanner:MonoBehaviour
{
    [SerializeField] PlayerMotor target;
    [SerializeField] TMP_Text label;
    [SerializeField] CanvasGroup group;
    static readonly Vector3[] centers={new Vector3(0,0,0),new Vector3(18,0,70),new Vector3(-14,0,145),new Vector3(25,0,215),new Vector3(0,0,290)};
    static readonly string[] names={"TRẠI GIẾNG TRỜI","VƯỜN RÊU","ĐIỆN TRỐNG","HÀNH LANG QUANG","ĐÀN KIM QUY"};
    int current=-1;float until;
    void LateUpdate()
    {
        if(!target||!label||!group)return;int area=0;float best=float.MaxValue;
        for(int i=0;i<centers.Length;i++){float d=(target.transform.position-centers[i]).sqrMagnitude;if(d<best){best=d;area=i;}}
        if(area!=current){current=area;label.text=names[area];until=Time.unscaledTime+4;group.alpha=1;}
        if(Time.unscaledTime>until)group.alpha=Mathf.MoveTowards(group.alpha,0,Time.unscaledDeltaTime*.7f);
    }
}
