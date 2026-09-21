#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HoanKiemAirVR.Editor
{
    [CustomEditor(typeof(HoanKiemTrafficSystem))]
    public class HoanKiemTrafficEditor : UnityEditor.Editor
    {
        private HoanKiemTrafficSystem traffic;

        private void OnEnable()
        {
            traffic = (HoanKiemTrafficSystem)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header Banner
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.normal.background = Texture2D.linearGrayTexture;
            boxStyle.padding = new RectOffset(10, 10, 10, 10);

            EditorGUILayout.BeginVertical(boxStyle);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 13;
            titleStyle.normal.textColor = new Color(0.2f, 0.8f, 1.0f);
            EditorGUILayout.LabelField("HỆ THỐNG XE CỘ & GIAO THÔNG ĐẶC TRƯNG HÀ NỘI", titleStyle);

            EditorGUILayout.LabelField(
                "Tái hiện đầy đủ: Xe buýt 2 tầng mui trần City Tour đỏ, Taxi Mai Linh / Xanh SM / Group, " +
                "ô tô con sedan & SUV, các hàng xe máy dựng nghiêng vỉa hè phố cổ, xích lô du lịch và xe điện Bờ Hồ.",
                EditorStyles.wordWrappedMiniLabel
            );
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("1. Chế Độ Vận Hành Giao Thông", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableMovingTraffic"), new GUIContent("Bật Xe Di Chuyển Khi Play (Luồng Xe)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("trafficSpeed"), new GUIContent("Tốc Độ Tuần Hành (m/s)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("movingVehicleCount"), new GUIContent("Số Lượng Xe Tuần Hành"));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("2. Bố Trí Xe Cộ Trong Phố", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableParkedVehicles"), new GUIContent("Bật Xe Đỗ Tĩnh"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnDoubleDeckerTourBus"), new GUIContent("Xe Buýt 2 Tầng City Tour"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnParkedTaxis"), new GUIContent("Taxi Mai Linh / Xanh SM"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnParkedMotorbikeRows"), new GUIContent("Hàng Xe Máy Vỉa Hè"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnParkedCyclos"), new GUIContent("Xe Xích Lô Du Lịch"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnElectricTourTrams"), new GUIContent("Xe Điện Ngắm Cảnh Bờ Hồ"));

            EditorGUILayout.Space(10);

            // Nút Tạo toàn bộ xe trong Scene
            GUI.backgroundColor = new Color(0.2f, 0.85f, 0.5f);
            if (GUILayout.Button("TẠO TOÀN BỘ XE CỘ TRONG SCENE", GUILayout.Height(36)))
            {
                Undo.RecordObject(traffic, "Spawn All Vehicles");
                traffic.SpawnAllVehiclesInEditor();
                EditorUtility.SetDirty(traffic);
            }

            GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f);
            if (GUILayout.Button("Xóa toàn bộ xe cộ trong Scene"))
            {
                if (EditorUtility.DisplayDialog("Xóa xe cộ", "Bạn có chắc muốn xóa tất cả xe cộ?", "Đồng ý", "Hủy"))
                {
                    Undo.RecordObject(traffic, "Clear Vehicles");
                    while (traffic.transform.childCount > 0)
                    {
                        DestroyImmediate(traffic.transform.GetChild(0).gameObject);
                    }
                    EditorUtility.SetDirty(traffic);
                }
            }
            GUI.backgroundColor = Color.white;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
