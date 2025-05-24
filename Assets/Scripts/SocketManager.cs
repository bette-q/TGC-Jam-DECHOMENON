using System.Collections.Generic;
using UnityEngine;

public class SocketManager : MonoBehaviour
{
    public Transform torsoRoot;
    public Transform[] sockets;

    [SerializeField] GameObject[] organPrefabs;
    [SerializeField] int greenSocketCt = 3;


    //track socket status
    private Dictionary<int, GameObject> attached = new Dictionary<int, GameObject>();

    //attach and socket status only, pass combo as a whole later
    public void AttachBodyPart(int socketIdx, int prefabIdx) 
    {
        if (attached.ContainsKey(socketIdx))
        {
            //remove from screen
            Destroy(attached[socketIdx]);
            attached.Remove(socketIdx);
        }

        //check if input location is valid
        Transform attachPoint = sockets[socketIdx].transform;
        if(attachPoint == null)
        {
            Debug.LogWarning("No attach point found at socket: " + socketIdx);
            return;
        }

        GameObject newAttachment = Instantiate(organPrefabs[prefabIdx], attachPoint.position, attachPoint.rotation, torsoRoot);
        attached[socketIdx] = newAttachment;
    }

    //attach random with green/red distinction
    private void AttachRandom(int prefabIdx, bool isGreen)
    {
   
        int socketIdx = isGreen ? Random.Range(0, greenSocketCt) : Random.Range(0, sockets.Length);

        Debug.LogWarning("location: " + socketIdx + " prefab: " + prefabIdx);


        AttachBodyPart(socketIdx, prefabIdx);

    }

    public void AttachRed()
    {


        AttachRandom (1, false);
    }

    public void AttachBlue()
    {


        AttachRandom(0, true);

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
