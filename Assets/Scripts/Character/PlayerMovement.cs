using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float maxForwardSpeed = 25;
    public float maxSidewaysSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 25f;
    public float airAcceleration = 10f;
    public float sprintSpeedMultiplier = 1.5f;  // Speed multiplier when sprinting
    
    [Header("Jumping")]
    public float jumpHeight = 2f;
    public float gravity = -30f;
    
    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float mouseSmoothing = 0.1f;
    
    [Header("Sprint FOV")]
    public float sprintFOVIncrease = 10f;  // How much to increase FOV when sprinting
    public float fovChangeSpeed = 5f;      // How fast FOV changes
    
    [Header("Physics")]
    public float pushbackResistance = 0.5f;
    public float pushbackDecay = 5f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 externalVelocity;
    private Vector3 horizontalVelocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private Camera playerCamera;
    private float defaultFOV;
    private bool isSprinting;
    
    // Smoothing variables
    private Vector2 smoothedMouseDelta;
    private Vector2 currentMouseDelta;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // Store the default FOV
        if (playerCamera != null)
            defaultFOV = playerCamera.fieldOfView;
            
        // Lock cursor to center of screen
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
        HandleJumping();
        ApplyGravity();
        HandleExternalForces();
        UpdateFOV();

        // Move the character (combine input velocity + external forces)
        Vector3 finalVelocity = velocity + externalVelocity;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    void UpdateFOV()
    {
        if (playerCamera == null) return;
        
        // Calculate target FOV based on sprint state
        float targetFOV = isSprinting ? defaultFOV + sprintFOVIncrease : defaultFOV;
        
        // Smoothly transition FOV
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovChangeSpeed * Time.deltaTime);
    }

    void HandleExternalForces()
    {
        // Decay external forces over time
        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, pushbackDecay * Time.deltaTime);
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
        
        // Check for sprint input (Left Shift)
        isSprinting = Input.GetKey(KeyCode.LeftShift) && moveZ > 0.1f;
        
        // Get input direction in local space
        Vector3 inputDirection = new Vector3(moveX, 0, moveZ).normalized;
        
        // Current horizontal velocity in local space
        Vector3 localHorizontalVelocity = transform.InverseTransformDirection(new Vector3(velocity.x, 0, velocity.z));
        
        if (inputDirection.magnitude > 0.1f)
        {
            // Calculate target velocity in local space
            Vector3 targetLocalVelocity = Vector3.zero;
            float currentAcceleration;
            float currentMaxForwardSpeed = maxForwardSpeed;  // Initialize with base value
            float currentMaxSidewaysSpeed = maxSidewaysSpeed;  // Initialize with base value
            // Apply sprint multiplier when sprinting

            if (isSprinting)
            {
                currentMaxForwardSpeed *= sprintSpeedMultiplier;
            }
            
            if (isGrounded)
            {
                currentAcceleration = acceleration;
            }
            else
            {
                // Air movement - more restrictive
                currentAcceleration = airAcceleration;
            }
            
            // Set target velocity with separate forward/sideways limits
            targetLocalVelocity.z = inputDirection.z * currentMaxForwardSpeed; // Forward/backward
            targetLocalVelocity.x = inputDirection.x * currentMaxSidewaysSpeed; // Left/right
            
            // Accelerate towards target velocity
            localHorizontalVelocity = Vector3.MoveTowards(localHorizontalVelocity, targetLocalVelocity, 
                currentAcceleration * Time.deltaTime);
            
            // Clamp each axis separately
            localHorizontalVelocity.z = Mathf.Clamp(localHorizontalVelocity.z, -currentMaxForwardSpeed, currentMaxForwardSpeed);
            localHorizontalVelocity.x = Mathf.Clamp(localHorizontalVelocity.x, -currentMaxSidewaysSpeed, currentMaxSidewaysSpeed);
        }
        else
        {
            // Decelerate when no input
            if (isGrounded)
            {
                localHorizontalVelocity = Vector3.MoveTowards(localHorizontalVelocity, Vector3.zero, 
                    deceleration * Time.deltaTime);
            }
            // In air, maintain horizontal velocity (no friction)
        }
        
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

    // PUBLIC METHOD: Other objects can call this to push the player
    public void AddExternalForce(Vector3 force)
    {
        externalVelocity += force * pushbackResistance;
    }

    // Handle collisions with other CharacterControllers
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Skip if hitting ground/floor
        if (hit.normal.y > 0.3f) return;

        // Get push direction (away from player to other object)
        Vector3 push = (hit.transform.position - transform.position);
        push.y = 0; // Only horizontal push
        push = push.normalized;

        // Check if the other object has our NPC script
        Pathfinding_Hostile hostile = hit.gameObject.GetComponent<Pathfinding_Hostile>();
        if (hostile != null)
        {
            // Push the NPC
            hostile.AddExternalForce(push * 3f);
        }

        // Check if the other object has a Rigidbody
        Rigidbody rb = hit.gameObject.GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            // Push the rigidbody object
            rb.AddForce(push * 500f);
        }

        // Always get pushed back ourselves from any collision
        AddExternalForce(-push * 2f);
    }
}