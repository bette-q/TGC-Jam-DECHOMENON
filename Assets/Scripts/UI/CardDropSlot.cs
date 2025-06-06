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

    private Vector2 _cachedSlotSize;

    private void Awake()
    {
        // Cache once; UI doesn¡¯t change at runtime
        RectTransform rt = GetComponent<RectTransform>();
        _cachedSlotSize = rt.rect.size;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedGO = eventData.pointerDrag;
        if (droppedGO == null) return;

        DraggableCard draggable = droppedGO.GetComponent<DraggableCard>();
        if (draggable == null || containedDraggable != null) return;

        // Snap the card UI into this slot
        droppedGO.transform.SetParent(transform, worldPositionStays: false);

        RectTransform droppedRect = droppedGO.GetComponent<RectTransform>();

        droppedRect.anchorMin = new Vector2(0.5f, 0.5f);
        droppedRect.anchorMax = new Vector2(0.5f, 0.5f);
        droppedRect.pivot = new Vector2(0.5f, 0.5f);

        droppedRect.anchoredPosition = Vector2.zero;
        droppedRect.localRotation = Quaternion.identity;

        //scale into slot
        droppedRect.localScale = Vector3.one;
        Vector2 cardSize = droppedRect.rect.size;

        // 2) Use the cached slot size instead of querying rect.size again
        float scaleX = _cachedSlotSize.x / cardSize.x;
        float scaleY = _cachedSlotSize.y / cardSize.y;
        float uniformScale = Mathf.Min(scaleX, scaleY);

        // 3) Never upscale¡ªonly shrink if too big
        float finalScale = Mathf.Min(1f, uniformScale);
        droppedRect.localScale = new Vector3(finalScale, finalScale, 1f);


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
