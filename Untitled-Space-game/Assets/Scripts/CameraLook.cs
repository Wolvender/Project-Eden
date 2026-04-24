using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLook : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody;
    public Transform cameraHolder;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 0.1f;

    [Header("Controller Settings")]
    public float controllerSensitivity = 180f; // degrees per second at full stick deflection

    public float verticalClamp = 80f;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector2 lookInput;
    private Rigidbody rb;
    private PlayerInput playerInput;

    void Start()
    {
        rb = playerBody.GetComponent<Rigidbody>();
        playerInput = playerBody.GetComponent<PlayerInput>(); // requires PlayerInput on the same object
        rb.freezeRotation = true;
        yRotation = playerBody.eulerAngles.y;
        LockCursor();
    }

    void Update()
    {
        LockCursor();

        float mouseX, mouseY;

        // Controller stick is a sustained value (-1 to 1), so scale by deltaTime.
        // Mouse is already a per-frame delta, so no deltaTime needed.
        bool isGamepad = playerInput != null &&
                         playerInput.currentControlScheme == "Gamepad";

        if (isGamepad)
        {
            mouseX = lookInput.x * controllerSensitivity * Time.deltaTime;
            mouseY = lookInput.y * controllerSensitivity * Time.deltaTime;
        }
        else
        {
            mouseX = lookInput.x * mouseSensitivity;
            mouseY = lookInput.y * mouseSensitivity;
        }

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalClamp, verticalClamp);
        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

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