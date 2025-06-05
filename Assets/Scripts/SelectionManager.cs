using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    public GameObject cardButtonPrefab;

    public RectTransform cardPanel;

    public List<GameObject> organCardPrefabs;

    public DefinePanelController definePanelController;

    // How many cards to show 
    public int numberOfCards = 5;

    // Internals: track every OrganCard and its corresponding UI©\button
    private List<OrganCard> currentCards = new List<OrganCard>();
    private List<GameObject> currentCardButtons = new List<GameObject>();

    void Start()
    {
        // Hide the bottom panel at the very beginning
        definePanelController.ClearPanel();

        // Generate & show the cards in the top panel
        GenerateRandomCards();
    }

    public void GenerateRandomCards()
    {
        // 1) Destroy any old card©\buttons
        foreach (var go in currentCardButtons)
            Destroy(go);
        currentCardButtons.Clear();
        currentCards.Clear();

        // 2) Shuffle the organ©\prefabs
        List<GameObject> shuffled = new List<GameObject>(organCardPrefabs);
        Shuffle(shuffled);

        // 3) Instantiate exactly numberOfCards cards (or fewer if prefab©\list is small)
        int count = Mathf.Min(numberOfCards, shuffled.Count);
        for (int i = 0; i < count; i++)
        {
            // a) Create a new OrganCard with a random type
            OrganCard card = new OrganCard(shuffled[i]);
            currentCards.Add(card);

            // b) Instantiate a cardButton under cardContainer
            GameObject btnGO = Instantiate(cardButtonPrefab, cardPanel, false);
            btnGO.SetActive(true);

            // c) Fill in the Name & Definition text on that button
            TMP_Text[] labels = btnGO.GetComponentsInChildren<TMP_Text>();
            foreach (var label in labels)
            {
                if (label.name == "Name")
                    label.text = card.organName;
                else if (label.name == "Definition")
                    label.text = card.GetDefinition();
            }

            // d) Wire up the ¡°Redefine¡± button on each card in the top panel
            Button redefineBtn = btnGO.transform.Find("RedefineButton")?.GetComponent<Button>();
            if (redefineBtn != null)
            {
                int capturedIndex = i;
                redefineBtn.onClick.AddListener(() =>
                {
                    // Re©\roll the type & update both label and bottom panel if needed
                    currentCards[capturedIndex].AssignRandomType();

                    // Update the label on the card itself
                    TMP_Text defLabel = btnGO.transform.Find("Definition")?.GetComponent<TMP_Text>();
                    if (defLabel != null)
                        defLabel.text = currentCards[capturedIndex].GetDefinition();

                    //// If this card is currently open in the bottom panel, refresh its details
                    //if (definePanelController.CurrentlyPreviewedCard == currentCards[capturedIndex])
                    //{
                    //    definePanelController.ShowCardDetails(currentCards[capturedIndex]);
                    //}
                });
            }

            // e) Wire up clicking anywhere on the card©\button to show its details
            Button cardBtn = btnGO.GetComponent<Button>();
            int capturedIdx = i; // must capture for the closure
            cardBtn.onClick.AddListener(() =>
            {
                ShowCardDetails(currentCards[capturedIdx], btnGO);
            });

            // f) Keep track of this UI GameObject
            currentCardButtons.Add(btnGO);
        }
    }

    /// <summary>
    /// Called when the user clicks a card in the top panel.
    /// This simply tells the bottom panel to display that card¡¯s details.
    /// </summary>
    private void ShowCardDetails(OrganCard card, GameObject cardUI)
    {
        // 1) Tell DefinePanelController to show this card¡¯s prefab & definition options
        definePanelController.ShowCardDetails(card);

        // 2) We also need a way for the bottom panel, when definitions change,
        //    to update this card¡¯s ¡°Definition¡± label. So we pass it a callback:
        definePanelController.OnDefinitionChangedExternally = (newDef) =>
        {
            // Update the ¡°Definition¡± TMP_Text on our cardUI to newDef
            TMP_Text defLabel = cardUI.transform.Find("Definition")?.GetComponent<TMP_Text>();
            if (defLabel != null)
                defLabel.text = newDef;
        };
    }

    private void Shuffle(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }
}
