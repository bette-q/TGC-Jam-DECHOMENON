using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartButtonCooldown : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;
    public TMP_Text buttonText;

    [Header("Cooldown Settings")]
    public float cooldownDuration = 60f;

    private float nextStartTime = 0f;
    private bool isCooldownActive = false;
    private int lastSecondsDisplayed = -1;

    void Start()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        buttonText.text = "";

        startButton.interactable = true;
    }

    void Update()
    {
        if (!isCooldownActive) return;

        float remaining = nextStartTime - Time.unscaledTime;

        if (remaining <= 0f)
        {
            SetButtonReady();
            return;
        }

        int secondsLeft = Mathf.CeilToInt(remaining);

        if (secondsLeft != lastSecondsDisplayed)
        {
            lastSecondsDisplayed = secondsLeft;
            buttonText.text = $"{secondsLeft}s";

            // Force UI refresh so text visually updates immediately
            LayoutRebuilder.ForceRebuildLayoutImmediate(buttonText.rectTransform);
        }

        startButton.interactable = false;
    }

    public void TriggerCooldown()
    {
        nextStartTime = Time.unscaledTime + cooldownDuration;
        isCooldownActive = true;
        startButton.interactable = false;
        lastSecondsDisplayed = -1; // Reset display cache
    }

    private void OnStartClicked()
    {
        Debug.Log("Start button clicked");
        // Call your game transition logic here
    }

    private void SetButtonReady()
    {
        isCooldownActive = false;
        startButton.interactable = true;
        buttonText.text = "";

        LayoutRebuilder.ForceRebuildLayoutImmediate(buttonText.rectTransform);
    }
}
