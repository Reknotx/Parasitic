﻿/*
 * Author: Ryan Dangerfield
 * Date: 9/16/2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionRange : MonoBehaviour
{

    //color of line for normal attack
    public Color attackLineColor;
    //color of line for ability1
    public Color AbilityColor;
    /// <summary> color of line for ability2 </summary>
    public Color Ability2Color;
    /// <summary> Line Renderer for action range; </summary>
    LineRenderer lineRenderer;
    /// <summary> Material of line </summary>
    Material lineMaterial;
    /// <summary> Height of action range line </summary>
    /// Should be higher than movement height if you want them to be displayed at the same time
    float height = 0.3f;
    /// <summary> Range of action on button hover </summary>
    bool[,] tempRange;
    /// <summary> Range of clicked action</summary>
    bool[,] selectedRange;
    /// <summary> A button has been clicked </summary>
    bool actionSelected = false;
    /// <summary> Color of action line being hovered over </summary>
    Color tempColor;
    /// <summary> Color of line of action that was clicked </summary>
    Color selectedColor;

    bool movementActive = false;

    

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineMaterial = lineRenderer.material;
        gameObject.SetActive(false);
    }

    /// <summary> Make normal attack range appear </summary>
    public void NormalAttack()
    {
        if(CharacterSelector.Instance.SelectedPlayerUnit != null)
        {
            tempRange = CharacterSelector.Instance.SelectedPlayerUnit.AttackTileRange;
            tempColor = attackLineColor;
            SetBoarder(tempRange, tempColor);
        }

    }

    /// <summary> Make ability1 range appear </summary>
    public void Ability1()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit != null)
        {
            tempRange = CharacterSelector.Instance.SelectedPlayerUnit.Ability1TileRange;
            tempColor = AbilityColor;
            SetBoarder(tempRange,tempColor);
        }
    }

    /// <summary> Make ability2 range appear </summary>
    public void Ability2()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit != null)
        {
            tempRange = CharacterSelector.Instance.SelectedPlayerUnit.Ability2TileRange;
            tempColor = Ability2Color;
            SetBoarder(tempRange,tempColor);
        }
    }

    /// <summary> Display range </summary>
    /// <param name="range"></param>
    /// <param name="color"></param>
    void SetBoarder(bool[,] range, Color color)
    {
        //if the movement range is being displayed, hide it
        if (CharacterSelector.Instance.BoarderLine.activeSelf)
        {
            movementActive = true;
            CharacterSelector.Instance.BoarderLine.SetActive(false);
        }
        lineMaterial.color = color;
        MapGrid.Instance.DrawBoarder(range, ref lineRenderer,height);
        gameObject.SetActive(true);
    }

    /// <summary> Action clicked </summary>
    public void AcionSelected()
    {
        actionSelected = true;
        selectedRange = tempRange;
        selectedColor = tempColor;
    }

    /// <summary> Current range is no longer selected - Hide range </summary>
    public void ActionDeselected()
    {
        actionSelected = false;
        gameObject.SetActive(false);
    }

    /// <summary> Hide temperary range selection </summary>
    public void HideBoarder()
    {
        if (actionSelected)
        {
            SetBoarder(selectedRange, selectedColor);
        }
        else
        {
            if (movementActive)
            {
                CharacterSelector.Instance.BoarderLine.SetActive(true);
                movementActive = false;
            }
            gameObject.SetActive(false);
        }
    }
}