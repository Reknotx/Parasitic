/**
 * Author: Chase O'Connor
 * 
 * Date: 9/4/2020
 * 
 * Brief: Implements the unique ranged enemy functionality, such as keeping
 * the spiker at maximum range.
 * 
 */

using System.Collections.Generic;
using UnityEngine;

public class Spiker : Enemy
{
    #region Combat Functions
    public override void Attack()
    {
        Debug.Log("Spiker Attack");
        base.Attack();
    }

    public override void Defend()
    {
        Debug.Log("Spiker Defend");
        base.Defend();
    }
    #endregion  

    public override void Move(List<Tile> path, bool bypassRangeCheck = false)
    {
        Debug.Log("Spiker Move");
        if (CheckIfInRangeOfTarget() == false)
        {
            base.Move(path);
        }
        else
        {
            // Perform the unique spiker functions.
            List<Tile> pathToMaxRange = FindOptimalMaxRangePos();
            base.Move(pathToMaxRange, true);
        }
    }

    private List<Tile> FindOptimalMaxRangePos()
    {
        Debug.Log("Finding optimal range");
        List<Tile> path = new List<Tile>();
        if (_currTarget == null)
        {
            FindNearestPlayer();
        }

        ///Perhaps find which direction is closer to the player?
        ///Like if we are closer in x and farther away in the Y then
        ///move away from the player in the X direction. 
        ///Vice versa for a situation when we are closer in the Y but farther 
        ///in the X
        
        CloserIn closerIn = FindDirCloserIn();

        DirToMove dirToMove = FindDirMoveIn(closerIn);

        Tile tempTile = currentTile;
        Tile[,] tempGrid = MapGrid.Instance.grid;

        Coord mapCoord = new Coord(this);
        Debug.Log(mapCoord.ToString());

        ///We are going to move one tile at a time until we are for sure are max range.
        ///After we move one tile we need to check to make sure that the tile we are going to
        ///is still going to be within the Spiker's attack range
        ///
        ///If we hit a wall, and we are not at max attack range we need to run some more 
        ///conditional tests.
        ///
        ///Is there a different direction we can move in that will keep us moving away from the player,
        ///and bring us into max attack range within our movement range. If so go in that direction.
        ///
        ///Run through the necessary directions that keep us at max attack range while also staying
        ///within move range.
        ///Then pass in that path. 
        ///
        
        for (int i = 0; i < MovementStat; i++)
        {
            ///Step 1. Adjust the coord on the map we are looking at by 1 in 
            ///the desired direction
            Debug.Log("Executing Step 1.");

            mapCoord.IncreaseCoordInDir(dirToMove);

            Debug.Log(mapCoord.ToString());

            ///Step 2. Gain a reference to the tile at that map coord as well as the neighbors
            ///around that tile that are within attack range.
            Debug.Log("Executing step 2.");
            if ((mapCoord.X > MapGrid.Instance.columns || mapCoord.X < 0)
                || (mapCoord.Y > MapGrid.Instance.rows || mapCoord.Y < 0))
            {
                break;
            }

            if (tempGrid[mapCoord.X, mapCoord.Y].movementTile)
            {
                tempTile = tempGrid[mapCoord.X, mapCoord.Y];
            }
            else
            {
                ///Make a turn
                ///
                ///Step 1. Since the next tile we would hit would be a wall
                ///we need to first move back once in the way we came from
                ///so we can start looking at turns.
                mapCoord.IncreaseCoordInOppositeDir(dirToMove);
                dirToMove = InspectBestTurnDirection(dirToMove);
                mapCoord.IncreaseCoordInDir(dirToMove);

                if ((mapCoord.X > MapGrid.Instance.columns || mapCoord.X < 0)
                || (mapCoord.Y > MapGrid.Instance.rows || mapCoord.Y < 0))
                {
                    break;
                }

                if (tempGrid[mapCoord.X, mapCoord.Y].movementTile)
                {
                    tempTile = tempGrid[mapCoord.X, mapCoord.Y];
                }
                else
                {
                    break;  
                }
                    
            }

            bool[,] neighbors = MapGrid.Instance.FindTilesInRange(tempTile, AttackRange, true);

            ///Step 3. Now run through all of the tiles that are within range of the proposed 
            ///tile and see if the player is within that range.
            Debug.Log("Executing step 3.");
            bool playerInRange = false;
            for (int x = 0; x < neighbors.GetLength(0); x++)
            {
                for (int y = 0; y < neighbors.GetLength(1); y++)
                {
                    if (!neighbors[x, y]) continue;

                    if (tempGrid[x, y].occupied && tempGrid[x, y].occupant is Player)
                    {
                        if (tempGrid[x, y].occupant == _currTarget)
                        {
                            playerInRange = true;
                            break;
                        }
                    }
                }

                if (playerInRange)
                {
                    break;
                }

            }

            ///Step 4. If the is indeed within range of the enemy at the proposed tile
            ///add it to the path. Else if they are not then break out of the loop as we 
            ///now know we have found the maximum range from the player.
            Debug.Log("Executing step 4.");
            if (playerInRange)
            {
                Debug.Log("Player is in range");
                path.Add(tempTile);
            }
            else
            {
                Debug.Log("Player is not in range");
                break;
            }


        }
        Debug.Log(path.Count);

        return path;
    }

    #region Find Move Direction
    /// <summary>
    /// Might need to change to include negative x and negative y
    /// </summary>
    enum CloserIn { NULL, X, Y }

