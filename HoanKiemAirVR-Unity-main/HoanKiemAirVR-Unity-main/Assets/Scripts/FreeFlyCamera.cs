using UnityEngine;

/// <summary>
/// A smooth, versatile Free-Fly Camera to explore Hoàn Kiếm Lake.
/// - Hold Right Mouse Button (or press Space to toggle cursor) to look around.
/// - W/A/S/D to move forward/backward/left/right.
/// - E to fly up, Q to fly down.
/// - Left Shift to sprint / boost speed.
/// - Mouse Scrollwheel to adjust movement speed dynamically.
/// </summary>
public class FreeFlyCamera : MonoBehaviour
{
    [Header("Movement")]
    public float normalSpeed = 25f;
    public float fastSpeed = 60f;
    public float slowSpeed = 8f;
    public float speedChangeSensitivity = 5f;

    [Header("Look")]
    public float lookSensitivity = 3f;
    public bool requireRightClick = true;

    private float currentSpeed;
    private float yaw = 0f;
    private float pitch = 0f;
    private bool cursorLocked = false;

    void Start()
    {
        currentSpeed = normalSpeed;
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        if (pitch > 180f) pitch -= 360f;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        if (!requireRightClick)
        {
            SetCursorLock(true);
        }
    }

    void Update()
    {
        HandleSpeedChange();
        HandleRotation();
        HandleMovement();
    }

    void HandleSpeedChange()
    {
        // Adjust speed with mouse scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            normalSpeed = Mathf.Clamp(normalSpeed + scroll * speedChangeSensitivity * 10f, 2f, 200f);
            fastSpeed = normalSpeed * 2.5f;
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed = fastSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            currentSpeed = slowSpeed;
        }
        else
        {
            currentSpeed = normalSpeed;
        }
    }

    void HandleRotation()
    {
        bool isLooking = false;

        if (requireRightClick)
        {
            if (Input.GetMouseButtonDown(1))
            {
                SetCursorLock(true);
            }
            if (Input.GetMouseButtonUp(1))
            {
                SetCursorLock(false);
            }
            isLooking = Input.GetMouseButton(1);
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SetCursorLock(false);
            }
            if (Input.GetMouseButtonDown(0))
            {
                SetCursorLock(true);
            }
            isLooking = cursorLocked;
        }

        if (isLooking)
        {
            float mouseX = Input.GetAxis("Mouse X") * lookSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * lookSensitivity;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -89f, 89f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }

    void HandleMovement()
    {
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move += transform.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move -= transform.forward;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) move += transform.right;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) move -= transform.right;
        if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Space)) move += Vector3.up;
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) move -= Vector3.up;

        if (move.sqrMagnitude > 0.001f)
        {
            transform.position += move.normalized * currentSpeed * Time.deltaTime;
        }
    }

    void SetCursorLock(bool locked)
    {
        cursorLocked = locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            SetCursorLock(false);
        }
    }

    void OnDisable()
    {
        SetCursorLock(false);
    }
}
