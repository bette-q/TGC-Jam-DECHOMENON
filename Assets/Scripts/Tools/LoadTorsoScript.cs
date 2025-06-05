using UnityEngine;

public class FBXSocketLoader : MonoBehaviour
{
    public GameObject fbxPrefab;     // Assign your FBX prefab here
    public Transform torsoParent;    // Assign the Torso GameObject in the scene

    /*[ContextMenu("Load Sockets From FBX")]
    void LoadSockets()
    {
        if (fbxPrefab == null || torsoParent == null)
        {
            Debug.LogError("Please assign both FBX prefab and Torso parent.");
            return;
        }

        GameObject fbxInstance = Instantiate(fbxPrefab);
        Transform defSpine = FindChildByName(fbxInstance.transform, "def_spine");

        if (defSpine == null)
        {
            Debug.LogError("def_spine not found.");
            DestroyImmediate(fbxInstance);
            return;
        }

        int count = 0;

        foreach (Transform child in defSpine)
        {
            if (child.name.StartsWith("torso_socket_"))
            {
                // Create new Socket GameObject
                GameObject socketGO = new GameObject($"Socket{count}");
                socketGO.transform.SetParent(torsoParent, false);
                socketGO.transform.position = child.position;
                socketGO.transform.rotation = child.rotation;

                // Add Anchor child
                GameObject anchorGO = new GameObject("Anchor");
                anchorGO.transform.SetParent(socketGO.transform, false);
                anchorGO.transform.localPosition = Vector3.zero;
                anchorGO.transform.localRotation = Quaternion.identity;

                count++;
            }
        }

        DestroyImmediate(fbxInstance);
        Debug.Log($"Imported {count} sockets into Torso.");
    }

    Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindChildByName(child, name);
            if (result != null) return result;
        }
        return null;
    }*/
}
