using System.Collections.Generic;
using UnityEngine;

namespace HoanKiemAirVR
{
    /// <summary>
    /// Hệ thống quản lý toàn bộ đám đông sống động quanh Hồ Gươm:
    /// - Người đi bộ trên các tuyến phố Đinh Tiên Hoàng, Lê Thái Tổ, Hàng Khay
    /// - Người chạy bộ thể dục quanh cung đường ven hồ
    /// - Các cụm tụ tập ăn kem Tràng Tiền & Thủy Tạ bên bờ hồ
    /// </summary>
    public class HoanKiemCrowdSystem : MonoBehaviour
    {
        [Header("Tự động sinh khi Play (Runtime)")]
        public bool spawnOnStart = true;

        [Header("Cấu hình số lượng")]
        [Range(10, 100)] public int pedestrianCount = 45;
        [Range(2, 30)] public int joggerCount = 8;
        [Range(3, 30)] public int iceCreamEaterCount = 10;

        [Header("Trò chơi dân gian (Nhảy dây, Ô ăn quan, Cờ tướng)")]
        public bool enableFolkGames = true;

        [Header("Tốc độ di chuyển")]
        public float walkSpeed = 1.35f;
        public float jogSpeed = 3.1f;

        [Header("Danh sách Prefab nhân vật Nữ")]
        public GameObject[] femalePrefabs;

        [Header("Danh sách Prefab nhân vật Nam")]
        public GameObject[] malePrefabs;

        [Header("Prefab Que Kem")]
        public GameObject iceCreamPrefab;

        // Định nghĩa các cung đường thực tế quanh Hồ Gươm (tọa độ World chuẩn xác theo Benches & StreetLights)
        public static readonly Vector3[] Waypoints_DinhTienHoang = new Vector3[]
        {
            new Vector3(670.0f, -0.49f, -630.0f), // Đầu phố Đinh Tiên Hoàng gần Cầu Gỗ
            new Vector3(677.0f, -0.49f, -665.0f), // Đoạn nhìn sang Cầu Thê Húc
            new Vector3(679.0f, -0.49f, -705.0f), // Đối diện Bưu điện Hà Nội
            new Vector3(685.0f, -0.49f, -745.0f), // Đoạn ngắm Tháp Rùa
            new Vector3(689.0f, -0.49f, -775.0f), // Gần Vườn hoa Lý Thái Tổ
            new Vector3(683.0f, -0.49f, -830.0f), // Gần rẽ Tràng Tiền Plaza
            new Vector3(675.0f, -0.49f, -875.0f)  // Góc Hàng Khay - Đinh Tiên Hoàng
        };

        public static readonly Vector3[] Waypoints_LeThaiTo = new Vector3[]
        {
            new Vector3(548.0f, -0.49f, -640.0f), // Phía Bắc phố Lê Thái Tổ
            new Vector3(552.0f, -0.49f, -670.0f),
            new Vector3(556.0f, -0.49f, -705.0f), // Gần tượng đài Vua Lê
            new Vector3(560.0f, -0.49f, -760.0f), // Đoạn giữa phố Lê Thái Tổ
            new Vector3(560.0f, -0.49f, -810.0f), // Gần phố Hàng Trống
            new Vector3(565.0f, -0.49f, -855.0f)  // Góc Hàng Khay - Tràng Thi
        };

        public static readonly Vector3[] Waypoints_HangKhay = new Vector3[]
        {
            new Vector3(565.0f, -0.49f, -865.0f), // Góc Lê Thái Tổ - Hàng Khay
            new Vector3(585.0f, -0.49f, -895.0f),
            new Vector3(615.0f, -0.49f, -925.0f), // Đoạn giữa phố Hàng Khay
            new Vector3(645.0f, -0.49f, -928.0f),
            new Vector3(670.0f, -0.49f, -890.0f)  // Góc Hàng Khay - Đinh Tiên Hoàng
        };

