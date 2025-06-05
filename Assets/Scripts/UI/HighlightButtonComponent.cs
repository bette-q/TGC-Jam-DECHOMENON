using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// Attach this to any Button that should swap its background sprite when clicked,
/// and also swap the child text color for contrast.
/// 
/// Requires:
///   • An Image component on the same GameObject (for the black/white‐box background).
///   • A child TMP_Text (or legacy Text) to control the label color.
/// 
/// In the Inspector, assign:
///   • normalSprite   (e.g. black‐box‐white‐outline)
///   • selectedSprite (e.g. white‐box‐black‐outline)
///   • normalTextColor = White
///   • selectedTextColor = Black
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class HighlightButtonComponent : MonoBehaviour, IPointerClickHandler
{
    [Header("Background Sprites")]
    [Tooltip("Sprite used when the button is NOT selected.")]
    public Sprite normalSprite;

    [Tooltip("Sprite used when the button IS selected.")]
    public Sprite selectedSprite;

    [Header("Text Colors")]
    [Tooltip("Child TMP/Text color when NOT selected.")]
    public Color normalTextColor = Color.white;

    [Tooltip("Child TMP/Text color when selected.")]
    public Color selectedTextColor = Color.black;

    private Button _button;
    private Image _backgroundImage;
    private TMP_Text _tmpText;   // if using TextMeshPro
    private Text _uiText;    // if using legacy UI Text

    private bool _isSelected = false;

    void Awake()
    {
        _button = GetComponent<Button>();
        _backgroundImage = GetComponent<Image>();
        _tmpText = GetComponentInChildren<TMP_Text>();
        if (_tmpText == null)
            _uiText = GetComponentInChildren<Text>();

        // Initialize to normal state
        SetToNormal();
    }

    /// <summary>
    /// Called when this button is clicked. It will select itself and
    /// deselect all sibling HighlightButtonComponents under the same parent.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isSelected) return;

        // Deselect siblings
        Transform parent = transform.parent;
        if (parent != null)
        {
            foreach (Transform sibling in parent)
            {
                if (sibling == transform) continue;
                var ssb = sibling.GetComponent<HighlightButtonComponent>();
                if (ssb != null)
                    ssb.Deselect();
            }
        }

        // Now select this one
        Select();
    }

    /// <summary>
    /// Set the background image to selectedSprite and text to selectedTextColor.
    /// </summary>
    private void Select()
    {
        if (selectedSprite != null)
            _backgroundImage.sprite = selectedSprite;

        if (_tmpText != null)
            _tmpText.color = selectedTextColor;
        else if (_uiText != null)
            _uiText.color = selectedTextColor;

        _isSelected = true;
    }

    /// <summary>
    /// Set the background image to normalSprite and text to normalTextColor.
    /// </summary>
    private void Deselect()
    {
        if (normalSprite != null)
            _backgroundImage.sprite = normalSprite;

        if (_tmpText != null)
            _tmpText.color = normalTextColor;
        else if (_uiText != null)
            _uiText.color = normalTextColor;

        _isSelected = false;
    }

    /// <summary>
    /// Public method to force this button into the selected state (deselects siblings).
    /// </summary>
    public void ForceSelect()
    {
        if (_isSelected) return;

        Transform parent = transform.parent;
        if (parent != null)
        {
            foreach (Transform sibling in parent)
            {
                if (sibling == transform) continue;
                var ssb = sibling.GetComponent<HighlightButtonComponent>();
                if (ssb != null)
                    ssb.Deselect();
            }
        }
        Select();
    }

    /// <summary>
    /// Public method to force this button into the normal (deselected) state.
    /// </summary>
    public void ForceDeselect()
    {
        if (!_isSelected) return;
        Deselect();
    }

    /// <summary>
    /// Returns true if this button is currently in the selected state.
    /// </summary>
    public bool IsSelected()
    {
        return _isSelected;
    }

    /// <summary>
    /// Helper to set everything back to normal state at start or when resetting UI.
    /// </summary>
    public void SetToNormal()
    {
        if (normalSprite != null)
            _backgroundImage.sprite = normalSprite;

        if (_tmpText != null)
            _tmpText.color = normalTextColor;
        else if (_uiText != null)
            _uiText.color = normalTextColor;

        _isSelected = false;
    }
}
