# C1 — sửa theo phản hồi tiếp 05/11/2026

**Điều chỉnh tiếp theo05/11:** người dùng xác nhận giữ ngôi sao, chỉ bỏ hiệu ứng màu trên thân khi suy yếu. Đã bỏ tint/emission runtime, giữ nguyên hành vi `EnemyStunIndicator` trước lượt đổi màu. Bài riêng52checks đạt, Windows cập nhật0lỗi/0warning; xem mục mới nhất trong PROJECT_STATUS và các report `C1_NoWeaknessVisuals_*`. Thông tin glow và hash bản chơi bên dưới thuộc mốc trước điều chỉnh này.

Phạm vi: bỏ bàn chế C1, chế Bình máu trong túi, sửa lửa/ghế gỗ, dùng A20 trống, A22 cổng, A23 gương, A24 bệ, hai giấy gợi ý, hướng model/mảnh trống, choáng đèn, sương URP và UI. Không thay model An. Người dùng chốt Hắc Ảnh choáng 4s/Hộ vệ 2s, chống choáng lặp; giấy là manh mối tùy chọn, vẫn cho tự thử.

## Tệp/công cụ đang có

- `Chapter01RevisionSetup.cs`: request `Library/SonDoongC1Revision.request.json`, action inspect/apply/validate/play-test/traversal-test/stun-test/build; report `Logs/C1Revision-{action}.json`. Refuse dirty scene/Play, backup Main/Preview trước sửa. Các repair-fire/repair-crowns/repair-mirrors/refine-details là bước tinh chỉnh có chủ đích của lượt này. Không chạy Expanded stage/apply lên world hiện hành.
- `Chapter01RevisionArt.cs`, `Chapter01RevisionActors.cs`, `Chapter01RevisionUI.cs`: authoring mới. Source model giữ nguyên; model dưới gameplay root và nhóm `World_Geometry/ART_C1_Revision`.
- Backup trước sửa: `SourceAssets/SceneBackups/2026-11-05_C1Revision/BeforeRevision/manifest.json` (30 file) + backup mỗi lần apply.
- Blender `5.2.3 LTS`, script `SourceAssets/Tools/c1_revision_models.py`: A22 83.036 polygon→22.000 triangles; A23 GLB 246.272→11.999 triangles, A24 OBJ324→324 triangles. FBX riêng ở `_Project/Art/Chapter01Revision/Models`, report `Logs/C1Revision-model-processing.json`. OBJ nguồn thiếu MTL, gán texture Albedo bằng material dự án; không sửa file nguồn.
- Các số A23/A24 mới theo **thư mục thực tế của người dùng**, không phải Particle Pack/VFX trong catalog cũ. Có bản trùng trong A39/_Project; dùng bản ở gốc Assets. Fog dùng `Assets/Fog Particles/Prefabs/Whitish Fog.prefab`, chuyển material dự án URP.

## Runtime đã sửa (chưa coi đã Play/build)

- InventoryService thêm CanCraft cùng hàm dựng giao dịch với TryCraft; InventoryPanel thêm button/caption/recipe/icons và TryCraftPotion trong InMenu, không autosave. ItemDefinition thêm Sprite icon, không đổi item ID/schema.
- WorldIds thêm Flags `c1_clue_drum`/`c1_clue_mirror`; PuzzleCluePickup giữ root active và bật/tắt visual/collider theo world state, nên restore được cả đã nhặt/chưa nhặt. Journal append manh mối, không cộng điểm/bia.
- DrumInteractable/Controller hiện Mặt Trời/Cánh Chim/Sóng Nước; giữ enum A–C–B. Quest/Inventory không hiển thị mã A/B/C cho các mảnh (Mặt/Thân/Chân Trống).
- EnemyLanternReceiver stun opt-in C1 (mặc định false ở scene khác), 4s/2s +3s immunity, phải bắt đầu lượt giữ mới; weakness x1.5 giữ cơ chế cũ. EnemyBrain/StoneGuardian có Stunned, ngừng motion/telegraph/damage; EnemyPresentation ưu tiên Stunned. Rig clip cúi đầu + EnemyStunIndicator; CampFireAnimation dùng thời gian gameplay.
- Ảnh hai phía `c1-revision-before-*` xác nhận visual con đang bị bù 180° từ cách parent giữ world rotation; sửa **localRotation identity**, không xoay root AI hoặc mặc định xoay FBX thêm180. Golem clips được dựng lại trong basis đã sửa, Ghost punch peak căn telegraph0.4s.

## Trạng thái tiếp nối

