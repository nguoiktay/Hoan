#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace HoanKiemAirVR.Editor
{
    /// <summary>
    /// Công cụ Bút Vẽ / Chấm Đường Trực Quan trong Scene View:
    /// Cho phép người dùng click chuột trực tiếp lên mặt đường/vỉa hè trong Scene View
    /// để tạo các điểm Waypoint với độ chính xác tuyệt đối từng centimet.
    /// </summary>
    public class WaypointPathPainter : EditorWindow
    {
        public enum PathTarget
        {
            DuongBe_ChayBoVenHo,       // Đường bé ven hồ dành cho người chạy bộ / đi dạo
            DuongTo_DinhTienHoang,     // Đường to phố đi bộ Đinh Tiên Hoàng
            DuongTo_LeThaiTo,          // Đường phố Lê Thái Tổ
            DuongTo_HangKhay           // Đường phố Hàng Khay
        }

        private static bool isPainting = false;
        private static PathTarget currentTarget = PathTarget.DuongBe_ChayBoVenHo;
        private static float pointSpacingMin = 1.0f; // Khoảng cách tối thiểu giữa 2 điểm
        private static bool autoSnapGround = true;
        private static float groundHeightOffset = 0.05f;

        [MenuItem("HoanKiem VR/Công Cụ Vẽ Đường Bằng Chuột (Path Painter)", false, 2)]
        public static void ShowWindow()
        {
            var win = GetWindow<WaypointPathPainter>("Vẽ Đường Đi");
            win.minSize = new Vector2(380, 480);
            win.Show();
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            isPainting = false;
        }

        private void OnGUI()
        {
            // Banner Tiêu Đề
            DrawHeader();

            EditorGUILayout.Space(10);

            // Chọn Tuyến Đường Cần Vẽ
            DrawTargetSelector();

            EditorGUILayout.Space(10);

            // Nút Bật / Tắt Chế Độ Vẽ
            DrawPaintToggle();

            EditorGUILayout.Space(10);

            // Hướng dẫn phím tắt
            DrawGuide();

            EditorGUILayout.Space(12);

            // Các Công Cụ Tiện Ích (Xóa, Làm mịn, Đảo chiều, Bắt đất)
            DrawUtilityButtons();

            EditorGUILayout.Space(12);

            // Tăng số lượng người & Bake ngay
            DrawCrowdControls();
        }

        private void DrawHeader()
        {
            GUIStyle headerBox = new GUIStyle(GUI.skin.box);
            headerBox.padding = new RectOffset(12, 12, 12, 12);
            headerBox.normal.background = Texture2D.linearGrayTexture;

            EditorGUILayout.BeginVertical(headerBox);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 14;
            titleStyle.normal.textColor = new Color(0.1f, 0.9f, 0.5f);
            EditorGUILayout.LabelField("BÚT VẼ ĐƯỜNG TRỰC QUAN (SCENE PATH PAINTER)", titleStyle);

            EditorGUILayout.LabelField("Click chuột trực tiếp lên mặt đường trong Scene View để chấm điểm chính xác 100%!", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawTargetSelector()
        {
            EditorGUILayout.LabelField("1. Chọn tuyến đường muốn vẽ / chỉnh sửa:", EditorStyles.boldLabel);

            currentTarget = (PathTarget)EditorGUILayout.EnumPopup("Tuyến đường:", currentTarget);

            Transform container = GetContainerForTarget(currentTarget);
            int count = (container != null) ? container.childCount : 0;

            GUIStyle countStyle = new GUIStyle(EditorStyles.helpBox);
            countStyle.fontSize = 11;
            EditorGUILayout.LabelField($"Hiện có: {count} điểm Waypoint trên tuyến này.", countStyle);
        }

        private void DrawPaintToggle()
        {
            EditorGUILayout.LabelField("2. Bật / Tắt chế độ vẽ chuột:", EditorStyles.boldLabel);

            if (isPainting)
            {
                GUI.backgroundColor = new Color(0.95f, 0.35f, 0.35f);
                if (GUILayout.Button(" DỪNG VẼ (EXIT DRAW MODE) [Phím Esc]", GUILayout.Height(42)))
                {
                    isPainting = false;
                    SceneView.RepaintAll();
                }
            }
            else
            {
                GUI.backgroundColor = new Color(0.2f, 0.85f, 0.4f);
                if (GUILayout.Button(" BẬT CHẾ ĐỘ CLICK ĐỂ VẼ ĐƯỜNG (BẮT ĐẦU CHẤM ĐIỂM)", GUILayout.Height(42)))
                {
                    isPainting = true;
                    // Đảm bảo container tồn tại
                    EnsureContainerExists(currentTarget);
                    SceneView.RepaintAll();
                    FocusScene();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawGuide()
        {
            EditorGUILayout.HelpBox(
                "THAO TÁC TRÊN SCENE VIEW KHI ĐANG BẬT BÚT VẼ:\n" +
                "• Click Chuột Trái: Chấm 1 điểm Waypoint mới ngay tại vị trí con trỏ chuột.\n" +
                "• Giữ Shift + Click: Chèn điểm vào vị trí mong muốn.\n" +
                "• Giữ Ctrl + Click: Xóa điểm gần con trỏ chuột nhất.\n" +
                "• Phím Backspace: Xóa điểm vừa chấm gần nhất.\n" +
                "• Phím Esc / Enter: Thoát chế độ vẽ đường.\n" +
                "• Giữ Chuột Phải: Xoay Camera Scene để di chuyển góc nhìn như bình thường.",
                MessageType.Info
            );
        }

        private void DrawUtilityButtons()
        {
            EditorGUILayout.LabelField("3. Thao tác hỗ trợ tuyến đường:", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Bắt Dính Mặt Đường (Snap)"))
            {
                SnapCurrentPathToGround();
            }

            if (GUILayout.Button("Đảo Chiều Đường (Reverse)"))
            {
                ReverseCurrentPath();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.9f, 0.4f, 0.4f);
            if (GUILayout.Button("Xóa Toàn Bộ Điểm Tuyến Này Để Vẽ Lại"))
            {
                if (EditorUtility.DisplayDialog("Xác nhận", "Bạn có chắc chắn muốn xóa toàn bộ điểm của tuyến này để vẽ lại từ đầu?", "Xóa", "Hủy"))
                {
                    ClearCurrentPath();
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawCrowdControls()
        {
            EditorGUILayout.LabelField("4. Mật độ người & Sinh vào Scene:", EditorStyles.boldLabel);

            HoanKiemCrowdSystem sys = FindCrowdSystem();
            if (sys != null)
            {
                sys.pedestrianCount = EditorGUILayout.IntSlider("Người đi bộ đường to:", sys.pedestrianCount, 10, 100);
                sys.joggerCount = EditorGUILayout.IntSlider("Người chạy đường bé:", sys.joggerCount, 2, 30);

                EditorGUILayout.Space(4);
                GUI.backgroundColor = new Color(0.1f, 0.85f, 0.4f);
                if (GUILayout.Button(" BAKE / LÀM MỚI ĐÁM ĐÔNG THEO ĐƯỜNG MỚI", GUILayout.Height(36)))
                {
                    sys.SpawnCrowd();
                    EditorSceneManager.MarkSceneDirty(sys.gameObject.scene);
                }
                GUI.backgroundColor = Color.white;
            }
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!isPainting) return;

            Event e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Vector3 hitPoint = Vector3.zero;
            bool hasHit = false;

            // 1. Thử Raycast trúng Mesh trong Scene
            if (Physics.Raycast(ray, out RaycastHit hit, 2000f))
            {
                hitPoint = hit.point;
                hasHit = true;
            }
            else
            {
                // Nếu không trúng collider, bắt vào mặt phẳng Y = -0.49f
                Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, -0.49f, 0f));
                if (groundPlane.Raycast(ray, out float enter))
                {
                    hitPoint = ray.GetPoint(enter);
                    hasHit = true;
                }
            }

            if (!hasHit) return;

            hitPoint.y += groundHeightOffset;

            Transform container = GetContainerForTarget(currentTarget);
            Vector3 lastPoint = Vector3.zero;
            bool hasLastPoint = false;

            if (container != null && container.childCount > 0)
            {
                lastPoint = container.GetChild(container.childCount - 1).position;
                hasLastPoint = true;
            }

            // Vẽ tâm ngắm Reticle tại vị trí chuột
            Color themeColor = GetColorForTarget(currentTarget);
            Handles.color = themeColor;
            Handles.DrawWireDisc(hitPoint, Vector3.up, 0.4f);
            Handles.DrawLine(hitPoint - Vector3.right * 0.3f, hitPoint + Vector3.right * 0.3f);
            Handles.DrawLine(hitPoint - Vector3.forward * 0.3f, hitPoint + Vector3.forward * 0.3f);

            // Vẽ đường nối nét đứt từ điểm trước đó tới con trỏ chuột
            if (hasLastPoint)
            {
                Handles.color = new Color(themeColor.r, themeColor.g, themeColor.b, 0.6f);
                Handles.DrawDottedLine(lastPoint, hitPoint, 4.0f);
                float dist = Vector3.Distance(lastPoint, hitPoint);
                Handles.Label(hitPoint + Vector3.up * 0.4f, $"+ {dist:F1}m", EditorStyles.whiteBoldLabel);
            }

            // Vẽ bảng hướng dẫn nhanh góc trên Scene View
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(15, 15, 340, 95), EditorStyles.helpBox);
            GUIStyle statusStyle = new GUIStyle(EditorStyles.boldLabel);
            statusStyle.normal.textColor = themeColor;
            GUILayout.Label($" ĐANG BẬT BÚT VẼ: {GetTargetName(currentTarget)}", statusStyle);
            GUILayout.Label("• Click Chuột Trái: Đặt điểm tiếp theo", EditorStyles.miniLabel);
            GUILayout.Label("• Ctrl + Click: Xóa điểm gần nhất | Backspace: Hoàn tác", EditorStyles.miniLabel);
            GUILayout.Label("• Bấm phím ESC hoặc Enter để hoàn thành vẽ", EditorStyles.miniLabel);
            GUILayout.EndArea();
            Handles.EndGUI();

            // Xử lý Sự Kiện Chuột & Phím Tắt
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            if (e.type == EventType.MouseDown && e.button == 0 && !e.alt)
            {
                if (e.control)
                {
                    // Xóa điểm gần nhất
                    DeleteNearestPoint(hitPoint, container);
                }
                else
                {
                    // Thêm điểm mới
                    AddPoint(hitPoint, container);
                }
                e.Use();
            }
            else if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape || e.keyCode == KeyCode.Return)
                {
                    isPainting = false;
                    e.Use();
                    sceneView.Repaint();
                }
                else if (e.keyCode == KeyCode.Backspace)
                {
                    DeleteLastPoint(container);
                    e.Use();
                    sceneView.Repaint();
                }
            }

            sceneView.Repaint();
        }

        private static void AddPoint(Vector3 pos, Transform container)
        {
            if (container == null) return;

            int newIndex = container.childCount + 1;
            GameObject nodeGO = new GameObject($"WP_{newIndex:D2}");
            nodeGO.transform.SetParent(container, false);
            nodeGO.transform.position = pos;

            WaypointNode node = nodeGO.AddComponent<WaypointNode>();
            node.nodeColor = GetColorForTarget(currentTarget);
            node.sphereRadius = (currentTarget == PathTarget.DuongBe_ChayBoVenHo) ? 0.35f : 0.45f;

            Undo.RegisterCreatedObjectUndo(nodeGO, "Add Waypoint Node");
            EditorSceneManager.MarkSceneDirty(container.gameObject.scene);

            // Cập nhật đường nối ngay lập tức
            HoanKiemCrowdSystem sys = FindCrowdSystem();
            if (sys != null)
            {
                sys.UpdateActiveJoggerPaths();
            }
        }

        private static void DeleteNearestPoint(Vector3 pos, Transform container)
        {
            if (container == null || container.childCount == 0) return;

            Transform nearest = null;
            float minDist = float.MaxValue;

            for (int i = 0; i < container.childCount; i++)
            {
                Transform child = container.GetChild(i);
                float d = Vector3.Distance(pos, child.position);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = child;
                }
            }

            if (nearest != null && minDist < 3.5f)
            {
                Undo.DestroyObjectImmediate(nearest.gameObject);
                RenamePoints(container);
            }
        }

        private static void DeleteLastPoint(Transform container)
        {
            if (container != null && container.childCount > 0)
            {
                Transform last = container.GetChild(container.childCount - 1);
                Undo.DestroyObjectImmediate(last.gameObject);
            }
        }

        private static void RenamePoints(Transform container)
        {
            for (int i = 0; i < container.childCount; i++)
            {
                container.GetChild(i).name = $"WP_{i + 1:D2}";
            }
        }

        private void SnapCurrentPathToGround()
        {
            Transform container = GetContainerForTarget(currentTarget);
            if (container != null)
            {
                WaypointNode[] nodes = container.GetComponentsInChildren<WaypointNode>();
                Undo.RecordObjects(nodes, "Snap Ground");
                foreach (var n in nodes) n.SnapToGround();
            }
        }

        private void ReverseCurrentPath()
        {
            Transform container = GetContainerForTarget(currentTarget);
            if (container != null && container.childCount > 1)
            {
                List<Transform> list = new List<Transform>();
                for (int i = 0; i < container.childCount; i++) list.Add(container.GetChild(i));
                list.Reverse();

                for (int i = 0; i < list.Count; i++)
                {
                    list[i].SetSiblingIndex(i);
                }
                RenamePoints(container);
            }
        }

        private void ClearCurrentPath()
        {
            Transform container = GetContainerForTarget(currentTarget);
            if (container != null)
            {
                for (int i = container.childCount - 1; i >= 0; i--)
                {
                    Undo.DestroyObjectImmediate(container.GetChild(i).gameObject);
                }
            }
        }

        private static void EnsureContainerExists(PathTarget target)
        {
            HoanKiemCrowdSystem sys = FindCrowdSystem();
            if (sys == null) return;

            string containerName = GetContainerName(target);
            Transform t = sys.transform.Find(containerName);
            if (t == null)
            {
                GameObject go = new GameObject(containerName);
                go.transform.SetParent(sys.transform, false);
                t = go.transform;
            }

            if (target == PathTarget.DuongBe_ChayBoVenHo) sys.jogWaypointsContainer = t;
            else if (target == PathTarget.DuongTo_DinhTienHoang) sys.pedDinhTienHoangContainer = t;
            else if (target == PathTarget.DuongTo_LeThaiTo) sys.pedLeThaiToContainer = t;
        }

        private static Transform GetContainerForTarget(PathTarget target)
        {
            HoanKiemCrowdSystem sys = FindCrowdSystem();
            if (sys == null) return null;

            string containerName = GetContainerName(target);
            return sys.transform.Find(containerName);
        }

        private static string GetContainerName(PathTarget target)
        {
            switch (target)
            {
                case PathTarget.DuongBe_ChayBoVenHo: return "[Waypoints_JogCircuit]";
                case PathTarget.DuongTo_DinhTienHoang: return "[Waypoints_DinhTienHoang]";
                case PathTarget.DuongTo_LeThaiTo: return "[Waypoints_LeThaiTo]";
                case PathTarget.DuongTo_HangKhay: return "[Waypoints_HangKhay]";
                default: return "[Waypoints_Custom]";
            }
        }

        private static string GetTargetName(PathTarget target)
        {
            switch (target)
            {
                case PathTarget.DuongBe_ChayBoVenHo: return "Đường Bé Ven Hồ (Joggers / Đi dạo)";
                case PathTarget.DuongTo_DinhTienHoang: return "Đường To Đinh Tiên Hoàng (Phố đi bộ)";
                case PathTarget.DuongTo_LeThaiTo: return "Đường Phố Lê Thái Tổ";
                case PathTarget.DuongTo_HangKhay: return "Đường Phố Hàng Khay";
                default: return "Tùy Biến";
            }
        }

        private static Color GetColorForTarget(PathTarget target)
        {
            switch (target)
            {
                case PathTarget.DuongBe_ChayBoVenHo: return new Color(0.1f, 0.85f, 1.0f, 0.95f); // Cyan
                case PathTarget.DuongTo_DinhTienHoang: return new Color(0.2f, 0.95f, 0.35f, 0.95f); // Xanh lá
                case PathTarget.DuongTo_LeThaiTo: return new Color(0.95f, 0.85f, 0.2f, 0.95f); // Vàng
                case PathTarget.DuongTo_HangKhay: return new Color(1.0f, 0.5f, 0.2f, 0.95f); // Cam
                default: return Color.white;
            }
        }

        private static HoanKiemCrowdSystem FindCrowdSystem()
        {
            GameObject root = GameObject.Find("[HoanKiem_CrowdSystem]");
            return (root != null) ? root.GetComponent<HoanKiemCrowdSystem>() : null;
        }

        private static void FocusScene()
        {
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.Focus();
            }
        }
    }
}
#endif
