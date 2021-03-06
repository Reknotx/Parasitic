﻿#pragma warning disable IDE0020 
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
using UnityEngine.SceneManagement;

/// <summary>
/// Enum for the various states of combat we can be in.
/// </summary>
public enum BattleState
{
    Start,
    Idle,
    PerformingAction,
    Targeting,
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

/// <summary> Enum representing which attack was executed. </summary>
public enum Attack
{
    NormalAttack,
    AbilityOne,
    AbilityTwo
}

/// <summary>
/// Main entry point of the program (for the moment) where all data is handled.
/// </summary>
public class CombatSystem : MonoBehaviour
{
    #region UI References

    /// <summary> The canvas that is displayed when the game has been won or lost. </summary>
    [Header("The UI Variables.", order = 0)]
    [Header("The canvas that is displayed when you have met the win/lose condition.", order = 1)]
    public GameObject endCanvas;
    public GameObject nextLevelButton;

    [Space]

    [Header("The Image that tells you whether you win or lose.")]
    public Image endGameImage;
    public Sprite loseSprite;
    public Sprite winSprite;

    public Text enemiesAliveText;

    public Image abilityInfo;

    /// <summary>  </summary>
    public Text roundCounterText;


    public Text abilityOneCDText;
    public Text abilityTwoCDText;

    /// <summary> Images Showing which side is active. </summary>
    public Image activeSideTextImage;
    public Image activeSideImage;

    public Sprite playerTurnSprite, playerTurnTextSprite, enemyTurnSprite, enemyTurnTextSprite, defendInfoSprite, cancelInfoSprite, endTurnInfoSprite;

    [Header("Knight Health Bar and Text References")]
    public Slider knightHealthSlider;
    public Text knightHealthText;

    [Header("Mage Health Bar and Text References")]
    public Slider mageHealthSlider;
    public Text mageHealthText;

    [Header("Archer Health Bar and Text References")]
    public Slider archerHealthSlider;
    public Text archerHealthText;

    [Header("Knight XP Bar and Text References")]
    public Slider knightXpSlider;
    public Text knightXpText;

    [Header("Mage XP Bar and Text References")]
    public Slider mageXpSlider;
    public Text mageXpText;

    [Header("Archer XP Bar and Text References")]
    public Slider archerXpSlider;
    public Text archerXpText;

    [Header("Player Icons")]
    public Image knightIcon;
    public Image mageIcon;
    public Image archerIcon;

    [Header("The canvas that is displayed when you have met the win/lose condition.")]
    /// <summary> The list of buttons used for combat when a player is selected. </summary>
    public List<Button> combatButtons = new List<Button>();
    #endregion

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
    //private Player player;

    /// <summary> The target of combat. </summary>
    //private Humanoid target;

    private int _roundCounter = 1;

    /// <summary> The list of player's that have yet to go this round. </summary>
    private List<Player> playersToGo = new List<Player>();

    /// <summary> The list of enemies that have yet to go this round. </summary>
    private List<Enemy> enemiesToGo = new List<Enemy>();

    /// <summary> The master list of all units that are currently alive. </summary>
    private List<Humanoid> unitsAlive = new List<Humanoid>();

    public List<Tile> coolingTiles = new List<Tile>();

    public ParticleSystem blood;
    public ParticleSystem bloodAndGuts;

    public bool IgnoreDoubleMoveCheck = false;

    public AudioClip GameWonAudio;
    public AudioClip GameLostAudio;

    public AudioSource EndGameAudioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    void Start()
    {
        state = BattleState.Start;

        SetupBattle();
        SetEnemyCountText();
        roundCounterText.text = _roundCounter.ToString();
    }

    private void Update()
    {
        if (abilityInfo.gameObject.activeInHierarchy)
        {
            abilityInfo.rectTransform.position = Input.mousePosition;
        }
    }


    #region Player Combat

