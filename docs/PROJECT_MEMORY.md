# Ghi nhớ thiết kế hiện hành — Sơn Đoòng: Cõi Mộng Kỳ Thần

**Đồng bộ04/11, cập nhật06/11/2026, Asia/Bangkok.** Tài liệu này giữ quyết định/yêu cầu, không tự chứng nhận chức năng hoàn thành. Trạng thái thật ở [PROJECT_STATUS](PROJECT_STATUS.md); component/API/ID ở [CURRENT_ARCHITECTURE](CURRENT_ARCHITECTURE.md); khoảng thiếu ở [REQUIREMENTS_MATRIX](REQUIREMENTS_MATRIX.md); nguồn chi tiết ở [SPEC_INDEX](SPEC_INDEX.md).

## 1. Quyết định mới nhất và nguồn ưu tiên

**Phong cách mỹ thuật 06/11/2026 — quyết định mới nhất:** người dùng chọn **Mid Poly Stylized Việt Nam**, texture **1K mặc định**, **2K cho nhân vật và công trình chính**, material **URP Lit**, **LOD cho vật thể lớn**. Đây là chuẩn áp dụng cho art mới/phần được giao chỉnh, chưa phải bằng chứng asset cũ đã chuyển đổi. Bảng màu số, ngân sách polygon, ngưỡng chuyển LOD và phạm vi chuyển art C1/C2 chưa được chốt. Quyết định “3D hoạt hình low poly” ngày 04/11 và “Stylized Semi-Realistic” trong kế hoạch là các mốc trước, không dùng làm ràng buộc phong cách hiện hành; xem [ART_BIBLE](ART_BIBLE.md) và [PROJECT_STATUS](PROJECT_STATUS.md).

**Ba trống giải đố06/11/2026:** người dùng gửi `trong-ong-ong-son-viet-nam.zip`, yêu cầu thay cả ba trống tại Điện Trống C1 và ưu tiên hoa văn văn hóa Việt rõ nét. Dùng model **ndnguyen3d**, Sketchfab `21e418a517a74b49a17ebbad94ba26c0`, CC BY4.0 đã đối chiếu API nguồn. Giữ mesh/texture nguồn2K; material URP PBR và ánh sáng cục bộ, ký hiệu gameplay Mặt Trời/Cánh Chim/Sóng Nước ở bảng riêng không che hoa văn. Giữ root/ref/ID/chuỗi A–C–B, giấy/Journal, schema4; model A20 Aaannnn vẫn ở các vị trí trang trí/đàn khác. [Chi tiết](DONGSON_DRUM_WORKLOG.md).

**Chương2 ngày05–06/11/2026:** người dùng yêu cầu chia việc cơ chế Chim Lạc, kiểm tra/sửa phần đã có trước và làm từng lượt khi nói “thực hiện tiếp”. Hàng đợi [CHAPTER02_MECHANICS_PLAN](CHAPTER02_MECHANICS_PLAN.md), lượt0–7 có bằng chứng trong STATUS. Lượt6 giữ lời “Vành của ta đứng thứ nhất” trong thẻ/Journal và nhánh C3 chưa có. **Lượt7 đã dựng đồ họa năm khu, âm thanh/HUD/túi riêng C2 và development Windows**, có hồi quy input/native Continue và đo góc đứng yên; không coi đó là duyệt mỹ thuật/animation cuối, cân nhịp trận tay/nghe cân âm hoặc thời lượng người mới. Các khoảng thiếu này giữ trong STATUS. Chưa được giao triển khai C3–C5 hoặc thay model/rig An; không tự bắt đầu C3 chỉ vì xong bản dựng C2. Giữ mạch Kim Quy→Giếng Trời→Chim Lạc→gợi ý Thành Đá; fallback không phải chuyển C3 thành công.

