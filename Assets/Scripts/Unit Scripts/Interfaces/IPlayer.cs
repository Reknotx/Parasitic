using System;

public interface IPlayer
{
    /// <summary>
    /// Triggers normal attack of unit.
    /// </summary>
    void NormalAttack(Action callback);

    /// <summary>
    /// Triggers the first ability of the unit.
    /// </summary>
    void AbilityOne(Action callback);

    /// <summary>
    /// Triggers the second ability of the unit.
    /// </summary>
    void AbilityTwo(Action callback);

    /// <summary>
    /// Triggers defense animation.
    /// </summary>
    void Defend();

    /// <summary>
    /// Passes the player's turn.
    /// </summary>
    //void Pass();
}
