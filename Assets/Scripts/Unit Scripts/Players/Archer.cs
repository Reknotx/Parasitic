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
    private bool hasTrueDamage = false;

    /// <summary>
    /// Triggers the normal attack of the archer.
    /// </summary>
    public override void NormalAttack(Action callback)
    {
        Debug.Log("Archer Normal Attack");
        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetEnemies);
        StartCoroutine(NormalAttackCR(callback));

    }

    /// <summary>
    /// Triggers the archer's first ability which heals players.
    /// </summary>
    public override void AbilityOne(Action callback)
    {
        Debug.Log("Archer Ability One");
        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetPlayers);
        StartCoroutine(AbilityOneCR(callback));
    }

    /// <summary>
    /// Triggers the archer's second ability.
    /// </summary>
    public override void AbilityTwo(Action callback)
    {
        Debug.Log("Archer Ability Two");
        hasTrueDamage = true;
        ActionRange.Instance.ActionDeselected(false);
        StartAbilityTwoCD();
        CombatSystem.Instance.SetAbilityTwoButtonState(false);
    }

    protected override IEnumerator NormalAttackCR(Action callback)
    {
        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        ActionRange.Instance.ActionDeselected();

        Debug.Log("Given a target");
        if (CharacterSelector.Instance.SelectedTargetUnit == this)
        {
            Debug.Log("Can't attack yourself.");
        }
        else if (CharacterSelector.Instance.SelectedTargetUnit.TakeDamage(AttackStat + (int)currentTile.TileBoost(TileEffect.Attack), hasTrueDamage))
        {
            CombatSystem.Instance.KillUnit(CharacterSelector.Instance.SelectedTargetUnit);
            Upgrades.Instance.ArcherXp += 50;
        }

        hasTrueDamage = false;

        callback();
    }

    /// <summary>
    /// Heals the player
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    protected override IEnumerator AbilityOneCR(Action callback)
    {
        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        ActionRange.Instance.ActionDeselected();

        if (CharacterSelector.Instance.SelectedTargetUnit is Player)
        {
            Player target = (Player) CharacterSelector.Instance.SelectedTargetUnit;

            target.Heal();
        }

        StartAbilityOneCD();

        callback();
    }

    protected override IEnumerator AbilityTwoCR(Action callback)
    {

        throw new System.NotImplementedException();
    }
}