        // Cung đường dạo bộ ven hồ cho người chạy bộ (Vòng lặp khép kín chuẩn 100% trên lòng đường chạy ven hồ, tránh cây và cột đèn)
        public static readonly Vector3[] Waypoints_LakeJogCircuit = new Vector3[]
        {
            // Bờ Đông (Phố Đinh Tiên Hoàng ven hồ - Chạy xuôi từ Bắc xuống Nam)
            new Vector3(664.0f, -0.49f, -640.0f), // Gần Cầu Gỗ
            new Vector3(667.5f, -0.49f, -658.0f), // Gần Cầu Thê Húc
            new Vector3(670.0f, -0.49f, -675.0f), // Vỉa hè ven hồ (cạnh ghế đá)
            new Vector3(671.0f, -0.49f, -690.0f),
            new Vector3(669.0f, -0.49f, -710.0f),
            new Vector3(671.5f, -0.49f, -730.0f),
            new Vector3(676.0f, -0.49f, -745.0f), // Ven hồ ngắm Tháp Rùa
            new Vector3(679.5f, -0.49f, -770.0f),
            new Vector3(677.0f, -0.49f, -800.0f),
            new Vector3(674.0f, -0.49f, -830.0f),
            new Vector3(668.0f, -0.49f, -860.0f),
            new Vector3(664.0f, -0.49f, -885.0f), // Góc Đông Nam

            // Bờ Nam (Phố Hàng Khay ven hồ - Chạy từ Đông sang Tây)
            new Vector3(658.0f, -0.49f, -908.0f),
            new Vector3(645.0f, -0.49f, -918.0f),
            new Vector3(625.0f, -0.49f, -916.0f),
            new Vector3(608.0f, -0.49f, -905.0f),
            new Vector3(592.0f, -0.49f, -890.0f),
            new Vector3(578.0f, -0.49f, -870.0f),
            new Vector3(570.0f, -0.49f, -850.0f), // Góc Tây Nam (rẽ vào Lê Thái Tổ)

            // Bờ Tây (Phố Lê Thái Tổ ven hồ - Đi dọc dải đường chạy kẹp giữa bồn cây và vỉa hè)
            new Vector3(566.0f, -0.49f, -830.0f), // Tránh cây xanh và ghế đá
            new Vector3(565.0f, -0.49f, -805.0f),
            new Vector3(566.5f, -0.49f, -775.0f), // Tuyến tránh hàng cây liễu đỏ
            new Vector3(566.5f, -0.49f, -740.0f), // Vị trí P23 thực tế trên mặt đường nhựa bé
            new Vector3(564.0f, -0.49f, -710.0f),
            new Vector3(558.5f, -0.49f, -680.0f),
            new Vector3(552.0f, -0.49f, -655.0f),
            new Vector3(551.0f, -0.49f, -635.0f), // Góc Tây Bắc

            // Bờ Bắc (Quảng trường Đông Kinh Nghĩa Thục - Chạy từ Tây sang Đông)
            new Vector3(568.0f, -0.49f, -622.0f),
            new Vector3(595.0f, -0.49f, -616.0f),
            new Vector3(620.0f, -0.49f, -618.0f),
            new Vector3(642.0f, -0.49f, -623.0f),
            new Vector3(658.0f, -0.49f, -632.0f)  // Nối vòng lặp về (664.0f, -0.49f, -640.0f)
        };

        public struct GatheringSpot
        {
            public string name;
            public Vector3 position;
            public float yaw;
            public bool isSeller;

            public GatheringSpot(string n, Vector3 pos, float rotY, bool seller = false)
            {
                name = n;
                position = pos;
                yaw = rotY;
                isSeller = seller;
            }
        }

        // Tọa độ nhóm ăn kem ven hồ (quầy ki-ốt & ghế đá ngắm Tháp Rùa)
        public static readonly GatheringSpot[] IceCreamSpots = new GatheringSpot[]
        {
            // Cụm 1: Quầy kem Thủy Tạ / Đinh Tiên Hoàng (đối diện Tháp Rùa)
            new GatheringSpot("Người bán kem quầy Đinh Tiên Hoàng", new Vector3(682.5f, -0.49f, -735.0f), 245f, true),
            new GatheringSpot("Bạn gái ăn kem dâu",               new Vector3(680.5f, -0.49f, -736.5f), 45f),
            new GatheringSpot("Bạn nam ăn kem sôcôla",            new Vector3(681.2f, -0.49f, -737.5f), 325f),
            new GatheringSpot("Bạn nữ ăn kem cốm Hà Nội",         new Vector3(679.5f, -0.49f, -737.0f), 75f),
            new GatheringSpot("Người ngồi ghế đá ngắm Tháp Rùa",   new Vector3(675.0f, -0.49f, -735.8f), 265f),
            new GatheringSpot("Bạn đồng hành cạnh ghế đá",        new Vector3(674.5f, -0.49f, -734.5f), 255f),

            // Cụm 2: Quầy kem góc Hàng Khay - Lê Thái Tổ
            new GatheringSpot("Người bán kem quầy Hàng Khay",     new Vector3(576.5f, -0.49f, -870.0f), 200f, true),
            new GatheringSpot("Khách mua kem ốc quế",             new Vector3(575.0f, -0.49f, -871.2f), 30f),
            new GatheringSpot("Bạn đi cùng tại quầy Hàng Khay",    new Vector3(574.5f, -0.49f, -870.0f), 110f)
        };

        [Header("Chỉnh sửa Waypoints trực quan trên Scene")]
        [Tooltip("Container chứa các GameObject điểm Waypoint chạy bộ (kéo thả tự do trên Scene)")]
        public Transform jogWaypointsContainer;
        [Tooltip("Container chứa các điểm người đi bộ Đinh Tiên Hoàng")]
        public Transform pedDinhTienHoangContainer;
        [Tooltip("Container chứa các điểm người đi bộ Lê Thái Tổ")]
        public Transform pedLeThaiToContainer;

        private void Awake()
        {
            FindContainersIfNull();
        }

        private void OnValidate()
        {
            FindContainersIfNull();
        }

