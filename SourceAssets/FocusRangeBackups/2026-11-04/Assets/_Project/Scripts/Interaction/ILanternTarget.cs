using UnityEngine;

public interface ILanternTarget
{
    Transform AimPoint { get; }

    bool CanReceiveFocus { get; }

    void ReceiveFocus(float deltaTime);
}