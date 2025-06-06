using UnityEngine;

public class VisualPanelController : MonoBehaviour
{
    [Header("View Helper (Visual)")]
    public ViewHelper viewHelper;
    public PlayerCameraControl playerCameraControl;

    [Header("Default Torso")]
    public GameObject defaultTorsoPrefab;

    [Header("Socket Manager")]
    public SocketManager socketManager;

    private GameObject _currentComboRoot;

    private void Start()
    {
        if (defaultTorsoPrefab != null)
        {
            // 1) Show the torso in the ViewHelper (this returns the instantiated GameObject)
            _currentComboRoot = viewHelper.ShowPreview(defaultTorsoPrefab);

            // 2) Point camera control at that new instance
            if (playerCameraControl != null && _currentComboRoot != null)
            {
                playerCameraControl.viewRoot = _currentComboRoot.transform;
            }

            // 3) Tell SocketManager to use the _currentComboRoot¡¯s transform,
            //    not the prefab¡¯s transform (which lives in Assets, not in the scene).
            socketManager.SetTorso(_currentComboRoot.transform);

            // 4) Now that torsoRoot is set, update the SocketManager¡¯s bindings:
            socketManager.BindSocketsToInstance();
        }
    }

    public void UpdateCombo(GameObject comboPrefab)
    {
        // If you later need to swap to a different ¡°comboPrefab¡± (e.g. torso+attached parts),
        // do the same: clear, re©\show, re©\bind.
        viewHelper.ClearPreview();

        if (comboPrefab == null)
        {
            playerCameraControl.viewRoot = null;
            _currentComboRoot = null;
            return;
        }

        _currentComboRoot = viewHelper.ShowPreview(comboPrefab);
        if (playerCameraControl != null && _currentComboRoot != null)
        {
            playerCameraControl.viewRoot = _currentComboRoot.transform;
        }

        socketManager.SetTorso(_currentComboRoot.transform);
        socketManager.BindSocketsToInstance();
    }

    public void ClearVisual()
    {
        viewHelper.ClearPreview();
        if (playerCameraControl != null)
            playerCameraControl.viewRoot = null;
        _currentComboRoot = null;
    }
}
