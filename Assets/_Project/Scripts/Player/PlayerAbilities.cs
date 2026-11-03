using UnityEngine;

// Năng lực vĩnh viễn của An, đọc từ WorldStateStore (tập Abilities) nên được lưu/tải và mang qua chương.
public class PlayerAbilities : MonoBehaviour
{
    public const string MovementKit = WorldIds.MovementKit;
    public const string Wind = WorldIds.AbilityWind;
    public const string Earth = WorldIds.AbilityEarth;
    public const string Water = WorldIds.AbilityWater;

    [SerializeField] private WorldStateStore worldState;

    private void Start()
    {
        // Prefab An không tham chiếu được object scene: tìm một lần khi vào scene.
        if (worldState == null) worldState = FindAnyObjectByType<WorldStateStore>();
        if (worldState == null) Debug.LogError($"{name}: PlayerAbilities không tìm thấy WorldStateStore.", this);
    }

    public bool Has(string abilityId) => worldState != null && worldState.Has(StateSet.Abilities, abilityId);
}
