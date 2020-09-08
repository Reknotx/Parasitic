using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Player
{
    public override void AbilityOne()
    {
        Debug.Log("Archer Ability One");
    }

    public override void AbilityTwo()
    {
        Debug.Log("Archer Ability Two");
    }

    public override void NormalAttack()
    {
        Debug.Log("Archer Normal Attack");
    }
}
