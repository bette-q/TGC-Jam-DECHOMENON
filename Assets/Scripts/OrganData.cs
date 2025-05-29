using UnityEngine;


public enum OrganType 
{
    Organic,
    Cellular,
    Genetic
}

public class OrganCard
{
    public GameObject organPrefab;
    public string organName;
    public OrganType type;

    public OrganCard(GameObject prefab)
    {
        organPrefab = prefab;
        organName = prefab.name;
        AssignRandomType();
    }
    public void AssignRandomType()
    {
        type = (OrganType)Random.Range(0, 3);
    }
    public string GetDefinition()
    {
        return type.ToString();
    }


}

