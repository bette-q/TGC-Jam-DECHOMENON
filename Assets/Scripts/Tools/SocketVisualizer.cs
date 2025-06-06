// File: Assets/Scripts/SocketVisualizer.cs
using UnityEngine;

[ExecuteAlways]
public class SocketVisualizer : MonoBehaviour
{
    [Tooltip("Radius of each socket gizmo sphere")]
    public float gizmoRadius = 0.05f;
    public SocketDatabase db;

    private void OnDrawGizmos()
    {
        if (db == null) return;

        var data = db.torsoData;
        if (string.IsNullOrEmpty(data.prefabName) || data.sockets == null) return;

        // DEBUG: log world positions so you can verify they're distinct
        for (int i = 0; i < data.sockets.Count; i++)
        {
            var entry = data.sockets[i];
            Vector3 worldPos = transform.TransformPoint(entry.localPosition);
            Debug.Log($"[Visualizer] Socket[{i}] '{entry.name}' at {worldPos}");
        }

        // Draw a small wire-sphere at each socket
        Gizmos.color = Color.yellow;
        for (int i = 0; i < data.sockets.Count; i++)
        {
            Vector3 worldPos = transform.TransformPoint(data.sockets[i].localPosition);
            Gizmos.DrawWireSphere(worldPos, gizmoRadius);
        }
    }
}