- inspect đã chạy, `Logs/C1Revision-inspect.json` có bounds/collider/rig. Main trước sửa là Boot clean, Edit Mode.
- apply thành công cả Main/Preview (`Logs/C1Revision-apply.json`), backup `Apply_012609783`. Lượt thử đầu gặp folder FBX vật lý chưa được AssetDatabase nhận; đã sửa Folder() recursive/synchronous, không xóa source model. Các folder trùng rỗng do thử tạo đã được dọn trong phạm vi project.
- Đã chụp Game View native1920×1080 và capture UI/world. Sau ảnh, tắt hẳn Bonfire_Fire cũ, chỉnh flame particles kéo dài; header đá khép tới6,15m thay vì chỉ phủ khe thấp. Kiểm Editor qua material/script/NavMesh/crown đã đạt; cần chạy lại sau phần chi tiết cuối.
- Các ca chạy mới đã đi qua craft paused/missing/full/freed-slot/spam/outside-menu, hai giấy E/Story/Journal và restoreworld, 80góc camera, cúi đầu/sao/khóa đòn và pause stun. Đã sửa NavMesh dư vận tốc bằng ResetPath/velocity0 khi choáng. Bài tổng hợp vẫn đang chạy lại, **không coi báo cáo thử bị dừng là nghiệm thu**.
- Tinh chỉnh cuối: bỏ nhãn 3D A/B/C còn sót ngoài root trống, thêm hoa văn đồng trên cửa; scrollbar/đầu trang Nhật ký, sao choáng giữ trong khung nhìn, không đổi toàn màu đá sang vàng khi yếu. InputReader hoãn đọc binding trong callback focus sang Update để tránh lỗi Input System khi Unity đổi bố cục/đang tháo thiết bị kiểm thử.
- Ảnh Play phát hiện `DrumVisualLabel.Awake` tự sinh chữ sau khi scene đã tắt nhãn; đã gỡ component này trong C1, không chỉ tắt renderer. `Chapter01PropSetup` từng xử lý lại mesh đã bevel nếu số vertex chia hết9, sinh `*_Beveled_Beveled` vỡ tam giác; đã chặn xử lý lần hai và trả các ref Main/Preview về mesh bevel một lần còn nguyên. Collider trống khớp bounds FBX mới, giữ component/enum/puzzle ref.
- Sau sửa: Integration213checks, Traversal542checks, tất cả0failures/0errors. Sau ảnh cuối phát hiện dấu sao Hộ vệ bị thân che khi hạ vào khung hình, đã đưa marker ra trước thân theo cùng tia nhìn, giữ depth test với Environment; Stun chạy lại50checks/0failures/0errors, thêm ray kiểm marker không bị body che. Bài Stun lần cuối mất focus ở đoạn chém boss nên dùng EnemyHealth damage API cho death; bài Integration213 trước đó có LMB thật qua CombatController. Báo cáo `docs/reports/C1Revision_{Stun,PlayMode,Traversal}_2026-11-05.json`.
- Toàn tuyến giữ hai gương cách5m, ray phản xạ thật và nhịp trống A–C–B; giấy không chặn giải thử. Bài traversal dùng CharacterController/API, tạm dừng AI và fixture thắng boss để cô lập đường/save; Integration kiểm boss chết/event qua health/combat thật. Sửa cuối chỉ đổi vị trí trình bày marker; không thay các hợp đồng/UI/collider đã qua Integration/Traversal.
- Probe stun dùng real damage eligibility trong khoảng choáng; chỉ bật bất tử trong giai đoạn chờ miễn choáng/fixture chém boss, nêu rõ trong report. Test RMB đặt mục tiêu đứng yên, cách An5m (nguồn đèn→aimpoint4,76m). Khoảng choáng cả hai actor không đổi HP và displacement0. Không coi các fixture là người chơi nghiệm thu cân bằng.
- EditorValidation:16checks/0failures cho Main/Preview; Metadata15meta giữ nguyên SHA; EditorLog từ trước Integration không có CS error/exception/assert/meta-write failure mới. Save chính/bak/tmp trả byte-for-byte; Integration trả settings/quality cũ. Ảnh `c1-revision-native-*` là Game View1920×1080; ảnh capture UI/world khác có kích thước riêng.
- Người dùng xác nhận A22/A23/A24 từ Sketchfab free. Chưa có URL/tác giả/giấy phép từng mẫu trong thư mục; credit ghi đúng mức nguồn đã biết, không tự gắn CC0.
- Các probe cũ Camp/Completion tìm bàn chế không còn đúng flow C1; dùng request Revision play-test/traversal-test/stun-test. Báo cáo150/569 trước revision giữ nguyên là lịch sử. Một lần bài traversal đầu dùng đường thẳng đâm vào bệ đèn cũ, đã đưa fixture về lối đi chính vòng qua bệ như tuyến trước; không xóa collider gameplay để làm phép thử đạt.
- Windows cuối: `Builds/Chapter01Revision/Windows/SonDoong.exe`, **Succeeded/0errors/0warnings**, size292.831.876bytes theo NativeBuildReport. Lượt full shader build trước marker có485cảnh báo AI Inference/Sentis; hai reportFullShaders giữ bằng chứng, lượt incremental không phát lại do cache, chưa sửa shader package. Source credit đã chép cạnh bản chơi. Chưa nghiệm thu chơi tay Windows/FPS/máy thứ hai/cân âm trên loa hoặc người chơi mới. Link nguồn/license cụ thể ba model Sketchfab còn cần ghi bổ sung trước phát hành.
- `C1Revision_Delivery_2026-11-05.json`: SHA bản chơi/scene/DLL, runtime classes hiện diện, Editor probes bị loại, source credit khớp, save cá nhân đúng hash. `C1Revision_EditorState_2026-11-05.json`: Main mở Edit Mode, không compile/dirty. EditorLog cập nhật tới sau sửa marker/build cuối:0lỗi mới.

## Ảnh kết quả

- [Ba trống A20 và cổng khép](images/c1-revision-native-three-drums-2026-11-05.png)
- [Túi và nút chế bình máu](images/c1-revision-native-inventory-ready-2026-11-05.png)
- [Gương A23](images/c1-revision-native-mirror-frames-2026-11-05.png)
- [Lửa trại](images/c1-revision-native-camp-fire-2026-11-05.png)
- [Hộ vệ cúi đầu và dấu choáng rõ ở cự ly gần](images/c1-revision-guardian-stunned-2026-11-05.png)
- [Nhật ký có giấy gợi ý và thanh cuộn](images/c1-revision-native-journal-clues-2026-11-05.png)

Không sửa script trong khi probe Play đang chạy. Luôn giữ manifest backup save; phép thử bị ngắt không phải bằng chứng nghiệm thu. Công cụ authoring chỉ nhận request tường minh và từ chối scene dirty; không tự chạy stage/apply từ các bộ dựng world cũ.
