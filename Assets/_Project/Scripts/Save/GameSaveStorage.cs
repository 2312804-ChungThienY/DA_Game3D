using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public static class GameSaveStorage
{
    public static string SavePath => Path.Combine(Application.persistentDataPath, "save_v1.json");
    public static string BackupPath => SavePath + ".bak";
    public static bool HasAnySaveFiles => File.Exists(SavePath) || File.Exists(BackupPath);

    [Serializable]
    private class SaveHeader
    {
        public int version = 0;
        public string checkpointId = null;
    }

    public static bool TryReadBest(out SaveData data, out bool usedBackup)
    {
        usedBackup = false;
        if (TryReadOne(SavePath, out data)) return true;
        if (!TryReadOne(BackupPath, out data)) return false;
        usedBackup = true;
        return true;
    }

    public static bool TryReadOne(string path, out SaveData data)
    {
        data = null;
        try
        {
            if (!File.Exists(path)) return false;
            string json = File.ReadAllText(path);
            SaveHeader header = JsonUtility.FromJson<SaveHeader>(json);
            if (header == null || header.version < 1 || header.version > 4)
                return false;

            SaveData candidate = JsonUtility.FromJson<SaveData>(json);
            if (candidate == null || candidate.version != header.version) return false;
            // Earliest camp-only saves did not always serialize a checkpoint ID.
            if (string.IsNullOrWhiteSpace(header.checkpointId))
            {
                if (header.version > 2) return false;
                candidate.checkpointId = "cp_trai";
            }
            if (candidate.version == 1)
            {
                candidate.solvedPuzzles = new List<string>();
                candidate.version = 2;
            }
            if (candidate.version == 2)
            {
                candidate.defeatedEnemies = new List<string>();
                candidate.version = 3;
            }
            if (candidate.version == 3)
            {
                if (candidate.defeatedEnemies == null)
                    candidate.defeatedEnemies = new List<string>();
                candidate.relicRestored = false;
                candidate.version = 4;
            }
            if (!IsValid(candidate)) return false;
            data = candidate;
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not read save: {exception.Message}");
            return false;
        }
    }

    public static bool TryWrite(SaveData data)
    {
        if (!IsValid(data)) return false;
        string temporaryPath = SavePath + ".tmp";
        try
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            using (FileStream stream = new FileStream(
                       temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(JsonUtility.ToJson(data, true));
                writer.Flush();
                stream.Flush(true);
            }
            if (!TryReadOne(temporaryPath, out _)) return false;
            if (File.Exists(SavePath))
            {
                // A corrupt primary must not overwrite a usable backup.
                bool primaryValid = TryReadOne(SavePath, out SaveData ignored);
                File.Replace(temporaryPath, SavePath, primaryValid ? BackupPath : null);
            }
            else File.Move(temporaryPath, SavePath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Could not write save: {exception.Message}");
            return false;
        }
    }

    public static bool IsValid(SaveData data)
    {
        bool knownCheckpoint = data != null &&
            (data.checkpointId == "cp_trai" || data.checkpointId == "cp_truoc_dan" ||
             data.checkpointId == "cp_kim_quy");
        if (!knownCheckpoint || data.version != 4 || data.collectedRelics == null ||
            data.solvedPuzzles == null || data.defeatedEnemies == null) return false;

        HashSet<string> relics = new HashSet<string>(StringComparer.Ordinal);
        foreach (string id in data.collectedRelics)
            if (!WorldStateStore.IsValidRelicId(id) || !relics.Add(id)) return false;

        HashSet<string> puzzles = new HashSet<string>(StringComparer.Ordinal);
        foreach (string id in data.solvedPuzzles)
        {
            bool supported = id == WorldStateStore.DrumPuzzleId || id == WorldStateStore.MirrorPuzzleId;
            if (!supported || !puzzles.Add(id)) return false;
        }

        HashSet<string> enemies = new HashSet<string>(StringComparer.Ordinal);
        foreach (string id in data.defeatedEnemies)
            if (id != WorldStateStore.GuardianId || !enemies.Add(id)) return false;

        bool drum = puzzles.Contains(WorldStateStore.DrumPuzzleId);
        bool mirror = puzzles.Contains(WorldStateStore.MirrorPuzzleId);
        bool guardian = enemies.Contains(WorldStateStore.GuardianId);
        bool complete = relics.Contains(WorldStateStore.DrumA) && relics.Contains(WorldStateStore.DrumB) &&
            relics.Contains(WorldStateStore.DrumC) && drum && mirror;
        if (data.campSealSolved && !data.hasLantern) return false;
        if ((relics.Count > 0 || puzzles.Count > 0 || enemies.Count > 0 || data.relicRestored) &&
            (!data.hasLantern || !data.campSealSolved)) return false;
        if (relics.Contains(WorldStateStore.DrumB) && !drum) return false;
        if (relics.Contains(WorldStateStore.DrumC) && !mirror) return false;
        if (mirror && !drum) return false;
        if ((data.checkpointId == "cp_truoc_dan" || data.checkpointId == "cp_kim_quy" || guardian) &&
            !complete) return false;
        if (data.relicRestored && (!guardian || data.checkpointId != "cp_kim_quy")) return false;
        if (data.checkpointId == "cp_kim_quy" && !data.relicRestored) return false;
        return true;
    }
}
