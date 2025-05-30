using System.Collections.Generic;
using System.Net.Sockets;
using Unity.Burst.Intrinsics;
using UnityEngine;


//add a middle layer between socket and organs
[System.Serializable]
public class SocketBinding 
{
    public Transform socket;       // e.g. socket0
    public Transform comboAnchor;  // child of socket0
}

public class SocketManager : MonoBehaviour
{
    public Transform torsoRoot;
    public List<SocketBinding> sockets;
    [SerializeField] int greenSocketCt = 3;

    //track socket status
    private Dictionary<int, GameObject> attached = new Dictionary<int, GameObject>();

    private void AttachBodyPart(int socketIdx, GameObject organRoot)
    {
        Debug.Assert(socketIdx >= 0 && socketIdx < sockets.Count,
                     $"Invalid socketIdx: {socketIdx}");

        /* -----------------------------------------------------------
            1.  Remove any part already on this socket
        -----------------------------------------------------------*/
        if (attached.TryGetValue(socketIdx, out var oldPart))
        {
            Destroy(oldPart);
            attached.Remove(socketIdx);
        }

        Transform attachPoint = sockets[socketIdx].comboAnchor;

        //find root sockethead
        Transform inputSocket = organRoot.transform.GetChild(0).Find("SocketHead");

        if (inputSocket == null)
        {
            // No marker found – simple snap & parent
            organRoot.transform.SetPositionAndRotation(attachPoint.position,
                                                       attachPoint.rotation);
        }
        else
        {
            /* -------------------------------------------------------
                 ROTATE   : make SocketHead's axes = attachPoint's axes
            ---------------------------------------------------------*/
            Quaternion deltaRot = attachPoint.rotation * Quaternion.Inverse(inputSocket.rotation);
            organRoot.transform.rotation = deltaRot * organRoot.transform.rotation;

            /* -------------------------------------------------------
                 MOVE     : bring SocketHead's pivot to attachPoint
            ---------------------------------------------------------*/
            Vector3 worldDelta = attachPoint.position - inputSocket.position;
            organRoot.transform.position += worldDelta;
        }

        organRoot.transform.SetParent(attachPoint, true);

        /* -----------------------------------------------------------
            SCALE FIX  : ensure world-scale stays 1:1
        -----------------------------------------------------------*/
        Vector3 parentScale = attachPoint.lossyScale;
        Vector3 currentScale = organRoot.transform.lossyScale;

        Vector3 scaleFix = new Vector3(
            parentScale.x / currentScale.x,
            parentScale.y / currentScale.y,
            parentScale.z / currentScale.z);

        organRoot.transform.localScale = Vector3.Scale(organRoot.transform.localScale, scaleFix);

        attached[socketIdx] = organRoot;
    }

    //attach to random socket with green/red distinction
    public void AttachRandom(GameObject organRoot, bool isGreen)
    {

        int socketIdx = isGreen ? Random.Range(0, greenSocketCt) : Random.Range(0, sockets.Count);

        Debug.LogWarning("location: " + socketIdx + " prefab: " + organRoot.name);


        AttachBodyPart(socketIdx, organRoot);

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
      
    }


}
