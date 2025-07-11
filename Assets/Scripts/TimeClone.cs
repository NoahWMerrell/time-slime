using UnityEngine;
using System.Collections.Generic;

public class TimeClone : MonoBehaviour
{
    private List<PlayerRecorder.Snapshot> snapshots;
    private float startTime;
    private int index = 0;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem landParticles;
    [SerializeField] private ParticleSystem moveParticles;
    [SerializeField] private Transform groundCheck; // Position for land particle placement
    private Vector2 previousVelocity;
    private bool wasGrounded = false;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void Init(List<PlayerRecorder.Snapshot> recordedSnapshots)
    {
        snapshots = recordedSnapshots;
        startTime = Time.time;
    }

    void Update()
    {
        if (snapshots == null || index >= snapshots.Count)
        {
            Destroy(gameObject); // End of playback
            return;
        }

        float playbackTime = Time.time - startTime;

        while (index < snapshots.Count && snapshots[index].time - snapshots[0].time <= playbackTime)
        {
            transform.position = snapshots[index].position; // Keep position
            transform.localEulerAngles = new Vector3(0, snapshots[index].facingRight ? 0 : 180, 0); // Flip the clone
            transform.localScale = snapshots[index].localScale; // Keep scale

            if (spriteRenderer != null)
                spriteRenderer.color = snapshots[index].slimeColor;

            index++;
        }

        var snapshot = snapshots[index];

        // Movement trail
        if (Mathf.Abs(snapshot.velocity.x) > 0.1f && IsGrounded())
        {
            if (!moveParticles.isPlaying)
                moveParticles.Play();

            moveParticles.transform.position = transform.position;
            SetParticleColor(moveParticles, snapshot.slimeColor);
            Debug.Log("CLONE MOVE PARTICLES");
        }
        else if (moveParticles.isPlaying)
        {
            moveParticles.Stop();
        }

        // Jump detection (y-velocity is going upward, was grounded)
        if (snapshot.velocity.y > 0.5f && wasGrounded && !IsGrounded())
        {
            Debug.Log("CLONE JUMP PARTICLES");
            jumpParticles.transform.position = transform.position;
            SetParticleColor(jumpParticles, snapshot.slimeColor);
            jumpParticles.Play();
        }

        // Land detection
        if (!wasGrounded && IsGrounded())
        {
            Debug.Log("CLONE LAND PARTICLES");
            landParticles.transform.position = groundCheck.position;
            SetParticleColor(landParticles, snapshot.slimeColor);
            landParticles.Play();
        }

        // Track last frame's info
        wasGrounded = IsGrounded();
        previousVelocity = snapshot.velocity;

    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, LayerMask.GetMask("Ground"));
    }


    private void SetParticleColor(ParticleSystem ps, Color baseColor)
    {
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();

        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(DarkenColor(baseColor, 0.4f), 0f),
                new GradientColorKey(baseColor, 0.5f),
                new GradientColorKey(BrightenColor(baseColor, 0.4f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(baseColor.a, 0f),
                new GradientAlphaKey(baseColor.a, 1f)
            }
        );

        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);
    }

    private Color BrightenColor(Color color, float amount)
    {
        return new Color(
            Mathf.Clamp01(color.r + amount),
            Mathf.Clamp01(color.g + amount),
            Mathf.Clamp01(color.b + amount),
            color.a
        );
    }

    private Color DarkenColor(Color color, float amount)
    {
        return new Color(
            Mathf.Clamp01(color.r - amount),
            Mathf.Clamp01(color.g - amount),
            Mathf.Clamp01(color.b - amount),
            color.a
        );
    }

}

