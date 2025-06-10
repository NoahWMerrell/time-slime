using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : MonoBehaviour
{
    public struct Snapshot
    {
        public Vector2 position;
        public Vector2 velocity;
        public bool facingRight;
        public float time;
        public Vector3 localScale;
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
            localScale = transform.localScale
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
        public float timeSinceStart; // Not absolute Time.time
        public List<Snapshot> cloneData;
    }

    public List<CloneSpawnEvent> cloneSpawnHistory = new List<CloneSpawnEvent>();
    public float recordingStartTime;

    void Start()
    {
        recordingStartTime = Time.time;
    } 
}

