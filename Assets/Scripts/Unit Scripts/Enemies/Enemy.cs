/*
 * Author: Chase O'Connor
 * Date: 9/4/2020
 * 
 * Brief: Enemy base class file
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Humanoid, IEnemy
{
    public virtual void Attack()
    {
        _currTarget.TakeDamage(base.AttackStat);
    }

    public virtual void Defend()
    {

    }

    public virtual void Dodge()
    {

    }

    /// <summary> The current target of the enemy. </summary>
    private Player _currTarget;

    public bool Taunted { get; set; } = false;


    /// <summary>
    /// Runs a search on all of the active players to see which player is closer. Then
    /// will find the path to that player.
    /// </summary>
    /// <returns>The path to the closest player.</returns>
    public List<Tile> FindNearestPlayer()
    {
        Player[] activePlayers = FindObjectsOfType<Player>();

        Player targetPlayer = null;
        float shortestDist = 0f;

        if(Taunted && _currTarget != null)
        {
            foreach (Player player in activePlayers)
            {
                float tempDist = Vector3.Distance(this.transform.position, player.transform.position);

                if (shortestDist == 0f || tempDist < shortestDist)
                {
                    targetPlayer = player;
                    shortestDist = tempDist;
                }
            }
        }
        else
        {
            targetPlayer = _currTarget;
        }

        Tile target = targetPlayer.currentTile;
        List<Tile> path = null;
        List<Tile> tempPath = MapGrid.Instance.FindPath(currentTile, target, false, true);

        //Debug.Log(currentTile);
        //Debug.Log(tempPath.ToString());
        if (tempPath == null)
        {
            tempPath = MapGrid.Instance.FindPath(currentTile, target, true);
            if (tempPath == null)
            {
                //find a different target
                for (int index = 0; index < activePlayers.Length; index++)
                {
                    if (activePlayers[index] == targetPlayer)
                    {
                        activePlayers[index] = null;
                        break;
                    }
                }
                //Examine what is the second closest player
                foreach (Player player in activePlayers)
                {
                    if (player == null) continue;

                    float tempDist = Vector3.Distance(this.transform.position, player.transform.position);

                    if (shortestDist == 0f || tempDist < shortestDist)
                    {
                        targetPlayer = player;
                        shortestDist = tempDist;
                    }
                }
            }
            else
            {
                //truncate (remove tiles from path until we are no longer blocked.)
                foreach(Tile tile in tempPath)
                {
                    if (tile.occupied) break;
                    path.Add(tile);
                }
            }
        }
        else
        {
            path = tempPath;

            path.RemoveAt(path.Count - 1);
        }

        _currTarget = targetPlayer;
        return path;
    }

    /// <summary>
    /// Runs a check to see if the target player is within range of their attack.
    /// </summary>
    /// <returns>True if in range, false otherwise.</returns>
    public bool CheckIfInRangeOfTarget()
    {
        List<Tile> neighbors = MapGrid.Instance.GetNeighbors(currentTile);

        foreach (Tile tile in neighbors)
        {
            if (tile.occupant == _currTarget)
            {
                return true;
            }
        }
        return false;
    }

    public void ForceTarget(Player player)
    {
        _currTarget = player;
        Taunted = true;
    }

    public override void AdvanceTimer()
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.ReduceDuration())
            {

                switch (effect.GetEffectType())
                {
                    case StatusEffect.StatusEffectType.Taunted:
                        Taunted = false;
                        _currTarget = null;
                        break;

                    case StatusEffect.StatusEffectType.AttackDown:
                        ResetStats();
                        break;

                    default:
                        break;
                }

                statusEffects.Remove(effect);

                if (statusEffects.Count == 0)
                {
                    CombatSystem.Instance.UnsubscribeAlteredUnit(this);
                }
            }
        }
    }
}
