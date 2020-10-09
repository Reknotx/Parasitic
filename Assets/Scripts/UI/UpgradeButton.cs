using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum UnitToUpgrade
{
    mage,
    knight,
    archer
}
public class UpgradeButton : MonoBehaviour
{
    public UnitToUpgrade unitToUpgrade;
    public Abilities abilityToUnlock;
    public Abilities requiredAbility;

    public int pointRequirement = 1;


}
