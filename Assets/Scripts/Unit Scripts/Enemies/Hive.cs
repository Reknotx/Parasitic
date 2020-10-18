using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hive : Enemy
{

    Spawner spawner;
    public override void Start()
    {
        base.Start();
        spawner = GetComponent<Spawner>();
        //if spawner script is not attached add it and set spawns to 0
        if (!spawner)
        {
            spawner = gameObject.AddComponent<Spawner>() as Spawner;
            spawner.maxSpawns = 0;
        }
    }

    public void SpawnEnemy()
    {
        if (spawner.SpawnsRemaining())
        {
            bool[,] neighbors = MapGrid.Instance.FindTilesInRange(currentTile, AttackRange, true);
            Tile[,] tempGrid = MapGrid.Instance.grid;
            List<Tile> OpenTiles = new List<Tile>();
            for (int i = 0; i < neighbors.GetLength(0); i++)
            {
                for (int j = 0; j < neighbors.GetLength(1); j++)
                {
                    if (!neighbors[i, j]) continue;

                    if (!tempGrid[i, j].occupied && tempGrid[i, j].movementTile)
                    {
                        OpenTiles.Add(tempGrid[i, j]);
                    }
                }
            }
            spawner.SpawnUnit(OpenTiles[Random.Range(0, OpenTiles.Count)]);
        }

    }

    public void Shuffle(List<Tile> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            Tile temp= list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }


    public override void Attack()
    {
        Debug.Log("Larva Attack");
        base.Attack();
    }


    public override void Defend()
    {
        Debug.Log("Larva Defend");
        base.Defend();
    }

}
