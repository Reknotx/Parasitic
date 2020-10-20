﻿/*
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

        if (Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2Upgrade2, UnitToUpgrade.archer))
        {
            MovementStat += 2;
            AttackRange += 2;
        }

        StartAbilityTwoCD();
        CombatSystem.Instance.SetAbilityTwoButtonState(false);
    }

    /// <summary>
    /// Starts the targetting process while we wait for the archer to attack.
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    protected override IEnumerator NormalAttackCR(Action callback)
    {
        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        ActionRange.Instance.ActionDeselected();

        int extraDamage = 0;

        if (hasTrueDamage && Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2Upgrade1, UnitToUpgrade.archer))
        {
            extraDamage = AttackStat;
            MovementStat = _baseMovement;
            AttackRange = _baseRange;
        }

        Debug.Log("Given a target");
        if (CharacterSelector.Instance.SelectedTargetUnit == this)
        {
            Debug.Log("Can't attack yourself.");
        }
        else if (CharacterSelector.Instance.SelectedTargetUnit.TakeDamage(AttackStat + (int)currentTile.TileBoost(TileEffect.Attack) + extraDamage, hasTrueDamage))
        {
            CombatSystem.Instance.KillUnit(CharacterSelector.Instance.SelectedTargetUnit);
            Upgrades.Instance.ArcherXp += 50;
        }
        ///If the attack doesn't kill the enemy, but does deal damage, and we have purchased the first upgrade
        ///for the basic attack, then we will apply a move speed debuff on the enemy hit.
        else if (Upgrades.Instance.IsAbilityUnlocked(Abilities.normalAttackUpgrade1, UnitToUpgrade.archer) 
            && CharacterSelector.Instance.SelectedTargetUnit.damagedThisTurn)
        {
            StatusEffect effect = new StatusEffect(StatusEffect.StatusEffectType.MoveDown, 3, this, CharacterSelector.Instance.SelectedTargetUnit);
            CharacterSelector.Instance.SelectedTargetUnit.AddStatusEffect(effect);
            CharacterSelector.Instance.SelectedTargetUnit.damagedThisTurn = false;
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

    protected override void AttackUpgradeOne()
    {
        Debug.Log("Enemy units will now have their move speed reduced when attack hits.");
    }

    protected override void AttackUpgradeTwo()
    {
        Debug.Log("Increases the accuracy of the player's basic attack.");
    }

    protected override void AbilityOneUpgradeOne()
    {
        Debug.Log("Heal now restores the target's health by 30% of their max.");
    }

    protected override void AbilityOneUpgradeTwo()
    {
        Debug.Log("Ability range is now 4 tiles.");
        AbilityOneRange = 4;
    }

    protected override void AbilityTwoUpgradeOne()
    {
        Debug.Log("Next attack will now deal double damage.");
    }

    protected override void AbilityTwoUpgradeTwo()
    {
        Debug.Log("While ability active move speed and attack range increased by 2.");
    }

    public override void ProcessUpgrade(Abilities abilityToUpgrade)
    {
        switch (abilityToUpgrade)
        {
            case Abilities.normalAttackUpgrade1:
                AttackUpgradeOne();
                break;

            case Abilities.normalAttackUpgrade2:
                AttackUpgradeTwo();
                break;

            case Abilities.ability1Upgrade1:
                AbilityOneUpgradeOne();
                break;

            case Abilities.ability1Upgrade2:
                AbilityTwoUpgradeTwo();
                break;

            case Abilities.ability2Upgrade1:
                AbilityTwoUpgradeOne();
                break;

            case Abilities.ability2Upgrade2:
                AbilityTwoUpgradeTwo();
                break;

            default:
                break;
        }
    }
}
