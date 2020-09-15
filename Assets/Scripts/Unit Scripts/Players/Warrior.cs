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
         * 
         * 
         */
    }

    /// <summary>
    /// Warrior's second ability. Taunts nearby enemies.
    /// </summary>
    public override void AbilityTwo(Action callback)
    {
        Debug.Log("Warrior Ability Two");
    }

    protected override IEnumerator NormalAttackCR(Action callback)
    {
        Debug.Log("Select a target for the warrior's normal attack.");

        yield return new WaitUntil(() => CharacterSelector.Instance.SelectedTargetUnit != null);

        Debug.Log("Given a target");
        if (CharacterSelector.Instance.SelectedTargetUnit == this)
        {
            Debug.Log("Can't attack yourself.");
        }
        else
        {
            CharacterSelector.Instance.SelectedTargetUnit.TakeDamage(BaseAttack);
        }

        callback();

    }

    protected override IEnumerator AbilityOneCR(Action callback)
    {
        bool[,] range = MapGrid.Instance.FindTilesInRange(currentTile, Ability1Range, true);
        Tile[,] tempGrid = MapGrid.Instance.grid;

        for (int i = 0; i < tempGrid.GetLength(0); i++)
        {
            for (int j = 0; j < tempGrid.GetLength(1); j++)
            {
                //Spot was not in range.
                if (!range[i, j]) continue;

                //if (tempGrid[i, j].occupied && tempGrid)
            }
        }
        yield return null;
    }

    protected override IEnumerator AbilityTwoCR(Action callback)
    {
        throw new System.NotImplementedException();
    }

    //IEnumerator NormalAttackCR()
    //{
    //    Debug.Log("Select target for normal attack.");
    //    yield return new WaitUntil(() => target != null);
    //    Debug.Log("Attacking " + target.gameObject.name);

    //    if (target == player) yield break;

    //    //if(target.TakeDamage(((IStatistics)player).BaseAttack)) { Destroy(target.gameObject); }

    //    ((IPlayer)player).NormalAttack(target);

    //    EndUnitTurn(player);

    //    yield return false;
    //}
}
