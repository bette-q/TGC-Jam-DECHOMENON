//// SpriteExporter.cs
//using UnityEngine;
//using UnityEditor;
//using System.Collections.Generic;
//using System.IO;

//public class SpriteExporter : MonoBehaviour
//{
//    public Camera renderCam;
//    public string outputPath = "Assets/Sprites/";
//    public List<OrganCardData> organCards = new List<OrganCardData>();
//    public int baseSize = 1024; // 基础单位尺寸
//    public bool forceSquareCardMode = false;

//    [ContextMenu("Export Sprites")]
//    public void ExportSprites()
//    {
//        if (!Directory.Exists(outputPath))
//            Directory.CreateDirectory(outputPath);

//        foreach (var card in organCards)
//        {
//            if (card == null || card.organPrefab == null)
//            {
//                Debug.LogWarning("Missing OrganCard or prefab reference.");
//                continue;
//            }

//            Debug.Log($"[Exporting] Now processing: {card.organPrefab.name}");

//            GameObject instance = Instantiate(card.organPrefab);
//            instance.transform.position = Vector3.zero;
//            instance.transform.rotation = Quaternion.identity;
//            instance.transform.localScale = Vector3.one;

//            Bounds localBounds = GetBoundsRecursive(instance);
//            Vector3 centerOffset = localBounds.center - instance.transform.position;
//            instance.transform.position -= centerOffset;
//            Debug.Log($"[Bounds Center] {card.organPrefab.name} at {localBounds.center}");

//            instance.transform.rotation = Quaternion.Euler(card.previewRotation);
//            // instance.transform.rotation = Quaternion.Euler(0,0,0);
//            instance.transform.localScale = Vector3.one;

//            // Reset to default pose if Animator has idle clip
//            var animator = instance.GetComponent<Animator>();
//            if (animator && animator.runtimeAnimatorController != null)
//            {
//                var clips = animator.runtimeAnimatorController.animationClips;
//                foreach (var clip in clips)
//                {
//                    if (clip.name.ToLower().Contains("idle"))
//                    {
//                        clip.SampleAnimation(instance, 0f);
//                        break;
//                    }
//                }
//            }

//            var animations = instance.GetComponentsInChildren<Animation>();
//            foreach (var a in animations) a.Stop();

//            // 计算 bounds 并设置摄像机
//            Bounds bounds = GetBoundsRecursive(instance);

//            float halfsize = Mathf.Max(localBounds.extents.x, localBounds.extents.y);
//            float paddingRatio = 1.1f;
//            renderCam.orthographicSize = halfsize * paddingRatio;

//            // float maxExtent = Mathf.Max(bounds.extents.x, bounds.extents.y);
//            // float paddingRatio = 1.1f;
//            // renderCam.orthographicSize = maxExtent * paddingRatio;
            
//            renderCam.transform.position = bounds.center + new Vector3(0, 0, -5f);
//            renderCam.transform.LookAt(bounds.center);
//            Debug.Log($"[PaddingDebug] {card.organPrefab.name} - halfSize: {halfsize:F2}, orthoSize: {renderCam.orthographicSize:F2}");

//            // 2. 计算 RenderTexture 尺寸
//            int width = forceSquareCardMode ? baseSize : card.gridSize.x * baseSize;
//            int height = forceSquareCardMode ? baseSize : card.gridSize.y * baseSize;

//            RenderTexture rt = new RenderTexture(width, height, 24);
//            renderCam.targetTexture = rt;
//            renderCam.Render();

//            RenderTexture.active = rt;
//            Texture2D image = new Texture2D(width, height, TextureFormat.ARGB32, false);
//            image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
//            image.Apply();

//            byte[] bytes = image.EncodeToPNG();
//            string filename = $"{outputPath}{card.organPrefab.name}.png";
//            File.WriteAllBytes(filename, bytes);

//            // 清理
//            RenderTexture.active = null;
//            renderCam.targetTexture = null;
//            DestroyImmediate(rt);
//            DestroyImmediate(instance);
//        }

//        AssetDatabase.Refresh();
//        Debug.Log("✅ All sprites exported!");
//    }

//    Bounds GetBoundsRecursive(GameObject go)
//    {
//        var renderers = go.GetComponentsInChildren<Renderer>();
//        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);

//        Bounds bounds = renderers[0].bounds;
//        for (int i = 1; i < renderers.Length; i++)
//            bounds.Encapsulate(renderers[i].bounds);

//        return bounds;
//    }
////     Bounds GetBoundsRecursive(GameObject go)
//// {
////     var meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
////     if (meshRenderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);

////     Bounds bounds = meshRenderers[0].bounds;
////     for (int i = 1; i < meshRenderers.Length; i++)
////         bounds.Encapsulate(meshRenderers[i].bounds);

////     return bounds;
//// }
//}