**Hướng dẫn và bệ05/11/2026:** thêm giấy giới thiệu game và hướng dẫn chơi tại Trại Giếng Trời, thêm giấy chuẩn bị trước cổng Đàn Kim Quy. Giữ cách nhặt E/đọc lại Nhật ký, giấy tùy chọn. Bỏ tấm vuông bổ sung trên bệ đá, căn mesh/vật phẩm theo mặt bệ; bệ đèn đầu game phải nhìn thấy và tồn tại cả sau khi nhận đèn. Giữ root/ID/ref gameplay, schema4 và save cũ.

**Điều chỉnh mới nhất05/11/2026:** bỏ đổi màu/phát sáng trên thân quái khi suy yếu; **giữ các ngôi sao báo choáng** theo xác nhận tiếp theo của người dùng. Giữ suy yếu×1,5, choáng4s/2s, cúi đầu, ngừng tấn công và miễn choáng lặp. Không bỏ ngôi sao cùng hiệu ứng màu trên thân.

**Phản hồi tiếp 05/11/2026:** bỏ bàn chế C1, thêm nút chế Bình máu trong túi; không tự lưu khi chế, E lửa chỉ lưu. Lửa/khói/tàn chuyển động và ghế gỗ đủ bề ngang. Ba trống dùng A20, ký hiệu Mặt Trời/Cánh Chim/Sóng Nước thay chữ A/B/C; enum A–C–B và ID save giữ nguyên. A22 cổng, A23 gương, A24 bệ theo file người dùng mới nhập (khác số trong catalog cũ). Hai giấy gợi ý là manh mối tùy chọn, nhặt bằng E và giữ trong Nhật ký; vẫn cho tự thử. Người dùng chốt **Hắc Ảnh choáng 4s, Hộ vệ 2s**, có khoảng miễn choáng lặp; cúi đầu, dấu sao và không đánh trong choáng. Sửa hướng model theo AI, mặt mảnh trống hướng về lối tới, khe phía trên cửa; sương mỏng URP/particle, UI rõ đẹp hơn. Vẫn loại trừ model/rig An.

**Yêu cầu tiếp theo 05/11/2026:** sửa lỗi Unity `.meta` trước, sửa khe hở hang/cửa và camera theo ảnh chơi thật; hoàn thiện chương 1 gồm HUD/menu/UI, âm thanh, cửa phong ấn, đèn sen, Hắc Ảnh và Hộ vệ bằng asset đã nhập. **Đã cho phép làm quái**, thay giới hạn chờ yêu cầu trước đó; vẫn **không thay nhân vật An**. Chỉ dùng asset miễn phí, giữ hợp đồng input/state/save/ID. Được sửa phụ kiện đèn và camera liên quan lỗi ảnh.

**Bổ sung 05/11/2026:** người dùng giao dựng lại và mở rộng toàn bộ cảnh chương 1 trong `02_HangLinhMach`, dùng asset miễn phí đã nhập, làm đường nối giữa các khu. Được thay địa hình/kích thước bản khối cũ; giữ thứ tự chơi, ID/ref và save. Spawn được căn lại theo checkpoint ID trong không gian mới. Không tạo actor/quái mới. Xem [CHAPTER01_EXPANDED](CHAPTER01_EXPANDED.md); kích thước/vị trí ngày 04/11 là mốc trước mở rộng nếu khác bảng mới.

Người dùng xác nhận trong lượt đồng bộ ngày 04/11/2026:

- **Mục tiêu toàn game là 5 chương**, ưu tiên **môi trường chương 1 trước**.
- **Chưa chốt hạn mới**. Lịch 7 ngày và lịch 12 tuần trong spec giữ làm dự kiến/tham chiếu cũ, không phải deadline hiện hành.
- **Tại mốc 04/11:** đã chọn 3D hoạt hình low poly thay Stylized Semi-Realistic/bán thực tế; quyết định **Mid Poly Stylized Việt Nam ngày 06/11** ở đầu tài liệu thay mốc này. Trại Giếng Trời là khu mẫu của bản dựng lúc đó; nhân vật/quái mới chờ yêu cầu sau.
- Giữ **ba thanh Tổng/Master, Nhạc/Music, Hiệu ứng/SFX**. Không giảm yêu cầu để khớp code thiếu sau merge; cấu hình mới xem AUDIO_SETTINGS và PROJECT_STATUS.
- Giữ HUD C1 bắt đầu ở **Vườn Rêu**, tiến độ **3 mảnh/2 câu đố** theo lựa chọn 02/11. Mục tiêu suy từ world state, phân biệt giải đố và nhận thưởng B/C.
- Trại C1 có **bàn E chế riêng và checkpoint E lưu riêng**; giữ ID, tham chiếu và điểm hồi sinh khi thay hình.

