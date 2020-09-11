using UnityEngine;

public class Warrior : Player
{
    public override void AbilityOne(Humanoid target)
    {
        Debug.Log("Warrior Ability One");
    }

    public override void AbilityTwo(Humanoid target)
    {
        Debug.Log("Warrior Ability Two");
    }

    public override void NormalAttack(Humanoid target)
    {
        Debug.Log("Warrior Normal Attack");
    }
}
