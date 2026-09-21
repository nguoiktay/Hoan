using UnityEngine;

/// <summary>
/// Bộ điều khiển Camera Dạo Phố & Khám Phá Hồ Gươm (Universal Free-Fly & Street Walker)
/// - Hỗ trợ cả New Input System và Old Input Manager
/// - Di chuyển tự do 100%, không bao giờ bị kẹt, không bị khóa độ cao
/// - Di chuyển: W/A/S/D hoặc 4 phím Mũi tên (Shift để tăng tốc, Space/E bay lên, Ctrl/C/Q hạ xuống)
/// - Xoay góc nhìn: Giữ chuột phải (hoặc chuột trái) để xoay 360°, hoặc click vào màn hình để xoay tự do
/// - Phím 1, 2, 7, 8: Dịch chuyển tức thì đến các thắng cảnh quanh hồ
/// - Màn hình sạch 100%, không chứa bất kỳ thanh UI nào
/// </summary>
public class PlayerWalkController : MonoBehaviour
{
    [Header("Tốc độ di chuyển")]
    public float moveSpeed = 12.0f;
    public float fastSpeed = 32.0f;
    public float mouseSensitivity = 2.5f;
    public float upDownLimit = 85.0f;

    private float pitch = 0f;
    private float yaw = 0f;
    private bool cursorLocked = false;
    private Vector3 initialPosition;
    private Quaternion initialRotation;

    // Các điểm dịch chuyển nhanh quanh Hồ Gươm
    private struct Landmark
    {
        public Vector3 position;
        public Vector3 rotation;
        public Landmark(Vector3 pos, Vector3 rot) { position = pos; rotation = rot; }
    }

    private Landmark[] landmarks;

    void Awake()
    {
        // Loại bỏ CharacterController nếu có để tránh xung đột vật lý
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            Destroy(cc);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
    }

    void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        Vector3 euler = transform.eulerAngles;
        pitch = euler.x;
        if (pitch > 180f) pitch -= 360f;
        yaw = euler.y;