Nguồn ưu tiên: **yêu cầu mới nhất của người dùng → đặc tả chương/§55 → kế hoạch §40–54 → §1–39 ở điểm chưa bị thay → ý tưởng/danh mục tham khảo**. Code/scene chứng minh hiện trạng; thiếu code không tự hủy yêu cầu. Tài liệu backlog không phải lệnh tự triển khai.

Phản hồi người dùng 02/11 được giữ theo nhật ký: độ nhạy mặc định .15, khoảng .03–.6; Focus ngang 90°, dung sai dọc±45°, phong ấn giữ 1 giây. Không ép về nón 60°/1.5 giây của spec gốc.

## 2. Game và phạm vi

**Sơn Đoòng: Cõi Mộng Kỳ Thần** (*Echoes of the Ancient Realm* trong ý tưởng): phiêu lưu hành động 3D góc nhìn thứ ba, Windows x64, chơi đơn offline, bàn phím/chuột, tiếng Việt, một độ khó.

An là nhà khảo cổ trẻ rơi vào không gian rạn nứt thời gian dưới Sơn Đoòng. Hắc Sương suy thoái linh mạch; An dùng Lồng Đèn Sen, khám phá, giải đố, thu thập cổ vật và thức tỉnh Linh Thần. Kim Quy, Chim Lạc, Sơn Tinh, Thủy Tinh là **Linh Thần trong thế giới hư cấu của game**, không phải cách phân loại lịch sử có thật.

| Chương | Vai trò/cơ chế | Mục tiêu thời lượng |
| --- | --- | --- |
| 1 Kim Quy | Năm khu đã có, đèn/trống/gương/dao; trao Phúc Kim Quy khi kết | 15–25 phút |
| 2 Chim Lạc — Giếng Trời | 5 phòng, **Cột Gió**, chuông, Hắc Ảnh Ném, Chim Lạc | 10–15 phút |
| 3 Sơn Tinh — Thành Đá | 5 phòng, **Ấn Núi** mức 0/1/2 m, Rết Đá | 10–15 phút |
| 4 Thủy Tinh — Sông Ngầm | 5 phòng, **Ngọc Nước**, van/nước, Thuồng Luồng | 10–15 phút |
| 5 Trục Linh Mạch | 6 phòng, ghép năng lực cũ, Hắc Sương Vương, thoát hang, 2 kết | 20–25 phút |

Tổng mục tiêu **65–95 phút**, phải đo người chơi mới; thời gian bot không phải thời lượng trải nghiệm. “Chỉ C1, 25–35 phút, 12 tuần” là phạm vi cũ đã được thay.

Không làm nỏ/móc dây/đu dây/cưỡi chim/bơi. Không thêm sinh tồn, xây nhà, chặt cây/đào khoáng, đói/khát/nhiệt độ, thời tiết động, multiplayer, open world hoặc nhiều công thức chỉ vì tài liệu tham khảo có liệt kê.

## 3. Tuyến chơi và khuôn chương

C1: **Trại Giếng Trời → Vườn Rêu → Điện Trống → Hành Lang Quang → Đàn Kim Quy**. Học đèn/E ở trại; mở phong ấn/nhận A; trống A→C→B/nhận B; hai gương/nhận C; đủ 3 mảnh+2 puzzle mở cổng; thắng Hộ vệ, phục dựng trống, trao movement_kit, chuyển C2. C1 có 3 bia kiến thức và **không Bệ Vấn Đáp**. HUD được chọn bắt đầu ở Vườn Rêu; hành vi nhận đèn vẫn có trong gameplay.

