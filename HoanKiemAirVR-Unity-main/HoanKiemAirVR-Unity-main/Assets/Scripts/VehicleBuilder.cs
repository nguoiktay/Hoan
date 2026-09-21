using System.Collections.Generic;
using UnityEngine;

namespace HoanKiemAirVR
{
    /// <summary>
    /// Bộ sinh mô hình 3D xe cộ đặc trưng của thủ đô Hà Nội quanh Hồ Gươm:
    /// - Xe buýt 2 tầng mui trần đỏ Hà Nội City Tour
    /// - Taxi Mai Linh, Taxi Xanh SM (VinFast), Taxi Group
    /// - Ô tô con Sedan & SUV hiện đại
    /// - Xe máy & Xe tay ga (Honda Wave, SH, Vespa, xe Grab)
    /// - Xe Xích lô du lịch phố cổ Hà Nội
    /// - Xe điện ngắm cảnh Bờ Hồ
    /// </summary>
    public static class VehicleBuilder
    {
        // Cache vật liệu để tiết kiệm bộ nhớ & Draw Calls
        private static Material matRedBus;
        private static Material matBusUpper;
        private static Material matTaxiMaiLinh;
        private static Material matTaxiXanhSM;
        private static Material matTaxiWhite;
        private static Material matCarWhite;
        private static Material matCarBlack;
        private static Material matCarSilver;
        private static Material matCarRed;
        private static Material matWheel;
        private static Material matWheelRim;
        private static Material matGlass;
        private static Material matLightYellow;
        private static Material matLightRed;
        private static Material matMotorbikeMetal;
        private static Material matSeatLeather;
        private static Material matGrabGreen;

        private static void EnsureMaterials()
        {
            if (matRedBus != null) return;

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");

            matRedBus = CreateMat(urpLit, "MAT_BusRed", new Color(0.82f, 0.08f, 0.12f, 1f), 0.6f);
            matBusUpper = CreateMat(urpLit, "MAT_BusUpper", new Color(0.95f, 0.95f, 0.92f, 1f), 0.5f);

            matTaxiMaiLinh = CreateMat(urpLit, "MAT_TaxiMaiLinh", new Color(0.00f, 0.58f, 0.23f, 1f), 0.7f);
            matTaxiXanhSM = CreateMat(urpLit, "MAT_TaxiXanhSM", new Color(0.00f, 0.64f, 0.65f, 1f), 0.75f);
            matTaxiWhite = CreateMat(urpLit, "MAT_TaxiWhite", new Color(0.96f, 0.96f, 0.96f, 1f), 0.7f);

            matCarWhite = CreateMat(urpLit, "MAT_CarWhite", new Color(0.95f, 0.95f, 0.95f, 1f), 0.8f);
            matCarBlack = CreateMat(urpLit, "MAT_CarBlack", new Color(0.05f, 0.05f, 0.06f, 1f), 0.85f);
            matCarSilver = CreateMat(urpLit, "MAT_CarSilver", new Color(0.75f, 0.76f, 0.78f, 1f), 0.8f);
            matCarRed = CreateMat(urpLit, "MAT_CarRed", new Color(0.65f, 0.05f, 0.08f, 1f), 0.8f);

            matWheel = CreateMat(urpLit, "MAT_WheelRubber", new Color(0.12f, 0.12f, 0.12f, 1f), 0.1f);
            matWheelRim = CreateMat(urpLit, "MAT_WheelRim", new Color(0.85f, 0.85f, 0.88f, 1f), 0.9f);

            matGlass = CreateMat(urpLit, "MAT_VehicleGlass", new Color(0.15f, 0.22f, 0.28f, 0.9f), 0.95f);
            matLightYellow = CreateMat(urpLit, "MAT_Headlight", new Color(1.0f, 0.95f, 0.6f, 1f), 0.9f);
            matLightRed = CreateMat(urpLit, "MAT_Taillight", new Color(0.9f, 0.05f, 0.05f, 1f), 0.8f);

            matMotorbikeMetal = CreateMat(urpLit, "MAT_MotoMetal", new Color(0.2f, 0.2f, 0.22f, 1f), 0.8f);
            matSeatLeather = CreateMat(urpLit, "MAT_SeatLeather", new Color(0.15f, 0.14f, 0.13f, 1f), 0.2f);
            matGrabGreen = CreateMat(urpLit, "MAT_GrabGreen", new Color(0.05f, 0.68f, 0.32f, 1f), 0.5f);
        }

