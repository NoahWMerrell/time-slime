using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D body;
    private InputSystem_Actions controls;
    private Vector2 moveInput;
    private bool facingRight = true;
    private bool isGrounded = false;
    private bool wasGrounded = false;
    private bool jumpQueued = false;
    private float jumpBufferTime = 0.1f;
    private float jumpBufferCounter = 0f;
    private float jumpCutMultiplier = 0.5f; // Range: 0-1
    private float stretchValue = 0.2f; // Range: 0-1
    private Vector3 originalScale;
    private Vector3 squashedScale;
    private Vector3 stretchedScale;
    private float scaleLerpSpeed = 10f;


    [SerializeField] public float acceleration = 8f;
    [SerializeField] float maxSpeed = 12f;
    [SerializeField] public float jumpForce = 12f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    private float groundCheckRadius = 0.2f;

    [SerializeField] private int maxJumps = 1;
    private int jumpsRemaining;

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

        controls.PlayerInput.Jump.performed += ctx => jumpQueued = true;
        controls.PlayerInput.Jump.performed += ctx => jumpBufferCounter = jumpBufferTime;
        controls.PlayerInput.Jump.canceled += ctx =>
        {
            if (body.linearVelocity.y > 0)
            {
                body.linearVelocity = new Vector2(body.linearVelocityX, body.linearVelocityY * jumpCutMultiplier);
            }
        };

        

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
            float friction = 0.9f; // Lower = faster stop
            Vector2 velocity = body.linearVelocity;
            velocity.x *= friction;

            // Optionally clamp small values to zero
            if (Mathf.Abs(velocity.x) < 0.05f)
                velocity.x = 0f;

            body.linearVelocity = velocity;
        }

        if (!isGrounded && Mathf.Approximately(moveInput.x, 0f))
        {
            float drag = 0.99f; // Lower = faster stop
            Vector2 velocity = body.linearVelocity;
            velocity.x *= drag;

            // Optional: snap to zero for very small values
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

}
