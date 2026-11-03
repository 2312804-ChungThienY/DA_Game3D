using UnityEngine;

// Câu hỏi 4 lựa chọn (spec phần 44). Luôn trả lời được từ bia relatedLoreId.
[CreateAssetMenu(menuName = "Son Doong/Quiz Definition", fileName = "QZ_New")]
public class QuizDefinition : ScriptableObject
{
    public string quizId;
    public ContentTag tag;
    [TextArea(2, 4)] public string question;
    public string[] options = new string[4];
    [Range(0, 3)] public int correctIndex;
    [TextArea(2, 5)] public string explanation;
    public LoreDefinition relatedLore;
    [TextArea(1, 4)] public string sourceRef;
    public bool verified;

    public bool IsWellFormed => !string.IsNullOrEmpty(quizId) && options != null && options.Length == 4 &&
                                correctIndex >= 0 && correctIndex < 4;
}
