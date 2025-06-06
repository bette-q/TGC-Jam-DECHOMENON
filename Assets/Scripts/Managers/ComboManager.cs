using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [SerializeField] private SocketManager socketManager;

    void Attach(GameObject child, GameObject parent, string childSocketName, string parentSocketName)
    {
        Transform childSocket = child.transform.Find(childSocketName);
        Transform parentSocket = parent.transform.Find(parentSocketName);

        if (childSocket == null || parentSocket == null)
        {
            Debug.LogError($"Missing sockets on {child.name} or {parent.name}");
            return;
        }

        // Align rotation
        Quaternion rotationOffset = Quaternion.Inverse(childSocket.rotation) * child.transform.rotation;
        child.transform.rotation = parentSocket.rotation * rotationOffset;

        // Snap position
        Vector3 socketOffset = child.transform.position - childSocket.position;
        child.transform.position = parentSocket.position + socketOffset;

        // Leaf-specific outward rotation
        if (childSocketName == "SocketLeaf")
        {
            Renderer rend = child.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                Vector3 leafWorldPos = childSocket.position;
                Vector3 meshCenterWorld = rend.bounds.center;
                Vector3 outwardDir = (meshCenterWorld - leafWorldPos).normalized;

                Vector3 comboForward = parentSocket.forward;
                Vector3 rotateAxis = Vector3.Cross(comboForward, outwardDir);

                if (rotateAxis.sqrMagnitude > 0.0001f)
                {
                    Quaternion look = Quaternion.LookRotation(outwardDir, rotateAxis);
                    child.transform.rotation = look;

                    Vector3 correctedOffset = child.transform.position - childSocket.position;
                    child.transform.position = parentSocket.position + correctedOffset;
                }
            }
            else
            {
                child.transform.RotateAround(childSocket.position, childSocket.right, 90f);
            }
        }

        child.transform.SetParent(parentSocket);
    }

    public void BuildFromOrder(List<GameObject> arrangedPrefabs, bool isGreen)
    {
        if (arrangedPrefabs == null || arrangedPrefabs.Count < 2 || arrangedPrefabs.Count > 5)
        {
            Debug.LogError("Combo must have between 2 and 5 organs.");
            return;
        }

        GameObject comboRoot = new GameObject("Combo");
        GameObject previousOrgan = null;

        for (int i = 0; i < arrangedPrefabs.Count; i++)
        {
            GameObject organPrefab = arrangedPrefabs[i];
            GameObject organInstance = Instantiate(organPrefab);
            organInstance.transform.SetParent(comboRoot.transform);

            if (i == 0)
            {
                // Root organ
                organInstance.transform.localPosition = Vector3.zero;
                organInstance.transform.localRotation = Quaternion.identity;
            }
            else
            {
                bool isLast = (i == arrangedPrefabs.Count - 1);
                string childSocket = isLast ? "SocketLeaf" : "SocketHead";
                string parentSocket = "SocketTail";

                Attach(organInstance, previousOrgan, childSocket, parentSocket);
            }

            previousOrgan = organInstance;
        }

        LayerUtils.SetLayerRecursively(comboRoot, LayerMask.NameToLayer("VisualLayer"));

        socketManager.AttachRandom(comboRoot, isGreen);
    }
}
