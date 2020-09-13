using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    
    public static CharacterSelector Instance;

    //layermask only hits player and grid layers
    int layermask = ((1 << 8) | (1 << 9));

    Player SelectedUnit;
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
        RaycastHit info = new RaycastHit();

        if (CombatSystem.Instance.state == BattleState.Player || CombatSystem.Instance.state == BattleState.Targetting)
        {
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out info);

            if (hit)
            {

            }
        }

        if (Input.GetMouseButtonDown(0))
        {



            //Chris's Implementation
            bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out info);

            if (hit)
            {
                Debug.Log("Selected " + info.transform.gameObject.name);

                if (info.transform.gameObject.layer == 8 && CombatSystem.Instance.state == BattleState.Player)
                {
                    CombatSystem.Instance.SetPlayer(info.transform.gameObject.GetComponent<Player>());
                }
                else if (CombatSystem.Instance.state == BattleState.Targetting)
                {
                    CombatSystem.Instance.SetTarget(info.transform.gameObject.GetComponent<Humanoid>());
                }
            }

        }

        
        //Ryan's Implementation
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out info, 100f, layermask))
        {
            Transform objectHit = info.transform;
            if (Input.GetMouseButtonDown(0) && objectHit.CompareTag("Player"))
            {
                if (objectHit.gameObject != SelectedUnit)
                {
                    if (SelectedUnit)
                    {
                        SelectedUnit.UnitDeselected();
                    }
                    SelectedUnitObj = objectHit.gameObject;
                    SelectedUnit = SelectedUnitObj.GetComponent<Player>();
                    SelectedUnit.UnitSelected();
                    MapGrid.Instance.DrawBoarder(SelectedUnit.TileRange, ref boarderRenderer);
                    BoarderLine.SetActive(true);
                    print("Selected Player Unit");
                }

            }
            else if (SelectedUnit)
            {
                Tile lastTile = selectedTile;
                selectedTile = MapGrid.Instance.TileFromPosition(info.point);
                //if the tile selected is a valid tile to move to find the path
                if (selectedTile.movementTile && !selectedTile.occupied && SelectedUnit.TileRange[(int)selectedTile.gridPosition.x, (int)selectedTile.gridPosition.y])
                {
                    //only recalculate path if tile has changed
                    if(lastTile != selectedTile)
                    {
                        path = MapGrid.Instance.FindPath(SelectedUnit.currentTile, selectedTile);
                        DrawPath();
                        //redraw path between points
                    }
                    if (Input.GetMouseButtonDown(0))
                    {
                        SelectedUnit.BeginMovement(path);
                        SelectedUnit.UnitDeselected();
                        SelectedUnit = null;
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
        else
        {
            HidePath();
        }


        void DrawPath()
        {
            List<Vector3> points = new List<Vector3>();
            points.Add(new Vector3(SelectedUnit.transform.position.x,pathHeight, SelectedUnit.transform.position.z));
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
