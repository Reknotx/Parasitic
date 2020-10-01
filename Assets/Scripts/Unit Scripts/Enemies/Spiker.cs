using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spiker : Enemy
{
    public override void Attack()
    {
        Debug.Log("Spiker Attack");
        base.Attack();
    }

    public override void Defend()
    {
        Debug.Log("Spiker Defend");
        base.Defend();
    }

    //public override void Dodge()
    //{
    //    Debug.Log("Spiker Dodge");
    //    base.Dodge();
    //}
}
