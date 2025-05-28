using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class FBXToJsonExporter : EditorWindow
{
    GameObject fbxPrefab;

    [MenuItem("Tools/Export FBX Sockets to JSON")]
    public static void ShowWindow()
    {
        GetWindow<FBXToJsonExporter>("FBX to JSON");
    }

    void OnGUI()
    {
        fbxPrefab = (GameObject)EditorGUILayout.ObjectField("FBX Prefab", fbxPrefab, typeof(GameObject), false);

        if (GUILayout.Button("Export to JSON"))
        {
            if (fbxPrefab == null)
            {
                Debug.LogError("Assign an FBX prefab first.");
                return;
            }

            ExportSockets(fbxPrefab);
        }
    }

    void ExportSockets(GameObject prefab)
    {
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        List<SocketData> socketList = new List<SocketData>();
        foreach (Transform child in instance.transform)
        {
            if (child.name.StartsWith("torso_socket_"))
            {
                socketList.Add(new SocketData
                {
                    name = child.name,
                    position = child.position,
                    rotation = child.rotation
                });
            }
        }

        FBXSocketExport export = new FBXSocketExport
        {
            fbxName = prefab.name,
            sockets = socketList.ToArray()
        };

        string json = JsonUtility.ToJson(export, true);
        string path = EditorUtility.SaveFilePanel("Save JSON", Application.dataPath, prefab.name + "_sockets", "json");

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, json);
            Debug.Log($"Socket data exported to: {path}");
        }

        DestroyImmediate(instance);
    }


    Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindChildByName(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
