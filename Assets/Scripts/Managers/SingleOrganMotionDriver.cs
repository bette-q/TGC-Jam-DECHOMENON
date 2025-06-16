using UnityEngine;
using System;

[RequireComponent(typeof(Transform))]
public class SingleOrganMotionDriver : MonoBehaviour
{
    [Tooltip("Which organ transforms its motion into root movement.")]
    public Transform activeOrgan;

    [Tooltip("Top-level parent of Torso (CharacterRoot).")]
    public Transform characterRoot;

    [Tooltip("0 = exact, higher = smoother (laggier).")]
    [Range(0f, 1f)]
    public float smoothing = 0.1f;

    private Vector3 _lastPos;
    private Quaternion _lastRot;
    private bool _initialized;

    // subscribe to the panel¡¯s ready event
    //void Awake()
    //{
    //    VisualPanelController.OnTorsoReady += SetCharacterRoot;
    //}
    //void OnDestroy()
    //{
    //    VisualPanelController.OnTorsoReady -= SetCharacterRoot;
    //}

    void Start()
    {
        // if both already assigned in inspector, initialize immediately
        TryInit();
    }

    void LateUpdate()
    {
        if (!_initialized) return;

        // 1) compute delta
        Vector3 posDelta = activeOrgan.position - _lastPos;
        Quaternion rotDelta = activeOrgan.rotation * Quaternion.Inverse(_lastRot);

        // 2) optional smoothing
        Vector3 appliedDelta = Vector3.Lerp(Vector3.zero, posDelta, 1f - smoothing);

        // 3) apply to root
        characterRoot.position += appliedDelta;
        characterRoot.rotation = rotDelta * characterRoot.rotation;

        // 4) store for next frame
        _lastPos = activeOrgan.position;
        _lastRot = activeOrgan.rotation;
    }

    /// <summary>
    /// Switch which organ drives the body at runtime.
    /// </summary>
    public void SetActiveOrgan(Transform organ)
    {
        activeOrgan = organ;
        TryInit();
    }

    /// <summary>
    /// Called by VisualPanelController (and Start) when torso is spawned.
    /// </summary>
    public void SetCharacterRoot(Transform root)
    {
        characterRoot = root;
        TryInit();
    }

    // initializes last-frame state once both root and organ are known
    private void TryInit()
    {
        if (_initialized) return;
        if (activeOrgan == null || characterRoot == null) return;

        _lastPos = activeOrgan.position;
        _lastRot = activeOrgan.rotation;
        _initialized = true;
    }
}
