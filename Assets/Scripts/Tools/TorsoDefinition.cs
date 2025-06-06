using UnityEngine;

[CreateAssetMenu(menuName = "Game/Definitions/TorsoDefinition")]
public class TorsoDefinition : ScriptableObject
{
    [Tooltip("The torso prefab that contains all socket child transforms.")]
    public GameObject torsoPrefab;

    [Tooltip("These must match exact Transform names inside the torso prefab.")]
    public string[] socketNames;
}
