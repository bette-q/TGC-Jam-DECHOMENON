using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class ToonProWarmupSceneGenerator
{
    [MenuItem("Tools/Toon Pro/Create Warmup Scene")]
    public static void CreateWarmupScene()
    {
        string scenePath = "Assets/ToonPro_WarmupScene.unity";

        // 新建场景
        var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
        Debug.Log("Created new warmup scene.");

        // 搜索所有材质
        string[] materialGUIDs = AssetDatabase.FindAssets("t:Material");
        int addedCount = 0;

        foreach (string guid in materialGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader != null && mat.shader.name.ToLower().Contains("toon"))
            {
                // 只筛选使用 Toon Pro shader 的材质
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.transform.position = new Vector3(addedCount * 2, 0, 0);
                go.name = mat.name;
                go.GetComponent<Renderer>().sharedMaterial = mat;
                addedCount++;
            }
        }

        Debug.Log($"Total Toon Pro materials added: {addedCount}");

        // 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(newScene, scenePath);
        AssetDatabase.Refresh();

        Debug.Log("Warmup scene saved at: " + scenePath);
        EditorUtility.DisplayDialog("Toon Pro Warmup", "Warmup Scene 已生成！请在Build Settings中勾选此场景进行Build。", "好的");
    }
}