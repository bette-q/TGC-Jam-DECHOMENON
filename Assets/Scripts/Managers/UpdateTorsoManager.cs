// TorsoSyncManager.cs

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.Networking;

#if !UNITY_WEBGL
using Firebase;
using Firebase.Firestore;
#endif

/// <summary>
/// Responsible for syncing torso state—either via Firestore (PC) or via Cloud Function (WebGL).
/// </summary>
#if UNITY_WEBGL
public class TorsoSyncManager : MonoBehaviour
{
    [SerializeField] private ComboManager comboManager;
    [SerializeField] private SocketManager socketManager;

    // local cache
    private Dictionary<int, List<string>> _localMap = new();
    private Dictionary<int, bool> _localGreenMap = new();

    /// <summary>
    /// WebGL initialization: pull JSON from Cloud Function.
    /// </summary>
    public void Initialize()
    {
        StartCoroutine(FetchSharedState());
    }
    private IEnumerator FetchSharedState()
    {
        string url = $"{FirestoreRestConfig.BASE_URL}/game/sharedState?key={FirestoreRestConfig.API_KEY}";
        using var req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("REST GET failed: " + req.error);
            yield break;
        }

        ParseAndApply(req.downloadHandler.text);
    }

    // Parses the Firestore REST JSON and applies identical logic to OnSnapshot
    private void ParseAndApply(string json)
    {
        var root = JObject.Parse(json);
        // Firestore REST wraps fields under "mapValue"/"fields"
        var sockets = root["fields"]?["sockets"]?["mapValue"]?["fields"];
        if (sockets == null) return;

        foreach (var prop in sockets.Children<JProperty>())
        {
            if (!int.TryParse(prop.Name, out int idx)) continue;
            var slotFields = prop.Value["mapValue"]?["fields"];
            if (slotFields == null) continue;

            bool isGreen = slotFields["isGreen"]["booleanValue"].Value<bool>();
            var namesArray = slotFields["names"]["arrayValue"]["values"];
            var names = namesArray
                .Select(v => v["stringValue"].Value<string>())
                .ToList();

            bool needsUpdate = !_localMap.TryGetValue(idx, out var oldNames)
                            || !oldNames.SequenceEqual(names)
                            || !_localGreenMap.TryGetValue(idx, out var oldG)
                            || oldG != isGreen;
            if (!needsUpdate) continue;

            _localMap[idx] = new List<string>(names);
            _localGreenMap[idx] = isGreen;

            var prefabs = names
                .Select(n => SocketDatabase.Instance.GetPrefabByName(n))
                .Where(p => p != null)
                .ToList();

            comboManager.BuildFromServer(prefabs, isGreen, idx);
        }
    }

    //private void OnWebGLSnapshot(string json)
    //{
    //    var payload = JsonUtility.FromJson<SlotPayloadWrapper>(json);
    //    if (payload?.sockets == null) return;

    //    foreach (var kv in payload.sockets)
    //    {
    //        if (!int.TryParse(kv.Key, out var idx)) continue;
    //        var slot = kv.Value;
    //        var names = slot.names;
    //        var isGreen = slot.isGreen;

    //        bool needsUpdate = !_localMap.TryGetValue(idx, out var oldNames) || !oldNames.SequenceEqual(names)
    //                        || !_localGreenMap.TryGetValue(idx, out var oldGreen) || oldGreen != isGreen;
    //        if (!needsUpdate) continue;

    //        _localMap[idx] = new List<string>(names);
    //        _localGreenMap[idx] = isGreen;

    //        var prefabs = names
    //            .Select(n => SocketDatabase.Instance.GetPrefabByName(n))
    //            .Where(p => p != null)
    //            .ToList();

    //        comboManager.BuildFromServer(prefabs, isGreen, idx);
    //    }
    //}
}
#else
public class TorsoSyncManager : MonoBehaviour
{
    private FirebaseFirestore   _db;
    private ListenerRegistration _listener;

    [SerializeField] private ComboManager comboManager;
    [SerializeField] private SocketManager socketManager;

    // local cache
    private Dictionary<int, List<string>> _localMap = new();
    private Dictionary<int, bool>       _localGreenMap = new();

    /// <summary>
    /// PC initialization: set up Firestore listener.
    /// </summary>
    public async Task Initialize()
    {
        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dep != DependencyStatus.Available)
        {
            Debug.LogError($"Firebase init failed: {dep}");
            return;
        }

        _db = FirebaseFirestore.DefaultInstance;
        var docRef = _db.Collection("game").Document("sharedState");
        Debug.Log("🔔 TorsoSyncManager: now listening…");
        _listener = docRef.Listen(OnSnapshot);
    }

    /// <summary>
    /// Firestore callback.
    /// </summary>
    private void OnSnapshot(DocumentSnapshot snap)
    {
        if (!snap.Exists) return;
        if (!snap.TryGetValue("sockets", out Dictionary<string, object> socketsRaw)) return;

        foreach (var kv in socketsRaw)
        {
            if (!int.TryParse(kv.Key, out var idx)) continue;
            if (!(kv.Value is Dictionary<string, object> slotData)) continue;

            var namesObj = slotData["names"] as List<object>;
            var names = namesObj?.Select(o => o.ToString()).ToList() 
                        ?? new List<string>();
            var isGreen = slotData.TryGetValue("isGreen", out var g) && (bool)g;

            bool needsUpdate = !_localMap.TryGetValue(idx, out var oldNames) || !oldNames.SequenceEqual(names)
                            || !_localGreenMap.TryGetValue(idx, out var oldGreen) || oldGreen != isGreen;
            if (!needsUpdate) continue;

            _localMap[idx] = new List<string>(names);
            _localGreenMap[idx] = isGreen;

            var prefabs = names
                .Select(n => SocketDatabase.Instance.GetPrefabByName(n))
                .Where(p => p != null)
                .ToList();

            comboManager.BuildFromServer(prefabs, isGreen, idx);
        }
    }

    private void OnDestroy()
    {
        _listener?.Stop();
    }
}
#endif
