using UnityEngine;
using UnityEngine.UI;
using TMPro;

public sealed class ChimLacArenaHUD:MonoBehaviour,IUiPauseOwner
{
    [SerializeField] BossChimLac boss;
    [SerializeField] ChimLacFocusReceiver focus;
    [SerializeField] TMP_Text stateText;
    [SerializeField] GameObject hintGroup;
    [SerializeField] TMP_Text focusText;
    [SerializeField] RectTransform focusFill;
    [SerializeField] GameObject focusGroup;
    [SerializeField] GameObject retryPanel;
    [SerializeField] Button retryButton;
    bool dismissed;
    public bool IsOpen=>retryPanel&&retryPanel.activeSelf;
    void Awake(){retryPanel.SetActive(false);}
    void LateUpdate()
    {
        if(!boss)return;
        if(boss.DefeatSavePending&&!dismissed&&!IsOpen&&AppRoot.Instance.Session.PushUiPause(this))retryPanel.SetActive(true);
        if(!boss.DefeatSavePending){dismissed=false;if(IsOpen)CloseFromBack();}
        stateText.gameObject.SetActive(boss.IsActive||boss.DefeatSavePending);
        if(hintGroup)hintGroup.SetActive(boss.IsActive||boss.DefeatSavePending);
        string message="";
        switch(boss.State)
        {
            case BossChimLac.ChimState.Perched:message="Chim đang đậu — vào cột gió, giữ chuột phải để chiếu đèn";break;
            case BossChimLac.ChimState.Gust:message=boss.GustIsActive?"Gió mạnh — trụ trong cột gió hoặc tránh mép vực":"Sắp quạt gió — vào cột gió để trụ vững";break;
            case BossChimLac.ChimState.DiveTelegraph:message="Sắp lao xuống — rời vệt đỏ và vòng điểm đáp";break;
            case BossChimLac.ChimState.Dive:message="Né sang bên!";break;
            case BossChimLac.ChimState.Stunned:message="Chim choáng — nhảy xuống, áp sát và tấn công";break;
            case BossChimLac.ChimState.Landed:message="Chim vừa đáp — tranh thủ tấn công";break;
            case BossChimLac.ChimState.FlyToPerch:message="Chim đang lấy độ cao — quan sát cột đậu tiếp theo";break;
        }
        stateText.text=boss.DefeatSavePending?"Đã giải thoát Chim Lạc. Có thể lưu lại ở bệ.":(boss.Enraged?"CHIM LẠC NỔI GIẬN · ":"")+message;
        focusGroup.SetActive(boss.IsActive&&boss.State==BossChimLac.ChimState.Perched);
        var anchor=focusFill.anchorMax;anchor.x=Mathf.Clamp01(focus.Progress/1.5f);focusFill.anchorMax=anchor;
        focusText.text=focus.CanReceiveFocus?$"Chiếu liên tục: {Mathf.Min(1.5f,focus.Progress):0.0}/1,5 giây":"Cần lơ lửng trong cột gió để chiếu tới Chim Lạc";
    }
    public void Retry(){if(boss.TrySaveDefeat())CloseFromBack();}
    public void CloseFromBack(){if(!IsOpen)return;retryPanel.SetActive(false);dismissed=true;if(AppRoot.Instance)AppRoot.Instance.Session.PopUiPause(this);}
    void OnDisable(){CloseFromBack();}
}
