using System.Collections.Generic;
using UnityEngine;

namespace HoanKiemAirVR
{
    /// <summary>
    /// Hệ thống Vạch Kẻ Đường Chuẩn Thực Tế Xung Quanh Hồ Gươm (QCVN 41:2019/BGTVT):
    /// - Vạch người đi bộ qua đường (Zebra Crossings) tại 8 nút giao & thắng cảnh trọng điểm
    /// - Vạch dừng xe (Stop Lines) trước vạch đi bộ
    /// - Vạch đứt phân chia các làn xe chạy (Dashed Lane Dividers)
    /// - Vạch đôi vàng phân chia tim đường (Double Yellow Centerlines)
    /// - Mũi tên sơn chỉ hướng rẽ / đi thẳng trên mặt đường
    /// - Vạch trạm dừng đỗ xe buýt Bờ Hồ
    /// </summary>
    [ExecuteInEditMode]
    public class HoanKiemRoadMarkings : MonoBehaviour
    {
        [Header("Độ cao vạch kẻ chống trùng mặt phẳng")]
        public float surfaceY = -0.472f;

        [Header("Vật liệu sơn đường URP")]
        public Material whitePaintMaterial;
        public Material yellowPaintMaterial;

        [Header("Tùy chọn hiển thị")]
        public bool showZebraCrossings = true;
        public bool showLaneDividers = true;
        public bool showCenterlines = true;
        public bool showStopLines = true;
        public bool showDirectionArrows = true;
        public bool showBusStops = true;

        public struct ZebraConfig
        {
            public string name;
            public Vector3 center;
            public float width;    // Bề rộng toàn dải (chiều xe chạy, ví dụ 5m)
            public float length;   // Chiều dài vạch ngang qua đường (ví dụ 12m)
            public float rotationY;// Góc xoay mặt đường
            public bool hasStopLine;

            public ZebraConfig(string n, Vector3 c, float w, float l, float rot, bool stop = true)
            {
                name = n;
                center = c;
                width = w;
                length = l;
                rotationY = rot;
                hasStopLine = stop;
            }
        }

        // Tọa độ các vị trí vạch sang đường thực tế quanh Bờ Hồ (kết nối vỉa hè hồ với vỉa hè ngoài)
        public static readonly ZebraConfig[] RealZebraCrossings = new ZebraConfig[]
        {
            // 1. Trước Đền Ngọc Sơn & Cầu Thê Húc (Đinh Tiên Hoàng: X = 692..704)
            new ZebraConfig("Vạch Đền Ngọc Sơn", new Vector3(698.0f, 0f, -660.0f), 5.5f, 12.0f, -90f),

            // 2. Trước Tòa nhà VNPT / Bưu điện Hà Nội (Đinh Tiên Hoàng sang Bờ Hồ)
            new ZebraConfig("Vạch Bưu Điện Hà Nội", new Vector3(698.0f, 0f, -710.0f), 5.5f, 12.0f, -90f),

            // 3. Trước Tượng đài Vườn hoa Lý Thái Tổ (Đinh Tiên Hoàng)
            new ZebraConfig("Vạch Vườn Hoa Lý Thái Tổ", new Vector3(698.0f, 0f, -775.0f), 5.0f, 12.0f, -90f),

            // 4. Ngã tư Hàng Bài - Tràng Tiền - Đinh Tiên Hoàng (Tràng Tiền Plaza)
            new ZebraConfig("Vạch Tràng Tiền Plaza", new Vector3(692.0f, 0f, -880.0f), 6.0f, 14.0f, -80f),

            // 5. Ngã tư Hàng Khay - Bà Triệu (Đoạn giữa phố Hàng Khay, đối diện Tháp Rùa: Z = -925..-942)
            new ZebraConfig("Vạch Hàng Khay - Bà Triệu", new Vector3(620.0f, 0f, -933.5f), 5.5f, 17.0f, 0f),

            // 6. Ngã ba Lê Thái Tổ - Hàng Khay - Tràng Thi (Góc Tây Nam)
            new ZebraConfig("Vạch Góc Lê Thái Tổ - Tràng Thi", new Vector3(550.0f, 0f, -885.0f), 5.5f, 18.0f, 40f),

            // 7. Trước Tòa soạn Báo Hà Nội Mới (Số 44 Lê Thái Tổ sang Bờ Hồ: X = 538..556)
            new ZebraConfig("Vạch Báo Hà Nội Mới", new Vector3(547.0f, 0f, -710.0f), 5.5f, 18.0f, 90f),

            // 8. Trước Khách sạn Apricot Hotel (Lê Thái Tổ sang Bờ Hồ)
            new ZebraConfig("Vạch Apricot Hotel", new Vector3(547.0f, 0f, -780.0f), 5.5f, 18.0f, 90f),

            // 9. Vòng cung Quảng trường Đông Kinh Nghĩa Thục (Trước Tòa nhà Hàm Cá Mập sang Hồ)
            new ZebraConfig("Vạch Đông Kinh Nghĩa Thục", new Vector3(610.0f, 0f, -608.0f), 6.0f, 16.0f, 0f)
        };

