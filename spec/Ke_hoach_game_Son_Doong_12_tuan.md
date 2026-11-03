Kế hoạch xây dựng game Sơn Đoòng Cõi Mộng Kỳ Thần

Mô tả hệ thống và hướng dẫn triển khai Unity 3D trong 12 tuần

Mục tiêu của dự án là hoàn thành một chương chơi có mở đầu, thử thách và kết thúc rõ ràng. Người chơi điều khiển An khám phá hang động, dùng Lồng Đèn Sen để tìm dấu tích, giải hai câu đố cổ vật và đánh bại Hộ vệ Đá để thức tỉnh Thần Kim Quy.

Kế hoạch dành cho **nhóm 2–3 người trong 12 tuần**. Bản chung cho cả nhóm tập trung vào một khu hang động gồm năm không gian liên kết, thời lượng trải nghiệm mục tiêu **25–35 phút**. Nỏ thần và móc dây tại điểm cố định được bố trí thành các phần mở rộng có điều kiện, để nhóm có thể bổ sung khi hệ thống chính đã ổn định.

## Kết quả cần bàn giao

Bản game Windows 64 bit có thể chơi từ menu đến màn kết thúc, có tạm dừng, cài đặt và tiếp tục từ mốc lưu.

Dự án Unity đầy đủ Assets, Packages và ProjectSettings; tài nguyên có nguồn và hướng dẫn mở đúng phiên bản.

Màn chơi sử dụng được lồng đèn, hai câu đố, dao găm, né tránh, AI, vật phẩm, chế thuốc và nhiệm vụ.

Hồ sơ thiết kế, bảng phân công, kết quả kiểm thử, số liệu hiệu năng và kịch bản trình diễn.

## Cách dùng tài liệu

Đọc phần phạm vi trước khi lập bảng công việc. Dùng phần hệ thống để thống nhất luật chơi và trách nhiệm của từng script. Làm theo phần triển khai trong Unity theo đúng thứ tự phụ thuộc. Cuối mỗi tuần, chạy bản build và đối chiếu điều kiện nghiệm thu trước khi chuyển mốc.

**Ngày lập kế hoạch:** 29/10/2026. **Phiên bản thiết kế:** 1.0. **Nền tảng:** Windows, chơi đơn, ngoại tuyến, bàn phím và chuột. **Thể loại:** phiêu lưu hành động 3D góc nhìn thứ ba. **Đồ họa:** URP, tạo hình bán thực tế có màu sắc huyền ảo.

# Nội dung và cách tra cứu

- Phạm vi dự án và bản bắt buộc (trang 3)
- Ý tưởng màn chơi và tuyến nhiệm vụ (trang 4)
- Kiến trúc thành phần và mẫu thiết kế (trang 7)
- Cấu trúc dự án và thiết lập Unity (trang 10)
- Nhân vật camera input và lồng đèn (trang 11)
- Hai câu đố môi trường (trang 14)
- Chiến đấu AI và Hộ vệ Đá (trang 16)
- Vật phẩm túi đồ và chế thuốc (trang 19)
- Lưu tải và hồi sinh (trang 20)
- UI nghệ thuật âm thanh và hiệu năng (trang 22)
- Các bước triển khai chi tiết trong Unity (trang 25)
- Nguồn lực lịch 12 tuần và phân công (trang 29)
- Nỏ móc dây và lộ trình toàn game (trang 34)
- Đối chiếu các kỹ thuật nguồn (trang 36)
- Kiểm thử chức năng dữ liệu và bản dựng (trang 37)
- Rủi ro và xử lý lỗi (trang 39)
- Bàn giao và kịch bản báo cáo (trang 40)
- Nguồn tham khảo và hướng học (trang 41)

## Các khái niệm dùng trong kế hoạch

**Chương chơi hoàn chỉnh** là một phần game có thể trải nghiệm từ đầu đến cuối, có chất lượng đại diện cho game dự kiến. **Bản dựng thử bằng khối** là màn chơi làm từ hình hộp và vật liệu đơn giản để kiểm tra đường đi trước khi làm đẹp.

**Prefab** là mẫu đối tượng có thể tái sử dụng. **ScriptableObject** chứa dữ liệu thiết kế như chỉ số quái hoặc công thức chế tạo. **Trạng thái lúc chơi** là dữ liệu thay đổi khi chạy game; dữ liệu này được lưu riêng, không ghi trực tiếp vào tài sản ScriptableObject.

**Điều kiện nghiệm thu** là thao tác hoặc kết quả có thể kiểm tra để xác nhận một công việc đã xong. Một chức năng chỉ được tính hoàn thành khi chạy được trong bản build, có cấu hình hợp lệ và không làm hỏng các chức năng đã tích hợp.

# 1 Phạm vi dự án trong 12 tuần

## Bản bắt buộc cho nhóm 2–3 người

Giữ một chương thức tỉnh Kim Quy với một tuyến chính. Màn chơi có năm không gian: Trại Giếng Trời, Vườn Rêu, Điện Trống, Hành Lang Quang và Đàn Kim Quy. Chỉ chuyển sang bổ sung nội dung sau khi toàn tuyến đã đi được từ đầu đến cuối bằng đồ họa đơn giản.

| **Hạng mục** | **Số lượng và giới hạn** | **Điều kiện hoàn thành** |
| --- | --- | --- |
| Nội dung | 1 chương, 5 không gian, mục tiêu 25–35 phút | Có mở đầu, hướng dẫn, cao trào và kết thúc |
| Nhân vật | 1 nhân vật An, đi, chạy, nhảy, né | Điều khiển ổn định, không xuyên tường |
| Lồng đèn | Hiện ký tự, xóa phong ấn, làm yếu địch | Luật ánh sáng có kiểm tra vật cản |
| Giải đố | 1 câu đố trống, 1 câu đố gương | Có gợi ý, làm lại và lưu trạng thái |
| Chiến đấu | Dao găm, 1 loại Hắc Ảnh, 1 Hộ vệ Đá | Đòn đánh có báo trước, sát thương đúng |
| Tiến trình | 3 mảnh trống, 1 tuyến nhiệm vụ | Không nhặt trùng, không kẹt tiến độ |
| Hỗ trợ | Túi 12 ô, 1 công thức thuốc, lưu và tải | Giao dịch vật phẩm và dữ liệu nhất quán |
| Hoàn thiện | Menu, HUD, âm thanh, tối ưu, hồ sơ | Có build bàn giao và bằng chứng kiểm thử |

## Phần mở rộng có điều kiện

Nỏ thần, móc dây tại điểm cố định và một nhánh khám phá phụ chỉ được đưa vào bản nộp khi tuyến chính đạt mốc tích hợp tuần 8, còn đủ giờ kiểm thử và không còn lỗi chặn hoàn thành màn. Đường đi bắt buộc luôn có thể đi được bằng bộ cơ chế chính.

## Giới hạn sản xuất

Chơi đơn, một ngôn ngữ tiếng Việt, một ô lưu chính và một bản sao lưu. Dùng mô hình và hoạt ảnh có sẵn phù hợp URP. Chưa đưa thế giới mở, nhiều người chơi, cưỡi ngựa, xây nhà, trồng trọt hoặc cả bốn vùng Linh Thần vào bản bắt buộc.

# 2 Ý tưởng và toàn cảnh game

## Cốt truyện và mục tiêu của An

An là một nhà khảo cổ trẻ rơi vào vùng không gian bị rạn nứt bên dưới Sơn Đoòng. Hắc Sương làm linh mạch suy yếu và khiến các hộ vệ cổ mất kiểm soát. An tìm thấy Lồng Đèn Sen và những mảnh Trống Đồng có thể nối lại liên hệ với các thực thể hộ mạch.

Trong chương đầu, An phải tìm ba mảnh trống, khôi phục Đàn Kim Quy và mở lối rời khỏi vùng hang bị phong kín. Câu chuyện được kể bằng không gian, ba văn tự ngắn và các dòng nhiệm vụ. Mở đầu dùng ảnh tĩnh hoặc camera ngắn; kết thúc dùng ánh sáng, âm thanh và lời dẫn để phù hợp nguồn lực.

**Định hướng hư cấu:** kiến trúc và biểu tượng lấy cảm hứng từ văn hóa Việt Nam. Kim Quy, Chim Lạc, Sơn Tinh và Thủy Tinh được gọi chung là các Linh Thần trong thế giới game. Đây là cách tổ chức cốt truyện của dự án, không phải một hệ thống phân loại lịch sử.

## Ba nguyên tắc thiết kế

Ánh sáng phải giúp người chơi hiểu đường đi và cách giải quyết thử thách. Một hiệu ứng quan trọng luôn đi cùng thay đổi luật chơi có thể quan sát.

Câu đố cung cấp đủ dấu hiệu trong môi trường. Ký hiệu trên trống hoặc hướng gương cần rõ trước khi yêu cầu người chơi suy luận.

Chiến đấu ưu tiên đọc đòn và chọn thời điểm. Địch báo trước, người chơi né hoặc chiếu đèn rồi phản công; tránh tăng độ khó chỉ bằng tăng máu.

## Bản đồ phát triển toàn game

| **Chương** | **Linh Thần và chủ đề** | **Cơ chế nổi bật dự kiến** |
| --- | --- | --- |
| 1 | Kim Quy và nền móng linh mạch | Lồng đèn, trống, gương, dao găm |
| 2 | Chim Lạc và các giếng trời | Móc dây, leo bậc, nỏ, đường đi trên cao |
| 3 | Sơn Tinh và thành đá | Trọng lực, cơ cấu đá, sức mạnh địa hình |
| 4 | Thủy Tinh và sông ngầm | Thuyền, mực nước, cửa van, chiến đấu ven nước |
| Kết | Phục hồi trục linh mạch | Kết hợp các năng lực và giải quyết Hắc Sương |

Trong 12 tuần, hoàn thiện chương 1 và chuẩn bị các điểm nối kỹ thuật cho chương sau. Mỗi chương bổ sung phải đi qua dựng thử, kiểm tra cơ chế, làm nội dung và kiểm thử riêng.

# 3 Thiết kế màn chơi và nhịp trải nghiệm

Sơ đồ năm không gian của chương Kim Quy và đường quay lại trại

| **Không gian** | **Trải nghiệm chính** | **Thời lượng mục tiêu** |
| --- | --- | --- |
| Trại Giếng Trời | Nhận đèn, học di chuyển, kích hoạt mốc lưu | 3–4 phút |
| Vườn Rêu | Rọi văn tự, xóa phong ấn, lấy mảnh A | 4–6 phút |
| Điện Trống | Hiểu ký hiệu, gõ trình tự, lấy mảnh B | 5–7 phút |
| Hành Lang Quang | Xoay gương, giao chiến ngắn, lấy mảnh C | 6–8 phút |
| Đàn Kim Quy | Hộ vệ Đá, phục dựng trống, kết thúc chương | 7–10 phút |

Các khoảng trên là mục tiêu thiết kế cần kiểm tra bằng người chơi mới. Tổng dự kiến 25–35 phút; không cộng thêm thời gian cố ý đi lạc hoặc lặp lại để kéo dài trải nghiệm.

## Quy tắc bố trí

Dùng một dấu hiệu sáng dẫn hướng từ mỗi phòng đến mục tiêu kế tiếp. Đặt vật tương tác gần tầm mắt, tạo khu vực an toàn trước khi giới thiệu một luật mới và mở lối tắt về trại sau Điện Trống. Hộ vệ Đá chỉ xuất hiện sau khi người chơi đã được thử chiếu đèn lên Hắc Ảnh.

Kích thước bắt đầu: lối chính rộng tối thiểu 2,5 m, bậc đi thường không cao hơn 0,3 m, phòng giải đố khoảng 12 × 12 m, đấu trường khoảng 18 × 18 m. Điều chỉnh sau khi đo bán kính nhân vật và NavMesh. Các vực không phục vụ thử thách phải có vách hoặc rào dễ nhận biết.

# 4 Luồng chơi và tiến trình nhiệm vụ

## Tuyến chính

1. An thức dậy tại trại; hướng dẫn đi, nhìn, nhảy và tương tác. Nhận đèn và dao găm qua một lần tương tác có kiểm soát.

2. Rọi dấu tích tại Vườn Rêu để thấy lời gợi ý. Giữ chiếu tập trung vào phong ấn để mở lối và nhận mảnh A.

3. Tới Điện Trống, đọc ba ký hiệu và gõ đúng thứ tự. Cửa đá mở, bệ hiện vật cho nhận mảnh B.

4. Gặp Hắc Ảnh trong đoạn giao chiến ngắn. Thử né, chiếu đèn và phản công trước khi vào Hành Lang Quang.

5. Xoay hai gương để ánh sáng đến ngọc thạch. Bệ vật phẩm mở cho nhận mảnh C; kích hoạt mốc lưu trước đấu trường.

6. Cổng Đàn Kim Quy kiểm tra đủ ba mảnh và hai câu đố đã giải. Đánh bại Hộ vệ Đá, tương tác bệ trống để phục dựng và xem kết thúc.

## Nhiệm vụ lấy dữ kiện thế giới làm nguồn chính

QuestSystem suy ra mục tiêu từ tập mảnh đã nhận, trạng thái câu đố và cờ Hộ vệ bị đánh bại. Không tăng một biến đếm nhiệm vụ mỗi lần nhận event rồi lưu biến đó độc lập; event có thể được gửi lại khi tải cảnh và gây đếm trùng.

| **Mã mục tiêu** | **Điều kiện đạt** | **Phản hồi cho người chơi** |
| --- | --- | --- |
| q_get_lantern | equipment_lantern đã được cấp | Hiện cách bật đèn và rọi dấu tích |
| q_fragment_a | fragment_a trong tập đã nhận | Chuyển hướng sang Điện Trống |
| q_drum | pz_trong_01 đã giải và có fragment_b | Mở lối tắt, hướng sang Hành Lang Quang |
| q_light | pz_guong_01 đã giải và có fragment_c | Thông báo đã đủ ba mảnh |
| q_guardian | guardian_01 đã bị đánh bại | Cho phép phục dựng tại bệ trống |
| q_restore | relicRestored bằng true | Khóa nhận thưởng lần hai, mở màn kết |

**Điều kiện thắng chương:** đủ ba mảnh, hai câu đố đã giải, Hộ vệ đã bị đánh bại và người chơi đã kích hoạt bệ trống. **Điều kiện thua:** máu về 0 hoặc rơi vào vùng chết; tải lại ảnh chụp trạng thái ở mốc lưu gần nhất.

# 5 Kiến trúc hệ thống

Quan hệ giữa dữ liệu thiết kế các hệ thống lúc chơi và dữ liệu lưu

## Phân chia trách nhiệm

GameBootstrap tạo các dịch vụ dùng chung một lần. GameSession quản lý phiên chơi. Mỗi cơ chế ở nhân vật, địch và môi trường xử lý phần việc của mình; giao diện đọc dữ liệu hoặc nhận event để cập nhật hiển thị.

Các dữ liệu thiết kế như ItemDefinition, EnemyDefinition, PlayerStats và RecipeDefinition là ScriptableObject. Chỉ số hiện tại như máu, năng lượng, số lượng vật phẩm và câu đố đã giải nằm trong các đối tượng trạng thái lúc chạy. File lưu chỉ chứa dữ liệu thuần và mã định danh.

## Luật phụ thuộc

UI gửi yêu cầu qua phương thức như TryCraft hoặc TryUsePotion; UI không tự trừ vật phẩm hay trừ máu.

Combat chỉ gọi IDamageable và đọc thông tin đội. Hệ nhiệm vụ nhận sự kiện kết quả, không can thiệp trực tiếp vào hitbox.

Các object môi trường có PersistentId; trạng thái đã giải hoặc đã nhận nằm trong WorldStateStore.

SaveService thu thập một ảnh chụp trạng thái nhất quán. Việc tải xong mới cho phép AI và input hoạt động.

Không cần tạo event bus tổng quát, hệ tiêm phụ thuộc lớn hoặc hàng chục Singleton. Tham chiếu trong Inspector và các event có kiểu rõ ràng đủ cho quy mô một chương.

# 6 Các thành phần và hợp đồng xử lý

| **Thành phần** | **Dữ liệu sở hữu** | **Trách nhiệm hoặc giao tiếp chính** |
| --- | --- | --- |
| GameSession | Trạng thái phiên và mốc lưu hiện hành | NewGame, Continue, Pause, Respawn |
| InputReader | Giá trị action và map đang bật | Chuyển Gameplay hoặc UI; xóa input khi khóa |
| PlayerMotor | Vận tốc, trạng thái di chuyển | Move, Jump, Dodge; một nơi điều khiển vị trí |
| PlayerVitals | Máu và thể lực hiện tại | Nhận sát thương, tiêu hao, hồi phục |
| InteractionScanner | Mục tiêu tương tác đang nhìn | Tìm mục tiêu gần và còn nhìn thấy |
| LanternController | Năng lượng và tiến độ chiếu | Reveal, Focus, gửi tác động lên ILanternTarget |
| CombatController | Đòn hiện hành và tập đã trúng | Mở cửa sổ sát thương, hủy đòn đúng cách |
| EnemyBrain | Trạng thái AI, mục tiêu, điểm sinh | Chọn trạng thái; điều khiển NavMeshAgent |
| InventoryService | Danh sách ô, lượng vật phẩm | TryAdd, TryRemove, CanAdd; không số âm |
| CraftingService | Công thức được chọn | Kiểm tra và thực hiện giao dịch chế thuốc |
| PuzzleController | Trạng thái một câu đố | TryInput, ResetAttempt, ApplySolvedState |
| WorldStateStore | Các tập ID và cờ tiến trình | Ghi nhận một lần, kiểm tra trước khi phát thưởng |
| QuestSystem | Mục tiêu suy ra từ dữ kiện | Tính bước hiện tại và thông báo thay đổi |
| SaveService | Phiên bản file và yêu cầu lưu | Capture, Validate, Write, Restore |
| AudioService | Mixer và nguồn phát âm | SFX, ambience, nhạc theo ngữ cảnh |

## Các interface cần thiết

IInteractable: trả lời có thể tương tác hay không, nội dung nhắc và thao tác tương tác. IDamageable: nhận DamageInfo chứa nguồn, đội, sát thương và mã đòn. ILanternTarget: nhận chiếu sáng có thời lượng, kết thúc chiếu và phản hồi tiến độ.

ISaveParticipant chỉ dùng khi có nhiều loại dữ liệu cần đóng góp vào ảnh chụp; với chương đầu có thể để SaveService gọi trực tiếp các dịch vụ đã biết. Đăng ký event trong OnEnable, hủy trong OnDisable; đối tượng bị hủy không tiếp tục phát thưởng hay gọi UI.