        private static Material CreateMat(Shader s, string name, Color c, float smoothness)
        {
            Material m = new Material(s);
            m.name = name;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
            if (m.HasProperty("_Color")) m.SetColor("_Color", c);
            m.SetFloat("_Smoothness", smoothness);
            return m;
        }

        /// <summary>
        /// Tạo Xe buýt 2 tầng mui trần Hà Nội City Tour
        /// </summary>
        public static GameObject BuildDoubleDeckerBus()
        {
            EnsureMaterials();
            GameObject bus = new GameObject("XeBuyt_2Tang_HanoiCityTour");

            // Kích thước chuẩn: Dài 11.2m, Rộng 2.5m, Cao 4.0m
            // 1. Thân tầng 1 (Đỏ rực rỡ)
            GameObject lowerBody = CreateBox("LowerBody", bus.transform, new Vector3(0f, 1.25f, 0f), new Vector3(2.5f, 1.9f, 11.0f), matRedBus);

            // 2. Kính tầng 1 (Dải kính 2 bên hông)
            CreateBox("LowerGlass_L", bus.transform, new Vector3(-1.26f, 1.6f, 0.3f), new Vector3(0.04f, 0.9f, 8.5f), matGlass);
            CreateBox("LowerGlass_R", bus.transform, new Vector3(1.26f, 1.6f, 0.3f), new Vector3(0.04f, 0.9f, 8.5f), matGlass);

            // Kính chắn gió trước tầng 1
            CreateBox("Windshield_Front1", bus.transform, new Vector3(0f, 1.6f, 5.51f), new Vector3(2.3f, 1.0f, 0.04f), matGlass);

            // 3. Sàn tầng 2
            CreateBox("Floor_Deck2", bus.transform, new Vector3(0f, 2.25f, 0f), new Vector3(2.55f, 0.15f, 11.1f), matRedBus);

            // 4. Lan can boong mui trần tầng 2 (Thành kim loại + kính chắn gió trước)
            CreateBox("Rail_Left", bus.transform, new Vector3(-1.24f, 2.85f, -0.2f), new Vector3(0.08f, 1.05f, 9.8f), matBusUpper);
            CreateBox("Rail_Right", bus.transform, new Vector3(1.24f, 2.85f, -0.2f), new Vector3(0.08f, 1.05f, 9.8f), matBusUpper);
            CreateBox("Rail_Rear", bus.transform, new Vector3(0f, 2.85f, -5.2f), new Vector3(2.4f, 1.05f, 0.08f), matBusUpper);

            // Kính chắn gió tầng 2 phía trước
            CreateBox("Windshield_Front2", bus.transform, new Vector3(0f, 2.85f, 5.2f), new Vector3(2.4f, 1.05f, 0.08f), matGlass);

            // 5. Mái che một phần phía trước tầng 2 (Nửa trước có mái che mát, nửa sau mui trần ngắm cảnh)
            CreateBox("Canopy_FrontDeck2", bus.transform, new Vector3(0f, 3.75f, 2.5f), new Vector3(2.55f, 0.1f, 5.5f), matRedBus);

            // Cột chống mái tầng 2
            CreateBox("Pillar_L1", bus.transform, new Vector3(-1.2f, 3.0f, 5.1f), new Vector3(0.08f, 1.5f, 0.08f), matBusUpper);
            CreateBox("Pillar_R1", bus.transform, new Vector3(1.2f, 3.0f, 5.1f), new Vector3(0.08f, 1.5f, 0.08f), matBusUpper);
            CreateBox("Pillar_L2", bus.transform, new Vector3(-1.2f, 3.0f, -0.2f), new Vector3(0.08f, 1.5f, 0.08f), matBusUpper);
            CreateBox("Pillar_R2", bus.transform, new Vector3(1.2f, 3.0f, -0.2f), new Vector3(0.08f, 1.5f, 0.08f), matBusUpper);

            // 6. Các hàng ghế đỏ ngắm cảnh tầng 2
            for (int r = -4; r <= 3; r++)
            {
                float zPos = r * 1.05f;
                // Ghế đôi bên trái
                CreateBox($"Seat_L_{r}", bus.transform, new Vector3(-0.75f, 2.65f, zPos), new Vector3(0.85f, 0.65f, 0.45f), matRedBus);
                // Ghế đôi bên phải
                CreateBox($"Seat_R_{r}", bus.transform, new Vector3(0.75f, 2.65f, zPos), new Vector3(0.85f, 0.65f, 0.45f), matRedBus);
            }

            // 7. Bảng LED tên điểm đến phía trước: "HÀ NỘI CITY TOUR - HỒ GƯƠM"
            GameObject destBoard = CreateBox("DestinationSign", bus.transform, new Vector3(0f, 2.2f, 5.52f), new Vector3(1.8f, 0.35f, 0.05f), matTaxiMaiLinh);

            // 8. Đèn pha trước & Đèn hậu
            CreateBox("Headlight_L", bus.transform, new Vector3(-0.95f, 0.7f, 5.51f), new Vector3(0.35f, 0.25f, 0.05f), matLightYellow);
            CreateBox("Headlight_R", bus.transform, new Vector3(0.95f, 0.7f, 5.51f), new Vector3(0.35f, 0.25f, 0.05f), matLightYellow);
            CreateBox("Taillight_L", bus.transform, new Vector3(-0.95f, 0.7f, -5.51f), new Vector3(0.35f, 0.25f, 0.05f), matLightRed);
            CreateBox("Taillight_R", bus.transform, new Vector3(0.95f, 0.7f, -5.51f), new Vector3(0.35f, 0.25f, 0.05f), matLightRed);

            // 9. Bánh xe (6 bánh: 2 bánh trước, 4 bánh sau kép)
            CreateWheel("Wheel_FL", bus.transform, new Vector3(-1.25f, 0.5f, 3.8f), 0.5f, 0.35f);
            CreateWheel("Wheel_FR", bus.transform, new Vector3(1.25f, 0.5f, 3.8f), 0.5f, 0.35f);
            CreateWheel("Wheel_RL1", bus.transform, new Vector3(-1.25f, 0.5f, -3.2f), 0.5f, 0.35f);
            CreateWheel("Wheel_RR1", bus.transform, new Vector3(1.25f, 0.5f, -3.2f), 0.5f, 0.35f);
            CreateWheel("Wheel_RL2", bus.transform, new Vector3(-1.25f, 0.5f, -4.3f), 0.5f, 0.35f);
            CreateWheel("Wheel_RR2", bus.transform, new Vector3(1.25f, 0.5f, -4.3f), 0.5f, 0.35f);

            // Collider vật lý bao trùm xe
            var col = bus.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 2.0f, 0f);
            col.size = new Vector3(2.6f, 4.0f, 11.2f);

