// PreviewHelper.cs
using UnityEngine;

public static class PreviewHelper
{
    private const string PreviewLayerName = "PreviewLayer";
    private static int _previewLayer = -1;

    private static GameObject _currentInstance;

    // get the layer index
    private static int PreviewLayer
    {
        get
        {
            if (_previewLayer < 0)
                _previewLayer = LayerMask.NameToLayer(PreviewLayerName);
            return _previewLayer;
        }
    }
    public static void ClearPreview()
    {
        if (_currentInstance != null)
        {
            Object.Destroy(_currentInstance);
            _currentInstance = null;
        }
    }

    /// <summary>
    /// Instantiates 'prefab' at world-space origin, puts it (and children) on PreviewLayer,
    /// and stores it so we can clear it later.
    /// </summary>
    public static void ShowPreview(GameObject prefab)
    {
        ClearPreview();
        if (prefab == null) return;

        GameObject go = Object.Instantiate(prefab);
        _currentInstance = go;

        int layer = PreviewLayer;
        SetLayerRecursive(go, layer);

        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
    }

    private static void SetLayerRecursive(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursive(child.gameObject, newLayer);
    }
}
