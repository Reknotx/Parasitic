//Ryan Dangerfield
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Dir { left, up, right, down }

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
    public float tileHeight = 1;
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
        //if the position is outside the bounds of the map clam the result
        if (percentX < 0 || percentZ < 0 || percentX > 1 || percentZ > 1)
        {
            //Debug.Log(percentX + " " + percentZ);
            Mathf.Clamp01(percentX);
            Mathf.Clamp01(percentZ);
        }
        
        //print((columns - 1) * percentX);
        int posX = (int)((columns) * percentX);
        int posZ = (int)((rows) * percentZ);
        return grid[posX, posZ];
        
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
                if (!ignoreOccupied && !ValidSlopeMovement(currentTile, neighbor))
                {
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
                    if (!ignoreOccupied && !ValidSlopeMovement(currentTile, neighbor))
                    {
                        continue;
                    }
                    
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
        if (actionShape != ActionShape.Flood)
        {
            for (int y = 0; y < inRange.GetLength(1); y++)
            {
                for (int x = 0; x < inRange.GetLength(0); x++)
                {
                    //ignore if not in range
                    if (!inRange[x, y])
                    {
                        continue;
                    }
                    //if in range check if it can be seen
                    else
                    {
                        inRange[x, y] = InLineOfSight((int)startTile.gridPosition.x, (int)startTile.gridPosition.y, x, y);
                    }
                }
            }
        }
        return inRange;
    }

    
    bool ValidSlopeMovement(Tile currentTile, Tile neighbor)
    {
        if (currentTile.slope)
        {
            if (neighbor.slope)
            {
                //if neighbor is a slope we will assume it connects for now
            }
            else if (currentTile.facing == Dir.up || currentTile.facing == Dir.down)
            {
                int up = (int)currentTile.gridPosition.y + 1;
                int down = (int)currentTile.gridPosition.y - 1;
                if (!(up == neighbor.gridPosition.y || down == neighbor.gridPosition.y))
                {
                    return false;
                }
            }
            else
            {
                int right = (int)currentTile.gridPosition.x + 1;
                int left = (int)currentTile.gridPosition.x - 1;
                if (!(right == neighbor.gridPosition.x || left == neighbor.gridPosition.x))
                {
                    return false;
                }
            }
        }
        else if (neighbor.slope)
        {
            if (neighbor.facing == Dir.up || neighbor.facing == Dir.down)
            {
                int up = (int)neighbor.gridPosition.y + 1;
                int down = (int)neighbor.gridPosition.y - 1;
                if (!(up == currentTile.gridPosition.y || down == currentTile.gridPosition.y))
                {
                    return false;
                }
            }
            else
            {
                int right = (int)neighbor.gridPosition.x + 1;
                int left = (int)neighbor.gridPosition.x - 1;
                if (!(right == currentTile.gridPosition.x || left == currentTile.gridPosition.x))
                {
                    return false;
                }
            }
        }
        else if (neighbor.level != currentTile.level)
        {
            return false;
        }
        return true;
    }

    //TODO: limmit search to tiles in range around selected unit
    public void DrawBoarder(bool[,] inRange, ref LineRenderer boarder,float height = 0.25f)
    {
        List<Vector3> points = new List<Vector3>();
        //List<Tile> boarderPath = new List<Tile>();
        Tile lastTile = null;
        Tile currentTile = null;
        Vector3 pos = Vector3.zero;
        Vector3 startPos = Vector3.zero;
        Vector3 hitPoint;
        bool startFound = false;
        bool endFound = false;
        //current position on boarder
        int xCoord = 0;
        int yCoord = 0;
        Dir facing = Dir.left;

        //find start position (bottom leftmost in range tile)
        for (int y = 0; y < inRange.GetLength(1); y++)
        {
            for (int x = 0; x < inRange.GetLength(0); x++)
            {
                if (inRange[x, y])
                {
                    pos = grid[x, y].ElevatedPos();
                    currentTile = grid[x, y];
                    
                    startPos = new Vector3(pos.x - tileSize / 2, height + pos.y, pos.z - tileSize / 2);
                    if ((currentTile.slope && currentTile.facing == Dir.down || currentTile.facing == Dir.left))
                    {
                        startPos = new Vector3(startPos.x, startPos.y + tileHeight, startPos.z);
                    }
                    //make start current position
                    xCoord = x;
                    yCoord = y;
                    points.Add(startPos);
                    lastTile = grid[xCoord, yCoord];
                    startFound = true;
                    break;
                }
                
            }
            if (startFound) break;
        }
        //travel around boarder until start position is encountered again
        //if tile ahead is not in range boarder position is added to the list and the check rotates right
        //if tile ahead is in range move that tile and rotate left
        int loops = 0;
        
        while (startFound && !endFound)
        {
            loops++;
            if (loops > 1000) break;
            lastTile = currentTile;
            currentTile = grid[xCoord, yCoord];

            switch (facing)
            {
                case Dir.left:
                    //valid space ahead
                    if (xCoord-1 >= 0 && inRange[xCoord-1, yCoord])
                    {
                        xCoord--;
                        //move to tile
                        pos = grid[xCoord, yCoord].ElevatedPos();
                        //rotate left
                        facing = Dir.down;
                        
                    }
                    //out of range, therefore left boarder
                    else
                    {
                        //rotate right
                        facing = Dir.up;
                        //add point to list
                        hitPoint = new Vector3(pos.x - tileSize / 2, height + pos.y, pos.z + tileSize / 2);
                        if ((currentTile.slope && currentTile.facing == Dir.up || currentTile.facing == Dir.left))
                        {
                            Vector3 pastHit = new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count - 1].z);
                            if (currentTile.level != lastTile.level && points[points.Count - 1] != pastHit)
                            {
                                points.Add(pastHit);
                            }
                            hitPoint = new Vector3(hitPoint.x, hitPoint.y + tileHeight, hitPoint.z);
                        }
                        if (hitPoint != startPos)
                        {
                            if (currentTile.level != lastTile.level && ( !currentTile.slope))
                            {
                                points.Add(new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count-1].z));
                            }
                            else if((currentTile.slope || lastTile.slope) && !ValidSlopeMovement(lastTile, currentTile))
                            {
                                points.Add(new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count - 1].z));
                            }
                            points.Add(hitPoint);
                            lastTile = grid[xCoord, yCoord];
                        }
                        else
                        {
                            endFound = true;
                        }
                        
                    }
                    break;
                case Dir.up:
                    //valid space ahead
                    if (yCoord + 1 < inRange.GetLength(1) && inRange[xCoord, yCoord +1])
                    {
                        yCoord++;
                        //move to tile
                        pos = grid[xCoord, yCoord].ElevatedPos();
                        //rotate left
                        facing = Dir.left;

                    }
                    //out of range, therefore upper boarder
                    else
                    {
                        //rotate right
                        facing = Dir.right;
                        //add pint to list
                        hitPoint = new Vector3(pos.x + tileSize / 2, height + pos.y, pos.z + tileSize / 2);
                        if ((currentTile.slope && currentTile.facing == Dir.up || currentTile.facing == Dir.right))
                        {
                            Vector3 pastHit = new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count - 1].z);
                            if (currentTile.level != lastTile.level && points[points.Count - 1] != pastHit)
                            {
                                points.Add(pastHit);
                            }
                            hitPoint = new Vector3(hitPoint.x, hitPoint.y + tileHeight, hitPoint.z);
                        }
                        if (hitPoint != startPos)
                        {
                            if (currentTile.level != lastTile.level && (!currentTile.slope))
                            {
                                points.Add(new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count - 1].z));
                            }
                            else if ((currentTile.slope || lastTile.slope) && !ValidSlopeMovement(lastTile, currentTile))
                            {
                                points.Add(new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count - 1].z));
                            }
                            points.Add(hitPoint);
                            //print("last: " + lastTile.gridPosition + ", current: " + currentTile.gridPosition + ", valid move: " + ValidSlopeMovement(lastTile, currentTile));
                            lastTile = grid[xCoord, yCoord];
                        }
                        else
                        {
                            endFound = true;
                        }

                    }
                    break;
                case Dir.right:
                    //valid space ahead
                    if (xCoord +1 < inRange.GetLength(0) && inRange[xCoord + 1, yCoord])
                    {
                        xCoord++;
                        //move to tile
                        pos = grid[xCoord, yCoord].ElevatedPos();
                        //rotate left
                        facing = Dir.up;

                    }
                    //out of range, therefore right boarder
                    else
                    {
                        //rotate right
                        facing = Dir.down;
                        //add pint to list
                        hitPoint = new Vector3(pos.x + tileSize / 2, height + pos.y, pos.z - tileSize / 2);
                        if((currentTile.slope && currentTile.facing == Dir.right || currentTile.facing == Dir.down))
                        {
                            Vector3 pastHit = new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count - 1].z);
                            if (currentTile.level != lastTile.level && points[points.Count - 1] != pastHit)
                            {
                                points.Add(pastHit);
                            }
                            hitPoint = new Vector3(hitPoint.x, hitPoint.y + tileHeight, hitPoint.z);
                        }
                        if (hitPoint != startPos)
                        {
                            if (currentTile.level != lastTile.level && (!currentTile.slope))
                            {
                                points.Add(new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count - 1].z));
                            }
                            else if ((currentTile.slope || lastTile.slope) && !ValidSlopeMovement(lastTile, currentTile))
                            {
                                points.Add(new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count - 1].z));
                            }
                            points.Add(hitPoint);
                            lastTile = grid[xCoord, yCoord];
                        }
                        else
                        {
                            endFound = true;
                        }

                    }
                    break;
                case Dir.down:
                    //valid space ahead
                    if (yCoord - 1 >= 0 && inRange[xCoord, yCoord -1])
                    {
                        yCoord--;
                        //move to tile
                        pos = grid[xCoord, yCoord].ElevatedPos();
                        //rotate left
                        facing = Dir.right;


                    }
                    //out of range, therefore bottom boarder
                    else
                    {
                        //rotate right
                        facing = Dir.left;
                        //add pint to list
                        hitPoint = new Vector3(pos.x - tileSize / 2, height + pos.y, pos.z - tileSize / 2);
                        if ((currentTile.slope && currentTile.facing == Dir.down || currentTile.facing == Dir.left))
                        {
                            Vector3 pastHit = new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count - 1].z);
                            if (currentTile.level != lastTile.level && points[points.Count - 1] != pastHit)
                            {
                                points.Add(pastHit);
                            }
                            hitPoint = new Vector3(hitPoint.x, hitPoint.y + tileHeight, hitPoint.z);
                        }
                        if (hitPoint != startPos)
                        {
                            if (currentTile.level != lastTile.level && (!currentTile.slope))
                            {
                                points.Add(new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count - 1].z));
                            }
                            else if ((currentTile.slope || lastTile.slope) && !ValidSlopeMovement(lastTile, currentTile))
                            {
                                points.Add(new Vector3(points[points.Count - 1].x, hitPoint.y, points[points.Count - 1].z));
                            }
                            points.Add(hitPoint);
                            lastTile = grid[xCoord, yCoord];
                        }
                        else
                        {
                            endFound = true;
                        }

                    }
                    break;
                default:
                    break;
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

    //Bresenham's Line drawing algorithm
    bool InLineOfSight(int x1, int y1, int x2, int y2)
    {
        bool steep = (Mathf.Abs(y2 - y1) > Mathf.Abs(x2 - x1));
        if (steep)
        {
            Swap<int>(ref x1, ref y1);
            Swap<int>(ref x2, ref y2);
        }
        if (x1 > x2)
        {
            Swap<int>(ref x1, ref x2);
            Swap<int>(ref y1, ref y2);
        }
        int y = y1;
        int dx = x2 - x1;
        int dy = Mathf.Abs(y2 - y1);
        int err = dx / 2;
        int ystep = 1;
        if(y1 > y2)
        {
            ystep = -1;
        }
        for (int x = x1; x<= x2; ++x)
        {
            if (steep)
            {
                if (grid[y, x].blocksLOS)
                {
                    return false;
                }
            }
            else
            {
                if (grid[x,y].blocksLOS)
                {
                    return false;
                }
            }
            err -= dy;
            if(err < 0)
            {
                y += ystep;
                err += dx;
            }
        }
        return true;
    }

    public static void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp = lhs;
        lhs = rhs;
        rhs = temp;
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



