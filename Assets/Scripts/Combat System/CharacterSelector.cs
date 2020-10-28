﻿/*
 * Author: Chase O'Connor
 * Date: 9/8/2020
 * 
 * Brief: Fires a ray through the camera at the mouse cursor's position to 
 * select game objects in the scene.
 */


using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable IDE0044 // Add readonly modifier
public class CharacterSelector : MonoBehaviour
{

    public enum TargettingType
    {
        TargetPlayers,
        TargetEnemies
    }

    public static CharacterSelector Instance;

    [HideInInspector] public TargettingType targettingType;

    //layermask only hits player and grid layers
    int layermask = ((1 << 8) | (1 << 9));
    int enemyLayerMask = (1 << 10);

    /// <summary> The selected player unit. </summary>
    [HideInInspector] public Player SelectedPlayerUnit;
    [HideInInspector] public Player LastSelectedPlayerUnit;

    /// <summary> The selected enemy unit. </summary>
    [HideInInspector] public Humanoid SelectedTargetUnit;

    /// <summary> GameObject of unity selcted </summary>
    GameObject SelectedUnitObj;

    /// <summary> Tile path between unit and tile selction </summary>
    List<Tile> path;

    /// <summary> The currently slected tile </summary>
    Tile selectedTile;

    /// <summary> Line drawn from selcted player to tile </summary>
    public GameObject PathLine;
    /// <summary> GameObject that shows which tile the mouse is hovering over at the end of the line</summary>
    public GameObject EndPoint;

    /// <summary> Line that surrounds the range a player can move </summary>
    public GameObject BoarderLine;

    /// <summary> Height of path line </summary>
    public float pathHeight = 0.3f;

    /// <summary> Line Renderers for the path and boarder </summary>
    LineRenderer lineRenderer; 
    [HideInInspector] public LineRenderer boarderRenderer;

    /// <summary> Player unit is currently moving in the scene</summary>
    [HideInInspector] public bool unitMoving = false;

    /// <summary> When true a player can still move after they have already moved </summary>
    public bool debugKeepMoving = false;

