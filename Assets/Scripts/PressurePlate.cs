using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public DoorController connectedDoor;
    private int objectsOnPlate = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsValidObject(other))
        {
            objectsOnPlate++;
            connectedDoor.OpenDoor();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsValidObject(other))
        {
            objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);
            if (objectsOnPlate == 0)
            {
                connectedDoor.CloseDoor();
            }
        }
    }

    private bool IsValidObject(Collider2D other)
    {
        return other.CompareTag("Player") || other.CompareTag("TimeClone");
    }
}
