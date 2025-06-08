using System.Collections.Generic;
using UnityEngine;

public class PreviewManager : MonoBehaviour
{
    public static PreviewManager I { get; private set; }
    public ViewHelper viewHelper;

    private Dictionary<GameObject, GameObject> _cache = new();

    void Awake()
    {
        if (I != null && I != this) Destroy(gameObject);
        else I = this;
    }

    public GameObject ShowPreview(GameObject prefab)
    {
        if (prefab == null || viewHelper == null) return null;

        // If we¡¯ve already spawned one for this prefab, just re-parent & activate:
        if (_cache.TryGetValue(prefab, out var existing) && existing != null)
        {
            existing.transform.SetParent(viewHelper.transform, false);
            existing.SetActive(true);
            return existing;
        }

        // Otherwise ask ViewHelper to create/return the right root,
        // which now only clears if necessary:
        var root = viewHelper.ShowPreview(prefab);
        if (root != null) _cache[prefab] = root;
        return root;
    }

    public void HidePreview(GameObject prefab)
    {
        if (prefab == null) return;
        if (_cache.TryGetValue(prefab, out var root) && root != null)
            root.SetActive(false);
    }
}
