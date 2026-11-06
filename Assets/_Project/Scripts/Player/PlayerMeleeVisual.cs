using UnityEngine;

// Cosmetic knife only. Melee attacks must not emit a beam or a luminous ribbon.
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
    private void LateUpdate(){ResetTrail();}
    public void ResetTrail(){if(bladeTrail!=null){bladeTrail.emitting=false;bladeTrail.enabled=false;if(bladeTrail.positionCount>0)bladeTrail.Clear();}}
}