C2–C4: P1 học năng lực+mảnh 1; P2 thử nhỏ+mảnh 2; P3 thử lớn+mảnh 3/mốc giữa; P4 giao chiến+công cụ trùm/thuốc/mốc trước trùm; P5 đấu trường+phục dựng+Linh Thần Vấn Đáp 3 câu. “P1–P3 lấy mảnh, P4 công cụ” là **phòng trong chương**. Vượt thử thách trước khi tới bệ; câu hỏi không thay thử thách môi trường.

C5 ghép năng lực cũ, 6 phòng, Đại Vấn Đáp 5 câu, bốn vành, Hắc Sương Vương 4 pha có mốc theo pha, thoát hang 90 s. **Kết 1 Ánh Sáng Trở Lại** khi hoàn tất; **Kết 2 Ký Ức Không Tắt** khi có Vành Ký Ức từ cơ chế ẩn Bia Câm. Kết dùng ảnh tĩnh/camera ngắn/ánh sáng/âm thanh/lời dẫn; hiện vẫn là thiết kế chưa triển khai. Theo đặc tả C5, Continue sau kết về `c5_cp_altar`; EndingShrine cho xem lại và có thể tìm Vành Ký Ức để xem Kết 2 sau Kết 1.

Lối chính khởi điểm ≥2.5 m, bậc thường ≤0.3 m, phòng puzzle khoảng 12×12 m, đấu trường khoảng 18×18 m; chỉnh theo controller/camera. Mỗi khu có hướng sáng/dấu dẫn và khoảng an toàn học luật. Không kéo dài bằng đi lạc/lặp vô nghĩa.

## 4. An, di chuyển, input và camera

**Yêu cầu mới 04/11/2026:** người dùng giao chỉnh toàn bộ cơ chế nhân vật và chọn cảm giác **nhanh, gọn, ít quán tính**. Yêu cầu này cho phép sửa điều khiển/animation hiện có sau giai đoạn ưu tiên môi trường; không đồng nghĩa yêu cầu tạo model nhân vật hoặc quái mới. Thông số hỗ trợ và kết quả thử nằm trong [CharacterPolish](reports/CharacterPolish_2026-11-04.md). Sau phản hồi xoay chuột/đánh không đúng, bản sửa mới đọc Look trước hành động, chuột dọc thuận chiều và chém theo camera khi không lock-on; xem [CharacterAim](reports/CharacterAim_2026-11-04.md), không coi các checks ban đầu là đã duyệt cảm giác tay.

**Chương 1 không lộn né.** An đi/chạy/nhảy/dao/đèn/khóa mục tiêu; chạy khỏi báo trước để né đòn. Hoàn tất C1 nhận **Phúc Kim Quy**, mở `movement_kit` vĩnh viễn cho C2–C5.

| Thông số khởi điểm | Giá trị |
| --- | --- |
| Capsule/step/slope | Cao 1.8 m, radius 0.3 m, tâm Y 0.9 m; step 0.3 m; slope 45° |
| Đi/chạy/Focus | 3.5/5.5/2 m/s |
| Nhảy/gravity | 1.2 m/−20 m/s² |
| Lộn | 25 stamina, 0.45 s, bất tử 0.10–0.30 s, khoảng bắt đầu ≥0.65 s |
| Shift+lộn; lộn+chém | Xa khoảng×1.5; đâm tới×1.25, vẫn tiêu 10 stamina |
| Né đúng lúc | .15 s đầu khoảng bất tử: chậm .3 s, hồi 15 năng lượng đèn |
| Khe thấp | Capsule khoảng 0.9 m; không đứng dậy xuyên trần |
| Camera | Cinemachine 3 Third Person Follow; pitch −35°..65°, radius tránh vật 0.2 m, near clip khoảng 0.1 m |

Một motor sở hữu dịch chuyển An; root motion không tranh quyền. Lộn có hướng tính theo camera, không hướng theo hướng An nhìn, vẫn va tường. Khóa mục tiêu chuột giữa từ C1, tầm 15 m, bỏ khi chết/xa/che>1 s. Animation làm một bộ dùng lại; clip tự tạo/model tạm không được gọi là chất lượng cuối khi chưa xem/chơi thử.

