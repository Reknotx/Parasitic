using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Item
{
    public List<Tile> tilesToOpen;
    public List<GameObject> objectsToHide;
    public List<GameObject> objectsToShow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach(Tile tile in tilesToOpen)
            {
                tile.movementTile = true;
                tile.blocksLOS = false;
            }
            foreach(GameObject item in objectsToHide)
            {
                item.SetActive(false);
            }
            foreach (GameObject item in objectsToShow)
            {
                item.SetActive(true);
            }
            Inventory.Instance.items.Add(this);
            gameObject.SetActive(false);
        }
    }
}
