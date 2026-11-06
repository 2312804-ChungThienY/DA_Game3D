using Unity.Cinemachine;
using UnityEngine;

// Final pipeline safety check after ThirdPersonFollow damping/obstacle avoidance.
// Corrects camera state rather than writing a second transform motor.
[DisallowMultipleComponent]
public sealed class CaveCameraCollisionGuard:CinemachineExtension
{
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase camera,CinemachineCore.Stage stage,ref CameraState state,float deltaTime)
    {
        if(stage!=CinemachineCore.Stage.Finalize||!camera.Follow)return;
        Vector3 origin=camera.Follow.position;
        Vector3 delta=state.GetFinalPosition()-origin;
        if(delta.sqrMagnitude<.0001f)return;
        if(Physics.SphereCast(origin,.20f,delta.normalized,out var hit,delta.magnitude,1<<3,QueryTriggerInteraction.Ignore))
        {
            Vector3 safe=origin+delta.normalized*Mathf.Max(.06f,hit.distance-.035f);
            state.PositionCorrection+=safe-state.GetFinalPosition();
        }
    }
}
