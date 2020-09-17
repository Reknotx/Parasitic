/*
 * Author: Ryan Dangerfield
 * Date: 9/16/2020
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionRange : MonoBehaviour
{
    public Color attackLineColor;
    public Color AbilityColor;
    public Color Ability2Color;
    LineRenderer lineRenderer;
    Material lineMaterial;
    float height = 0.3f;
    bool[,] range;
    bool actionSelected = false;
    Color color;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineMaterial = lineRenderer.material;
        gameObject.SetActive(false);
    }


    public void NormalAttack()
    {
        if(CharacterSelector.Instance.SelectedPlayerUnit != null)
        {
            range = CharacterSelector.Instance.SelectedPlayerUnit.AttackTileRange;
            color = attackLineColor;
            SetBoarder();
        }

    }

    public void Ability1()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit != null)
        {
            range = CharacterSelector.Instance.SelectedPlayerUnit.Ability1TileRange;
            color = AbilityColor;
            SetBoarder();
        }
    }

    public void Ability2()
    {
        if (CharacterSelector.Instance.SelectedPlayerUnit != null)
        {
            range = CharacterSelector.Instance.SelectedPlayerUnit.Ability2TileRange;
            color = Ability2Color;
            SetBoarder();
        }
    }

    void SetBoarder()
    {
        lineMaterial.color = color;
        MapGrid.Instance.DrawBoarder(range, ref lineRenderer,height);
        gameObject.SetActive(true);
    }

    void AcionSelected()
    {
        actionSelected = true;
    }

    public void HideBoarder()
    {
        gameObject.SetActive(false);
    }
}
