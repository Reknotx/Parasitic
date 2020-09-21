using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    //X-axis
    [Range(1, 150)]
    public int columns = 10;
    //Z-axis
    [Range(1, 150)]
    public int rows = 10;
    public float gridHeight = 0.5f;
    public float gridSize = 1;
    public Mesh gizmoMesh;
    public bool drawGizmoMesh = true;
    public GameObject grid;
    public MeshFilter meshFilter;

    public Vector2 TileCount()
    {
        Transform tiles = transform.Find("Tiles");
        int z = 0;
        int x = 0;
        if (tiles.childCount == 0)
        {
            return Vector2.zero;
        }
        else
        {
            print("tiles has children");
            foreach (Transform row in tiles.transform)
            {
                if (x == 0)
                {
                    foreach (Transform column in row.transform)
                    {
                        x++;
                    }
                }
                z++;
            }
        }
        return new Vector2(x, z);
    }

    public void MakeGrid()
    {
        MapGrid mapGrid = GetComponent<MapGrid>();
        mapGrid.columns = columns;
        mapGrid.rows = rows;
        mapGrid.tileSize = gridSize;
        GridMesh gridMesh = GridGenerator.GenerateGrid(columns, rows, gridHeight, gridSize);
        Vector2 currentSize = TileCount();
        Debug.Log(currentSize);
        meshFilter.sharedMesh = gridMesh.CreateMesh();
        MeshCollider meshCollider = grid.GetComponent<MeshCollider>();
        if (meshCollider)
        {
            DestroyImmediate(meshCollider);
        }
        grid.AddComponent<MeshCollider>();
        Transform tiles = transform.Find("Tiles");
        GameObject currentRow;
        GameObject currentTile;
        int length = GreaterInt(rows, (int)currentSize.y);
        int width = GreaterInt(columns, (int)currentSize.x);
        for (int z = 0; z < length; z++)
        {
            //if there are more rows are needed than currently exist spawn the new row
            if (currentSize.y < z + 1)
            {
                currentRow = new GameObject("Row" + z.ToString());
                currentRow.transform.parent = tiles;
                for (int x = 0; x < columns; x++)
                {
                    currentTile = new GameObject("Tile" + x.ToString() + "," + z.ToString());
                    currentTile.AddComponent<Tile>();
                    currentTile.transform.parent = currentRow.transform;
                    currentTile.transform.position = new Vector3(x * gridSize + gridSize / 2, 0, z * gridSize + gridSize / 2);
                }
            }
            //if more rows currently exist than are needed delete the rows
            else if (z + 1 > rows)
            {
                currentRow = tiles.Find("Row" + z.ToString()).gameObject;
                DestroyImmediate(currentRow);
            }
            else
            {
                currentRow = tiles.Find("Row" + z.ToString()).gameObject;
                for (int x = 0; x < width; x++)
                {
                    //if more columns are needed than currently exist spawn the new row
                    if (currentSize.x < x + 1)
                    {
                        currentTile = new GameObject("Tile" + x.ToString() + "," + z.ToString());
                        currentTile.AddComponent<Tile>();
                        currentTile.transform.parent = currentRow.transform;
                        currentTile.transform.position = new Vector3(x * gridSize + gridSize / 2, 0, z * gridSize + gridSize / 2);
                    }
                    //if more columns exist than are needed delete the column
                    else if (x + 1 > columns)
                    {
                        currentTile = currentRow.transform.Find("Tile" + x.ToString() + "," + z.ToString()).gameObject;
                        DestroyImmediate(currentTile);
                    }
                    //update existing tile positions
                    else
                    {
                        currentTile = currentRow.transform.Find("Tile" + x.ToString() + "," + z.ToString()).gameObject;
                        currentTile.transform.position = new Vector3(x * gridSize + gridSize / 2, 0, z * gridSize + gridSize / 2);
                    }
                }
            }
        }
        drawGizmoMesh = false;
    }

    private int GreaterInt(int a, int b)
    {
        if (a >= b)
            return a;
        else
            return b;
    }


    private void OnDrawGizmos()
    {
        Vector3 gizmoPos = new Vector3(columns * gridSize * 0.5f, -0.1f, rows * gridSize * 0.5f);
        Vector3 gizmoScale = new Vector3(columns, 0f, rows) * gridSize * 0.1f;
        Gizmos.color = Color.green;
        if (drawGizmoMesh)
            Gizmos.DrawMesh(gizmoMesh, gizmoPos, Quaternion.identity, gizmoScale);
    }
}
