using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class OrganSocketData
{
    public string prefabName;
    public Transform rootSocket;
    public List<Transform> connectionSockets = new();
}

public class OrganPrefabParser : EditorWindow
{
    private List<OrganSocketData> parsedData = new();

    [MenuItem("Tools/Parse Organ Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<OrganPrefabParser>("Organ Prefab Parser");
    }

    void OnGUI()
    {
        if (GUILayout.Button("Parse All Organ Prefabs"))
        {
            parsedData.Clear();
            ParsePrefabsInFolder("Assets/Prefabs/Organs");
        }

        if (parsedData.Count > 0)
        {
            GUILayout.Label($"Parsed {parsedData.Count} prefabs", EditorStyles.boldLabel);
            foreach (var organ in parsedData)
            {
                GUILayout.Label($"• {organ.prefabName} ({organ.connectionSockets.Count} sockets)");
            }
        }
    }

    void ParsePrefabsInFolder(string folderPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            OrganSocketData data = new OrganSocketData { prefabName = prefab.name };

            Transform[] all = instance.GetComponentsInChildren<Transform>(true);
            foreach (var t in all)
            {
                if (t.name.ToLower() == "heart_socket_root")
                    data.rootSocket = t;

                else if (t.name.ToLower().StartsWith("heart_socket_"))
                    data.connectionSockets.Add(t);
            }

            parsedData.Add(data);
            DestroyImmediate(instance);
        }

        Debug.Log($"Parsed {parsedData.Count} organ prefabs.");
    }
}