        public void FindContainersIfNull()
        {
            if (jogWaypointsContainer == null)
            {
                Transform t = transform.Find("[Waypoints_JogCircuit]");
                if (t != null) jogWaypointsContainer = t;
            }
            if (pedDinhTienHoangContainer == null)
            {
                Transform t = transform.Find("[Waypoints_DinhTienHoang]");
                if (t != null) pedDinhTienHoangContainer = t;
            }
            if (pedLeThaiToContainer == null)
            {
                Transform t = transform.Find("[Waypoints_LeThaiTo]");
                if (t != null) pedLeThaiToContainer = t;
            }
        }

        public List<Vector3> GetLakeJogCircuitWaypoints()
        {
            FindContainersIfNull();
            if (jogWaypointsContainer != null && jogWaypointsContainer.childCount > 0)
            {
                List<Vector3> pts = new List<Vector3>();
                for (int i = 0; i < jogWaypointsContainer.childCount; i++)
                {
                    Transform t = jogWaypointsContainer.GetChild(i);
                    if (t.gameObject.activeSelf)
                    {
                        pts.Add(t.position);
                    }
                }
                if (pts.Count > 0) return pts;
            }
            return new List<Vector3>(Waypoints_LakeJogCircuit);
        }

        public List<Vector3> GetDinhTienHoangWaypoints()
        {
            FindContainersIfNull();
            if (pedDinhTienHoangContainer != null && pedDinhTienHoangContainer.childCount > 0)
            {
                List<Vector3> pts = new List<Vector3>();
                for (int i = 0; i < pedDinhTienHoangContainer.childCount; i++)
                {
                    Transform t = pedDinhTienHoangContainer.GetChild(i);
                    if (t.gameObject.activeSelf)
                    {
                        pts.Add(t.position);
                    }
                }
                if (pts.Count > 0) return pts;
            }
            return new List<Vector3>(Waypoints_DinhTienHoang);
        }

        public List<Vector3> GetLeThaiToWaypoints()
        {
            FindContainersIfNull();
            if (pedLeThaiToContainer != null && pedLeThaiToContainer.childCount > 0)
            {
                List<Vector3> pts = new List<Vector3>();
                for (int i = 0; i < pedLeThaiToContainer.childCount; i++)
                {
                    Transform t = pedLeThaiToContainer.GetChild(i);
                    if (t.gameObject.activeSelf)
                    {
                        pts.Add(t.position);
                    }
                }
                if (pts.Count > 0) return pts;
            }
            return new List<Vector3>(Waypoints_LeThaiTo);
        }

        /// <summary>
        /// Tạo các điểm Waypoint chạy bộ thành các GameObject con trong Scene để người dùng tự do kéo thả bằng chuột
        /// </summary>
        public void CreateJogWaypointsInScene()
        {
            Transform existing = transform.Find("[Waypoints_JogCircuit]");
            GameObject containerGO;
            if (existing != null)
            {
                containerGO = existing.gameObject;
                for (int i = containerGO.transform.childCount - 1; i >= 0; i--)
                {
                    DestroyImmediate(containerGO.transform.GetChild(i).gameObject);
                }
            }
            else
            {
                containerGO = new GameObject("[Waypoints_JogCircuit]");
                containerGO.transform.SetParent(transform, false);
            }

            jogWaypointsContainer = containerGO.transform;

            for (int i = 0; i < Waypoints_LakeJogCircuit.Length; i++)
            {
                Vector3 pos = Waypoints_LakeJogCircuit[i];
                GameObject nodeGO = new GameObject($"WP_{i + 1:D2}");
                nodeGO.transform.SetParent(containerGO.transform, false);
                nodeGO.transform.position = pos;

                WaypointNode node = nodeGO.AddComponent<WaypointNode>();
                node.nodeColor = new Color(0.1f, 0.85f, 1.0f, 0.95f);
                node.sphereRadius = 0.35f;
            }

            Debug.Log($"<color=#00FFAA>[HoanKiemCrowdSystem] Đã tạo {Waypoints_LakeJogCircuit.Length} điểm Waypoints chạy bộ thành công! Bạn có thể chọn và kéo thả trực tiếp trong Scene View.</color>");
        }

        /// <summary>
        /// Nắn chỉnh toàn bộ các điểm Waypoint chạy bộ trong Scene về đúng lòng đường chạy ven hồ thực tế
        /// (hoàn toàn tránh cây liễu, cột đèn và mép nước hồ)
        /// </summary>
        [ContextMenu("Áp Dụng Tọa Độ Đường Chạy Chuẩn Thực Tế")]
        public void ApplyCalibratedJogCircuit()
        {
            CreateJogWaypointsInScene();
            SnapAllWaypointsToGround();
            UpdateActiveJoggerPaths();
            Debug.Log("<color=#00FFAA>[HoanKiemCrowdSystem] Đã nắn chỉnh toàn bộ 32 điểm đường chạy bám sát mặt đường thực tế và tránh cây xanh!</color>");
        }

        /// <summary>
        /// Bắt dính toàn bộ các điểm Waypoint xuống mặt phẳng đường/vỉa hè bên dưới
        /// </summary>
        public void SnapAllWaypointsToGround()
        {
            if (jogWaypointsContainer != null)
            {
                WaypointNode[] nodes = jogWaypointsContainer.GetComponentsInChildren<WaypointNode>();
                foreach (var n in nodes)
                {
                    n.SnapToGround();
                }
            }
        }

