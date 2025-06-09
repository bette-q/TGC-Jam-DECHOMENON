using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class ComboSender : MonoBehaviour
{
    FirebaseFirestore _db;

    async void Start()
    {
        // 1) Initialize Firebase
        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dep != DependencyStatus.Available)
        {
            Debug.LogError($"❌ Firebase init failed: {dep}");
            return;
        }
        Debug.Log("✅ Firebase initialized");

        // 2) Grab Firestore instance
        _db = FirebaseFirestore.DefaultInstance;
    }

    /// <summary>
    /// Sends a combo list into one slot of the shared map.
    /// This will auto-create the doc & map on first run, then merge thereafter.
    /// </summary>
    public void SendCombo(int idx, List<string> names)
    {
        // 3) Build the nested map for just one slot:
        //    { sockets: { "5": ["Heart","Lung","Valve"] } }
        var slotMap = new Dictionary<string, object>
        {
            { idx.ToString(), names }
        };
        var payload = new Dictionary<string, object>
        {
            { "sockets", slotMap }
        };

        // 4) Write with MergeAll so we merge into the existing map
        var docRef = _db.Collection("game").Document("sharedState");
        docRef
            .SetAsync(payload, SetOptions.MergeAll)
            .ContinueWithOnMainThread(t =>
            {
                if (t.IsFaulted)
                    Debug.LogError($"✖ Combo write failed: {t.Exception}");
                else
                    Debug.Log($"✔ Merged combo into slot {idx}: [{string.Join(",", names)}]");
            });
    }

    // — Demo only: press Space to send a random combo into slots 0–5
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int idx = Random.Range(0, 6);
            var demo = new List<string> { "Heart", "Lung", "Valve" };
            Debug.Log($"🚀 Sending to slot {idx}: [{string.Join(",", demo)}]");
            SendCombo(idx, demo);
        }
    }
}