Một asset `PlayerControls.inputactions`, map **Gameplay/UI**, đọc qua InputReader. WASD/chuột; Shift chạy; Space nhảy; Ctrl lộn sau mở khóa; LMB đánh; F đèn; RMB Focus; E tương tác; Q thuốc; MMB lock-on; I túi; J nhật ký; Esc pause/back; F9 debug Editor/Development. Khóa phiên/pause/dead không để input giữ hoặc clickUI biến thành lệnh gameplay. Giữ ưu tiên trạng thái; code thiếu guard được ghi vào MATRIX.

Người dùng xác nhận tiếp 04/11 lỗi gồm **cả hướng và động tác**. Đã bổ sung hai nhát chém, lunge riêng và dao gắn tay trên rig hiện có; xem [MeleeVisual](reports/MeleeVisual_2026-11-04.md). Việc này không chứng nhận model An cuối hoặc cảm giác chơi tay đã được duyệt.

## 5. Tương tác, đèn, puzzle và chiến đấu

E chọn collider hợp lệ đầu từ tâm camera, đo tầm tay từ An/InteractionOrigin≤3 m, kiểm LOS từ nhân vật và CanInteract lúc thực hiện. Một prompt; không vừa trigger tự nhặt vừa E cấp cùng vật. IInteractRange chỉ cho đối tượng có thiết kế tầm riêng.

Đèn thường miễn năng lượng, reveal 5 m có LOS. Theo phản hồi 04/11 muốn chiếu quái xa hơn, hiện cấu hình **Focus quái 10 m**, puzzle/phong ấn vẫn **8 m**, ngang 90°/dọc±45° theo hướng camera để chiếu cao. Phong ấn giữ 1 s, mất chiếu>.25 s reset. Địch Focus 1 s→yếu×1.5 trong 4 s; chiếu tiếp làm mới, không cộng dồn/choáng vô hạn. Năng lượng 100, tiêu 12/s, nghỉ 1 s hồi 20/s. Pause không cộng tiến độ; logic độc lập Light/VFX/quality.

Trống `pz_trong_01`: A→C→B, debounce.2 s, sai phản hồi/reset lượt, solved/apply lại an toàn, nhận B riêng. Không gọi chuỗi gõ hư cấu là nhạc cổ xác thực. Gương `pz_guong_01`: 2 gương×4 nấc, ngọc 1 s; ray chặn tường/mặt lưng, giới hạn 3 phản xạ/40 m, thử 16 tổhợp. SequencePuzzleController chung cho chuông C2/bốn vành C5, không thay trống C1 chỉ để đồng nhất tên.

HP khởi điểm: An 100, Hắc Ảnh 60, Hộ vệ 180. Dao 20 damage/10 stamina/.65 s, active.15–.30, tầm khoảng 1.8 m; nhiều collider một actor chỉ trúng 1 lần. Phải lọc team/LOS/tường và xét giao cửa sổ active khi FPS thấp; đã bổ sung kiểm LOS/tường trong lượt điều khiển 04/11, có kiểm tra Play Mode; chưa thay bằng cân bằng chiến đấu toàn tuyến. Quái thường dùng NavMeshAgent/FSM/LOS/leash; checkpoint trùm an toàn; chết restore toàn snapshot, thắng ghi ID duy nhất và không sinh lại sau Continue. Số HP/nhịp đòn phải cân bằng bằng người thật.

**Phản hồi mới 04/11:** chuột trái không được trông như tia chiếu vào quái. Đã tắt ribbon dao, LMB ngắt Focus đang giữ và cần thả/giữ lại RMB để chiếu tiếp; xem [FocusCombat](reports/FocusCombat_2026-11-04.md). Các ảnh/report melee có vệt sáng bên dưới là phiên bản trước phản hồi này.

## 6. Túi, chế thuốc và reward

