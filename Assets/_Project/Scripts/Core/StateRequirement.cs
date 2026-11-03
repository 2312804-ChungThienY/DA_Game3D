using System;

// Một yêu cầu tiến trình (cửa, nhiệm vụ). Dùng chung cho mọi chương.
[Serializable]
public class StateRequirement
{
    public StateSet set;
    public string id;
    public string displayName;

    public bool IsMet(WorldStateStore world) => world != null && world.Has(set, id);
}
