// File: Assets/Scripts/SocketManager.cs
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SocketBinding
{
    public string socketName;     // e.g. "torso_socket_0"
    public Transform socket;      // the generated socket transform under torsoRoot
    public Transform comboAnchor; // child named "comboAnchor" or same as socket
}

public class SocketManager : MonoBehaviour
{
    private Transform torsoRoot;
    private List<SocketBinding> runtimeBindings = new List<SocketBinding>();
    private Dictionary<int, GameObject> attached = new Dictionary<int, GameObject>();

    [Tooltip("How many of the first sockets count as green")]
    [SerializeField] private int greenSocketCt = 3;

    public void SetTorso(Transform torso)
    {
        torsoRoot = torso;
        BindSocketsToInstance();
    }

    private void BindSocketsToInstance()
    {
        runtimeBindings.Clear();
        if (torsoRoot == null)
        {
            Debug.LogError("SocketManager: torsoRoot is null.");
            return;
        }

        var data = SocketDatabase.Instance.torsoData;
        if (string.IsNullOrEmpty(data.prefabName))
        {
            Debug.LogError("SocketManager: torsoData.prefabName empty; run parser first.");
            return;
        }

        // Remove any previously generated sockets
        for (int i = torsoRoot.childCount - 1; i >= 0; i--)
        {
            var child = torsoRoot.GetChild(i);
            if (child.name.StartsWith("torso_socket_"))
                DestroyImmediate(child.gameObject);
        }

        // For each entry in the database, create a socket under torsoRoot
        foreach (var entry in data.sockets)
        {
            // 1) Create an empty GameObject named exactly like the socket
            var sockGO = new GameObject(entry.name);
            sockGO.transform.SetParent(torsoRoot, false);

            // 2) Apply local position & rotation from the parsed data
            sockGO.transform.localPosition = entry.localPosition;
            sockGO.transform.localRotation = entry.localRotation;
            sockGO.transform.localScale = Vector3.one;

            // 3) Optionally create or find a "comboAnchor" child under this socket
            //    Here we just use the socket itself as the anchor, but you could do:
            GameObject anchorGO = new GameObject("comboAnchor");
            anchorGO.transform.SetParent(sockGO.transform, false);
            anchorGO.transform.localPosition = Vector3.zero;
            anchorGO.transform.localRotation = Quaternion.identity;
            anchorGO.transform.localScale = Vector3.one;

            // 4) Add to runtimeBindings
            runtimeBindings.Add(new SocketBinding
            {
                socketName = entry.name,
                socket = sockGO.transform,
                comboAnchor = anchorGO.transform
            });
        }
    }

    public void AttachRandom(GameObject comboRoot, bool isGreen)
    {
        LayerUtils.SetLayerRecursively(comboRoot, LayerMask.NameToLayer("VisualLayer"));

        if (runtimeBindings.Count == 0)
        {
            Debug.LogError("SocketManager: No sockets bound.");
            return;
        }

        int maxGreen = Mathf.Min(greenSocketCt, runtimeBindings.Count);
        int idx = isGreen
            ? Random.Range(0, maxGreen)
            : Random.Range(0, runtimeBindings.Count);

        Debug.LogError("SocketManager: attach at " + idx + "socket");


        AttachBodyPart(idx, comboRoot);
    }

    private void AttachBodyPart(int i, GameObject organRoot)
    {
        if (i < 0 || i >= runtimeBindings.Count) return;

        if (attached.TryGetValue(i, out var old))
        {
            Destroy(old);
            attached.Remove(i);
        }

        var sb = runtimeBindings[i];
        var attachPoint = sb.comboAnchor;

        // align organ_socket_head
        var inputSocket = organRoot.transform.Find("organ_socket_head");
        if (inputSocket == null)
        {
            organRoot.transform.SetPositionAndRotation(attachPoint.position, attachPoint.rotation);
        }
        else
        {
            var deltaRot = attachPoint.rotation * Quaternion.Inverse(inputSocket.rotation);
            organRoot.transform.rotation = deltaRot * organRoot.transform.rotation;
            var deltaPos = attachPoint.position - inputSocket.position;
            organRoot.transform.position += deltaPos;
        }

        organRoot.transform.SetParent(attachPoint, true);

        // fix scale
        var ps = attachPoint.lossyScale;
        var cs = organRoot.transform.lossyScale;
        var fix = new Vector3(ps.x / cs.x, ps.y / cs.y, ps.z / cs.z);
        organRoot.transform.localScale = Vector3.Scale(organRoot.transform.localScale, fix);

        attached[i] = organRoot;
    }
}
