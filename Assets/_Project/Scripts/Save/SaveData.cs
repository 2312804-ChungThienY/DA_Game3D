using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 1;
    public string checkpointId = "cp_trai";
    public bool hasLantern;
    public bool campSealSolved;
    public List<string> collectedRelics = new List<string>();
}
