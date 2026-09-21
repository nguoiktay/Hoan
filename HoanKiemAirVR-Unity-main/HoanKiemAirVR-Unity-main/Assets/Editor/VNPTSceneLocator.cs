#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace HoanKiemAirVR.Editor
{
    public static class VNPTSceneLocator
    {
        [MenuItem("VNPT/Focus Toa Nha VNPT", false, 10)]
        public static void FocusVNPT()
        {
            var vnpt = GameObject.Find("ToaNha_VNPT_Hanoi");
            if (vnpt != null)
            {
                Selection.activeGameObject = vnpt;
                SceneView.FrameLastActiveSceneView();
                Debug.Log("<color=#00FFAA>[VNPTSceneLocator] Đã định vị và Focus tới Tòa nhà VNPT (Bưu điện Hà Nội)!</color>");
            }
            else
            {
                Debug.LogWarning("[VNPTSceneLocator] Không tìm thấy ToaNha_VNPT_Hanoi trong Scene!");
            }
        }
    }
}
#endif
