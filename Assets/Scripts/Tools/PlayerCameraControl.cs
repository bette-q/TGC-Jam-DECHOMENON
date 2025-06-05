using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCameraControl : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [Tooltip("The Camera that renders the preview. Should be set to cull only the preview layer.")]
    public Camera viewCamera;

    [Tooltip("The root Transform of the instantiated preview model.")]
    public Transform viewRoot;

    [Header("Rotation Settings")]
    [Tooltip("Degrees per second when dragging right mouse button.")]
    public float rotationSpeed = 200f;

    [Header("Zoom Settings")]
    [Tooltip("How quickly the camera moves in/out on scroll.")]
    public float zoomSpeed = 5f;

    [Tooltip("Minimum allowed distance from camera to viewRoot.")]
    public float minDistance = 2f;

    [Tooltip("Maximum allowed distance from camera to viewRoot.")]
    public float maxDistance = 10f;

    private Vector3 _lastMousePosition;
    private bool _isDragging = false;

    void Update()
    {
        HandleRotation();
        HandleZoom();
        EnsureCameraLooksAtRoot();
    }

    private void HandleRotation()
    {
        // Begin dragging as soon as right mouse is pressed (no UI check)
        if (Input.GetMouseButtonDown(1) && !IsPointerOverUIButton())
        {
            _isDragging = true;
            _lastMousePosition = Input.mousePosition;
        }
        // Stop dragging when right mouse is released
        if (Input.GetMouseButtonUp(1))
        {
            _isDragging = false;
        }

        if (_isDragging && viewRoot != null)
        {
            Vector3 delta = Input.mousePosition - _lastMousePosition;
            _lastMousePosition = Input.mousePosition;

            // Horizontal drag ¡ú rotate root around world Y
            float rotY = delta.x * rotationSpeed * Time.deltaTime;
            viewRoot.Rotate(Vector3.up, rotY, Space.World);

            // Vertical drag ¡ú rotate root around its local X
            float rotX = -delta.y * rotationSpeed * Time.deltaTime;
            viewRoot.Rotate(Vector3.right, rotX, Space.Self);
        }
    }


    private void HandleZoom()
    {
        if (viewCamera == null || viewRoot == null) return;

        // Get scroll input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) < 0.001f) return;

        // Compute current direction from root to camera
        Vector3 rootToCamera = viewCamera.transform.position - viewRoot.position;
        float currentDistance = rootToCamera.magnitude;
        Vector3 direction = rootToCamera.normalized;

        // Compute new distance clamped between minDistance and maxDistance
        float newDistance = currentDistance - (scroll * zoomSpeed);
        newDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);

        // Reposition camera along that direction
        viewCamera.transform.position = viewRoot.position + (direction * newDistance);
    }

    private void EnsureCameraLooksAtRoot()
    {
        if (viewCamera != null && viewRoot != null)
        {
            viewCamera.transform.LookAt(viewRoot.position);
        }
    }

    // Prevent rotating when pointer is over a UI element
    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsPointerOverUIButton()
    {
        // Perform an EventSystem raycast and see if any hit is a Button
        if (EventSystem.current == null) return false;
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        foreach (var r in results)
        {
            if (r.gameObject.GetComponent<UnityEngine.UI.Button>() != null)
                return true;
        }
        return false;
    }
}
