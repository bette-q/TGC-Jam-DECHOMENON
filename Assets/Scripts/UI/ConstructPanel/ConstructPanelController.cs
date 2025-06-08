using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CardDropSlot;

public class ConstructPanelController : MonoBehaviour
{
    [Header("Drag & Drop Slots (0¨C4)")]
    public CardDropSlot[] slots = new CardDropSlot[5];

    [Header("UI Elements")]
    public Button inputButton;
    public Text infoText;

    [Header("Combo System")]
    public ComboManager comboManager;

    [Header("Preview Camera Setup")]
    public PlayerCameraControl playerCameraControl;
    public ViewHelper viewHelper;
    private GameObject _currentPreviewRoot = null;

    // Track if we've already warned about a red combo
    private bool waitingForRedConfirmation = false;

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
        if (slotIndex != 0) return;

        if (newCard != null && newCard.organCard != null)
        {
            viewHelper.ClearPreview();
            _currentPreviewRoot = viewHelper.ShowPreview(newCard.organCard.organPrefab);
            if (playerCameraControl != null)
                playerCameraControl.viewRoot = _currentPreviewRoot.transform;

            // Look for the Animator on any child
            var anim = _currentPreviewRoot.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.ResetTrigger("ActionStop");
                anim.SetTrigger("InvalidAction");
            }
            else
            {
                Debug.LogWarning("No Animator found under preview root!");
            }
        }
        else if (_currentPreviewRoot != null)
        {
            var anim = _currentPreviewRoot.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.ResetTrigger("InvalidAction");
                anim.SetTrigger("ActionStop");
            }
        }
    }




    private void OnInputClicked()
    {
        if (slots[0].containedDraggable == null || slots[4].containedDraggable == null)
        {
            infoText.text = "Missing key components";
            return;
        }

        //Collect each slot¡¯s 3D prefab and OrganCard data in slot order
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

        //¡°Green combo¡± check (root = first, terminal = last, conductors = middle)
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

        //Either it¡¯s already green, or the player confirmed a red combo
        waitingForRedConfirmation = false;
        HighlightSlots(Color.white);
        infoText.text = "";

        comboManager.BuildFromOrder(arrangedPrefabs, isGreen);

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
        _currentPreviewRoot = null;
    }

}
