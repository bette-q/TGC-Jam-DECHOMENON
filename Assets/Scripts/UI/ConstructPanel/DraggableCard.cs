using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup), typeof(Image))]
public class DraggableCard : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public OrganCard organCard;
    [HideInInspector] public RectTransform homeParent;
    [HideInInspector] public int homeSiblingIndex;

    private Canvas _rootCanvas;
    private RectTransform _dragGhost;
    private CanvasGroup _ghostCanvasGroup;
    private Transform _originalParent;
    private CardDropSlot _originSlot;
    private CanvasGroup _selfCanvasGroup;
    private Vector3 _originalScale;

    private void Awake()
    {
        _selfCanvasGroup = GetComponent<CanvasGroup>();
        _rootCanvas = GetComponentInParent<Canvas>();
        if (_rootCanvas == null)
            Debug.LogError($"DraggableCard '{name}' has no parent Canvas. Dragging will fail.");

        // Cache this card¡¯s scale in the pool
        _originalScale = transform.localScale;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_rootCanvas == null) return;

        // Remember original parent
        _originalParent = transform.parent;

        // If that parent was a slot, clear its reference now
        _originSlot = _originalParent.GetComponent<CardDropSlot>();
        if (_originSlot != null)
            _originSlot.RemoveDraggableReference();

        // Create a ghost copy under the root Canvas
        _dragGhost = Instantiate(transform as RectTransform, _rootCanvas.transform);
        _dragGhost.name = "DragGhost_" + name;
        _dragGhost.sizeDelta = ((RectTransform)transform).sizeDelta;

        // Copy Image visuals
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

        // Ensure ghost has a CanvasGroup for alpha & raycasts
        _ghostCanvasGroup = _dragGhost.GetComponent<CanvasGroup>();
        if (_ghostCanvasGroup == null)
            _ghostCanvasGroup = _dragGhost.gameObject.AddComponent<CanvasGroup>();

        _ghostCanvasGroup.blocksRaycasts = false;
        _ghostCanvasGroup.alpha = 0.8f;

        // Hide the original card while dragging
        _selfCanvasGroup.alpha = 0f;
        _selfCanvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_dragGhost == null || _rootCanvas == null) return;

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
        // Destroy the ghost
        if (_dragGhost != null)
        {
            Destroy(_dragGhost.gameObject);
            _dragGhost = null;
        }

        // Restore original card¡¯s visibility
        _selfCanvasGroup.alpha = 1f;
        _selfCanvasGroup.blocksRaycasts = true;

        // If the card wasn¡¯t dropped into a new slot, it remains under _originalParent
        if (transform.parent == _originalParent)
        {
            ReturnToHome();
        }
        // Otherwise, CardDropSlot.OnDrop reparented it and set the new slot¡¯s reference already
    }

    public void ReturnToHome()
    {
        transform.SetParent(homeParent, worldPositionStays: false);

        transform.localRotation = Quaternion.identity;
        transform.localScale = _originalScale;

        transform.SetSiblingIndex(homeSiblingIndex);
    }
}
