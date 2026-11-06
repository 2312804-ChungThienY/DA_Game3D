# Hoàn thiện chương 1 theo ảnh lỗi — 05/11/2026

Phạm vi mới nhất: hoàn thiện C1 trừ model/rig An; người dùng đã cho phép Hắc Ảnh, Hộ vệ, đèn sen, cửa, UI/audio và các vật C1. Dùng asset miễn phí đã nhập. Không tạo chương mới hoặc thay hệ input/state/save/ID.

## Các thay đổi

- **Metadata:** Unity mở ReadWrite, ImportAsset và SaveAndReimport thành công ba file `T_Step_N.tga`, `T_Tile_N.tga`, `Torch_B.prefab`, giữ GUID. File không readonly và ACL cho phép ghi lúc kiểm tra; không khẳng định nguyên nhân cũ là readonly. Không xóa Library/meta hoặc đổi ACL cả cây. Backup tại `SourceAssets/SceneBackups/2026-11-05_Chapter01Completion/Metadata_210110047`.
- **Hang:** `ENV_C1_ClosedJunctions` thêm tám vách nối đủ chiều cao, vòm/nền chồng lấn giữa khoang và đường uốn cong. Giữ giếng trời giữa khoang, chiều rộng lối đi, collider nền, thứ tự và vị trí gameplay của bản mở rộng. NavMesh được bake lại; collider khối đá khớp bề dày hình cảnh.
- **Camera:** Teleport thông báo warp cho Cinemachine 3 để camera không kéo từ vị trí cũ qua vách khi chuyển vị trí/hồi sinh. Năm vách mỏng có thêm collider mặt ngược để truy vấn nhận cả mặt ngoài, giữ nguyên render mesh và thiết lập physics toàn game. `CaveCameraCollisionGuard` kiểm sphere cast cuối pipeline, sửa CameraState sau ThirdPersonFollow; không tạo motor vị trí cho actor. `CameraBodyOcclusion` chuyển body sang ShadowsOnly khi camera ép sát. Giữ model/rig An.
- **Quái:** root AI/health/collider/focus/ID cũ giữ nguyên. Child `VIS_ImportedEnemy` chứa Ghost A08 và Golem Kevin Iglesias. Ghost dùng năm clip native. Golem nguồn không có clip; sáu clip Idle/Walk/Sweep/Slam/Hurt/Death được dự án tạo trên bộ xương thật, generic/avatar null/root motion off. `EnemyPresentation` theo FSM, không điều khiển damage/movement. StoneGuardian vẫn một transform motor, thêm CapsuleCast chống xuyên vật cản.
- **Vật cảnh:** cửa A10 `Doors_RoundArch.fbx` và sigil ngọc dưới barrier cũ; tinh thể A17, bình thuốc A17, cây A11 và nấm A19 dưới pickup root cũ. Mảnh trống là cung đồng khắc ngọc, checkpoint có tinh thể; 48 cụm tinh thể FBX thay hình nón đơn. Đèn sen là mesh cánh/khung đồng của dự án, treo dưới tay, giữ kích thước và giảm chói. Đá lát có cạnh vát, pháp tuyến hướng lên; không thêm collider lổn nhổn vào đường đi.
- **UI:** menu có ảnh trại mới, bốn nút gốc và Continue theo save. HUD có vitals/thuốc, vùng E, tên khu, mục tiêu Vườn Rêu, tiến độ 3 mảnh/2 câu đố. Đồng bộ settings/pause/death/confirm/toast/túi/nhật ký/story/boss bar; nội dung dài cuộn được, giữ Noto Sans. Trạng thái puzzle chuyển xuống gần prompt, giữ ProximityVisibility.
- **Âm thanh:** Master/Music/SFX nối mixer dB, đọc fallback khóa cũ, debounce flush; thêm quality Thấp/Vừa. Mixer chuyển bằng AssetDatabase sang `Resources/Audio/MainAudioMixer.mixer`, giữ GUID/group. SoundscapeRuntime/palette thêm menu/explore/combat crossfade, ambience và one-shot UI/pickup/puzzle/enemy. Tham chiếu footsteps/SFX trước đó giữ nguyên.
- **Phục dựng:** RelicAltar chỉ hoàn tất sau SaveAt thành công. Nếu service chưa sẵn sàng/lưu trả false, rollback snapshot world/ability, giữ màn kết đóng. Không đổi native schema4, migration legacy1–5 hoặc WorldIds.

Scene chính và preview đã được sao lưu theo từng lượt tại `SourceAssets/SceneBackups/2026-11-05_Chapter01Completion`. Source FBX/texture/audio/license giữ nguyên; model và vật liệu mới của dự án nằm trong `Assets/_Project/Art/Chapter01Completion`.

