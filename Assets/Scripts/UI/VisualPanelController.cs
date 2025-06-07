// File: Assets/Scripts/VisualPanelController.cs
using UnityEngine;

public class VisualPanelController : MonoBehaviour
{
    [Header("View Helper (Visual)")]
    public ViewHelper viewHelper;
    public PlayerCameraControl playerCameraControl;

    [Header("Default Torso Prefab")]
    public GameObject defaultTorsoPrefab;

    [Header("Socket Manager")]
    public SocketManager socketManager;

    private GameObject _currentPreview;

    private void Start()
    {
        defaultTorsoPrefab = SocketDatabase.Instance.GetTorsoPrefab();

        if (defaultTorsoPrefab != null)
        {
            // 1) Spawn the default torso into the preview panel
            _currentPreview = viewHelper.ShowPreview(defaultTorsoPrefab);

            // 2) Put it on the VisualLayer
            LayerUtils.SetLayerRecursively(_currentPreview, LayerMask.NameToLayer("VisualLayer"));

            // 3) Tell the camera where to orbit
            if (playerCameraControl != null)
                playerCameraControl.viewRoot = _currentPreview.transform;

            // 4) Bind sockets using the new API
            socketManager.SetTorso(_currentPreview);
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
        _currentPreview = viewHelper.ShowPreview(comboPrefab);

        // 2) Layer it
        LayerUtils.SetLayerRecursively(_currentPreview, LayerMask.NameToLayer("VisualLayer"));

        // 3) Update camera target
        if (playerCameraControl != null)
            playerCameraControl.viewRoot = _currentPreview.transform;

        // 4) Re-bind sockets for the new torso
        socketManager.SetTorso(_currentPreview);
    }

    public void ClearVisual()
    {
        viewHelper.ClearPreview();
        if (playerCameraControl != null)
            playerCameraControl.viewRoot = null;
        _currentPreview = null;
    }
}
