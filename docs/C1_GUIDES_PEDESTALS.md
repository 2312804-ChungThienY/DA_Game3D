# Giấy hướng dẫn và bệ chương1 —05/11/2026

Theo yêu cầu người dùng: thêm giới thiệu game/hướng dẫn chơi ở Trại Giếng Trời, hướng dẫn trước cổng Kim Quy; sửa tấm vuông thừa, vật phẩm lệch bệ và bệ đèn biến mất. Không thay model An hoặc các cơ chế choáng đã chốt.

## Nội dung và object

Ba giấy mới ở `World_Geometry/ART_C1_Revision/Camp_And_KimQuy_Guides`, nhặt bằng E, mở Story và đọc lại J. Giấy tùy chọn, không bổ sung điều kiện mở cổng.

| Object | Vị trí bệ | ID Flags | Nội dung |
| --- | --- | --- | --- |
| Guide_Camp_Introduction | (2.6,0,-3) | c1_guide_camp_intro | Vai An, bối cảnh hư cấu, tuyến năm khu C1,3mảnh/2câu đố |
| Guide_Camp_Controls | (2.6,0,0.5) | c1_guide_camp_controls | WASD/chuột/Shift/Space/Ctrl/E/F/RMB/LMB/con lăn/I/Q/J/Esc; chế và lưu |
| Guide_Before_KimQuy | (4.2,0,262.5) | c1_guide_kim_quy | Điều kiện mở cổng, nhớ lấy mảnh thưởng, checkpoint/bình máu, tránh đòn Hộ vệ, phục dựng |

Hai giấy trống/gương và ID cũ giữ nguyên. `PuzzleCluePickup.AllClueIds` dùng chung cho Journal; Known Flags thêm3ID, schema4 giữ nguyên. Story mở lại ở đầu trang sau khi thay nội dung. Nút/phím được đối chiếu `PlayerControls.inputactions` và runtime, không đưa phím debug F9 vào hướng dẫn.

## Sửa bệ

Co Fit không căn lại origin làm mesh A24 lệch khoảng0,3m so với object/vật phẩm; `Top_Bronze_Rim` là tấm vuông bổ sung nằm lệch. Đã gỡ các tấm này, căn15mesh A24 mỗi scene về đúng tâm/chạm nền. Dùng vertex bounds qua transform để xử lý cả reward chưa active, tránh bounds cache cũ của renderer.

Ba mảnh Trống Đồng, tinh thể checkpoint và giấy được căn tâm/mặt bệ; collider tương tác khớp phần hình, root pickup/ID/ref được giữ. Đã thêm model A24 cho bệ reset trống vốn bị tắt phần hình.

`Gameplay/Camp_Interactables/LanternPedestal` trước chỉ còn collider vì renderer bị tắt. Nay giữ collider và có con `VIS_LanternPedestal_A24`, tách khỏi `PF_LanternPickup`. Chỉ đèn biến mất khi nhận; bệ vẫn còn sau khi nhặt/Continue. Factory Pedestal bỏ tấm vuông mới và Width có bước căn lại sau co.

Source FBX/OBJ không sửa. Asset dùng lại A24/material hiện có, không cần gói tải thêm. Backup Main/Preview/meta và runtime liên quan ở `SourceAssets/SceneBackups/2026-11-05_GuidesPedestals`, gồm backup Apply riêng có timestamp.

## Bằng chứng

- [Editor](reports/C1_GuidesPedestals_Editor_2026-11-05.json): Main/Preview,5giấy unique,15A24 căn đúng tâm/nền mỗi scene, ba mảnh cách mặt bệ2,5cm, bệ đèn độc lập, các chỗ đứng đọc giấy có nền.
- [Play Mode](reports/C1_GuidesPedestals_PlayMode_2026-11-05.json):238checks/0failures/0errors; mục tiêu/menus/craft, ray chọn đèn và cả5giấy, Story/Journal/restore, sao vẫn hiện/thân quái không đổi màu, kết chương/lưu/Continue/hồi sinh. Kiểm bệ đèn trước/sau nhặt và sau Continue khi đã sở hữu đèn.
- [Traversal](reports/C1_GuidesPedestals_Traversal_2026-11-05.json):542checks/0failures/0errors, bốn đường nối, hai puzzle thật,3mảnh/cổng/checkpoint/Continue/rơi/hồi sinh. AI dừng và cờ thắng boss là fixture để cô lập địa hình/save.
- [Material/NavMesh](reports/C1_GuidesPedestals_MaterialsNavMesh_2026-11-05.json), [meta hai scene giữ SHA](reports/C1_GuidesPedestals_Metadata_2026-11-05.json).

Unity mất focus sau khi đổi bố cục Play; bài238 kiểm bằng API/physics ray thay E/RMB ở chỗ không có focus, không bypass guard input. Boss death khi thiếu focus dùng EnemyHealth API; không phải chơi tay hoặc đo cân bằng. Save chính/bak/tmp và thiết lập gốc được trả lại. Chưa nghiệm thu chơi tay Windows hoặc FPS.

[Kiểm tra tiếp cận cuối](reports/C1_GuidesPedestals_Access_2026-11-05.json) xác nhận mỗi giấy có chỗ đứng đủ khoảng trống capsule, nền đi được và tia tương tác không bị che ở cả Main/Preview. [Build Windows](reports/C1_GuidesPedestals_Build_2026-11-05.json) thành công, 0 errors và485 shader warnings AI Inference/Sentis; [BuildReport gốc](reports/C1_GuidesPedestals_NativeBuildReport_2026-11-05.json). Bản chơi `Builds/Chapter01Revision/Windows/SonDoong.exe` cùng Data/DLL/CREDITS. [Bàn giao](reports/C1_GuidesPedestals_Delivery_2026-11-05.json) ghi SHA và xác nhận save/credit; [Editor cuối](reports/C1_GuidesPedestals_EditorState_2026-11-05.json) mở Main sạch, dừng Play.

## Ảnh

- [Bệ đèn trước khi nhận](images/c1-revision-native-lantern-pedestal-before-pickup-2026-11-05.png)
- [Bệ còn nguyên sau khi nhận](images/c1-revision-native-lantern-pedestal-after-pickup-2026-11-05.png)
- [Giấy giới thiệu](images/c1-revision-c1_guide_camp_intro-2026-11-05.png)
- [Giấy điều khiển](images/c1-revision-c1_guide_camp_controls-2026-11-05.png)
- [Giấy trước cổng Kim Quy](images/c1-revision-c1_guide_kim_quy-2026-11-05.png)
