// File: Assets/Editor/SocketParserEditor.cs
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class SocketParserEditor : EditorWindow
{
    private string prefabFolder = "Assets/Prefabs/OrganPrefab";
    private SocketDatabase _db;

    [MenuItem("Tools/SocketParser")]
    public static void ShowWindow() => GetWindow<SocketParserEditor>("Socket Parser");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Socket Parser (fills SocketDatabase)", EditorStyles.boldLabel);
        prefabFolder = EditorGUILayout.TextField("Folder to scan:", prefabFolder);

        if (_db == null)
        {
            _db = Object.FindFirstObjectByType<SocketDatabase>();
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

        // Clear existing
        _db.torsoData.sockets.Clear();
        _db.torsoData.prefabName = "";
        _db.allOrgans.Clear();

        // Find all prefabs
        string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { prefabFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                continue;

            // === Torso parsing ===
            var torsoRoot = prefab
                .GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == "torso_root");

            if (torsoRoot != null)
            {
                TorsoData td = new TorsoData
                {
                    prefabName = prefab.name,
                    torsoPrefab = prefab,
                    sockets = new System.Collections.Generic.List<TorsoSocketEntry>()
                };

                Debug.Log($"[SocketParser] Found torso_root in '{prefab.name}' at path '{GetFullPath(torsoRoot)}'");

                // find all torso_socket_* under torsoRoot
                var socketTransforms = torsoRoot
                    .GetComponentsInChildren<Transform>(true)
                    .Where(t => t.name.StartsWith("torso_socket_"));

                foreach (var socket in socketTransforms)
                {
                    var localPos = torsoRoot.InverseTransformPoint(socket.position);
                    var localRot = Quaternion.Inverse(torsoRoot.rotation) * socket.rotation;

                    TorsoSocketEntry entry = new TorsoSocketEntry
                    {
                        name = socket.name,
                        localPosition = localPos,
                        localRotation = localRot
                    };
                    td.sockets.Add(entry);

                    Debug.Log($"   • Parsed {entry.name} at localPos={entry.localPosition}, localRot={entry.localRotation.eulerAngles}");
                }

                _db.torsoData = td;
                continue;
            }

            // === Organ parsing ===
            var organRoot = prefab
                .GetComponentsInChildren<Transform>(true)
                .FirstOrDefault(t => t.name == "organ_root");

            if (organRoot != null)
            {
                OrganData od = new OrganData
                {
                    prefabName = prefab.name,
                    organPrefab = prefab
                };

                Debug.Log($"[SocketParser] Found organ_root in '{prefab.name}' at path '{GetFullPath(organRoot)}'");

                // head
                var head = organRoot
                    .GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name == "organ_socket_head");
                // tail
                var tail = organRoot
                    .GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name == "organ_socket_tail");
                // leaf or fallback
                var leaf = organRoot
                    .GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(t => t.name == "organ_socket_leaf")
                    ?? head;

                if (head == null || tail == null)
                {
                    Debug.LogError($"[SocketParser] Organ '{prefab.name}' missing head or tail socket. Skipping.");
                    continue;
                }

                od.head = new OrganSocketEntry
                {
                    name = head.name,
                    localPosition = organRoot.InverseTransformPoint(head.position),
                    localRotation = Quaternion.Inverse(organRoot.rotation) * head.rotation
                };
                od.tail = new OrganSocketEntry
                {
                    name = tail.name,
                    localPosition = organRoot.InverseTransformPoint(tail.position),
                    localRotation = Quaternion.Inverse(organRoot.rotation) * tail.rotation
                };
                od.leaf = new OrganSocketEntry
                {
                    name = leaf.name,
                    localPosition = organRoot.InverseTransformPoint(leaf.position),
                    localRotation = Quaternion.Inverse(organRoot.rotation) * leaf.rotation
                };

                Debug.Log($"   • Parsed head at {od.head.localPosition}, tail at {od.tail.localPosition}, leaf at {od.leaf.localPosition}");

                _db.allOrgans.Add(od);
                continue;
            }

            // neither torso nor organ
            Debug.LogWarning($"[SocketParser] Prefab '{prefab.name}' has neither torso_root nor organ_root. Skipping.");
        }

        // Save
        EditorUtility.SetDirty(_db);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("SocketParser: Finished parsing all prefabs.");
    }

    // helper to get full hierarchy path
    private static string GetFullPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
