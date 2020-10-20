using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    public GameObject gear1, gear2;
    public float gearSpeed = 2f;
    public int experienceToAdd = 100;


    private void Update()
    {
        gear1.transform.RotateAround(gear1.transform.position, gear1.transform.forward, gearSpeed * Time.deltaTime);
        gear2.transform.RotateAround(gear2.transform.position, gear2.transform.forward, gearSpeed * Time.deltaTime);
    }

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
