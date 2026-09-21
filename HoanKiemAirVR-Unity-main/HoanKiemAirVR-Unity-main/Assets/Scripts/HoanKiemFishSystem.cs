using System.Collections.Generic;
using UnityEngine;

namespace HoanKiemAirVR
{
    /// <summary>
    /// Hệ thống quản lý các đàn cá sống động bơi lội khắp lòng Hồ Gươm:
    /// - Đàn cá Cầu Thê Húc & Đền Ngọc Sơn (ngay dưới chân cầu đỏ)
    /// - Đàn cá Tháp Rùa (bơi lội quanh chân tháp cổ kính)
    /// - Đàn cá Bờ Đông Đinh Tiên Hoàng (ngắm nhìn toà nhà VNPT)
    /// - Đàn cá Bờ Tây Lê Thái Tổ & Thuỷ Tạ
    /// - Các đàn cá bơi tự do khắp lòng hồ
    /// </summary>
    public class HoanKiemFishSystem : MonoBehaviour
    {
        [Header("Tự động sinh cá khi Play (Runtime)")]
        public bool spawnOnStart = true;

        [Header("Số lượng cá trong hồ (Càng nhiều càng tốt)")]
        [Range(20, 250)] public int totalFishCount = 120;

        [Header("Danh sách Prefab Cá")]
        public GameObject[] fishPrefabs;

        [System.Serializable]
        public class FishSchoolZone
        {
            public string zoneName;
            public Vector3 center;
            public float radius;
            public int fishCount;
            public Vector3[] patrolWaypoints;
            [HideInInspector] public Transform zoneTransform;
            [HideInInspector] public int currentWaypointIndex;
        }

        [Header("Các khu vực đàn cá trọng điểm quanh Hồ Gươm")]
        public List<FishSchoolZone> schoolZones = new List<FishSchoolZone>();

        private Transform fishContainer;

