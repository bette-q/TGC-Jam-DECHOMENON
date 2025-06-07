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
                string parentKey = "organ_socket_tail";
                ChainTwo(inst, previous, childKey, parentKey);
            }

            previous = inst;
        }
    }

    private void ChainTwo(GameObject childGO, GameObject parentGO,
                          string childKey, string parentKey)
    {
        // 1) get database & bones
        var db = SocketDatabase.Instance
                 ?? Object.FindFirstObjectByType<SocketDatabase>();
        if (db == null)
        {
            Debug.LogError("ComboTestSolo: No SocketDatabase in scene.");
            return;
        }

        Transform childRoot = db.GetOrganRoot(childGO);
        Transform parentRoot = db.GetOrganRoot(parentGO);
        if (childRoot == null || parentRoot == null)
        {
            Debug.LogError("ComboTestSolo: Missing organ_root bone on child or parent.");
            return;
        }

        // 2) lookup socket entries
        var cd = db.GetOrganData(childGO.name);
        var pd = db.GetOrganData(parentGO.name);
        if (cd == null || pd == null) return;

        OrganSocketEntry cEntry = childKey switch
        {
            "organ_socket_head" => cd.head,
            "organ_socket_tail" => cd.tail,
            _ => cd.leaf
        };
        OrganSocketEntry pEntry = pd.tail; // always attach into tail

        // 3) compute world-space socket poses
        Vector3 childSocketWS = childRoot.TransformPoint(cEntry.localPosition);
        Quaternion childRotWS = childRoot.rotation * cEntry.localRotation;

        Vector3 parentSocketWS = parentRoot.TransformPoint(pEntry.localPosition);
        Quaternion parentRotWS = parentRoot.rotation * pEntry.localRotation;

        // --- after you compute childRotWS and parentRotWS ---

        // 1) Compute the basic socket alignment
        Quaternion deltaR = parentRotWS * Quaternion.Inverse(childRotWS);

        // 2) Figure out each socket¡¯s ¡°up¡± in world space
        Vector3 childUpWorld = childRotWS * Vector3.up;            // child's socket-up
        Vector3 parentUpWorld = parentRotWS * Vector3.up;           // parent's socket-up

        // 3) We want childUpWorld ¡ú exactly opposite of parentUpWorld
        Quaternion flip = Quaternion.FromToRotation(childUpWorld, -parentUpWorld);

        // 4) Combine them: first align frames, then flip the up-axis
        Quaternion fullDelta = flip * deltaR;

        // 5) Apply the full rotation to the entire prefab
        childGO.transform.rotation = fullDelta * childGO.transform.rotation;

        // 6) Then apply your position offset and parenting as before
        childGO.transform.position += (parentSocketWS - childSocketWS);
        childGO.transform.SetParent(parentRoot, true);

    }
}
