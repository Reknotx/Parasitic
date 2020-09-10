using System.Collections;
using UnityEngine;

public enum BattleState
{
    Start,
    Player,
    Enemy,
    Targetting,
    Won,
    Lost
}

public class CombatSystem : MonoBehaviour
{
    public BattleState state;

    public static CombatSystem Instance;

    private Player player;
    private Humanoid target;

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

    void Move()
    {

    }

    void SetupBattle()
    {
        SetState(BattleState.Player);
    }

    public void SetState(BattleState state) { this.state = state; }

    public void SetPlayer(Player selection)
    {
        player = selection;
        Debug.Log(selection.GetComponent<Renderer>().material.color);
        selection.gameObject.GetComponent<Renderer>().material.color = Color.red;
    }

    public void SetTarget(Humanoid selection)
    {
        if (selection == player)
        {
            Debug.Log("Can't select yourself for attack");
            return;
        }
        target = selection;
    }

    /// <summary>
    /// Executes the coroutine for normal attack of player.
    /// </summary>
    public void NormalAttack()
    {
        //((IPlayer)selectedPlayer).NormalAttack(target);
        if (player == null) return;
        StopAllCoroutines();
        SetState(BattleState.Targetting);
        StartCoroutine(NormalAttackCR());
    }

    public void AbilityOne()
    {
        if (player == null) return;

        StopAllCoroutines();

        ((IPlayer)player).AbilityOne(target);
    }

    public void AbilityTwo()
    {
        ((IPlayer)player).AbilityTwo(target);
    }

    public void Cancel()
    {
        player = null;
        target = null;
        SetState(BattleState.Player);
    }

    IEnumerator NormalAttackCR()
    {
        Debug.Log("Select target for normal attack.");
        yield return new WaitUntil(() => target != null);
        Debug.Log("Attacking " + target.gameObject.name);

        if (target == player) yield break;

        if(target.TakeDamage(((IStatistics)player).BaseAttack)) { Destroy(target.gameObject); }

        player.GetComponent<MeshRenderer>().material.color = Color.white;
        player = null;
        target = null;
        SetState(BattleState.Player);
    }
}
