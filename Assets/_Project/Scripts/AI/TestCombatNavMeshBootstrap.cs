using Unity.AI.Navigation;
using UnityEngine;

// The test scene can run before its Editor-baked NavMeshData has been saved.
[DefaultExecutionOrder(-500)]
[RequireComponent(typeof(NavMeshSurface))]
public class TestCombatNavMeshBootstrap : MonoBehaviour
{
    private void Awake()
    {
        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        if (surface.navMeshData == null)
            surface.BuildNavMesh();
    }
}
