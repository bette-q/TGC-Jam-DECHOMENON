using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SocketBinding
{
    public string socketName;     // e.g. "torso_socket_0"
    public Transform socket;      // the generated socket transform under torsoRoot
    public Transform comboAnchor; // child named "comboAnchor"
}

public class SocketManager : MonoBehaviour
{
    private Transform torsoRoot;
    private List<SocketBinding> runtimeBindings = new List<SocketBinding>();
    private Dictionary<int, GameObject> attached = new Dictionary<int, GameObject>();

    [Tooltip("How many of the first sockets count as green")]
    [SerializeField] private int greenSocketCt = 6;

    /// <summary>
    /// Call this with the instantiated torso GameObject.
    /// </summary>
    public void SetTorso(GameObject torsoInstance)
    {
        // 1) find the correct "torso_root" on this instance
        torsoRoot = SocketDatabase.Instance
            .GetTorsoRoot(torsoInstance);

        if (torsoRoot == null)
        {
            Debug.LogError($"SocketManager: could not find torso_root on {torsoInstance.name}");
            return;
        }

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

        // Remove any old sockets
        foreach (Transform child in torsoRoot)
            if (child.name.StartsWith("torso_socket_"))
                DestroyImmediate(child.gameObject);

        // Re-create sockets under torsoRoot
        foreach (var entry in data.sockets)
        {
            var sockGO = new GameObject(entry.name);
            sockGO.transform.SetParent(torsoRoot, false);

            // world‐position & rotation via TransformPoint, then parent
            Vector3 worldPos = torsoRoot.TransformPoint(entry.localPosition);
            Quaternion worldRot = torsoRoot.rotation * entry.localRotation;

            sockGO.transform.position = worldPos;
            sockGO.transform.rotation = worldRot;
            sockGO.transform.localScale = Vector3.one;

            // comboAnchor child
            var anchorGO = new GameObject("comboAnchor");
            anchorGO.transform.SetParent(sockGO.transform, false);

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

        // align organ_socket_head (organRoot must have been parsed from organ_root bone)
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
        attached[i] = organRoot;
    }
}
