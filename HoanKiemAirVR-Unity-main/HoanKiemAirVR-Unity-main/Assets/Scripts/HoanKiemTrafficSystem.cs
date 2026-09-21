using System.Collections.Generic;
using UnityEngine;

namespace HoanKiemAirVR
{
    /// <summary>
    /// Hệ thống Quản lý Giao Thông & Xe Cộ quanh Hồ Gươm:
    /// - Điều phối xe cộ di chuyển tuần hành (Moving Traffic) theo các làn đường
    /// - Quản lý xe dừng đỗ thực tế (Parked Vehicles): Các hàng xe máy vỉa hè quán xá,
    ///   taxi đón trả khách, xích lô ven hồ, và xe buýt 2 tầng đỏ tại trạm Bưu điện
    /// - Hỗ trợ cả sinh xe tĩnh trực tiếp trong Scene View và chạy mô phỏng khi Play
    /// </summary>
    [ExecuteInEditMode]
    public class HoanKiemTrafficSystem : MonoBehaviour
    {
        [Header("Chế độ Giao Thông")]
        [Tooltip("Cho phép luồng xe cộ di chuyển tuần hành quanh hồ (Play Mode)")]
        public bool enableMovingTraffic = true;

        [Tooltip("Bố trí các cụm xe dừng đỗ tĩnh trên vỉa hè & bến bãi")]
        public bool enableParkedVehicles = true;

        [Header("Cấu hình Luồng Xe Di Chuyển")]
        [Range(3.0f, 15.0f)] public float trafficSpeed = 6.5f; // ~23 km/h trong phố
        [Range(2, 20)] public int movingVehicleCount = 8;

        [Header("Số lượng xe dừng đỗ")]
        public bool spawnDoubleDeckerTourBus = true;
        public bool spawnParkedMotorbikeRows = true;
        public bool spawnParkedTaxis = true;
        public bool spawnParkedCyclos = true;
        public bool spawnElectricTourTrams = true;

        // Vòng đường di chuyển cho xe cộ (Đi theo chiều quy định quanh hồ trên các làn đường đã hiệu chỉnh)
        public static readonly Vector3[] TrafficLoopWaypoints = new Vector3[]
        {
            // Đi dọc Đinh Tiên Hoàng (Bắc xuống Nam: X = 695..698)
            new Vector3(696.0f, -0.49f, -635.0f),
            new Vector3(697.0f, -0.49f, -670.0f),
            new Vector3(697.5f, -0.49f, -710.0f), // Ngang qua VNPT
            new Vector3(697.5f, -0.49f, -750.0f),
            new Vector3(697.0f, -0.49f, -780.0f),
            new Vector3(696.0f, -0.49f, -835.0f),
            new Vector3(694.0f, -0.49f, -870.0f), // Góc Tràng Tiền Plaza

            // Rẽ vào Hàng Khay (Đông sang Tây: Z = -933.5)
            new Vector3(680.0f, -0.49f, -928.0f),
            new Vector3(645.0f, -0.49f, -933.5f),
            new Vector3(610.0f, -0.49f, -933.5f),
            new Vector3(575.0f, -0.49f, -932.0f),
            new Vector3(555.0f, -0.49f, -900.0f), // Góc Hàng Khay - Tràng Thi

            // Rẽ vào Lê Thái Tổ (Nam lên Bắc: X = 546..548)
            new Vector3(545.0f, -0.49f, -850.0f),
            new Vector3(546.0f, -0.49f, -800.0f),
            new Vector3(546.5f, -0.49f, -750.0f),
            new Vector3(546.5f, -0.49f, -710.0f), // Ngang qua Báo Hà Nội Mới
            new Vector3(546.0f, -0.49f, -665.0f),
            new Vector3(547.0f, -0.49f, -635.0f), // Góc Lương Văn Can - Hàng Gai

            // Ôm qua Quảng trường Đông Kinh Nghĩa Thục (Tây sang Đông: Z = -608)
            new Vector3(575.0f, -0.49f, -610.0f),
            new Vector3(610.0f, -0.49f, -608.0f),
            new Vector3(645.0f, -0.49f, -610.0f),
            new Vector3(675.0f, -0.49f, -618.0f)
        };

        private class MovingVehicleInstance
        {
            public GameObject gameObject;
            public float currentDistance;
            public float speedOffset;
        }

        private List<MovingVehicleInstance> activeMovingVehicles = new List<MovingVehicleInstance>();
        private List<Vector3> smoothTrackPath;
        private float totalTrackLength = 0f;

