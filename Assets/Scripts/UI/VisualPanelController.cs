// File: Assets/Scripts/VisualPanelController.cs
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class VisualPanelController : MonoBehaviour
{
    [Header("View Helper (Visual)")]
    public ViewHelper viewHelper;
    public PlayerCameraControl playerCameraControl;

    [Header("Default Torso Prefab")]
    public GameObject defaultTorsoPrefab;

    [Header("Socket Manager")]
    public SocketManager socketManager;

    [Header("Torso Motion Controller")]
    public TorsoMotionController torsoMotionController;

    [Header("Torso Sync (Firestore)")]
    public TorsoSyncManager torsoSyncManager;

    private GameObject _currentPreview;
    //public static event Action<Transform> OnTorsoReady;

    async void Start()
    {
        defaultTorsoPrefab = SocketDatabase.Instance.GetTorsoPrefab();

        if (defaultTorsoPrefab != null)
        {
            // 1) Spawn the default torso into the preview panel
            _currentPreview = viewHelper.ShowVisual(defaultTorsoPrefab);

            // 2) Put it on the VisualLayer
            LayerUtils.SetLayerRecursively(_currentPreview, LayerMask.NameToLayer("VisualLayer"));

            // 3) Tell the camera where to orbit
            if (playerCameraControl != null)
                playerCameraControl.viewRoot = _currentPreview.transform;

            // 4) Bind sockets using the new API
            socketManager.SetTorso(_currentPreview);

            // 4) Hook up motion controller
            if (torsoMotionController != null)
            {
                // Assign the preview root as the torso root
                torsoMotionController.torsoRoot = _currentPreview.transform;

                // If any organ combos are already attached, register their anchors:
                foreach (var binding in socketManager.GetActiveBindings())
                {
                    torsoMotionController.RegisterNewAnchor(binding.comboAnchor);
                }

                // Reset internal state to avoid first-frame spikes
                torsoMotionController.ResetMotionState();
            }

            if (torsoSyncManager != null)
            {
                //Debug.Log("VisualPanelController: initializing torso sync...");
#if !UNITY_WEBGL
                await torsoSyncManager.Initialize();
#else   
                torsoSyncManager.Initialize();
                await Task.Yield();
#endif
                //Debug.Log("VisualPanelController: torso sync is now listening");
            }
            else
            {
                Debug.LogError("VisualPanelController: no TorsoSyncManager assigned!");
            }
        }
        else
        {
            Debug.LogError("VisualPanelController: No default torso prefab found in SocketDatabase.");
        }
    }

    /// <summary>
    /// Replace whatever is in the preview with a new combo or prefab.
    /// </summary>
    public void UpdateCombo(GameObject comboPrefab)
    {
        // clear out the old preview
        viewHelper.ClearPreview();

        if (comboPrefab == null)
        {
            if (playerCameraControl != null)
                playerCameraControl.viewRoot = null;
            _currentPreview = null;
            return;
        }

        // 1) Spawn the new combo/prefab
        _currentPreview = viewHelper.ShowVisual(comboPrefab);

        // 2) Layer it
        LayerUtils.SetLayerRecursively(_currentPreview, LayerMask.NameToLayer("VisualLayer"));

        // 3) Update camera target
        if (playerCameraControl != null)
            playerCameraControl.viewRoot = _currentPreview.transform;

        // 4) Re-bind sockets for the new torso
        socketManager.SetTorso(_currentPreview);

        if (torsoMotionController != null)
        {
            torsoMotionController.torsoRoot = _currentPreview.transform;
            foreach (var binding in socketManager.GetActiveBindings())
                torsoMotionController.RegisterNewAnchor(binding.comboAnchor);

            torsoMotionController.ResetMotionState();
        }
    }

    public void ClearVisual()
    {
        viewHelper.ClearPreview();
        if (playerCameraControl != null)
            playerCameraControl.viewRoot = null;
        _currentPreview = null;

        if (torsoMotionController != null)
            torsoMotionController.ResetMotionState();
    }
}