    //[HideInInspector] public bool selectPlayer = true;
    //[HideInInspector] public bool selectTarget = false;

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        Instance = this;
        boarderRenderer = BoarderLine.GetComponent<LineRenderer>();
        lineRenderer = PathLine.GetComponent<LineRenderer>();
    }


    void Update()
    {
        if (CombatSystem.Instance.activeUnits == ActiveUnits.Enemies) return;

        if (CombatSystem.Instance.state == BattleState.PerformingAction) return;

        if (CombatSystem.Instance.state == BattleState.Won || CombatSystem.Instance.state == BattleState.Lost) return;

        RaycastHit info = new RaycastHit();
        bool upgradeUiUp = false;
        bool pauseUiUp = false;
        if (Upgrades.Instance)
        {
            upgradeUiUp = Upgrades.Instance.upgradeWindow.activeSelf;
        }
        if (UI.Instance)
        {
            pauseUiUp = UI.Instance.PausedStatus;
        }


        //Ryan's Implementation
        //Added features of Chase's implementation to help drive combat better.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!EventSystem.current.IsPointerOverGameObject() && !upgradeUiUp && !pauseUiUp)
        {
            if (CombatSystem.Instance.state != BattleState.Targetting && Physics.Raycast(ray, out info, 100f, layermask))
            {
                Transform objectHit = info.transform;
                if (Input.GetMouseButtonDown(0) && objectHit.CompareTag("Player") && !unitMoving)
                {
                    Player playerObj = objectHit.gameObject.GetComponent<Player>();

                    if (playerObj != SelectedPlayerUnit && !playerObj.HasAttacked)
                    {
                        if (SelectedPlayerUnit != null)
                        {
                            SetLastSelected();
                            SelectedPlayerUnit.UnitDeselected();
                        }
                        SelectedUnitObj = playerObj.gameObject;
                        SelectedPlayerUnit = playerObj;
                        if(LastSelectedPlayerUnit == null)
                        {
                            SetLastSelected();
                        }
                        SelectedPlayerUnit.UnitSelected();
                        BoarderLine.SetActive(false);
                        CombatSystem.Instance.SetCoolDownText(SelectedPlayerUnit);
                        if (SelectedPlayerUnit.HasMoved == false || debugKeepMoving)
                        {
                            SelectedPlayerUnit.FindMovementRange();
                            MapGrid.Instance.DrawBoarder(SelectedPlayerUnit.TileRange, ref boarderRenderer);
                            BoarderLine.SetActive(true);
                        }
                        //Make sure previous action range is no longer displayed
                        ActionRange.Instance.ActionDeselected();
                        SelectedPlayerUnit.FindActionRanges();
                        //print("Selected Player Unit");
                    }
                    else if (SelectedPlayerUnit != null && playerObj.gameObject == SelectedPlayerUnit.gameObject)
                    {
                        //print("Deselecting the already selected unit.");
                        SetLastSelected();
                        SelectedPlayerUnit.UnitDeselected();
                        SelectedUnitObj = null;
                        SelectedPlayerUnit = null;
                        BoarderLine.SetActive(false);
                        CombatSystem.Instance.SetCoolDownText(LastSelectedPlayerUnit);
                    }
                }
                else if (SelectedPlayerUnit && (SelectedPlayerUnit.HasMoved == false || debugKeepMoving))
                {
                    //Selected player unit can move this turn.

                    Tile lastTile = selectedTile;
                    selectedTile = MapGrid.Instance.TileFromPosition(info.point);
                    if(selectedTile == null)
                    {
                        Debug.Log("this bitch empty");
                        Debug.Log(info.point);
                    }
                    //if the tile selected is a valid tile to move to find the path
                    if (selectedTile.movementTile && !selectedTile.occupied && SelectedPlayerUnit.TileRange[(int)selectedTile.gridPosition.x, (int)selectedTile.gridPosition.y])
                    {
                        //only recalculate path if tile has changed
                        if (lastTile != selectedTile)
                        {
                            path = MapGrid.Instance.FindPath(SelectedPlayerUnit.currentTile, selectedTile);
                            DrawPath();
                            //redraw path between points
                        }
                        if (Input.GetMouseButtonDown(0))
                        {
                            SelectedPlayerUnit.Move(path);
                            SetLastSelected();
                            //SelectedPlayerUnit.UnitDeselected();
                            //SelectedPlayerUnit = null;
                            selectedTile = null;
                            BoarderLine.SetActive(false);
                            HidePath();
                            CombatSystem.Instance.SetCoolDownText(LastSelectedPlayerUnit);
                        }
                    }
                    else
                    {
                        HidePath();
                    }
                }

            }
            else if (CombatSystem.Instance.state == BattleState.Targetting &&
                ((targettingType == TargettingType.TargetEnemies && Physics.Raycast(ray, out info, 100f, enemyLayerMask))
                || (targettingType == TargettingType.TargetPlayers && Physics.Raycast(ray, out info, 100f, layermask))
                ))
            {
                Transform objectHit = info.transform;
                if (SelectedPlayerUnit && SelectedPlayerUnit.HasAttacked == false)
                {
                    //Player unit has moved and can now attack.
                    if (targettingType == TargettingType.TargetEnemies && objectHit.gameObject.GetComponent<Humanoid>() is Enemy)
                    {
                        Humanoid tempE = objectHit.gameObject.GetComponent<Enemy>();

                        //bool[,] tempRange = MapGrid.Instance.FindTilesInRange(SelectedPlayerUnit.currentTile, SelectedPlayerUnit.AttackRange, true);
                        bool[,] tempRange = SelectedPlayerUnit.AttackTileRange;

                        if (!tempRange[(int)tempE.currentTile.gridPosition.x, (int)tempE.currentTile.gridPosition.y])
                        {
                            //If the tile at position in grid is false, meaning not in our range, then return
                            //and cancel the rest of the execution.
                            return;
                        }


                        if (Input.GetMouseButtonDown(0))
                        {
                            SelectedTargetUnit = tempE;
                            return;
                        }
                    }
                    else if (targettingType == TargettingType.TargetPlayers && objectHit.gameObject.GetComponent<Humanoid>() is Player)
                    {
                        Humanoid tempP = objectHit.gameObject.GetComponent<Player>();

                        //bool[,] tempRange = MapGrid.Instance.FindTilesInRange(SelectedPlayerUnit.currentTile, SelectedPlayerUnit.AttackRange, true);
                        bool[,] tempRange = SelectedPlayerUnit.AttackTileRange;

                        if (!tempRange[(int)tempP.currentTile.gridPosition.x, (int)tempP.currentTile.gridPosition.y])
                        {
                            //If the tile at position in grid is false, meaning not in our range, then return
                            //and cancel the rest of the execution.
                            return;
                        }


                        if (Input.GetMouseButtonDown(0))
                        {
                            SelectedTargetUnit = tempP;
                            return;
                        }
                    }
                }
            }
            else
            {
                HidePath();
            }
        }
    }

    void DrawPath()
    {
        List<Vector3> points = new List<Vector3>();
        points.Add(new Vector3(SelectedPlayerUnit.transform.position.x, pathHeight, SelectedPlayerUnit.transform.position.z));
        foreach (Tile tile in path)
        {
            points.Add(new Vector3(tile.transform.position.x, pathHeight, tile.transform.position.z));
        }
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
        EndPoint.transform.position = new Vector3(selectedTile.transform.position.x, 0.25f, selectedTile.transform.position.z);
        PathLine.SetActive(true);
        EndPoint.SetActive(true);
    }

    public void HidePath()
    {
        selectedTile = null;
        PathLine.SetActive(false);
        EndPoint.SetActive(false);
    }

    public void SetTargettingType(TargettingType type)
    {
        targettingType = type;
    }

    /// <summary>
    /// Sets the Last Selected Player to the Current Selected Player
    /// </summary>
    /// Author: Jeremy Casada
    /// 10/6/20
    public void SetLastSelected()
    {
        LastSelectedPlayerUnit = SelectedPlayerUnit;
    }
}
