# C2 lượt2 — tuyến cột gió P1–P3

Ngày05/11/2026. Người dùng giao “thực hiện tiếp” sau lượt1. Phạm vi: bốn cột gió đầu, chỗ nhảy/đáp, ba lối nối phòng, camera, checkpoint giữa và rơi/hồi sinh. Giữ thứ tự phòng, bệ/chuông/reward/ID/ref/schema4; không làm C3 hoặc thay An. Chưa thay art toàn chương của lượt7.

## Tuyến kiểm tra

| Đoạn | Cột gió | Chỗ đáp/đường nối |
| --- | --- | --- |
| Đáy P1→gờ5m | WindColumn_P1_Tut (5,0,3.5), hover6 | Gờ(5,5,6.2), bệ1; cửa bắc x3.5..6.5, qua z8.5→11 |
| Sàn P2→bệ A9m | WindColumn_P2_1 (7.6,5,12.6), hover10 | Bệ A(4.5,9,13.6) |
| Bệ A→bệ B13m | WindColumn_P2_2 (5.4,9,16.2), hover14 | Bệ B(8.6,13,16.6), bệ2; cửa cao x7.75..10.75 qua z22→24 |
| P3→gờ18m | WindColumn_P3_Final (5,13,32.8), hover19 | Chỉ bật sau chuông2–3–1; gờ(5.6,18,35.6), bệ3; cửa bắc tới(5,18,38.6) |

Độ cao hover đã có cao hơn mặt bệ1m. Bệ A thực tế rộng5×5m, bệ B4×4m; không ép về kích thước spec nếu bản hiện tại đã được chỉnh. [Ảnh chụp cấu hình trước sửa](reports/C2_Traversal_Before_2026-11-05.json) giữ bounds vách/bệ/cột/cửa hiện hữu.

## Điều chỉnh trong lượt này

Hình cột gió cũ tuy alpha.28 nhưng hiển thị như khối xanh đặc trong Game View, che chỗ đáp. Tạo vật liệu URP riêng `Art/Chapter02Traversal/Wind_Transparent.mat`, SurfaceTransparent/blend alpha/ZWrite0, alpha.12; áp vào visual cột gió hiện có, không đổi trigger hoặc sửa `M_BO_Mechanic` dùng chung. Bảy cột dùng cùng vật liệu để nhất quán; cơ chế/công cụ P4/P5 giữ nguyên.

Camera C2 `PlayerFollowCamera` trước đây chưa có `CaveCameraCollisionGuard`. Gắn extension Cinemachine3 đã dùng trong C1; guard sửa CameraState cuối pipeline, không tạo camera motor thứ hai. Không thay model/rig An, không tạo asset trả phí.

Thêm chín dấu đá thấp không collider/layer2 tại các chỗ đáp và lối vòng nhà sàn, nhóm `World_Geometry/ART_C2_Traversal`; material riêng `Landing_Stone.mat`. Giữ các vách/cửa/rào hiện hữu vì các cửa và lối trống đã đủ khoảng đứng; không tạo cầu tắt, không đổi độ cao bệ/cột. Motor/wind runtime và NavMesh C2 giữ nguyên so với backup.

Backup scene/meta/NavMesh C2, WindColumn và PlayerMotor trước lượt ở đường dẫn trong `Logs/C2Traversal-backup.json` dưới `SourceAssets/SceneBackups/2026-11-05_C2Traversal`. Runtime motor/wind chỉ sửa nếu bài thử cho thấy lỗi thực tế; không ghi hoàn thành nhảy chỉ từ vị trí transform/bounds.

## Cách kiểm chứng

`Chapter02TraversalSetup` nhận request `Library/SonDoongC2Traversal.request.json` với action inspect/apply/validate/test; từ chối chạy khi Play hoặc scene người dùng chưa lưu. `Chapter02TraversalProbe` opt-in `UNITY_EDITOR`, bảo vệ save chính/bak/tmp và trả lại sau mỗi lượt.

Probe dùng bàn phím giả qua Input System/InputReader thật, W theo yaw camera và Space khi hover; PlayerMotor vẫn là nơi duy nhất di chuyển CharacterController. Không đặt vận tốc dọc/teleport để giả lập bốn lượt nhảy. Cột gió, gravity, va chạm, pause và camera pipeline chạy thật. Tương tác quiz/checkpoint/chuông qua API sau kiểm khoảng tay/LOS; AI giao chiến dừng để cô lập đường đi.

Sau khi tới checkpoint giữa bằng tuyến thật, thử chuông/cột cuối/bệ3, cố ý đặt vị trí rơi dưới map làm fixture cho KillZone, Respawn và menu/Continue. Fixture rơi không chứng minh kỹ năng né mép; kiểm snapshot phải mất mảnh3/chuông chưa lưu và giữ hai mảnh đầu/năng lực gió.

Game chủ động chặn Input khi Game View mất focus. Không bỏ guard hoặc dùng API để nhận là đã thử Space. Người dùng xác nhận đưa Unity trước; helper Windows `SourceAssets/Tools/C2FocusGameView.ps1` chỉ kích hoạt/click cửa sổ Unity đang chạy. Harness Pause/Resume thật khi OS mất focus, chờ input hoạt động rồi mới gửi W/Space. Các lần phát triển test dừng vì focus, đi vào tâm collider vật nhặt hoặc fixture thiếu chuỗi C1 được giữ ở Logs; không sửa gameplay để làm yếu validator/khóa input.

## Kết quả cuối

[Editor](reports/C2_Traversal_Editor_2026-11-05.json): **20checks,0failures**. [Play Mode](reports/C2_Traversal_PlayMode_2026-11-05.json): **150checks,0failures,0runtime errors**, gồm40góc camera tại5điểm và **bốn lần nhấn Space thật qua InputReader, tăng độ cao và đáp đúng5/9/13/18m**. Không teleport dọc tuyến leo. Có negative trước nhận gió, pause lift, ba cửa nối, vòng chuông qua lối trống; bệ1–3 được tương tác trong tầm tay/LOS bằng API. State C1 hoàn tất là fixture để SaveService chấp nhận dữ liệu đúng hợp đồng; không nới validator.

Save giữa chương giữ mảnh1/2 và gió. Sau lấy mảnh3 chưa lưu, fixture rơi xuống KillZone gây chết thật; Respawn thay snapshot về mốc giữa, mất mảnh3/chuông/cột cuối chưa lưu. Menu/Continue cũng trở lại cùng nền an toàn. Save cá nhân chính/bak/tmp đã trả byte-for-byte; [SHA/meta](reports/C2_Traversal_Delivery_2026-11-05.json) xác nhận C1 scene/motor/wind/NavMesh C2 không đổi.

[Editor cuối](reports/C2_Traversal_EditorState_2026-11-05.json) dừng Play, không compile, scene03 sạch. Executable Windows chưa dựng lại trong lượt2.

Ảnh Game View: [bệ A và gió trong suốt](images/c2-traversal-platform-a-2026-11-05.png), [miệng lối P3→P4](images/c2-traversal-p3-exit-2026-11-05.png). Đây còn là greybox; art cuối ở lượt7. Chưa nghiệm thu full P4/P5, mọi đường tùy chọn/quay ngược, Windows mới, FPS hoặc cảm giác chơi tay. Lượt3 là phần kế tiếp tại mốc này và hiện đã đạt trong [C2_KNOWLEDGE_WORKLOG](C2_KNOWLEDGE_WORKLOG.md); hàng đợi hiện hành ở PROJECT_STATUS.
