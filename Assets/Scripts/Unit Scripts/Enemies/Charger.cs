using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charger : Enemy
{
    public override void Attack()
    {
        Debug.Log("Charger Attack");
        base.Attack();
    }

    public override void Defend()
    {
        Debug.Log("Charger Defend");
        base.Defend();
    }

    //public override void Dodge()
    //{
    //    Debug.Log("Dodge");
    //}
}
