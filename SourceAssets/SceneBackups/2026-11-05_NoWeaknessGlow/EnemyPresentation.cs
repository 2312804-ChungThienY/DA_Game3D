using UnityEngine;
using UnityEngine.AI;

// The imported rig follows the existing FSM. Animation never owns actor motion or damage.
public sealed class EnemyPresentation:MonoBehaviour
{
    [SerializeField] EnemyHealth health;
    [SerializeField] EnemyBrain brain;
    [SerializeField] StoneGuardian guardian;
    [SerializeField] Animator animator;
    [SerializeField] Renderer[] surfaces;
    EnemyLanternReceiver lantern;MaterialPropertyBlock block;Vector3 previous;
    string pose;float hurtUntil,nextVoice;float lastHp,stepDistance;bool struck;
    void Awake(){lantern=health.GetComponent<EnemyLanternReceiver>();block=new MaterialPropertyBlock();previous=health.transform.position;lastHp=health.Current;animator.applyRootMotion=false;}
    void OnEnable(){if(health)health.Changed+=OnHealth;}
    void OnDisable(){if(health)health.Changed-=OnHealth;}
    void OnHealth(){if(health.Current<lastHp&&!health.IsDead){hurtUntil=Time.time+.24f;SoundscapeRuntime.Play(guardian?SoundCue.Stone:SoundCue.Hit,health.transform.position,.55f);}lastHp=health.Current;}
    void LateUpdate()
    {
        if(!animator||!health)return;
        Vector3 delta=health.transform.position-previous;previous=health.transform.position;
        string next=health.IsDead?"Death":Time.time<hurtUntil?"Hurt":delta.sqrMagnitude>.00001f?"Walk":"Idle";
        bool attack=guardian&&(guardian.State==StoneGuardian.GuardianState.Sweep||guardian.State==StoneGuardian.GuardianState.Slam)||brain&&brain.State==EnemyState.Attack;
        bool stunned=lantern&&lantern.IsStunned&&!health.IsDead;
        if(stunned){next="Stunned";attack=false;}
        else if(attack&&!health.IsDead)next=guardian?(guardian.State==StoneGuardian.GuardianState.Sweep?"Sweep":"Slam"):"Attack";
        if(next!=pose)
        {
            pose=next;animator.CrossFadeInFixedTime(pose,.09f,0,0);struck=false;
            if(next=="Death")SoundscapeRuntime.Play(guardian?SoundCue.Stone:SoundCue.Ghost,health.transform.position,.7f);
            if(brain&&next=="Walk"&&Time.time>=nextVoice){SoundscapeRuntime.Play(SoundCue.Ghost,health.transform.position,.23f);nextVoice=Time.time+7;}
        }
        if(attack&&!struck)
        {
            float elapsed=guardian?guardian.StateElapsed:brain.StateElapsed;
            float windup=guardian?guardian.AttackWindup:brain.Definition.attackTelegraph;
            if(elapsed>=windup){struck=true;SoundscapeRuntime.Play(guardian?SoundCue.Stone:SoundCue.Hit,health.transform.position,.8f);}
        }
        if(guardian&&!health.IsDead&&Time.deltaTime>0){stepDistance+=delta.magnitude;if(stepDistance>1.2f){stepDistance=0;SoundscapeRuntime.Play(SoundCue.Stone,health.transform.position,.22f);}}
        bool weak=lantern&&lantern.IsWeakened;
        foreach(var rr in surfaces)
        {
            if(!rr)continue;rr.GetPropertyBlock(block);block.SetColor("_EmissionColor",weak?new Color(.12f,.45f,.31f):Color.black);rr.SetPropertyBlock(block);
        }
    }
}
