using UnityEngine;

// Thẻ ảnh tĩnh + lời dẫn (spec chapters/00 mục 4). Blockout để trống ảnh.
[CreateAssetMenu(menuName = "Son Doong/Story Card", fileName = "SC_NewCard")]
public class StoryCard : ScriptableObject
{
    public string title;
    [TextArea(4, 14)] public string body;
    public Sprite image;
}
