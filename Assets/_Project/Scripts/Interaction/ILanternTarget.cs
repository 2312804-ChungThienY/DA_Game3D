using UnityEngine;

public interface ILanternTarget
{
    Transform AimPoint { get; }

    bool CanReceiveFocus { get; }

    void ReceiveFocus(float deltaTime);
}

// Enemies use the longer combat-focus reach; puzzle targets retain their range.
public interface IEnemyLanternTarget : ILanternTarget { }
