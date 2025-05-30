using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SelectionManager : MonoBehaviour
{
    public GameObject cardButtonPrefab;
    public Transform cardPanel;
    public List<GameObject> organCardPrefabs;

    public Button proceedButton;
    public ComboUIManager comboUIManager;

    private List<OrganCard> selectedCards = new();
    private List<GameObject> currentCardButtons = new();
    private List<OrganCard> currentCards = new(); // stores 9 generated cards


    public int maxSelections = 3;

    void Start()
    {
        cardButtonPrefab.SetActive(false);
        GenerateRandomCards();
    }
    public void RedefineCard(int index)
    {
        if (index < 0 || index >= currentCards.Count) return;

        currentCards[index].AssignRandomType();

        // Update definition label
        TMP_Text defLabel = currentCardButtons[index].transform.Find("Definition")?.GetComponent<TMP_Text>();
        if (defLabel != null)
        {
            defLabel.text = currentCards[index].GetDefinition();
        }
    }


    public void GenerateRandomCards()
    {
        selectedCards.Clear();
        currentCards.Clear();
        UpdateProceedButton();

        List<GameObject> shuffled = new List<GameObject>(organCardPrefabs);
        Shuffle(shuffled);

        for (int i = 0; i < 5 && i < shuffled.Count; i++)
        {
            OrganCard card = new OrganCard(shuffled[i]);
            currentCards.Add(card);

            GameObject button = Instantiate(cardButtonPrefab, cardPanel);
            button.SetActive(true);

            // Set name and category
            TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>();
            foreach (var label in labels)
            {
                if (label.name == "Name") label.text = card.organName;
                else if (label.name == "Definition") label.text = card.GetDefinition();
            }

   /*         TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            label.text = $"{card.organName} ({card.GetDefinition()})";*/


            int capturedIndex = i;
            button.GetComponent<Button>().onClick.AddListener(() =>
                ToggleCardSelection(button, card)
            );

            Button redefineBtn = button.transform.Find("RedefineButton")?.GetComponent<Button>();
            if (redefineBtn != null)
            {
                redefineBtn.onClick.AddListener(() => RedefineCard(capturedIndex));
            }

            currentCardButtons.Add(button);
        }
    }

    private void ToggleCardSelection(GameObject cardUI, OrganCard cardData)
    {
        Image cardImage = cardUI.GetComponent<Image>();

        if (selectedCards.Contains(cardData))
        {
            selectedCards.Remove(cardData);
            cardImage.color = Color.white;
        }
        else if (selectedCards.Count < maxSelections)
        {
            selectedCards.Add(cardData);
            cardImage.color = Color.green;
        }

        UpdateProceedButton();
    }


    private void UpdateProceedButton()
    {
        proceedButton.interactable = selectedCards.Count >= 2;
    }

/*    private void ClearOldCards()
    {
        foreach (var card in currentCardInstances)
            Destroy(card);
        currentCardInstances.Clear();
    }*/

    private void Shuffle(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public void ProceedToStage2()
    {
        comboUIManager.LoadSelectedCards(selectedCards);
        this.gameObject.SetActive(false);
        comboUIManager.gameObject.SetActive(true);
    }

}
