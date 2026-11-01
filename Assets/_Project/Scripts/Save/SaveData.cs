using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 5;
    public string checkpointId = "cp_trai";
    public bool hasLantern;
    public bool campSealSolved;
    public bool relicRestored;
    public List<InventorySlotData> inventory = InventoryRules.EmptySlots();
    public List<string> collectedPickups = new List<string>();
    public List<string> collectedRelics = new List<string>();
    public List<string> solvedPuzzles = new List<string>();
    public List<string> defeatedEnemies = new List<string>();
}
