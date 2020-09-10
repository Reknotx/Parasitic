//Ryan Dangerfield
//9/9/20
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    public Camera camera;
    public GameObject SelectedUnitObj;
    public int layermask = ((1 << 8) | (1 << 9));
    TestUnit SelectedUnit;
    Vector3 gridSelection;
    List<Tile> path;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);
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
                    path = MapGrid.Instance.FindPath(SelectedUnit.currentTile, MapGrid.Instance.TileFromPosition(hit.point));
                    SelectedUnit.BeginMovement(path);
                    SelectedUnit.UnitDeselected();
                    SelectedUnit = null;
                    
                }
                
            }
        }
    }
}
