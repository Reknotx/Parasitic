using UnityEngine;

public interface IPlayer
{
    /// <summary>
    /// Triggers normal attack of unit.
    /// </summary>
    /// <param name="target">The target of attack.</param>
    void NormalAttack(Humanoid target);

    /// <summary>
    /// Triggers the first ability of the unit.
    /// </summary>
    /// <param name="target">The target of unit's ability.</param>
    void AbilityOne(Humanoid target);

    /// <summary>
    /// Triggers the second ability of the unit.
    /// </summary>
    /// <param name="target">The target of unit's ability.</param>
    void AbilityTwo(Humanoid target);

    /// <summary>
    /// Triggers defense animation.
    /// </summary>
    void Defend();

    /// <summary>
    /// Passes the player's turn.
    /// </summary>
    void Pass();
}
