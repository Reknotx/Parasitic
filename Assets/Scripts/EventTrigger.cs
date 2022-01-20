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

    public int pointsToAdd = 0;

    public GameObject spawnedObject;


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            switch (type)
            {
                case eventType.addUpgradePoints:
                    addPoints(unit);
                    break;
                case eventType.spawnTutoiralScreen:
                    spawnedObject.SetActive(true);
                    break;
                case eventType.none:
                    break;
                default:
                    break;
            }

            switch (subType)
            {
                case eventType.addUpgradePoints:
                    addPoints(unit);
                    break;
                case eventType.spawnTutoiralScreen:
                    spawnedObject.SetActive(true);
                    break;
                case eventType.none:
                    break;
                default:
                    break;
            }
        }
    }


    private void addPoints(UnitToUpgrade unit)
    {
        switch (unit)
        {
            case UnitToUpgrade.mage:
                Upgrades.Instance.MagePoints += pointsToAdd;
                break;
            case UnitToUpgrade.knight:
                Upgrades.Instance.KnightPoints += pointsToAdd;
                break;
            case UnitToUpgrade.archer:
                Upgrades.Instance.ArcherPoints += pointsToAdd;
                break;
            case UnitToUpgrade.none:
                break;
            default:
                break;
        }
    }
}
