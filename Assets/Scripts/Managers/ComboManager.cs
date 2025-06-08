using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class ComboManager : MonoBehaviour
{
    [SerializeField] private SocketManager socketManager;
    void Attach(GameObject childGO, GameObject parentGO, string childKey)
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

    /// <summary>
    /// Builds a combo from 2–5 organs, chains them via Attach, then snaps to torso.
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
        for (int i = 0; i < arrangedPrefabs.Count; i++)
        {
            var prefab = arrangedPrefabs[i];
            if (prefab == null) continue;

            GameObject inst = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);
            inst.name = prefab.name;
            inst.transform.localScale = Vector3.one;
            inst.transform.SetParent(comboRoot.transform, false);

            var animators = inst.GetComponentsInChildren<Animator>(true);
            foreach (var anim in animators)
            {
                if (isGreen)
                {
                    // Entire combo goes “valid”
                    anim.ResetTrigger("ActionStop");
                    anim.ResetTrigger("ToValid");
                    anim.SetTrigger("ToValid");
                }
                else
                {
                    // Only NON-root pieces stop their action
                    if (i != 0)
                    {
                        anim.ResetTrigger("ToValid");
                        anim.ResetTrigger("ActionStop");
                        anim.SetTrigger("ActionStop");
                    }
                }
            }

            if (i == 0)
            {
                // first organ sits at this GameObject's origin
                inst.transform.localPosition = Vector3.zero;
                inst.transform.localRotation = Quaternion.identity;
            }
            else
            {
                bool isLast = (i == arrangedPrefabs.Count - 1);
                string childKey = isLast ? "organ_socket_leaf" : "organ_socket_head";
                Attach(inst, prev, childKey);
            }

            prev = inst;
        }

        // 3) Layer it
        LayerUtils.SetLayerRecursively(comboRoot, LayerMask.NameToLayer("VisualLayer"));

        // 4) Attach to torso
        socketManager.AttachRandom(comboRoot, isGreen);
    }
    
}



