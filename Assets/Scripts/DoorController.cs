using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Vector3 openOffset = new Vector3(0, 3, 0); // How far the door moves when open
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private bool isOpen = false;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + openOffset;
    }

    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            transform.position = openPosition;
        }
    }

    public void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            transform.position = closedPosition;
        }
    }
}
