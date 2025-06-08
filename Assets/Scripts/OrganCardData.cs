using UnityEngine;

[CreateAssetMenu(fileName = "OrganCard", menuName = "Organ/Card")]
public class OrganCardData : ScriptableObject
{
    public GameObject organPrefab;          // 指向实际的3D模型
    public Vector2Int gridSize = new Vector2Int(1, 1);  // 占用格子尺寸
    public Sprite icon;                     // 可选：用于 UI 显示的图标
    public string displayName;              // 可选：器官名称
    public string description;              // 可选：器官描述
    public Vector3 previewRotation = Vector3.zero; // 可选：截图时模型的旋转角度
}