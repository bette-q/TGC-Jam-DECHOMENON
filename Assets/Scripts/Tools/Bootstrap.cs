// File: Assets/Scripts/FirebaseBootstrap.cs

using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Functions;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class FirebaseBootstrap : MonoBehaviour
{
    void Awake()
    {
        // 1) Check all Firebase dependencies
        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.Result != DependencyStatus.Available)
                {
                    Debug.LogError($"Firebase init failed: {task.Result}");
                    return;
                }

                // 2) Initialize Firestore and Functions for PRODUCTION
                //    (No emulator overrides here.)
                var firestore = FirebaseFirestore.DefaultInstance;
                var functions = FirebaseFunctions.DefaultInstance;

                Debug.Log("✅ Firebase initialized for PRODUCTION Firestore & Functions");
            });
    }
}
