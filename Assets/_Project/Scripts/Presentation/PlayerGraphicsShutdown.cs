using UnityEngine;
using UnityEngine.Rendering;

// The installed RP keeps GPU Resident Drawer buffers alive on player shutdown.
// Use its public reinitialization path once at quit; project assets and gameplay rendering stay intact.
public static class PlayerGraphicsShutdown
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void Register()
    {
#if !UNITY_EDITOR
        Application.quitting -= Release;
        Application.quitting += Release;
#endif
    }

    static void Release()
    {
        if (Application.isEditor || !(GraphicsSettings.currentRenderPipeline is IGPUResidentRenderPipeline pipeline) ||
            pipeline.gpuResidentDrawerMode == GPUResidentDrawerMode.Disabled) return;

        // ScriptableObject state here is in player memory only. Reinitialize disposes the current drawer.
        pipeline.gpuResidentDrawerMode = GPUResidentDrawerMode.Disabled;
        IGPUResidentRenderPipeline.ReinitializeGPUResidentDrawer();
    }
}
