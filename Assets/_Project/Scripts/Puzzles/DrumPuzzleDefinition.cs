using UnityEngine;

public enum DrumNote { A, B, C }

[CreateAssetMenu(fileName = "DrumPuzzleDefinition", menuName = "Son Doong/Puzzles/Drum Puzzle")]
public class DrumPuzzleDefinition : ScriptableObject
{
    [SerializeField] private DrumNote[] sequence = { DrumNote.A, DrumNote.C, DrumNote.B };
    [SerializeField, Min(0.2f)] private float inputDelay = 0.2f;
    [SerializeField, Min(0.1f)] private float failureDelay = 0.7f;

    public DrumNote[] Sequence => sequence;
    public float InputDelay => inputDelay;
    public float FailureDelay => failureDelay;
}
