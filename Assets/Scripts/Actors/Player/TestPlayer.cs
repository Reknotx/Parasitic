//Ryan Dangerfield
//9/9/20
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject SelectedUnitObj;
    //layermask only hits player and grid layers
    public int layermask = ((1 << 8) | (1 << 9));
    TestUnit SelectedUnit;
    Vector3 gridSelection;
    List<Tile> path;
    Tile selectedTile;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if(Physics.Raycast(ray, out hit, 100f, layermask))
            {
                Transform objectHit = hit.transform;
                if (objectHit.CompareTag("Player"))
                {
                    if(objectHit.gameObject != SelectedUnit)
                    {
                        if (SelectedUnit)
                        {
                            SelectedUnit.UnitDeselected();
                        }
                        SelectedUnitObj = objectHit.gameObject;
                        SelectedUnit = SelectedUnitObj.GetComponent<TestUnit>();
                        SelectedUnit.UnitSelected();
                        print("Selected Player Unit");
                    }
                    
                }
                else if (SelectedUnit)
                {
                    selectedTile = MapGrid.Instance.TileFromPosition(hit.point);
                    //if the tile selected is a valid tile to move to find the path
                    if(selectedTile.movementTile && !selectedTile.occupied)
                    {
                        path = MapGrid.Instance.FindPath(SelectedUnit.currentTile, selectedTile);
                        SelectedUnit.BeginMovement(path);
                        SelectedUnit.UnitDeselected();
                        SelectedUnit = null;
                    }
                }
                
            }
        }
    }
}