        /// <summary>
        /// Cập nhật đường chạy mới cho các người chạy bộ đang hoạt động trong Scene ngay lập tức
        /// </summary>
        public void UpdateActiveJoggerPaths()
        {
            List<Vector3> currentPath = GetLakeJogCircuitWaypoints();
            if (currentPath == null || currentPath.Count == 0) return;

            CrowdCharacter[] characters = GetComponentsInChildren<CrowdCharacter>();
            int count = 0;
            foreach (var c in characters)
            {
                if (c.role == CrowdRole.Jogger)
                {
                    c.waypoints = new List<Vector3>(currentPath);
                    if (c.currentWaypointIndex >= currentPath.Count)
                    {
                        c.currentWaypointIndex = 0;
                    }
                    count++;
                }
            }
            Debug.Log($"<color=#00FFAA>[HoanKiemCrowdSystem] Đã cập nhật đường chạy mới cho {count} người chạy bộ trong Scene!</color>");
        }

        void Start()
        {
            FindContainersIfNull();

            // Tự động sinh đám đông đi bộ & chạy bộ quanh hồ khi bắt đầu Play
            if (spawnOnStart)
            {
                SpawnCrowd();
            }
        }

        /// <summary>
        /// Sinh đầy đủ các nhóm đám đông: Đi bộ, chạy bộ, và tụ tập ăn kem
        /// </summary>
        public void SpawnCrowd()
        {
            ClearCrowd();

            EnsurePrefabListLoaded();

            // 1. Tạo nhóm Người Đi Bộ Trên Đường (Pedestrians)
            SpawnPedestrians();

            // 2. Tạo nhóm Người Chạy Bộ Quanh Hồ (Joggers)
            SpawnJoggers();

            // 3. Tạo nhóm Người Tập Trung Ăn Kem (Ice Cream Gatherings)
            SpawnIceCreamGatherings();

            // 4. Tạo các cụm Trò Chơi Dân Gian (Folk Games: Nhảy Dây, Ô Ăn Quan, Cờ Tướng)
            if (enableFolkGames)
            {
                SpawnFolkGames();
            }

            Debug.Log($"<color=#00FF88>[HoanKiemCrowdSystem] Đã sinh thành công {transform.childCount} nhóm hoạt động sống động quanh Hồ Gươm!</color>");
        }

        public void ClearCrowd()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                // KHÔNG xóa container chứa waypoints do người dùng tinh chỉnh trên Scene!
                if (child.name.StartsWith("[Waypoints_")) continue;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(child.gameObject);
                }
                else
#endif
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void SpawnPedestrians()
        {
            GameObject pedGroup = new GameObject("=== Nguoi_Di_Bo (Pedestrians) ===");
            pedGroup.transform.SetParent(transform, false);

            // Phân bổ 70% số người đi bộ cho phố đi bộ Đinh Tiên Hoàng (đường to chính)
            int countDTH = Mathf.RoundToInt(pedestrianCount * 0.7f);
            int countLTT = pedestrianCount - countDTH;

            List<Vector3> ptsDTH = GetDinhTienHoangWaypoints();

            // Tuyến 1: Phố Đinh Tiên Hoàng (Đường to - đông đúc, tấp nập)
            for (int i = 0; i < countDTH; i++)
            {
                bool forward = (i % 2 == 0);
                float progress = (float)i / Mathf.Max(1, countDTH);
                Vector3 spawnPos = GetInterpolatedPoint(ptsDTH.ToArray(), progress);
                // Độ lệch ngang trải rộng từ vỉa hè đến giữa lòng đường nhựa rộng
                Vector3 lateralOffset = Vector3.Cross((ptsDTH[1] - ptsDTH[0]).normalized, Vector3.up) * Random.Range(-2.8f, 2.8f);
                spawnPos += lateralOffset;

                GameObject prefab = PickRandomPrefab();
                if (prefab == null) continue;

                GameObject agentGO = Instantiate(prefab, spawnPos, Quaternion.identity, pedGroup.transform);
                agentGO.name = $"DiBo_DinhTienHoang_{i + 1}_{prefab.name}";

                CrowdCharacter agent = agentGO.AddComponent<CrowdCharacter>();
                agent.role = CrowdRole.Pedestrian;
                // Thêm chút ngẫu nhiên về tốc độ để dòng người đi lại tự nhiên
                agent.moveSpeed = walkSpeed * Random.Range(0.85f, 1.25f);
                agent.pingPongPath = true;
                agent.loopPath = false;

                // Thiết lập waypoints
                List<Vector3> pts = new List<Vector3>(ptsDTH);
                if (!forward) pts.Reverse();
                agent.waypoints = pts;
                agent.currentWaypointIndex = Random.Range(0, pts.Count);
                agent.SnapGroundImmediate();
            }

            // Tuyến 2: Phố Lê Thái Tổ & Hàng Khay
            List<Vector3> ptsLTT = GetLeThaiToWaypoints();
            for (int i = 0; i < countLTT; i++)
            {
                bool forward = (i % 2 == 0);
                float progress = (float)i / Mathf.Max(1, countLTT);
                Vector3 spawnPos = GetInterpolatedPoint(ptsLTT.ToArray(), progress);
                Vector3 lateralOffset = Vector3.Cross((ptsLTT[1] - ptsLTT[0]).normalized, Vector3.up) * Random.Range(-1.8f, 1.8f);
                spawnPos += lateralOffset;

                GameObject prefab = PickRandomPrefab();
                if (prefab == null) continue;

                GameObject agentGO = Instantiate(prefab, spawnPos, Quaternion.identity, pedGroup.transform);
                agentGO.name = $"DiBo_LeThaiTo_{i + 1}_{prefab.name}";

                CrowdCharacter agent = agentGO.AddComponent<CrowdCharacter>();
                agent.role = CrowdRole.Pedestrian;
                agent.moveSpeed = walkSpeed * Random.Range(0.85f, 1.2f);
                agent.pingPongPath = true;
                agent.loopPath = false;

                List<Vector3> pts = new List<Vector3>(ptsLTT);
                if (!forward) pts.Reverse();
                agent.waypoints = pts;
                agent.currentWaypointIndex = Random.Range(0, pts.Count);
                agent.SnapGroundImmediate();
            }
        }

