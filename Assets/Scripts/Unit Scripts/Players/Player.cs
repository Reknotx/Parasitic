using UnityEngine;

public abstract class Player : Humanoid, IPlayer
{
    public abstract void AbilityOne(Humanoid target);
    public abstract void AbilityTwo(Humanoid target);
    public abstract void NormalAttack(Humanoid target);

    public void Hello()
    {
        Debug.Log("Hello");
    }

    public override void Move(Transform start, Transform target)
    {
        // Player movement based on player input
        Debug.Log("Player movement");
    }

    public void Defend()
    {

    }

    public void Pass()
    {
        throw new System.NotImplementedException();
    }
}
