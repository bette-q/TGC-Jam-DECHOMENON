using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[AddComponentMenu("Visualization/Torso Socket Visualizer")]
public class TorsoSocketVisualizer : MonoBehaviour
{
    [Tooltip("Radius of each socket gizmo sphere")]
    public float gizmoRadius = 0.05f;
    [Tooltip("Color of the socket gizmo spheres")]
    public Color gizmoColor = Color.yellow;

    private void OnDrawGizmos()
    {
        // 1) grab the database
        var db = SocketDatabase.Instance
                 ?? Object.FindFirstObjectByType<SocketDatabase>();
        var data = db?.torsoData;
        if (data?.sockets == null) return;

        // 2) choose the *correct* root transform
        Transform root = db.GetOrganRoot(gameObject);

        Gizmos.color = gizmoColor;

        // 3) draw
        foreach (var entry in data.sockets)
            DrawSocket(entry, root);
    }

    private void DrawSocket(TorsoSocketEntry entry, Transform parent)
    {
        // FBX-local ¡ú world
        Vector3 worldPos = parent.TransformPoint(entry.localPosition);
        Gizmos.DrawSphere(worldPos, gizmoRadius);

#if UNITY_EDITOR
        Handles.color = gizmoColor;
        Handles.Label(worldPos + Vector3.up * gizmoRadius * 1.2f, entry.name);
#endif
    }
}
