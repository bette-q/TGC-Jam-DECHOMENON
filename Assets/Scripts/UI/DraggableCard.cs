using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach this to your Card©\UI prefab.  
/// Requires that the prefab be a child of a Canvas in Scene (so GetComponentInParent<Canvas>() finds something).
/// The prefab should already have a CanvasGroup (for fade©\out on drag), an Image (so it¡¯s visible), and a Button (if you also use click).
/// </summary>
[RequireComponent(typeof(CanvasGroup), typeof(Image))]
public class DraggableCard : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector]
    public OrganCard organCard;

    private Canvas _rootCanvas;
    private RectTransform _dragGhost;
    private CanvasGroup _ghostCanvasGroup;
    private Transform _originalParent;
    private int _originalSiblingIndex;

    private CanvasGroup _selfCanvasGroup;

    private void Awake()
    {
        // Cache our own CanvasGroup (so we can hide the original on drag)
        _selfCanvasGroup = GetComponent<CanvasGroup>();

        // Find the top©\level Canvas; if this card lives under multiple nested Canvases, this returns the nearest parent.
        _rootCanvas = GetComponentInParent<Canvas>();
        if (_rootCanvas == null)
        {
            Debug.LogError($"DraggableCard: No Canvas found in parents of {name}. This will break dragging.");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_rootCanvas == null)
        {
            // Safeguard: if we don¡¯t have a Canvas, bail out.
            return;
        }

        // 1) Remember original parent so we can restore if dropped nowhere
        _originalParent = transform.parent;
        _originalSiblingIndex = transform.GetSiblingIndex();

        // 2) Create a new ghost copy under the root Canvas
        //    We cast 'transform as RectTransform' because our card UI is a UI element.
        _dragGhost = Instantiate(transform as RectTransform, _rootCanvas.transform);
        _dragGhost.name = "DragGhost_" + name;
        _dragGhost.sizeDelta = ((RectTransform)transform).sizeDelta;

        // 3) Copy over image sprite (and text if you want¡ªbut usually the prefab¡¯s child texts already copy over)
        var srcImage = GetComponent<Image>();
        if (srcImage != null)
        {
            var ghostImage = _dragGhost.GetComponent<Image>();
            if (ghostImage != null)
            {
                ghostImage.sprite = srcImage.sprite;
                ghostImage.color = srcImage.color;
            }
        }

        // 4) Ensure the ghost has a CanvasGroup so we can set alpha=0.8 and blocksRaycasts=false
        _ghostCanvasGroup = _dragGhost.GetComponent<CanvasGroup>();
        if (_ghostCanvasGroup == null)
        {
            _ghostCanvasGroup = _dragGhost.gameObject.AddComponent<CanvasGroup>();
        }
        _ghostCanvasGroup.blocksRaycasts = false;
        _ghostCanvasGroup.alpha = 0.8f;

        // 5) Hide the original so it looks ¡°picked up¡±
        _selfCanvasGroup.alpha = 0f;
        _selfCanvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_dragGhost == null || _rootCanvas == null) return;

        // Move the ghost to follow the cursor
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rootCanvas.transform as RectTransform,
            eventData.position,
            _rootCanvas.worldCamera,
            out Vector2 localPoint
        );
        _dragGhost.localPosition = localPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Destroy the ghost copy
        if (_dragGhost != null)
        {
            Destroy(_dragGhost.gameObject);
            _dragGhost = null;
        }

        // Restore the original card¡¯s visibility and raycasts
        _selfCanvasGroup.alpha = 1f;
        _selfCanvasGroup.blocksRaycasts = true;

        // If the card never got reparented by a slot¡¯s OnDrop, put it back
        if (transform.parent == _originalParent)
        {
            transform.SetSiblingIndex(_originalSiblingIndex);
        }
    }
}
