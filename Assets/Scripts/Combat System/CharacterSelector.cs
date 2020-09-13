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
    [HideInInspector] public Enemy SelectedEnemyUnit;

    GameObject SelectedUnitObj;
    Vector3 gridSelection;
    List<Tile> path;
    Tile selectedTile;
    public GameObject PathLine;
    public GameObject EndPoint;
    public GameObject BoarderLine;
    public float pathHeight = 0.3f;
    LineRenderer lineRenderer;
    LineRenderer boarderRenderer;
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
                    
                    //This will never be reached
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
                    SelectedEnemyUnit = objectHit.gameObject.GetComponent<Enemy>();

                    if (Input.GetMouseButtonDown(0))
                    {
                        //We are about to perform an attack on an enemy game object.
                        List<Tile> neighbors = MapGrid.Instance.GetNeighbors(SelectedPlayerUnit.currentTile);

                        foreach (Tile tile in neighbors)
                        {
                            if (tile.occupied && tile.occupant == SelectedEnemyUnit)
                            {
                                //((IPlayer)SelectedPlayerUnit).NormalAttack(SelectedEnemyUnit);
                                CombatSystem.Instance.SetTarget(SelectedEnemyUnit);
                                break;
                            }
                        }
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
