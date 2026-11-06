---
name: unity-test-and-debug
description: Dùng khi cần tái hiện lỗi, đọc Console, chạy test Unity hoặc xác minh một bản sửa bằng Unity MCP; không dùng để thiết kế tính năng mới, dựng scene hay tạo asset mỹ thuật.
---

# Kiểm thử và debug Unity

## Quy trình

1. Đọc `docs/PROJECT_STATUS.md` và hợp đồng liên quan trong `docs/CURRENT_ARCHITECTURE.md`; ghi cách tái hiện, kết quả mong đợi và phạm vi kiểm tra. Đọc `mcpforunity://instances`, `mcpforunity://editor/state`, `mcpforunity://custom-tools`; nhận diện scene đang mở, Play Mode, compile và dirty state. Không làm mất trạng thái Editor của người dùng chỉ để chạy test.
2. Lấy baseline bằng `read_console(action="get", types=["error","warning"])`; đối chiếu lỗi cũ với lỗi của nhiệm vụ. Khi cần tìm test có sẵn, xem resource `mcpforunity://tests` nếu được liệt kê, hoặc kiểm tra file test trong project. Chọn ca nhỏ nhất kiểm tra đúng rủi ro: EditMode, PlayMode, thao tác tay hay build.
3. Với test tự động được giao, dùng `run_tests(mode="EditMode"|"PlayMode", test_names=...)` theo schema đang có và theo dõi `job_id` bằng `get_test_job`. Play Mode có thể đổi trạng thái scene; chỉ chạy sau khi đã xử lý dirty state. Không xóa save/fixture hoặc clear Console để làm đẹp kết quả.
4. Nếu phải sửa lỗi, khoanh vùng log/stack trace và code/ref liên quan, sửa tối thiểu, đợi Unity compile xong qua `mcpforunity://editor/state`, rồi đọc Console lại. Chạy lại đúng ca đã lỗi và ca lân cận có chung hợp đồng. Với lỗi hiệu năng, đo bằng `manage_profiler` trên tuyến/cấu hình được nêu, không kết luận FPS từ Editor tĩnh.
5. Báo kết quả gồm tên ca, mode, số pass/fail/skip, lỗi còn lại và trạng thái scene/save. Phân biệt kiểm tra file, Editor, Play Mode, build Windows và chơi tay.

## Giới hạn và hoàn thành

- Không xóa dữ liệu người dùng, thay ID/save schema, reset/clean Git, ghi đè scene hoặc thay package chỉ để xử lý test. Test chưa chạy hay bị chặn phải ghi rõ, không gắn nhãn pass.
- Hoàn thành khi đã tái hiện hoặc lý giải lỗi bằng bằng chứng, bản sửa nếu có đã compile và ca liên quan được chạy lại; nếu không thể chạy, nêu nguyên nhân cụ thể và bước kiểm chứng còn lại. Cập nhật `docs/PROJECT_STATUS.md` sau thay đổi đáng kể.