# 7 Mẫu thiết kế và vòng đời phiên chơi

## Mẫu được áp dụng

| **Mẫu** | **Nơi áp dụng** | **Lợi ích và giới hạn** |
| --- | --- | --- |
| State | Nhân vật và EnemyBrain | Quy tắc chuyển trạng thái rõ, tránh nhiều cờ mâu thuẫn |
| Observer | Máu, nhiệm vụ, inventory, puzzle | UI cập nhật theo thay đổi; cần hủy đăng ký đúng vòng đời |
| Strategy đơn giản | Kiểu đòn và phản ứng với đèn | Dùng cấu hình hoặc component riêng, chỉ tách khi có khác biệt thực |
| Factory giới hạn | Tạo địch hoặc projectile | Một chỗ cấu hình đối tượng; chưa cần framework factory |
| Object Pool | Projectile hoặc VFX lặp nhiều | Chỉ thêm sau đo đạc; reset sạch dữ liệu khi lấy lại |
| Dữ liệu tách hành vi | ScriptableObject và runtime state | Chỉnh thông số không sửa code, tránh sửa nhầm dữ liệu gốc |

## Bắt đầu phiên mới

Boot tạo AppRoot một lần rồi mở MainMenu. Chọn Chơi mới tạo trạng thái mặc định, vào HangLinhMach, áp dụng trạng thái ban đầu rồi bật input. Nếu có ô lưu cũ, menu giải thích rằng Chơi mới sẽ thay ô lưu khi xác nhận.

## Tiếp tục và hồi sinh

Continue xác thực file trước khi chuyển cảnh. Khi cảnh đã sẵn sàng, các đối tượng đăng ký ID; sau đó áp dụng vật phẩm đã nhận, cửa đã mở, câu đố đã giải và địch độc nhất đã bị đánh bại. Cuối cùng đặt An ở mốc lưu, cập nhật HUD và bật mô phỏng.

Hồi sinh tải lại cùng loại ảnh chụp như Continue. Không giữ lại vật phẩm vừa nhặt sau lần lưu rồi đồng thời khôi phục vật nhặt ngoài cảnh. Việc khôi phục phải thay toàn bộ trạng thái phiên bằng ảnh chụp, không trộn hai phiên.

## Trạng thái cấp phiên

MainMenu → Loading → Playing; từ Playing có thể sang Paused, Dead hoặc ChapterComplete. Loading, Dead và ChapterComplete chặn action Gameplay. Paused dùng UI map; quay lại game phải xóa lệnh tấn công hoặc tương tác còn giữ từ trước.

# 8 Cấu trúc dự án và thiết lập Unity

## Phiên bản và gói sử dụng

Chọn **Unity 6.3 LTS**, bản vá ổn định tương thích các tài nguyên đã chọn; ghi chính xác trong ProjectVersion.txt và giữ cố định trong dự án. Unity công bố dòng 6.3 là LTS; Package Manager có Cinemachine 3.1 cho dòng này [S1, S2]. Không tự động nâng Editor giữa kỳ.

| **Thành phần** | **Lựa chọn** | **Quy tắc áp dụng** |
| --- | --- | --- |
| Đồ họa | Universal 3D với URP đi kèm Editor | Giữ nguyên gói tương thích do template tạo |
| Điều khiển | Input System bản Released tương thích | Chỉ dùng một hệ input trong code chính |
| Camera | Cinemachine 3.1 | Dùng CinemachineCamera, tránh trộn ví dụ API 2.x |
| AI | AI Navigation 2.x tương thích | NavMeshSurface cho nền đi được [S4] |
| UI | Unity UI và TextMeshPro | Một EventSystem, InputSystemUIInputModule |
| Tài nguyên | Prefab và tham chiếu trực tiếp | Chưa cần Addressables hoặc tải Resources hàng loạt |
| Âm thanh | AudioSource và AudioMixer | Không phụ thuộc FMOD trong bản bắt buộc |
| Lưu | JSON, DTO, persistentDataPath | Không phụ thuộc gói serialize trả phí |

## Thư mục

Trong Assets/_Project tạo Art, Audio, Animations, Materials, Prefabs, Scenes, Scripts, Data, UI và Tests. Chia Scripts theo Core, Player, Interaction, Combat, AI, Puzzles, Inventory, Quests, Save và UI.

## Scene và layer

Scene chính: 00_Boot, 01_MainMenu, 02_HangLinhMach. Scene kiểm tra: 90_TestMechanics, 91_TestCombat, 92_TestSave. Chỉ ba scene chính vào danh sách build.

Các layer: Environment, Player, Enemy, Interactable, Hurtbox, LanternTarget và PuzzleBeam. Collider quyết định layer theo vai trò; collider phụ có thể nằm ở layer khác object gốc. Ghi rõ từng LayerMask trong prefab và kiểm tra bằng Gizmos.

Trong Project Settings bật Visible Meta Files và Force Text. Theo dõi Assets, Packages, ProjectSettings và .meta; bỏ Library, Temp, Obj, Logs và Builds khỏi Git. Lưu file packages-lock.json để cả nhóm dùng cùng bộ gói.

# 9 Điều khiển nhân vật và camera

## Cấu trúc prefab An

Object gốc Player có CharacterController, PlayerMotor, PlayerVitals, InputReader, CombatController và InteractionScanner. Con VisualRoot chứa mô hình và Animator; các điểm gắn tay nằm trong rig. CameraPivot đi theo vị trí nhân vật nhưng có góc yaw và pitch độc lập. Giữ mô hình quay theo hướng đi hoặc hướng đánh, không ép pivot quay theo mô hình mỗi frame.

| **Thông số ban đầu** | **Giá trị** | **Cách kiểm tra** |
| --- | --- | --- |
| Controller | Cao 1,8 m, bán kính 0,3 m, center Y 0,9 | Chân chạm nền, không kẹt cửa |
| Bậc và dốc | Step Offset 0,3 m, Slope Limit 45° | Đi bậc thấp, không leo vách dốc |
| Tốc độ | Đi 3,5 m/s, chạy 5,5 m/s | Đi chéo không nhanh hơn đi thẳng |
| Nhảy và trọng lực | Cao 1,2 m, gia tốc Y âm 20 m/s² | Có chạm đất rõ, không nhảy vô hạn |
| Camera | Khoảng cách 3,5 m, pivot cao 1,5 m | Thấy chân và phần đường phía trước |
| Giới hạn nhìn | Pitch khoảng âm 35° đến 65° | Không lật góc nhìn khi rê chuột lâu |

## Thứ tự xử lý

Đọc input, chiếu hướng camera xuống mặt phẳng XZ, chuẩn hóa vector di chuyển rồi chọn tốc độ theo trạng thái. Tính vận tốc dọc và gọi CharacterController.Move từ một nơi. Phương thức này không tự áp dụng trọng lực [S5]; phải tự cộng gia tốc theo thời gian và xử lý chạm trần, chạm nền.

Để Animator Apply Root Motion tắt trong bản đầu. Animator nhận Speed, Grounded, VerticalSpeed và các trigger Attack, Dodge, Hurt, Dead. Animation không đồng thời tự đẩy nhân vật khi PlayerMotor đang điều khiển vị trí.

## Camera trong Cinemachine 3

Main Camera có CinemachineBrain. CinemachineCamera theo CameraPivot và dùng Third Person Follow; cơ chế này có xử lý va chạm camera [S3]. Cho camera va với Environment, bỏ Player và VFX khỏi mask. Bắt đầu với camera radius 0,2 m, near clip khoảng 0,1 m rồi thử ở góc tường, hầm thấp và sát vách.

# 10 Input trạng thái và quy tắc ưu tiên

| **Phím** | **Action** | **Hành vi** |
| --- | --- | --- |
| WASD và chuột | Move, Look | Di chuyển theo camera và xoay góc nhìn |
| Shift và Space | Sprint, Jump | Chạy tốn thể lực; nhảy khi chạm đất |
| Ctrl trái | Dodge | Né theo hướng đi, không có hướng thì né theo hướng nhìn |
| Chuột trái | Attack | Đánh dao khi ở trạng thái cho phép |
| F và chuột phải | ToggleLantern, Focus | Bật đèn; giữ chiếu tập trung khi đèn đang bật |
| E | Interact | Tương tác với mục tiêu hợp lệ đang nhìn |
| Q | UsePotion | Dùng thuốc hồi máu ngoài menu |
| I và J | Inventory, Journal | Xem túi đồ hoặc nhiệm vụ, chuyển sang UI map |
| Esc | Pause hoặc Back | Mở tạm dừng hoặc đóng lớp menu trên cùng |

## Action map

Tạo PlayerControls.inputactions có Gameplay và UI. Move và Look trả về Vector2; các thao tác còn lại là Button. Dùng một phương thức đọc input nhất quán, không vừa dùng callback PlayerInput vừa đọc phím cũ trong Update cho cùng action. Khi đổi map, xóa giá trị Move, Focus và Attack đang giữ.

## Luật trạng thái

Ưu tiên: Dead cao nhất, sau đó Loading hoặc khóa theo phiên, Hurt, Dodge, Attack, Interact và Locomotion. Trên không vẫn nhận hướng đi có giới hạn; không cho mở tương tác câu đố hoặc né lặp giữa không trung. Trạng thái Focus giảm tốc độ xuống 2 m/s và không chạy.

Né tiêu hao 25 thể lực, kéo dài 0,45 giây; bất tử từ giây 0,10 đến 0,30, thời gian giữa hai lần bắt đầu né tối thiểu 0,65 giây. Hướng né được chốt lúc bắt đầu và đi qua Controller.Move để vẫn bị tường chặn.

Menu túi đồ, nhật ký và chế thuốc làm tạm dừng thế giới. UI dùng thời gian không phụ thuộc timeScale; gameplay, hồi phục, AI và thời gian đòn đánh dùng thời gian mô phỏng. Đóng menu không được gây một cú đánh do click còn tồn từ UI.

# 11 Tương tác và Lồng Đèn Sen

## Chọn mục tiêu tương tác

Bắn ray hoặc sphere cast nhỏ từ tâm camera, lấy vật thể gần nhất trên mask có cả Environment và Interactable. Sau đó kiểm tra khoảng cách từ An đến mục tiêu không quá 3 m và đường nhìn từ nhân vật không bị chắn. Camera ở sau lưng nên khoảng cách ray không đồng nghĩa với tầm tay.

Chỉ hiện một lời nhắc như “E Gõ trống” hoặc “E Nhặt linh chi”. Nhấn E gọi CanInteract lần nữa trước khi thực hiện. Vật nhặt chỉ xử lý qua tương tác này; trigger vùng không đồng thời tự cho vật phẩm.

## Ba tác dụng của đèn

| **Chế độ hoặc mục tiêu** | **Luật xử lý** | **Dữ liệu cần lưu** |
| --- | --- | --- |
| Sáng thường | Không tốn năng lượng; hiện văn tự trong 5 m nếu nhìn thấy | Chỉ lưu đã đọc nếu là dấu tích nhiệm vụ |
| Xóa phong ấn | Focus trong 8 m, góc toàn phần 60°, có đường nhìn, giữ 1,5 giây | ID phong ấn đã xóa |
| Làm yếu địch | Focus liên tục 1 giây lên địch hợp lệ | Không lưu hiệu ứng tạm; kết thúc sau 4 giây |

Đèn có 100 năng lượng, Focus tốn 12 mỗi giây. Thả Focus một giây thì hồi 20 mỗi giây, kể cả khi đèn tắt. Hết năng lượng dừng Focus và hiện thông báo ngắn; người chơi luôn có thể chờ hồi, không cần mua nhiên liệu để vượt cửa bắt buộc.

## Triển khai

LanternController lọc mục tiêu gần, kiểm tra góc giữa hướng chiếu và vector đến mục tiêu rồi raycast Environment để xác thực vật cản. Dùng chính nguồn và hướng chiếu có ý nghĩa với animation tay; vẽ Gizmos khi kiểm tra. Đèn nhìn đẹp trên màn hình không tự có nghĩa là mục tiêu nhận được hiệu ứng.

Tiến độ xóa phong ấn về 0 nếu mất chiếu quá 0,25 giây. Khi đủ 1,5 giây, ghi trạng thái đã xóa trước, mở đường rồi phát hiệu ứng một lần. Địch bị làm yếu nhận sát thương nhân 1,5 trong 4 giây; chiếu tiếp có thể làm mới thời hạn nhưng không gây choáng vô hạn.

# 12 Câu đố Trống Đồng

## Thiết kế và lời giải mẫu

Ba mặt trống có ký hiệu A, B, C bằng hình khác nhau. Văn tự hiện dưới đèn cho thứ tự **A → C → B**. Âm thanh khác nhau hỗ trợ ghi nhớ; ký hiệu thị giác vẫn đủ để giải khi tắt tiếng. Chuỗi minh họa là quy ước gameplay, không được giới thiệu như một bản nhạc cổ đã được xác thực.

Một lần gõ hợp lệ phát âm, sáng ký hiệu và chuyển kiểm tra sau 0,2 giây chống nhấn lặp. Bản bắt buộc kiểm tra thứ tự, không yêu cầu độ chính xác nhịp điệu theo mili giây. Có thể nâng thành câu đố nhịp ở giai đoạn sau.

## Dữ liệu và trạng thái

DrumPuzzleDefinition chứa chuỗi mục tiêu, thời gian chống nhấn lặp và ID phần thưởng. DrumPuzzleController có Unsolved, Listening, Failed, Solved; giữ currentIndex của lượt đang thử. Nút Làm lại trong UI câu đố chỉ xóa lượt thử, không xóa một câu đố đã hoàn thành.

## Thuật toán xử lý

1. Nếu đã Solved hoặc đang chờ chống nhấn lặp thì bỏ input mới.

2. Nhận mã trống, phát phản hồi rồi so với phần tử tại currentIndex.

3. Nếu đúng, tăng currentIndex; nếu đủ ba phần tử, ghi pz_trong_01 vào tập đã giải.

4. Nếu sai, hiện ký hiệu báo sai, đợi ngắn rồi đặt currentIndex về 0; không phạt mất nguyên liệu hay khóa cửa.

5. Sau khi đã ghi solved, mở cửa và bệ mảnh B. Việc nhận mảnh B là một tương tác riêng, có kiểm tra chưa nhận.

## Dựng trong Unity

Tạo object gốc Puzzle_Drum_01, gắn PersistentId và controller. Đặt ba con Drum_A, Drum_B, Drum_C có Collider, Interactable và noteId. Gán cửa, bệ thưởng, renderer ký hiệu và AudioSource trong Inspector. Trong scene kiểm tra, dùng vật liệu màu đơn giản trước khi nhập mô hình trống.

## Nghiệm thu

Chuỗi A C B mở cửa đúng một lần. A B hoặc A C A báo sai và thử lại được. Giữ phím hoặc nhấn liên tục không bỏ qua chỉ số. Rời phòng, chết, tiếp tục hoặc tải lại sau khi đã lưu đều không sinh thêm phần thưởng. Một người thử có thể giải chỉ bằng ký hiệu và lời gợi ý.

# 13 Câu đố ánh sáng và gương

## Bố cục và mục tiêu

Một nguồn sáng cố định chiếu qua hai gương tới ngọc thạch nhận sáng. Người chơi tương tác E để xoay từng gương theo bốn nấc 90°. Gương có mặt phản xạ nghiêng phù hợp và dấu chỉ hướng rõ; thiết kế sẵn một cấu hình đúng, kiểm tra thực tế đường tia trước khi dựng trang trí.

Giữ ngọc nhận đủ sáng liên tục 1 giây sẽ mở bệ mảnh C và ghi pz_guong_01 đã giải. Bản bắt buộc dùng một nguồn, tối đa hai gương và không có tách tia, đổi màu hay quang học động phức tạp.

## Phân biệt mô phỏng và hiệu ứng

Raycast mới là phép kiểm tra logic; LineRenderer chỉ vẽ kết quả từng đoạn. Nếu tia gặp tường, tia dừng ở tường. Nếu gặp gương ở đúng mặt phản xạ, hướng mới lấy từ Vector3.Reflect(direction, hit.normal). Dịch điểm bắt đầu tiếp theo một khoảng nhỏ để không đập lại đúng bề mặt vừa trúng.

Giới hạn ba lần phản xạ và tổng chiều dài 40 m để tránh vòng lặp giữa hai gương. Mặt lưng không phản xạ; kiểm tra hướng tới so với pháp tuyến đã quy định. Dùng mask có cả bề mặt cản và mục tiêu quang học, không chỉ raycast riêng vào gương.

## Trạng thái câu đố

Trước khi hoàn thành, mất tia chiếu làm tiến độ về 0. Sau khi hoàn thành, trạng thái Solved được khóa và cửa không đóng lại nếu gương bị xoay. Khóa tương tác xoay ở trạng thái Solved hoặc áp dụng hình dáng đã giải khi tải. Bản lưu không cần giữ từng lượt xoay chưa hoàn thành.

## Thứ tự dựng và kiểm tra

1. Dựng nguồn và ngọc trên nền phẳng, xác nhận nhận tia trực tiếp.

2. Thêm một gương, kiểm tra pháp tuyến và mặt lưng bằng Gizmos.

3. Thêm gương thứ hai, kiểm tra đầy đủ 16 tổ hợp hai gương trong scene thử.

4. Thêm tường chắn thử; không có tia nhận xuyên tường. Thử hai gương chiếu vào nhau; số đoạn vẫn bị giới hạn.

5. Nối cửa và phần thưởng sau khi logic tia đúng. Cung cấp dấu hướng vào và ra để người chơi hiểu tác dụng mỗi lần xoay.

Lưu sẵn bộ nấc của lời giải trong dữ liệu thiết kế để người dựng màn có thể kiểm tra lại sau khi di chuyển nguồn, gương hoặc ngọc.

# 14 Chiến đấu và chỉ số sinh tồn phù hợp game

## Bộ chỉ số ban đầu

| **Chỉ số** | **An** | **Hắc Ảnh** | **Hộ vệ Đá** |
| --- | --- | --- | --- |
| Máu tối đa | 100 | 60 | 180 |
| Sát thương đòn | Dao 20 | 15 | Quét 20, nện 25 |
| Thể lực | 100 | Không mô phỏng | Không mô phỏng |
| Đặc tính | Né, thuốc, lồng đèn | Bị đèn làm yếu | Hai đòn có báo trước |

An chạy tốn 8 thể lực mỗi giây, một đòn dao tốn 10, né tốn 25. Hồi 20 mỗi giây sau 1 giây không tiêu hao. Thuốc hồi 35 máu, tối đa bằng máu trần và không bị dùng khi đã đầy máu. Các con số là điểm bắt đầu để cân bằng, không phải số liệu đã đo.

