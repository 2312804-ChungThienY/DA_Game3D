---
name: vietnam-world-builder
description: Dùng khi được giao xây hoặc cải tạo một khu bản đồ, kiến trúc, cảnh quan hay tuyến di chuyển của thế giới Sơn Đoòng theo phong cách Stylized Việt Nam; không dùng cho một GameObject đơn lẻ, model Blender riêng lẻ hoặc debug thuần túy.
---

# Xây thế giới Stylized Việt Nam

## Quy trình

1. Xác định chương/khu và phạm vi người dùng giao. Đọc `docs/ART_BIBLE.md`, `docs/GAME_DESIGN.md`, phần chương qua `docs/SPEC_INDEX.md` và worklog môi trường tương ứng. Ghi riêng các yếu tố đã duyệt, tư liệu văn hóa/địa hình, chỗ hư cấu và câu hỏi chưa quyết định. Áp dụng Mid Poly Stylized Việt Nam, texture 1K mặc định/2K cho công trình chính, URP Lit và LOD cho vật thể lớn; không tự đặt ngân sách polygon hoặc khoảng chuyển LOD.
2. Qua Unity MCP, kiểm tra `mcpforunity://editor/state`, scene đang mở, dirty state, hierarchy và root gameplay. Với C1 mở rộng, kiểm tra `World_Geometry/ENV_C1_ExpandedWorld`, tuyến năm khu, checkpoint, ref/ID, collider/NavMesh và art cũ inactive trước khi bố trí mới. Không chạy công cụ dựng lại C1 chỉ để đọc cảnh.
3. Phác tuyến người chơi, mốc nhìn và nhịp sáng của khu; đặt visual dưới root phù hợp, giữ hành lang di chuyển, khe cửa/cầu, tín hiệu puzzle/prompt và camera thấy rõ. Đối chiếu các hình ảnh lấy cảm hứng Việt Nam với tư liệu nguồn; không gắn nhãn “khảo cổ chính xác” cho phần cách điệu.
4. Dựng từng cụm bằng quy trình của `unity-scene-builder`. Chỉ dùng `blender-asset-creator` khi thực sự được giao tạo model mới. Ưu tiên asset/prefab hợp lệ đã có; giữ source, license, `.meta`, GUID và các root/ref/ID runtime. Không đổi cơ chế, actor, chương hay tuyến nhiệm vụ chỉ vì thay cảnh.
5. Xác minh tại góc nhìn người chơi: lối đi liên tục, chân vật bám nền, không hở sáng, collider vừa đủ, vùng NavMesh/checkpoint liên quan, tương phản prompt/tia/đòn đánh dưới sương và ánh sáng. Dùng hierarchy/component/Console và ảnh Scene hoặc Game View nếu có; chạy Play Mode chỉ khi cần kiểm chứng tương tác. Nếu thay đổi đáng kể, đo hiệu năng đúng tuyến/máy đích trước khi kết luận chất lượng.

## Giới hạn và hoàn thành

- Không xóa hoặc ghi đè scene, prefab, asset, source nhập sẵn; không thay model/rig An, Unity/URP/package, save ID hay chương ngoài phạm vi đã giao. Scene dirty ngoài phạm vi phải được giữ nguyên. Đề xuất motif/màu chưa được duyệt phải mang nhãn đề xuất.
- Hoàn thành khi khu có tuyến và hình ảnh kiểm được, các mốc gameplay vẫn hoạt động ở mức đã thử, asset nguồn/credit rõ, scene đích được lưu mà scene khác không bị sửa. Báo minh bạch những kiểm tra chỉ mới ở Editor và các kiểm tra Play Mode/build/performance còn thiếu.
