using System.Collections.Generic;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public GameObject torso;
    public GameObject bodyPart;

    private Dictionary<string, GameObject> attached = new Dictionary<string, GameObject>();

    public void AttachBodyPart(string location) 
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

        GameObject newAttachment = Instantiate(bodyPart, attachPoint.position, attachPoint.rotation, torso.transform);
        attached[location] = newAttachment;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AttachBodyPart("LeftShoulder");
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