        private void SpawnJoggers()
        {
            GameObject jogGroup = new GameObject("=== Nguoi_Chay_Bo_Quanh_Ho (Joggers) ===");
            jogGroup.transform.SetParent(transform, false);

            List<Vector3> jogPath = GetLakeJogCircuitWaypoints();
            int ptCount = jogPath.Count;

            for (int i = 0; i < joggerCount; i++)
            {
                // Rải đều vị trí xuất phát của người chạy bộ quanh hồ
                int startIndex = (i * (ptCount / joggerCount)) % ptCount;
                Vector3 startPos = jogPath[startIndex];

                // Người chạy bộ thường là thanh niên nam hoặc nữ trẻ trung
                GameObject prefab = PickJoggerPrefab(i % 2 == 0);
                if (prefab == null) continue;

                GameObject joggerGO = Instantiate(prefab, startPos, Quaternion.identity, jogGroup.transform);
                joggerGO.name = $"ChayBo_QuanhHo_{i + 1}_{prefab.name}";

                CrowdCharacter agent = joggerGO.AddComponent<CrowdCharacter>();
                agent.role = CrowdRole.Jogger;
                agent.moveSpeed = jogSpeed;
                agent.loopPath = true;
                agent.pingPongPath = false;
                agent.waypointRadius = 2.0f;

                // Waypoints vòng khép kín quanh hồ
                agent.waypoints = new List<Vector3>(jogPath);
                agent.currentWaypointIndex = (startIndex + 1) % ptCount;
                agent.SnapGroundImmediate();
            }
        }

        private void SpawnIceCreamGatherings()
        {
            GameObject iceCreamGroup = new GameObject("=== Nhom_Tap_Trung_An_Kem (IceCream) ===");
            iceCreamGroup.transform.SetParent(transform, false);

            int totalSpots = Mathf.Min(iceCreamEaterCount, IceCreamSpots.Length);
            for (int i = 0; i < totalSpots; i++)
            {
                GatheringSpot spot = IceCreamSpots[i];
                GameObject prefab = (i % 2 == 0) ? PickRandomFemalePrefab() : PickRandomMalePrefab();
                if (prefab == null) continue;

                Quaternion rot = Quaternion.Euler(0f, spot.yaw, 0f);
                GameObject eaterGO = Instantiate(prefab, spot.position, rot, iceCreamGroup.transform);
                eaterGO.name = $"{spot.name}_{prefab.name}";

                CrowdCharacter agent = eaterGO.AddComponent<CrowdCharacter>();
                agent.role = CrowdRole.IceCreamGathering;
                agent.SnapGroundImmediate();

                // Gắn que kem (hoặc ốc quế kem) vào bàn tay phải của tất cả các bạn ăn kem!
                agent.AttachIceCream(iceCreamPrefab);
            }
        }

        private void SpawnFolkGames()
        {
            GameObject gamesGroup = new GameObject("=== Tro_Choi_Dan_Gian (FolkGames) ===");
            gamesGroup.transform.SetParent(transform, false);

            // 1. Cụm Nhảy Dây Dân Gian (Lòng đường phố đi bộ Đinh Tiên Hoàng gần Cầu Thê Húc)
            SpawnJumpRopeActivity(gamesGroup.transform);

            // 2. Cụm Ô Ăn Quan 1 (Vỉa hè phố Đinh Tiên Hoàng)
            SpawnOAnQuanActivity(gamesGroup.transform, "OAnQuan_DinhTienHoang", new Vector3(676.0f, -0.49f, -685.0f), 90f);

            // 3. Cụm Ô Ăn Quan 2 (Vỉa hè phố Lê Thái Tổ bờ Tây)
            SpawnOAnQuanActivity(gamesGroup.transform, "OAnQuan_LeThaiTo", new Vector3(563.0f, -0.49f, -730.0f), 0f);

            // 4. Cụm Cờ Tướng Vỉa Hè (Vỉa hè phố Đinh Tiên Hoàng ngắm cảnh)
            SpawnXiangqiActivity(gamesGroup.transform, new Vector3(677.5f, -0.49f, -715.0f), 45f);
        }

