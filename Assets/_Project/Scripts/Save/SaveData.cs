using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 4;
    public string checkpointId = "cp_trai";
    public bool hasLantern;
    public bool campSealSolved;
    public bool relicRestored;
    public List<string> collectedRelics = new List<string>();
    public List<string> solvedPuzzles = new List<string>();
    public List<string> defeatedEnemies = new List<string>();
}