Túi 12 ô, Lá cẩm/Linh chi stack 10, thuốc stack 5; dao/đèn/mảnh/cổ vật không chiếm ô. Q hồi 35, cooldown 2 s, không tiêu khi đầyHP/chết/thiếu thuốc. TryAdd báo lượng nhận, pickup chỉ mất khi nhận đủ. Craft thử bản nháp, trừ/thêm cùng giao dịch, thất bại giữ túi; ô vừa trống do tiêu nguyên liệu dùng được cho thuốc.

**Trại C1 — thay đổi 05/11:** bỏ bàn chế; mở túi I và dùng nút Chế bình máu, 1 Lá cẩm+1 Linh chi→1 Bình máu. Giao dịch InventoryService, không tự lưu; E lửa chỉ lưu. C2 hiện còn panel/altar có flow riêng; phải mô tả tách biệt. Reward/quiz/pickup/ability không trao lại khi replay/restore; reward nhiều item cũng phải nguyên tử, không suy mọi reward đạt chỉ từ TryCraft.

## 7. State, ID, save và chuyển chương

Tách SO design/runtime state/DTO thuần. Quest/Điểm Tri Thức suy từ dữ kiện, không tăng counter theo event có thể phát lại; subscribe/unsubscribe đúng vòng đời; apply state lặp an toàn.

Thiết kế cần catalogID/validator. Hiện dùng **WorldIds tĩnh**, StateSet/WorldStateStore theo tập; native **schema 4**, đọc **legacy version 1–5**. World/Inventory theo scene, chuyển bằng snapshot, chưa ở AppRoot; int-state C3–C5 còn thiếu. Không dùng API PlayerInventory/save v5 prototype làm hợp đồng hiện tại.

Mảnh C1 hiện `relic_drum_a/b/c`; yêu cầu C1-R11 chuẩn `fragment_*` chưa làm. Chỉ đổi cùng migration/catalog/ref, không đổi lẻ ID đang có save. Mốc C1: `cp_trai`, `cp_dien_trong`, `cp_truoc_dan`, `cp_kim_quy`; phục dựng Puzzles `c1_relic_restored`. Mốc C2: `c2_cp_start`, `c2_cp_mid`, `c2_cp_arena`; C3–C5 theo spec, chưa đăng ký runtime.

Snapshot đầy đủ: chương/scene/mốc, túi, năng lực, mảnh/công cụ, puzzle/cửa/seal/pickup, lore/quiz, trùm, tiến trình/ending và int-state đất/nước khi triển khai. Tải thay toàn bộ, reset trạng thái tạm, đặt actor tại mốc an toàn, hồi đầy chỉ số, cuối cùng bật input/AI. Chết không giữ đồ kiếm sau save.

Một writer main thread: capture→tạm cùng thư mục→flush→đọc validate→replace/backup. Lỗi giữ bản cũ, chỉ báo lưu/hoàn thành khi ghi thành công. NewGame xác nhận khi có save và chỉ thay ô cũ sau khởi tạo mới an toàn. File hiện `save_v1.json`+`.bak`/`.tmp`; tên file không phải schema. Native validation, NewGame và RelicAltar còn gap bảo toàn dữ liệu, không tự gọi các hợp đồng này hoàn thành.

## 8. Tri thức và nội dung

Nhãn **Lịch sử/Truyền thuyết/Tự nhiên** trên từng bia/câu. Không hỏi Hắc Sương/Linh Thần/chuỗi gõ hư cấu như sự thật. C2 dễ, C3–C4 khó dần, C5 tổng hợp/sắp xếp; đáp án tìm được từ bia. Sai có giải thích/thử lại/đủ đồ. Điểm suy từ QuizFirstTry; luật đúng lần đầu phải giữ qua save/restart, không chỉ RAM.

Nội dung đối chiếu nguồn tin cậy trước build, có hồ sơ/sourceRef và validator. `verified:1` hiện có không tự chứng minh KN01; nhật ký ghi nhiều mục chỉ đối chiếu kiến thức AI. Lượt đồng bộ này không xác minh web từng bia/câu.

## 9. Môi trường, UI/audio và asset