        private void Start()
        {
            InitializeSmoothPath();

            if (Application.isPlaying && enableMovingTraffic)
            {
                SpawnMovingTrafficForRuntime();
            }
        }

        private void Update()
        {
            if (!Application.isPlaying || !enableMovingTraffic || activeMovingVehicles.Count == 0) return;

            float dt = Time.deltaTime;
            foreach (var v in activeMovingVehicles)
            {
                if (v.gameObject == null) continue;

                float currentSpeed = trafficSpeed + v.speedOffset;
                v.currentDistance += currentSpeed * dt;
                if (v.currentDistance >= totalTrackLength)
                {
                    v.currentDistance -= totalTrackLength;
                }

                // Cập nhật vị trí & hướng xoay
                Vector3 newPos = GetPositionAtDistance(v.currentDistance, out Vector3 forward);
                v.gameObject.transform.position = newPos;
                if (forward != Vector3.zero)
                {
                    v.gameObject.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                }
            }
        }

        private void InitializeSmoothPath()
        {
            smoothTrackPath = HoanKiemGreenRunningTrack.GenerateSmoothSpline(new List<Vector3>(TrafficLoopWaypoints), 10, true);
            totalTrackLength = 0f;
            for (int i = 0; i < smoothTrackPath.Count - 1; i++)
            {
                totalTrackLength += Vector3.Distance(smoothTrackPath[i], smoothTrackPath[i + 1]);
            }
        }

        private Vector3 GetPositionAtDistance(float dist, out Vector3 forward)
        {
            float acc = 0f;
            for (int i = 0; i < smoothTrackPath.Count - 1; i++)
            {
                float segLen = Vector3.Distance(smoothTrackPath[i], smoothTrackPath[i + 1]);
                if (acc + segLen >= dist)
                {
                    float t = (dist - acc) / segLen;
                    forward = (smoothTrackPath[i + 1] - smoothTrackPath[i]).normalized;
                    return Vector3.Lerp(smoothTrackPath[i], smoothTrackPath[i + 1], t);
                }
                acc += segLen;
            }

            forward = (smoothTrackPath[1] - smoothTrackPath[0]).normalized;
            return smoothTrackPath[0];
        }

        private void SpawnMovingTrafficForRuntime()
        {
            activeMovingVehicles.Clear();
            float spacing = totalTrackLength / movingVehicleCount;

            for (int i = 0; i < movingVehicleCount; i++)
            {
                GameObject vObj;
                int type = i % 4;
                if (type == 0) vObj = VehicleBuilder.BuildTaxi(VehicleBuilder.TaxiType.MaiLinh);
                else if (type == 1) vObj = VehicleBuilder.BuildTaxi(VehicleBuilder.TaxiType.XanhSM);
                else if (type == 2) vObj = VehicleBuilder.BuildCivilianCar(VehicleBuilder.CarColor.White);
                else vObj = VehicleBuilder.BuildCivilianCar(VehicleBuilder.CarColor.Black, true);

                vObj.transform.SetParent(transform, true);

                MovingVehicleInstance inst = new MovingVehicleInstance
                {
                    gameObject = vObj,
                    currentDistance = i * spacing,
                    speedOffset = Random.Range(-0.5f, 0.5f)
                };

                Vector3 p = GetPositionAtDistance(inst.currentDistance, out Vector3 fwd);
                vObj.transform.position = p;
                if (fwd != Vector3.zero) vObj.transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);

                activeMovingVehicles.Add(inst);
            }
        }

