using UnityEngine;

public enum DamageTeam
{
    Neutral,
    Player,
    Enemy
}

public struct DamageInfo
{
    public GameObject Source;
    public float Amount;
    public DamageTeam SourceTeam;
    public string AttackId;

    public DamageInfo(GameObject source, float amount, DamageTeam sourceTeam)
    {
        Source = source;
        Amount = amount;
        SourceTeam = sourceTeam;
        AttackId = null;
    }
}