## Chu kỳ một đòn dao

Đòn dài 0,65 giây: lấy đà 0,15 giây, gây sát thương trong khoảng 0,15–0,30 giây, sau đó hồi thế đến hết đòn. Tầm kiểm tra khoảng 1,8 m trước người. Chốt hướng ở lúc bắt đầu, cho phép quay có giới hạn trong lấy đà để đòn không đổi hướng bất ngờ.

Trong cửa sổ gây sát thương, dùng OverlapCapsule hoặc một vùng quét phù hợp với quỹ đạo dao. Tìm IDamageable ở object gốc, lọc đội và đường nhìn. Một đòn giữ tập mục tiêu đã trúng nên nhiều collider của cùng địch chỉ nhận một lần.

## Hoạt ảnh và quyền điều khiển

CombatController giữ thời gian logic. Animation Events đồng bộ tiếng vung, tiếng chân hoặc VFX; không dùng một Animation Event đơn lẻ làm nguồn duy nhất của sát thương. Nếu một frame nhảy qua cửa sổ active, kiểm tra giao nhau giữa khoảng thời gian frame trước và frame hiện tại để đòn không mất sát thương ở FPS thấp.

Hurt có thể hủy đòn khi còn lấy đà theo luật đã chọn. Dead luôn hủy đòn và xóa hitbox. Khi rời Dodge, Attack hoặc chết, reset cờ bất tử, tốc độ và tập đã trúng đúng vòng đời. PlayerMotor là nơi duy nhất dịch chuyển nhân vật.

Máu và thể lực phục vụ chiến đấu; đói, khát và nhiệt độ không thuộc bản bắt buộc vì không hỗ trợ vòng khám phá và giải đố đã chọn.

# 15 AI kẻ địch và tìm đường

Máy trạng thái của Hắc Ảnh và các chuyển trạng thái ưu tiên

## Hắc Ảnh

Idle hoặc Patrol tuần tra giữa 2–3 điểm. Nhận biết An trong 10 m, góc nhìn toàn phần 120° và có đường nhìn. Chase dùng NavMeshAgent tới mục tiêu; vào Attack khi gần khoảng 1,7 m và hết hồi chiêu 1,6 giây. Mất dấu quá 3 giây thì Search ngắn rồi Return về khu vực ban đầu.

Giới hạn đuổi tối đa khoảng 18 m tính từ điểm sinh để địch không chạy xuyên suốt màn. Trước khi gây sát thương vẫn kiểm tra mục tiêu còn sống, ở trong vùng đánh và không bị tường che. Địch không được gây sát thương chỉ vì Animator đang chạy clip Attack.

## Dựng NavMesh

NavMeshSurface lấy hình học nền và vật cản tĩnh phù hợp. Chọn Agent Type khớp kích thước địch, bake sau khi khối màn đã ổn định [S4]. Cho vùng nguy hiểm là Not Walkable. Cửa đóng cần một phương án chặn đường thống nhất, ví dụ NavMeshObstacle có carving trên cánh cửa.

Không điều khiển cùng một địch bằng cả Rigidbody động, NavMeshAgent và root motion dịch chuyển. Bản đầu để Agent điều khiển vị trí, Animator chỉ hiển thị. Đánh kiểm tra đường đi ở góc hẹp, chân cầu và cửa trước khi trang trí.

## Chết và xuất hiện lại

Dead dừng Agent và tắt khả năng đánh, cho hiệu ứng tan Hắc Sương rồi loại khỏi phiên hiện tại. Địch thường khôi phục ở điểm sinh khi tải ảnh chụp mốc lưu; Hộ vệ là địch độc nhất và chỉ khôi phục nếu ID chưa nằm trong tập bị đánh bại.

Không xây hệ sinh địch vô hạn trong chương đầu. Đặt số lượng hữu hạn, tối đa ba Hắc Ảnh hoạt động gần người chơi cùng lúc để dễ kiểm soát độ khó và hiệu năng.

# 16 Hộ vệ Đá và trận kết chương

## Vai trò trong màn chơi

Hộ vệ kiểm tra khả năng đọc đòn, né và dùng lồng đèn đã học. Tận dụng EnemyBrain, hệ sát thương và trạng thái làm yếu của Hắc Ảnh; thay dữ liệu, mô hình, hoạt ảnh và bộ chọn đòn. Không cần một hệ boss tách biệt chỉ cho một trận.

| **Đòn** | **Báo trước** | **Vùng gây sát thương** | **Khoảng phản công** |
| --- | --- | --- | --- |
| Quét tay | Nâng tay, sáng hoa văn 0,8 giây | Vùng phía trước, active 0,2 giây, 20 máu | Hồi thế 1,2 giây |
| Nện đất | Giơ hai tay, vòng cảnh báo 1,2 giây | Bán kính khoảng 3 m, active 0,25 giây, 25 máu | Hồi thế 1,5 giây |

Chọn quét khi An ở gần, chọn nện khi An ở khoảng trung bình, còn quá xa thì đuổi. Mỗi đòn vẫn có tập mục tiêu đã trúng và kiểm tra vật cản. Không có sóng gây sát thương vô hình kéo dài sau khi vòng cảnh báo biến mất.

## Luật đấu trường

Cổng khóa sau khi An đi qua vùng bắt đầu, mốc lưu nằm trước cổng. Thua trận đưa An về phía ngoài cổng và khôi phục toàn bộ phiên theo ảnh chụp; không khôi phục nửa trận. Khi Hộ vệ chết, ghi guardian_01, mở cổng và bật tương tác bệ trống.

Chiếu tập trung đủ 1 giây làm yếu Hộ vệ trong 4 giây giống luật chung. Không bắt buộc phải có năng lượng ngay thời điểm vào trận; An có thể né, chờ hồi rồi chiếu tiếp. Không sử dụng một vật phẩm hiếm để mở khóa sát thương boss.

## Kết thúc và lưu

Tương tác bệ trống khi đủ điều kiện đặt relicRestored = true, chuyển mốc lưu sang bệ Kim Quy và lưu một ảnh chụp an toàn. Chạy camera kết thúc ngắn, đổi ánh sáng và nhạc, hiện hoàn thành chương. Continue sau đó không dựng lại Hộ vệ hoặc cấp lại ba mảnh.

## Kiểm tra cân bằng

Mời người chưa biết cơ chế thử trận. Ghi số lần bị trúng vì không nhìn được báo hiệu, thời gian hạ Hộ vệ, lượng thuốc dùng và vị trí kẹt camera. Mục tiêu một lượt thắng khoảng 1–3 phút; điều chỉnh nhịp đòn và cơ hội phản công trước khi tăng hoặc giảm máu.

# 17 Vật phẩm túi đồ và chế thuốc

## Danh mục dữ liệu

| **Nhóm** | **Mã định danh** | **Luật sở hữu** |
| --- | --- | --- |
| Nguyên liệu | item_la_cam, item_linh_chi | Trong túi, tối đa 10 mỗi chồng |
| Thuốc | item_thuoc_mau | Trong túi, tối đa 5 mỗi chồng, hồi 35 máu |
| Trang bị cố định | equipment_dao, equipment_lantern | Cấp một lần, không chiếm ô, không vứt bỏ |
| Mảnh nhiệm vụ | fragment_a, fragment_b, fragment_c | Tập ID riêng, không thể mất vì túi đầy |
| Cổ vật hoàn chỉnh | relic_kim_quy | Suy ra từ relicRestored, hiển thị trong nhật ký |

Tạo chín định nghĩa tương ứng. Túi có 12 ô, hỗ trợ chọn ô và xem số lượng; chưa cần kéo thả hoặc trọng lượng. Đồ nhiệm vụ không bị hủy và không được dùng làm nguyên liệu chế tạo.

## Nhặt và sử dụng

TryAdd trả lại số lượng đã nhận và phần còn thiếu chỗ. Nếu túi đầy, vật ngoài cảnh còn lại đúng lượng chưa nhận. Chỉ đánh dấu pickup đã thu khi đối tượng đã hết lượng. Trong bản bắt buộc, mỗi pickup chứa đúng một đơn vị để trạng thái lưu chỉ cần đã nhận hoặc chưa nhận. Nếu bổ sung chồng vật ngoài cảnh, phải lưu cả lượng còn lại của mỗi pickup.

Phím Q tìm thuốc theo ID, kiểm tra còn sống, chưa đầy máu và hết hồi dùng 2 giây. Trừ đúng một thuốc và cộng máu trong cùng lệnh; input lặp không được gọi hai lần trong cùng lần nhấn. Túi đồ dùng để xem; sử dụng nhanh diễn ra ngoài menu.

## Công thức lửa trại

Một lá cẩm và một linh chi tạo một thuốc máu. Chỉ chế tại trại. TryCraft kiểm tra đủ nguyên liệu và chỗ chứa của **inventory sau giao dịch** rồi mới thay đổi. Nếu nguyên liệu tiêu hết làm trống một ô, ô đó có thể chứa thuốc đầu ra.

Tạo bản thay đổi tạm, thử trừ nguyên liệu và thêm kết quả; chỉ commit khi tất cả bước thành công. Khi thất bại, trả nguyên nhân cụ thể và giữ nguyên túi. UI không tự gọi Remove rồi Add bằng hai đường riêng.

Mỗi thao tác chế thành công tại trại yêu cầu lưu trạng thái an toàn. Bản bắt buộc chưa có vứt đồ, phá hủy công cụ hoặc mua bán để giảm các trường hợp mất vật và nhân đôi vật.

# 18 Thiết kế lưu tải và hồi sinh

## Chính sách mốc lưu

Lưu tại lửa trại, sau giải câu đố ở thời điểm an toàn và sau khi hoàn thành bệ Kim Quy. Mốc đặt nhân vật gồm cp_trai, cp_dien_trong, cp_truoc_dan và cp_kim_quy. Mỗi lần lưu ghi lại dữ liệu hiện tại nhưng vị trí hồi sinh là Transform của mốc an toàn đang kích hoạt, không phải vị trí giữa cú nhảy hay giữa trận.

Ảnh chụp bao gồm inventory, đồ nhiệm vụ, cửa và câu đố, các phong ấn đã xóa, vật đã nhặt, văn tự đã đọc, Hộ vệ đã bị đánh bại, tiến trình và thời gian chơi. Hồi máu, thể lực và năng lượng đầy tại mốc lưu để việc hồi sinh có luật nhất quán. Hiệu ứng tạm và lượt giải đố chưa xong được xóa khi tải.

## Định danh và dữ liệu

Mỗi puzzle, pickup, seal, checkpoint và địch độc nhất có ID ổn định. Không dùng tên object có thể đổi, vị trí trong Hierarchy hoặc GetInstanceID() làm khóa lưu. Công cụ Editor phát hiện ID trùng sau khi duplicate prefab; quá trình kiểm tra build phải báo lỗi nếu còn ID trùng hoặc trống.

| **Nhóm dữ liệu** | **Các trường chính** | **Nguồn khi tải** |
| --- | --- | --- |
| Đầu file | schemaVersion, buildVersion, timestamp | Kiểm tra định dạng và khả năng đọc |
| Vị trí an toàn | sceneId, checkpointId | Tra catalog scene và các mốc đã đăng ký |
| Người chơi | health, stamina, lanternEnergy | Đặt theo luật mốc lưu và giới hạn tối đa |
| Túi | itemId và quantity từng ô | Catalog vật phẩm, kiểm tra lượng hợp lệ |
| Thế giới | Các danh sách ID đã hoàn thành hoặc đã nhận | WorldStateStore khôi phục thành các tập |
| Tiến trình | equipmentGranted, relicRestored | QuestSystem tính lại bước hiện hành |

## Quy tắc kỹ thuật

Dùng class DTO có [Serializable] và các field dạng List hoặc array. JsonUtility không hỗ trợ Dictionary trực tiếp [S6]; có thể dùng HashSet trong runtime nhưng phải chuyển thành danh sách lúc lưu. Không serialize tham chiếu GameObject hoặc ScriptableObject trực tiếp thành dữ liệu phiên.

# 19 Quy trình ghi file và khôi phục thế giới

## Ví dụ ảnh chụp dữ liệu

{

  "schemaVersion": 1,

  "buildVersion": "0.1.0",

  "sceneId": "HangLinhMach",

  "checkpointId": "cp_dien_trong",

  "health": 100,

  "stamina": 100,

  "lanternEnergy": 100,

  "inventory": [

    {"slot": 0, "itemId": "item_la_cam", "quantity": 1}

  ],

  "solvedPuzzles": ["pz_trong_01"],

  "clearedSeals": ["seal_vuon_01"],

  "collectedPickups": ["pickup_la_cam_01"],

  "readLoreIds": ["lore_vuon_01"],

  "fragments": ["fragment_a", "fragment_b"],

  "defeatedUniqueEnemies": [],

  "equipmentGranted": true,

  "relicRestored": false,

  "elapsedPlaySeconds": 780

}

## Ghi file

Tạo snapshot trên luồng chính sau khi các thao tác vật phẩm đã xong. Ghi save.tmp trong cùng thư mục với save.json, flush và đóng; thử đọc lại và xác thực trước khi thay file chính. Trên Windows dùng thao tác thay thế file có backup, hoặc đổi tên khi tạo file lần đầu; kiểm tra hành vi trên máy build thực tế.

Dùng Application.persistentDataPath, không ghi vào Assets hay thư mục cài game [S7]. Chỉ một lệnh ghi được hoạt động mỗi thời điểm. Nếu ghi lỗi, giữ file cũ và báo chưa lưu; không hiển thị “Đã lưu” trước khi thành công.

## Tải và xử lý lỗi

Ưu tiên file chính; nếu hỏng thì thử backup hợp lệ. Từ chối phiên bản schema cao hơn chưa hỗ trợ, giữ nguyên file và đưa lựa chọn bắt đầu mới rõ ràng. Đợi scene và danh sách ID sẵn sàng, áp trạng thái thế giới, đặt người chơi tại checkpoint rồi mới bật AI và input.

Mã hóa không cần cho bản ngoại tuyến. Checksum chỉ giúp nhận biết hỏng dữ liệu, không chứng minh chống sửa save. Không chọn BinaryFormatter hoặc chỉ đổi JSON thành Base64 rồi coi đó là bảo vệ dữ liệu.

# 20 Giao diện và trải nghiệm người chơi

## Các màn hình

| **Màn hình** | **Nội dung** | **Quy tắc tương tác** |
| --- | --- | --- |
| Menu chính | Chơi mới, Tiếp tục, Cài đặt, Thoát | Continue chỉ bật khi có dữ liệu hợp lệ |
| HUD | Máu, thể lực, năng lượng đèn, thuốc, mục tiêu | Chỉ hiển thị chỉ số cần dùng, chữ dễ đọc |
| Nhắc tương tác | Phím E và tên hành động | Một mục tiêu, không hiện qua tường |
| Túi đồ | 12 ô, mô tả, số lượng | Không cho UI tự thay inventory |
| Nhật ký | Mục tiêu và ba văn tự đã đọc | Nhắc đích đến, tránh tiết lộ toàn lời giải |
| Chế thuốc | Nguyên liệu đang có, kết quả, nút chế | Hiện lý do thiếu đồ hoặc thiếu chỗ |
| Tạm dừng | Tiếp tục, cài đặt, về menu | Gameplay dừng, UI vẫn nhận input |
| Thua và kết chương | Hồi sinh, về menu hoặc xem kết quả | Không còn thao tác chiến đấu |

## Cài đặt tối thiểu

Âm lượng tổng, nhạc và hiệu ứng; độ nhạy chuột; chế độ cửa sổ hoặc toàn màn hình; chất lượng thấp hoặc vừa. Lưu các thiết lập này bằng PlayerPrefs, tách khỏi file tiến trình. Đổi chất lượng không được làm thay đổi luật puzzle hay AI.

## Khả năng đọc

Dùng biểu tượng kèm nhãn ngắn, không để mọi nút chỉ có icon. Dấu A B C, trạng thái sai và trạng thái đúng khác hình dạng, không chỉ khác màu. Mục tiêu nhiệm vụ ngắn, gợi ý chi tiết nằm trong nhật ký. TextMeshPro có font hỗ trợ đầy đủ dấu tiếng Việt và fallback đã thử trong build.

## Tổ chức Canvas

Một Canvas HUD và một nhóm panel menu có thứ tự rõ. Canvas Scaler lấy mốc 1920 × 1080, dùng anchor đúng góc và thử 1280 × 720, 1920 × 1080, tỷ lệ 16:10. Một EventSystem dùng InputSystemUIInputModule; không để cả module input cũ và mới cùng điều khiển UI.

Khi pause, mở túi hoặc nhật ký: đổi action map, mở cursor và tạm dừng gameplay. Khi đóng: phục hồi map và cursor theo trạng thái phiên, không gọi bật Gameplay bất kể game đã chết hay đang tải.

# 21 Môi trường đồ họa hoạt ảnh và âm thanh

## Môi trường hang động

Dùng bộ mesh đá ghép cho vòm và lòng hang; Unity Terrain chủ yếu dùng vùng nền hoặc giếng trời. Không cố tạo vòm hang và khoảng rỗng dưới đất chỉ bằng một heightmap. Chia cảnh thành các phòng có đường nối để kiểm soát ánh sáng, camera và số địch.

URP dùng ánh sáng baked cho phần tĩnh, một số ít đèn động quan trọng và probe cho nhân vật. Điện Trống và gương phải đọc được ngay cả ở chất lượng thấp. Vật liệu nước đơn giản, không cần mô phỏng dòng chảy thật trong chương đầu.

Tài liệu ý tưởng gợi ý sương thể tích. Với bản URP đã chọn, dùng fog nền và particle hoặc mesh mờ có kiểm soát; volumetric fog kiểu HDRP không được coi là tính năng có sẵn tương đương trong URP [S8]. Chỉ thêm gói ngoài sau khi thử trên đúng phiên bản và đo chi phí.

## Hoạt ảnh cần có

An cần Idle, Walk, Run, Jump, Fall, Land, Attack, Dodge, Hurt, Death và một clip tương tác. Dùng clip chung tương tác cho nhặt, trống và bệ nếu cần. Hắc Ảnh dùng Idle, Walk, Attack, Hurt, Death; Hộ vệ dùng thêm hai clip đòn có tư thế báo trước rõ ràng.

Đặt điểm gắn dao và đèn trong rig; dùng layer tay hoặc Animation Rigging chỉ khi clip nền chưa giữ được vật đúng. Kiểm tra chân trượt và root motion trước khi thêm IK. Không thêm gói phức tạp chỉ để sửa sai vị trí socket đơn giản.

## Âm thanh theo ngữ cảnh

