using UnityEngine;

public static class LayerUtils
{
    /// <summary>
    /// Walks through `go` and every child GameObject, setting its .layer to `layer`.
    /// </summary>
    public static void SetLayerRecursively(GameObject go, int layer)
    {
        if (go == null) return;
        go.layer = layer;
        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
