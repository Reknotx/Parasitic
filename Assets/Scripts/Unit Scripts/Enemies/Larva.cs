using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Larva : Enemy
{
    public override void Attack()
    {
        Debug.Log("Larva Attack");
        base.Attack();
    }

    public override void Defend()
    {
        Debug.Log("Larva Defend");
        base.Defend();
    }

    public override void Dodge()
    {
        Debug.Log("Larva Dodge");
        base.Dodge();
    }
}
