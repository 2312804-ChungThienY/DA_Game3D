# Chỉ mục nguồn đã đọc

Đã đọc **toàn văn bốn file** ngày30/10/2026. Không sửa nội dung nguồn. Các ghi chú chỉ là cách tra cứu; khi triển khai phải quay lại đoạn nguồn tương ứng. Tài liệu trên web được dẫn bên trong spec chưa được kiểm chứng lại trong lượt đọc này.

## Nguồn và vai trò

| File | Vai trò | Phân biệt cần giữ |
| --- | --- | --- |
| [Tên_dự_án.md](../spec/Tên_dự_án.md) | Tên dự kiến, bối cảnh, cốt truyện toàn game, core loop, art/audio và gợi ý công nghệ | Tầm nhìn lớn có4 Linh Thần, móc/leo/thuyền/nỏ/parry; không phải tất cả bắt buộc12 tuần |
| [Các_kỹ_thuật_làm_game.md](../spec/Các_kỹ_thuật_làm_game.md) |9 nhóm kỹ thuật: controller,interaction,inventory,building,AI/combat,survival,quest,save,architecture | Có cả FPS/cưỡi ngựa/sinh tồn; mức áp dụng được kế hoạch§34 lọc lại |
| [Ke_hoach_game_Son_Doong_12_tuan.md](../spec/Ke_hoach_game_Son_Doong_12_tuan.md) | Thiết kếv1.0 ngày29/10/2026,39 phần, scope/logic/kiến trúc/thông số/triển khai/lịch/test/bàn giao | Tài liệu vận hành chi tiết cho chương Kim Quy; số gameplay là khởi điểm, chưa phải số liệu đo |
| [Danh_muc_asset_free_Son_Doong.md](../spec/Danh_muc_asset_free_Son_Doong.md) |40 asset A01–A40,8 công cụ T01–T08,10 phần cần tự làm M01–M10 | Ứng viên phân Chính/Thay thế/Tùy chọn, nguồn kiểm tra29/10/2026; không chứng minh đã nhập/thử/license hiện tại |

Kế hoạch có tham chiếu tên `.docx` ở D1/D2; các file thực tế đang đọc là bản `.md` trong spec. Giữ nguyên tên tiếng Việt/Unicode khi mở file bằng UTF-8.

## Tra cứu đủ39 phần kế hoạch

