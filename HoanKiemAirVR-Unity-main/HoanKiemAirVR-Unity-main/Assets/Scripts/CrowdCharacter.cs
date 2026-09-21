using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoanKiemAirVR
{
    public enum CrowdRole
    {
        Pedestrian,        // Đi bộ dọc theo các tuyến phố
        Jogger,            // Chạy bộ vòng quanh hồ
        IceCreamGathering, // Nhóm tụ tập ăn kem, trò chuyện ven hồ
        JumpRopeTurner,    // Người quay dây nhảy
        JumpRopeJumper,    // Người nhảy dây ở giữa
        OAnQuanPlayer,     // Người chơi ô ăn quan
        ChessPlayer,       // Người chơi cờ tướng
        GameSpectator      // Khán giả cổ vũ trò chơi dân gian
    }

    /// <summary>
    /// Điều khiển từng nhân vật trong hệ thống đám đông Hồ Gươm
    /// Hỗ trợ đi bộ, chạy bộ và nhóm ăn kem với chuyển động và hoạt ảnh tự nhiên
    /// </summary>
    public class CrowdCharacter : MonoBehaviour
    {
        [Header("Vai trò & Hành vi")]
        public CrowdRole role = CrowdRole.Pedestrian;
        public bool isFemale = true;

        [Header("Di chuyển & Tốc độ")]
        public float moveSpeed = 1.35f;
        public float turnSpeed = 6.0f;
        public float waypointRadius = 1.2f;
        public bool loopPath = true;
        public bool pingPongPath = false;

        [Header("Bám mặt đường (Ground Snapping)")]
        public bool snapToGround = true;
        public float groundCheckOffset = 1.5f;
        public float groundCheckDistance = 3.5f;
        public float defaultGroundY = -0.49f;

        [Header("Lộ trình Waypoints")]
        public List<Vector3> waypoints = new List<Vector3>();
        public int currentWaypointIndex = 0;
        private int pathDirection = 1;

        [Header("Vật phẩm ăn kem")]
        public bool holdsIceCream = false;
        public GameObject iceCreamPropInstance;

        // References
        private Animator animator;
        private Coroutine idleShuffleCoroutine;
        private float originalSpeed;

        // Animation clip names
        private const string ANIM_F_WALK = "locom_f_basicWalk_30f";
        private const string ANIM_M_WALK = "locom_m_basicWalk_30f";
        private const string ANIM_F_JOG  = "locom_f_jogging_30f";
        private const string ANIM_M_JOG  = "locom_m_jogging_30f";

        private readonly string[] ANIM_F_IDLES = new string[]
        {
            "idle_f_1_150f",
            "idle_f_2_190f",
            "idle_selfcheck_1_300f"
        };

        private readonly string[] ANIM_M_IDLES = new string[]
        {
            "idle_m_2_220f",
            "idle_phoneTalking_180f",
            "idle_selfcheck_1_300f"
        };

        void Awake()
        {
            animator = GetComponent<Animator>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            // Vô hiệu hóa script CityPeople mặc định nếu có để tránh tự đổi clip ngẫu nhiên
            var cityPeopleScript = GetComponent<CityPeople.CityPeople>();
            if (cityPeopleScript != null)
            {
                cityPeopleScript.enabled = false;
            }

            // Tự động nhận diện giới tính dựa vào tên animator controller hoặc tên prefab
            DetectGender();
        }

        void Start()
        {
            // Thêm chút ngẫu nhiên về tốc độ để nhân vật không bước đi đồng loạt như robot
            originalSpeed = moveSpeed;
            moveSpeed = originalSpeed * Random.Range(0.88f, 1.12f);

            // Bám đất ngay khi sinh ra
            if (snapToGround)
            {
                SnapGroundImmediate();
            }

            // Thiết lập hoạt ảnh tương ứng vai trò
            SetupInitialAnimation();

            if (role == CrowdRole.IceCreamGathering || role == CrowdRole.GameSpectator || 
                role == CrowdRole.OAnQuanPlayer || role == CrowdRole.ChessPlayer || role == CrowdRole.JumpRopeTurner)
            {
                idleShuffleCoroutine = StartCoroutine(IceCreamIdleRoutine());
            }
        }

        void Update()
        {
            if (role == CrowdRole.Pedestrian || role == CrowdRole.Jogger)
            {
                HandleMovement();
            }

            if (snapToGround)
            {
                UpdateGroundPosition();
            }
        }

        private void DetectGender()
        {
            string n = gameObject.name.ToLower();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                string ctrlName = animator.runtimeAnimatorController.name.ToLower();
                if (ctrlName.Contains("city f") || ctrlName.Contains("_f_") || ctrlName.Contains("female"))
                {
                    isFemale = true;
                    return;
                }
                if (ctrlName.Contains("city m") || ctrlName.Contains("_m_") || ctrlName.Contains("male"))
                {
                    isFemale = false;
                    return;
                }
            }

            if (n.Contains("female") || n.Contains("_f_") || n.Contains("girl") || n.Contains("elder_female"))
            {
                isFemale = true;
            }
            else
            {
                isFemale = false;
            }
        }

        private void SetupInitialAnimation()
        {
            if (animator == null) return;

            switch (role)
            {
                case CrowdRole.Pedestrian:
                    string walkAnim = isFemale ? ANIM_F_WALK : ANIM_M_WALK;
                    SafeCrossFade(walkAnim, 0.25f, Random.value * 2.0f);
                    break;

                case CrowdRole.Jogger:
                    string jogAnim = isFemale ? ANIM_F_JOG : ANIM_M_JOG;
                    SafeCrossFade(jogAnim, 0.25f, Random.value * 1.5f);
                    break;

                case CrowdRole.IceCreamGathering:
                case CrowdRole.GameSpectator:
                    string initialIdle = isFemale 
                        ? ANIM_F_IDLES[Random.Range(0, ANIM_F_IDLES.Length)]
                        : ANIM_M_IDLES[Random.Range(0, ANIM_M_IDLES.Length)];
                    SafeCrossFade(initialIdle, 0.3f, Random.value * 3.0f);
                    break;

                case CrowdRole.JumpRopeJumper:
                    string jumpAnim = "exercise_warmingUp_170f";
                    SafeCrossFade(jumpAnim, 0.2f, Random.value);
                    break;

                case CrowdRole.JumpRopeTurner:
                    string turnerAnim = isFemale ? "idle_f_2_190f" : "idle_m_2_220f";
                    SafeCrossFade(turnerAnim, 0.25f);
                    break;

                case CrowdRole.OAnQuanPlayer:
                    string oanquanAnim = isFemale ? "idle_selfcheck_1_300f" : "idle_phoneTalking_180f";
                    SafeCrossFade(oanquanAnim, 0.3f);
                    break;

                case CrowdRole.ChessPlayer:
                    string chessAnim = isFemale ? "idle_f_1_150f" : "idle_m_2_220f";
                    SafeCrossFade(chessAnim, 0.3f);
                    break;
            }
        }

        private void SafeCrossFade(string stateName, float transitionDuration, float normalizedTime = -1f)
        {
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null)
                return;

            int hash = Animator.StringToHash(stateName);
            if (animator.HasState(0, hash))
            {
                if (normalizedTime >= 0f)
                    animator.CrossFadeInFixedTime(stateName, transitionDuration, 0, normalizedTime);
                else
                    animator.CrossFadeInFixedTime(stateName, transitionDuration, 0);
            }
            else
            {
                // Fallback nếu controller khác
                if (animator.HasState(0, Animator.StringToHash("idle")))
                    animator.CrossFadeInFixedTime("idle", transitionDuration, 0);
                else if (animator.HasState(0, Animator.StringToHash("forward")))
                    animator.CrossFadeInFixedTime("forward", transitionDuration, 0);
            }
        }

        private void HandleMovement()
        {
            if (waypoints == null || waypoints.Count == 0) return;

            Vector3 target = waypoints[currentWaypointIndex];
            Vector3 myPos = transform.position;

            Vector3 toTarget = target - myPos;
            toTarget.y = 0f;

            float dist = toTarget.magnitude;
            if (dist <= waypointRadius)
            {
                AdvanceWaypoint();
                return;
            }

            Vector3 moveDir = toTarget.normalized;

            // Xoay mượt mà theo hướng di chuyển
            if (moveDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * turnSpeed);
            }

            // Di chuyển thẳng về phía waypoint
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }

        private void AdvanceWaypoint()
        {
            if (waypoints.Count <= 1) return;

            if (loopPath)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            }
            else if (pingPongPath)
            {
                currentWaypointIndex += pathDirection;
                if (currentWaypointIndex >= waypoints.Count)
                {
                    currentWaypointIndex = waypoints.Count - 2;
                    pathDirection = -1;
                }
                else if (currentWaypointIndex < 0)
                {
                    currentWaypointIndex = 1;
                    pathDirection = 1;
                }
            }
            else
            {
                if (currentWaypointIndex < waypoints.Count - 1)
                {
                    currentWaypointIndex++;
                }
            }
        }

        private void UpdateGroundPosition()
        {
            // Hồ Gươm hoàn toàn bằng phẳng ở độ cao -0.49f.
            // Bỏ qua Raycast để tránh việc tia Ray xuyên qua mặt đường (nếu thiếu collider) và chạm xuống mặt nước gây lỗi nửa người.
            Vector3 p = transform.position;
            p.y = defaultGroundY;
            transform.position = p;
        }

        public void SnapGroundImmediate()
        {
            Vector3 p = transform.position;
            p.y = defaultGroundY;
            transform.position = p;
        }

        private IEnumerator IceCreamIdleRoutine()
        {
            while (true)
            {
                float waitTime = Random.Range(7.0f, 14.0f);
                yield return new WaitForSeconds(waitTime);

                if (animator != null)
                {
                    string nextClip = isFemale 
                        ? ANIM_F_IDLES[Random.Range(0, ANIM_F_IDLES.Length)]
                        : ANIM_M_IDLES[Random.Range(0, ANIM_M_IDLES.Length)];

                    SafeCrossFade(nextClip, 0.45f);
                }

                // Xoay nhẹ góc nhìn tự nhiên trong nhóm
                float randomTurn = Random.Range(-25f, 25f);
                Quaternion newRot = transform.rotation * Quaternion.Euler(0f, randomTurn, 0f);
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * 1.5f;
                    transform.rotation = Quaternion.Slerp(transform.rotation, newRot, t);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Gắn que kem (hoặc ốc quế kem) vào bàn tay phải của nhân vật
        /// </summary>
        public void AttachIceCream(GameObject iceCreamPrefab)
        {
            Transform handTransform = FindHandBone(transform);
            if (handTransform == null)
            {
                handTransform = transform;
            }

            if (iceCreamPrefab != null)
            {
                iceCreamPropInstance = Instantiate(iceCreamPrefab, handTransform);
                iceCreamPropInstance.name = "Held_IceCream";
                iceCreamPropInstance.transform.localPosition = new Vector3(0.03f, 0.07f, 0.04f);
                iceCreamPropInstance.transform.localRotation = Quaternion.Euler(85f, -20f, 15f);
                iceCreamPropInstance.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            }
            else
            {
                // Fallback nếu không có prefab: Tạo ốc quế kem 3D xinh xắn tự động
                CreateProceduralIceCreamCone(handTransform);
            }

            holdsIceCream = true;
        }

        private Transform FindHandBone(Transform current)
        {
            string lowerName = current.name.ToLower();
            if (lowerName.Contains("r hand") || lowerName.Contains("hand.r") || lowerName.Contains("right hand") || lowerName.Contains("r_hand"))
            {
                return current;
            }

            for (int i = 0; i < current.childCount; i++)
            {
                Transform found = FindHandBone(current.GetChild(i));
                if (found != null) return found;
            }

            return null;
        }

        private void CreateProceduralIceCreamCone(Transform parentHand)
        {
            GameObject iceCreamHolder = new GameObject("Held_IceCreamCone");
            iceCreamHolder.transform.SetParent(parentHand, false);
            iceCreamHolder.transform.localPosition = new Vector3(0.04f, 0.08f, 0.03f);
            iceCreamHolder.transform.localRotation = Quaternion.Euler(80f, 10f, 0f);

            // Bánh ốc quế (Cone / Cylinder)
            GameObject cone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cone.name = "Cone";
            cone.transform.SetParent(iceCreamHolder.transform, false);
            cone.transform.localScale = new Vector3(0.055f, 0.075f, 0.055f);
            cone.transform.localPosition = Vector3.zero;

            var coneCol = cone.GetComponent<Collider>();
            if (coneCol != null) DestroyImmediate(coneCol);

            // Viên kem tròn (Scoop)
            GameObject scoop = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            scoop.name = "IceCream_Scoop";
            scoop.transform.SetParent(iceCreamHolder.transform, false);
            scoop.transform.localScale = new Vector3(0.085f, 0.085f, 0.085f);
            scoop.transform.localPosition = new Vector3(0f, 0.075f, 0f);

            var scoopCol = scoop.GetComponent<Collider>();
            if (scoopCol != null) DestroyImmediate(scoopCol);

            // Tạo màu sắc kem ngọt ngào (Vani / Dâu / Sôcôla)
            Color[] scoopColors = new Color[]
            {
                new Color(1.0f, 0.95f, 0.78f), // Kem vani Tràng Tiền
                new Color(0.98f, 0.65f, 0.72f), // Kem dâu tây
                new Color(0.55f, 0.90f, 0.65f), // Kem cốm Tràng Tiền đặc sản Hà Nội
                new Color(0.48f, 0.32f, 0.22f)  // Kem sôcôla
            };
            Color chosenColor = scoopColors[Random.Range(0, scoopColors.Length)];

            Renderer scoopRenderer = scoop.GetComponent<Renderer>();
            if (scoopRenderer != null)
            {
                scoopRenderer.material.color = chosenColor;
            }

            Renderer coneRenderer = cone.GetComponent<Renderer>();
            if (coneRenderer != null)
            {
                coneRenderer.material.color = new Color(0.85f, 0.64f, 0.35f); // Màu ốc quế vàng giòn
            }

            iceCreamPropInstance = iceCreamHolder;
        }

        void OnDrawGizmosSelected()
        {
            if (waypoints == null || waypoints.Count < 2) return;

            Gizmos.color = (role == CrowdRole.Jogger) ? Color.cyan : Color.green;
            for (int i = 0; i < waypoints.Count; i++)
            {
                Gizmos.DrawSphere(waypoints[i], 0.35f);
                if (i < waypoints.Count - 1)
                {
                    Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
                }
                else if (loopPath)
                {
                    Gizmos.DrawLine(waypoints[i], waypoints[0]);
                }
            }
        }
    }
}