        private void Start()
        {
            if (transform.childCount == 0)
            {
                GenerateAllMarkings();
            }
        }

        public void EnsureMaterials()
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");

            if (whitePaintMaterial == null)
            {
                whitePaintMaterial = new Material(urpLit);
                whitePaintMaterial.name = "MAT_RoadMarking_White";
                Color c = new Color(0.96f, 0.96f, 0.95f, 1f);
                if (whitePaintMaterial.HasProperty("_BaseColor")) whitePaintMaterial.SetColor("_BaseColor", c);
                if (whitePaintMaterial.HasProperty("_Color")) whitePaintMaterial.SetColor("_Color", c);
                whitePaintMaterial.SetFloat("_Smoothness", 0.3f);
            }

            if (yellowPaintMaterial == null)
            {
                yellowPaintMaterial = new Material(urpLit);
                yellowPaintMaterial.name = "MAT_RoadMarking_Yellow";
                Color y = new Color(0.96f, 0.78f, 0.05f, 1f); // Vàng giao thông rực rỡ
                if (yellowPaintMaterial.HasProperty("_BaseColor")) yellowPaintMaterial.SetColor("_BaseColor", y);
                if (yellowPaintMaterial.HasProperty("_Color")) yellowPaintMaterial.SetColor("_Color", y);
                yellowPaintMaterial.SetFloat("_Smoothness", 0.3f);
            }
        }

        [ContextMenu("Tạo toàn bộ vạch kẻ đường")]
        public void GenerateAllMarkings()
        {
            EnsureMaterials();

            // Xóa các đối tượng cũ
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            // 1. Tạo Vạch Đi Bộ Qua Đường (Zebra Crossings)
            if (showZebraCrossings)
            {
                GameObject zebraRoot = new GameObject("1_ZebraCrossings_VachDiBo");
                zebraRoot.transform.SetParent(transform, false);

                foreach (var z in RealZebraCrossings)
                {
                    CreateZebraCrossing(zebraRoot.transform, z);
                }
            }

            // 2. Tạo Vạch Đứt Phân Làn Xe (Lane Dividers)
            if (showLaneDividers)
            {
                GameObject lanesRoot = new GameObject("2_LaneDividers_PhanLan");
                lanesRoot.transform.SetParent(transform, false);
                CreateDashedLaneDividers(lanesRoot.transform);
            }

            // 3. Tạo Vạch Đôi Vàng Tim Đường (Centerlines)
            if (showCenterlines)
            {
                GameObject centerRoot = new GameObject("3_Centerlines_VachVangTimDuong");
                centerRoot.transform.SetParent(transform, false);
                CreateDoubleYellowCenterlines(centerRoot.transform);
            }

            // 4. Tạo Mũi Tên Chỉ Hướng Sơn Trên Mặt Đường
            if (showDirectionArrows)
            {
                GameObject arrowRoot = new GameObject("4_DirectionArrows_MuiTenChiHuong");
                arrowRoot.transform.SetParent(transform, false);
                CreateDirectionArrows(arrowRoot.transform);
            }

            // 5. Tạo Trạm Xe Buýt
            if (showBusStops)
            {
                GameObject busRoot = new GameObject("5_BusStops_TramXeBuyt");
                busRoot.transform.SetParent(transform, false);
                CreateBusStopMarkings(busRoot.transform);
            }
        }

