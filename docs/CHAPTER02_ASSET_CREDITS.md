# Nguồn đồ họa và âm thanh C2 —06/11/2026

Chỉ dùng bộ đã có trong dự án và nội dung tự tạo; không tải hoặc mua asset trong lượt7.

| Nguồn | Cách dùng trong C2 |
| --- | --- |
| A09 Mines and Cave Set | Các khối đá, trụ đá và vách xa. Giữ bản nguồn; fit ở nhóm trang trí, không thêm collider vào tuyến chơi |
| A10 Ultimate Modular Ruins Pack — Quaternius | Cột nhà sàn, trụ đậu, lan can, lát đá. CC0 theo hồ sơ asset hiện có |
| A11 Ultimate Stylized Nature Pack — Quaternius | Cây cọ, bụi, cỏ, đá theo cụm; dùng vật liệu URP của bản mẫu C1. Giữ license/tệp nguồn của bộ đã nhập |
| A17 Ultimate RPG Pack — Quaternius | Chai thuốc nếu có item tương ứng; không thay item ID hoặc catalog |
| A22/A24 do người dùng nhập | Bản FBX tối ưu của cổng và bệ từ lượt C1. Dùng lại, không sửa source |
| Fog Particles đã nhập | Vật liệu URP bản sao ở C1 cho các hạt gió/lửa nhỏ; giữ nguồn/giấy phép theo hồ sơ C1 |
| Đồ họa tự tạo trong dự án | Mesh vách kín với UV theo mét, mái cong, chuông, lông chim, cổ vật mặt trời, tạo hình Chim Lạc và bố cục HUD. Không tuyên bố đây là phục dựng kiến trúc/lịch sử |
| Âm thanh tự tạo trong dự án | `SourceAssets/Tools/c2_audio.py`: gió, tiếng chuông tổng hợp, tiếng chim cách điệu, hai vòng âm nhạc. Không dùng mẫu thu ngoài hoặc gọi đây là bản thu nhạc cụ truyền thống |
| Nguồn C1 dùng lại | Music/SFX/UI AudioMixer, tiếng UI/nhặt đồ/đòn đánh; [hồ sơ C1](CHAPTER01_ASSET_CREDITS.txt) |

Người dùng xác nhận A22/A23/A24 lấy từ Sketchfab free. Chưa có URL từng model/tác giả/giấy phép cụ thể trong thư mục nhập; giữ trạng thái hồ sơ này, không gán nhãn CC0 cho ba mẫu. Bản Windows lượt7 là development build để thử cục bộ, không phải công bố phát hành.

Giữ `.meta` và GUID của asset cũ. C2 chỉ dùng bản sao material/mesh/audio do dự án quản lý; không đổi pipeline hoặc vật liệu C1 để đạt màu C2.
