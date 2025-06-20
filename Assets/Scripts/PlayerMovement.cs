using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private InputSystem_Actions controls;
    private Vector2 moveInput;
    private bool facingRight = true;
    private bool isGrounded = false;
    private bool wasGrounded = false;
    private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter = 0f;
    private float jumpCutMultiplier = 0.5f; // Range: 0-1
    private float stretchValue = 0.2f; // Range: 0-1
    private Vector3 originalScale;
    private Vector3 squashedScale;
    private Vector3 stretchedScale;
    private float scaleLerpSpeed = 10f;


    [SerializeField] public float acceleration = 50f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] public float jumpForce = 14f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    private float groundCheckRadius = 0.2f;

    [SerializeField] private int maxJumps = 1;
    private int jumpsRemaining;

    void Start()
    {
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
    }

    private void Awake()
    {
        // Grab references from objects
        body = GetComponent<Rigidbody2D>();
        controls = new InputSystem_Actions();
        originalScale = transform.localScale;
        squashedScale = new Vector3(1f + stretchValue, 1f - stretchValue, 1f);
        stretchedScale = new Vector3(1f - stretchValue, 1f + stretchValue, 1f);
    }

    private void OnEnable()
    {
        controls.PlayerInput.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.PlayerInput.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.PlayerInput.Jump.performed += ctx => jumpBufferCounter = jumpBufferTime;
        controls.PlayerInput.Jump.canceled += ctx =>
        {
            if (body.linearVelocity.y > 0)
            {
                body.linearVelocity = new Vector2(body.linearVelocityX, body.linearVelocityY * jumpCutMultiplier);
            }
        };

        controls.PlayerInput.TimeTravel.performed += ctx => TriggerTimeTravel();

        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        float horizontalInput = moveInput.x;

        if (horizontalInput > 0.01f && !facingRight)
        {
            facingRight = true;
            Flip();
        }
        else if (horizontalInput < -0.01f && facingRight)
        {
            facingRight = false;
            Flip();
        }

        // Decrease jumpBufferCounter overtime
        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Accelerate in direction when moving
        float targetForceX = moveInput.x * acceleration;
        body.AddForce(new Vector2(targetForceX, 0f), ForceMode2D.Force);

        // Continue acceleration until maxSpeed is reached
        if (Mathf.Abs(body.linearVelocityX) > maxSpeed)
        {
            body.linearVelocity = new Vector2(Mathf.Sign(body.linearVelocityX) * maxSpeed, body.linearVelocityY);
        }

        // Apply manual friction when grounded and no horizontal input
        if (isGrounded && Mathf.Approximately(moveInput.x, 0f))
        {
            float friction = 0.5f;
            Vector2 velocity = body.linearVelocity;
            velocity.x *= friction;

            if (Mathf.Abs(velocity.x) < 0.05f)
                velocity.x = 0f;

            body.linearVelocity = velocity;
        }

        if (!isGrounded && Mathf.Approximately(moveInput.x, 0f))
        {
            float drag = 0.95f;
            Vector2 velocity = body.linearVelocity;
            velocity.x *= drag;

            if (Mathf.Abs(velocity.x) < 0.05f)
                velocity.x = 0f;

            body.linearVelocity = velocity;
        }

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        // Jumping
        if (jumpBufferCounter > 0 && (isGrounded || jumpsRemaining > 0))
        {
            body.linearVelocity = new Vector2(body.linearVelocityX, jumpForce);
            transform.localScale = stretchedScale;
            if (!isGrounded)
                jumpsRemaining--;

            jumpBufferCounter = 0f;
        }


        // Landing Detection
        if (!wasGrounded && isGrounded)
        {
            // Landed this frame
            transform.localScale = squashedScale;
        }

        // Smoothly return to normal scale
        transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * scaleLerpSpeed);

        // Track grounded state
        wasGrounded = isGrounded;

    }

    void Flip()
    {
        transform.localEulerAngles = new Vector3(0, facingRight ? 0 : 180, 0);
    }
    
    [SerializeField] private GameObject timeClonePrefab;
    [SerializeField] private PlayerRecorder recorder;

    private float timeTravelCooldown = 6f;  // cooldown duration in seconds
    private float lastTimeTravel = -Mathf.Infinity;  // last time triggered, start very negative so it can trigger immediatel

    void TriggerTimeTravel()
    {   

        // Check cooldown
        if (Time.time < lastTimeTravel + timeTravelCooldown)
        {
            Debug.Log("Time travel on cooldown!");
            return;
        }

        // Update last trigger time
        lastTimeTravel = Time.time;

        float now = Time.time;
        float rewindDuration = 3f;
        float currentTimeSinceStart = now - recorder.recordingStartTime;

        // Get current clone data
        List<PlayerRecorder.Snapshot> playerCloneData = recorder.GetSnapshots();
        if (playerCloneData.Count == 0) return;

        // Replay past clones first
        foreach (var pastClone in recorder.cloneSpawnHistory)
        {
            float cloneTime = pastClone.timeSinceStart;
            float timeSinceCloneSpawned = currentTimeSinceStart - cloneTime;

            if (timeSinceCloneSpawned >= 0 && timeSinceCloneSpawned <= rewindDuration)
            {
                float delay = rewindDuration - timeSinceCloneSpawned;
                StartCoroutine(SpawnDelayedClone(pastClone.cloneData, delay));
            }
        }

        // Spawn the main clone
        GameObject playerClone = Instantiate(timeClonePrefab, playerCloneData[0].position, Quaternion.identity);
        playerClone.GetComponent<TimeClone>().Init(playerCloneData);

        // Record clone for future use
        recorder.cloneSpawnHistory.Add(new PlayerRecorder.CloneSpawnEvent
        {
            timeSinceStart = currentTimeSinceStart,
            cloneData = new List<PlayerRecorder.Snapshot>(playerCloneData)
        });
    }




    IEnumerator SpawnDelayedClone(List<PlayerRecorder.Snapshot> cloneData, float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject clone = Instantiate(timeClonePrefab, cloneData[0].position, Quaternion.identity);
        clone.GetComponent<TimeClone>().Init(cloneData);
    }
}
