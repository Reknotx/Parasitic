/*
 * Author: Chase O'Connor
 * Date: 9/4/2020
 * 
 * Brief: Warrior class file.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable IDE0020 // Use pattern matching

public class Warrior : Player
{

    public override void Start()
    {
        healthBar = CombatSystem.Instance.knightHealthSlider;
        healthText = CombatSystem.Instance.knightHealthText;
        base.Start();
    }

    #region Normal Attack
    /// <summary>
    /// The warrior's attack.
    /// </summary>
    public override void NormalAttack(Action callback)
    {
        //Debug.Log("Warrior Normal Attack");
        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetEnemies);
        ///Execute the animation
        //target.TakeDamage(BaseAttack);
        StartCoroutine(NormalAttackCR(callback));
    }

    protected override IEnumerator NormalAttackCR(Action callback)
    {
        //Debug.Log("Select a target for the warrior's normal attack.");

        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        ActionRange.Instance.ActionDeselected();

        StartCoroutine(LookToTarget());
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => IsTurning == false);
        LookToTarget();

        //Debug.Log("Given a target");
        if (CharacterSelector.Instance.SelectedTargetUnit == this)
        {
            Debug.Log("Can't attack yourself.");
        }
        else if (CharacterSelector.Instance.SelectedTargetUnit is Enemy)
        {
            Enemy attackedEnemy = (Enemy)CharacterSelector.Instance.SelectedTargetUnit;
            int oldEnemyHealth = attackedEnemy.Health;

            AttackAnim();

            yield return new WaitUntil(() => AnimationComplete);

            if (attackedEnemy.TakeDamage(AttackStat + (int)currentTile.TileBoost(TileEffect.Attack)))
            {
                if (!attackedEnemy.playersWhoAttacked.Contains(this)) attackedEnemy.playersWhoAttacked.Add(this);

                CombatSystem.Instance.KillUnit(attackedEnemy);
            }
            else if (!attackedEnemy.playersWhoAttacked.Contains(this) && attackedEnemy.Health < oldEnemyHealth)
            {
                attackedEnemy.playersWhoAttacked.Add(this);
            }

        }

        callback();

    }
    #endregion

    #region Ability One
    /// <summary>
    /// Warrior's first ability. Lowers attack of enemies in radius.
    /// </summary>
    public override void AbilityOne(Action callback)
    {
        //Debug.Log("Warrior Ability One");

        /*
         * Scare the surrounding enemies of the warrior and causing those
         * enemies to deal less damage. Effectively lowering their attack stat.
         */

        StartCoroutine(AbilityOneCR(callback));
    }

    /// <summary>
    /// Warrior's first ability. Lowers attack of enemies in radius.
    /// </summary>
    protected override IEnumerator AbilityOneCR(Action callback)
    {
        ActionRange.Instance.ActionDeselected();
        bool[,] range = MapGrid.Instance.FindTilesInRange(currentTile, AbilityOneRange, true);
        Tile[,] tempGrid = MapGrid.Instance.grid;
        List<Enemy> enemies = new List<Enemy>();


        for (int i = 0; i < tempGrid.GetLength(0); i++)
        {
            for (int j = 0; j < tempGrid.GetLength(1); j++)
            {
                //Spot was not in range.
                if (!range[i, j]) continue;

                if (tempGrid[i, j].occupied && tempGrid[i, j].occupant is Enemy)
                {
                    if (!enemies.Contains((Enemy)(tempGrid[i, j].occupant)))
                        enemies.Add((Enemy)(tempGrid[i, j].occupant));
                }
            }
        }

        float attackReductionVal = 0f;
        bool ability1U1 = Upgrades.Instance.IsAbilityUnlocked(Abilities.ability1Upgrade1, UnitToUpgrade.knight);
        attackReductionVal = ability1U1 ? .75f : .5f;

        AbilityOneAnim();

        yield return new WaitUntil(() => AnimationComplete);

        ActivateAbilityOneParticle();

        foreach (Enemy enemy in enemies)
        {
            //enemy.CreateAttackDownStatusEffect(this, enemy);
            StatusEffect effect = new StatusEffect(StatusEffect.StatusEffectType.AttackDown, 3, this, enemy);
            enemy.AddStatusEffect(effect);
            enemy.AttackStat = Mathf.FloorToInt(AttackStat * attackReductionVal);
        }

        yield return null;

        StartAbilityOneCD();

        callback();
    }
    #endregion

    #region Ability Two
    /// <summary>
    /// Warrior's second ability. Taunts nearby enemies.
    /// </summary>
    public override void AbilityTwo(Action callback)
    {
        //Debug.Log("Warrior Ability Two");

        StartCoroutine(AbilityTwoCR(callback));
    }

    /// <summary>
    /// Warrior's second ability. Taunts nearby enemies.
    /// </summary>
    protected override IEnumerator AbilityTwoCR(Action callback)
    {
        ActionRange.Instance.ActionDeselected();
        bool[,] range = MapGrid.Instance.FindTilesInRange(currentTile, AbilityOneRange, true);
        Tile[,] tempGrid = MapGrid.Instance.grid;
        List<Enemy> enemies = new List<Enemy>();


        for (int i = 0; i < tempGrid.GetLength(0); i++)
        {
            for (int j = 0; j < tempGrid.GetLength(1); j++)
            {
                //Spot was not in range.
                if (!range[i, j]) continue;

                if (tempGrid[i, j].occupied && tempGrid[i, j].occupant is Enemy)
                {
                    enemies.Add((Enemy)(tempGrid[i, j].occupant));
                }
            }
        }

        AbilityTwoAnim();

        yield return new WaitUntil(() => AnimationComplete);

        ActivateAbilityTwoParticle();

        foreach (Enemy enemy in enemies)
        {
            StatusEffect effect = new StatusEffect(StatusEffect.StatusEffectType.Taunted, 3, this, enemy);

            enemy.AddStatusEffect(effect);
        }

        yield return null;

        if (Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2Upgrade1, UnitToUpgrade.knight))
        {
            StatusEffect effect = new StatusEffect(StatusEffect.StatusEffectType.DefenseUp, 2, this, this);
            statusEffects.Add(effect);
            DefenseStat += 2;
        }

        StartAbilityTwoCD();

        callback();
    }
    #endregion

    #region Upgrade Functions
    protected override void AttackUpgradeOne()
    {
        AttackStat += 3;
    }

    protected override void AttackUpgradeTwo()
    {
        Debug.Log("Attack range increased by 1.");
        AttackRange++;
        FindActionRanges();
    }

    protected override void AbilityOneUpgradeOne()
    {
        Debug.Log("Increase damage reduciton value by 25%.");
    }

    protected override void AbilityOneUpgradeTwo()
    {
        AbilityOneRange += 2;
        FindActionRanges();
        Debug.Log("Ability range is increased by 2 tiles.");
    }

    protected override void AbilityTwoUpgradeOne()
    {
        Debug.Log("Ability now increases the caster's defense by 2 points on cast.");
    }

    protected override void AbilityTwoUpgradeTwo()
    {
        AbilityTwoRange += 1;
        FindActionRanges();
        Debug.Log("Ability range is increased by 1 tile.");
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
