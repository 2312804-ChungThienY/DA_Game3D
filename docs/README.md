# Tài liệu dự án Sơn Đoòng

Kiểm kê toàn dự án ngày **04/11/2026**, cập nhật môi trường C1 mở rộng ngày **05/11/2026**, Asia/Bangkok. Mục tiêu: **5 chương; ưu tiên môi trường chương 1 trước; chưa chốt hạn mới**. Lịch 7 ngày và lịch 12 tuần trong spec là lịch tham chiếu cũ.

## Đọc theo thứ tự

| Tài liệu | Dùng để biết |
| --- | --- |
| [PROJECT_MEMORY](PROJECT_MEMORY.md) | Thiết kế và quyết định hiện hành; quy tắc phải giữ |
| [PROJECT_STATUS](PROJECT_STATUS.md) | Trạng thái thật, bằng chứng kiểm thử và việc còn lại |
| [REQUIREMENTS_MATRIX](REQUIREMENTS_MATRIX.md) | Từng yêu cầu: nguồn, triển khai hiện có, khoảng thiếu và mức kiểm chứng |
| [CURRENT_ARCHITECTURE](CURRENT_ARCHITECTURE.md) | Component/API/ID/save/input/scene thực tế sau merge |
| [DONGSON_DRUM_WORKLOG](DONGSON_DRUM_WORKLOG.md) | Ba trống giải đố C1 dùng FBX ndnguyen3d từ ZIP06/11, texture2K/PBR, ảnh, credit và kiểm thử |
| [SPEC_INDEX](SPEC_INDEX.md) | Tìm đúng phần thiết kế nguồn và phiên bản đã đối chiếu |
| [CHAPTER02_MECHANICS_PLAN](CHAPTER02_MECHANICS_PLAN.md) | Chia cơ chế Chim Lạc thành từng lượt; sửa phần đã có trước, phần mới làm khi người dùng yêu cầu tiếp |
| [C2_OPENING_WORKLOG](C2_OPENING_WORKLOG.md) | Lượt1 đã đạt: sửa cổng C1 bị vách che, nối C1→C2, giấy/lông/hướng dẫn đầu chương, save/Continue |
| [C2_TRAVERSAL_WORKLOG](C2_TRAVERSAL_WORKLOG.md) | Lượt2 đã đạt:4Space/đáp bằng controller, cột gió trong suốt, camera và mid save/death/Continue |
| [C2_KNOWLEDGE_WORKLOG](C2_KNOWLEDGE_WORKLOG.md), [C2_KNOWLEDGE_SOURCES](C2_KNOWLEDGE_SOURCES.md) | Lượt3: chuông/giấy/bia/quiz, thưởng nguyên tử và cờ sai qua Continue; nguồn trực tiếp |
| [C2_BRIDGE_WORKLOG](C2_BRIDGE_WORKLOG.md) | Lượt4: Ghost/quái ném/choáng, đạn và cửa an toàn khi lưu lỗi; E/RMB/W/Space qua hai khe/hai gờ |
| [C2_ARENA_WORKLOG](C2_ARENA_WORKLOG.md) | Lượt5 cơ chế: pha2/hit/choáng/quạt, HUD, win/save-retry/Continue và chuỗi input thật; cân tay/art cuối7 |
| [C2_ENDING_WORKLOG](C2_ENDING_WORKLOG.md) | Lượt6: phục dựng/ba câu/lời Chim Lạc, save lỗi/thử lại/Journal và menu/Continue; art/Windows có bằng chứng lượt7 |
| [C2_ENVIRONMENT_WORKLOG](C2_ENVIRONMENT_WORKLOG.md), [asset/âm thanh C2](CHAPTER02_ASSET_CREDITS.md) | Lượt7: năm khu có đồ họa, HUD/túi C2 và âm thanh riêng; hồi quy/Windows và giới hạn nghiệm thu |
| [C1_GUIDES_PEDESTALS](C1_GUIDES_PEDESTALS.md) | Mới nhất05/11: giấy giới thiệu/điều khiển/trước Kim Quy, bệ và vật phẩm đúng tâm, khôi phục bệ đèn |
| [C1_REVISION_WORKLOG](C1_REVISION_WORKLOG.md) | Mới nhất 05/11: chế trong túi, giấy gợi ý, A20/A22/A23/A24, lửa động, choáng quái và sương |
| [CHAPTER01_COMPLETION_WORKLOG](CHAPTER01_COMPLETION_WORKLOG.md) | C1 mới nhất: sửa meta/khe hang/camera, FBX quái/đèn/cửa, UI/audio, Play Mode và bản Windows |
| [CHAPTER01_EXPANDED](CHAPTER01_EXPANDED.md) | C1 mở rộng 05/11: năm khoang, bốn lối hang nối, cấu trúc scene, tọa độ, asset và kiểm chứng |
| [CHAPTER01_ENVIRONMENT](CHAPTER01_ENVIRONMENT.md) | Bối cảnh toàn năm khu C1, sửa tiếp xúc đá/cỏ, asset miễn phí, ảnh và giới hạn kiểm chứng |
| [CAMP_ENVIRONMENT](CAMP_ENVIRONMENT.md) | Khu mẫu Trại Giếng Trời, scene preview, asset, bố cục và cách kiểm tra |
| [AUDIO_SETTINGS](AUDIO_SETTINGS.md), [QUEST_HUD](QUEST_HUD.md) | Cấu hình hiện có và yêu cầu âm thanh/HUD còn thiếu sau merge |
| [INVENTORY_POTIONS](INVENTORY_POTIONS.md), [CAMP_CRAFTING](CAMP_CRAFTING.md) | Túi/chế bình máu trong C1, dùng Q và E checkpoint lưu riêng |
| [ENVIRONMENT_ASSET_TRIALS](ENVIRONMENT_ASSET_TRIALS.md), [UNITY_IMPORT_FIXES](UNITY_IMPORT_FIXES.md) | Asset đã thử, lịch sử sửa import và giới hạn của bằng chứng |

