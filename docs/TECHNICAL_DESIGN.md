# Thiết kế kỹ thuật — hợp đồng đang dùng

**Cập nhật 06/11/2026.** Chi tiết API/ID ở [CURRENT_ARCHITECTURE](CURRENT_ARCHITECTURE.md); mức kiểm chứng ở [PROJECT_STATUS](PROJECT_STATUS.md), khoảng thiếu ở [REQUIREMENTS_MATRIX](REQUIREMENTS_MATRIX.md). Đọc các mục này khi sửa hệ tương ứng.

## Đã kiểm từ repository

| Phần | Hiện trạng |
| --- | --- |
| Editor/render | Unity `6000.4.8f1`, URP `17.4.0` trong Graphics/Quality; Cinemachine `3.1.7`, Input System `1.19.0`, AI Navigation `2.0.12` |
| Cấu trúc | `Assets/_Project/Scripts` chia theo hệ; `Prefabs` và `Data` giữ prefab/SO; gameplay C# chủ yếu ở global namespace |
| Vòng đời | `AppRoot` giữ Input/Settings/Session/UI; object `GameSession` trong scene có WorldStateStore, InventoryService, SaveService, reward |
| Input/actor | `PlayerControls.inputactions` có Gameplay/UI, đọc qua InputReader; PlayerMotor là motor của An; Cinemachine 3 |
| State/save | `WorldStateStore` theo StateSet/WorldIds; InventoryService xử lý túi/giao dịch; SaveService native schema 4, migration legacy 1–5 |
| Dữ liệu | ItemDefinition, EnemyDefinition, QuizDefinition, LoreDefinition, puzzle definitions là SO; prefab `PF_*` đặt runtime root/visual |
| Kiểm tra | `Assets/_Project/Tests` có probe Editor/Play Mode; báo cáo trong `docs/reports`. Tồn tại probe không chứng minh lần sửa mới đã qua test |

## Quy tắc khi sửa

- Giữ một nguồn điều khiển vị trí mỗi actor và một đường input. Giao diện gửi lệnh tới service; object trong scene đọc World/Inventory, không tự đọc file save. Tách dữ liệu thiết kế SO, state runtime và DTO lưu.
- Đổi ID, serialized field, SO, prefab hoặc scene cần tìm mọi ref; thay ID save phải có migration/Continue. Bản sao visual không được thay gameplay root. Reward/craft chuẩn bị đầy đủ trước commit; snapshot restore phải thay toàn bộ.
- Với map/nhà/NPC/item mới, trước tiên xác định ownership, collider/layer/LOS/NavMesh, prefab/ID/save và cách kiểm. Không tạo NPC framework hoặc API `ChapterContext/PlayerInventory/save v5` song song với runtime hiện có chỉ vì spec cũ dùng tên ấy.
- Unity MCP: chọn đúng project/scene, đọc editor state và dirty state trước sửa, chờ compile/Console sau code, xác minh hierarchy/ref sau scene. Blender MCP: khảo sát scene/object trước export; chỉ nhập khi có phạm vi art và file nguồn rõ.
- Nêu chính xác mức kiểm thử: đọc tĩnh, Editor, Play Mode, build Windows, chơi tay hay benchmark. Performance dùng build/cấu hình/tuyến/median/P95; hiệu ứng chất lượng thấp không được đổi logic gameplay.

## Khoảng thiếu đã ghi nhận, chưa phải lệnh tự sửa

- Bảo toàn save khi NewGame thất bại, validation/catalog/ID migration, C3–C5 và nghiệm thu người chơi/hiệu năng vẫn cần xử lý theo phạm vi được giao; kiểm mục mới nhất trong STATUS/MATRIX trước khi lập kế hoạch.
- Đề xuất cần quyết định: chuẩn Git LFS cho file nhị phân lớn; quy ước namespace cho mã **mới** nếu muốn thay global namespace, không áp hồi tố lên code hiện có.

