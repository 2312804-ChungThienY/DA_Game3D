---
name: unity-scene-builder
description: Dùng khi được giao tạo hoặc chỉnh bố cục, GameObject, collider hay prefab trong một Unity Scene bằng Unity MCP; không dùng cho yêu cầu chỉ kiểm thử, tạo model Blender hoặc thiết kế cả khu thế giới.
---

# Dựng Unity Scene

## Quy trình

1. Đọc `AGENTS.md` và trạng thái liên quan trong `docs/PROJECT_STATUS.md`. Qua Unity MCP, xem `mcpforunity://instances`, `mcpforunity://editor/state`, `mcpforunity://custom-tools`; chọn đúng instance nếu có nhiều Editor. Đọc `manage_scene(action="get_active")` và `manage_scene(action="get_loaded_scenes")`. Ghi lại scene đang dirty và Prefab Stage trước khi sửa.
2. Xác định chính xác scene đích, object được phép tạo/sửa và yêu cầu kích thước, tọa độ, layer, collider, camera. Với scene gameplay, đọc chương tương ứng qua `docs/SPEC_INDEX.md` và kiểm tra root/ref/ID/checkpoint hiện có; với scene thử nghiệm, đừng suy ra đó là màn chơi chính.
3. Đọc hierarchy bằng `manage_scene(action="get_hierarchy")` theo trang; dùng `find_gameobjects` và thông tin component để phân biệt object trùng tên. Chọn prefab/visual root sẵn có nếu phù hợp. Không chạy script dựng lại scene chỉ để khảo sát.
4. Dùng `manage_scene`, `manage_gameobject`, `manage_components` hoặc `manage_prefabs` đúng action mà tool hiện hỗ trợ. Tạo/chỉnh từng nhóm nhỏ; sau mỗi nhóm kiểm tra tên, parent, transform, component, collider và tham chiếu. Nếu tạo scene mới, bảo đảm có Camera và Directional Light theo nhu cầu scene.
5. Kiểm tra lại hierarchy và các component đích; dùng `manage_scene(action="validate")` nếu khả dụng. Xem Console qua `read_console(action="get")` khi thay đổi có thể gây lỗi. Chỉ lưu scene đích bằng `manage_scene(action="save")` sau khi xác nhận đường dẫn và dirty state; kiểm tra scene khác không bị lưu hoặc sửa ngoài phạm vi.

## Giới hạn và hoàn thành

- Quyền chỉnh scene được giao không bao gồm xóa/ghi đè scene, prefab hoặc asset hiện có; không đổi package, render pipeline, runtime ID/save schema hay model An khi chưa có ủy quyền cụ thể. Giữ `.meta`/GUID, ref và các thay đổi người dùng đang làm. Nếu scene đích dirty với thay đổi không thể phân biệt, dừng phần ghi và báo tình trạng.
- Hoàn thành khi object đúng vị trí/kích thước, collider và parent phù hợp, ref gameplay còn hợp lệ, đúng scene đã lưu và bằng chứng từ hierarchy/component/Console được báo rõ. Ghi những phần chưa kiểm tra bằng Play Mode hoặc build; không gọi đó là đã kiểm thử.
