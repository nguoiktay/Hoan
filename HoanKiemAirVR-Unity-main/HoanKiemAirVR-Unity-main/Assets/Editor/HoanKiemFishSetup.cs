#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HoanKiemAirVR.Editor
{
    [InitializeOnLoad]
    public static class HoanKiemFishSetup
    {
        private static readonly string[] PrefabPaths = new string[]
        {
            "Assets/Floreswa/Prefabs/fish01.prefab",
            "Assets/Floreswa/Prefabs/fish01_shade.prefab",
            "Assets/Floreswa/Prefabs/fish02.prefab",
            "Assets/Floreswa/Prefabs/fish02_shade.prefab",
            "Assets/Floreswa/Prefabs/fish03.prefab",
            "Assets/Floreswa/Prefabs/fish03_shade.prefab"
        };

        static HoanKiemFishSetup()
        {
            EditorApplication.delayCall += AutoSetupFishIfPresent;
        }

        public static void AutoSetupFishIfPresent()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != "HoanKiem" && !activeScene.path.Contains("HoanKiem"))
            {
                return;
            }

            var existing = GameObject.Find("[HoanKiem_FishSystem]");
            if (existing == null)
            {
                SetupFishInScene();
            }
        }

        [MenuItem("HoanKiem VR/Thả Đàn Cá Vào Hồ Gươm (Spawn Fish)", false, 5)]
        public static void SetupFishInScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "HoanKiem" && !scene.path.Contains("HoanKiem"))
            {
                scene = EditorSceneManager.OpenScene("Assets/Scenes/HoanKiem.unity");
            }

            // 1. Tải danh sách Prefab cá
            List<GameObject> loadedPrefabs = new List<GameObject>();
            foreach (var path in PrefabPaths)
            {
                var p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (p != null) loadedPrefabs.Add(p);
            }

            if (loadedPrefabs.Count == 0)
            {
                Debug.LogError("[HoanKiemFishSetup] Không tìm thấy Prefab cá nào trong Assets/Floreswa/Prefabs!");
                return;
            }

            // 2. Tìm hoặc tạo GameObject [HoanKiem_FishSystem]
            GameObject sysObj = GameObject.Find("[HoanKiem_FishSystem]");
            if (sysObj == null)
            {
                sysObj = new GameObject("[HoanKiem_FishSystem]");
                Undo.RegisterCreatedObjectUndo(sysObj, "Create Fish System");
            }

            sysObj.transform.position = Vector3.zero;
            sysObj.transform.rotation = Quaternion.identity;

            // Đặt cạnh [HoanKiem_CrowdSystem] trong Hierarchy cho ngăn nắp
            var crowdSys = GameObject.Find("[HoanKiem_CrowdSystem]");
            if (crowdSys != null)
            {
                sysObj.transform.SetSiblingIndex(crowdSys.transform.GetSiblingIndex() + 1);
            }

            // 3. Cấu hình component HoanKiemFishSystem
            var fishSystem = sysObj.GetComponent<HoanKiemFishSystem>();
            if (fishSystem == null) fishSystem = sysObj.AddComponent<HoanKiemFishSystem>();

            fishSystem.fishPrefabs = loadedPrefabs.ToArray();
            fishSystem.totalFishCount = 130; // Thả 130 chú cá bơi lội khắp hồ
            fishSystem.spawnOnStart = true;
            fishSystem.InitDefaultZones();

            // 4. Thả cá ngay vào scene
            fishSystem.SpawnAllFish();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("<color=#00FFAA>[HoanKiemFishSetup] ĐÃ THẢ 130 CHÚ CÁ (Đỏ, Vàng, Lam) BƠI LỘI TUNG TĂNG KHẮP HỒ GƯƠM!</color>");
            Debug.Log("<color=#00E5FF>✦ 35 cá tại Cầu Thê Húc & Đền Ngọc Sơn\n✦ 30 cá quanh Tháp Rùa\n✦ 25 cá bờ Đông (Đinh Tiên Hoàng - VNPT)\n✦ 20 cá bờ Tây (Lê Thái Tổ - Thuỷ Tạ)\n✦ 20 cá bơi tự do lòng hồ</color>");
        }

        [MenuItem("HoanKiem VR/Xóa Đàn Cá Trong Hồ (Clear Fish)", false, 6)]
        public static void ClearFishInScene()
        {
            var sysObj = GameObject.Find("[HoanKiem_FishSystem]");
            if (sysObj != null)
            {
                var fishSys = sysObj.GetComponent<HoanKiemFishSystem>();
                if (fishSys != null) fishSys.ClearFish();
                Object.DestroyImmediate(sysObj);
                var scene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("<color=#FFAA00>[HoanKiemFishSetup] Đã xóa toàn bộ cá khỏi hồ.</color>");
            }
        }
    }
}
#endif
