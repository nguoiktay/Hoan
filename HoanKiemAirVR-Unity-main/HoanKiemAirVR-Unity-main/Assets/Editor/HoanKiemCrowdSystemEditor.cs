#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HoanKiemAirVR.Editor
{
    [CustomEditor(typeof(HoanKiemCrowdSystem))]
    public class HoanKiemCrowdSystemEditor : UnityEditor.Editor
    {
        private HoanKiemCrowdSystem crowdSystem;

        private void OnEnable()
        {
            crowdSystem = (HoanKiemCrowdSystem)target;
            crowdSystem.FindContainersIfNull();
        }

        public override void OnInspectorGUI()
        {
            // Vẽ các thuộc tính mặc định
            DrawDefaultInspector();

            EditorGUILayout.Space(12);

            // Khung công cụ chỉnh sửa Waypoints trực quan
            DrawWaypointTools();

            EditorGUILayout.Space(10);

            // Nút Thao Tác Nhanh
            DrawActionButtons();
        }

        private void DrawWaypointTools()
        {
            GUIStyle sectionBox = new GUIStyle(EditorStyles.helpBox);
            sectionBox.padding = new RectOffset(12, 12, 10, 10);

            EditorGUILayout.BeginVertical(sectionBox);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 13;
            titleStyle.normal.textColor = new Color(0.1f, 0.85f, 1.0f);
            EditorGUILayout.LabelField("📍 CHỈNH SỬA ĐƯỜNG CHẠY TRỰC QUAN (SCENE WAYPOINTS)", titleStyle);

            EditorGUILayout.LabelField(
                "Bạn có thể tạo các điểm Waypoint thành các GameObject con trong Scene. " +
                "Sau đó click chọn từng điểm trực tiếp trên màn hình Scene View và dùng phím W kéo thả đến đúng vị trí mong muốn!",
                EditorStyles.wordWrappedMiniLabel
            );

            EditorGUILayout.Space(6);

            crowdSystem.FindContainersIfNull();
            bool hasContainer = (crowdSystem.jogWaypointsContainer != null && crowdSystem.jogWaypointsContainer.childCount > 0);

            if (hasContainer)
            {
                int count = crowdSystem.jogWaypointsContainer.childCount;
                EditorGUILayout.HelpBox(
                    $"✔ ĐANG CÓ {count} ĐIỂM WAYPOINT TRONG SCENE!\n" +
                    $"• Mở mục '[Waypoints_JogCircuit]' trong Hierarchy hoặc click trực tiếp điểm màu xanh Cyan trong Scene View.\n" +
                    $"• Dùng công cụ Move (W) kéo thả điểm đến đúng mặt đường hoặc vỉa hè.\n" +
                    $"• Đường vẽ màu Cyan sẽ tự động co giãn nối liền các điểm ngay lập tức!",
                    MessageType.Info
                );

                EditorGUILayout.Space(4);

                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.3f, 0.85f, 0.5f);
                if (GUILayout.Button("Bắt dính tất cả điểm xuống mặt đường (Snap)", GUILayout.Height(30)))
                {
                    Undo.RecordObjects(crowdSystem.jogWaypointsContainer.GetComponentsInChildren<Transform>(), "Snap Waypoints");
                    crowdSystem.SnapAllWaypointsToGround();
                }

                GUI.backgroundColor = new Color(0.2f, 0.75f, 1.0f);
                if (GUILayout.Button("Cập nhật đường chạy ngay (Live Update)", GUILayout.Height(30)))
                {
                    crowdSystem.UpdateActiveJoggerPaths();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(4);

                GUI.backgroundColor = new Color(0.1f, 0.85f, 1.0f);
                if (GUILayout.Button(" NẮN CHỈNH ĐƯỜNG CHẠY CHUẨN THỰC TẾ (TRÁNH CÂY & MẶT NƯỚC)", GUILayout.Height(34)))
                {
                    Undo.RegisterFullObjectHierarchyUndo(crowdSystem.gameObject, "Apply Calibrated Jog Path");
                    crowdSystem.ApplyCalibratedJogCircuit();
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(crowdSystem.gameObject.scene);
                }
                GUI.backgroundColor = Color.white;

                EditorGUILayout.Space(2);

                GUI.backgroundColor = new Color(0.9f, 0.5f, 0.3f);
                if (GUILayout.Button("Tạo lại bộ điểm gốc (Reset về mặc định)", GUILayout.Height(24)))
                {
                    if (EditorUtility.DisplayDialog("Xác nhận", "Bạn có muốn xóa các điểm hiện tại và tạo lại danh sách điểm mặc định không?", "Đồng ý", "Hủy"))
                    {
                        Undo.RegisterFullObjectHierarchyUndo(crowdSystem.gameObject, "Reset Waypoints");
                        crowdSystem.CreateJogWaypointsInScene();
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Hiện tại đường chạy đang dùng mảng tọa độ cố định trong mã nguồn. " +
                    "Bấm nút dưới đây để chuyển toàn bộ điểm thành các GameObject trong Scene và tự do kéo thả bằng chuột!",
                    MessageType.Warning
                );

                EditorGUILayout.Space(4);

                GUI.backgroundColor = new Color(0.1f, 0.85f, 0.4f);
                if (GUILayout.Button(" TẠO BỘ ĐIỂM WAYPOINTS TRONG SCENE ĐỂ KÉO THẢ", GUILayout.Height(38)))
                {
                    Undo.RegisterFullObjectHierarchyUndo(crowdSystem.gameObject, "Create Scene Waypoints");
                    crowdSystem.CreateJogWaypointsInScene();
                }
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.Space(6);
            GUI.backgroundColor = new Color(0.1f, 0.9f, 0.5f);
            if (GUILayout.Button(" MỞ BÚT VẼ / CHẤM ĐƯỜNG BẰNG CHUỘT (CLICK-TO-DRAW)", GUILayout.Height(36)))
            {
                WaypointPathPainter.ShowWindow();
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        private void DrawActionButtons()
        {
            GUI.backgroundColor = new Color(0.2f, 0.85f, 0.4f);
            if (GUILayout.Button(" BAKE / LÀM MỚI TOÀN BỘ ĐÁM ĐÔNG (1-Click)", GUILayout.Height(36)))
            {
                crowdSystem.SpawnCrowd();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(crowdSystem.gameObject.scene);
            }
            GUI.backgroundColor = Color.white;
        }

        private void OnSceneGUI()
        {
            if (crowdSystem == null) return;
            crowdSystem.FindContainersIfNull();

            if (crowdSystem.jogWaypointsContainer == null) return;

            // Hiển thị nhãn số thứ tự trên từng điểm Waypoint trong Scene View
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.cyan;
            labelStyle.fontSize = 11;
            labelStyle.fontStyle = FontStyle.Bold;

            Camera cam = SceneView.currentDrawingSceneView != null ? SceneView.currentDrawingSceneView.camera : Camera.current;
            Vector3 camPos = (cam != null) ? cam.transform.position : Vector3.zero;

            for (int i = 0; i < crowdSystem.jogWaypointsContainer.childCount; i++)
            {
                Transform child = crowdSystem.jogWaypointsContainer.GetChild(i);
                if (!child.gameObject.activeSelf) continue;

                Vector3 pos = child.position;
                float dist = (cam != null) ? Vector3.Distance(camPos, pos) : 0f;

                // Chỉ vẽ nhãn và đĩa khi camera ở gần (dưới 35m) để màn hình thoáng sạch
                if (cam == null || dist < 35.0f)
                {
                    Handles.color = new Color(0.1f, 0.85f, 1.0f, 0.7f);
                    Handles.DrawWireDisc(pos, Vector3.up, 0.3f);
                    Handles.Label(pos + Vector3.up * 0.55f, $"P{i + 1}", labelStyle);
                }
            }
        }
    }
}
#endif
