using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility functions to extract ¡°socket¡± transforms from a prefab or instantiated GameObject,
/// based on naming conventions (e.g. any child whose name contains ¡°Socket_¡± is a torso socket,
/// and the single child named ¡°SocketHead¡± inside an organ prefab is its attachment point).
/// </summary>
public static class SocketParser
{
    /// <summary>
    /// Given a torso prefab (not yet instantiated), find all child paths whose names contain "Socket_".
    /// You cannot read Transform hierarchy directly from the asset, so you must instantiate it (inactive)
    /// or use PrefabUtility if in Editor. Here we do a runtime instantiate under a temporary parent.
    /// </summary>
    public static List<Transform> GetTorsoSockets(GameObject torsoPrefab)
    {
        var sockets = new List<Transform>();
        if (torsoPrefab == null) return sockets;

        // 1) Instantiate the torso off-screen (set inactive so Start/Awake/etc. don¡¯t run scripts)
        GameObject temp = Object.Instantiate(torsoPrefab);
        temp.SetActive(false);

        // 2) Walk the hierarchy looking for any Transform whose name contains "Socket_"
        foreach (var t in temp.GetComponentsInChildren<Transform>(includeInactive: true))
        {
            if (t.name.Contains("Socket_"))
            {
                sockets.Add(t);
            }
        }

        // 3) Destroy the temporary instance
        Object.Destroy(temp);

        return sockets;
    }

    /// <summary>
    /// Given an organ prefab, find the child Transform named exactly ¡°SocketHead¡±.
    /// If not found, returns null.
    /// </summary>
    public static Transform GetOrganSocketHead(GameObject organPrefab)
    {
        if (organPrefab == null) return null;

        GameObject temp = Object.Instantiate(organPrefab);
        temp.SetActive(false);

        Transform found = null;
        foreach (var t in temp.GetComponentsInChildren<Transform>(includeInactive: true))
        {
            if (t.name.Equals("SocketHead"))
            {
                found = t;
                break;
            }
        }

        Object.Destroy(temp);
        return found;
    }
}