        private void SpawnJumpRopeActivity(Transform parentGroup)
        {
            GameObject cluster = new GameObject("Cum_Nhay_Day_DinhTienHoang");
            cluster.transform.SetParent(parentGroup, false);

            // 2 người quay dây (Turner 1 và Turner 2 đối diện nhau theo trục Z trên lòng đường đi bộ)
            Vector3 posT1 = new Vector3(676.0f, -0.49f, -657.4f);
            Vector3 posT2 = new Vector3(676.0f, -0.49f, -652.6f);

            GameObject pT1 = PickRandomFemalePrefab() ?? PickRandomPrefab();
            GameObject pT2 = PickRandomMalePrefab() ?? PickRandomPrefab();

            GameObject turner1GO = Instantiate(pT1, posT1, Quaternion.Euler(0f, 0f, 0f), cluster.transform);
            turner1GO.name = "Nguoi_Quay_Day_1";
            CrowdCharacter cT1 = turner1GO.AddComponent<CrowdCharacter>();
            cT1.role = CrowdRole.JumpRopeTurner;
            cT1.SnapGroundImmediate();

            GameObject turner2GO = Instantiate(pT2, posT2, Quaternion.Euler(0f, 180f, 0f), cluster.transform);
            turner2GO.name = "Nguoi_Quay_Day_2";
            CrowdCharacter cT2 = turner2GO.AddComponent<CrowdCharacter>();
            cT2.role = CrowdRole.JumpRopeTurner;
            cT2.SnapGroundImmediate();

            // Người nhảy dây ở chính giữa
            Vector3 posJumper = new Vector3(676.0f, -0.49f, -655.0f);
            GameObject pJumper = PickRandomFemalePrefab() ?? PickRandomPrefab();
            GameObject jumperGO = Instantiate(pJumper, posJumper, Quaternion.Euler(0f, 90f, 0f), cluster.transform);
            jumperGO.name = "Nguoi_Nhay_Day";
            CrowdCharacter cJumper = jumperGO.AddComponent<CrowdCharacter>();
            cJumper.role = CrowdRole.JumpRopeJumper;
            cJumper.SnapGroundImmediate();

            // Sợi dây nhảy 3D chuyển động xoay tròn nhịp nhàng
            GameObject ropeGO = new GameObject("Day_Nhay_3D_Arc");
            ropeGO.transform.SetParent(cluster.transform, false);
            JumpRopeArc arc = ropeGO.AddComponent<JumpRopeArc>();
            arc.turner1 = turner1GO.transform;
            arc.turner2 = turner2GO.transform;
            arc.jumper = jumperGO.transform;
            arc.ropeSpeed = 1.15f;

            // Khán giả đứng xem và vỗ tay cổ vũ xung quanh
            Vector3[] spectatorPositions = new Vector3[]
            {
                new Vector3(673.8f, -0.49f, -656.0f),
                new Vector3(673.8f, -0.49f, -654.0f),
                new Vector3(678.2f, -0.49f, -655.2f)
            };
            float[] spectatorYaws = new float[] { 75f, 105f, 265f };

            for (int i = 0; i < spectatorPositions.Length; i++)
            {
                GameObject pSpec = (i % 2 == 0) ? PickRandomFemalePrefab() : PickRandomMalePrefab();
                if (pSpec == null) continue;

                GameObject specGO = Instantiate(pSpec, spectatorPositions[i], Quaternion.Euler(0f, spectatorYaws[i], 0f), cluster.transform);
                specGO.name = $"Khan_Gia_Nhay_Day_{i + 1}";
                CrowdCharacter cSpec = specGO.AddComponent<CrowdCharacter>();
                cSpec.role = CrowdRole.GameSpectator;
                cSpec.SnapGroundImmediate();
            }
        }

        private void SpawnOAnQuanActivity(Transform parentGroup, string groupName, Vector3 centerPos, float rotY)
        {
            GameObject cluster = new GameObject(groupName);
            cluster.transform.SetParent(parentGroup, false);

            // Dựng bàn cờ Ô ăn quan 3D kẻ phấn và sỏi thạch anh
            OAnQuanBuilder.CreateBoard(centerPos, rotY);

            // 2 người chơi ngồi/đứng 2 bên bàn cờ
            Vector3 forward = Quaternion.Euler(0f, rotY, 0f) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0f, rotY, 0f) * Vector3.right;

            Vector3 posP1 = centerPos + forward * 0.75f;
            Vector3 posP2 = centerPos - forward * 0.75f;

            GameObject p1Prefab = PickRandomFemalePrefab() ?? PickRandomPrefab();
            GameObject p2Prefab = PickRandomMalePrefab() ?? PickRandomPrefab();

