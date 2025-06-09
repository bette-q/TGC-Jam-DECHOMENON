using UnityEngine;

[ExecuteAlways]
public class CameraFollowCanvas : MonoBehaviour
{
    public Camera mainCamera;
    public Canvas canvas;
    public float padding = 0.5f;

    void LateUpdate()
    {
        if (!canvas || !mainCamera) return;

        RectTransform rect = canvas.GetComponent<RectTransform>();
        if (rect == null) return;

        float w = rect.rect.width * canvas.transform.lossyScale.x;
        float h = rect.rect.height * canvas.transform.lossyScale.y;

        float fov = mainCamera.fieldOfView * Mathf.Deg2Rad;
        float aspect = mainCamera.aspect;

        float dH = (h / 2f + padding) / Mathf.Tan(fov / 2f);
        float dW = (w / 2f + padding) / Mathf.Tan(fov / 2f) / aspect;
        float dist = Mathf.Max(dH, dW);

        Vector3 center = canvas.transform.position;
        Vector3 forward = canvas.transform.forward;

        mainCamera.transform.position = center - forward * dist;
        mainCamera.transform.rotation = Quaternion.LookRotation(forward);
    }
}