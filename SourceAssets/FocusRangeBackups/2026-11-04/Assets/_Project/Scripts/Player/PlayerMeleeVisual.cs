using UnityEngine;

// Cosmetic blade/trail only. CombatController owns damage; PlayerMotor owns motion.
[DisallowMultipleComponent]
public sealed class PlayerMeleeVisual : MonoBehaviour
{
    [SerializeField] private TrailRenderer bladeTrail;
    [SerializeField] private Transform knifeGrip;
    private CombatController combat;
    public Transform KnifeGrip => knifeGrip;
    public bool IsTrailEmitting => bladeTrail != null && bladeTrail.emitting;
    private void Awake(){combat=GetComponent<CombatController>();ResetTrail();}
    private void OnEnable(){if(combat==null)combat=GetComponent<CombatController>();if(combat!=null)combat.AttackStarted+=OnAttack;}
    private void OnDisable(){if(combat!=null)combat.AttackStarted-=OnAttack;ResetTrail();}
    private void OnAttack(bool lunge){ResetTrail();}
    private void LateUpdate(){if(bladeTrail!=null)bladeTrail.emitting=combat!=null&&combat.IsInDamageWindow&&Time.timeScale>0;}
    public void ResetTrail(){if(bladeTrail!=null){bladeTrail.emitting=false;bladeTrail.Clear();}}
}
