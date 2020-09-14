/*
 * Author: Chase O'Connor
 * Date: 9/20/2020
 * 
 * Brief: The main script that handles combat logic.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Enum for the various states of combat we can be in.
/// </summary>
public enum BattleState
{
    Start,
    Idle,
    PerformingAction,
    Targetting,
    Won,
    Lost
}

/// <summary>
/// Enum for the units in system that are currently active.
/// </summary>
public enum ActiveUnits
{
    Players,
    Enemies
}

/// <summary>
/// Main entry point of the program (for the moment) where all data is handled.
/// </summary>
public class CombatSystem : MonoBehaviour
{
    /// <summary>
    /// The current state of the battle system.
    /// </summary>
    public BattleState state;

    /// <summary>
    /// The currently active units. Either player's or enemies.
    /// </summary>
    public ActiveUnits activeUnits;

    public static CombatSystem Instance;

    /// <summary> The selected player for combat. </summary>
    private Player player;

    /// <summary> The target of combat. </summary>
    private Humanoid target;


    public GameObject turnSwitch;

    /// <summary> The list of player's that have yet to go this round. </summary>
    private List<Player> playersToGo = new List<Player>();

    /// <summary> The list of enemies that have yet to go this round. </summary>
    private List<Enemy> enemiesToGo = new List<Enemy>();

    /// <summary> The master list of all units that are currently alive. </summary>
    private List<Humanoid> unitsAlive = new List<Humanoid>();

    /// <summary> The list of buttons used for combat when a player is selected. </summary>
    public List<Button> combatButtons = new List<Button>();

    public Text turnIndicator;

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

        DeactivateCombatButtons();

        SetBattleState(BattleState.Start);
        SetActiveUnits(ActiveUnits.Players);
    }

    /// <summary>
    /// Moves a random enemy on the grid. For testing purposes only.
    /// </summary>
    public void MoveRandEnemy()
    {
        int index = Random.Range(0, enemiesToGo.Count);

        List<Tile> path = enemiesToGo[index].FindNearestPlayer();

        enemiesToGo[index].Move(path);
    }

    /// <summary>
    /// Triggers a random enemy attack on grid if they are near a player. For testing purposes only
    /// </summary>
    public void TriggerEnemyAttack()
    {
        int index = Random.Range(0, enemiesToGo.Count);

        List<Tile> temp =  MapGrid.Instance.GetNeighbors(enemiesToGo[index].currentTile);

        foreach (Tile tile in temp)
        {
            if (tile.occupied && tile.occupant is Player)
            {
                tile.occupant.TakeDamage(enemiesToGo[index].BaseAttack);
                break;
            }
        }

    }

    /// <summary>
    /// Sets the state of the game.
    /// </summary>
    /// <param name="state">The new state of the game.</param>
    public void SetBattleState(BattleState state) { this.state = state; }

    /// <summary> 
    /// Sets the current active units of the game. 
    /// </summary>
    /// <param name="activeUnits">The new active units.</param>
    public void SetActiveUnits(ActiveUnits activeUnits) { this.activeUnits = activeUnits; }

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

        player = selection;
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
        //if (player == null) return;
        //StopAllCoroutines();
        //SetState(BattleState.Targetting);
        //StartCoroutine(NormalAttackCR());

        if (CharacterSelector.Instance.SelectedPlayerUnit == null) return;
        StopAllCoroutines();
        SetBattleState(BattleState.Targetting);
        StartCoroutine(NormalAttackCR());
    }

    /// <summary>
    /// The first ability of the player.
    /// </summary>
    public void AbilityOne()
    {
        if (player == null) return;

        StopAllCoroutines();
        SetBattleState(BattleState.Targetting);
        StartCoroutine(AbilityOneCR());
    }

    /// <summary>
    /// The second ability of the player.
    /// </summary>
    public void AbilityTwo()
    {
        if (player == null) return;

        StopAllCoroutines();
        SetBattleState(BattleState.Targetting);
        StartCoroutine(AbilityTwoCR());
    }

    /// <summary>
    /// Currently a hard pass which cancels all of the player's actions.
    /// </summary>
    public void Pass()
    {
        if (player == null) return;

        StopAllCoroutines();
        SetBattleState(BattleState.Idle);
        player.Pass();
        EndUnitTurn(player);
    }

    /// <summary>
    /// Cancles the current action we have selected.
    /// </summary>
    public void Cancel()
    {
        player = null;
        target = null;
        SetBattleState(BattleState.Start);
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
            player.GetComponent<MeshRenderer>().material.color = Color.gray;

            if (playersToGo.Count == 0) { StartCoroutine(EnemyTurn()); }

        }
        else
        {
            enemiesToGo.Remove((Enemy)unit);

            if (enemiesToGo.Count == 0) { StartCoroutine(TurnSwitchCR()); }
        }

        player = null;
        target = null;
        
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

            unit.HasMoved = false;
            unit.HasAttacked = false;
        }

        SetActiveUnits(ActiveUnits.Players);
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

        //if(target.TakeDamage(((IStatistics)player).BaseAttack)) { Destroy(target.gameObject); }

        ((IPlayer)player).NormalAttack(target);

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
        //turnSwitch.SetActive(true);
        yield return new WaitForSeconds(0f);
        //turnSwitch.SetActive(false);

        NewRound();
    }

    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(2f);

        while (enemiesToGo.Count != 0)
        {
            int index = Random.Range(0, enemiesToGo.Count);

            Enemy tempE = enemiesToGo[index];

            tempE.Move(tempE.FindNearestPlayer());

            yield return new WaitUntil(() => tempE.HasMoved == true);

            if (tempE.CheckIfInRangeOfTarget())
            {
                tempE.Attack();
            }


            EndUnitTurn(enemiesToGo[index]);
        }

        //foreach (Enemy enemy in enemiesToGo)
        //{
        //    enemy.Move(enemy.FindNearestPlayer());

        //    yield return new WaitUntil(() => enemy.HasMoved == true);

        //    EndUnitTurn(enemy);
        //}


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

    public void ActivateCombatButtons()
    {
        foreach (Button button in combatButtons) { button.interactable = true; }
    }

    public void DeactivateCombatButtons()
    {
        foreach (Button button in combatButtons) { button.interactable = false; }
    }
}