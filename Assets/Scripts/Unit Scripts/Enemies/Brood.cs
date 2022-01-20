using UnityEngine;

public class Brood : Hive
{
    public override void Attack()
    {
        Debug.Log("Brood Attack");
        base.Attack();
    }

    public override void Defend()
    {
        Debug.Log("Brood Defend");
        base.Defend();
    }

    //public override void Dodge()
    //{
    //    Debug.Log("Brood Dodge");
    //}
}