    private CloserIn FindDirCloserIn()
    {
        CloserIn closerIn = CloserIn.NULL;

        Coord playerCoord = new Coord(_currTarget);
        Coord spikerCoord = new Coord(this);

        ///Refers to the gap between spiker and player in X and Y coords q
        int xDelta = Mathf.Abs(playerCoord.X - spikerCoord.X);
        int yDelta = Mathf.Abs(playerCoord.Y - spikerCoord.Y);

        Debug.Log("X Delta: " + xDelta);
        Debug.Log("Y Delta: " + yDelta);


        if (xDelta < yDelta)
        {
            //Meaning we are closer to the player in the X coord
            closerIn = CloserIn.X;
            Debug.Log("Closer in X");
        }
        else if (yDelta < xDelta)
        {
            //Meaning we are closer to the player in the Y coord
            closerIn = CloserIn.Y;
            Debug.Log("Closer in Y");
        }
        else
        {
            //Approaching here means we are the same distance away in
            //the x and y coords. meaning a diagonal line.
            int randSelection = Random.Range(0, 2);

            if (randSelection == 0)
            {
                closerIn = CloserIn.X;
            }
            else
            {
                closerIn = CloserIn.Y;
            }
        }

        return closerIn;
    }

    enum DirToMove { NULL, negX, posX, negY, posY }


    private DirToMove FindDirMoveIn(CloserIn closerIn)
    {
        Coord playerCoord = new Coord(_currTarget);
        Coord spikerCoord = new Coord(this);

        if (closerIn == CloserIn.X)
        {
            if (playerCoord.X > spikerCoord.X)
            {
                ///Meaning the player is to the right of the spiker
                ///Meaning the spiker needs to decrease their X pos
                return DirToMove.negX;
            }
            else
            {
                ///Meaning the player is to the left of the spiker.
                ///Meaning the spiker needs to increase their X Pos
                return DirToMove.posX;
            }
        }
        else
        {
            if (playerCoord.Y > spikerCoord.Y)
            {
                ///Meaning the player is above the spiker.
                ///Meaning the spiker needs to decrease their Y pos.
                return DirToMove.negY;
            }
            else
            {
                ///Meaning the player is below the spiker
                ///Meaning the spiker needs to increase their Y pos.
                return DirToMove.posY;
            }
        }
    }

    private DirToMove InverseDirToMove(DirToMove dirToMove)
    {
        switch (dirToMove)
        {
            case DirToMove.negX:
                return DirToMove.posX;

            case DirToMove.posX:
                return DirToMove.negX;

            case DirToMove.negY:
                return DirToMove.posY;

            case DirToMove.posY:
                return DirToMove.negY;
        }

        return DirToMove.NULL;
    }

    private DirToMove InspectBestTurnDirection(DirToMove dirToMove)
    {
        Coord targetCoord = new Coord(_currTarget);
        Coord spikerCoord = new Coord(this);
        DirToMove newDir;

        if (dirToMove == DirToMove.negX || dirToMove == DirToMove.posX)
        {
            ///Means we were originally trying to move right or left.
            ///Now we want to move up or down.
            
            if (targetCoord.Y > spikerCoord.Y)
            {
                newDir = DirToMove.negY;
            }
            else
            {
                newDir = DirToMove.posY;
            }
        }
        else
        {
            ///Means we were originally trying to move up or down.
            ///Now We want to move left or right.
            if (targetCoord.X > spikerCoord.X)
            {
                newDir = DirToMove.negX;
            }
            else
            {
                newDir = DirToMove.posX;
            }
        }

        return newDir;
    }
    #endregion

    void Notes()
    {
        /*
         * 1. If the spiker is not in range then he needs to move towards
         * a player until he is in range. This is already dealt with with 
         * the normal behaviour of the parent scripts.
         * 
         * 2. If Spiker is within range, but not at MAXIMUM range from the
         * closest player, then the spiker needs to move until they are at 
         * max range from the player
         * 
         * 3. We can just move in any direction that keeps us within range
         * and moving away from the player, depending on current X and Y
         * position on the grid.
         * 
         * 
         * 4. We need to find which tile is going to give us that desired
         * range and make sure that it is actually reachable .
         */

    }

   

    //public override void Dodge()
    //{
    //    Debug.Log("Spiker Dodge");
    //    base.Dodge();
    //}

    public void AdjustMapCoordInDir()
    {

    }
    
    private class Coord
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Coord(Humanoid unit)
        {
            X = (int)unit.currentTile.gridPosition.x;
            Y = (int)unit.currentTile.gridPosition.y;
        }

        public void IncreaseCoordInDir(DirToMove dirToMove)
        {
            switch (dirToMove)
            {
                case DirToMove.negX:
                    X--;
                    break;

                case DirToMove.posX:
                    X++;
                    break;

                case DirToMove.negY:
                    Y--;
                    break;

                case DirToMove.posY:
                    Y++;

                    break;
                default:
                    break;
            }
        }

        public void IncreaseCoordInOppositeDir(DirToMove dirToMove)
        {
            switch (dirToMove)
            {
                case DirToMove.negX:
                    X++;
                    break;

                case DirToMove.posX:
                    X--;
                    break;

                case DirToMove.negY:
                    Y++;
                    break;

                case DirToMove.posY:
                    Y--;
                    break;

                default:
                    break;
            }
        }

        public override string ToString()
        {
            return "Grid Coord: { " + X + ", " + Y + " }";
        }
    }
}
