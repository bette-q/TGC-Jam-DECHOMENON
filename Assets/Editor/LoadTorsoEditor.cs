/*using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FBXSocketLoader))]
public class FBXSocketLoaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FBXSocketLoader loader = (FBXSocketLoader)target;

        if (GUILayout.Button("Load Sockets from FBX"))
        {
            if (loader.fbxPrefab == null || loader.torsoParent == null)
            {
                Debug.LogError("Please assign both FBX Prefab and Torso Parent.");
                return;
            }

            GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(loader.fbxPrefab);
            Transform[] all = fbxInstance.GetComponentsInChildren<Transform>(true);

            Transform defSpine = null;
            foreach (var t in all)
            {
                if (t.name == "DEF-spine")
                {
                    defSpine = t;
                    break;
                }
            }

            if (defSpine == null)
            {
                Debug.LogError("DEF-spine not found.");
                DestroyImmediate(fbxInstance);
                return;
            }

            // Copy full transform to torso
            loader.torsoParent.position = defSpine.position;
            loader.torsoParent.rotation = defSpine.rotation;
            loader.torsoParent.localScale = defSpine.lossyScale;

            int count = 0;
            foreach (var t in all)
            {
                if (t.name.StartsWith("torso_socket_"))
                {
                    // Local transform relative to DEF-spine
                    Vector3 localPos = defSpine.InverseTransformPoint(t.position);
                    Quaternion localRot = Quaternion.Inverse(defSpine.rotation) * t.rotation;
                    Vector3 localScale = new Vector3(
                        t.lossyScale.x / defSpine.lossyScale.x,
                        t.lossyScale.y / defSpine.lossyScale.y,
                        t.lossyScale.z / defSpine.lossyScale.z
                    );

                    GameObject socketGO = new GameObject($"Socket{count}");
                    socketGO.transform.SetParent(loader.torsoParent, false);
                    socketGO.transform.localPosition = localPos;
                    socketGO.transform.localRotation = localRot;
                    socketGO.transform.localScale = localScale;

                    GameObject anchorGO = new GameObject("Anchor");
                    anchorGO.transform.SetParent(socketGO.transform, false);
                    anchorGO.transform.localPosition = Vector3.zero;
                    anchorGO.transform.localRotation = Quaternion.identity;
                    anchorGO.transform.localScale = Vector3.one;

                    count++;
                }
            }

            DestroyImmediate(fbxInstance);
            EditorUtility.SetDirty(loader);
            Debug.Log($"Created {count} sockets and copied full transform from DEF-spine.");
        }
    }
}
*/