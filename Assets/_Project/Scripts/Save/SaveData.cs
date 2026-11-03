using System;
using System.Collections.Generic;

// Schema 4 (spec chapters/00 mục 5). Máu, thể lực, đèn không lưu: đặt đầy khi tải tại mốc.
[Serializable]
public class SaveData
{
    public const int CurrentSchema = 4;

    public int schemaVersion = CurrentSchema;
    public string buildVersion;
    public long timestampUtcTicks;
    public string chapterId = "c1";
    public string sceneName = GameSession.FirstChapterScene;
    public string checkpointId = SaveService.CampCheckpointId;
    public List<InventorySlotDto> inventory = new List<InventorySlotDto>();
    public WorldSnapshot world = new WorldSnapshot();
    public float elapsedPlaySeconds;
}

[Serializable]
public class InventorySlotDto
{
    public int slot;
    public string itemId;
    public int quantity;
}

[Serializable]
public class WorldSnapshot
{
    public List<StringSetDto> sets = new List<StringSetDto>();
    public List<IntEntryDto> ints = new List<IntEntryDto>();

    public List<string> Get(StateSet set)
    {
        string key = set.ToString();
        if (sets != null)
            foreach (StringSetDto dto in sets)
                if (dto != null && dto.set == key) return dto.ids ?? new List<string>();
        return new List<string>();
    }
}

[Serializable]
public class StringSetDto
{
    public string set;
    public List<string> ids = new List<string>();
}

[Serializable]
public class IntEntryDto
{
    public string key;
    public int value;
}

// Định dạng version 1–5 của chương 1 trước merge, chỉ dùng để chuyển đổi khi đọc.
[Serializable]
public class LegacySaveData
{
    public int version;
    public int schemaVersion;
    public string checkpointId;
    public bool hasLantern;
    public bool campSealSolved;
    public bool relicRestored;
    public List<string> collectedRelics;
    public List<string> solvedPuzzles;
    public List<string> defeatedEnemies;
    public List<string> collectedPickups;
    public List<LegacyInventorySlot> inventory;
}

[Serializable]
public class LegacyInventorySlot
{
    public string itemId;
    public int quantity;
}
