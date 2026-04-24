using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Sprinting")]
    public float sprintSpeed = 9f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashCooldown = 1f;

    [Header("Crouch")]
    public float crouchSpeed = 2f;
    public float crouchScaleY = 0.5f;

    [Header("Camera")]
    public Transform cameraHolder;
    public Camera playerCamera;

    [Header("Sprint FOV")]
    public float normalFov = 60f;
    public float sprintFov = 75f;
    public float fovLerpSpeed = 8f;

    [Header("Head Bob")]
    public float bobFrequency = 8f;
    public float bobAmplitudeWalk = 0.05f;
    public float bobAmplitudeSprint = 0.12f;
    public float bobReturnSpeed = 6f;

    [Header("Dash FOV")]
    public float dashFov = 95f;
    public float dashFovInSpeed = 20f;   // how fast it spikes up
    public float dashFovOutSpeed = 8f;   // how fast it fades back (slower = more cinematic)

    private float dashFovTimer = 0f;
    private float dashFovDuration = 0.15f; // how long the spike holds before fading

    // ── Private state ──────────────────────────────────────────────────────────

    private Rigidbody rb;
    private Vector2 moveInput;
    private Vector3 originalScale;
    private Vector3 cameraHolderOriginLocal;

    private bool isCrouching;
    private bool isSprinting;
    private bool isGrounded;
    private bool jumpPressed;

    // Cached every frame to avoid recomputing in Update + FixedUpdate
    private bool isMoving;
    private bool sprintActive;

    private float bobTimer;
    private float lastDashTime = -10f;
    private float currentFov;

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;
        cameraHolderOriginLocal = cameraHolder.localPosition;

        if (playerCamera == null)
            playerCamera = cameraHolder.GetComponentInChildren<Camera>();

        if (playerCamera != null)
        {
            playerCamera.fieldOfView = normalFov;
            currentFov = normalFov;
        }

        if (groundCheck == null)
        {
            var check = new GameObject("GroundCheck");
            check.transform.SetParent(transform);
            check.transform.localPosition = new Vector3(0f, -1f, 0f);
            groundCheck = check.transform;
        }
    }

    void Update()
    {
        isMoving = moveInput.sqrMagnitude > 0.01f;

        // Cancel sprint automatically when the player stops moving
        if (!isMoving) isSprinting = false;

        sprintActive = isSprinting && isMoving && !isCrouching;

        HandleFov();
        HandleHeadBob();
    }

    // ── Physics ────────────────────────────────────────────────────────────────

    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(
            groundCheck.position, groundCheckRadius,
            groundLayer, QueryTriggerInteraction.Ignore);

        // Build movement direction relative to camera
        Vector3 forward = cameraHolder.forward;
        Vector3 right = cameraHolder.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        float speed = isCrouching ? crouchSpeed
                    : sprintActive ? sprintSpeed
                    : moveSpeed;

        Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;
        Vector3 targetVelocity = moveDir * speed;

        // Preserve vertical velocity so gravity and jumps aren't cancelled
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;

        if (jumpPressed)
        {
            if (isGrounded)
            {
                // Zero out Y before impulse so jump height is always consistent
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            jumpPressed = false;
        }
    }

    // ── Visual effects ─────────────────────────────────────────────────────────

    void HandleFov()
    {
        if (playerCamera == null) return;

        float targetFov;

        if (dashFovTimer > 0f)
        {
            // Spike up fast, dash FOV takes full priority
            dashFovTimer -= Time.deltaTime;
            targetFov = dashFov;
            currentFov = Mathf.Lerp(currentFov, targetFov, Time.deltaTime * dashFovInSpeed);
        }
        else
        {
            // Fade back to sprint or normal FOV slowly for a trailing feel
            targetFov = sprintActive ? sprintFov : normalFov;
            currentFov = Mathf.Lerp(currentFov, targetFov, Time.deltaTime * dashFovOutSpeed);
        }

        if (Mathf.Abs(playerCamera.fieldOfView - currentFov) > 0.01f)
            playerCamera.fieldOfView = currentFov;
    }

    void HandleHeadBob()
    {
        if (isMoving && isGrounded)
        {
            float amplitude = sprintActive
                ? bobAmplitudeWalk + bobAmplitudeSprint
                : bobAmplitudeWalk;

            bobTimer += Time.deltaTime * bobFrequency;

            // Reuse bobTimer to avoid redundant sin calls
            float sinMain = Mathf.Sin(bobTimer);
            float bobY = sinMain * amplitude;
            float bobX = Mathf.Sin(bobTimer * 0.5f) * amplitude * 0.5f;

            cameraHolder.localPosition = Vector3.Lerp(
                cameraHolder.localPosition,
                cameraHolderOriginLocal + new Vector3(bobX, bobY, 0f),
                Time.deltaTime * bobFrequency);
        }
        else
        {
            // Let the timer decay so it resumes from near-zero next time
            // rather than snapping mid-cycle
            bobTimer = Mathf.Lerp(bobTimer, 0f, Time.deltaTime * bobReturnSpeed);

            cameraHolder.localPosition = Vector3.Lerp(
                cameraHolder.localPosition,
                cameraHolderOriginLocal,
                Time.deltaTime * bobReturnSpeed);
        }
    }

    // ── Input callbacks ────────────────────────────────────────────────────────

    public void OnMove(InputValue value) => moveInput = value.Get<Vector2>();
    public void OnJump(InputValue value) { if (value.isPressed) jumpPressed = true; }
    public void OnSprint(InputValue value)
    {
        if (!value.isPressed) return;
        isSprinting = !isSprinting;
    }

    public void OnDash(InputValue value)
    {
        if (!value.isPressed || Time.time < lastDashTime + dashCooldown) return;

        Vector3 forward = cameraHolder.forward;
        Vector3 right = cameraHolder.right;
        forward.y = 0f; right.y = 0f;
        forward.Normalize(); right.Normalize();

        Vector3 dashDir = (forward * moveInput.y + right * moveInput.x).normalized;
        if (dashDir == Vector3.zero) dashDir = forward;

        rb.AddForce(dashDir * dashForce, ForceMode.Impulse);
        lastDashTime = Time.time;

        // Trigger the FOV spike
        dashFovTimer = dashFovDuration;
    }

    public void OnCrouch(InputValue value)
    {
        if (!value.isPressed) return;
        isCrouching = !isCrouching;
        transform.localScale = isCrouching
            ? new Vector3(originalScale.x, crouchScaleY, originalScale.z)
            : originalScale;
    }
}