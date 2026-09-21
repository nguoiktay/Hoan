#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HoanKiemAirVR.Editor
{
    /// <summary>
    /// Công cụ Editor tiện lợi cho phép người dùng sinh trực tiếp hoặc tùy biến
    /// đám đông đi bộ, chạy bộ và tụ tập ăn kem vào Scene với 1 cú click chuột
    /// </summary>
    public class CityCrowdEditorWindow : EditorWindow
    {
        private const string SYSTEM_ROOT_NAME = "[HoanKiem_CrowdSystem]";

        [SerializeField] private int pedestrianCount = 14;
        [SerializeField] private int joggerCount = 4;
        [SerializeField] private int iceCreamCount = 8;
        [SerializeField] private bool enableFolkGames = true;
        [SerializeField] private float walkSpeed = 1.35f;
        [SerializeField] private float jogSpeed = 3.1f;

        private Vector2 scrollPos;

        [MenuItem("HoanKiem VR/Bake Đám Đông Vào Scene Ngay (1-Click)", false, 0)]
        public static void BakeDirectly()
        {
            var win = CreateInstance<CityCrowdEditorWindow>();
            win.BakeCrowdToScene();
            DestroyImmediate(win);
        }

        [MenuItem("HoanKiem VR/Quản lý Đám Đông (Crowd System)", false, 1)]
        public static void ShowWindow()
        {
            var win = GetWindow<CityCrowdEditorWindow>("Đám Đông Hồ Gươm");
            win.minSize = new Vector2(420, 520);
            win.Show();
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // Banner tiêu đề
            DrawHeader();

            EditorGUILayout.Space(8);

            // Trạng thái hiện tại trong Scene
            DrawCurrentStatus();

            EditorGUILayout.Space(10);

            // Cấu hình số lượng & tốc độ
            DrawSettings();

            EditorGUILayout.Space(14);

            // Các nút thao tác chính
            DrawActionButtons();

            EditorGUILayout.Space(14);

            // Phím tắt dịch chuyển góc nhìn Scene View để xem ngay
            DrawViewShortcuts();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            GUIStyle headerBox = new GUIStyle(GUI.skin.box);
            headerBox.padding = new RectOffset(12, 12, 12, 12);
            headerBox.normal.background = Texture2D.linearGrayTexture;

            EditorGUILayout.BeginVertical(headerBox);
            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
            titleStyle.fontSize = 15;
            titleStyle.normal.textColor = new Color(0.1f, 0.85f, 0.5f);
            EditorGUILayout.LabelField("HỆ THỐNG ĐÁM ĐÔNG SỐNG ĐỘNG HỒ GƯƠM", titleStyle);

            EditorGUILayout.LabelField("Đi bộ phố Đinh Tiên Hoàng & Lê Thái Tổ | Chạy bộ ven hồ | Ăn kem Thủy Tạ", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawCurrentStatus()
        {
            GameObject root = GameObject.Find(SYSTEM_ROOT_NAME);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            if (root != null)
            {
                int totalKids = 0;
                int peds = 0;
                int jogs = 0;
                int ices = 0;
                int games = 0;

                for (int i = 0; i < root.transform.childCount; i++)
                {
                    Transform group = root.transform.GetChild(i);
                    totalKids += group.childCount;
                    if (group.name.Contains("Pedestrian") || group.name.Contains("Di_Bo")) peds = group.childCount;
                    else if (group.name.Contains("Jogger") || group.name.Contains("Chay_Bo")) jogs = group.childCount;
                    else if (group.name.Contains("IceCream") || group.name.Contains("An_Kem")) ices = group.childCount;
                    else if (group.name.Contains("FolkGames") || group.name.Contains("Dan_Gian")) games = group.childCount;
                }

                EditorGUILayout.LabelField("Trạng thái Scene:", "ĐÃ BAKE ĐÁM ĐÔNG TRONG SCENE", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"• Người đi bộ trên phố: {peds} người");
                EditorGUILayout.LabelField($"• Người chạy bộ quanh hồ: {jogs} người");
                EditorGUILayout.LabelField($"• Người tụ tập ăn kem ven hồ: {ices} người (cầm que kem)");
                EditorGUILayout.LabelField($"• Cụm trò chơi dân gian: {games} cụm (Nhảy dây, Ô ăn quan, Cờ tướng)");
                EditorGUILayout.LabelField($"Tổng cộng: {totalKids} nhóm/nhân vật đang hoạt động", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.LabelField("Trạng thái Scene:", "Chưa có đám đông trong Scene", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Bấm nút 'Bake Đám Đông' bên dưới để sinh toàn bộ nhân vật vào Scene ngay lập tức.", EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawSettings()
        {
            EditorGUILayout.LabelField("Cấu Hình Số Lượng & Tốc Độ", EditorStyles.boldLabel);

            pedestrianCount = EditorGUILayout.IntSlider("Số người đi bộ trên đường", pedestrianCount, 4, 30);
            joggerCount = EditorGUILayout.IntSlider("Số người chạy bộ quanh hồ", joggerCount, 2, 10);
            iceCreamCount = EditorGUILayout.IntSlider("Số người tụ tập ăn kem", iceCreamCount, 3, 15);
            enableFolkGames = EditorGUILayout.Toggle("Bật trò chơi dân gian (Nhảy dây, Ô ăn quan)", enableFolkGames);

            EditorGUILayout.Space(4);
            walkSpeed = EditorGUILayout.Slider("Tốc độ đi bộ (m/s)", walkSpeed, 0.9f, 2.0f);
            jogSpeed = EditorGUILayout.Slider("Tốc độ chạy bộ (m/s)", jogSpeed, 2.2f, 4.0f);
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Chỉnh Sửa Đường Chạy Trực Tiếp Bằng Chuột", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = new Color(0.1f, 0.85f, 1.0f);
            if (GUILayout.Button("Tạo các điểm Waypoint trên Scene (WP_01..32)", GUILayout.Height(28)))
            {
                GameObject r = GameObject.Find(SYSTEM_ROOT_NAME);
                if (r != null)
                {
                    var cs = r.GetComponent<HoanKiemCrowdSystem>();
                    if (cs != null)
                    {
                        cs.CreateJogWaypointsInScene();
                        EditorSceneManager.MarkSceneDirty(r.scene);
                    }
                }
            }
            GUI.backgroundColor = new Color(0.3f, 0.85f, 0.5f);
            if (GUILayout.Button("Bắt dính mặt đường (Snap)", GUILayout.Height(28)))
            {
                GameObject r = GameObject.Find(SYSTEM_ROOT_NAME);
                if (r != null)
                {
                    var cs = r.GetComponent<HoanKiemCrowdSystem>();
                    if (cs != null)
                    {
                        cs.SnapAllWaypointsToGround();
                        EditorSceneManager.MarkSceneDirty(r.scene);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.LabelField("Thao Tác Tạo / Xóa Đám Đông", EditorStyles.boldLabel);

            // Nút BAKE TO SCENE
            GUI.backgroundColor = new Color(0.2f, 0.85f, 0.4f);
            if (GUILayout.Button(" BAKE ĐÁM ĐÔNG VÀO SCENE NGAY (1-Click)", GUILayout.Height(40)))
            {
                BakeCrowdToScene();
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(4);

            // Nút XÓA ĐÁM ĐÔNG
            GUI.backgroundColor = new Color(0.95f, 0.4f, 0.4f);
            if (GUILayout.Button("Xóa Đám Đông Đã Tạo Khỏi Scene", GUILayout.Height(28)))
            {
                DeleteCrowdFromScene();
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawViewShortcuts()
        {
            EditorGUILayout.LabelField("Di Chuyển Góc Nhìn Scene View", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Nhảy Dây Phố Đi Bộ"))
            {
                FocusSceneView(new Vector3(676.0f, 0.0f, -655.0f), 12.0f);
            }
            if (GUILayout.Button("Bàn Cờ Ô Ăn Quan"))
            {
                FocusSceneView(new Vector3(676.0f, 0.0f, -685.0f), 10.0f);
            }
            if (GUILayout.Button("Bàn Cờ Tướng"))
            {
                FocusSceneView(new Vector3(677.5f, 0.0f, -715.0f), 10.0f);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Quầy Kem Đinh Tiên Hoàng"))
            {
                FocusSceneView(new Vector3(681.0f, 0.0f, -736.0f), 12.0f);
            }
            if (GUILayout.Button("Cầu Thê Húc"))
            {
                FocusSceneView(new Vector3(643.2f, 0.0f, -664.4f), 25.0f);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Phố Đinh Tiên Hoàng"))
            {
                FocusSceneView(new Vector3(678.0f, 0.0f, -710.0f), 35.0f);
            }
            if (GUILayout.Button("Phố Lê Thái Tổ"))
            {
                FocusSceneView(new Vector3(558.0f, 0.0f, -760.0f), 35.0f);
            }
            EditorGUILayout.EndHorizontal();
        }

        public void BakeCrowdToScene()
        {
            // Tìm hoặc tạo GameObject Root
            GameObject root = GameObject.Find(SYSTEM_ROOT_NAME);
            if (root == null)
            {
                root = new GameObject(SYSTEM_ROOT_NAME);
                Undo.RegisterCreatedObjectUndo(root, "Create Crowd System Root");
            }

            HoanKiemCrowdSystem crowdSys = root.GetComponent<HoanKiemCrowdSystem>();
            if (crowdSys == null)
            {
                crowdSys = root.AddComponent<HoanKiemCrowdSystem>();
            }

            crowdSys.pedestrianCount = pedestrianCount;
            crowdSys.joggerCount = joggerCount;
            crowdSys.iceCreamEaterCount = iceCreamCount;
            crowdSys.enableFolkGames = enableFolkGames;
            crowdSys.walkSpeed = walkSpeed;
            crowdSys.jogSpeed = jogSpeed;

            // Nạp các prefab nhân vật CityPeople
            LoadPrefabsIntoSystem(crowdSys);

            // Sinh đám đông
            crowdSys.SpawnCrowd();

            // Đánh dấu Scene đã thay đổi để lưu
            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;

            Debug.Log($"<color=#00FF88>[Crowd System] Đã nướng thành công đám đông vào Scene: {pedestrianCount} người đi bộ, {joggerCount} người chạy bộ, {iceCreamCount} người ăn kem, cùng các trò chơi dân gian (Nhảy dây, Ô ăn quan, Cờ tướng)!</color>");
        }

        public void DeleteCrowdFromScene()
        {
            GameObject root = GameObject.Find(SYSTEM_ROOT_NAME);
            if (root != null)
            {
                Undo.DestroyObjectImmediate(root);
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                Debug.Log("<color=#FFAA00>[Crowd System] Đã xóa hệ thống đám đông khỏi Scene.</color>");
            }
            else
            {
                Debug.Log("[Crowd System] Không tìm thấy đám đông nào trong Scene để xóa.");
            }
        }

        private void LoadPrefabsIntoSystem(HoanKiemCrowdSystem sys)
        {
            List<GameObject> fList = new List<GameObject>();
            List<GameObject> mList = new List<GameObject>();

            string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/DenysAlmaral/CityPeople/Prefabs" });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject p = AssetDatabase.LoadAssetAtPath<GameObject>(path);
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

            sys.femalePrefabs = fList.ToArray();
            sys.malePrefabs = mList.ToArray();

            sys.iceCreamPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Eloi_CityKit/Mesh/HoanKiemLake/Prefabs/IceCream.prefab");
        }

        private void FocusSceneView(Vector3 target, float size)
        {
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.LookAt(target, SceneView.lastActiveSceneView.rotation, size);
                SceneView.lastActiveSceneView.Repaint();
            }
        }
    }

    [InitializeOnLoad]
    public static class CityCrowdAutoBake
    {
        static CityCrowdAutoBake()
        {
            EditorApplication.delayCall += () =>
            {
                GameObject root = GameObject.Find("[HoanKiem_CrowdSystem]");
                if (root == null || root.transform.Find("=== Tro_Choi_Dan_Gian (FolkGames) ===") == null)
                {
                    CityCrowdEditorWindow.BakeDirectly();
                }
            };
        }
    }
}
#endif
