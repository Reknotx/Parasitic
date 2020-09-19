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
    Material defaultMat;
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

    
    /// <summary> Tiles ability1 affects </summary>
    [HideInInspector] public bool[,] Ability1TileRange { get; set; }
    /// <summary> Tiles ability 1 affects </summary>
    [HideInInspector] public bool[,] Ability2TileRange { get; set; }

    /// <summary> Abstract method for player ability one.</summary>
    public abstract void AbilityOne(Action callback);
    /// <summary> Abstract method for player ability two.</summary>
    public abstract void AbilityTwo(Action callback);
    /// <summary> Abstract method for player normal attack.</summary>
    public abstract void NormalAttack(Action callback);

    protected abstract IEnumerator NormalAttackCR(Action callback);
    protected abstract IEnumerator AbilityOneCR(Action callback);
    protected abstract IEnumerator AbilityTwoCR(Action callback);

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


    }

    public void Pass()
    {
        HasAttacked = true;
        HasMoved = true;

        CharacterSelector.Instance.SelectedPlayerUnit = null;
        State = HumanoidState.Done;
    }

    public void FindActionRanges()
    {
        AttackTileRange = MapGrid.Instance.FindTilesInRange(currentTile, AttackRange, true);
        Ability1TileRange = MapGrid.Instance.FindTilesInRange(currentTile, Ability1Range, true);
        Ability2TileRange = MapGrid.Instance.FindTilesInRange(currentTile, Ability2Range, true);
        //print("Ranges found");
    }
}
