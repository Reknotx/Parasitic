﻿using System;
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

        Debug.Log("Select a target for the mage's normal attack.");

        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        Debug.Log("Given a target");
        if (CharacterSelector.Instance.SelectedTargetUnit == this)
        {
            Debug.Log("Can't attack yourself.");
        }
        else if (CharacterSelector.Instance.SelectedTargetUnit.TakeDamage(BaseAttack))
        {
            CombatSystem.Instance.KillUnit(CharacterSelector.Instance.SelectedTargetUnit);
        }

        callback();
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
