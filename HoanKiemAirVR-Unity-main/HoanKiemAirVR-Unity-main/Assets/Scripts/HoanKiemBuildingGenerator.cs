using System.Collections.Generic;
using UnityEngine;

namespace HoanKiemAirVR
{
    /// <summary>
    /// Bộ Tạo Dãy Phố & Kiến Trúc Nhà Cửa Quanh Hồ Gươm (Hanoi Urban Street Architecture):
    /// - Bờ Bắc: Tòa nhà Hàm Cá Mập (Shark Jaw Building) & Phố Cổ Đông Kinh Nghĩa Thục
    /// - Bờ Tây: Tòa soạn Báo Hà Nội Mới (với bảng tin kính đỏ), Apricot Hotel & Biệt thự Pháp cổ Lê Thái Tổ
    /// - Bờ Nam: Dãy nhà phố cổ Hàng Khay (phố tranh, tiệm kem, quán cà phê mái hiên)
    /// - Bờ Đông: Dãy phố Đinh Tiên Hoàng (kết nối liền mạch với Tòa nhà VNPT / Bưu điện Hà Nội)
    /// - Chi tiết chân thực: Bồn nước inox Sơn Hà trên mái, cục nóng điều hòa, ban công hoa, mái hiên di động
    /// </summary>
    public static class HoanKiemBuildingGenerator
    {
        private static Material matFrenchYellow;
        private static Material matColonialCream;
        private static Material matWhiteNeoclassical;
        private static Material matOldRoofTile;
        private static Material matGreenShutter;
        private static Material matDarkWindow;
        private static Material matAwningStripe;
        private static Material matRedNoticeBoard;
        private static Material matInoxTank;
        private static Material matSignboard;

        private static void EnsureMaterials()
        {
            if (matFrenchYellow != null) return;

            Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) urpLit = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (urpLit == null) urpLit = Shader.Find("Standard");

            // Vàng nghệ Pháp cổ đặc trưng của Hà Nội (màu Báo Hà Nội Mới, Nhà hát Lớn, dinh thự Pháp)
            matFrenchYellow = CreateMat(urpLit, "MAT_FrenchYellow", new Color(0.88f, 0.68f, 0.22f, 1f), 0.25f);

            // Màu kem cổ điển
            matColonialCream = CreateMat(urpLit, "MAT_ColonialCream", new Color(0.85f, 0.82f, 0.74f, 1f), 0.25f);

            // Màu trắng tân cổ điển (Apricot Hotel, Tràng Tiền)
            matWhiteNeoclassical = CreateMat(urpLit, "MAT_WhiteNeoclassical", new Color(0.95f, 0.94f, 0.92f, 1f), 0.4f);

            // Mái ngói đỏ rêu phong
            matOldRoofTile = CreateMat(urpLit, "MAT_OldRoofTile", new Color(0.55f, 0.22f, 0.16f, 1f), 0.15f);

            // Cửa chớp gỗ xanh lá cây Pháp cổ
            matGreenShutter = CreateMat(urpLit, "MAT_GreenShutter", new Color(0.12f, 0.35f, 0.22f, 1f), 0.35f);

            // Kính cửa sổ phản chiếu
            matDarkWindow = CreateMat(urpLit, "MAT_DarkWindow", new Color(0.12f, 0.18f, 0.24f, 0.9f), 0.9f);

            // Bạt mái hiên di động sọc xanh trắng
            matAwningStripe = CreateMat(urpLit, "MAT_AwningStripe", new Color(0.15f, 0.45f, 0.75f, 1f), 0.2f);

            // Bảng tin Báo Hà Nội Mới viền đỏ rực rỡ
            matRedNoticeBoard = CreateMat(urpLit, "MAT_RedNoticeBoard", new Color(0.78f, 0.08f, 0.08f, 1f), 0.6f);

            // Bồn nước inox Sơn Hà kim loại sáng bóng
            matInoxTank = CreateMat(urpLit, "MAT_InoxTank", new Color(0.82f, 0.84f, 0.86f, 1f), 0.95f);

