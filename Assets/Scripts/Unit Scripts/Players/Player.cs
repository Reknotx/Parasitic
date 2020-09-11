using System.Collections.Generic;
using UnityEngine;

public abstract class Player : Humanoid, IPlayer
{
    public int Ability1Range;
    public int Ability2Range;

    public abstract void AbilityOne(List<Humanoid> targets);
    public abstract void AbilityTwo(List<Humanoid> targets);
    public abstract void NormalAttack(Humanoid target);

    public override void Move(Transform start, Transform target)
    {
        // Player movement based on player input
        Debug.Log("Player movement");
    }

    /// <summary>
    /// Raises the defense stat of the player temporarily.
    /// </summary>
    public void Defend()
    {

    }

    public void Pass()
    {
        throw new System.NotImplementedException();
    }

    //public void OnMouseOver()
    //{
    //    gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
    //}

    //public void OnMouseExit()
    //{
    //    gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
    //}
}
