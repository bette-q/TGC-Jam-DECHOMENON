#if !UNITY_WEBGL
using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseInitializer : MonoBehaviour
{
    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    FirebaseApp app = FirebaseApp.DefaultInstance;
                    Debug.Log("✅ Firebase initialized successfully.");
                }
                else
                {
                    Debug.LogError($"❌ Could not resolve Firebase dependencies: {dependencyStatus}");
                }
            });
    }
}
#endif