## Cách giải quyết khác biệt

1. Yêu cầu mới nhất của người dùng ưu tiên cao nhất. Ngày04/11 chọn **3D hoạt hình low poly**, **Master/Music/SFX** và HUD mở đầu **Vườn Rêu**, tiến độ **3mảnh/2câu đố**. Ngày05/11 chuyển chế thuốc vào túi thay bàn, dùng A20/A22/A23/A24 thực tế, giấy gợi ý tùy chọn và choáng Hắc Ảnh4s/Hộ vệ2s. Chỉ model An vẫn được giữ nguyên.
2. `spec/chapters/` và kế hoạch §55 chi tiết hóa chương; §40–54 thay các điểm mâu thuẫn trong §1–39. Phần cũ còn giá trị khi chưa bị thay thế.
3. Code/scene là bằng chứng của **cái đang có**. Code thiếu một yêu cầu không có nghĩa người dùng đã bỏ yêu cầu đó. Ghi khoảng thiếu vào bảng yêu cầu; không tự sửa thiết kế để hợp thức hóa code.
4. File/prefab tồn tại, cấu hình đã đọc, thử Editor, thử Play Mode và thử Windows build là các mức khác nhau. Không chuyển kết quả của phiên bản trước merge sang phiên bản hiện tại.

## Báo cáo và lịch sử

- [Rà soát runtime](reports/RuntimeAudit_2026-11-04.md), [rà soát phạm vi/chương](reports/ScopeAudit_2026-11-04.md), [kiểm kê cấu trúc/tham chiếu](reports/ProjectAudit_2026-11-04.json).
- [53 kiểm tra Play Mode Trại Giếng Trời](reports/CampEnvironment_PlayMode_2026-11-04.json) là bằng chứng sau merge cho đoạn trại, không phải toàn bộ 5 chương.
- [PROJECT_STATUS trước đồng bộ](history/PROJECT_STATUS_before_sync_2026-11-04.md) và [PROJECT_MEMORY trước đồng bộ](history/PROJECT_MEMORY_before_sync_2026-11-04.md) giữ nhật ký cũ. Các mô tả “hiện tại” trong thư mục `history/` chỉ thuộc mốc cũ, không dùng làm chỉ dẫn triển khai.

Đợt đồng bộ tài liệu ban đầu chỉ đối chiếu tĩnh. Các lượt điều khiển và bối cảnh C1 sau đó có sửa code/scene và kiểm thử riêng, ghi tại PROJECT_STATUS. Backlog còn thiếu không tự trở thành lệnh triển khai.
