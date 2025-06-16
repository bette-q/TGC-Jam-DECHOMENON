using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationTriggerTester : MonoBehaviour
{
    [Tooltip("Name of the trigger parameter in your Animator")]
    public string triggerName = "ActionStart";

    [Tooltip("Key to press to fire the trigger")]
    public KeyCode testKey = KeyCode.Space;

    [Tooltip("If true, will automatically fire on Start()")]
    public bool autoFireOnStart = false;

    private Animator _anim;

    void Awake()
    {
        _anim = GetComponent<Animator>();
    }

    void Start()
    {
       
            FireTrigger();
    }

    void Update()
    {
        if (Input.GetKeyDown(testKey))
            FireTrigger();
    }

    private void FireTrigger()
    {
        if (!_anim) return;
        _anim.ResetTrigger(triggerName);
        _anim.SetTrigger(triggerName);
        Debug.Log($"[{name}] Fired trigger '{triggerName}'");
    }
}
