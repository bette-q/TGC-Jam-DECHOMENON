using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SocketBinding
{
    public string socketName;       // e.g. “Socket_ArmLeft”
    public string comboAnchorName;  // e.g. “ComboAnchor_ArmLeft”
    [HideInInspector] public Transform socket;       // actual Transform under instantiated torso
    [HideInInspector] public Transform comboAnchor;  // actual child under that socket
}

public class SocketManager : MonoBehaviour
{
    [Tooltip("List all socket names + anchor names here (matches children under the torso prefab).")]
    public List<SocketBinding> sockets = new List<SocketBinding>();

    [SerializeField] int greenSocketCt = 3;

    private Transform torsoRoot;   // set by ViewHelper

    private Dictionary<int, GameObject> attached = new Dictionary<int, GameObject>();

    public void SetTorso(Transform torso)
    {
        torsoRoot = torso;
    }

    public void BindSocketsToInstance()
    {
        if (torsoRoot == null)
        {
            Debug.LogError("SocketManager.BindSocketsToInstance: torsoRoot is null.");
            return;
        }

        for (int i = 0; i < sockets.Count; i++)
        {
            var sb = sockets[i];

            // Find the socket Transform under the instantiated torso by name
            Transform foundSocket = torsoRoot.Find(sb.socketName);
            if (foundSocket == null)
            {
                Debug.LogError($"SocketManager: Could not find child '{sb.socketName}' under torso.");
                continue;
            }

            sb.socket = foundSocket;

            // Now find the comboAnchor under that socket by name
            Transform foundAnchor = foundSocket.Find(sb.comboAnchorName);
            if (foundAnchor == null)
            {
                Debug.LogError($"SocketManager: Could not find comboAnchor '{sb.comboAnchorName}' as child of '{sb.socketName}'.");
                continue;
            }

            sb.comboAnchor = foundAnchor;

            // Write back into the list
            sockets[i] = sb;
        }
    }

    private void AttachBodyPart(int socketIdx, GameObject organRoot)
    {
        Debug.Assert(socketIdx >= 0 && socketIdx < sockets.Count,
                     $"Invalid socketIdx: {socketIdx}");

        if (attached.TryGetValue(socketIdx, out var oldPart))
        {
            Destroy(oldPart);
            attached.Remove(socketIdx);
        }

        var sb = sockets[socketIdx];
        if (sb.comboAnchor == null)
        {
            Debug.LogError($"SocketManager: comboAnchor is null for socket index {socketIdx}.");
            return;
        }

        Transform attachPoint = sb.comboAnchor;

        // Find the organ’s internal "SocketHead" (as before)
        Transform inputSocket = organRoot.transform.GetChild(0).Find("SocketHead");

        if (inputSocket == null)
        {
            organRoot.transform.SetPositionAndRotation(attachPoint.position,
                                                       attachPoint.rotation);
        }
        else
        {
            Quaternion deltaRot = attachPoint.rotation * Quaternion.Inverse(inputSocket.rotation);
            organRoot.transform.rotation = deltaRot * organRoot.transform.rotation;

            Vector3 worldDelta = attachPoint.position - inputSocket.position;
            organRoot.transform.position += worldDelta;
        }

        organRoot.transform.SetParent(attachPoint, worldPositionStays: true);

        // Fix scale
        Vector3 parentScale = attachPoint.lossyScale;
        Vector3 currentScale = organRoot.transform.lossyScale;

        Vector3 scaleFix = new Vector3(
            parentScale.x / currentScale.x,
            parentScale.y / currentScale.y,
            parentScale.z / currentScale.z);

        organRoot.transform.localScale = Vector3.Scale(organRoot.transform.localScale, scaleFix);

        attached[socketIdx] = organRoot;
    }

    public void AttachRandom(GameObject organRoot, bool isGreen)
    {
        int socketIdx = isGreen
            ? Random.Range(0, Mathf.Min(greenSocketCt, sockets.Count))
            : Random.Range(0, sockets.Count);

        AttachBodyPart(socketIdx, organRoot);
    }
}
