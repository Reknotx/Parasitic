using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stalker : Enemy
{
    public override void Attack()
    {
        Debug.Log("Stalker Attack");
    }

    public override void Defend()
    {
        Debug.Log("Stalker Defend");
    }

    public override void Dodge()
    {
        Debug.Log("Stalker Dodge");
    }
}
