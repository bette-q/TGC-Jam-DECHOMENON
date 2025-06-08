using System;
using Firebase;
using Firebase.Extensions;
using Firebase.Functions;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class FirebaseBootstrap : MonoBehaviour
{
    void Awake()
    {
        // 1) Tell the Firestore core library to use the emulator
        //    (Unity’s FirestoreSettings API is read-only / buggy right now,
        //     so we use the env var trick instead)
        Environment.SetEnvironmentVariable(
            "FIRESTORE_EMULATOR_HOST", "localhost:8080"
        );

        // 2) Initialize Firebase, then wire up the Functions emulator
        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.Result != DependencyStatus.Available)
                {
                    Debug.LogError($"Could not initialize Firebase: {task.Result}");
                    return;
                }

                // UseFunctionsEmulator takes a single URL string in the Unity SDK, not host+port args
                FirebaseFunctions.DefaultInstance
                    .UseFunctionsEmulator("http://localhost:5001");  // :contentReference[oaicite:0]{index=0}

                Debug.Log("✅ Firebase emulators configured: " +
                          "Functions→http://localhost:5001, " +
                          "Firestore→localhost:8080");
            });
    }
}
