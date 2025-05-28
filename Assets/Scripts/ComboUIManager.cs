using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ComboUIManager : MonoBehaviour
{
    [System.Serializable]
    public class OrganSlot
    {
        public Button slotButton;
        public TMP_Text labelText;
        public OrganType expectedType;

        [HideInInspector] public GameObject assignedPrefab;

        public void Assign(GameObject prefab)
        {
            assignedPrefab = prefab;
            labelText.text = prefab.name;
            slotButton.gameObject.name = $"Slot_{prefab.name}";
        }

        public void Clear()
        {
            assignedPrefab = null;

            // Reset text and name
            labelText.text = "Empty";
            slotButton.gameObject.name = "Slot_Empty";
        }
    }

    //list of prefabs to select from
    // public List<GameObject> randomPrefabs = new()  -> will be passed from randomizer 
    [Header("Prefabs")]
    public GameObject prefabA;
    public GameObject prefabB;
    public GameObject prefabC;

    [Header("UI References")]
    public GameObject prefabButtonTemplate;
    public Transform selectionPanel;
    public Button confirmButton;

    public List<OrganSlot> orderedSlots;
    private List<GameObject> selectedPrefabs;

    private GameObject currentlySelectedPrefab = null;
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
                slot.expectedType = OrganType.Terminal;
            else if (i == orderedSlots.Count - 1)
                slot.expectedType = OrganType.Actuator;
            else
                slot.expectedType = OrganType.Conductor;

            // Capture index and wire up click
            var capturedSlot = slot;
            slot.slotButton.onClick.AddListener(() => OnSlotClicked(capturedSlot));
            slot.Clear();
        }
    }


    void PopulateSelectionPanel()
    {
         foreach (GameObject prefab in selectedPrefabs)
        {
            GameObject newButton = Instantiate(prefabButtonTemplate, selectionPanel);
            newButton.SetActive(true);

            Button btn = newButton.GetComponent<Button>();
            TMP_Text label = newButton.GetComponentInChildren<TMP_Text>(); // TMP version
            label.text = prefab.name;

            btn.onClick.AddListener(() =>
            {
                currentlySelectedPrefab = prefab;
                Debug.Log($"Selected prefab: {prefab.name}");
            });
        }
    }

    public void LoadSelectedCards(List<GameObject> selected)
    {
        selectedPrefabs = new List<GameObject>(selected);

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

        currentlySelectedPrefab = null;
        isAwaitingRedConfirmation = false;
    }


    void OnSlotClicked(OrganSlot slot)
    {
        // If nothing is selected and the slot is filled, clear it
        if (currentlySelectedPrefab == null && slot.assignedPrefab != null)
        {
            Debug.Log($"Cleared {slot.assignedPrefab.name} from slot.");
            slot.Clear();
            return;
        }

        // If same prefab is already assigned to another slot, remove it
        foreach (var otherSlot in orderedSlots)
        {
            if (otherSlot != slot && otherSlot.assignedPrefab == currentlySelectedPrefab)
            {
                otherSlot.Clear();
            }
        }

        // Toggle logic: if prefab already in this slot, clear it
        if (slot.assignedPrefab == currentlySelectedPrefab)
        {
            slot.Clear();
        }
        else
        {
            slot.Assign(currentlySelectedPrefab);
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
        bool isValid = true;
        bool hasConductor = false;
        List<GameObject> orderedPrefabs = new();

        for (int i = 0; i < orderedSlots.Count; i++)
        {
            var slot = orderedSlots[i];
            var prefab = slot.assignedPrefab;

            if (slot.expectedType == OrganType.Terminal && prefab == null)
            {
                Debug.LogWarning("Missing Terminal in first slot.");
                isValid = false;
            }
            else if (slot.expectedType == OrganType.Actuator && prefab == null)
            {
                Debug.LogWarning("Missing Actuator in last slot.");
                isValid = false;
            }
            else if (slot.expectedType == OrganType.Conductor && prefab != null)
            {
                hasConductor = true;
            }

            if (prefab != null)
                orderedPrefabs.Add(prefab);
        }

        if (!isValid)
        {
            ResetAllSlots("Invalid combo: must include Terminal and Actuator.");
            return;
        }

        // Combo is red (no conductor)
        if (!hasConductor && !isAwaitingRedConfirmation)
        {
            isAwaitingRedConfirmation = true;
            HighlightAllSlots(Color.red);
            Debug.Log("Warning: This is a RED combo. Confirm again to proceed.");
            return;
        }

        // Proceed with combo
        isAwaitingRedConfirmation = false;

        bool isGreen = hasConductor;

        Object.FindFirstObjectByType<ComboManager>().BuildFromOrder(orderedPrefabs, isGreen);

        HighlightAllSlots(Color.white);//reset highlight
    }

}
