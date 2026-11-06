# Nguồn tri thức Chương 2

Đối chiếu ngày **05/11/2026** cho lượt3. Chỉ áp dụng cho năm `LR_c2_lore_p*` và bảy `QZ_c2_qz_*` trong `Assets/_Project/Data/Knowledge`. `verified` xác nhận nội dung đã đối chiếu ở mốc này; không thay thế giấy phép asset hoặc chứng nhận mọi nội dung toàn game.

| Nguồn trực tiếp | Asset dùng nguồn | Điểm đối chiếu |
| --- | --- | --- |
| [Oxalis — Sơn Đoòng](https://oxalisadventure.com/cave/son-doong-cave/) | Lore P1, Quiz P1 | Hố sụt, ánh sáng và vùng cây trong hang |
| [Nguyễn Văn Đoàn — Nét đặc sắc của nhóm Bảo vật quốc gia văn hóa Đông Sơn, tr.50](https://baotanglichsu.vn/DataFiles/2021/10/News/Ti%E1%BA%BFng%20Vi%E1%BB%87t/12.10.2021/Thong%20bao%20khoa%20hoc/Net%20dac%20sac%20cua%20nhom%20Bao%20vat%20Quoc%20gia...%20Nguyen%20Van%20Doan.pdf) | Lore P2, Quiz P2, Quiz LT1 | Họa tiết trung tâm trống Ngọc Lũ; cách diễn giải mặt trời |
| [Bảo tàng Lịch sử Quốc gia — Trống đồng Ngọc Lũ, 2013](https://baotanglichsu.vn/vi/Articles/1001/13602/gioi-thieu-bao-vat-quoc-gia-1-trong-djong-ngoc-lu-hien-vat-tieu-bieu-nhat-cua-nen-van-hoa-djong-son.html) | Lore P3/P4, Quiz P3/P4/LT2 | Nhà sàn, cảnh sinh hoạt, bối cảnh Đông Sơn/Lạc Việt |
| [Bảo tàng Lịch sử Quốc gia — Trống đồng Ngọc Lũ, 2017](https://baotanglichsu.vn/VI/Articles/1001/19707/bao-vat-quoc-gia-viet-nam-trong-djong-ngoc-lu.html) | Lore P5, Quiz LT3 | Tên hình tượng chim trong mô tả của bảo tàng |

Các đoạn trong game là lời diễn đạt ngắn, không sao chép bài viết. Phần lịch sử giới hạn vào hiện vật được nêu; không khẳng định mọi trống đồng đều có cùng hoa văn. Lore P5/Quiz LT3 hỏi về tên hình tượng trang trí, không dùng làm bằng chứng xác định loài chim hiện đại. Linh Thần Chim Lạc, nhà sàn đá treo, dấu lông và chuông mở cột gió là thiết kế hư cấu của game.

ID, relatedLore và thứ tự đáp án đúng giữ nguyên: P1/P2/P3/P4 đều chỉ số0; LT1=1, LT2=2, LT3=3. Câu hỏi có thể trả lời từ bia liên quan; không bắt buộc tìm giấy gợi ý trước khi tự giải chuông.

`Chapter02KnowledgeSetup.ValidateContent` kiểm năm bia/bảy câu có nội dung, nguồn, nhãn verified, bốn lựa chọn riêng biệt và liên kết bia. `Chapter02KnowledgeBuildGate` chặn build có scene03 nếu các dữ liệu C2 này không đạt. Cổng này không tự truy cập mạng khi build và chưa kiểm chứng nội dung C1/C3–C5. Bản Windows C1 cũ chưa chứa lượt sửa C2 này.
