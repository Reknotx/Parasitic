using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Player
{
    /// <summary>
    /// Triggers the archer's first ability.
    /// </summary>
    public override void AbilityOne(Action callback)
    {
        Debug.Log("Archer Ability One");
    }

    /// <summary>
    /// Triggers the archer's second ability.
    /// </summary>
    public override void AbilityTwo(Action callback)
    {
        Debug.Log("Archer Ability Two");
    }

    /// <summary>
    /// Triggers the normal attack of the archer.
    /// </summary>
    public override void NormalAttack(Action callback)
    {
        Debug.Log("Archer Normal Attack");
    }

    protected override IEnumerator NormalAttackCR(Action callback)
    {
        callback();
        yield return null;
    }

    protected override IEnumerator AbilityOneCR(Action callback)
    {
        yield return null;
        throw new System.NotImplementedException();
    }

    protected override IEnumerator AbilityTwoCR(Action callback)
    {
        throw new System.NotImplementedException();
    }
}
