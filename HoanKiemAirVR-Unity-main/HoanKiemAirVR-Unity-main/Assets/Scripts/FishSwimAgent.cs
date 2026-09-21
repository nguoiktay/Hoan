using UnityEngine;

namespace HoanKiemAirVR
{
    /// <summary>
    /// Điều khiển chuyển động bơi lội uốn lượn tự nhiên của từng con cá trong Hồ Gươm:
    /// - Vẫy đuôi / lắc mình hình sin sống động
    /// - Nhấp nhô bơi lội dưới mặt nước
    /// - Bơi theo đàn hoặc bơi tự do
    /// - Tự động quay đầu khi đến gần bờ hồ
    /// </summary>
    [SelectionBase]
    public class FishSwimAgent : MonoBehaviour
    {
        [Header("Tốc độ bơi")]
        public float baseSpeed = 1.6f;
        public float turnSpeed = 2.5f;
        public float currentSpeed;

        [Header("Chuyển động uốn lượn (Waggle)")]
        public float waggleSpeed = 7.0f;
        public float waggleAngle = 18.0f;
        public float rollAngle = 4.0f;

        [Header("Độ sâu bơi")]
        public float targetDepth = -0.8f;
        public float minDepth = -1.6f;
        public float maxDepth = -1.1f;

        [Header("Mục tiêu & Đàn cá")]
        public Transform schoolCenter;
        public float schoolRadius = 12.0f;
        public Vector3 currentTargetPos;

        private float phaseOffset;
        private float nextDecisionTime;
        private Quaternion smoothFacingRot;

        // Tâm hồ và giới hạn an toàn lòng hồ
        private static readonly Vector3 LakeCenter = new Vector3(620.0f, -1.3f, -770.0f);

        private void Start()
        {
            currentSpeed = baseSpeed * Random.Range(0.85f, 1.25f);
            phaseOffset = Random.Range(0f, 100f);
            waggleSpeed = Random.Range(6.0f, 9.0f);
            waggleAngle = Random.Range(14.0f, 22.0f);
            targetDepth = Random.Range(minDepth, maxDepth);

            smoothFacingRot = transform.rotation;
            PickNewTarget();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            // 1. Kiểm tra mục tiêu hoặc đổi hướng ngẫu nhiên
            if (Time.time >= nextDecisionTime || Vector3.Distance(transform.position, currentTargetPos) < 2.0f)
            {
                PickNewTarget();
            }

            // 2. Hướng di chuyển
            Vector3 toTarget = currentTargetPos - transform.position;
            // Giới hạn độ sâu cá bơi (chắc chắn dưới mặt nước y = -1.0)
            targetDepth = Mathf.Clamp(targetDepth, -1.5f, -1.1f);
            toTarget.y = (targetDepth - transform.position.y) * 0.5f;

            if (toTarget.sqrMagnitude > 0.01f)
            {
                Quaternion desiredRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
                smoothFacingRot = Quaternion.Slerp(smoothFacingRot, desiredRot, turnSpeed * dt);
            }

            // 3. Tiến về phía trước
            transform.position += smoothFacingRot * Vector3.forward * (currentSpeed * dt);

            // Giới hạn độ sâu không vượt khỏi mặt nước
            Vector3 clampedPos = transform.position;
            clampedPos.y = Mathf.Clamp(clampedPos.y, minDepth, maxDepth);
            transform.position = clampedPos;

            // 4. Hiệu ứng vẫy đuôi / lắc mình nhịp nhàng khi bơi
            float waggle = Mathf.Sin(Time.time * waggleSpeed + phaseOffset) * waggleAngle;
            float roll = Mathf.Sin((Time.time * waggleSpeed * 0.5f) + phaseOffset) * rollAngle;
            transform.rotation = smoothFacingRot * Quaternion.Euler(0f, waggle, roll);
        }

        public void PickNewTarget()
        {
            nextDecisionTime = Time.time + Random.Range(4.0f, 9.0f);

            // Nếu quá gần bờ, ưu tiên bơi về phía lòng hồ
            if (IsNearShore(transform.position))
            {
                currentTargetPos = LakeCenter + new Vector3(Random.Range(-25f, 25f), 0f, Random.Range(-40f, 40f));
                targetDepth = Random.Range(minDepth, maxDepth);
                return;
            }

            if (schoolCenter != null)
            {
                // Bơi xung quanh tâm đàn
                Vector2 randCircle = Random.insideUnitCircle * schoolRadius;
                currentTargetPos = schoolCenter.position + new Vector3(randCircle.x, 0f, randCircle.y);
            }
            else
            {
                // Bơi tự do
                currentTargetPos = transform.position + (transform.forward * Random.Range(6.0f, 15.0f)) + (Random.insideUnitSphere * 4.0f);
            }

            targetDepth = Random.Range(minDepth, maxDepth);
            currentTargetPos.y = targetDepth;
        }

        private bool IsNearShore(Vector3 pos)
        {
            // Bờ Đông Đinh Tiên Hoàng: X > 666
            // Bờ Tây Lê Thái Tổ: X < 566
            // Bờ Bắc Cầu Gỗ/Đông Kinh Nghĩa Thục: Z > -625
            // Bờ Nam Hàng Khay: Z < -910
            if (pos.x > 666.0f || pos.x < 566.0f || pos.z > -625.0f || pos.z < -910.0f)
                return true;

            return false;
        }
    }
}
