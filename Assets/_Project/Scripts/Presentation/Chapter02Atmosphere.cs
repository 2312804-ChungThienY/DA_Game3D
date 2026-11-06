using TMPro;
using UnityEngine;

// Presentation only. Wind activation, boss movement and progress remain owned by gameplay components.
public sealed class Chapter02Atmosphere : MonoBehaviour
{
    [SerializeField] PlayerMotor player;
    [SerializeField] BossChimLac boss;
    [SerializeField] CanvasGroup banner;
    [SerializeField] TMP_Text zoneText;
    [SerializeField] WorldStateStore worldState;
    [SerializeField] TMP_Text progressText;
    [SerializeField] AudioSource[] windSources;
    [SerializeField] WindColumn[] winds;
    int zone=-1;
    float bannerUntil;
    BossChimLac.ChimState previous;
    static readonly string[] Names={"ĐÁY GIẾNG GIÓ","VÁCH GIÓ","NHÀ SÀN ĐÁ TREO","CẦU MÂY GÃY","ĐỈNH TỔ CHIM"};
    static readonly string[] Fragments={"c2_frag_1","c2_frag_2","c2_frag_3"};
    void Update()
    {
        if(!player||!AppRoot.Instance)return;
        if(worldState&&progressText)
        {
            int fragments=0;foreach(string id in Fragments)if(worldState.Has(StateSet.Fragments,id))fragments++;
            string text=$"Mảnh cổ vật  {fragments}/3  ·  Chuông "+(worldState.Has(StateSet.Puzzles,"c2_pz_chuong_gio")?"đã giải":"chưa giải");
            if(progressText.text!=text)progressText.text=text;
        }
        bool playing=AppRoot.Instance.Session.State==GameSession.SessionState.Playing;
        if(playing)
        {
            float z=player.transform.position.z;
            int current=z<10?0:z<24?1:z<40?2:z<73?3:4;
            if(current!=zone){zone=current;zoneText.text="CHƯƠNG II  ·  "+Names[zone];bannerUntil=Time.time+4.5f;}
            banner.alpha=Mathf.MoveTowards(banner.alpha,Time.time<bannerUntil?1:0,Time.deltaTime*2);
        }
        else banner.alpha=0;
        for(int i=0;i<winds.Length;i++)
        {
            var source=windSources[i];
            if(winds[i].IsActive&&!source.isPlaying&&playing)source.Play();
            else if(!winds[i].IsActive&&source.isPlaying)source.Stop();
        }
        if(boss&&boss.gameObject.activeInHierarchy&&boss.State!=previous)
        {
            if(playing&&(boss.State==BossChimLac.ChimState.Gust||boss.State==BossChimLac.ChimState.DiveTelegraph))SoundscapeRuntime.Play(SoundCue.Bird,boss.transform.position,.4f);
            previous=boss.State;
        }
    }
}
