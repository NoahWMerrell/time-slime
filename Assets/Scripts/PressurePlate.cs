using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public DoorController connectedDoor;
    private int objectsOnPlate = 0;

    public Transform visualTransform;  // Assign this in Inspector (child sprite object)
    private Vector3 originalPosition;
    private Vector3 pressedPosition;
    public float pressDepth = 0.0625f;  // 1 pixel down (assuming 16px = 1 unit)

    private void Start()
    {
        if (visualTransform == null)
            visualTransform = transform.GetChild(0); // fallback: assumes first child is visual

        originalPosition = visualTransform.localPosition;
        pressedPosition = originalPosition + new Vector3(0, -pressDepth, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsValidObject(other))
        {
            objectsOnPlate++;
            if (objectsOnPlate == 1)
            {
                connectedDoor.OpenDoor();
                visualTransform.localPosition = pressedPosition;
            }
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
                visualTransform.localPosition = originalPosition;
            }
        }
    }

    private bool IsValidObject(Collider2D other)
    {
        return other.CompareTag("Player") || other.CompareTag("TimeClone");
    }
}
