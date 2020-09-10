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
    public float tileCrossTime = 0.3f;
    bool moving = false;
    float timeStart;
    private int _pathMove;
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p01;

    // Start is called before the first frame update
    void Start()
    {
        defaultMat = GetComponent<MeshRenderer>().material;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UnitSelected()
    {
        GetComponent<MeshRenderer>().material = selectedMat;
        selected = true;
        currentTile = MapGrid.Instance.TileFromPosition(transform.position);
    }

    public void UnitDeselected()
    {
        GetComponent<MeshRenderer>().material = defaultMat;
        selected = false;
    }

    public void BeginMovement(List<Tile> path)
    {
        moving = true;
        
        StartCoroutine(Move(path));
    }

    IEnumerator Move(List<Tile> path)
    {
        _pathMove = 0;
        foreach(Tile tile in path)
        {
            if(_pathMove != path.Count - 1)
            {
                timeStart = Time.time;
                moving = true;
                p0 = tile.transform.position;
                p0 = new Vector3(p0.x, transform.position.y, p0.z);
                p1 = path[_pathMove + 1].transform.position;
                p1 = new Vector3(p1.x, transform.position.y, p1.z);
                print(currentTile);
            }
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
            _pathMove++;
        }

        
    }
}
