using UnityEngine;

[CreateAssetMenu(menuName = "Son Doong/Sequence Puzzle", fileName = "SQ_New")]
public class SequencePuzzleDefinition : ScriptableObject
{
    public string puzzleId;
    public string[] sequence;
    public float inputDelay = 0.2f;
    public float failDelay = 0.7f;
}
