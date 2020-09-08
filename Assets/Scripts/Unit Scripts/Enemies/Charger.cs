using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charger : Enemy
{
    public override void Attack()
    {
        Debug.Log("Charger Attack");
    }

    public override void Defend()
    {
        Debug.Log("Charger Defend");
    }

    public override void Dodge()
    {
        Debug.Log("Dodge");
    }
}
