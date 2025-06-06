// File: Assets/Scripts/SocketData.cs
using UnityEngine;
using System.Collections.Generic;

// Represents one socket (name + local transform) for a torso prefab.
[System.Serializable]
public struct TorsoSocketEntry
{
    public string name;             // e.g. "torso_socket_0"
    public Vector3 localPosition;   // offset from ¡°torso_root¡±
    public Quaternion localRotation;
}

// Represents the parsed socket list for one torso prefab.
[System.Serializable]
public class TorsoData
{
    public string prefabName;                  // e.g. "Torso_A" (no "(Clone)")
    public List<TorsoSocketEntry> sockets = new List<TorsoSocketEntry>();
}

// Represents one named socket (head/tail/leaf) for an organ prefab.
[System.Serializable]
public struct OrganSocketEntry
{
    public string name;            // e.g. "organ_socket_head"
    public Vector3 localPosition;  // offset from ¡°organ_root¡±
    public Quaternion localRotation;
}

// Represents parsed head/tail/leaf info for one organ prefab.
[System.Serializable]
public class OrganData
{
    public string prefabName;      // e.g. "LegOrgan"
    public OrganSocketEntry head;
    public OrganSocketEntry tail;
    public OrganSocketEntry leaf;
}
