using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class GlobalClickSound : MonoBehaviour
{
    public AudioClip clickClip;
    private AudioSource _audio;
    public GraphicRaycaster uiRaycaster; 

    void Awake()
    {
        _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
         
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var results = new List<RaycastResult>();
            uiRaycaster.Raycast(pointerData, results);

            // if caught a btn do not play
            foreach (var r in results)
            {
                if (r.gameObject.GetComponent<Button>() != null)
                    return;
            }

            // else play
            if (clickClip != null)
                _audio.PlayOneShot(clickClip);
        }
    }
}