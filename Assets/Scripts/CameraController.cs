using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Camera Controls")]
    public float rotationSpeed = 50f;
    public float zoomSpeed = 2f;
    public float minFieldOfView = 30f;
    public float maxFieldOfView = 80f;
    public Vector3 targetPosition = Vector3.zero;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        HandleRotation();
        HandleZoom();
    }

    void HandleRotation()
    {
        // Rotate around the board with Q/E keys
        if (Input.GetKey(KeyCode.Q))
        {
            transform.RotateAround(targetPosition, Vector3.up, -rotationSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.RotateAround(targetPosition, Vector3.up, rotationSpeed * Time.deltaTime);
        }

        // Optional: Mouse rotation (right mouse button)
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            transform.RotateAround(targetPosition, Vector3.up, mouseX * rotationSpeed * Time.deltaTime);
        }
    }

    void HandleZoom()
    {
        // Mouse wheel zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            cam.fieldOfView -= scroll * zoomSpeed;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFieldOfView, maxFieldOfView);
        }
    }
}