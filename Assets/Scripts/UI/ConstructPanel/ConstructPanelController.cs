using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Collections;
using UnityEngine.Networking;

#if !UNITY_WEBGL
using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
#endif

using static CardDropSlot;

public class ConstructPanelController : MonoBehaviour
{
    [Header("Drag & Drop Slots (0?C4)")]
    public CardDropSlot[] slots = new CardDropSlot[5];

    [Header("UI Elements")]
    public Button inputButton;
    public Text infoText;
    public Animator panelAnimator;
    public GameObject startPanel;

    [Header("Combo System")]
    public ComboManager comboManager;

    public StartButtonCooldown cdControl;

    [Header("Preview Camera Setup")]
    public PlayerCameraControl playerCameraControl;
    public ViewHelper viewHelper;
    private GameObject curPreviewRoot = null;

    // Track if we've already warned about a red combo
    private bool waitingForRedConfirmation = false;

#if !UNITY_WEBGL
    FirebaseFirestore _db;
#endif

    private void Awake()
    {
#if !UNITY_WEBGL
        _db = FirebaseFirestore.DefaultInstance;
#endif
    }
    private void Start()
    {
        inputButton.onClick.AddListener(OnInputClicked);


        //hook up preview callback
        foreach (var slot in slots)
        {
            slot.OnSlotChanged += OnSlotChangedHandler;
        }
    }


    private void OnSlotChangedHandler(int slotIndex, DraggableCard newCard)
    {
        if (newCard != null && newCard.organCard != null)
        {
            viewHelper.ClearPreview();
            curPreviewRoot = viewHelper.ShowPreview(newCard.organCard.organPrefab);
            if (playerCameraControl != null)
                playerCameraControl.viewRoot = curPreviewRoot.transform;

        }
  
    }

    private void OnInputClicked()
    {
        if (slots[0].containedDraggable == null || slots[4].containedDraggable == null)
        {
            infoText.text = "All required slots must be filled before you can input.";
            return;
        }

        //Collect each slot??s 3D prefab and OrganCard data in slot order
        List<GameObject> arrangedPrefabs = new List<GameObject>();
        List<OrganCard> arrangedCards = new List<OrganCard>();

        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot.containedDraggable != null)
            {
                OrganCard cardData = slot.containedDraggable.organCard;
                if (cardData != null && cardData.organPrefab != null)
                {
                    arrangedCards.Add(cardData);
                    arrangedPrefabs.Add(cardData.organPrefab);
                }
            }
        }

        //??Green combo?? check (root = first, terminal = last, conductors = middle)
        OrganCard rootCard = arrangedCards[0];
        OrganCard terminalCard = arrangedCards[arrangedCards.Count - 1];
        List<OrganCard> conductorCards = arrangedCards.GetRange(1, arrangedCards.Count - 2);

        bool conductorsOK = (conductorCards.Count > 0)
                          && conductorCards.TrueForAll(c => c.curType == OrganType.Genetic);

        bool isGreen = (rootCard.curType == OrganType.Cellular)
                    && (terminalCard.curType == OrganType.Organic)
                    && conductorsOK;

        if (!isGreen && !waitingForRedConfirmation)
        {
            waitingForRedConfirmation = true;
            infoText.text = "This assembly breaks the grammar, click INPUT again to proceed.";
            HighlightSlots(Color.red);
            return;
        }

        //Either it??s already green, or the player confirmed a red combo
        // animator added here
        panelAnimator.SetTrigger("Input");
        startPanel.SetActive(true);

        waitingForRedConfirmation = false;
        HighlightSlots(Color.white);
        infoText.text = "";

        // send to server 
        int idx = GenerateIdx(isGreen);

        // Build a List<string> of the prefab names
        List<string> organNames = arrangedPrefabs
            .Select(prefab => prefab.name)
            .ToList();

#if UNITY_WEBGL
        StartCoroutine(PatchSharedState(idx, organNames, isGreen));
#else
        var slotData = new Dictionary<string, object>
        {
            { "names",   organNames },
            { "isGreen", isGreen   }
        };

        var slotMap = new Dictionary<string, object>
        {
            { idx.ToString(), slotData }
        };

        var payload = new Dictionary<string, object>
        {
            { "sockets", slotMap }
        };

        _db.Collection("game")
           .Document("sharedState")
           .SetAsync(payload, SetOptions.MergeAll);
#endif


        cdControl.TriggerCooldown();
        ClearAllSlots();
    }

    private void HighlightSlots(Color c)
    {
        foreach (var slot in slots)
        {
            Image img = slot.GetComponent<Image>();
            if (img != null)
                img.color = c;
        }
    }

    private void ClearAllSlots()
    {
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }

        viewHelper.ClearPreview();
        if (playerCameraControl != null)
            playerCameraControl.viewRoot = null;
        curPreviewRoot = null;
    }

    public void ShowCardDetails(OrganCard card)
    {

        // Update text display
        infoText.text = card.curType.ToString();

    }

    private int GenerateIdx(bool isGreen)
    {
        int idx = isGreen
            ? Random.Range(0, 6)
            : Random.Range(0, 10);

        return idx;
    }

#if UNITY_WEBGL
    private IEnumerator PatchSharedState(int idx, List<string> names, bool isGreen)
    {
        // build Firestore REST “Document” JSON
        var doc = new JObject(
          new JProperty("fields",
            new JObject(
              new JProperty("sockets",
                new JObject(
                  new JProperty("mapValue",
                    new JObject(
                      new JProperty("fields",
                        new JObject(
                          new JProperty(idx.ToString(),
                            new JObject(
                              new JProperty("mapValue",
                                new JObject(
                                  new JProperty("fields",
                                    new JObject(
                                      new JProperty("isGreen",
                                        new JObject(new JProperty("booleanValue", isGreen))),
                                      new JProperty("names",
                                        new JObject(new JProperty("arrayValue",
                                          new JObject(new JProperty("values",
                                            new JArray(names.Select(n =>
                                              new JObject(new JProperty("stringValue", n))))))))
                                      )
                                    )
                                  )
                                )
                              )
                            )
                          )
                        )
                      )
                    )
                  )
                )
              )
            )
          )
        );
        string body = doc.ToString();

        string url = $"{FirestoreRestConfig.BASE_URL}/game/sharedState" +
                     $"?key={FirestoreRestConfig.API_KEY}" +
                     $"&currentDocument.exists=true";

        var req = new UnityWebRequest(url, "PATCH")
        {
            uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
            Debug.LogError("REST PATCH failed: " + req.error);
    }
#endif

}

