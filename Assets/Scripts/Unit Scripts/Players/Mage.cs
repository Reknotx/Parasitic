using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mage : Player
{
    public override void AbilityOne()
    {
        Debug.Log("Mage Ability One");
    }

    public override void AbilityTwo()
    {
        Debug.Log("Mage Ability Two");
    }

    public override void NormalAttack()
    {
        Debug.Log("Mage Normal Attack");
    }
}
