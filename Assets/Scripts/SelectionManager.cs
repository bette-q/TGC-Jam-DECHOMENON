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

    private List<GameObject> selectedCards = new List<GameObject>();
    private List<GameObject> currentCardInstances = new List<GameObject>();

    public int maxSelections = 3;

    void Start()
    {
        cardButtonPrefab.SetActive(false);
        GenerateRandomCards();
    }

    public void GenerateRandomCards()
    {
        ClearOldCards();
        selectedCards.Clear();
        UpdateProceedButton();

        List<GameObject> shuffled = new List<GameObject>(organCardPrefabs);
        Shuffle(shuffled);

        for (int i = 0; i < 5 && i < shuffled.Count; i++)
        {
            GameObject organ = shuffled[i];
            GameObject card = Instantiate(cardButtonPrefab, cardPanel);
            card.SetActive(true);

            TMP_Text label = card.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = organ.name;

            Button btn = card.GetComponent<Button>();
            GameObject organRef = organ; // capture correctly

            btn.onClick.AddListener(() => ToggleCardSelection(card, organRef));

            currentCardInstances.Add(card);
        }
    }

    private void ToggleCardSelection(GameObject cardUI, GameObject cardData)
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

    private void ClearOldCards()
    {
        foreach (var card in currentCardInstances)
            Destroy(card);
        currentCardInstances.Clear();
    }

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
        // Pass selected cards to ComboUIManager
        comboUIManager.LoadSelectedCards(selectedCards);

        // Toggle panel visibility if needed
        this.gameObject.SetActive(false);
        comboUIManager.gameObject.SetActive(true);
    }
}
