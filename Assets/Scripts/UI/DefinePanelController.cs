// DefinePanelController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DefinePanelController : MonoBehaviour
{
    //Hook ups
    public HighlightButtonComponent btnCellular;
    public HighlightButtonComponent btnGenetic;
    public HighlightButtonComponent btnOrganic;
    public Button RevertButton;
    public Button RedefineButton;
    public Transform previewPanel;
    public ConstructPanelController constructPanelController;

    public Text CurrentDef;

    [Header("Preview Camera Setup")]
    public PlayerCameraControl playerCameraControl;
    public ViewHelper viewHelper;
    private GameObject curPreviewRoot;

    // Internals
    private OrganCard previewedCard;
    private OrganType? selectedType;
    private System.Action<string> onTopPanelUpdateCallback;

    private Dictionary<OrganType, HighlightButtonComponent> buttonMap;



    void Awake()
    {
        buttonMap = new Dictionary<OrganType, HighlightButtonComponent>
        {
            { OrganType.Cellular, btnCellular },
            { OrganType.Genetic,  btnGenetic  },
            { OrganType.Organic,  btnOrganic  }
        };

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

        ChangeSelectedButton(current);

        // Enable control buttons
        RevertButton.interactable = true;
        RedefineButton.interactable = true;

        viewHelper.ShowPreview(card.organPrefab);

        // Instantiate the model under a preview root, and capture that root
        curPreviewRoot = viewHelper.ShowPreview(card.organPrefab);

        // Assign the previewRoot to the interaction script so the user can rotate/zoom it
        if (playerCameraControl != null && curPreviewRoot != null)
        {
            playerCameraControl.viewRoot = curPreviewRoot.transform;
        }
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

        viewHelper.ClearPreview();

        if (playerCameraControl != null)
        {
            playerCameraControl.viewRoot = null;
        }
        curPreviewRoot = null;
    }

    private void ChangeSelectedButton(OrganType newType)
    {
        // If there was a previously selected button, deselect it
        if (selectedType.HasValue &&
            buttonMap.TryGetValue(selectedType.Value, out var prevBtn))
        {
            prevBtn.ForceDeselect();
        }

        // Select the new button
        if (buttonMap.TryGetValue(newType, out var newBtn))
        {
            newBtn.ForceSelect();
            selectedType = newType;
        }
        else
        {
            selectedType = null;
        }
    }

    public void OnCellularOptionClicked()
    {
        ChangeSelectedButton(OrganType.Cellular);
    }

    public void OnGeneticOptionClicked()
    {
        ChangeSelectedButton(OrganType.Genetic);
    }

    public void OnOrganicOptionClicked()
    {
        ChangeSelectedButton(OrganType.Organic);
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
        ChangeSelectedButton(orig);

        onTopPanelUpdateCallback?.Invoke(orig.ToString());
    }

    public void OnRedefineClicked()
    {
        if (previewedCard == null) return;

        // Assign the chosen type inside the card
        previewedCard.AssignDefinition(selectedType.Value);
        CurrentDef.text = selectedType.ToString();

        //update visuals
        ChangeSelectedButton(selectedType.Value);

        onTopPanelUpdateCallback?.Invoke(selectedType.ToString());
    }
}
