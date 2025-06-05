using System;
using UnityEngine;

[Serializable]
public class SocketData
{
    public string name;
    public Vector3 position;
    public Quaternion rotation;
}

[Serializable]
public class FBXSocketExport
{
    public string fbxName;
    public SocketData[] sockets;
}
