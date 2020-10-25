using System.Collections;
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

    /// <summary>
    /// Might need to change to include negative x and negative y
    /// </summary>
    enum CloserIn { NULL, X, Y }

    public List<Tile> FindOptimalMaxRangePos()
    {
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

        while (true)
        {
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
            break;
        }

        return path;
    }

    private CloserIn FindDirCloserIn()
    {
        CloserIn closerIn = CloserIn.NULL;

        Coord playerCoord = new Coord(0, 0);
        Coord spikerCoord = new Coord(this);

        ///Refers to the gap between spiker and player in X and Y coords q
        int xDelta = Mathf.Abs(playerCoord.X - spikerCoord.X);
        int yDelta = Mathf.Abs(playerCoord.Y - spikerCoord.Y);


        if (xDelta < yDelta)
        {
            //Meaning we are closer to the player in the X coord
            closerIn = CloserIn.X;
        }
        else if (yDelta < xDelta)
        {
            //Meaning we are closer to the player in the Y coord
            closerIn = CloserIn.Y;
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

    enum DirToMove { negX, posX, negY, posY }


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
    }
}
