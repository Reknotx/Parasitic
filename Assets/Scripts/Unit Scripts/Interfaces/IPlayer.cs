using System.Collections.Generic;

public interface IPlayer
{
    /// <summary>
    /// Triggers normal attack of unit.
    /// </summary>
    /// <param name="target">The target of attack.</param>
    void NormalAttack(Humanoid targets);

    /// <summary>
    /// Triggers the first ability of the unit.
    /// </summary>
    /// <param name="target">The target(s) of unit's ability.</param>
    void AbilityOne(List<Humanoid> targets);

    /// <summary>
    /// Triggers the second ability of the unit.
    /// </summary>
    /// <param name="target">The target(s) of unit's ability.</param>
    void AbilityTwo(List<Humanoid> target);

    /// <summary>
    /// Triggers defense animation.
    /// </summary>
    void Defend();

    /// <summary>
    /// Passes the player's turn.
    /// </summary>
    void Pass();
}
