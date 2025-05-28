using UnityEngine;


public enum OrganType 
{
    Terminal,
    Conductor,
    Actuator
}
public class OrganData
{
    public OrganType type;
    public bool animate => type == OrganType.Actuator;
          
}