            return bus;
        }

        public enum TaxiType
        {
            MaiLinh,
            XanhSM,
            HanoiGroup
        }

        /// <summary>
        /// Tạo Xe Taxi Hà Nội (Mai Linh, Xanh SM VinFast, Hanoi Group)
        /// </summary>
        public static GameObject BuildTaxi(TaxiType type)
        {
            EnsureMaterials();

            string tName = (type == TaxiType.MaiLinh) ? "Taxi_MaiLinh" : (type == TaxiType.XanhSM ? "Taxi_XanhSM_VinFast" : "Taxi_HanoiGroup");
            GameObject taxi = new GameObject(tName);

            Material bodyMat = (type == TaxiType.MaiLinh) ? matTaxiMaiLinh : (type == TaxiType.XanhSM ? matTaxiXanhSM : matTaxiWhite);
            Material roofMat = (type == TaxiType.XanhSM) ? matCarBlack : matTaxiWhite;

            // Kích thước xe sedan: Dài 4.6m, Rộng 1.8m, Cao 1.5m
            // 1. Thân dưới
            CreateBox("LowerBody", taxi.transform, new Vector3(0f, 0.45f, 0f), new Vector3(1.8f, 0.55f, 4.6f), bodyMat);

            // 2. Cabin trên
            CreateBox("Cabin", taxi.transform, new Vector3(0f, 1.05f, -0.2f), new Vector3(1.55f, 0.65f, 2.3f), roofMat);

            // 3. Kính xe (Kính trước dốc, kính sau dốc, kính hông)
            CreateBox("Windshield_Front", taxi.transform, new Vector3(0f, 0.98f, 0.95f), new Vector3(1.45f, 0.55f, 0.2f), matGlass, new Vector3(-25f, 0f, 0f));
            CreateBox("Windshield_Rear", taxi.transform, new Vector3(0f, 0.98f, -1.35f), new Vector3(1.45f, 0.55f, 0.2f), matGlass, new Vector3(25f, 0f, 0f));
            CreateBox("Glass_SideL", taxi.transform, new Vector3(-0.79f, 1.05f, -0.2f), new Vector3(0.04f, 0.5f, 2.1f), matGlass);
            CreateBox("Glass_SideR", taxi.transform, new Vector3(0.79f, 1.05f, -0.2f), new Vector3(0.04f, 0.5f, 2.1f), matGlass);

            // 4. Mào Taxi trên nóc xe
            Material capMat = (type == TaxiType.MaiLinh) ? matCarWhite : matLightYellow;
            CreateBox("TaxiRoofSign", taxi.transform, new Vector3(0f, 1.45f, -0.2f), new Vector3(0.45f, 0.16f, 0.22f), capMat);

            // Dải sọc bên hông nếu là Hanoi Group
            if (type == TaxiType.HanoiGroup)
            {
                CreateBox("Stripe_L", taxi.transform, new Vector3(-0.91f, 0.48f, 0f), new Vector3(0.02f, 0.08f, 4.2f), matTaxiMaiLinh);
                CreateBox("Stripe_R", taxi.transform, new Vector3(0.91f, 0.48f, 0f), new Vector3(0.02f, 0.08f, 4.2f), matTaxiMaiLinh);
            }

            // 5. Đèn xe
            CreateBox("Headlight_L", taxi.transform, new Vector3(-0.65f, 0.5f, 2.31f), new Vector3(0.3f, 0.16f, 0.04f), matLightYellow);
            CreateBox("Headlight_R", taxi.transform, new Vector3(0.65f, 0.5f, 2.31f), new Vector3(0.3f, 0.16f, 0.04f), matLightYellow);
            CreateBox("Taillight_L", taxi.transform, new Vector3(-0.65f, 0.55f, -2.31f), new Vector3(0.3f, 0.16f, 0.04f), matLightRed);
            CreateBox("Taillight_R", taxi.transform, new Vector3(0.65f, 0.55f, -2.31f), new Vector3(0.3f, 0.16f, 0.04f), matLightRed);

            // 6. 4 Bánh xe
            CreateWheel("Wheel_FL", taxi.transform, new Vector3(-0.85f, 0.32f, 1.4f), 0.32f, 0.22f);
            CreateWheel("Wheel_FR", taxi.transform, new Vector3(0.85f, 0.32f, 1.4f), 0.32f, 0.22f);
            CreateWheel("Wheel_RL", taxi.transform, new Vector3(-0.85f, 0.32f, -1.4f), 0.32f, 0.22f);
            CreateWheel("Wheel_RR", taxi.transform, new Vector3(0.85f, 0.32f, -1.4f), 0.32f, 0.22f);

            var col = taxi.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.75f, 0f);
            col.size = new Vector3(1.85f, 1.5f, 4.65f);

