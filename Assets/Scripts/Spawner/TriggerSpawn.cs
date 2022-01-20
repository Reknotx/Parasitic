using UnityEngine;

public class TriggerSpawn : MonoBehaviour
{
    Spawner spawner;
    Tile tile;
    public bool attackRoundSpawned = true;
    public Transform spawnPoint;
    private void Start()
    {
       
        spawner = transform.GetComponentInParent<Spawner>();
        tile = MapGrid.Instance.TileFromPosition(spawnPoint.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        print("trigger entered");
        if (other.CompareTag("Player"))
        {
            spawner.SpawnUnit(tile,true);
        }
    }
}
