using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Enemy : Humanoid, IEnemy
{
    public abstract void Attack();
    public abstract void Defend();
    public abstract void Dodge();

    public override void Move()
    {
        //Enemy movement is dependent on the Astar algorithm
        Debug.Log("Enemy movement");
    }

}
