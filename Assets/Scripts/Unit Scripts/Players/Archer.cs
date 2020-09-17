/*
 * Author: Chase O'Connor
 * Date: 9/4/2020
 * 
 * Brief: Archer class file.
 */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Player
{
    /// <summary>
    /// Triggers the normal attack of the archer.
    /// </summary>
    public override void NormalAttack(Action callback)
    {
        Debug.Log("Archer Normal Attack");
    }

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

    protected override IEnumerator NormalAttackCR(Action callback)
    {
        Debug.Log("Select a target for the archer's normal attack.");

        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        Debug.Log("Given a target");
        if (CharacterSelector.Instance.SelectedTargetUnit == this)
        {
            Debug.Log("Can't attack yourself.");
        }
        else if (CharacterSelector.Instance.SelectedTargetUnit.TakeDamage(AttackStat))
        {
            CombatSystem.Instance.KillUnit(CharacterSelector.Instance.SelectedTargetUnit);
        }

        callback();
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
