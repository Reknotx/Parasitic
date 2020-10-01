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

public class Warrior : Player
{
    /// <summary>
    /// The warrior's attack.
    /// </summary>
    public override void NormalAttack(Action callback)
    {
        Debug.Log("Warrior Normal Attack");
        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetEnemies);
        ///Execute the animation
        //target.TakeDamage(BaseAttack);
        StartCoroutine(NormalAttackCR(callback));
    }

    /// <summary>
    /// Warrior's first ability. Lowers attack of enemies in radius.
    /// </summary>
    public override void AbilityOne(Action callback)
    {
        Debug.Log("Warrior Ability One");

        /*
         * Scare the surrounding enemies of the warrior and causing those
         * enemies to deal less damage. Effectively lowering their attack stat.
         */

        StartCoroutine(AbilityOneCR(callback));
    }

    /// <summary>
    /// Warrior's second ability. Taunts nearby enemies.
    /// </summary>
    public override void AbilityTwo(Action callback)
    {
        Debug.Log("Warrior Ability Two");

        StartCoroutine(AbilityTwoCR(callback));
    }

    protected override IEnumerator NormalAttackCR(Action callback)
    {
        Debug.Log("Select a target for the warrior's normal attack.");

        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        ActionRange.Instance.ActionDeselected();

        Debug.Log("Given a target");
        if (CharacterSelector.Instance.SelectedTargetUnit == this)
        {
            Debug.Log("Can't attack yourself.");
        }
        else if(CharacterSelector.Instance.SelectedTargetUnit.TakeDamage(AttackStat))
        {
            CombatSystem.Instance.KillUnit(CharacterSelector.Instance.SelectedTargetUnit);
        }

        callback();

    }

    /// <summary>
    /// Warrior's first ability. Lowers attack of enemies in radius.
    /// </summary>
    protected override IEnumerator AbilityOneCR(Action callback)
    {
        ActionRange.Instance.ActionDeselected();
        bool[,] range = MapGrid.Instance.FindTilesInRange(currentTile, Ability1Range, true);
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

        foreach (Enemy enemy in enemies)
        {
            enemy.CreateAttackDownStatusEffect(this, enemy);
        }

        yield return null;

        StartAbilityOneCD();

        callback();
    }

    /// <summary>
    /// Warrior's second ability. Taunts nearby enemies.
    /// </summary>
    protected override IEnumerator AbilityTwoCR(Action callback)
    {
        ActionRange.Instance.ActionDeselected();
        bool[,] range = MapGrid.Instance.FindTilesInRange(currentTile, Ability1Range, true);
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

        foreach (Enemy enemy in enemies)
        {
            //enemy.ForceTarget(this);
            enemy.CreateTauntedStatusEffect(this, enemy);
        }

        yield return null;

        StartAbilityTwoCD();

        callback();
    }
}
