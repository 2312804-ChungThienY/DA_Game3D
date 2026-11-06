using UnityEngine;

// The stationary C2 enemy uses the imported Ghost rig; AI owns attacks and projectiles.
[DisallowMultipleComponent]
public sealed class ThrowerPresentation:MonoBehaviour
{
    [SerializeField] EnemyHealth health;
    [SerializeField] ThrowerBrain brain;
    [SerializeField] EnemyLanternReceiver receiver;
    [SerializeField] Animator animator;
    string pose;ThrowerBrain.ThrowerState previous;float lastHp,hurtUntil;
    void Awake(){animator.applyRootMotion=false;lastHp=health.Current;}
    void OnEnable(){if(health)health.Changed+=OnHealth;}
    void OnDisable(){if(health)health.Changed-=OnHealth;}
    void OnHealth(){if(health.Current<lastHp&&!health.IsDead)hurtUntil=Time.time+.22f;lastHp=health.Current;}
    void LateUpdate()
    {
        if(!health||!brain||!animator)return;
        bool attack=brain.enabled&&(brain.State==ThrowerBrain.ThrowerState.Aim||brain.State==ThrowerBrain.ThrowerState.Melee);
        string next=health.IsDead?"Death":receiver&&receiver.IsStunned?"Stunned":Time.time<hurtUntil?"Hurt":attack?"Attack":"Idle";
        if(next!=pose||(attack&&brain.State!=previous))
        {pose=next;animator.CrossFadeInFixedTime(next,.09f,0,0);}
        previous=brain.State;
    }
}
