using UnityEngine;

public class Shambler : Enemy
{
    public override void Attack()
    {
        Debug.Log("Shambler Attack");
        base.Attack();
    }

    public override void Defend()
    {
        Debug.Log("Shambler Defend");
        base.Defend();
    }

    //public override void Dodge()
    //{
    //    Debug.Log("Shambler Dodge");
    //    base.Dodge();
    //}
}
