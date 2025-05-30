using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum SlotType
{
    Root,
    Conductor,
    Terminal
}

public class ComboUIManager : MonoBehaviour
{
    [System.Serializable]
    public class OrganSlot
    {
        public Button slotButton;
        public TMP_Text labelText;
        public SlotType expectedType;

        [HideInInspector] public OrganCard assignedCard;

        public void Assign(OrganCard card)
        {
            assignedCard = card;
            labelText.text = card.organName;
            slotButton.gameObject.name = $"Slot_{card.organName}";
        }

        public void Clear()
        {
            assignedCard = null;
            labelText.text = expectedType.ToString();
            slotButton.gameObject.name = "Slot_Empty";
        }
    }


    [Header("UI References")]
    public GameObject prefabButtonTemplate;
    public Transform selectionPanel;
    public Button confirmButton;

    public List<OrganSlot> orderedSlots;
    private List<OrganCard> selectedCards;

    private OrganCard currentlySelectedCard = null;
    private bool isAwaitingRedConfirmation = false;



    void Start()
    {
       // selectedPrefabs = new List<GameObject> { prefabA, prefabB, prefabC }; // or however you're loading these

        confirmButton.onClick.AddListener(OnConfirmOrder);

        for (int i = 0; i < orderedSlots.Count; i++)
        {
            var slot = orderedSlots[i];

            // Assign expected organ type based on position
            if (i == 0)
                slot.expectedType = SlotType.Root;
            else if (i == orderedSlots.Count - 1)
                slot.expectedType = SlotType.Terminal;
            else
                slot.expectedType = SlotType.Conductor;

            // Capture index and wire up click
            var capturedSlot = slot;
            slot.slotButton.onClick.AddListener(() => OnSlotClicked(capturedSlot));
            slot.Clear();
        }
    }


    void PopulateSelectionPanel()
    {
        foreach (OrganCard card in selectedCards)
        {
            GameObject newButton = Instantiate(prefabButtonTemplate, selectionPanel);
            newButton.SetActive(true);

            Button btn = newButton.GetComponent<Button>();
            TMP_Text label = newButton.GetComponentInChildren<TMP_Text>();
            label.text = $"{card.organName} ({card.GetDefinition()})";

            btn.onClick.AddListener(() =>
            {
                currentlySelectedCard = card;
                Debug.Log($"Selected card: {card.organName} ({card.GetDefinition()})");
            });
        }
    }


    public void LoadSelectedCards(List<OrganCard> selected)
    {
        selectedCards = new List<OrganCard>(selected);

        // Clear any previous buttons in UI
        foreach (Transform child in selectionPanel)
        {
            if (child.gameObject != prefabButtonTemplate) // keep template
                Destroy(child.gameObject);
        }

        prefabButtonTemplate.SetActive(false);

        // Populate with new selection
        PopulateSelectionPanel();

        // Clear previous slot assignments
        foreach (var slot in orderedSlots)
            slot.Clear();

        currentlySelectedCard = null;
        isAwaitingRedConfirmation = false;
    }


    void OnSlotClicked(OrganSlot slot)
    {
        // If nothing selected and slot is filled ¡ú clear
        if (currentlySelectedCard == null && slot.assignedCard != null)
        {
            Debug.Log($"Cleared {slot.assignedCard.organName} from slot.");
            slot.Clear();
            return;
        }

        // Remove from other slots
        foreach (var otherSlot in orderedSlots)
        {
            if (otherSlot != slot && otherSlot.assignedCard == currentlySelectedCard)
            {
                otherSlot.Clear();
            }
        }

        // Toggle logic
        if (slot.assignedCard == currentlySelectedCard)
        {
            slot.Clear();
        }
        else
        {
            slot.Assign(currentlySelectedCard);
        }
    }


    void ResetAllSlots(string message)
    {
        Debug.Log(message);
        isAwaitingRedConfirmation = false;

        foreach (var slot in orderedSlots)
        {
            slot.Clear();
            slot.slotButton.image.color = Color.white;
        }
    }


    void HighlightAllSlots(Color color)
    {
        foreach (var slot in orderedSlots)
        {
            slot.slotButton.image.color = color;
        }
    }


    void OnConfirmOrder()
    {
        bool isGreen = false;
        List<GameObject> orderedPrefabs = new();

        OrganCard rootCard = null;
        OrganCard terminalCard = null;
        List<OrganCard> conductorCards = new();

        // Categorize cards from slots
        foreach (var slot in orderedSlots)
        {
            var card = slot.assignedCard;
            if (card != null)
                orderedPrefabs.Add(card.organPrefab);

            switch (slot.expectedType)
            {
                case SlotType.Root:
                    rootCard = card;
                    break;
                case SlotType.Terminal:
                    terminalCard = card;
                    break;
                case SlotType.Conductor:
                    if (card != null)
                        conductorCards.Add(card);
                    break;
            }
        }

        // Validate required slots
        if (rootCard == null || terminalCard == null)
        {
            ResetAllSlots("Invalid combo: Missing required root or terminal.");
            return;
        }

        // Green combo = Cellular root, all Genetic conductors, Organic terminal
        bool conductorsOK = conductorCards.Count > 0 && conductorCards.TrueForAll(c => c.type == OrganType.Genetic);

        isGreen = (rootCard.type == OrganType.Cellular) && 
                  (terminalCard.type == OrganType.Organic) && 
                  conductorsOK;

        if (!isGreen && !isAwaitingRedConfirmation)
        {
            isAwaitingRedConfirmation = true;
            HighlightAllSlots(Color.red);
            Debug.Log("Warning: This is a RED combo. Confirm again to proceed.");
            return;
        }

        isAwaitingRedConfirmation = false;

        Object.FindFirstObjectByType<ComboManager>().BuildFromOrder(orderedPrefabs, isGreen);
        HighlightAllSlots(Color.white);
    }



}
