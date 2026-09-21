#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RoadGeometryCalibrator
{
    [MenuItem("HoanKiem VR/Calibrate Road & Building Geometry")]
    public static void Calibrate()
    {
        Debug.Log("[RoadGeometryCalibrator] Starting calibration...");
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/HoanKiem.unity", OpenSceneMode.Single);

        // 1. Locate full_roads
        GameObject roadObj = GameObject.Find("GRAPH/Lake/Decor/full_roads");
        if (roadObj == null)
        {
            var all = UnityEngine.Object.FindObjectsOfType<MeshRenderer>(true);
            foreach (var r in all)
            {
                if (r.name == "full_roads")
                {
                    roadObj = r.gameObject;
                    break;
                }
            }
        }

        if (roadObj == null)
        {
            Debug.LogError("[RoadGeometryCalibrator] full_roads GameObject not found!");
            return;
        }

        var mf = roadObj.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("[RoadGeometryCalibrator] MeshFilter on full_roads has no sharedMesh!");
            return;
        }

        Mesh mesh = mf.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        Transform tr = roadObj.transform;

        Debug.Log($"[RoadGeometryCalibrator] full_roads has {vertices.Length} vertices. Road Transform Pos={tr.position}, Rot={tr.eulerAngles}, LossyScale={tr.lossyScale}");

        // Convert all vertices to world space
        List<Vector3> worldVerts = new List<Vector3>(vertices.Length);
        Vector3 center = Vector3.zero;
        foreach (var v in vertices)
        {
            Vector3 wv = tr.TransformPoint(v);
            worldVerts.Add(wv);
            center += wv;
        }
        center /= vertices.Length;
        Debug.Log($"[RoadGeometryCalibrator] Mesh World Center: {center}");

        // Find min/max bounds
        float minX = float.MaxValue, maxX = float.MinValue;
        float minZ = float.MaxValue, maxZ = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var wv in worldVerts)
        {
            if (wv.x < minX) minX = wv.x;
            if (wv.x > maxX) maxX = wv.x;
            if (wv.z < minZ) minZ = wv.z;
            if (wv.z > maxZ) maxZ = wv.z;
            if (wv.y < minY) minY = wv.y;
            if (wv.y > maxY) maxY = wv.y;
        }

        Debug.Log($"[RoadGeometryCalibrator] Road World Bounds: X=[{minX:F2} .. {maxX:F2}], Z=[{minZ:F2} .. {maxZ:F2}], Y=[{minY:F2} .. {maxY:F2}]");

        // 2. Divide road into angular sectors (e.g. 72 sectors of 5 degrees) around the lake center
        // In Unity XZ plane: angle = atan2(z - center.z, x - center.x)
        int numSectors = 72;
        float sectorAngle = 360f / numSectors;

        List<float>[] sectorInnerRadii = new List<float>[numSectors];
        List<float>[] sectorOuterRadii = new List<float>[numSectors];
        List<Vector3>[] sectorPoints = new List<Vector3>[numSectors];

        for (int i = 0; i < numSectors; i++)
        {
            sectorPoints[i] = new List<Vector3>();
        }

        foreach (var wv in worldVerts)
        {
            float dx = wv.x - center.x;
            float dz = wv.z - center.z;
            float ang = Mathf.Atan2(dz, dx) * Mathf.Rad2Deg;
            if (ang < 0) ang += 360f;

            int sectorIdx = Mathf.Clamp(Mathf.FloorToInt(ang / sectorAngle), 0, numSectors - 1);
            sectorPoints[sectorIdx].Add(wv);
        }

        // For each sector, find min and max radius
        StringBuilder jsonSb = new StringBuilder();
        jsonSb.AppendLine("{");
        jsonSb.AppendLine($"  \"roadCenter\": [{center.x:F3}, {center.y:F3}, {center.z:F3}],");
        jsonSb.AppendLine($"  \"bounds\": {{ \"minX\": {minX:F3}, \"maxX\": {maxX:F3}, \"minZ\": {minZ:F3}, \"maxZ\": {maxZ:F3}, \"minY\": {minY:F3}, \"maxY\": {maxY:F3} }},");

        jsonSb.AppendLine("  \"sectors\": [");

        List<Vector3> innerBoundary = new List<Vector3>();
        List<Vector3> outerBoundary = new List<Vector3>();
        List<Vector3> centerLine = new List<Vector3>();

        for (int i = 0; i < numSectors; i++)
        {
            float midAngle = (i + 0.5f) * sectorAngle;
            float rad = midAngle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            if (sectorPoints[i].Count == 0) continue;

            float minR = float.MaxValue;
            float maxR = float.MinValue;
            float sumY = 0f;

            foreach (var p in sectorPoints[i])
            {
                float d = Vector2.Distance(new Vector2(p.x, p.z), new Vector2(center.x, center.z));
                if (d < minR) minR = d;
                if (d > maxR) maxR = d;
                sumY += p.y;
            }

            float avgY = sumY / sectorPoints[i].Count;
            float midR = (minR + maxR) * 0.5f;

            Vector3 innerPt = new Vector3(center.x + dir.x * minR, avgY, center.z + dir.y * minR);
            Vector3 outerPt = new Vector3(center.x + dir.x * maxR, avgY, center.z + dir.y * maxR);
            Vector3 centerPt = new Vector3(center.x + dir.x * midR, avgY, center.z + dir.y * midR);

            innerBoundary.Add(innerPt);
            outerBoundary.Add(outerPt);
            centerLine.Add(centerPt);

            string comma = (i < numSectors - 1) ? "," : "";
            jsonSb.AppendLine($"    {{ \"index\": {i}, \"angleDeg\": {midAngle:F1}, \"minR\": {minR:F2}, \"maxR\": {maxR:F2}, \"width\": {(maxR - minR):F2}, \"inner\": [{innerPt.x:F2}, {innerPt.y:F2}, {innerPt.z:F2}], \"outer\": [{outerPt.x:F2}, {outerPt.y:F2}, {outerPt.z:F2}], \"center\": [{centerPt.x:F2}, {centerPt.y:F2}, {centerPt.z:F2}] }}{comma}");
        }

        jsonSb.AppendLine("  ]");
        jsonSb.AppendLine("}");

        string outPath = Path.Combine(Application.dataPath, "road_geometry_calibrated.json");
        File.WriteAllText(outPath, jsonSb.ToString(), Encoding.UTF8);
        Debug.Log($"<color=#00FFAA>[RoadGeometryCalibrator] Done! Calibrated {centerLine.Count} points around Hoan Kiem road into: {outPath}</color>");

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }
}
#endif
