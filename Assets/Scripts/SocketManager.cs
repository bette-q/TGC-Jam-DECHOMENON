using System.Collections.Generic;
using UnityEngine;

public class SocketManager : MonoBehaviour
{
    public Transform torsoRoot;
    public Transform[] sockets;

    public GameObject torso;
    public GameObject[] organPrefabs;

    public GameObject red;
    public GameObject blue;

    //track socket status
    private Dictionary<string, GameObject> attached = new Dictionary<string, GameObject>();

    //for random attachment
    private string[] bodySocket = { 
                                 "LeftShoulder", 
                                 "RightShoulder",
                                 "LeftLeg",
                                 "RightLeg",
                                 "Head"
    };

    public void AttachBodyPart(string location, int prefabIdx) 
    {
        if (attached.ContainsKey(location))
        {
            //remove from screen
            Destroy(attached[location]);
            attached.Remove(location);
        }

        //check if input location is valid
        Transform attachPoint = torso.transform.Find(location);
        if(attachPoint == null)
        {
            Debug.LogWarning("No attach point found at: " + location);
            return;
        }

        GameObject newAttachment = Instantiate(organPrefabs[prefabIdx], attachPoint.position, attachPoint.rotation, torso.transform);
        attached[location] = newAttachment;
    }

    private void AttachRandom()
    {
        int socketIdx = Random.Range(0, bodySocket.Length);
        string location = bodySocket[socketIdx];

        int prefabIdx = Random.Range(0, organPrefabs.Length);

        Debug.LogWarning("location: " + location + " prefab: " + prefabIdx);


        AttachBodyPart(location, prefabIdx);

    }

    public void AttachRed()
    {
        int socketIdx = Random.Range(0, bodySocket.Length);
        string location = bodySocket[socketIdx];

        Debug.LogWarning("Attach Red at: " + location);

        AttachBodyPart (location, 1);
    }

    public void AttachBlue()
    {
        int socketIdx = Random.Range(0, bodySocket.Length);
        string location = bodySocket[socketIdx];

        Debug.LogWarning("Attach Blue at: " + location);

        AttachBodyPart(location, 0);
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
