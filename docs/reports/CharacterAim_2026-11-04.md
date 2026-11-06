# Sửa chuột và hướng tấn công — 04/11/2026

Người dùng chơi thử và phản hồi xoay chuột khó, tấn công không đúng. Các phép thử cơ chế của lượt trước không đủ để kết luận cảm giác điều khiển đã đạt.

## Nguyên nhân và sửa đổi

- Trục Y cộng trực tiếp vào pitch của Unity, khiến rê chuột lên thành nhìn xuống. Đã đổi thành rê lên nhìn lên, rê xuống nhìn xuống; giữ độ nhạy người dùng và giới hạn góc hiện có.
- Chuột được đọc cuối frame, còn Combat chạy trước đó và lấy hướng WASD/đứng cũ. Bây giờ PlayerCameraController đọc Look ở Update order −80, trước Dodge −60, Combat −40 và Motor 0; chỉ ghi pivot ở LateUpdate sau khi actor di chuyển. Motor dùng yaw đã đọc trong frame, không lấy góc của camera render khung trước.
- Không lock-on: đòn chém theo hướng camera chiếu xuống XZ, kể cả khi đứng yên, đi lùi hoặc đi ngang. Có lock-on: tiếp tục hướng về mục tiêu. PlayerMotor.FaceDirection thống nhất hướng model và hướng logic đòn; giữ hướng đã chọn trong đòn.
- Bắt đầu đánh dừng tốc độ di chuyển cũ và animation locomotion, tránh trượt lùi hoặc chạy chân trong lúc chém. Lunge vẫn qua dash có va chạm như trước.

Không đổi độ nhạy đã lưu, tốc độ, stamina/damage, luật mở lộn, collider cảnh, ID hoặc schema save. Không thay model/clip tay/dao trong lượt sửa hướng này.

## Kiểm chứng

[Play Mode JSON](CharacterAim_PlayMode_2026-11-04.json): **91 checks, 0 failures, 0 errors** qua virtual keyboard/mouse trên Input System thật. Bao gồm 76 phép kiểm cơ chế tương ứng lượt trước và 15 phép kiểm mới:

- Đứng yên, camera yaw 0/90/−90/180: An quay đúng, world pivot không bị model kéo theo, mục tiêu đúng hướng nhận đúng một hit.
- Đi lùi rồi đánh: mặt hướng về camera đang nhìn, không tiếp tục trượt theo S.
- Xoay chuột + A + LMB cùng frame: đòn theo yaw mới, không theo A hoặc camera render cũ.

Các kiểm tra Y đã đổi để xác nhận **chuột lên nhìn lên**, thay vì chỉ kiểm con số clamp như trước. Giữ kiểm tra va chạm, nhảy, lộn, perfect/lunge, wind/hover, Focus/E/thuốc/craft, save/Continue, pause/death. Save gốc primary/.bak/.tmp được trả byte-for-byte. Đây không phải chơi tay hoặc benchmark FPS.

File sửa: `PlayerCameraController.cs`, `PlayerMotor.cs`, `CombatController.cs`; bổ sung case trong `CharacterPolishSmokeProbe.cs`. Backup trước sửa tại `SourceAssets/CharacterAimBackups/2026-11-04/manifest.json`. Bằng chứng 76 checks và build trước vẫn giữ nguyên để phân biệt hai phiên bản.

**Windows x64 Release đã build thành công: 0 errors, 485 warnings từ shader Unity AI Inference/Sentis**; [Build JSON](CharacterAim_Build_2026-11-04.json). Mở `Builds/CharacterAimValidation/Windows/SonDoong.exe` để dùng bản sửa mới; bản trước ở CharacterPolishValidation được giữ nguyên. Đã kiểm assembly đóng gói có API hướng đánh mới và không chứa probe Editor; chưa chơi tay Windows có đồ họa trong lượt này. Cần người dùng thử lại cảm giác xoay chuột; nếu phản hồi liên quan động tác tay/dao thì cần kiểm tra trực quan clip hiện có, không coi sửa hướng là đã hoàn thiện animation.
