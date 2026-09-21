#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

namespace HoanKiemAirVR.Editor
{
    [InitializeOnLoad]
    public static class CrowdAutoRebake
    {
        static CrowdAutoRebake()
        {
            EditorApplication.delayCall += ReBakeIfPresent;
        }

        public static void ReBakeIfPresent()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPaused || EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            GameObject root = GameObject.Find("[HoanKiem_CrowdSystem]");
            if (root != null)
            {
                var crowdSys = root.GetComponent<HoanKiemCrowdSystem>();
                if (crowdSys != null)
                {
                    crowdSys.FindContainersIfNull();
                    crowdSys.ApplyCalibratedJogCircuit();
                    crowdSys.SpawnCrowd();
                    if (!EditorApplication.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(root.scene);
                    }
                    Debug.Log("<color=#00FFAA>[CrowdAutoRebake] Đã tự động nắn chỉnh 32 điểm Waypoints chạy bộ bám sát lòng đường thực tế (tránh cây xanh) và cập nhật đám đông!</color>");
                }
            }
        }
    }
}
#endif