| Phần | Nội dung cần đọc trước công việc tương ứng |
| --- | --- |
|1| Scope bắt buộc, mở rộng có điều kiện, giới hạn sản xuất |
|2| An/Hắc Sương/Linh Thần,3 trụ cột, roadmap chương1–4/kết |
|3|5 khu, nhịp25–35 phút, rộng đường/bậc/phòng/arena/lối tắt |
|4| Tuyến nhiệm vụ, mã quest, world-state-derived objectives, thắng/thua |
|5| Kiến trúc, tách dữ liệu thiết kế/runtime/save, luật phụ thuộc |
|6| Component ownership/contracts, IInteractable/IDamageable/ILanternTarget |
|7| State/Observer/các pattern giới hạn, NewGame/Continue/Respawn, session states |
|8| Unity6.3 LTS trong thiết kế, packages, thư mục, scene/layer/Git |
|9| Prefab An, controller/camera parameters, một motor, Cinemachine3 |
|10| Input map/phím, state priority, né/i-frame, pause/UI time |
|11| Tương tác3m/LOS, lantern Reveal/Focus/seal/weaken/energy |
|12| TrốngACB, debounce, solved/reset, reward riêng, test đúng/sai |
|13|2 gương4 nấc, ray/reflect/backface,3 bounces/40m,16 tổ hợp |
|14| HP/stamina/damage, active windows, multi-collider dedup, FPS thấp |
|15| Hắc Ảnh FSM/LOS/leash/attack, NavMesh, respawn hữu hạn |
|16| Hộ vệ2 đòn, gate/checkpoint, weaken, kết chương/save |
|17|9 item definitions,12 ô/stack, Q, pickup/craft nguyên tử |
|18|4 checkpoint, snapshot, PersistentId, DTO/list/array |
|19| JSON minh họa, tmp/validate/backup/atomic replace, restore/error |
|20| Menu/HUD/prompt/inventory/journal/craft/settings/pause, Vietnamese TMP |
|21| Cave meshes/Terrain, URP lighting/fog, animation list, AudioMixer |
|22| Máy đo,60FPS1080p,median/P95,perf procedure, quality-independent logic |
|23| Bước1–4: setup/blockout/controller/session services |
|24| Bước5–9: catalog/interaction/lantern/2 puzzle/đoạn8–12 phút |
|25| Bước10–13: damage/AI/Hộ vệ/inventory/quest |
|26| Bước14–18: save sớm/art/UI/audio/profile/release |
|27|360h,300+60, work packages, milestone gates |
|28| Tuần1–4: nền/controller/save/lantern/puzzles |
|29| Tuần5–8: combat/AI/boss/items/full-state/integration0.8 |
|30| Tuần9–12: polish/freeze/perf/RC1/release |
|31| Vai tròA/B/C, thêm180h, tối đa1 mở rộng, workflow/thẻ công việc |
|32| Nỏ/móc dây có điều kiện, controls/energy/projectile/traversal acceptance |
|33| Leo/thuyền/XP/skills/NPC/chương sau, input ownership/schema migration |
|34| Ánh xạ kỹ thuật tham khảo, các hệ sinh tồn bị loại khỏi scope |
|35| CT/IN/LT/PZ/CB/AI/IT/QS/BS:16 ca chức năng |
|36| SV01–07,UI01–02,PF01,RL01; số liệu chưa đo/bàn giao |
|37| Rủi ro/symptoms, scope-cut khi trễ, tương thích asset |
|38|6 nhóm hồ sơ, demo8–10 phút, snapshot demo và build máy khác |
|39| D1/D2,S1–S8, thứ tự học công nghệ |

## Tài nguyên: các nguyên tắc đọc danh mục

- Không nhập Chính/Thay thế/Tùy chọn đồng thời chỉ để có nhiều asset. Ưu tiên một bộ nhân vật/hang/animation/phong cách thống nhất, thử ở scene riêng trước.
- “Miễn phí” không tự có nghĩa CC0. Giữ file LICENSE, tác giả, URL, phiên bản/ngày, quyền dùng, phần đã chỉnh, credit cần thiết trong Asset_Sources khi thực sự nhập.
- Quaternius **Standard** là phần miễn phí; Pro/Source hoặc mọi clip quảng cáo không tự thuộc Standard. Kiểm tra archive thật.
- CC Attribution cần credit đúng nguồn/tác giả; game-icons có tác giả theo từng icon. Asset Store/Mixamo dùng điều khoản riêng. Không coi license Blender/Audacity là license của file nhập.
- Model/VFX/UI art/audio không cung cấp sẵn quest/save/inventory/crafting/lantern/combat/AI.
- PBR phải cấu hình URP: normal map, roughness→smoothness, đóng gói map đúng; không gán ARM tùy tiện vào metallic. Texture1K/2K là điểm bắt đầu.
- Tài nguyên Việt cần chỉnh/tự làm: nhà khảo cổ An, đèn sen,3 mảnh trống, hoa văn/glyph, thảo dược, tượng Kim Quy, âm trống/nhạc cụ. Asset trung tính không tự thành hiện vật/kiến trúc Việt xác thực.

## Chỉ mục A01–A40

Các ưu tiên/quyền trong bảng sau là **thông tin ghi trong spec ngày29/10/2026**, không phải xác minh pháp lý hoặc trạng thái tải hiện tại. URL/license chi tiết nằm nguyên trong file danh mục nguồn.

