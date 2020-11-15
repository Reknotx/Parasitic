using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
    GameObject gridline = null;

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
        MeshRenderer renderer = grid.GetComponent<MeshRenderer>();
        //DestroyImmediate(renderer);
        renderer.enabled = false;
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

    public void GenerateGridlines()
    {
        gridline = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Misc/Gridline.prefab", typeof(GameObject)) as GameObject;
        print(gridline.name);
        GameObject gridlines;
        MapGrid mapGrid = GetComponent<MapGrid>();
        Tile[,] tiles = mapGrid.GetTiles(columns, rows);

        if (transform.Find("Gridlines"))
        {
            gridlines = transform.Find("Gridlines").gameObject;
            DestroyImmediate(gridlines);
        }
        gridlines = new GameObject("Gridlines");
        gridlines.transform.parent = this.transform;
        Transform ColumnLines = new GameObject("ColumnLines").transform;
        ColumnLines.transform.parent = gridlines.transform;

        float lastElevation;
        float currentElevation;
        for (int c = 0; c <= columns; c++)
        {
            List<Vector3> rightPoints = GetPoints(tiles, c, true, mapGrid.tileHeight);
            List<Vector3> leftPoints = GetPoints(tiles, c, true, mapGrid.tileHeight, true);
            GameObject line = Instantiate(gridline, ColumnLines);
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

            lineRenderer.positionCount = rightPoints.Count;
            lineRenderer.SetPositions(rightPoints.ToArray());
            line.name = "column " + c + " right";
            if(leftPoints.Count > 0 && leftPoints != rightPoints)
            {
                line = Instantiate(gridline, ColumnLines);
                lineRenderer = line.GetComponent<LineRenderer>();
                lineRenderer.positionCount = leftPoints.Count;
                lineRenderer.SetPositions(leftPoints.ToArray());
                line.name = "column " + c + " left";
            }
        }
        Transform RowLines = new GameObject("RowLines").transform;
        RowLines.transform.parent = gridlines.transform;
        for (int r = 0; r <= rows; r++)
        {
            List<Vector3> upperPoints = GetPoints(tiles, r, false, mapGrid.tileHeight);
            List<Vector3> lowerPoints = GetPoints(tiles, r, false, mapGrid.tileHeight, true);
            GameObject line = Instantiate(gridline, RowLines);
            LineRenderer lineRenderer = line.GetComponent<LineRenderer>();

            lineRenderer.positionCount = upperPoints.Count;
            lineRenderer.SetPositions(upperPoints.ToArray());
            line.name = "row " + r + " up";
            if (lowerPoints.Count > 0 && lowerPoints != upperPoints)
            {
                line = Instantiate(gridline, RowLines);
                lineRenderer = line.GetComponent<LineRenderer>();
                lineRenderer.positionCount = lowerPoints.Count;
                lineRenderer.SetPositions(lowerPoints.ToArray());
                line.name = "row " + r + " down";
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tiles">array of tiles</param>
    /// <param name="l">line index</param>
    /// <param name="column">is line a column or row</param>
    /// <param name="height">hight of tile</param>
    /// <param name="secondPass">is this the second pass for a line</param>
    /// <returns>list of line points</returns>
    List <Vector3> GetPoints(Tile[,] tiles, int l, bool column, float height, bool secondPass = false)
    {
        List<Vector3> points = new List<Vector3>();
        float lastElevation;
        float currentElevation;
        if(secondPass && l == 0)
        {
            return points;
        }
        if (column)
        {
            int c = l;
            if (c == columns)
            {
                secondPass = true;
                c--;
            }
            else if (secondPass)
            {
                c--;
            }
            //add 1 elevation if slope is facing down, left if not second pass and right if second pass
            lastElevation = tiles[c, 0].level * height + 
                (tiles[c, 0].slope && ((tiles[c, 0].facing == Dir.down || (!secondPass && tiles[c, 0].facing == Dir.left || secondPass && tiles[c, 0].facing == Dir.right))) ? height : 0);
            points.Add(new Vector3(l * gridSize, gridHeight + lastElevation, 0));
            for (int r = 1; r < rows; r++)
            {

                currentElevation = tiles[c, r].level * height;
                if ((!secondPass && tiles[c, r].slope && tiles[c, r].facing == Dir.left) || (secondPass && tiles[c, r].slope && tiles[c, r].facing == Dir.right))
                {
                    currentElevation += height;
                }
                if (currentElevation != lastElevation)
                {
                    //if the previous tile is a slope facing up don't make it a cliff
                    if ( !(tiles[c, r - 1].slope && tiles[c, r - 1].facing == Dir.up))
                    {
                        //adds a point that is at the previous elevation but on the current position
                        points.Add(new Vector3(l * gridSize, gridHeight + lastElevation, r * gridSize));
                    }
                    //if tile is a not a slope facing down don't make it a cliff
                    if (!(tiles[c, r].slope && tiles[c, r].facing == Dir.down))
                    {
                        //adds a point that is the current elevation but at the previous postion
                        points.Add(new Vector3(l * gridSize, gridHeight + currentElevation, r * gridSize));
                    }
                }
                else
                {
                    points.Add(new Vector3(l * gridSize, gridHeight + currentElevation, r * gridSize));
                }
                lastElevation = currentElevation;
            }
            Tile lastTile = tiles[c, rows - 1];
            lastElevation = lastTile.level * height +
                (lastTile.slope && ((lastTile.facing == Dir.up || (!secondPass && lastTile.facing == Dir.left || secondPass && lastTile.facing == Dir.right))) ? height : 0);
            points.Add(new Vector3(l * gridSize, gridHeight + lastElevation, rows * gridSize));
        }
        else
        {
            int r = l;
            if (r == rows)
            {
                secondPass = true;
                r--;
            }
            else if (secondPass)
            {
                r--;
            }
            //add 1 elevation if slope is facing down, left if not second pass and right if second pass
            lastElevation = tiles[0, r].level * height +
                (tiles[0, r].slope && ((tiles[0, r].facing == Dir.left || (!secondPass && tiles[0, r].facing == Dir.down || secondPass && tiles[0, r].facing == Dir.up))) ? height : 0);
            points.Add(new Vector3(0, gridHeight + lastElevation, l * gridSize));
            for (int c = 1; c < columns; c++)
            {

                currentElevation = tiles[c, r].level * height;
                if ((!secondPass && tiles[c, r].slope && tiles[c, r].facing == Dir.down) || (secondPass && tiles[c, r].slope && tiles[c, r].facing == Dir.up))
                {
                    currentElevation += height;
                }
                if (currentElevation != lastElevation)
                {
                    if(!(tiles[c - 1, r].slope && tiles[c - 1, r].facing == Dir.right))
                    {
                        points.Add(new Vector3(c * gridSize, gridHeight + lastElevation, l * gridSize));
                    }
                    //if tile is a not a slope facing down and the last tile was not a slop facing up
                    if (!(tiles[c, r].slope && tiles[c, r].facing == Dir.left))
                    {
                        points.Add(new Vector3(c * gridSize, gridHeight + currentElevation, l * gridSize));
                    }
                }
                else
                {
                    points.Add(new Vector3(c * gridSize, gridHeight + currentElevation, l * gridSize));
                }
                lastElevation = currentElevation;
            }
            Tile lastTile = tiles[columns - 1, r];
            lastElevation = lastTile.level * height +
                (lastTile.slope && ((lastTile.facing == Dir.right || (!secondPass && lastTile.facing == Dir.down || secondPass && lastTile.facing == Dir.up))) ? height : 0);
            points.Add(new Vector3(columns * gridSize, gridHeight + lastElevation, l * gridSize));
        }
        return points;
    }

    float GetLineElevation(Tile[,] tiles, int w, int h, bool row)
    {
        if((w - 1 < 0 && row) || w >= columns)
        {
            return (w - 1 < 0 ? tiles[w, h].level : tiles[w - 1, h].level);
        }
        else if((h - 1 < 0 && !row) || h >= rows)
        {
            return (h - 1 < 0 ? tiles[w, h].level : tiles[w, h - 1].level);
        }
        else if (row)
        {
            return (tiles[w, h].level > tiles[w - 1, h].level ? tiles[w, h].level : tiles[w - 1, h].level);
        }
        else
        {
            return (tiles[w, h].level > tiles[w, h - 1].level ? tiles[w, h].level : tiles[w, h - 1].level);
        }
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
