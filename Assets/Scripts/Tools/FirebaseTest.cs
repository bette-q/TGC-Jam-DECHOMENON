using UnityEngine;
using Firebase;
using Firebase.Functions;
using System.Collections.Generic;      // IDictionary
using System.Collections;

public class HelloFirebase : MonoBehaviour
{
    FirebaseFunctions functions;

    async void Start()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
        {
            Debug.LogError($"Firebase not ready: {status}");
            return;
        }

        functions = FirebaseFunctions.DefaultInstance;
#if UNITY_EDITOR
        functions.UseFunctionsEmulator("http://localhost:5001");
#endif

        // --- call & log --------------------------------------------
        try
        {
            Debug.Log("📤 Calling helloWorld…");

            var result = await functions
                .GetHttpsCallable("helloWorld")
                .CallAsync(null);

            Debug.Log($"✅ Function returned; Raw = {result.Data}");

            if (result.Data is IDictionary dict && dict.Contains("reply"))
            {
                Debug.Log($"🎉 Cloud replied: {dict["reply"]}");
            }
            else
            {
                Debug.Log("⚠️ Payload does not contain a \"reply\" key.");
            }

        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Call failed: {ex}");
        }

    }
}
