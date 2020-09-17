/*
 * Author: Chase O'Connor
 * Date: 9/8/2020
 * 
 * Brief: Fires a ray through the camera at the mouse cursor's position to 
 * select game objects in the scene.
 */


using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    
    public static CharacterSelector Instance;

    //layermask only hits player and grid layers
    int layermask = ((1 << 8) | (1 << 9));
    int enemyLayerMask = (1 << 10);

    /// <summary> The selected player unit. </summary>
    [HideInInspector] public Player SelectedPlayerUnit;

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
    LineRenderer boarderRenderer;

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

        RaycastHit info = new RaycastHit();

        //Ryan's Implementation
        //Added features of Chase's implementation to help drive combat better.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (CombatSystem.Instance.state != BattleState.Targetting && Physics.Raycast(ray, out info, 100f, layermask))
        {
            Transform objectHit = info.transform;
            if (Input.GetMouseButtonDown(0) && objectHit.CompareTag("Player"))
            {
                Player playerObj = objectHit.gameObject.GetComponent<Player>();

                if (playerObj != SelectedPlayerUnit)
                {
                    if (SelectedPlayerUnit != null) { SelectedPlayerUnit.UnitDeselected(); }
                    SelectedUnitObj = playerObj.gameObject;
                    SelectedPlayerUnit = playerObj;
                    SelectedPlayerUnit.UnitSelected();
                    MapGrid.Instance.DrawBoarder(SelectedPlayerUnit.TileRange, ref boarderRenderer);
                    BoarderLine.SetActive(true);
                    print("Selected Player Unit");
                }
                else if (playerObj.gameObject == SelectedPlayerUnit.gameObject)
                {
                    print("Deselecting the already selected unit.");
                    SelectedPlayerUnit.UnitDeselected();
                    SelectedUnitObj = null;
                    SelectedPlayerUnit = null;
                    BoarderLine.SetActive(false);
                }
            }
            else if (SelectedPlayerUnit && SelectedPlayerUnit.HasMoved == false || debugKeepMoving)
            {
                //Selected player unit can move this turn.

                Tile lastTile = selectedTile;
                selectedTile = MapGrid.Instance.TileFromPosition(info.point);
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
                        SelectedPlayerUnit.UnitDeselected();
                        SelectedPlayerUnit = null;
                        selectedTile = null;
                        BoarderLine.SetActive(false);
                        HidePath();
                    }
                }
                else
                {
                    HidePath();
                }
            }
            
        }
        else if (CombatSystem.Instance.state == BattleState.Targetting && Physics.Raycast(ray, out info, 100f, enemyLayerMask))
        {
            Transform objectHit = info.transform;
            if (SelectedPlayerUnit && SelectedPlayerUnit.HasAttacked == false)
            {
                //Player unit has moved and can now attack.
                if (objectHit.gameObject.GetComponent<Humanoid>() is Enemy)
                {
                    Humanoid tempE = objectHit.gameObject.GetComponent<Enemy>();

                    bool[,] tempRange = MapGrid.Instance.FindTilesInRange(SelectedPlayerUnit.currentTile, SelectedPlayerUnit.AttackRange, true);

                    if (!tempRange[(int)tempE.currentTile.gridPosition.x, (int)tempE.currentTile.gridPosition.y])
                    {
                        //If the tile at position in grid is false, meaning not in our range, then return
                        //and cancel the rest of the execution.
                        Debug.Log("Enemy not in range.");
                        return;
                    }


                    if (Input.GetMouseButtonDown(0))
                    {
                        SelectedTargetUnit = tempE;
                        return; 
                    }
                }
            }
        }
        else
        {
            HidePath();
        }


        void DrawPath()
        {
            List<Vector3> points = new List<Vector3>();
            points.Add(new Vector3(SelectedPlayerUnit.transform.position.x,pathHeight, SelectedPlayerUnit.transform.position.z));
            foreach (Tile tile in path){
                points.Add(new Vector3(tile.transform.position.x,pathHeight, tile.transform.position.z));
            }
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPositions(points.ToArray());
            EndPoint.transform.position = new Vector3(selectedTile.transform.position.x, 0.25f, selectedTile.transform.position.z);
            PathLine.SetActive(true);
            EndPoint.SetActive(true);
        }

        void HidePath()
        {
            selectedTile = null;
            PathLine.SetActive(false);
            EndPoint.SetActive(false);
        }

    }
}