            GameObject p1GO = Instantiate(p1Prefab, posP1, Quaternion.Euler(0f, rotY + 180f, 0f), cluster.transform);
            p1GO.name = "Nguoi_Choi_OAnQuan_1";
            CrowdCharacter c1 = p1GO.AddComponent<CrowdCharacter>();
            c1.role = CrowdRole.OAnQuanPlayer;
            c1.SnapGroundImmediate();

            GameObject p2GO = Instantiate(p2Prefab, posP2, Quaternion.Euler(0f, rotY, 0f), cluster.transform);
            p2GO.name = "Nguoi_Choi_OAnQuan_2";
            CrowdCharacter c2 = p2GO.AddComponent<CrowdCharacter>();
            c2.role = CrowdRole.OAnQuanPlayer;
            c2.SnapGroundImmediate();

            // 3 khán giả vây quanh xem cờ
            Vector3[] specOffsets = new Vector3[]
            {
                right * 1.05f,
                -right * 1.05f,
                right * 0.9f + forward * 0.6f
            };

            for (int i = 0; i < specOffsets.Length; i++)
            {
                GameObject pSpec = (i % 2 == 0) ? PickRandomMalePrefab() : PickRandomFemalePrefab();
                if (pSpec == null) continue;

                Vector3 spPos = centerPos + specOffsets[i];
                Vector3 toCenter = (centerPos - spPos).normalized;
                float specYaw = (toCenter.sqrMagnitude > 0.001f) ? Quaternion.LookRotation(toCenter).eulerAngles.y : 0f;

                GameObject specGO = Instantiate(pSpec, spPos, Quaternion.Euler(0f, specYaw, 0f), cluster.transform);
                specGO.name = $"Khan_Gia_OAnQuan_{i + 1}";
                CrowdCharacter cSpec = specGO.AddComponent<CrowdCharacter>();
                cSpec.role = CrowdRole.GameSpectator;
                cSpec.SnapGroundImmediate();
            }
        }

        private void SpawnXiangqiActivity(Transform parentGroup, Vector3 centerPos, float rotY)
        {
            GameObject cluster = new GameObject("Cum_Co_Tuong_Via_He");
            cluster.transform.SetParent(parentGroup, false);

            // Dựng bàn cờ tướng 3D
            XiangqiBuilder.CreateTable(centerPos, rotY);

            // 2 kỳ thủ ngồi đối cờ
            Vector3 forward = Quaternion.Euler(0f, rotY, 0f) * Vector3.forward;
            Vector3 right = Quaternion.Euler(0f, rotY, 0f) * Vector3.right;

            Vector3 posKyThu1 = centerPos + forward * 0.85f;
            Vector3 posKyThu2 = centerPos - forward * 0.85f;

            GameObject p1 = PickRandomMalePrefab() ?? PickRandomPrefab();
            GameObject p2 = PickRandomMalePrefab() ?? PickRandomPrefab();

            GameObject kyThu1GO = Instantiate(p1, posKyThu1, Quaternion.Euler(0f, rotY + 180f, 0f), cluster.transform);
            kyThu1GO.name = "Ky_Thu_Co_Tuong_1";
            CrowdCharacter c1 = kyThu1GO.AddComponent<CrowdCharacter>();
            c1.role = CrowdRole.ChessPlayer;
            c1.SnapGroundImmediate();

            GameObject kyThu2GO = Instantiate(p2, posKyThu2, Quaternion.Euler(0f, rotY, 0f), cluster.transform);
            kyThu2GO.name = "Ky_Thu_Co_Tuong_2";
            CrowdCharacter c2 = kyThu2GO.AddComponent<CrowdCharacter>();
            c2.role = CrowdRole.ChessPlayer;
            c2.SnapGroundImmediate();

            // 3 khán giả ghé mắt xem cờ thế
            Vector3[] specOffsets = new Vector3[]
            {
                right * 1.0f,
                -right * 1.0f,
                -right * 0.8f + forward * 0.7f
            };

            for (int i = 0; i < specOffsets.Length; i++)
            {
                GameObject pSpec = PickRandomPrefab();
                if (pSpec == null) continue;

                Vector3 spPos = centerPos + specOffsets[i];
                Vector3 toCenter = (centerPos - spPos).normalized;
                float specYaw = (toCenter.sqrMagnitude > 0.001f) ? Quaternion.LookRotation(toCenter).eulerAngles.y : 0f;

                GameObject specGO = Instantiate(pSpec, spPos, Quaternion.Euler(0f, specYaw, 0f), cluster.transform);
                specGO.name = $"Khan_Gia_Co_Tuong_{i + 1}";
                CrowdCharacter cSpec = specGO.AddComponent<CrowdCharacter>();
                cSpec.role = CrowdRole.GameSpectator;
                cSpec.SnapGroundImmediate();
            }
        }

        private Vector3 GetInterpolatedPoint(Vector3[] pts, float t)
        {
            if (pts == null || pts.Length == 0) return Vector3.zero;
            if (pts.Length == 1) return pts[0];

            float totalDist = 0f;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                totalDist += Vector3.Distance(pts[i], pts[i + 1]);
            }

            float targetDist = t * totalDist;
            float currentDist = 0f;

            for (int i = 0; i < pts.Length - 1; i++)
            {
                float seg = Vector3.Distance(pts[i], pts[i + 1]);
                if (currentDist + seg >= targetDist)
                {
                    float segT = (seg > 0.001f) ? (targetDist - currentDist) / seg : 0f;
                    return Vector3.Lerp(pts[i], pts[i + 1], segT);
                }
                currentDist += seg;
            }

            return pts[pts.Length - 1];
        }

        private void EnsurePrefabListLoaded()
        {
#if UNITY_EDITOR
            if ((femalePrefabs == null || femalePrefabs.Length == 0) || (malePrefabs == null || malePrefabs.Length == 0))
            {
                List<GameObject> fList = new List<GameObject>();
                List<GameObject> mList = new List<GameObject>();

                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/DenysAlmaral/CityPeople/Prefabs" });
                foreach (string guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    GameObject p = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (p != null && p.GetComponent<Animator>() != null)
                    {
                        string lower = p.name.ToLower();
                        if (lower.Contains("female") || lower.Contains("_f_"))
                        {
                            fList.Add(p);
                        }
                        else if (lower.Contains("male") || lower.Contains("_m_") || lower.Contains("boy"))
                        {
                            mList.Add(p);
                        }
                    }
                }

                if (femalePrefabs == null || femalePrefabs.Length == 0) femalePrefabs = fList.ToArray();
                if (malePrefabs == null || malePrefabs.Length == 0) malePrefabs = mList.ToArray();

                if (iceCreamPrefab == null)
                {
                    iceCreamPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Eloi_CityKit/Mesh/HoanKiemLake/Prefabs/IceCream.prefab");
                }
            }
#endif
        }

        private GameObject PickRandomPrefab()
        {
            EnsurePrefabListLoaded();
            bool female = (Random.value > 0.5f);
            return female ? PickRandomFemalePrefab() : PickRandomMalePrefab();
        }

        private GameObject PickRandomFemalePrefab()
        {
            if (femalePrefabs != null && femalePrefabs.Length > 0)
                return femalePrefabs[Random.Range(0, femalePrefabs.Length)];
            return null;
        }

        private GameObject PickRandomMalePrefab()
        {
            if (malePrefabs != null && malePrefabs.Length > 0)
                return malePrefabs[Random.Range(0, malePrefabs.Length)];
            return null;
        }

        private GameObject PickJoggerPrefab(bool preferFemale)
        {
            EnsurePrefabListLoaded();
            if (preferFemale && femalePrefabs != null && femalePrefabs.Length > 0)
            {
                // Ưu tiên casual_Female_G hoặc casual_Female_K cho người chạy bộ
                foreach (var p in femalePrefabs)
                {
                    if (p.name.Contains("casual_Female")) return p;
                }
                return femalePrefabs[0];
            }
            else if (malePrefabs != null && malePrefabs.Length > 0)
            {
                // Ưu tiên casual_Male_G hoặc casual_Male_K cho người chạy bộ
                foreach (var p in malePrefabs)
                {
                    if (p.name.Contains("casual_Male")) return p;
                }
                return malePrefabs[0];
            }
            return PickRandomPrefab();
        }

        void OnDrawGizmos()
        {
            // Vẽ đường đi bộ phố Đinh Tiên Hoàng (Màu Xanh Lá Cây)
            Gizmos.color = new Color(0.2f, 0.9f, 0.3f, 0.85f);
            DrawPathGizmo(GetDinhTienHoangWaypoints().ToArray(), false);

            // Vẽ đường đi bộ phố Lê Thái Tổ (Màu Vàng Chanh)
            Gizmos.color = new Color(0.95f, 0.85f, 0.2f, 0.85f);
            DrawPathGizmo(GetLeThaiToWaypoints().ToArray(), false);

            // Vẽ cung đường chạy bộ quanh hồ (Màu Xanh Dương / Cyan khép kín)
            // Tự động kết nối trực tiếp các điểm trong container, kéo thả đến đâu đường vẽ theo đến đó
            Gizmos.color = new Color(0.1f, 0.8f, 1.0f, 0.95f);
            DrawPathGizmo(GetLakeJogCircuitWaypoints().ToArray(), true);

            // Vẽ các điểm tụ tập ăn kem ven hồ (Màu Hồng / Magenta)
            Gizmos.color = new Color(1.0f, 0.35f, 0.75f, 1.0f);
            for (int i = 0; i < IceCreamSpots.Length; i++)
            {
                Gizmos.DrawSphere(IceCreamSpots[i].position, 0.45f);
            }
        }

        private void DrawPathGizmo(Vector3[] pts, bool loop)
        {
            if (pts == null || pts.Length < 2) return;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                Gizmos.DrawSphere(pts[i], 0.3f);
                Gizmos.DrawLine(pts[i], pts[i + 1]);
            }
            Gizmos.DrawSphere(pts[pts.Length - 1], 0.3f);
            if (loop)
            {
                Gizmos.DrawLine(pts[pts.Length - 1], pts[0]);
            }
        }
    }
}
