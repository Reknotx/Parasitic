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
        if (_currTarget.TakeDamage(base.AttackStat + (int)currentTile.TileBoost(TileEffect.Attack))) CombatSystem.Instance.KillUnit(_currTarget);
    }

    public virtual void Defend()
    {
        DefendState = DefendingState.Defending;
    }

    //public virtual void Dodge()
    //{
    //    //Activate the dodge animation

    //}

    /// <summary> The current target of the enemy. </summary>
    private Player _currTarget;

    /// <summary> Indicates that this enemy currently being taunted by the warrior. </summary>
    public bool Taunted { get; set; } = false;

    /// <summary> Indicates that an enemy is not hidden by fog. </summary>
    public bool Revealed { get; set; } = true;

    public override void Move(List<Tile> path)
    {
        if (CheckIfInRangeOfTarget() == false)
        {
            base.Move(path);
        }
        else
        {
            HasMoved = true;
        }
    }

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

        foreach (Player player in activePlayers)
        {
            float tempDist = Vector3.Distance(this.transform.position, player.transform.position);

            if (shortestDist == 0f || tempDist < shortestDist)
            {
                targetPlayer = player;
                shortestDist = tempDist;
            }
        }
        List<Tile> path = ObtainPathToTarget(targetPlayer);
        _currTarget = targetPlayer;
        return path;
    }

    /// <summary>
    /// Returns a path to the source of the enemy's taunted status effect.
    /// </summary>
    /// <returns>A list of tiles containing the path we wish to follow.</returns>
    public List<Tile> TauntedPath()
    {
        Humanoid tempH = GetSourceOfStatusEffect(StatusEffect.StatusEffectType.Taunted);

        if (tempH is Player)
        {
            _currTarget = (Player)tempH;
            return ObtainPathToTarget((Player)tempH);
        }

        return null;
    }

    /// <summary>
    /// Private helper funciton that returns the path to the target.
    /// </summary>
    /// <param name="targetPlayer">The target we wish to move too.</param>
    /// <returns>A list of tiles containing the path we wish to follow.</returns>
    private List<Tile> ObtainPathToTarget(Player targetPlayer)
    {
        Tile target = targetPlayer.currentTile;
        List<Tile> path = new List<Tile>();
        List<Tile> tempPath = MapGrid.Instance.FindPath(currentTile, target, false, true);

        //Debug.Log(currentTile);
        //Debug.Log(tempPath.ToString());
        if (tempPath == null)
        {
            tempPath = MapGrid.Instance.FindPath(currentTile, target, true, true);
            float shortestDist = 0f;

            if (tempPath == null)
            {
                Player[] activePlayers = GameObject.FindObjectsOfType<Player>();

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
                foreach (Tile tile in tempPath)
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

        int movementDist = Mathf.Min(MovementStat, path.Count);
        //truncate path to movement range
        path.RemoveRange(movementDist, path.Count - movementDist);
        return path;
    }

    /// <summary>
    /// Runs a check to see if the target player is within range of their attack.
    /// </summary>
    /// <returns>True if in range, false otherwise.</returns>
    public bool CheckIfInRangeOfTarget()
    {
        if (_currTarget == null) return false;

        //List<Tile> neighbors = MapGrid.Instance.GetNeighbors(currentTile);
        bool[,] neighbors = MapGrid.Instance.FindTilesInRange(currentTile, AttackRange, true);
        Tile[,] tempGrid = MapGrid.Instance.grid;
        List<Player> players = new List<Player>();

        for (int i = 0; i < neighbors.GetLength(0); i++)
        {
            for (int j = 0; j < neighbors.GetLength(1); j++)
            {
                if (!neighbors[i, j]) continue;

                if (tempGrid[i, j].occupied && tempGrid[i, j].occupant is Player)
                {
                    if (!players.Contains((Player)(tempGrid[i, j].occupant)))
                        players.Add((Player)(tempGrid[i, j].occupant));
                }
            }
        }

        foreach (Player player in players)
        {
            if (player == _currTarget) return true;
        }

        return false;
    }

    /// <summary>
    /// Adds the enemy to the combat system when they are revealed.
    /// </summary>
    public void OnFogLifted()
    {
        Revealed = true;

        CombatSystem.Instance.SubscribeEnemy(this);
    }

    /// <summary>
    /// Forces the enemy to have a set target. (I.e. when the enemy is taunted.)
    /// </summary>
    /// <param name="player">The target player.</param>
    public void ForceTarget(Player player)
    {
        _currTarget = player;
        Taunted = true;
    }

    public bool IsTaunted()
    {
        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.Type == StatusEffect.StatusEffectType.Taunted)
                return true;
        }
        return false;
    }

    public override void AdvanceTimer()
    {
        List<StatusEffect> removeList = new List<StatusEffect>();

        foreach (StatusEffect effect in statusEffects)
        {
            if (effect.ReduceDuration())
            {
                removeList.Add(effect);
            }
        }

        foreach (StatusEffect effect in removeList)
        {
            switch (effect.Type)
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

        }

        removeList.Clear();

        if (statusEffects.Count == 0)
        {
            CombatSystem.Instance.UnsubscribeAlteredUnit(this);
        }
    }
}