| ID | Asset / vai trò | Ưu tiên và lưu ý chính |
| --- | --- | --- |
|A01|Unity First Person + Third Person Character Controllers (Starter Assets)|Chính; third-person; một motor/input; Non-standard EULA theo trang, đọc LICENSE |
|A02|Quaternius Universal Base Characters Standard|Chính; base An/rig; CC0 Standard, Source có phí |
|A03|Modular Character Outfits Fantasy Standard|Chính; chỉnh thành nhà khảo cổ, chưa có trang phục Việt hoàn thiện |
|A04|Universal Animation Library Standard|Chính; chọn clip thực có trong free, in-place/root motion phù hợp |
|A05|Universal Animation Library2 Standard|Thay thế/bổ sung; combat/parkour/interact chỉ nếu Standard có |
|A06|Mixamo|Thay thế; AdobeID/điều khoản riêng, không CC0/không phát thư viện raw |
|A07|Giant Monster Model Golem — Kevin Iglesias|Chính; Hộ vệ; rig không bảo đảm đủ2 clip đánh; tự AI/hitbox |
|A08|Quaternius Ultimate Monsters|Thay thế; Hắc Ảnh silhouette; đổi màu/sương, tự AI |
|A09|Mines and Cave Set — Loafbrr|Chính; nền/vòm/hầm, bỏ props mỏ hiện đại, thử URP |
|A10|Ultimate Modular Ruins Pack|Chính; kiến trúc trung tính, chỉnh hoa văn Việt |
|A11|Ultimate Stylized Nature Pack|Chính; giếng trời/Vườn Rêu, ít loài/cùng bảng màu |
|A12|Kenney Nature Kit|Thay thế; low-poly nhẹ, chọn một bộ chủ đạo |
|A13|Poly Haven Rock Moss Set01|Tùy chọn; scanned rock, chỉnh style,mesh/LOD/texture1–2K |
|A14|Poly Haven Mossy Rock|Chính; PBR đá rêu/Hộ vệ, roughness/smoothness/ARM |
|A15|ambientCG Ground037|Chính; nền ẩm/rêu, scale/normal/texture1–2K |
|A16|Quaternius Survival Pack|Chính; props trại trung tính, bỏ súng/đồ hiện đại; lửa từA23 |
|A17|Quaternius Ultimate RPG Pack|Chính; bình thuốc/ngọc/hòm; dữ liệu/icon/pickup tự làm |
|A18|Quaternius Modular Weapons Pack|Chính; dagger sửa màu/chuôi, socket; không sẵn hitbox |
|A19|Quaternius Ultimate Crops Pack|Chính; cây/nấm thử; lá cẩm/linh chi cuối phải chỉnh riêng |
|A20|Trống Đồng Đông Sơn — Aaannnn/Sketchfab|Chính; CC Attribution; khoảng50,8k triangles theo nguồn, giảm/tách3 mảnh, credit |
|A21|Low poly turtle — ClydeXYZ/Sketchfab|Tùy chọn;556 triangles theo nguồn, tượng tĩnh; không bằng chứng rig/clip; CC Attribution |
|A22|Wooden Crossbow — Wind astella|Tùy chọn sau gate mở rộng; CC0 .blend, tự cơ chế nỏ |
|A23|Unity Particle Pack / Starter Assets|Chính; lửa/tan/hạt, đọc Unity Companion License thực; không ghi đè settings từ demo |
|A24|Fog Particles — Game Seed Assets|Chính; particle fog/Hắc Sương, không HDRP volumetric, đo overdraw |
|A25|Free Game VFX Collection URP — Eric VFX Studio|Thay thế; chỉ Free10 prefab theo nguồn; tự logic tác động |
|A26|Simple FX Cartoon Particles — Synty|Thay thế; chuyển material URP, không nhập nhiều bộ khi đủ |
|A27|A.U.R.W. URP Free|Chính; shader nước trang trí; depth/transparency, không gameplay thuyền/buoyancy |
|A28|Kenney Fantasy UI Borders|Chính; khung PNG/9-slice, màu đồng; không sẵn inventory |
|A29|Kenney UI Pack|Thay thế; nút/slider khi A28 thiếu, đồng nhất style |
|A30|Game-icons.net|Chính; CC BY3.0, credit từng icon/tác giả; nhãn cùng icon |
|A31|Noto Sans|Chính; SIL OFL, Vietnamese subset/TMP, static TTF nếu cần |
|A32|Kenney UI Audio|Chính; CC0,2D AudioMixer UI |
|A33|Kenney Impact Sounds|Chính;130 file theo nguồn, âm va chạm tạm không phải trống đồng thật |
|A34|Fantozzis Footsteps Grass Sand and Stone|Chính;12 bước theo nguồn, Fantozzi/qubodup, event/quãng đường |
|A35|Medieval sound effects Weapon Textures|Chính; Ben Jaszczak/Brian Nelson/MedicineStorm; đây là sound, không texture ảnh |
|A36|Loopable Dungeon Ambience — JaggedStone|Chính; nền gió/nước nhỏ, CC0 |
|A37|Dark Cavern Ambient — Paul Wortmann|Thay thế; fade/loop, không chồng mọi ambience |
|A38|Deep Humidity — TinyWorlds|Chính; ambient tạm, chưa có bản sắc nhạc cụ Việt |
|A39|Ghost Monster Voice Moaning and Growling — qubodup|Chính; Hắc Ảnh, clip ngắn/giới hạn voices |
|A40|Earth Element Magic Spell — qubodup|Chính; âm cổ vật/seal tổng quát, không logic |

