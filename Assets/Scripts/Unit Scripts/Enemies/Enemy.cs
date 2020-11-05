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
    #region Combat Functions
    public virtual void Attack()
    {
        AnimationComplete = false;
        //if (_currTarget.TakeDamage(base.AttackStat + (int)currentTile.TileBoost(TileEffect.Attack))) CombatSystem.Instance.KillUnit(_currTarget);
        StartCoroutine(AttackCR());
    }

    IEnumerator AttackCR()
    {
        ///MESSAGE FOR RYAN 10/27/2020
        ///Hey dude if you get to this point where you need to set up the animations and trigger the behavior so that
        ///the damage is only applied on the animation event as well as activating the animations themselves just
        ///uncomment these two lines and you should be just fine :) Just make sure that your triggers are set
        ///with the proper naming or you can adjust the trigger name here too, up to you too. Good luck
        //animatorController.SetTrigger("CastAttack");

        //yield return new WaitUntil(() => AnimationComplete);
        StartCoroutine(LookToTarget());
        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => IsTurning == false);

        int attack = AttackStat;

        if (CheckForEffectOfType(StatusEffect.StatusEffectType.AttackDown))
            attack /= 2;

        if (_currTarget.TakeDamage(attack + (int)currentTile.TileBoost(TileEffect.Attack))) CombatSystem.Instance.KillUnit(_currTarget);
    }

    public virtual void Defend()
    {
        DefendState = DefendingState.Defending;
    }
    #endregion

    //public virtual void Dodge()
    //{
    //    //Activate the dodge animation

    //}
    public GameObject healthCanvas;
    public List<Player> playersWhoAttacked = new List<Player>();

    /// <summary> The current target of the enemy. </summary>
    protected Player _currTarget;

    /// <summary> Indicates that this enemy currently being taunted by the warrior. </summary>
    //public bool Taunted { get; set; } = false;

    /// <summary> Indicates that an enemy is not hidden by fog. </summary>
    public bool Revealed { get; set; } = true;

    [SerializeField] private ParticleSystem TauntedParticle;

    #region Move Functions
    public override void Move(List<Tile> path, bool bypassRangeCheck = false)
    {
        if (bypassRangeCheck || CheckIfInRangeOfTarget() == false)
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
        //shorten path so the enemy is as far as they can be when they attack
        for(int i = path.Count - 1; i >= 0; i--)
        {
            if (CheckIfInRangeOfTarget(path[i]))
            {
                if(path.Count - 1 != i)
                {
                    path.RemoveRange(i+1,1);
                }
            }
            else
            {
                break;
            }
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
    public bool CheckIfInRangeOfTarget(Tile target = null)
    {
        if (_currTarget == null) return false;
        if(target == null)
        {
            target = currentTile;
        }
        //List<Tile> neighbors = MapGrid.Instance.GetNeighbors(currentTile);
        bool[,] neighbors = MapGrid.Instance.FindTilesInRange(target, AttackRange, true, AttackShape);
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

    protected override IEnumerator LookToTarget()
    {
        IsTurning = true;
        Vector3 thisUnit = currentTile.transform.position;
        Vector3 targetUnit = _currTarget.currentTile.transform.position;

        Vector3 angle = (targetUnit - thisUnit).normalized;

        while (LookInDirection(angle))
        {
            yield return new WaitForFixedUpdate();
        }
        print("Done turning");
        IsTurning = false;
    }
    #endregion

    /// <summary>
    /// Adds the enemy to the combat system when they are revealed.
    /// </summary>
    public void OnFogLifted()
    {
        Revealed = true;

        CombatSystem.Instance.SubscribeEnemy(this);
    }

    /// <summary>
    /// Tells us if the enemy is taunted by the warrior.
    /// </summary>
    /// <returns>True if enemy is currently taunted, else false.</returns>
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
            if (effect.Type == StatusEffect.StatusEffectType.Taunted)
            {
                //Taunted = false;
                _currTarget = null;
                CancelInvoke();
            }
            else
            {
                ResetSpecificStat(effect.Type);
            }
            statusEffects.Remove(effect);

        }

        removeList.Clear();
    }

    public override void AddStatusEffect(StatusEffect effect)
    {
        if (effect.Type == StatusEffect.StatusEffectType.Taunted)
        {
            InvokeRepeating("ActivateTauntedParticle", 0f, 10f);
        }

        base.AddStatusEffect(effect);
    }

    private void ActivateTauntedParticle()
    {
        if (TauntedParticle != null)
        {
            TauntedParticle.Play();
        }
    }

    private void OnMouseEnter()
    {
        healthCanvas.SetActive(true);
    }

    private void OnMouseExit()
    {
        healthCanvas.SetActive(false);
    }
}
