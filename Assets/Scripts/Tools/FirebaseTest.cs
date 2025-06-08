using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Extensions;
using Firebase.Functions;
using Firebase.Firestore;
using UnityEngine;

public class ComboNetworkTest : MonoBehaviour
{
    FirebaseFunctions _funcs;
    FirebaseFirestore _db;
    ListenerRegistration _listener;

    const string RoomId = "debug-room";

    void Start()
    {
        _funcs = FirebaseFunctions.DefaultInstance;   // uses emulator URL you set in editor
        _db = FirebaseFirestore.DefaultInstance;

        // 1)  Live listener – whenever server wins a race we’ll see it.
        _listener = _db.Collection("rooms").Document(RoomId)
            .Listen(snap =>
            {
                if (!snap.Exists) return;
                var sockets = snap.GetValue<List<object>>("sockets");
                Debug.Log($"[CLIENT] Re-draw torso – sockets now: {snap.ToDictionary()["sockets"]}");
                // TODO:  call your VisualPanel / SocketManager here
            });

        // 2)  Press space to send a fake combo
        Debug.Log("Press <Space> to push a random combo.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(SendTestCombo());
        }
    }

    IEnumerator SendTestCombo()
    {
        // fabricate some data
        int socketIndex = UnityEngine.Random.Range(0, 10);
        var prefabs = new List<string> { "Valve_A", "Muscle_B" };
        bool isGreen = UnityEngine.Random.value > 0.5f;

        var payload = new Dictionary<string, object> {
            { "roomId"     , RoomId },
            { "socketIndex", socketIndex },
            { "prefabs"    , prefabs },
            { "isGreen"    , isGreen },
            { "clientTime" , DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() }
        };

        Debug.Log($"[CLIENT] sending to socket {socketIndex}, green={isGreen}");
        var callable = _funcs.GetHttpsCallable("submitCombo");           // :contentReference[oaicite:1]{index=1}
        var task = callable.CallAsync(payload);

        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
            Debug.LogError(task.Exception);
        else
            Debug.Log($"[CLIENT] server echoed sockets array:\n{task.Result.Data}");
    }

    void OnDestroy()
    {
        _listener?.Stop();
    }
}
