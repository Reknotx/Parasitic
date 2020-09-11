using System.Collections.Generic;
using UnityEngine;

public class Archer : Player
{
    /// <summary>
    /// Triggers the archer's first ability.
    /// </summary>
    /// <param name="target">The target(s) of the archer's ability.</param>
    public override void AbilityOne(List<Humanoid> targets)
    {
        Debug.Log("Archer Ability One");
    }

    /// <summary>
    /// Triggers the archer's second ability.
    /// </summary>
    /// <param name="targets">The target(s) of the archer's ability.</param>
    public override void AbilityTwo(List<Humanoid> targets)
    {
        Debug.Log("Archer Ability Two");
    }

    /// <summary>
    /// Triggers the normal attack of the archer.
    /// </summary>
    /// <param name="target">The target of the archer's attack.</param>
    public override void NormalAttack(Humanoid target)
    {
        Debug.Log("Archer Normal Attack");
    }
}
