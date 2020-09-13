using System.Collections.Generic;
using UnityEngine;

public class Mage : Player
{
    /// <summary>
    /// Mage's first ability. AOE fire blast.
    /// </summary>
    /// <param name="targets">The target(s) of the ability.</param>
    public override void AbilityOne(List<Humanoid> targets)
    {
        Debug.Log("Mage Ability One");
        AttackComplete();
    }

    /// <summary>
    /// Mage's second ability. Damage boost.
    /// </summary>
    /// <param name="targets">A reference to the mage itself. Can be empty.</param>
    public override void AbilityTwo(List<Humanoid> targets)
    {
        Debug.Log("Mage Ability Two");
        AttackComplete();
    }

    /// <summary>
    /// Mage's normal attack.
    /// </summary>
    /// <param name="target">The target of the attack.</param>
    public override void NormalAttack(Humanoid target)
    {
        Debug.Log("Mage Normal Attack");
        AttackComplete();
    }
}
