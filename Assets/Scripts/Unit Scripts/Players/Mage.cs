using UnityEngine;

public class Mage : Player
{
    public override void AbilityOne(Humanoid target)
    {
        Debug.Log("Mage Ability One");
    }

    public override void AbilityTwo(Humanoid target)
    {
        Debug.Log("Mage Ability Two");
    }

    public override void NormalAttack(Humanoid target)
    {
        Debug.Log("Mage Normal Attack");
    }
}
