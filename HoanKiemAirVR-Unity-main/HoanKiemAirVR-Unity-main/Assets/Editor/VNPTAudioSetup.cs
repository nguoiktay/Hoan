#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace HoanKiemAirVR.Editor
{
    /// <summary>
    /// Công cụ tự động lồng âm thanh vào mô hình Tòa nhà VNPT (Bưu điện Hà Nội)
    /// - Âm thanh tiếng xe cộ đường phố (Tieng-xe-co-duong-pho.mp3)
    /// - Âm thanh môi trường nhộn nhịp phố xá (Am_thanh_moi_truong_tieng_on_ao.mp3)
    /// Hỗ trợ 3D Spatial Audio chân thực cho trải nghiệm thực tế ảo VR.
    /// </summary>
    [InitializeOnLoad]
    public static class VNPTAudioSetup
    {
        private const string TrafficAudioPath = "Assets/Audio/Tieng-xe-co-duong-pho.mp3";
        private const string AmbientAudioPath = "Assets/Audio/Am_thanh_moi_truong_tieng_on_ao.mp3";

        private static int retryCount = 0;

        static VNPTAudioSetup()
        {
            EditorApplication.update += RunOnceWhenReady;
        }

        private static void RunOnceWhenReady()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            retryCount++;
            if (retryCount % 30 != 0 && retryCount != 1) return; // Kiểm tra mỗi ~0.5s

            if (ExecuteAudioSetup(false))
            {
                EditorApplication.update -= RunOnceWhenReady;
            }
            else if (retryCount > 300)
            {
                EditorApplication.update -= RunOnceWhenReady;
            }
        }

        [MenuItem("VNPT/Attach Audio to VNPT Model", false, 1)]
        public static void CheckAndSetupAudio()
        {
            ExecuteAudioSetup(true);
        }

        public static bool ExecuteAudioSetup(bool verbose)
        {
            if (EditorApplication.isPlaying || EditorApplication.isPaused)
                return false;

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            AudioClip trafficClip = AssetDatabase.LoadAssetAtPath<AudioClip>(TrafficAudioPath);
            AudioClip ambientClip = AssetDatabase.LoadAssetAtPath<AudioClip>(AmbientAudioPath);

            if (trafficClip == null)
            {
                if (verbose) Debug.LogWarning($"[VNPTAudioSetup] Chưa tìm thấy AudioClip tại {TrafficAudioPath}.");
                return false;
            }

            if (ambientClip == null)
            {
                if (verbose) Debug.LogWarning($"[VNPTAudioSetup] Chưa tìm thấy AudioClip tại {AmbientAudioPath}.");
                return false;
            }

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "HoanKiem" && !scene.path.Contains("HoanKiem"))
            {
                scene = EditorSceneManager.OpenScene("Assets/Scenes/HoanKiem.unity");
            }

            var vnpt = GameObject.Find("ToaNha_VNPT_Hanoi");
            if (vnpt == null)
            {
                if (verbose) Debug.LogWarning("[VNPTAudioSetup] Không tìm thấy ToaNha_VNPT_Hanoi trong Scene!");
                return false;
            }

            bool modified = false;

            // 1. Gắn âm thanh xe cộ đường phố Đinh Tiên Hoàng
            Transform trafficObj = vnpt.transform.Find("Audio_StreetTraffic");
            if (trafficObj == null)
            {
                GameObject go = new GameObject("Audio_StreetTraffic");
                go.transform.SetParent(vnpt.transform, false);
                // Đặt sát vỉa hè trước mặt tiền toà nhà, hướng ra lòng đường
                go.transform.position = new Vector3(703.0f, 1.5f, -710.0f);
                trafficObj = go.transform;
                modified = true;
            }

            AudioSource trafficSource = trafficObj.GetComponent<AudioSource>();
            if (trafficSource == null) trafficSource = trafficObj.gameObject.AddComponent<AudioSource>();
            trafficSource.clip = trafficClip;
            trafficSource.playOnAwake = true;
            trafficSource.loop = true;
            trafficSource.spatialBlend = 1.0f; // 3D Spatial Sound
            trafficSource.volume = 0.65f;
            trafficSource.rolloffMode = AudioRolloffMode.Logarithmic;
            trafficSource.minDistance = 12.0f;
            trafficSource.maxDistance = 110.0f;
            trafficSource.dopplerLevel = 0.2f;

            // 2. Gắn âm thanh môi trường nhộn nhịp tại sảnh và bậc tam cấp
            Transform ambientObj = vnpt.transform.Find("Audio_AmbientCrowd");
            if (ambientObj == null)
            {
                GameObject go = new GameObject("Audio_AmbientCrowd");
                go.transform.SetParent(vnpt.transform, false);
                // Đặt tại sảnh chính toà nhà
                go.transform.position = new Vector3(705.5f, 2.0f, -710.0f);
                ambientObj = go.transform;
                modified = true;
            }

            AudioSource ambientSource = ambientObj.GetComponent<AudioSource>();
            if (ambientSource == null) ambientSource = ambientObj.gameObject.AddComponent<AudioSource>();
            ambientSource.clip = ambientClip;
            ambientSource.playOnAwake = true;
            ambientSource.loop = true;
            ambientSource.spatialBlend = 0.85f; // 3D Spatial Ambient
            ambientSource.volume = 0.55f;
            ambientSource.rolloffMode = AudioRolloffMode.Logarithmic;
            ambientSource.minDistance = 15.0f;
            ambientSource.maxDistance = 130.0f;
            ambientSource.dopplerLevel = 0.1f;

            // 3. Cập nhật vào Prefab Assets/VNPT/VNPT_Building.prefab
            string prefabPath = "Assets/VNPT/VNPT_Building.prefab";
            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefabAsset != null)
            {
                PrefabUtility.SaveAsPrefabAssetAndConnect(vnpt, prefabPath, InteractionMode.AutomatedAction);
                Debug.Log($"<color=#00FFAA>[VNPTAudioSetup] Đã đồng bộ 2 nguồn âm thanh 3D vào Prefab {prefabPath}!</color>");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("<color=#00FFAA>[VNPTAudioSetup] ĐÃ LỒNG THÀNH CÔNG ÂM THANH 3D VÀO MÔ HÌNH TOÀ NHÀ VNPT!</color>\n" +
                      $"✦ Tiếng xe cộ đường phố: {trafficClip.name} ({trafficClip.length:F1}s) tại Pos: {trafficObj.position}\n" +
                      $"✦ Tiếng môi trường phố xá: {ambientClip.name} ({ambientClip.length:F1}s) tại Pos: {ambientObj.position}");
            return true;
        }

        [MenuItem("VNPT/Verify VNPT Model & Audio", false, 2)]
        public static void VerifyModelAndAudio()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "HoanKiem" && !scene.path.Contains("HoanKiem"))
            {
                scene = EditorSceneManager.OpenScene("Assets/Scenes/HoanKiem.unity");
            }

            var vnpt = GameObject.Find("ToaNha_VNPT_Hanoi");
            if (vnpt == null)
            {
                Debug.LogError("[VERIFY] ToaNha_VNPT_Hanoi KHÔNG tồn tại trong scene!");
                return;
            }

            var sources = vnpt.GetComponentsInChildren<AudioSource>(true);
            Debug.Log($"<color=#00FFAA>[VERIFY] ToaNha_VNPT_Hanoi tại Pos: {vnpt.transform.position}. Tổng số AudioSource: {sources.Length}</color>");
            foreach (var s in sources)
            {
                Debug.Log($"  ✦ AudioSource '{s.gameObject.name}': Clip='{s.clip?.name}', Volume={s.volume}, SpatialBlend={s.spatialBlend}, Loop={s.loop}, Pos={s.transform.position}");
            }
        }
        }
    }
}
#endif
