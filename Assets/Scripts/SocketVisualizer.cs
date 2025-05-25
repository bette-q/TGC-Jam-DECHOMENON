using UnityEngine;

public class SocketVisualizer : MonoBehaviour
{
    public float gizmoLength = 0.2f;

    private void OnDrawGizmos()
    {
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Socket"))
            {

                // Draw position sphere
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(child.position, 0.08f);

      /*          // Draw forward direction (blue)
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(child.position, child.position + child.forward * gizmoLength);

                // Draw up direction (green)
                Gizmos.color = Color.green;
                Gizmos.DrawLine(child.position, child.position + child.up * gizmoLength);

                // Draw right direction (red)
                Gizmos.color = Color.red;
                Gizmos.DrawLine(child.position, child.position + child.right * gizmoLength);*/
            }
        }
    }

}
