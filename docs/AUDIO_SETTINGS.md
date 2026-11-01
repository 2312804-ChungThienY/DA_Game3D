# Cài đặt âm lượng

Triển khai ngày 02/11/2026 từ `Son_Doong_Audio_Settings.zip` theo yêu cầu. Dự án hiện ở `E:\DoAnGame3D\Doan_Game3D`, Unity 6000.4.8f1.

- Menu chính và menu pause đều có nút Âm thanh, ba slider Tổng/Nhạc/Hiệu ứng, phần trăm, Mặc định và Đóng. Panel tắt trong Edit Mode, callback slider đăng ký trong script. Đóng panel trong pause vẫn giữ trò chơi dừng; Esc ưu tiên đóng cài đặt.
- Hai `SceneAudio` hoạt động trong hai scene, dùng chung `Assets/_Project/Audio/Mixers/MainAudioMixer.mixer`. Master chứa Music/SFX; UI dưới SFX. Tên exposed parameter: `MasterVolume`, `MusicVolume`, `SFXVolume`.
- Mặc định 1 / 0.7 / 1. PlayerPrefs chỉ dùng `sondoong.audio.master`, `sondoong.audio.music`, `sondoong.audio.sfx`. Áp mixer ngay; flush khi đóng, đổi scene, tạm dừng/thoát ứng dụng hoặc Mặc định. Không flush mỗi nhịp kéo, không DeleteAll, không đổi SaveData v4 hoặc checkpoint.
- An kế thừa SfxOutput→SFX từ `PF_An_Prototype`; bước chân/tiếp đất dùng `RoutedAudio`. Assembly `SonDoong.Audio` cho phép StarterAssets gọi helper; chỉ sửa hai hàm âm thanh và thêm trường/using trong motor. Giữ GUID và fallback StarterAssetsInputs.
- Hai scene chưa có nhạc nền hoặc AudioSource trống/cửa/địch đặt sẵn. Nguồn bổ sung cần route Music/SFX/UI. Nhạc: volume 1, 2D, Loop, tắt Play On Awake; thêm vào Start After Apply của service.
- Font `UI/Fonts/AudioSettings_Vietnamese.asset` dựng từ Noto Sans có sẵn, atlas Static; giữ nguyên font nhập từ A31.

## Kiểm chứng

Windows x64 Development build đã thành công. Lần kiểm tra trước có 0 lỗi và 485 cảnh báo (chủ yếu compute shader ConvGeneric của AI Inference có sẵn). Một lần dựng lại ghi một lỗi AssetImportWorker5 crash dù trạng thái Succeeded; đã lưu [báo cáo](reports/20261102_AudioBuildWorkerCrash.json). Lần dựng lại sau đó đạt Succeeded, 0 lỗi và 0 cảnh báo mới ([báo cáo cuối](reports/20261102_AudioBuild.json)); đây là build tăng dần nên không có nghĩa mọi cảnh báo shader trước đó đã được sửa. Build ở `Builds/AudioSettingsValidation/SonDoong.exe`, không đưa vào Git.

Hai lần mở ứng dụng độc lập đạt 35 + 5 kiểm tra: tham số mixer native/dB, slider/phần trăm/mặc định, không trùng listener, chuyển menu→game→menu, pause giữ timeScale=0, nguồn one-shot SFX, hash save tiến trình không đổi và lưu âm lượng qua mở lại ứng dụng. Đã sao lưu rồi khôi phục ba PlayerPrefs ban đầu. Báo cáo: [write](reports/20261102_AudioPlayerWrite.json), [read](reports/20261102_AudioPlayerRead.json).

Đã xem ảnh panel tiếng Việt ở mức 100%/70%/100%: [menu](images/AudioSettings_01_MainMenu.png), [pause](images/AudioSettings_02_HangLinhMach.png). Đây là ảnh panel riêng trong Editor.

Còn thử trực tiếp: nghe Master=0/SFX=0, Music=0 khi có nhạc, nhấn Esc hai lần trong pause và kéo slider bằng chuột. Chạy nền không có luồng âm thanh để đo và không giả lập bàn phím có focus, nên chưa đánh dấu các mục này đạt.

## Tái kiểm tra

Editor tool `AudioSettingsSetup.cs` đọc `Library/SonDoongAudioSetup.request.json`, action `validate`, `preview` (menu), `preview-game` (pause), `build-player`; báo cáo ở `Logs/AudioSettingsSetup.json`. `install` thay UI riêng của công việc này; `finish` đặt lại giá trị serialized mặc định. Không chạy hai action này chỉ để kiểm tra.

`AudioSettingsSmokeProbe` chỉ hoạt động khi Development build/Editor có `-audio-self-test`. Chạy `-audio-phase write` rồi `read`, cùng `-audio-backup <file mới>` và `-audio-report <path>`. Pha read khôi phục prefs; không bỏ qua pha read.
