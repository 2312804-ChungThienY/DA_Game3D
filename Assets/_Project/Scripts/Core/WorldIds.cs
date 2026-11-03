using System;
using System.Collections.Generic;

// Danh mục ID hợp lệ theo StateSet cho mọi chương đã làm. Không đổi ID sau khi đã có bản lưu.
// (Bản ScriptableObject WorldIdCatalog trong spec chưa làm; lớp tĩnh này giữ cùng vai trò.)
public static class WorldIds
{
    // Chương 1
    public const string FlagLantern = "equipment_lantern";
    public const string SealGarden = "seal_vuon_01";
    public const string DoorKimQuy = "door_dan_kim_quy";
    public const string GuardianBoss = "guardian_01";
    public const string RelicRestoredC1 = "c1_relic_restored";
    public const string LegacyRelicRestoredFlag = "relic_restored"; // bản lưu v4 đầu tiên dùng Flags

    // Năng lực (PlayerAbilities)
    public const string MovementKit = "movement_kit";
    public const string AbilityWind = "ability_wind";
    public const string AbilityEarth = "ability_earth";
    public const string AbilityWater = "ability_water";

    private static readonly Dictionary<StateSet, HashSet<string>> Known = new Dictionary<StateSet, HashSet<string>>
    {
        { StateSet.Fragments, Set(WorldStateStore.DrumA, WorldStateStore.DrumB, WorldStateStore.DrumC,
                                  "c2_frag_1", "c2_frag_2", "c2_frag_3") },
        { StateSet.Puzzles, Set(WorldStateStore.DrumPuzzleId, WorldStateStore.MirrorPuzzleId, RelicRestoredC1,
                                "c2_pz_chuong_gio", "c2_wind_p3_final", "c2_relic_restored", "c2_linhthan_done") },
        { StateSet.Pickups, Set(
            "pickup_la_cam_01", "pickup_la_cam_02", "pickup_la_cam_03",
            "pickup_linh_chi_01", "pickup_linh_chi_02", "pickup_linh_chi_03",
            "pickup_thuoc_mau_01",
            "c2_ped_p1", "c2_ped_p2", "c2_ped_p3", "c2_ped_p4",
            "c2_pickup_la_cam_01", "c2_pickup_linh_chi_01", "c2_pickup_linh_chi_02") },
        { StateSet.Seals, Set(SealGarden) },
        { StateSet.Doors, Set(DoorKimQuy, "c2_door_arena") },
        { StateSet.Bosses, Set(GuardianBoss, "c2_boss_chim_lac") },
        { StateSet.BossTools, Set("c2_tool_long_gio_lon") },
        { StateSet.Abilities, Set(MovementKit, AbilityWind, AbilityEarth, AbilityWater) },
        { StateSet.Lore, Set("c1_lore_1", "c1_lore_2", "c1_lore_3",
                             "c2_lore_p1", "c2_lore_p2", "c2_lore_p3", "c2_lore_p4", "c2_lore_p5") },
        { StateSet.QuizAnswered, Set("c2_qz_p1", "c2_qz_p2", "c2_qz_p3", "c2_qz_p4", "c2_qz_lt1", "c2_qz_lt2", "c2_qz_lt3") },
        { StateSet.QuizFirstTry, Set("c2_qz_p1", "c2_qz_p2", "c2_qz_p3", "c2_qz_p4", "c2_qz_lt1", "c2_qz_lt2", "c2_qz_lt3") },
        { StateSet.Flags, Set(FlagLantern) }
    };

    // Mốc lưu theo scene.
    private static readonly Dictionary<string, string> CheckpointScene = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        { "cp_trai", "02_HangLinhMach" }, { "cp_dien_trong", "02_HangLinhMach" },
        { "cp_truoc_dan", "02_HangLinhMach" }, { "cp_kim_quy", "02_HangLinhMach" },
        { "c2_cp_start", "03_GiengTroiChimLac" }, { "c2_cp_mid", "03_GiengTroiChimLac" },
        { "c2_cp_arena", "03_GiengTroiChimLac" }
    };

    public static bool IsKnown(StateSet set, string id)
    {
        return !string.IsNullOrEmpty(id) && Known.TryGetValue(set, out HashSet<string> ids) && ids.Contains(id);
    }

    public static bool IsKnownCheckpoint(string id) => id != null && CheckpointScene.ContainsKey(id);

    public static string SceneOfCheckpoint(string id) =>
        id != null && CheckpointScene.TryGetValue(id, out string scene) ? scene : null;

    private static HashSet<string> Set(params string[] ids) => new HashSet<string>(ids, StringComparer.Ordinal);
}
