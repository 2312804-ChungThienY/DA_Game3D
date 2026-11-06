# Chương 2 Chim Lạc — chia cơ chế và thứ tự thực hiện

Ngày05/11/2026, theo xác nhận của người dùng: **chia việc, kiểm tra và sửa phần đã có trước**. Không dựng mới toàn bộ chương trong lượt này. Khi người dùng nói “thực hiện tiếp”, làm phần chưa hoàn thành đầu tiên bên dưới, kiểm chứng và cập nhật trạng thái; không tự mở rộng sang C3–C5. Không thay model/rig An. Chỉ sử dụng asset miễn phí đã nhập hoặc được duyệt.

Nguồn: [đặc tả C2](../spec/chapters/C2_Chim_Lac.md), kế hoạch§46/§55; API thực tế ở [CURRENT_ARCHITECTURE](CURRENT_ARCHITECTURE.md). Runtime giữ `WorldIds`, `WorldStateStore/StateSet`, `InventoryService`, schema4. Không viết thêm `ChapterContext/WorldIdCatalog` song song chỉ vì tài liệu nền tảng dùng tên cũ.

## Mạch truyện và luật giữ xuyên chương

Kim Quy thức tỉnh → An giữ Lồng Đèn Sen, Phúc Kim Quy (`movement_kit`), túi và tri thức → đi theo luồng gió tới Giếng Trời Chim Lạc → nhận Lông Chim Lạc (`ability_wind`) → leo giếng, thu ba mảnh Mặt Trời Lông Chim → tìm Lông Gió Lớn → giải thoát Chim Lạc → phục dựng → trò chuyện ba câu → nhận gợi ý **“Vành của ta đứng thứ nhất”** cho C5 → hướng tới Thành Đá Sơn Tinh.

C3 chưa có scene/runtime. Cổng C2 hiện phải thông báo chương kế tiếp chưa có; không coi màn thông báo này là đã chuyển C3 thành công. Chỉ khi được giao làm C3 mới nối thật sang `04_ThanhDaSonTinh/c3_cp_start`, kiểm save/Continue ở chương mới. Giữ dữ kiện C1/C2 để các chương sau dùng lại; không xóa tiến trình khi đổi scene.

## Những gì đã có trước lượt này

Scene `03_GiengTroiChimLac` có bản khối năm phòng đi lên cao, không phải đồ họa C1 đã hoàn thiện. Có các lớp `WindColumn`, `SequencePuzzleController/SequenceInputNode`, `QuizPedestal/QuizSequence`, `ThrowerBrain/EnemyProjectile`, `BossChimLac/ChimLacFocusReceiver`, cùng checkpoint, nhiệm vụ, cửa và cổng chuyển chương. File/prefab có sẵn không đồng nghĩa đã nghiệm thu toàn tuyến.

| Khu | Người chơi học/thực hiện | Điều kiện và kết quả giữ nguyên |
| --- | --- | --- |
| P1 Đáy Giếng Gió | Nhận Lông Chim Lạc, học nâng/lơ lửng/nhảy ra bệ, đọc bia và trả lời | Chưa có năng lực thì không bay; nhận `c2_frag_1`; `c2_cp_start` |
| P2 Vách Gió | Nối hai cột gió lên bệ cao, đọc bia, nhận mảnh | `c2_frag_2`; không thêm cưỡi chim/đu dây |
| P3 Nhà Sàn Đá Treo | Rọi phù điêu, gõ chuông **2–3–1**, lên gờ và trả lời | `c2_pz_chuong_gio` mở `c2_wind_p3_final`; `c2_frag_3`; `c2_cp_mid` |
| P4 Cầu Mây Gãy | Nhảy hai khe cầu, né quái ném, lên gờ và nhận công cụ | Bệ trao `c2_tool_long_gio_lon` và2thuốc; `c2_cp_arena`; cửa cần3mảnh+công cụ |
| P5 Đỉnh Tổ Chim | Lên cột gió khi chim đậu, Focus1,5s→choáng; xuống chém, né gió/lao | Boss200HP, choáng4s/trần60damage, HP≤100 đổi nhịp; phục dựng rồi3câu Linh Thần |

