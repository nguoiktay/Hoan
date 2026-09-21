#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace HoanKiemAirVR.Editor
{
    [InitializeOnLoad]
    public static class SelectedInspector
    {
        static SelectedInspector()
        {
            EditorApplication.delayCall += () =>
            {
                if (Selection.activeTransform != null)
                {
                    Debug.Log($"[SELECTED OBJECT] Name: {Selection.activeTransform.name}, Pos: {Selection.activeTransform.position}, Parent: {Selection.activeTransform.parent?.name}");
                }
                else
                {
                    Debug.Log("[SELECTED OBJECT] Nothing currently selected.");
                }
            };
        }
    }
}
#endif