        private void Awake()
        {
            InitDefaultZones();
        }

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnAllFish();
            }
        }

        private void Update()
        {
            // Di chuyển tâm các đàn cá tuần hoàn nhẹ nhàng quanh các waypoints
            float dt = Time.deltaTime;
            for (int i = 0; i < schoolZones.Count; i++)
            {
                var zone = schoolZones[i];
                if (zone.zoneTransform == null || zone.patrolWaypoints == null || zone.patrolWaypoints.Length == 0) continue;

                Vector3 targetWp = zone.patrolWaypoints[zone.currentWaypointIndex];
                zone.zoneTransform.position = Vector3.MoveTowards(zone.zoneTransform.position, targetWp, 0.8f * dt);

                if (Vector3.Distance(zone.zoneTransform.position, targetWp) < 3.0f)
                {
                    zone.currentWaypointIndex = (zone.currentWaypointIndex + 1) % zone.patrolWaypoints.Length;
                }
            }
        }

        public void InitDefaultZones()
        {
            if (schoolZones != null && schoolZones.Count > 0) return;

            schoolZones = new List<FishSchoolZone>()
            {
                // 1. Đàn cá chân Cầu Thê Húc & Đền Ngọc Sơn (Khu vực ngắm cảnh đẹp nhất)
                new FishSchoolZone
                {
                    zoneName = "DanCa_CauTheHuc",
                    center = new Vector3(642.0f, -1.25f, -668.0f),
                    radius = 16.0f,
                    fishCount = 35,
                    patrolWaypoints = new Vector3[]
                    {
                        new Vector3(642.0f, -1.25f, -668.0f), // Dưới chân Cầu Thê Húc
                        new Vector3(632.0f, -1.30f, -680.0f), // Ven đảo Ngọc Sơn
                        new Vector3(648.0f, -1.25f, -655.0f), // Đầu cầu phía Đông
                        new Vector3(638.0f, -1.35f, -660.0f)
                    }
                },

                // 2. Đàn cá quanh Tháp Rùa (Điểm nhấn lịch sử giữa hồ)
                new FishSchoolZone
                {
                    zoneName = "DanCa_ThapRua",
                    center = new Vector3(618.0f, -1.35f, -815.0f),
                    radius = 18.0f,
                    fishCount = 30,
                    patrolWaypoints = new Vector3[]
                    {
                        new Vector3(618.0f, -1.35f, -815.0f), // Quanh Tháp Rùa
                        new Vector3(626.0f, -1.40f, -825.0f),
                        new Vector3(610.0f, -1.35f, -830.0f),
                        new Vector3(608.0f, -1.40f, -805.0f)
                    }
                },

                // 3. Đàn cá Bờ Đông (Đinh Tiên Hoàng & Toà nhà VNPT)
                new FishSchoolZone
                {
                    zoneName = "DanCa_BoDong_VNPT",
                    center = new Vector3(658.0f, -1.25f, -725.0f),
                    radius = 15.0f,
                    fishCount = 25,
                    patrolWaypoints = new Vector3[]
                    {
                        new Vector3(658.0f, -1.25f, -725.0f), // Đối diện Bưu điện VNPT
                        new Vector3(662.0f, -1.25f, -705.0f),
                        new Vector3(655.0f, -1.30f, -750.0f), // Đoạn ngắm Tháp Rùa
                        new Vector3(660.0f, -1.25f, -770.0f)
                    }
                },

                // 4. Đàn cá Bờ Tây (Lê Thái Tổ & Thuỷ Tạ)
                new FishSchoolZone
                {
                    zoneName = "DanCa_BoTay_ThuyTa",
                    center = new Vector3(580.0f, -1.30f, -745.0f),
                    radius = 16.0f,
                    fishCount = 20,
                    patrolWaypoints = new Vector3[]
                    {
                        new Vector3(580.0f, -1.30f, -745.0f),
                        new Vector3(576.0f, -1.30f, -710.0f), // Gần tượng đài Vua Lê
                        new Vector3(582.0f, -1.35f, -780.0f),
                        new Vector3(588.0f, -1.35f, -810.0f)
                    }
                },

                // 5. Đàn cá Lòng Hồ Trung Tâm (Bơi tuần tra diện rộng)
                new FishSchoolZone
                {
                    zoneName = "DanCa_LongHo_TrungTam",
                    center = new Vector3(615.0f, -1.40f, -740.0f),
                    radius = 22.0f,
                    fishCount = 20,
                    patrolWaypoints = new Vector3[]
                    {
                        new Vector3(615.0f, -1.40f, -740.0f),
                        new Vector3(625.0f, -1.40f, -710.0f),
                        new Vector3(605.0f, -1.45f, -760.0f),
                        new Vector3(620.0f, -1.40f, -780.0f)
                    }
                }
            };
        }

        [ContextMenu("Sinh toàn bộ đàn cá vào hồ")]
        public void SpawnAllFish()
        {
            ClearFish();

            if (fishPrefabs == null || fishPrefabs.Length == 0)
            {
                Debug.LogWarning("[HoanKiemFishSystem] Chưa gán danh sách fishPrefabs!");
                return;
            }

            InitDefaultZones();

            fishContainer = transform.Find("DanhSach_Ca_HoanKiem");
            if (fishContainer == null)
            {
                var go = new GameObject("DanhSach_Ca_HoanKiem");
                go.transform.SetParent(transform, false);
                fishContainer = go.transform;
            }

            int spawnedTotal = 0;

            foreach (var zone in schoolZones)
            {
                // Tạo GameObject đại diện cho tâm đàn di động
                GameObject zoneObj = new GameObject($"Zone_{zone.zoneName}");
                zoneObj.transform.SetParent(fishContainer, false);
                zoneObj.transform.position = zone.center;
                zone.zoneTransform = zoneObj.transform;

                for (int i = 0; i < zone.fishCount; i++)
                {
                    GameObject prefab = fishPrefabs[Random.Range(0, fishPrefabs.Length)];
                    if (prefab == null) continue;

                    // Vị trí xuất phát ngẫu nhiên quanh tâm bầy, độ sâu sát mặt nước
                    // Chiều sâu thả cá (mặt nước hồ Gươm ở mức khoảng y = -1.0)
                    Vector2 randCircle = Random.insideUnitCircle * zone.radius;
                    float randY = Random.Range(-1.5f, -1.1f);
                    Vector3 spawnPos = zone.center + new Vector3(randCircle.x, randY - zone.center.y, randCircle.y);

                    Quaternion spawnRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

                    GameObject fish = Instantiate(prefab, spawnPos, spawnRot, zoneObj.transform);
                    fish.name = $"{prefab.name}_{zone.zoneName}_{i + 1:00}";

                    // Kích thước cá vừa phải (khoảng 40cm - 90cm)
                    float scaleFactor = Random.Range(0.4f, 0.9f);
                    fish.transform.localScale = Vector3.one * scaleFactor;

                    // Gắn FishSwimAgent
                    var agent = fish.GetComponent<FishSwimAgent>();
                    if (agent == null) agent = fish.AddComponent<FishSwimAgent>();

                    agent.schoolCenter = zone.zoneTransform;
                    agent.schoolRadius = zone.radius;
                    agent.baseSpeed = Random.Range(1.2f, 2.0f);
                    agent.targetDepth = randY;

                    spawnedTotal++;
                }
            }

            Debug.Log($"<color=#00FFAA>[HoanKiemFishSystem] Đã thả thành công {spawnedTotal} chú cá tung tăng bơi lội trong lòng Hồ Gươm!</color>");
        }

        [ContextMenu("Xóa toàn bộ cá")]
        public void ClearFish()
        {
            var container = transform.Find("DanhSach_Ca_HoanKiem");
            if (container != null)
            {
                // Đổi tên để transform.Find ở dưới không tìm thấy lại nó khi nó chưa kịp bị hủy (vì Destroy chỉ chạy cuối frame)
                container.name = "DanhSach_Ca_HoanKiem_Dying";
                if (Application.isPlaying)
                    Destroy(container.gameObject);
                else
                    DestroyImmediate(container.gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            if (schoolZones == null) return;

            Color[] colors = new Color[]
            {
                new Color(1f, 0.3f, 0.3f, 0.4f), // Đỏ (Cầu Thê Húc)
                new Color(1f, 0.85f, 0.2f, 0.4f), // Vàng (Tháp Rùa)
                new Color(0.2f, 0.8f, 1f, 0.4f), // Xanh dương (Bờ Đông VNPT)
                new Color(0.3f, 1f, 0.5f, 0.4f), // Xanh lá (Bờ Tây Thuỷ Tạ)
                new Color(0.8f, 0.4f, 1f, 0.4f)  // Tím (Lòng hồ trung tâm)
            };

            for (int i = 0; i < schoolZones.Count; i++)
            {
                var zone = schoolZones[i];
                Gizmos.color = colors[i % colors.Length];
                Vector3 c = zone.zoneTransform != null ? zone.zoneTransform.position : zone.center;
                Gizmos.DrawWireSphere(c, zone.radius);

                if (zone.patrolWaypoints != null && zone.patrolWaypoints.Length > 1)
                {
                    Gizmos.color = new Color(colors[i % colors.Length].r, colors[i % colors.Length].g, colors[i % colors.Length].b, 0.8f);
                    for (int j = 0; j < zone.patrolWaypoints.Length; j++)
                    {
                        Vector3 p1 = zone.patrolWaypoints[j];
                        Vector3 p2 = zone.patrolWaypoints[(j + 1) % zone.patrolWaypoints.Length];
                        Gizmos.DrawLine(p1, p2);
                        Gizmos.DrawSphere(p1, 0.8f);
                    }
                }
            }
        }
    }
}
