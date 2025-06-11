using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float maxForwardSpeed = 25;
    public float maxSidewaysSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 25f;
    public float airAcceleration = 10f;
    
    [Header("Jumping")]
    public float jumpHeight = 2f;
    public float gravity = -30f;
    
    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float mouseSmoothing = 0.1f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 horizontalVelocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private Camera playerCamera;
    
    // Smoothing variables
    private Vector2 smoothedMouseDelta;
    private Vector2 currentMouseDelta;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleJumping();
        ApplyGravity();
        
        // Move the character
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        // Raw mouse input
        Vector2 rawMouseDelta = new Vector2(
            Input.GetAxisRaw("Mouse X") * mouseSensitivity,
            Input.GetAxisRaw("Mouse Y") * mouseSensitivity
        );
        
        // Low-pass filter for smoothing
        currentMouseDelta = Vector2.Lerp(currentMouseDelta, rawMouseDelta, mouseSmoothing);
        smoothedMouseDelta = currentMouseDelta;

        // Apply smoothed mouse look
        float mouseX = smoothedMouseDelta.x;
        float mouseY = smoothedMouseDelta.y;

        // Rotate the player body horizontally
        transform.Rotate(Vector3.up * mouseX);
        
        // Rotate the camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        // Check if grounded
        isGrounded = controller.isGrounded;
        
        // Movement input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Get input direction in local space
        Vector3 inputDirection = new Vector3(moveX, 0, moveZ).normalized;
        
        // Current horizontal velocity in local space
        Vector3 localHorizontalVelocity = transform.InverseTransformDirection(new Vector3(velocity.x, 0, velocity.z));
        

        // Calculate target velocity in local space
        Vector3 targetLocalVelocity = Vector3.zero;
        float currentAcceleration;
        float currentMaxForwardSpeed;
        float currentMaxSidewaysSpeed;
        
        if (isGrounded)
        {
            currentAcceleration = acceleration;

        }
        else
        {
            // Air movement - more restrictive
            currentAcceleration = airAcceleration;
        }
        currentMaxForwardSpeed = maxForwardSpeed;
        currentMaxSidewaysSpeed = maxSidewaysSpeed;
        
        // Set target velocity with separate forward/sideways limits
        targetLocalVelocity.z = inputDirection.z * currentMaxForwardSpeed; // Forward/backward
        targetLocalVelocity.x = inputDirection.x * currentMaxSidewaysSpeed; // Left/right
        
        // Accelerate towards target velocity
        localHorizontalVelocity = Vector3.MoveTowards(localHorizontalVelocity, targetLocalVelocity, 
            currentAcceleration * Time.deltaTime);
        
        // Clamp each axis separately
        localHorizontalVelocity.z = Mathf.Clamp(localHorizontalVelocity.z, -currentMaxForwardSpeed, currentMaxForwardSpeed);
        localHorizontalVelocity.x = Mathf.Clamp(localHorizontalVelocity.x, -currentMaxSidewaysSpeed, currentMaxSidewaysSpeed);

        // Decelerate when no input
        if (isGrounded)
        {
            localHorizontalVelocity = Vector3.MoveTowards(localHorizontalVelocity, Vector3.zero, 
                deceleration * Time.deltaTime);
        }
        // In air, maintain horizontal velocity (no friction)
        
        // Convert back to world space and apply to main velocity
        Vector3 worldHorizontalVelocity = transform.TransformDirection(localHorizontalVelocity);
        velocity.x = worldHorizontalVelocity.x;
        velocity.z = worldHorizontalVelocity.z;
    }

    void HandleJumping()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
    }
}