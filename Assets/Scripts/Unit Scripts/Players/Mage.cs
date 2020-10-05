/*
 * Author: Chase O'Connor
 * Date: 9/4/2020
 * 
 * Brief: Mage class file
 */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mage : Player
{

    /// <summary>
    /// Mage's normal attack.
    /// </summary>
    public override void NormalAttack(Action callback)
    {
        Debug.Log("Mage Normal Attack");
        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetEnemies);
        StartCoroutine(NormalAttackCR(callback));
    }

    /// <summary>
    /// Mage's first ability. AOE fire blast.
    /// </summary>
    public override void AbilityOne(Action callback)
    {
        Debug.Log("Mage Ability One");

        CharacterSelector.Instance.SetTargettingType(CharacterSelector.TargettingType.TargetEnemies);

        StartCoroutine(AbilityOneCR(callback));
    }

    /// <summary>
    /// Mage's second ability. Damage boost.
    /// </summary>
    public override void AbilityTwo(Action callback)
    {
        Debug.Log("Mage Ability Two");

        CreateAttackUpStatusEffect(this, this);

        ActionRange.Instance.ActionDeselected(false);

        CombatSystem.Instance.SetAbilityTwoButtonState(false);

        CombatSystem.Instance.SetBattleState(BattleState.Idle);

        StartAbilityTwoCD();
    }

    protected override IEnumerator NormalAttackCR(Action callback)
    {
        Debug.Log("Select a target for the mage's normal attack.");

        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        ActionRange.Instance.ActionDeselected();

        int damageModifier = CheckForEffectOfType(StatusEffect.StatusEffectType.AttackUp) ? AttackStat / 2 : 0;

        Debug.Log("Given a target");
        if (CharacterSelector.Instance.SelectedTargetUnit == this)
        {
            Debug.Log("Can't attack yourself.");
        }
        else if (CharacterSelector.Instance.SelectedTargetUnit.TakeDamage(AttackStat + damageModifier + (int)currentTile.TileBoost(TileEffect.Attack)))
        {
            CombatSystem.Instance.KillUnit(CharacterSelector.Instance.SelectedTargetUnit);
        }

        callback();
    }

    /// <summary>
    /// AOE
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns>
    protected override IEnumerator AbilityOneCR(Action callback)
    {
        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        ActionRange.Instance.ActionDeselected();

        int damageModifier = CheckForEffectOfType(StatusEffect.StatusEffectType.AttackUp) ? AttackStat / 2 : 0;
        Enemy focus = (Enemy) CharacterSelector.Instance.SelectedTargetUnit;
        bool[,] range = MapGrid.Instance.FindTilesInRange(focus.currentTile, 1, true);
        Tile[,] tempGrid = MapGrid.Instance.grid;
        List<Enemy> enemies = new List<Enemy>();

        enemies.Add(focus);

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

        int damageToDeal = (AttackStat / 3) + damageModifier + (int)currentTile.TileBoost(TileEffect.Attack);

        List<Enemy> killList = new List<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            if (enemy.TakeDamage(damageToDeal))
            {
                killList.Add(enemy);
            }
        }

        foreach (Enemy enemy in killList)
        {
            CombatSystem.Instance.KillUnit(enemy);
        }

        StartAbilityOneCD();

        callback();
    }

    protected override IEnumerator AbilityTwoCR(Action callback)
    {
        throw new System.NotImplementedException();
    }
}
