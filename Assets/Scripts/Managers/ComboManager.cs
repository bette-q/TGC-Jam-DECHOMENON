using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [SerializeField] private SocketManager socketManager;

    /// <summary>
    /// Attaches `child` under `parent` by aligning the child¡¯s socket named
    /// `childKey` to the parent¡¯s socket named `parentKey`.
    /// </summary>
    void Attach(GameObject child, GameObject parent, string childKey, string parentKey)
    {
        // 1) Find the organ_root on this child instance
        var childRoot = SocketDatabase.Instance
            .GetOrganRoot(child);
        if (childRoot == null)
        {
            Debug.LogError($"ComboManager: no organ_root on child '{child.name}'");
            return;
        }

        // 2) Lookup child data
        string childPrefab = child.name.Replace("(Clone)", "");
        OrganData childData = SocketDatabase.Instance.GetOrganData(childPrefab);
        if (childData == null)
        {
            Debug.LogError($"ComboManager: no OrganData for child '{childPrefab}'");
            return;
        }

        // pick correct socket entry
        OrganSocketEntry cEntry = childKey switch
        {
            "organ_socket_head" => childData.head,
            "organ_socket_tail" => childData.tail,
            "organ_socket_leaf" => childData.leaf,
            _ => throw new System.ArgumentException($"Invalid childKey '{childKey}'")
        };

        // world©\space of child socket
        Vector3 childWorldPos = childRoot.TransformPoint(cEntry.localPosition);
        Quaternion childWorldRot = childRoot.rotation * cEntry.localRotation;


        // 3) Find parent organ_root
        var parentRoot = SocketDatabase.Instance
            .GetOrganRoot(parent);
        if (parentRoot == null)
        {
            Debug.LogError($"ComboManager: no organ_root on parent '{parent.name}'");
            return;
        }

        // only tail supported as parentKey
        if (parentKey != "organ_socket_tail")
        {
            Debug.LogError($"ComboManager: unsupported parentKey '{parentKey}'");
            return;
        }

        OrganSocketEntry pEntry = childKey switch
        {
            _ => childData.tail   // for now only tail¡úhead chaining 
        };

        Vector3 parentWorldPos = parentRoot.TransformPoint(pEntry.localPosition);
        Quaternion parentWorldRot = parentRoot.rotation * pEntry.localRotation;


        // 4) Align child to parent
        Quaternion deltaRot = parentWorldRot * Quaternion.Inverse(childWorldRot);
        child.transform.rotation = deltaRot * child.transform.rotation;

        Vector3 deltaPos = parentWorldPos - childWorldPos;
        child.transform.position += deltaPos;

        // 5) Parent under the parentRoot so it moves with it
        child.transform.SetParent(parentRoot, true);
    }

    /// <summary>
    /// Builds a combo from 2¨C5 organs, chains them via Attach, then snaps to torso.
    /// </summary>
    public void BuildFromOrder(List<GameObject> arrangedPrefabs, bool isGreen)
    {
        if (arrangedPrefabs == null || arrangedPrefabs.Count < 2 || arrangedPrefabs.Count > 5)
        {
            Debug.LogError("ComboManager: Combo must have between 2 and 5 organs.");
            return;
        }

        // 1) Spawn the combo root
        GameObject comboRoot = new GameObject("Combo");
        GameObject prev = null;

        // 2) Instantiate & chain each prefab
        foreach (var prefab in arrangedPrefabs)
        {
            GameObject inst = Instantiate(prefab);
            inst.transform.SetParent(comboRoot.transform, false);

            if (prev != null)
            {
                bool isLast = (prefab == arrangedPrefabs[^1]);
                string childKey = isLast ? "organ_socket_leaf" : "organ_socket_head";
                string parentKey = "organ_socket_tail";
                Attach(inst, prev, childKey, parentKey);
            }

            prev = inst;
        }

        // 3) Layer it
        LayerUtils.SetLayerRecursively(comboRoot, LayerMask.NameToLayer("VisualLayer"));

        // 4) Attach to torso
        socketManager.AttachRandom(comboRoot, isGreen);
    }
}