            // Biển hiệu cửa hàng phố cổ
            matSignboard = CreateMat(urpLit, "MAT_Signboard", new Color(0.18f, 0.14f, 0.12f, 1f), 0.4f);
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
        /// Tạo toàn bộ các dãy phố và công trình bao quanh 4 mặt Hồ Gươm
        /// </summary>
        public static GameObject GenerateAllStreetBuildings()
        {
            EnsureMaterials();

            GameObject root = new GameObject("[HoanKiem_StreetBuildings]");

            // 1. BỜ BẮC: Tòa nhà Hàm Cá Mập & Phố Cổ Đông Kinh Nghĩa Thục
            GameObject northRoot = new GameObject("1_BoBac_DongKinhNghiaThuc");
            northRoot.transform.SetParent(root.transform, false);
            BuildHamCaMapBuilding(northRoot.transform);
            BuildDongKinhNghiaThucShophouses(northRoot.transform);

            // 2. BỜ TÂY: Tòa soạn Báo Hà Nội Mới, Apricot Hotel & Dãy Phố Lê Thái Tổ
            GameObject westRoot = new GameObject("2_BoTay_LeThaiTo");
            westRoot.transform.SetParent(root.transform, false);
            BuildBaoHanoiMoiBuilding(westRoot.transform);
            BuildApricotHotelBuilding(westRoot.transform);
            BuildLeThaiToShophouseBlocks(westRoot.transform);

            // 3. BỜ NAM: Dãy phố cổ Hàng Khay (Phố tranh, kem Tràng Tiền & cửa hàng lưu niệm)
            GameObject southRoot = new GameObject("3_BoNam_HangKhay");
            southRoot.transform.SetParent(root.transform, false);
            BuildHangKhayShophouseRow(southRoot.transform);

            // 4. BỜ ĐÔNG: Dãy phố Đinh Tiên Hoàng (flanking Tòa nhà VNPT)
            GameObject eastRoot = new GameObject("4_BoDong_DinhTienHoang");
            eastRoot.transform.SetParent(root.transform, false);
            BuildDinhTienHoangStreetBlocks(eastRoot.transform);

            return root;
        }

        // ================= 1. BỜ BẮC: TÒA NHÀ HÀM CÁ MẬP & ĐÔNG KINH NGHĨA THỤC =================

