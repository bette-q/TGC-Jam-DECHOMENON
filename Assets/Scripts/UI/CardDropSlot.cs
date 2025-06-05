using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach this to each slot Image (Slot0..Slot4).  
/// When a DraggableCard is dropped over it, we snap the card UI into the slot
/// and remember which DraggableCard ended up here (so we know the OrganCard.organPrefab).
/// </summary>
[RequireComponent(typeof(Image))]
public class CardDropSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("Index of this slot (0¨C4)")]
    public int slotIndex;

    // The DraggableCard currently occupying this slot (null if empty)
    [HideInInspector]
    public DraggableCard containedDraggable = null;

    private RectTransform _cardRect;

    public void OnDrop(PointerEventData eventData)
    {
        // 1) Find the UI object that was dragged
        GameObject droppedGO = eventData.pointerDrag;
        if (droppedGO == null) return;

        DraggableCard draggable = droppedGO.GetComponent<DraggableCard>();
        if (draggable == null) return;

        // 2) If this slot is already filled, do nothing
        if (containedDraggable != null) return;

        // 3) Snap the UI card under this slot so it ¡°fits¡± exactly
        droppedGO.transform.SetParent(this.transform, worldPositionStays: false);

        RectTransform droppedRect = droppedGO.GetComponent<RectTransform>();
        droppedRect.anchoredPosition = Vector2.zero;
        droppedRect.localRotation = Quaternion.identity;
        droppedRect.localScale = Vector3.one;
        _cardRect = droppedRect;

        // 4) Store the DraggableCard so later we can grab draggable.organCard.organPrefab
        containedDraggable = draggable;
    }

    /// <summary>
    /// Call this if you want to clear out this slot (e.g. when rebuilding or swapping).
    /// </summary>
    public void ClearSlot()
    {
        if (_cardRect != null)
        {
            Destroy(_cardRect.gameObject);
            _cardRect = null;
        }
        containedDraggable = null;
    }
}
