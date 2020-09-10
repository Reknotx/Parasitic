using UnityEngine;

public interface IPlayer
{
    void NormalAttack(Humanoid target);
    void AbilityOne(Humanoid target);
    void AbilityTwo(Humanoid target);
    void Defend();
    void Pass();
}
