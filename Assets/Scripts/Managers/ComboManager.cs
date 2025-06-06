// File: Assets/Scripts/ComboManager.cs
using System.Collections.Generic;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    [SerializeField] private SocketManager socketManager;

    /// <summary>
    /// Attaches `child` under `parent` by aligning the child¡¯s socket named
    /// `childKey` to the parent¡¯s socket named `parentKey`, using OrganData for both.
    /// </summary>
    void Attach(GameObject child, GameObject parent, string childKey, string parentKey)
    {
        // --- Child socket world transform via OrganData ---
        string childPrefab = child.name.Replace("(Clone)", "");
        OrganData childData = SocketDatabase.Instance.GetOrganData(childPrefab);
        if (childData == null)
        {
            Debug.LogError($"ComboManager: no OrganData for child '{childPrefab}'");
            return;
        }

        Vector3 childLocalPos;
        Quaternion childLocalRot;
        switch (childKey)
        {
            case "organ_socket_head":
                childLocalPos = childData.head.localPosition;
                childLocalRot = childData.head.localRotation;
                break;
            case "organ_socket_tail":
                childLocalPos = childData.tail.localPosition;
                childLocalRot = childData.tail.localRotation;
                break;
            case "organ_socket_leaf":
                childLocalPos = childData.leaf.localPosition;
                childLocalRot = childData.leaf.localRotation;
                break;
            default:
                Debug.LogError($"ComboManager: invalid childKey '{childKey}'");
                return;
        }

        Transform childRoot = child.transform;
        Vector3 childWorldPos = childRoot.TransformPoint(childLocalPos);
        Quaternion childWorldRot = childRoot.rotation * childLocalRot;

        // --- Parent socket world transform via OrganData ---
        string parentPrefab = parent.name.Replace("(Clone)", "");
        OrganData parentData = SocketDatabase.Instance.GetOrganData(parentPrefab);
        if (parentData == null)
        {
            Debug.LogError($"ComboManager: no OrganData for parent '{parentPrefab}'");
            return;
        }

        Vector3 parentLocalPos;
        Quaternion parentLocalRot;
        if (parentKey == "organ_socket_tail")
        {
            parentLocalPos = parentData.tail.localPosition;
            parentLocalRot = parentData.tail.localRotation;
        }
        else
        {
            Debug.LogError($"ComboManager: unsupported parentKey '{parentKey}'");
            return;
        }

        Transform parentRoot = parent.transform;
        Vector3 parentWorldPos = parentRoot.TransformPoint(parentLocalPos);
        Quaternion parentWorldRot = parentRoot.rotation * parentLocalRot;

        // --- Align child to parent ---
        Quaternion deltaRot = parentWorldRot * Quaternion.Inverse(childWorldRot);
        child.transform.rotation = deltaRot * child.transform.rotation;

        Vector3 deltaPos = parentWorldPos - childWorldPos;
        child.transform.position += deltaPos;

        // --- Parent the child under the parent socket anchor ---
        // If you need an actual Transform under parentRoot to attach to,
        // you can create/find a child named parentKey. Here we just parent
        // under parentRoot so it moves together.
        child.transform.SetParent(parentRoot, true);
    }

    /// <summary>
    /// Builds a combo from 2¨C5 organs, chains them via Attach, layers, then snaps to a torso socket.
    /// </summary>
    public void BuildFromOrder(List<GameObject> arrangedPrefabs, bool isGreen)
    {
        if (arrangedPrefabs == null || arrangedPrefabs.Count < 2 || arrangedPrefabs.Count > 5)
        {
            Debug.LogError("ComboManager: Combo must have between 2 and 5 organs.");
            return;
        }

        // 1) Create combo root
        GameObject comboRoot = new GameObject("Combo");
        GameObject prev = null;

        // 2) Instantiate and chain
        for (int i = 0; i < arrangedPrefabs.Count; i++)
        {
            GameObject inst = Instantiate(arrangedPrefabs[i]);
            inst.transform.SetParent(comboRoot.transform, false);

            if (i == 0)
            {
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
            }
            else
            {
                bool last = (i == arrangedPrefabs.Count - 1);
                string childKey = last ? "organ_socket_leaf" : "organ_socket_head";
                string parentKey = "organ_socket_tail";
                Attach(inst, prev, childKey, parentKey);
            }

            prev = inst;
        }

        // 3) Layer the combo
        LayerUtils.SetLayerRecursively(comboRoot, LayerMask.NameToLayer("VisualLayer"));

        // 4) Attach to torso
        socketManager.AttachRandom(comboRoot, isGreen);
    }
}
