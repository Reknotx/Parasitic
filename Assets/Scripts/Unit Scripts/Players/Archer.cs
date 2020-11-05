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
#pragma warning disable IDE0020 // Use pattern matching

public class Archer : Player
{
    private bool hasTrueDamage = false;

    public override void Start()
    {
        healthBar = CombatSystem.Instance.archerHealthSlider;
        healthText = CombatSystem.Instance.archerHealthText;
        base.Start();
    }

    #region Normal Attack
        /// <summary>
        /// Triggers the normal attack of the archer.
        /// </summary>
    public override void NormalAttack(Action callback)
    {
        //Debug.Log("Archer Normal Attack");
        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetEnemies);
        StartCoroutine(NormalAttackCR(callback));

    }

    protected override IEnumerator NormalAttackCR(Action callback)
    {
        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        ActivateAttackParticle();

        ActionRange.Instance.ActionDeselected();

        int extraDamage = 0;

        if (hasTrueDamage && Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2Upgrade1, UnitToUpgrade.archer))
        {
            extraDamage = AttackStat;
            MovementStat = _baseMovement;
            AttackRange = _baseRange;
        }

        //Debug.Log("Given a target");
        if (CharacterSelector.Instance.SelectedTargetUnit == this)
        {
            Debug.Log("Can't attack yourself.");
        }
        else if (CharacterSelector.Instance.SelectedTargetUnit is Enemy)
        {
            Enemy attackedEnemy = (Enemy)CharacterSelector.Instance.SelectedTargetUnit;

            int oldEnemyHealth = attackedEnemy.Health;

            //animatorController.SetTrigger("CastAttack");

            //yield return new WaitUntil(() => AnimationComplete);

            if (attackedEnemy.TakeDamage(AttackStat + extraDamage + (int)currentTile.TileBoost(TileEffect.Attack), hasTrueDamage))
            {
                if (!attackedEnemy.playersWhoAttacked.Contains(this)) attackedEnemy.playersWhoAttacked.Add(this);

                CombatSystem.Instance.KillUnit(attackedEnemy);
            }
            else if (!attackedEnemy.playersWhoAttacked.Contains(this) && attackedEnemy.Health < oldEnemyHealth)
            {
                attackedEnemy.playersWhoAttacked.Add(this);
            }

            ///If the attack doesn't kill the enemy, but does deal damage, and we have purchased the first upgrade
            ///for the basic attack, then we will apply a move speed debuff on the enemy hit.
            if (Upgrades.Instance.IsAbilityUnlocked(Abilities.normalAttackUpgrade1, UnitToUpgrade.archer)
                && attackedEnemy.Health < oldEnemyHealth)
            {
                StatusEffect effect = new StatusEffect(StatusEffect.StatusEffectType.MoveDown, 3, this, attackedEnemy);
                attackedEnemy.AddStatusEffect(effect);
                attackedEnemy.damagedThisTurn = false;
                attackedEnemy.MovementStat--;
            }
        }

        hasTrueDamage = false;

        callback();
    }
    #endregion

    #region Ability One
    /// <summary>
    /// Triggers the archer's first ability which heals players.
    /// </summary>
    public override void AbilityOne(Action callback)
    {
        //Debug.Log("Archer Ability One");
        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetPlayers);
        StartCoroutine(AbilityOneCR(callback));
    }

    /// <summary>
    /// Heals the player
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    protected override IEnumerator AbilityOneCR(Action callback)
    {
        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        ActivateAbilityTwoParticle();

        if (CharacterSelector.Instance.SelectedTargetUnit is Player)
        {
            ActionRange.Instance.ActionDeselected();

            Player target = (Player)CharacterSelector.Instance.SelectedTargetUnit;

            //animatorController.SetTrigger("CastHeal");

            //yield return new WaitUntil(() => AnimationComplete);

            target.Heal();

            StartAbilityOneCD();

            callback();
        }
    }
    #endregion

    #region Ability Two
    /// <summary>
    /// Triggers the archer's second ability.
    /// </summary>
    public override void AbilityTwo(Action callback)
    {
        //Debug.Log("Archer Ability Two");
        hasTrueDamage = true;
        ActionRange.Instance.ActionDeselected(false);

        if (Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2Upgrade2, UnitToUpgrade.archer))
        {
            Debug.Log("Increasing attack and move range.");
            MovementStat += 2;
            AttackRange += 2;
            FindMovementRange();
            MapGrid.Instance.DrawBoarder(TileRange, ref CharacterSelector.Instance.boarderRenderer);
            FindActionRanges();
        }

        StartAbilityTwoCD();

        //animatorController.SetTrigger("CastEagleEye");

        CombatSystem.Instance.SetBattleState(BattleState.Idle);
        CombatSystem.Instance.SetAbilityTwoButtonState(false);
    }

    protected override IEnumerator AbilityTwoCR(Action callback)
    {
        throw new System.NotImplementedException();
    }
    #endregion

    #region Upgrade Functions
    protected override void AttackUpgradeOne()
    {
        Debug.Log("Enemy units will now have their move speed reduced when attack hits.");
    }

    /// <summary> WIP. NOT YET IMPLEMENTED </summary>
    protected override void AttackUpgradeTwo()
    {
        ///WIP NOT YET IMPLEMENTED
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
        FindActionRanges();
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
                AbilityOneUpgradeTwo();
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
    #endregion
}
