using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 2;
    public string checkpointId = "cp_trai";
    public bool hasLantern;
    public bool campSealSolved;
    public List<string> collectedRelics = new List<string>();
    public List<string> solvedPuzzles = new List<string>();
}
