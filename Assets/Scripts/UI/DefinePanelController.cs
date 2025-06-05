using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DefinePanelController : MonoBehaviour
{
    [Header("Drag in your existing DefinePanel children")]
    [Tooltip("DefinePanel/BodySelectPreview (RectTransform)")]
    public RectTransform BodySelectPreview;

    [Tooltip("DefinePanel/CurrentDef (TMP_Text)")]
    public TMP_Text CurrentDef;

    [Tooltip("DefinePanel/selector (RectTransform)")]
    public RectTransform selector;

    [Tooltip("DefinePanel/SelectHighlight (Image)")]
    public Image SelectHighlight;

    [Tooltip("Prefab for one definition©\option row (Button + TMP_Text)")]
    public GameObject definitionOptionPrefab;

    // Externally assigned callback: called when the definition changes in the bottom panel.
    // The top©\panel ¡°cardUI¡± will have supplied this so it can update its own label.
    [HideInInspector]
    public System.Action<string> OnDefinitionChangedExternally;

    // Internals
    private List<GameObject> currentOptionButtons = new List<GameObject>();
    private OrganCard previewedCard = null;
    private string originalDefinition;

    private void Awake()
    {
        ClearPanel();
    }

    /// <summary>
    /// Shows the chosen card¡¯s prefab, its definition, and spawns definition©\option buttons.
    /// </summary>
    public void ShowCardDetails(OrganCard card)
    {
        previewedCard = card;

        // 1) Clear any old preview object
        foreach (Transform t in BodySelectPreview)
            Destroy(t.gameObject);

        // 2) Instantiate the organPrefab under BodySelectPreview
        if (card.organPrefab != null)
        {
            GameObject go = Instantiate(card.organPrefab, BodySelectPreview, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            // If it's a 3D prefab, you might have to tweak its scale or rotation so it ¡°fits¡± inside the UI box.
        }

        // 3) Show the card¡¯s current definition
        originalDefinition = card.GetDefinition();
        CurrentDef.text = originalDefinition;

        // 4) Spawn the three definition-option rows under ¡°selector¡±
        PopulateDefinitionOptions(card);

        // 5) Move the highlight under whichever option matches originalDefinition
        MoveHighlightTo(originalDefinition);

        // 6) Make sure SelectHighlight is visible
        SelectHighlight.gameObject.SetActive(true);

        // Clear any leftover callback from a previous card
        OnDefinitionChangedExternally = null;
    }

    /// <summary>
    /// Clears everything in the DefinePanel (hides preview, clears text & buttons).
    /// </summary>
    public void ClearPanel()
    {
        previewedCard = null;

        // Destroy preview object
        foreach (Transform t in BodySelectPreview)
            Destroy(t.gameObject);

        // Clear definition text
        if (CurrentDef != null)
            CurrentDef.text = "";

        // Destroy all option buttons
        foreach (var btn in currentOptionButtons)
            Destroy(btn);
        currentOptionButtons.Clear();

        // Hide highlight
        if (SelectHighlight != null)
            SelectHighlight.gameObject.SetActive(false);
    }

    /// <summary>
    /// Creates one button for each possible definition (¡°Organic,¡± ¡°Cellular,¡± ¡°Genetic¡±).
    /// </summary>
    private void PopulateDefinitionOptions(OrganCard card)
    {
        // 1) Clear old buttons
        foreach (var btn in currentOptionButtons)
            Destroy(btn);
        currentOptionButtons.Clear();

        // 2) Get the three possible definition©\strings
       // string[] allDefs = OrganCard.AllPossibleDefinitions();

        // 3) Instantiate one row per definition
        //foreach (string defText in allDefs)
        //{
        //    GameObject row = Instantiate(definitionOptionPrefab, selector, false);
        //    row.SetActive(true);

        //    // Set the TMP_Text child to defText
        //    TMP_Text lbl = row.GetComponentInChildren<TMP_Text>();
        //    if (lbl != null) lbl.text = defText;

        //    // Wire up the Button so that clicking it sets this card¡¯s type and notifies externally
        //    Button btn = row.GetComponentInChildren<Button>();
        //    if (btn != null)
        //    {
        //        btn.onClick.AddListener(() =>
        //        {
        //            OnDefinitionChosen(defText);
        //        });
        //    }

        //    currentOptionButtons.Add(row);
        //}
    }

    /// <summary>
    /// Called when the player picks one of the definitions in the bottom panel.
    /// Updates the OrganCard¡¯s type, the CurrentDef text, the highlight, and
    /// invokes the external callback so the top panel updates its label.
    /// </summary>
    private void OnDefinitionChosen(string chosenDef)
    {
        if (previewedCard == null) return;

        // 1) Update the OrganCard¡¯s type
      //  previewedCard.AssignDefinition(chosenDef);

        // 2) Update the CurrentDef label
        CurrentDef.text = chosenDef;

        // 3) Move highlight under the chosen row
        MoveHighlightTo(chosenDef);

        // 4) If the top©\panel passed us a callback, call it with newDef
        OnDefinitionChangedExternally?.Invoke(chosenDef);
    }

    /// <summary>
    /// Moves the SelectHighlight so it overlaps the definition©\option row whose label == defText.
    /// </summary>
    private void MoveHighlightTo(string defText)
    {
        RectTransform highlightRT = SelectHighlight.rectTransform;

        for (int i = 0; i < currentOptionButtons.Count; i++)
        {
            TMP_Text lbl = currentOptionButtons[i].GetComponentInChildren<TMP_Text>();
            if (lbl != null && lbl.text == defText)
            {
                RectTransform optionRT = currentOptionButtons[i].GetComponent<RectTransform>();

                // Reparent the highlight to the same parent (selector) so coordinates match
                highlightRT.SetParent(optionRT.parent, false);

                // Copy the anchoredPosition & sizeDelta
                highlightRT.anchoredPosition = optionRT.anchoredPosition;
                highlightRT.sizeDelta = optionRT.sizeDelta;
                return;
            }
        }
    }

    /// <summary>
    /// Call this (e.g. from a ¡°Revert¡± button somewhere under DefinePanel)
    /// to restore the card¡¯s original definition.
    /// </summary>
    public void RevertDefinition()
    {
        if (previewedCard == null) return;

        // 1) Restore OrganCard¡¯s type
        previewedCard.RevertToOriginal();

        // 2) Update CurrentDef text
        CurrentDef.text = previewedCard.GetDefinition();

        // 3) Move highlight back to original
        MoveHighlightTo(previewedCard.GetDefinition());

        // 4) Fire the external callback so the top panel¡¯s label also reverts
        OnDefinitionChangedExternally?.Invoke(previewedCard.GetDefinition());
    }
}
