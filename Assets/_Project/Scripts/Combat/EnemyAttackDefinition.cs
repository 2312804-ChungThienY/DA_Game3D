using UnityEngine;

public enum EnemyAttackShape
{
    FrontCone,
    Radial
}

[CreateAssetMenu(fileName = "EnemyAttack", menuName = "Son Doong/Combat/Enemy Attack")]
public class EnemyAttackDefinition : ScriptableObject
{
    public EnemyAttackShape Shape = EnemyAttackShape.FrontCone;
    [Min(0.01f)] public float Damage = 20f;
    [Min(0.01f)] public float StartRange = 2f;
    [Min(0.01f)] public float HitRange = 2.4f;
    [Range(1f, 360f)] public float FullHitAngle = 120f;
    [Min(0.01f)] public float WindupTime = 0.8f;
    [Min(0.01f)] public float ActiveDuration = 0.2f;
    [Min(0f)] public float RecoveryTime = 1.2f;
    [Min(0f)] public float MinimumStartInterval = 0f;

    public bool IsValid =>
        Damage > 0f && StartRange > 0f && HitRange > 0f &&
        FullHitAngle > 0f && FullHitAngle <= 360f &&
        WindupTime > 0f && ActiveDuration > 0f &&
        RecoveryTime >= 0f && MinimumStartInterval >= 0f;
}
