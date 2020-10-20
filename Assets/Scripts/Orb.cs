using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    public int experienceToAdd = 100;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            Upgrades.Instance.MageXp += experienceToAdd;
            Upgrades.Instance.KnightXp += experienceToAdd;
            Upgrades.Instance.ArcherXp += experienceToAdd;
            
            Destroy(gameObject);
        }
    }
}
