using System.Collections.Generic;
using UnityEngine;

public class DoorRecorder : MonoBehaviour
{
    public struct Snapshot
    {
        public Vector3 position;
        public bool isOpen;
        public float time;
    }

    private List<Snapshot> snapshots = new();
    private float recordDuration = 3f;
    private DoorController door;

    private void Awake()
    {
        door = GetComponent<DoorController>();
    }

    private void Update()
    {
        // Record current door state
        snapshots.Add(new Snapshot
        {
            position = transform.position,
            isOpen = door.IsOpen(),
            time = Time.time
        });

        // Trim snapshots older than 3 seconds
        while (snapshots.Count > 0 && Time.time - snapshots[0].time > recordDuration)
        {
            snapshots.RemoveAt(0);
        }
    }

    public Snapshot? GetSnapshotFrom(float secondsAgo)
    {
        float targetTime = Time.time - secondsAgo;
        for (int i = snapshots.Count - 1; i >= 0; i--)
        {
            if (snapshots[i].time <= targetTime)
                return snapshots[i];
        }
        return null;
    }
}