        private void CreateZebraCrossing(Transform parent, ZebraConfig cfg)
        {
            GameObject zebraObj = new GameObject(cfg.name);
            zebraObj.transform.SetParent(parent, false);
            zebraObj.transform.position = new Vector3(cfg.center.x, surfaceY, cfg.center.z);
            zebraObj.transform.rotation = Quaternion.Euler(0f, cfg.rotationY, 0f);

            // Tạo các nan sọc trắng: Bề rộng mỗi nan 0.5m, khe hở 0.5m
            float stripeWidth = 0.5f;
            float stripeGap = 0.5f;
            float step = stripeWidth + stripeGap;
            int stripeCount = Mathf.FloorToInt(cfg.length / step);

            float startOffset = -((stripeCount - 1) * step) * 0.5f;

            for (int i = 0; i < stripeCount; i++)
            {
                float offsetAlongLength = startOffset + i * step;

                // Mỗi vạch là 1 Quad dẹt trên mặt đất
                GameObject stripe = CreateFlatQuad($"Stripe_{i}", stripeWidth, cfg.width, whitePaintMaterial);
                stripe.transform.SetParent(zebraObj.transform, false);
                stripe.transform.localPosition = new Vector3(offsetAlongLength, 0f, 0f);
            }

            // Tạo vạch dừng xe (Stop line) dày 0.45m đặt phía trước vạch đi bộ 3m
            if (cfg.hasStopLine && showStopLines)
            {
                float stopLineWidth = cfg.length * 0.95f;
                GameObject stopLine1 = CreateFlatQuad("StopLine_Forward", stopLineWidth, 0.45f, whitePaintMaterial);
                stopLine1.transform.SetParent(zebraObj.transform, false);
                stopLine1.transform.localPosition = new Vector3(0f, 0f, (cfg.width * 0.5f) + 3.0f);

                GameObject stopLine2 = CreateFlatQuad("StopLine_Backward", stopLineWidth, 0.45f, whitePaintMaterial);
                stopLine2.transform.SetParent(zebraObj.transform, false);
                stopLine2.transform.localPosition = new Vector3(0f, 0f, -((cfg.width * 0.5f) + 3.0f));
            }
        }

        private void CreateDashedLaneDividers(Transform parent)
        {
            float dashLen = 3.0f;
            float gapLen = 3.0f;
            float dashWidth = 0.18f;
            float step = dashLen + gapLen;

            // 1. Tuyến Đinh Tiên Hoàng (Bờ Đông: X = 692..704, Centerline = 698)
            CreateDashedLineLinear(parent, "Dashes_DinhTienHoang_L1", new Vector3(695.0f, surfaceY, -640.0f), new Vector3(695.0f, surfaceY, -870.0f), dashLen, gapLen, dashWidth);
            CreateDashedLineLinear(parent, "Dashes_DinhTienHoang_L2", new Vector3(701.0f, surfaceY, -640.0f), new Vector3(701.0f, surfaceY, -870.0f), dashLen, gapLen, dashWidth);

            // 2. Tuyến Lê Thái Tổ (Bờ Tây: X = 538..556, Centerline = 547)
            CreateDashedLineLinear(parent, "Dashes_LeThaiTo_L1", new Vector3(542.5f, surfaceY, -645.0f), new Vector3(542.5f, surfaceY, -870.0f), dashLen, gapLen, dashWidth);
            CreateDashedLineLinear(parent, "Dashes_LeThaiTo_L2", new Vector3(551.5f, surfaceY, -645.0f), new Vector3(551.5f, surfaceY, -870.0f), dashLen, gapLen, dashWidth);

            // 3. Tuyến Hàng Khay (Bờ Nam: Z = -925..-942, Centerline = -933.5)
            CreateDashedLineLinear(parent, "Dashes_HangKhay_L1", new Vector3(580.0f, surfaceY, -929.0f), new Vector3(665.0f, surfaceY, -929.0f), dashLen, gapLen, dashWidth);
            CreateDashedLineLinear(parent, "Dashes_HangKhay_L2", new Vector3(580.0f, surfaceY, -938.0f), new Vector3(665.0f, surfaceY, -938.0f), dashLen, gapLen, dashWidth);
        }

