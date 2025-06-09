using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CardDropSlot;
using System.Linq;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase;


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

    [Header("Preview Camera Setup")]
    public PlayerCameraControl playerCameraControl;
    public ViewHelper viewHelper;
    private GameObject curPreviewRoot = null;

    // Track if we've already warned about a red combo
    private bool waitingForRedConfirmation = false;

    FirebaseFirestore _db;

    private void Awake()
    {
        _db = FirebaseFirestore.DefaultInstance;
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
            infoText.text = "Missing key components";
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
            infoText.text = "Warning: This is a RED combo. Click Input again to confirm.";
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

        // Build the per-slot object with names + isGreen
        var slotData = new Dictionary<string, object>
        {
            { "names",   organNames },
            { "isGreen", isGreen   }
        };
        
                // Merge that single slot into the sockets map
                var slotMap = new Dictionary<string, object>
        {
            { idx.ToString(), slotData }
        };
                var payload = new Dictionary<string, object>
        {
            { "sockets", slotMap }
        };

        // Firestore SetAsync with MergeAll will auto-create or merge your doc/map
        _db.Collection("game")
           .Document("sharedState")
           .SetAsync(payload, SetOptions.MergeAll)
           .ContinueWithOnMainThread(task =>
           {
               if (task.IsFaulted)
                   Debug.LogError($"✖ Failed to send combo: {task.Exception}");
               else
                   Debug.Log($"✔ Sent slot {idx}: [{string.Join(", ", organNames)}], isGreen={isGreen}");
           });

        //comboManager.BuildFromOrder(arrangedPrefabs, isGreen);

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

}

