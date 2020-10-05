using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spiker : Enemy
{
    public override void Attack()
    {
        Debug.Log("Spiker Attack");
        base.Attack();
    }

    public override void Defend()
    {
        Debug.Log("Spiker Defend");
        base.Defend();
    }

    public override void Move(List<Tile> path)
    {
        if (path != null)
        {
            CharacterSelector.Instance.unitMoving = true;
            CombatSystem.Instance.SetBattleState(BattleState.PerformingAction);
            State = HumanoidState.Moving;
            StartCoroutine(MoveCR(path));
        }
        if (CheckIfInRangeOfTarget())
        {
            base.Move(path);
        }

        /*
         * 1. If not at max range from target, but are within the range meaning
         * less than three tiles from player and enemy, then find the path to 
         * the closest tile that keeps them at max range.
         * 2. If well beyond max range, move until you are within range of player.
         * 3. Once spiker is at max range from target, execute an attack to deal
         * damage to the player character.
         * 4. 
         * 
         */
    }

    private bool CheckIfAtMaxRange()
    {

        return false;
    }

    //public override void Dodge()
    //{
    //    Debug.Log("Spiker Dodge");
    //    base.Dodge();
    //}
}