            return taxi;
        }

        public enum CarColor
        {
            White,
            Black,
            Silver,
            Red
        }

        /// <summary>
        /// Tạo xe ô tô con dân dụng (Sedan / SUV)
        /// </summary>
        public static GameObject BuildCivilianCar(CarColor color, bool isSUV = false)
        {
            EnsureMaterials();

            Material bodyMat = matCarWhite;
            if (color == CarColor.Black) bodyMat = matCarBlack;
            else if (color == CarColor.Silver) bodyMat = matCarSilver;
            else if (color == CarColor.Red) bodyMat = matCarRed;

            string cName = isSUV ? $"Car_SUV_{color}" : $"Car_Sedan_{color}";
            GameObject car = new GameObject(cName);

            float width = isSUV ? 1.9f : 1.8f;
            float length = isSUV ? 4.7f : 4.5f;
            float height = isSUV ? 1.7f : 1.45f;
            float wheelRadius = isSUV ? 0.38f : 0.32f;

            // Thân xe
            CreateBox("LowerBody", car.transform, new Vector3(0f, 0.5f, 0f), new Vector3(width, 0.6f, length), bodyMat);

            // Cabin
            float cabinLen = isSUV ? 2.9f : 2.3f;
            float cabinZ = isSUV ? -0.1f : -0.2f;
            CreateBox("Cabin", car.transform, new Vector3(0f, 1.1f, cabinZ), new Vector3(width * 0.86f, 0.65f, cabinLen), bodyMat);

            // Kính xe
            CreateBox("Windshield_Front", car.transform, new Vector3(0f, 1.05f, cabinZ + cabinLen * 0.5f + 0.05f), new Vector3(width * 0.8f, 0.55f, 0.2f), matGlass, new Vector3(-25f, 0f, 0f));
            CreateBox("Windshield_Rear", car.transform, new Vector3(0f, 1.05f, cabinZ - cabinLen * 0.5f - 0.05f), new Vector3(width * 0.8f, 0.55f, 0.2f), matGlass, new Vector3(25f, 0f, 0f));

            // Đèn
            CreateBox("Headlight_L", car.transform, new Vector3(-width * 0.38f, 0.55f, length * 0.5f + 0.01f), new Vector3(0.3f, 0.16f, 0.04f), matLightYellow);
            CreateBox("Headlight_R", car.transform, new Vector3(width * 0.38f, 0.55f, length * 0.5f + 0.01f), new Vector3(0.3f, 0.16f, 0.04f), matLightYellow);
            CreateBox("Taillight_L", car.transform, new Vector3(-width * 0.38f, 0.6f, -length * 0.5f - 0.01f), new Vector3(0.3f, 0.16f, 0.04f), matLightRed);
            CreateBox("Taillight_R", car.transform, new Vector3(width * 0.38f, 0.6f, -length * 0.5f - 0.01f), new Vector3(0.3f, 0.16f, 0.04f), matLightRed);

            // 4 Bánh xe
            float axleZ = length * 0.31f;
            CreateWheel("Wheel_FL", car.transform, new Vector3(-width * 0.48f, wheelRadius, axleZ), wheelRadius, 0.22f);
            CreateWheel("Wheel_FR", car.transform, new Vector3(width * 0.48f, wheelRadius, axleZ), wheelRadius, 0.22f);
            CreateWheel("Wheel_RL", car.transform, new Vector3(-width * 0.48f, wheelRadius, -axleZ), wheelRadius, 0.22f);
            CreateWheel("Wheel_RR", car.transform, new Vector3(width * 0.48f, wheelRadius, -axleZ), wheelRadius, 0.22f);

            var col = car.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, height * 0.5f, 0f);
            col.size = new Vector3(width + 0.05f, height, length + 0.05f);

            return car;
        }

        public enum MotorbikeType
        {
            WaveStandard, // Honda Wave / xe số
            SHScooter,    // Honda SH / xe ga
            GrabDelivery  // Xe công nghệ Grab
        }

        /// <summary>
        /// Tạo xe máy / xe tay ga đặc trưng của Hà Nội
        /// </summary>
        public static GameObject BuildMotorbike(MotorbikeType type)
        {
            EnsureMaterials();
            string mName = (type == MotorbikeType.WaveStandard) ? "XeMay_HondaWave" : (type == MotorbikeType.SHScooter ? "XeGa_HondaSH" : "XeMay_GrabBike");
            GameObject bike = new GameObject(mName);

            Material bodyMat = (type == MotorbikeType.GrabDelivery) ? matGrabGreen : (type == MotorbikeType.SHScooter ? matCarBlack : matCarRed);

            // 1. Khung xe & Thân áo
            CreateBox("Chassis", bike.transform, new Vector3(0f, 0.45f, 0f), new Vector3(0.35f, 0.45f, 1.2f), bodyMat);

            // 2. Yên xe bọc da đen
            CreateBox("Seat", bike.transform, new Vector3(0f, 0.72f, -0.15f), new Vector3(0.32f, 0.12f, 0.75f), matSeatLeather);

            // 3. Đầu xe & Ghi đông tay lái
            CreateBox("Handlebar", bike.transform, new Vector3(0f, 0.95f, 0.55f), new Vector3(0.7f, 0.06f, 0.06f), matMotorbikeMetal);
            CreateBox("Headlight", bike.transform, new Vector3(0f, 0.95f, 0.62f), new Vector3(0.2f, 0.15f, 0.08f), matLightYellow);

            // 2 Gương chiếu hậu
            CreateBox("Mirror_L", bike.transform, new Vector3(-0.35f, 1.15f, 0.55f), new Vector3(0.1f, 0.12f, 0.02f), matMotorbikeMetal);
            CreateBox("Mirror_R", bike.transform, new Vector3(0.35f, 1.15f, 0.55f), new Vector3(0.1f, 0.12f, 0.02f), matMotorbikeMetal);

            // 4. Ống xả (Pô xe) bên phải
            CreateBox("Exhaust", bike.transform, new Vector3(0.22f, 0.28f, -0.4f), new Vector3(0.08f, 0.08f, 0.65f), matWheelRim);

            // 5. Thùng hàng Grab nếu là xe Grab
            if (type == MotorbikeType.GrabDelivery)
            {
                CreateBox("GrabBox", bike.transform, new Vector3(0f, 0.95f, -0.6f), new Vector3(0.45f, 0.45f, 0.45f), matGrabGreen);
            }
            else
            {
                // Mũ bảo hiểm treo trên tay lái
                CreateBox("Helmet", bike.transform, new Vector3(-0.25f, 0.85f, 0.55f), new Vector3(0.22f, 0.2f, 0.22f), matCarRed);
            }

            // 6. 2 Bánh xe nan hoa
            CreateWheel("Wheel_Front", bike.transform, new Vector3(0f, 0.3f, 0.75f), 0.3f, 0.08f);
            CreateWheel("Wheel_Rear", bike.transform, new Vector3(0f, 0.3f, -0.65f), 0.3f, 0.08f);

            // Chân chống nghiêng nhẹ
            bike.transform.localRotation = Quaternion.Euler(0f, 0f, -4f);

            var col = bike.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.6f, 0f);
            col.size = new Vector3(0.7f, 1.2f, 1.9f);

            return bike;
        }

        /// <summary>
        /// Tạo một hàng xe máy dựng nghiêng trên vỉa hè theo đúng phong cách phố cổ
        /// </summary>
        public static GameObject BuildParkedMotorbikeRow(int count, float spacing = 0.85f)
        {
            GameObject row = new GameObject($"HangXeMay_ViaHe_{count}Chiec");
            for (int i = 0; i < count; i++)
            {
                MotorbikeType t = (i % 3 == 0) ? MotorbikeType.WaveStandard : (i % 3 == 1 ? MotorbikeType.SHScooter : MotorbikeType.GrabDelivery);
                GameObject bike = BuildMotorbike(t);
                bike.transform.SetParent(row.transform, false);
                bike.transform.localPosition = new Vector3(i * spacing, 0f, 0f);
                bike.transform.localRotation = Quaternion.Euler(0f, 45f, -4f); // Nghiêng góc 45 độ so với vỉa hè
            }
            return row;
        }

        /// <summary>
        /// Tạo xe xích lô du lịch phố cổ
        /// </summary>
        public static GameObject BuildCyclo()
        {
            EnsureMaterials();
            GameObject cyclo = new GameObject("XeXichLo_PhoCo");

            // 1. Khoang ghế ngồi phía trước cho du khách
            CreateBox("PassengerSeat", cyclo.transform, new Vector3(0f, 0.55f, 0.4f), new Vector3(0.75f, 0.45f, 0.65f), matSeatLeather);
            CreateBox("FootRest", cyclo.transform, new Vector3(0f, 0.25f, 0.8f), new Vector3(0.65f, 0.05f, 0.4f), matMotorbikeMetal);

            // 2. Mái che bạt xích lô gập
            CreateBox("Canopy", cyclo.transform, new Vector3(0f, 1.15f, 0.3f), new Vector3(0.85f, 0.35f, 0.7f), matCarWhite);
            CreateBox("Frame_L", cyclo.transform, new Vector3(-0.4f, 0.85f, 0.3f), new Vector3(0.04f, 0.65f, 0.04f), matWheelRim);
            CreateBox("Frame_R", cyclo.transform, new Vector3(0.4f, 0.85f, 0.3f), new Vector3(0.04f, 0.65f, 0.04f), matWheelRim);

            // 3. Khung xe đạp người đạp phía sau
            CreateBox("DriverSeat", cyclo.transform, new Vector3(0f, 0.95f, -0.65f), new Vector3(0.22f, 0.1f, 0.25f), matSeatLeather);
            CreateBox("Handlebars", cyclo.transform, new Vector3(0f, 1.1f, -0.3f), new Vector3(0.6f, 0.04f, 0.04f), matMotorbikeMetal);
            CreateBox("FrameBar", cyclo.transform, new Vector3(0f, 0.6f, -0.4f), new Vector3(0.05f, 0.05f, 0.8f), matMotorbikeMetal);

            // 4. Bánh xe: 2 bánh trước to, 1 bánh sau
            CreateWheel("Wheel_FrontL", cyclo.transform, new Vector3(-0.45f, 0.45f, 0.45f), 0.45f, 0.06f);
            CreateWheel("Wheel_FrontR", cyclo.transform, new Vector3(0.45f, 0.45f, 0.45f), 0.45f, 0.06f);
            CreateWheel("Wheel_Rear", cyclo.transform, new Vector3(0f, 0.35f, -0.95f), 0.35f, 0.06f);

            var col = cyclo.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.75f, 0f);
            col.size = new Vector3(1.0f, 1.5f, 2.4f);

            return cyclo;
        }

        /// <summary>
        /// Tạo xe điện ngắm cảnh Bờ Hồ (Hanoi Eco Tour Tram)
        /// </summary>
        public static GameObject BuildElectricTourTram()
        {
            EnsureMaterials();
            GameObject tram = new GameObject("XeDien_DuLich_BoHo");

            // Kích thước: Dài 4.8m, Rộng 1.5m, Cao 1.95m
            // 1. Thân dưới màu xanh ngọc / trắng
            CreateBox("Floor", tram.transform, new Vector3(0f, 0.4f, 0f), new Vector3(1.5f, 0.35f, 4.8f), matTaxiXanhSM);

            // 2. Mái che bằng phẳng trên cao
            CreateBox("Roof", tram.transform, new Vector3(0f, 1.85f, 0f), new Vector3(1.55f, 0.1f, 4.9f), matCarWhite);

            // Cột chống mái
            for (int i = -2; i <= 2; i++)
            {
                float z = i * 1.1f;
                CreateBox($"Pillar_L_{i}", tram.transform, new Vector3(-0.72f, 1.15f, z), new Vector3(0.05f, 1.35f, 0.05f), matWheelRim);
                CreateBox($"Pillar_R_{i}", tram.transform, new Vector3(0.72f, 1.15f, z), new Vector3(0.05f, 1.35f, 0.05f), matWheelRim);
            }

            // 3. Các hàng ghế mở không cửa
            for (int i = -1; i <= 2; i++)
            {
                float z = i * 0.95f - 0.2f;
                CreateBox($"Bench_{i}", tram.transform, new Vector3(0f, 0.75f, z), new Vector3(1.35f, 0.4f, 0.35f), matSeatLeather);
            }

            // Vô lăng phía trước
            CreateBox("Steering", tram.transform, new Vector3(-0.35f, 0.85f, 1.8f), new Vector3(0.35f, 0.35f, 0.04f), matWheel, new Vector3(30f, 0f, 0f));
            CreateBox("Windshield", tram.transform, new Vector3(0f, 1.25f, 2.3f), new Vector3(1.4f, 0.8f, 0.05f), matGlass);

            // 4. Bánh xe nhỏ
            CreateWheel("Wheel_FL", tram.transform, new Vector3(-0.7f, 0.28f, 1.6f), 0.28f, 0.18f);
            CreateWheel("Wheel_FR", tram.transform, new Vector3(0.7f, 0.28f, 1.6f), 0.28f, 0.18f);
            CreateWheel("Wheel_RL", tram.transform, new Vector3(-0.7f, 0.28f, -1.6f), 0.28f, 0.18f);
            CreateWheel("Wheel_RR", tram.transform, new Vector3(0.7f, 0.28f, -1.6f), 0.28f, 0.18f);

            var col = tram.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 1.0f, 0f);
            col.size = new Vector3(1.6f, 2.0f, 4.9f);

            return tram;
        }

        // ================= HÀM TIỆN ÍCH TẠO KHỐI & BÁNH XE =================

        private static GameObject CreateBox(string name, Transform parent, Vector3 localPos, Vector3 size, Material mat, Vector3? rotEuler = null)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent, false);
            box.transform.localPosition = localPos;
            box.transform.localScale = size;
            if (rotEuler.HasValue) box.transform.localRotation = Quaternion.Euler(rotEuler.Value);

            // Bỏ collider trên từng chi tiết nhỏ, dùng BoxCollider tổng trên root
            Collider c = box.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);

            MeshRenderer mr = box.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;

            return box;
        }

        private static GameObject CreateWheel(string name, Transform parent, Vector3 localPos, float radius, float thickness)
        {
            GameObject wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = name;
            wheel.transform.SetParent(parent, false);
            wheel.transform.localPosition = localPos;
            wheel.transform.localScale = new Vector3(radius * 2f, thickness * 0.5f, radius * 2f);
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); // Xoay ngang thành bánh xe

            Collider c = wheel.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);

            MeshRenderer mr = wheel.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = matWheel;

            // Vành mâm xe kim loại nhỏ bên trong
            GameObject hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "RimHub";
            hub.transform.SetParent(wheel.transform, false);
            hub.transform.localPosition = Vector3.zero;
            hub.transform.localScale = new Vector3(0.55f, 1.05f, 0.55f);
            hub.transform.localRotation = Quaternion.identity;

            Collider hc = hub.GetComponent<Collider>();
            if (hc != null) Object.DestroyImmediate(hc);

            MeshRenderer hmr = hub.GetComponent<MeshRenderer>();
            if (hmr != null) hmr.sharedMaterial = matWheelRim;

            return wheel;
        }
    }
}
