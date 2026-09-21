using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoanKiemAirVR
{
    /// <summary>
    /// Hiệu ứng sợi dây nhảy 3D chuyển động xoay tròn nhịp nhàng giữa 2 người quay dây
    /// </summary>
    public class JumpRopeArc : MonoBehaviour
    {
        public Transform turner1;
        public Transform turner2;
        public Transform jumper;

        public float ropeSpeed = 3.6f;     // Tốc độ quay dây (vòng/giây)
        public float ropeMaxRadius = 1.35f; // Độ võng tối đa của dây khi quay
        public int segmentCount = 28;      // Số điểm tạo đường cong dây mượt mà
        public Color ropeColor = new Color(0.95f, 0.45f, 0.15f, 1.0f); // Màu dây thừng cam đỏ nổi bật

        private LineRenderer lineRenderer;
        private float currentAngle = 0f;

        void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.useWorldSpace = true;
            lineRenderer.positionCount = segmentCount;
            lineRenderer.startWidth = 0.038f;
            lineRenderer.endWidth = 0.038f;

            // Shader Standard hoặc Unlit cho dây
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material mat = new Material(shader);
            mat.color = ropeColor;
            lineRenderer.material = mat;
        }

        void Update()
        {
            if (turner1 == null || turner2 == null) return;

            // Lấy vị trí tay 2 người quay dây (cao ngang thắt lưng ~0.8m)
            Vector3 p1 = turner1.position + Vector3.up * 0.85f + turner1.forward * 0.35f;
            Vector3 p2 = turner2.position + Vector3.up * 0.85f + turner2.forward * 0.35f;

            // Góc xoay của sợi dây theo thời gian
            currentAngle += ropeSpeed * Mathf.PI * 2f * Time.deltaTime;
            if (currentAngle > Mathf.PI * 2f) currentAngle -= Mathf.PI * 2f;

            Vector3 axis = (p2 - p1).normalized;
            Vector3 upDir = Vector3.up;
            Vector3 sideDir = Vector3.Cross(axis, upDir).normalized;

            // Vector bán kính quay của sợi dây tại góc currentAngle
            // Khi angle = 0: dây ở vị trí thấp nhất sát đất (người nhảy phải nhảy qua)
            // Khi angle = PI: dây ở vị trí cao nhất trên đầu người nhảy
            Vector3 radialDir = Mathf.Cos(currentAngle) * sideDir - Mathf.Sin(currentAngle) * upDir;

            for (int i = 0; i < segmentCount; i++)
            {
                float t = (float)i / (segmentCount - 1);
                Vector3 centerLinePoint = Vector3.Lerp(p1, p2, t);

                // Dây võng hình parabol/sin: 2 đầu gắn vào tay, ở giữa võng nhiều nhất
                float sag = Mathf.Sin(t * Mathf.PI) * ropeMaxRadius;
                Vector3 ropePoint = centerLinePoint + radialDir * sag;

                // Giữ dây không bị lún sâu xuống lòng đất
                if (ropePoint.y < -0.48f) ropePoint.y = -0.48f;

                lineRenderer.SetPosition(i, ropePoint);
            }

            // Đồng bộ bước nhảy của người ở giữa (nếu có Jumper)
            if (jumper != null)
            {
                // Dây ở dưới đất khi sin(angle) > 0.8
                float jumpPhase = Mathf.Sin(currentAngle);
                if (jumpPhase > 0.1f)
                {
                    float jumpHeight = Mathf.Sin((jumpPhase - 0.1f) / 0.9f * Mathf.PI) * 0.32f;
                    Vector3 jp = jumper.position;
                    jp.y = -0.49f + jumpHeight;
                    jumper.position = jp;
                }
                else
                {
                    Vector3 jp = jumper.position;
                    jp.y = -0.49f;
                    jumper.position = jp;
                }
            }
        }
    }

    /// <summary>
    /// Dựng bàn cờ Ô Ăn Quan 3D truyền thống với vạch phấn và sỏi thạch anh trên vỉa hè
    /// </summary>
    public static class OAnQuanBuilder
    {
        public static GameObject CreateBoard(Vector3 centerPos, float rotY = 0f)
        {
            GameObject boardRoot = new GameObject("OAnQuan_Board_3D");
            boardRoot.transform.position = centerPos;
            boardRoot.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

            // Mặt bàn cờ kẻ phấn trên nền đất (tấm quad mỏng áp sát mặt đường)
            GameObject boardPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            boardPlane.name = "Board_Chalk_Lines";
            boardPlane.transform.SetParent(boardRoot.transform, false);
            boardPlane.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            boardPlane.transform.localPosition = new Vector3(0f, 0.008f, 0f); // Nhô nhẹ trên mặt đất 8mm
            boardPlane.transform.localScale = new Vector3(2.4f, 0.9f, 1.0f);

            var col = boardPlane.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            // Tạo Texture vẽ phấn bàn cờ Ô Ăn Quan sắc nét
            Texture2D chalkTex = GenerateOAnQuanTexture();
            Material chalkMat = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture") ?? Shader.Find("Sprites/Default"));
            chalkMat.mainTexture = chalkTex;
            boardPlane.GetComponent<Renderer>().material = chalkMat;

            // Rải 2 viên Quan to 2 bên đầu bàn cờ (bán nguyệt)
            CreateStone(boardRoot.transform, new Vector3(-0.95f, 0.035f, 0f), 0.12f, new Color(0.85f, 0.25f, 0.2f), "Quan_Do");
            CreateStone(boardRoot.transform, new Vector3(0.95f, 0.035f, 0f), 0.12f, new Color(0.2f, 0.65f, 0.85f), "Quan_Xanh");

            // Rải các viên sỏi nhỏ xinh xắn vào 10 ô dân (mỗi ô 4-5 viên sỏi)
            float[] xOffsets = new float[] { -0.6f, -0.3f, 0.0f, 0.3f, 0.6f };
            float[] zOffsets = new float[] { 0.22f, -0.22f };

            Color[] stoneColors = new Color[]
            {
                new Color(0.95f, 0.95f, 0.92f), // Sỏi trắng thạch anh
                new Color(0.78f, 0.72f, 0.65f), // Sỏi cát vàng
                new Color(0.55f, 0.52f, 0.50f), // Sỏi xám bờ hồ
                new Color(0.90f, 0.85f, 0.75f)
            };

            int stoneIdx = 0;
            foreach (float z in zOffsets)
            {
                foreach (float x in xOffsets)
                {
                    int pebblesInCell = Random.Range(4, 6);
                    for (int p = 0; p < pebblesInCell; p++)
                    {
                        Vector3 randOffset = new Vector3(Random.Range(-0.065f, 0.065f), 0.015f, Random.Range(-0.065f, 0.065f));
                        Color c = stoneColors[Random.Range(0, stoneColors.Length)];
                        CreateStone(boardRoot.transform, new Vector3(x, 0f, z) + randOffset, Random.Range(0.035f, 0.048f), c, $"Dan_{stoneIdx++}");
                    }
                }
            }

            return boardRoot;
        }

        private static void CreateStone(Transform parent, Vector3 localPos, float radius, Color color, string name)
        {
            GameObject stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stone.name = name;
            stone.transform.SetParent(parent, false);
            stone.transform.localPosition = localPos;
            stone.transform.localScale = new Vector3(radius, radius * 0.65f, radius);

            var sc = stone.GetComponent<Collider>();
            if (sc != null) Object.DestroyImmediate(sc);

            Renderer r = stone.GetComponent<Renderer>();
            if (r != null)
            {
                Material m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default"));
                m.color = color;
                r.material = m;
            }
        }

        private static Texture2D GenerateOAnQuanTexture()
        {
            int w = 512;
            int h = 192;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            Color transparent = new Color(0.18f, 0.18f, 0.18f, 0.82f); // Nền xi măng vỉa hè
            Color chalk = new Color(0.96f, 0.96f, 0.92f, 1.0f);        // Màu phấn trắng

            Color[] pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent;
            tex.SetPixels(pixels);

            // Vẽ viền phấn bàn cờ Ô ăn quan (2 ô bán nguyệt 2 đầu + 10 ô chữ nhật ở giữa)
            DrawBox(tex, 75, 20, 362, 152, chalk, 3);
            DrawLineHoriz(tex, 75, 437, 96, chalk, 3); // Đường ngăn đôi 2 hàng dân

            // 4 đường kẻ dọc ngăn cách 5 cột dân
            for (int col = 1; col < 5; col++)
            {
                int x = 75 + (col * 362) / 5;
                DrawLineVert(tex, x, 20, 172, chalk, 2);
            }

            // Vẽ 2 ô Quan hình bán nguyệt ở 2 đầu
            DrawSemiCircle(tex, 75, 96, 76, true, chalk, 3);   // Ô Quan trái
            DrawSemiCircle(tex, 437, 96, 76, false, chalk, 3); // Ô Quan phải

            tex.Apply();
            return tex;
        }

        private static void DrawBox(Texture2D tex, int x, int y, int bw, int bh, Color c, int thickness)
        {
            DrawLineHoriz(tex, x, x + bw, y, c, thickness);
            DrawLineHoriz(tex, x, x + bw, y + bh, c, thickness);
            DrawLineVert(tex, x, y, y + bh, c, thickness);
            DrawLineVert(tex, x + bw, y, y + bh, c, thickness);
        }

        private static void DrawLineHoriz(Texture2D tex, int x0, int x1, int y, Color c, int thick)
        {
            for (int dy = -thick / 2; dy <= thick / 2; dy++)
            {
                int py = y + dy;
                if (py < 0 || py >= tex.height) continue;
                for (int x = x0; x <= x1; x++)
                {
                    if (x >= 0 && x < tex.width) tex.SetPixel(x, py, c);
                }
            }
        }

        private static void DrawLineVert(Texture2D tex, int x, int y0, int y1, Color c, int thick)
        {
            for (int dx = -thick / 2; dx <= thick / 2; dx++)
            {
                int px = x + dx;
                if (px < 0 || px >= tex.width) continue;
                for (int y = y0; y <= y1; y++)
                {
                    if (y >= 0 && y < tex.height) tex.SetPixel(px, y, c);
                }
            }
        }

        private static void DrawSemiCircle(Texture2D tex, int cx, int cy, int r, bool isLeft, Color c, int thick)
        {
            for (int a = -90; a <= 90; a++)
            {
                float rad = a * Mathf.Deg2Rad;
                int px = cx + (int)((isLeft ? -1 : 1) * Mathf.Cos(rad) * r);
                int py = cy + (int)(Mathf.Sin(rad) * r);
                for (int dx = -thick; dx <= thick; dx++)
                {
                    for (int dy = -thick; dy <= thick; dy++)
                    {
                        if (px + dx >= 0 && px + dx < tex.width && py + dy >= 0 && py + dy < tex.height)
                        {
                            tex.SetPixel(px + dx, py + dy, c);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Dựng bàn Cờ Tướng vỉa hè truyền thống với bàn gỗ và 32 quân cờ Đỏ - Đen
    /// </summary>
    public static class XiangqiBuilder
    {
        public static GameObject CreateTable(Vector3 centerPos, float rotY = 0f)
        {
            GameObject chessRoot = new GameObject("Ban_Co_Tuong_Via_He");
            chessRoot.transform.position = centerPos;
            chessRoot.transform.rotation = Quaternion.Euler(0f, rotY, 0f);

            // Bàn cờ gỗ thấp
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = "Table_Go";
            table.transform.SetParent(chessRoot.transform, false);
            table.transform.localScale = new Vector3(1.1f, 0.45f, 1.1f);
            table.transform.localPosition = new Vector3(0f, 0.225f, 0f);

            var col = table.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            Renderer tableRenderer = table.GetComponent<Renderer>();
            if (tableRenderer != null)
            {
                Material woodMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default"));
                woodMat.color = new Color(0.72f, 0.50f, 0.28f); // Màu gỗ mộc
                tableRenderer.material = woodMat;
            }

            // Tấm bàn cờ
            GameObject boardTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            boardTop.name = "Board_Top";
            boardTop.transform.SetParent(chessRoot.transform, false);
            boardTop.transform.localScale = new Vector3(0.9f, 0.02f, 0.9f);
            boardTop.transform.localPosition = new Vector3(0f, 0.46f, 0f);

            var col2 = boardTop.GetComponent<Collider>();
            if (col2 != null) Object.DestroyImmediate(col2);

            Renderer boardTopRenderer = boardTop.GetComponent<Renderer>();
            if (boardTopRenderer != null)
            {
                Material boardMat = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default"));
                boardMat.color = new Color(0.92f, 0.80f, 0.58f); // Màu giấy dó bàn cờ
                boardTopRenderer.material = boardMat;
            }

            // Các quân cờ tướng Đỏ và Đen
            for (int i = 0; i < 7; i++)
            {
                CreateChessPiece(chessRoot.transform, new Vector3(-0.35f + i * 0.11f, 0.48f, 0.32f), Color.red);
                CreateChessPiece(chessRoot.transform, new Vector3(-0.35f + i * 0.11f, 0.48f, -0.32f), new Color(0.12f, 0.12f, 0.12f));
            }

            return chessRoot;
        }

        private static void CreateChessPiece(Transform parent, Vector3 localPos, Color color)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            piece.name = "Quan_Co";
            piece.transform.SetParent(parent, false);
            piece.transform.localScale = new Vector3(0.055f, 0.015f, 0.055f);
            piece.transform.localPosition = localPos;

            var col = piece.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            Renderer r = piece.GetComponent<Renderer>();
            if (r != null)
            {
                Material m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Sprites/Default"));
                m.color = color;
                r.material = m;
            }
        }
    }
}
