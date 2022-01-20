using UnityEngine;

public class Stalker : Enemy
{
    public override void Attack()
    {
        Debug.Log("Stalker Attack");
        base.Attack();
    }

    public override void Defend()
    {
        Debug.Log("Stalker Defend");
        base.Defend();
    }

    //public override void Dodge()
    //{
    //    Debug.Log("Stalker Dodge");
    //}
}
