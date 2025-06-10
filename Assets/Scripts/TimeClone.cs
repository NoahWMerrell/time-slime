using UnityEngine;

using System.Collections.Generic;
// using UnityEngine;

public class TimeClone : MonoBehaviour
{
    private List<PlayerRecorder.Snapshot> snapshots;
    private float startTime;
    private int index = 0;

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
            transform.localEulerAngles = new Vector3(0, snapshots[index].facingRight ? 0 : 180, 0); // Flip the clone by rotation to match PlayerMovement's Flip() method
            transform.localScale = snapshots[index].localScale;  // Keep scale

            index++;
        }
    }
}

