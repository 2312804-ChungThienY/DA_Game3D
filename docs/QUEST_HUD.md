# HUD mục tiêu chương Kim Quy

Tích hợp ngày 02/11/2026 theo `Son_Doong_Quest_HUD.zip` trong `02_HangLinhMach`. Mốc trước sửa `24b34e4`; scene, mã QuestTracker cũ và ZIP gốc sao lưu tại `Temp/QuestHUDBackup_20261102`.

## Cấu hình

- Thêm ba script nguyên nội dung gói: `Scripts/Quests/ChapterObjectiveRules.cs` (dùng thư mục Quests sẵn có), `Scripts/UI/ChapterQuestHUD.cs`, `Tests/Editor/ChapterObjectiveChecks.cs`. Không thay mã cũ.
- `Canvas_Gameplay/PNL_Quest`: anchor/pivot `(1,1)`, position `(-30,-30)`, size `480×210`, Image đen alpha0.55, không Raycast Target. Đặt dưới các panel modal/pause.
- `TXT_Objective`: neo trên trái, pivot `(0,1)`, `(18,-14)`, `444×125`, font24. `TXT_QuestProgress`: `(18,-148)`, `444×44`, font20. Cả hai Top Left, wrap, không raycast, dùng font static tiếng Việt `Inventory_Vietnamese.asset` đã có.
- `Canvas_Gameplay/QuestUI` luôn bật, ChapterQuestHUD đọc WorldState của GameSession, gán hai text; checklist để trống. QuestTracker cũ và `TXT_QuestObjective` tắt trong scene, giữ nguyên file/mã/GUID.

## Luật

Tám mục tiêu theo dữ kiện WorldState: Vườn Rêu/A → giải trống → nhặt B → giải gương → nhặt C → Hộ vệ → bệ phục hồi → hoàn thành. Progress đếm riêng ba mảnh/hai puzzle; không gộp reward và solved thành một bước. HUD bắt đầu bằng Vườn Rêu theo yêu cầu; không có bước nhận đèn trong HUD mới, gameplay nhận đèn vẫn giữ nguyên.

HUD chỉ đăng ký/hủy WorldState.Changed và tính lại toàn bộ; không cấp mảnh, giải puzzle, đánh Hộ vệ, ghi file hoặc lưu currentQuestIndex. RestoreSnapshot phát Changed nên tải checkpoint cũ tự trả mục tiêu cũ. Nguyên liệu/chế/uống thuốc không đổi mục tiêu chính.

## Kiểm chứng và tiếp nối

Menu `Son Doong → Tests → Check chapter objectives` đã chạy: **128 tổ hợp và reset completed→new game đạt**. [Báo cáo](reports/20261102_QuestObjectiveChecks.json). Play Mode từ Boot đạt **64 kiểm tra**, không lỗi runtime ([báo cáo](reports/20261102_QuestHUDPlayMode.json)). Đã thử cả tám mục tiêu, solved riêng với nhận B/C, checkpoint trước B rồi tiến tới C chưa lưu và tải lại, Continue bản hoàn thành, New Game, nguyên liệu/chế/uống không đổi nhiệm vụ; Refresh không cấp thưởng/ghi save, OnEnable/OnDisable không trùng listener. Save gốc chính/backup/tmp được khôi phục byte-for-byte.

Kiểm thử gọi API gameplay có sẵn: RelicPickup.Interact, chuỗi A-C-B của DrumPuzzleController, LightPuzzleReceiver.TickLight, Guardian encounter/damage/death và bệ phục hồi/SaveService. Đây là kiểm tra đường cập nhật trạng thái/HUD, chưa phải đi toàn tuyến bằng bàn phím, kiểm tra quang học gương hoặc cân bằng combat.

Đã xem [HUD trong Play Mode](images/QuestHUD_PlayMode.png) và panel ở [1920×1080](images/QuestHUD_1920x1080.png), [1280×720](images/QuestHUD_1280x720.png), [1600×1000](images/QuestHUD_1600x1000.png); chữ Việt đủ dấu, không tràn khung. [Báo cáo layout](reports/20261102_QuestHUDLayout.json). Các ảnh theo độ phân giải render riêng panel, không thay cấu hình game. Chưa dựng/chạy Windows cho bước HUD này.

`QuestHUDSetup.cs` đọc `Library/SonDoongQuestHUD.request.json`, các action `install`, `validate`, `contracts`, `play-test`, `preview`, `open-main`; xuất `Logs/QuestHUDSetup.json`. Install dựng lại cấu hình riêng của bước này; không chạy chỉ để kiểm tra. Preview render riêng panel ở 1920×1080, 1280×720 và 1600×1000, không đổi game settings.

`QuestHUDSmokeProbe` chỉ chạy khi công cụ bật SessionState trong Editor hoặc Development build có `-quest-hud-self-test`; tùy chọn `-quest-hud-report <path>`, `-quest-hud-backup <thư mục mới>`. Probe sao lưu file chính/backup/tmp và khôi phục byte-for-byte khi kết thúc hoặc assertion. Không chạy cùng các probe khác; nếu bị buộc tắt giữa chừng phải khôi phục theo manifest trước khi chơi tiếp.
