# Điều chỉnh cơ chế An — 04/11/2026

Người dùng yêu cầu làm mượt toàn bộ cơ chế hiện có, chọn **nhanh, gọn, ít quán tính**. Giữ tốc độ thiết kế, luật C1 chưa lộn trước movement_kit, ID gameplay và schema save. Model/clip A01 và clip muscle blockout được dùng lại; không tạo nhân vật/quái mới.

## Thay đổi

- **Di chuyển:** tăng/giảm tốc 30/45 m/s², xoay tối đa .06 s; đi/chạy/Focus vẫn 3.5/5.5/2 m/s. Nhảy vẫn 1.2 m/gravity −20, đệm nhảy .15 s, coyote .12 s; không chờ cooldown sau tiếp đất. SphereCast xét normal tránh coi vách là nền, chạm trần ngắt vận tốc lên, chia bước dịch chuyển lớn qua cùng PlayerMotor. Chạy cạn thể lực phải nhả Shift để chạy lại, tránh nhịp chạy/hồi lặp. Nhảy khỏi hover tạm bỏ nhận lift .25 s.
- **Ưu tiên hành động:** PlayerActionState dùng chung cho Motor/Combat/Dodge/đèn/E/Q. Hit reaction .18 s hủy đòn/lộn; E/Q không tranh hành động. Teleport/Continue hủy trạng thái tạm, trả capsule và pose, không tạo lunge giả khi lộn bị hủy.
- **Input:** InputReader chặn giữ nút qua đổi map/focus, xóa bộ đệm theo Revision, chặn delta chuột trong chuyển tiếp khóa con trỏ. Giữ đường UI bật lẻ I/J hiện có.
- **Dao/lộn:** đệm nối đòn .16 s, hướng chém được chốt đầu đòn, active đúng .15–.30 s; LOS Default+Environment và một hit cho mỗi IDamageable/đòn. Lộn đọc hướng phím mới trong cùng frame, Shift chỉ tăng khoảng cách khi có hướng di chuyển; giữ điều kiện mở khóa, stamina, perfect dodge và lunge.
- **Camera:** PlayerCameraController sở hữu pivot cả thường/lock-on. Khóa có điểm dự phòng cao 1.2 m, không ngắm chân; bỏ tranh góc với chuột; marker theo camera trước Canvas render. Giữ Cinemachine 3 và cấu hình tránh vật cản hiện có.
- **Animation/âm thanh:** cache Animator parameters, giữ local bind transform, đèn nhường roll/death; death dùng unscaled visual time nhưng actor vẫn bị khóa. Reset pose khi teleport. Fly orphan được sửa bằng InAir trong controller dự án, không đổi GUID/asset A01. OnFootstep/OnLand dùng một AudioSource, 10+1 clip A01 và SFX mixer, có debounce/weight guard.
- **Quét tương tác/Focus:** prompt chỉ ghi khi thay đổi; Focus dùng buffer NonAlloc có thể tăng, giữ dedup/LOS/nón hiện có.

## Bằng chứng

[Play Mode JSON](CharacterPolish_PlayMode_2026-11-04.json): **76 checks, 0 failures, 0 errors**. Probe opt-in từ 00_Boot dùng virtual Keyboard/Mouse qua Input System hiện hành và API thật, không thêm một bộ điều khiển runtime thay thế.

- Bước thời gian mô phỏng 30/60/120 Hz: đi, chạy, dừng nhanh, chéo không tăng tốc, stamina, nhảy/tiếp đất. Đỉnh nhảy đo 1.218/1.200/1.220 m.
- Coyote, bấm nhảy trước tiếp đất, trần/vách, sensitivity và hai giới hạn pitch.
- Dao dedup/tường/nối đòn, bị đánh hủy đòn; C1 không lộn; lộn hướng bấm cùng frame, Shift, bất tử/perfect/lunge, hủy khi teleport; chạy cạn không giật nhịp hồi.
- Pause/resume với nút giữ; lift→hover→nhảy thoát; lock mục tiêu/bám/ngắt khi che.
- Focus tiêu năng lượng/đi chậm/nhường đánh, >64 collider vẫn chỉ cộng một target; E và E lúc đánh.
- Thuốc/cooldown, craft và craft lúc đánh; lưu cp_trai, menu/Continue an toàn. Spawn y=.20 hạ xuống nền y≈.02 trước khi xét grounded; không yêu cầu grounded ngay frame load.
- Death state/Animator và khả năng chạy visual khi timeScale=0. GameSession hiện vẫn đặt timeScale=1 khi chết; không đổi luật này chỉ để khớp bài thử.
- Primary/.bak/.tmp gốc được sao lưu và **khôi phục byte-for-byte** sau mỗi probe, kể cả lượt thử thất bại.

Các lượt thử đầu phát hiện điểm ngắm dự phòng ở chân và giúp sửa; hai phép kiểm ban đầu giả định sai về timeScale khi chết và grounded ngay sau load đã được điều chỉnh đúng vòng đời thật. JSON trên là lượt cuối, không cộng các checks trùng từ lượt trước.

## Bảo toàn và giới hạn

Backup ban đầu + SHA256: `SourceAssets/CharacterPolishBackups/2026-11-04/manifest.json`; giữ 38 file ban đầu, cùng backup bổ sung theo đường dẫn. Không reset workspace, không thay imported source/model/navmesh/ID/schema. Công cụ cài đặt ở `Assets/_Project/Editor/CharacterPolishSetup.cs`; probe ở `Assets/_Project/Tests/CharacterPolishSmokeProbe.cs`, chỉ biên dịch Editor và chỉ chạy khi được yêu cầu.

Đây là kiểm tra cơ chế trong Editor, **không phải benchmark FPS thực, chơi tay hoặc nghiệm thu game hoàn thiện**. Cần người chơi duyệt cảm giác tay/animation/âm thanh, thử 4 hướng lộn/khe thấp trên tuyến C2 và full C1→C2/boss trên Windows. Còn gap độc lập: ràng buộc actor sở hữu inventory ở bàn, Music/SFX UI, HUD 3/2 và bảo toàn save nhánh lỗi; không đánh dấu chúng đã xong từ kết quả này.

## Windows và hồi quy trại

[Build JSON](CharacterPolish_Build_2026-11-04.json): **Windows x64 Release thành công, 0 errors, 485 warnings**, 208.26 MB theo BuildReport. Cả 485 cảnh báo thuộc Unity AI Inference/Sentis: 460 compute shader ConvGeneric và 25 pixel shader; không tự bỏ package hay sửa shader nguồn để làm số cảnh báo bằng 0.

Mở `Builds/CharacterPolishValidation/Windows/SonDoong.exe` để thử có hình ảnh. Smoke khởi động không đồ họa xác nhận Input/PhysX khởi tạo và không có managed exception; có một thông báo Curl không nối được localhost:9 trong môi trường mạng thử. Tiến trình thử được dừng riêng sau khi quan sát khởi động; không khẳng định clean-exit code hay gameplay Windows đã qua.

[Camp Play Mode](CampEnvironment_PlayMode_2026-11-04.json) đã **chạy lại sau thay đổi nhân vật**: 53/53 checks, 0 failures/errors, gồm tuyến quanh bàn/lối đi, craft/save/Continue và 24 góc camera tại 3 vị trí. Không thay thế full C1/C2/boss hoặc chơi tay.

`git diff --check` cho code/tài liệu mới không lỗi. Một dòng trắng cuối m_Name trong scene chính do Unity serialize đã có từ lượt trại, ngoài thay đổi nhân vật; giữ nguyên scene đó.
