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

        if (inputSocket != null)
        {
            // First, orient the organ so that X+ points along attachPoint.up
            organRoot.transform.rotation = Quaternion.LookRotation(
                attachPoint.up,         // prefab's X+ maps to attachPoint.up
                attachPoint.forward     // prefab's Y+ maps to attachPoint.forward
            );

            // Then apply the offset: move the organ so inputSocket aligns with attachPoint
            Vector3 worldOffset = organRoot.transform.TransformVector(-inputSocket.localPosition);
            organRoot.transform.position = attachPoint.position + worldOffset;
        }
        else
        {
            // fallback with just alignment
            organRoot.transform.position = attachPoint.position;
            organRoot.transform.rotation = Quaternion.LookRotation(
                attachPoint.up,
                attachPoint.forward
            );
        }


        /* organRoot.transform.localScale = Vector3.one; // reset
         organRoot.transform.localScale = attachPoint.localScale;*/

        GameObject organInstance = Instantiate(organRoot, attachPoint);
        organInstance.transform.localPosition = Vector3.zero;
        organInstance.transform.localRotation = Quaternion.identity;

        // Compensate for parent scale (usually torso's scale)
        Vector3 parentScale = attachPoint.lossyScale;
        Vector3 prefabWorldScale = organInstance.transform.lossyScale;

        organInstance.transform.localScale = new Vector3(
            organInstance.transform.localScale.x / prefabWorldScale.x * parentScale.x,
            organInstance.transform.localScale.y / prefabWorldScale.y * parentScale.y,
            organInstance.transform.localScale.z / prefabWorldScale.z * parentScale.z
        );



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
