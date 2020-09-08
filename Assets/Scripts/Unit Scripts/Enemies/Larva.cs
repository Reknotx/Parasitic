using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Larva : Enemy
{
    public override void Attack()
    {
        Debug.Log("Larva Attack");
    }

    public override void Defend()
    {
        Debug.Log("Larva Defend");
    }

    public override void Dodge()
    {
        Debug.Log("Larva Dodge");
    }
}
