using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

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

    public void ForceDeselect()
    {
        if (!_isSelected) return;
        Deselect();
    }

    public bool IsSelected()
    {
        return _isSelected;
    }

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
