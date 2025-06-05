using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCameraControl : MonoBehaviour
{
    public Camera viewCamera;
    public Transform viewRoot;

    [Header("Rotation Settings")]
    public float rotationSpeed = 200f;   // degrees per second

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;         // how fast to move camera
    public float minDistance = 2f;       // closest camera can be
    public float maxDistance = 10f;      // farthest camera can be

    private Vector3 _lastMousePosition;
    private bool _isDragging = false;

    void Update()
    {
        HandleRotation();
        HandleZoom();
    }

    private void HandleRotation()
    {
        // Right mouse button begins drag
        if (Input.GetMouseButtonDown(1) && !IsPointerOverUI())
        {
            _isDragging = true;
            _lastMousePosition = Input.mousePosition;
        }
        // Right mouse button released
        if (Input.GetMouseButtonUp(1))
        {
            _isDragging = false;
        }

        if (_isDragging)
        {
            Vector3 delta = Input.mousePosition - _lastMousePosition;
            _lastMousePosition = Input.mousePosition;

            // Rotate around world Y (horizontal drag) and local X (vertical drag)
            float rotY = delta.x * rotationSpeed * Time.deltaTime;
            float rotX = -delta.y * rotationSpeed * Time.deltaTime;

            // Rotate previewRoot
            viewRoot.Rotate(Vector3.up, rotY, Space.World);
            viewRoot.Rotate(Vector3.right, rotX, Space.Self);
        }
    }

    private void HandleZoom()
    {
        // Zoom with scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            Vector3 dir = viewCamera.transform.forward;
            Vector3 newPos = viewCamera.transform.position + dir * scroll * zoomSpeed;
            float distance = Vector3.Distance(newPos, viewRoot.position);

            if (distance >= minDistance && distance <= maxDistance)
            {
                viewCamera.transform.position = newPos;
            }
        }
    }

    // Prevent rotating when pointer is over UI element
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
