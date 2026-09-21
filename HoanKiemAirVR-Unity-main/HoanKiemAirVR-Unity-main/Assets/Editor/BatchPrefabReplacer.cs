#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HoanKiemAirVR.Editor
{
    public class BatchPrefabReplacer : EditorWindow
    {
        private enum ToolTab
        {
            BatchReplace,
            FixLyingTrees
        }

        private enum TargetMode
        {
            SelectedInHierarchy,
            FindByNameInScene
        }

        public enum RotationMode
        {
            StandUpKeepY,        // Dựng thẳng (X=0, Z=0, giữ góc Y) - Khuyên dùng cho cây
            CompensateMinus90X,  // Bù góc -90 độ trục X (Sửa lỗi FBX nghiêng)
            CompensatePlus90X,   // Bù góc +90 độ trục X
            ExactRotation,       // Giữ nguyên 100% góc xoay cũ
            CustomOffset         // Cộng thêm góc xoay tuỳ chỉnh
        }

        public enum ScaleMode
        {
            KeepOriginalAbs,     // Giữ nguyên Scale (tự sửa nếu bị âm scale.z = -1)
            UsePrefabDefault,    // Dùng kích thước mặc định của Prefab mới
            KeepExactOriginal    // Giữ nguyên 100% không đổi
        }

        private enum NamingMode
        {
            UsePrefabName,
            KeepOriginalName,
            CustomName
        }

        private enum MatchType
        {
            ExactMatch,
            StartsWith,
            Contains
        }

        [SerializeField] private ToolTab currentTab = ToolTab.FixLyingTrees; // Mặc định mở tab sửa cây nằm cho người dùng
        [SerializeField] private GameObject replacementPrefab;
        [SerializeField] private TargetMode targetMode = TargetMode.SelectedInHierarchy;
        [SerializeField] private string searchName = "Tree";
        [SerializeField] private MatchType matchType = MatchType.Contains;
        [SerializeField] private bool caseSensitive = false;
        
        [SerializeField] private bool keepPosition = true;
        [SerializeField] private RotationMode rotationMode = RotationMode.StandUpKeepY;
        [SerializeField] private Vector3 customRotationOffset = Vector3.zero;
        [SerializeField] private bool randomizeY = false;

        [SerializeField] private ScaleMode scaleMode = ScaleMode.KeepOriginalAbs;
        [SerializeField] private bool keepParent = true;
        [SerializeField] private bool keepSiblingIndex = true;
        [SerializeField] private bool keepTag = false;
        [SerializeField] private bool keepLayer = true;

        [SerializeField] private NamingMode namingMode = NamingMode.UsePrefabName;
        [SerializeField] private string customName = "WillowTree";

        private Vector2 scrollPos;
        private List<GameObject> cachedFoundObjects = new List<GameObject>();

        [MenuItem("Tools/Batch Replace GameObjects", false, 10)]
        [MenuItem("GameObject/Batch Replace...", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<BatchPrefabReplacer>("Batch Replace Objects");
            window.minSize = new Vector2(400, 520);
            window.Show();
        }

        private void OnEnable()
        {
            if (replacementPrefab == null)
            {
                string[] guids = AssetDatabase.FindAssets("WillowTree_02 t:Prefab");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    replacementPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }
        }

        private void OnSelectionChange()
        {
            if (targetMode == TargetMode.SelectedInHierarchy)
            {
                Repaint();
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Batch GameObject Tools", EditorStyles.boldLabel);

            // TAB SELECTOR
            currentTab = (ToolTab)GUILayout.Toolbar((int)currentTab, new string[] { "Thay thế Prefab", "Dựng thẳng cây (Fix Nằm)" }, GUILayout.Height(30));
            EditorGUILayout.Space(8);

            if (currentTab == ToolTab.FixLyingTrees)
            {
                DrawFixLyingTreesGUI();
            }
            else
            {
                DrawBatchReplaceGUI();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawFixLyingTreesGUI()
        {
            EditorGUILayout.HelpBox("Nguyên nhân cây bị nằm: Model Tree cũ có góc xoay X = 90° (do xuất từ 3D Max/FBX), trong khi WillowTree chuẩn lại đứng thẳng ở góc 0°. Công cụ này sẽ dựng đứng toàn bộ cây dậy ngay lập tức!", MessageType.Info);
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Chọn các cây cần dựng thẳng", EditorStyles.boldLabel);
            targetMode = (TargetMode)EditorGUILayout.EnumPopup("Chế độ chọn", targetMode);

            List<GameObject> targets = new List<GameObject>();

            if (targetMode == TargetMode.SelectedInHierarchy)
            {
                targets = GetSelectedTargets();
                EditorGUILayout.LabelField($"Số object đang chọn trong Hierarchy: {targets.Count}", EditorStyles.boldLabel);
                if (targets.Count == 0)
                {
                    EditorGUILayout.HelpBox("Hãy chọn các cây đang bị nằm trong Hierarchy (hoặc chuyển sang chế độ 'Find By Name In Scene').", MessageType.Warning);
                }
            }
            else
            {
                searchName = EditorGUILayout.TextField("Tên cây cần tìm", searchName);
                matchType = (MatchType)EditorGUILayout.EnumPopup("Kiểu so khớp", matchType);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Tìm theo tên trên"))
                {
                    RefreshFoundObjects();
                }
                if (GUILayout.Button("Tìm nhanh 'WillowTree'"))
                {
                    searchName = "WillowTree";
                    matchType = MatchType.Contains;
                    RefreshFoundObjects();
                }
                if (GUILayout.Button("Tìm nhanh 'Tree'"))
                {
                    searchName = "Tree";
                    matchType = MatchType.Contains;
                    RefreshFoundObjects();
                }
                EditorGUILayout.EndHorizontal();

                targets = cachedFoundObjects;
                EditorGUILayout.LabelField($"Tìm thấy: {targets.Count} object(s)", EditorStyles.boldLabel);

                if (targets.Count > 0)
                {
                    if (GUILayout.Button("Chọn các cây này trong Hierarchy"))
                    {
                        Selection.objects = targets.ToArray();
                    }
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            GUI.enabled = targets.Count > 0;

            GUI.backgroundColor = new Color(0.2f, 0.85f, 0.4f);
            if (GUILayout.Button($"DỰNG THẲNG ĐỨNG NGAY ({targets.Count} cây)\n(Set X=0, Z=0 & Sửa Scale âm)", GUILayout.Height(45)))
            {
                FixTreesStandUp(targets);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(6);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Xoay thêm -90° trục X"))
            {
                RotateTreesManual(targets, new Vector3(-90, 0, 0));
            }
            if (GUILayout.Button("Xoay thêm +90° trục X"))
            {
                RotateTreesManual(targets, new Vector3(90, 0, 0));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            if (GUILayout.Button("Xoay ngẫu nhiên góc Y (0-360°) để rừng cây tự nhiên hơn"))
            {
                RandomizeTreeYRotation(targets);
            }

            GUI.enabled = true;
        }

        private void DrawBatchReplaceGUI()
        {
            EditorGUILayout.HelpBox("Thay thế hàng loạt GameObject với chế độ tự động căn chỉnh góc xoay đứng thẳng.", MessageType.Info);
            EditorGUILayout.Space(8);

            // 1. PREFAB SELECTION
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("1. Prefab Mới (Replacement Prefab)", EditorStyles.boldLabel);
            replacementPrefab = (GameObject)EditorGUILayout.ObjectField("New Prefab", replacementPrefab, typeof(GameObject), false);
            if (replacementPrefab == null)
            {
                EditorGUILayout.HelpBox("Kéo Prefab bạn muốn đổi thành vào ô trên (ví dụ: WillowTree_02).", MessageType.Warning);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // 2. TARGET OBJECTS
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("2. Đối tượng cần thay thế (Targets)", EditorStyles.boldLabel);
            targetMode = (TargetMode)EditorGUILayout.EnumPopup("Chế độ chọn", targetMode);

            List<GameObject> targets = new List<GameObject>();

            if (targetMode == TargetMode.SelectedInHierarchy)
            {
                targets = GetSelectedTargets();
                EditorGUILayout.LabelField($"Số object đang chọn: {targets.Count}", EditorStyles.boldLabel);
                if (targets.Count == 0)
                {
                    EditorGUILayout.HelpBox("Hãy chọn các object trong Hierarchy (hoặc chuyển sang chế độ 'Find By Name In Scene').", MessageType.Info);
                }
            }
            else
            {
                searchName = EditorGUILayout.TextField("Tên cần tìm", searchName);
                matchType = (MatchType)EditorGUILayout.EnumPopup("Kiểu so khớp", matchType);
                caseSensitive = EditorGUILayout.Toggle("Phân biệt hoa/thường", caseSensitive);

                if (GUILayout.Button("Tìm trong Scene hiện tại"))
                {
                    RefreshFoundObjects();
                }

                targets = cachedFoundObjects;
                EditorGUILayout.LabelField($"Tìm thấy: {targets.Count} object(s)", EditorStyles.boldLabel);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // 3. TRANSFORM & ROTATION
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("3. Thiết lập Góc xoay & Tỉ lệ", EditorStyles.boldLabel);
            keepPosition = EditorGUILayout.Toggle("Giữ nguyên Position", keepPosition);
            
            rotationMode = (RotationMode)EditorGUILayout.EnumPopup("Xử lý Góc xoay (Rotation)", rotationMode);
            if (rotationMode == RotationMode.StandUpKeepY)
            {
                EditorGUILayout.HelpBox("Khuyên dùng cho cây: Tự động đặt X=0, Z=0 (đứng thẳng 100%) và giữ góc hướng Y ban đầu.", MessageType.None);
            }
            else if (rotationMode == RotationMode.CustomOffset)
            {
                customRotationOffset = EditorGUILayout.Vector3Field("Góc bù thêm (Offset Euler)", customRotationOffset);
            }

            randomizeY = EditorGUILayout.Toggle("Xoay ngẫu nhiên trục Y (tự nhiên hơn)", randomizeY);

            scaleMode = (ScaleMode)EditorGUILayout.EnumPopup("Xử lý Tỉ lệ (Scale)", scaleMode);
            if (scaleMode == ScaleMode.KeepOriginalAbs)
            {
                EditorGUILayout.HelpBox("Giữ nguyên kích thước và tự động chuyển scale âm (-1) thành dương (1).", MessageType.None);
            }

            keepParent = EditorGUILayout.Toggle("Giữ nguyên Parent", keepParent);
            keepSiblingIndex = EditorGUILayout.Toggle("Giữ nguyên Thứ tự (Sibling Index)", keepSiblingIndex);
            keepLayer = EditorGUILayout.Toggle("Giữ nguyên Layer", keepLayer);
            keepTag = EditorGUILayout.Toggle("Giữ nguyên Tag", keepTag);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            // 4. NAMING
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("4. Đặt tên Object mới", EditorStyles.boldLabel);
            namingMode = (NamingMode)EditorGUILayout.EnumPopup("Cách đặt tên", namingMode);
            if (namingMode == NamingMode.CustomName)
            {
                customName = EditorGUILayout.TextField("Tên mới", customName);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(12);

            // 5. BUTTON
            GUI.enabled = replacementPrefab != null && targets.Count > 0;
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.3f);
            if (GUILayout.Button($"THAY THẾ HÀNG LOẠT ({targets.Count} objects)", GUILayout.Height(38)))
            {
                if (EditorUtility.DisplayDialog("Xác nhận thay thế",
                    $"Bạn có chắc chắn muốn thay thế {targets.Count} object bằng '{replacementPrefab.name}' không?\n\n(Bạn vẫn có thể nhấn Ctrl+Z để Undo)",
                    "Thay thế ngay", "Huỷ"))
                {
                    PerformReplacement(targets);
                }
            }
            GUI.backgroundColor = Color.white;
            GUI.enabled = true;
        }

        private void FixTreesStandUp(List<GameObject> targets)
        {
            if (targets == null || targets.Count == 0) return;

            Undo.SetCurrentGroupName("Stand Up Trees");
            int group = Undo.GetCurrentGroup();

            try
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    GameObject go = targets[i];
                    if (go == null) continue;

                    Undo.RecordObject(go.transform, "Stand Up Tree");

                    // Keep Y yaw, set X=0 and Z=0
                    Vector3 euler = go.transform.rotation.eulerAngles;
                    go.transform.rotation = Quaternion.Euler(0f, euler.y, 0f);

                    // Fix negative scale if any
                    Vector3 scale = go.transform.localScale;
                    go.transform.localScale = new Vector3(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

                    EditorSceneManager.MarkSceneDirty(go.scene);
                }
            }
            finally
            {
                Undo.CollapseUndoOperations(group);
            }

            Debug.Log($"[BatchPrefabReplacer] Đã dựng thẳng đứng {targets.Count} cây thành công! Nhấn Ctrl+Z nếu muốn hoàn tác.");
        }

        private void RotateTreesManual(List<GameObject> targets, Vector3 deltaEuler)
        {
            if (targets == null || targets.Count == 0) return;

            Undo.SetCurrentGroupName("Rotate Trees");
            int group = Undo.GetCurrentGroup();

            try
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    GameObject go = targets[i];
                    if (go == null) continue;

                    Undo.RecordObject(go.transform, "Rotate Tree");
                    go.transform.Rotate(deltaEuler, Space.Self);
                    EditorSceneManager.MarkSceneDirty(go.scene);
                }
            }
            finally
            {
                Undo.CollapseUndoOperations(group);
            }
        }

        private void RandomizeTreeYRotation(List<GameObject> targets)
        {
            if (targets == null || targets.Count == 0) return;

            Undo.SetCurrentGroupName("Randomize Tree Rotation");
            int group = Undo.GetCurrentGroup();

            try
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    GameObject go = targets[i];
                    if (go == null) continue;

                    Undo.RecordObject(go.transform, "Randomize Tree Rotation");
                    Vector3 euler = go.transform.rotation.eulerAngles;
                    go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                    EditorSceneManager.MarkSceneDirty(go.scene);
                }
            }
            finally
            {
                Undo.CollapseUndoOperations(group);
            }
        }

        private List<GameObject> GetSelectedTargets()
        {
            List<GameObject> list = new List<GameObject>();
            foreach (var obj in Selection.gameObjects)
            {
                if (obj != null && obj.scene.IsValid())
                {
                    list.Add(obj);
                }
            }
            return list;
        }

        private void RefreshFoundObjects()
        {
            cachedFoundObjects.Clear();
            if (string.IsNullOrEmpty(searchName)) return;

            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            var comparison = caseSensitive ? System.StringComparison.Ordinal : System.StringComparison.OrdinalIgnoreCase;

            foreach (var go in allObjects)
            {
                if (go == null || !go.scene.IsValid() || EditorUtility.IsPersistent(go))
                    continue;

                if ((go.hideFlags & (HideFlags.NotEditable | HideFlags.HideAndDontSave)) != 0)
                    continue;

                bool matched = false;
                switch (matchType)
                {
                    case MatchType.ExactMatch:
                        matched = go.name.Equals(searchName, comparison);
                        break;
                    case MatchType.StartsWith:
                        matched = go.name.StartsWith(searchName, comparison);
                        break;
                    case MatchType.Contains:
                        matched = go.name.IndexOf(searchName, comparison) >= 0;
                        break;
                }

                if (matched)
                {
                    cachedFoundObjects.Add(go);
                }
            }
        }

        private void PerformReplacement(List<GameObject> targets)
        {
            if (replacementPrefab == null || targets == null || targets.Count == 0) return;

            Undo.SetCurrentGroupName($"Batch Replace {targets.Count} Objects with {replacementPrefab.name}");
            int undoGroup = Undo.GetCurrentGroup();

            List<GameObject> newObjects = new List<GameObject>();
            int replacedCount = 0;

            try
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    GameObject oldObj = targets[i];
                    if (oldObj == null) continue;

                    EditorUtility.DisplayProgressBar("Replacing Objects", $"Replacing {oldObj.name} ({i + 1}/{targets.Count})...", (float)i / targets.Count);

                    Transform oldTransform = oldObj.transform;
                    Transform parent = oldTransform.parent;
                    int siblingIndex = oldTransform.GetSiblingIndex();
                    Vector3 position = oldTransform.position;
                    Quaternion rotation = oldTransform.rotation;
                    Vector3 localScale = oldTransform.localScale;
                    int layer = oldObj.layer;
                    string tag = oldObj.tag;

                    // Calculate rotation
                    Quaternion finalRotation = rotation;
                    switch (rotationMode)
                    {
                        case RotationMode.StandUpKeepY:
                            float yAngle = randomizeY ? Random.Range(0f, 360f) : rotation.eulerAngles.y;
                            finalRotation = Quaternion.Euler(0f, yAngle, 0f);
                            break;
                        case RotationMode.CompensateMinus90X:
                            finalRotation = rotation * Quaternion.Euler(-90f, 0f, 0f);
                            if (randomizeY) finalRotation = Quaternion.Euler(finalRotation.eulerAngles.x, Random.Range(0f, 360f), finalRotation.eulerAngles.z);
                            break;
                        case RotationMode.CompensatePlus90X:
                            finalRotation = rotation * Quaternion.Euler(90f, 0f, 0f);
                            if (randomizeY) finalRotation = Quaternion.Euler(finalRotation.eulerAngles.x, Random.Range(0f, 360f), finalRotation.eulerAngles.z);
                            break;
                        case RotationMode.ExactRotation:
                            if (randomizeY) finalRotation = Quaternion.Euler(rotation.eulerAngles.x, Random.Range(0f, 360f), rotation.eulerAngles.z);
                            break;
                        case RotationMode.CustomOffset:
                            finalRotation = rotation * Quaternion.Euler(customRotationOffset);
                            if (randomizeY) finalRotation = Quaternion.Euler(finalRotation.eulerAngles.x, Random.Range(0f, 360f), finalRotation.eulerAngles.z);
                            break;
                    }

                    // Calculate scale
                    Vector3 finalScale = localScale;
                    switch (scaleMode)
                    {
                        case ScaleMode.KeepOriginalAbs:
                            finalScale = new Vector3(Mathf.Abs(localScale.x), Mathf.Abs(localScale.y), Mathf.Abs(localScale.z));
                            break;
                        case ScaleMode.UsePrefabDefault:
                            finalScale = replacementPrefab.transform.localScale;
                            break;
                        case ScaleMode.KeepExactOriginal:
                            finalScale = localScale;
                            break;
                    }

                    // Instantiate Prefab
                    GameObject newObj;
                    if (PrefabUtility.IsPartOfPrefabAsset(replacementPrefab))
                    {
                        newObj = (GameObject)PrefabUtility.InstantiatePrefab(replacementPrefab, keepParent ? parent : null);
                    }
                    else
                    {
                        newObj = Instantiate(replacementPrefab, keepParent ? parent : null);
                    }

                    if (newObj == null) continue;

                    Undo.RegisterCreatedObjectUndo(newObj, "Instantiate Replacement");

                    if (keepParent)
                    {
                        newObj.transform.SetParent(parent, true);
                    }

                    if (keepPosition) newObj.transform.position = position;
                    newObj.transform.rotation = finalRotation;
                    newObj.transform.localScale = finalScale;
                    if (keepSiblingIndex) newObj.transform.SetSiblingIndex(siblingIndex);
                    if (keepLayer) newObj.layer = layer;
                    if (keepTag && !string.IsNullOrEmpty(tag) && tag != "Untagged")
                    {
                        try { newObj.tag = tag; } catch { }
                    }

                    // Naming
                    switch (namingMode)
                    {
                        case NamingMode.UsePrefabName:
                            newObj.name = replacementPrefab.name;
                            break;
                        case NamingMode.KeepOriginalName:
                            newObj.name = oldObj.name;
                            break;
                        case NamingMode.CustomName:
                            newObj.name = string.IsNullOrEmpty(customName) ? replacementPrefab.name : customName;
                            break;
                    }

                    EditorSceneManager.MarkSceneDirty(newObj.scene);

                    if (PrefabUtility.IsPartOfPrefabInstance(oldObj))
                    {
                        var root = PrefabUtility.GetOutermostPrefabInstanceRoot(oldObj);
                        if (root != null && root == oldObj)
                        {
                            PrefabUtility.UnpackPrefabInstance(root, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                        }
                    }

                    Undo.DestroyObjectImmediate(oldObj);

                    newObjects.Add(newObj);
                    replacedCount++;
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                Undo.CollapseUndoOperations(undoGroup);
            }

            if (newObjects.Count > 0)
            {
                Selection.objects = newObjects.ToArray();
            }

            if (targetMode == TargetMode.FindByNameInScene)
            {
                RefreshFoundObjects();
            }

            Debug.Log($"[BatchPrefabReplacer] Đã thay thế thành công {replacedCount} object thành '{replacementPrefab.name}'. Nhấn Ctrl+Z nếu muốn hoàn tác.");
        }
    }
}
#endif
