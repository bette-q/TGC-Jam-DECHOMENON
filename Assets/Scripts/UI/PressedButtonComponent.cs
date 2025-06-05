using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class PressedButtonComponent : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Assign these two sprites in the Inspector:")]
    [Tooltip("Sprite for the normal (unpressed) state, e.g. black box with white outline.")]
    public Sprite normalSprite;
    [Tooltip("Sprite for the pressed state, e.g. white box with black outline.")]
    public Sprite pressedSprite;

    [Header("Text Colors:")]
    [Tooltip("Label color in normal state (usually white).")]
    public Color normalTextColor = Color.white;
    [Tooltip("Label color in pressed state (usually black).")]
    public Color pressedTextColor = Color.black;

    private Image _backgroundImage;
    private TMP_Text _tmpText;   // if using TextMeshPro
    private Text _uiText;     // if using legacy UI Text
    private bool _isPressed = false;

    void Awake()
    {
        _backgroundImage = GetComponent<Image>();
        _tmpText = GetComponentInChildren<TMP_Text>();
        if (_tmpText == null)
            _uiText = GetComponentInChildren<Text>();

        // Initialize to normal state
        ApplyNormal();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _isPressed = true;
        ApplyPressed();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _isPressed = false;
        ApplyNormal();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isPressed)
        {
            _isPressed = false;
            ApplyNormal();
        }
    }

    private void ApplyNormal()
    {
        if (normalSprite != null)
            _backgroundImage.sprite = normalSprite;

        if (_tmpText != null)
            _tmpText.color = normalTextColor;
        else if (_uiText != null)
            _uiText.color = normalTextColor;
    }

    private void ApplyPressed()
    {
        if (pressedSprite != null)
            _backgroundImage.sprite = pressedSprite;

        if (_tmpText != null)
            _tmpText.color = pressedTextColor;
        else if (_uiText != null)
            _uiText.color = pressedTextColor;
    }
}
