using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eventType
{
    addUpgradePoints,
    spawnTutoiralScreen,
    none
}



public class EventTrigger : MonoBehaviour
{
    
    public eventType type;

    public eventType subType; //if there is 2 interastions on one trigger

    public UnitToUpgrade unit;

    public GameObject spawnedObject;

    
}
