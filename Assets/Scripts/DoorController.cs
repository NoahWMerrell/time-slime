using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Vector3 openOffset = new Vector3(0, 3, 0); // How far the door moves when open
    public float moveSpeed = 2f;                      // Speed of door movement

    private Vector3 closedPosition;
    private Vector3 openPosition;
    private Vector3 targetPosition;
    private bool isOpen = false;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
        targetPosition = closedPosition; // Start closed
    }

    void Update()
    {
        // Smoothly move the door toward its target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            targetPosition = openPosition;
        }
    }

    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            targetPosition = closedPosition;
        }
    }

    public void ApplySnapshot(Vector3 position, bool open)
    {
        transform.position = position;
        isOpen = open;
        targetPosition = open ? openPosition : closedPosition;
    }

    public bool IsOpen() => isOpen;
}