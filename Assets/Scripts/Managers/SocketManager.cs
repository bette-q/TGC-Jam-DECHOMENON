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

        // 1) Randomly pick a socket index
        int maxGreen = Mathf.Min(greenSocketCt, runtimeBindings.Count);
        int idx = isGreen
            ? Random.Range(0, maxGreen)
            : Random.Range(0, runtimeBindings.Count);

        // 2) Pull the *one* organ you want to attach *for this test* out of the combo container
        //    (assuming your comboRoot has exactly one direct child when you're calling AttachRandom)
        if (comboRoot.transform.childCount == 0)
        {
            Debug.LogError("SocketManager: comboRoot has no child organs to attach.");
            return;
        }
        GameObject organGO = comboRoot.transform.GetChild(0).gameObject;

        Debug.LogError("SocketManager: socket " + idx);

        // 3) Snap *that* organ onto the torso
        AttachBodyPart(idx, organGO);

        // 4) Finally, parent the entire combo under the torso so it moves with it
        comboRoot.transform.SetParent(torsoRoot, true);

        // record
        attached[idx] = organGO;
    }

    private void AttachBodyPart(int i, GameObject organGO)
    {
        if (i < 0 || i >= runtimeBindings.Count) return;

        // 1) Get the *actual* socket GO under the torso, not just the parsed data
        var sb = runtimeBindings[i];
        Transform socketGO = sb.socket;               // this is the GameObject you created in BindSockets
        Transform attachPoint = sb.comboAnchor;       // for final parenting

        // 2) Find the organ_root on the *organ* instance
        Transform childRoot = SocketDatabase.Instance.GetOrganRoot(organGO);
        if (childRoot == null)
        {
            Debug.LogError("SocketManager: no organ_root on " + organGO.name);
            return;
        }

        // 3) Find the head socket in that hierarchy
        Transform headSocket = childRoot.Find("organ_socket_head");
        if (headSocket == null)
        {
            Debug.LogError("SocketManager: no organ_socket_head under organ_root");
            return;
        }

        // 4) Record child socket world‐pose *before* we move anything
        Vector3 childWS0 = headSocket.position;
        Quaternion childRot0 = headSocket.rotation;

        // 5) Record torso socket world‐pose from the *actual* socket GO
        Vector3 parentWS = socketGO.position;
        Quaternion parentRot = socketGO.rotation;

        // 6) Rotate the *whole* organ so its socket frame matches the torso
        Quaternion deltaR = parentRot * Quaternion.Inverse(childRot0);
        organGO.transform.rotation = deltaR * organGO.transform.rotation;

        // 7) Re‐sample the child socket’s world‐pos *after* rotation
        childWS0 = headSocket.position;

        // 8) Translate the organ so the sockets coincide
        Vector3 deltaP = parentWS - childWS0;
        organGO.transform.position += deltaP;

        // 9) Parent the organ under the socket anchor so it stays attached
        organGO.transform.SetParent(attachPoint, true);
    }

}
