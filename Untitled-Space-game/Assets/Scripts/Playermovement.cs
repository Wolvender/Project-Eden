using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashCooldown = 1f;
    private float lastDashTime = -10f;

    [Header("Crouch")]
    public float crouchSpeed = 2f;
    public float crouchScaleY = 0.5f;
    private bool isCrouching = false;
    private Vector3 originalScale;

    [Header("Camera")]
    public Transform cameraHolder;

    private Rigidbody rb;
    private bool isGrounded;
    private Vector2 moveInput;
    private bool jumpPressed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalScale = transform.localScale;

        if (groundCheck == null)
        {
            GameObject check = new GameObject("GroundCheck");
            check.transform.SetParent(transform);
            check.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = check.transform;
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
            jumpPressed = true;
    }

    // Shift — dash
    public void OnDash(InputValue value)
    {
        if (!value.isPressed) return;
        if (Time.time < lastDashTime + dashCooldown) return;

        // Dash in the movement direction, or forward if standing still
        Vector3 forward = cameraHolder.forward;
        Vector3 right = cameraHolder.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 dashDir = (forward * moveInput.y + right * moveInput.x).normalized;
        if (dashDir == Vector3.zero) dashDir = forward;

        rb.AddForce(dashDir * dashForce, ForceMode.Impulse);
        lastDashTime = Time.time;
    }

    // Control — crouch
    public void OnCrouch(InputValue value)
    {
        if (value.isPressed)
        {
            isCrouching = !isCrouching;

            if (isCrouching)
                transform.localScale = new Vector3(originalScale.x, crouchScaleY, originalScale.z);
            else
                transform.localScale = originalScale;
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);

        Vector3 forward = cameraHolder.forward;
        Vector3 right = cameraHolder.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        float speed = isCrouching ? crouchSpeed : moveSpeed;

        Vector3 moveDir = (forward * moveInput.y + right * moveInput.x).normalized;
        Vector3 targetVelocity = moveDir * speed;
        targetVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = targetVelocity;

        if (jumpPressed)
        {
            if (isGrounded)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
            jumpPressed = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}