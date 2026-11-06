# Hướng dẫn Codex — Sơn Đoòng: Cõi Mộng Kỳ Thần

Tệp này hướng dẫn cách làm việc, không thay đặc tả game hoặc bằng chứng nghiệm thu. Trao đổi với người dùng bằng tiếng Việt. Chỉ mục tài liệu ở [docs/README.md](docs/README.md).

## Phạm vi và nguồn sự thật

- **Đã xác nhận:** game phiêu lưu hành động 3D góc nhìn thứ ba, An khám phá Sơn Đoòng bằng Lồng Đèn Sen; Windows x64, chơi đơn offline. Mục tiêu thiết kế là 5 chương/65–95 phút; C1 và C2 có scene/runtime, C3–C5 chưa triển khai; ưu tiên môi trường C1 trong phạm vi người dùng giao. Phong cách hiện hành là **Mid Poly Stylized Việt Nam**: texture 1K mặc định, 2K cho nhân vật và công trình chính, material URP Lit, LOD cho vật thể lớn. Chi tiết áp dụng ở `docs/ART_BIBLE.md`; chưa có hạn mới.
- Yêu cầu mới nhất của người dùng ưu tiên trước đặc tả chương và §55, rồi §40–54, rồi §1–39 ở điểm chưa bị thay. `spec/Tên_dự_án.md` là tầm nhìn; danh mục kỹ thuật/asset là tham khảo. `docs/history/` chỉ lưu lịch sử. Code/scene cho biết hiện trạng, không tự hủy yêu cầu khi code thiếu.
- Trước việc gameplay/art: đọc `docs/PROJECT_MEMORY.md` và phần mới nhất của `docs/PROJECT_STATUS.md`. Khi đổi API/ID/save, đọc `docs/CURRENT_ARCHITECTURE.md` và hàng liên quan trong `docs/REQUIREMENTS_MATRIX.md`. Dùng `docs/SPEC_INDEX.md` để mở đúng phần `spec/`. Chỉ đọc tài liệu chuyên biệt khi công việc liên quan; khi chỉ sửa tài liệu, đọc nguồn mà tài liệu đó tóm tắt.
- Tra art ở `docs/ART_BIBLE.md`, thiết kế game ở `docs/GAME_DESIGN.md`, runtime ở `docs/TECHNICAL_DESIGN.md`, hàng đợi ở `docs/TASKS.md`. Bốn tệp này là chỉ mục ngắn; quyết định và bằng chứng chi tiết vẫn ở tài liệu gốc.

## Stack và cấu trúc đã kiểm

- `ProjectSettings/ProjectVersion.txt`: Unity **6000.4.8f1**; Graphics/Quality gán **URP 17.4.0**. Cinemachine 3.1.7, Input System 1.19.0, AI Navigation 2.0.12, ProBuilder 6.0.9, Test Framework 1.6.0 theo `Packages/manifest.json`. Không tự đổi Editor theo mốc Unity 6.3 LTS trong spec.
- Mã chính ở `Assets/_Project/Scripts/` chia Core, Player, Interaction, Combat, AI, Puzzles, Inventory, Quests, Save, Knowledge, UI, Presentation, Chapters. Scene/prefab/SO ở `Assets/_Project/Scenes`, `Prefabs`, `Data`; `SourceAssets/` giữ nguồn và bản sao lưu theo lượt. Scene thử ngoài tuyến build không tự được coi là màn chơi chính.
- Mã gameplay hiện dùng **global namespace**; đừng di chuyển hàng loạt sang namespace mới. Theo kiểu của module lân cận: lớp/method/property PascalCase, giữ tên serialized field; `PF_` cho prefab, `ID_`/`ED_`/`QZ_`/`LR_` cho SO tương ứng. Đừng đổi tên serialized field/GUID chỉ để chuẩn hóa cách viết.

## Hợp đồng runtime cần giữ