        /// <summary>
        /// Tòa nhà Hàm Cá Mập (Shark Jaw Building / City View Complex)
        /// Kiến trúc vòng cung nhiều tầng nhìn trực diện ra toàn cảnh Hồ Gươm, ban công lượn sóng
        /// Đặt ở bờ Bắc, mặt tiền quay về hướng Nam (-Z) nhìn thẳng ra quảng trường và hồ
        /// </summary>
        private static void BuildHamCaMapBuilding(Transform parent)
        {
            GameObject bldg = new GameObject("ToaNha_HamCaMap_CityView");
            bldg.transform.SetParent(parent, false);
            // Tọa độ tại Quảng trường Đông Kinh Nghĩa Thục (Mặt tiền quay về phía Nam -Z ngắm toàn cảnh hồ)
            bldg.transform.position = new Vector3(610.0f, -0.49f, -574.0f);
            bldg.transform.rotation = Quaternion.Euler(0f, 0f, 0f); // rotY = 0 để local -Z quay về hướng Nam (-Z trong world space)

            // Khối nhà 5 tầng: Rộng 32m, Sâu 22m, Cao 21m
            // Tầng 1: Sảnh thương mại & cửa hàng (Highlands Coffee / The Coffee Club)
            CreateBlock("Floor1_Retail", bldg.transform, new Vector3(0f, 2.25f, 0f), new Vector3(32f, 4.5f, 20f), matColonialCream);

            // Cửa kính lớn tầng 1
            CreateBlock("GlassDoors_F1", bldg.transform, new Vector3(0f, 1.8f, -10.1f), new Vector3(26f, 3.2f, 0.1f), matDarkWindow);

            // Mái hiên sảnh tầng 1
            CreateBlock("Canopy_F1", bldg.transform, new Vector3(0f, 4.2f, -11.0f), new Vector3(28f, 0.3f, 3.0f), matAwningStripe);

            // Tầng 2, 3, 4: Khối nhà với ban công vòng cung uốn lượn đặc trưng "Hàm Cá Mập"
            for (int f = 2; f <= 4; f++)
            {
                float y = (f - 1) * 4.2f + 2.25f;
                float balconyZ = -10.5f - (f - 2) * 0.4f;

                // Thân tầng
                CreateBlock($"Floor_{f}_Body", bldg.transform, new Vector3(0f, y, 0f), new Vector3(30f, 3.8f, 19f), matColonialCream);

                // Dải cửa sổ kính panorama nhìn ra hồ
                CreateBlock($"Glass_F{f}", bldg.transform, new Vector3(0f, y, -9.6f), new Vector3(26f, 2.4f, 0.1f), matDarkWindow);

                // Ban công uốn lượn nhô ra ngoài
                CreateBlock($"Balcony_F{f}", bldg.transform, new Vector3(0f, y - 1.2f, balconyZ), new Vector3(28f, 1.1f, 2.2f), matWhiteNeoclassical);

                // Lan can ban công
                CreateBlock($"Rail_F{f}", bldg.transform, new Vector3(0f, y - 0.4f, balconyZ - 1.0f), new Vector3(28f, 0.8f, 0.1f), matGreenShutter);
            }

            // Tầng 5 & Rooftop: Quán cafe ngắm cảnh trên cao có dù che nắng
            float topY = 4 * 4.2f + 2.25f; // ~19m
            CreateBlock("Floor5_Rooftop", bldg.transform, new Vector3(0f, topY, 2.0f), new Vector3(26f, 3.5f, 15f), matColonialCream);

            // Lan can sân thượng lộ thiên ngắm cảnh
            CreateBlock("Rooftop_Railing", bldg.transform, new Vector3(0f, topY + 0.6f, -7.5f), new Vector3(27f, 1.1f, 0.15f), matWhiteNeoclassical);

            // Biển hiệu lớn trên nóc "CITY VIEW - HIGHLANDS COFFEE"
            CreateBlock("NeonSign_CityView", bldg.transform, new Vector3(0f, topY + 4.2f, -5.0f), new Vector3(16f, 1.6f, 0.3f), matRedNoticeBoard);

            // Dù che nắng ngoài trời trên ban công tầng thượng
            CreateParasol("CafeParasol_1", bldg.transform, new Vector3(-8f, topY, -5.0f));
            CreateParasol("CafeParasol_2", bldg.transform, new Vector3(0f, topY, -5.5f));
            CreateParasol("CafeParasol_3", bldg.transform, new Vector3(8f, topY, -5.0f));

            // Bồn nước Inox Sơn Hà trên mái
            CreateInoxTank("WaterTank_1", bldg.transform, new Vector3(-9f, topY + 3.8f, 5f));
            CreateInoxTank("WaterTank_2", bldg.transform, new Vector3(9f, topY + 3.8f, 5f));

            // Collider
            var col = bldg.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 10.5f, 0f);
            col.size = new Vector3(33f, 22f, 24f);
        }

        /// <summary>
        /// Dãy nhà phố cổ bao quanh Quảng trường Đông Kinh Nghĩa Thục (Phố Hàng Gai & Cầu Gỗ)
        /// Mặt tiền quay về hướng Nam (-Z) nhìn ra quảng trường và hồ
        /// </summary>
        private static void BuildDongKinhNghiaThucShophouses(Transform parent)
        {
            // Dãy phố phía Tây quảng trường (Giao Hàng Gai)
            for (int i = 0; i < 4; i++)
            {
                float x = 575.0f - i * 9.0f;
                float z = -574.0f;
                BuildSingleShophouse(parent, $"Shophouse_HangGai_{i}", new Vector3(x, -0.49f, z), 180f, 8.5f, 15.0f, 3 + (i % 2), i % 2 == 0 ? matFrenchYellow : matColonialCream);
            }

            // Dãy phố phía Đông quảng trường (Giao Cầu Gỗ)
            for (int i = 0; i < 4; i++)
            {
                float x = 645.0f + i * 9.0f;
                float z = -574.0f;
                BuildSingleShophouse(parent, $"Shophouse_CauGo_{i}", new Vector3(x, -0.49f, z), 180f, 8.5f, 15.0f, 3 + ((i + 1) % 2), i % 2 == 1 ? matFrenchYellow : matColonialCream);
            }
        }

