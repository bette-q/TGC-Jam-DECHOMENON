using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ViewHelper : MonoBehaviour
{
    [Tooltip("The Camera that renders to a RenderTexture for this view.")]
    public Camera viewCamera;

    [Tooltip("Name of the layer used exclusively by this ViewHelper.")]
    public string viewLayerName = "None";

    // Tracks the integer ID for the view layer
    private int _viewLayer = -1;

    // Keep track of the current root and instance so we can clear them
    private GameObject _currentRoot;
    private GameObject _currentInstance;

    private int ViewLayer
    {
        get
        {
            if (_viewLayer < 0)
                _viewLayer = LayerMask.NameToLayer(viewLayerName);
            return _viewLayer;
        }
    }

    private void Awake()
    {
        // If no camera assigned in Inspector, use the one on this GameObject
        if (viewCamera == null)
            viewCamera = GetComponent<Camera>();

        if (viewCamera == null)
        {
            Debug.LogError($"ViewHelper on '{name}': No Camera found or assigned.");
            return;
        }

        // Cache layer ID and make camera cull only that layer
        int layer = ViewLayer;
        if (layer < 0)
        {
            Debug.LogError($"ViewHelper on '{name}': Layer '{viewLayerName}' does not exist.");
        }
        else
        {
            viewCamera.cullingMask = 1 << layer;
        }
    }

    public void ClearPreview()
    {
        if (_currentRoot != null)
        {
            Destroy(_currentRoot);
            _currentRoot = null;
            _currentInstance = null;
        }
    }

    public GameObject ShowPreview(GameObject prefab)
    {
        ClearPreview();
        if (prefab == null) return null;

        // 1) Create an empty root at world origin
        _currentRoot = new GameObject("ViewRoot_" + prefab.name);
        _currentRoot.transform.position = Vector3.zero;
        _currentRoot.transform.rotation = Quaternion.identity;
        _currentRoot.transform.localScale = Vector3.one;

        // 2) Instantiate the actual prefab as a child of that root
        GameObject instance = Instantiate(prefab, _currentRoot.transform, worldPositionStays: false);
        _currentInstance = instance;

        // 3) Recursively set the root (and all children) to our view layer
        SetLayerRecursive(_currentRoot, ViewLayer);

        // 4) Center the instance under the root
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one * 0.8f;

        // 5) Return the root to the caller (so they can assign it to a camera controller)
        return _currentRoot;
    }

    private void SetLayerRecursive(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, layer);
    }
}
