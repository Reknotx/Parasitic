using System.Collections.Generic;
using UnityEngine;

public class Warrior : Player
{
    /// <summary>
    /// Warrior's first ability. Lowers attack of enemies in radius.
    /// </summary>
    /// <param name="target">The target(s) of the ability.</param>
    /// Currently the best way to do this would be to call this method multiple times.
    /// Passing in each individual target. But how exactly can i do this and keep
    /// it generic.
    /// 
    /// Another option is to just require a passing in of a list of enemies that are 
    /// in the area, even if the ability is only a single target. This will keep
    /// the code generic, flexible, and easy to micromanage. It's not the best
    /// solution, but it's one that will work the best.
    public override void AbilityOne(List<Humanoid> targets)
    {
        Debug.Log("Warrior Ability One");

        /*
         * Scare the surrounding enemies of the warrior and causing those
         * enemies to deal less damage. Effectively lowering their attack stat.
         * 
         * 
         */
    }

    /// <summary>
    /// Warrior's second ability. Taunts nearby enemies.
    /// </summary>
    /// <param name="targets">The target(s) of the ability.</param>
    public override void AbilityTwo(List<Humanoid> targets)
    {
        Debug.Log("Warrior Ability Two");
    }

    /// <summary>
    /// The warrior's attack.
    /// </summary>
    /// <param name="target">The target of the attack.</param>
    public override void NormalAttack(Humanoid target)
    {
        Debug.Log("Warrior Normal Attack");

        ///Execute the animation
        target.TakeDamage(BaseAttack);
    }
}
