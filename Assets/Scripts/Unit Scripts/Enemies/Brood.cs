using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brood : Enemy
{
    public override void Attack()
    {
        Debug.Log("Brood Attack");
    }

    public override void Defend()
    {
        Debug.Log("Brood Defend");
    }

    public override void Dodge()
    {
        Debug.Log("Brood Dodge");
    }
}
