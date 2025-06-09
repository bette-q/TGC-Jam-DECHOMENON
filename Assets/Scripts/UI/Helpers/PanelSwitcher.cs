using UnityEngine;
using UnityEngine.UI;

public class PanelSwitcher : MonoBehaviour
{
    [Header("Tabs")]
    public Button defineButton;
    public Button constructButton;

    [Header("Panels")]
    public GameObject definePanel;
    public GameObject constructPanel;

    [Header("Voices")]
    public AudioSource voice1;   // for Define
    public AudioSource voice2;   // for Construct

    public ViewHelper viewHelper;

    private HighlightButtonComponent _defineHighlight;
    private HighlightButtonComponent _constructHighlight;

    void Awake()
    {
        _defineHighlight = defineButton.GetComponent<HighlightButtonComponent>();
        _constructHighlight = constructButton.GetComponent<HighlightButtonComponent>();

        definePanel.SetActive(true);
        constructPanel.SetActive(false);

        // Make sure both sources are set up correctly:
        voice1.loop = true;
        voice2.loop = true;
        voice1.playOnAwake = voice2.playOnAwake = false;
    }

    void Start()
    {
        if (_defineHighlight != null)
            _defineHighlight.ForceSelect();

        defineButton.onClick.AddListener(ShowDefine);
        constructButton.onClick.AddListener(ShowConstruct);

    }

    private void ShowDefine()
    {
        definePanel.SetActive(true);
        constructPanel.SetActive(false);

        _defineHighlight?.ForceSelect();
        viewHelper.ClearPreview();

        ActivateVoice(voice1, voice2);
    }

    private void ShowConstruct()
    {
        definePanel.SetActive(false);
        constructPanel.SetActive(true);

        _constructHighlight?.ForceSelect();
        viewHelper.ClearPreview();

        ActivateVoice(voice2, voice1);
    }

    private void OnDestroy()
    {
        defineButton.onClick.RemoveListener(ShowDefine);
        constructButton.onClick.RemoveListener(ShowConstruct);
    }

    /// <summary>
    /// Stops & rewinds `off` source, then plays `on` source.
    /// </summary>
    private void ActivateVoice(AudioSource on, AudioSource off)
    {
        if (off.isPlaying)
        {
            off.Stop();
            off.time = 0f;
        }

        if (!on.isPlaying)
            on.Play();
    }
}