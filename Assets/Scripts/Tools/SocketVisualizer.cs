using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[AddComponentMenu("Visualization/Socket Visualizer")]
public class SocketVisualizer : MonoBehaviour
{
    [Tooltip("Radius of each socket gizmo sphere")]
    public float gizmoRadius = 0.05f;

    [Tooltip("Color of the socket gizmo spheres")]
    public Color gizmoColor = Color.yellow;

    [Tooltip("Reference to the socket database holding organ socket data")]
    public SocketDatabase db;

    private void OnDrawGizmos()
    {
        if (db == null) return;

        // Determine the clean prefab name (strip Clone suffix)
        string goName = gameObject.name.Replace("(Clone)", "");
        OrganData organ = db.GetOrganData(goName);
        if (organ == null) return;

        Gizmos.color = gizmoColor;
        Transform t = transform;

        // Draw solid spheres for head, tail, and leaf sockets
        DrawSocket(organ.head, t);
        DrawSocket(organ.tail, t);
        DrawSocket(organ.leaf, t);
    }

    private void DrawSocket(OrganSocketEntry entry, Transform parent)
    {
        Vector3 worldPos = parent.TransformPoint(entry.localPosition);
        Gizmos.DrawSphere(worldPos, gizmoRadius);
#if UNITY_EDITOR
        // Optional label in Scene view
        Handles.color = gizmoColor;
        Handles.Label(worldPos + Vector3.up * gizmoRadius * 1.2f, entry.name);
#endif
    }
}
