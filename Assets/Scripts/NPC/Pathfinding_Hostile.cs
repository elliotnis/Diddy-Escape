using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding_Hostile : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    private CharacterController NPCControl;
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float stoppingDistance = 2f;
    public float gravity = -30f;

    [Header("Detection")]
    public float detectionRange = 10f;
    public LayerMask obstacleLayer = 1;

    [Header("Physics")]
    public float pushForce = 51;
    public float pushbackResistance = 0.3f;
    public float pushbackDecay = 5f;

    private bool playerInRange = false;
    private Vector3 velocity;
    private Vector3 externalVelocity;
    private bool isGrounded;

    void Start()
    {
        // Find player if not assigned
        NPCControl = GetComponent<CharacterController>();
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"[{gameObject.name}] Player found: {player.name}");
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] Player not found! Make sure player GameObject has 'Player' tag.");
            }
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Player manually assigned: {player.name}");
        }
    }

    void Update()
    {
        if (player == null) return;

        CheckGrounded();
        CheckPlayerDistance();

        if (playerInRange)
        {
            FollowPlayer();
        }
        else
        {
            velocity.x = 0;
            velocity.z = 0;
        }

        ApplyGravity();
        HandleExternalForces();

        // Move the NPC using CharacterController (combine movement + pushback)
        Vector3 finalVelocity = velocity + externalVelocity;
        NPCControl.Move(finalVelocity * Time.deltaTime);
    }

    void HandleExternalForces()
    {
        // Decay external forces over time
        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, pushbackDecay * Time.deltaTime);
    }

    void CheckGrounded()
    {
        isGrounded = NPCControl.isGrounded;
    }

    void CheckPlayerDistance()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        playerInRange = distanceToPlayer <= detectionRange;
    }

    void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Only move if not within stopping distance
        if (distanceToPlayer > stoppingDistance)
        {
            // Calculate direction to player (remove Y component for horizontal-only movement)
            Vector3 direction = (player.position - transform.position);
            direction.y = 0; // Remove vertical component
            direction = direction.normalized;

            // Set horizontal velocity instead of directly moving transform
            velocity.x = direction.x * moveSpeed;
            velocity.z = direction.z * moveSpeed;

            // Rotate to face player
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                    rotationSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Stop horizontal movement when close enough
            velocity.x = 0;
            velocity.z = 0;
        }
    }

    void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small downward force to keep grounded
        }
        else
        {
            velocity.y += gravity * Time.deltaTime; // Apply gravity
        }
    }

    // PUBLIC METHOD: Other objects can call this to push the NPC
    public void AddExternalForce(Vector3 force)
    {
        externalVelocity += force * pushbackResistance;
    }

    // Handle collisions with any object
    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Skip if hitting ground/floor
        if (hit.normal.y > 0.3f) return;

        // Get push direction (away from NPC to other object)
        Vector3 push = (hit.transform.position - transform.position);
        push.y = 0; // Only horizontal push
        push = push.normalized;

        // Check if we hit the player
        if (hit.gameObject.CompareTag("Player"))
        {
            PlayerMovement playerMovement = hit.gameObject.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.AddExternalForce(push * pushForce);
            }

            // Get pushed back ourselves (Newton's 3rd law)
            AddExternalForce(-push * pushForce * 0.25f);
        }
        // Check if we hit another NPC
        else
        {
            Pathfinding_Hostile otherNPC = hit.gameObject.GetComponent<Pathfinding_Hostile>();
            if (otherNPC != null)
            {
                // Push the other NPC
                otherNPC.AddExternalForce(push * pushForce * 0.5f);

                // Get pushed back ourselves
                AddExternalForce(-push * pushForce * 0.5f);
            }

            // Check if the other object has a Rigidbody
            Rigidbody rb = hit.gameObject.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                // Push the rigidbody object
                rb.AddForce(push * pushForce * 10f);

                // Get minimal pushback from light objects
                AddExternalForce(-push * pushForce * 0.1f);
            }

            // For any other static object, just get pushed back
            if (otherNPC == null && (rb == null || rb.isKinematic))
            {
                AddExternalForce(-push * pushForce * 0.3f);
            }
        }
    }
}