AudioMixer có Master, Music, SFX, Ambience và UI. Âm môi trường là lớp nước nhỏ giọt, gió và dòng sông xa; phát 3D ở vị trí phù hợp. Tiếng nút là 2D, tiếng đánh hoặc gõ trống có phản hồi ngay. Nhạc chuyển giữa khám phá, giao chiến và hoàn thành bằng fade ngắn.

Tiếng bước chân gắn theo nhịp hoạt ảnh hoặc quãng đường, không gọi PlayOneShot mỗi frame. Khi pause, dừng âm gameplay cần dừng và vẫn cho âm UI hoạt động. Mỗi file nhạc, mẫu âm và mô hình có nguồn sử dụng được ghi trong bảng tài nguyên.

# 22 Hiệu năng và tiêu chí chất lượng

## Mục tiêu để đo

Mục tiêu ban đầu là 60 FPS ở 1920 × 1080, chất lượng vừa, trên máy được nhóm chọn làm máy đo tham chiếu ở tuần 1. Ghi CPU, GPU, RAM, phiên bản Windows và build. Chưa có cấu hình tham chiếu thì không được dùng con số FPS như một cam kết cho mọi máy.

| **Nội dung đo** | **Cách đo nhất quán** | **Ngưỡng hoặc mục tiêu** |
| --- | --- | --- |
| Khung hình | Bản build, cùng tuyến đi 3 phút, chạy 3 lượt | Mốc 60 FPS tương ứng khoảng 16,7 ms/frame |
| Frame time | Ghi trung vị và P95 sau khi tải ổn định | Theo dõi đoạn nặng; tránh giật lặp lại trên 33 ms |
| Bộ nhớ | Mở game, đi hết màn, quay lại menu 3 lần | Không tăng mãi sau mỗi vòng lặp |
| Lưu và tải | 10 lần tại các mốc có trạng thái khác nhau | Không mất dữ liệu, không chặn điều khiển kéo dài |
| AI | 3 địch cùng hoạt động trong cảnh nặng | Không kẹt đường hoặc đánh xuyên tường |
| Hiển thị | Chất lượng thấp và vừa, nhiều độ phân giải | Ký hiệu và báo hiệu đòn luôn đọc được |

## Thứ tự tối ưu

Đầu tiên đo CPU và GPU bằng Development Build cùng Profiler. Tắt Deep Profile cho phép đo thông thường; dùng lại khi cần truy vết hẹp. Sau khi xác định nút thắt, xử lý số đèn và bóng, vật liệu trong suốt, VFX, số renderer hoặc code gây cấp phát. Cuối cùng kiểm tra lại bản release trên cùng tuyến.

Dùng LOD cho đá hoặc cây lớn có nhiều biến thể khoảng cách, culling theo phòng và collider đơn giản. Cache component; không FindObjectOfType hoặc tải Resources trong vòng Update. Chỉ pool projectile hay VFX khi lượng tạo hủy đáng kể, và xác nhận pool đã reset mọi trạng thái.

## Không đổi luật chơi vì cài đặt đồ họa

Vùng sát thương, đường ray puzzle, nhận biết đèn và thời gian AI dùng dữ liệu gameplay độc lập. Giảm particle hay bloom không được làm mất vật cản logic hoặc khiến lồng đèn ngừng giải phong ấn.

# 23 Các bước triển khai nền tảng trong Unity

## Bước 1 Chuẩn bị dự án

Tạo project Universal 3D bằng Editor đã thống nhất. Lưu bộ package tương thích, thử mở project trên máy thứ hai rồi commit nền. Tạo thư mục theo phần 8, ba scene chính và ba scene kiểm tra. Cài Git LFS nếu đã quyết định dùng cho mô hình, âm thanh và texture lớn.

**Kết quả:** cả nhóm clone được, mở cùng phiên bản, Console không có lỗi compile và xuất được một build Windows mở menu trống.

## Bước 2 Dựng tuyến bằng khối

Trong HangLinhMach tạo các object gốc World_Geometry, Gameplay, Lighting, Cameras và UI. Dựng năm phòng, các cửa và bốn checkpoint bằng khối đơn giản. Tạo khu vực rơi chết và tường chặn ngoài tuyến. Chưa nhập bộ hang động nặng trước khi kiểm tra tuyến đi.

**Kết quả:** đi được toàn tuyến bằng nhân vật capsule, khoảng cách và kích thước cửa phù hợp, không còn vực vô tình hoặc lối bỏ qua mục tiêu.

## Bước 3 Làm điều khiển và camera

Tạo action map, Player prefab, motor và pivot. Nối CinemachineCamera, main camera và mask va chạm. Làm đi, chạy, nhảy trước; sau đó mới thêm né và Animator. Mỗi lần chỉ một thành phần điều khiển vị trí.

**Kết quả:** chạy trong scene kiểm tra với dốc, bậc, trần thấp và góc tường. Thử đổi FPS, pause và mất focus cửa sổ; nhân vật không trôi hoặc bị giữ phím.

## Bước 4 Dựng dịch vụ phiên chơi

Boot tạo GameSession, WorldStateStore, AudioService và SaveService. Chọn một cách truyền tham chiếu, tránh tự tìm object toàn cảnh liên tục. Tạo luồng Menu → Loading → Playing và khóa input đúng giai đoạn.

**Kết quả:** vào và ra gameplay ba lần không sinh trùng dịch vụ, camera hoặc EventSystem. Mọi prefab thiếu dữ liệu quan trọng báo lỗi có tên đối tượng để sửa trong Editor.

# 24 Các bước làm tương tác lồng đèn và giải đố

## Bước 5 Tạo catalog dữ liệu

Tạo PlayerStats, EnemyDefinition, chín ItemDefinition và RecipeDefinition. Viết kiểm tra ID trùng, lượng chồng và tham chiếu rỗng. Đặt một object thử cho mỗi loại vật phẩm và một bệ mảnh nhiệm vụ.

**Kết quả:** nhặt đúng ID, túi đủ hoặc đầy có phản hồi, vật nhiệm vụ vào tập riêng. Chưa kết nối nhiệm vụ theo số event.

## Bước 6 Làm tương tác thống nhất

Viết InteractionScanner và IInteractable. Thử nhặt đồ, đọc văn tự và bật checkpoint bằng cùng đường tương tác. Hiển thị một lời nhắc; đo khoảng cách từ An thay vì chỉ từ camera.

**Kết quả:** không thể nhặt xuyên tường hoặc đứng xa quá 3 m. Bấm E liên tục vào một mảnh không nhận được hai bản sao.

## Bước 7 Làm lồng đèn bằng đối tượng thử

Đặt một rune, một phong ấn và một địch giả nhận ILanternTarget. Làm bật đèn, tốn và hồi năng lượng, kiểm tra nón chiếu và vật cản; sau đó mới thêm hiệu ứng đẹp.

**Kết quả:** giữ đủ thời gian mới mở phong ấn, hết năng lượng có thể chờ hồi, pause không tiếp tục tích tiến độ. Chất lượng đồ họa thấp vẫn có tác dụng gameplay như nhau.

## Bước 8 Làm hai câu đố độc lập

Dựng Trống Đồng và gương trong TestMechanics theo phần 12–13. Chỉ sau khi chuỗi đúng sai và ray phản xạ ổn định mới nối vào cửa thật trong HangLinhMach. Cho mỗi puzzle một phương thức ApplySolvedState có thể gọi nhiều lần mà không cấp thưởng thêm.

**Kết quả:** giải cả hai không cần combat; reset được khi chưa giải; dùng lệnh Editor áp Solved vẫn mở đúng cửa. Bệ thưởng tách riêng khỏi lần mở cửa để dễ khôi phục dữ liệu.

## Bước 9 Nối một đoạn chơi hoàn chỉnh

Nối trại, Vườn Rêu và Điện Trống thành một đoạn 8–12 phút. Người ngoài nhóm thử và nói lại mục tiêu tiếp theo. Nếu không hiểu, sửa dấu hiệu hoặc gợi ý trước khi tăng lượng nội dung.

# 25 Các bước làm chiến đấu AI và tiến trình

## Bước 10 Hoàn thành hệ sát thương

Tạo IDamageable, DamageInfo và vùng kiểm tra dao trong TestCombat. Làm máu, tiêu thể lực, né và trạng thái chết; dùng hình khối hiển thị vùng active. Kiểm tra nhiều collider và thay đổi frame rate trước khi nhập clip đánh hoàn chỉnh.

**Kết quả:** một đòn trúng một mục tiêu đúng một lần, không gây sát thương khi đã hủy đòn, né chỉ bất tử trong khoảng đã quy định.

## Bước 11 Làm AI từ đơn giản đến đầy đủ

Bake NavMesh, cho địch đi giữa hai điểm. Thêm phát hiện An bằng khoảng cách, góc và raycast; thêm Chase và Return, cuối cùng mới nối Attack. Khi đổi trạng thái, dừng hoặc khôi phục Agent rõ ràng.

**Kết quả:** địch không nhìn xuyên tường, không theo vượt khu vực, không cố đi xuống vực và không đánh người chơi đang chết hoặc đang ở menu tải.

## Bước 12 Làm Hộ vệ bằng thành phần dùng chung

Thay EnemyDefinition, thêm hai kiểu đòn và điều kiện khóa đấu trường. Cho chạy clip báo trước, vùng đánh và hồi thế đúng thời gian. Thử Hộ vệ trong phòng phẳng trước khi đưa vào đàn đá có trang trí.

**Kết quả:** từ mốc trước đấu trường có thể đánh, chết, hồi sinh và đánh lại; thắng chỉ ghi một ID duy nhất, cửa không khóa lại sau Continue.

## Bước 13 Nối inventory chế thuốc và nhiệm vụ

Hoàn thiện giao dịch TryCraft, HUD thuốc và Q. QuestSystem đọc tập mảnh và puzzle để đổi mục tiêu. Cửa Đàn Kim Quy đọc điều kiện từ trạng thái thế giới, không đọc chuỗi chữ trên HUD.

**Kết quả:** có thể hoàn thành toàn tuyến bằng hình khối từ menu đến kết chương. Tại mốc này chưa yêu cầu đẹp; mọi điều kiện thắng thua, mở cửa và vật phẩm phải thực sự hoạt động.

# 26 Các bước lưu tải tích hợp và hoàn thiện

## Bước 14 Lưu từ đầu thay vì chờ cuối kỳ

Từ tuần 2 tạo DTO và save tối thiểu cho checkpoint; tuần 7 hoàn thiện toàn bộ world state. Kiểm tra New Game, Continue, chết và quay menu trên cả Editor và build. Tạo ảnh chụp ở trước puzzle, sau puzzle, trước Hộ vệ và sau kết chương.

**Kết quả:** tải một ảnh chụp không làm tăng vật phẩm, không reset câu đố đã giải và không đặt nhân vật trong cửa đóng. File hỏng dùng backup hoặc báo lỗi rõ.

## Bước 15 Thay khối bằng tài nguyên

Chia vùng cho người làm môi trường; thay từng phòng trong khi giữ nguyên collider gameplay và ID đối tượng. Bake ánh sáng và NavMesh lại sau thay đổi có ảnh hưởng. Kiểm tra camera trong các lối mới trước khi thêm sương hoặc nước.

**Kết quả:** bản đẹp giữ nguyên tuyến chơi đã qua kiểm tra. Báo hiệu đòn, đường tia gương và lời nhắc tương tác không bị hiệu ứng che khuất.

## Bước 16 Hoàn thiện UI và âm thanh

Thêm menu, HUD, nhật ký, inventory, panel chế thuốc, cài đặt, màn thua và kết chương. Kiểm tra font tiếng Việt và nhiều độ phân giải. Trộn âm lượng để tiếng báo đòn và puzzle nghe rõ hơn lớp ambience.

**Kết quả:** không còn UI thử nghiệm ở bản nộp; pause, resume và đổi scene hoạt động nhất quán. Cài đặt còn sau khi tắt và mở lại game.

## Bước 17 Đo và sửa theo thứ tự ảnh hưởng

Ưu tiên lỗi không hoàn thành được màn, mất save và crash; sau đó lỗi điều khiển, sát thương, UI; cuối cùng mới chỉnh đẹp. Ghi số liệu trên cùng máy và tuyến đo. Sau mỗi sửa liên quan dữ liệu chạy lại ca hồi sinh và Continue.

**Kết quả:** bản ứng viên phát hành qua toàn bộ checklist bắt buộc, có build dự phòng và bản source được gắn tag.

## Bước 18 Đóng gói

Tạo build release Windows x64, mở thử trên máy không cài Unity bằng toàn bộ thư mục build. Viết README, hướng dẫn phím, nguồn tài nguyên, known issues và kịch bản demo. Không chỉ gửi riêng file exe.

# 27 Nguồn lực và kế hoạch 12 tuần

## Giả định tính công

Kế hoạch chung dùng **2 người × 15 giờ mỗi tuần × 12 tuần = 360 giờ công**. Phân 300 giờ thực hiện và 60 giờ dự phòng cho học công nghệ, sửa lỗi và tích hợp. Đây là giả định để lập lịch; nhóm cần ghi giờ khả dụng thật ở tuần 1. Nếu có ba người cùng 15 giờ, tổng năng lực là 540 giờ và phần tăng thêm không tự động biến thành tính năng bắt buộc.

| **Gói công việc** | **Giờ cơ sở** | **Chủ trì gợi ý** |
| --- | --- | --- |
| Công cụ và kiến trúc | 18 | A |
| Điều khiển và camera | 32 | A |
| Tương tác và lồng đèn | 28 | A |
| Hai câu đố | 32 | B |
| Combat AI và Hộ vệ | 42 | A |
| Vật phẩm và chế thuốc | 18 | B |
| Nhiệm vụ và lưu tải | 28 | B, A rà soát |
| Màn chơi và mỹ thuật | 36 | B hoặc C |
| UI và âm thanh | 22 | B hoặc C |
| Kiểm thử build và hồ sơ | 44 | Cả nhóm |
| Tổng thực hiện | 300 | Hai người tối thiểu |
| Dự phòng | 60 | Không đặt trước thành tính năng |

## Các mốc phải giữ

Cuối tuần 2 có điều khiển và camera ổn định; tuần 4 có đoạn chơi từ trại đến câu đố đầu; tuần 6 có combat và AI; tuần 8 có toàn tuyến và save; tuần 10 khóa nội dung; tuần 11 có bản ứng viên; tuần 12 bàn giao. Nếu mốc chậm, dùng dự phòng và bỏ phần mở rộng trước khi giảm thời gian kiểm thử cuối kỳ.

# 28 Công việc tuần 1 đến tuần 4

## Tuần 1 Chốt thiết kế và dựng khung

**20 giờ cơ sở, 10 giờ dự phòng.** A tạo project, repo, action map và nhân vật thử. B chốt sơ đồ năm phòng, danh mục tài nguyên, ID và luồng nhiệm vụ; cả nhóm chọn máy đo hiệu năng. Nếu có C, C thử bộ hang động URP và font tiếng Việt trên một scene riêng.

**Cuối tuần:** build đầu tiên chạy trên hai máy; tài liệu phạm vi, phân công và giờ làm đã thống nhất. Không mua hoặc nhập nhiều asset trước khi thử khả năng tương thích.

## Tuần 2 Nhân vật camera và save tối thiểu

**26 giờ cơ sở, 4 giờ dự phòng.** A hoàn thiện đi chạy nhảy, pivot, va chạm camera và Animator nền. B tạo GameSession, checkpoint, catalog vật phẩm, DTO và luồng lưu tải tối thiểu. C có thể làm âm nền và khối phòng mà không sửa Player prefab chính.

**Cuối tuần:** đi qua dốc, bậc và góc tường ổn định; New Game và Continue đặt đúng checkpoint; không có dịch vụ hoặc camera bị tạo trùng khi đổi scene.

## Tuần 3 Tương tác và lồng đèn

**30 giờ cơ sở.** A làm scanner, mục tiêu hợp lệ, đèn và phong ấn. B dựng Vườn Rêu, ba văn tự và túi đồ tối thiểu; nối trạng thái đã nhặt vào save. Rà soát mask, ID và phản hồi khi thiếu năng lượng.

**Cuối tuần:** người thử nhận đèn, mở phong ấn, nhận mảnh A và lưu lại được; không nhặt hoặc chiếu xuyên tường.

## Tuần 4 Hai câu đố và đoạn chơi đầu

**30 giờ cơ sở.** B làm câu đố trống, A làm logic gương; thống nhất hệ Solved và cửa. Người dựng màn nối trại đến Điện Trống; triển khai gợi ý và thử với ít nhất một người chưa đọc thiết kế.

**Cuối tuần:** có đoạn chơi 8–12 phút; cả hai puzzle hoạt động trong scene thử, câu đố trống đã vào tuyến chính. Ghi lại những chỗ người thử không hiểu để sửa tuần 5.

# 29 Công việc tuần 5 đến tuần 8

## Tuần 5 Combat người chơi và AI nền

**28 giờ cơ sở, 2 giờ dự phòng.** A làm đòn dao, vùng đánh, thể lực, né và sát thương. B bake NavMesh, làm Patrol, Chase, Return và cấu hình Hắc Ảnh. Dành một lượt tích hợp chung để xác nhận địch nhận tác động của lồng đèn.

**Cuối tuần:** trận nhỏ với một địch hoàn chỉnh; không trúng nhiều lần trong một đòn, không bất tử vô hạn và không đánh xuyên tường. Câu đố gương được đặt vào Hành Lang Quang.

## Tuần 6 Hộ vệ Đá và kinh tế vật phẩm

**28 giờ cơ sở, 2 giờ dự phòng.** A làm hai đòn Hộ vệ, khóa đấu trường và trạng thái chết. B hoàn thiện nguyên liệu, thuốc, TryCraft và panel trại; đặt số lượng tài nguyên hữu hạn theo tuyến. Rà soát khả năng hoàn thành khi không có thuốc.

**Cuối tuần:** thắng, thua và đánh lại Hộ vệ được. Người chơi biết dùng đèn làm yếu và có khoảng phản công; không có công thức khiến mất đồ khi túi đầy.

## Tuần 7 Toàn tuyến và lưu tải đầy đủ

**28 giờ cơ sở, 2 giờ dự phòng.** B hoàn thiện world state, ID, nhiệm vụ suy ra từ dữ kiện và các snapshot. A xử lý hồi sinh, reset trạng thái, cửa đấu trường và kết chương. Cả nhóm chạy một lượt từ menu tới kết thúc, sau đó thử các ảnh chụp lưu giữa tuyến.

**Cuối tuần:** tuyến chính hoàn thành bằng tài nguyên tạm; không có điều kiện tiến trình phụ thuộc vào thứ tự event hoặc tên object.

