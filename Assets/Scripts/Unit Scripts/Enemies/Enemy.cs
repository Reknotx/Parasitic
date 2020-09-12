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
                    Debug.Log(tile);
                }
            }
        }
        else
        {
            path = tempPath;

            path.RemoveAt(path.Count - 1);
        }

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
