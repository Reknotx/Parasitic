using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Humanoid, IEnemy
{
    public abstract void Attack();
    public abstract void Defend();
    public abstract void Dodge();

    public override void Move(Transform start, Transform target)
    {
        //Enemy movement is dependent on the Astar algorithm
        Debug.Log("Enemy movement");
    }

    public void TestForMovement()
    {
        Player[] activePlayers = GameObject.FindObjectsOfType<Player>();

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

        List<Tile> tempNeighbors = MapGrid.Instance.GetNeighbors(targetPlayer.currentTile);
        Tile target = null;
        float nearestTileDist = 0f;

        foreach (Tile tile in tempNeighbors)
        {
            if (tile.occupied || !tile.movementTile) continue;

            if (nearestTileDist == 0f || Vector2.Distance(tile.gridPosition, currentTile.gridPosition) < nearestTileDist)
            {
                target = tile;
            }
        }

        List<Tile> path = MapGrid.Instance.FindPath(currentTile, target);

        BeginMovement(path);
    }

    //public void OnMouseOver()
    //{
    //    gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
    //}

    //public void OnMouseExit()
    //{
    //    gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
    //}
}
