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
using UnityEngine.Events;

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

    [Header("The canvas that is displayed when you have met the win/lose condition.")]
    /// <summary> The canvas that is displayed when the game has been won.</summary>
    public GameObject endCanvas;

    [Space]

    [Header("The text that tells you whether you win or lose.")]
    public Text endGameText;

    public Text enemiesAliveText;

    /// <summary>  </summary>
    public Text roundCounterText;

    private int _roundCounter = 1;
    
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

    public List<Tile> coolingTiles = new List<Tile>();

    void Start()
    {
        state = BattleState.Start;
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        SetupBattle();
        SetEnemyCountText();
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

        if (endCanvas.activeSelf) endCanvas.SetActive(false);
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

    /// <summary> Executes the coroutine for normal attack of player. </summary>
    public void NormalAttack()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit == null) return;
        ActionRange.Instance.ActionSelected();
        StopAllCoroutines();
        SetBattleState(BattleState.Targetting);
        //StartCoroutine(NormalAttackCR());
        ProcessAttack(Attack.NormalAttack);
    }

    /// <summary> The first ability of the player. </summary>
    public void AbilityOne()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit == null) return;
        ActionRange.Instance.ActionSelected();
        StopAllCoroutines();
        SetBattleState(BattleState.Targetting);
        //StartCoroutine(AbilityOneCR());
        ProcessAttack(Attack.AbilityOne);
    }

    /// <summary> The second ability of the player. </summary>
    public void AbilityTwo()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit == null) return;
        ActionRange.Instance.ActionSelected();
        StopAllCoroutines();
        SetBattleState(BattleState.Targetting);
        //StartCoroutine(AbilityTwoCR());
        ProcessAttack(Attack.AbilityTwo);
    }

    /// <summary> Allows the player to defend this turn. </summary>
    public void Defend()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit == null) return;

        CharacterSelector.Instance.SelectedPlayerUnit.Defend();

        AttackComplete();
    }

    /// <summary> Currently a hard pass which cancels all of the player's actions. </summary>
    public void Pass()
    {
        if (player == null) return;

        StopAllCoroutines();
        SetBattleState(BattleState.Idle);
        player.Pass();
        EndUnitTurn(player);
    }

    /// <summary> Cancles the current action we have selected. </summary>
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
            //Make sure action range is no longer displayed
            ActionRange.Instance.ActionDeselected();
            //Make sure movement range is no longer displayed
            CharacterSelector.Instance.BoarderLine.SetActive(false);
            //Deactivate combat buttons
            DeactivateCombatButtons();
           // player.GetComponent<MeshRenderer>().material.color = Color.gray;
            player.GetComponent<MeshRenderer>().material = player.defaultMat;
            if (playersToGo.Count == 0)
            {
                if (enemiesToGo.Count > 0)
                {
                    StartCoroutine(EnemyTurn());
                    SetActiveUnits(ActiveUnits.Enemies);
                    activeSideText.text = "Enemy Turn";
                }
                else
                {
                    NewRound();
                }
            }
        }
        else
        {
            //(Enemy)unit.

            enemiesToGo.Remove((Enemy)unit);

            if (enemiesToGo.Count == 0)
            {
                //StartCoroutine(TurnSwitchCR());
                //StopCoroutine(EnemyTurn());
                //NewRound();
            }
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
                //unit.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                unit.GetComponent<MeshRenderer>().material = unit.GetComponent<Player>().defaultMat;
            }
            else if (unit is Enemy && ((Enemy)unit).Revealed == true)
            {
                enemiesToGo.Add((Enemy)unit);
            }

            unit.DefendState = DefendingState.NotDefending;
            unit.HasMoved = false;
            unit.HasAttacked = false;
        }
        //increment tile cooldown
        for(int tile = coolingTiles.Count - 1; tile >= 0; tile--)
        {
            if (coolingTiles[tile].NewRound())
            {
                coolingTiles.Remove(coolingTiles[tile]);
            }
        }

        UpdateTimers();

        SetActiveUnits(ActiveUnits.Players);

        activeSideText.text = "Player's turn";

        _roundCounter++;
    }

    /// <summary> Executes the attack type that we have passed in. </summary>
    /// <param name="type">The attack of the selected player to activate. </param>
    void ProcessAttack(Attack type)
    {
        //SetBattleState(BattleState.PerformingAction);

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
    /// This is performed through an Action callback
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

        while (enemiesToGo.Count > 0)
        {
            int index = Random.Range(0, enemiesToGo.Count);

            if (enemiesToGo[index].Revealed == false)
            {
                enemiesToGo.Remove(enemiesToGo[index]);
                continue;
            }

            Enemy tempE = enemiesToGo[index];

            if (tempE.GetNumOfStatusEffects() > 0 && tempE.IsTaunted())
            {
                tempE.Move(tempE.TauntedPath());
            }
            else
            {
                tempE.Move(tempE.FindNearestPlayer());
            }

            yield return new WaitUntil(() => tempE.HasMoved == true);

            if (tempE.CheckIfInRangeOfTarget())
            {
                tempE.Attack();
            }
            else
            {
                tempE.Defend();
            }

            EndUnitTurn(enemiesToGo[index]);
        }

        NewRound();
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
        unitsAlive.Remove(unit);

        SetEnemyCountText();

        unit.currentTile.occupant = null;
        unit.currentTile.occupied = false;

        if (unit is Player)
        {
            playersToGo.Remove((Player)unit);
            if (CheckLoseCondition()) GameLost();
        }
        else
        {
            enemiesToGo.Remove((Enemy)unit);
            if (CheckWinCondition()) GameWon();
        }


        Destroy(unit.gameObject);

    }

    /// <summary>
    /// Sets enemiesAliveText
    /// </summary>
    /// Author: Jeremy Casada
    public void SetEnemyCountText()
    {
        int count = 0;
        foreach (Humanoid unit in unitsAlive)
        {
            if (unit is Enemy)
            {
                count++;
            }
        }
        enemiesAliveText.text = "Enemies Left: " + count;
    }

    /// <summary> Checks the win condition to see if it's met. </summary>
    /// <returns>True if win condition met, false otherwise.</returns>
    private bool CheckWinCondition()
    {
        foreach (Humanoid unit in unitsAlive)
        {
            if (unit is Enemy) return false;
        }

        return true;
    }

    /// <summary> Checks the lose condition to see if it's met. </summary>
    /// <returns>True if lose condition met, false otherwise. </returns>
    private bool CheckLoseCondition()
    {
        foreach (Humanoid unit in unitsAlive)
        {
            if (unit is Player) return false;
        }

        return true;
    }

    /// <summary> Activate the win screen canvas here when the win condition is met. </summary>
    private void GameWon()
    {
        SetBattleState(BattleState.Won);

        endGameText.text = "You Win!";

        endCanvas.SetActive(true);
    }

    /// <summary> Active the end screen canvas and change the text to You Lose! when the game is lost. </summary>
    private void GameLost()
    {
        SetBattleState(BattleState.Lost);

        endGameText.text = "You Lose!";

        endCanvas.SetActive(true);
    }


    /// <summary>
    /// Activates the combat buttons.
    /// </summary>
    public void ActivateCombatButtons()
    {
        Player tempP = CharacterSelector.Instance.SelectedPlayerUnit;

        foreach (Button button in combatButtons)
        {
            button.interactable = true;

            if (button.gameObject.name == "Ability One")
            {
                button.GetComponentInChildren<Text>().text = tempP.Ability1Name;

                print("Ability one CD: " + tempP.RemainingAbilityOneCD);

                if (tempP.RemainingAbilityOneCD > 0) button.interactable = false;
            }
            else if (button.gameObject.name == "Ability Two")
            {
                button.GetComponentInChildren<Text>().text = tempP.Ability2Name;

                print("Ability two CD: " + tempP.RemainingAbilityTwoCD);

                if (tempP.RemainingAbilityTwoCD > 0) button.interactable = false;
            }

        }
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
    private void UpdateTimers()
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

    /// <summary>
    /// Called to set the ability to uninteractable.
    /// Used for abilities that don't take up one of your actions.
    /// </summary>
    /// <param name="activeState">Indicates if we want it on (true) or off (false).</param>
    public void SetAbilityOneButtonState(bool activeState)
    {
        GameObject.Find("Ability One").GetComponent<Button>().interactable = activeState;
    }

    /// <summary>
    /// Called to set the ability to uninteractable.
    /// Used for abilities that don't take up one of your actions.
    /// </summary>
    /// <param name="activeState">Indicates if we want it on (true) or off (false).</param>
    public void SetAbilityTwoButtonState(bool activeState)
    {
        GameObject.Find("Ability Two").GetComponent<Button>().interactable = activeState;
    }
}