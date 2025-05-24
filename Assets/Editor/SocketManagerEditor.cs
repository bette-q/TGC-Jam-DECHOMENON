using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SocketManager))]
public class SocketManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SocketManager manager = (SocketManager)target;

        if (GUILayout.Button("Auto-Fill Sockets from Torso Root"))
        {
            if (manager.torsoRoot == null)
            {
                Debug.LogWarning("Assign a Torso Root first.");
                return;
            }

            var socketList = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in manager.torsoRoot)
            {
                if (child.name.ToLower().Contains("socket"))
                    socketList.Add(child);
            }

            //socketList.Sort((a, b) => string.Compare(a.name, b.name));

            Debug.Log("Socket order and transforms:");
            for (int i = 0; i < socketList.Count; i++)
            {
                Transform t = socketList[i];
                string pos = t.position.ToString("F3");    // world position
                string rot = t.eulerAngles.ToString("F1"); // world rotation
                Debug.Log($"{i}: {t.name} | Position: {pos} | Rotation: {rot}");
            }

            manager.sockets = socketList.ToArray();

            EditorUtility.SetDirty(manager);
            Debug.Log($"Filled {socketList.Count} sockets.");
        }
    }
}
