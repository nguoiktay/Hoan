#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HoanKiemAirVR.Editor
{
    [InitializeOnLoad]
    public static class HoanKiemWaterSetup
    {
        private const string WaterGameObjectName = "[HoanKiem_LakeWater]";
        private const string LakeMaterialPath = "Assets/Idyllic Fantasy Nature/Demo/Materials/Lake.mat";

        static HoanKiemWaterSetup()
        {
            EditorApplication.delayCall += AutoSetupWaterIfMissing;
        }

        public static void AutoSetupWaterIfMissing()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != "HoanKiem" && !activeScene.path.Contains("HoanKiem"))
                return;

            var existingWater = GameObject.Find(WaterGameObjectName);
            if (existingWater == null)
            {
                SetupLakeWater();
            }
        }

        [MenuItem("HoanKiem VR/Tạo Mặt Nước Hồ Gươm Xanh Ngọc Bích (Setup Lake Water)", false, 6)]
        public static void SetupLakeWater()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "HoanKiem" && !scene.path.Contains("HoanKiem"))
            {
                scene = EditorSceneManager.OpenScene("Assets/Scenes/HoanKiem.unity");
            }

            // 1. Kiểm tra hoặc tạo GameObject gốc chứa mặt nước
            GameObject rootWater = GameObject.Find(WaterGameObjectName);
            if (rootWater == null)
            {
                rootWater = new GameObject(WaterGameObjectName);
                Undo.RegisterCreatedObjectUndo(rootWater, "Create Hoan Kiem Water Surface");
            }

            // 2. Tải vật liệu Lake.mat
            Material lakeMat = AssetDatabase.LoadAssetAtPath<Material>(LakeMaterialPath);
            if (lakeMat == null)
            {
                Debug.LogWarning($"[HoanKiemWaterSetup] Không tìm thấy material tại: {LakeMaterialPath}");
                return;
            }

            // 3. Xóa các con cũ nếu có để tạo lại chuẩn xác
            while (rootWater.transform.childCount > 0)
            {
                Object.DestroyImmediate(rootWater.transform.GetChild(0).gameObject);
            }

            rootWater.transform.position = new Vector3(620.0f, -1.02f, -772.5f);
            rootWater.transform.rotation = Quaternion.identity; // Canh thẳng trục toạ độ để phủ trọn vẹn toàn bộ các khúc uốn của hồ

            // 4. Tạo mặt phẳng nước (Unity Plane: 10m x 10m ở scale 1)
            // Lòng Hồ Gươm rộng tối đa ~164m, dài tối đa ~353m
            // Đặt Scale X = 19.0 (190m) và Scale Z = 39.0 (390m) để phủ kín 100% mọi khúc cua, bờ vịnh
            GameObject waterPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            waterPlane.name = "WaterSurface_Mesh";
            waterPlane.transform.SetParent(rootWater.transform, false);
            waterPlane.transform.localPosition = Vector3.zero;
            waterPlane.transform.localRotation = Quaternion.identity;
            waterPlane.transform.localScale = new Vector3(19.0f, 1.0f, 39.0f);

            // Bỏ collider để không cản trở di chuyển hay tương tác
            Collider col = waterPlane.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            // Gán vật liệu nước
            MeshRenderer mr = waterPlane.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sharedMaterial = lakeMat;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = true;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("<color=#00FF88><b>[HoanKiemWaterSetup] ĐÃ TẠO MẶT NƯỚC HỒ GƯƠM MÀU XANH LỤC THÀNH CÔNG!</b></color>");
        }

        [MenuItem("HoanKiem VR/Xóa Mặt Nước Hồ Gươm (Remove Lake Water)", false, 7)]
        public static void RemoveLakeWater()
        {
            var existing = GameObject.Find(WaterGameObjectName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
                var scene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[HoanKiemWaterSetup] Đã xóa mặt nước Hồ Gươm.");
            }
        }
    }
}
#endif
