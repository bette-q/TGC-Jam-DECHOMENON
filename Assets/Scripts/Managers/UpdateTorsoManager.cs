using System;
using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Firestore;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using System.Threading.Tasks;


/// <summary>
/// Responsible for syncing torso state from Firestore and updating the visual sockets accordingly.
/// </summary>
public class TorsoSyncManager : MonoBehaviour
{
    private FirebaseFirestore _db;
    private ListenerRegistration _listener;

    // Local cache of the last-known socket states
    private Dictionary<int, List<string>> _localMap = new Dictionary<int, List<string>>();
    private Dictionary<int, bool> _localGreenMap = new Dictionary<int, bool>();

    [SerializeField] public ComboManager comboManager;
    [SerializeField] public SocketManager socketManager;


    //async void Start()
    //{
    //    // 1) Ensure Firebase is initialized
    //    var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
    //    if (dep != DependencyStatus.Available)
    //    {
    //        Debug.LogError($"Firebase init failed: {dep}");
    //        return;
    //    }

    //    _db = FirebaseFirestore.DefaultInstance;

    //    // 2) Attach real-time listener to /game/sharedState
    //    var docRef = _db.Collection("game").Document("sharedState");
    //    Debug.Log("🔔 TorsoSyncManager: Listening on /game/sharedState");
    //    _listener = docRef.Listen(OnSnapshot);
    //}

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
    void OnSnapshot(DocumentSnapshot snap)
    {
        if (!snap.Exists) return;

        if (!snap.TryGetValue("sockets", out Dictionary<string, object> socketsRaw))
            return;

        // 3) Iterate each entry in the map
        foreach (var kv in socketsRaw)
        {
            if (!int.TryParse(kv.Key, out int idx))
                continue;

            if (!(kv.Value is Dictionary<string, object> slotData))
                continue;

            // Parse "names" list
            var namesObj = slotData["names"] as List<object>;
            List<string> names = namesObj?.Select(o => o.ToString()).ToList()
                                  ?? new List<string>();

            // Parse "isGreen" flag
            bool isGreen = slotData.TryGetValue("isGreen", out var gObj) && (bool)gObj;

            // 4) Detect changes vs local cache
            bool needsUpdate = false;
            if (!_localMap.TryGetValue(idx, out var oldNames) || !oldNames.SequenceEqual(names))
                needsUpdate = true;
            if (!_localGreenMap.TryGetValue(idx, out var oldGreen) || oldGreen != isGreen)
                needsUpdate = true;

            if (needsUpdate)
            {
                _localMap[idx] = new List<string>(names);
                _localGreenMap[idx] = isGreen;

                // 5) Build the combo and attach at the specified socket index
                List<GameObject> prefabObjects = names
                    .Select(name => SocketDatabase.Instance.GetPrefabByName(name))
                    .Where(prefab => prefab != null)
                    .ToList();

                comboManager.BuildFromServer(prefabObjects, isGreen, idx);

                Debug.Log($"🔄 Updated socket {idx} with combo [{string.Join(",", names)}], isGreen={isGreen}");
            }
        }
    }

    void OnDestroy()
    {
        _listener?.Stop();
    }
}
