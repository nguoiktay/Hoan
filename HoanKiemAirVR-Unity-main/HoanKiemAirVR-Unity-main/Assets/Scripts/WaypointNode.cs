using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HoanKiemAirVR
{
    /// <summary>
    /// Đại diện cho một điểm Waypoint trực quan trong Scene.
    /// Cho phép lập trình viên / người dùng click chọn và dùng công cụ Move (W)
    /// kéo thả trực tiếp đến vị trí mong muốn trên mặt đường hoặc vỉa hè.
    /// </summary>
    [ExecuteInEditMode]
    [SelectionBase]
    public class WaypointNode : MonoBehaviour
    {
        [Header("Màu sắc hiển thị trong Scene View")]
        public Color nodeColor = new Color(0.1f, 0.85f, 1.0f, 0.95f);
        public float sphereRadius = 0.35f;

        [Header("Tự động bám dính mặt đất")]
        public bool autoSnapGround = false;

        void Update()
        {
            if (autoSnapGround && !Application.isPlaying)
            {
                SnapToGround();
            }
        }

        /// <summary>
        /// Bắt dính vị trí Y xuống mặt phẳng đường / vỉa hè phía dưới
        /// </summary>
        [ContextMenu("Snap To Ground (Bắt dính mặt đường)")]
        public void SnapToGround()
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 3.0f;
            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 15.0f))
            {
                Vector3 p = transform.position;
                p.y = hit.point.y + 0.05f; // Nổi nhẹ 5cm trên mặt đường để dễ nhìn
                transform.position = p;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = nodeColor;
            Gizmos.DrawSphere(transform.position, sphereRadius);

#if UNITY_EDITOR
            // Chỉ hiển thị nhãn chữ khi camera ở gần (dưới 30m), tránh làm rối tầm nhìn ở xa
            Camera cam = SceneView.currentDrawingSceneView != null ? SceneView.currentDrawingSceneView.camera : Camera.current;
            if (cam != null)
            {
                float dist = Vector3.Distance(cam.transform.position, transform.position);
                if (dist < 32.0f)
                {
                    GUIStyle labelStyle = new GUIStyle();
                    labelStyle.normal.textColor = Color.white;
                    labelStyle.fontSize = (dist < 12.0f) ? 12 : 10;
                    labelStyle.fontStyle = FontStyle.Bold;
                    labelStyle.alignment = TextAnchor.MiddleCenter;

                    string labelText = gameObject.name.Replace("WP_", "P");
                    Handles.Label(transform.position + Vector3.up * 0.55f, labelText, labelStyle);
                }
            }
#endif
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, sphereRadius * 1.3f);
        }
    }
}
