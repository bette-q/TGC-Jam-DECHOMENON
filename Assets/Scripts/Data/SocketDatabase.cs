// File: Assets/Scripts/SocketDatabase.cs
using UnityEngine;
using System.Collections.Generic;

public class SocketDatabase : MonoBehaviour
{
    public static SocketDatabase Instance { get; private set; }

    [Header("After running the parser, these lists will contain all socket data")]
    public List<TorsoData> allTorsos = new List<TorsoData>();
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

    /// <summary>
    /// Return the TorsoData whose prefabName matches (no "(Clone)").
    /// </summary>
    public TorsoData GetTorsoData(string prefabName)
    {
        return allTorsos.Find(t => t.prefabName == prefabName);
    }

    /// <summary>
    /// Return the OrganData whose prefabName matches (no "(Clone)").
    /// </summary>
    public OrganData GetOrganData(string prefabName)
    {
        return allOrgans.Find(o => o.prefabName == prefabName);
    }
}
