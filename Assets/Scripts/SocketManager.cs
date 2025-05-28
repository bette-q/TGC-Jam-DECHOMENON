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

    //[SerializeField] GameObject[] organPrefabs;

    //track socket status
    private Dictionary<int, GameObject> attached = new Dictionary<int, GameObject>();

    private void AttachBodyPart(int socketIdx, GameObject organRoot) 
    {
        UnityEngine.Debug.Assert(socketIdx >= 0 && socketIdx < sockets.Count, $"Invalid socketIdx: {socketIdx}");

        if (attached.ContainsKey(socketIdx))
        {
            //remove from screen
            Destroy(attached[socketIdx]);
            attached.Remove(socketIdx);
        }

        Transform attachPoint = sockets[socketIdx].comboAnchor.transform;
        Transform inputSocket = organRoot.transform.GetChild(0).Find("SocketHead"); // combo -> root organ

/*        Debug.Log("Socket_Head position: " + inputSocket.position);
        Debug.Log("Socket_Head rotation: " + inputSocket.rotation.eulerAngles);
        Debug.Log("arm pos: " + organRoot.transform.position);
*/

        // First, calculate correct world position and rotation
        if (inputSocket != null)
        {
            //rotated offset
            Vector3 offset = attachPoint.rotation * (-inputSocket.position);

            organRoot.transform.position = attachPoint.position + offset;
            organRoot.transform.rotation = attachPoint.rotation;
        }
        else
        {
            organRoot.transform.position = attachPoint.position;
            organRoot.transform.rotation = attachPoint.rotation;
        }

/*
        Debug.Log("After transform: ");
        Debug.Log("Socket_Head position: " + inputSocket.position);
        Debug.Log("Socket_Head rotation: " + inputSocket.rotation.eulerAngles);
        Debug.Log("arm pos: " + organRoot.transform.position);
        Debug.Log("anchor pos: " + attachPoint.position);*/


        // Then parent to the socket
        organRoot.transform.SetParent(attachPoint, true);
        attached[socketIdx] = organRoot;
    }

    //attach to random socket with green/red distinction
    public void AttachRandom(GameObject organRoot, bool isGreen)
    {

        int socketIdx = isGreen ? Random.Range(0, greenSocketCt) : Random.Range(0, sockets.Count);

        Debug.LogWarning("location: " + socketIdx + " prefab: " + organRoot.name);


        AttachBodyPart(socketIdx, organRoot);

    }

    public void AttachRed()
    {


        //AttachRandom (1, false);
    }

    public void AttachBlue()
    {


        //AttachRandom(0, true);

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //InvokeRepeating("AttachRandom", 0f, 5f);
    }

    // Update is called once per frame
    void Update()
    {
      
    }


}
