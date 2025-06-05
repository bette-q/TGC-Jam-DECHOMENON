// PreviewHelper.cs
using UnityEngine;

public static class PreviewHelper
{
    private const string PreviewLayerName = "PreviewLayer";
    private static int _previewLayer = -1;

    // Keep track of both the root and the actual mesh instance
    private static GameObject _currentRoot;
    private static GameObject _currentInstance;

    private static int PreviewLayer
    {
        get
        {
            if (_previewLayer < 0)
                _previewLayer = LayerMask.NameToLayer(PreviewLayerName);
            return _previewLayer;
        }
    }

    /// <summary>
    /// Destroys any existing preview objects (both root and instance).
    /// </summary>
    public static void ClearPreview()
    {
        if (_currentRoot != null)
        {
            Object.Destroy(_currentRoot);
            _currentRoot = null;
            _currentInstance = null;
        }
    }

    /// <summary>
    /// Instantiates 'prefab' under a new empty ¡°preview root¡± at world (0,0,0),
    /// sets the layer on the root and all children, and returns the root so
    /// you can pass it to PreviewInteraction (for rotation/zoom).
    /// </summary>
    public static GameObject ShowPreview(GameObject prefab)
    {
        ClearPreview();
        if (prefab == null) return null;

        // 1) Create an empty root at world origin
        _currentRoot = new GameObject("PreviewRoot");
        _currentRoot.transform.position = Vector3.zero;
        _currentRoot.transform.rotation = Quaternion.identity;
        _currentRoot.transform.localScale = Vector3.one;

        // 2) Instantiate the actual prefab as a child of that root
        GameObject go = Object.Instantiate(prefab, _currentRoot.transform, worldPositionStays: false);
        _currentInstance = go;

        // 3) Set the root (and therefore go & all children) to PreviewLayer
        SetLayerRecursive(_currentRoot, PreviewLayer);

        // 4) Reset the child¡¯s local transform so it¡¯s centered under the root
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        // Return the root to whoever called this
        return _currentRoot;
    }

    private static void SetLayerRecursive(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, newLayer);
    }
}
