using System.Collections.Generic;
using Firebase;
using Firebase.Firestore;
using UnityEngine;

public class ComboListener : MonoBehaviour
{
    ListenerRegistration _listener;
    FirebaseFirestore _db;

    async void Start()
    {
        // 1) Init Firebase
        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dep != DependencyStatus.Available)
        {
            Debug.LogError($"Firebase init failed: {dep}");
            return;
        }
        _db = FirebaseFirestore.DefaultInstance;

        // 2) Attach listener
        var docRef = _db.Collection("game").Document("sharedState");
        _listener = docRef.Listen(OnSnapshot);
        Debug.Log("🔔 Listening on /game/sharedState");
    }

    void OnSnapshot(DocumentSnapshot snap)
    {
        if (!snap.Exists)
        {
            Debug.Log("⚠️ sharedState missing—waiting for data.");
            return;
        }

        // 3) Pull back the 'sockets' map
        if (!snap.TryGetValue("sockets", out Dictionary<string, object> sockets))
        {
            Debug.LogWarning("No 'sockets' field yet");
            return;
        }

        // 4) Log every slot
        Debug.Log($"📡 sockets contains {sockets.Count} entries:");
        foreach (var kv in sockets)
        {
            var idx = kv.Key;
            var arr = kv.Value as List<object>;
            var names = arr?.ConvertAll(o => o.ToString()) ?? new List<string>();
            Debug.Log($"Listener • Slot {idx}: [{string.Join(",", names)}]");
        }
    }

    void OnDestroy()
    {
        _listener?.Stop();
    }
}