        /// <summary>
        /// Tạo toàn bộ xe tĩnh và xe tuần hành trong Scene View để người dùng thấy ngay
        /// </summary>
        [ContextMenu("Tạo toàn bộ xe trong Scene")]
        public void SpawnAllVehiclesInEditor()
        {
            // Xóa các xe cũ nếu có
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            InitializeSmoothPath();

            // 1. Xe buýt 2 tầng mui trần đỗ tại trạm đón Bưu điện Hà Nội (VNPT)
            if (spawnDoubleDeckerTourBus)
            {
                GameObject busRoot = new GameObject("1_XeBuyt_2Tang_HanoiCityTour");
                busRoot.transform.SetParent(transform, false);

                // Đỗ sát lề đường trước bưu điện VNPT (làn ngoài X ≈ 701.5)
                GameObject bus1 = VehicleBuilder.BuildDoubleDeckerBus();
                bus1.transform.SetParent(busRoot.transform, false);
                bus1.transform.position = new Vector3(701.5f, -0.49f, -730.0f);
                bus1.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                // Chiếc thứ 2 đang đi trên đường đoạn Hàng Khay
                GameObject bus2 = VehicleBuilder.BuildDoubleDeckerBus();
                bus2.transform.SetParent(busRoot.transform, false);
                bus2.transform.position = new Vector3(635.0f, -0.49f, -933.5f);
                bus2.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            }

            // 2. Xe Taxi đón khách tại các sảnh lớn & ngã tư
            if (spawnParkedTaxis)
            {
                GameObject taxiRoot = new GameObject("2_XeTaxi_Hanoi");
                taxiRoot.transform.SetParent(transform, false);

                // Taxi Mai Linh đỗ trước khách sạn Apricot (Lê Thái Tổ, làn đỗ sát lề)
                GameObject tx1 = VehicleBuilder.BuildTaxi(VehicleBuilder.TaxiType.MaiLinh);
                tx1.transform.SetParent(taxiRoot.transform, false);
                tx1.transform.position = new Vector3(541.5f, -0.49f, -780.0f);
                tx1.transform.rotation = Quaternion.Euler(0f, 5f, 0f);

                // Taxi Xanh SM (VinFast) đỗ đón khách tại Tràng Tiền Plaza
                GameObject tx2 = VehicleBuilder.BuildTaxi(VehicleBuilder.TaxiType.XanhSM);
                tx2.transform.SetParent(taxiRoot.transform, false);
                tx2.transform.position = new Vector3(699.0f, -0.49f, -870.0f);
                tx2.transform.rotation = Quaternion.Euler(0f, 175f, 0f);

                // Taxi Group đỗ cạnh Quảng trường Đông Kinh Nghĩa Thục
                GameObject tx3 = VehicleBuilder.BuildTaxi(VehicleBuilder.TaxiType.HanoiGroup);
                tx3.transform.SetParent(taxiRoot.transform, false);
                tx3.transform.position = new Vector3(580.0f, -0.49f, -608.0f);
                tx3.transform.rotation = Quaternion.Euler(0f, 75f, 0f);

                // Thêm 2 xe taxi trên làn xe Đinh Tiên Hoàng & Lê Thái Tổ
                GameObject tx4 = VehicleBuilder.BuildTaxi(VehicleBuilder.TaxiType.XanhSM);
                tx4.transform.SetParent(taxiRoot.transform, false);
                tx4.transform.position = new Vector3(695.5f, -0.49f, -675.0f);
                tx4.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                GameObject tx5 = VehicleBuilder.BuildTaxi(VehicleBuilder.TaxiType.MaiLinh);
                tx5.transform.SetParent(taxiRoot.transform, false);
                tx5.transform.position = new Vector3(549.0f, -0.49f, -735.0f);
                tx5.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

            // 3. Ô tô con Sedan & SUV di chuyển trên các tuyến phố
            GameObject carRoot = new GameObject("3_Oto_DanDung");
            carRoot.transform.SetParent(transform, false);

            GameObject c1 = VehicleBuilder.BuildCivilianCar(VehicleBuilder.CarColor.White, true); // SUV Trắng
            c1.transform.SetParent(carRoot.transform, false);
            c1.transform.position = new Vector3(696.0f, -0.49f, -650.0f);
            c1.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            GameObject c2 = VehicleBuilder.BuildCivilianCar(VehicleBuilder.CarColor.Black, false); // Sedan Đen
            c2.transform.SetParent(carRoot.transform, false);
            c2.transform.position = new Vector3(600.0f, -0.49f, -933.5f);
            c2.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

            GameObject c3 = VehicleBuilder.BuildCivilianCar(VehicleBuilder.CarColor.Red, false); // Sedan Đỏ
            c3.transform.SetParent(carRoot.transform, false);
            c3.transform.position = new Vector3(550.0f, -0.49f, -670.0f);
            c3.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            GameObject c4 = VehicleBuilder.BuildCivilianCar(VehicleBuilder.CarColor.Silver, true); // SUV Bạc
            c4.transform.SetParent(carRoot.transform, false);
            c4.transform.position = new Vector3(645.0f, -0.49f, -608.0f);
            c4.transform.rotation = Quaternion.Euler(0f, 105f, 0f);

            // 4. Các hàng xe máy dựng nghiêng trên vỉa hè các tuyến phố
            if (spawnParkedMotorbikeRows)
            {
                GameObject motoRoot = new GameObject("4_HangXeMay_ViaHe");
                motoRoot.transform.SetParent(transform, false);

                // Hàng xe máy trước Bưu điện Hà Nội (VNPT - trên vỉa hè ngoài)
                GameObject row1 = VehicleBuilder.BuildParkedMotorbikeRow(8, 0.9f);
                row1.name = "XeMay_ViaHe_BưuDien";
                row1.transform.SetParent(motoRoot.transform, false);
                row1.transform.position = new Vector3(705.5f, -0.49f, -700.0f);
                row1.transform.rotation = Quaternion.Euler(0f, 90f, 0f);

                // Hàng xe máy dọc phố Hàng Khay (trên vỉa hè trước các cửa hàng tranh & cafe)
                GameObject row2 = VehicleBuilder.BuildParkedMotorbikeRow(10, 0.85f);
                row2.name = "XeMay_ViaHe_HangKhay";
                row2.transform.SetParent(motoRoot.transform, false);
                row2.transform.position = new Vector3(610.0f, -0.49f, -944.5f);
                row2.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

                // Hàng xe máy trước Tòa soạn Báo Hà Nội Mới (Lê Thái Tổ - trên vỉa hè ngoài)
                GameObject row3 = VehicleBuilder.BuildParkedMotorbikeRow(7, 0.9f);
                row3.name = "XeMay_ViaHe_BaoHanoiMoi";
                row3.transform.SetParent(motoRoot.transform, false);
                row3.transform.position = new Vector3(535.5f, -0.49f, -705.0f);
                row3.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

                // Hàng xe máy gần Tòa nhà Hàm Cá Mập (Đông Kinh Nghĩa Thục)
                GameObject row4 = VehicleBuilder.BuildParkedMotorbikeRow(8, 0.85f);
                row4.name = "XeMay_ViaHe_HamCaMap";
                row4.transform.SetParent(motoRoot.transform, false);
                row4.transform.position = new Vector3(600.0f, -0.49f, -590.0f);
                row4.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                // Xe máy đang chạy trên đường (xe Grab & xe số)
                GameObject ride1 = VehicleBuilder.BuildMotorbike(VehicleBuilder.MotorbikeType.GrabDelivery);
                ride1.name = "XeMay_Grab_DangChay";
                ride1.transform.SetParent(motoRoot.transform, false);
                ride1.transform.position = new Vector3(699.0f, -0.49f, -690.0f);
                ride1.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

                GameObject ride2 = VehicleBuilder.BuildMotorbike(VehicleBuilder.MotorbikeType.WaveStandard);
                ride2.name = "XeMay_Wave_DangChay";
                ride2.transform.SetParent(motoRoot.transform, false);
                ride2.transform.position = new Vector3(544.0f, -0.49f, -770.0f);
                ride2.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

            // 5. Xe Xích lô du lịch phố cổ
            if (spawnParkedCyclos)
            {
                GameObject cycloRoot = new GameObject("5_XeXichLo_DuLich");
                cycloRoot.transform.SetParent(transform, false);

                // Đỗ đón khách ven hồ gần Cầu Thê Húc
                GameObject c1_obj = VehicleBuilder.BuildCyclo();
                c1_obj.transform.SetParent(cycloRoot.transform, false);
                c1_obj.transform.position = new Vector3(675.0f, -0.49f, -655.0f);
                c1_obj.transform.rotation = Quaternion.Euler(0f, -120f, 0f);

                // Chiếc xích lô thứ 2 gần góc Hàng Khay
                GameObject c2_obj = VehicleBuilder.BuildCyclo();
                c2_obj.transform.SetParent(cycloRoot.transform, false);
                c2_obj.transform.position = new Vector3(665.0f, -0.49f, -895.0f);
                c2_obj.transform.rotation = Quaternion.Euler(0f, 45f, 0f);

                // Chiếc xích lô thứ 3 gần Vua Lê
                GameObject c3_obj = VehicleBuilder.BuildCyclo();
                c3_obj.transform.SetParent(cycloRoot.transform, false);
                c3_obj.transform.position = new Vector3(560.0f, -0.49f, -750.0f);
                c3_obj.transform.rotation = Quaternion.Euler(0f, -40f, 0f);
            }

            // 6. Xe Điện Du Lịch Bờ Hồ
            if (spawnElectricTourTrams)
            {
                GameObject tramRoot = new GameObject("6_XeDien_DuLich");
                tramRoot.transform.SetParent(transform, false);

                // Xe điện tại bến đỗ Quảng trường Đông Kinh Nghĩa Thục
                GameObject tram1 = VehicleBuilder.BuildElectricTourTram();
                tram1.transform.SetParent(tramRoot.transform, false);
                tram1.transform.position = new Vector3(615.0f, -0.49f, -618.0f);
                tram1.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }

            Debug.Log("<color=#00FFAA>[HoanKiemTrafficSystem] Đã tạo thành công toàn bộ hệ thống xe cộ đặc trưng Hà Nội quanh Hồ Gươm!</color>");
        }
    }
}
