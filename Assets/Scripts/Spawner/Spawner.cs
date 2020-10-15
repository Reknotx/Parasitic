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
            GameObject spawnedObject = Instantiate(spawnee, tile.transform.position + Vector3.up * spawnee.transform.position.y, Quaternion.identity);
            currentSpawns++;
            Enemy unit = spawnedObject.GetComponent<Enemy>();
            if (unit)
            {
                CombatSystem.Instance.SubscribeEnemy(unit);
                //ask chase about subscribe timer
            }

        }
    }
}
