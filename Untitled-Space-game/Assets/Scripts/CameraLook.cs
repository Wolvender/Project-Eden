using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;
    public Transform cameraHolder;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 0.1f;
    public float verticalClamp = 80f;

    private float xRotation = 0f; // Vertical (pitch)
    private float yRotation = 0f; // Horizontal (yaw)
    private Vector2 lookInput;
    private Rigidbody rb;

    void Start()
    {
        rb = playerBody.GetComponent<Rigidbody>();
        // Freeze rigidbody rotation so physics never interferes with turning
        rb.freezeRotation = true;

        yRotation = playerBody.eulerAngles.y;
        LockCursor();
    }

    void Update()
    {
        LockCursor();

        float mouseX = lookInput.x * mouseSensitivity;
        float mouseY = lookInput.y * mouseSensitivity;

        // Vertical look on camera only
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Horizontal look — accumulate and set directly, never use Rotate()
        yRotation += mouseX;
        playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}