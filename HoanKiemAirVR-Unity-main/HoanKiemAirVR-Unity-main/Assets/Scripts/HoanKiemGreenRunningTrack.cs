using System.Collections.Generic;
using UnityEngine;

namespace HoanKiemAirVR
{
    /// <summary>
    /// Hệ thống Đường Chạy Xanh Lá Cây Thể Thao (Green Running Track) quanh Hồ Gươm:
    /// - Nằm ở làn đường ngoài theo đúng quy hoạch phố đi bộ & thể thao Bờ Hồ
    /// - Cho phép chỉnh sửa trực quan (Interactive Handles) trực tiếp trong Unity Scene View & Inspector
    /// - Tự động sinh lưới Procedural Ribbon Mesh với đường uốn Catmull-Rom spline mượt mà
    /// - Có vạch viền trắng 2 bên mép đường chạy chống lóa, mặt sân xanh lá cây cao su thể thao
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class HoanKiemGreenRunningTrack : MonoBehaviour
    {
        [Header("Thông số hình học đường chạy")]
        [Tooltip("Bề rộng mặt đường chạy xanh (mét)")]
        [Range(1.2f, 5.0f)] public float trackWidth = 2.6f;

        [Tooltip("Bề rộng vạch viền sơn trắng 2 bên mép (mét)")]
        [Range(0.05f, 0.3f)] public float borderWidth = 0.12f;

        [Tooltip("Độ nhô cao so với mặt đường nhựa để tránh trùng mặt phẳng (Z-fighting)")]
        [Range(0.005f, 0.1f)] public float heightOffset = 0.025f;

        [Tooltip("Số phân đoạn làm mượt giữa 2 điểm mốc (Catmull-Rom Subdivisions)")]
        [Range(2, 20)] public int curveSubdivisions = 8;

        [Tooltip("Đường chạy là vòng lặp khép kín quanh hồ")]
        public bool isLoop = true;

        [Header("Vật liệu & Màu sắc")]
        [Tooltip("Màu xanh lục thể thao tươi sáng của mặt đường chạy")]
        public Color trackColor = new Color(0.08f, 0.68f, 0.38f, 1f); // #14AD61

        [Tooltip("Màu vạch viền trắng thể thao 2 bên mép")]
        public Color borderColor = new Color(0.96f, 0.96f, 0.95f, 1f);

        [Tooltip("Vật liệu mặt đường chạy chính (nếu null sẽ tự tạo vật liệu URP Lit chuẩn)")]
        public Material trackMaterial;

        [Tooltip("Vật liệu vạch viền trắng")]
        public Material borderMaterial;

        [Header("Danh sách các điểm mốc (Waypoints) trên Scene")]
        [SerializeField]
        public List<Vector3> waypoints = new List<Vector3>();

        // Cache mesh
        private Mesh generatedMesh;

        // Tọa độ vành đai đường ngoài chuẩn xác quanh Hồ Gươm (chạy dọc làn ngoài các phố)
        // Tọa độ vành đai đường ngoài chuẩn xác quanh Hồ Gươm (chạy dọc làn ngoài các phố, nằm trọn trên mặt đường nhựa)
        public static readonly Vector3[] DefaultOuterRoadWaypoints = new Vector3[]
        {
            // === BỜ TÂY: Phố Lê Thái Tổ (Làn ngoài giáp vỉa hè phía Tây Báo Hà Nội Mới / Apricot) ===
            // Lòng đường Lê Thái Tổ: X = 538 .. 556 (Rộng 18m). Làn ngoài: X ≈ 539.5 .. 540.5
            new Vector3(541.0f, -0.49f, -630.0f), // Góc Tây Bắc (ngã ba Hàng Gai - Lê Thái Tổ)
            new Vector3(540.0f, -0.49f, -670.0f), // Lê Thái Tổ Bắc
            new Vector3(539.5f, -0.49f, -710.0f), // Trước Tòa soạn Báo Hà Nội Mới (Số 44 Lê Thái Tổ)
            new Vector3(539.5f, -0.49f, -745.0f), // Đối diện Tượng đài Vua Lê
            new Vector3(539.5f, -0.49f, -780.0f), // Trước Khách sạn Apricot Hotel
            new Vector3(540.5f, -0.49f, -825.0f), // Lê Thái Tổ Nam
            new Vector3(543.0f, -0.49f, -870.0f), // Đoạn ngã tư Tràng Thi - Bà Triệu

            // === GÓC TÂY NAM & BỜ NAM: Phố Hàng Khay (Làn ngoài giáp vỉa hè dãy phố tranh Hàng Khay) ===
            // Lòng đường Hàng Khay: Z = -925 .. -942 (Rộng 17m). Làn ngoài: Z ≈ -940.5
            new Vector3(555.0f, -0.49f, -915.0f), // Cua Tây Nam vào Hàng Khay
            new Vector3(575.0f, -0.49f, -939.5f), // Đoạn Tây Hàng Khay
            new Vector3(605.0f, -0.49f, -940.5f), // Giữa phố Hàng Khay (đối diện Tháp Rùa)
            new Vector3(635.0f, -0.49f, -940.5f), // Phố tranh nghệ thuật Hàng Khay
            new Vector3(665.0f, -0.49f, -940.0f), // Đoạn Đông Hàng Khay gần Hàng Bài
            new Vector3(685.0f, -0.49f, -935.0f), // Ngã tư Tràng Tiền Plaza

            // === GÓC ĐÔNG NAM & BỜ ĐÔNG: Phố Đinh Tiên Hoàng (Làn ngoài giáp vỉa hè VNPT / Dãy nhà) ===
            // Lòng đường Đinh Tiên Hoàng: X = 692 .. 704 (Rộng 12m). Làn ngoài: X ≈ 702.5
            new Vector3(701.0f, -0.49f, -870.0f), // Đoạn Nam Đinh Tiên Hoàng
            new Vector3(702.0f, -0.49f, -810.0f), // Đối diện trụ sở UBND TP
            new Vector3(702.5f, -0.49f, -760.0f), // Vườn hoa tượng đài Lý Thái Tổ
            new Vector3(702.5f, -0.49f, -710.0f), // Phía trước Tòa nhà VNPT / Bưu điện Hà Nội
            new Vector3(702.0f, -0.49f, -665.0f), // Đối diện Đền Ngọc Sơn & Cầu Thê Húc
            new Vector3(699.0f, -0.49f, -635.0f), // Đoạn đền Bà Kiệu

            // === GÓC ĐÔNG BẮC & BỜ BẮC: Quảng trường Đông Kinh Nghĩa Thục (Làn ngoài trước Tòa nhà Hàm Cá Mập) ===
            // Lòng đường bờ Bắc: Z = -600 .. -615 (Rộng 15m). Làn ngoài: Z ≈ -602.0
            new Vector3(680.0f, -0.49f, -615.0f), // Cua vào phố Cầu Gỗ
            new Vector3(645.0f, -0.49f, -604.0f), // Quảng trường Đông Kinh Nghĩa Thục phía Đông
            new Vector3(610.0f, -0.49f, -602.0f), // Phía trước Tòa nhà Hàm Cá Mập (Shark Jaw Building)
            new Vector3(575.0f, -0.49f, -606.0f), // Cửa ngõ phố Hàng Gai
            new Vector3(552.0f, -0.49f, -618.0f)  // Nối vòng cung khép kín về (541, -0.49, -630)
        };

        private void Reset()
        {
            ResetToDefaultWaypoints();
        }

        public void ResetToDefaultWaypoints()
        {
            waypoints.Clear();
            foreach (var pt in DefaultOuterRoadWaypoints)
            {
                waypoints.Add(pt);
            }
            RebuildMesh();
        }

        private void OnValidate()
        {
            if (waypoints == null || waypoints.Count < 2)
            {
                ResetToDefaultWaypoints();
            }
        }

        /// <summary>
        /// Tạo hoặc tái tạo toàn bộ lưới 3D Procedural Ribbon Mesh
        /// </summary>
        [ContextMenu("Rebuild Running Track Mesh")]
        public void RebuildMesh()
        {
            if (waypoints == null || waypoints.Count < 3) return;

            // Đảm bảo Materials tồn tại
            EnsureMaterials();

            // 1. Tính toán danh sách điểm cong Spline (Catmull-Rom)
            List<Vector3> splinePoints = GenerateSmoothSpline(waypoints, curveSubdivisions, isLoop);
            int count = splinePoints.Count;
            if (count < 2) return;

            // 2. Tạo đỉnh (Vertices), Pháp tuyến (Normals), UVs, và Tam giác (Triangles)
            // Cấu trúc 4 dải điểm ngang trên mỗi tiết diện:
            // [0] Mép ngoài viền trái  (-trackWidth/2)
            // [1] Mép trong viền trái  (-trackWidth/2 + borderWidth)
            // [2] Mép trong viền phải (+trackWidth/2 - borderWidth)
            // [3] Mép ngoài viền phải (+trackWidth/2)
            // => Dải [0..1] là Viền Trái (Submesh 1 - Trắng)
            // => Dải [1..2] là Lòng Đường Chạy (Submesh 0 - Xanh lục)
            // => Dải [2..3] là Viền Phải (Submesh 1 - Trắng)

            List<Vector3> verts = new List<Vector3>(count * 4);
            List<Vector3> normals = new List<Vector3>(count * 4);
            List<Vector2> uvs = new List<Vector2>(count * 4);

            List<int> trackTris = new List<int>();
            List<int> borderTris = new List<int>();

            float halfWidth = trackWidth * 0.5f;
            float innerHalfWidth = Mathf.Max(0.1f, halfWidth - borderWidth);

            float accumulatedDist = 0f;

            for (int i = 0; i < count; i++)
            {
                Vector3 current = splinePoints[i];
                Vector3 prev = (i > 0) ? splinePoints[i - 1] : (isLoop ? splinePoints[count - 2] : current);
                Vector3 next = (i < count - 1) ? splinePoints[i + 1] : (isLoop ? splinePoints[1] : current);

                Vector3 forward = (next - prev).normalized;
                if (forward == Vector3.zero) forward = Vector3.forward;

                Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

                if (i > 0)
                {
                    accumulatedDist += Vector3.Distance(current, splinePoints[i - 1]);
                }

                // Điểm nâng cao nhẹ theo trục Y
                Vector3 center = current + Vector3.up * heightOffset;

                Vector3 p0 = center - right * halfWidth;
                Vector3 p1 = center - right * innerHalfWidth;
                Vector3 p2 = center + right * innerHalfWidth;
                Vector3 p3 = center + right * halfWidth;

                // Chuyển sang Local Space của GameObject
                p0 = transform.InverseTransformPoint(p0);
                p1 = transform.InverseTransformPoint(p1);
                p2 = transform.InverseTransformPoint(p2);
                p3 = transform.InverseTransformPoint(p3);

                verts.Add(p0);
                verts.Add(p1);
                verts.Add(p2);
                verts.Add(p3);

                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);
                normals.Add(Vector3.up);

                float v = accumulatedDist * 0.5f;
                uvs.Add(new Vector2(0.0f, v));
                uvs.Add(new Vector2(0.1f, v));
                uvs.Add(new Vector2(0.9f, v));
                uvs.Add(new Vector2(1.0f, v));

                if (i < count - 1)
                {
                    int r0 = i * 4;
                    int r1 = (i + 1) * 4;

                    // Viền trái (p0 -> p1)
                    borderTris.Add(r0 + 0); borderTris.Add(r1 + 0); borderTris.Add(r0 + 1);
                    borderTris.Add(r0 + 1); borderTris.Add(r1 + 0); borderTris.Add(r1 + 1);

                    // Lòng đường xanh lục (p1 -> p2)
                    trackTris.Add(r0 + 1); trackTris.Add(r1 + 1); trackTris.Add(r0 + 2);
                    trackTris.Add(r0 + 2); trackTris.Add(r1 + 1); trackTris.Add(r1 + 2);

                    // Viền phải (p2 -> p3)
                    borderTris.Add(r0 + 2); borderTris.Add(r1 + 2); borderTris.Add(r0 + 3);
                    borderTris.Add(r0 + 3); borderTris.Add(r1 + 2); borderTris.Add(r1 + 3);
                }
            }

            if (generatedMesh == null)
            {
                generatedMesh = new Mesh();
                generatedMesh.name = "HoanKiem_GreenRunningTrack_Mesh";
            }
            else
            {
                generatedMesh.Clear();
            }

            generatedMesh.SetVertices(verts);
            generatedMesh.SetNormals(normals);
            generatedMesh.SetUVs(0, uvs);

            generatedMesh.subMeshCount = 2;
            generatedMesh.SetTriangles(trackTris, 0);   // Submesh 0: Mặt đường xanh
            generatedMesh.SetTriangles(borderTris, 1);  // Submesh 1: Vạch viền trắng

            generatedMesh.RecalculateBounds();

            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf != null) mf.sharedMesh = generatedMesh;

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterials = new Material[] { trackMaterial, borderMaterial };
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = true;
            }
        }

        public void EnsureMaterials()
        {
            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");

            if (trackMaterial == null)
            {
                trackMaterial = new Material(urpLit);
                trackMaterial.name = "MAT_GreenRunningTrack";
                if (trackMaterial.HasProperty("_BaseColor")) trackMaterial.SetColor("_BaseColor", trackColor);
                if (trackMaterial.HasProperty("_Color")) trackMaterial.SetColor("_Color", trackColor);
                trackMaterial.SetFloat("_Smoothness", 0.15f); // Nhám cao su thể thao chống trơn trượt
            }
            else
            {
                if (trackMaterial.HasProperty("_BaseColor")) trackMaterial.SetColor("_BaseColor", trackColor);
                if (trackMaterial.HasProperty("_Color")) trackMaterial.SetColor("_Color", trackColor);
            }

            if (borderMaterial == null)
            {
                borderMaterial = new Material(urpLit);
                borderMaterial.name = "MAT_TrackBorderWhite";
                if (borderMaterial.HasProperty("_BaseColor")) borderMaterial.SetColor("_BaseColor", borderColor);
                if (borderMaterial.HasProperty("_Color")) borderMaterial.SetColor("_Color", borderColor);
                borderMaterial.SetFloat("_Smoothness", 0.35f);
            }
            else
            {
                if (borderMaterial.HasProperty("_BaseColor")) borderMaterial.SetColor("_BaseColor", borderColor);
                if (borderMaterial.HasProperty("_Color")) borderMaterial.SetColor("_Color", borderColor);
            }
        }

        /// <summary>
        /// Thuật toán Catmull-Rom Spline tạo dải điểm nội suy uốn lượn mượt mà
        /// </summary>
        public static List<Vector3> GenerateSmoothSpline(List<Vector3> pts, int subdivisions, bool loop)
        {
            List<Vector3> result = new List<Vector3>();
            int n = pts.Count;
            if (n < 2) return result;

            int segmentCount = loop ? n : (n - 1);

            for (int i = 0; i < segmentCount; i++)
            {
                Vector3 p0 = loop ? pts[(i - 1 + n) % n] : pts[Mathf.Max(i - 1, 0)];
                Vector3 p1 = pts[i];
                Vector3 p2 = loop ? pts[(i + 1) % n] : pts[Mathf.Min(i + 1, n - 1)];
                Vector3 p3 = loop ? pts[(i + 2) % n] : pts[Mathf.Min(i + 2, n - 1)];

                for (int step = 0; step < subdivisions; step++)
                {
                    float t = (float)step / subdivisions;
                    Vector3 interpolated = CatmullRom(p0, p1, p2, p3, t);
                    result.Add(interpolated);
                }
            }

            if (loop)
            {
                result.Add(result[0]); // Nối điểm cuối trùng khớp điểm đầu
            }
            else
            {
                result.Add(pts[n - 1]);
            }

            return result;
        }

        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2.0f * p1) +
                (-p0 + p2) * t +
                (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
                (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t3
            );
        }

        /// <summary>
        /// Bắn tia Raycast xuống đất để tất cả các điểm waypoint bám dính chuẩn vào mặt đường
        /// </summary>
        public void SnapWaypointsToGround()
        {
            for (int i = 0; i < waypoints.Count; i++)
            {
                Vector3 origin = waypoints[i] + Vector3.up * 5.0f;
                if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 20.0f))
                {
                    waypoints[i] = new Vector3(waypoints[i].x, hit.point.y, waypoints[i].z);
                }
            }
            RebuildMesh();
        }
    }
}
