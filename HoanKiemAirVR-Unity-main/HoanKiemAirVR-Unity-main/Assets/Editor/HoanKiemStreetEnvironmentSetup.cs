#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HoanKiemAirVR.Editor
{
    /// <summary>
    /// Master Editor Window & Menu Commands:
    /// Nâng cấp toàn diện khung cảnh phố xá bao quanh Hồ Gươm
    /// </summary>
    [InitializeOnLoad]
    public static class HoanKiemStreetEnvironmentSetup
    {
        private const string RootRunningTrackName = "[HoanKiem_GreenRunningTrack]";
        private const string RootRoadMarkingsName = "[HoanKiem_RoadMarkings]";
        private const string RootTrafficSystemName = "[HoanKiem_TrafficSystem]";
        private const string RootBuildingsName = "[HoanKiem_StreetBuildings]";
        private const string CurrentCalibrationVersion = "Calibrated_V2_RealisticRoads";

        static HoanKiemStreetEnvironmentSetup()
        {
            EditorApplication.delayCall += AutoSetupIfMissing;
        }

        public static void AutoSetupIfMissing()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name != "HoanKiem" && !activeScene.path.Contains("HoanKiem"))
                return;

            bool missingTrack = GameObject.Find(RootRunningTrackName) == null;
            bool missingMarkings = GameObject.Find(RootRoadMarkingsName) == null;
            bool missingTraffic = GameObject.Find(RootTrafficSystemName) == null;
            bool missingBuildings = GameObject.Find(RootBuildingsName) == null;
            bool outdatedCalibration = GameObject.Find(CurrentCalibrationVersion) == null;

            if (missingTrack || missingMarkings || missingTraffic || missingBuildings || outdatedCalibration)
            {
                UpgradeAllScene();
                Debug.Log("<color=#00FFAA>[HoanKiemStreetEnvironmentSetup] Đã tự động cập nhật khung cảnh phố Hồ Gươm theo tọa độ thực tế chuẩn xác (V2)!</color>");
            }
        }

        [MenuItem("HoanKiem VR/1. NÂNG CẤP TOÀN DIỆN PHỐ HỒ GƯƠM (ALL-IN-ONE UPGRADE)", false, 1)]
        public static void UpgradeAllScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "HoanKiem" && !scene.path.Contains("HoanKiem"))
            {
                scene = EditorSceneManager.OpenScene("Assets/Scenes/HoanKiem.unity");
            }

            SetupGreenRunningTrack();
            SetupRoadMarkings();
            SetupTrafficSystem();
            SetupStreetBuildings();

            // Đánh dấu phiên bản tọa độ chuẩn V2
            var existingMarker = GameObject.Find(CurrentCalibrationVersion);
            if (existingMarker == null)
            {
                var marker = new GameObject(CurrentCalibrationVersion);
                var buildingsRoot = GameObject.Find(RootBuildingsName);
                if (buildingsRoot != null)
                {
                    marker.transform.SetParent(buildingsRoot.transform, false);
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("<color=#00FF88><b>========================================================================</b></color>");
            Debug.Log("<color=#00FF88><b>[HoanKiem VR] ĐÃ NÂNG CẤP TOÀN DIỆN KHUNG CẢNH PHỐ HỒ GƯƠM THÀNH CÔNG!</b></color>");
            Debug.Log("<color=#00FFFF>1. Đường chạy xanh lá cây thể thao (Green Track) bên đường ngoài có thể chỉnh sửa trực tiếp trên Unity.</color>");
            Debug.Log("<color=#00FFFF>2. Vạch kẻ đường chuẩn QCVN 41:2019 (vạch sang đường cho người đi bộ, vạch đứt phân làn, vạch đôi vàng tim đường, trạm xe buýt).</color>");
            Debug.Log("<color=#00FFFF>3. Xe cộ đặc trưng: Xe buýt 2 tầng đỏ City Tour, Taxi Mai Linh / Xanh SM, ô tô con, hàng xe máy dựng vỉa hè phố cổ, xích lô, xe điện.</color>");
            Debug.Log("<color=#00FFFF>4. Nhà cửa 4 mặt hồ: Tòa nhà Hàm Cá Mập, Tòa soạn Báo Hà Nội Mới, Apricot Hotel, dãy phố cổ Hàng Khay & Đinh Tiên Hoàng.</color>");
            Debug.Log("<color=#00FF88><b>========================================================================</b></color>");

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog(
                    "Nâng Cấp Hoàn Tất",
                    "Đã nâng cấp thành công toàn diện khung cảnh phố quanh Hồ Gươm:\n\n" +
                    "1. Đường chạy xanh lá cây (chỉnh sửa trực tiếp trên Scene View bằng 3D Handles)\n" +
                    "2. Hệ thống vạch kẻ đường chuẩn thực tế (Zebra crossings tại các nút giao)\n" +
                    "3. Xe cộ phong phú (Xe buýt 2 tầng, Taxi, hàng xe máy vỉa hè, xích lô)\n" +
                    "4. Nhà cửa 4 mặt hồ (Hàm Cá Mập, Báo Hà Nội Mới, Apricot Hotel, Hàng Khay, Đinh Tiên Hoàng)",
                    "Tuyệt vời!"
                );
            }
            else
            {
                EditorApplication.Exit(0);
            }
        }

        [MenuItem("HoanKiem VR/2. Thiết Lập Đường Chạy Xanh Lá Cây (Setup Green Running Track)", false, 2)]
        public static void SetupGreenRunningTrack()
        {
            var existing = GameObject.Find(RootRunningTrackName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            GameObject trackObj = new GameObject(RootRunningTrackName);
            Undo.RegisterCreatedObjectUndo(trackObj, "Create Green Running Track");

            var trackComp = trackObj.AddComponent<HoanKiemGreenRunningTrack>();
            trackComp.ResetToDefaultWaypoints();
            trackComp.RebuildMesh();

            Selection.activeGameObject = trackObj;
            Debug.Log("<color=#00FFAA>[HoanKiemStreetEnvironmentSetup] Đã thiết lập thành công Đường Chạy Xanh Lá Cây!</color>");
        }

        [MenuItem("HoanKiem VR/3. Thiết Lập Vạch Kẻ Đường Thực Tế (Setup Road Markings)", false, 3)]
        public static void SetupRoadMarkings()
        {
            var existing = GameObject.Find(RootRoadMarkingsName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            GameObject markObj = new GameObject(RootRoadMarkingsName);
            Undo.RegisterCreatedObjectUndo(markObj, "Create Road Markings");

            var markComp = markObj.AddComponent<HoanKiemRoadMarkings>();
            markComp.GenerateAllMarkings();

            Debug.Log("<color=#00FFAA>[HoanKiemStreetEnvironmentSetup] Đã thiết lập thành công Vạch Kẻ Đường Thực Tế!</color>");
        }

        [MenuItem("HoanKiem VR/4. Thiết Lập Xe Cộ & Giao Thông (Setup Vehicles & Traffic)", false, 4)]
        public static void SetupTrafficSystem()
        {
            var existing = GameObject.Find(RootTrafficSystemName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            GameObject trafficObj = new GameObject(RootTrafficSystemName);
            Undo.RegisterCreatedObjectUndo(trafficObj, "Create Traffic System");

            var trafficComp = trafficObj.AddComponent<HoanKiemTrafficSystem>();
            trafficComp.SpawnAllVehiclesInEditor();

            Debug.Log("<color=#00FFAA>[HoanKiemStreetEnvironmentSetup] Đã thiết lập thành công Xe Cộ & Giao Thông Hà Nội!</color>");
        }

        [MenuItem("HoanKiem VR/5. Xây Dựng Nhà Cửa & Dãy Phố 4 Mặt Hồ (Setup Street Buildings)", false, 5)]
        public static void SetupStreetBuildings()
        {
            var existing = GameObject.Find(RootBuildingsName);
            if (existing != null)
            {
                Undo.DestroyObjectImmediate(existing);
            }

            GameObject bldgRoot = HoanKiemBuildingGenerator.GenerateAllStreetBuildings();
            bldgRoot.name = RootBuildingsName;
            Undo.RegisterCreatedObjectUndo(bldgRoot, "Generate Street Buildings");

            Debug.Log("<color=#00FFAA>[HoanKiemStreetEnvironmentSetup] Đã xây dựng thành công Nhà Cửa & Dãy Phố 4 Mặt Hồ Gươm!</color>");
        }

        [MenuItem("HoanKiem VR/6. Xóa Tất Cả Các Đối Tượng Nâng Cấp (Clear Upgrades)", false, 20)]
        public static void ClearAllUpgrades()
        {
            if (!EditorUtility.DisplayDialog("Xóa các đối tượng nâng cấp", "Bạn có chắc muốn xóa tất cả các công trình nâng cấp (Đường chạy xanh, vạch kẻ đường, xe cộ, nhà cửa phố xung quanh)?", "Đồng ý", "Hủy"))
                return;

            string[] names = new string[] { RootRunningTrackName, RootRoadMarkingsName, RootTrafficSystemName, RootBuildingsName };
            foreach (var n in names)
            {
                var obj = GameObject.Find(n);
                if (obj != null) Undo.DestroyObjectImmediate(obj);
            }

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[HoanKiemStreetEnvironmentSetup] Đã xóa toàn bộ các đối tượng nâng cấp.");
        }
    }
}
#endif
