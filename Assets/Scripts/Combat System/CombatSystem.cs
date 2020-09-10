using System.Collections;
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

    public static CombatSystem Instance;

    public Player selectedPlayer;
    public Humanoid target;

    void Start()
    {
        state = BattleState.Start;
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        SetupBattle();
    }

    void SetupBattle()
    {

    }

    public void NormalAttack()
    {
        //((IPlayer)selectedPlayer).NormalAttack(target);

        StopAllCoroutines();
        StartCoroutine(NormalAttackCR());
    }

    public void AbilityOne()
    {
        ((IPlayer)selectedPlayer).AbilityOne(target);
    }

    public void AbilityTwo()
    {
        ((IPlayer)selectedPlayer).AbilityTwo(target);
    }

    IEnumerator NormalAttackCR()
    {
        yield return new WaitUntil(() => target != null);
    }
}