        private void CreateDashedLineLinear(Transform parent, string name, Vector3 start, Vector3 end, float dashLen, float gapLen, float width)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent, false);

            float totalDist = Vector3.Distance(start, end);
            Vector3 dir = (end - start).normalized;
            float rotY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            float step = dashLen + gapLen;
            int count = Mathf.FloorToInt(totalDist / step);

            for (int i = 0; i < count; i++)
            {
                float distAlong = i * step + dashLen * 0.5f;
                Vector3 pos = start + dir * distAlong;

                GameObject dash = CreateFlatQuad($"Dash_{i}", width, dashLen, whitePaintMaterial);
                dash.transform.SetParent(group.transform, false);
                dash.transform.position = pos;
                dash.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
            }
        }

        private void CreateDoubleYellowCenterlines(Transform parent)
        {
            float lineWidth = 0.15f;
            float lineGap = 0.15f;

            // 1. Phố Đinh Tiên Hoàng (Tim đường chuẩn X = 698 giữa lòng đường 12m)
            CreateDoubleSolidLine(parent, "Centerline_DinhTienHoang", new Vector3(698.0f, surfaceY, -640.0f), new Vector3(698.0f, surfaceY, -870.0f), lineWidth, lineGap);

            // 2. Phố Lê Thái Tổ (Tim đường chuẩn X = 547 giữa lòng đường 18m)
            CreateDoubleSolidLine(parent, "Centerline_LeThaiTo", new Vector3(547.0f, surfaceY, -645.0f), new Vector3(547.0f, surfaceY, -870.0f), lineWidth, lineGap);

            // 3. Phố Hàng Khay (Tim đường chuẩn Z = -933.5 giữa lòng đường 17m)
            CreateDoubleSolidLine(parent, "Centerline_HangKhay", new Vector3(575.0f, surfaceY, -933.5f), new Vector3(670.0f, surfaceY, -933.5f), lineWidth, lineGap);
        }

        private void CreateDoubleSolidLine(Transform parent, string name, Vector3 start, Vector3 end, float width, float gap)
        {
            GameObject lineObj = new GameObject(name);
            lineObj.transform.SetParent(parent, false);

            Vector3 dir = (end - start).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, dir).normalized;
            float dist = Vector3.Distance(start, end);
            float rotY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            Vector3 center = (start + end) * 0.5f;
            float offsetDist = (width + gap) * 0.5f;

            // Vạch vàng 1
            GameObject v1 = CreateFlatQuad("Yellow_Line_Left", width, dist, yellowPaintMaterial);
            v1.transform.SetParent(lineObj.transform, false);
            v1.transform.position = center - right * offsetDist;
            v1.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

            // Vạch vàng 2
            GameObject v2 = CreateFlatQuad("Yellow_Line_Right", width, dist, yellowPaintMaterial);
            v2.transform.SetParent(lineObj.transform, false);
            v2.transform.position = center + right * offsetDist;
            v2.transform.rotation = Quaternion.Euler(0f, rotY, 0f);
        }

        private void CreateDirectionArrows(Transform parent)
        {
            // Các vị trí mũi tên chuẩn bị rẽ trước các ngã tư
            // 1. Góc Tràng Tiền - Đinh Tiên Hoàng
            CreateArrow(parent, "Arrow_TurnLeft_TrangTien", new Vector3(683.0f, surfaceY, -840.0f), 180f, false, true);
            CreateArrow(parent, "Arrow_Straight_TrangTien", new Vector3(691.0f, surfaceY, -840.0f), 180f, true, false);

            // 2. Góc Hàng Gai - Lương Văn Can (Bờ Bắc Lê Thái Tổ)
            CreateArrow(parent, "Arrow_Straight_HangGai", new Vector3(547.0f, surfaceY, -660.0f), 0f, true, false);
            CreateArrow(parent, "Arrow_TurnRight_HangGai", new Vector3(553.0f, surfaceY, -660.0f), 0f, false, false, true);

            // 3. Phố Hàng Khay - Tràng Thi
            CreateArrow(parent, "Arrow_Straight_HangKhay", new Vector3(590.0f, surfaceY, -923.0f), -90f, true, false);
        }

        private void CreateArrow(Transform parent, string name, Vector3 pos, float rotY, bool straight, bool left = false, bool right = false)
        {
            GameObject arrowObj = new GameObject(name);
            arrowObj.transform.SetParent(parent, false);
            arrowObj.transform.position = pos;
            arrowObj.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

            // Thân mũi tên
            GameObject stem = CreateFlatQuad("Stem", 0.35f, 3.5f, whitePaintMaterial);
            stem.transform.SetParent(arrowObj.transform, false);
            stem.transform.localPosition = new Vector3(0f, 0f, -0.5f);

            if (straight)
            {
                // Cánh mũi tên đi thẳng
                GameObject headLeft = CreateFlatQuad("Head_Left", 0.3f, 1.6f, whitePaintMaterial);
                headLeft.transform.SetParent(arrowObj.transform, false);
                headLeft.transform.localPosition = new Vector3(-0.45f, 0f, 1.2f);
                headLeft.transform.localRotation = Quaternion.Euler(0f, 35f, 0f);

                GameObject headRight = CreateFlatQuad("Head_Right", 0.3f, 1.6f, whitePaintMaterial);
                headRight.transform.SetParent(arrowObj.transform, false);
                headRight.transform.localPosition = new Vector3(0.45f, 0f, 1.2f);
                headRight.transform.localRotation = Quaternion.Euler(0f, -35f, 0f);
            }
            else if (left)
            {
                // Mũi rẽ trái
                GameObject curve = CreateFlatQuad("CurveLeft", 0.35f, 1.8f, whitePaintMaterial);
                curve.transform.SetParent(arrowObj.transform, false);
                curve.transform.localPosition = new Vector3(-0.7f, 0f, 1.1f);
                curve.transform.localRotation = Quaternion.Euler(0f, -45f, 0f);
            }
            else if (right)
            {
                // Mũi rẽ phải
                GameObject curve = CreateFlatQuad("CurveRight", 0.35f, 1.8f, whitePaintMaterial);
                curve.transform.SetParent(arrowObj.transform, false);
                curve.transform.localPosition = new Vector3(0.7f, 0f, 1.1f);
                curve.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            }
        }

        private void CreateBusStopMarkings(Transform parent)
        {
            // Trạm xe buýt trước Bưu điện Hà Nội (Đinh Tiên Hoàng)
            GameObject busVNPT = new GameObject("BusStop_VNPT_Hanoi");
            busVNPT.transform.SetParent(parent, false);
            busVNPT.transform.position = new Vector3(697.0f, surfaceY, -725.0f);
            busVNPT.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

            // Hộp vàng kích thước 15m x 3.2m
            CreateYellowBoxFrame(busVNPT.transform, 16.0f, 3.2f);

            // Chữ "BUS" bằng các vạch trắng tượng trưng
            GameObject busText = CreateFlatQuad("BUS_Marker", 2.2f, 8.0f, yellowPaintMaterial);
            busText.transform.SetParent(busVNPT.transform, false);
            busText.transform.localPosition = Vector3.zero;
        }

        private void CreateYellowBoxFrame(Transform parent, float length, float width)
        {
            float w = 0.25f;
            // 4 cạnh hộp vàng
            GameObject top = CreateFlatQuad("Top", length, w, yellowPaintMaterial);
            top.transform.SetParent(parent, false);
            top.transform.localPosition = new Vector3(0f, 0f, width * 0.5f);

            GameObject bottom = CreateFlatQuad("Bottom", length, w, yellowPaintMaterial);
            bottom.transform.SetParent(parent, false);
            bottom.transform.localPosition = new Vector3(0f, 0f, -width * 0.5f);

            GameObject left = CreateFlatQuad("Left", w, width, yellowPaintMaterial);
            left.transform.SetParent(parent, false);
            left.transform.localPosition = new Vector3(-length * 0.5f, 0f, 0f);

            GameObject right = CreateFlatQuad("Right", w, width, yellowPaintMaterial);
            right.transform.SetParent(parent, false);
            right.transform.localPosition = new Vector3(length * 0.5f, 0f, 0f);
        }

        /// <summary>
        /// Tạo nhanh một Mesh Quad dẹt trên mặt phẳng XZ
        /// </summary>
        public static GameObject CreateFlatQuad(string name, float sizeX, float sizeZ, Material mat)
        {
            GameObject quad = new GameObject(name);
            MeshFilter mf = quad.AddComponent<MeshFilter>();
            MeshRenderer mr = quad.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();
            mesh.name = name + "_Mesh";

            float hx = sizeX * 0.5f;
            float hz = sizeZ * 0.5f;

            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-hx, 0f, -hz),
                new Vector3(-hx, 0f,  hz),
                new Vector3( hx, 0f,  hz),
                new Vector3( hx, 0f, -hz)
            };

            Vector3[] normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
            Vector2[] uvs = new Vector2[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };
            int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };

            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = true;

            return quad;
        }
    }
}
