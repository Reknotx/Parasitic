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
    /// <summary> Enum representing which attack was executed. </summary>
    enum Attack
    {
        NormalAttack,
        AbilityOne,
        AbilityTwo
    }

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


    //public GameObject turnSwitch;

    /// <summary> The text stating which side is active. </summary>
    public Text activeSideText;

    /// <summary> The list of player's that have yet to go this round. </summary>
    private List<Player> playersToGo = new List<Player>();

    /// <summary> The list of enemies that have yet to go this round. </summary>
    private List<Enemy> enemiesToGo = new List<Enemy>();

    /// <summary> The master list of all units that are currently alive. </summary>
    private List<Humanoid> unitsAlive = new List<Humanoid>();

    /// <summary> The list of buttons used for combat when a player is selected. </summary>
    public List<Button> combatButtons = new List<Button>();

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

            if (enemy.Revealed == true) enemiesToGo.Add(enemy);
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
                tile.occupant.TakeDamage(enemiesToGo[index].AttackStat);
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
        if (!playersToGo.Contains(selection) && selection != null)
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
        //StartCoroutine(NormalAttackCR());
        ProcessAttack(Attack.NormalAttack);
    }

    /// <summary>
    /// The first ability of the player.
    /// </summary>
    public void AbilityOne()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit == null) return;

        StopAllCoroutines();
        SetBattleState(BattleState.Targetting);
        //StartCoroutine(AbilityOneCR());
        ProcessAttack(Attack.AbilityOne);
    }

    /// <summary>
    /// The second ability of the player.
    /// </summary>
    public void AbilityTwo()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit == null) return;

        StopAllCoroutines();
        SetBattleState(BattleState.Targetting);
        //StartCoroutine(AbilityTwoCR());
        ProcessAttack(Attack.AbilityTwo);
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
        //Sets both HasAttacked and HasMoved to true just to make
        //sure that nothing is missed.
        unit.HasAttacked = true;
        unit.HasMoved = true;

        if (unit is Player)
        {
            playersToGo.Remove((Player)unit);
            player.GetComponent<MeshRenderer>().material.color = Color.gray;

            if (playersToGo.Count == 0)
            {
                StartCoroutine(EnemyTurn());
                activeSideText.text = "Enemy Turn";
            }
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
            else if (unit is Enemy && ((Enemy)unit).Revealed != false)
            {
                enemiesToGo.Add((Enemy)unit);
            }

            unit.HasMoved = false;
            unit.HasAttacked = false;
        }

        UpdateList();

        SetActiveUnits(ActiveUnits.Players);

        activeSideText.text = "Player's turn";
    }

    /// <summary> Executes the attack type that we have passed in. </summary>
    /// <param name="type">The attack of the selected player to activate. </param>
    void ProcessAttack(Attack type)
    {
        SetBattleState(BattleState.PerformingAction);

        switch (type)
        {
            case Attack.NormalAttack:
                ((IPlayer)CharacterSelector.Instance.SelectedPlayerUnit).NormalAttack(AttackComplete);
                break;

            case Attack.AbilityOne:
                ((IPlayer)CharacterSelector.Instance.SelectedPlayerUnit).AbilityOne(AttackComplete);
                break;

            case Attack.AbilityTwo:
                ((IPlayer)CharacterSelector.Instance.SelectedPlayerUnit).AbilityTwo(AttackComplete);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Called when the attack or ability is completed.
    /// Set's the battle state to idle.
    /// </summary>
    public void AttackComplete()
    {
        EndUnitTurn(CharacterSelector.Instance.SelectedPlayerUnit);

        CharacterSelector.Instance.SelectedPlayerUnit = null;
        CharacterSelector.Instance.SelectedTargetUnit = null;

        SetBattleState(BattleState.Idle);

    }

    /// <summary>
    /// Switches the turn after a few seconds. Is this in though(?)
    /// </summary>
    /// <returns>Whatever a coroutine returns</returns>
    IEnumerator TurnSwitchCR()
    {
        //turnSwitch.SetActive(true);
        yield return new WaitForSeconds(0f);
        //turnSwitch.SetActive(false);

        NewRound();
    }

    /// <summary>
    /// Executes the logic for the enemies turn.
    /// </summary>
    /// <returns></returns>
    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(2f);

        while (enemiesToGo.Count != 0)
        {
            int index = Random.Range(0, enemiesToGo.Count);

            if (enemiesToGo[index].Revealed == false)
            {
                enemiesToGo.Remove(enemiesToGo[index]);
                continue;
            }

            Enemy tempE = enemiesToGo[index];

            tempE.Move(tempE.FindNearestPlayer());

            yield return new WaitUntil(() => tempE.HasMoved == true);

            if (tempE.CheckIfInRangeOfTarget())
            {
                tempE.Attack();
            }


            EndUnitTurn(enemiesToGo[index]);
        }
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
    /// Kills the unit in game and removes it from system. Also checks the win condition
    /// and ends the game if it is met.
    /// </summary>
    /// <param name="unit">The unit who's health is at or below 0.</param>
    public void KillUnit(Humanoid unit)
    {
        if (unit is Player)
        {
            playersToGo.Remove((Player)unit);
        }
        else
        {
            enemiesToGo.Remove((Enemy)unit);
        }

        unitsAlive.Remove(unit);

        Destroy(unit.gameObject);

        if (CheckWinCondition()) GameWon();
    }


    /// <summary> Checks the win condition to see if it's met. </summary>
    private bool CheckWinCondition()
    {
        foreach (Humanoid unit in unitsAlive)
        {
            if (unit is Enemy) return false;
        }

        return true;
    }

    /// <summary> Activate the win screen canvas here when the win condition is met. </summary>
    private void GameWon()
    {
        SetBattleState(BattleState.Won);

        //winCanvas.SetActive(true);
    }


    /// <summary>
    /// Activates the combat buttons.
    /// </summary>
    public void ActivateCombatButtons()
    {
        foreach (Button button in combatButtons) { button.interactable = true; }
    }

    /// <summary>
    /// Deactivates the combat buttons.
    /// </summary>
    public void DeactivateCombatButtons()
    {
        foreach (Button button in combatButtons) { button.interactable = false; }
    }

    /// <summary> Adds a revealed enemy to the turn system. </summary>
    /// <param name="enemy">The enemy to add.</param>
    public void SubscribeEnemy(Enemy enemy)
    {
        enemiesToGo.Add(enemy);
    }



    public List<Humanoid> alteredUnits = new List<Humanoid>();

    /// <summary>
    /// Subscribes a unit that has been buffed or debuffed to the system.
    /// After every round these units will have their counters updated.
    /// </summary>
    /// <param name="subject">The unit that is altered.</param>
    public void SubscribeAlteredUnit(Humanoid subject)
    {
        alteredUnits.Add(subject);
    }

    /// <summary>
    /// Unsubscribes the altered unit when their (de)buff timer has run out.
    /// </summary>
    /// <param name="subject">The unit that was previously altered.</param>
    public void UnsubscribeAlteredUnit(Humanoid subject)
    {
        removeList.Add(subject);
    }

    private List<Humanoid> removeList = new List<Humanoid>();


    /// <summary>
    /// Updates the list of altered units.
    /// </summary>
    private void UpdateList()
    {
        removeList = new List<Humanoid>();

        foreach (Humanoid unit in alteredUnits)
        {
            unit.AdvanceTimer();
        }


        foreach (Humanoid unit in removeList)
        {
            unit.ResetStats();
            alteredUnits.Remove(unit);
        }

        removeList.Clear();
    }
}