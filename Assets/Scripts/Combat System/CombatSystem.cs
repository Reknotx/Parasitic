using System.Collections;
using System.Collections.Generic;
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

    /// <summary> The selected player for combat. </summary>
    private Player player;

    /// <summary> The target of combat. </summary>
    private Humanoid target;

    public GameObject turnSwitch;
    private List<Player> playersToGo = new List<Player>();
    private List<Enemy> enemiesToGo = new List<Enemy>();
    private List<Humanoid> unitsAlive = new List<Humanoid>();

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
        Player[] tempP = FindObjectsOfType<Player>();
        Enemy[] tempE = FindObjectsOfType<Enemy>();

        foreach (Player player in tempP)
        {
            playersToGo.Add(player);
            unitsAlive.Add(player);
        }

        foreach (Enemy enemy in tempE)
        {
            enemiesToGo.Add(enemy);
            unitsAlive.Add(enemy);
        }

        SetState(BattleState.Player);
    }

    public void MoveRandEnemy()
    {
        int index = Random.Range(0, enemiesToGo.Count);

        enemiesToGo[index].TestForMovement();
    }

    /// <summary>
    /// Sets the state of the game.
    /// </summary>
    /// <param name="state">The new state of the game.</param>
    public void SetState(BattleState state) { this.state = state; }

    /// <summary>
    /// Sets the player we have currently selected.
    /// </summary>
    /// <param name="selection">The player we have selected</param>
    public void SetPlayer(Player selection)
    {
        if (!playersToGo.Contains(selection))
        {
            Debug.Log("Player has already gone this round");
            return;
        }

        if (player != null)
        {
            player.GetComponent<MeshRenderer>().material.color = Color.white;
        }


        player = selection;
        selection.gameObject.GetComponent<Renderer>().material.color = Color.green;
    }

    /// <summary>
    /// Sets the target of our attack or ability.
    /// </summary>
    /// <param name="selection">The target of our attack or ability.</param>
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

    /// <summary>
    /// The first ability of the player.
    /// </summary>
    public void AbilityOne()
    {
        if (player == null) return;

        StopAllCoroutines();
        SetState(BattleState.Targetting);
        StartCoroutine(AbilityOneCR());
    }

    /// <summary>
    /// The second ability of the player.
    /// </summary>
    public void AbilityTwo()
    {
        if (player == null) return;

        StopAllCoroutines();
        SetState(BattleState.Targetting);
        StartCoroutine(AbilityTwoCR());
    }

    /// <summary>
    /// 
    /// </summary>
    private void ClearSystem()
    {
       
    }

    /// <summary>
    /// Cancles the current action we have selected.
    /// </summary>
    public void Cancel()
    {
        player = null;
        target = null;
        SetState(BattleState.Player);
    }

    /// <summary>
    /// Checks if there are any units left to go this round.
    /// </summary>
    /// <returns>Returns true if everyone has gone, false otherwise.</returns>
    private bool CheckUnitsLeft()
    {
        if (playersToGo.Count == 0 && enemiesToGo.Count == 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Ends the turn for the current unit. Removing them from the list.
    /// </summary>
    /// <param name="unit">The unit whose turn is over.</param>
    private void EndUnitTurn(Humanoid unit)
    {

        if (unit is Player)
        {
            playersToGo.Remove((Player)unit);

            if (playersToGo.Count == 0) { SetState(BattleState.Enemy); }
        }
        else
        {
            enemiesToGo.Remove((Enemy)unit);

            if (enemiesToGo.Count == 0) { StartCoroutine(TurnSwitchCR()); }
        }

        
        unit.State = HumanoidState.Done;
    }

    /// <summary>
    /// Adds the players and enemies to the queue for combat.
    /// </summary>
    private void NewRound()
    {
        foreach (Humanoid unit in unitsAlive)
        {
            if (unit is Player)
            {
                playersToGo.Add((Player)unit);
                unit.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
            }
            else if (unit is Enemy) enemiesToGo.Add((Enemy)unit);
        }
    }

    /// <summary>
    /// Normal attack targetting coroutine.
    /// </summary>
    /// <returns>Waits until a target has been selected.</returns>
    IEnumerator NormalAttackCR()
    {
        Debug.Log("Select target for normal attack.");
        yield return new WaitUntil(() => target != null);
        Debug.Log("Attacking " + target.gameObject.name);

        if (target == player) yield break;

        if(target.TakeDamage(((IStatistics)player).BaseAttack)) { Destroy(target.gameObject); }

        player.GetComponent<MeshRenderer>().material.color = Color.gray;
        player = null;
        target = null;

        EndUnitTurn(player);
    }

    IEnumerator AbilityOneCR()
    {
        yield return new WaitUntil(() => target != null);

    }

    IEnumerator AbilityTwoCR()
    {
        yield return new WaitUntil(() => target != null);
    }

    IEnumerator TurnSwitchCR()
    {
        turnSwitch.SetActive(true);
        yield return new WaitForSeconds(4f);
        turnSwitch.SetActive(false);

        NewRound();
    }
}