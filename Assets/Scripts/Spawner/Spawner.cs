using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject spawnee;
    public int maxSpawns = 1;
    int currentSpawns = 0;

    public void SpawnUnit(Tile tile)
    {
        if (currentSpawns < maxSpawns && !tile.occupied)
        {
            Instantiate(spawnee, tile.transform.position + Vector3.up, Quaternion.identity);
            currentSpawns++;
        }
    }
}
