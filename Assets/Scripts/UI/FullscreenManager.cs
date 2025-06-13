using UnityEngine;
using UnityEngine.UI;

public class FullscreenManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject leftPanelContainer;    // 左侧面板容器
    public GameObject rightPanelContainer;   // 右侧面板容器 (VisualPanel)
    
    [Header("Fullscreen Settings")]
    public float margin = 0f;               // 全屏时的边距（0为真正全屏）

    [Header("Visual Panel Components")]
    public RawImage visualPanelRawImage;

    // 内部变量
    private bool isFullscreen = false;
    private Vector2 originalPosition;
    private Vector2 originalSize;
    private Vector2 originalAnchorMin;
    private Vector2 originalAnchorMax;
    private Vector2 originalPivot;
    private Vector2 originalOffsetMin;
    private Vector2 originalOffsetMax;
    private RectTransform rightPanelRect;
    
    private void Start()
    {
        // 获取右侧面板的RectTransform
        if (rightPanelContainer != null)
        {
            rightPanelRect = rightPanelContainer.GetComponent<RectTransform>();
            if (rightPanelRect != null)
            {
                // 保存原始设置
                SaveOriginalSettings();
            }
            else
            {
                Debug.LogError("FullscreenManager: Right panel container has no RectTransform!");
            }
        }
        else
        {
            Debug.LogError("FullscreenManager: Right panel container is not assigned!");
        }

        // Force RawImage to stretch with parent panel
        if (visualPanelRawImage != null)
        {
            RectTransform rawRect = visualPanelRawImage.GetComponent<RectTransform>();
            rawRect.anchorMin = Vector2.zero;
            rawRect.anchorMax = Vector2.one;
            rawRect.offsetMin = Vector2.zero;
            rawRect.offsetMax = Vector2.zero;
            rawRect.pivot = new Vector2(0.5f, 0.5f);

            Debug.Log("FullscreenManager: Visual panel RawImage rect set to stretch.");
        }
    }
    
    private void SaveOriginalSettings()
    {
        originalPosition = rightPanelRect.anchoredPosition;
        originalSize = rightPanelRect.sizeDelta;
        originalAnchorMin = rightPanelRect.anchorMin;
        originalAnchorMax = rightPanelRect.anchorMax;
        originalPivot = rightPanelRect.pivot;
        originalOffsetMin = rightPanelRect.offsetMin;
        originalOffsetMax = rightPanelRect.offsetMax;
        
        Debug.Log($"FullscreenManager: Original settings saved - Size: {originalSize}, Position: {originalPosition}");
    }
    
    public void EnterFullscreen()
    {
        if (isFullscreen) return;
        
        Debug.Log("FullscreenManager: Entering fullscreen mode - NEW VERSION");
        isFullscreen = true;
        
        // 隐藏左侧面板
        if (leftPanelContainer != null)
        {
            leftPanelContainer.SetActive(false);
            Debug.Log("FullscreenManager: Left panel hidden");
        }
        
        // 诊断信息 - 检查当前设置的对象
        if (rightPanelContainer != null)
        {
            Debug.Log($"FullscreenManager: Right panel name: {rightPanelContainer.name}");
            Debug.Log($"FullscreenManager: Right panel path: {GetGameObjectPath(rightPanelContainer)}");
            Debug.Log($"FullscreenManager: Right panel parent: {(rightPanelContainer.transform.parent ? rightPanelContainer.transform.parent.name : "None")}");
        }
        
        // 将右侧面板扩展到整个Canvas区域
        if (rightPanelRect != null)
        {
            Debug.Log($"FullscreenManager: Before - Position: {rightPanelRect.anchoredPosition}, Size: {rightPanelRect.sizeDelta}");
            Debug.Log($"FullscreenManager: Before - Anchors: min={rightPanelRect.anchorMin}, max={rightPanelRect.anchorMax}");
            
            // 强制设置为完全全屏 - 不管margin设置
            rightPanelRect.anchorMin = Vector2.zero;        // (0,0)
            rightPanelRect.anchorMax = Vector2.one;         // (1,1)  
            rightPanelRect.pivot = new Vector2(0.5f, 0.5f);
            rightPanelRect.offsetMin = Vector2.zero;        // 清除所有偏移
            rightPanelRect.offsetMax = Vector2.zero;        
            rightPanelRect.anchoredPosition = Vector2.zero; // 清除位置
            rightPanelRect.sizeDelta = Vector2.zero;        // 清除手动尺寸
            
            Debug.Log($"FullscreenManager: After - Position: {rightPanelRect.anchoredPosition}, Size: {rightPanelRect.sizeDelta}");
            Debug.Log($"FullscreenManager: After - Anchors: min={rightPanelRect.anchorMin}, max={rightPanelRect.anchorMax}");
            Debug.Log($"FullscreenManager: After - Offsets: min={rightPanelRect.offsetMin}, max={rightPanelRect.offsetMax}");
            
            // 强制立即更新
            rightPanelRect.gameObject.SetActive(false);
            rightPanelRect.gameObject.SetActive(true);
            
            Debug.Log("FullscreenManager: Right panel should now be fullscreen!");
        }
    }
    
    public void ExitFullscreen()
    {
        if (!isFullscreen) return;
        
        Debug.Log("FullscreenManager: Exiting fullscreen mode");
        isFullscreen = false;
        
        // 显示左侧面板
        if (leftPanelContainer != null)
        {
            leftPanelContainer.SetActive(true);
            Debug.Log("FullscreenManager: Left panel shown");
        }
        
        // 完全恢复右侧面板原始设置
        if (rightPanelRect != null)
        {
            rightPanelRect.anchorMin = originalAnchorMin;
            rightPanelRect.anchorMax = originalAnchorMax;
            rightPanelRect.pivot = originalPivot;
            rightPanelRect.offsetMin = originalOffsetMin;
            rightPanelRect.offsetMax = originalOffsetMax;
            rightPanelRect.anchoredPosition = originalPosition;
            rightPanelRect.sizeDelta = originalSize;
            
            Debug.Log($"FullscreenManager: Right panel restored - Size: {originalSize}, Position: {originalPosition}");
        }
    }
    
    public bool IsFullscreen()
    {
        return isFullscreen;
    }
    
    // 用于测试的快捷键
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isFullscreen)
                ExitFullscreen();
            else
                EnterFullscreen();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isFullscreen)
                ExitFullscreen();
        }
    }
    
    // 辅助函数：获取GameObject的完整路径
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
} 