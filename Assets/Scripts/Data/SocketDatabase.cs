// File: Assets/Scripts/SocketDatabase.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SocketDatabase : MonoBehaviour
{
    public static SocketDatabase Instance { get; private set; }

    [Header("Parsed data for the single torso prefab")]
    public TorsoData torsoData;

    [Header("Parsed data for all organ prefabs")]
    public List<OrganData> allOrgans = new List<OrganData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public OrganData GetOrganData(string prefabName)
    {
        return allOrgans.Find(o => o.prefabName == prefabName);
    }
    public Transform GetTorsoRoot(GameObject torsoInstance)
    {
        // fast name lookup; will return the first Transform named "torso_root"
        return torsoInstance.transform
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == "torso_root");
    }

    public Transform GetOrganRoot(GameObject organInstance)
    {
        return organInstance.transform
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == "organ_root");
    }

    public GameObject GetTorsoPrefab()
    {
        return torsoData.torsoPrefab;
    }

    public GameObject GetOrganPrefab(string prefabName) 
    {
        foreach(var od in allOrgans)
        {
            if(od.prefabName.Equals(prefabName))
            {
                return od.organPrefab;
            }
        }

        return null;
    }

    [ContextMenu("Print All Socket Info")]
    public void PrintAllInfo()
    {
        // Print torso info
        if (string.IsNullOrEmpty(torsoData.prefabName))
        {
            Debug.LogWarning("SocketDatabase: torsoData.prefabName is empty. No torso data to print.");
        }
        else
        {
            Debug.Log($"--- Torso: {torsoData.prefabName} ({torsoData.sockets.Count} sockets) ---");
            for (int i = 0; i < torsoData.sockets.Count; i++)
            {
                var s = torsoData.sockets[i];
                Debug.Log($"[{i}] {s.name} ¡ú LocalPos {s.localPosition}, LocalRot {s.localRotation.eulerAngles}");
            }
        }

        // Print each organ info
        if (allOrgans == null || allOrgans.Count == 0)
        {
            Debug.LogWarning("SocketDatabase: No organs parsed.");
        }
        else
        {
            Debug.Log($"--- Organs ({allOrgans.Count}) ---");
            foreach (var od in allOrgans)
            {
                Debug.Log($"Organ: {od.prefabName}");
                Debug.Log($"    Head ¡ú LocalPos {od.head.localPosition}, LocalRot {od.head.localRotation.eulerAngles}");
                Debug.Log($"    Tail ¡ú LocalPos {od.tail.localPosition}, LocalRot {od.tail.localRotation.eulerAngles}");
                Debug.Log($"    Leaf ¡ú LocalPos {od.leaf.localPosition}, LocalRot {od.leaf.localRotation.eulerAngles}");
            }
        }
    }
}
