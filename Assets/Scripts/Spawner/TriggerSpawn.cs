using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSpawn : MonoBehaviour
{
    Spawner spawner;
    Tile tile;
    private void Start()
    {
        spawner = GetComponent<Spawner>();
        tile = MapGrid.Instance.TileFromPosition(transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        print("trigger entered");
        if (other.CompareTag("Player"))
        {
            spawner.SpawnUnit(tile);
        }
    }
}
