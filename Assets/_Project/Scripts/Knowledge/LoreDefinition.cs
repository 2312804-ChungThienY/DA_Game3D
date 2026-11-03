using UnityEngine;

// Một bia văn tự. verified = false thì chưa được đưa vào bản Release (spec phần 44, KN01).
[CreateAssetMenu(menuName = "Son Doong/Lore Definition", fileName = "LR_New")]
public class LoreDefinition : ScriptableObject
{
    public string loreId;
    public ContentTag tag;
    public string title;
    [TextArea(3, 8)] public string body;
    [TextArea(1, 4)] public string sourceRef;
    public bool verified;
}
