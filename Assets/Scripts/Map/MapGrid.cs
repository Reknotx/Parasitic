//Ryan Dangerfield
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGrid : MonoBehaviour
{

    public static MapGrid Instance;
    public Tile[,] grid;
    
    //X-axis
    [HideInInspector]
    public int columns = 10;
    //Z-axis
    [HideInInspector]
    public int rows = 10;
    [HideInInspector]
    public float tileSize = 1;
    public Mesh gizmoMesh;
    public bool drawGizmoMesh = true;
    List<Tile> path;

    //make singlton
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
        grid = new Tile[columns, rows];
        //find tile fill tile array
        GetTiles();
    }

    void Start()
    {

    }

    private void Update()
    {
        //FindPath(TileFromPosition(start.transform.position), TileFromPosition(end.transform.position));
        //RetracePath(TileFromPosition(start.transform.position), TileFromPosition(end.transform.position));
    }

    //fills grid array with tiles in the scene
    public void GetTiles()
    {
        Transform tiles = transform.Find("Tiles");
        int z = 0;
        int x = 0;
        if (tiles.childCount == 0)
        {
            return;
        }
        else
        {
            
            foreach (Transform row in tiles.transform)
            {

                foreach(Transform column in row.transform)
                {
                    //add Tile to array
                    grid[x, z] = column.GetComponent<Tile>();
                    //set variable of tiles grid position in each tile
                    grid[x, z].gridPosition = new Vector2(x, z);
                    x++;
                    
                }
                x = 0;
                z++;
            }
        }
    }

    public Tile TileFromPosition(Vector3 pos)
    {
        float gridWidth = tileSize * columns;
        float gridHeight = tileSize * rows;
        float percentX = pos.x / gridWidth;
        float percentZ = pos.z / gridHeight;
        //if the position is outside the bounds of the map return null
        if (percentX < 0 || percentZ < 0 || percentX > 1 || percentZ > 1)
        {
            return null;
        }
        else
        {
            //print((columns - 1) * percentX);
            int posX = (int)((columns) * percentX);
            int posZ = (int)((rows) * percentZ);
            return grid[posX, posZ];
        }
    }

    //A* algorithm
    public List<Tile> FindPath(Tile startTile, Tile endTile, bool ignoreOccupied = false, bool ignoreEnd = false, ActionShape actionShape = ActionShape.Flood)
    {
        //nodes that need to be evaluated
        List<Tile> frontier = new List<Tile>();
        //nodes that have been evaluated
        HashSet<Tile> explored = new HashSet<Tile>();
        frontier.Add(startTile);
        //will loop until every possible pathway is exhausted
        startTile.parent = null;
        foreach (Tile tile in grid)
        {
            tile.gCost = 0;
        }
        while (frontier.Count > 0)
        {
            Tile currentTile = frontier[0];
            //sets currentTile to tile with lowest fCost (or lowest hCost if fCost is the same) in the frontier
            for (int i = 1; i < frontier.Count; i++)
            {
                if (frontier[i].fCost < currentTile.fCost || (frontier[i].fCost == currentTile.fCost && frontier[i].hCost < currentTile.hCost))
                {
                    currentTile = frontier[i];
                }
            }

            frontier.Remove(currentTile);
            explored.Add(currentTile);
            
            if (currentTile == endTile)
            {
                //end tile has been reached
                return RetracePath(startTile,endTile);
            }
            foreach (Tile neighbor in GetNeighbors(currentTile, actionShape))
            {
                //skip tile if it is not valid to move through, has already been explored, or is currently occupied 
                if(!neighbor.movementTile || explored.Contains(neighbor) || (neighbor.occupied && !ignoreOccupied))
                {
                    if (!(ignoreEnd && (endTile == neighbor)))
                        continue;
                }
                int currentCost = currentTile.gCost + GetDistanceCost(currentTile, neighbor, actionShape);
                //if the current cost of moving to the tile is lower than the previous set it to the new cost
                if(currentCost < neighbor.gCost || !frontier.Contains(neighbor))
                {
                    neighbor.gCost = currentCost;
                    neighbor.hCost = GetDistanceCost(neighbor, endTile, actionShape);
                    neighbor.parent = currentTile;
                    if (!explored.Contains(neighbor))
                    {
                        frontier.Add(neighbor);
                    }
                }
            }
        }
        //print("frontier explored");
        //failed to find a path that connects the points
        return null;
    }

    private List<Tile> RetracePath(Tile startTile, Tile endTile)
    {
        path = new List<Tile>();
        Tile currentTile = endTile;
        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.parent;
        }
        path.Reverse();
        return path;
    }

    public int GetDistanceCost(Tile tileA, Tile tileB, ActionShape actionShape)
    {
        int distZ = (int)Mathf.Abs(tileA.gridPosition.y - tileB.gridPosition.y);
        int distX = (int)Mathf.Abs(tileA.gridPosition.x - tileB.gridPosition.x);
        if (actionShape != ActionShape.Square)
        {
            return 10 * (distZ + distX);
        }
        else
        {
            if (distX > distZ)
            {
                return 10 * distZ + 10 * (distX - distZ);
            }
            else
            {
                return 10 * distX + 10 * (distZ - distX);
            }
        }
        
    }

    //returns list of tiles neighboring tile passed in
    public List<Tile> GetNeighbors(Tile tile, ActionShape actionShape = ActionShape.Flood)
    {
        List<Tile> neighbors = new List<Tile>();
        int tilePosX = (int)tile.gridPosition.x;
        int tilePosZ = (int)tile.gridPosition.y;
        if (actionShape != ActionShape.Cross || tile.parent == null || tile.parent.gridPosition.x == tilePosX)
        {
            //check above
            if (tilePosZ > 0)
            {
                neighbors.Add(grid[tilePosX, tilePosZ - 1]);
                if(actionShape == ActionShape.Square)
                {
                    //check left
                    if (tilePosX > 0)
                    {
                        neighbors.Add(grid[tilePosX - 1, tilePosZ - 1]);
                    }
                    //check right
                    if (tilePosX < columns - 1)
                    {
                        neighbors.Add(grid[tilePosX + 1, tilePosZ - 1]);
                    }
                }
            }
            //check below
            if (tilePosZ < rows - 1)
            {
                neighbors.Add(grid[tilePosX, tilePosZ + 1]);
                if (actionShape == ActionShape.Square)
                {
                    //check left
                    if (tilePosX > 0)
                    {
                        neighbors.Add(grid[tilePosX - 1, tilePosZ + 1]);
                    }
                    //check right
                    if (tilePosX < columns - 1)
                    {
                        neighbors.Add(grid[tilePosX + 1, tilePosZ + 1]);
                    }
                }
            }
        }
        if (actionShape != ActionShape.Cross || tile.parent == null || tile.parent.gridPosition.y == tilePosZ)
        {
            //check left
            if (tilePosX > 0)
            {
                neighbors.Add(grid[tilePosX - 1, tilePosZ]);
            }
            //check right
            if (tilePosX < columns - 1)
            {
                neighbors.Add(grid[tilePosX + 1, tilePosZ]);
            }
        }
        return neighbors;
    }


    //Realized while making this that is was actually not ideal for what we need to accomplish
    //public Tile NearestOpenTile(Tile startTile, Tile targetTile)
    //{
    //    List<Tile> occupied = new List<Tile>();
    //    List<Tile> open = new List<Tile>();
    //    HashSet<Tile> explored = new HashSet<Tile>();
    //    Tile currentTile = targetTile;
    //    occupied.Add(currentTile);
    //    if (!targetTile.occupied && targetTile.movementTile)
    //    {
    //        return targetTile;
    //    }

    //    while (occupied.Count > 0)
    //    {
    //        occupied.Remove(currentTile);
    //        explored.Add(currentTile);
    //        foreach (Tile neighbor in GetNeighbors(currentTile))
    //        {
    //            if (!neighbor.movementTile || explored.Contains(neighbor))
    //            {
    //                continue;
    //            }
    //            else if (neighbor.occupied)
    //            {
    //                occupied.Add(neighbor);
    //            }
    //            else
    //            {
    //                open.Add(neighbor);
    //            }
    //        }
    //        if(open.Count > 0)
    //        {
    //            int shortestDist = -1;
    //            Tile shortestTile = open[0];
    //            if(open.Count == 1)
    //            {
    //                return shortestTile;
    //            }
    //            foreach (Tile tile in open)
    //            {
    //                int pathDist = FindPath(tile, startTile).Count;
    //                if (shortestDist > pathDist || shortestDist == -1)
    //                {
    //                    shortestDist = pathDist;
    //                    shortestTile = tile;
    //                }
    //            }
    //            return shortestTile;
    //        }
    //        else()
    //    }

    //}

    //Breadth first search
    public bool[,] FindTilesInRange(Tile startTile, int range, bool ignoreOccupied = false, ActionShape actionShape = ActionShape.Flood)
    {
        List<Tile> frontier = new List<Tile>();
        HashSet<Tile> explored = new HashSet<Tile>();
        Tile currentTile = startTile;
        frontier.Add(startTile);
        bool[,] inRange = new bool[columns, rows];
        inRange[(int)startTile.gridPosition.x, (int)startTile.gridPosition.y] = true;
        startTile.parent = null;
        foreach (Tile tile in grid)
        {
            tile.gCost = 0;
        }
        while (frontier.Count > 0)
        {
            currentTile = frontier[0];
            frontier.Remove(currentTile);
            explored.Add(currentTile);
            foreach (Tile neighbor in GetNeighbors(currentTile, actionShape))
            {
                //skip tile if it is not valid to move through, has already been explored, or is currently occupied 
                if ((neighbor.movementTile || (ignoreOccupied && !neighbor.blocksLOS)) && !explored.Contains(neighbor) && (!neighbor.occupied || ignoreOccupied))
                {

                    int currentCost = currentTile.gCost + GetDistanceCost(currentTile, neighbor, actionShape);
                    if (currentCost < neighbor.gCost || !frontier.Contains(neighbor))
                    {

                        neighbor.gCost = currentCost;
                        if(currentCost <= range * 10)
                        {
                            neighbor.parent = currentTile;
                            frontier.Add(neighbor);
                            inRange[(int)neighbor.gridPosition.x, (int)neighbor.gridPosition.y] = true;
                        }
                    }
                }
            }
        }

        return inRange;
    }

    //TODO: limmit search to tiles in range around selected unit
    public void DrawBoarder(bool[,] inRange, ref LineRenderer boarder,float height = 0.25f)
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 pos;
        Vector3 lowPos;
        Vector3 highPos;
        //up right search
        for (int y = 0; y< inRange.GetLength(1); y++)
        {
            for (int x = 0; x < inRange.GetLength(0); x++)
            {
                if (inRange[x, y])
                {
                    pos = grid[x, y].transform.position;
                    lowPos = new Vector3(pos.x - tileSize / 2, height, pos.z - tileSize / 2);
                    highPos = new Vector3(pos.x - tileSize / 2, height, pos.z + tileSize / 2);
                    if (!points.Contains(lowPos))
                    {
                        points.Add(lowPos);
                    }
                    if (!points.Contains(highPos))
                    {
                        points.Add(highPos);
                    }
                    break;
                }
            }
        }
        //down left search
        for (int y = inRange.GetLength(1)-1; y >= 0; y--)
        {
            for (int x = inRange.GetLength(0)-1; x >= 0; x--)
            {
                if (inRange[x, y])
                {
                    pos = grid[x, y].transform.position;
                    lowPos = new Vector3(pos.x + tileSize / 2, height, pos.z - tileSize / 2);
                    highPos = new Vector3(pos.x + tileSize / 2, height, pos.z + tileSize / 2);
                    if (!points.Contains(highPos))
                    {
                        points.Add(highPos);
                    }

                    if (!points.Contains(lowPos))
                    {
                        points.Add(lowPos);
                    }
                    break;
                }
            }
        }

        boarder.positionCount = points.Count;
        boarder.SetPositions(points.ToArray());
    }

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
            foreach (Transform row in tiles.transform)
            {
                //number of columns only need to be counted once
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

    //returns the greater int or int a if equal 
    private int GreaterInt(int a, int b)
    {
        if (a >= b)
            return a;
        else
            return b;
    }


    private void OnDrawGizmos()
    {
        Vector3 gizmoPos = new Vector3(columns * tileSize * 0.5f, -0.1f, rows * tileSize * 0.5f);
        Vector3 gizmoScale = new Vector3(columns, 0f, rows) * tileSize * 0.1f;
        Gizmos.color = Color.green;
        if(drawGizmoMesh)
            Gizmos.DrawMesh(gizmoMesh,gizmoPos,Quaternion.identity,gizmoScale);
        /*if(grid != null) {
            foreach (Tile t in grid)
            {
                Gizmos.color = Color.cyan;
                if(path != null)
                {
                    if (path.Contains(t))
                    {
                        Gizmos.DrawCube(t.gameObject.transform.position, Vector3.one * tileSize);
                    }
                }
            }
        }*/
    }
}



