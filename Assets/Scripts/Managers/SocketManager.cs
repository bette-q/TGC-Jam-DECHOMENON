using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SocketBinding
{
    public string socketName;     
    public Transform socket;      
    public Transform comboAnchor; 
}

public class SocketManager : MonoBehaviour
{
    private Transform torsoRoot;
    private List<SocketBinding> runtimeBindings = new List<SocketBinding>();
    private Dictionary<int, GameObject> attached = new Dictionary<int, GameObject>();

    [SerializeField] private int greenSocketCt = 6;

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
    public List<SocketBinding> GetActiveBindings()
    {
        return runtimeBindings;
    }
    private void ClearSocket(int i)
    {
        if (attached.TryGetValue(i, out var oldCombo))
        {
            // you might want to add some fade-out or pooling instead of Destroy
            Destroy(oldCombo);
            attached.Remove(i);
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

        ClearSocket(idx);

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
                                                      // 
        var db = SocketDatabase.Instance
             ?? Object.FindFirstObjectByType<SocketDatabase>();
        if (db == null) { Debug.LogError("SocketManager: No SocketDatabase."); return; }

        Transform childRoot = db.GetOrganRoot(organGO);
        if (childRoot == null)
        {
            Debug.LogError("SocketManager: no organ_root on " + organGO.name);
            return;
        }

        // B) Look-up OrganData so we can use the head socket offsets
        string prefabName = organGO.name.Replace("(Clone)", "");
        OrganData cd = db.GetOrganData(prefabName);
        if (cd == null) { Debug.LogError("SocketManager: no OrganData for " + prefabName); return; }

        // We always attach HEAD → torso socket here
        var cEntry = cd.head;

        // 2) Compute *world* socket frames BEFORE we move anything
        Vector3 pPosWS = socketGO.position;
        Quaternion pRotWS = socketGO.rotation;

        Vector3 cPosWS = childRoot.TransformPoint(cEntry.localPosition);
        Quaternion cRotWS = childRoot.rotation * cEntry.localRotation;

        // 3) Rotate organ so its head socket frame matches the torso socket frame
        Quaternion deltaR = pRotWS * Quaternion.Inverse(cRotWS);

        // Optional 180° flip around local X (same trick used in Attach)
        Vector3 socketX = pRotWS * Vector3.right;
        Quaternion flipX = Quaternion.AngleAxis(180f, socketX);
        Quaternion fullR = flipX * deltaR;

        organGO.transform.rotation = fullR * organGO.transform.rotation;

        // 4) Re-sample child socket position AFTER rotation
        Vector3 cPosWS2 = childRoot.TransformPoint(cEntry.localPosition);

        // 5) Translate so the two sockets coincide
        Vector3 deltaP = pPosWS - cPosWS2;
        organGO.transform.position += deltaP;

        // 6) Finally parent under the socket anchor so it stays attached
        organGO.transform.SetParent(attachPoint, true);
    }

}