## Lượt0 — kiểm tra và sửa cơ chế cũ (đã đạt)

Không thay scene C1/C2, không đổi ID hoặc JSON schema, không thêm đạo cụ/nhiệm vụ mới.

- `ThrowerBrain`: xét khoảng cách3D và LOS khi đánh gần, tránh đánh qua tầng; sát thương không phụ thuộc renderer báo đòn tùy chọn. Đòn cận chiến có ID riêng mỗi lượt. Nếu receiver được cấu hình choáng thì hủy đòn đang chuẩn bị, ngừng đánh/ném và có nhịp hồi sau choáng; disable tắt telegraph. **Chưa tự bật choáng hoặc thêm model/sao mới trong scene C2**; probe dùng cấu hình choáng làm fixture để kiểm hợp đồng này.
- `ChimLacFocusReceiver`: chỉ cộng khi chim đang đậu trong trận, An đang lơ lửng trong cột gió bật; tránh tầm chiếu quái10m làm bỏ qua cơ chế lên cao. Pause và delta không hợp lệ không cộng tiến độ.
- `BossBase/BossChimLac`: reset trận sống xóa bất tử, trần sát thương choáng, tiến độ chiếu, FSM và các telegraph; trùm đã ghi thắng không bị ResetBoss hồi sinh. Disable dừng coroutine đang chạy.
- `WindColumn`: disable tắt visual/cờ hoạt động, enable đồng bộ lại ngay; lấy năng lực từ đúng actor vào cột.
- `WorldStateStore.RestoreVersion` và `SequencePuzzleController`: đổi snapshot hủy lượt chuông/input đang chờ; thêm dữ kiện khác vẫn giữ lượt đang thử. Version chỉ trong RAM, không đổi save schema.

Backup mã/scene/meta trước sửa: `SourceAssets/SceneBackups/2026-11-05_C2MechanicsAudit`. Công cụ chạy có chủ đích: `Chapter02MechanicsAudit` (Editor) và `Chapter02MechanicsProbe` (`UNITY_EDITOR`, không chạy trong phiên Play thường hoặc player build). Request `Library/SonDoongC2Mechanics.request.json`, action `audit` hoặc `test`. Không chạy bot C2 cũ vì nó dùng tọa độ C1 trước mở rộng và tự ghi save fixture.

**Đã đạt lượt rà soát hiện tại:** [Editor21checks](reports/C2_Mechanics_Editor_2026-11-05.json) và [PlayMode56checks](reports/C2_Mechanics_PlayMode_2026-11-05.json),0failures/0runtime errors. Trigger gió thật nâng/hover/ra cột, chuông sai/đúng/restore, bốn quiz/reward không lặp, quái ném và boss Focus/stun cap/reset đã thử. Save cá nhân trả byte-for-byte. Phép thử component dùng vị trí fixture/API và physics, không chứng nhận input tay, toàn tuyến, thời lượng10–15phút hay đồ họa cuối; chưa dựng Windows mới. [Bàn giao SHA](reports/C2_Mechanics_Delivery_2026-11-05.json) xác nhận C1/C2 scene, meta và save cá nhân giữ nguyên; [Editor cuối](reports/C2_Mechanics_EditorState_2026-11-05.json) đã dừng Play, scene sạch.

Khoảng thiếu ở mốc lượt0: điểm khi sai chưa lưu; RewardService kiểm từng item riêng; nguồn tri thức chưa xác minh. **Ba mục này đã sửa/kiểm ở lượt3 bên dưới; QuizSequence/save lỗi đã sửa/kiểm ở lượt6; cầu/AI/gust/dive/pha2 ở lượt4–5.** Cân bằng người chơi thật còn ở lượt7. Range10m/cooldown3s quái ném là cấu hình cũ, đã trả14m/2,5s ở lượt4.

