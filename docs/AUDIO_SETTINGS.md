# Âm thanh và cài đặt — 05/11/2026

Yêu cầu hiện hành giữ ba thanh **Âm lượng tổng / Nhạc / Hiệu ứng**. Bản hoàn thiện C1 đã nối đủ ba thanh trong `Resources/PF_AppRootUI.prefab`, cùng độ nhạy chuột, toàn màn hình và chất lượng Thấp/Vừa. Các báo cáo 02/11 là lịch sử trước merge.

C2 lượt7 ngày06/11 có `Resources/C2SoundPalette.asset`: loop khám phá/đấu trường, gió, chuông và tiếng chim tổng hợp của dự án; giữ cue UI/va chạm C1 và group Music/SFX/UI. Soundscape chọn palette theo scene, chuyển nhạc khi quái ném/Chim Lạc giao chiến; gió cột là nguồn3D, chuông có ba cao độ. Pause dừng SFX, nhạc/UI theo hợp đồng cũ. [Worklog](C2_ENVIRONMENT_WORKLOG.md), [nguồn](CHAPTER02_ASSET_CREDITS.md); chưa nghiệm thu nghe cân âm qua loa/tai nghe.

## Cấu hình hiện hành

`SettingsService` tải **`Assets/_Project/Resources/Audio/MainAudioMixer.mixer`**. Mixer được di chuyển bằng AssetDatabase, giữ GUID/group; đường dẫn `Audio/Mixers/MainAudioMixer.mixer` trong báo cáo cũ là vị trí trước lượt này. Nguồn footsteps/RoutedAudio giữ tham chiếu bằng GUID.

Ba tham số exposed: `MasterVolume`, `MusicVolume`, `SFXVolume`. Giá trị tuyến tính đổi sang dB bằng `20*log10(value)`, zero thành −80 dB. Khi có mixer, `AudioListener.volume=1` tránh nhân Master hai lần. Áp mixer ở Start/runtime, không giả định SetFloat trong Awake đã có hiệu lực.

| Thiết lập | PlayerPrefs / mặc định |
| --- | --- |
| Tổng / Nhạc / Hiệu ứng | `settings.masterVolume` / `settings.musicVolume` / `settings.sfxVolume`; 1 / 0.7 / 1 |
| Tương thích cũ | Đọc `sondoong.audio.master/music/sfx` nếu khóa mới chưa có; chỉ ghi khóa mới khi thay đổi |
| Độ nhạy | `settings.mouseSensitivity.v2`; 0.03–0.6, mặc định 0.15 |
| Toàn màn hình | `settings.fullscreen`; chỉ áp cửa sổ ngoài Editor |
| Chất lượng | `settings.quality`; 0/1 ánh xạ hai quality hiện có |
| Flush | Sau 0.4 giây unscaled, khi đóng cài đặt/pause/quit/destroy |

`SoundscapeRuntime` nằm trong `Scripts/Presentation`, không nằm trong assembly riêng `SonDoong.Audio`. AppRoot tạo nó sau GameSession. Palette `Resources/C1SoundPalette.asset` nối các group Music/SFX/UI và bản sao audio trong `_Project/Audio/C1`. Nhạc menu/khám phá/giao chiến chuyển chéo khoảng 0.95 giây; ambience chỉ phát ở scene có player. Âm thế giới dừng theo pause; UI và nhạc vẫn phản hồi trong menu, nhạc giảm mức khi pause.

Phản hồi gồm hover/click, nhặt đồ/đèn, chế thuốc, mở phong ấn/cửa, giải đố, ba cao độ trống, xoay gương, quái đánh/trúng đòn/chết. Âm thanh không trao vật phẩm hoặc ghi save. Bind scene đợi khôi phục state trước khi nghe Changed để tránh phát tiếng thưởng hàng loạt khi Continue.

## Kiểm chứng và giới hạn

Công cụ hiện hành: `Chapter01CompletionProbe`, opt-in từ `Chapter01CompletionSetup` action `play-test`. Nó sao lưu/khôi phục file lưu cá nhân, các khóa PlayerPrefs bị thử và chất lượng ban đầu. Kết quả ở [C1Completion Play Mode](reports/C1Completion_PlayMode_2026-11-05.json); đọc failures/errors và ngày chạy, không suy đạt từ sự tồn tại của file.

Bài thử gồm callback ba slider, dB độc lập, mute SFX, quality, flush, pause/settings/back/resume và nguồn nhạc đang phát. Chưa thay cho nghe cân âm trên loa/tai nghe hoặc qua hai lần chạy bản Windows. A38 và ba tiếng trống từ A33 là âm thử theo danh mục, chưa thay bản thu nhạc cụ Việt/trống đồng. Nguồn ở [credit](CHAPTER01_ASSET_CREDITS.txt).
