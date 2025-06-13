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
    public OrganType curType;
    private OrganType originalType;
    public GameObject previewPrefab;

    public OrganCard(GameObject prefab)
    {
        organPrefab = prefab;
        organName = prefab.name;
        previewPrefab = null;
        AssignRandomType();
    }
    public void AssignRandomType()
    {
        curType = (OrganType)Random.Range(0, 3);
        originalType = curType;
    }
    public string GetDefinition()
    {
        return curType.ToString();
    }
    public void RevertToOriginal()
    {
        curType = originalType;
    }
    public void AssignDefinition(OrganType typeIn)
    {
        curType = typeIn;
    }

}

