// File: Assets/Scripts/ComboTestSolo.cs
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ComboTestSolo : MonoBehaviour
{
    [Header("Drag in 2¨C5 organ prefabs here")]
    public List<GameObject> prefabsToChain = new List<GameObject>();

    [Tooltip("Automatically build on both Play and when script reloads in edit")]
    public bool autoBuild = true;

    private void Awake()
    {
        if (autoBuild)
            BuildComboSolo();
    }

    [ContextMenu("Build Combo Solo")]
    public void BuildComboSolo()
    {
        // sanity
        if (prefabsToChain.Count < 2 || prefabsToChain.Count > 5)
        {
            Debug.LogError("ComboTestSolo: assign between 2 and 5 prefabs in the Inspector.");
            return;
        }

        // clear any previous
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        GameObject previous = null;

        // instantiate & chain
        for (int i = 0; i < prefabsToChain.Count; i++)
        {
            var prefab = prefabsToChain[i];
            if (prefab == null) continue;

            GameObject inst = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
            inst.name = prefab.name;
            inst.transform.localScale = Vector3.one;

            if (i == 0)
            {
                // first organ sits at this GameObject's origin
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
            }
            else
            {
                bool isLast = (i == prefabsToChain.Count - 1);
                string childKey = isLast ? "organ_socket_leaf" : "organ_socket_head";
                ChainTwo(inst, previous, childKey);
            }

            previous = inst;
        }
    }

    private void ChainTwo(GameObject childGO, GameObject parentGO, string childKey)
    {
        var db = SocketDatabase.Instance
                 ?? Object.FindFirstObjectByType<SocketDatabase>();
        if (db == null) { Debug.LogError("No SocketDatabase."); return; }

        var childRoot = db.GetOrganRoot(childGO);
        var parentRoot = db.GetOrganRoot(parentGO);
        if (childRoot == null || parentRoot == null)
        {
            Debug.LogError("Missing organ_root bone.");
            return;
        }

        var cd = db.GetOrganData(childGO.name);
        var pd = db.GetOrganData(parentGO.name);
        if (cd == null || pd == null) return;

        // pick the right child socket
        var cEntry = childKey == "organ_socket_head"
            ? cd.head
            : childKey == "organ_socket_tail"
                ? cd.tail
                : cd.leaf;
        var pEntry = pd.tail;

        // 1) compute world-space socket positions *before* any transforms
        Vector3 pPosWS = parentRoot.TransformPoint(pEntry.localPosition);
        Quaternion pRotWS = parentRoot.rotation * pEntry.localRotation;

        Vector3 cPosWS = childRoot.TransformPoint(cEntry.localPosition);
        Quaternion cRotWS = childRoot.rotation * cEntry.localRotation;

        // 2) compute and apply rotation (no translation yet)
        Quaternion deltaR = pRotWS * Quaternion.Inverse(cRotWS);

        // if you need to flip around X-axis, do it here:
        Vector3 socketX = pRotWS * Vector3.right;
        Quaternion flipX = Quaternion.AngleAxis(180f, socketX);
        Quaternion fullR = flipX * deltaR;

        childGO.transform.rotation = fullR * childGO.transform.rotation;

        // 3) now that we've rotated, recompute *child* socket world-pos
        Vector3 cPosWS2 = childRoot.TransformPoint(cEntry.localPosition);

        // 4) apply translation to line up sockets
        Vector3 deltaP = pPosWS - cPosWS2;
        childGO.transform.position += deltaP;

        // 5) parent under the socket bone
        childGO.transform.SetParent(parentRoot, true);
    }


}
