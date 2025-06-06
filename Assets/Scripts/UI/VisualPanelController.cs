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

    private GameObject _currentComboRoot;

    private void Start()
    {
        if (defaultTorsoPrefab != null)
        {
            // 1) Show the torso under the ViewHelper
            _currentComboRoot = viewHelper.ShowPreview(defaultTorsoPrefab);

            // 2) Layer it onto VisualLayer
            LayerUtils.SetLayerRecursively(_currentComboRoot, LayerMask.NameToLayer("VisualLayer"));

            // 3) Point camera controls at it
            if (playerCameraControl != null && _currentComboRoot != null)
                playerCameraControl.viewRoot = _currentComboRoot.transform;

            // 4) Bind sockets now that torso exists
            socketManager.SetTorso(_currentComboRoot.transform);
        }
    }

    /// <summary>
    /// Swap in a new combo or prefab into the VisualPanel.
    /// </summary>
    public void UpdateCombo(GameObject comboPrefab)
    {
        viewHelper.ClearPreview();
        if (comboPrefab == null)
        {
            if (playerCameraControl != null)
                playerCameraControl.viewRoot = null;
            _currentComboRoot = null;
            return;
        }

        // 1) Show the new prefab/combo
        _currentComboRoot = viewHelper.ShowPreview(comboPrefab);

        // 2) Layer it
        LayerUtils.SetLayerRecursively(_currentComboRoot, LayerMask.NameToLayer("VisualLayer"));

        // 3) Update camera target
        if (playerCameraControl != null && _currentComboRoot != null)
            playerCameraControl.viewRoot = _currentComboRoot.transform;

        // 4) Re-bind sockets (for new torso or combo)
        socketManager.SetTorso(_currentComboRoot.transform);
    }

    public void ClearVisual()
    {
        viewHelper.ClearPreview();
        if (playerCameraControl != null)
            playerCameraControl.viewRoot = null;
        _currentComboRoot = null;
    }
}
