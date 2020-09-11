using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Player
{
    public override void AbilityOne(Humanoid target)
    {
        Debug.Log("Archer Ability One");
    }

    public override void AbilityTwo(Humanoid target)
    {
        Debug.Log("Archer Ability Two");
    }

    public override void NormalAttack(Humanoid target)
    {
        Debug.Log("Archer Normal Attack");
    }
}
