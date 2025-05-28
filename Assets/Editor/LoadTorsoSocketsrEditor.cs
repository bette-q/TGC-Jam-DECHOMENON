using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SocketManager))]
public class LoadTorsoSocketsrEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SocketManager manager = (SocketManager)target;

        if (GUILayout.Button("Auto-Find Socket/Anchor Pairs"))
        {
            Undo.RecordObject(manager, "Auto-Find Sockets");

            manager.sockets = new List<SocketBinding>();

            foreach (Transform child in manager.torsoRoot)
            {
                if (child.name.ToLower().StartsWith("socket"))
                {
                    Transform anchor = child.Find("Anchor");
                    if (anchor != null)
                    {
                        SocketBinding binding = new SocketBinding
                        {
                            socket = child,
                            comboAnchor = anchor

                        };

                        Debug.LogWarning("Socket added: " + child.name + " | Position: " + child.transform.position + " | Anchor: " + anchor.name + " at " + anchor.position);
                        manager.sockets.Add(binding);
                    }
                    else
                    {
                        Debug.LogWarning($"Socket '{child.name}' is missing a ComboAnchor child.");
                    }
                }
            }

            Debug.LogWarning(manager.sockets.Count + " loaded");

            EditorUtility.SetDirty(manager);
        }
    }
}
