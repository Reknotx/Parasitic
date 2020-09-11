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

/// <summary>
/// Main entry point of the program (for the moment) where all data is handled.
/// </summary>
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

    /// <summary>
    /// Sets up the map and necessary information.
    /// </summary>
    void SetupBattle()
    {
        SetState(BattleState.Player);
    }

    public void SetState(BattleState state) { this.state = state; }

    public void SetPlayer(Player selection)
    {
        if (player != null)
        {
            player.GetComponent<MeshRenderer>().material.color = Color.white;
        }

        player = selection;
        selection.gameObject.GetComponent<Renderer>().material.color = Color.green;
    }

    public void SetTarget(Humanoid selection)
    {
        if (selection == player)
        {
            Debug.Log("Can't select yourself for attack.");
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
        SetState(BattleState.Targetting);
        StartCoroutine(AbilityOneCR());
    }

    public void AbilityTwo()
    {
        if (player == null) return;

        StopAllCoroutines();
        SetState(BattleState.Targetting);
        StartCoroutine(AbilityTwoCR());
    }

    private void ClearSystem()
    {
       
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

    IEnumerator AbilityOneCR()
    {
        yield return new WaitUntil(() => target != null);

    }

    IEnumerator AbilityTwoCR()
    {
        yield return new WaitUntil(() => target != null);
    }
}
