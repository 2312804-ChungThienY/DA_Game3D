# Xử lý cảnh báo Unity — đối chiếu 04/11/2026

## Sửa lỗi ghi metadata ngày 05/11/2026

Ba file `DungeonModularPack/Textures/T_Step_N.tga.meta`, `T_Tile_N.tga.meta`, `Prefabs/Torch_B.prefab.meta` đã được Unity mở ReadWrite, ImportAsset và SaveAndReimport thành công, giữ GUID. Báo cáo `C1Completion_Metadata_2026-11-05.json` trong docs/reports; backup trong `SourceAssets/SceneBackups/2026-11-05_Chapter01Completion/Metadata_210110047`.

Lỗi trước có thật trong Editor.log, nhưng khi kiểm tra các file không readonly và ACL cho phép sửa. Không kết luận nguyên nhân cũ là readonly; lỗi khóa file/import tạm thời chưa được tái hiện. Không xóa meta/Library, không đổi ACL cả cây. Sau reimport phải xét lỗi mới, không dùng các dòng Console cũ để kết luận vẫn lỗi.

## Trạng thái đọc lại trên đĩa ngày 04/11

- Unity vẫn là 6000.4.8f1. Manifest/lock không còn com.unity.ai.assistant; com.unity.ai.inference vẫn có. Đây là trạng thái package hiện tại, không khẳng định Console đang trống mọi cảnh báo.
- Đã đối chiếu lại SHA-256 của **33/33 model** trong SourceAssets/ImportFixBackups/2026-10-30/repair_manifest.json: đều khớp after_sha256, không thiếu file. GUID .meta của **33/33 model** vẫn khớp bản backup. Đây là kiểm tra file, không chạy lại import/mesh validation trong Unity ở lượt tài liệu này.
- Source A09 ở Assets/A09 - Mines and Cave Set/MInesAndCaveSet; MinesSet.fbx.meta tại thư mục fbx hiện có isReadable:1 theo lần xử lý Read/Write sau đó. Không suy từ bit này rằng NavMesh runtime hoặc Windows build đã được kiểm tra lại hôm nay.
- AssetImportDiagnostics.cs và menu Tools → Son Doong → Validate Selected Imported Assets vẫn tồn tại, xem nguồn tại :59–120 và :123–137. Công cụ chỉ kiểm các file được yêu cầu; kết quả không đại diện toàn bộ dự án.
- Logs/AssetImportValidation.json bị ghi đè mỗi lần dùng công cụ; phase hiện tại có thể là lần import công cụ khu trại, không phải after-repair của 30/10. Bảng tổng hợp dưới đây là **bằng chứng lịch sử**, không giả định file Logs còn giữ report cũ hoặc đã tái chạy trong lượt đồng bộ.
- A09/A11 của khu mẫu mới dùng material riêng. Việc shader nguồn được hỗ trợ không bảo đảm hình ảnh trong camera đúng; xem [thử asset môi trường](ENVIRONMENT_ASSET_TRIALS.md) và [khu mẫu trại](CAMP_ENVIRONMENT.md) để biết các bước sửa hình đã làm sau lượt import.

Chưa force reimport, kiểm tra Console mới, Play Mode hoặc build trong lượt đồng bộ tài liệu. Không cần chạy lại công cụ sửa mesh để đọc hiện trạng; giữ backup source và GUID, chỉ thực hiện công việc sửa mới khi được giao.

## Hồ sơ công việc ngày 30/10 — giữ để truy nguyên

Nguồn công việc: danh sách Console người dùng gửi trong `Pasted text.txt`, bắt đầu bằng `Error reason is 'NoSubscription'`. Người dùng xác nhận **không dùng Unity AI Assistant và đồng ý gỡ gói**. Công việc này xử lý package và asset import; không bổ sung cơ chế gameplay.

## Kết quả kiểm tra import ngày 30/10

Đã chạy kiểm tra trực tiếp trong Editor dự án chính, Unity **6000.4.8f1**, sau khi sửa và force reimport:

| Phép kiểm tra | Kết quả |
| --- | --- |
| File được kiểm tra | 41: 33 model và 8 material ngoài model |
| Tài nguyên con được đọc | 467, trong đó 330 Mesh |
| Thông báo warning/error thu được trong lượt import kiểm tra | 0 |
| Lỗi kiểm tra | 0 |
| Mesh rỗng / tên Mesh trùng trong cùng model | 0 |
| Thay đổi bounds giữa các mesh có cùng tên trước/sau | 0, ngưỡng so sánh 0,0001 |
| GUID của 33 model | Giữ nguyên so với bản sao lưu `.meta` |

Báo cáo JSON chi tiết: `Logs/AssetImportValidation.json`, phase `after-repair`. Logs là dữ liệu cục bộ bị Git ignore; kết quả tổng hợp được giữ tại tài liệu này. Đây là kiểm tra import và cấu trúc tài nguyên, không phải Play Mode, kiểm tra chất lượng hình ảnh hoặc build gameplay.

## 1. Unity AI — NoSubscription

Stack trace đi qua ModelSelector của `com.unity.ai.assistant` khi gọi `generators.ai.unity.com`. Đây là yêu cầu dịch vụ AI cần quyền tài khoản, không phải lỗi controller hay gameplay.

- Đã gỡ dependency `com.unity.ai.assistant` phiên bản `2.21.0-pre.2` khỏi `Packages/manifest.json` theo lựa chọn của người dùng.
- Cập nhật `packages-lock.json` theo đồ thị dependency còn dùng. `com.unity.2d.sprite` chỉ được kéo bởi Assistant trong đồ thị này nên cũng được loại khỏi lock.
- Giữ `com.unity.ai.inference` và các package còn lại mà manifest vẫn khai báo. Không thay đổi subscription hay tài khoản Unity.
- Bản sao manifest/lock trước thao tác nằm trong thư mục backup bên dưới.

