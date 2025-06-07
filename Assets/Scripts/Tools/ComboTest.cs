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
        // Clear out any previously spawned children
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        if (prefabsToChain.Count < 2 || prefabsToChain.Count > 5)
        {
            Debug.LogError("ComboTestSolo: assign between 2 and 5 prefabs in the Inspector.");
            return;
        }

        GameObject previous = null;

        // Instantiate & chain
        for (int i = 0; i < prefabsToChain.Count; i++)
        {
            var prefab = prefabsToChain[i];
            if (prefab == null) continue;

            // Always use Instantiate for runtime compatibility
            GameObject inst = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            inst.name = prefab.name;       // clean up the "(Clone)" suffix
            inst.transform.localScale = Vector3.one;

            if (i == 0)
            {
                // Place the first organ at the root
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
            }
            else
            {
                // Chain inst to previous
                bool isLast = (i == prefabsToChain.Count - 1);
                string childKey = isLast ? "organ_socket_leaf" : "organ_socket_head";
                string parentKey = "organ_socket_tail";
                ChainTwo(inst, previous, childKey, parentKey);
            }

            previous = inst;
        }
    }

    private void ChainTwo(GameObject child, GameObject parent, string childKey, string parentKey)
    {
        // Get child socket info
        OrganData odChild = SocketDatabase.Instance.GetOrganData(child.name);
        if (odChild == null) { Debug.LogError($"No OrganData for {child.name}"); return; }

        Vector3 cLocalPos;
        Quaternion cLocalRot;
        switch (childKey)
        {
            case "organ_socket_head":
                cLocalPos = odChild.head.localPosition;
                cLocalRot = odChild.head.localRotation;
                break;
            case "organ_socket_tail":
                cLocalPos = odChild.tail.localPosition;
                cLocalRot = odChild.tail.localRotation;
                break;
            case "organ_socket_leaf":
                cLocalPos = odChild.leaf.localPosition;
                cLocalRot = odChild.leaf.localRotation;
                break;
            default:
                return;
        }

        // Compute child socket world transform
        Vector3 cWorldPos = child.transform.TransformPoint(cLocalPos);
        Quaternion cWorldRot = child.transform.rotation * cLocalRot;

        // Get parent socket info (tail only)
        OrganData odParent = SocketDatabase.Instance.GetOrganData(parent.name);
        if (odParent == null) { Debug.LogError($"No OrganData for {parent.name}"); return; }

        Vector3 pLocalPos = odParent.tail.localPosition;
        Quaternion pLocalRot = odParent.tail.localRotation;

        Vector3 pWorldPos = parent.transform.TransformPoint(pLocalPos);
        Quaternion pWorldRot = parent.transform.rotation * pLocalRot;

        // Align rotation
        Quaternion deltaRot = pWorldRot * Quaternion.Inverse(cWorldRot);
        child.transform.rotation = deltaRot * child.transform.rotation;

        // Align position
        Vector3 deltaPos = pWorldPos - cWorldPos;
        child.transform.position += deltaPos;

        // Finally parent under the previous organ
        child.transform.SetParent(parent.transform, true);
    }
}