## Tuần 8 Tích hợp bản chơi hoàn chỉnh

**26 giờ cơ sở, 4 giờ dự phòng.** Thay dần tài nguyên, làm HUD và nhật ký, sửa gợi ý, âm phản hồi và camera. Mời người thử mới hoàn thành toàn tuyến, ghi thời gian và điểm kẹt.

**Cuối tuần:** build tích hợp 0.8 hoàn thành chương, Continue và hồi sinh đúng. Chỉ khi đạt mốc này mới chọn nỏ, móc dây hoặc nhánh phụ; không nhận đồng thời mọi phần mở rộng chỉ vì có người thứ ba.

# 30 Công việc tuần 9 đến tuần 12

## Tuần 9 Hoàn thiện cảm giác chơi và nội dung

**26 giờ cơ sở, 4 giờ dự phòng.** Sửa nhịp đòn, khoảng phản công, gợi ý puzzle và bố trí tài nguyên từ kết quả thử. Hoàn thiện vật liệu, ánh sáng, âm thanh và chuyển cảnh. Nếu đã duyệt phần mở rộng, người phụ trách tích hợp qua nhánh riêng và có ca kiểm thử riêng.

**Cuối tuần:** toàn tuyến có hình ảnh và âm thanh mục tiêu; các cơ chế bắt buộc có phản hồi đủ rõ trên cấu hình thấp và vừa.

## Tuần 10 Khóa nội dung và tối ưu

**22 giờ cơ sở, 8 giờ dự phòng.** Không thêm cơ chế mới sau cuối tuần. Đo build trên máy tham chiếu, sửa điểm nghẽn, kiểm tra bộ nhớ, độ phân giải và điều khiển. Chạy lại bộ ca tiến trình và save sau khi đổi scene hoặc prefab.

**Cuối tuần:** chốt danh sách nội dung, nguồn asset và known issues. Nỏ hoặc móc dây chưa qua kiểm tra phải tắt bằng cờ tính năng và không nằm trên đường bắt buộc.

## Tuần 11 Kiểm thử hồi quy và bản ứng viên

**20 giờ cơ sở, 10 giờ dự phòng.** Hai người đổi vai kiểm tra chức năng của nhau. Thực hiện các ca file hỏng, thoát mở game, chết tại các mốc, chơi mới sau khi đã hoàn thành và đổi scene nhiều lần. Hoàn thiện bảng số liệu đo và hướng dẫn chạy.

**Cuối tuần:** bản RC1 không còn lỗi nghiêm trọng hoặc lỗi chặn tiến trình. Ghi chính xác commit, phiên bản Editor và bộ gói đã dùng để tạo RC1.

## Tuần 12 Bàn giao và diễn tập báo cáo

**16 giờ cơ sở, 14 giờ dự phòng.** Chỉ sửa lỗi ảnh hưởng bàn giao. Tạo release, kiểm tra trên máy khác, chuẩn bị video dự phòng và ảnh minh chứng. Diễn tập demo 8–10 phút ít nhất hai lượt với đúng bản build nộp.

**Cuối tuần:** tag v1.0-demo, source và thư mục build đầy đủ, README, tài liệu, kết quả kiểm thử, nguồn tài nguyên và video đã sẵn sàng. Dự phòng cuối kỳ dùng cho sự cố máy, lỗi đóng gói và lỗi phát hiện muộn.

# 31 Phân công và quản lý công việc

## Khi có hai thành viên

**A phụ trách nền điều khiển và chiến đấu:** Player, camera, lồng đèn, hệ sát thương, AI và Hộ vệ. **B phụ trách môi trường và tiến trình:** khối màn, puzzle, inventory, chế thuốc, nhiệm vụ, save và UI. Cả hai cùng rà soát save, tích hợp và build; không để một người giữ toàn bộ kiến thức xuất bản.

Người chủ trì có trách nhiệm chuẩn bị prefab và scene kiểm tra; người còn lại rà soát các điều kiện biên. A hỗ trợ logic gương và B hỗ trợ cấu hình AI để cân bằng tải ở tuần 4–6.

## Khi có ba thành viên

C nhận màn chơi, tài nguyên, UI, âm thanh và tổ chức kiểm thử. A và B nhờ đó có thời gian hoàn thiện hành vi và sửa lỗi. Với giả định C cũng làm 15 giờ mỗi tuần, 180 giờ tăng thêm có thể phân như sau. Trước tuần 8, ưu tiên nâng chất lượng bản chính; sau mốc tích hợp chỉ chọn tối đa một phần mở rộng phù hợp thời gian còn lại.

| **Hạng mục tăng thêm** | **Giờ dự kiến gồm kiểm thử** | **Điều kiện** |
| --- | --- | --- |
| Một phần mở rộng | Tối đa 36 | Chọn nỏ 32 giờ, móc dây 36 giờ hoặc phòng phụ 32 giờ |
| Môi trường hoạt ảnh VFX âm thanh | 52 | Làm xuyên suốt kỳ, giữ ngân sách hiệu năng |
| Thử với người mới và kiểm thử bổ sung | 52 | Tăng độ rõ của puzzle, độ ổn định và hồ sơ |
| Dự phòng và rà soát chung | 40 | Ưu tiên sửa lỗi và ổn định |

## Nhịp làm việc

Đầu tuần chọn các việc đủ giờ khả dụng và xác định người phụ thuộc. Giữa tuần tích hợp trên nhánh chung; cuối tuần tạo build và nghiệm thu. Tránh chờ hết nhiều tuần mới ghép scene, prefab và code.

Mỗi thẻ công việc có ID, kết quả cần có, người chủ trì, giờ dự kiến, phụ thuộc và ca kiểm tra. Dùng trạng thái Chưa làm, Đang làm, Chờ tích hợp, Chờ kiểm tra, Hoàn thành. Tài nguyên lớn và scene dùng chung có người giữ quyền chỉnh tại một thời điểm để giảm xung đột.

**Ví dụ thẻ:** LTN-03 Xóa phong ấn; phụ thuộc tương tác và ID; hoàn thành khi giữ Focus 1,5 giây có đường nhìn mở seal đúng một lần và Continue không đóng lại.

# 32 Thiết kế nỏ và móc dây mở rộng

## Nỏ thần

Tạo WeaponDefinition cho sát thương 15, hồi bắn 1 giây và tiêu 20 năng lượng linh khí mỗi phát. Nếu dùng chung năng lượng đèn, không bắn khi thiếu 20; luôn hồi theo luật đèn. Phím 1 dùng dao, phím 2 dùng nỏ. Khi cầm dao, chuột phải Focus đèn; khi cầm nỏ, chuột phải ngắm và Focus đèn tạm không khả dụng. HUD phải chỉ đúng ngữ cảnh.

Projectile có hướng ban đầu theo điểm ngắm nhưng sinh tại đầu nỏ; kiểm tra vật cản từ nỏ đến điểm ngắm để không bắn xuyên góc tường. Dùng Rigidbody cho đường bay hoặc phép quét theo đoạn giữa hai vị trí để bắt va chạm ở tốc độ cao. Bỏ qua collider chủ thể, mỗi projectile chỉ xử lý một va chạm gây sát thương rồi trả pool hoặc hủy.

**Nghiệm thu:** đạn không tự trúng An, không xuyên tường mỏng khi FPS thấp, không bắn khi UI mở, không tốn hai lần năng lượng cho một phát. Nỏ không cần để mở cửa bắt buộc hoặc tiêu diệt một địch chỉ nhận đúng loại vũ khí này.

## Móc dây tại điểm cố định

Dùng GrappleAnchor đặt sẵn và một cặp điểm đầu cuối đã kiểm tra. Bản mở rộng mô phỏng kéo người theo một đường định sẵn, không mô phỏng dây tự do. Tầm bắt đầu khoảng 12 m; chỉ hiện nhắc khi nhìn thấy anchor và đường đi an toàn.

Khi bắt đầu, chuyển PlayerState sang Traversal, tạm chặn Attack và Jump; đường di chuyển vẫn qua phép kiểm tra capsule. Nếu có vật cản mới hoặc điểm đến không hợp lệ, hủy an toàn về vị trí hợp lệ gần nhất. Chết hoặc đổi scene hủy toàn bộ tiến trình và hiệu ứng dây.

Móc dây không tự cấp bất tử. Không lưu tọa độ giữa chuyển động; Continue trở về checkpoint. Dùng một đoạn móc dây làm lối tắt hoặc nhánh đồ phụ để bản chính vẫn chơi được khi tính năng bị tắt.

**Nghiệm thu:** không xuyên trần, không đến điểm không đứng được, không khóa motor sau khi ngắt, camera không lật. Thử từ nhiều vị trí và ở FPS thấp trước khi dùng hoạt ảnh hoàn chỉnh.

# 33 Hệ thống cho toàn game sau chương đầu

| **Hệ thống** | **Thiết kế dữ liệu và xử lý** | **Trình tự phát triển** |
| --- | --- | --- |
| Leo trèo | ClimbVolume, điểm bám và điểm đứng; PlayerState Climb; kiểm tra capsule điểm ra | Leo bậc định sẵn trước, bám vách tự do sau |
| Thuyền sông ngầm | BoatController, điểm lên xuống, trạng thái Mounted; chỉ một bộ nhận input | Nước phẳng và bến cố định trước, dòng chảy sau |
| Nỏ nâng cấp | WeaponDefinition, projectile, điểm ngắm và loại tác động | Hoàn thiện một loại tên trước khi thêm biến thể |
| Kỹ năng | SkillDefinition, XP, điều kiện mở và hiệu ứng chỉ số | Bổ sung khi có đủ thử thách để kỹ năng có ý nghĩa |
| Công thức mới | RecipeDefinition và giao dịch chung | Thuốc thể lực trước, tránh nhiều nguyên liệu không có công dụng |
| Puzzle nhiều chương | Điều kiện, trạng thái, đối tượng tác động và ID | Tái dùng cơ sở solved và save; mỗi chương thêm luật riêng |
| NPC và đối thoại | DialogueDefinition, cờ hội thoại, lựa chọn có điều kiện | Đối thoại tuyến tính trước khi phân nhánh |

## Leo và thuyền không được tranh quyền với motor

Khi leo, motor chuyển chế độ và animation chỉ có một nguồn dịch chuyển được chọn. Kiểm tra không gian đứng phía trên trước khi cho leo. Khi lên thuyền, tắt điều khiển đi bộ, gắn vị trí hành khách và chuyển camera; khi xuống phải có điểm đứng an toàn. Không cho cả nhân vật và thuyền cùng xử lý WASD.

## Mở rộng dữ liệu lưu

Khi có các vùng và phương tiện mới, tăng schemaVersion có quy trình migrate. Lưu vùng đang chơi và các điểm an toàn, trạng thái cửa van, phương tiện ở bến và danh sách kỹ năng. Chỉ lưu vị trí vật động tự do nếu thiết kế thực sự cho phép người chơi giữ chúng giữa hai phiên.

## Lộ trình tiếp tục

Sau bản chương 1, đo thời gian thật đã tiêu tốn cho từng cơ chế. Từ đó ước tính riêng các chương Chim Lạc, Sơn Tinh và Thủy Tinh; mỗi chương cần một bản thử hoàn chỉnh và kiểm thử save xuyên vùng trước khi làm nội dung lớn. Chưa ấn định toàn bộ chiến dịch bằng cách nhân thời lượng một phòng thử.

# 34 Chọn kỹ thuật từ tài liệu nguồn

| **Kỹ thuật trong danh mục nguồn** | **Áp dụng cho Sơn Đoòng** | **Mức ưu tiên** |
| --- | --- | --- |
| FPS controller và Mouse Look | Thay bằng third person, camera theo pivot | Bắt buộc sau điều chỉnh |
| Camera phụ cho vũ khí FPS | Không dùng mặc định; xử lý camera và socket ở góc nhìn ba | Chỉ thêm nếu có nhu cầu cụ thể |
| Raycast và trigger | Tương tác, đường nhìn, vùng nhiệm vụ | Bắt buộc |
| Nhặt và thả đồ | Nhặt có kiểm tra chỗ; thả đồ chưa cần | Nhặt bắt buộc |
| Inventory kéo thả | Túi chọn ô; kéo thả bổ sung sau | Chọn ô bắt buộc |
| Quick slots và trang bị | Q dùng thuốc, dao và đèn cố định | Bắt buộc |
| Crafting và độ bền | Một công thức thuốc; không dùng độ bền dao | Chế thuốc bắt buộc |
| FSM và NavMesh | Hắc Ảnh, Hộ vệ, tìm đường và hồi về | Bắt buộc |
| Spawner và respawn | Điểm sinh hữu hạn; khôi phục theo checkpoint | Bắt buộc ở mức đơn giản |
| Melee và Animation Events | Hitbox theo thời gian logic, event cho âm và VFX | Bắt buộc |
| Cung hoặc nỏ và đạn đạo | Nỏ thần và projectile | Mở rộng có điều kiện |
| Máu và thể lực | Chiến đấu, chạy, né, hồi phục | Bắt buộc |
| Quest và XP | Tuyến nhiệm vụ chính; XP dành cho toàn game | Quest bắt buộc |
| PlayerPrefs và JSON | Cài đặt và tiến trình tách riêng | Bắt buộc |
| Mã hóa hoặc binary | Không cần chống sửa save trong demo ngoại tuyến | Sau nếu có yêu cầu |
| Events và quản lý audio | Kết nối UI và luật chơi, mixer theo nhóm | Bắt buộc |

## Những cơ chế chưa thuộc game hiện tại

Cưỡi ngựa, chặt cây, đào khoáng, xây nhà module, trồng trọt, nấu ăn nhiều công thức, câu cá, đói khát, nhiệt độ và thời tiết động thuộc vòng chơi sinh tồn. Chúng có thể thành một dự án khác hoặc được bổ sung khi câu chuyện và thiết kế màn cần đến; việc có tên trong danh mục kỹ thuật không làm chúng thành yêu cầu của chương phiêu lưu này.

Ngày đêm chỉ dùng làm thiết lập mỹ thuật theo phòng. Toàn chương ở trong hang nên không cần mô phỏng một đồng hồ thiên văn hoặc thời tiết ngoài trời để hoàn thành mục tiêu đã chọn.

# 35 Kiểm thử chức năng và tiến trình

| **Mã** | **Thao tác kiểm tra** | **Kết quả phải đạt** |
| --- | --- | --- |
| CT01 | Đi chéo, chạy, nhảy ở bậc dốc và sát trần | Tốc độ đúng, không xuyên hoặc nhảy vô hạn |
| CT02 | Quay camera ở góc tường và hầm thấp | Không lọt vào tường, vẫn thấy nhân vật |
| IN01 | Nhìn vật qua tường hoặc xa hơn 3 m rồi bấm E | Không tương tác; không hiện nhắc sai |
| IN02 | Bấm E nhiều lần vào cùng mảnh | Có đúng một ID và một phần thưởng |
| LT01 | Chiếu seal, rời giữa chừng, chiếu lại | Tiến độ theo luật, không mở khi chưa đủ |
| LT02 | Dùng hết năng lượng rồi chờ hồi | Không kẹt tiến trình, Focus hoạt động lại |
| PZ01 | A C B; A B; spam các mặt trống | Đúng mới mở, sai thử lại, không nhận trùng |
| PZ02 | Thử 16 tổ hợp gương và một tường chắn | Tia bị chặn đúng, không nhận xuyên tường |
| CB01 | Một đòn chạm nhiều collider của một địch | Trừ máu đúng một lần |
| CB02 | Né trước, trong và sau cửa sổ bất tử | Chỉ được bảo vệ ở khoảng quy định |
| AI01 | Nấp sau tường, chạy xa rồi quay lại | Phát hiện có LOS, Return trong giới hạn |
| AI02 | Dẫn địch qua cửa và mép vực | Không xuyên cửa hoặc rơi khỏi NavMesh |
| IT01 | Chế khi đủ, thiếu hoặc không có chỗ sau giao dịch | Thành công nguyên tử hoặc giữ nguyên túi |
| IT02 | Q khi đầy máu, thiếu thuốc hoặc đang chết | Không tiêu thuốc trái luật |
| QS01 | Đi lại bệ đã nhận và tải lại sự kiện | Không đếm tăng hoặc cấp lại |
| BS01 | Chết trong đấu trường rồi thắng ở lượt sau | Cổng, AI và mốc lưu đều đúng |

## Cách ghi nhận

Mỗi ca có build, máy, bước tái hiện, kết quả thực tế, đạt hoặc chưa đạt và người kiểm tra. Với lỗi khó thấy, đính kèm video ngắn hoặc ảnh vùng Gizmos. Không ghi “đã test” nếu chỉ thử trong Editor mà chưa kiểm tra bản build liên quan.

# 36 Kiểm thử lưu tải và chất lượng bản dựng

| **Mã** | **Tình huống** | **Kết quả phải đạt** |
| --- | --- | --- |
| SV01 | Lưu trước puzzle rồi Continue | Puzzle chưa giải, vật phẩm đúng snapshot |
| SV02 | Giải puzzle, nhận mảnh, lưu và mở lại game | Cửa mở, mảnh không sinh lại, nhiệm vụ đúng |
| SV03 | Nhặt và dùng đồ sau save rồi chết | Trả về toàn bộ snapshot, không trộn phiên |
| SV04 | Làm hỏng file chính, giữ backup tốt | Phục hồi backup và thông báo phù hợp |
| SV05 | File có schema mới hơn hoặc ID không tồn tại | Từ chối hoặc xử lý có kiểm soát, giữ file gốc |
| SV06 | Tải sau khi đã phục dựng Trống Đồng | Không có lại Hộ vệ, kết quả chương còn đúng |
| SV07 | Đặt hai object trùng PersistentId | Công cụ kiểm tra báo lỗi trước khi build |
| UI01 | Mở đóng menu khi giữ chuột và WASD | Không còn input gameplay bị treo |
| UI02 | Thử nhiều độ phân giải và tắt tiếng | Chữ không cắt, puzzle vẫn giải được |
| PF01 | Đi tuyến đo ba lần trong bản build | Có log frame time và cấu hình đầy đủ |
| RL01 | Chơi hết chương ở máy không cài Unity | Từ menu đến kết thúc không thiếu dữ liệu |

## Mẫu bảng hiệu năng

| **Build và máy** | **Trung vị ms** | **P95 ms** | **Bộ nhớ đỉnh** | **Đoạn nặng và ghi chú** |
| --- | --- | --- | --- | --- |
| Chưa đo | Chưa đo | Chưa đo | Chưa đo | Điền sau lượt đo tuần 10 |

