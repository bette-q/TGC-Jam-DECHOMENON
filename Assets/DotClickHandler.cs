using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// When this dot is clicked, tells the StartUIManager
/// to draw a line from this dot to its assigned button.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DotClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("Reference to the manager that handles drawing and button activation")]
    public StartUIManager uiManager;

    [Tooltip("The UI Button this dot should connect to")]
    public RectTransform targetButton;

    private RectTransform _myRect;

    void Awake()
    {
        // Cache our own RectTransform
        _myRect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Called by Unity when the user clicks this UI element.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (uiManager == null)
        {
            Debug.LogWarning("DotClickHandler: no UI Manager assigned.", this);
            return;
        }
        if (targetButton == null)
        {
            Debug.LogWarning("DotClickHandler: no targetButton assigned.", this);
            return;
        }

        // Ask the manager to draw a connection line from this dot to its target button
        uiManager.DrawConnection(_myRect, targetButton);
    }
}