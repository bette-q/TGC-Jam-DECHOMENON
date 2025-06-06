using UnityEngine;

[CreateAssetMenu(menuName = "Game/Definitions/OrganDefinition")]
public class OrganDefinition : ScriptableObject
{
    [Tooltip("The organ prefab that has a child socket bone (e.g. 'SocketHead').")]
    public GameObject organPrefab;

    [Tooltip("Name of the child Transform inside organPrefab to align with the torso socket.")]
    public string socketHeadName;
}