## 2. Material — No upgrader available

Các dòng này xuất hiện khi chạy **Convert All Built-In Materials to Current SRP**. Không thể kết luận một material hỏng chỉ vì bộ chuyển không có converter cho shader đó.

Đã đọc shader nguồn và kiểm tra bằng Unity:

- Năm material hang `A_MIneCarts_MAt`, `A_MinesGround`, `A_MineTrim`, `A_MineWalls`, `A_MineWalls2` dùng `Shader Graphs/ORM_Shader`. Graph có UniversalTarget; Unity báo shader được hỗ trợ.
- Ba material nhân vật đang dùng `M_Armature_Arms`, `M_Armature_Body`, `M_Armature_Legs` dùng `Universal Render Pipeline/Lit`, được hỗ trợ.
- Các material `M_ArmatureColor_*` trong thư mục `Built-in_RP` là phiên bản dành cho pipeline cũ; danh sách log còn gồm shader công cụ ProBuilder và material có sẵn của package. Không thay shader hàng loạt cho chúng chỉ để làm mất dòng log.
- Giữ vật liệu/shader hợp lệ. Khi thực sự có material Built-in cần chuyển, chọn đúng các material đó và dùng chuyển **Selected**. Custom shader cần xem pipeline đích và kiểm tra lỗi shader riêng; hướng dẫn chính thức: [Upgrade material assets](https://docs.unity3d.com/kr/current/Manual/upgrade-material.html).

Các dòng converter đã ghi trong Console không tự biến mất sau khi asset được sửa. Có thể **Clear** Console để phân biệt lịch sử với thông báo mới; Clear không sửa nguyên nhân và không phải bằng chứng nghiệm thu.

## 3. Polygon self-intersecting — bộ A10

Đã sửa **28 file** trong `Assets/A10 - Ultimate Modular Ruins Pack`:

- 11 file FBX và 11 file Blend: Floor_Diamond, Floor_Hole_Corner, Floor_Hole_Straight, Floor_SquareLarge, Floor_Squares, Floor_Tree, Stairs, Stairs_2, Support_Center, Support_Left, Support_Right.
- 5 file OBJ: Stairs, Stairs_2, Support_Center, Support_Left, Support_Right.
- `Preview.blend` chứa nhiều bản sao model cũng được sửa.

Phương pháp: chia polygon thành tam giác trước khi Unity nhập. Với FBX binary, sửa trực tiếp buffer polygon cùng mapping normal/UV/material/edge bằng thư viện FBX của Blender; giữ vertex buffer, object ID, transform và liên kết vật liệu. Với Blend dùng BMesh triangulate và xác nhận tọa độ vertex không đổi. Với OBJ giữ các dòng vertex/UV/normal và mapping token của từng mặt.

Blender dùng trên máy: **5.2.3 LTS**. Nguồn Blend cũ được lưu lại bằng bản Blender này; bản gốc trước nâng định dạng đã được giữ trong backup. Unity reimport bằng môi trường thực tế của máy đã qua kiểm tra. Không xóa file thay thế, không tắt kiểm tra hình học hoặc lọc cảnh báo để che lỗi.

## 4. Identifier uniqueness — bộ A12

Đã sửa **5 file** thuộc `Assets/A12 - Nature Kit/Models`:

- `FBX format/crops_cornStageD.fbx`
- `FBX format/tree_palmDetailedShort.fbx`
- `FBX format/tree_palmDetailedTall.fbx`
- `DAE format/tree_palmDetailedShort.dae`
- `DAE format/tree_palmDetailedTall.dae`

Đặt tên khác nhau cho các node/geometry trước đây cùng tên `corn` hoặc `leafs`; giữ tên lần xuất hiện đầu, thêm hậu tố cho phần trùng, đặt tên geometry rõ ràng. Giữ nguyên ID kết nối, tọa độ và material. Trước thao tác không tìm thấy tham chiếu GUID của 33 model bị ảnh hưởng trong các scene/prefab/material/asset YAML của `Assets`; GUID file vẫn được giữ khi sửa.

## Bản gốc và cách kiểm tra lại

- Bản gốc của 33 model, `.meta` tương ứng và manifest/lock: `SourceAssets/ImportFixBackups/2026-10-30/`.
- `repair_manifest.json` liệt kê từng file cùng SHA-256 trước/sau và thống kê hình học. Script sửa cùng hai danh sách input được giữ cạnh manifest để tra lại phương pháp.
- Thư mục backup nằm ngoài Assets nên Unity không nhập thêm bản lỗi cũ.
- Công cụ Editor: `Assets/_Project/Editor/AssetImportDiagnostics.cs`. Chọn file model/material rồi dùng **Tools → Son Doong → Validate Selected Imported Assets**; công cụ force reimport các file đã chọn và ghi JSON ở Logs.
- Công cụ chỉ chạy trong Editor. Nó không sửa scene đang mở, không vào Play Mode, không xóa Console và không sửa mesh khi kiểm tra.
- Khi mở lại dự án sau thay đổi package, để Unity resolve/compile xong; nếu Editor chưa refresh, dùng **Assets → Refresh**. Sau đó Clear Console và quan sát thông báo mới. Nếu phát sinh cảnh báo khác ngoài danh sách này, ghi lại đường dẫn và stack trace mới.
