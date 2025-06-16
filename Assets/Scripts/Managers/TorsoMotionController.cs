using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Rendering.DebugUI.Table;

public class TorsoMotionController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Root transform of the torso to drive")] public Transform torsoRoot;
    [Tooltip("List of all active organ anchors (combo ends)")] public List<Transform> organAnchors = new List<Transform>();

    [Header("Spring Settings")]
    [Tooltip("Time (in seconds) for the torso to catch up (~critically damped)")] public float positionSmoothTime = 0.1f;
    [Tooltip("Damping speed for rotation (higher = snappier)")] public float rotationDamping = 10f;
    [Tooltip("Maximum world-space pull magnitude per frame")] public float maxPullPerFrame = 0.5f;


    [Header("Local Bounds")]
    [Tooltip("Max offset from initial local position on each axis")]
    // public Vector3 maxLocalOffset = new Vector3(0.5f, 0.5f, 0f);
    public float maxLocalOffset = 0.5f;
    private Vector3 _initialLocalPos;   


    [Tooltip("Margin in viewport coords (0¨C1) to keep inside")]
    public Vector2 viewportMargin = new Vector2(0.1f, 0.1f);


    // Internal state
    private Vector3 _posVelocity;
    private Vector3 _lastTorsoPos;
    private Quaternion _lastTorsoRot;
    private Dictionary<Transform, (Vector3 pos, Quaternion rot)> _lastOrganStates = new Dictionary<Transform, (Vector3, Quaternion)>();

    void Start()
    {
        // If torsoRoot hasn't been assigned yet, skip initialization; ResetMotionState will handle it when set
        if (torsoRoot == null)
            return;

        // Initialize all motion state
        ResetMotionState();
    }

    void LateUpdate()
    {
        if (torsoRoot == null) return;

        float dt = Mathf.Min(Time.deltaTime, 0.05f);

        Vector3 summedPull = Vector3.zero;
        Quaternion summedTwist = Quaternion.identity;
        float totalWeight = 0f;

        foreach (var anchor in organAnchors)
        {
            if (anchor == null) continue;
            if (!_lastOrganStates.TryGetValue(anchor, out var last)) continue;

            Vector3 dp = anchor.position - last.pos;
            Quaternion dr = anchor.rotation * Quaternion.Inverse(last.rot);

            if (dp.sqrMagnitude > maxPullPerFrame * maxPullPerFrame)
                dp = dp.normalized * maxPullPerFrame;

            float weight = 1f;
            summedPull += dp * weight;
            summedTwist = Quaternion.Slerp(summedTwist, dr, weight / (totalWeight + weight));
            totalWeight += weight;

            _lastOrganStates[anchor] = (anchor.position, anchor.rotation);
        }

        if (totalWeight <= 0f) return;

        Vector3 targetPos = _lastTorsoPos + summedPull;
        torsoRoot.position = Vector3.SmoothDamp(
            torsoRoot.position,
            targetPos,
            ref _posVelocity,
            positionSmoothTime,
            Mathf.Infinity,
            dt
        );

        Quaternion targetRot = _lastTorsoRot * summedTwist;
        float t = 1 - Mathf.Exp(-rotationDamping * dt);
        torsoRoot.rotation = Quaternion.Slerp(
            torsoRoot.rotation,
            targetRot,
            t
        );

        Vector3 local = torsoRoot.localPosition;
        Vector3 offset = local - _initialLocalPos;
        if (offset.magnitude > maxLocalOffset)
            local = _initialLocalPos + offset.normalized * maxLocalOffset;
        torsoRoot.localPosition = local;

        _lastTorsoPos = torsoRoot.position;
        _lastTorsoRot = torsoRoot.rotation;

    }

    public void RegisterNewAnchor(Transform anchor)
    {
        if (anchor == null) return;
        if (!_lastOrganStates.ContainsKey(anchor))
            _lastOrganStates[anchor] = (anchor.position, anchor.rotation);
        if (!organAnchors.Contains(anchor))
            organAnchors.Add(anchor);
    }

    public void UnregisterAnchor(Transform anchor)
    {
        if (anchor == null) return;
        organAnchors.Remove(anchor);
        _lastOrganStates.Remove(anchor);
    }

    public void ResetMotionState()
    {
        _posVelocity = Vector3.zero;
        if (torsoRoot != null)
        {
            _lastTorsoPos = torsoRoot.position;
            _lastTorsoRot = torsoRoot.rotation;
            _initialLocalPos = torsoRoot.localPosition;

        }

        var keys = new List<Transform>(_lastOrganStates.Keys);
        foreach (var anchor in keys)
        {
            if (anchor != null)
                _lastOrganStates[anchor] = (anchor.position, anchor.rotation);
        }
    }
}