## Tiến độ và các lượt tiếp theo khi được yêu cầu

**Lượt1 đã đạt ngày05/11:** sửa cổng C1 bị đá bao kín, nối lời dẫn, thêm giấy/năng lực/hướng sáng đầu C2, khung Story/Journal có cuộn. [Chi tiết và ảnh](C2_OPENING_WORKLOG.md).21kiểm Editor/74kiểm Play Mode đều đạt; cả cổng trong cảnh và nút cuối C1 đã chuyển scene/lưu/Menu/Continue thật, API/vị trí fixture và physics aim, không chơi tay toàn tuyến.

**Lượt2 đã đạt ngày05/11:** [tuyến P1–P3](C2_TRAVERSAL_WORKLOG.md),20kiểm Editor/150kiểm Play Mode,0failures/0runtime errors. Bàn phím giả qua InputReader thật: bốn lần Space rời hover và đáp đúng độ cao,40góc camera, đi tới checkpoint giữa, chuông/cột cuối, KillZone/Respawn/Continue. Tương tác qua API có kiểm tầm tay/LOS; fixture C1 đã hoàn tất, AI dừng. Giữ source motor/wind/NavMesh C2 và hai scene C1. Chưa full C2/chơi tay/Windows mới.

**Lượt3 đã đạt ngày05/11:** [chuông/bia/câu hỏi/thưởng](C2_KNOWLEDGE_WORKLOG.md),110kiểm Play Mode,0failures/0runtime errors. Dấu lông2–3–1, giấy tùy chọn, đèn/LOS của5bia, UI sai/thoát/thử lại, bốn bệ, thưởng nhiều item nguyên tử và chống gọi lồng/nhận lặp. Native save/Menu/Continue giữ cờ sai, giấy/lore/chuông, P4 đã trả lời nhưng túi đầy và P4 đã nhận đồ. Nguồn5bia/7câu được đối chiếu trực tiếp, build gate chỉ áp C2. API/nút UI và vị trí fixture/physics, không E tay/full C2 hoặc Windows mới.

**Lượt4 đã đạt ngày05/11:** [cầu/quái ném/cửa](C2_BRIDGE_WORKLOG.md),25Editor/117Play đạt,0unexpected runtime errors,1diagnostic I/O dự kiến. Ghost A08 cho4actor, choáng4s/sao/cúi đầu, không tint; đạn/LOS/melee và save lỗi cửa; E/RMB/W/Space thật qua InputReader, hai khe/hai gờ/trigger checkpoint, menu/Continue và death/Respawn. C1 completion/vị trí đầu bài là fixture, AI dừng trong bài đi cầu; chưa cân bằng/full C2/Windows. C2 dùng DisallowGPUDrivenRendering cục bộ sau lỗi BatchDrawCommand, không đổi pipeline C1.

**Lượt5 cơ chế đã kiểm chứng ngày06/11:** [đấu trường](C2_ARENA_WORKLOG.md),8Editor/87Play đạt,0errors. Pha2 lấy độ cao và hai cảnh báo/hit; wind/gust/stun/cap/dive/vật cản/pause/reset, native death/save-retry/Continue; W/RMB/Space/LMB thật chứng minh một chuỗi chiếu→xuống→đánh. Combat proxy/HUD mới, giữ An và ID/schema4. Aim/progression/damage thắng là fixture, chưa trận200HP tay/cân bằng/FPS/Windows. Chơi tay và art cuối vẫn ở nghiệm thu7.

**Lượt6 cơ chế đã kiểm chứng ngày06/11:** [phục dựng/kết C2](C2_ENDING_WORKLOG.md),7Editor/71Play đạt,0unexpected errors,2diagnostic khóa file dự kiến. Điều kiện bệ/ba câu, Esc/restore/cờ sai/điểm, save lỗi/thử lại/không lặp, lời Chim Lạc trong Journal và native menu/Continue đã thử; E thật qua InputReader. Progression/boss thắng là fixture; bản khối ở mốc lượt6 đã có cập nhật art lượt7 bên dưới, C3 chưa có.

