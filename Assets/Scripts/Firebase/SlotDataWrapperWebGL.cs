using System.Collections.Generic;

[System.Serializable]
public class SlotPayloadWrapper
{
    public Dictionary<string, SlotData> sockets;
}

[System.Serializable]
public class SlotData
{
    public List<string> names;
    public bool isGreen;
}