// File: Assets/Editor/SocketParserEditor.cs
using UnityEngine;
using UnityEditor;
using System.IO;

public class SocketParserEditor : EditorWindow
{
    private string prefabFolder = "Assets/Prefabs";
    private SocketDatabase _db;

    [MenuItem("Tools/SocketParser")]
    public static void ShowWindow() => GetWindow<SocketParserEditor>("Socket Parser");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Socket Parser (fills SocketDatabase)", EditorStyles.boldLabel);
        prefabFolder = EditorGUILayout.TextField("Folder to scan:", prefabFolder);

        if (_db == null)
        {
            _db = FindObjectOfType<SocketDatabase>();
            if (_db == null)
                EditorGUILayout.HelpBox("No SocketDatabase found in the scene. Create one first.", MessageType.Warning);
        }

        using (new EditorGUI.DisabledScope(_db == null))
        {
            if (GUILayout.Button("Parse All Prefabs into Database"))
                ParseAll();
        }
    }

    private void ParseAll()
    {
        if (_db == null)
        {
            Debug.LogError("SocketParser: No SocketDatabase in scene.");
            return;
        }

        _db.allTorsos.Clear();
        _db.allOrgans.Clear();

        // 1) Find all GameObject assets under prefabFolder
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { prefabFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            // 2) Detect if this prefab has "torso_root" or "organ_root"
            Transform torsoRoot = prefab.transform.Find("torso_root");
            Transform organRoot = prefab.transform.Find("organ_root");

            if (torsoRoot != null)
            {
                // This is a torso prefab
                TorsoData td = new TorsoData
                {
                    prefabName = prefab.name,
                    sockets = new System.Collections.Generic.List<TorsoSocketEntry>()
                };

                // Find all children named "torso_socket_*"
                foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name.StartsWith("torso_socket_"))
                    {
                        TorsoSocketEntry entry = new TorsoSocketEntry
                        {
                            name = child.name,
                            localPosition = child.localPosition,
                            localRotation = child.localRotation
                        };
                        td.sockets.Add(entry);
                    }
                }

                _db.allTorsos.Add(td);
                Debug.Log($"[SocketParser] Parsed Torso '{prefab.name}' with {td.sockets.Count} sockets.");
            }
            else if (organRoot != null)
            {
                // This is an organ prefab
                OrganData od = new OrganData
                {
                    prefabName = prefab.name
                };

                Transform head = prefab.transform.Find("organ_socket_head");
                Transform tail = prefab.transform.Find("organ_socket_tail");
                Transform leaf = prefab.transform.Find("organ_socket_leaf");

                if (head == null || tail == null)
                {
                    Debug.LogError($"[SocketParser] Organ '{prefab.name}' is missing head or tail socket. Skipping.");
                    continue;
                }

                od.head = new OrganSocketEntry
                {
                    name = "organ_socket_head",
                    localPosition = head.localPosition,
                    localRotation = head.localRotation
                };
                od.tail = new OrganSocketEntry
                {
                    name = "organ_socket_tail",
                    localPosition = tail.localPosition,
                    localRotation = tail.localRotation
                };

                if (leaf != null)
                {
                    od.leaf = new OrganSocketEntry
                    {
                        name = "organ_socket_leaf",
                        localPosition = leaf.localPosition,
                        localRotation = leaf.localRotation
                    };
                }
                else
                {
                    // If no explicit leaf, reuse head
                    od.leaf = new OrganSocketEntry
                    {
                        name = "organ_socket_head",
                        localPosition = head.localPosition,
                        localRotation = head.localRotation
                    };
                }

                _db.allOrgans.Add(od);
                Debug.Log($"[SocketParser] Parsed Organ '{prefab.name}'.");
            }
            // else: neither torso nor organ ¡ú ignore
        }

        // Mark scene dirty so Unity saves these lists
        EditorUtility.SetDirty(_db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("SocketParser: Done parsing all prefabs into SocketDatabase.");
    }
}
