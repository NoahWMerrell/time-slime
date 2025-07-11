using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cameraTransform;
    public Vector2 parallaxMultiplier = new Vector2(0.25f, 0.25f);

    private Vector3 lastCameraPosition;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cameraTransform.position - lastCameraPosition;
        Vector3 newPos = transform.position + new Vector3(delta.x * parallaxMultiplier.x, delta.y * parallaxMultiplier.y, 0f);
        transform.position = newPos;
        lastCameraPosition = cameraTransform.position;
    }

}