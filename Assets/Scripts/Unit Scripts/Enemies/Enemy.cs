using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Humanoid, IEnemy
{
    public virtual void Attack()
    {
        _currTarget.TakeDamage(BaseAttack);
    }

    public virtual void Defend()
    {

    }

    public virtual void Dodge()
    {

    }

    private Player _currTarget;

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
}
