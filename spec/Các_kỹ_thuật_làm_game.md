**1. Điều khiển Nhân vật & Camera (Player & Camera Mechanics)**

- **First-Person Controller (FPS)**: Xử lý di chuyển, nhảy, chạy nhanh (sprint) và cúi người (crouch). 

- **Mouse Look & Camera Smooth**: Xoay góc nhìn camera theo chuột.

- **Dual Camera Setup (Sửa lỗi Clipping)**: Sử dụng một camera phụ (*Equippable Only Camera*) đè lên camera chính để xử lý hiện tượng mô hình vũ khí/công cụ bị đâm xuyên qua tường hoặc địa hình.

- **Mount / Horse Riding System**: Lập trình hệ thống cưỡi ngựa và điều khiển phương tiện di chuyển trong thế giới mở. 

**2. Tương tác & Thu thập Tài nguyên (Interaction & Resource Gathering)**

- **Raycasting & Trigger Colliders**: Phát hiện vật thể tương tác (nhặt đồ, mở hòm) và hiển thị giao diện UI nhắc nhở khi người chơi tiến lại gần.

- **Resource Chopping & Mining**: Kỹ thuật tác động lực và trừ máu của tài nguyên thế giới (chặt cây rơi ra gỗ, đập đá thu khoáng sản).

- **Item Pickup & Drop**: Nhặt vật thể dưới đất đưa vào túi đồ và ném vật thể từ túi đồ ra ngoài không gian 3D.

**3. Giao diện Hành trang & Chế tạo (Inventory & Crafting Systems)**

- **UI Drag & Drop System**: Xử lý di chuyển, sắp xếp và trao đổi vị trí vật phẩm giữa các ô trong hành trang bằng thao tác kéo thả.

- **Quick Slots & Equipping System**: Gán phím tắt nhanh, khởi tạo (Instantiate) mô hình vũ khí/công cụ tương ứng vào tay nhân vật và chạy Animation cầm nắm. 

- **Crafting System**: Xây dựng công thức chế tạo (Recipes), kiểm tra nguyên liệu đầu vào và tự động trừ vật phẩm trong Inventory khi chế tạo.

- **Item Durability**: Quản lý độ bền của công cụ/vũ khí, giảm chỉ số khi sử dụng và phá hủy trang bị khi hết độ bền.

**4. Hệ thống Xây dựng, Trồng trọt & Nấu ăn (Building, Farming & Cooking)**

- **Modular Building System**: Xây dựng công trình theo dạng mảnh (nền, tường, mái) hỗ trợ bắt điểm (Snap alignment).

- **Plant & Harvest System**: Hệ thống gieo hạt, đếm thời gian cây lớn theo các giai đoạn và thu hoạch nông sản.

- **Campfire & Cooking System**: Tương tác với lửa trại, đun nấu nguyên liệu thô thành thức ăn tăng chỉ số sinh tồn.

**5. Hệ thống AI & Chiến đấu (AI, Enemy & Combat Systems)**

- **Finite State Machine (FSM)**: Lập trình trí tuệ nhân tạo cho quái vật và động vật hoang dã với các trạng thái: Thong dong (Idle), Tuần tra (Patrol), Đuổi theo (Chase), Tấn công (Attack) và Bỏ chạy (Flee).

- **NavMesh Navigation**: Tìm đường và di chuyển thông minh cho AI trên địa hình phức tạp.

- **Creature Spawner**: Hệ thống tự động sinh (respawn) quái vật/động vật tại vị trí cố định theo thời gian. 

- **Melee Combat**: Tấn công cận chiến bằng rìu/vũ khí, xử lý Va chạm (Hitbox) và gắn sự kiện vào Animation (*Animation Events*).

- **Ranged Combat (Bow & Arrow)**: Bắn cung với cơ chế tính đạn đạo vật lý (Projectile Physics). 

- **Fishing System**: Cơ chế câu cá gồm cần câu, hoạt ảnh giật câu và minigame bắt cá.

**6. Chỉ số Sinh tồn & Môi trường (Survival Vitals & Environment)**

- **Survival Vitals**: Quản lý thanh chỉ số Máu (Health), Thể lực (Stamina), Đội đói (Hunger) và Độ khát (Thirst).

- **Temperature System**: Hệ thống nhiệt độ môi trường (Nóng/Lạnh) ảnh hưởng trực tiếp đến trạng thái cơ thể nhân vật. 

- **Day/Night & Weather**: Chu kỳ ngày đêm và biến đổi thời tiết.

- **Terrain Building**: Tạo địa hình núi, sông, cây cối bằng Unity Terrain Tools.

**7. Nhiệm vụ & Kỹ năng (Quest & Skill Systems)**

- **Quest System**: Quản lý danh sách nhiệm vụ (Đang làm/Hoàn thành), bảng hiển thị tiến độ (*Quest Tracker Overlay*). 

- **Skill & XP Progression**: Tích lũy điểm kinh nghiệm (XP), thăng cấp và mở khóa các kỹ năng/Perk hỗ trợ sinh tồn. 

**8. Lưu trữ Dữ liệu & Mã hóa (Save & Load Systems) **

- **PlayerPrefs**: Lưu trữ các cài đặt cơ bản (âm thanh, đồ họa).

- **JSON & Binary Serialization**: Lưu/Tải toàn bộ trạng thái game phức tạp (vị trí người chơi, đồ trong Inventory, vị trí công trình đã xây, thời gian).

- **Encryption / Decryption**: Mã hóa file JSON để ngăn chặn người chơi can thiệp hoặc sửa dữ liệu save game. 

**9. Kiến trúc Code & Tối ưu hóa (Software Architecture)**

- **Unity Events & Action Delegates**: Giảm sự phụ thuộc giữa các class (Decoupling), viết code sạch và dễ mở rộng. 

- **Resources & Asset Management**: Quản lý và tải động các tài nguyên Prefab từ thư mục Resources.

- **Audio Manager**: Quản lý tập trung các hiệu ứng âm thanh (SFX) khi chặt cây, nhặt đồ, mở menu và nhạc nền (BGM).