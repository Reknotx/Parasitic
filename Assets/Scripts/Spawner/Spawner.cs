using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject spawnee;
    public int maxSpawns = 1;
    int _currentSpawns = 0;

    public void SpawnUnit(Tile tile, bool attackThisRound = false)
    {
        if (SpawnsRemaining() && !tile.occupied)
        {
            GameObject spawnedObject = Instantiate(spawnee, tile.ElevatedPos() + Vector3.up * (spawnee.transform.position.y + (tile.slope ? MapGrid.Instance.tileHeight/2f : 0)), Quaternion.Euler(0,90*Random.Range(0,4),0));
            _currentSpawns++;
            Humanoid unit = spawnedObject.GetComponent<Humanoid>();
            if (unit)
            {
                CombatSystem.Instance.NewSpawn(unit);
                if (attackThisRound && unit is Enemy)
                {
                    Enemy tempEnemy = (Enemy)unit;
                    CombatSystem.Instance.SubscribeEnemy(tempEnemy);
                }
                //ask chase about subscribe timer
            }
            else
            {
                tile.movementTile = false;
            }

        }
    }

    public bool SpawnsRemaining()
    {
        return _currentSpawns < maxSpawns;
    }
}