## Kiểm chứng

- [Metadata](reports/C1Completion_Metadata_2026-11-05.json), [hull](reports/C1Completion_Hull_2026-11-05.json), [actors](reports/C1Completion_Actors_2026-11-05.json), [audio](reports/C1Completion_Audio_2026-11-05.json), [UI](reports/C1Completion_UI_2026-11-05.json), [props](reports/C1Completion_Props_2026-11-05.json): báo cáo các lượt cài vào scene; không thay bằng chứng Play.
- [Editor](reports/C1Completion_Editor_2026-11-05.json): Main và Preview mỗi scene 1.561 renderer đang bật, 0 missing scripts, 48 cụm tinh thể FBX, 2 đèn sen, 8 vestibule. Không có material mất/unsupported/Standard trên renderer đang bật; 4 checkpoint có NavMesh an toàn.
- [Traversal](reports/C1Completion_Traversal_2026-11-05.json): **569 checks, 0 failures/errors** ở lượt sau vá hang. CharacterController đi tuyến thật, A–C–B và phản xạ qua hai gương thật; ba mảnh/hai câu đố/cổng/Continue/rơi/hồi sinh. AI bị dừng và cờ thắng trùm dùng fixture để cô lập đo đường; không gọi đây là đánh trùm bằng input.
- [Completion Play Mode](reports/C1Completion_PlayMode_2026-11-05.json): **150 checks, 0 failures/errors**, sau sửa collider mặt ngoài và guard camera. Gồm menu/UI/mixer/input/hoạt ảnh/quái/ending/save, 80 hướng nhìn tại bốn vị trí với pitch −35/−20/4/45/65; kiểm cả sphere cuối camera, không chỉ ray tới sát trước camera. Chẩn đoán trước sửa giữ riêng trong CameraBeforeFix và ảnh c1-diagnostic-before-camera-fix, không dùng làm ảnh nghiệm thu.
- E thử qua Input System/InteractionScanner với fixture camera ngắm cố định, tách bàn chế và checkpoint. Chuột trái đi qua CombatController, trùm dùng fixture đứng yên và người thử bất tử; vẫn giữ FSM/health/death/event ghi boss ID thật. Hắc Ảnh gây damage, Golem báo quét/nện, xương chuyển động, tránh vùng nện. Ending/restoration có fixture 3 mảnh/2 puzzle vì traversal đã giải riêng toàn tuyến.
- Probe sao lưu và trả save chính/backup/tmp **byte-for-byte**, trả các khóa PlayerPrefs/chất lượng cũ. Không để tiến trình kiểm thử ghi đè tiến trình cá nhân.
- Ảnh `c1-complete-*`: capture UI ở 1280×720, 1600×900, 1600×1000; `c1-complete-native-*` là Game View 1920×1080. Capture camera/Canvas không thay cho nghiệm thu chơi tay.

## Công cụ và bàn giao

Bridge `Assets/_Project/Editor/Chapter01CompletionSetup.cs`, request `Library/SonDoongC1Completion.request.json`, report `Logs/C1Completion-{action}.json`. Các action sửa từ chối Play/dirty scene; giữ thay đổi người dùng. `play-test`/`traversal-test` opt-in và có backup, không chạy trong build. Không chạy lại Expanded stage/apply lên scene đã mở rộng.

Bản Windows: `Builds/Chapter01Complete/Windows/SonDoong.exe` cùng Data/DLL/CREDITS. [Build mới](reports/C1Completion_Build_2026-11-05.json) **Succeeded, 0 errors, 485 shader warnings** từ AI Inference/Sentis; size theo BuildReport 282.066.148 bytes. Không lấy binary Chapter01Expanded cũ làm bằng chứng cho lượt này. Nguồn và giấy phép ở [credit](CHAPTER01_ASSET_CREDITS.txt).

[Delivery audit](reports/C1Completion_Delivery_2026-11-05.json) xác nhận hash scene/binary/report, credit trùng và assembly Windows có runtime mới, không chứa probe Editor. [Trạng thái Editor cuối](reports/C1Completion_EditorState_2026-11-05.json): Play=False, mở Main, dirty=False. UI HUD nằm dưới lớp modal; toast đã hạ xuống để không đè nút/mép màn kết.

Giới hạn: chưa chơi tay bản Windows/máy thứ hai, đo FPS, bake GI hoặc cân âm bằng loa/tai nghe. Âm trống A33 và nhạc A38 vẫn là âm thử, chưa là bản thu nhạc cụ Việt. Model An vẫn mannequin theo phạm vi loại trừ. Không cần thêm asset trả phí để chạy cấu hình C1 này.