        // Định nghĩa các điểm tham quan đẹp nhất quanh Hồ Gươm
        landmarks = new Landmark[]
        {
            new Landmark(new Vector3(672.0f, 1.8f, -635.0f), new Vector3(0f, 225f, 0f)), // 1. Cầu Thê Húc
            new Landmark(new Vector3(685.0f, 1.8f, -735.0f), new Vector3(0f, 260f, 0f)), // 2. Tháp Rùa
            new Landmark(new Vector3(560.0f, 1.8f, -850.0f), new Vector3(0f, 65f, 0f)),  // 3. Phố Lê Thái Tổ
            new Landmark(new Vector3(461.2f, 65.0f, -322.0f), new Vector3(18f, 178f, 0f)),// 4. Toàn cảnh trên cao
            new Landmark(new Vector3(682.0f, 1.8f, -733.0f), new Vector3(0f, 210f, 0f)), // 5. Quầy kem ven hồ
            new Landmark(new Vector3(676.0f, 1.8f, -648.0f), new Vector3(0f, 180f, 0f)), // 6. Trò chơi dân gian
            new Landmark(new Vector3(678.0f, 2.0f, -710.0f), new Vector3(0f, 90f, 0f)),  // 7. Tòa Nhà VNPT
            new Landmark(new Vector3(664.0f, 0.5f, -642.0f), new Vector3(20f, 230f, 0f)), // 8. Đàn Cá Bơi Ven Hồ
            new Landmark(new Vector3(605.0f, 2.0f, -615.0f), new Vector3(0f, 0f, 0f)),   // 9. Tòa nhà Hàm Cá Mập
            new Landmark(new Vector3(542.0f, 2.0f, -710.0f), new Vector3(0f, 270f, 0f))  // 0. Báo Hà Nội Mới
        };
    }

    void Update()
    {
        // Luôn đảm bảo thời gian chạy bình thường
        if (Time.timeScale < 0.01f) Time.timeScale = 1.0f;

        HandleInputShortcuts();
        HandleCameraRotation();
        HandleCameraMovement();
    }

    void HandleInputShortcuts()
    {
        // Esc: Mở chuột | Click chuột: Khóa chuột để xoay tự do
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorLocked(false);
        }
        else if (Input.GetMouseButtonDown(0) && !cursorLocked)
        {
            SetCursorLocked(true);
        }

        // Phím 1 -> 0: Dịch chuyển nhanh
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) Teleport(0);
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) Teleport(1);
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) Teleport(2);
        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4)) Teleport(3);
        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5)) Teleport(4);
        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6)) Teleport(5);
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7)) Teleport(6);
        if (Input.GetKeyDown(KeyCode.Alpha8) || Input.GetKeyDown(KeyCode.Keypad8)) Teleport(7);
        if (Input.GetKeyDown(KeyCode.Alpha9) || Input.GetKeyDown(KeyCode.Keypad9)) Teleport(8);
        if (Input.GetKeyDown(KeyCode.Alpha0) || Input.GetKeyDown(KeyCode.Keypad0)) Teleport(9);

        // Phím R: Quay về vị trí ban đầu
        if (Input.GetKeyDown(KeyCode.R))
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            Vector3 euler = initialRotation.eulerAngles;
            pitch = euler.x;
            if (pitch > 180f) pitch -= 360f;
            yaw = euler.y;
        }
    }

    void HandleCameraRotation()
    {
        // Cho phép xoay khi chuột được khóa, hoặc khi người dùng giữ chuột phải / chuột trái kéo
        bool allowRotate = cursorLocked || Input.GetMouseButton(1) || Input.GetMouseButton(0);

        if (allowRotate)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            if (Mathf.Abs(mouseX) > 0.0001f || Mathf.Abs(mouseY) > 0.0001f)
            {
                yaw += mouseX;
                pitch -= mouseY;
                pitch = Mathf.Clamp(pitch, -upDownLimit, upDownLimit);
            }
        }

        // Bổ sung phím Q / E để quay trái / quay phải bằng bàn phím
        if (Input.GetKey(KeyCode.Q)) yaw -= 75f * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) yaw += 75f * Time.deltaTime;

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleCameraMovement()
    {
        Vector3 moveInput = Vector3.zero;

        // Đọc phím Legacy Input
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.Z)) moveInput += Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveInput -= Vector3.forward;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveInput += Vector3.right;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveInput -= Vector3.right;
        if (Input.GetKey(KeyCode.Space)) moveInput += Vector3.up;
        if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl)) moveInput -= Vector3.up;

        // Fallback đọc trục Axis
        try
        {
            float axisH = Input.GetAxisRaw("Horizontal");
            float axisV = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(axisH) > 0.1f && Mathf.Approximately(moveInput.x, 0f)) moveInput.x = axisH;
            if (Mathf.Abs(axisV) > 0.1f && Mathf.Approximately(moveInput.z, 0f)) moveInput.z = axisV;
        }
        catch { }

#if ENABLE_INPUT_SYSTEM
        // Fallback cho New Input System nếu có
        try
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) moveInput += Vector3.forward;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) moveInput -= Vector3.forward;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveInput += Vector3.right;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveInput -= Vector3.right;
                if (kb.spaceKey.isPressed) moveInput += Vector3.up;
                if (kb.cKey.isPressed || kb.leftCtrlKey.isPressed) moveInput -= Vector3.up;
            }
        }
        catch { }
#endif

        // Ensure speed is valid
        if (moveSpeed <= 0.1f) moveSpeed = 12.0f;
        if (fastSpeed <= 0.1f) fastSpeed = 32.0f;

        if (moveInput.sqrMagnitude > 0.0001f)
        {
            bool isFast = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            float speed = isFast ? fastSpeed : moveSpeed;

            // Di chuyển theo hướng nhìn của camera, sử dụng unscaledDeltaTime để tránh bị kẹt nếu Time.timeScale = 0
            Vector3 worldMove = transform.TransformDirection(moveInput.normalized);
            transform.position += worldMove * speed * Time.unscaledDeltaTime;
        }
    }

    public void Teleport(int index)
    {
        if (landmarks == null || index < 0 || index >= landmarks.Length) return;

        Landmark lm = landmarks[index];
        transform.position = lm.position;
        yaw = lm.rotation.y;
        pitch = lm.rotation.x;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -upDownLimit, upDownLimit);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void SetCursorLocked(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SetCursorLocked(false);
        }
    }

    void OnDisable()
    {
        SetCursorLocked(false);
    }
}
