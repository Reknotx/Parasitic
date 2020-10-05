/*
 * Author: Chase O'Connor
 * Date: 9/4/2020
 * 
 * Brief: The base player class 
 * 
 */


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0414
public abstract class Player : Humanoid, IPlayer
{
    protected enum AttackType
    {
        NormalAttack,
        AbilityOne,
        AbilityTwo
    }

    bool selected = false;
     public Material defaultMat;
    /// <summary> The material for the player when they are selected. </summary>
    public Material selectedMat;

    /// <summary> Range of player's first ability. </summary>
    [Header("The range of the player's first ability.")]
    public int Ability1Range;

    [Space]
    /// <summary> Range of player's second ability. </summary>
    [Header("The range of the player's second ability.")]
    public int Ability2Range;

    [Space]
    [Header("The cooldown of the player's first ability.")]
    public int Ability1Cooldown;

    [Space]
    [Header("The cooldown of the player's second ability.")]
    public int Ability2Cooldown;

    /// <summary> The remaining cooldown on ability one. </summary>
    int _remainingAbilityOneCD;

    /// <summary> The remaining cooldown on ability two.  </summary>
    int _remainingAbilityTwoCD;

    [Space]
    /// <summary> The name of the player's second ability.</summary>
    public Sprite NormalAttackSpriteDefault, NormalAttackSpriteHover, NormalAttackSpriteClick, NormalAttackInfo;

    [Space]
    /// <summary> The name of the player's first ability.</summary>
    public Sprite Ability1SpriteDefault, Ability1SpriteHover, Ability1SpriteClick, Ability1Info;

    [Space]
    /// <summary> The name of the player's second ability.</summary>
    public Sprite Ability2SpriteDefault, Ability2SpriteHover, Ability2SpriteClick, Ability2Info;

    
    
    /// <summary> Tiles ability1 affects </summary>
    [HideInInspector] public bool[,] Ability1TileRange { get; set; }
    /// <summary> Tiles ability 1 affects </summary>
    [HideInInspector] public bool[,] Ability2TileRange { get; set; }

    /// <summary> Public property to get the remaining cooldown of ability 1. </summary>
    public int RemainingAbilityOneCD { get { return _remainingAbilityOneCD; } }

    /// <summary> Public property to get the remaining cooldown of ability2. </summary>
    public int RemainingAbilityTwoCD { get { return _remainingAbilityTwoCD; } }

    /// <summary> Abstract method for player ability one.</summary>
    public abstract void AbilityOne(Action callback);
    /// <summary> Abstract method for player ability two.</summary>
    public abstract void AbilityTwo(Action callback);
    /// <summary> Abstract method for player normal attack.</summary>
    public abstract void NormalAttack(Action callback);

    protected abstract IEnumerator NormalAttackCR(Action callback);
    protected abstract IEnumerator AbilityOneCR(Action callback);
    protected abstract IEnumerator AbilityTwoCR(Action callback);

    public AudioClip abilityOneSoundEffect;
    public AudioClip abilityTwoSoundEffect;

    public override void Start()
    {
        defaultMat = GetComponent<MeshRenderer>().material;
        if (selectedMat == null) selectedMat = Resources.Load<Material>("SelectedMat");

        base.Start();
    }

    public void UnitSelected()
    {
        print("Player selected");
        GetComponent<MeshRenderer>().material = selectedMat;
        State = HumanoidState.Selected;

        CombatSystem.Instance.ActivateCombatButtons();
        CombatSystem.Instance.SetPlayer(this);
        selected = true;
    }

    public void UnitDeselected()
    {
        print("Player deselected");
        GetComponent<MeshRenderer>().material = defaultMat;
        State = HumanoidState.Idle;
        CombatSystem.Instance.SetPlayer(null);
        CombatSystem.Instance.DeactivateCombatButtons();
        selected = false;
    }

    /// <summary>
    /// Raises the defense stat of the player temporarily.
    /// </summary>
    public void Defend()
    {
        print("Defending this round.");
        DefendState = DefendingState.Defending;
    }

    /// <summary>
    /// Allows the unit to pass there turn.
    /// </summary>
    public void Pass()
    {
        HasAttacked = true;
        HasMoved = true;

        CharacterSelector.Instance.SelectedPlayerUnit = null;
        State = HumanoidState.Done;
    }

    /// <summary>
    /// Override of advance timer that also reduces the cooldown on abilities.
    /// </summary>
    public override void AdvanceTimer()
    {
        base.AdvanceTimer();

        if (_remainingAbilityOneCD > 0) _remainingAbilityOneCD--;

        if (_remainingAbilityTwoCD > 0) _remainingAbilityTwoCD--;

    }

    /// <summary>
    /// Starts the cooldown of this unit's first ability.
    /// </summary>
    protected void StartAbilityOneCD()
    {
        _remainingAbilityOneCD = Ability1Cooldown;
    }

    /// <summary>
    /// Starts the cooldown of this unit's second ability.
    /// </summary>
    protected void StartAbilityTwoCD()
    {
        _remainingAbilityTwoCD = Ability2Cooldown;
    }

    /// <summary> Play's the sound effect for this player's first ability. </summary>
    protected void PlayAbilityOneSoundEffect()
    {
        audioSource.clip = abilityOneSoundEffect;

        audioSource.Play();
    }

    /// <summary> Play's the sound effect for this player's second ability. </summary>
    protected void PlayAbilityTwoSoundEffect()
    {
        audioSource.clip = abilityTwoSoundEffect;

        audioSource.Play();
    }

    public void FindActionRanges()
    {
        AttackTileRange = MapGrid.Instance.FindTilesInRange(currentTile, AttackRange, true, AttackShape);
        Ability1TileRange = MapGrid.Instance.FindTilesInRange(currentTile, Ability1Range, true);
        Ability2TileRange = MapGrid.Instance.FindTilesInRange(currentTile, Ability2Range, true);
        //print("Ranges found");
    }

    public void Heal()
    {
        Health += Mathf.FloorToInt(MaxHealth * 0.2f);

        if (Health > MaxHealth) Health = MaxHealth;

        healthText.text = Health + "/" + _maxHealth;

        healthBar.value = (float)Health / (float)_maxHealth;
    }
}
