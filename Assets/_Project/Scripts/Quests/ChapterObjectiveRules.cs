public enum ChapterObjective
{
    FindFragmentA, SolveDrums, CollectFragmentB, SolveMirrors,
    CollectFragmentC, DefeatGuardian, RestoreAltar, Complete
}

// Stateless: loading an earlier checkpoint naturally returns an earlier objective.
public static class ChapterObjectiveRules
{
    public static ChapterObjective Evaluate(bool fragmentA, bool drumsSolved,
        bool fragmentB, bool mirrorsSolved, bool fragmentC, bool guardianDefeated, bool restored)
    {
        if (restored) return ChapterObjective.Complete;
        if (!fragmentA) return ChapterObjective.FindFragmentA;
        if (!drumsSolved) return ChapterObjective.SolveDrums;
        if (!fragmentB) return ChapterObjective.CollectFragmentB;
        if (!mirrorsSolved) return ChapterObjective.SolveMirrors;
        if (!fragmentC) return ChapterObjective.CollectFragmentC;
        if (!guardianDefeated) return ChapterObjective.DefeatGuardian;
        return ChapterObjective.RestoreAltar;
    }

    public static string Description(ChapterObjective objective)
    {
        switch (objective)
        {
            case ChapterObjective.FindFragmentA:
                return "Đến Vườn Rêu. Dùng đèn giải phong ấn, rồi nhặt mảnh trống A.";
            case ChapterObjective.SolveDrums:
                return "Đến Điện Trống. Đọc ký hiệu và gõ trống theo lời gợi ý.";
            case ChapterObjective.CollectFragmentB:
                return "Câu đố trống đã giải. Nhặt mảnh trống B tại bệ hiện vật.";
            case ChapterObjective.SolveMirrors:
                return "Đến Hành Lang Quang. Xoay gương để đưa ánh sáng đến ngọc nhận.";
            case ChapterObjective.CollectFragmentC:
                return "Đường sáng đã nối. Nhặt mảnh trống C tại bệ hiện vật.";
            case ChapterObjective.DefeatGuardian:
                return "Đến Đàn Kim Quy. Đánh bại Hộ vệ Đá để tiếp cận bệ trống.";
            case ChapterObjective.RestoreAltar:
                return "Tương tác bệ trống tại Đàn Kim Quy để phục hồi cổ vật.";
            default:
                return "Đã thức tỉnh Kim Quy và hoàn thành chương.";
        }
    }
}
