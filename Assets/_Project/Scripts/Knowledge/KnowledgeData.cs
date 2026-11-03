using UnityEngine;

// Nhãn nội dung tri thức (spec phần 44). Truyền thuyết luôn ghi rõ là truyền thuyết.
public enum ContentTag { LichSu, TruyenThuyet, TuNhien }

public static class ContentTagText
{
    public static string Label(ContentTag tag)
    {
        switch (tag)
        {
            case ContentTag.LichSu: return "Lịch sử";
            case ContentTag.TruyenThuyet: return "Truyền thuyết";
            default: return "Tự nhiên";
        }
    }
}