Không thay ô chưa đo bằng số ước lượng. So sánh hai phiên bản chỉ khi cùng máy, độ phân giải, chất lượng, tuyến đi và chế độ build. Ghi thay đổi đã thực hiện giữa hai lượt.

## Điều kiện được bàn giao

Không còn crash, mất save, nhân đôi vật phẩm hoặc lỗi chặn hoàn thành. Các lỗi ảnh hưởng điều khiển, camera và giao diện chính đã sửa. Lỗi nhỏ còn lại có mô tả, vị trí và cách tránh; không dùng known issues để hợp thức hóa một tính năng bắt buộc chưa hoạt động.

# 37 Rủi ro và cách xử lý lỗi thường gặp

| **Rủi ro hoặc dấu hiệu** | **Cách xác định nguyên nhân** | **Hành động** |
| --- | --- | --- |
| Phạm vi tăng liên tục | Thẻ mới không thuộc tuyến hoặc mốc tuần | Đưa vào backlog, giữ khóa nội dung tuần 10 |
| Model hoặc Terrain màu tím | Shader không thuộc URP hoặc tham chiếu thiếu | Thử chuyển vật liệu trong bản sao, gán URP/Lit |
| Lỗi input cũ và mới | Script dùng Input.GetAxis trong project Input System | Chuyển cùng action asset, không vá nhiều hệ song song |
| Camera rung hoặc xoay theo vòng | Cả model và pivot cùng ghi rotation | Tách trách nhiệm, chỉ một nơi đọc Look |
| Nhân vật không nhảy | Ground mask, controller, trần hoặc state sai | Vẽ kiểm tra đất, xem vận tốc Y và trạng thái |
| AI đứng yên | Agent ngoài mesh, sai Agent Type hoặc đường bị chặn | Kiểm tra isOnNavMesh, bake và mask |
| Vũ khí gây nhiều hit | Nhiều collider và không có tập mục tiêu mỗi đòn | Gộp theo IDamageable gốc, reset đúng mỗi đòn |
| Cửa reset sau Continue | ID sai hoặc ApplySolvedState chưa chạy | Áp world state trước khi bật điều khiển |
| Event chạy hai lần | Đăng ký lặp khi đổi scene hoặc bật panel | Kiểm tra OnEnable và OnDisable |
| Màn đẹp nhưng chạy chậm | Nhiều bóng động, transparency hoặc VFX | Dùng Profiler, sửa điểm nghẽn được đo |

## Khi chậm tiến độ

Nếu tuần 4 chưa có đoạn chơi đầu, dừng trang trí để sửa đường tương tác và puzzle. Nếu tuần 8 chưa đi được toàn tuyến, bỏ nỏ và móc dây, giảm số địch và đoạn phim; giữ save, dấu hiệu puzzle và kiểm thử. Nếu tuần 10 vẫn còn lỗi chặn tiến trình, chỉ sửa lỗi và chuẩn bị bản dự phòng.

## Tài nguyên và phụ thuộc

Ưu tiên một bộ nhân vật, một bộ đá hang, một bộ hoạt ảnh và phong cách vật liệu thống nhất. Thử asset trong scene riêng trước khi merge. Lưu nguồn, tác giả, điều kiện sử dụng và phiên bản; tránh đổi đồng thời Editor, URP và bộ shader trong giai đoạn tích hợp.

# 38 Hồ sơ bàn giao và kịch bản báo cáo

## Gói sản phẩm

1. Build_Windows chứa toàn bộ thư mục build, phiên bản và mã commit tương ứng.

2. Source có Assets, Packages, ProjectSettings và tệp meta; có hướng dẫn cài Editor, mở scene Boot và build lại.

3. README mô tả nội dung, phím điều khiển, cách lưu, cách hồi sinh, cấu hình đã thử và các lỗi nhỏ còn lại.

4. Tài liệu thiết kế hệ thống, bảng công việc đã hoàn thành và nhật ký thay đổi phạm vi.

5. Bảng kiểm thử có kết quả thực tế, cấu hình máy và bảng hiệu năng; ảnh hoặc video minh chứng.

6. Asset_Sources ghi tên, tác giả, URL, quyền sử dụng và phần được dùng của từng gói hoặc file.

## Demo 8–10 phút

| **Thời gian** | **Nội dung trình diễn** | **Điều cần cho thấy** |
| --- | --- | --- |
| 0–1 phút | Bối cảnh, mục tiêu chương và quy mô nhóm | Game có vòng chơi rõ ràng |
| 1–3 phút | Di chuyển, lồng đèn, phong ấn | Tương tác và ánh sáng gắn với gameplay |
| 3–5 phút | Trống và gương ở trạng thái dễ trình diễn | Dấu hiệu, logic giải và thay đổi môi trường |
| 5–7 phút | Hắc Ảnh hoặc Hộ vệ, né và làm yếu | Báo hiệu đòn, phản hồi sát thương |
| 7–8 phút | Lưu, quay menu và Continue | Cửa, vật phẩm, nhiệm vụ được khôi phục |
| 8–10 phút | Kiến trúc, kết quả kiểm thử và hướng tiếp | Nêu số liệu đã đo, giới hạn và đóng góp |

Chuẩn bị các snapshot demo riêng cho đúng phiên bản build; không ghi đè vào ô lưu cá nhân của người thử. Snapshot hỗ trợ trình diễn nhưng vẫn phải có một video hoặc lượt chơi từ đầu đến cuối chứng minh tuyến chính hoàn thành được.

## Checklist cuối cùng

Mở build trên máy thứ hai; kiểm tra font và âm; xác nhận Continue sau khi tắt hẳn ứng dụng; chép đủ thư mục; ghi tag và checksum gói bàn giao nếu nhóm sử dụng; diễn tập không cần Internet hoặc Editor. Chỉ cập nhật build nộp sau khi chạy lại các ca bắt buộc có liên quan.

# 39 Nguồn tham khảo và hướng học

## Tài liệu ý tưởng và kỹ thuật của dự án

**D1** Tên dự án.docx. Nguồn cho tên game, nhân vật An, Sơn Đoòng, Hắc Sương, các Linh Thần, Lồng Đèn Sen, Trống Đồng, ánh sáng, chiến đấu và định hướng nghệ thuật.

**D2** Các kỹ thuật làm game.docx. Danh mục kỹ thuật để đối chiếu điều khiển, tương tác, inventory, chế tạo, AI, chỉ số, nhiệm vụ, lưu và kiến trúc. Phần 34 chỉ rõ mức áp dụng từng nhóm.

## Tài liệu Unity chính thức

Các nguồn dưới được kiểm tra ngày 29/10/2026. Dùng đúng nhánh tài liệu tương ứng phiên bản package đã khóa; tài liệu API trong kế hoạch là hướng dẫn triển khai, còn thông số gameplay là các giá trị thiết kế đề xuất.

**S1** Unity 6 Releases and Support. Cơ sở chọn dòng LTS và giữ phiên bản ổn định.

