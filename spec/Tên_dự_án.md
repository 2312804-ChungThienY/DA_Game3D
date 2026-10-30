**Tên dự án (Dự kiến):** *Sơn Đoòng: Cõi Mộng Kỳ Thần* (*Echoes of the Ancient Realm*)

**Thể loại:** 3D Action-Adventure, Exploration & Puzzle (Góc nhìn thứ ba)

**Công cụ phát triển:** Unity Engine (Universal Render Pipeline - URP)

**Phong cách đồ họa:** Stylized Semi-Realistic (Bán thực tế kết hợp màu sắc huyền ảo cổ tích Việt Nam)

**1. Bối Cảnh & Cốt Truyện**

- **Bối cảnh:** Toàn bộ game diễn ra sâu trong một vùng không gian rạn nứt thời gian bên dưới quần thể hang động Sơn Đoòng – Phong Nha. Nơi đây lưu giữ một vương quốc thần thoại bị lãng quên từ thời văn hóa Đông Sơn và Hồng Bàng.

- **Cốt truyện:** Người chơi vào vai **An**, một nhà khảo cổ học trẻ vô tình rơi xuống giếng trời cấm của hang động. An phát hiện ra trục linh mạch của đất nước đang bị thoái hóa bởi "Hắc Sương" (thế lực tà khí). Để thoát ra và bảo vệ thế giới trên mặt đất, An phải thức tỉnh 4 Linh Vật (Thần Kim Quy, Chim Lạc, Sơn Tinh, Thủy Tinh) thông bằng cách phục dựng các cổ vật Trống Đồng bị phân tán.

**2. Cơ Chế Gameplay Chính (Core Gameplay Loop)**

[Khám Phá & Di Chuyển] ➔ [Giải Đố Cổ Vật] ➔ [Thu Thập Linh Thạch] ➔ [Chiến Đấu/Thức Tỉnh Thần Thú]

**A. Khám Phá & Di Chuyển (Traversal & Exploration)**

- **Cơ chế Đu dây & Leo núi (Grappling & Climbing):** Sử dụng móc đu bằng dây mây phép thuật để di chuyển qua các măng đá, thạch nhũ và bờ vực thẳm.

- **Lồng Đèn Sen (Lotus Lantern):** Vật dụng cốt lõi của An. Lồng đèn tỏa ra ánh sáng xua tan Hắc Sương, kích hoạt các mầm cây cổ thụ phát sáng tạo đường đi, và rọi tỏ các văn tự ẩn trên bức tường đá.

- **Chèo thuyền thúng / Thuyền độc mộc:** Di chuyển qua các dòng sông ngầm chảy xiết trong hang động.

**B. Giải Đố Môi Trường (Environmental Puzzles)**

- **Câu đố Trống Đồng (Sonic Puzzles):** Người chơi tương tác với các mặt Trống Đồng Đông Sơn bằng cách gõ đúng nhịp điệu (ngũ âm truyền thống). Sóng âm tạo ra phản ứng vật lý biến đổi môi trường (nâng cột đá, mở dòng chảy nước).

- **Định vị Ánh sáng & Thuỷ tinh:** Điều hướng ánh sáng mặt trời chiếu qua các giếng trời (Doline) vào các viên ngọc thạch để kích hoạt cửa đá phong ấn.

**C. Chiến Đấu & Nhập Vai (Combat & RPG Light)**

- **Vũ khí:** Dao găm đồng Đông Sơn và Nỏ thần (bắn ra các mũi tên năng lượng linh khí).

- **Kẻ thù:** Quái vật tạo từ Hắc Sương, Rết khổng lồ hang động, và các Hộ vệ Đá bị thao túng.

- **Chiến thuật:** Tập trung vào né tránh (Dodge-roll), phản công (Parry) và chiếu sáng làm suy yếu kẻ thù trước khi đả kích.

- **Chế tạo (Crafting):** Thu thập thảo dược hang động (lá cẩm, linh chi ngầm, trầm hương) tại các điểm đốt lửa trại để pha chế thuốc hồi máu và tăng thể lực.

**3. Kiến Trúc Kỹ Thuật Trên Unity (Unity Implementation Plan)**

| **Hệ thống** | **Package / Tool gợi ý** | **Chi tiết triển khai** |
| --- | --- | --- |
| **Graphics & Lighting** | URP, Volumetric Fog & Mist, Shader Graph | Dùng Volumetric Fog tạo độ sâu cho không gian hang động; Shader Graph làm hiệu ứng dòng nước ngầm và hoa văn trống đồng phát sáng. |
| **Character Controller** | Kinematic Character Controller / Character Controller | Kết hợp Cinemachine cho góc nhìn thứ 3 linh hoạt (Khám phá: Camera rộng; Giải đố: Camera cố định). |
| **Animation** | Animator, Animation Rigging | Rigging cho nhân vật leo trèo, tương tác tay với bề mặt đá và cầm lồng đèn. |
| **Audio** | FMOD / Unity Audio | Phối hợp tiếng nước nhỏ giọt, gió lùa hang động với nhạc cụ dân tộc (Đàn bầu, Sáo trúc, Trống cồng chiêng) biến đổi theo ngữ cảnh. |
| **Save & World State** | Odin Serializer / JSON Save System | Lưu vị trí lửa trại, trạng thái các câu đố đã giải và cổ vật thu thập. |

**4. Định Hướng Nghệ Thuật (Art & Audio Direction)**

- **Thiết kế môi trường:**

- **Tầng trên (Giếng trời):** Thảm thực vật nguyên sinh xanh mút, ánh nắng chiếu xuyên qua sương mờ.

- **Tầng trung (Cõi Linh Văn):** Kiến trúc đền thờ đá cổ đại chạm khắc hoa văn Chim Lạc, nhà sàn trên đá.

- **Tầng sâu (Mạch Hắc Sương):** Tinh thể thạch nhũ phát sáng tím/đỏ, các vực sâu thăm thẳm.

- **Âm nhạc:** Nhạc nền ambient phối khí giữa tiếng nhạc cụ dân tộc Việt Nam (đàn tranh, sáo meo, trống cồng) và synth hiện đại tạo cảm giác vừa huyền bí vừa kỳ vĩ.