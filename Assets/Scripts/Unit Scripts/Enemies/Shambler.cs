using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shambler : Enemy
{
    public override void Attack()
    {
        Debug.Log("Shambler Attack");
    }

    public override void Defend()
    {
        Debug.Log("Shambler Defend");
    }

    public override void Dodge()
    {
        Debug.Log("Shambler Dodge");
    }
}
