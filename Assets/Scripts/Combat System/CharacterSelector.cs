using System.Collections.Generic;
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
    //[HideInInspector] public bool selectPlayer = true;
    //[HideInInspector] public bool selectTarget = false;

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        Instance = this;
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

            //Ryan's Implementation
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out info, 100f, layermask))
            {
                Transform objectHit = info.transform;
                if (objectHit.CompareTag("Player"))
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
                        print("Selected Player Unit");
                    }

                }
                else if (SelectedUnit)
                {
                    selectedTile = MapGrid.Instance.TileFromPosition(info.point);
                    //if the tile selected is a valid tile to move to find the path
                    if (selectedTile.movementTile && !selectedTile.occupied)
                    {
                        path = MapGrid.Instance.FindPath(SelectedUnit.currentTile, selectedTile);
                        SelectedUnit.BeginMovement(path);
                        SelectedUnit.UnitDeselected();
                        SelectedUnit = null;
                    }
                }

            }

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
    }
}
