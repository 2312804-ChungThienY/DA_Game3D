# Công việc và mức bằng chứng

**Ảnh chụp trạng thái 06/11/2026.** Danh sách ngắn để định hướng; [PROJECT_STATUS](PROJECT_STATUS.md) là nhật ký mới nhất, [REQUIREMENTS_MATRIX](REQUIREMENTS_MATRIX.md) là bảng khoảng thiếu. Mọi việc dưới đây chỉ triển khai khi có yêu cầu tương ứng.

## Đã có triển khai và bằng chứng theo phạm vi báo cáo

- C1: môi trường năm khu/bốn đường nối, puzzle/đèn/quái/HUD/audio/save và ba trống Đông Sơn mới trong scene chính; xem [C1 mở rộng](CHAPTER01_EXPANDED.md), [hoàn thiện C1](CHAPTER01_COMPLETION_WORKLOG.md), [trống mới](DONGSON_DRUM_WORKLOG.md). Các kiểm tra Editor/Play Mode và build có phạm vi riêng; chưa là nghiệm thu toàn game.
- C2: các lượt cơ chế 0–6, art/âm thanh/HUD và development Windows lượt 7 có báo cáo; xem [kế hoạch C2](CHAPTER02_MECHANICS_PLAN.md) và [art C2](C2_ENVIRONMENT_WORKLOG.md). Không suy đã cân bằng/trình bày cuối.
- Bộ hướng dẫn Codex: `AGENTS.md` và bốn tài liệu định hướng trong `docs/` được rà soát trong lượt này; chỉ thay Markdown.
- Quyết định mỹ thuật 06/11: ghi nhận **Mid Poly Stylized Việt Nam**, texture 1K mặc định/2K cho nhân vật và công trình chính, URP Lit, LOD cho vật thể lớn; chưa chuyển scene/model/material theo chuẩn mới.

## Đang cần kiểm/duyệt khi được giao

Đây là các mục chờ kiểm hoặc phê duyệt, không phải xác nhận rằng Codex đang thi công gameplay. Git dirty có thể là công việc người dùng đang mở trong Unity.

- Chơi tay liên tục C1→C2 trên bản hiện hành; cân trận Chim Lạc 200 HP, nhịp người mới, art/rig chim, nghe cân âm và kiểm bản Windows trên máy thứ hai.
- Khi được giao đổi art: đối chiếu từng khu C1/C2 với hướng Mid Poly Stylized Việt Nam, chuẩn 1K/2K, URP Lit, LOD vật thể lớn và concept/ảnh tham chiếu được người dùng duyệt; báo rõ phần nào đã chuyển, phần nào vẫn là bản cũ.
- Đo hiệu năng trên tuyến và phần cứng tham chiếu, ghi median/P95; số đo góc đứng yên C2 không đại diện toàn tuyến.
- Kiểm lại các gap save/ID/legacy asset còn mở theo STATUS/MATRIX trước thay đổi runtime; một số dòng lịch sử trong STATUS đã được lượt mới khắc phục, không dùng riêng bảng cũ làm kết luận.

## Chưa triển khai hoặc chưa được giao

- Scene/runtime C3–C5 và ending toàn game; không khởi động từ backlog khi chưa có yêu cầu.
- Hệ NPC gameplay/hội thoại; chưa có bằng chứng module NPC trong mã chính.
- Bảng màu số, ngân sách polygon, khoảng chuyển LOD, cấu hình máy đích và chính sách Git LFS; chưa chốt. Chuẩn texture 1K/2K, URP Lit và yêu cầu LOD đã được người dùng quyết định.