        // ================= 2. BỜ TÂY: BÁO HÀ NỘI MỚI, APRICOT HOTEL & LÊ THÁI TỔ =================

        /// <summary>
        /// Tòa soạn Báo Hà Nội Mới (Số 44 Lê Thái Tổ)
        /// Kiến trúc Pháp cổ màu vàng nghệ, cửa chớp xanh, bảng tin kính viền đỏ nổi tiếng
        /// Mặt tiền hướng về phía Đông (+X) nhìn thẳng ra đường Lê Thái Tổ và Hồ Gươm
        /// </summary>
        private static void BuildBaoHanoiMoiBuilding(Transform parent)
        {
            GameObject bldg = new GameObject("ToaSoan_BaoHanoiMoi_44LeThaiTo");
            bldg.transform.SetParent(parent, false);
            // Tọa độ bờ Tây: Mặt tiền (local -Z) quay về phía Đông (+X) sát vỉa hè X = 533
            bldg.transform.position = new Vector3(525.0f, -0.49f, -710.0f);
            bldg.transform.rotation = Quaternion.Euler(0f, 270f, 0f); // -90 deg: local -Z -> world +X

            // Tòa nhà 3 tầng kiến trúc Pháp cổ: Rộng 26m, Sâu 16m, Cao 14m
            // 1. Thân tòa nhà màu vàng nghệ
            CreateBlock("MainBody", bldg.transform, new Vector3(0f, 6.5f, 0f), new Vector3(26f, 13f, 16f), matFrenchYellow);

            // 2. Mái ngói dốc kiểu Pháp cổ
            GameObject roof = CreateBlock("MansardRoof", bldg.transform, new Vector3(0f, 14.2f, 0f), new Vector3(25.5f, 3.0f, 15.5f), matOldRoofTile);

            // 3. Sảnh đón chính có mái che cột trụ cổ điển
            CreateBlock("Porch_Roof", bldg.transform, new Vector3(0f, 4.5f, -8.8f), new Vector3(6.5f, 0.4f, 2.5f), matFrenchYellow);
            CreateBlock("Porch_Column_L", bldg.transform, new Vector3(-2.8f, 2.2f, -9.5f), new Vector3(0.5f, 4.4f, 0.5f), matColonialCream);
            CreateBlock("Porch_Column_R", bldg.transform, new Vector3(2.8f, 2.2f, -9.5f), new Vector3(0.5f, 4.4f, 0.5f), matColonialCream);

            // Cửa chính gỗ nâu sẫm
            CreateBlock("MainDoor", bldg.transform, new Vector3(0f, 2.0f, -8.05f), new Vector3(3.2f, 3.8f, 0.1f), matGreenShutter);

            // 4. Các ô cửa sổ Pháp cổ 2 tầng trên (Viền trắng, chớp xanh)
            for (int f = 1; f <= 2; f++)
            {
                float y = f * 4.2f + 2.0f;
                for (int w = -3; w <= 3; w++)
                {
                    if (w == 0 && f == 1) continue; // Chừa lối sảnh chính
                    float x = w * 3.5f;

                    // Khung cửa viền trắng
                    CreateBlock($"WindowFrame_{f}_{w}", bldg.transform, new Vector3(x, y, -8.05f), new Vector3(1.8f, 2.6f, 0.1f), matColonialCream);
                    // Kính bên trong
                    CreateBlock($"WindowGlass_{f}_{w}", bldg.transform, new Vector3(x, y, -8.07f), new Vector3(1.3f, 2.1f, 0.05f), matDarkWindow);
                    // Cửa chớp xanh 2 bên
                    CreateBlock($"Shutter_L_{f}_{w}", bldg.transform, new Vector3(x - 0.9f, y, -8.08f), new Vector3(0.55f, 2.2f, 0.04f), matGreenShutter);
                    CreateBlock($"Shutter_R_{f}_{w}", bldg.transform, new Vector3(x + 0.9f, y, -8.08f), new Vector3(0.55f, 2.2f, 0.04f), matGreenShutter);
                }
            }

            // 5. BẢNG TIN BÁO HÀ NỘI MỚI NỔI TIẾNG (Đặt ngay trên vỉa hè trước cửa số 44)
            // Khung kính viền đỏ đặc trưng, bên trong dán các trang báo in hàng ngày
            GameObject noticeBoard = CreateBlock("BangTin_BaoHanoiMoi", bldg.transform, new Vector3(6.5f, 1.8f, -8.12f), new Vector3(4.8f, 2.2f, 0.15f), matRedNoticeBoard);

            // Mái che nhỏ trên bảng tin
            CreateBlock("NoticeBoard_Canopy", bldg.transform, new Vector3(6.5f, 3.0f, -8.35f), new Vector3(5.2f, 0.15f, 0.55f), matGreenShutter);

            // Chữ "BÁO HÀ NỘI MỚI" bằng vạch vàng trên bảng tin
            CreateBlock("Sign_HanoiMoi", bldg.transform, new Vector3(6.5f, 2.7f, -8.22f), new Vector3(3.6f, 0.35f, 0.05f), matFrenchYellow);

            // Bồn hoa & cây cảnh nhỏ ven tường
            CreateBlock("FlowerPlanter", bldg.transform, new Vector3(0f, 0.35f, -8.6f), new Vector3(18f, 0.7f, 0.9f), matGreenShutter);

            // Collider
            var col = bldg.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 7.5f, 0f);
            col.size = new Vector3(27f, 16f, 18f);
        }

        /// <summary>
        /// Khách sạn Apricot Hotel & Biệt thự Pháp cổ (Lê Thái Tổ)
        /// Kiến trúc tân cổ điển hoàng gia màu trắng kem sang trọng
        /// Mặt tiền hướng về phía Đông (+X) nhìn thẳng ra đường Lê Thái Tổ và Hồ Gươm
        /// </summary>
        private static void BuildApricotHotelBuilding(Transform parent)
        {
            GameObject hotel = new GameObject("KhachSan_Apricot_Hotel");
            hotel.transform.SetParent(parent, false);
            // Tâm đặt tại 523, depth 20m -> Mặt tiền ở 523 + 10 = 533.0 sát vỉa hè
            hotel.transform.position = new Vector3(523.0f, -0.49f, -780.0f);
            hotel.transform.rotation = Quaternion.Euler(0f, 270f, 0f); // -90 deg: local -Z -> world +X

            // Khối khách sạn 5 tầng: Rộng 36m, Sâu 20m, Cao 22m
            CreateBlock("MainBody", hotel.transform, new Vector3(0f, 11f, 0f), new Vector3(36f, 22f, 20f), matWhiteNeoclassical);

            // Sảnh lớn có mái vòm kính & cột trụ tân cổ điển
            CreateBlock("GrandCanopy", hotel.transform, new Vector3(0f, 4.5f, -11.5f), new Vector3(10f, 0.5f, 4.0f), matDarkWindow);
            CreateBlock("Pillar_L1", hotel.transform, new Vector3(-4.5f, 2.2f, -12.5f), new Vector3(0.7f, 4.4f, 0.7f), matWhiteNeoclassical);
            CreateBlock("Pillar_R1", hotel.transform, new Vector3(4.5f, 2.2f, -12.5f), new Vector3(0.7f, 4.4f, 0.7f), matWhiteNeoclassical);

            // Biển hiệu khách sạn "APRICOT HOTEL"
            CreateBlock("HotelSign", hotel.transform, new Vector3(0f, 5.2f, -10.1f), new Vector3(7.5f, 1.0f, 0.2f), matSignboard);

            // Ban công sắt mỹ thuật các tầng
            for (int f = 2; f <= 5; f++)
            {
                float y = (f - 1) * 4.0f + 3.0f;
                for (int w = -3; w <= 3; w++)
                {
                    float x = w * 4.8f;
                    CreateBlock($"Balcony_F{f}_{w}", hotel.transform, new Vector3(x, y - 0.8f, -10.4f), new Vector3(2.8f, 0.9f, 0.8f), matGreenShutter);
                    CreateBlock($"Window_F{f}_{w}", hotel.transform, new Vector3(x, y + 0.4f, -10.05f), new Vector3(2.2f, 2.6f, 0.1f), matDarkWindow);
                }
            }

            // Mái vòm hoàng gia trên đỉnh
            CreateBlock("RoofDome", hotel.transform, new Vector3(0f, 23.5f, -4.0f), new Vector3(12f, 4.0f, 12f), matWhiteNeoclassical);

            var col = hotel.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 11f, 0f);
            col.size = new Vector3(37f, 24f, 22f);
        }

        /// <summary>
        /// Các khối nhà phố cổ và quán cà phê dọc tuyến phố Lê Thái Tổ
        /// Mặt tiền hướng về phía Đông (+X) nhìn ra đường và hồ
        /// </summary>
        private static void BuildLeThaiToShophouseBlocks(Transform parent)
        {
            // Đoạn phía Bắc Lê Thái Tổ (Nối từ Hàm Cá Mập đến Báo Hà Nội Mới)
            for (int i = 1; i <= 3; i++)
            {
                float z = -710.0f + i * 22.0f;
                BuildSingleShophouse(parent, $"LeThaiTo_North_{i}", new Vector3(525.0f, -0.49f, z), 90f, 18.0f, 16.0f, 4, (i % 2 == 0) ? matFrenchYellow : matColonialCream);
            }

            // Đoạn phía Nam Lê Thái Tổ (Nối Apricot Hotel tới Hàng Khay)
            for (int i = 1; i <= 3; i++)
            {
                float z = -780.0f - i * 24.0f;
                BuildSingleShophouse(parent, $"LeThaiTo_South_{i}", new Vector3(525.0f, -0.49f, z), 90f, 20.0f, 16.0f, 3 + (i % 2), (i % 2 == 1) ? matFrenchYellow : matColonialCream);
            }
        }

        // ================= 3. BỜ NAM: DÃY PHỐ CỔ HÀNG KHAY =================

        /// <summary>
        /// Dãy phố cổ Hàng Khay liền kề (Phố tranh nghệ thuật, kem Tràng Tiền, cafe phố cổ)
        /// Trải dài từ X = 575 đến X = 665, mặt tiền hướng về phía Bắc (+Z) nhìn ra đường và Hồ Gươm
        /// </summary>
        private static void BuildHangKhayShophouseRow(Transform parent)
        {
            float startX = 575.0f;
            float endX = 665.0f;
            float stepX = 13.0f;
            int count = Mathf.FloorToInt((endX - startX) / stepX);

            for (int i = 0; i < count; i++)
            {
                float x = startX + i * stepX;
                float z = -954.0f; // Tâm nhà ở -954, depth 16m -> Mặt tiền ở -954 + 8 = -946.0 sát vỉa hè
                int floors = 3 + (i % 2); // Nhà 3 tầng hoặc 4 tầng xen kẽ
                Material wallMat = (i % 3 == 0) ? matFrenchYellow : ((i % 3 == 1) ? matColonialCream : matWhiteNeoclassical);

                GameObject sh = BuildSingleShophouse(parent, $"HangKhay_Shophouse_{i}", new Vector3(x, -0.49f, z), 0f, 12.5f, 16.0f, floors, wallMat);

                // Thêm mái hiên di động sọc xanh trắng trước cửa hàng tranh
                if (i % 2 == 0)
                {
                    CreateBlock("Awning", sh.transform, new Vector3(0f, 3.6f, 8.8f), new Vector3(11.5f, 0.2f, 2.2f), matAwningStripe, new Vector3(15f, 0f, 0f));
                }

                // Thêm bồn nước inox trên mái
                CreateInoxTank($"Tank_{i}", sh.transform, new Vector3(Random.Range(-3f, 3f), floors * 3.8f + 1.2f, Random.Range(-2f, 2f)));
            }

            // Khối góc ngã tư Hàng Khay - Đinh Tiên Hoàng (Tràng Tiền Plaza style)
            // Đặt lùi ra góc Đông Nam thực tế (X=706, Z=-952) hoàn toàn ngoài ngã tư đường
            GameObject cornerBldg = new GameObject("GocPho_TrangTienPlaza");
            cornerBldg.transform.SetParent(parent, false);
            cornerBldg.transform.position = new Vector3(706.0f, -0.49f, -952.0f);
            cornerBldg.transform.rotation = Quaternion.Euler(0f, -45f, 0f);

            CreateBlock("Corner_MainBody", cornerBldg.transform, new Vector3(0f, 10f, 0f), new Vector3(24f, 20f, 24f), matWhiteNeoclassical);
            CreateBlock("Corner_Dome", cornerBldg.transform, new Vector3(0f, 21.5f, 0f), new Vector3(10f, 4f, 10f), matWhiteNeoclassical);
            CreateBlock("Corner_Sign", cornerBldg.transform, new Vector3(0f, 5.5f, -12.1f), new Vector3(10f, 1.2f, 0.2f), matSignboard);

            var cCol = cornerBldg.AddComponent<BoxCollider>();
            cCol.center = new Vector3(0f, 10f, 0f);
            cCol.size = new Vector3(25f, 22f, 25f);
        }

        // ================= 4. BỜ ĐÔNG: DÃY PHỐ ĐINH TIÊN HOÀNG =================

        /// <summary>
        /// Dãy phố Đinh Tiên Hoàng kết nối liền mạch 2 phía Tòa nhà VNPT (Bưu điện Hà Nội)
        /// Căn chỉnh thẳng hàng với lối vào VNPT (X ~ 706), mặt tiền hướng về phía Tây (-X) nhìn ra hồ
        /// </summary>
        private static void BuildDinhTienHoangStreetBlocks(Transform parent)
        {
            // Phía Bắc VNPT (Z = -635 đến -685): Tâm ở 717, depth 18m -> Mặt tiền ở 717 - 9 = 708.0 sát vỉa hè
            for (int i = 0; i < 3; i++)
            {
                float z = -640.0f - i * 22.0f;
                BuildSingleShophouse(parent, $"DinhTienHoang_North_{i}", new Vector3(717.0f, -0.49f, z), -90f, 20.0f, 18.0f, 4, (i % 2 == 0) ? matColonialCream : matFrenchYellow);
            }

            // Phía Nam VNPT (Z = -745 đến -860: Khu vực Vườn hoa Lý Thái Tổ & các trụ sở hành chính)
            for (int i = 0; i < 4; i++)
            {
                float z = -750.0f - i * 26.0f;
                BuildSingleShophouse(parent, $"DinhTienHoang_South_{i}", new Vector3(717.0f, -0.49f, z), -90f, 24.0f, 18.0f, 4, (i % 2 == 1) ? matFrenchYellow : matColonialCream);
            }
        }

        // ================= HÀM HỖ TRỢ XÂY DỰNG NHÀ PHỐ ĐIỂN HÌNH =================

        private static GameObject BuildSingleShophouse(Transform parent, string name, Vector3 pos, float rotY, float width, float depth, int floors, Material wallMat)
        {
            GameObject house = new GameObject(name);
            house.transform.SetParent(parent, false);
            house.transform.position = pos;
            house.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

            float floorH = 3.8f;
            float totalH = floors * floorH;

            // 1. Khung nhà chính
            CreateBlock("MainStructure", house.transform, new Vector3(0f, totalH * 0.5f, 0f), new Vector3(width, totalH, depth), wallMat);

            // 2. Mái dốc ngói
            CreateBlock("PitchedRoof", house.transform, new Vector3(0f, totalH + 1.2f, 0f), new Vector3(width + 0.5f, 2.2f, depth + 0.5f), matOldRoofTile);

            // 3. Tầng 1: Cửa hàng buôn bán / Cafe phố cổ
            CreateBlock("ShopFront_Door", house.transform, new Vector3(0f, 1.6f, depth * 0.5f + 0.05f), new Vector3(width * 0.75f, 2.8f, 0.1f), matDarkWindow);
            CreateBlock("Shop_Sign", house.transform, new Vector3(0f, 3.3f, depth * 0.5f + 0.15f), new Vector3(width * 0.85f, 0.65f, 0.1f), matSignboard);

            // 4. Các tầng trên: Cửa sổ chớp & ban công
            for (int f = 2; f <= floors; f++)
            {
                float y = (f - 1) * floorH + 1.8f;

                // Ban công nhỏ nhô ra
                CreateBlock($"Balcony_F{f}", house.transform, new Vector3(0f, y - 0.9f, depth * 0.5f + 0.65f), new Vector3(width * 0.8f, 0.9f, 1.2f), matGreenShutter);

                // Cửa sổ
                CreateBlock($"Window_F{f}", house.transform, new Vector3(0f, y + 0.4f, depth * 0.5f + 0.05f), new Vector3(width * 0.65f, 2.1f, 0.1f), matDarkWindow);
            }

            // 5. Cục nóng điều hòa gắn bên tường ngoài (chi tiết đặc trưng của Hà Nội)
            CreateBlock("AC_Unit", house.transform, new Vector3(width * 0.38f, floorH * 1.5f, depth * 0.5f + 0.35f), new Vector3(0.9f, 0.65f, 0.45f), matColonialCream);

            // BoxCollider
            var col = house.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, totalH * 0.5f, 0f);
            col.size = new Vector3(width + 0.5f, totalH + 2f, depth + 1.5f);

            return house;
        }

        private static GameObject CreateBlock(string name, Transform parent, Vector3 localPos, Vector3 size, Material mat, Vector3? rotEuler = null)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPos;
            cube.transform.localScale = size;
            if (rotEuler.HasValue) cube.transform.localRotation = Quaternion.Euler(rotEuler.Value);

            Collider c = cube.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);

            MeshRenderer mr = cube.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;

            return cube;
        }

        private static GameObject CreateInoxTank(string name, Transform parent, Vector3 localPos)
        {
            GameObject tank = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tank.name = name;
            tank.transform.SetParent(parent, false);
            tank.transform.localPosition = localPos;
            tank.transform.localScale = new Vector3(1.4f, 0.9f, 1.4f);
            tank.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); // Bồn nằm ngang

            Collider c = tank.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);

            MeshRenderer mr = tank.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = matInoxTank;

            return tank;
        }

        private static GameObject CreateParasol(string name, Transform parent, Vector3 localPos)
        {
            GameObject parasol = new GameObject(name);
            parasol.transform.SetParent(parent, false);
            parasol.transform.localPosition = localPos;

            // Cột chống
            CreateBlock("Pole", parasol.transform, new Vector3(0f, 1.25f, 0f), new Vector3(0.08f, 2.5f, 0.08f), matColonialCream);

            // Tán dù tròn / bát giác
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            canopy.name = "Canopy";
            canopy.transform.SetParent(parasol.transform, false);
            canopy.transform.localPosition = new Vector3(0f, 2.35f, 0f);
            canopy.transform.localScale = new Vector3(2.8f, 0.2f, 2.8f);

            Collider c = canopy.GetComponent<Collider>();
            if (c != null) Object.DestroyImmediate(c);

            MeshRenderer mr = canopy.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = matAwningStripe;

            return parasol;
        }
    }
}
