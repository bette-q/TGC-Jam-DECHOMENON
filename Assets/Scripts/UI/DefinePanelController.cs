// DefinePanelController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DefinePanelController : MonoBehaviour
{
    public HighlightButtonComponent btnCellular;
    public HighlightButtonComponent btnGenetic;
    public HighlightButtonComponent btnOrganic;
    public Button RevertButton;
    public Button RedefineButton;

    public Text CurrentDef;

    // Internals
    private OrganCard previewedCard;
    private OrganType selectedType;
    private System.Action<string> onTopPanelUpdateCallback;

    void Awake()
    {
        ClearPanelState();
    }

    public void ShowCardDetails(OrganCard card, System.Action<string> topPanelCallback)
    {
        previewedCard = card;
        onTopPanelUpdateCallback = topPanelCallback;

        // Get the card's current definition (string) and parse to enum
        OrganType current = card.curType;
        selectedType = current;

        // Update text display
        CurrentDef.text = current.ToString();

        // Deselect all option buttons first
        btnCellular.ForceDeselect();
        btnGenetic.ForceDeselect();
        btnOrganic.ForceDeselect();

        // Force©\select the matching option by enum
        if (current == OrganType.Cellular) btnCellular.ForceSelect();
        else if (current == OrganType.Genetic) btnGenetic.ForceSelect();
        else if (current == OrganType.Organic) btnOrganic.ForceSelect();

        // Enable control buttons
        RevertButton.interactable = true;
        RedefineButton.interactable = true;
    }

    public void ClearPanelState()
    {
        previewedCard = null;
        CurrentDef.text = "";

        RevertButton.interactable = false;
        RedefineButton.interactable = false;

        btnCellular.ForceDeselect();
        btnGenetic.ForceDeselect();
        btnOrganic.ForceDeselect();

        onTopPanelUpdateCallback = null;
    }

    public void OnCellularOptionClicked()
    {
        selectedType = OrganType.Cellular;
        btnCellular.ForceSelect();
    }

    public void OnGeneticOptionClicked()
    {
        selectedType = OrganType.Genetic;
        btnGenetic.ForceSelect();
    }

    public void OnOrganicOptionClicked()
    {
        selectedType = OrganType.Organic;
        btnOrganic.ForceSelect();
    }

    public void OnRevertClicked()
    {
        if (previewedCard == null) return;

        // Revert inside the card itself
        previewedCard.RevertToOriginal();
        OrganType orig = previewedCard.curType;
        CurrentDef.text = orig.ToString();
        selectedType = orig;

        // Update visuals
        btnCellular.ForceDeselect();
        btnGenetic.ForceDeselect();
        btnOrganic.ForceDeselect();
        if (orig == OrganType.Cellular) btnCellular.ForceSelect();
        else if (orig == OrganType.Genetic) btnGenetic.ForceSelect();
        else if (orig == OrganType.Organic) btnOrganic.ForceSelect();

        onTopPanelUpdateCallback?.Invoke(orig.ToString());
    }

    public void OnRedefineClicked()
    {
        if (previewedCard == null) return;

        // Assign the chosen type inside the card
        previewedCard.AssignDefinition(selectedType);
        CurrentDef.text = selectedType.ToString();

        // Force©\select for visuals
        btnCellular.ForceDeselect();
        btnGenetic.ForceDeselect();
        btnOrganic.ForceDeselect();
        if (selectedType == OrganType.Cellular) btnCellular.ForceSelect();
        else if (selectedType == OrganType.Genetic) btnGenetic.ForceSelect();
        else if (selectedType == OrganType.Organic) btnOrganic.ForceSelect();

        onTopPanelUpdateCallback?.Invoke(selectedType.ToString());
    }
}
