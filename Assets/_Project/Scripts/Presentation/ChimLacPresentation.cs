using UnityEngine;

// Combat-readable project mesh; the boss FSM alone owns root movement.
public sealed class ChimLacPresentation:MonoBehaviour
{
    [SerializeField] BossChimLac boss;
    [SerializeField] Transform leftWing,rightWing,neck,stars;
    void LateUpdate()
    {
        bool stunned=boss.State==BossChimLac.ChimState.Stunned;
        bool flying=boss.State==BossChimLac.ChimState.FlyToPerch||boss.State==BossChimLac.ChimState.Gust;
        float flap=flying?Mathf.Sin(Time.time*(boss.GustIsActive?18:10))*28:stunned?-18:8;
        leftWing.localRotation=Quaternion.Euler(0,0,-flap);rightWing.localRotation=Quaternion.Euler(0,0,flap);
        neck.localRotation=Quaternion.Euler(stunned?58:boss.State==BossChimLac.ChimState.Dive?25:0,0,0);
        stars.gameObject.SetActive(stunned);
        if(stunned)stars.localRotation=Quaternion.Euler(0,Time.time*90,0);
    }
}