    #region Combat Button Functions
    /// <summary> Executes the coroutine for normal attack of player. </summary>
    public void NormalAttack()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit == null) return;
        if (CharacterSelector.Instance.SelectedPlayerUnit.State == HumanoidState.Moving) return;
        CharacterSelector.Instance.SelectedPlayerUnit.FindActionRanges();
        ActionRange.Instance.ActionSelected();
        StopAllCoroutines();
        SetBattleState(BattleState.Targeting);
        //StartCoroutine(NormalAttackCR());
        ProcessAttack(Attack.NormalAttack);
    }

    /// <summary> The first ability of the player. </summary>
    public void AbilityOne()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit == null) return;
        if (CharacterSelector.Instance.SelectedPlayerUnit.State == HumanoidState.Moving) return;
        CharacterSelector.Instance.SelectedPlayerUnit.FindActionRanges();
        ActionRange.Instance.ActionSelected();
        StopAllCoroutines();
        SetBattleState(BattleState.Targeting);
        //StartCoroutine(AbilityOneCR());
        ProcessAttack(Attack.AbilityOne);
    }

    /// <summary> The second ability of the player. </summary>
    public void AbilityTwo()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit == null) return;
        if (CharacterSelector.Instance.SelectedPlayerUnit.State == HumanoidState.Moving) return;
        CharacterSelector.Instance.SelectedPlayerUnit.FindActionRanges();
        ActionRange.Instance.ActionSelected();
        StopAllCoroutines();
        SetBattleState(BattleState.Targeting);
        //StartCoroutine(AbilityTwoCR());
        ProcessAttack(Attack.AbilityTwo);
    }

    /// <summary> Allows the player to defend this turn. </summary>
    public void Defend()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit == null) return;

        CharacterSelector.Instance.SelectedPlayerUnit.Defend();
        CharacterSelector.Instance.HidePath();
        AttackComplete();
    }

    /// <summary>
    /// Makes All Player Units Defend if they are able to this round
    /// </summary>
    /// Author: Jeremy Casada 
    /// Date: 11/15/2020
    public void AllDefend()
    {
        List<Player> defending = new List<Player>();

        if (Mage.Instance != null && playersToGo.Contains(Mage.Instance))
        {
            Mage.Instance.Defend();
            defending.Add(Mage.Instance);
            Mage.Instance.SetAnimationComplete(false);
        }

        if (Warrior.Instance != null && playersToGo.Contains(Warrior.Instance))
        {
            Warrior.Instance.Defend();
            defending.Add(Warrior.Instance);
            Warrior.Instance.SetAnimationComplete(false);
        }

        if (Archer.Instance != null && playersToGo.Contains(Archer.Instance))
        {
            Archer.Instance.Defend();
            defending.Add(Archer.Instance);
            Archer.Instance.SetAnimationComplete(false);
        }


        CharacterSelector.Instance.SelectedPlayerUnit = null;
        CharacterSelector.Instance.SelectedTargetUnit = null;

        CharacterSelector.Instance.HidePath();

        foreach (Player player in defending)
        {
            EndUnitTurn(player);
        }

        if (state != BattleState.Won)
            SetBattleState(BattleState.Idle);

        //SetCoolDownText(CharacterSelector.Instance.LastSelectedPlayerUnit);
    }

    /// <summary> Cancles the current action we have selected. </summary>
    public void Cancel(bool deselectPlayer = true)
    {

        Player selectedPlayer = null;


        if (CharacterSelector.Instance.SelectedPlayerUnit != null)
        {
            if (CharacterSelector.Instance.SelectedPlayerUnit.State == HumanoidState.Moving) return;
            selectedPlayer = CharacterSelector.Instance.SelectedPlayerUnit;
            ActionRange.Instance.ActionDeselected(false);
            selectedPlayer.StopAllCoroutines();
        }


        if (state == BattleState.Targeting)
        {
            ///Assumes that you have a player character in the process of targeting an ability.
            ///Cancel the targetting
            SetBattleState(BattleState.Idle);
            if (selectedPlayer != null && selectedPlayer.HasMoved == false)
            {
                CharacterSelector.Instance.BoarderLine.SetActive(true);
            }
        }
        else if (state == BattleState.Idle && selectedPlayer != null)
        {
            ///deselect the player
            if (deselectPlayer)
            {
                CharacterSelector.Instance.SelectedPlayerUnit = null;
                selectedPlayer.UnitDeselected();
            }
        }

    }
    #endregion

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
        CharacterSelector.Instance.SelectedPlayerUnit.SetAnimationComplete(false);
       
        CharacterSelector.Instance.SelectedPlayerUnit = null;
        CharacterSelector.Instance.SelectedTargetUnit = null;

        if (state != BattleState.Won)
            SetBattleState(BattleState.Idle);

        //SetCoolDownText(CharacterSelector.Instance.LastSelectedPlayerUnit);
    }
    #endregion

    #region Enemy Funtions
    /// <summary>
    /// Executes the logic for the enemies turn.
    /// </summary>
    IEnumerator EnemyTurn()
    {
        yield return new WaitForSeconds(2f);

        while (enemiesToGo.Count > 0 && !CheckLoseCondition())
        {
            int index = Random.Range(0, enemiesToGo.Count);
            enemiesToGo[index].DefendState = DefendingState.NotDefending;

            //Removes the enemies from the list if they are in it but still in fog.
            if (enemiesToGo[index].Revealed == false)
            {
                enemiesToGo.Remove(enemiesToGo[index]);
                continue;
            }

            Enemy tempE = enemiesToGo[index];

            if (tempE is Hive)
            {
                //print("Hive turn");
                Hive tempS = (Hive)tempE;
                //if enemy is spawned wait
                if (tempS.SpawnEnemy())
                {
                    yield return new WaitForSeconds(1.5f);
                }
                if (!(tempE is Brood))
                {
                    EndUnitTurn(tempE);
                    continue;
                }
            }

            //if (tempE.CheckIfInRangeOfTarget())
            if (tempE.MovementStat > 0)
            {
                if (tempE.GetNumOfStatusEffects() > 0 && tempE.IsTaunted())
                {
                    tempE.Move(tempE.TauntedPath());
                }
                else
                {
                    tempE.Move(tempE.FindNearestPlayer());
                }
                tempE.TargetIconDisplay(true);
                yield return new WaitUntil(() => tempE.HasMoved == true);
            }
            if (tempE.CheckIfInRangeOfTarget())
            {
                tempE.Attack();
            }
            else
            {
                tempE.Defend();
            }

            EndUnitTurn(enemiesToGo[index]);

            yield return new WaitForSeconds(1.5f);
            tempE.TargetIconDisplay(false);
        }

        NewRound();
    }
    #endregion

    #region Combat System Functions
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
            if (unit is Warrior)
            {
                knightIcon.color = Color.gray;
            }
            else if(unit is Mage)
            {
                mageIcon.color = Color.gray;
            }
            else if(unit is Archer)
            {
                archerIcon.color = Color.gray;
            }
            playersToGo.Remove((Player)unit);
            //Make sure action range is no longer displayed
            ActionRange.Instance.ActionDeselected();
            //Make sure movement range is no longer displayed
            CharacterSelector.Instance.BoarderLine.SetActive(false);
            //CharacterSelector.Instance.SelectedPlayerUnit.UnitDeselected();

            ((Player)unit).UnitDeselected();

            //Deactivate combat buttons
            DeactivateCombatButtons();
            // player.GetComponent<MeshRenderer>().material.color = Color.gray;
            //player.GetComponent<MeshRenderer>().material = player.defaultMat;
            if (playersToGo.Count == 0 && (state != BattleState.Won && state != BattleState.Lost))
            {
                if (enemiesToGo.Count > 0)
                {
                    StartCoroutine(EnemyTurn());
                    SetActiveUnits(ActiveUnits.Enemies);
                    SetTurnUI(activeUnits);
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

        //player = null;
        //target = null;

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
                //((Player)unit).CoolDown(); //bandaid
                if (unit is Warrior)
                {
                    knightIcon.color = Color.white;
                }
                else if (unit is Mage)
                {
                    mageIcon.color = Color.white;
                }
                else if (unit is Archer)
                {
                    archerIcon.color = Color.white;
                }
                playersToGo.Add((Player)unit);
                unit.DefendState = DefendingState.NotDefending;
                //unit.GetComponent<MeshRenderer>().material = unit.GetComponent<Player>().defaultMat;
            }
            else if (unit is Enemy enemy && enemy.Revealed == true)
            {
                enemiesToGo.Add(enemy);
            }
            unit.HasMoved = false;
            unit.HasAttacked = false;
            unit.damagedThisTurn = false;
        }
        //increment tile cooldown
        for (int tile = coolingTiles.Count - 1; tile >= 0; tile--)
        {
            if (coolingTiles[tile].NewRound())
            {
                coolingTiles.Remove(coolingTiles[tile]);
            }
        }

        if (IgnoreDoubleMoveCheck == false)
        {
            foreach (Player player in playersToGo)
            {
                if (enemiesToGo.Count == 0)
                {
                    player.DoubleMoveSpeed();
                }
                else
                {
                    player.SetMoveSpeedNormal();
                }
            }
        }

        UpdateTimers();

        SetActiveUnits(ActiveUnits.Players);

        SetTurnUI(activeUnits);

        _roundCounter++;
        roundCounterText.text = _roundCounter.ToString();
    }

    /// <summary>
    /// Kills the unit in game and removes it from system. Also checks the win condition
    /// and ends the game if it is met.
    /// </summary>
    /// <param name="unit">The unit who's health is at or below 0.</param>
    public void KillUnit(Humanoid unit)
    {
        unitsAlive.Remove(unit);
        UnsubscribeTimerUnit(unit);

        SetEnemyCountText();

        unit.currentTile.occupant = null;
        unit.currentTile.occupied = false;

        if (unit is Player)
        {
            if (unit is Warrior)
            {
                knightIcon.color = Color.red;
            }
            else if (unit is Mage)
            {
                mageIcon.color = Color.red;
            }
            else if (unit is Archer)
            {
                archerIcon.color = Color.red;
            }
            playersToGo.Remove((Player)unit);
            foreach (Humanoid temp in unitsAlive)
            {

                if (temp is Enemy enemy && enemy.playersWhoAttacked.Count > 0)
                {
                    // Debug.Log("Count Before: " + ((Enemy)temp).playersWhoAttacked.Count);
                    enemy.playersWhoAttacked.Remove((Player)unit);
                    //Debug.Log("Count After: " + ((Enemy)temp).playersWhoAttacked.Count);
                }
            }
            Instantiate(blood, unit.transform.position, blood.transform.rotation);

            if (CheckLoseCondition()) GameLost();
        }
        else
        {
            Upgrades.Instance.SplitExp((Enemy)unit);
            enemiesToGo.Remove((Enemy)unit);
            var emission = bloodAndGuts.emission;

            if (unit is Larva) emission.rateOverTime = 20;
            else emission.rateOverTime = 75;

            Instantiate(bloodAndGuts, unit.transform.position, bloodAndGuts.transform.rotation);

            if (WinConditions.Instance.CheckWinCondition(Condition.KillEnemies))
            {
                GameWon();
                
            }

        }


        //StartCoroutine(KillUnit());

        Destroy(unit.parentTransform.gameObject);

    }

    /// <summary>
    /// The point of this coroutine is to allow the audio death clip of the killed
    /// unit to play out all the way to the end before the gameobject is completed.
    /// 
    /// Will be utilized once the death audio clips have been added. To prevent a 
    /// hiccup in gameplay the unit's mesh renderer will be disabled, as we have no
    /// death animations in the project at this current time. 
    /// </summary>
    /// <returns></returns>
    IEnumerator KillUnitCR(Humanoid unit)
    {
        AudioSource tempSource = unit.unitAudio.GetAudioSource();
        unit.PlayAudio(UnitAudioPlayer.AudioToPlay.Damaged);

        yield return new WaitUntil(() => tempSource.isPlaying == false);

        Destroy(unit.parentTransform.gameObject);

    }
    #endregion

    #region Helpers and UI functions

    #region UI

    /// <summary>
    /// Sets Turn UI Based on activeSides
    /// </summary>
    /// <param name="activeSide"></param>
    /// Author: Jeremy Casada
    private void SetTurnUI(ActiveUnits activeSide)
    {
        switch (activeSide)
        {
            case ActiveUnits.Players:
                activeSideImage.sprite = playerTurnSprite;
                activeSideTextImage.sprite = playerTurnTextSprite;
                break;
            case ActiveUnits.Enemies:
                activeSideImage.sprite = enemyTurnSprite;
                activeSideTextImage.sprite = enemyTurnTextSprite;
                break;
            default:
                break;
        }
        activeSideTextImage.GetComponent<Animation>().Play();
        activeSideImage.GetComponent<Animation>().Play();
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
        enemiesAliveText.text = count.ToString();
    }

    /// <summary>
    /// Sets Sprite for Ability Info Popup Window Based on name of "button"
    /// </summary>
    /// <param name="button">Button to check</param>
    /// Author: Jeremy Casada
    public void SetAbilityInfo(Button button)
    {
        Player tempP = CharacterSelector.Instance.SelectedPlayerUnit;
        if (tempP)
        {
            if (button.gameObject.name == "Normal Attack")
            {
                abilityInfo.sprite = tempP.NormalAttackSprites[4];
            }
            else if (button.gameObject.name == "Ability One")
            {
                abilityInfo.sprite = tempP.Ability1Sprites[4];
            }
            else if (button.gameObject.name == "Ability Two")
            {
                abilityInfo.sprite = tempP.Ability2Sprites[4];
            }
            else if (button.gameObject.name == "Defend")
            {
                abilityInfo.sprite = defendInfoSprite;
            }
            else if (button.gameObject.name == "End Turn")
            {
                abilityInfo.sprite = endTurnInfoSprite;
            }
            else if(button.name == "Cancel")
            {
                abilityInfo.sprite = cancelInfoSprite;
            }
            abilityInfo.gameObject.SetActive(true);
        }


    }

    /// <summary>
    /// Hides Ability Info Popup
    /// </summary>
    /// Author: Jeremy Casada
    public void HideAbilityInfo()
    {
        abilityInfo.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the Cool Down Text on Each Ability Button
    /// </summary>
    /// <param name="player">player to compare to selected and last selected</param>
    /// Author: Jeremy Casada
    /// 10/6/20
    public void SetCoolDownText(Player player)
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit == player )
        {
            if (player.RemainingAbilityOneCD > 0)
            {
                abilityOneCDText.text = player.RemainingAbilityOneCD.ToString();
                abilityOneCDText.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                abilityOneCDText.transform.parent.gameObject.SetActive(false);
            }

            if (player.RemainingAbilityTwoCD > 0)
            {
                abilityTwoCDText.text = player.RemainingAbilityTwoCD.ToString();
                abilityTwoCDText.transform.parent.gameObject.SetActive(true);
            }
            else
            {
                abilityTwoCDText.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Activates the combat buttons.
    /// </summary>
    public void ActivateCombatButtons()
    {
        Player tempP = CharacterSelector.Instance.SelectedPlayerUnit;
        UnitToUpgrade unitType = Upgrades.Instance.GetUnitType();

        foreach (Button button in combatButtons)
        {
            button.gameObject.SetActive(true);
            button.interactable = true;

            if (button.gameObject.name == "Normal Attack")
            {
                button.GetComponent<Image>().sprite = tempP.NormalAttackSprites[0];
                SpriteState st;
                st.highlightedSprite = tempP.NormalAttackSprites[1];
                st.pressedSprite = tempP.NormalAttackSprites[2];
                st.disabledSprite = tempP.NormalAttackSprites[3];
                button.spriteState = st;
            }
            else if (button.gameObject.name == "Ability One")
            {

                button.GetComponent<Image>().sprite = tempP.Ability1Sprites[0];
                SpriteState st;
                st.highlightedSprite = tempP.Ability1Sprites[1];
                st.pressedSprite = tempP.Ability1Sprites[2];
                st.disabledSprite = tempP.Ability1Sprites[3];
                button.spriteState = st;

                //print(tempP.name + " ability one CD: " + tempP.RemainingAbilityOneCD);


                if (tempP.RemainingAbilityOneCD > 0 || !Upgrades.Instance.IsAbilityUnlocked(Abilities.ability1, unitType))
                {
                    //if (!Upgrades.Instance.IsAbilityUnlocked(Abilities.ability1, unitType))
                    //{
                    //    print(tempP.name + " ability One not unlocked");
                    //}

                    button.interactable = false;
                }

            }
            else if (button.gameObject.name == "Ability Two")
            {
                // button.GetComponentInChildren<Text>().text = tempP.Ability2Name;
                button.GetComponent<Image>().sprite = tempP.Ability2Sprites[0];
                SpriteState st;
                st.highlightedSprite = tempP.Ability2Sprites[1];
                st.pressedSprite = tempP.Ability2Sprites[2];
                st.disabledSprite = tempP.Ability2Sprites[3];
                button.spriteState = st;

                //print(tempP.name + " ability two CD: " + tempP.RemainingAbilityTwoCD);


                if (tempP.RemainingAbilityTwoCD > 0 || !Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2, unitType))
                {
                    //if (!Upgrades.Instance.IsAbilityUnlocked(Abilities.ability2, unitType))
                    //{
                    //    print(tempP.name + " ability Two not unlocked");
                    //}

                    button.interactable = false;
                }

            }
        }
    }

    /// <summary>
    /// Deactivates the combat buttons.
    /// </summary>
    public void DeactivateCombatButtons()
    {
        foreach (Button button in combatButtons)
        {
            button.gameObject.SetActive(false);
            button.interactable = false;
        }
        HideAbilityInfo();
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
    #endregion
    
    /// <summary> Checks the win condition to see if it's met. </summary>
    /// <returns>True if win condition met, false otherwise.</returns>
    public bool CheckKillCondition(EnemyType typeToKill)
    {
        //Condition winCondition = WinConditions.Instance.condition;
        //EnemyType typeToKill = WinConditions.Instance.typeToKill;

        bool winConditionMet = true;

        foreach (Humanoid unit in unitsAlive)
        {
            if (unit is Player) continue;

            switch (typeToKill)
            {
                case EnemyType.AllTypes:
                    if (unit is Enemy) winConditionMet = false;
                    break;

                case EnemyType.Larva:
                    if (unit is Larva) winConditionMet = false;
                    break;

                case EnemyType.Shambler:
                    if (unit is Shambler) winConditionMet = false;
                    break;

                case EnemyType.Spiker:
                    if (unit is Spiker) winConditionMet = false;
                    break;

                case EnemyType.Charger:
                    if (unit is Charger) winConditionMet = false;
                    break;

                case EnemyType.Brood:
                    if (unit is Brood) winConditionMet = false;
                    break;

                case EnemyType.Hive:
                    if (unit is Hive) winConditionMet = false;
                    break;

                default:
                    break;
            }

            if (winConditionMet == false) break;

        }

        return winConditionMet;
    }

    /// <summary>
    /// Checks if each player is in the target zone;
    /// </summary>
    /// <param name="zone"></param>
    /// <returns>Returns true if the player is in the target zone, false otherwise.</returns>
    public bool CheckAreaCondition(ObjectiveZone zone)
    {
        bool winConditionMet = true;
        foreach (Humanoid unit in unitsAlive)
        {
            if(unit is Player playerUnit)
            {
                winConditionMet = playerUnit.AtTargetObjective(zone);
            }
            if (winConditionMet == false) break;
        }
        return winConditionMet;
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
    public void GameWon()
    {
        SetBattleState(BattleState.Won);

        endGameImage.sprite = winSprite;

        DeactivateCombatButtons();

        endCanvas.SetActive(true);

        

        if(SceneManager.GetActiveScene().name.Contains("Level_6"))
        {
            endCanvas.SetActive(false);
            switch (SceneManager.GetActiveScene().name)
            {
                case "Level_6_P1":
                    LevelLoader.Instance.LoadLevel("Level_6_P2");
                    break;
                case "Level_6_P2":
                    LevelLoader.Instance.LoadLevel("Level_6_P3");
                    break;
                case "Level_6_P3":
                    LevelLoader.Instance.LoadLevel("Level_6_P4");
                    break;
                default:
                    break;
            }
            if (SceneManager.GetActiveScene().name == "Level_6_P4")
            {
                nextLevelButton.SetActive(false);
                endCanvas.SetActive(true);
            }
        }
        else
        {
            EndGameAudioSource.clip = GameWonAudio;

            EndGameAudioSource.Play();
        }
    }

    private IEnumerator LoadMultiScene()
    {
        yield return new WaitUntil(() => EndGameAudioSource.isPlaying == true);
        
    }

    /// <summary> Active the end screen canvas and change the text to You Lose! when the game is lost. </summary>
    private void GameLost()
    {
        SetBattleState(BattleState.Lost);

        endGameImage.sprite = loseSprite;

        DeactivateCombatButtons();
        endCanvas.SetActive(true);

        EndGameAudioSource.clip = GameLostAudio;

        EndGameAudioSource.Play();
    }
    #endregion

    #region Deprecated Functions
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

        //player = selection;
    }
    #endregion


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
            SubscribeTimerUnit(player);
        }

        foreach (Enemy enemy in tempE)
        {

            if (enemy.Revealed == true) enemiesToGo.Add(enemy);
            unitsAlive.Add(enemy);
            SubscribeTimerUnit(enemy);
        }

        if (IgnoreDoubleMoveCheck == false && enemiesToGo.Count == 0)
        {
            foreach(Player player in tempP)
            {
                player.DoubleMoveSpeed();
            }
        }

        DeactivateCombatButtons();

        SetBattleState(BattleState.Idle);
        SetActiveUnits(ActiveUnits.Players);

        if (endCanvas.activeSelf) endCanvas.SetActive(false);
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

    /// <summary> Adds a revealed enemy to the turn system. </summary>
    /// <param name="enemy">The enemy to add.</param>
    public void SubscribeEnemy(Enemy enemy)
    {
        enemiesToGo.Add(enemy);
    }

    public List<Humanoid> timerUnits = new List<Humanoid>();

    /// <summary>
    /// Subscribes a unit that has been buffed or debuffed to the system.
    /// After every round these units will have their counters updated.
    /// </summary>
    /// <param name="subject">The unit that is altered.</param>
    public void SubscribeTimerUnit(Humanoid subject)
    {
        if (timerUnits == null) { timerUnits = new List<Humanoid>(); }
        if(!timerUnits.Contains(subject))
        {
            timerUnits.Add(subject);
        }
    }

    /// <summary>
    /// Unsubscribes the altered unit when they are killed.
    /// </summary>
    /// <param name="subject">The unit that was previously killed.</param>
    public void UnsubscribeTimerUnit(Humanoid subject)
    {
        removeList.Add(subject);

        timerUnits.Remove(subject);
    }

    private List<Humanoid> removeList = new List<Humanoid>();


    /// <summary>
    /// Updates the list of altered units.
    /// </summary>
    private void UpdateTimers()
    {
        removeList = new List<Humanoid>();

        foreach (Humanoid unit in timerUnits)
        {
            if (unit != null)
            {
                unit.AdvanceTimer();
            }
        }

        ///This is run for units that have died
        ///and need to be removed from the timer list.
        foreach (Humanoid unit in removeList)
        {
            timerUnits.Remove(unit);
        }

        removeList.Clear();
    }

    public void NewSpawn(Humanoid spawn)
    {
        unitsAlive.Add(spawn);
    }
}