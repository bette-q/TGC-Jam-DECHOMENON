using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(SelectionManager))]
public class LoadPrefabsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SelectionManager manager = (SelectionManager)target;

        if (GUILayout.Button("Load All Organ Prefabs"))
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Organs" });
            List<GameObject> loadedPrefabs = new List<GameObject>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                    loadedPrefabs.Add(prefab);
            }

            manager.organCardPrefabs = loadedPrefabs;
            EditorUtility.SetDirty(manager);

            Debug.Log($"Loaded {loadedPrefabs.Count} prefabs from Assets/Prefabs/Organs");
        }
    }
}
