using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    Start,
    Player,
    Enemy,
    Won,
    Lost
}

public class CombatSystem : MonoBehaviour
{
    public BattleState state;

    void Start()
    {
        state = BattleState.Start;
        SetupBattle();
    }

    void SetupBattle()
    {

    }
}
