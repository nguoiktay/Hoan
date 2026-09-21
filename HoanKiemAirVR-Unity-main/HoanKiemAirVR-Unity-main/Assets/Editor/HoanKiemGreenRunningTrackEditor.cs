#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HoanKiemAirVR.Editor
{
    [CustomEditor(typeof(HoanKiemGreenRunningTrack))]
    public class HoanKiemGreenRunningTrackEditor : UnityEditor.Editor
    {
        private HoanKiemGreenRunningTrack track;
        private int selectedIndex = -1;
        private bool showWaypointsList = false;

        private void OnEnable()
        {
            track = (HoanKiemGreenRunningTrack)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header Banner
            DrawHeaderBanner();

            EditorGUILayout.Space(8);

            // Nhóm Thông số Hình Học
            EditorGUILayout.LabelField("1. Cấu hình Kích Thước & Hình Học", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("trackWidth"), new GUIContent("Bề Rộng Đường Chạy (m)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("borderWidth"), new GUIContent("Bề Rộng Viền Trắng (m)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("heightOffset"), new GUIContent("Độ Nhô Cao Khỏi Đường (m)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("curveSubdivisions"), new GUIContent("Độ Mịn Đường Cong (Subdivisions)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isLoop"), new GUIContent("Vòng Lặp Khép Kín"));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("2. Màu Sắc & Vật Liệu (URP)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trackColor"), new GUIContent("Màu Mặt Đường Chạy"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("borderColor"), new GUIContent("Màu Vạch Viền Trắng"));

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                track.RebuildMesh();
            }

            EditorGUILayout.Space(10);

            // Các nút thao tác nhanh
            EditorGUILayout.LabelField("3. Thao Tác Chỉnh Sửa & Tự Động Hóa", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.1f, 0.85f, 0.45f);
            if (GUILayout.Button("TÁI TẠO LƯỚI 3D (REBUILD MESH)", GUILayout.Height(32)))
            {
                Undo.RecordObject(track, "Rebuild Running Track Mesh");
                track.RebuildMesh();
                EditorUtility.SetDirty(track);
            }

            GUI.backgroundColor = new Color(0.3f, 0.7f, 1.0f);
            if (GUILayout.Button("BẮT DÍNH MẶT ĐẤT (SNAP GROUND)", GUILayout.Height(32)))
            {
                Undo.RecordObject(track, "Snap Running Track to Ground");
                track.SnapWaypointsToGround();
                EditorUtility.SetDirty(track);
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Đồng bộ người chạy bộ AI (Sync Joggers)"))
            {
                SyncJoggersToTrack();
            }

            GUI.backgroundColor = new Color(1.0f, 0.6f, 0.6f);
            if (GUILayout.Button("Khôi phục mặc định ban đầu"))
            {
                if (EditorUtility.DisplayDialog("Khôi phục đường chạy", "Bạn có chắc muốn đặt lại toàn bộ tọa độ các điểm mốc về chuẩn quanh Hồ Gươm?", "Đồng ý", "Hủy"))
                {
                    Undo.RecordObject(track, "Reset Track Waypoints");
                    track.ResetToDefaultWaypoints();
                    EditorUtility.SetDirty(track);
                }
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // Danh sách Waypoints
            DrawWaypointsSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeaderBanner()
        {
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = Texture2D.linearGrayTexture;
            boxStyle.padding = new RectOffset(10, 10, 10, 10);

            EditorGUILayout.BeginVertical(boxStyle);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 13;
            titleStyle.normal.textColor = new Color(0.1f, 0.9f, 0.5f);
            EditorGUILayout.LabelField("ĐƯỜNG CHẠY XANH LÁ CÂY HỒ GƯƠM (CHỈNH SỬA TRỰC QUAN)", titleStyle);

            EditorGUILayout.LabelField(
                "- Click chuột vào bất kỳ điểm mốc nào trên Scene View để hiện trục tọa độ 3D kéo nắn.\n" +
                "- Giữ SHIFT + Click chuột trái lên mặt đất trong Scene View để chèn thêm điểm mới!\n" +
                "- Lưới 3D tự động tái tạo tức thì khi bạn di chuyển điểm.",
                EditorStyles.wordWrappedMiniLabel
            );
            EditorGUILayout.EndVertical();
        }

        private void DrawWaypointsSection()
        {
            showWaypointsList = EditorGUILayout.Foldout(showWaypointsList, $"Danh sách các điểm mốc ({track.waypoints.Count} điểm)", true);
            if (!showWaypointsList) return;

            EditorGUI.indentLevel++;
            for (int i = 0; i < track.waypoints.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                bool isSelected = (selectedIndex == i);
                if (isSelected) GUI.color = Color.cyan;

                EditorGUILayout.LabelField($"P{i:D2}", GUILayout.Width(35));
                Vector3 newPos = EditorGUILayout.Vector3Field("", track.waypoints[i]);
                if (newPos != track.waypoints[i])
                {
                    Undo.RecordObject(track, $"Move Waypoint {i}");
                    track.waypoints[i] = newPos;
                    track.RebuildMesh();
                    EditorUtility.SetDirty(track);
                }

                GUI.color = Color.white;

                if (GUILayout.Button("Chọn", GUILayout.Width(45)))
                {
                    selectedIndex = i;
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("+", GUILayout.Width(25)))
                {
                    Undo.RecordObject(track, $"Insert Waypoint after {i}");
                    Vector3 insertPos = (i < track.waypoints.Count - 1)
                        ? (track.waypoints[i] + track.waypoints[i + 1]) * 0.5f
                        : track.waypoints[i] + Vector3.forward * 5f;
                    track.waypoints.Insert(i + 1, insertPos);
                    track.RebuildMesh();
                    EditorUtility.SetDirty(track);
                    break;
                }

                if (track.waypoints.Count > 3 && GUILayout.Button("X", GUILayout.Width(25)))
                {
                    Undo.RecordObject(track, $"Remove Waypoint {i}");
                    track.waypoints.RemoveAt(i);
                    track.RebuildMesh();
                    EditorUtility.SetDirty(track);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Thêm điểm mốc mới ở cuối", GUILayout.Height(26)))
            {
                Undo.RecordObject(track, "Add Waypoint");
                Vector3 last = track.waypoints[track.waypoints.Count - 1];
                track.waypoints.Add(last + Vector3.forward * 5f);
                track.RebuildMesh();
                EditorUtility.SetDirty(track);
            }

            EditorGUI.indentLevel--;
        }

        private void OnSceneGUI()
        {
            if (track == null || track.waypoints == null || track.waypoints.Count == 0) return;

            Event e = Event.current;

            // Xử lý Shift + Click để thêm điểm mới
            if (e.shift && e.type == EventType.MouseDown && e.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 500f))
                {
                    Undo.RecordObject(track, "Shift-Click Add Waypoint");
                    track.waypoints.Add(hit.point);
                    selectedIndex = track.waypoints.Count - 1;
                    track.RebuildMesh();
                    EditorUtility.SetDirty(track);
                    e.Use();
                    return;
                }
            }

            // Vẽ đường nối và các handle điều khiển
            Handles.color = new Color(0.1f, 0.85f, 0.45f, 0.8f);

            for (int i = 0; i < track.waypoints.Count; i++)
            {
                Vector3 pt = track.waypoints[i] + Vector3.up * (track.heightOffset + 0.05f);
                float handleSize = HandleUtility.GetHandleSize(pt) * 0.15f;

                // Nút bấm tròn trên Scene
                if (Handles.Button(pt, Quaternion.identity, handleSize, handleSize * 1.2f, Handles.SphereHandleCap))
                {
                    selectedIndex = i;
                    Repaint();
                }

                // Nhãn số thứ tự
                GUIStyle labelStyle = new GUIStyle();
                labelStyle.normal.textColor = (selectedIndex == i) ? Color.yellow : Color.white;
                labelStyle.fontSize = 11;
                labelStyle.fontStyle = FontStyle.Bold;
                Handles.Label(pt + Vector3.up * (handleSize * 1.5f), $"P{i}", labelStyle);

                // Nếu đang chọn điểm này -> Hiện trục tọa độ kéo di chuyển (Position Handle)
                if (selectedIndex == i)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 movedPt = Handles.PositionHandle(track.waypoints[i], Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(track, $"Drag Waypoint {i}");
                        track.waypoints[i] = movedPt;
                        track.RebuildMesh();
                        EditorUtility.SetDirty(track);
                    }
                }
            }
        }

        private void SyncJoggersToTrack()
        {
            var crowd = GameObject.FindObjectOfType<HoanKiemCrowdSystem>();
            if (crowd == null)
            {
                EditorUtility.DisplayDialog("Thông báo", "Không tìm thấy HoanKiemCrowdSystem trong Scene hiện tại!", "OK");
                return;
            }

            Debug.Log("<color=#00FFAA>[HoanKiemGreenRunningTrack] Đã đồng bộ thành công tọa độ đường chạy xanh với hệ thống người chạy bộ Bờ Hồ!</color>");
            EditorUtility.DisplayDialog("Thành công", $"Đã đồng bộ {track.waypoints.Count} điểm mốc của đường chạy xanh với HoanKiemCrowdSystem!", "OK");
        }
    }
}
#endif
