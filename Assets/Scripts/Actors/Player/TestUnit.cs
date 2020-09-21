//Ryan Dangerfield
//9/9/20
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestUnit : MonoBehaviour
{
    bool selected = false;
    Material defaultMat;
    public Material selectedMat;
    public Tile currentTile;
    //time it takes the player to move across a single tile
    public float tileCrossTime = 0.3f;
    bool moving = false;
    


    // Start is called before the first frame update
    void Start()
    {
        defaultMat = GetComponent<MeshRenderer>().material;
        currentTile = MapGrid.Instance.TileFromPosition(transform.position);
        currentTile.occupied = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UnitSelected()
    {
        GetComponent<MeshRenderer>().material = selectedMat;
        selected = true;
    }

    public void UnitDeselected()
    {
        GetComponent<MeshRenderer>().material = defaultMat;
        selected = false;
    }

    public void BeginMovement(List<Tile> path)
    {
        if (path != null)
        {
            StartCoroutine(Move(path));
        }
    }

    IEnumerator Move(List<Tile> path)
    {
        Vector3 p0;
        Vector3 p1;
        Vector3 p01;
        float timeStart;
        foreach (Tile tile in path)
        {

           
            timeStart = Time.time;
            moving = true;
            
            //get the position of the tile the unit is starting on
            p0 = currentTile.transform.position;
            

            //get the positon of the tile to move to
            p1 = tile.transform.position;

            // set the y position to be that of the moving unit
            p0 = new Vector3(p0.x, transform.position.y, p0.z);
            p1 = new Vector3(p1.x, transform.position.y, p1.z);

            //mark the starting tile as no longer occupied
            currentTile.occupied = false;
            //change the current tile to the tile being moved to
            currentTile = tile;
            //mark it as occupied
            currentTile.occupied = true;
            //interpolate between the two points
            while (moving)
            {
                float u = (Time.time - timeStart) / tileCrossTime;
                if (u >= 1)
                {
                    u = 1;
                    moving = false;
                }

                p01 = (1 - u) * p0 + u * p1;
                transform.position = p01;
                yield return new WaitForFixedUpdate();
            }
        }
    }
}
