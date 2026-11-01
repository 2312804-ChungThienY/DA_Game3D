#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public static class ChapterObjectiveChecks
{
    [MenuItem("Son Doong/Tests/Check chapter objectives")]
    public static void Run()
    {
        // All 128 combinations, including valid backtracking and missing rewards.
        for (int mask = 0; mask < 128; mask++)
        {
            bool[] flags = new bool[7];
            for (int i = 0; i < 7; i++) flags[i] = (mask & (1 << i)) != 0;
            int expected = 7;
            if (!flags[6])
            {
                expected = 6;
                for (int i = 0; i < 6; i++) if (!flags[i]) { expected = i; break; }
            }
            var actual = ChapterObjectiveRules.Evaluate(flags[0], flags[1], flags[2],
                flags[3], flags[4], flags[5], flags[6]);
            if ((int)actual != expected || string.IsNullOrWhiteSpace(ChapterObjectiveRules.Description(actual)))
                throw new InvalidOperationException("Objective contract failed at mask " + mask);
        }
        // Complete -> new game must be recomputed, not remain complete.
        ChapterObjectiveRules.Evaluate(true, true, true, true, true, true, true);
        if (ChapterObjectiveRules.Evaluate(false, false, false, false, false, false, false) != ChapterObjective.FindFragmentA)
            throw new InvalidOperationException("New game retained old objective");
        Debug.Log("PASS: 128 objective combinations and completed -> new game reset.");
    }
}
#endif