[https://unity.com/releases/unity-6/support](https://unity.com/releases/unity-6/support)

**S2** Cinemachine cho Unity 6000.3. Thông tin tương thích dòng Cinemachine 3.1.

[https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/packages-all/pack-safe/com-unity-cinemachine](https://docs.unity.com/en-us/engine/6000.3/manual/packages-list/packages-all/pack-safe/com-unity-cinemachine)

**S3** Cinemachine 3.1 Third Person Follow. Cấu trúc target và xử lý va chạm camera.

[https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineThirdPersonFollow.html](https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/manual/CinemachineThirdPersonFollow.html)

**S4** AI Navigation 2.0 NavMeshSurface. Thiết lập bề mặt và bake vùng tìm đường.

[https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavMeshSurface.html](https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/NavMeshSurface.html)

**S5** CharacterController.Move. Chuyển động, va chạm và việc tự xử lý trọng lực.

[https://docs.unity3d.com/6000.0/Documentation/ScriptReference/CharacterController.Move.html](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/CharacterController.Move.html)

**S6** Unity JSON Serialization. Các kiểu dữ liệu và giới hạn của JsonUtility.

[https://docs.unity3d.com/6000.0/Documentation/Manual/json-serialization.html](https://docs.unity3d.com/6000.0/Documentation/Manual/json-serialization.html)

**S7** Application.persistentDataPath. Vị trí dữ liệu bền vững theo nền tảng.

[https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-persistentDataPath.html](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Application-persistentDataPath.html)

**S8** Render pipeline feature comparison. Phân biệt fog và tính năng giữa URP và HDRP.

[https://docs.unity.com/en-us/engine/6000.0/manual/render-pipelines/choose-a-render-pipeline/feature-comparison](https://docs.unity.com/en-us/engine/6000.0/manual/render-pipelines/choose-a-render-pipeline/feature-comparison)

## Học theo thứ tự phục vụ sản phẩm

Học component, prefab, scene và Inspector trước; tiếp theo là CharacterController, Input System và Animator. Sau đó làm raycast, interface, dữ liệu ScriptableObject, FSM và NavMesh. Chỉ học shader, IK hoặc tối ưu sâu khi hệ thống hiện có đặt ra một vấn đề cụ thể cần giải quyết.

Sơn Đoòng Cõi Mộng Kỳ Thần  |  1
# 40 Điều chỉnh kế hoạch ngày 01/11/2026

## Lý do và phạm vi mới

Ngày 01/11/2026, người dùng chốt hướng mới cho toàn game sau khi xem kết quả chạy thử chương 1. Game được mở rộng thành **5 chương ngắn** (Kim Quy, Chim Lạc, Sơn Tinh, Thủy Tinh và chương kết Trục Linh Mạch), có thêm lớp **lịch sử – giáo dục**, **hai cái kết** và **bộ di chuyển do Kim Quy trao**. Thời gian sản xuất thực tế khoảng **1 tuần**, nên mọi chương được rút gọn thành các phòng nhỏ, chơi nhanh, ít kỹ thuật nhưng phải làm trọn vẹn.

Các phần 40–54 là bản điều chỉnh. Khi mâu thuẫn với các phần 1–39, **ưu tiên phần 40–54**. Các luật kỹ thuật cũ (một motor, một hệ input, WorldStateStore, save nguyên tử, quest suy từ dữ kiện, thông số đèn, câu đố trống và gương) vẫn giữ nguyên.

| Phần cũ | Nội dung thay đổi |
| --- | --- |
| 1 Phạm vi | Từ 1 chương 25–35 phút thành 5 chương ngắn, tổng mục tiêu 65–95 phút |
| 2 Bản đồ toàn game | Chương 2–5 được thiết kế chi tiết tại phần 46–49; mỗi chương chỉ có 1 cơ chế mới |
| 10 Né trong chương 1 | Chương 1 **không có lộn né**. Toàn bộ bộ di chuyển nâng cao là quà Kim Quy khi hoàn thành chương 1 (phần 43) |
| 8 Scene và build | Thêm scene cho chương 2–5 (phần 50) |
| 18–19 Dữ liệu lưu | Mở rộng schema cho chương, năng lực, tri thức, cơ chế ẩn và cái kết (phần 50) |
| 20 Nhật ký | Thêm mục Tri thức; câu hỏi kiến thức bắt đầu từ chương 2 |
| 27–30 Lịch 12 tuần | Lịch sản xuất thực tế là 7 ngày (phần 52). Phần 27–30 giữ làm tham chiếu |
| 32 Nỏ và móc dây | Không làm. Không có đu dây, cưỡi chim, bơi trong toàn game |

# 41 Nguyên tắc sản xuất bắt buộc

1. **Cơ chế trước, đồ họa sau.** Mỗi chương chơi được từ đầu đến cuối bằng khối cơ bản (logic, tương tác, UI blockout, lưu tải, luồng màn) trước khi làm đồ họa.
2. **Concept đồ họa cố định:** Stylized Semi-Realistic, huyền ảo cổ tích Việt, cảm hứng Đông Sơn – Hồng Bàng. Trước khi import phải trình concept và danh sách asset cho người dùng duyệt. Không tải hoặc dựng asset tràn lan.
3. **Cơ chế tinh gọn, có ý nghĩa với cốt truyện.** Mỗi chương chỉ thêm **một** cơ chế mới, lấy từ chính Linh Thần của chương đó. Lồng Đèn Sen và Trống Đồng là công cụ nền xuyên suốt.
4. **Đặt độ phức tạp vào môi trường thay vì nhân vật.** Gió, cột đất, mặt nước là vật thể môi trường. Nhân vật An chỉ dùng một bộ animation làm ở chương 1.
5. **Chức năng nào đã đưa vào phải làm tới nơi:** có phản hồi, có lưu tải, có kiểm thử. Không để tính năng dở dang trong bản nộp; nếu không kịp thì cắt hẳn theo thứ tự ở phần 52.
6. **Hỏi người dùng tại điểm then chốt:** góc nhìn, cách biến đổi môi trường, di chuyển mới, diễn biến cốt truyện, duyệt asset.
7. **Lịch sử – giáo dục chính xác:** mỗi nội dung gắn nhãn Lịch sử, Truyền thuyết hoặc Tự nhiên; đối chiếu nguồn tin cậy trước khi đưa vào build; không hỏi phần hư cấu của game như sự thật.
8. **Một mức độ khó mặc định.** Không có menu chọn độ khó.

# 42 Khuôn chung của một chương

Mỗi chương 2–4 gồm **5 phòng gọn**, mỗi phòng khoảng 12 × 12 m (đấu trường khoảng 18 × 18 m), chơi 2–3 phút một phòng, cả chương **10–15 phút**. Phòng được dựng bằng prefab blockout dùng chung để làm nhanh.

| Phòng | Vai trò | Phần thưởng |
| --- | --- | --- |
| P1 Mở đầu | Nhận năng lực của chương, học cách dùng ở nơi an toàn; lửa trại (lưu, chế thuốc); Bia văn tự 1 | **Mảnh cổ vật 1** qua Bệ Vấn Đáp |
| P2 Thử thách nhỏ | Dùng năng lực để tới bệ; Bia văn tự 2 | **Mảnh cổ vật 2** qua Bệ Vấn Đáp |
| P3 Thử thách lớn | Câu đố chính của chương; mốc lưu giữa chương; Bia văn tự 3 | **Mảnh cổ vật 3** qua Bệ Vấn Đáp |
| P4 Giao chiến | Đánh quái thường kết hợp năng lực; Bia văn tự 4; mốc lưu trước đấu trường | **Công cụ trận trùm** và thuốc qua Bệ Vấn Đáp |
| P5 Đấu trường | Cổng kiểm tra đủ 3 mảnh và công cụ; đánh trùm; phục dựng cổ vật; Bia văn tự 5; **Linh Thần Vấn Đáp** 3 câu; Linh Thần thức tỉnh, chuyển chương | Năng lực được giữ vĩnh viễn, mở chương sau |

- Mảnh cổ vật ở P1–P3 dựng nên cốt truyện; công cụ ở P4 phục vụ trận trùm ở P5.
- Người chơi phải vượt thử thách của phòng mới tới được bệ. Bệ Vấn Đáp là cách nhận vật phẩm, không thay cho thử thách.
- Chương 1 giữ cấu trúc 5 khu đã dựng (phần 3–4) và không có Bệ Vấn Đáp.
- Chương 5 có 6 phòng, dài và khó hơn (phần 49).

# 43 Bộ di chuyển "Quà Kim Quy" và chiến đấu

## Chương 1

An chỉ có đi, chạy, nhảy, đánh dao, Lồng Đèn và khóa mục tiêu. Không có lộn né. Chiến đấu chương 1 được cân bằng để né bằng cách chạy ra khỏi vùng báo trước: đòn quét của Hộ vệ đánh phía trước, bước sang ngang là tránh được; đòn nện có vòng cảnh báo bán kính 3 m trong 1,2 giây, chạy 5,5 m/s thoát được khoảng 6,6 m. Đèn làm yếu địch là cách chính để rút ngắn trận. Thể lực chương 1 chỉ tiêu cho chạy (8/giây) và đánh dao (10/đòn).

## Quà Kim Quy

Khi phục dựng Trống Đồng và hoàn thành chương 1, Kim Quy trao **Phúc Kim Quy**. Toàn bộ bộ di chuyển nâng cao được mở khóa vĩnh viễn, lưu bằng cờ `movement_kit` và dùng cho chương 2–5. Phòng P1 chương 2 có khu hướng dẫn an toàn cho các thao tác này.

| Thao tác | Phím | Hành vi | Chi phí |
| --- | --- | --- | --- |
| Lộn theo hướng | Ctrl + W/A/S/D | Lộn theo hướng phím, tính theo camera | 25 thể lực, 0,45 giây, bất tử 0,10–0,30 giây |
| Lộn khi không bấm hướng | Ctrl | Lộn về hướng An đang nhìn (giữ luật phần 10) | Như trên |
| Chạy + lộn | Shift rồi Ctrl | Lộn xa khoảng 1,5 lần | Như trên |
| Lộn + chém | Chuột trái ngay khi kết thúc lộn | Đâm tới, sát thương ×1,25 | 10 thể lực |
| Né đúng lúc | Lộn khi đòn địch sắp trúng (trong 0,15 giây đầu của khoảng bất tử) | Chậm thời gian 0,3 giây, hồi 15 năng lượng đèn | Không thêm |
| Lộn chui khe thấp | Lộn vào khe thấp | Capsule hạ còn khoảng 0,9 m trong lúc lộn | Không thêm |

Khoảng giữa hai lần bắt đầu lộn tối thiểu 0,65 giây. Mọi chuyển động đi qua PlayerMotor nên vẫn bị tường chặn.

## Khóa mục tiêu

Bấm chuột giữa để khóa hoặc bỏ khóa mục tiêu gần nhất trong tầm nhìn. Khi khóa, camera giữ quái trong khung hình và An quay mặt về quái; lộn trái hoặc phải thành lộn ngang quanh mục tiêu. Mục tiêu chết, ra khỏi tầm 15 m hoặc bị che quá 1 giây thì tự bỏ khóa. Dùng được từ chương 1.

## Animation làm một lần

- Làm toàn bộ ở chương 1, dùng Humanoid retarget để chạy được cả trên nhân vật tạm và An bản cuối.
- Bộ clip: blend tree đứng, đi, chạy, chạy nhanh; bật nhảy, trên không, rơi, chạm đất; **4 clip lộn trước, sau, trái, phải**; chuỗi 2–3 nhát dao; đòn chém sau lộn; bị đánh; chết; tương tác; tư thế giơ đèn ở layer nửa thân trên.
- Nguồn: ưu tiên thư viện miễn phí A04 Universal Animation Library Standard, sau đó A06 Mixamo. Kiểm tra clip có thật trong gói trước khi dùng. Nếu không có đủ 4 clip lộn thì dùng 1 clip lộn và xoay thân theo hướng lộn, ghi lại trong danh sách asset.
- Tiêu chí mượt: không trượt chân, chuyển đổi có thời gian blend, lộn nối về đứng hoặc chạy không giật, Apply Root Motion tắt.

# 44 Hệ thống Tri thức

## Thành phần

| Thành phần | Cách hoạt động |
| --- | --- |
| Bia văn tự | Mỗi phòng 1 bia. Chữ chỉ hiện khi rọi đèn trong 5 m (chế độ sáng thường, phần 11). Đọc xong ghi vào Nhật ký, mục Tri thức |
| Bệ Vấn Đáp | Tương tác E mở bảng câu hỏi 4 lựa chọn, gameplay tạm dừng, dùng UI map. Đúng thì bệ mở và trao vật phẩm của phòng. Sai thì hiện lời giải thích, chỉ ra bia liên quan rồi cho trả lời lại; **vẫn nhận đủ đồ** |
| Linh Thần Vấn Đáp | Cuối mỗi chương 2–4, 3 câu hỏi trước khi Linh Thần thức tỉnh, luật trả lời như Bệ Vấn Đáp |
| Đại Vấn Đáp | Chương 5, 5 câu tổng hợp, trong đó có câu sắp xếp mốc thời gian |
| Điểm Tri Thức | Mỗi câu trả lời **đúng ngay lần đầu** được ghi ID. Điểm được suy ra từ tập ID này, không lưu bộ đếm riêng. Điểm mở dần các manh mối dẫn tới cơ chế ẩn của chương 5 (phần 49) |

## Quy tắc nội dung

- Mỗi bia và câu hỏi gắn nhãn **Lịch sử**, **Truyền thuyết** hoặc **Tự nhiên**. Truyền thuyết luôn ghi là truyền thuyết.
- Không hỏi về Hắc Sương, cách gọi Linh Thần, chuỗi gõ trống hay bất kỳ chi tiết hư cấu nào của game.
- Mọi nội dung đối chiếu SGK Lịch sử lớp 6, Bảo tàng Lịch sử Quốc gia, UNESCO, VQG Phong Nha – Kẻ Bàng hoặc nguồn tương đương trước khi đưa vào build. Mục ghi "cần kiểm chứng" chưa được đưa vào build khi chưa kiểm tra.
- Người chơi mục tiêu là mọi lứa tuổi: chương 2 câu dễ, nhận biết trực tiếp từ bia; chương 3–4 cần hiểu ý nghĩa; chương 5 tổng hợp và sắp xếp thời gian.
- Câu hỏi luôn trả lời được từ bia trong cùng chương, trừ Đại Vấn Đáp dùng bia của nhiều chương.

## Dữ liệu

QuizDefinition là ScriptableObject gồm ID, chương, nhãn, câu hỏi, 4 đáp án, chỉ số đúng, lời giải thích, nguồn tham chiếu và ID bia liên quan. LoreDefinition gồm ID, nhãn, nội dung, nguồn. Trạng thái đã đọc, đã trả lời, đúng lần đầu nằm trong WorldStateStore và được lưu.

# 45 Chương 1 Kim Quy: bổ sung hoàn thiện

Chương 1 giữ 5 khu và tuyến chính ở phần 3–4, thời lượng mục tiêu rút xuống **15–25 phút**. Ba văn tự của chương dùng nội dung thật, chưa có câu hỏi: Sơn Đoòng thuộc VQG Phong Nha – Kẻ Bàng; văn hóa Đông Sơn đặt theo tên làng Đông Sơn (Thanh Hóa); truyền thuyết Kim Quy giúp An Dương Vương xây thành Cổ Loa (Truyền thuyết).

## Sửa lỗi tồn đọng

| Mã | Việc | Điều kiện xong |
| --- | --- | --- |
| C1-F01 | Gộp hai file Input Actions còn một file PlayerControls với map Gameplay và UI; bỏ tham chiếu InputSystem_Actions | Chỉ một asset input được dùng; mọi phím ở phần 10 chạy đúng |
| C1-F02 | Bật Avoid Obstacles cho camera trong 90_TestMechanics | Camera không xuyên tường ở scene thử |
| C1-F03 | Thêm tường bao và vùng chết dưới sàn ở 91_TestCombat và mọi map | Rơi khỏi sàn thì hồi sinh tại mốc lưu |
| C1-F04 | HUD: tiếng Việt có dấu thống nhất bằng font Noto Sans; ẩn dòng debug trạng thái câu đố khỏi HUD chính, chỉ hiện khi ở gần câu đố; căn giữa màn chết | Không còn chữ không dấu, không chồng chữ |
| C1-F05 | Ký hiệu A, B, C trên trống to, khác hình, nhìn rõ từ lối vào phòng; gợi ý thứ tự chỉ hiện khi rọi đèn | Người chơi mới đọc được ký hiệu ở chất lượng thấp |
| C1-F06 | Lần đầu mở scene vật thể hiện màu xanh lơ do shader biên dịch bất đồng bộ trong Editor. Kiểm tra lại trong build Windows; nếu còn lỗi màu tím (vật liệu hỏng) thì chuyển vật liệu sang URP/Lit | Build không có vật liệu hồng hoặc tím |
| C1-F07 | Build Settings: 00_Boot, 01_MainMenu, 02_HangLinhMach và các scene chương ở phần 50; bỏ SampleScene | Build mở từ Boot |
| C1-F08 | DrumPuzzleController không reset lượt gõ dở khi world state đổi vì lý do khác | Nhặt vật trong lúc đang gõ không mất tiến độ |

## Thay đồ tạm bằng hệ thống chuẩn

| Mã | Việc |
| --- | --- |
| C1-R01 | Thay ThirdPersonController bằng PlayerMotor và InputReader theo phần 9–10; giữ thông số capsule, tốc độ, nhảy, camera |
| C1-R02 | CombatController: đòn dao 0,65 giây, khoảng sát thương 0,15–0,30 giây, tầm 1,8 m, một mục tiêu trúng một lần mỗi đòn; thể lực chạy 8/giây, đánh 10/đòn |
| C1-R03 | Hắc Ảnh: FSM Patrol → Chase → Attack → Search → Return, NavMeshAgent, tầm 10 m, góc 120°, leash 18 m; đèn chiếu 1 giây làm yếu ×1,5 trong 4 giây |
| C1-R04 | Khu 5 Đàn Kim Quy: cổng kiểm tra 3 mảnh và 2 câu đố, mốc lưu trước cổng, đấu trường khoảng 18 × 18 m khóa cổng khi vào |
| C1-R05 | Hộ vệ Đá: quét tay (báo 0,8 giây, 20 sát thương) và nện đất (báo 1,2 giây, bán kính 3 m, 25 sát thương); 180 máu; chết ghi guardian_01 |
| C1-R06 | Phục dựng Trống Đồng: relicRestored, cảnh kết chương bằng ảnh tĩnh và lời dẫn, **trao Quà Kim Quy**, lưu, chuyển sang chương 2 |
| C1-R07 | HUD dạng thanh: máu, thể lực, năng lượng đèn, số thuốc, mục tiêu |
| C1-R08 | Túi 12 ô chỉ xem, thuốc máu dùng bằng Q, chế thuốc tại lửa trại (1 lá cẩm + 1 linh chi) theo phần 17 |
| C1-R09 | Boot, Main Menu (Chơi mới, Tiếp tục, Cài đặt, Thoát), Pause, Settings (âm lượng, độ nhạy chuột, cửa sổ hoặc toàn màn hình) |
| C1-R10 | Chết và rơi vực hồi sinh tại mốc lưu; đủ 4 mốc cp_trai, cp_dien_trong, cp_truoc_dan, cp_kim_quy; F9 chỉ còn trong bản Development |
| C1-R11 | Đổi ID mảnh relic_drum_a/b/c sang fragment_a/b/c theo phần 17, chuyển đổi bản lưu cũ |
| C1-R12 | Bộ animation di chuyển và khóa mục tiêu ở phần 43 |

# 46 Chương 2 Chim Lạc: Giếng Trời

| Hạng mục | Thiết kế |
| --- | --- |
| Bố cục | 5 phòng đi lên cao, tổng chênh khoảng 30 m: P1 Đáy Giếng Gió → P2 Vách Gió → P3 Nhà Sàn Đá Treo → P4 Cầu Mây Gãy → P5 Đỉnh Tổ Chim (đấu trường tròn khoảng 18 m) |
| Cơ chế mới | **Cột Gió.** Lông Chim Lạc nhận ở P1 cho phép An bước vào cột gió để được nâng lên tới đỉnh cột rồi nhảy sang bệ khác. Cột gió là vùng trigger cộng vận tốc lên, An dùng clip rơi hoặc trên không. Một số cột chỉ bật khi tương tác với chuông gió trong phòng |
| Câu đố chính (P3) | Bật đúng 3 chuông gió theo thứ tự hình chim trên phù điêu để tạo chuỗi cột gió lên nhà sàn cao nhất |
| Quái thường | Hắc Ảnh; **Hắc Ảnh Ném**: cùng model, đứng trên gờ cao ném cầu sương chậm có báo trước |
| Công cụ P4 | **Lông Gió Lớn**: trong đấu trường, bật được cột gió trung tâm để lên ngang tầm lõi của trùm |
| Trùm | **Chim Lạc nhiễm Hắc Sương**. Đậu luân phiên trên 3 cột, lao xuống theo vệt cảnh báo 1 giây, quạt gió đẩy An về mép. Chiếu đèn khi chim đậu làm nó choáng; dùng cột gió trung tâm lên đánh. Máu 200 |
| Môi trường | Giếng trời có nắng xiên, rêu và dương xỉ, nhà sàn đá bám vách, hoa văn chim bay quanh mặt trời. Màu xanh lá, vàng nắng, đá xám |
| Cổ vật | 3 mảnh **Mặt Trời Lông Chim** của Trống Đồng |
| Cốt truyện | Hình mặt trời và vòng chim trên mặt trống là bản đồ linh mạch trong thế giới game. Hắc Sương tràn vào qua các giếng trời bị bịt kín |

## Tri thức chương 2 (dễ)

| Phòng | Bia | Nhãn |
| --- | --- | --- |
| P1 | Hố sụt ở Sơn Đoòng cho ánh sáng lọt xuống nên có rừng cây mọc trong lòng hang | Tự nhiên |
| P2 | Chính giữa mặt trống đồng Đông Sơn là ngôi sao nhiều cánh, thường được hiểu là hình mặt trời | Lịch sử |
| P3 | Hoa văn trống đồng có hình nhà sàn, kiểu nhà phổ biến của cư dân Lạc Việt | Lịch sử |
| P4 | Hoa văn trống đồng còn có người hóa trang lông chim, cảnh giã gạo, thuyền | Lịch sử |
| P5 | Chim Lạc thường được coi là biểu tượng của cư dân Lạc Việt | Lịch sử |

Câu hỏi mẫu: hình gì ở giữa mặt trống đồng; vì sao trong hang có rừng; hoa văn cho thấy cư dân Lạc Việt ở kiểu nhà nào.

# 47 Chương 3 Sơn Tinh: Thành Đá

| Hạng mục | Thiết kế |
| --- | --- |
| Bố cục | 5 phòng đi xuống tầng giữa: P1 Cổng Thành Đá → P2 Mỏ Cột Núi → P3 Sân Dáng Núi → P4 Hầm Hộ Vệ → P5 Điện Sơn Tinh (khoảng 18 × 18 m) |
| Cơ chế mới | **Ấn Núi.** Nhận ở P1. Tương tác E tại ô đất có dấu để chuyển khối đất qua 3 mức cao (0 m, 1,5 m, 3 m). Khối đất làm bậc thang, cầu qua vực hoặc tường chặn quái. Trạng thái mức của từng ô được lưu |
| Câu đố chính (P3) | Đặt 4 ô đất đúng độ cao theo hình "dáng núi" khắc trên tường thì cổng mở |
| Quái thường | Hắc Ảnh; **Hộ Vệ Đá Nhỏ**: dùng lại Hộ vệ Đá thu nhỏ, chỉ có đòn nện, 80 máu |
| Công cụ P4 | **Ấn Núi Lớn**: trong đấu trường, nâng được cùng lúc 4 cột chặn hang |
| Trùm | **Rết Đá**. Chui ra từ 4 hang theo thứ tự có rung đất báo trước 1,2 giây, quét thân thành đường thẳng. Nâng cột chặn hang thì rết bị kẹt và lộ đầu để đánh. Máu 220 |
| Môi trường | Thành đá, cổng xếp đá lớn, hoa văn núi, ánh lửa cam, bụi đá rơi khi khối đất chuyển động |
| Cổ vật | 3 mảnh **Vành Núi** của Trống Đồng |
| Cốt truyện | Hắc Sương khơi lại mối tranh chấp núi và nước. Sơn Tinh thức tỉnh và cảnh báo An về Thủy Tinh |

## Tri thức chương 3 (trung bình)

| Phòng | Bia | Nhãn |
| --- | --- | --- |
| P1 | Nhà nước Văn Lang do các vua Hùng đứng đầu, kinh đô Phong Châu (Phú Thọ ngày nay) | Lịch sử |
| P2 | Hang động đá vôi hình thành do nước hòa tan đá vôi trong thời gian rất dài | Tự nhiên |
| P3 | Hùng Vương thứ 18 kén rể cho công chúa Mỵ Nương | Truyền thuyết |
| P4 | Sính lễ gồm voi chín ngà, gà chín cựa, ngựa chín hồng mao; Sơn Tinh mang đến trước | Truyền thuyết |
| P5 | Sơn Tinh là thần núi Tản Viên (Ba Vì) | Truyền thuyết |

Câu hỏi mẫu: kinh đô Văn Lang; sính lễ vua Hùng yêu cầu; Sơn Tinh là thần núi nào.

# 48 Chương 4 Thủy Tinh: Sông Ngầm

| Hạng mục | Thiết kế |
| --- | --- |
| Bố cục | 5 phòng dọc sông, đi ngang tầng sâu: P1 Bến Ngầm → P2 Hồ Cửa Van → P3 Đền Ngập → P4 Thác Ngược → P5 Vực Thuồng Luồng (hồ tròn khoảng 18 m) |
| Cơ chế mới | **Ngọc Nước.** Nhận ở P1. Tương tác tại đền thờ trong phòng để đổi mực nước giữa 3 mức Thấp, Giữa, Cao. Nước thấp lộ đường dưới đáy; nước cao làm bè và đá nổi thành bậc đi. Không có bơi: rơi vào nước sâu thì về điểm đứng an toàn gần nhất. Ấn Núi của chương 3 vẫn dùng được và nâng khối đất lên trên mặt nước |
| Câu đố chính (P3) | Đền có hai gian: rút nước để vào gian dưới lấy chìa, dâng nước để lên gian trên đặt chìa |
| Quái thường | **Hắc Ảnh Nước**: trồi lên khi nước dâng; **Hắc Ảnh Vỏ Đá**: phải chiếu đèn làm vỡ giáp rồi mới gây sát thương |
| Công cụ P4 | **Ngọc Triều**: trong đấu trường, đổi mực nước ngay cả khi đang giao chiến |
| Trùm | **Thuồng Luồng**. Ẩn trong nước, trồi lên ở 3 điểm cố định, quất đuôi theo vệt sóng báo trước 1 giây. Rút nước thì thân lộ ra để đánh; nước cao thì nó lặn và hồi máu chậm. Máu 240 |
| Môi trường | Sông ngầm, thạch nhũ phát sáng tím và đỏ, rêu phát quang, thuyền độc mộc cũ trang trí, đền ngập nửa nước |
| Cổ vật | 3 mảnh **Vành Sóng** của Trống Đồng |
| Cốt truyện | Thủy Tinh bị Hắc Sương giam dưới đáy vực, Thuồng Luồng là kẻ canh ngục. Mối thù núi và nước hóa ra đã bị lợi dụng; Thủy Tinh và Sơn Tinh hòa giải |

## Tri thức chương 4 (trung bình đến khó)

| Phòng | Bia | Nhãn |
| --- | --- | --- |
| P1 | Sơn Đoòng có sông ngầm chảy qua lòng hang (tên sông cần kiểm chứng) | Tự nhiên |
| P2 | Thạch nhũ và măng đá hình thành từ nước nhỏ giọt mang chất đá vôi, lắng đọng qua thời gian rất dài | Tự nhiên |
| P3 | Truyền thuyết Sơn Tinh – Thủy Tinh giải thích hiện tượng lũ lụt hằng năm | Truyền thuyết |
| P4 | Cư dân Văn Lang sống chủ yếu bằng nghề trồng lúa nước ven các dòng sông lớn | Lịch sử |
| P5 | Truyền thuyết thể hiện khát vọng chế ngự thiên tai của người Việt cổ; hoa văn thuyền trên trống đồng cho thấy vai trò của sông nước | Truyền thuyết, Lịch sử |

Câu hỏi mẫu: truyền thuyết giải thích hiện tượng gì; nghề chính của cư dân Văn Lang; thạch nhũ hình thành do đâu.

# 49 Chương 5 Trục Linh Mạch: màn kết

Chương 5 dài hơn (**20–25 phút**), khó hơn và dùng nhiều cơ chế nhất. Không thêm cơ chế di chuyển mới; độ khó đến từ ghép các năng lực đã có, quái hỗn hợp, trùm nhiều pha và đoạn chạy giới hạn thời gian. Quái vẫn báo đòn rõ, không tăng độ khó chỉ bằng tăng máu.

| Phòng | Nội dung | Cơ chế dùng |
| --- | --- | --- |
| P1 Cửa Linh Mạch | Bóng tối gần như hoàn toàn, An chỉ dựa vào Lồng Đèn. Hành lang có quái hỗn hợp của chương 1–4 | Đèn, lộn né, khóa mục tiêu |
| P2 Cầu Tứ Linh | Một câu đố liên hoàn: dâng nước, nâng khối đất trên mặt nước, cột gió lên cao, chiếu đèn mở phong ấn | Cột Gió, Ấn Núi, Ngọc Nước, đèn |
| P3 Chiến Lũy | Hai đợt quái liên tiếp, đợt cuối có Hộ vệ Đá tái xuất làm trùm phụ | Toàn bộ chiến đấu |
| P4 Đàn Đồng Tế | Đặt 4 cổ vật; **Đại Vấn Đáp** 5 câu; gõ Trống Đồng 4 vành theo thứ tự được gợi ý rải rác qua các chương. Nơi đặt **cơ chế ẩn** | Tri thức, trống |
| P5 Lõi Hắc Sương | **Trùm cuối Hắc Sương Vương**, 4 pha. Thắng mỗi pha thì một Linh Thần hỗ trợ bằng hiệu ứng môi trường: Kim Quy dựng khiên ánh sáng, Chim Lạc thổi cột gió, Sơn Tinh nâng núi chắn, Thủy Tinh rút nước để lộ lõi. Có mốc lưu ở đầu mỗi pha | Mọi năng lực |
| P6 Lối Lên Mặt Đất | Hang sụp, chạy thoát trong 90 giây, dùng lộn, cột gió và khối đất | Di chuyển |

Mốc lưu chương 5: đầu chương, trước P4, trước P5 và đầu mỗi pha trùm. Chỉ có một lửa trại ở P1.

## Trùm cuối Hắc Sương Vương

| Pha | Hành vi | Cách thắng |
| --- | --- | --- |
| 1 Ánh sáng | Tạo màn sương che, phóng cầu sương | Chiếu đèn xóa màn sương rồi đánh |
| 2 Gió | Bay lên cao, nện xuống theo vòng cảnh báo | Lên cột gió để đánh khi nó ở trên cao |
| 3 Núi | Quét sóng sương sát mặt đất khắp đấu trường | Nâng khối đất để đứng trên và chặn sóng |
| 4 Nước | Lặn vào hồ trung tâm, tạo vòng xoáy kéo An vào giữa | Rút nước để lộ lõi, đánh lõi |

Máu 120 mỗi pha. Hình dạng là người khổng lồ bằng sương, dùng lại rig humanoid của Hắc Ảnh phóng to cùng particle sương để có animation thật mà không cần model mới.

## Cơ chế ẩn và hai cái kết

**Kết 1 "Ánh Sáng Trở Lại":** hoàn thành chương 5. Hắc Sương bị đẩy lùi và phong ấn, linh mạch hồi phục, An thoát lên mặt đất. Lời dẫn cuối cảnh báo Hắc Sương còn ngủ yên nơi ký ức bị lãng quên.

**Cơ chế ẩn "Vành Ký Ức":**

1. Ở P4 có một **Bia Câm** không có dấu hiệu nào, nằm khuất sau Đàn Đồng Tế. Chữ trên bia chỉ hiện khi An đứng đúng điểm giao nhau của 4 luồng sáng Linh Thần và giữ Focus đèn vào bia trong 3 giây.
2. Manh mối dẫn tới bia mở dần trong Nhật ký theo Điểm Tri Thức. Mỗi 5 điểm mở một dòng gợi ý, ví dụ "nơi bốn ánh sáng gặp nhau", "điều không được khắc trên đàn". Người chơi không đủ điểm vẫn có thể tự tìm ra nhưng khó hơn.
3. Bia Câm đưa ra **Câu hỏi Cội Nguồn**: sắp xếp đúng thứ tự Văn Lang → Âu Lạc và gắn đúng truyền thuyết với Linh Thần. Trả lời sai được làm lại.
4. Thành công thì nhận **Vành Ký Ức**, vành thứ năm của Trống Đồng; cờ `hidden_memory_ring` được lưu ngay.

**Kết 2 "Ký Ức Không Tắt"** (cần Vành Ký Ức trước khi thắng trùm cuối): Trống Đồng được phục dựng đủ 5 vành. Hắc Sương, hiện thân của sự lãng quên, tan hoàn toàn chứ không chỉ bị phong ấn. Bốn Linh Thần hiện ra tiễn An. An lên tới mặt đất lúc bình minh ở Sơn Đoòng. Phần kết điểm lại từng chương bằng ảnh tĩnh, cuốn nhật ký của An trở thành bộ sưu tập tri thức, và màn danh đề hiện toàn bộ thẻ kiến thức đã thu thập.

Cả hai kết dùng ảnh tĩnh, camera di chuyển ngắn, ánh sáng, âm thanh và lời dẫn theo phần 2, không cần cutscene animation.

## Tri thức chương 5 (khó, tổng hợp)

| Phòng | Bia | Nhãn |
| --- | --- | --- |
| P1 | Từ nước Văn Lang của các vua Hùng đến nước Âu Lạc của An Dương Vương | Lịch sử |
| P2 | Trống đồng được tìm thấy ở nhiều nơi tại Đông Nam Á, cho thấy giao lưu văn hóa | Lịch sử |
| P3 | Phong Nha – Kẻ Bàng được UNESCO công nhận là Di sản thiên nhiên thế giới (năm cần kiểm chứng) | Tự nhiên |
| P4 | Giỗ Tổ Hùng Vương ngày mùng 10 tháng 3 âm lịch | Lịch sử |
| P5 | Tín ngưỡng thờ cúng Hùng Vương ở Phú Thọ được UNESCO ghi danh là di sản văn hóa phi vật thể đại diện của nhân loại (năm cần kiểm chứng) | Lịch sử |

# 50 Dữ liệu, scene và lưu tải mở rộng

## Scene

| Scene | Nội dung |
| --- | --- |
| 00_Boot | Tạo dịch vụ dùng chung một lần |
| 01_MainMenu | Chơi mới, Tiếp tục, Cài đặt, Thoát |
| 02_HangLinhMach | Chương 1 Kim Quy |
| 03_GiengTroiChimLac | Chương 2 |
| 04_ThanhDaSonTinh | Chương 3 |
| 05_SongNgamThuyTinh | Chương 4 |
| 06_TrucLinhMach | Chương 5 và hai cái kết |

Các scene 90–93 vẫn là scene thử, không vào build.

## Lưu tải

Tăng schemaVersion và chuyển đổi bản lưu cũ. Thêm các trường:

| Trường | Ý nghĩa |
| --- | --- |
| chapterId, sceneId, checkpointId | Chương và mốc đang kích hoạt |
| unlockedAbilities | movement_kit, ability_wind, ability_earth, ability_water |
| fragments | ID mảnh cổ vật của mọi chương |
| bossTools | Công cụ trận trùm đã nhận |
| earthTileLevels, waterLevels | Mức của từng ô đất và từng phòng nước đã giải xong |
| readLoreIds, answeredQuizIds, firstTryCorrectQuizIds | Trạng thái tri thức; Điểm Tri Thức suy ra từ tập cuối |
| defeatedUniqueEnemies | Thêm ID trùm từng chương |
| hiddenMemoryRing | Đã có Vành Ký Ức |
| endingsSeen | Kết đã xem |

Chuyển chương lưu một snapshot ở mốc đầu chương mới. Continue mở đúng chương và mốc. Năng lực và tri thức không mất khi chết vì đã nằm trong snapshot ở mốc.

# 51 Quy trình sản xuất và duyệt asset

## Bốn giai đoạn cho mỗi chương

| Giai đoạn | Việc | Điều kiện chuyển |
| --- | --- | --- |
| A Blockout | Dựng phòng bằng ProBuilder hoặc khối cơ bản, bake NavMesh, code cơ chế, câu đố, AI, trùm, Bệ Vấn Đáp, nhiệm vụ, lưu tải | Chơi được từ đầu đến cuối chương bằng khối |
| B Chơi thử và cân bằng | Chỉnh thời gian lộn, tầm đánh, nhịp đòn trùm, độ khó câu đố và câu hỏi; chạy lại các ca lưu tải | Không còn lỗi chặn tiến trình |
| C Duyệt concept và asset | Trình bảng concept theo tông màu và danh sách asset đề xuất cho người dùng duyệt | Người dùng duyệt |
| D Làm đồ họa | Thay khối bằng model đã duyệt, bake ánh sáng, đặt VFX, âm thanh qua AudioMixer | Không thay đổi collider, ID và tuyến chơi đã kiểm thử |

Giai đoạn A và B của mọi chương làm xong trước giai đoạn C của toàn game.

## Bảng concept trình duyệt

| Tầng | Chương | Tông màu và chất liệu |
| --- | --- | --- |
| Tầng trên | 1 (Trại, Vườn Rêu), 2 | Xanh lá, vàng nắng, sương mỏng, đá xám, rêu |
| Tầng giữa | 1 (Điện Trống, Đàn Kim Quy), 3 | Đá nâu, đồng xỉn, hoa văn Đông Sơn, ánh lửa cam |
| Tầng sâu | 4, 5 | Thạch nhũ tím và đỏ, nước tối, rêu phát quang, sương đen của Hắc Sương |

Mỗi bảng concept gồm ảnh chụp blockout, ảnh tham chiếu màu, mô tả vật liệu và danh sách asset của chương. Không import trước khi người dùng duyệt.

## Quy tắc tìm asset

1. Ưu tiên asset miễn phí có giấy phép rõ: danh mục A01–A40, sau đó Quaternius, Kenney, Poly Haven, ambientCG, OpenGameArt, Sketchfab (lọc Downloadable và giấy phép cho phép), Unity Asset Store bản miễn phí, Mixamo cho animation humanoid.
2. Với mỗi asset ghi tên, tác giả, URL, giấy phép, phần dùng, phần đã chỉnh vào Asset_Sources.
3. Chỉ đề xuất tự làm khi không tìm được asset miễn phí phù hợp; ghi rõ lý do và cách làm (Blender, ProBuilder, Shader Graph, particle).

## Ứng viên asset cho trùm có animation

Trùm phải là sinh vật có animation thật. Danh sách dưới là hướng tìm, chưa xác nhận tồn tại hoặc đủ clip; giai đoạn C phải kiểm tra từng ứng viên.

| Trùm | Ứng viên miễn phí cần kiểm tra | Phương án tự làm nếu không có |
| --- | --- | --- |
| Hộ vệ Đá (chương 1, chương 5) | A07 Giant Monster Model Golem (Kevin Iglesias) | Retarget clip humanoid từ Mixamo cho rig golem |
| Chim Lạc nhiễm sương | Chim có rig và clip bay, đậu, lao trên Quaternius hoặc Sketchfab | Model chim đơn giản trong Blender, rig ít xương, 3 clip đậu, vỗ cánh, lao |
| Rết Đá | Rết hoặc côn trùng nhiều đốt có animation trên Sketchfab | Chuỗi đốt đá nối nhau chuyển động bằng code |
| Thuồng Luồng | Rắn hoặc rồng nước có animation trong A08 Ultimate Monsters, Quaternius hoặc Sketchfab | Dùng chung code chuỗi đốt của Rết Đá |
| Hắc Sương Vương | Rig humanoid của Hắc Ảnh (A08) phóng to, clip Mixamo | Particle sương bao quanh mô hình humanoid cơ bản |

# 52 Lịch sản xuất 7 ngày

Lịch giả định người dùng làm cùng trợ lý AI, mỗi ngày kết thúc bằng một bản chơi được trong Editor. Giai đoạn A–B của toàn game xong trước ngày 6.

| Ngày | Việc | Kết quả cuối ngày |
| --- | --- | --- |
| 1 | C1-F01 đến F08; PlayerMotor, InputReader; Boot, Menu, Pause, Settings; hồi sinh, vùng chết, đủ 4 mốc chương 1 | Chơi chương 1 hiện có từ Menu, chết và hồi sinh đúng |
| 2 | Đánh dao, thể lực, Hắc Ảnh và NavMesh, đèn làm yếu; Đàn Kim Quy, Hộ vệ Đá, phục dựng, kết chương; HUD thanh; túi đồ và chế thuốc | Chương 1 hoàn chỉnh bằng khối |
| 3 | Bộ animation di chuyển, Quà Kim Quy, khóa mục tiêu; hệ Tri thức (bia, Bệ Vấn Đáp, Linh Thần Vấn Đáp, Nhật ký); khuôn phòng dùng chung; chương 2 | Chương 1 chuyển sang chương 2, chương 2 chơi hết bằng khối |
| 4 | Chương 3 và chương 4 (Ấn Núi, Ngọc Nước, quái biến thể, Rết Đá, Thuồng Luồng) | Chơi chương 2–4 bằng khối |
| 5 | Chương 5: 6 phòng, Hắc Sương Vương 4 pha, cơ chế ẩn, hai cái kết; schema lưu đầy đủ | Chơi từ Menu đến cả hai kết bằng khối |
| 6 | Chơi thử toàn game, cân bằng, sửa lỗi, chạy các ca kiểm thử; soạn và kiểm chứng nội dung tri thức; trình concept và danh sách asset cho người dùng duyệt | Bản blockout ổn định; concept đã gửi duyệt |
| 7 | Làm đồ họa theo asset đã duyệt, âm thanh cơ bản; build Windows; chạy thử bản build từ đầu đến hai kết | Build Windows chơi được, có bằng chứng kiểm thử |

## Mức tối thiểu không được cắt

Chơi được từ Menu đến cả hai kết; lưu, tải, hồi sinh đúng; mỗi chương có cơ chế mới, câu hỏi kiến thức, quái thường và trùm; Quà Kim Quy; nội dung tri thức đã kiểm chứng.

## Thứ tự cắt khi trễ

1. Giảm phạm vi đồ họa: giữ blockout với vật liệu màu theo concept, chỉ thay model nhân vật, quái và trùm.
2. Túi 12 ô thu về hiển thị số thuốc trên HUD; giữ chế thuốc tại lửa trại.
3. Bỏ "lộn chui khe thấp" và các khe thiết kế cho nó.
4. Bỏ "lộn + chém".
5. Giảm quái thường mỗi phòng giao chiến xuống một đợt.

Mỗi thứ bị cắt phải được gỡ hẳn khỏi game và tài liệu, không để dở dang.

# 53 Kiểm thử bổ sung

| Mã | Thao tác | Kết quả phải đạt |
| --- | --- | --- |
| MV01 | Bấm Ctrl trong chương 1 | Không lộn |
| MV02 | Hoàn thành chương 1, tiếp tục chương 2, thoát và Continue | Lộn và các thao tác nâng cao dùng được, còn sau khi tải |
| MV03 | Lộn 4 hướng có và không khóa mục tiêu, sát tường, ở FPS thấp | Đúng hướng, không xuyên tường, bất tử đúng khoảng |
| LK01 | Khóa mục tiêu rồi để mục tiêu chết, đi xa, bị che | Tự bỏ khóa, camera không giật |
| QZ01 | Trả lời sai rồi đúng tại Bệ Vấn Đáp | Có giải thích, nhận đủ đồ, không có điểm lần đầu |
| QZ02 | Trả lời đúng ngay, lưu, chết, tải | Điểm và đồ còn đúng, bệ không trao lại |
| QZ03 | Mở bảng câu hỏi khi đang giữ phím di chuyển hoặc đánh | Gameplay dừng, đóng bảng không còn input treo |
| AB01 | Dùng năng lực chương, lưu giữa câu đố, tải | Ô đất, mực nước, cột gió về đúng trạng thái đã lưu |
| BS02 | Chết ở mỗi trùm rồi thắng | Đấu trường, công cụ, mốc lưu đúng; trùm không sống lại sau Continue |
| CH01 | Chuyển chương và Continue ở mọi chương | Mở đúng scene và mốc, năng lực giữ nguyên |
| EN01 | Hoàn thành chương 5 không có Vành Ký Ức | Kết 1 |
| EN02 | Lấy Vành Ký Ức, chết trước trùm, tải, thắng | Vẫn ra Kết 2 |
| KN01 | Đối chiếu từng bia và câu hỏi với nguồn | Mỗi mục có nguồn ghi lại, không còn mục "cần kiểm chứng" trong build |

# 54 Giả định và điểm cần xác nhận

1. **Bệ Vấn Đáp theo phòng:** người dùng chốt "P1–P3 lấy mảnh để dựng cốt truyện, P4 lấy công cụ cho trận trùm". Kế hoạch hiểu đây là các phòng trong mỗi chương. Nếu ý người dùng là theo chương (chương 1–3 lấy mảnh, chương 4 lấy công cụ cho chương 5) thì sửa phần 42.
2. **Chương 1 không có lộn né** theo quyết định Kim Quy trao toàn bộ bộ di chuyển sau chương 1. Chiến đấu chương 1 cân bằng lại theo phần 43.
3. **Lịch 7 ngày rất sát** cho 5 chương. Nếu đến cuối ngày 3 chương 2 chưa chơi được hết thì dừng lại hỏi người dùng để giảm phạm vi trước khi làm tiếp.
4. Số máu trùm, thời lượng và kích thước phòng là điểm bắt đầu để cân bằng, chưa phải số liệu đo.
5. Nội dung tri thức trong phần 45–49 là bản đề xuất, phải kiểm chứng nguồn trước khi đưa vào build.

Sơn Đoòng Cõi Mộng Kỳ Thần  |  2

# 55 Đặc tả triển khai từng chương

Đặc tả cho AI Agent nằm trong thư mục `spec/chapters/`. Mỗi file chương có đúng 2 phần: Tóm tắt tổng quan, và Đặc tả kỹ thuật gồm 5 hạng mục (greybox, cơ chế mới, AI và đấu trường, lưu tải, kiểm thử).

| File | Nội dung |
| --- | --- |
| `00_Nen_tang_dung_chung.md` | Quy ước, layer (thêm Hazard index 12), API dùng chung phải có sau chương 1, thành phần tri thức, địch, trùm, lưu tải v4, thứ tự làm một chương |
| `C2_Chim_Lac.md` | Cột Gió, câu đố chuông gió, Hắc Ảnh Ném, trùm Chim Lạc |
| `C3_Son_Tinh.md` | Ấn Núi (mức 0 / 1 / 2 m), câu đố dáng núi, Hộ Vệ Đá Nhỏ, trùm Rết Đá |
| `C4_Thuy_Tinh.md` | Ngọc Nước, cửa van, đền ngập, Hắc Ảnh Nước, Hắc Ảnh Vỏ Đá, trùm Thuồng Luồng |
| `C5_Truc_Linh_Mach.md` | Cầu Tứ Linh, Chiến Lũy, Đàn Đồng Tế, cơ chế ẩn Vành Ký Ức, Hắc Sương Vương 4 pha, thoát hang, hai cái kết |

Điều chỉnh phát sinh khi viết đặc tả, ưu tiên hơn phần 46–49:
1. Mức khối đất chương 3 là 0 / 1 / 2 m thay vì 0 / 1,5 / 3 m, vì An chỉ nhảy cao 1,2 m.
2. Hướng Focus của Lồng Đèn lấy theo hướng camera (có góc ngẩng) để chiếu được mục tiêu trên cao.
3. Câu đố chuỗi (chuông gió, trống bốn vành) dùng `SequencePuzzleController` chung; `DrumPuzzleController` chương 1 giữ nguyên.
4. Đền thờ trong đấu trường chương 4 chỉ rút nước, tự dâng lại sau 6 giây.
5. Chương 5 thêm mốc `c5_cp_bridge` trước Chiến Lũy và hai bệ vấn đáp trao thuốc ở P1, P3.
6. Thẻ thức tỉnh Kim Quy (chương 1) có câu gợi ý thứ tự trống bốn vành; ID phục dựng chương 1 trong bản lưu v4 là `c1_relic_restored`.
