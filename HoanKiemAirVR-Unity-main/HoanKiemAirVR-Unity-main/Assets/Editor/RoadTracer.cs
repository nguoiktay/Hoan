#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Text;

namespace HoanKiemAirVR.Editor
{
    [InitializeOnLoad]
    public static class RoadTracer
    {
        static RoadTracer()
        {
            EditorApplication.delayCall += () =>
            {
                Trace();
            };
        }

        [MenuItem("HoanKiem VR/Trace Real Road and Lake Boundaries")]
        public static void Trace()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== TRACING REAL ROAD, BENCHES & WALKWAY ===");

            // 1. Tìm tất cả các Benches dọc bờ hồ (ghế đá ven hồ luôn đặt chính xác trên vỉa hè ven hồ!)
            var allBenches = GameObject.FindObjectsOfType<Transform>();
            int benchCount = 0;
            foreach (var t in allBenches)
            {
                if (t.name.ToLower().Contains("bench") && t.parent != null && !t.name.Contains("Crowd"))
                {
                    sb.AppendLine($"BENCH: {t.name} -> Pos: {t.position}, Forward: {t.forward}");
                    benchCount++;
                    if (benchCount >= 20) break;
                }
            }

            // 2. Tìm các vật thể đường và vỉa hè
            var allRenderers = GameObject.FindObjectsOfType<MeshRenderer>();
            foreach (var r in allRenderers)
            {
                string n = r.name.ToLower();
                if (n.Contains("road") || n.Contains("lake") || n.Contains("water") || n.Contains("full_roads"))
                {
                    sb.AppendLine($"RENDERER: {r.name} -> Center: {r.bounds.center}, Size: {r.bounds.size}, Min: {r.bounds.min}, Max: {r.bounds.max}");
                }
            }

            Debug.Log(sb.ToString());
        }
    }
}
#endif
