using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCameraControl : MonoBehaviour
{
    public Camera viewCamera;
    public Transform viewRoot;
    public List<RectTransform> interactiveRects;

    [Header("Rotation Settings")]
    public float rotationSpeed = 200f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minDistance = 0.5f;
    public float maxDistance = 20.0f;

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

        if (Input.GetMouseButtonDown(1))
        {
            if (IsMouseOverAnyPanel() && !IsPointerOverUIButton())
            {
                _isDragging = true;
                _lastMousePosition = Input.mousePosition;
            }
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

            // Horizontal drag �� rotate root around world Y
            float rotY = delta.x * rotationSpeed * Time.deltaTime;
            viewRoot.Rotate(Vector3.up, rotY, Space.World);

            // Vertical drag �� rotate root around its local X
            float rotX = -delta.y * rotationSpeed * Time.deltaTime;
            viewRoot.Rotate(Vector3.right, rotX, Space.Self);
        }
    }


    private void HandleZoom()
    {
        if (viewCamera == null || viewRoot == null) return;

        if (!IsMouseOverAnyPanel())
            return;

        if (IsPointerOverUIButton())
            return;

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

    private bool IsMouseOverAnyPanel()
    {
        // We need to consider Screen Space �C Overlay vs. Screen Space �C Camera:
        // If your Canvas is Screen Space �C Camera, pass canvas.worldCamera into ScreenPointToLocalPointInRectangle.
        foreach (var rect in interactiveRects)
        {
            if (IsMouseOverRect(rect))
                return true;
        }
        return false;
    }

    private bool IsMouseOverRect(RectTransform rt)
    {
        // Determine which camera to use for UI screenspace:
        Canvas canvas = rt.GetComponentInParent<Canvas>();
        Camera uiCam = null;
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            uiCam = canvas.worldCamera;

        Vector2 localPoint;
        bool inside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt,
            Input.mousePosition,
            uiCam,
            out localPoint
        ) && rt.rect.Contains(localPoint);

        return inside;
    }
}
