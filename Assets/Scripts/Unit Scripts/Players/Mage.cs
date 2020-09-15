using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mage : Player
{
    /// <summary>
    /// Mage's first ability. AOE fire blast.
    /// </summary>
    public override void AbilityOne(Action callback)
    {
        Debug.Log("Mage Ability One");
    }

    /// <summary>
    /// Mage's second ability. Damage boost.
    /// </summary>
    public override void AbilityTwo(Action callback)
    {
        Debug.Log("Mage Ability Two");
    }

    /// <summary>
    /// Mage's normal attack.
    /// </summary>
    public override void NormalAttack(Action callback)
    {
        Debug.Log("Mage Normal Attack");
    }

    protected override IEnumerator NormalAttackCR(Action callback)
    {

        callback.Invoke();
        yield return null;
    }

    protected override IEnumerator AbilityOneCR(Action callback)
    {
        throw new System.NotImplementedException();
    }

    protected override IEnumerator AbilityTwoCR(Action callback)
    {
        throw new System.NotImplementedException();
    }
}
