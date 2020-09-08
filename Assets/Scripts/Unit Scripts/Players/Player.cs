using UnityEngine;

public abstract class Player : Humanoid, IPlayer
{
    public abstract void AbilityOne();
    public abstract void AbilityTwo();
    public abstract void NormalAttack();

    public void Hello()
    {
        Debug.Log("Hello");
    }

    public override void Move()
    {
        // Player movement based on player input
        Debug.Log("Player movement");
    }
}
