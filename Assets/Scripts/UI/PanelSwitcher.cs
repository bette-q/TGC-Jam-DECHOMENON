using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    [Header("Buttons")]
    public Button defineButton;
    public Button constructButton;

    [Header("Panels")]
    public GameObject definePanel;
    public GameObject constructPanel;

    private HighlightButtonComponent _defineHighlight;
    private HighlightButtonComponent _constructHighlight;

    private void Awake()
    {
        // Cache each button¡¯s HighlightButtonComponent
        _defineHighlight = defineButton.GetComponent<HighlightButtonComponent>();
        _constructHighlight = constructButton.GetComponent<HighlightButtonComponent>();

        // Show Define panel by default, hide Construct
        definePanel.SetActive(true);
        constructPanel.SetActive(false);
    }

    private void Start()
    {
        // Now that all HighlightButtonComponents have run Awake(),
        // we can safely force©\select Define without null refs in siblings:
        if (_defineHighlight != null)
            _defineHighlight.ForceSelect();

        defineButton.onClick.AddListener(ShowDefine);
        constructButton.onClick.AddListener(ShowConstruct);
    }

    private void ShowDefine()
    {
        definePanel.SetActive(true);
        constructPanel.SetActive(false);

        if (_defineHighlight != null)
            _defineHighlight.ForceSelect();

        PreviewHelper.ClearPreview();
    }

    private void ShowConstruct()
    {
        definePanel.SetActive(false);
        constructPanel.SetActive(true);

        if (_constructHighlight != null)
            _constructHighlight.ForceSelect();
        
        PreviewHelper.ClearPreview();
    }

    private void OnDestroy()
    {
        defineButton.onClick.RemoveListener(ShowDefine);
        constructButton.onClick.RemoveListener(ShowConstruct);
    }
}
