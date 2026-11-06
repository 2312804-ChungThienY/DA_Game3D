# Art Bible — trạng thái và nguyên tắc mỹ thuật

**Cập nhật 06/11/2026.** Tệp này là chỉ mục định hướng, không thay ảnh tham chiếu hoặc hồ sơ asset. Các chi tiết theo khu ở [C1 mở rộng](CHAPTER01_EXPANDED.md), [art C2](C2_ENVIRONMENT_WORKLOG.md), [trống Đông Sơn](DONGSON_DRUM_WORKLOG.md) và [nguồn C2](CHAPTER02_ASSET_CREDITS.md). Nguồn thiết kế trong [SPEC_INDEX](SPEC_INDEX.md).

## Đã được xác nhận

- **Quyết định mới nhất 06/11:** phong cách hiện hành là **Mid Poly Stylized Việt Nam**. Texture **1K mặc định**, **2K cho nhân vật và công trình chính**; dùng **URP Lit** cho material và tạo **LOD cho vật thể lớn**. Đây là chuẩn cho công việc art mới hoặc phần được giao chỉnh; không tự chuyển đổi toàn bộ asset cũ. Cảnh quan, kiến trúc, hoa văn và ánh sáng có thể lấy cảm hứng từ tư liệu Việt Nam phù hợp từng khu, được cách điệu để người chơi đọc rõ hình và lối đi. Bảng màu và concept cuối vẫn chưa được duyệt.
- C1 có năm khu theo tuyến Trại Giếng Trời → Vườn Rêu → Điện Trống → Hành Lang Quang → Đàn Kim Quy. Trại/giếng trời dùng ánh ấm và cây xanh rêu; hang đá xám lạnh, Điện Trống nhấn đồng, Hành Lang Quang có tinh thể lam/tím. Đây là định hướng định tính từ bản dựng và ảnh hiện có, chưa phải bảng màu số đã duyệt.
- C2 có năm khu với vách đá/cây, nhà sàn mái cong, chuông, cầu và đỉnh tổ Chim Lạc. Đây là bản dựng kỹ thuật; hình thái chim, animation và cân màu/âm cần người dùng xem trực tiếp.
- Dự án dùng **URP**, sương mỏng/particle và đèn cục bộ; phải nhìn rõ lối đi, tia puzzle, prompt và báo đòn ở chất lượng thấp/vừa. Ba trống gameplay C1 dùng FBX ndnguyen3d, texture gốc 2K, hoa văn không bị bảng ký hiệu che; nguồn/giấy phép ở worklog.

## Quy tắc tác nghiệp

- Tham chiếu ảnh và bản dựng đúng **chương/khu/phiên bản**. Ưu tiên art mới ở `World_Geometry/ENV_C1_ExpandedWorld`; các nhóm C1 cũ inactive là lịch sử. Không chạy script dựng lại để “đọc” scene.
- Khi dựng bản đồ/nhà/đạo cụ, giữ gameplay root, ID, ref, checkpoint, collider và đường NavMesh; mesh trang trí đặt dưới visual root khi thích hợp. Đo chân vật bám nền, vách không hở sáng, cửa/cầu đủ khe, camera nhìn thấy tín hiệu tương tác. Không thêm collider vào lá/cỏ hoặc thay tuyến chơi theo cảm tính.
- Model từ Blender/import giữ bản nguồn và license; kiểm đơn vị, hướng trục, pivot, scale, UV, normals trước khi đưa vào prefab. Theo chuẩn texture 1K/2K đúng loại asset, dùng material **URP Lit**, chuẩn bị LOD cho vật thể lớn và kiểm chuyển mức từ góc camera chơi. Ưu tiên prefab variant/bản dẫn xuất trong `Assets/_Project/Art`; không sửa trực tiếp file nguồn nhập nếu cấu hình instance đáp ứng được.
- Thiết kế hình lấy cảm hứng văn hóa phải ghi rõ phần hư cấu. Không gọi hoa văn, tượng, nhà hoặc âm thanh tự dựng là phục dựng khảo cổ/xác thực nếu chưa có nguồn. Ghi công riêng cho asset CC BY và kiểm hồ sơ giấy phép trước phát hành.
- Đánh giá art bằng góc camera người chơi, ảnh Editor, Play Mode và build Windows theo mức bằng chứng thực tế. Đo hiệu năng trước khi giảm chất lượng model/texture; chuẩn 1K/2K và yêu cầu LOD đã chốt, còn số polygon, ngưỡng chuyển LOD và draw call mục tiêu chưa có con số chung.
- Bản dựng C1/C2 và ảnh trong worklog phản ánh **phiên bản đã làm**, chưa chứng minh toàn bộ scene đã chuyển sang định hướng Stylized Việt Nam. Khi sửa art theo hướng mới, đối chiếu từng khu với ảnh/tư liệu được duyệt rồi kiểm lại khả năng đọc gameplay.

## Chưa chốt — cần người dùng quyết định khi làm phần liên quan

- Bảng màu số, biến thể vật liệu, ngân sách polygon theo loại asset, khoảng chuyển LOD và cấu hình máy đích. Chuẩn texture 1K/2K là điểm xuất phát đã chốt; ngoại lệ cho asset đặc thù cần nêu lý do và đo/duyệt theo công việc cụ thể.
- Bộ ảnh/concept cuối cho nhân vật mới, NPC, nhà cửa ngoài C2 và các chương C3–C5; tiêu chí duyệt mỹ thuật/animation cuối.

