using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spiker : Enemy
{
    public override void Attack()
    {
        Debug.Log("Spiker Attack");
    }

    public override void Defend()
    {
        Debug.Log("Spiker Defend");
    }

    public override void Dodge()
    {
        Debug.Log("Spiker Dodge");
    }
}
