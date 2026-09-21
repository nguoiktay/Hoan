#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneGeometryInspector
{
    [MenuItem("HoanKiem VR/Inspect Scene Geometry & Roads")]
    public static void Inspect()
    {
        Debug.Log("[SceneGeometryInspector] Starting inspection...");
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/HoanKiem.unity", OpenSceneMode.Single);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"=== SCENE GEOMETRY INSPECTION: {scene.name} ({scene.path}) ===");

        // 1. Root GameObjects in scene
        var rootObjects = scene.GetRootGameObjects();
        sb.AppendLine($"\n--- ROOT GAMEOBJECTS ({rootObjects.Length}) ---");
        foreach (var go in rootObjects)
        {
            sb.AppendLine($"ROOT: '{go.name}' (Active: {go.activeSelf}, Pos: {go.transform.position}, Children: {go.transform.childCount})");
        }

        // 2. Find ALL MeshRenderers
        var renderers = Object.FindObjectsOfType<MeshRenderer>(true);
        sb.AppendLine($"\n--- ALL MESH RENDERERS ({renderers.Length}) ---");
        foreach (var r in renderers)
        {
            string path = GetHierarchyPath(r.transform);
            sb.AppendLine($"RENDERER: '{r.name}' | Path: '{path}' | Center: {r.bounds.center} | Size: {r.bounds.size} | Min: {r.bounds.min} | Max: {r.bounds.max}");
        }

        // 3. Search specifically for objects with 'road', 'street', 'trottoir', 'lake', 'building', 'tree', 'decor'
        sb.AppendLine("\n--- SPECIAL SEARCH KEYWORDS ---");
        var allTransforms = Object.FindObjectsOfType<Transform>(true);
        foreach (var t in allTransforms)
        {
            string n = t.name.ToLower();
            if (n.Contains("road") || n.Contains("street") || n.Contains("trottoir") || n.Contains("floor") || 
                n.Contains("walk") || n.Contains("path") || n.Contains("vnpt") || n.Contains("lake") || 
                n.Contains("temple") || n.Contains("bridge") || n.Contains("water"))
            {
                sb.AppendLine($"MATCH: '{t.name}' | Path: '{GetHierarchyPath(t)}' | Pos: {t.position} | Rot: {t.eulerAngles} | Scale: {t.lossyScale}");
            }
        }

        // 4. Cameras
        var cams = Object.FindObjectsOfType<Camera>(true);
        sb.AppendLine($"\n--- CAMERAS ({cams.Length}) ---");
        foreach (var c in cams)
        {
            sb.AppendLine($"CAMERA: '{c.name}' | Path: '{GetHierarchyPath(c.transform)}' | Pos: {c.transform.position} | Rot: {c.transform.eulerAngles}");
        }

        string outPath = Path.Combine(Application.dataPath, "scene_geometry_inspection.txt");
        File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);
        Debug.Log($"<color=#00FFAA>[SceneGeometryInspector] Done! Wrote to: {outPath}</color>");

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }

    private static string GetHierarchyPath(Transform t)
    {
        if (t.parent == null) return t.name;
        return GetHierarchyPath(t.parent) + "/" + t.name;
    }
}
#endif
