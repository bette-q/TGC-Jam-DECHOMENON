using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class CardDropSlot : MonoBehaviour, IDropHandler
{
    [Tooltip("Index of this slot (0¨C4).")]
    public int slotIndex;

    [HideInInspector]
    public DraggableCard containedDraggable = null;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedGO = eventData.pointerDrag;
        if (droppedGO == null) return;

        DraggableCard draggable = droppedGO.GetComponent<DraggableCard>();
        if (draggable == null || containedDraggable != null) return;

        // Snap the card UI into this slot
        droppedGO.transform.SetParent(transform, worldPositionStays: false);

        RectTransform droppedRect = droppedGO.GetComponent<RectTransform>();
        droppedRect.anchoredPosition = Vector2.zero;
        droppedRect.localRotation = Quaternion.identity;
        droppedRect.localScale = Vector3.one;

        containedDraggable = draggable;
    }

    public void RemoveDraggableReference()
    {
        containedDraggable = null;
    }

    public void ClearSlot()
    {
        if (containedDraggable != null)
        {
            containedDraggable.ReturnToHome();
            containedDraggable = null;
        }
    }
}
