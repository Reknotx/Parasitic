using UnityEngine;
using UnityEngine.UI;


public enum UnitToUpgrade
{
    mage,
    knight,
    archer,
    none
}
public class UpgradeButton : MonoBehaviour
{
    public UnitToUpgrade unitToUpgrade;
    public Abilities abilityToUnlock;
    public Abilities requiredAbility;

    public int pointRequirement = 1;

    public Image lockImage;
}