- `AppRoot` giữ Settings/Input/Session/UI dùng chung; `GameSession` trong scene chứa WorldStateStore, InventoryService, SaveService và reward. Một motor điều khiển vị trí mỗi actor; An dùng PlayerMotor/InputReader và một asset `PlayerControls.inputactions` với map Gameplay/UI. Dùng Cinemachine **3**, không trộn API 2.x.
- SO giữ dữ liệu thiết kế; state runtime đi qua `StateSet/WorldStateStore/WorldIds`, InventoryService và SaveService. Native save schema **4**, đọc legacy 1–5. ID C1 còn `relic_drum_*`; đổi ID phải có migration, ref và kiểm tra Continue. Reward/craft phải nguyên tử, phát sự kiện sau commit, không trao lại khi replay.
- Chỉ sửa phần được giao. C1 hiện chế Bình máu trong túi I, E lửa chỉ lưu; C2 có trạm chế riêng. Giữ HUD C1 mở đầu Vườn Rêu, tiến độ 3 mảnh/2 câu đố và ba thanh Master/Music/SFX. Không tự triển khai C3–C5, sinh tồn, nỏ/móc dây/đu dây/cưỡi chim/bơi. Các lượt C1 đã cho phép hoàn thiện Hắc Ảnh/Hộ vệ/đèn/UI/audio và mở rộng địa hình, **không thay model/rig An**; kiểm scope mới nhất trước khi làm tiếp.

## Scene, asset và MCP

- Đầu lượt: xem `git status`, Scene/Prefab Stage đang mở, Play Mode, dirty state và Console; người dùng có thể đang chỉnh Unity. Không lưu/đóng scene dirty ngoài phạm vi. Trước khi sửa scene/prefab/build settings, ghi nhận đường dẫn, root, ref, ID, collider, NavMesh và checkpoint; sửa tối thiểu rồi kiểm lại.
- Unity MCP: chọn đúng instance; đọc `mcpforunity://editor/state` và resource liên quan trước lệnh; kiểm custom tools/tool groups khi tool thiếu. Sau sửa script chờ compile, đọc Console; sau sửa scene kiểm hierarchy/component/dirty và chỉ lưu đúng scene được giao. Không chạy công cụ dựng lại C1/C2 chỉ để đọc cảnh.
- Blender MCP: dùng `blender_bridge(status/scene_info/object_info)` để khảo sát. Chỉ export/import hoặc chạy Python khi yêu cầu cho phép sửa, đã xác định object/đường dẫn/đơn vị và cách giữ nguồn. Không đồng bộ addon hoặc import model chỉ để thử kết nối.
- Giữ asset với `.meta` và GUID; để Unity quản lý khi di chuyển/đổi tên. Với model nguồn nhập, ưu tiên prefab variant/material hoặc mesh dẫn xuất trong `_Project`; giữ giấy phép/credit. C1 world hiện ở `World_Geometry/ENV_C1_ExpandedWorld`, art cũ inactive; giữ root gameplay/ref/ID khi đổi hình. Bản đồ, nhà, actor, NPC, vật phẩm mới phải theo spec và scope được giao; không coi asset hiện diện là gameplay hoàn chỉnh.

## Kiểm chứng, Git và quyền thao tác

- Phân biệt thiết kế, file có trên đĩa, cấu hình Editor, Play Mode, Windows build, chơi tay và benchmark. Test đúng rủi ro thay đổi; báo số ca, phiên bản, môi trường, lỗi và giới hạn. Với hiệu năng, đo build trên cấu hình/tuyến rõ ràng, frame time trung vị/P95; không tuyên bố 60 FPS chỉ từ ảnh hoặc cảnh đứng yên.
- Git hiện có `.gitignore` bỏ `Library/Temp/Obj/Builds/UserSettings`; Git LFS được cài nhưng chưa có `.gitattributes` hoặc file LFS được theo dõi. Không sửa cache Unity, reset/clean/force push, xóa thay đổi của người dùng, hoặc tạo commit ngoài yêu cầu. Xem diff và giữ file không thuộc việc.
- Các thay đổi có tác động rộng cần người dùng quyết định **nếu chưa được ủy quyền trước đó**: cơ chế/chương/NPC mới, thay An, đổi ID/schema/save, thay Unity/URP/package, nhập asset mới có vấn đề giấy phép/chi phí, xóa/thay thế hàng loạt scene/prefab/nguồn, công bố build. Công việc đọc, sửa nhỏ có thể hoàn tác và kiểm thử trong phạm vi đã giao không cần hỏi lại.
- **Hoàn thành:** thay đổi đúng phạm vi; asset/meta/ref/ID vẫn hợp lệ; compile/Console sạch đối với thay đổi mã; test phù hợp thực sự đã chạy; ghi rõ phần chưa kiểm. Sau thay đổi đáng kể, cập nhật `docs/PROJECT_STATUS.md` với ngày, file, bằng chứng và việc còn lại. Chỉ cập nhật `PROJECT_MEMORY` khi có quyết định mới được người dùng chấp thuận.
