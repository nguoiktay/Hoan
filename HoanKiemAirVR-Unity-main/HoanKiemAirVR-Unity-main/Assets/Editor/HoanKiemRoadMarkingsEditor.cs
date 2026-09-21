#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace HoanKiemAirVR.Editor
{
    [CustomEditor(typeof(HoanKiemRoadMarkings))]
    public class HoanKiemRoadMarkingsEditor : UnityEditor.Editor
    {
        private HoanKiemRoadMarkings markings;

        private void OnEnable()
        {
            markings = (HoanKiemRoadMarkings)target;
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
            titleStyle.normal.textColor = new Color(0.95f, 0.85f, 0.1f);
            EditorGUILayout.LabelField("HỆ THỐNG VẠCH KẺ ĐƯỜNG THỰC TẾ HỒ GƯƠM", titleStyle);

            EditorGUILayout.LabelField(
                "Tạo tự động các vạch sang đường (Zebra Crossing) tại 8 nút giao trọng điểm, " +
                "vạch nét đứt phân làn, vạch đôi vàng tim đường, vạch dừng xe & trạm xe buýt chuẩn QCVN 41:2019.",
                EditorStyles.wordWrappedMiniLabel
            );
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("1. Độ Cao & Bề Mặt", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("surfaceY"), new GUIContent("Độ Cao Mặt Đường Y"));

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("2. Bật / Tắt Các Loại Vạch Kẻ", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showZebraCrossings"), new GUIContent("Vạch Đi Bộ Qua Đường (Zebra)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showStopLines"), new GUIContent("Vạch Dừng Xe (Stop Lines)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showLaneDividers"), new GUIContent("Vạch Đứt Phân Làn (Lane Lines)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showCenterlines"), new GUIContent("Vạch Đôi Vàng Tim Đường"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showDirectionArrows"), new GUIContent("Mũi Tên Chỉ Hướng"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("showBusStops"), new GUIContent("Trạm Dừng Xe Buýt"));

            EditorGUILayout.Space(10);

            // Nút Tạo / Làm mới
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f);
            if (GUILayout.Button("TẠO / LÀM MỚI TOÀN BỘ VẠCH KẺ ĐƯỜNG", GUILayout.Height(35)))
            {
                Undo.RecordObject(markings, "Generate Road Markings");
                markings.GenerateAllMarkings();
                EditorUtility.SetDirty(markings);
            }

            GUI.backgroundColor = new Color(1.0f, 0.5f, 0.5f);
            if (GUILayout.Button("Xóa toàn bộ vạch kẻ đường trong Scene"))
            {
                if (EditorUtility.DisplayDialog("Xóa vạch kẻ đường", "Bạn có chắc muốn xóa tất cả các vạch kẻ đường?", "Đồng ý", "Hủy"))
                {
                    Undo.RecordObject(markings, "Clear Road Markings");
                    while (markings.transform.childCount > 0)
                    {
                        DestroyImmediate(markings.transform.GetChild(0).gameObject);
                    }
                    EditorUtility.SetDirty(markings);
                }
            }
            GUI.backgroundColor = Color.white;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