**Yêu cầu tiếp theo 04/11/2026 (mốc art trước đổi phong cách 06/11):** dựng lại bối cảnh **toàn bộ năm khu C1** theo năm ảnh người dùng gửi; sửa đá/cỏ lệch tiếp xúc; chỉ dùng asset miễn phí, cần bộ nào thì tìm nguồn miễn phí và báo người dùng nhập. Bản dựng khi đó theo low poly đã chọn. A09/A11/A16 và cột A10 có trên đĩa đủ cho lượt này; có thể dựng mesh trang trí riêng, không cần tải bộ trả phí. Không đổi tuyến/puzzle/ID/save hoặc làm model actor mới. Xem [CHAPTER01_ENVIRONMENT](CHAPTER01_ENVIRONMENT.md) và STATUS để phân biệt ảnh tham chiếu, art đã ghép và kiểm thử.

Trại mẫu từ cửa tối ra vùng nắng: lửa trái, bàn giữa, lối Vườn Rêu phải. Dựng **92_EnvironmentPreview**, chuyển **ART_Camp** vào **02_HangLinhMach/World_Geometry/ENV_01_Camp** sau kiểm tra. Đá lớn→cây→đồ nhỏ; xem từ spawn/cạnh bàn/cửa bằng camera thấp. Giữ collider nền/tuyến 20×20 m và hành lang 4×10 m/lòng 3 m hiện có; đây không phải kích thước hang thật.

Bộ chính A09 Mines and Cave Set, A11 Ultimate Stylized Nature Pack, A16 Survival Pack. A10 dành di tích sau, A12 thay thế, A14 chỉ dùng khi được chọn. Mẫu hư cấu lấy cảm hứng đá vôi/hố sụt/cây sâu trong hang. Dùng wrapper/variant/material dự án, không sửa source nếu tránh được; GUID/.meta đi cùng asset. Model dưới object chứa script, giữ ref/ID/spawn. Ghi nguồn/license/credit/khác biệt bản phân phối; bảng asset không chứng minh đã tải/đạt.

UI Việt/TMP/fallback đủ dấu, một EventSystem/UI input, Esc theo lớp panel. Giữ HUD Vườn Rêu/progress 3/2 và 3 slider Master/Music/SFX; routing còn định hướng Ambience/UI, ambience 3D/UI2D, fade, bước chân theo animation/quãng đường. Code hiện thiếu nhiều phần audio/HUD sau merge, xem MATRIX.

URP: vòm mesh ghép, không tạo cả hang bằng heightmap; baked light/probe cho tĩnh, ít đèn động sau đo, fog/particle kiểm overdraw. Không mặc nhiên có volumetric fog của HDRP. Trại mới chỉ cấu hình đèn/probe thử, chưa bake GI hoặc đo FPS.

## 10. Kiểm chứng và tiếp nối

- Mục tiêu 60 FPS ở 1080p trên máy tham chiếu chưa chốt; cần median/P95, scene cycles/memory/save-load, Low/Medium vẫn đọc gameplay. Không dùng ảnh Editor/bot làm benchmark.
- Camp mới có 53 checks Play Mode API/controller và 115 mẫu khoảng trống; không thay chơi tay/toàn C1–C2/build. Report Windows 02/11 và bot nhánh cũ phải giữ nhãn phiên bản/phạm vi.
- Đọc lại đĩa trước sửa, giữ thay đổi ngoài công việc. Đợt đồng bộ tài liệu ban đầu không sửa gameplay; các lượt điều khiển/art sau đó có phạm vi và kiểm chứng riêng trong STATUS. Không tự làm C3–C5/actor/quái/sửa backlog vì audit liệt kê thiếu.
- Sau thay đổi đáng kể cập nhật STATUS bằng file/bằng chứng/ngày/việc còn lại; quyết định user mới cập nhật MEMORY/MATRIX. Không biến nhận xét audit thành yêu cầu mới.

Bản trước đồng bộ ở [history](history/PROJECT_MEMORY_before_sync_2026-11-04.md). Spec giữ nguồn theo ngày để truy nguyên, ưu tiên mới ghi tại đây và SPEC_INDEX.