## Công cụ T01–T08 và phần tự làm M01–M10

| ID | Công cụ | Quy tắc |
| --- | --- | --- |
|T01|Input System|Released tương thích Editor, Gameplay/UI maps |
|T02|Cinemachine3.1|Khóa cùng Starter Assets, không API2.x |
|T03|AI Navigation|Bake sau đổi geometry, Agent Type thống nhất |
|T04|ProBuilder|Blockout/bệ, không phải bộ hang hoặc puzzle system |
|T05|Animation Rigging|Tùy chọn nếu socket/clip chưa đủ |
|T06|Shader Graph|Theo URP tương thích, rune/Hắc Sương/cổ vật |
|T07|Blender|Đèn sen/chỉnh mesh/xuất FBX, license tool khác asset |
|T08|Audacity|Cắt/loop/chuyển/mix âm có quyền dùng |

| ID | Cần tự xây/chỉnh | Phụ trách gợi ý |
| --- | --- | --- |
|M01|An cuối: trang phục/rig/socket/clips, không xuyên đồ|A |
|M02|Đèn Sen model riêng, light/VFX và logic Reveal/Focus/energy|B model; A logic |
|M03|Trống giảm mesh/tách3 mảnh/phục hồi, ID/collider/credit|B |
|M04|Gương/ngọc/bệ, raycast logic/LineRenderer, chặn/giới hạn phản xạ|A+B |
|M05|GlyphABC/hoa văn Việt, nguồn rõ/đọc ở Low/khác hình|B |
|M06|Lá cẩm/linh chi phân biệt model/icon/nhãn|B |
|M07|Tượng Kim Quy từ rùa + bệ/hoa văn/credit|B |
|M08|Âm trống/nhạc cụ Việt có quyền, hình+âm phân biệt trống|C nếu có/cả nhóm |
|M09|Quest/save/inventory/crafting/lantern/AI/combat và test/full build|A+B |
|M10|Nỏ/móc dây chỉ nếu chọn mở rộng sau tuần8, tắt không chặn tuyến|A |

## Dấu vết phiên bản nguồn

SHA-256 của bốn file đã đọc. Nếu hash khác, bản ghi nhớ có thể đã cũ; đọc lại nguồn thay đổi trước khi làm việc liên quan. Các hash không phải khóa cấm sửa spec.

```text
Tên_dự_án.md
152A34D14E145E442DDB3764E3A0713940282A264592DC1D978A9015AE74B249

Các_kỹ_thuật_làm_game.md
DCDC858B431BD7D224E2CC16A39BD65BAEBB5A148A95C35195AA6499742CB1E5

Ke_hoach_game_Son_Doong_12_tuan.md
82EAB3ACBF52467C2AFBA99A9A2563B4B9F830F5D5B7ED12EE1030527F3B050D

Danh_muc_asset_free_Son_Doong.md
68FECE9A64467B3835701DAFFC1C30C2480A4AF237F4B198EF10B706D956B9B6
```
