// SelectionManager.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SelectionManager : MonoBehaviour
{
    public GameObject cardButtonPrefab;     
    public RectTransform cardPanel;       
    public List<GameObject> organCardPrefabs; 

    public DefinePanelController definePanelController;
    public ConstructPanelController constructPanelController;

    //cards to generate
    public int numberOfCards = 5;

    private List<OrganCard> currentCards = new List<OrganCard>();
    private List<GameObject> currentCardButtons = new List<GameObject>();


    void Start()
    {
        organCardPrefabs = SocketDatabase.Instance
                                   .allOrgans
                                   .Select(o => o.organPrefab)
                                   .ToList();

        definePanelController.ClearPanelState();
        cardButtonPrefab.SetActive(false);
        GenerateRandomCards();
    }

    public void GenerateRandomCards()
    {        
        // Destroy old buttons
        foreach (var btn in currentCardButtons)
            Destroy(btn);
        currentCardButtons.Clear();
        currentCards.Clear();

        // Shuffle organ prefabs
        List<GameObject> shuffled = new List<GameObject>(organCardPrefabs);
        Shuffle(shuffled);

        int count = Mathf.Min(numberOfCards, shuffled.Count);
        for (int i = 0; i < count; i++)
        {
            OrganCard card = new OrganCard(shuffled[i]);
            card.organPrefab = shuffled[i];
            currentCards.Add(card);

            GameObject btnGO = Instantiate(cardButtonPrefab, cardPanel, false);
            btnGO.SetActive(true);

            // added icon 
            var iconImg = btnGO.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImg != null)
            {
                // find icon in prefab 
                var spr = shuffled[i].GetComponentInChildren<SpriteRenderer>();
                if (spr != null)
                    iconImg.sprite = spr.sprite;
            }           

            TMP_Text[] labels = btnGO.GetComponentsInChildren<TMP_Text>();
            foreach (var label in labels)
            {
                if (label.name == "Name")
                    label.text = card.organName;
                else if (label.name == "Definition")
                    label.text = card.GetDefinition();
            }

            //set up dragging for construct slots + stores original pos
            DraggableCard dragComp = btnGO.GetComponent<DraggableCard>();
            if (dragComp == null)
                dragComp = btnGO.AddComponent<DraggableCard>();
            dragComp.organCard = card;
            dragComp.homeParent = cardPanel;               
            dragComp.homeSiblingIndex = btnGO.transform.GetSiblingIndex();

            Button cardBtn = btnGO.GetComponent<Button>();
            cardBtn.onClick.AddListener(() =>
            {
                OnCardButtonClicked(btnGO);
            });

            currentCardButtons.Add(btnGO);
        }
    }

    public void OnCardButtonClicked(GameObject cardUIButton)
    {
        int idx = currentCardButtons.IndexOf(cardUIButton);
        if (idx < 0 || idx >= currentCards.Count) return;

        OrganCard selectedCard = currentCards[idx];

        System.Action<string> topPanelCallback = (newDef) =>
        {
            TMP_Text defLabel = cardUIButton.transform.Find("Definition")?.GetComponent<TMP_Text>();
            if (defLabel != null)
                defLabel.text = newDef;
        };

        definePanelController.ShowCardDetails(selectedCard, topPanelCallback);
        constructPanelController.ShowCardDetails(selectedCard);
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