**Lượt7 đã dựng và kiểm kỹ thuật ngày06/11:** [môi trường/âm/UI/Windows](C2_ENVIRONMENT_WORKLOG.md),10Editor/77ArtPlay; hồi quy140tuyến/123cầu/87arena/71ending,20standalone. Đủ năm khu có phần hình, giữ An/collision/ID/save; đo120frame/góc đứng yên1080p PC, chưa toàn tuyến/máy tham chiếu. Còn duyệt mỹ thuật/rig chim, nghe cân âm, cân trận tay200HP/thời lượng người mới. Không tự dựng C3 khi chưa được giao.

| Thứ tự | Phần triển khai/hoàn thiện | Điều kiện nghiệm thu |
| --- | --- | --- |
| **1 — đã đạt** | **Chuyển C1→C2 và nhịp mở đầu**: cổng thật từ Đàn Kim Quy; lời dẫn/giấy hướng dẫn năng lực mới; thẻ nhận Lông Chim Lạc; hướng sáng và mục tiêu đầu chương | Report mới21Editor/74Play,0failures/errors; giới hạn và ảnh ở worklog |
| **2 — đã đạt** | **Tuyến đi lên P1–P3**: cột gió trong suốt, dấu đáp/lối trống, camera, checkpoint giữa |20Editor/150Play; bốn lần Space/đáp thật,40góc camera và save/death/Continue; giới hạn ở worklog |
| **3 — đã đạt** | **Chuông, bia, bệ và phần thưởng**: dấu lông, giấy tùy chọn, UI quiz, giao dịch thưởng, cờ sai theo snapshot |110Play; native save/Menu/Continue với panel mới; nguồn5bia/7câu và build gate riêng C2; phạm vi ở worklog |
| **4 — đã đạt** | **Cầu gãy và Hắc Ảnh Ném**: Ghost/choáng, đạn/LOS/melee, cửa/lưu, input thật |25Editor/117Play; E/RMB/hai khe/hai gờ, save lỗi/Continue/Respawn, giới hạn ở worklog |
| **5 — cơ chế đã kiểm chứng** | **Đấu trường Chim Lạc**: đậu/gió/lao/stun/pha2, HUD, reset/death, win/save-retry/Continue |8Editor/87Play, chuỗi input thật; chơi tay cân nhịp/camera/full model còn trong nghiệm thu7 |
| **6 — cơ chế đã kiểm chứng** | **Kết C2 và chuẩn bị C3**: cổ vật phục dựng, ba câu Linh Thần, lời gợi ý và lưu; kiểm nhánh C3 chưa sẵn sàng | [7Editor/71Play](C2_ENDING_WORKLOG.md), thiếu điều kiện/save lỗi/retry/không lặp/Journal/native menu/Continue, E thật; progression/boss thắng fixture; C3 chưa dựng |
| **7 — đã dựng/kiểm kỹ thuật; còn duyệt trải nghiệm** | **Đồ họa/âm thanh/UI C2 và Windows**: năm khu, đường nối/bệ/cầu/cây, HUD/túi C2 và âm gió/chuông/nhạc riêng | [Worklog](C2_ENVIRONMENT_WORKLOG.md): input/camera/native C1→C2/Continue, chỉ đo FPS góc đứng yên. Còn hình/rig cuối, nghe cân âm và trận tay/thời lượng người mới; C3 chưa dựng |

Mỗi lượt cập nhật report và PROJECT_STATUS, giữ save cá nhân của người dùng, giữ GUID/meta. Nếu gặp lỗi chặn phần đang làm thì sửa lỗi đó trước khi chuyển phần; không đánh dấu các phần sau hoàn tất chỉ vì đã có script.
