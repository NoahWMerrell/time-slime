using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public struct Snapshot
    {
        public Vector2 position;
        public Vector2 velocity;
        public bool facingRight;
        public float time;
        public Vector3 localScale;
        public Color slimeColor;
    }

    private List<Snapshot> snapshots = new();
    private float recordDuration = 3f;

    void Update()
    {
        snapshots.Add(new Snapshot
        {
            position = transform.position,
            velocity = GetComponent<Rigidbody2D>().linearVelocity,
            facingRight = transform.localEulerAngles.y < 90f || transform.localEulerAngles.y > 270f,
            time = Time.time,
            localScale = transform.localScale,
            slimeColor = spriteRenderer.color
        });

        // Trim old data
        while (snapshots.Count > 0 && Time.time - snapshots[0].time > recordDuration)
        {
            snapshots.RemoveAt(0);
        }
    }

    public List<Snapshot> GetSnapshots() => new List<Snapshot>(snapshots);
    
    [System.Serializable]
    public struct CloneSpawnEvent   
    {
        public int cloneID;               // Unique ID for this clone
        public int? parentCloneID;        // Nullable parent ID, null for player clone
        public float timeSinceStart;      // Time of spawn relative to recordingStartTime
        public List<Snapshot> cloneData;  // The snapshots for playback
    }


    public List<CloneSpawnEvent> cloneSpawnHistory = new List<CloneSpawnEvent>();
    public float recordingStartTime;

    void Start()
    {
        recordingStartTime = Time.time;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Optional fallback: if the renderer is on a child
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        } 
}

