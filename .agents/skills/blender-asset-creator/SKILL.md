---
name: blender-asset-creator
description: Dùng khi được giao tạo hoặc chỉnh một model 3D trong Blender qua Blender MCP, có thể đưa model đó vào Unity nếu được yêu cầu; không dùng cho việc chỉ liệt kê object Blender hay bố trí Scene Unity.
---

# Tạo asset Blender

## Quy trình

1. Đọc `docs/ART_BIBLE.md` và brief của asset; nếu gắn với khu/chương, mở worklog tương ứng qua `docs/SPEC_INDEX.md`. Phong cách hiện hành là Mid Poly Stylized Việt Nam: texture 1K mặc định, 2K cho nhân vật/công trình chính, URP Lit và LOD cho vật thể lớn. Chốt kích thước thực, silhouette, mức độ trang trí và ảnh/tư liệu được phép dùng; ghi rõ phần cách điệu hoặc hư cấu.
2. Dùng Unity MCP `blender_bridge(action="status")`, `scene_info` và `object_info` để đọc scene Blender đang mở. Xác định tên object, collection, đơn vị, trục, pivot và nơi lưu nguồn trước khi sửa. Nếu chỉ được yêu cầu đọc, dừng ở đây.
3. Khi được giao tạo/sửa model, thao tác đúng object qua `blender_bridge(action="run_python")` theo schema hiện có; đặt tên có nghĩa, giữ mesh nguồn và bản cũ, không xóa asset. Tách phần tạo hình với các quyết định chưa duyệt như biểu tượng văn hóa, màu chuẩn hay budget cố định.
4. Kiểm tra lại bằng `scene_info`, `object_info` và `screenshot` nếu tool có. Đánh giá tỷ lệ người chơi, silhouette từ xa, normals, UV, vật liệu và độ đọc gameplay. Nếu có nguồn bên ngoài, lưu thông tin giấy phép và credit; không tự nhận là phục dựng lịch sử chính xác.
5. Chỉ khi người dùng yêu cầu đưa vào Unity, dùng `blender_bridge(action="import_model")` theo tham số/tool thực tế; sau đó kiểm tra asset bằng `manage_asset(action="get_info")` hoặc `search`, prefab/scale/material trong Unity. `import_model` là quy trình nhập model của bridge, không giả định có action export riêng. Giữ file nguồn, `.meta` và GUID.

## Giới hạn và hoàn thành

- Không chạy Python hoặc import model cho yêu cầu chỉ đọc. Không ghi đè model, prefab, scene hay asset nhập sẵn; không thay rig/model An hoặc mua/tải asset khi chưa được giao. Nếu bridge không khả dụng, báo kết quả kiểm tra và chỉ chuẩn bị brief từ dữ liệu đã có.
- Hoàn thành khi object và kích thước được xác minh trong Blender; nếu có nhập Unity, xác minh cả đường dẫn asset, scale, hướng và vật liệu. Báo rõ rig, animation, collision và hiệu năng đã kiểm tra ở mức nào; không coi sự hiện diện của file là bằng chứng các phần đó hoạt động.
