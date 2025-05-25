using UnityEngine;

public class TestCombo : MonoBehaviour
{
    [SerializeField] private GameObject prefabA;
    [SerializeField] private GameObject prefabB;
    [SerializeField] private GameObject prefabC;
    [SerializeField] private SocketManager socketManager;

    void Start()
    {

    }

    void Attach(GameObject child, GameObject parent, string childSocketName, string parentSocketName)
    {
        Transform childSocket = child.transform.Find(childSocketName);
        Transform parentSocket = parent.transform.Find(parentSocketName);

        if (childSocket == null || parentSocket == null)
        {
            Debug.LogError("Missing sockets!");
            return;
        }

        // Align rotation first
        Quaternion rotationOffset = Quaternion.Inverse(childSocket.rotation) * child.transform.rotation;
        child.transform.rotation = parentSocket.rotation * rotationOffset;

        // Recalculate socket position after rotation
        Vector3 socketOffset = child.transform.position - childSocket.position;
        child.transform.position = parentSocket.position + socketOffset;

        // Parent after alignment to preserve world position/rotation
        child.transform.SetParent(parentSocket);
    }

    public void Input()
    {
        GameObject organA = Instantiate(prefabA, Vector3.zero, Quaternion.identity);
        GameObject organB = Instantiate(prefabB);
        GameObject organC = Instantiate(prefabC);

        // Attach B to A
        Attach(organB, organA, "SocketHead", "SocketTail");

        // Attach C to B
        Attach(organC, organB, "SocketHead", "SocketTail");

        socketManager.AttachRandom(organA, false);
    }
}
