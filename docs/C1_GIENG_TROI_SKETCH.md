# Phác thảo Trại Giếng Trời — Chương 1

**Ngày 06/11/2026 · Bản phác thảo đã được duyệt và dựng bước đầu.** [Kết quả và ảnh sau dựng](C1_GIENG_TROI_WORKLOG.md). Đích: `Assets/_Project/Scenes/02_HangLinhMach.unity`, nhóm `World_Geometry/ENV_C1_ExpandedWorld/ENV_01_TraiGiengTroi`. [Sơ đồ mặt bằng](images/c1-gieng-troi-sketch-2026-11-06.svg) chỉ thể hiện quan hệ không gian, không phải tọa độ đặt asset hoặc hình render trong game. [Ảnh hiện trạng trước sửa từ góc chơi](images/c1-expanded-play-spawn-2026-11-05.png).

## Ý tưởng nhìn từ điểm xuất phát

1. Giữ vùng đứng ban đầu thoáng để An, camera và hướng ra cửa đều đọc rõ. Đường chính dẫn về phong ấn ở Z≈20; hai mép đường ít vật cản, thay độ lặp của các phiến lát bằng các đoạn đá lớn, đất và rêu có nhịp không đều.
2. Miệng giếng trời là điểm nhấn sáng ở phần giữa khoang. Mảng đá vôi xám ẩm và cây xanh tạo vòng bao quanh; tia sáng ấm rơi thành vệt lệch tâm, không che bệ nhận Lồng Đèn Sen, lửa lưu hoặc chữ hướng dẫn.
3. Cụm trại, lửa `cp_trai` và vật dụng ở cánh trái. Bệ nhận đèn và giấy hướng dẫn ở vùng người chơi có thể nhìn thấy từ đường chính. Không dựng lại bàn chế thuốc: quy định mới là chế Bình máu trong túi, E tại lửa chỉ lưu.
4. Phong ấn và vòm lối ra là điểm kết của khung nhìn. Giữ đường tiếp cận, collider chắn và góc chiếu đèn; dùng ánh sáng và biến đổi mặt đá để thu hút mắt thay cho thêm biển chỉ đường.

## Thạch nhũ

- **Trong trại:** hai cụm nhỏ trên trần kín ở hai vai vòm và mép vách phía xa. Chừa khoảng trời mở và trục nhìn từ spawn đến bệ đèn/phong ấn. Dáng thạch nhũ bất đối xứng, vài khối dài xen khối cụt, chân gắn tự nhiên vào đá vôi, rêu chỉ ở phần có ẩm/ánh sáng.
- **Đường nối Trại → Vườn Rêu:** một cụm rõ hơn tại đoạn trần kín sau phong ấn, tạo nhịp chuyển từ nắng sang hang tối. Không hạ khối xuống vùng camera đi qua hoặc đường chạy của An.
- **Các khu khác:** chỉ đề xuất vài điểm nhấn ở đoạn trần kín của Điện Trống và đường nối Điện Trống → Hành Lang Quang. Quyết định mật độ sau khi duyệt từng khu; không rải đều khắp hang.

Đây là **cách điệu lấy cảm hứng** từ hang đá vôi Sơn Đoòng, không phải mô phỏng đúng vị trí hay hình dạng địa chất. Ảnh [Oxalis về thạch nhũ và hố sụt](https://oxalisadventure.com/best-son-doong-cave-photos/) và [UNESCO về địa hình karst Phong Nha–Kẻ Bàng](https://whc.unesco.org/en/list/951/) là tư liệu tham chiếu. Thạch nhũ mọc từ trần; măng đá mọc từ nền, nên trong bản dựng sẽ phân biệt hai loại.

## Nguồn đồ họa ưu tiên

| Nhu cầu | Nguồn có trong dự án | Cách dùng dự kiến |
| --- | --- | --- |
| Vách, đá, thạch nhũ | A09 Mines and Cave Set và mesh `CaveVault_Skylight_0`/`Vault_Tooth_*` hiện có | Thử biến thể dưới visual root, giữ nguồn và collider gameplay. Điều khoản A09 theo gói Asset Store đã nhập cần giữ cùng bản phân phối. |
| Cây, bụi, đá stylized | A11 Ultimate Stylized Nature Pack, [nguồn Quaternius CC0](https://quaternius.com/packs/ultimatestylizednature.html) | Chọn ít loài, đổi tỷ lệ/cụm, giữ cây thấp không collider. |
| Lều, thùng, củi | A16 Survival Pack, [nguồn Quaternius CC0](https://quaternius.com/packs/survival.html) | Giữ đạo cụ trại ở mép trái, không thêm vật cản đường chính. |
| Chất liệu đá rêu | A14 Mossy Rock, [nguồn Poly Haven CC0](https://polyhaven.com/a/mossy_rock) | Bản sao 1K, material URP Lit; giới hạn rêu theo ánh sáng/độ ẩm. |

**Thiếu thực sự sau khi thử asset sẵn có:** nếu `Vault_Tooth_*`/đá A09 không cho ra hình thạch nhũ tự nhiên từ góc camera chơi, tạo một **mesh dẫn xuất riêng** với 2–3 dáng và LOD, dùng material đã có. Không cần nhập gói mới hoặc vẽ texture mới ở bước phác thảo.

## Đánh giá cơ chế 3D ở khu mở đầu

**Khung cơ chế phù hợp:** di chuyển góc nhìn thứ ba, nhận đèn bằng E, chiếu phong ấn khoảng 1 giây, lửa lưu và hai giấy hướng dẫn tạo một chuỗi học thao tác ngắn. `PlayerMotor` có chạy/nhảy, `InteractionScanner` dùng tia camera nhưng giới hạn khoảng cách từ An mặc định 3 m và kiểm vật cản; phong ấn có tiến độ/gián đoạn. Khu này không cần cơ chế mới.

**Đề xuất cần thử bằng người chơi sau khi dựng:**

- Khu 36×36 m có thể làm người mới đi lạc trước khi thấy đèn và phong ấn. Dùng bố cục và tương phản sáng để dẫn đường; nếu vẫn lạc mới cân nhắc gợi ý UI ngắn, không đổi HUD C1 bắt đầu ở Vườn Rêu.
- Tia tương tác trung tâm và cự ly 3 m có thể khó dùng nếu bệ/giấy bị che bởi cây hoặc vật trang trí. Kiểm bằng camera gameplay ở góc đứng thật, nhất là sau khi thêm thạch nhũ.
- Kiểm nhịp: từ spawn thấy bệ đèn, nhận đèn, thử bật/chiếu, xóa phong ấn và rời trại. Nếu khoảng chạy trống quá dài hoặc người chơi không hiểu đèn, điều chỉnh dấu hiệu thị giác/giấy hướng dẫn trước khi đổi luật chơi.
- Kiểm camera ở mép vách và dưới thạch nhũ, collider/NavMesh, lối rộng tối thiểu hiện có và 2 mức đồ họa. Chỉ kết luận hiệu năng sau khi đo Windows build trên tuyến di chuyển.

## Điểm cần duyệt trước khi dựng

Đề xuất: giữ phong cách **Mid Poly Stylized Việt Nam**, nắng ấm ở trại, đá xám lạnh và rêu vừa phải; thạch nhũ tập trung trên phần trần kín và đoạn nối đầu. Khi bạn duyệt, lượt thực hiện sẽ chỉ sửa môi trường của Trại Giếng Trời, chụp lại góc chơi, kiểm tương tác/collider/NavMesh/save/Console và báo phần chưa thử bằng tay.
