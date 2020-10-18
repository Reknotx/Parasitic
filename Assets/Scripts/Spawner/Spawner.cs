using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject spawnee;
    public int maxSpawns = 1;
    int _currentSpawns = 0;

    public void SpawnUnit(Tile tile)
    {
        if (SpawnsRemaining() && !tile.occupied)
        {
            GameObject spawnedObject = Instantiate(spawnee, tile.transform.position + Vector3.up * spawnee.transform.position.y, Quaternion.identity);
            _currentSpawns++;
            Enemy unit = spawnedObject.GetComponent<Enemy>();
            if (unit)
            {
                //CombatSystem.Instance.SubscribeEnemy(unit);
                CombatSystem.Instance.NewSpawn(unit);
                //ask chase about subscribe timer
            }

        }
    }

    public bool SpawnsRemaining()
    {
        return _currentSpawns < maxSpawns;
    }
}
