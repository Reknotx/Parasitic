using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior : Player
{
    public override void AbilityOne()
    {
        Debug.Log("Warrior Ability One");
    }

    public override void AbilityTwo()
    {
        Debug.Log("Warrior Ability Two");
    }

    public override void NormalAttack()
    {
        Debug.Log("Warrior Normal Attack");
    }
}
