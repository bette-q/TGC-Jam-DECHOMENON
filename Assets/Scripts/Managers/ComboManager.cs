using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class ComboManager : MonoBehaviour
{
    [SerializeField] private SocketManager socketManager;
    void Attach(GameObject child, GameObject parent, string childKey, string parentKey)
    {
        // 1) grab the DB & organ_root bones
        var db = SocketDatabase.Instance
                 ?? Object.FindFirstObjectByType<SocketDatabase>();
        if (db == null)
        {
            Debug.LogError($"ComboManager: No SocketDatabase in scene.");
            return;
        }

        Transform childRoot = db.GetOrganRoot(child);
        Transform parentRoot = db.GetOrganRoot(parent);
        if (childRoot == null || parentRoot == null)
        {
            Debug.LogError($"ComboManager: Missing organ_root on '{child.name}' or '{parent.name}'.");
            return;
        }

        // 2) lookup data entries
        string childPrefab = child.name.Replace("(Clone)", "");
        OrganData cd = db.GetOrganData(childPrefab);
        string parentPrefab = parent.name.Replace("(Clone)", "");
        OrganData pd = db.GetOrganData(parentPrefab);
        if (cd == null || pd == null)
        {
            Debug.LogError($"ComboManager: Missing OrganData for '{childPrefab}' or '{parentPrefab}'.");
            return;
        }


        OrganSocketEntry cEntry = childKey switch
        {
            "organ_socket_head" => cd.head,
            "organ_socket_tail" => cd.tail,
            "organ_socket_leaf" => cd.leaf,
            _ => throw new System.ArgumentException($"Invalid childKey '{childKey}'")
        };

        if (parentKey != "organ_socket_tail")
        {
            Debug.LogError($"ComboManager: unsupported parentKey '{parentKey}'");
            return;
        }
        OrganSocketEntry pEntry = pd.tail;

        // 3) compute both socket world�\poses *before* moving anything
        Vector3 childPosWS0 = childRoot.TransformPoint(cEntry.localPosition);
        Quaternion childRotWS = childRoot.rotation * cEntry.localRotation;


        Vector3 parentPosWS = parentRoot.TransformPoint(pEntry.localPosition);
        Quaternion parentRotWS = parentRoot.rotation * pEntry.localRotation;

        // 4) compute & apply rotation to the whole prefab
        Quaternion deltaR = parentRotWS * Quaternion.Inverse(childRotWS);
        child.transform.rotation = deltaR * child.transform.rotation;

        // 5) **now** recompute child socket pos after rotation
        Vector3 childPosWS1 = childRoot.TransformPoint(cEntry.localPosition);


        // 6) compute & apply translation
        Vector3 deltaP = parentPosWS - childPosWS1;
        child.transform.position += deltaP;

        // 7) parent under the socket bone (worldPositionStays = true)
        child.transform.SetParent(parentRoot, true);
    }
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
            GameObject inst = Instantiate(prefab);
            inst.transform.SetParent(comboRoot.transform, false);

            if (prev != null)
            {
                bool isLast = (i == arrangedPrefabs.Count - 1);
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
    /*
        /// <summary>
        /// Attaches `child` under `parent` by aligning the child’s socket named
        /// `childKey` to the parent’s tail socket—rotating first, then translating.
        /// </summary>
        void Attach(GameObject child, GameObject parent, string childKey, string parentKey)
        {
            var db = SocketDatabase.Instance
                     ?? Object.FindFirstObjectByType<SocketDatabase>();
            if (db == null)
            {
                Debug.LogError("ComboManager: No SocketDatabase in scene.");
                return;
            }

            // find the organ_root bones
            Transform childRoot = db.GetOrganRoot(child);
            Transform parentRoot = db.GetOrganRoot(parent);
            if (childRoot == null || parentRoot == null)
            {
                Debug.LogError($"ComboManager: Missing organ_root on '{child.name}' or '{parent.name}'.");
                return;
            }

            // lookup OrganData entries
            string childPrefab = child.name.Replace("(Clone)", "");
            string parentPrefab = parent.name.Replace("(Clone)", "");
            OrganData cd = db.GetOrganData(childPrefab);
            OrganData pd = db.GetOrganData(parentPrefab);
            if (cd == null || pd == null)
            {
                Debug.LogError($"ComboManager: Missing OrganData for '{childPrefab}' or '{parentPrefab}'.");
                return;
            }

            // select sockets
            OrganSocketEntry cEntry = childKey switch
            {
                "organ_socket_head" => cd.head,
                "organ_socket_tail" => cd.tail,
                "organ_socket_leaf" => cd.leaf,
                _ => throw new System.ArgumentException($"Invalid childKey '{childKey}'")
            };
            if (parentKey != "organ_socket_tail")
            {
                Debug.LogError($"ComboManager: unsupported parentKey '{parentKey}'");
                return;
            }
            OrganSocketEntry pEntry = pd.tail;

            // 1) compute world-space positions & rotations *before* moving
            Vector3 parentPosWS = parentRoot.TransformPoint(pEntry.localPosition);
            Quaternion parentRotWS = parentRoot.rotation * pEntry.localRotation;

            //Vector3 childPosWS = childRoot.TransformPoint(cEntry.localPosition);
            Quaternion childRotWS = childRoot.rotation * cEntry.localRotation;

            // 2) compute rotation delta and apply flip around socket X-axis
            Quaternion deltaR = parentRotWS * Quaternion.Inverse(childRotWS);
            Vector3 socketX = parentRotWS * Vector3.right;
            Quaternion flipX = Quaternion.AngleAxis(180f, socketX);
            Quaternion fullR = flipX * deltaR;
            child.transform.rotation = fullR * child.transform.rotation;

            // 3) recompute child socket world position after rotation
            Vector3 childPosWS2 = childRoot.TransformPoint(cEntry.localPosition);

            // 4) compute translation delta and apply
            Vector3 deltaP = parentPosWS - childPosWS2;
            child.transform.position += deltaP;

            // 5) set parent under the organ_root bone
            child.transform.SetParent(parentRoot, true);
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
                GameObject inst = Instantiate(prefab);
                inst.transform.SetParent(comboRoot.transform, false);

                inst.transform.localScale = Vector3.one;
                if (i == 0)
                {
                    inst.transform.localPosition = Vector3.zero;
                    inst.transform.localRotation = Quaternion.identity;
                }

                var anim = inst.GetComponentInChildren<Animator>();
                if (isGreen)
                {                
                    anim.ResetTrigger("ToValid");
                    anim.SetTrigger("ToValid");
                }
                else
                {
                    if(i == 0)
                    {
                        continue;                    
                    }
                    else
                    {
                        anim.ResetTrigger("ActionStop");
                        anim.SetTrigger("ActionStop");
                    }
                }
                anim.ResetTrigger("ActionStop");
                anim.SetTrigger("ActionStop");
                anim.Update(0f);

                if (prev != null)
                {
                    bool isLast = (i == arrangedPrefabs.Count - 1);
                    string childKey = isLast ? "organ_socket_leaf" : "organ_socket_head";
                    string parentKey = "organ_socket_tail";
                    Attach(inst, prev, childKey, parentKey);
                }

                prev = inst;
            }


            //// tell the driver to follow our root organ (first prefab in the chain)
            //var driver = Object.FindFirstObjectByType<SingleOrganMotionDriver>();
            //if (driver != null && comboRoot != null)
            //    driver.SetActiveOrgan(comboRoot.transform);


            // 3) Layer it
            LayerUtils.SetLayerRecursively(comboRoot, LayerMask.NameToLayer("VisualLayer"));

            // 4) Attach to torso
            socketManager.AttachRandom(comboRoot, isGreen);

            //foreach (var organ in instances)
            //{
            //    organ.transform.SetParent(comboRoot.transform, true);
            //}
        }
    */
}




